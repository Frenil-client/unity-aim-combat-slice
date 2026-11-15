using System.Collections.Generic;
using AimCombat.Core.Random;

namespace AimCombat.Core.Combat
{
    /// <summary>
    /// 스킬을 실행하는 쪽이 공급하는 전투 상황. 위치와 조준처럼 Core 가 모르는 것만 묻는다.
    /// 목록에는 쓰러진 단위가 섞여 있어도 된다. 실행기가 살아 있는 대상만 고른다.
    /// </summary>
    public interface ISkillWorld
    {
        IReadOnlyList<Combatant> Allies { get; }
        IReadOnlyList<Combatant> Enemies { get; }

        /// <summary>시전자가 지금 노리는 적. 없으면 null.</summary>
        Combatant FocusEnemy(Combatant caster);

        /// <summary>시전자에게서 가까운 살아 있는 적을 가까운 순서로 count 기까지 result 에 넣는다.</summary>
        void NearestEnemies(Combatant caster, int count, List<Combatant> result);

        /// <summary>피해가 들어가는 순간 팀이 강화 상태인지(SPEC 3.2).</summary>
        bool Enhanced { get; }

        void RefillAmmo(Combatant ally);
    }

    /// <summary>
    /// 스킬 실행. 효과를 정의 순서가 아니라 종류별 단계로 실행한다(SPEC 3.2).
    /// 1) 상태효과 지우기 2) 상태효과 걸기 3) 피해 4) 회복 5) 탄창 채우기.
    /// 그래서 같은 스킬이 거는 버프와 디버프는 그 스킬의 피해에 이미 반영된다.
    /// 같은 단계 안에서는 정의 순서를 따른다. 대상은 효과마다 실행하는 순간에 다시 고른다.
    /// 실행 도중에 다른 스킬을 실행하지 않는다(버퍼를 같이 쓴다). 패시브는 SkillTriggerHub 가 실행이 끝난 뒤 차례로 돌린다.
    /// </summary>
    public static class SkillRunner
    {
        public static readonly SkillEffectKind[] Stages =
        {
            SkillEffectKind.RemoveStatus,
            SkillEffectKind.ApplyStatus,
            SkillEffectKind.Damage,
            SkillEffectKind.Heal,
            SkillEffectKind.RefillAmmo,
        };

        // 실행 중에만 쓰는 버퍼. Core 는 한 스레드에서만 돈다.
        private static readonly List<Combatant> targets = new List<Combatant>();
        private static readonly List<Combatant> pool = new List<Combatant>();
        private static readonly List<Combatant> randomPicks = new List<Combatant>();
        private static bool randomPicked;

        /// <summary>
        /// triggerSource 는 패시브 트리거를 일으킨 쪽이다(맞은 적, 처치한 적, 버스트를 쓴 아군).
        /// TriggerEnemy, TriggerAlly 대상이 이것을 고른다. 액티브와 버스트는 null 이다.
        /// </summary>
        public static void Execute(SkillBlockDef skill, Combatant caster, ISkillWorld world, DeterministicRandom random, Combatant triggerSource = null)
        {
            if (skill == null || caster == null || !caster.IsAlive) return;

            randomPicked = false;
            randomPicks.Clear();
            foreach (SkillEffectKind stage in Stages)
            {
                foreach (SkillEffectDef effect in skill.Effects)
                {
                    if (effect == null || effect.Kind != stage) continue;
                    ResolveTargets(effect, caster, world, random, triggerSource);
                    foreach (Combatant target in targets) ApplyEffect(effect, caster, target, world, random);
                }
            }
            targets.Clear();
            randomPicks.Clear();
        }

        private static void ApplyEffect(SkillEffectDef effect, Combatant caster, Combatant target, ISkillWorld world, DeterministicRandom random)
        {
            if (!target.IsAlive) return; // 같은 스킬의 앞선 피해로 쓰러졌다.

            switch (effect.Kind)
            {
                case SkillEffectKind.RemoveStatus:
                    target.Statuses.Remove(effect.Status);
                    break;

                case SkillEffectKind.ApplyStatus:
                    target.Statuses.Apply(effect.Status, caster);
                    break;

                case SkillEffectKind.Damage:
                {
                    DamageInput input = DamageInput.Skill(caster.Attack, effect.Coefficient);
                    input.ApplyStatuses(caster, target);
                    input.Critical = random.NextUnitFixed() < DamageFormula.CritChance(caster);
                    input.Enhanced = world.Enhanced;
                    input.SkillCoreHit = effect.CoreHit;
                    target.ApplyDamage(DamageFormula.Compute(input), DamageSource.Skill, caster, input.Critical, effect.CoreHit);
                    break;
                }

                case SkillEffectKind.Heal:
                    target.Heal(target.MaxHealth * effect.Coefficient);
                    break;

                case SkillEffectKind.RefillAmmo:
                    world.RefillAmmo(target);
                    break;
            }
        }

        private static void ResolveTargets(SkillEffectDef effect, Combatant caster, ISkillWorld world, DeterministicRandom random, Combatant triggerSource)
        {
            targets.Clear();
            switch (effect.Target)
            {
                case SkillTarget.Self:
                    targets.Add(caster);
                    break;

                case SkillTarget.AllAllies:
                    AddAlive(world.Allies);
                    break;

                case SkillTarget.LowestHealthAlly:
                {
                    Combatant lowest = null;
                    foreach (Combatant ally in world.Allies)
                    {
                        if (ally == null || !ally.IsAlive) continue;
                        // 비율 비교를 곱셈으로 바꿔 나눗셈 반올림을 피한다: a.Health / a.Max < b.Health / b.Max
                        if (lowest == null || ally.Health * lowest.MaxHealth < lowest.Health * ally.MaxHealth) lowest = ally;
                    }
                    if (lowest != null) targets.Add(lowest);
                    break;
                }

                case SkillTarget.FocusEnemy:
                {
                    Combatant focus = world.FocusEnemy(caster);
                    if (focus != null && focus.IsAlive) targets.Add(focus);
                    break;
                }

                case SkillTarget.NearestEnemies:
                    world.NearestEnemies(caster, effect.Count < 1 ? 1 : effect.Count, targets);
                    break;

                case SkillTarget.AllEnemies:
                    AddAlive(world.Enemies);
                    break;

                case SkillTarget.RandomEnemies:
                {
                    // 한 번의 실행에서 무작위 대상은 한 번만 뽑고, 같은 스킬의 다른 무작위 효과도 그 대상을 쓴다.
                    // "무작위 적에게 발사: 피해, 명중한 대상에게 디버프"가 같은 적에게 들어가야 하기 때문이다.
                    // 처음 뽑는 효과의 대상 수와 조건(TargetStatus)이 기준이 된다. 조건에 맞는 적 중에서 겹치지 않게 뽑는다.
                    if (!randomPicked)
                    {
                        randomPicked = true;
                        pool.Clear();
                        foreach (Combatant c in world.Enemies)
                        {
                            if (c != null && c.IsAlive && (effect.TargetStatus == null || c.Statuses.Has(effect.TargetStatus))) pool.Add(c);
                        }
                        int count = effect.Count < 1 ? 1 : effect.Count;
                        while (count-- > 0 && pool.Count > 0)
                        {
                            int pick = random.NextInt(pool.Count);
                            randomPicks.Add(pool[pick]);
                            pool.RemoveAt(pick);
                        }
                        pool.Clear();
                    }
                    foreach (Combatant c in randomPicks) if (c.IsAlive) targets.Add(c);
                    break;
                }

                case SkillTarget.TriggerEnemy:
                case SkillTarget.TriggerAlly:
                    if (triggerSource != null && triggerSource.IsAlive) targets.Add(triggerSource);
                    break;
            }

            if (effect.TargetStatus == null) return;
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (!targets[i].Statuses.Has(effect.TargetStatus)) targets.RemoveAt(i);
            }
        }

        private static void AddAlive(IReadOnlyList<Combatant> list)
        {
            foreach (Combatant c in list)
            {
                if (c != null && c.IsAlive) targets.Add(c);
            }
        }
    }
}

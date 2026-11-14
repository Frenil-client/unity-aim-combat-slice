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
    /// 1) 상태효과 2) 피해 3) 회복 4) 탄창 채우기. 그래서 같은 스킬이 거는 버프와 디버프는 그 스킬의 피해에 이미 반영된다.
    /// 같은 단계 안에서는 정의 순서를 따른다. 대상은 효과마다 실행하는 순간에 다시 고른다.
    /// </summary>
    public static class SkillRunner
    {
        private static readonly SkillEffectKind[] Stages =
        {
            SkillEffectKind.ApplyStatus,
            SkillEffectKind.Damage,
            SkillEffectKind.Heal,
            SkillEffectKind.RefillAmmo,
        };

        // 실행 중에만 쓰는 버퍼. Core 는 한 스레드에서만 돈다.
        private static readonly List<Combatant> targets = new List<Combatant>();

        public static void Execute(SkillDef skill, Combatant caster, ISkillWorld world, DeterministicRandom random)
        {
            if (skill == null || caster == null || !caster.IsAlive) return;

            foreach (SkillEffectKind stage in Stages)
            {
                foreach (SkillEffectDef effect in skill.Effects)
                {
                    if (effect.Kind != stage) continue;
                    ResolveTargets(effect, caster, world);
                    foreach (Combatant target in targets) ApplyEffect(effect, caster, target, world, random);
                }
            }
            targets.Clear();
        }

        private static void ApplyEffect(SkillEffectDef effect, Combatant caster, Combatant target, ISkillWorld world, DeterministicRandom random)
        {
            if (!target.IsAlive) return; // 같은 스킬의 앞선 피해로 쓰러졌다.

            switch (effect.Kind)
            {
                case SkillEffectKind.ApplyStatus:
                    target.Statuses.Apply(effect.Status, caster);
                    break;

                case SkillEffectKind.Damage:
                {
                    DamageInput input = DamageInput.Skill(caster.Attack, effect.Coefficient);
                    input.ApplyStatuses(caster, target);
                    input.Critical = random.NextUnitFixed() < DamageFormula.CritChance(caster);
                    input.Enhanced = world.Enhanced;
                    target.ApplyDamage(DamageFormula.Compute(input), DamageSource.Skill, caster, input.Critical);
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

        private static void ResolveTargets(SkillEffectDef effect, Combatant caster, ISkillWorld world)
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

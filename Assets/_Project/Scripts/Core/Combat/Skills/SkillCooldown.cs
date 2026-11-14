namespace AimCombat.Core.Combat
{
    /// <summary>
    /// 스킬 쿨다운(SPEC 3.2). 쓰는 순간부터 다시 센다. 액티브 스킬은 전투 시작 때 쿨다운이 다 돈 뒤에 쓸 수 있고,
    /// 버스트는 전투 시작 때 바로 쓸 수 있다(startReady).
    /// </summary>
    public sealed class SkillCooldown
    {
        public SkillCooldown(Fixed cooldown, bool startReady = false)
        {
            Cooldown = cooldown < Fixed.Zero ? Fixed.Zero : cooldown;
            Remaining = startReady ? Fixed.Zero : Cooldown;
        }

        public Fixed Cooldown { get; }
        public Fixed Remaining { get; private set; }
        public bool IsReady => Remaining <= Fixed.Zero;

        /// <summary>찬 정도 0~1. 다 차면 1.</summary>
        public Fixed Progress01 => Cooldown <= Fixed.Zero ? Fixed.One : Fixed.One - Remaining / Cooldown;

        public void Tick(Fixed deltaTime)
        {
            if (Remaining <= Fixed.Zero || deltaTime <= Fixed.Zero) return;
            Remaining = Remaining - deltaTime;
            if (Remaining < Fixed.Zero) Remaining = Fixed.Zero;
        }

        /// <summary>쓸 수 있으면 쿨다운을 다시 시작하고 true.</summary>
        public bool TryUse()
        {
            if (!IsReady) return false;
            Remaining = Cooldown;
            return true;
        }
    }
}

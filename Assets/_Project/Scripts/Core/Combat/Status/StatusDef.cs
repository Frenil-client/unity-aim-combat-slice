namespace AimCombat.Core.Combat
{
    /// <summary>같은 효과가 다시 걸릴 때의 규칙(SPEC 3.3).</summary>
    public enum StatusRule
    {
        /// <summary>최대 중첩 수까지 쌓이고 중첩마다 남은 시간을 따로 센다. 가득 찬 뒤 다시 걸리면 가장 먼저 끝날 중첩을 새로 채운다.</summary>
        Stack,
        /// <summary>중첩하지 않고 남은 시간만 새로 채운다.</summary>
        Refresh,
        /// <summary>같은 능력치를 올리는 효과가 여러 개 걸려도 가장 강한 값 하나만 적용한다. 같은 효과가 다시 걸리면 시간만 채운다.</summary>
        Highest,
        /// <summary>지속 피해. 시전자별로 따로 유지하고 틱마다 시전자의 현재 버프로 피해를 계산한다.</summary>
        DamageOverTime,
        /// <summary>행동 불가. 걸린 동안 같은 효과에 면역이고 풀린 뒤 짧은 면역 시간이 있다.</summary>
        Control,
        /// <summary>표식. 능력치를 바꾸지 않고 걸려 있다는 사실만 스킬의 발동 조건과 대상 조건이 읽는다. 다시 걸리면 시간만 채운다.</summary>
        Marker,
    }

    /// <summary>상태효과가 바꾸는 능력치. 지속 피해와 행동 불가는 능력치를 바꾸지 않으므로 None 이다.</summary>
    public enum StatKind
    {
        None,
        /// <summary>공격력 증가 %(SPEC 3.1 1단계).</summary>
        AttackUp,
        /// <summary>치명 확률 증가. 기본 15%에 더한다.</summary>
        CritChanceUp,
        /// <summary>치명 항목 증가(SPEC 3.1 3단계 주요 배율).</summary>
        CritBonusUp,
        /// <summary>약점 항목 증가(SPEC 3.1 3단계 주요 배율).</summary>
        WeakPointBonusUp,
        /// <summary>받는 피해 증가(SPEC 3.1 5단계). 대상에게 거는 디버프다.</summary>
        DamageTakenUp,
        /// <summary>이동 속도 감소 비율. 대상에게 거는 디버프다.</summary>
        Slow,
        /// <summary>주는 피해 증가(SPEC 3.1 5단계). 공격자에게 거는 버프이고 대상의 받는 피해 증가와 같은 묶음에 더한다.</summary>
        DamageUp,
    }

    /// <summary>
    /// 상태효과 하나의 정의. 데이터(Unity 의 StatusData 에셋)에서 만들어지고 실행 중에는 읽기만 한다.
    /// 값의 뜻은 규칙에 따라 다르다. 능력치 효과는 중첩 하나당 올리는 양, 지속 피해는 틱 하나의 스킬 계수,
    /// 행동 불가는 게이지형 대상(보스)의 저항 게이지를 깎는 양이다.
    /// </summary>
    public sealed class StatusDef
    {
        public string Id = "";
        public string Name = "";
        public StatusRule Rule;
        public StatKind Stat;
        public Fixed Value;
        public Fixed Duration;
        /// <summary>지속형. 시간이 지나도 풀리지 않고 대상이 쓰러지거나 스킬이 지울 때만 없어진다(니케의 [지속]). Duration 은 쓰지 않는다.</summary>
        public bool Permanent;
        /// <summary>Stack 규칙의 최대 중첩 수. 다른 규칙은 1로 본다.</summary>
        public int MaxStacks = 1;
        /// <summary>DamageOverTime 규칙의 틱 간격(초).</summary>
        public Fixed TickInterval;
        /// <summary>Control 규칙이 풀린 뒤 같은 효과에 면역인 시간(초).</summary>
        public Fixed ImmunityAfter;

        /// <summary>대상에게 해로운 효과인지. 검증에서 아군에게 거는지 적에게 거는지 확인할 때 쓴다.</summary>
        public bool IsHarmful =>
            Rule == StatusRule.DamageOverTime || Rule == StatusRule.Control
            || Stat == StatKind.DamageTakenUp || Stat == StatKind.Slow;

        public int StackLimit => Rule == StatusRule.Stack && MaxStacks > 1 ? MaxStacks : 1;

        public override string ToString() => string.IsNullOrEmpty(Name) ? Id : Name;
    }
}

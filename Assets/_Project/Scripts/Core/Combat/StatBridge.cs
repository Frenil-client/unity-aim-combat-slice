using StatSystem;

namespace AimCombat.Core.Combat
{
    /// <summary>
    /// 이 프로젝트의 고정소수점(Fixed, 소수 3자리)과 stat-system 의 StatValue(소수 4자리) 사이 변환.
    /// 두 쪽 모두 long 정수 연산이라 변환도 정수로만 한다(결정적). StatValue 에서 Fixed 로 갈 때 소수 넷째 자리는 버린다.
    /// </summary>
    public static class StatBridge
    {
        private const long Ratio = StatValue.Scale / Fixed.Scale;

        public static StatValue ToStat(Fixed value) => StatValue.FromRaw(value.raw * Ratio);

        public static Fixed ToFixed(StatValue value) => Fixed.FromRaw(value.Raw / Ratio);
    }
}

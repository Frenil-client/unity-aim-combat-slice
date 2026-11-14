using AimCombat.Core;
using AimCombat.Core.Combat;
using UnityEngine;

namespace AimCombat.Data
{
    /// <summary>Unity 의 float 와 Core 의 Fixed 사이 변환. 데이터 에셋과 화면 표시만 float 를 쓴다.</summary>
    public static class FixedConvert
    {
        public static Fixed ToFixed(float value) => Fixed.FromMilli((long)Mathf.Round(value * Fixed.Scale));

        public static float ToFloat(Fixed value) => value.raw / (float)Fixed.Scale;
    }

    /// <summary>
    /// 전투 시간. 프레임마다 한 번 Time.deltaTime 을 Fixed 초로 바꾸고, 1밀리초에 못 미치는 잔여분은 다음 프레임으로 넘긴다.
    /// 상태효과, 쿨다운, 궁극기처럼 Core 가 세는 시간은 모두 이 값을 쓴다. 그래서 지속 피해 총량이 프레임률과 무관하다.
    /// </summary>
    public static class CombatTime
    {
        private static int frame = -1;
        private static Fixed delta;
        private static TickAccumulator accumulator;

        public static Fixed Delta
        {
            get
            {
                if (Time.frameCount != frame)
                {
                    frame = Time.frameCount;
                    delta = accumulator.Consume((long)System.Math.Round(Time.deltaTime * 1000000.0));
                }
                return delta;
            }
        }
    }
}

namespace AimCombat.Core.Combat
{
    public enum UltimatePhase
    {
        /// <summary>게이지를 채우는 중. 다 차면 1번 순번을 발동할 수 있다.</summary>
        Charging,
        /// <summary>연계 중. 제한 시간 안에 다음 순번을 발동해야 한다.</summary>
        Chain,
        /// <summary>마지막 순번까지 발동해 팀 전체가 강화 상태다.</summary>
        Enhanced,
    }

    /// <summary>
    /// SPEC 3.2 궁극기 게이지와 연계. 팀 공용 게이지가 다 차면 순번대로 1번 -> 2번 -> 3번을 발동하고,
    /// 마지막 순번까지 발동하면 일정 시간 강화 상태가 된다.
    /// - 연계를 시작할 때 게이지를 비운다. 연계 중과 강화 상태 동안에는 게이지가 차지 않는다.
    /// - 제한 시간 안에 다음 순번을 발동하지 않으면 연계가 끊기고 게이지를 처음부터 채운다.
    /// 순번마다 누가 발동하는지와 궁극기의 효과는 바깥에서 정한다.
    /// </summary>
    public sealed class UltimateChain
    {
        private readonly int orderCount;
        private readonly Fixed gaugeMax;
        private readonly Fixed stageWindow;
        private readonly Fixed enhancedDuration;

        public UltimateChain(int orderCount, Fixed gaugeMax, Fixed stageWindow, Fixed enhancedDuration)
        {
            this.orderCount = orderCount < 1 ? 1 : orderCount;
            this.gaugeMax = gaugeMax;
            this.stageWindow = stageWindow;
            this.enhancedDuration = enhancedDuration;
        }

        public UltimatePhase Phase { get; private set; }
        public Fixed Gauge { get; private set; }
        public Fixed GaugeMax => gaugeMax;

        /// <summary>연계 창 또는 강화 상태의 남은 시간. 충전 중에는 0.</summary>
        public Fixed TimeLeft { get; private set; }

        /// <summary>지금 발동하면 몇 번째 순번인지(0부터).</summary>
        public int ActivatingOrder { get; private set; }

        public bool IsEnhanced => Phase == UltimatePhase.Enhanced;

        /// <summary>지금 다음 순번을 발동할 수 있으면 true.</summary>
        public bool CanActivate => Phase == UltimatePhase.Chain || (Phase == UltimatePhase.Charging && Gauge >= gaugeMax);

        public void AddGauge(Fixed amount)
        {
            if (Phase != UltimatePhase.Charging || amount <= Fixed.Zero) return;
            Gauge = Gauge + amount;
            if (Gauge > gaugeMax) Gauge = gaugeMax;
        }

        /// <summary>다음 순번을 발동한다. 발동한 순번(0부터)을 돌려주고, 발동할 수 없으면 -1 을 돌려준다.</summary>
        public int Activate()
        {
            if (!CanActivate) return -1;

            int order = ActivatingOrder;
            if (Phase == UltimatePhase.Charging) Gauge = Fixed.Zero;

            if (order + 1 >= orderCount)
            {
                Phase = UltimatePhase.Enhanced;
                TimeLeft = enhancedDuration;
                ActivatingOrder = 0;
            }
            else
            {
                Phase = UltimatePhase.Chain;
                TimeLeft = stageWindow;
                ActivatingOrder = order + 1;
            }
            return order;
        }

        public void Tick(Fixed deltaTime)
        {
            if (Phase == UltimatePhase.Charging) return;

            TimeLeft = TimeLeft - deltaTime;
            if (TimeLeft > Fixed.Zero) return;

            // 연계가 끊겼거나 강화 상태가 끝났다. 둘 다 게이지를 처음부터 채운다(연계를 시작할 때 이미 비웠다).
            Phase = UltimatePhase.Charging;
            TimeLeft = Fixed.Zero;
            ActivatingOrder = 0;
        }
    }
}

namespace AimCombat.Core.Combat
{
    public enum BurstPhase
    {
        /// <summary>게이지를 채우는 중.</summary>
        Charging,
        /// <summary>연계 중. Stage 단계의 버스트를 기다린다. 1단계는 시간 제한이 없고 2, 3단계는 창이 닫히면 연계가 끊긴다.</summary>
        Stage,
        /// <summary>3단계까지 써서 풀 버스트 중이다. 모든 피해의 주요 배율에 0.5를 더한다(SPEC 3.1).</summary>
        FullBurst,
    }

    /// <summary>
    /// SPEC 3.2 버스트 게이지와 연계(니케 버스트 구조, NIKKE_COMBAT_ANALYSIS.md 5절).
    /// - 팀 공용 게이지가 다 차면 1단계가 열린다. 1단계는 시간 제한 없이 기다린다.
    /// - 각 단계에서는 버스트 단계가 같은 아군의 버스트 1개만 쓸 수 있다. 쓰면 다음 단계가 열리고 그 버스트가 정한 시간만큼 창이 열린다.
    /// - 2, 3단계 창이 닫힐 때까지 아무도 쓰지 않으면 연계가 끊기고 게이지를 처음부터 채운다.
    /// - 3단계를 쓰면 풀 버스트가 정해진 시간 이어진다.
    /// - 1단계 버스트를 쓸 때 게이지를 비운다. 연계 중과 풀 버스트 동안에는 게이지가 차지 않는다.
    /// 누가 어느 단계인지, 쿨다운이 돌았는지는 바깥(분대)이 정하고 여기서는 단계 진행만 맡는다.
    /// </summary>
    public sealed class BurstChain
    {
        public const int StageCount = 3;

        private readonly Fixed gaugeMax;
        private readonly Fixed fullBurstDuration;

        public BurstChain(Fixed gaugeMax, Fixed fullBurstDuration)
        {
            this.gaugeMax = gaugeMax;
            this.fullBurstDuration = fullBurstDuration;
        }

        public BurstPhase Phase { get; private set; }
        public Fixed Gauge { get; private set; }
        public Fixed GaugeMax => gaugeMax;
        public Fixed FullBurstDuration => fullBurstDuration;

        /// <summary>연계 중일 때 기다리는 단계(1~3). 연계 중이 아니면 0.</summary>
        public int Stage { get; private set; }

        /// <summary>지금 단계에 시간 제한이 있는지. 1단계는 없다.</summary>
        public bool StageTimed { get; private set; }

        /// <summary>단계 창 또는 풀 버스트의 남은 시간. 시간 제한이 없으면 0.</summary>
        public Fixed TimeLeft { get; private set; }

        public bool IsFullBurst => Phase == BurstPhase.FullBurst;

        /// <summary>창이 닫혀 연계가 끊긴 횟수. HUD 알림과 확인용.</summary>
        public int Collapses { get; private set; }

        public void AddGauge(Fixed amount)
        {
            if (Phase != BurstPhase.Charging || amount <= Fixed.Zero) return;
            Gauge = Gauge + amount;
            if (Gauge < gaugeMax) return;

            Gauge = gaugeMax;
            Phase = BurstPhase.Stage;
            Stage = 1;
            StageTimed = false;
            TimeLeft = Fixed.Zero;
        }

        /// <summary>버스트 단계가 burstStage 인 아군이 지금 쓸 수 있는 단계인지.</summary>
        public bool Accepts(int burstStage) => Phase == BurstPhase.Stage && burstStage == Stage;

        /// <summary>
        /// 지금 단계의 버스트를 썼다. nextWindow 는 다음 단계 창의 길이(버스트마다 다르다).
        /// 단계가 맞지 않으면 아무것도 하지 않고 false.
        /// </summary>
        public bool Use(int burstStage, Fixed nextWindow)
        {
            if (!Accepts(burstStage)) return false;

            if (Stage == 1) Gauge = Fixed.Zero;

            if (Stage >= StageCount)
            {
                Phase = BurstPhase.FullBurst;
                Stage = 0;
                StageTimed = false;
                TimeLeft = fullBurstDuration;
                return true;
            }

            Stage++;
            StageTimed = true;
            TimeLeft = nextWindow;
            return true;
        }

        public void Tick(Fixed deltaTime)
        {
            if (Phase == BurstPhase.Charging) return;
            if (Phase == BurstPhase.Stage && !StageTimed) return;

            TimeLeft = TimeLeft - deltaTime;
            if (TimeLeft > Fixed.Zero) return;

            // 창이 닫혔거나 풀 버스트가 끝났다. 1단계에서 이미 비운 게이지를 처음부터 채운다.
            if (Phase == BurstPhase.Stage) Collapses++;
            Phase = BurstPhase.Charging;
            Gauge = Fixed.Zero;
            Stage = 0;
            StageTimed = false;
            TimeLeft = Fixed.Zero;
        }
    }
}

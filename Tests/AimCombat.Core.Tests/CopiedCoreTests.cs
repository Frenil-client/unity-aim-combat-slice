using AimCombat.Core.Combat;
using AimCombat.Core.Data;
using AimCombat.Core.Text;

namespace AimCombat.Core.Tests
{
    // DefenceGame(Synthesis.Core.Tests)에서 복사한 파일에 해당하는 테스트만 옮겨 온 것이다.
    // 원본 위치: SkillRulesTests(TickAccumulator), StringTableTests(CsvUtil, StringFormatter).
    public class CopiedCoreTests
    {
        // 명목상 같은 1초라면 프레임률이 달라도 소비한 시간의 합이 같아야 한다.
        [Theory]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(120)]
        [InlineData(144)]
        public void TickAccumulator_OneSecondIsFrameRateIndependent(int fps)
        {
            TickAccumulator acc = new TickAccumulator();
            long perFrame = 1000000L / fps;
            long leftover = 1000000L - perFrame * fps; // 나누어떨어지지 않는 몫은 마지막 프레임에 얹는다

            Fixed total = Fixed.Zero;
            for (int i = 0; i < fps; ++i)
            {
                long micros = perFrame;
                if (i == fps - 1) micros += leftover;
                total = total + acc.Consume(micros);
            }

            Assert.Equal(Fixed.One.raw, total.raw);
        }

        // 1밀리초를 못 채운 프레임은 0을 돌려주되 잔여분을 버리지 않는다.
        [Fact]
        public void TickAccumulator_KeepsSubMilliRemainder()
        {
            TickAccumulator acc = new TickAccumulator();

            Assert.Equal(0, acc.Consume(400).raw);
            Assert.Equal(0, acc.Consume(400).raw);
            Assert.Equal(Fixed.FromMilli(1).raw, acc.Consume(400).raw); // 1200 마이크로 -> 1밀리 소비, 200 보존
            Assert.Equal(0, acc.Consume(400).raw);                      // 600 마이크로 누적
            Assert.Equal(Fixed.FromMilli(1).raw, acc.Consume(400).raw); // 1000 마이크로 -> 1밀리
        }

        // 재시작 시 이전 판의 잔여 시간이 새 판으로 넘어가면 안 된다.
        [Fact]
        public void TickAccumulator_ResetDropsRemainder()
        {
            TickAccumulator acc = new TickAccumulator();
            acc.Consume(900);
            acc.Reset();
            Assert.Equal(0, acc.Consume(500).raw);
            Assert.Equal(Fixed.FromMilli(1).raw, acc.Consume(500).raw);
        }

        // dt 가 커져도 한 번에 소비한다. 음수와 0 입력은 아무것도 소비하지 않는다.
        [Fact]
        public void TickAccumulator_HandlesLargeAndNonPositiveDelta()
        {
            TickAccumulator acc = new TickAccumulator();
            Assert.Equal(Fixed.FromMilli(50).raw, acc.Consume(50000).raw);
            Assert.Equal(0, acc.Consume(0).raw);
            Assert.Equal(0, acc.Consume(-1000).raw);
        }

        // 값에 쉼표가 들어가는 문자열이 한 필드로 읽혀야 한다.
        [Fact]
        public void SplitCsvFields_HandlesQuotes()
        {
            var split = CsvUtil.SplitCsvFields("key,\"쉼표, 포함\",\"quoted \"\"word\"\"\"");
            Assert.Equal(3, split.Count);
            Assert.Equal("key", split[0]);
            Assert.Equal("쉼표, 포함", split[1]);
            Assert.Equal("quoted \"word\"", split[2]);
        }

        [Fact]
        public void Formatter_ReplacesPlaceholders()
        {
            StringValues values = new StringValues();
            values.Set("label", "공격력").Set("value", "450").Set("delta", "90");

            Assert.Equal("공격력 450", StringFormatter.Format("{label} {value}", values));

            // 값이 없는 치환자는 그대로 남는다.
            Assert.Equal("{missing}", StringFormatter.Format("{missing}", values));

            // 닫히지 않은 중괄호는 리터럴이다.
            Assert.Equal("{broken", StringFormatter.Format("{broken", values));
        }

        [Fact]
        public void Formatter_CollectsPlaceholders()
        {
            var names = StringFormatter.CollectPlaceholders("{label} {value} (+{delta})");
            Assert.Equal(3, names.Count);
            Assert.Equal("label", names[0]);
            Assert.Equal("delta", names[2]);
        }
    }
}

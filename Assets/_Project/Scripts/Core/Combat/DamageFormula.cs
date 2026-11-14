namespace AimCombat.Core.Combat
{
    /// <summary>한 번의 피해 계산에 들어가는 값. 사격은 Shot 으로 만들고 해당하는 항목만 채운다.</summary>
    public struct DamageInput
    {
        /// <summary>기본 공격력.</summary>
        public Fixed Attack;
        /// <summary>공격력 증가 % 합계. 0.2 = 20%.</summary>
        public Fixed AttackUpSum;
        /// <summary>대상 방어력. 0 밑으로 내려가지 않는다.</summary>
        public Fixed Defense;
        /// <summary>무기 계수. 한 발(산탄형은 탄알 하나)이 공격력의 몇 배를 주는지.</summary>
        public Fixed WeaponCoefficient;
        public Fixed SkillCoefficient;
        /// <summary>사격이면 true. 약점, 적정 사거리, 차지는 사격에만 붙는다.</summary>
        public bool IsWeaponShot;
        public bool Critical;
        /// <summary>치명 항목을 올리는 버프 합계. 치명일 때만 더한다.</summary>
        public Fixed CritBonusUp;
        public bool WeakPoint;
        /// <summary>약점 항목을 올리는 버프 합계. 약점에 맞았을 때만 더한다.</summary>
        public Fixed WeakPointBonusUp;
        public bool InEffectiveRange;
        /// <summary>피해가 들어가는 순간 강화 상태인지(SPEC 3.2).</summary>
        public bool Enhanced;
        /// <summary>충전량 0~1. 차지형이 아니면 0.</summary>
        public Fixed Charge;
        /// <summary>대상의 받는 피해 증가 디버프 합계.</summary>
        public Fixed DamageTakenUpSum;

        public static DamageInput Shot(Fixed attack, Fixed weaponCoefficient)
        {
            return new DamageInput
            {
                Attack = attack,
                WeaponCoefficient = weaponCoefficient,
                SkillCoefficient = Fixed.One,
                IsWeaponShot = true,
            };
        }
    }

    /// <summary>
    /// SPEC 3.1 데미지 연산 순서. 니케 공식 구조를 따른다(NIKKE_COMBAT_ANALYSIS.md 4절).
    /// 공격력에서 방어력을 빼고, 치명, 약점, 적정 사거리, 강화 상태는 한 덧셈 묶음(주요 배율)으로 더한다.
    /// 묶음끼리는 곱한다. 그래서 치명과 약점을 동시에 맞혀도 x3 이 아니라 x2.5 다.
    /// </summary>
    public static class DamageFormula
    {
        // 시작값. 3단계 튜닝에서 바뀔 수 있다.
        public static readonly Fixed BaseCritChance = Fixed.FromRatio(15, 100);
        public static readonly Fixed CritBonus = Fixed.FromRatio(1, 2);
        public static readonly Fixed WeakPointBonus = Fixed.One;
        public static readonly Fixed EffectiveRangeBonus = Fixed.FromRatio(3, 10);
        public static readonly Fixed EnhancedBonus = Fixed.FromRatio(1, 2);
        public static readonly Fixed ChargeBonus = Fixed.FromRatio(3, 2);

        public static Fixed Compute(in DamageInput input)
        {
            // 1. 공격력
            Fixed attack = input.Attack * (Fixed.One + input.AttackUpSum);

            // 2. 기본 피해. 방어력이 공격력보다 크면 음수가 되고 6단계의 하한에 걸린다.
            Fixed defense = input.Defense < Fixed.Zero ? Fixed.Zero : input.Defense;
            Fixed damage = (attack - defense) * input.WeaponCoefficient * input.SkillCoefficient;

            // 3. 주요 배율
            damage = damage * MajorMultiplier(input);

            // 4. 차지 배율
            if (input.IsWeaponShot) damage = damage * (Fixed.One + Clamp01(input.Charge) * ChargeBonus);

            // 5. 받는 피해 증가
            damage = damage * (Fixed.One + input.DamageTakenUpSum);

            // 6. 하한
            return damage < Fixed.One ? Fixed.One : damage;
        }

        /// <summary>3단계 주요 배율. 1 + 해당하는 항목의 합.</summary>
        public static Fixed MajorMultiplier(in DamageInput input)
        {
            Fixed sum = Fixed.One;
            if (input.Critical) sum = sum + CritBonus + input.CritBonusUp;
            if (input.IsWeaponShot && input.WeakPoint) sum = sum + WeakPointBonus + input.WeakPointBonusUp;
            if (input.IsWeaponShot && input.InEffectiveRange) sum = sum + EffectiveRangeBonus;
            if (input.Enhanced) sum = sum + EnhancedBonus;
            return sum;
        }

        private static Fixed Clamp01(Fixed value)
        {
            if (value < Fixed.Zero) return Fixed.Zero;
            return value > Fixed.One ? Fixed.One : value;
        }
    }
}

using System.Collections.Generic;
using AimCombat.Data;

namespace AimCombat.Squad
{
    /// <summary>
    /// 캐릭터마다 고른 스킬 세트를 기억한다(SPEC 3.2 스킬 세트 선택). 씬을 다시 불러도 남고 앱을 끄면 사라진다.
    /// 저장 파일에 남기는 것은 범위 밖이라 메모리에만 둔다.
    /// </summary>
    public static class SquadLoadout
    {
        private static readonly Dictionary<string, int> chosen = new Dictionary<string, int>();

        /// <summary>고른 세트의 번호. 고른 적이 없으면 기본 세트(0)다. 세트 수가 줄었으면 범위 안으로 맞춘다.</summary>
        public static int Get(CharacterData character)
        {
            if (character == null) return 0;
            return chosen.TryGetValue(character.name, out int set) && set < character.SetCount ? set : 0;
        }

        public static void Set(CharacterData character, int set)
        {
            if (character != null) chosen[character.name] = set;
        }
    }
}

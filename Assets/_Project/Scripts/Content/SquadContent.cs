using System;
using AddressablesDelivery.Loading;
using AimCombat.Battle;
using AimCombat.Data;
using AimCombat.Squad;
using UnityEngine;
using UnityEngine.Rendering;

namespace AimCombat.Content
{
    /// <summary>
    /// 분대 캐릭터를 주소로 불러와 자리(slot)에 세우고 분대 조작(SquadController)에 꽂는다.
    /// 전투 씬은 스킬 구성(CharacterData), 얼굴과 컷인 그림, 엄폐물까지 함께 꽂고, 사격장은 캐릭터만 세운다.
    /// </summary>
    public sealed class SquadContent : ContentStep
    {
        [Serializable]
        private struct Slot
        {
            [Tooltip("캐릭터 이름(주소 Characters/이름).")]
            public string character;
            [Tooltip("서는 자리. 위치와 방향을 그대로 쓰고 이 자리의 부모 아래에 놓는다.")]
            public Transform point;
            [Tooltip("전투 씬에서 이 캐릭터가 숨는 엄폐물. 사격장은 비운다.")]
            public CoverObject cover;
        }

        [SerializeField] private SquadController squad;
        [SerializeField] private Slot[] slots;
        [Tooltip("스킬 구성과 얼굴, 컷인 그림도 불러온다(전투 씬). 끄면 캐릭터만 세운다(사격장).")]
        [SerializeField] private bool withCombatData = true;

        public override async Awaitable LoadAsync(AssetHandleSet handles)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                Slot slot = slots[i];
                GameObject prefab = await Require<GameObject>(handles, ContentAddress.Character(slot.character));
                CharacterData data = null;
                Sprite face = null, cutIn = null;
                if (withCombatData)
                {
                    data = await Require<CharacterData>(handles, ContentAddress.CharacterData(slot.character));
                    face = await handles.LoadAsync<Sprite>(ContentAddress.CharacterFace(slot.character));
                    cutIn = await handles.LoadAsync<Sprite>(ContentAddress.CharacterCutIn(slot.character));
                }

                GameObject character = Instantiate(prefab, slot.point.position, slot.point.rotation, slot.point.parent);
                character.name = prefab.name;
                // 겹칠 때는 오른쪽 캐릭터가 앞에 그려진다.
                if (character.TryGetComponent(out SortingGroup sorting)) sorting.sortingOrder = i;

                AllyUnit unit = withCombatData ? character.GetComponent<AllyUnit>() : null;
                if (unit != null) unit.SetCover(slot.cover);
                squad.BindMember(i, character, unit, data, face, cutIn);
            }
        }
    }
}

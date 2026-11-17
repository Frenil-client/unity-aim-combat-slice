using System;
using System.Collections.Generic;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// 씬 하나를 실행하는 데 필요한 기초 리소스 목록(Addressables 주소). 씬에 들어가기 전에 이 목록을 다 불러 두고(ContentCache),
    /// 씬의 불러오기 단계(ContentStep)는 여기 있는 것만 꺼내 쓴다. 목록에 없는 주소를 꺼내면 오류다(빠뜨림을 바로 알린다).
    /// - 유지 규칙: 씬을 옮길 때 다음 씬 목록에도 있는 리소스는 그대로 두고, 없는 리소스는 새 씬이 열린 뒤 반환한다.
    ///   Persistent 로 표시한 리소스는 어느 씬으로 가도 반환하지 않는다(여러 씬이 계속 쓰는 것, 다시 받는 비용이 큰 것).
    /// - 같은 씬을 다시 열면(다시 시작) 목록이 같아 반환도 새로 불러오기도 없다.
    /// - 에셋을 직접 가리키지 않고 주소 문자열만 둔다. 직접 가리키면 이 목록을 든 씬과 시작 씬에 그 에셋이 의존성으로 끌려 들어간다.
    /// </summary>
    [CreateAssetMenu(fileName = "Content_", menuName = "Aim Combat/Scene Content Set")]
    public sealed class SceneContentSet : ScriptableObject
    {
        public enum Kind
        {
            /// <summary>주 에셋(프리팹, 데이터 에셋).</summary>
            Asset,
            /// <summary>그림 파일의 스프라이트.</summary>
            Sprite,
        }

        public enum Retention
        {
            /// <summary>다음 씬이 쓰지 않으면 반환한다.</summary>
            Scene,
            /// <summary>씬을 옮겨도 반환하지 않는다.</summary>
            Persistent,
        }

        [Serializable]
        public struct Entry
        {
            public string address;
            public Kind kind;
            public Retention retention;

            public Entry(string address, Kind kind = Kind.Asset, Retention retention = Retention.Scene)
            {
                this.address = address;
                this.kind = kind;
                this.retention = retention;
            }
        }

        [Tooltip("이 목록을 쓰는 씬의 주소(씬 이름).")]
        [SerializeField] private string sceneAddress;
        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public string SceneAddress => sceneAddress;
        public IReadOnlyList<Entry> Entries => entries;

        public bool Contains(string address)
        {
            foreach (Entry entry in entries) if (entry.address == address) return true;
            return false;
        }

        public void Configure(string scene, Entry[] list)
        {
            sceneAddress = scene;
            entries = list;
        }
    }
}

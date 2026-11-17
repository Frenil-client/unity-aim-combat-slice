using System;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// 씬별 기초 리소스 목록(SceneContentSet)을 모은 표. 시작 씬(Boot)이 들고 씬 전환(SceneFlow)에 넘긴다.
    /// 전환하는 쪽이 다음 씬의 목록을 미리 알면, 씬을 여는 동안 리소스를 함께 불러오고 이미 다 있으면 로딩 화면 없이 넘어간다.
    /// </summary>
    [CreateAssetMenu(fileName = "ContentCatalog", menuName = "Aim Combat/Content Catalog")]
    public sealed class ContentCatalog : ScriptableObject
    {
        [SerializeField] private SceneContentSet[] sets = Array.Empty<SceneContentSet>();

        public SceneContentSet Find(string sceneAddress)
        {
            foreach (SceneContentSet set in sets) if (set != null && set.SceneAddress == sceneAddress) return set;
            return null;
        }

        public void Configure(SceneContentSet[] list) => sets = list;
    }
}

using System;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// 씬의 실행 리소스를 준비한 뒤 게임 오브젝트 묶음을 켠다.
    /// - 씬에는 배경과 관리자(분대, 전투 감독, HUD)만 있고, 캐릭터, 적, 데이터는 씬의 기초 리소스 목록(SceneContentSet)대로 Addressables 로 불러온다.
    ///   묶음(gameplayRoot)은 꺼진 채 저장돼 있어 준비가 끝나기 전에는 누구의 Awake 도 돌지 않는다. 단계(ContentStep)가 꽂아 넣은 뒤 켠다.
    /// - 리소스는 씬이 아니라 보관소(ContentCache)가 쥔다. 씬 전환(SceneFlow)이 미리 불러 두었으면 기다리지 않고 같은 프레임에 켜진다(다시 시작).
    ///   에디터에서 이 씬을 바로 열어 실행했을 때는 여기서 불러온다.
    /// - 준비가 끝나면 이 씬 목록에 없는 리소스(앞 씬만 쓰던 것)를 반환한다. 앞 씬은 이미 내려가 있다.
    /// - 복제한 인스턴스는 씬 오브젝트라 씬과 함께 사라진다.
    /// </summary>
    public sealed class SceneContent : MonoBehaviour
    {
        [Tooltip("이 씬의 기초 리소스 목록.")]
        [SerializeField] private SceneContentSet contentSet;
        [Tooltip("준비가 끝나면 켜는 게임 오브젝트 묶음. 씬에는 꺼진 채 저장한다.")]
        [SerializeField] private GameObject gameplayRoot;
        [Tooltip("차례로 부르는 꽂아 넣기 단계.")]
        [SerializeField] private ContentStep[] steps;

        /// <summary>모든 단계가 끝나 묶음을 켰으면 true.</summary>
        public bool Ready { get; private set; }

        public SceneContentSet ContentSet => contentSet;

        private async void Awake()
        {
            if (gameplayRoot != null) gameplayRoot.SetActive(false);
            try
            {
                // 로딩 화면 진행률의 뒤 절반(앞 절반은 씬 열기). 이미 다 있으면 기다리지 않는다.
                await ContentCache.PrepareAsync(contentSet, p => LoadingScreen.Report(0.5f + 0.5f * p));
                if (this == null) return;   // 불러오는 사이 씬이 내려갔다
                foreach (ContentStep step in steps) step.Apply();
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                LoadingScreen.Hide();
                return;
            }

            int released = ContentCache.ReleaseExcept(contentSet);
            Debug.Log($"[Content] {gameObject.scene.name} 준비 완료: 보유 {ContentCache.Count}개, 앞 씬 리소스 반환 {released}개");

            Ready = true;
            if (gameplayRoot != null) gameplayRoot.SetActive(true);
            LoadingScreen.Hide();
        }
    }
}

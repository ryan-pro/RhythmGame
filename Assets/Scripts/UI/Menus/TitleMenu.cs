using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

namespace RhythmGame.UI
{
    public sealed class TitleMenu : BaseMenu
    {
        [Header("Scene References")]
        [SerializeField]
        private RhythmEventScheduler startEvents;
        [SerializeField]
        private CanvasGroup menuCanvasGroup;

        [Header("Configuration")]
        [SerializeField]
        private string mainMenuKey = "Main";

        public override UniTask Display(CancellationToken token)
        {
            menuCanvasGroup.alpha = 0f;
            gameObject.SetActive(true);

            return startEvents.Begin(token);
        }

        public async UniTaskVoid ProcessGameStart()
        {
            var token = this.GetCancellationTokenOnDestroy();

            //TODO: Add game start logic (remote content loading, etc.)

            await UniTask.Yield();
            manager.ChangeScene(mainMenuKey, token).Forget();
        }

        public override async UniTask Hide(CancellationToken token)
        {
            await menuCanvasGroup.DOFade(0f, startEvents.SecondsPerBeat).SetEase(Ease.OutCirc).WithCancellation(token);
            gameObject.SetActive(false);
        }
    }
}

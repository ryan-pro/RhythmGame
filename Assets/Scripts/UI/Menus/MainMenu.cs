using Cysharp.Threading.Tasks;
using DG.Tweening;
using RhythmGame.GeneralAudio;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace RhythmGame.UI
{
    public class MainMenu : BaseMenu
    {
        [SerializeField]
        private CanvasGroup menuCanvasGroup;

        [SerializeField]
        private float fadeDuration = 0.8f;
        [SerializeField]
        private Ease fadeEase = Ease.InOutSine;

        private CancellationToken lifetimeToken;

        public override async UniTask Display(CancellationToken token)
        {
            lifetimeToken = token;

            menuCanvasGroup.blocksRaycasts = false;
            menuCanvasGroup.alpha = 0f;
            gameObject.SetActive(true);

            await menuCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase).WithCancellation(token);
            menuCanvasGroup.blocksRaycasts = true;
        }

        public void PlayGame()
        {
            Debug.Log("Play game.");

            //TODO: Move most logic to MenuManager
            UniTask.Void(async () =>
            {
                //TODO: Remove scene-specific dependency
                var sceneLoadTask = Addressables.LoadSceneAsync("Ingame", UnityEngine.SceneManagement.LoadSceneMode.Additive, false);

                await UniTask.WhenAll(
                    AudioSystem.StopMusic(fadeDuration, lifetimeToken),
                    manager.CloseMenus(lifetimeToken));

                var loadedScene = await sceneLoadTask;

                //TODO: Replace with addressables alternative (need load handle)
                await loadedScene.ActivateAsync().WithCancellation(lifetimeToken);

                var stage = loadedScene.Scene.FindInSceneRoot<BaseSceneController>();
                await stage.InitializeScene();

                SceneManager.UnloadSceneAsync(gameObject.scene).WithCancellation(lifetimeToken).Forget();
                stage.StartScene().Forget();
            });
        }

        public void PlayTutorial()
        {
            Debug.Log("Play tutorial.");
            //TODO
        }

        public void OpenOptions()
        {
            Debug.Log("Open options.");
            //TODO
        }

        public override async UniTask Hide(CancellationToken token)
        {
            menuCanvasGroup.blocksRaycasts = false;

            await menuCanvasGroup.DOFade(0f, fadeDuration).SetEase(fadeEase).WithCancellation(token);
            gameObject.SetActive(false);
        }
    }
}

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RhythmGame.Initialization
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField]
        private AssetReferenceScene[] backgroundScenes;
        [SerializeField]
        private AssetReferenceScene postInitializationScene;

        [Header("Internal References")]
        [SerializeField]
        private SplashScreenPlayer splashPlayer;

        private void Awake() => InitializeGame(destroyCancellationToken).Forget();

        private async UniTaskVoid InitializeGame(CancellationToken token)
        {
            //Splash
            var splashTask = splashPlayer.PlaySplashScreen();
            await Addressables.InitializeAsync();

            //Background scenes (Audio, input, data, etc.)
            await UniTask.WhenAll(backgroundScenes.Select(a => a.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Additive, token: token)));

            //Load next scene, but don't activate yet
            var loadedScene = await postInitializationScene.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Additive, false, token: token);
            await splashTask;

            await loadedScene.ActivateAsync().WithCancellation(token);
        }
    }
}

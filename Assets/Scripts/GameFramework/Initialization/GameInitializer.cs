using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

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
            var loadedScene = await SceneLoader.LoadSceneAsync(postInitializationScene, false, token);
            await splashTask;

            await loadedScene.ActivateAsync().WithCancellation(token);

            if (!loadedScene.TryFindController(out var controller))
            {
                Debug.LogError("No controller found in post-initialization scene");
                return;
            }

            await controller.InitializeScene();
            controller.StartScene().Forget();

            //Not using SceneLoader because this script is housed in the only built-in scene
            SceneManager.UnloadSceneAsync(gameObject.scene).ToUniTask().Forget();
        }
    }
}

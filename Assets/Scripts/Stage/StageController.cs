using Cysharp.Threading.Tasks;
using RhythmGame.SongModels;
using RhythmGame.UI;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythmGame
{
    /// <summary>
    /// The entry point for the stage scene; handles the lifecycle of the stage.
    /// </summary>
    public class StageController : BaseSceneController
    {
        [Header("External References")]
        [SerializeField]
        private AssetReferenceScene menuSceneRef;

        [Header("Scene References")]
        [SerializeField]
        private GameplayCoordinator gameplayCoord;

        [Header("Debug")]
        [SerializeField]
        private SongData debugSong;
        [SerializeField]
        private SongOptions debugOptions = new();

        private CancellationTokenSource stageSource;

        public SongData DebugSong => !Application.isEditor ? null : debugSong;
        public SongOptions DebugOptions => !Application.isEditor ? null : debugOptions;

        public override async UniTask InitializeScene()
        {
            await base.InitializeScene();
            await InitializeStage(debugSong, debugOptions);
        }

        public override UniTask StartScene()
        {
            //TODO: Stage presentation, effects, etc.

            //PlayStage() will run for the duration of the song, so fire and forget
            PlayStage().Forget();
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Ends the stage and transitions back to the menu.
        /// </summary>
        public async UniTask EndStage()
        {
            //TODO: Stage ending presentation stuff

            await gameplayCoord.ShowResults();
            CleanUp();

            //TODO: Streamline this process, too manual
            var sceneInstance = await menuSceneRef.LoadSceneAsync(LoadSceneMode.Additive);
            var sceneController = sceneInstance.Scene.FindInSceneRoot<BaseSceneController>();
            await sceneController.InitializeScene();

            if (sceneController is MenuManager menuManager)
                menuManager.StartingMenuKey = "Main";

            sceneController.StartScene().Forget();

            //TODO: Replace with addressables method
            SceneManager.UnloadSceneAsync(gameObject.scene).ToUniTask().Forget();
        }

        private UniTask InitializeStage(SongData data, SongOptions options)
        {
            stageSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            return gameplayCoord.InitializeGameplayComponents(data, options, stageSource.Token);
        }

        private async UniTaskVoid PlayStage()
        {
            await gameplayCoord.PlaySong();

            //TODO: End-stage presentation, loading of results screen

            await EndStage();
        }

        private void CleanUp()
        {
            stageSource?.Cancel();
            gameplayCoord.EndGameplay();
        }

        private void OnDisable() => CleanUp();

        private void OnDestroy() => stageSource?.Dispose();
    }
}
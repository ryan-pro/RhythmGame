using Cysharp.Threading.Tasks;
using RhythmGame.Songs;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class StageController : BaseSceneController
    {
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

        public void DebugStartStage()
            => InitializeScene().ContinueWith(() => StartScene()).Forget();

        public override async UniTask InitializeScene()
        {
            await base.InitializeScene();
            await InitializeStage(debugSong, debugOptions);
        }

        public override UniTask StartScene()
        {
            //TODO: Stage presentation, effects, etc.
            PlayStage().Forget();

            return UniTask.CompletedTask;
        }

        private async UniTask PlayStage()
        {
            await gameplayCoord.PlaySong();

            //TODO: End-stage presentation, loading of results screen
            Debug.Log("Stage ended!");
        }

        private UniTask InitializeStage(SongData data, SongOptions options)
        {
            stageSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            return gameplayCoord.InitializeGameplayComponents(data, options, stageSource.Token);
        }

        public void EndStage()
        {
            //TODO: Stage ending presentation stuff
            CleanUp();
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
using Cysharp.Threading.Tasks;
using RhythmGame.Songs;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class StageController : MonoBehaviour
    {
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

        public void DebugStartStage() => InitializeSong(debugSong, debugOptions).Forget();

        public async UniTask InitializeSong(SongData data, SongOptions options)
        {
            stageSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            var initTask = gameplayCoord.InitializeGameplayComponents(data, options, stageSource.Token);

            //TODO: Finish view presentation

            await initTask;
            gameplayCoord.ScheduleSong();
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
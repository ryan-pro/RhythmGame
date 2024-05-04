using Cysharp.Threading.Tasks;
using RhythmGame.Songs;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class StageController : MonoBehaviour
    {
        [SerializeField]
        private RhythmConductor conductor;
        [SerializeField]
        private SongPlayer songPlayer;
        [SerializeField]
        private TrackPlayer trackPlayer;

        [SerializeField]
        private int beatsBeforeStart = 4;

        private CancellationTokenSource stageSource;

        public async UniTask InitializeSong(SongData data, SongOptions options)
        {
            var startOffset = data.StartOffset + options.CustomOffset;
            conductor.StartConducting(data.BPM, startOffset);

            stageSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            var clipLoad = songPlayer.LoadClip(data, stageSource.Token);
            var notesLoad = trackPlayer.LoadNotes(data, options.Difficulty, stageSource.Token);
            await UniTask.WhenAll(clipLoad, notesLoad);

            //TODO: Finish view presentation

            conductor.ScheduleSongStart(beatsBeforeStart);
        }

        public void EndStage()
        {
            //TODO: Stage ending presentation stuff
            CleanUp();
        }

        private void CleanUp()
        {
            stageSource?.Cancel();
            conductor.StopConducting();
        }

        private void OnDisable() => CleanUp();

        private void OnDestroy() => stageSource?.Dispose();
    }
}
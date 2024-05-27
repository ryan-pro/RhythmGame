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
        private TrackBarView barView;

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
            InitializeComponents(data, options);

            stageSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            await LoadSongData(data, options, stageSource.Token);

            //TODO: Finish view presentation

            conductor.ScheduleSongStart();
        }

        public void InitializeComponents(SongData data, SongOptions options)
        {
            var startOffset = data.StartOffset + options.CustomOffset;
            conductor.StartConducting(data.BPM, data.BeatsPerBar, startOffset);
            barView.Initialize(data.BeatsPerBar, trackPlayer.BeatsBeforeNoteSpawn);
        }

        private UniTask LoadSongData(SongData data, SongOptions options, CancellationToken token)
        {
            var clipLoad = songPlayer.LoadClip(data, token);
            var notesLoad = trackPlayer.LoadNotes(data, options.Difficulty, token);
            return UniTask.WhenAll(clipLoad, notesLoad);
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
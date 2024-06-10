using Cysharp.Threading.Tasks;
using RhythmGame.Songs;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythmGame
{
    public class GameplayCoordinator : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField]
        private AssetReferenceScene resultsSceneRef;

        [Header("Scene References")]
        [SerializeField]
        private RhythmConductor conductor;
        [SerializeField]
        private SongPlayer songPlayer;
        [SerializeField]
        private TrackPlayer trackPlayer;

        [SerializeField]
        private TrackBarView barView;

        [Header("Configuration")]
        [SerializeField]
        private int beatsBeforeNoteSpawn = 4;
        [SerializeField]
        private float greatHitThreshold = 0.2f;
        [SerializeField]
        private float okayHitThreshold = 0.4f;

        private CancellationToken stageToken;

        public UniTask InitializeGameplayComponents(SongData data, SongOptions options, CancellationToken token)
        {
            stageToken = token;

            var songLoad = songPlayer.LoadClip(data, stageToken);
            var notesLoad = trackPlayer.LoadNotes(data, options.Difficulty, stageToken);

            var startOffset = data.StartOffset + options.CustomOffset;
            conductor.StartConducting(data.BPM, data.BeatsPerBar, startOffset);

            trackPlayer.Initialize(conductor, beatsBeforeNoteSpawn, greatHitThreshold, okayHitThreshold);
            barView.Initialize(data.BeatsPerBar, beatsBeforeNoteSpawn);

            return UniTask.WhenAll(songLoad, notesLoad);
        }

        public async UniTask PlaySong()
        {
            if (!conductor.IsStarted)
            {
                Debug.LogError("Conductor not started!");
                return;
            }

            var startTime = conductor.ScheduleNewSongStart();
            songPlayer.ScheduleSong(startTime);
            await trackPlayer.PlayScheduledSong();

            var (lastNoteBeat, songEndBeat) = trackPlayer.GetEndTimeInBeats();
            await EndSong(lastNoteBeat, songEndBeat);
        }

        private async UniTask EndSong(float lastNoteBeat, float endTimeInBeats)
        {
            if (Mathf.Approximately(endTimeInBeats, -1f))
            {
                await UniTask.WaitWhile(() => songPlayer.IsPlaying, cancellationToken: stageToken).SuppressCancellationThrow();
                return;
            }

            if (endTimeInBeats > lastNoteBeat)
            {
                var startVolume = songPlayer.SongVolume;
                var duration = endTimeInBeats - lastNoteBeat;

                while (songPlayer.IsPlaying && conductor.SongBeatPosition <= endTimeInBeats)
                {
                    songPlayer.SongVolume = Mathf.Lerp(startVolume, 0f, (conductor.SongBeatPosition - lastNoteBeat) / duration);

                    if (await UniTask.Yield(stageToken).SuppressCancellationThrow())
                        break;
                }
            }

            songPlayer.StopSong();
        }

        public async UniTask ShowResults()
        {
            trackPlayer.SetInputEnabled(false);

            var hitResults = trackPlayer.GetNoteHitCounts();
            var sceneInstance = await resultsSceneRef.LoadSceneAsync(LoadSceneMode.Additive, true, stageToken);

            var resultsController = sceneInstance.Scene.FindInSceneRoot<ResultsController>();
            resultsController.SetResults(hitResults.GreatCount, hitResults.OkayCount, hitResults.MissCount);

            await resultsController.Display();
            resultsSceneRef.UnloadSceneAsync().Forget();
        }

        public void StartPause()
        {
            if (!conductor.StartPause())
                return;

            songPlayer.StartPause();
            trackPlayer.StartPause();
        }

        public void EndPause()
        {
            if (!conductor.EndPause())
                return;

            songPlayer.EndPause();
            trackPlayer.EndPause();
        }

        public void EndGameplay()
        {
            conductor.StopConducting();
            songPlayer.UnloadClip();
            trackPlayer.UnloadNotes();
        }
    }
}

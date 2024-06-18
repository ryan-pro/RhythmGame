using Cysharp.Threading.Tasks;
using RhythmGame.SongModels;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythmGame
{
    /// <summary>
    /// Provides an interface for interacting with the gameplay components of the stage.
    /// </summary>
    public class GameplayCoordinator : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField]
        private AssetReferenceScene resultsSceneRef;    //TODO: Move to StageController; out of scope for this class

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

        /// <summary>
        /// Loads song and note data, and passes relevent data to components.
        /// </summary>
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

        /// <summary>
        /// Schedules the song to play and waits for it to finish.
        /// </summary>
        public async UniTask PlaySong()
        {
            if (!conductor.IsStarted)
            {
                Debug.LogError("Conductor not started!");
                return;
            }

            var startTime = conductor.SetSongStartTime();
            songPlayer.ScheduleSong(startTime);
            await trackPlayer.PlayNotes();

            if (stageToken.IsCancellationRequested)
                await UniTask.FromCanceled(stageToken);

            var (lastNoteBeat, songEndBeat) = trackPlayer.GetEndTimeInBeats();
            await EndSong(lastNoteBeat, songEndBeat);
        }

        /// <summary>
        /// If indicated by the song data, fades out the song before stopping it.
        /// </summary>
        /// <param name="lastNoteBeat">The beat position of the song's last note.</param>
        /// <param name="endTimeInBeats">The calculated beat position to stop the song.</param>
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

        /// <summary>
        /// Displays the results of the gameplay session and cleans up when it's closed.
        /// </summary>
        public async UniTask ShowResults()
        {
            trackPlayer.SetInputEnabled(false);

            var hitResults = trackPlayer.GetNoteHitCounts();
            var sceneInstance = await SceneLoader.LoadSceneAsync(resultsSceneRef, token: stageToken);
            var resultsController = sceneInstance.Scene.FindInSceneRoot<ResultsController>();

            resultsController.SetResults(hitResults.GreatCount, hitResults.OkayCount, hitResults.MissCount);
            await resultsController.Display();

            SceneLoader.UnloadSceneAsync(resultsSceneRef, stageToken).Forget();
        }

        /// <summary>
        /// Triggers the pause state of the gameplay components.
        /// No effect if already paused.
        /// </summary>
        public void StartPause()
        {
            if (!conductor.StartPause())
                return;

            songPlayer.StartPause();
            trackPlayer.StartPause();
        }

        /// <summary>
        /// Resumes the gameplay components from the pause state.
        /// No effect if not paused.
        /// </summary>
        public void EndPause()
        {
            if (!conductor.EndPause())
                return;

            songPlayer.EndPause();
            trackPlayer.EndPause();
        }

        /// <summary>
        /// Stops the conductor and unloads the song and note data.
        /// </summary>
        public void EndGameplay()
        {
            conductor.StopConducting();
            songPlayer.UnloadClip();
            trackPlayer.UnloadNotes();
        }
    }
}

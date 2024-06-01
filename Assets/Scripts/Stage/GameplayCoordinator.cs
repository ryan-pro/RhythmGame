using Cysharp.Threading.Tasks;
using DG.Tweening;
using RhythmGame.Songs;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class GameplayCoordinator : MonoBehaviour
    {
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

        public UniTask InitializeGameplayComponents(SongData data, SongOptions options, CancellationToken token)
        {
            var songLoad = songPlayer.LoadClip(data, token);
            var notesLoad = trackPlayer.LoadNotes(data, options.Difficulty, token);

            var startOffset = data.StartOffset + options.CustomOffset;
            conductor.StartConducting(data.BPM, data.BeatsPerBar, startOffset);

            trackPlayer.Initialize(beatsBeforeNoteSpawn, greatHitThreshold, okayHitThreshold);
            barView.Initialize(data.BeatsPerBar, beatsBeforeNoteSpawn);

            return UniTask.WhenAll(songLoad, notesLoad);
        }

        public void ScheduleSong()
        {
            if (!conductor.IsStarted)
            {
                Debug.LogError("Conductor not started!");
                return;
            }

            var startTime = conductor.ScheduleNewSongStart();
            songPlayer.ScheduleSong(startTime);
            trackPlayer.ScheduleSong(conductor);
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

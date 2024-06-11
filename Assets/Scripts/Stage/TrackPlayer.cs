using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using RhythmGame.SongModels;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RhythmGame
{
    /// <summary>
    /// Manages the spawning and tracking of notes on the tracks.
    /// </summary>
    public class TrackPlayer : MonoBehaviour
    {
        [SerializeField]
        private UnityObjectPoolBase notePrefabPool;
        [SerializeField]
        private Track[] tracks;

        private RhythmConductor conductor;
        private int beatsBeforeNoteSpawn;

        private AsyncOperationHandle<NotesMap> mapHandle;
        private NotesMap loadedMap;
        private NoteData[] notes = new NoteData[0];

        CancellationToken stageToken;

        public void Initialize(RhythmConductor conductor, int beatsBeforeNoteSpawn, float greatThreshold, float okayThreshold)
        {
            this.conductor = conductor;
            this.beatsBeforeNoteSpawn = beatsBeforeNoteSpawn;

            foreach (var track in tracks)
                track.SetScoreThresholds(greatThreshold, okayThreshold);
        }

        /// <summary>
        /// Loads the notes for the song and difficulty specified.
        /// </summary>
        public async UniTask LoadNotes(SongData songData, SongDifficulty difficulty, CancellationToken token)
        {
            stageToken = token;

            if (mapHandle.IsValid())
                Addressables.Release(mapHandle);

            mapHandle = songData.LoadNoteMap(difficulty);
            loadedMap = await mapHandle.WithCancellation(token);
            notes = loadedMap.NotesList.ToArray();

            //TODO: Determine how many notes show at most populated point
            await notePrefabPool.PopulatePool(token);
        }

        /// <summary>
        /// Clears notes cache and unloads the notes from memory.
        /// </summary>
        public void UnloadNotes()
        {
            Array.Clear(notes, 0, notes.Length);

            if (mapHandle.IsValid())
                Addressables.Release(mapHandle);
        }

        /// <summary>
        /// Plays the notes for the loaded song.
        /// </summary>
        /// <returns>Returns when the notes have finished playing.</returns>
        public async UniTask PlayNotes()
        {
            //await UniTask.WaitUntil(() => conductor.SongStartTime > 1f);
            int noteIndex = 0;

            //Acts as an update loop, filtered by changes in the song's beat position
            await foreach (var beatPos in UniTaskAsyncEnumerable.EveryValueChanged(conductor, s => s.SongBeatPosition).WithCancellation(stageToken))
            {
                if (noteIndex >= notes.Length)
                    break;

                var note = notes[noteIndex];

                if (beatPos >= note.BeatPosition - beatsBeforeNoteSpawn)
                {
                    var track = tracks[note.TrackIndex];
                    var floatPosition = (float)note.BeatPosition;
                    var startPosition = floatPosition - beatsBeforeNoteSpawn;

                    track.AddNote(startPosition, floatPosition, stageToken);
                    noteIndex++;

                    if (noteIndex >= notes.Length)
                        break;
                }
            }
        }

        public void StartPause()
        {
            //TODO: Pause trackController/set TimeScale to 0
        }

        public void EndPause()
        {
            //TODO: Resume trackController/set TimeScale to 1
        }

        /// <summary>
        /// Enables or disables input for all tracks.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            foreach (var track in tracks)
                track.InputEnabled = enabled;
        }

        /// <summary>
        /// Gets the total number of notes hit on all tracks by rating.
        /// </summary>
        public NoteHitCounts GetNoteHitCounts()
        {
            int greatCount = 0;
            int okayCount = 0;
            int missCount = 0;

            foreach (var track in tracks)
            {
                greatCount += track.NoteHits[NoteHitRating.Great];
                okayCount += track.NoteHits[NoteHitRating.Okay];
                missCount += track.NoteHits[NoteHitRating.Miss];
            }

            return new NoteHitCounts
            {
                GreatCount = greatCount,
                OkayCount = okayCount,
                MissCount = missCount
            };
        }

        /// <summary>
        /// Helper method to get the last note's beat position and the song's end beat position.
        /// </summary>
        /// <returns>Beat position of the last note, beat position of the target end time.</returns>
        public (float lastNoteBeat, float songEndBeat) GetEndTimeInBeats()
        {
            var lastNotePosition = (float)notes[notes.Length - 1].BeatPosition;

            if (!loadedMap.FadeOutOnLastNote)
                return (lastNotePosition, -1f);

            var fadeOutPosition = lastNotePosition + loadedMap.FadeOutInBeats;

            if (fadeOutPosition < conductor.SongBeatPosition)
            {
                Debug.LogError("Fade out position is before last note position.");
                return (lastNotePosition, lastNotePosition);
            }

            return (lastNotePosition, fadeOutPosition);
        }

        private void OnDestroy() => UnloadNotes();
    }

    public struct NoteHitCounts
    {
        public int GreatCount;
        public int OkayCount;
        public int MissCount;
    }
}

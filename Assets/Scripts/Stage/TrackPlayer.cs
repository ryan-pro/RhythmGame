using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using RhythmGame.Songs;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RhythmGame
{
    public class TrackPlayer : MonoBehaviour
    {
        [SerializeField]
        private UnityObjectPoolBase notePrefabPool;
        [SerializeField]
        private Track[] tracks;

        private int beatsBeforeNoteSpawn;

        private AsyncOperationHandle<NotesMap> loadedNotesMap;
        private NoteData[] notes = new NoteData[0];

        CancellationToken stageToken;

        public void Initialize(int beatsBeforeNoteSpawn, float greatThreshold, float okayThreshold)
        {
            this.beatsBeforeNoteSpawn = beatsBeforeNoteSpawn;

            foreach (var track in tracks)
                track.SetScoreThresholds(greatThreshold, okayThreshold);
        }

        public async UniTask LoadNotes(SongData songData, SongDifficulty difficulty, CancellationToken token)
        {
            stageToken = token;

            if (loadedNotesMap.IsValid())
                Addressables.Release(loadedNotesMap);

            loadedNotesMap = songData.LoadNoteMapByDifficulty(difficulty, token);
            notes = (await loadedNotesMap.WithCancellation(token)).NotesList.ToArray();

            //TODO: Determine how many notes show at most populated point
            await notePrefabPool.PopulatePool(token);
        }

        public void UnloadNotes()
        {
            Array.Clear(notes, 0, notes.Length);

            if (loadedNotesMap.IsValid())
                Addressables.Release(loadedNotesMap);
        }

        public void ScheduleSong(RhythmConductor conductor)
        {
            UpdateNotes(conductor).Forget();
        }

        private async UniTaskVoid UpdateNotes(RhythmConductor conductor)
        {
            await UniTask.WaitUntil(() => conductor.SongStartTime > 1f);

            int noteIndex = 0;

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

        private void OnDestroy() => UnloadNotes();
    }
}

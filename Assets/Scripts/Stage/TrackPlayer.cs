using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using RhythmGame.Songs;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RhythmGame
{
    public class TrackPlayer : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField]
        private UnityObjectPoolBase notePrefabPool;

        [Header("Scene References")]
        [SerializeField]
        private Track[] tracks;

        [Header("Configuration")]
        [SerializeField]
        private int beatsBeforeSpawn = 3;

        private NotesMap loadedNotesMap;
        private NoteData[] notes = new NoteData[0];

        CancellationToken stageToken;

        public async UniTask LoadNotes(SongData songData, SongDifficulty difficulty, CancellationToken token)
        {
            stageToken = token;

            if (loadedNotesMap != null)
                Addressables.Release(loadedNotesMap);

            //TODO: Move to SongData?
            loadedNotesMap = difficulty switch
            {
                SongDifficulty.Hard => await songData.HardNoteTrack.LoadAssetAsync().WithCancellation(token),
                SongDifficulty.Medium => await songData.MediumNoteTrack.LoadAssetAsync().WithCancellation(token),
                _ => await songData.EasyNoteTrack.LoadAssetAsync().WithCancellation(token)
            };

            notes = loadedNotesMap.NotesList;

            //TODO: Determine how many notes show at most populated point
            await notePrefabPool.PopulatePool(token);
        }

        public void UnloadNotes()
        {
            notes = Array.Empty<NoteData>();

            if (loadedNotesMap != null)
                Addressables.Release(loadedNotesMap);
        }

        public void ScheduleSongStart(RhythmConductor conductor)
        {
            UpdateNotes(conductor).Forget();
        }

        private async UniTaskVoid UpdateNotes(RhythmConductor conductor)
        {
            while (conductor.SongStartTime < 1f)
                await UniTask.Yield(stageToken);

            int noteIndex = 0;

            await foreach (var beatPos in UniTaskAsyncEnumerable.EveryValueChanged(conductor, s => s.SongBeatPosition).WithCancellation(stageToken))
            {
                if (noteIndex >= notes.Length)
                    break;

                var noteData = notes[noteIndex];

                if (noteData.BeatPosition <= beatPos + beatsBeforeSpawn)
                {
                    var track = tracks[noteData.TrackIndex];
                    var note = (await notePrefabPool.GetObject(tracks[noteData.TrackIndex].transform, true, stageToken)).GetComponent<NoteObject>();

                    note.InitializeNote(conductor, track.Start, track.End, beatPos, (float)noteData.BeatPosition);

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

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using RhythmGame.Songs;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

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
        private int beatsBeforeNoteSpawn = 3;
        [SerializeField]
        private float greatThreshold = 0.1f;
        [SerializeField]
        private float okayThreshold = 0.2f;

        private NotesMap loadedNotesMap;
        private NoteData[] notes = new NoteData[0];

        CancellationToken stageToken;

        public int BeatsBeforeNoteSpawn => beatsBeforeNoteSpawn;

        private void Awake()
        {
            foreach (var track in tracks)
                track.SetScoreThresholds(greatThreshold, okayThreshold);
        }

        public async UniTask LoadNotes(SongData songData, SongDifficulty difficulty, CancellationToken token)
        {
            stageToken = token;

            if (loadedNotesMap != null)
                Addressables.Release(loadedNotesMap);

            loadedNotesMap = await songData.LoadNoteMapByDifficulty(difficulty, token);
            notes = loadedNotesMap.NotesList;

            //TODO: Determine how many notes show at most populated point
            await notePrefabPool.PopulatePool(token);
        }

        public void UnloadNotes()
        {
            notes = System.Array.Empty<NoteData>();

            if (loadedNotesMap != null)
                Addressables.Release(loadedNotesMap);
        }

        public void ScheduleSongStart(RhythmConductor conductor)
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

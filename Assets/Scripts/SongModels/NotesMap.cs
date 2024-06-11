using Cysharp.Threading.Tasks.Linq;
using System.Linq;
using UnityEngine;

namespace RhythmGame.SongModels
{
    [CreateAssetMenu(menuName = "Rhythm Game/Songs/Notes Map")]
    public class NotesMap : ScriptableObject
    {
        [SerializeField]
        private NoteData[] notesList = new NoteData[0];

        [Header("Configuration")]
        public bool FadeOutOnLastNote;
        public float FadeOutInBeats = 2f;

        private bool sorted;

        public NoteData[] NotesList
        {
            get
            {
                if (!sorted)
                {
                    SortListByPosition();
                    sorted = true;
                }

                return notesList;
            }
        }

        public void SortListByPosition() => notesList = notesList.OrderBy(a => a.BeatPosition).ToArray();
    }
}

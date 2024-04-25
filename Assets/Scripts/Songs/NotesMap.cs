using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace RhythmGame.Songs
{
    [CreateAssetMenu(menuName = "Rhythm Game/Songs/Notes Map")]
    public class NotesMap : ScriptableObject
    {
        [SerializeField]
        private Note[] notesList = new Note[0];
        private bool sorted;

        public Note[] NotesList
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

        public int myInt;
    }
}

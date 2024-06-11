using UnityEngine;

namespace RhythmGame.SongModels
{
    [System.Serializable]
    public struct NoteData
    {
        [SerializeField, Min(0)]
        private int trackIndex;
        [SerializeField, Min(0)]
        private double beatPosition;

        public readonly int TrackIndex => trackIndex;
        public readonly double BeatPosition => beatPosition;
    }
}

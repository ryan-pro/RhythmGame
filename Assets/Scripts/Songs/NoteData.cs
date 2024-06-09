using UnityEngine;

namespace RhythmGame.Songs
{
    [System.Serializable]
    public struct NoteData
    {
        [SerializeField, Min(0)]
        private int trackIndex;
        [SerializeField, Min(0)]
        private double beatPosition;

        public int TrackIndex => trackIndex;
        public double BeatPosition => beatPosition;
    }
}

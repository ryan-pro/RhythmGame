using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using UnityEngine;

namespace RhythmGame.Songs
{
    [System.Serializable]
    public struct Note
    {
        [SerializeField, Min(0)]
        private int trackIndex;
        [SerializeField, Min(0)]
        private double beatPosition;

        public int TrackIndex => trackIndex;
        public double BeatPosition => beatPosition;
    }
}

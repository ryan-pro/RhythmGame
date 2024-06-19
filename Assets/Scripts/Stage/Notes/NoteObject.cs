using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    /// <summary>
    /// Represents an object (mainly a note)
    /// that moves from one point to another on the track.
    /// </summary>
    public class NoteObject : PooledObject
    {
        [SerializeField]
        private bool moveToSongBeat = true; //True for notes, false for other objects
        [SerializeField]
        private bool logHitsToConsole;

        private Transform startTransform;
        private Transform endTransform;
        private float startBeat;
        private float targetBeat;

        private bool isInitialized;
        private CancellationTokenSource movingSource;

        public float StartBeat => startBeat;
        public float TargetBeat => targetBeat;

        public void InitializeNote(RhythmConductor conductor, Transform start, Transform end, float startBeat, float targetBeat)
        {
            if (isInitialized)
                return;

            isInitialized = true;

            startTransform = start;
            endTransform = end;

            this.startBeat = startBeat;
            this.targetBeat = targetBeat;

            movingSource = new CancellationTokenSource();
            MoveToTarget(conductor, movingSource.Token).Forget();
        }

        /// <summary>
        /// Sets the hit rating of the note
        /// and returns it to the pool.
        /// </summary>
        public void SetNoteHitRating(NoteHitRating rating)
        {
            //TODO: Flesh out this method

            if (logHitsToConsole)
            {
                switch (rating)
                {
                    case NoteHitRating.Great:
                        Debug.Log("GREAT HIT!");
                        break;
                    case NoteHitRating.Okay:
                        Debug.Log("OKAY HIT!");
                        break;
                    case NoteHitRating.Miss:
                        Debug.Log("MISS!");
                        break;
                }
            }

            ReturnToPool();
        }

        private async UniTaskVoid MoveToTarget(RhythmConductor conductor, CancellationToken token)
        {
            var cachedTransform = transform;

            //Acts as an update loop, filtered by changes to either the song beat or stage beat
            await foreach (var beatPos in UniTaskAsyncEnumerable.EveryValueChanged(conductor, m => moveToSongBeat ? m.SongBeatPosition : m.StageBeatPosition).WithCancellation(token))
            {
                if (beatPos >= targetBeat + 1f)
                {
                    SetNoteHitRating(NoteHitRating.Miss);
                    break;
                }

                var t = (beatPos - startBeat) / (targetBeat - startBeat);
                cachedTransform.position = Vector3.LerpUnclamped(startTransform.position, endTransform.position, t);
            }
        }

        private void OnDisable()
        {
            movingSource?.Cancel();
            isInitialized = false;
        }
    }
}

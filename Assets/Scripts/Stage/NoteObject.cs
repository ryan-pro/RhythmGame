using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class NoteObject : MonoBehaviour
    {
        [SerializeField]
        private RectTransform rectTransform;
        [SerializeField]
        private bool moveToSongBeat = true;

        private RectTransform startTransform;
        private RectTransform endTransform;
        private float startBeat;
        private float targetBeat;

        private bool isInitialized;
        private CancellationTokenSource movingSource;

        private void Reset() => rectTransform = GetComponent<RectTransform>();

        public void InitializeNote(RhythmConductor conductor, RectTransform start, RectTransform end, float startBeat, float targetBeat)
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

        private void OnDisable()
        {
            movingSource?.Cancel();
            isInitialized = false;
        }

        private async UniTaskVoid MoveToTarget(RhythmConductor conductor, CancellationToken token)
        {
            var valueChanged = (moveToSongBeat
                ? UniTaskAsyncEnumerable.EveryValueChanged(conductor, m => m.SongBeatPosition)
                : UniTaskAsyncEnumerable.EveryValueChanged(conductor, m => m.StageBeatPosition))
                .WithCancellation(token);

            await foreach (var beatPos in valueChanged)
            {
                if(beatPos >= targetBeat + 1f)
                {
                    //Handle miss and trigger return to pool
                    break;
                }

                var t = (beatPos - startBeat) / (targetBeat - startBeat);
                rectTransform.position = Vector3.LerpUnclamped(startTransform.position, endTransform.position, t);
            }
        }
    }
}

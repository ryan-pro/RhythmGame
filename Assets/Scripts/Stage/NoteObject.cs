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

        private RectTransform startTransform;
        private RectTransform endTransform;
        private float startBeat;
        private float targetBeat;

        private bool isInitialized;
        private CancellationTokenSource movingSource;

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
            await foreach (var beatPos in UniTaskAsyncEnumerable.EveryValueChanged(conductor, m => m.SongBeatPosition).WithCancellation(token))
            {
                if(beatPos >= targetBeat + 1f)
                {
                    //Handle miss and trigger return to pool
                    break;
                }

                var t = (beatPos - startBeat) / (targetBeat - startBeat);
                rectTransform.anchoredPosition = Vector2.Lerp(startTransform.anchoredPosition, endTransform.anchoredPosition, t);
            }
        }
    }
}

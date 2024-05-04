using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class TrackBarView : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField]
        private UnityObjectPoolBase barPrefabPool;

        [Header("Scene References")]
        [SerializeField]
        private Track barTrack;
        [SerializeField]
        private RhythmConductor conductor;

        [Header("Configuration")]
        [SerializeField]
        private int trackBeatLength = 3;

        private void Awake()
        {
            var lifetimeToken = this.GetCancellationTokenOnDestroy();
            Initialize(lifetimeToken).Forget();
        }

        private async UniTaskVoid Initialize(CancellationToken token)
        {
            await barPrefabPool.PopulatePool(token);

            await UniTask.WaitUntil(() => conductor.IsStarted, cancellationToken: token);

            int lastBeat = -1;

            await foreach (var beatPos in UniTaskAsyncEnumerable.EveryValueChanged(conductor, c => c.StageBeatPosition).WithCancellation(token))
            {
                if ((int)beatPos > lastBeat)
                {
                    lastBeat = (int)beatPos;

                    var bar = (await barPrefabPool.GetObject(barTrack.transform, true, token)).GetComponent<NoteObject>();
                    bar.InitializeNote(conductor, barTrack.Start, barTrack.End, lastBeat, lastBeat + trackBeatLength);
                }
            }
        }
    }
}

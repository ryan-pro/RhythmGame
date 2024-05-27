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

        private CancellationTokenSource tokenSource;

        [Header("Debug")]
        [SerializeField]
        private bool debugMode;
        [SerializeField]
        private int debugBeatsPerBar = 4;
        [SerializeField]
        private int debugBeatsBeforeSpawn = 3;

        private void Awake()
        {
            if (debugMode)
                Initialize(debugBeatsPerBar, debugBeatsBeforeSpawn);
        }

        public void Initialize(int beatsPerBar, int beatsBeforeSpawn)
        {
            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();

            InitializeInternal(beatsPerBar, beatsBeforeSpawn, tokenSource.Token).Forget();
        }

        private async UniTaskVoid InitializeInternal(int beatsPerBar, int beatsBeforeSpawn, CancellationToken token)
        {
            await barPrefabPool.PopulatePool(token);

            await UniTask.WaitUntil(() => conductor.IsStarted, cancellationToken: token);

            int lastBeat = -1;

            await foreach (var beatPos in UniTaskAsyncEnumerable.EveryValueChanged(conductor, c => c.StageBeatPosition).WithCancellation(token))
            {
                var intPos = (int)beatPos;
                if (intPos > lastBeat)
                {
                    lastBeat = intPos;

                    if ((intPos + beatsBeforeSpawn) % beatsPerBar == 0)
                        barTrack.AddNote(lastBeat, lastBeat + beatsBeforeSpawn, token);
                }
            }
        }

        private void OnDestroy() => tokenSource?.Cancel();
    }
}

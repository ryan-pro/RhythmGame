using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    /// <summary>
    /// Spawns and manages bar objects that move down the track.
    /// </summary>
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

        public void Initialize(int beatsPerBar, int beatsBeforeSpawn)
        {
            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();

            InitializeInternal(beatsPerBar, beatsBeforeSpawn, tokenSource.Token).Forget();
        }

        private async UniTaskVoid InitializeInternal(int beatsPerBar, int beatsBeforeSpawn, CancellationToken token)
        {
            await barPrefabPool.PopulatePool(token);
            //await UniTask.WaitUntil(() => conductor.IsStarted, cancellationToken: token);

            int lastBeat = -1;

            //Acts as an update loop, filtered by changes in the stage's beat position
            await foreach (float beatPos in UniTaskAsyncEnumerable.EveryValueChanged(conductor, c => c.StageBeatPosition).WithCancellation(token))
            {
                //Only spawn a bar object when the beat position is a new whole number
                var intPos = (int)beatPos;

                if (intPos > lastBeat)
                {
                    lastBeat = intPos;

                    //If a bar is coming up, spawn it
                    if ((intPos + beatsBeforeSpawn) % beatsPerBar == 0)
                        barTrack.AddNote(lastBeat, lastBeat + beatsBeforeSpawn, token);
                }
            }
        }

        private void OnDestroy() => tokenSource?.Cancel();
    }
}

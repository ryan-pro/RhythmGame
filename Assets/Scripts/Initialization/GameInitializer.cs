using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RhythmGame
{
    public class GameInitializer : MonoBehaviour
    {
        private void Awake() => InitializeGame(destroyCancellationToken).Forget();

        private async UniTaskVoid InitializeGame(CancellationToken token)
        {
            await Addressables.InitializeAsync();

            //TODO
        }
    }
}

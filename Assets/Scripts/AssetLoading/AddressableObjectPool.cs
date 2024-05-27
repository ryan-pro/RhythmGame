using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public abstract class AddressableObjectPool : UnityObjectPoolBase
    {
        [Header("Pool Settings")]
        [SerializeField]
        private AssetReferencePooledObject prefabRef;

        private PooledObject loadedAsset;

        private readonly Queue<PooledObject> availableObjects = new();
        private readonly HashSet<PooledObject> allObjects = new();

        private async UniTask LoadPrefab(CancellationToken token)
            => loadedAsset = await prefabRef.LoadAssetAsync().WithCancellation(token);

        public override async UniTask PopulatePool(int minimumCount, CancellationToken token)
        {
            if (loadedAsset == null)
                await LoadPrefab(token);

            while (allObjects.Count < minimumCount)
            {
                var newObject = Instantiate(loadedAsset, poolParent);
                newObject.InitPooledObject(this);

                allObjects.Add(newObject);
                availableObjects.Enqueue(newObject);
            }
        }

        public override async UniTask<GameObject> GetObject(Transform newParent, bool activateObject, CancellationToken token)
        {
            if (!availableObjects.TryDequeue(out var toReturn))
            {
                if (loadedAsset == null)
                    await LoadPrefab(token);

                toReturn = Instantiate(loadedAsset, poolParent);
                toReturn.InitPooledObject(this);
                allObjects.Add(toReturn);
            }

            toReturn.transform.SetParent(newParent, false);
            toReturn.gameObject.SetActive(activateObject);

            return toReturn.gameObject;
        }

        public override void ReturnObject(PooledObject toReturn)
        {
            if (!allObjects.Contains(toReturn))
            {
                Debug.LogError($"Object {toReturn.name} not found in pool");
                return;
            }

            toReturn.gameObject.SetActive(false);
            toReturn.transform.SetParent(poolParent);
            toReturn.transform.localPosition = Vector3.zero;
        }

        public override void ClearPool()
        {
            foreach (var obj in allObjects)
                Destroy(obj.gameObject);

            availableObjects.Clear();
            allObjects.Clear();

            if (loadedAsset != null)
            {
                loadedAsset = null;
                prefabRef.ReleaseAsset();
            }
        }
    }
}

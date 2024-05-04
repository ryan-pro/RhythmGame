using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RhythmGame
{
    public abstract class AddressableObjectPoolT<T> : UnityObjectPoolBase where T : AssetReference
    {
        [Header("Pool Settings")]
        [SerializeField]
        private T prefabRef;

        private GameObject loadedAsset;
        private readonly List<PooledObject> pool = new();

        private async UniTask LoadPrefab(CancellationToken token)
        {
            loadedAsset = await prefabRef.LoadAssetAsync<GameObject>().WithCancellation(token);
        }

        public override async UniTask PopulatePool(int minimumCount, CancellationToken token)
        {
            if (loadedAsset == null)
                await LoadPrefab(token);

            while (pool.Count < minimumCount)
            {
                var newObject = Instantiate(loadedAsset, poolParent);
                newObject.SetActive(false);
                pool.Add(new PooledObject(newObject));
            }
        }

        public override async UniTask<GameObject> GetObject(Transform newParent, bool activateObject, CancellationToken token)
        {
            PooledObject toReturn = pool.Find(a => !a.InUse);

            if (toReturn == null)
            {
                if (loadedAsset == null)
                    await LoadPrefab(token);

                toReturn = new PooledObject(Instantiate(loadedAsset, poolParent));
                pool.Add(toReturn);
            }

            toReturn.InUse = true;
            toReturn.Object.transform.SetParent(newParent, false);
            toReturn.Object.SetActive(activateObject);

            return toReturn.Object;
        }

        public override void ReturnObject(GameObject toReturn)
        {
            var pooledObject = pool.Find(a => a.Object == toReturn);

            if (pooledObject == null)
            {
                Debug.LogError($"Object {toReturn.name} not found in pool");
                return;
            }

            pooledObject.Object.SetActive(false);
            pooledObject.Object.transform.SetParent(poolParent);
            pooledObject.Object.transform.localPosition = Vector3.zero;

            pooledObject.InUse = false;
        }

        public override void ClearPool()
        {
            for (int i = pool.Count - 1; i >= 0; i--)
            {
                Destroy(pool[i].Object);
                pool.RemoveAt(i);
            }

            if (loadedAsset != null)
            {
                loadedAsset = null;
                prefabRef.ReleaseAsset();
            }
        }
    }

    public class AddressableGameObjectPool : AddressableObjectPoolT<AssetReferenceGameObject> { }

    public class AddressableNoteObjectPool : AddressableObjectPoolT<AssetReferenceNoteObject> { }
}

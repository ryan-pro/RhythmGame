using Cysharp.Threading.Tasks;
using System.Collections.Generic;
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

        private async UniTask LoadPrefab()
        {
            var lifetimeToken = this.GetCancellationTokenOnDestroy();

            loadedAsset = await prefabRef.LoadAssetAsync<GameObject>().WithCancellation(lifetimeToken);
        }

        public override async UniTask PopulatePool(int minimumCount)
        {
            if (loadedAsset == null)
                await LoadPrefab();

            while (pool.Count < minimumCount)
            {
                var newObject = Instantiate(loadedAsset, poolParent);
                newObject.SetActive(false);
                pool.Add(new PooledObject(newObject));
            }
        }

        public override async UniTask<GameObject> GetObject(Transform newParent, bool activateObject)
        {
            PooledObject toReturn = pool.Find(a => !a.InUse);

            if (toReturn == null)
            {
                if (loadedAsset == null)
                    await LoadPrefab();

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

    public class AddressableObjectPool : AddressableObjectPoolT<AssetReferenceGameObject> { }
}

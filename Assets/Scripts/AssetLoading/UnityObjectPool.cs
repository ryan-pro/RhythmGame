using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGame
{
    public class UnityObjectPool : UnityObjectPoolBase
    {
        [Header("Pool Settings")]
        [SerializeField]
        private GameObject objectPrefab;

        private readonly List<PooledObject> pool = new();

        public override UniTask PopulatePool(int minimumCount)
        {
            while (pool.Count < minimumCount)
            {
                var newObject = Instantiate(objectPrefab, poolParent);
                newObject.SetActive(false);
                pool.Add(new PooledObject(newObject));
            }

            return UniTask.CompletedTask;
        }

        public override UniTask<GameObject> GetObject(Transform newParent, bool activateObject)
        {
            PooledObject toReturn = pool.Find(a => !a.InUse);

            if (toReturn == null)
            {
                toReturn = new PooledObject(Instantiate(objectPrefab, poolParent));
                pool.Add(toReturn);
            }

            toReturn.InUse = true;
            toReturn.Object.SetActive(activateObject);
            toReturn.Object.transform.SetParent(newParent, false);

            return UniTask.FromResult(toReturn.Object);
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
        }
    }
}

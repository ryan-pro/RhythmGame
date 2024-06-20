using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class UnityObjectPool : UnityObjectPoolBase
    {
        [Header("Pool Settings")]
        [SerializeField]
        private PooledObject objectPrefab;

        private readonly Queue<PooledObject> availableObjects = new();
        private readonly HashSet<PooledObject> allObjects = new();

        public override UniTask PopulatePool(int minimumCount, CancellationToken token)
        {
            while (allObjects.Count < minimumCount)
            {
                var newObject = Instantiate(objectPrefab, poolParent);
                newObject.InitializePooledObject(this);

                allObjects.Add(newObject);
                availableObjects.Enqueue(newObject);
            }

            return UniTask.CompletedTask;
        }

        public override UniTask<GameObject> GetObject(Transform newParent, bool activateObject, CancellationToken token)
        {
            if (!availableObjects.TryDequeue(out var toReturn))
            {
                toReturn = Instantiate(objectPrefab, poolParent);
                toReturn.InitializePooledObject(this);
                allObjects.Add(toReturn);
            }

            toReturn.gameObject.SetActive(activateObject);
            toReturn.transform.SetParent(newParent, false);

            return UniTask.FromResult(toReturn.gameObject);
        }

        public override void ReturnObject(PooledObject toReturn)
        {
            if (!allObjects.Contains(toReturn))
            {
                Debug.LogError($"Object {toReturn.name} not found in pool");
                return;
            }

            toReturn.gameObject.SetActive(false);
            toReturn.transform.SetParent(poolParent, false);
            toReturn.transform.localPosition = Vector3.zero;

            availableObjects.Enqueue(toReturn);
        }

        public override void ClearPool()
        {
            foreach (var obj in allObjects)
                Destroy(obj.gameObject);

            availableObjects.Clear();
            allObjects.Clear();
        }
    }
}

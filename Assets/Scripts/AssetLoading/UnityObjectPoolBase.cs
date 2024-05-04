using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class PooledObject
    {
        public GameObject Object { get; }
        public bool InUse { get; set; }

        public PooledObject(GameObject obj)
        {
            Object = obj;
            InUse = false;
        }
    }

    [RequireComponent(typeof(Transform))]
    public abstract class UnityObjectPoolBase : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField]
        protected Transform poolParent;

        [SerializeField]
        protected bool populateOnAwake;
        [SerializeField]
        protected int initialPoolSize = 10;

        protected void Reset() => poolParent = transform;

        protected void Awake()
        {
            if (populateOnAwake)
                PopulatePool(initialPoolSize);
        }

        public abstract UniTask PopulatePool(int minimumCount);

        public abstract UniTask<GameObject> GetObject(Transform newParent, bool activateObject);
        public abstract void ReturnObject(GameObject toReturn);

        public abstract void ClearPool();
    }
}

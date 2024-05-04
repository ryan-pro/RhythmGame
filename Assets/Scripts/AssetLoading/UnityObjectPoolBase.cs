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
            {
                var lifetimeToken = this.GetCancellationTokenOnDestroy();
                PopulatePool(initialPoolSize, lifetimeToken);
            }
        }

        public virtual UniTask PopulatePool(CancellationToken token) => PopulatePool(initialPoolSize, token);

        public abstract UniTask PopulatePool(int minimumCount, CancellationToken token);

        public abstract UniTask<GameObject> GetObject(Transform newParent, bool activateObject, CancellationToken token);
        public abstract void ReturnObject(GameObject toReturn);

        public abstract void ClearPool();
    }
}

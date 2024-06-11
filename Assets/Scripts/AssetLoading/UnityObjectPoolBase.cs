using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    /// <summary>
    /// Base class for object pools that use Unity's GameObjects.
    /// </summary>
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

        /// <summary>
        /// Populates the pool to the initial pool size.
        /// </summary>
        public virtual UniTask PopulatePool(CancellationToken token) => PopulatePool(initialPoolSize, token);

        /// <summary>
        /// Populates the pool with a minimum number of objects.
        /// </summary>
        /// <param name="minimumCount">The mimimum amount to populate.
        /// If pool already has more than this amount, nothing happens.</param>
        public abstract UniTask PopulatePool(int minimumCount, CancellationToken token);

        /// <summary>
        /// Gets an object from the pool.
        /// If none are available, one is instantiated and added to the pool.
        /// </summary>
        /// <param name="newParent">Pooled object will be parented to this transform.</param>
        /// <param name="activateObject">Whether the GameObject be activated on check-out.</param>
        public abstract UniTask<GameObject> GetObject(Transform newParent, bool activateObject, CancellationToken token);

        /// <summary>
        /// Returns an object to the pool and deactivates it.
        /// Objects that don't belong in the pool will be ignored.
        /// </summary>
        public abstract void ReturnObject(PooledObject toReturn);

        /// <summary>
        /// Destroys all objects in the pool and then clears it.
        /// </summary>
        public abstract void ClearPool();
    }
}

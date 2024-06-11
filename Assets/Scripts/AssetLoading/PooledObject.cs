using UnityEngine;

namespace RhythmGame
{
    /// <summary>
    /// Base class for objects that can be pooled.
    /// </summary>
    public abstract class PooledObject : MonoBehaviour
    {
        protected UnityObjectPoolBase pool;

        /// <summary>
        /// Initializes the pooled object with a reference to the pool that created it.
        /// </summary>
        public void InitializePooledObject(UnityObjectPoolBase pool)
        {
            this.pool = pool;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Returns the object to the pool.
        /// </summary>
        public void ReturnToPool() => pool.ReturnObject(this);
    }
}

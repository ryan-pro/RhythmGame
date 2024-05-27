using UnityEngine;

namespace RhythmGame
{
    public abstract class PooledObject : MonoBehaviour
    {
        protected UnityObjectPoolBase pool;

        public void InitPooledObject(UnityObjectPoolBase pool)
        {
            this.pool = pool;
            gameObject.SetActive(false);
        }

        public void ReturnToPool() => pool.ReturnObject(this);
    }
}

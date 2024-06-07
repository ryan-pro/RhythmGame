using Cysharp.Threading.Tasks;
using RhythmGame.GeneralAudio;
using UnityEngine;

namespace RhythmGame
{
    public abstract class BaseSceneController : MonoBehaviour
    {
        [Header("Base Scene Controller")]
        [SerializeField]
        private bool debugStartSceneOnAwake = true;

        public bool IsInitialized { get; protected set; }

        protected virtual void Awake()
        {
            //If audio system isn't initialized before scene loads,
            //assume we're playing from editor and initialize the scene

            if (!AudioSystem.IsInitialized && debugStartSceneOnAwake)
                InitializeScene().ContinueWith(() => StartScene()).Forget();
        }

        public virtual UniTask InitializeScene()
        {
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public abstract UniTask StartScene();
    }
}

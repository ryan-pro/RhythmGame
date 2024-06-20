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

        //For instances where we want to start the scene from the editor
        protected virtual void Awake()
        {
            //If audio system isn't initialized before scene loads,
            //assume we're playing from editor and initialize the scene

            if (!AudioSystem.IsInitialized && debugStartSceneOnAwake)
                InitializeScene().ContinueWith(() => StartScene()).Forget();
        }

        /// <summary>
        /// Handles any loading or setup that needs to be done before the scene starts.
        /// </summary>
        public virtual UniTask InitializeScene()
        {
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Executes the start of the scene.
        /// Should return once the scene is ready to be interacted with.
        /// </summary>
        public abstract UniTask StartScene();   //Should return when the scene is ready to be interacted with
    }
}

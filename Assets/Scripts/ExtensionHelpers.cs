using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace RhythmGame
{
    public static class ExtensionHelpers
    {
        /// <summary>
        /// Finds a component of type T in the scene's root game objects.
        /// </summary>
        public static T FindInSceneRoot<T>(this Scene scene) where T : Component
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return null;

            T result = null;
            foreach (var obj in scene.GetRootGameObjects())
            {
                if (obj.TryGetComponent(out result))
                    break;
            }

            return result;
        }

        public static bool FindInSceneRoot<T>(this Scene scene, out T result) where T : Component
        {
            result = scene.FindInSceneRoot<T>();
            return result != null;
        }

        public static BaseSceneController FindController(this SceneInstance sceneInstance)
            => sceneInstance.Scene.FindInSceneRoot<BaseSceneController>();

        public static bool TryFindController(this SceneInstance sceneInstance, out BaseSceneController controller)
        {
            controller = sceneInstance.Scene.FindInSceneRoot<BaseSceneController>();
            return controller != null;
        }

        public static async UniTask<BaseSceneController> InitializeController(this SceneInstance sceneInstance)
        {
            var controller = sceneInstance.Scene.FindInSceneRoot<BaseSceneController>();

            if (controller == null)
            {
                Debug.LogError($"No controller found in scene {sceneInstance.Scene.name}!");
                return null;
            }

            await controller.InitializeScene();
            return controller;
        }
    }
}

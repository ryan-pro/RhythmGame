using UnityEngine;
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
                return default;

            T result = default;
            foreach (var obj in scene.GetRootGameObjects())
            {
                if (obj.TryGetComponent(out result))
                    break;
            }

            return result;
        }
    }
}

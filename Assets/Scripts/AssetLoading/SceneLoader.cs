using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace RhythmGame
{
    public class SceneLoader : MonoBehaviour
    {
        private const string sceneKey = "SceneLoader";
        private static SceneLoader instance;

        [SerializeField]
        private List<SerializedKeyValue<string, AssetReferenceScene>> stringSceneMap;

        private Dictionary<string, string> _sceneMap;
        private Dictionary<string, string> SceneMap
        {
            get
            {
                _sceneMap ??= stringSceneMap
                    .Where(a => a.Value.InternalReference.RuntimeKeyIsValid())
                    .ToDictionary(k => k.Key, v => v.Value.InternalReference.RuntimeKey.ToString());

                return _sceneMap;
            }
        }

        private readonly Dictionary<object, AsyncOperationHandle<SceneInstance>> keyHandleLoadedScenes = new();

        public static async UniTask<SceneLoader> GetInstance(CancellationToken token)
        {
            if (instance == null)
            {
                var loadedInstance = await Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive).WithCancellation(token);

                if (instance == null)
                    instance = loadedInstance.Scene.FindInSceneRoot<SceneLoader>();
                else
                    Addressables.UnloadSceneAsync(loadedInstance).ToUniTask().Forget();
            }

            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        private void OnDisable()
        {
            foreach (var handle in keyHandleLoadedScenes.Values)
            {
                if (handle.IsValid())
                    Addressables.UnloadSceneAsync(handle).ToUniTask().Forget();
            }

            keyHandleLoadedScenes.Clear();
        }

        public static async UniTask<SceneInstance> LoadSceneAsync(string sceneName, bool activateOnLoad = true, CancellationToken token = default)
        {
            var loader = await GetInstance(token);

            if (!loader.SceneMap.TryGetValue(sceneName, out var runtimeKey))
            {
                Debug.LogError($"Scene {sceneName} not found!");
                return default;
            }

            if (loader.keyHandleLoadedScenes.TryGetValue(runtimeKey, out var handle) && SceneManager.GetSceneByName(sceneName).IsValid())
            {
                Debug.LogWarning($"Scene {sceneName} is already loaded.");
                await UniTask.WaitUntil(() => handle.IsDone, cancellationToken: token);
                return handle.Result;
            }

            var newHandle = Addressables.LoadSceneAsync(runtimeKey, LoadSceneMode.Additive, activateOnLoad);
            loader.keyHandleLoadedScenes[sceneName] = newHandle;

            return await newHandle.WithCancellation(token);
        }

        public static async UniTask<SceneInstance> LoadSceneAsync(AssetReferenceScene sceneRef, bool activateOnLoad = true, CancellationToken token = default)
        {
            if (!sceneRef.InternalReference.RuntimeKeyIsValid())
            {
                Debug.LogError("Provided AssetReferenceScene not valid!");
                return default;
            }

            var loader = await GetInstance(token);
            var runtimeKey = sceneRef.InternalReference.RuntimeKey.ToString();

            if (!loader.SceneMap.Values.Any(a => a == runtimeKey))
            {
                Debug.LogError($"Scene with RuntimeKey {runtimeKey} not found!");
                return default;
            }

            if (loader.keyHandleLoadedScenes.TryGetValue(runtimeKey, out var handle) && handle.IsValid())
            {
                Debug.LogWarning($"Scene with RuntimeKey {runtimeKey} is already loaded.");
                await UniTask.WaitUntil(() => handle.IsDone, cancellationToken: token);
                return handle.Result;
            }

            var newHandle = Addressables.LoadSceneAsync(runtimeKey, LoadSceneMode.Additive, activateOnLoad);
            loader.keyHandleLoadedScenes[runtimeKey] = newHandle;

            return await newHandle.WithCancellation(token);
        }

        public static async UniTask<BaseSceneController> LoadInitControllerSceneAsync(string sceneName, CancellationToken token = default)
        {
            var sceneInstance = await LoadSceneAsync(sceneName, true, token);
            return await sceneInstance.InitializeController();
        }

        public static async UniTask<BaseSceneController> LoadInitControllerSceneAsync(AssetReferenceScene sceneRef, CancellationToken token = default)
        {
            var sceneInstance = await LoadSceneAsync(sceneRef, true, token);
            return await sceneInstance.InitializeController();
        }

        public static UniTask UnloadSceneAsync(Scene sceneObj, CancellationToken token = default) => UnloadSceneAsync(sceneObj.name, token);

        public static async UniTask UnloadSceneAsync(string sceneName, CancellationToken token = default)
        {
            var loader = await GetInstance(token);

            if (!loader.SceneMap.TryGetValue(sceneName, out var runtimeKey))
            {
                Debug.LogError($"Scene {sceneName} not found!");
                return;
            }

            if (!loader.keyHandleLoadedScenes.TryGetValue(runtimeKey, out var handle) || !handle.IsValid())
            {
                Debug.LogWarning($"Scene {sceneName} is not loaded.");
                return;
            }

            loader.keyHandleLoadedScenes.Remove(runtimeKey);
            await Addressables.UnloadSceneAsync(handle).ToUniTask();
        }

        public static async UniTask UnloadSceneAsync(AssetReferenceScene sceneRef, CancellationToken token = default)
        {
            if (!sceneRef.InternalReference.RuntimeKeyIsValid())
            {
                Debug.LogError("Provided AssetReferenceScene not valid!");
                return;
            }

            var loader = await GetInstance(token);
            var runtimeKey = sceneRef.InternalReference.RuntimeKey.ToString();

            if (loader.SceneMap.Values.All(a => a != runtimeKey))
            {
                Debug.LogError($"Scene with RuntimeKey {runtimeKey} not found!");
                return;
            }

            if (!loader.keyHandleLoadedScenes.TryGetValue(runtimeKey, out var handle) || !handle.IsValid())
            {
                Debug.LogWarning($"Scene with RuntimeKey {runtimeKey} is not loaded.");
                return;
            }

            loader.keyHandleLoadedScenes.Remove(runtimeKey);
            await Addressables.UnloadSceneAsync(handle).ToUniTask();
        }

#if UNITY_EDITOR

        public void SetSceneReference(string key, AssetReferenceScene sceneRef)
        {
            var entry = stringSceneMap.Find(a => a.Key == key);

            if (entry != null)
            {
                entry.Value = sceneRef;
                return;
            }

            entry = new SerializedKeyValue<string, AssetReferenceScene>(key, sceneRef);
            stringSceneMap.Add(entry);
        }

#endif
    }
}

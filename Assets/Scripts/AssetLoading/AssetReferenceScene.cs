using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace RhythmGame
{
    [System.Serializable]
    public class AssetReferenceScene
    {
        [SerializeField]
        [AssetReferenceUILabelRestriction("scene")]
        private AssetReference sceneReference = new();

        private AsyncOperationHandle<SceneInstance> sceneOperation;

        public AssetReference InternalReference => sceneReference;
        public Scene Scene => sceneOperation.Result.Scene;
        public bool IsValidAsset => sceneReference.RuntimeKeyIsValid();
        public bool IsLoaded => sceneOperation.IsValid() && sceneOperation.IsDone;

        public UniTask<SceneInstance> LoadSceneAsync(LoadSceneMode loadMode, bool activateOnLoad = true, CancellationToken token = default)
        {
            if (IsLoaded)
                return UniTask.FromResult(sceneOperation.Result);

            if (!sceneOperation.IsValid())
                sceneOperation = sceneReference.LoadSceneAsync(loadMode, activateOnLoad);

            return sceneOperation.WithCancellation(token);
        }

        public UniTask ActivateSceneAsync(CancellationToken token = default)
        {
            if (IsLoaded)
                return sceneOperation.Result.ActivateAsync().WithCancellation(token);

            return UniTask.CompletedTask;
        }

        public async UniTask UnloadSceneAsync(CancellationToken token = default)
        {
            if (!sceneOperation.IsValid())
                return;

            if (!sceneOperation.IsDone)
                await sceneOperation.WithCancellation(token);

            sceneOperation = default;
            await sceneReference.UnLoadScene().WithCancellation(token);
        }

#if UNITY_EDITOR

        private Object cachedRef;

        public bool CheckReferenceValueChanged()
        {
            if(sceneReference.editorAsset == null || sceneReference.editorAsset == cachedRef)
                return false;

            cachedRef = sceneReference.editorAsset;
            return true;
        }

        public void UpdateReferenceValue()
        {
            //Check if the AssetReference is a scene, and nullify it if it's not
            if (sceneReference.editorAsset is UnityEditor.SceneAsset)
                return;

            Debug.LogError($"Cannot set {sceneReference.editorAsset.name}; it is not a scene asset.");
            sceneReference.SetEditorAsset(null);
            cachedRef = null;
        }
#endif
    }
}

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace RhythmGame
{
    [CreateAssetMenu(menuName = "Rhythm Game/Loadable Audio List")]
    public class LoadableAudioList : ScriptableObject
    {
        [SerializeField]
        private SerializedKeyValue<string, AssetReferenceAudioClip>[] referenceList = new SerializedKeyValue<string, AssetReferenceAudioClip>[0];

        private Dictionary<string, AssetReferenceAudioClip> _clipsMap;
        private Dictionary<string, AssetReferenceAudioClip> ClipsMap
            => _clipsMap ??= referenceList.ToDictionary(a => a.Key, a => a.Value);

        private AssetReferenceAudioClip[] _clipsList;
        private AssetReferenceAudioClip[] ClipsList
            => referenceList.Select(a => a.Value).ToArray();

        private AsyncLazy preloadTask;

        public UniTask<AudioClip> GetClip(string name, CancellationToken token)
        {
            if (!ClipsMap.TryGetValue(name, out var clipRef))
            {
                Debug.LogError($"Clip with name {name} not found in list.");
                return UniTask.FromResult<AudioClip>(null);
            }

            return GetClipInternal(clipRef, token);
        }

        public UniTask<AudioClip> GetClip(int index, CancellationToken token)
        {
            if (index < 0 || index >= ClipsList.Length)
            {
                Debug.LogError($"Provided index of {index} outside bounds of the list.");
                return UniTask.FromResult<AudioClip>(null);
            }

            return GetClipInternal(ClipsList[index], token);
        }

        private UniTask<AudioClip> GetClipInternal(AssetReferenceAudioClip clipRef, CancellationToken token)
        {
            if (clipRef.OperationHandle.IsValid())
            {
                if (clipRef.OperationHandle.IsDone)
                    return UniTask.FromResult((AudioClip)clipRef.Asset);

                return clipRef.OperationHandle.Convert<AudioClip>().WithCancellation(token);
            }

            return clipRef.LoadAssetAsync().WithCancellation(token);
        }

        public void UnloadClip(string name)
        {
            if (!ClipsMap.TryGetValue(name, out var clipRef))
            {
                Debug.LogError($"Clip with name {name} not found in list.");
                return;
            }

            if (clipRef.OperationHandle.IsValid())
                clipRef.ReleaseAsset();
        }

        public void UnloadClip(int index)
        {
            if (index < 0 || index >= ClipsList.Length)
            {
                Debug.LogError($"Provided index of {index} outside bounds of the list.");
                return;
            }

            var clipRef = ClipsList[index];

            if (clipRef.OperationHandle.IsValid())
                clipRef.ReleaseAsset();
        }

        public UniTask PreloadAllClips(CancellationToken token)
        {
            if (referenceList.Length == 0)
            {
                Debug.Log(name + ": No AudioClips to load.");
                return UniTask.CompletedTask;
            }

            preloadTask ??= new AsyncLazy(() => UniTask.WhenAll(referenceList.Select(a => a.Value.LoadAssetAsync().WithCancellation(token))));
            return preloadTask.Task;
        }

        public void UnloadAllClips()
        {
            foreach (var entry in referenceList)
            {
                if (entry.Value.OperationHandle.IsValid())
                    entry.Value.ReleaseAsset();
            }
        }

        private void OnDestroy() => UnloadAllClips();
    }
}

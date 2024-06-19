using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    [CreateAssetMenu(menuName = "Rhythm Game/Loadable Audio List")]
    public class LoadableAudioList : ScriptableObject
    {
        [SerializeField]
        private SerializedKeyValue<string, AssetReferenceAudioClip>[] clipsList = new SerializedKeyValue<string, AssetReferenceAudioClip>[0];

        [NonSerialized]
        public Dictionary<string, AudioClip> LoadedClipsMap = new();
        [NonSerialized]
        public AudioClip[] LoadedClips = new AudioClip[0];

        private UniTask<AudioClip[]> loadTask;
        private CancellationTokenSource loadTokenSource;
        public bool IsLoaded => loadTask.Status.IsCompletedSuccessfully();

        public async UniTask LoadClips()
        {
            if (clipsList.Length == 0 || loadTokenSource != null)
                return;

            loadTokenSource = new CancellationTokenSource();
            loadTask = UniTask.WhenAll(clipsList.Select(a => a.Value.LoadAssetAsync().WithCancellation(loadTokenSource.Token)));

            LoadedClipsMap.EnsureCapacity(clipsList.Length);
            Array.Resize(ref LoadedClips, clipsList.Length);

            LoadedClips = await loadTask;

            for (int i = 0; i < clipsList.Length; i++)
                LoadedClipsMap[clipsList[i].Key] = LoadedClips[i];
        }

        public void UnloadClips()
        {
            loadTokenSource?.Cancel();

            LoadedClipsMap.Clear();
            Array.Clear(LoadedClips, 0, LoadedClips.Length);

            foreach (var entry in clipsList)
            {
                if (entry.Value.OperationHandle.IsValid())
                    entry.Value.ReleaseAsset();
            }
        }
    }
}

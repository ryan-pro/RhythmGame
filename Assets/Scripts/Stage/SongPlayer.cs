using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RhythmGame
{
    [RequireComponent(typeof(AudioSource))]
    public class SongPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioSource songSource;

        private AsyncOperationHandle<AudioClip> loadedClip;

        private void Reset() => songSource = GetComponent<AudioSource>();

        public async UniTask LoadClip(SongData songData, CancellationToken token)
        {
            if(loadedClip.IsValid())
                Addressables.Release(loadedClip);

            loadedClip = songData.AudioClip.LoadAssetAsync();
            songSource.clip = await loadedClip.WithCancellation(token);
        }

        public void UnloadClip()
        {
            songSource.clip = null;

            if (loadedClip.IsValid() && Application.isPlaying)
                Addressables.Release(loadedClip);
        }

        public void ScheduleSong(float dspTime)
        {
            songSource.PlayScheduled(dspTime);
            Debug.Log($"Scheduled song start in {AudioSettings.dspTime - dspTime:N2} seconds!");
        }

        public void StartPause() => songSource.Pause();
        public void EndPause() => songSource.Play();

        private void OnDestroy() => UnloadClip();
    }
}

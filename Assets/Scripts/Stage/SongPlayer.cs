using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RhythmGame
{
    [RequireComponent(typeof(AudioSource))]
    public class SongPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioSource songSource;

        private AudioClip loadedClip;

        private void Reset() => songSource = GetComponent<AudioSource>();

        public async UniTask LoadClip(SongData songData, CancellationToken token)
        {
            if(loadedClip != null)
                Addressables.Release(loadedClip);

            loadedClip = await songData.AudioClip.LoadAssetAsync().WithCancellation(token);
            songSource.clip = loadedClip;
        }

        public void UnloadClip()
        {
            songSource.clip = null;

            if (loadedClip != null)
                Addressables.Release(loadedClip);
        }

        public void ScheduleSongStart(float dspTime)
        {
            songSource.PlayScheduled(dspTime);
            Debug.Log($"Scheduled song start in {AudioSettings.dspTime - dspTime:N2} seconds!");
        }

        public void StartPause() => songSource.Pause();
        public void EndPause() => songSource.Play();

        private void OnDestroy() => UnloadClip();
    }
}

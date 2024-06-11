using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RhythmGame
{
    /// <summary>
    /// Manages the playback and lifecycle of a song's audio clip.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SongPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioSource songSource;

        private AsyncOperationHandle<AudioClip> loadedClip;

        public bool IsPlaying => songSource.isPlaying;

        public float SongVolume
        {
            get => songSource.volume;
            set => songSource.volume = value;
        }

        private void Reset() => songSource = GetComponent<AudioSource>();

        /// <summary>
        /// Loads the audio clip for the song data provided.
        /// </summary>
        public async UniTask LoadClip(SongData songData, CancellationToken token)
        {
            if(loadedClip.IsValid())
                Addressables.Release(loadedClip);

            loadedClip = songData.AudioClip.LoadAssetAsync();
            songSource.clip = await loadedClip.WithCancellation(token);
        }

        /// <summary>
        /// Removes audio clip and releases it from memory.
        /// </summary>
        public void UnloadClip()
        {
            songSource.clip = null;

            if (loadedClip.IsValid() && Application.isPlaying)
                Addressables.Release(loadedClip);
        }

        /// <summary>
        /// Schedules loaded song to start at a specific time in the future.
        /// </summary>
        /// <param name="dspTime"></param>
        public void ScheduleSong(float dspTime)
        {
            songSource.PlayScheduled(dspTime);
            Debug.Log($"Scheduled song start in {dspTime - AudioSettings.dspTime:N2} seconds!");
        }

        public void StopSong()
        {
            songSource.Stop();
            songSource.volume = 1f;
        }

        public void StartPause() => songSource.Pause();
        public void EndPause() => songSource.Play();

        private void OnDestroy() => UnloadClip();
    }
}

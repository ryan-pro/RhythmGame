using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace RhythmGame.GeneralAudio
{
    public class AudioSystem : MonoBehaviour
    {
        private const string audioSceneName = "GameAudio";
        private static AudioSystem instance;

        [SerializeField]
        private MusicPlayer musicPlayer;
        [SerializeField]
        private SoundPlayer soundPlayer;

        public static bool IsInitialized => instance != null;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
        }

        public static async UniTask Initialize(CancellationToken token)
        {
            if (instance != null)
            {
                Debug.Log("Audio system already initialized.");
                return;
            }

            var scene = SceneManager.GetSceneByName(audioSceneName);

            if (!scene.isLoaded)
            {
                var loadedScene = await Addressables.LoadSceneAsync(audioSceneName, LoadSceneMode.Additive).WithCancellation(token);

                //From memory, might need to skip a frame before the scene is fully loaded
                //await UniTask.Yield(token);
                scene = loadedScene.Scene;
            }

            instance = scene.FindInSceneRoot<AudioSystem>();
        }

        #region Music

        public static void PlayMusic(AudioClip clip, bool crossfade = false)
        {
            if (!CheckInitialized())
                return;

            instance.musicPlayer.Play(clip, crossfade);
        }

        public static UniTask PlayMusic(AudioClip clip, bool crossfade = false, CancellationToken token = default)
        {
            if (!CheckInitialized())
                return UniTask.CompletedTask;

            return instance.musicPlayer.Play(clip, crossfade, token);
        }

        public static int PlayScheduledMusic(AudioClip clip, double startTime)
        {
            if (!CheckInitialized())
                return -1;

            return instance.musicPlayer.PlayScheduled(clip, startTime);
        }

        public static void StopMusic(float fadeDuration = 0f)
        {
            if (!CheckInitialized())
                return;

            instance.musicPlayer.Stop(fadeDuration);
        }

        public static UniTask StopMusic(float fadeDuration = 0f, CancellationToken token = default)
        {
            if (!CheckInitialized())
                return UniTask.CompletedTask;

            return instance.musicPlayer.Stop(fadeDuration, token);
        }

        #endregion

        #region Sound Effects

        public static void PlayOneShot(AudioClip clip)
        {
            if (!CheckInitialized())
                return;

            instance.soundPlayer.PlayOneShot(clip);
        }

        public static int PlaySound(AudioClip clip)
        {
            if (!CheckInitialized())
                return -1;

            return instance.soundPlayer.Play(clip);
        }

        public static void StopSound(int index)
        {
            if (!CheckInitialized())
                return;

            instance.soundPlayer.Stop(index);
        }

        public static void StopAllSounds()
        {
            if (!CheckInitialized())
                return;

            instance.soundPlayer.StopAll();
        }

        #endregion

        private static bool CheckInitialized()
        {
            if (instance == null)
            {
                Debug.LogError("Audio system not initialized!");
                return false;
            }

            return true;
        }
    }
}

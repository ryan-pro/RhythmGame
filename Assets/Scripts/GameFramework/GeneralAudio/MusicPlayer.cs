using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmGame.GeneralAudio
{
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioSource[] sources;
        [SerializeField]
        private float crossfadeDuration = 1f;

        public void Play(AudioClip clip, bool crossfade = false)
        {
            var token = this.GetCancellationTokenOnDestroy();
            Play(clip, crossfade, token).Forget();
        }

        public UniTask Play(AudioClip clip, bool crossfade = false, CancellationToken token = default)
        {
            if (!token.CanBeCanceled)
                token = this.GetCancellationTokenOnDestroy();

            var (freeSource, playingSource) = GetSources();

            if (freeSource == null && playingSource == null)
            {
                Debug.LogError("No sources found!");
                return UniTask.CompletedTask;
            }
            if (freeSource == null)
            {
                freeSource = playingSource;
                playingSource = null;

                freeSource.Stop();
            }

            freeSource.clip = clip;
            freeSource.volume = 1f;

            if (playingSource == null)
            {
                freeSource.Play();
                return UniTask.CompletedTask;
            }

            if (!crossfade)
            {
                playingSource.Stop();
                freeSource.Play();
                return UniTask.CompletedTask;
            }

            var fadeIn = FadeAudio(freeSource, 0f, freeSource.volume, crossfadeDuration, token);
            var fadeOut = FadeAudio(playingSource, playingSource.volume, 0f, crossfadeDuration, token);
            return UniTask.WhenAll(fadeIn, fadeOut);
        }

        public int PlayScheduled(AudioClip clip, double startTime)
        {
            var freeSource = System.Array.Find(sources, a => !a.isPlaying);

            if(freeSource == null)
            {
                Debug.LogError("No open sources found!");
                return -1;
            }

            freeSource.clip = clip;
            freeSource.volume = 1f;
            freeSource.PlayScheduled(startTime);

            return System.Array.IndexOf(sources, freeSource);
        }

        public void Stop(float fadeDuration = 0f)
        {
            var token = this.GetCancellationTokenOnDestroy();
            Stop(fadeDuration, token).Forget();
        }

        public async UniTask Stop(float fadeDuration = 0f, CancellationToken token = default)
        {
            if (!token.CanBeCanceled)
                token = this.GetCancellationTokenOnDestroy();

            fadeDuration = Mathf.Max(fadeDuration, 0f);
            var isZero = Mathf.Approximately(fadeDuration, 0f);

            var playingSources = sources.Where(source => source.isPlaying);

            if (!isZero)
                await UniTask.WhenAll(playingSources.Select(source => FadeAudio(source, source.volume, 0f, fadeDuration, token)));

            foreach (var source in playingSources)
            {
                source.Stop();
                source.volume = 1f;
            }
        }

        private (AudioSource freeSource, AudioSource playingSource) GetSources()
        {
            AudioSource freeSource = null;
            AudioSource playingSource = null;

            foreach (var source in sources)
            {
                if (source.isPlaying)
                    playingSource = source;
                else
                    freeSource = source;

                if (playingSource != null && freeSource != null)
                    break;
            }

            return (freeSource, playingSource);
        }

        private async UniTask FadeAudio(AudioSource source, float startVol, float endVol, float duration, CancellationToken token)
        {
            if (!source.isPlaying)
                source.Play();

            for (float curTime = 0f; curTime < duration; curTime += Time.deltaTime)
            {
                source.volume = Mathf.Lerp(startVol, endVol, curTime / duration);

                if (await UniTask.Yield(token).SuppressCancellationThrow())
                    break;
            }

            if (Mathf.Approximately(endVol, 0f))
                source.Stop();
        }
    }
}

using Cysharp.Threading.Tasks;
using RhythmGame.GeneralAudio;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class TrackPresenter : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField]
        private AssetReferenceLoadableAudioList audioListRef;

        [Header("Scene References")]
        [SerializeField]
        private SpriteRenderer trackLight;

        [Header("Configuration")]
        [SerializeField]
        private float hitLightTimer = 0.15f;
        [SerializeField]
        private string greatSoundKey = "GreatHit";
        [SerializeField]
        private string okaySoundKey = "OkayHit";
        [SerializeField]
        private string missSoundKey = "MissHit";

        private LoadableAudioList audioList;

        private UniTask<LoadableAudioList> loadTask;
        private CancellationTokenSource loadTokenSource;

        CancellationToken lifetimeToken;

        private void OnEnable()
        {
            lifetimeToken = this.GetCancellationTokenOnDestroy();
            loadTokenSource = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken);

            LoadAudioList(loadTokenSource.Token).Forget();
        }

        private void OnDisable()
        {
            loadTokenSource?.Cancel();

            audioList.UnloadAllClips();
            audioList = null;

            audioListRef.ReleaseAsset();
        }

        private void OnDestroy() => loadTokenSource?.Dispose();

        private async UniTaskVoid LoadAudioList(CancellationToken token)
        {
            loadTask = audioListRef.LoadAssetAsync().WithCancellation(token).Preserve();
            audioList = await loadTask;
            await audioList.PreloadAllClips(token);
        }

        public void HandleHit(NoteObject note, NoteHitRating rating)
        {
            if (rating == NoteHitRating.Great)
            {
                TriggerHitSound(greatSoundKey);
                SetLightColor(Color.green);
            }
            else if (rating == NoteHitRating.Okay)
            {
                TriggerHitSound(okaySoundKey);
                SetLightColor(Color.yellow);
            }
            else
            {
                TriggerHitSound(missSoundKey);
                SetLightColor(Color.red);
            }
        }

        public void SetLightEnabled(bool enabled) => trackLight.enabled = enabled;

        private void SetLightColor(Color lightColor)
        {
            trackLight.color = lightColor;
            CancelInvoke(nameof(ResetLightColor));
            Invoke(nameof(ResetLightColor), hitLightTimer);
        }

        private void ResetLightColor() => trackLight.color = Color.white;

        private async UniTaskVoid TriggerHitSound(string clipKey)
        {
            if (audioList == null)
            {
                Debug.LogError("Audio list not loaded yet.");
                return;
            }

            var clip = await audioList.GetClip(clipKey, lifetimeToken);

            if (clip == null)
            {
                Debug.LogError($"Clip key {clipKey} not found in audio list.");
                return;
            }

            AudioSystem.PlayOneShot(clip);
        }
    }
}

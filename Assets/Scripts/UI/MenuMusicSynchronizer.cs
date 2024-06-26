﻿using Cysharp.Threading.Tasks;
using RhythmGame.GeneralAudio;
using System.Threading;
using UnityEngine;

namespace RhythmGame.UI
{
    public class MenuMusicSynchronizer : MonoBehaviour
    {
        [SerializeField]
        private AssetReferenceSongData startingSongRef;
        [SerializeField]
        private RhythmConductor conductor;

        private SongData cachedData;

        public async UniTask BeginMusicConduction(CancellationToken token)
        {
            cachedData = await startingSongRef.LoadAssetAsync().WithCancellation(token);
            var audioLoad = cachedData.AudioClip.LoadAssetAsync().WithCancellation(token);

            conductor.StartConducting(cachedData.BPM, cachedData.BeatsPerBar, cachedData.StartOffset);
            var audioClip = await audioLoad;

            var startTime = conductor.SetSongStartTime();
            AudioSystem.PlayScheduledMusic(audioClip, startTime);
        }

        private void OnDestroy() => ReleaseAssets();

        private void ReleaseAssets()
        {
            if (cachedData == null)
                return;

            cachedData.AudioClip.ReleaseAsset();
            cachedData = null;

            startingSongRef.ReleaseAsset();
        }
    }
}

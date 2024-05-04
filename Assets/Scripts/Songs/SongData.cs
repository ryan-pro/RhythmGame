using RhythmGame.Songs;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RhythmGame
{
    [CreateAssetMenu(menuName = "Rhythm Game/Songs/Song Data")]
    public class SongData : ScriptableObject
    {
        [SerializeField]
        private string songName;
        [SerializeField]
        private string artist;

        [SerializeField, Space]
        private AssetReferenceAudioClip audioClip;
        [SerializeField]
        private AssetReferenceSprite albumArt;

        [SerializeField, Space]
        private int bpm;
        [SerializeField]
        private int startOffset;

        [SerializeField, Space]
        private AssetReferenceNotesMap easyNoteTrack;
        [SerializeField]
        private AssetReferenceNotesMap mediumNoteTrack;
        [SerializeField]
        private AssetReferenceNotesMap hardNoteTrack;

        public string SongName => songName;
        public string Artist => artist;

        public AssetReferenceAudioClip AudioClip => audioClip;
        public AssetReferenceSprite AlbumArt => albumArt;

        public int BPM => bpm;
        public int StartOffset => startOffset;

        public AssetReferenceNotesMap EasyNoteTrack => easyNoteTrack;
        public AssetReferenceNotesMap MediumNoteTrack => mediumNoteTrack;
        public AssetReferenceNotesMap HardNoteTrack => hardNoteTrack;
    }
}

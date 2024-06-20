using RhythmGame.SongModels;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RhythmGame
{
    /// <summary>
    /// Represents a song in the game, containing metadata and references to the audio and note tracks.
    /// </summary>
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
        private int beatsPerBar;
        [SerializeField]
        private float startOffset;

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
        public int BeatsPerBar => beatsPerBar;
        public float StartOffset => startOffset;

        public AssetReferenceNotesMap EasyNoteTrack => easyNoteTrack;
        public AssetReferenceNotesMap MediumNoteTrack => mediumNoteTrack;
        public AssetReferenceNotesMap HardNoteTrack => hardNoteTrack;

        /// <summary>
        /// Loads the note map for the specified difficulty.
        /// </summary>
        /// <param name="difficulty">The difficulty level associated with the notes map.</param>
        /// <returns>A handle for the load operation, which can be used to release the asset later.</returns>
        public AsyncOperationHandle<NotesMap> LoadNoteMap(SongDifficulty difficulty)
        {
            return difficulty switch
            {
                SongDifficulty.Hard => hardNoteTrack.LoadAssetAsync(),
                SongDifficulty.Medium => mediumNoteTrack.LoadAssetAsync(),
                _ => easyNoteTrack.LoadAssetAsync()
            };
        }
    }
}

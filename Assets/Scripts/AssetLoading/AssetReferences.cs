﻿using RhythmGame.Songs;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RhythmGame
{
    /// <summary>
    /// SongData only asset reference.
    /// </summary>
    [System.Serializable]
    public class AssetReferenceSongData : AssetReferenceT<SongData>
    {
        /// <summary>
        /// Constructs a new reference to a SongData.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceSongData(string guid) : base(guid) { }
    }

    /// <summary>
    /// AudioClip only asset reference.
    /// </summary>
    [System.Serializable]
    public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
    {
        /// <summary>
        /// Constructs a new reference to a AudioClip.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceAudioClip(string guid) : base(guid) { }
    }

    /// <summary>
    /// NotesMap only asset reference.
    /// </summary>
    [System.Serializable]
    public class AssetReferenceNotesMap : AssetReferenceT<NotesMap>
    {
        /// <summary>
        /// Constructs a new reference to a NotesMap.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceNotesMap(string guid) : base(guid) { }
    }

    /// <summary>
    /// NoteObject only asset reference.
    /// </summary>
    [System.Serializable]
    public class AssetReferenceNoteObject : AssetReferenceT<NoteObject>
    {
        /// <summary>
        /// Constructs a new reference to a NoteObject.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceNoteObject(string guid) : base(guid) { }
    }

    /// <summary>
    /// PooledObject only asset reference.
    /// </summary>
    [System.Serializable]
    public class AssetReferencePooledObject : AssetReferenceT<PooledObject>
    {
        /// <summary>
        /// Constructs a new reference to a PooledObject.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferencePooledObject(string guid) : base(guid) { }
    }
}

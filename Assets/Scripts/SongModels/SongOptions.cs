namespace RhythmGame.SongModels
{
    /// <summary>
    /// Represents the user-changeable options for a song.
    /// </summary>
    [System.Serializable]
    public class SongOptions
    {
        public SongDifficulty Difficulty;
        public float CustomOffset;
    }
}

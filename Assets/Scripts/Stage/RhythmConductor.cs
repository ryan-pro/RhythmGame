using UnityEngine;

namespace RhythmGame
{
    /// <summary>
    /// Conducts the rhythm of the game by calculating the beat positions of the stage and song.
    /// Stage beats and song beats are synchronized; beats can be measured even when a song isn't playing.
    /// </summary>
    public class RhythmConductor : MonoBehaviour
    {
        [SerializeField]
        private float bpm = 60f;
        [SerializeField]
        private int beatsPerBar = 4;
        [SerializeField]
        private float songStartOffset;

        private float secondsPerBeat;

        private float stageStartTime;
        private float stagePosition;
        private float stageBeatPosition;

        private float songStartTime;
        private float songPosition;
        private float songBeatPosition;

        private bool isStarted;
        private bool isPaused;
        private float pauseStartTime;

        public bool IsStarted => isStarted;

        public float SecondsPerBeat => secondsPerBeat;
        public int BeatsPerBar => beatsPerBar;

        public float StageStartTime => stageStartTime;
        public float StageBeatPosition => stageBeatPosition;

        public float SongStartTime => songStartTime;
        public float SongBeatPosition => songBeatPosition;

        /// <summary>
        /// Begin calculating beats using the specified BPM, beats-per-bar, and offset.
        /// </summary>
        /// <param name="bpm">Beats-per-minute of the song.</param>
        /// <param name="beatsPerBar">The number of beats that make up a bar/measure.</param>
        /// <param name="offset">Offset used to adjust the song's start time relative to the beat.</param>
        public void StartConducting(float bpm, int beatsPerBar, float offset)
        {
            if (isStarted)
                return;

            this.bpm = bpm;
            this.beatsPerBar = beatsPerBar;
            songStartOffset = offset;

            StartConducting();
        }

        /// <summary>
        /// Begin calculating beats using the BPM and beats-per-bar already set.
        /// </summary>
        public void StartConducting()
        {
            if(isStarted)
                return;

            isStarted = true;
            secondsPerBeat = 60f / bpm;
            stageStartTime = (float)AudioSettings.dspTime;
        }

        /// <summary>
        /// Signals the conductor to stop calculating beats.
        /// </summary>
        public void StopConducting() => isStarted = false;

        private void Update()
        {
            if (!isStarted || isPaused)
                return;

            var curDspTime = (float)AudioSettings.dspTime;
            stagePosition = curDspTime - stageStartTime - songStartOffset;
            stageBeatPosition = stagePosition / secondsPerBeat;

            if (songStartTime < 1)
                return;

            songPosition = curDspTime - songStartTime - songStartOffset;
            songBeatPosition = songPosition / secondsPerBeat;
        }

        /// <summary>
        /// Starts conducting if necessary,
        /// and then sets the song's start time to the next bar.
        /// Note: This method does not start the song, it only determines the start time.
        /// </summary>
        /// <returns>The song's calculated start time, also cached by RhythmConductor.</returns>
        public float SetSongStartTime()
        {
            if (!isStarted)
                StartConducting();

            var secsPerBar = beatsPerBar * secondsPerBeat;

            var timeDiff = (float)AudioSettings.dspTime - stageStartTime;
            var barsSinceStart = Mathf.FloorToInt(timeDiff / secsPerBar);

            songStartTime = stageStartTime + ((barsSinceStart + 1) * secsPerBar);
            return songStartTime;
        }

        /// <summary>
        /// Returns the next beat position in the song in seconds.
        /// </summary>
        /// <returns>The position of the next beat.</returns>
        public float GetSongNextBeatPosition()
        {
            var timeDiff = (float)AudioSettings.dspTime - songStartTime;
            var nextPosition = Mathf.CeilToInt(timeDiff / secondsPerBeat) * secondsPerBeat;

            return Mathf.Max(0f, nextPosition);
        }

        /// <summary>
        /// Pauses the conductor and caches the time of the pause.
        /// </summary>
        /// <returns>Returns true if pause was successful, false when already paused.</returns>
        public bool StartPause()
        {
            if(isPaused)
                return false;

            isPaused = true;
            pauseStartTime = (float)AudioSettings.dspTime;

            return true;
        }

        /// <summary>
        /// Resumes the conductor and adjusts the start times to account for the pause.
        /// </summary>
        /// <returns>Returns true if game successfully resumed, false when already unpaused.</returns>
        public bool EndPause()
        {
            if(!isPaused)
                return false;

            isPaused = false;

            var pauseOffset = (float)AudioSettings.dspTime - pauseStartTime;
            stageStartTime += pauseOffset;
            songStartTime += pauseOffset;

            return true;
        }
    }
}

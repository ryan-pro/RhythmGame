using UnityEngine;

namespace RhythmGame
{
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

        public void StartConducting(float bpm, int beatsPerBar, float offset)
        {
            if (isStarted)
                return;

            this.bpm = bpm;
            this.beatsPerBar = beatsPerBar;
            songStartOffset = offset;

            StartConducting();
        }

        public void StartConducting()
        {
            if(isStarted)
                return;

            isStarted = true;
            secondsPerBeat = 60f / bpm;
            stageStartTime = (float)AudioSettings.dspTime;
        }

        public void StopConducting() => isStarted = false;

        public float ScheduleNewSongStart()
        {
            if (!isStarted)
                StartConducting();

            var secsPerBar = beatsPerBar * secondsPerBeat;

            var timeDiff = (float)AudioSettings.dspTime - stageStartTime;
            var barsSinceStart = Mathf.FloorToInt(timeDiff / secsPerBar);

            songStartTime = stageStartTime + ((barsSinceStart + 1) * secsPerBar);
            return songStartTime;
        }

        public float GetSongNextBeatPosition()
        {
            var timeDiff = (float)AudioSettings.dspTime - songStartTime;
            var nextPosition = Mathf.CeilToInt(timeDiff / secondsPerBeat) * secondsPerBeat;

            return Mathf.Max(0f, nextPosition);
        }

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

        public bool StartPause()
        {
            if(isPaused)
                return false;

            isPaused = true;
            pauseStartTime = (float)AudioSettings.dspTime;

            return true;
        }

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

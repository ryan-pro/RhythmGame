using UnityEngine;

namespace RhythmGame
{
    public class RhythmConductor : MonoBehaviour
    {
        [SerializeField]
        private SongPlayer song;
        [SerializeField]
        private TrackPlayer track;

        [SerializeField]
        private float bpm = 60f;
        [SerializeField]
        private float songStartOffset;

        private float secsPerBeat;

        private float stageStartTime;
        private float stagePosition;
        private float stageBeatPosition;

        private float songStartTime;
        private float songPosition;
        private float songBeatPosition;

        private bool isStarted;
        private bool isPaused;
        private float pauseStartTime;

        public float StageStartTime => stageStartTime;
        public float StageBeatPosition => stageBeatPosition;

        public float SongStartTime => songStartTime;
        public float SongBeatPosition => songBeatPosition;

        public void StartConducting(float bpm, float offset)
        {
            if (isStarted)
                return;

            this.bpm = bpm;
            songStartOffset = offset;
            StartConducting();
        }

        public void StartConducting()
        {
            if(isStarted)
                return;

            isStarted = true;
            secsPerBeat = 60f / bpm;
            stageStartTime = (float)AudioSettings.dspTime;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
                StartConducting();

            if (!isStarted || isPaused)
                return;

            var curDspTime = (float)AudioSettings.dspTime;
            stagePosition = curDspTime - stageStartTime - songStartOffset;
            stageBeatPosition = stagePosition / secsPerBeat;

            if (songStartTime < 1)
                return;

            songPosition = curDspTime - songStartTime - songStartOffset;
            songBeatPosition = songPosition / secsPerBeat;
        }

        public void StopConducting() => isStarted = false;

        public void ScheduleSongStart(int beatsUntilStart)
        {
            if (!isStarted)
                StartConducting();

            var timeDiff = (float)AudioSettings.dspTime - stageStartTime;
            var beatsSinceStart = Mathf.FloorToInt(timeDiff / secsPerBeat);

            songStartTime = stageStartTime + ((beatsSinceStart + beatsUntilStart + 1) * secsPerBeat);

            song.ScheduleSongStart(songStartTime);
            track.ScheduleSongStart(this);
        }

        public void StartPause()
        {
            if(isPaused)
                return;

            isPaused = true;

            song.StartPause();
            track.StartPause();
            pauseStartTime = (float)AudioSettings.dspTime;
        }

        public void EndPause()
        {
            if(!isPaused)
                return;

            isPaused = false;
            var pauseOffset = (float)AudioSettings.dspTime - pauseStartTime;
            //TODO: Add countdown feature

            stageStartTime += pauseOffset;
            songStartTime += pauseOffset;

            song.EndPause();
            track.EndPause();
        }
    }
}

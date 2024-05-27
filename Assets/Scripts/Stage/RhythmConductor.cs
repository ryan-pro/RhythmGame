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
        private int beatsPerBar = 4;
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

        public bool IsStarted => isStarted;
        public float SecsPerBeat => secsPerBeat;

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
            secsPerBeat = 60f / bpm;
            stageStartTime = (float)AudioSettings.dspTime;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                if(!isStarted)
                {
                    Debug.Log("Conduction started.");
                    StartConducting();
                }
                else
                {
                    Debug.Log("Conduction stopped.");
                    StopConducting();
                }
            }

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

        public void ScheduleSongStart()
        {
            if (!isStarted)
                StartConducting();

            //var timeDiff = (float)AudioSettings.dspTime - stageStartTime;
            //var beatsSinceStart = Mathf.FloorToInt(timeDiff / secsPerBeat);

            //songStartTime = stageStartTime + ((beatsSinceStart + (beatsPerBar * 2) + 1) * secsPerBeat);

            var secsPerBar = beatsPerBar * secsPerBeat;

            var timeDiff = (float)AudioSettings.dspTime - stageStartTime;
            var barsSinceStart = Mathf.FloorToInt(timeDiff / secsPerBar);

            songStartTime = stageStartTime + ((barsSinceStart + 1) * secsPerBar);

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

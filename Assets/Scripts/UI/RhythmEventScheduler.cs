using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace RhythmGame
{
    /// <summary>
    /// Schedules events to be executed at specific beats in the song.
    /// </summary>
    public class RhythmEventScheduler : MonoBehaviour
    {
        [SerializeField]
        private RhythmConductor conductor;
        [SerializeField]
        private SynchronizedEvent[] eventList;

        private float startTime;

        private UniTask runningTask;
        private CancellationTokenSource schedulerSource;

        public bool IsPlaying => schedulerSource != null && !runningTask.Status.IsCompleted();
        public float SecondsPerBeat => conductor.SecondsPerBeat;

        public UniTask Begin(CancellationToken token)
        {
            if (schedulerSource?.IsCancellationRequested == false)
                return UniTask.CompletedTask;

            schedulerSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            startTime = conductor.GetSongNextBeatPosition();
            float curTime = startTime;

            foreach (var evt in eventList)
            {
                curTime += evt.BeatsUntilEvent * conductor.SecondsPerBeat;
                evt.TargetBeat = curTime;
            }

            runningTask = ExecuteEvents(schedulerSource.Token).Preserve();
            return runningTask;
        }

        private async UniTask ExecuteEvents(CancellationToken token)
        {
            var eventQueue = new Queue<SynchronizedEvent>(eventList);
            await foreach (float beat in UniTaskAsyncEnumerable.EveryValueChanged(conductor, c => c.SongBeatPosition))
            {
                //If cancelled, invoke all remaining events and break
                if (token.IsCancellationRequested)
                {
                    while (eventQueue.TryDequeue(out var evt))
                        evt.OnBeat.Invoke(evt.DurationInBeats * conductor.SecondsPerBeat, token);

                    break;
                }

                while (eventQueue.TryPeek(out var peeked)
                    && (beat >= peeked.TargetBeat || token.IsCancellationRequested))
                {
                    var evt = eventQueue.Dequeue();
                    evt.OnBeat.Invoke(evt.DurationInBeats * conductor.SecondsPerBeat, token);
                }

                if (eventQueue.Count == 0)
                    break;
            }
        }

        public void Cancel()
        {
            schedulerSource?.Cancel();
        }

        private void OnDisable() => schedulerSource?.Cancel();
        private void OnDestroy() => schedulerSource?.Dispose();
    }

    /// <summary>
    /// Represents an event that is synchronized to the beat of a song.
    /// </summary>
    [System.Serializable]
    public class SynchronizedEvent
    {
        [System.Serializable]
        public class CancelableFloatEvent : UnityEvent<float, CancellationToken> { }

        public int BeatsUntilEvent;
        public int DurationInBeats = 1;

        public CancelableFloatEvent OnBeat = new();

        [HideInInspector]
        public float TargetBeat;
    }
}

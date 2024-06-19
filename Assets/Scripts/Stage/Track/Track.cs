using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    /// <summary>
    /// Manages the spawning and tracking of notes on the tracks.
    /// </summary>
    [RequireComponent(typeof(TrackPresenter))]
    public class Track : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField]
        private UnityObjectPoolBase notePrefabPool;
        [SerializeField]
        private RhythmConductor conductor;
        [SerializeField]
        private TrackPresenter presenter;

        [Header("Internal References")]
        [SerializeField]
        private Transform start;
        [SerializeField]
        private Transform end;

        [Header("Configuration")]
        public bool InputEnabled = true;

        private float greatThreshold = 0.1f;
        private float okayThreshold = 0.2f;

        private readonly Queue<UniTask<NoteObject>> loadQueue = new();
        private readonly List<NoteObject> noteQueue = new();

        //Maps input index/touch ID to the time it was last pressed
        private readonly Dictionary<int, float> inputTimers = new(5);
        private readonly int[] expiredIDs = new int[5];

        public readonly Dictionary<NoteHitRating, int> NoteHits = new()
        {
            { NoteHitRating.Great, 0 },
            { NoteHitRating.Okay, 0 },
            { NoteHitRating.Miss, 0 }
        };

        public Transform Start => start;
        public Transform End => end;

        private void Reset() => presenter = GetComponent<TrackPresenter>();

        private void Update()
        {
            ProcessLoadQueue();

            //Handle input
            inputTimers.Keys.CopyTo(expiredIDs, 0);
            var inputCount = inputTimers.Count;

            for (int i = 0; i < inputCount; i++)
            {
                var inputKey = expiredIDs[i];
                var inputTime = inputTimers[inputKey];

                if (Time.time - inputTime > Time.deltaTime)
                    HandleTouchEnd(inputKey);
            }

            //Handle missed notes
            var curBeat = conductor.SongBeatPosition;
            while (noteQueue.Count > 0)
            {
                var note = noteQueue[0];

                if (note.TargetBeat >= curBeat - okayThreshold)
                    break;

                noteQueue.Remove(note);
                NoteHits[NoteHitRating.Miss]++;
            }
        }

        public void HandleTouchStart(int inputIndex = 0)
        {
            if (!InputEnabled)
                return;

            var curBeat = conductor.SongBeatPosition;
            inputTimers[inputIndex] = Time.time;

            presenter.SetLightEnabled(true);

            var closestNote = noteQueue.OrderBy(note => Mathf.Abs(note.TargetBeat - curBeat)).FirstOrDefault();

            if (closestNote == null)
                return;

            //Judge score based on distance from target beat
            var distance = Mathf.Abs(closestNote.TargetBeat - curBeat);

            if (distance > conductor.SecondsPerBeat)
                return;

            var hitRating = NoteHitRating.Miss;

            if (distance <= greatThreshold)
                hitRating = NoteHitRating.Great;
            else if (distance <= okayThreshold)
                hitRating = NoteHitRating.Okay;

            NoteHits[hitRating]++;
            closestNote.SetNoteHitRating(hitRating);
            presenter.HandleHit(closestNote, hitRating);

            //Discard other notes as missed
            while (noteQueue.Count > 0)
            {
                var firstNote = noteQueue[0];
                noteQueue.RemoveAt(0);

                if (firstNote == closestNote)
                    break;
            }

            //TODO: Implement hold notes
        }

        public void HandleTouchHold(int inputIndex = 0)
        {
            if (!InputEnabled)
                return;

            if (inputTimers.Count == 0)
            {
                HandleTouchStart(inputIndex);
                return;
            }

            inputTimers[inputIndex] = Time.time;

            //TODO: Implement hold notes
        }

        public void HandleTouchEnd(int inputIndex = 0)
        {
            if (!inputTimers.Remove(inputIndex))
                return;

            if (inputTimers.Count > 0)
                return;

            presenter.SetLightEnabled(false);

            //TODO: Implement hold notes
        }

        public void SetScoreThresholds(float greatThreshold, float okayThreshold)
        {
            this.greatThreshold = greatThreshold;
            this.okayThreshold = okayThreshold;
        }

        /// <summary>
        /// Gets or creates a note object and adds it to a load queue.
        /// </summary>
        /// <param name="startPosition">Starting time in beats.</param>
        /// <param name="endPosition">Target time in beats.</param>
        public void AddNote(float startPosition, float endPosition, CancellationToken token)
        {
            var loadTask = notePrefabPool.GetObject(transform, true, token).ContinueWith(obj =>
            {
                var noteObject = obj.GetComponent<NoteObject>();
                noteObject.InitializeNote(conductor, start, end, startPosition, endPosition);
                return noteObject;
            });

            loadQueue.Enqueue(loadTask);
        }

        /// <summary>
        /// Move notes from the load queue
        /// to the notes queue once they are loaded.
        /// </summary>
        private void ProcessLoadQueue()
        {
            while (loadQueue.Count > 0)
            {
                var status = loadQueue.Peek().Status;
                if (status != UniTaskStatus.Succeeded)
                {
                    if (status == UniTaskStatus.Canceled)
                        Debug.LogWarning("Note load canceled.");
                    else if (status == UniTaskStatus.Faulted)
                        Debug.LogError("Note failed to load.");

                    break;
                }

                noteQueue.Add(loadQueue.Dequeue().GetAwaiter().GetResult());
            }
        }
    }
}

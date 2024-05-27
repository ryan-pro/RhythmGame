using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    public class TimedInputData
    {
        public readonly int InputIndex;
        public float Time;

        public TimedInputData(int inputIndex, float time)
        {
            InputIndex = inputIndex;
            Time = time;
        }
    }

    public class Track : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField]
        private UnityObjectPoolBase notePrefabPool;
        [SerializeField]
        private RhythmConductor conductor;

        [Header("Local References")]
        [SerializeField]
        private Transform start;
        [SerializeField]
        private Transform end;
        [SerializeField]
        private SpriteRenderer trackLight;

        private float greatThreshold = 0.1f;
        private float okayThreshold = 0.2f;

        private readonly Queue<UniTask<NoteObject>> loadQueue = new();
        private readonly List<NoteObject> noteQueue = new();

        private List<TimedInputData> inputIDs = new(2);
        private readonly Dictionary<int, float> inputTimers = new(5);
        private readonly int[] expiredIDs = new int[5];

        public Transform Start => start;
        public Transform End => end;

        private void Update()
        {
            ProcessLoadQueue();

            inputTimers.Keys.CopyTo(expiredIDs, 0);
            var inputCount = inputTimers.Count;

            for (int i = 0; i < inputCount; i++)
            {
                var inputKey = expiredIDs[i];
                var inputTime = inputTimers[inputKey];

                if (Time.time - inputTime > Time.deltaTime)
                    HandleTouchEnd(inputKey);
            }

            var curBeat = conductor.SongBeatPosition;
            while (noteQueue.Count > 0)
            {
                var note = noteQueue[0];

                if (note.TargetBeat >= curBeat - okayThreshold)
                    break;

                noteQueue.Remove(note);
            }
        }

        public void HandleTouchStart(int inputIndex = 0)
        {
            var curBeat = conductor.SongBeatPosition;
            inputTimers[inputIndex] = Time.time;

            trackLight.color = Color.white;
            trackLight.enabled = true;

            var closestNote = noteQueue.OrderBy(note => Mathf.Abs(note.TargetBeat - curBeat)).FirstOrDefault();

            if (closestNote == null)
                return;

            //Judge score based on distance from target beat
            var distance = Mathf.Abs(closestNote.TargetBeat - curBeat);

            if (distance > conductor.SecsPerBeat)
                return;

            if (distance <= greatThreshold)
            {
                trackLight.color = Color.green;
                closestNote.SetNoteHitRating(NoteHitRating.Great);
            }
            else if (distance <= okayThreshold)
            {
                trackLight.color = Color.yellow;
                closestNote.SetNoteHitRating(NoteHitRating.Okay);
            }
            else
            {
                trackLight.color = Color.red;
                closestNote.SetNoteHitRating(NoteHitRating.Miss);
            }

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
            if(inputTimers.Count == 0)
            {
                HandleTouchStart(inputIndex);
                return;
            }

            inputTimers[inputIndex] = Time.time;

            //TODO: Implement hold notes
        }

        public void HandleTouchEnd(int inputIndex = 0)
        {
            if(!inputTimers.Remove(inputIndex))
                return;

            if(inputTimers.Count > 0)
                return;

            trackLight.enabled = false;

            //TODO: Implement hold notes
        }

        private void SetLightColor(Color lightColor)
        {
            void ResetLightColor() => trackLight.color = Color.white;

            trackLight.color = lightColor;
            CancelInvoke(nameof(ResetLightColor));
            Invoke(nameof(ResetLightColor), 0.1f);
        }

        public void SetScoreThresholds(float greatThreshold, float okayThreshold)
        {
            this.greatThreshold = greatThreshold;
            this.okayThreshold = okayThreshold;
        }

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

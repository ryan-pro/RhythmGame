using UnityEngine;

namespace RhythmGame
{
    [System.Serializable]
    public class InputData
    {
        public Track Track;
        public KeyCode KeyCode = KeyCode.None;
        public string InputName = string.Empty;
    }

    public class StageInputManager : MonoBehaviour
    {
        [SerializeField]
        private Camera gameplayCam;
        [SerializeField]
        private LayerMask interactiveLayer;

        [SerializeField]
        private InputData[] inputs;

        private void Update()
        {
            if (Application.isEditor)
                CheckMouseInput();

            if (!Application.isMobilePlatform)
                CheckGenericInput();
            else
                CheckTouchInput();
        }

        private void CheckGenericInput()
        {
            foreach (var input in inputs)
            {
                //if(!string.IsNullOrEmpty(input.InputName))
                //{
                //    if (Input.GetButtonDown(input.InputName))
                //    {
                //        input.Track.HandleTouchStart();
                //    }
                //    else if (Input.GetButton(input.InputName))
                //    {
                //        input.Track.HandleTouchHold();
                //    }
                //    else if (Input.GetButtonUp(input.InputName))
                //    {
                //        input.Track.HandleTouchEnd();
                //    }
                //}
                /*else*/ if (input.KeyCode != KeyCode.None)
                {
                    if (Input.GetKeyDown(input.KeyCode))
                    {
                        input.Track.HandleTouchStart();
                    }
                    else if (Input.GetKey(input.KeyCode))
                    {
                        input.Track.HandleTouchHold();
                    }
                    else if (Input.GetKeyUp(input.KeyCode))
                    {
                        input.Track.HandleTouchEnd();
                    }
                }
            }
        }

        private void CheckMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = gameplayCam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 100f, interactiveLayer.value) && hit.collider.TryGetComponent<Track>(out var track))
                    track.HandleTouchStart();
            }
            else if (Input.GetMouseButton(0))
            {
                var ray = gameplayCam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 100f, interactiveLayer.value) && hit.collider.TryGetComponent<Track>(out var track))
                    track.HandleTouchHold();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                var ray = gameplayCam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 100f, interactiveLayer.value) && hit.collider.TryGetComponent<Track>(out var track))
                    track.HandleTouchEnd();
            }
        }

        private void CheckTouchInput()
        {
            int touchCount = Input.touchCount;
            for (int i = 0; i < touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                var ray = gameplayCam.ScreenPointToRay(touch.position);

                if (Physics.Raycast(ray, out var hit, 100f, interactiveLayer.value) && hit.collider.TryGetComponent<Track>(out var track))
                {
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            track.HandleTouchStart(touch.fingerId);
                            break;
                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
                            track.HandleTouchHold(touch.fingerId);
                            break;
                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            track.HandleTouchEnd(touch.fingerId);
                            break;
                    }
                }
            }
        }
    }
}

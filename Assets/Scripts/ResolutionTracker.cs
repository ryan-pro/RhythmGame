using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmGame
{
    [System.Obsolete("This script is not used in the project, " +
        "but it's a good example of how to use UniTaskAsyncEnumerable " +
        "to track changes in the resolution of the screen and adjust the camera's FOV accordingly.")]
    [RequireComponent(typeof(Camera))]
    public class ResolutionTracker : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Camera cam;

        [SerializeField]
        private int targetVertFOV = 90;
        [SerializeField]
        private int minHorFOV = 121;
        [SerializeField]
        private int maxHorFOV = 180;

        private CancellationTokenSource enabledSource;

        private Vector2 CurrentResolution => new(Screen.width, Screen.height);

        private void OnEnable()
        {
            enabledSource = new CancellationTokenSource();
            TrackResolutionChanges(enabledSource.Token).Forget();
        }

        private void OnDisable() => enabledSource?.Cancel();

        private void Reset() => cam = GetComponent<Camera>();

        private async UniTaskVoid TrackResolutionChanges(CancellationToken token)
        {
            await foreach (var _ in UniTaskAsyncEnumerable.EveryValueChanged(this, a => a.CurrentResolution).WithCancellation(token))
            {
                Debug.Log("Updating FOV");
                var horFOV = Camera.VerticalToHorizontalFieldOfView(targetVertFOV, cam.aspect);
                cam.fieldOfView = Camera.HorizontalToVerticalFieldOfView(Mathf.Clamp(horFOV, minHorFOV, maxHorFOV), cam.aspect);
            }
        }
    }
}

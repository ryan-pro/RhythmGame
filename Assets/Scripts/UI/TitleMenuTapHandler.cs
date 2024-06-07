using UnityEngine;
using UnityEngine.EventSystems;

namespace RhythmGame.UI
{
    public class TitleMenuTapHandler : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private TitleMenu titleMenu;
        [SerializeField]
        private RhythmEventScheduler eventScheduler;

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log ("Pointer down detected");

            if (eventScheduler.IsPlaying)
            {
                eventScheduler.Cancel();
                return;
            }

            titleMenu.ProcessGameStart().Forget();
            enabled = false;
        }
    }
}

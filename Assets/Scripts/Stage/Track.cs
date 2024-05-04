using UnityEngine;
using UnityEngine.UI;

namespace RhythmGame
{
    public class Track : MonoBehaviour
    {
        [SerializeField]
        private RectTransform start;
        [SerializeField]
        private RectTransform end;

        [SerializeField]
        private bool showDebugLine;
        [SerializeField]
        private Image debugLine;

        public RectTransform Start => start;
        public RectTransform End => end;

        private void Awake()
            => debugLine.enabled = (Application.isEditor || Debug.isDebugBuild) && showDebugLine;
    }
}

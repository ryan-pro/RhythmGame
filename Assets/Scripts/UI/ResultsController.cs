using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace RhythmGame
{
    public class ResultsController : MonoBehaviour
    {
        [SerializeField]
        public Canvas resultsCanvas;

        [SerializeField]
        private TextMeshProUGUI greatCounter;
        [SerializeField]
        private TextMeshProUGUI okayCounter;
        [SerializeField]
        private TextMeshProUGUI missCounter;

        public void SetResults(int greats, int okays, int misses)
        {
            greatCounter.text = greats.ToString();
            okayCounter.text = okays.ToString();
            missCounter.text = misses.ToString();
        }

        public UniTask Display()
        {
            resultsCanvas.gameObject.SetActive(true);
            return UniTask.WaitWhile(() => resultsCanvas.gameObject.activeSelf);
        }

        public void Hide()
        {
            resultsCanvas.gameObject.SetActive(false);
        }
    }
}

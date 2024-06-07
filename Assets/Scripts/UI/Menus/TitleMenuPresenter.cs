using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGame.UI
{
    public class TitleMenuPresenter : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup menuCanvasGroup;
        [SerializeField]
        private CanvasGroup logoCanvasGroup;

        [SerializeField]
        private Image titleTextImage;
        [SerializeField]
        private TextMeshProUGUI tapText;

        public void ResetAll(float duration, CancellationToken token)
        {
            tapText.alpha = 0f;
            logoCanvasGroup.alpha = 0f;
            titleTextImage.color = new Color(titleTextImage.color.r, titleTextImage.color.g, titleTextImage.color.b, 0f);

            menuCanvasGroup.alpha = 1f;
        }

        public void RevealLogo(float duration, CancellationToken token)
        {
            Lerp(Mathf.Lerp, a => logoCanvasGroup.alpha = a, 0f, 1f, duration, token).Forget();
        }

        public void RevealTitleText(float duration, CancellationToken token)
        {
            var targetColor = new Color(titleTextImage.color.r, titleTextImage.color.g, titleTextImage.color.b, 1f);
            var startColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);

            Lerp(Color.Lerp, c => titleTextImage.color = c, startColor, targetColor, duration, token).Forget();
        }

        public void RevealTapText(float duration, CancellationToken token)
        {
            UniTask.Void(async () =>
            {
                await Lerp(Mathf.Lerp, a => tapText.alpha = a, 0f, 1f, duration, token);

                tapText.DOFade(0f, duration * 2f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .onComplete += () => tapText.alpha = 0f;
            });
        }

        private static async UniTask Lerp<T>(Func<T, T, float, T> lerpFunc, Action<T> setter, T start, T end, float duration, CancellationToken token)
        {
            for (float curTime = 0; curTime < duration; curTime += Time.deltaTime)
            {
                setter(lerpFunc(start, end, curTime / duration));

                if (await UniTask.Yield(token).SuppressCancellationThrow())
                    break;
            }

            setter(end);
        }
    }
}

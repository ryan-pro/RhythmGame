using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace RhythmGame.Initialization
{
    public class SplashScreenPlayer : MonoBehaviour
    {
        [SerializeField]
        private SplashScreen.StopBehavior splashStopBehavior = SplashScreen.StopBehavior.FadeOut;

        private bool stopTriggered;

        //To show the enabled checkbox in the inspector
        private void OnEnable() { }

        public async UniTask PlaySplashScreen()
        {
            if (!enabled)
                return;

            if (!SplashScreen.isFinished)
            {
                Debug.LogError("Splash screen already playing!");
                return;
            }

            Debug.Log("Splash screen beginning.");
            SplashScreen.Begin();

            while (Application.isPlaying && !SplashScreen.isFinished)
            {
                SplashScreen.Draw();
                await UniTask.Yield();
            }

            Debug.Log("Splash screen ended.");
        }

        public void StopSplashScreen()
        {
            if (!enabled || stopTriggered)
                return;

            stopTriggered = true;

            Debug.Log("Stopping splash screen.");
            SplashScreen.Stop(splashStopBehavior);
        }
    }
}

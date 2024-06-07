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

        public async UniTask PlaySplashScreen()
        {
            if (!SplashScreen.isFinished)
            {
                Debug.LogError("Splash screen already playing!");
                return;
            }

            Debug.Log("Splash screen beginning.");
            SplashScreen.Begin();

            while(!SplashScreen.isFinished)
            {
                SplashScreen.Draw();
                await UniTask.Yield();
            }

            Debug.Log("Splash screen ended.");
        }

        public void StopSplashScreen()
        {
            if(stopTriggered)
                return;

            stopTriggered = true;

            Debug.Log("Stopping splash screen.");
            SplashScreen.Stop(splashStopBehavior);
        }
    }
}

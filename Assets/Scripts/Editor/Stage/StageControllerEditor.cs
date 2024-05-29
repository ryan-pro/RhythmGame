using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RhythmGame
{
    [CustomEditor(typeof(StageController))]
    public class StageControllerEditor : Editor
    {
        private bool initializePressed;
        private bool startPressed;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);

            var controller = (StageController)target;

            if (!initializePressed && Application.isPlaying && GUILayout.Button("Debug Initialize"))
            {
                initializePressed = true;
                controller.InitializeComponents(controller.DebugSong, controller.DebugOptions);
            }

            if (!startPressed && Application.isPlaying && GUILayout.Button("Debug Start"))
            {
                startPressed = true;
                controller.InitializeSong(controller.DebugSong, controller.DebugOptions).Forget();
            }
        }
    }
}

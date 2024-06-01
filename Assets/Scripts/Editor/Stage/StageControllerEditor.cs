using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RhythmGame
{
    [CustomEditor(typeof(StageController))]
    public class StageControllerEditor : Editor
    {
        private bool startPressed;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);

            var controller = (StageController)target;

            if (!startPressed && Application.isPlaying && GUILayout.Button("Debug Start"))
            {
                startPressed = true;
                controller.InitializeSong(controller.DebugSong, controller.DebugOptions).Forget();
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using RhythmGame;
using UnityEditor.AddressableAssets;
using NUnit.Framework;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;

namespace RhythmGameEditor
{
    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);

            var loader = (SceneLoader)target;

            if (!Application.isPlaying && GUILayout.Button("Copy Addressable Scenes to List"))
                CopyAddressables(loader);
        }

        private void CopyAddressables(SceneLoader loader)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError("Addressable Asset Settings not found.");
                return;
            }

            foreach (var group in settings.groups)
            {
                if (group == null)
                    continue;

                foreach (var entry in group.entries)
                {
                    if (!entry.labels.Contains("scene"))
                        continue;

                    var sceneRef = new AssetReferenceScene();
                    sceneRef.InternalReference.SetEditorAsset(entry.MainAsset);

                    loader.SetSceneReference(entry.MainAsset.name, sceneRef);
                }
            }
        }
    }
}

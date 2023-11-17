using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLightmapSwitcher
{
    [CustomEditor(typeof(MagicLightmapSwitcher))]
    public class MLSEditor : Editor
    {
        public string lightmapName;
        public Vector2 objectsGroupScrollPosition;        
        public string currentSceneSettingsPresetName;
        private static List<string> scenes = new List<string>();
        private bool sceneIsUnloaded;

        public override void OnInspectorGUI()
        {
            MLSEditorUtils.InitStyles();

            MagicLightmapSwitcher magicLightmapSwitcher = (MagicLightmapSwitcher)target;

            scenes.Clear();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == Selection.activeGameObject.scene.name)
                {
                    MLSManager.selectedScene = i;
                    break;
                }
            }

            MLSManager.DrawSwitchingBlendingSection(magicLightmapSwitcher);
        } 
    }
}
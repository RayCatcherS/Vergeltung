using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class MLSBlendingConfigurator : EditorWindow
    {
        public static MagicLightmapSwitcher magicLightmapSwitcher;
        public static MLSBlendingConfigurator managerWindow;
        public static bool initialized;
        public static string targetScene;

        //[MenuItem("Tools/Magic Tools/Magic Lightmap Switcher/Blending Preview", priority = 0)]
        public static void Init()
        {
            managerWindow = (MLSBlendingConfigurator) GetWindow(typeof(MLSBlendingConfigurator), false, "Lightmaps Blending Preview");
            managerWindow.minSize = new Vector2(300 * EditorGUIUtility.pixelsPerPoint, 150 * EditorGUIUtility.pixelsPerPoint);
            managerWindow.Show();

            magicLightmapSwitcher = RuntimeAPI.GetSwitcherInstanceStatic(targetScene);

            //EditorSceneManager.sceneOpened += OnSceneOpened;
            //EditorSceneManager.sceneOpening += OnSceneOpening;
            //EditorApplication.update += CheckIfReloaded;

            initialized = true;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (!initialized)
            {
                Init();
            }

            if (!MLSEditorUtils.stylesInitialized)
            {
                MLSEditorUtils.InitStyles();
            }

            GUILayout.Label("Blending Configurator", MLSEditorUtils.captionStyle);

            MLSEditorUtils.DrawBlendingConfigurator(magicLightmapSwitcher, magicLightmapSwitcher.currentLightmapScenario);
        }
    }
}

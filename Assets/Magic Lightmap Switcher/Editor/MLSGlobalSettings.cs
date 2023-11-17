using System.IO;
using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class MLSGlobalSettings : EditorWindow
    {
        public static MagicLightmapSwitcher magicLightmapSwitcher;
        public static MLSGlobalSettings settingsWindow;

        private static bool initialized;
        private static SystemProperties systemProperties;

        //[MenuItem("Tools/Magic Tools/Magic Lightmap Switcher/Global Settings", priority = 10)]
        public static void Init()
        {
            settingsWindow = (MLSGlobalSettings) GetWindow(typeof(MLSGlobalSettings), false, "Global Settings");
            settingsWindow.minSize = new Vector2(300 * EditorGUIUtility.pixelsPerPoint, 150 * EditorGUIUtility.pixelsPerPoint);
            settingsWindow.Show();

            magicLightmapSwitcher = FindObjectOfType<MagicLightmapSwitcher>();

            string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);
            string projectRelativePath = Application.dataPath + directories[0].Split(new[] { "Assets" }, System.StringSplitOptions.None)[1];

            systemProperties = AssetDatabase.LoadAssetAtPath(FileUtil.GetProjectRelativePath(projectRelativePath + "/Editor/SystemProperties.asset"), typeof(SystemProperties)) as SystemProperties;

            if (systemProperties == null)
            {
                systemProperties = ScriptableObject.CreateInstance<SystemProperties>();

                AssetDatabase.CreateAsset(systemProperties, FileUtil.GetProjectRelativePath(projectRelativePath + "/Editor/SystemProperties.asset"));
                AssetDatabase.SaveAssets();
            }

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

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Clear Original Lighting Data", GUILayout.MinWidth(180));
                    systemProperties.clearOriginalLightingData = EditorGUILayout.Toggle(systemProperties.clearOriginalLightingData, GUILayout.MaxWidth(240));
                }

                EditorGUILayout.HelpBox("Clear all lighting data in the default folder.", MessageType.Info);

                //if (!magicLightmapSwitcher.systemProperties.highDefinitionRPActive)
                //{
                //    using (new GUILayout.HorizontalScope())
                //    {
                //        GUILayout.Label(MLSTooltipManager.MainComponent.GetParameter("Batch By Lightmap Index", MLSTooltipManager.MainComponent.Tabs.Global), GUILayout.MinWidth(180));
                //        systemProperties.batchByLightmapIndex = EditorGUILayout.Toggle(systemProperties.batchByLightmapIndex, GUILayout.MaxWidth(240));
                //    }

                //    EditorGUILayout.HelpBox("The system will try to batch the renderers by " +
                //        "the materials used and by the index in the lightmap. This is necessary " +
                //        "to properly assign lightmaps to objects. This option should always be enabled " +
                //        "if your scene requires baking into multiple lightmaps (many objects and/or high resolution lightmap). " +
                //        "Unity's standard static batching system must be disabled.", MessageType.Info);
                //}
            }
        }
    }
}

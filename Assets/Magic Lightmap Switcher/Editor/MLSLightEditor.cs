using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [CustomEditor(typeof(MLSLight))]
    public class MLSLightEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MLSLight mlsLight = (MLSLight)target;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Switch Shadow Type On Blending Value");
                mlsLight.shadowTypeSwitchValue = EditorGUILayout.Slider(mlsLight.shadowTypeSwitchValue, 0.0f, 1.0f);
            }

            using (new EditorGUILayout.HorizontalScope())
            {                
                mlsLight.exludeFromStoring = EditorGUILayout.Toggle(mlsLight.exludeFromStoring, GUILayout.MaxWidth(20));
                GUILayout.Label("Exlude From Storing");
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Script GUID");
                GUILayout.Label(mlsLight.lightGUID);
            }

            if (GUILayout.Button("Update GUID"))
            {
                mlsLight.UpdateGUID();
            }
        }
    }
}

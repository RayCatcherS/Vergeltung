using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [CustomEditor(typeof(MLSDynamicRenderer))]
    public class MLSDynamicRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MLSDynamicRenderer dynamicRenderer = (MLSDynamicRenderer)target;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Script GUID");
                GUILayout.Label(dynamicRenderer.scriptId);
                GUILayout.Space(10);
            }

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("Closest Reflection Probes:");
                GUILayout.Label(dynamicRenderer.probeIndexes[0].ToString());
                GUILayout.Label(dynamicRenderer.probeIndexes[1].ToString());
            }

            if (GUILayout.Button("Update GUID"))
            {
                dynamicRenderer.UpdateGUID();
            }
        }
    }
}

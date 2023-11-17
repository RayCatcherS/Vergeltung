using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [CustomEditor(typeof(MLSStaticRenderer))]
    public class MLSStaticRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MLSStaticRenderer staticRenderer = (MLSStaticRenderer)target;

            MeshRenderer meshRenderer = staticRenderer.GetComponent<MeshRenderer>();
            Terrain terrain = staticRenderer.GetComponent<Terrain>();

            if (meshRenderer != null)
            {
                if (meshRenderer.scaleInLightmap == 0)
                {
                    DestroyImmediate(staticRenderer.GetComponent<MLSStaticRenderer>());
                }
            }

            if (terrain != null)
            {
                if (terrain.lightmapScaleOffset.x == 0 || terrain.lightmapScaleOffset.y == 0)
                {
                    DestroyImmediate(staticRenderer.GetComponent<MLSStaticRenderer>());
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Script GUID");
                GUILayout.Label(staticRenderer.scriptId);
            }

            if (GUILayout.Button("Update GUID"))
            {
                staticRenderer.UpdateGUID();
            }
        }
    }
}

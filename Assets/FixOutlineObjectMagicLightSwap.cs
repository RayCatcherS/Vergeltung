using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MagicLightmapSwitcher;

public class FixOutlineObjectMagicLightSwap {
#if UNITY_EDITOR
    [MenuItem("Auto/Fix objects with outline component")]
    public static void FindAndFix() {
        GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allGameObjects) {
            Outline outline = go.GetComponent<Outline>();
            if (outline != null) {

                MLSDynamicRenderer mLSDynamicRenderer = go.GetComponent<MLSDynamicRenderer>();
                if(mLSDynamicRenderer != null) {

                    Object.DestroyImmediate(mLSDynamicRenderer);
                }
                MLSStaticRenderer mLSStaticRenderer = go.GetComponent<MLSStaticRenderer>();
                if(mLSStaticRenderer != null) {

                    Object.DestroyImmediate(mLSStaticRenderer);
                }
            }
        }
    }
#endif
}

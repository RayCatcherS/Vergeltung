#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MagicLightmapSwitcher
{
    public class StoreReflectionProbesData
    {
        public IEnumerator Execute(StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            MLSProgressBarHelper.StartNewStage("Storing Reflection Probes...");

            TextureImporterSettings textureImporterSettings = new TextureImporterSettings();

            textureImporterSettings.npotScale = TextureImporterNPOTScale.ToNearest;
            textureImporterSettings.textureShape = TextureImporterShape.TextureCube;
            textureImporterSettings.alphaSource = TextureImporterAlphaSource.FromInput;
            textureImporterSettings.filterMode = FilterMode.Bilinear;
            textureImporterSettings.mipmapEnabled = true;
            //textureImporterSettings.cubemapConvolution = TextureImporterCubemapConvolution.Specular;
            //textureImporterSettings.sRGBTexture = true;

            yield return null;

            ReflectionProbe[] sceneReflectionProbes = Object.FindObjectsOfType(typeof(ReflectionProbe)) as ReflectionProbe[];
            lightmapData.sceneLightingData.reflectionProbes = new StoredLightmapData.ReflectionProbes();
            lightmapData.sceneLightingData.reflectionProbes.name = new string[sceneReflectionProbes.Length];
            lightmapData.sceneLightingData.reflectionProbes.cubeReflectionTexture = new Cubemap[sceneReflectionProbes.Length];

            string fullStorePath = "";

            for (int i = 0; i < sceneReflectionProbes.Length; i++)
            {
                fullStorePath = mainComponent.currentDataPath + "/ReflectionProbe-" + lightmapData.sceneLightingData.lightmapName + "_" + i + ".exr";

                if (!sceneReflectionProbes[i].gameObject.name.Contains("MLS"))
                {
                    sceneReflectionProbes[i].gameObject.name = "MLS_" + sceneReflectionProbes[i].gameObject.name + "::" + i;
                }

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                HDAdditionalReflectionData additionalReflectionData = sceneReflectionProbes[i].GetComponent<HDAdditionalReflectionData>();

                if (additionalReflectionData == null)
                {
                    Selection.activeGameObject = sceneReflectionProbes[i].gameObject;
                }
                else
                {
                    Debug.LogWarningFormat("<color=cyan>MLS:</color> " +
                                           "The HDAdditionalReflectionData component of the reflection probe was not found. " +
                                           "You may have just installed the HDRP package and Unity hasn't updated the reflection probe " +
                                           "components yet. To fix the problem, simply select the reflection probe in the scene, and then rebake.");
                }
                
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(additionalReflectionData.bakedTexture), fullStorePath);

                lightmapData.sceneLightingData.reflectionProbes.name[i] = sceneReflectionProbes[i].name;
                lightmapData.sceneLightingData.reflectionProbes.cubeReflectionTexture[i] = AssetDatabase.LoadAssetAtPath<Cubemap>(fullStorePath);
#else
                sceneReflectionProbes[i].bakedTexture = null;

                if (Lightmapping.BakeReflectionProbe(sceneReflectionProbes[i], fullStorePath))
                {
                    TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(fullStorePath);

                    importer.SetTextureSettings(textureImporterSettings);
                    importer.SaveAndReimport();
                }

                // sceneReflectionProbes[i].mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;
                sceneReflectionProbes[i].bakedTexture = sceneReflectionProbes[i].customBakedTexture;

                lightmapData.sceneLightingData.reflectionProbes.name[i] = sceneReflectionProbes[i].name;
                lightmapData.sceneLightingData.reflectionProbes.cubeReflectionTexture[i] = sceneReflectionProbes[i].bakedTexture as Cubemap;
#endif

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateProgress(sceneReflectionProbes.Length, 0))
                    {
                        yield return null;
                    }
                }
            }

            AssetDatabase.SaveAssets();

            fullStorePath = mainComponent.currentDataPath + "/SkyboxReflectionProbe-" + lightmapData.sceneLightingData.lightmapName + ".exr";

            GameObject tmpGameObject = new GameObject();
            ReflectionProbe tmpReflection = tmpGameObject.AddComponent<ReflectionProbe>();
            tmpReflection.resolution = RenderSettings.defaultReflectionResolution;
            tmpReflection.clearFlags = UnityEngine.Rendering.ReflectionProbeClearFlags.Skybox;
            tmpReflection.cullingMask = 0;
            tmpReflection.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;

            if (Lightmapping.BakeReflectionProbe(tmpReflection, fullStorePath))
            {
                if (mainComponent.systemProperties.highDefinitionRPActive)
                {
                    TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(fullStorePath);

                    importer.SetTextureSettings(textureImporterSettings);
                    importer.SaveAndReimport();
                }
            }

            lightmapData.sceneLightingData.skyboxReflectionTexture = new Cubemap[1];
            lightmapData.sceneLightingData.skyboxReflectionTexture[0] = tmpReflection.customBakedTexture as Cubemap;

            GameObject.DestroyImmediate(tmpGameObject);

            AssetDatabase.Refresh();
            EditorUtility.SetDirty(lightmapData);
            AssetDatabase.Refresh();

            MLSLightmapDataStoring.stageExecuting = false;
        }
    }
}
#endif
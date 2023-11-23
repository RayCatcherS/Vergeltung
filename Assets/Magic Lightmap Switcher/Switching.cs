using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Reflection;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
using UnityEngine.Rendering.HighDefinition;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicLightmapSwitcher
{
    public class Switching
    {
        private static bool stageInOperation;

#if UNITY_EDITOR
        private static IEnumerator loadLightingDataRoutine;

        private static void LoadLightingDataIteratorUpdate()
        {
            if (loadLightingDataRoutine != null && loadLightingDataRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= LoadLightingDataIteratorUpdate;
        }
#endif

        public enum LoadMode
        {
            Asynchronously,
            Synchronously
        }

        private static IEnumerator LoadLightingDataAsync(StoredLightmapData lightmapData)
        {
            LoadTextues(lightmapData);

            while (stageInOperation)
            {
                yield return null;
            }

            LoadRenderersData(lightmapData);

            while (stageInOperation)
            {
                yield return null;
            }

            LoadTerrainsData(lightmapData);

            while (stageInOperation)
            {
                yield return null;
            }

            LoadLightSourcesData(lightmapData);

            while (stageInOperation)
            {
                yield return null;
            }

            LoadReflectionProbesData(lightmapData);

            while (stageInOperation)
            {
                yield return null;
            }

            LoadCustomBlendablesData(lightmapData);

            while (stageInOperation)
            {
                yield return null;
            }

            LoadLightProbesData(lightmapData);

            while (stageInOperation)
            {
                yield return null;
            }

            LoadEnvironmentSettings(lightmapData);

            while (stageInOperation)
            {
                yield return null;
            }

#if UNITY_EDITOR
            EditorApplication.update -= LoadLightingDataIteratorUpdate;
#endif
        }

        private static void LoadTextues(StoredLightmapData lightmapData)
        {
            stageInOperation = true;

            LightmapData[] newLightmaps = new LightmapData[lightmapData.sceneLightingData.lightmapsLight.Length];

            for (int i = 0; i < newLightmaps.Length; i++)
            {
                newLightmaps[i] = new LightmapData();
                newLightmaps[i].lightmapColor = lightmapData.sceneLightingData.lightmapsLight[i];

                if (lightmapData.sceneLightingData.lightmapsDirectional.Length > 0)
                {
                    if (lightmapData.sceneLightingData.lightmapsDirectional[i] != null)
                    {
                        newLightmaps[i].lightmapDir = lightmapData.sceneLightingData.lightmapsDirectional[i];
                    }
                }

                if (lightmapData.sceneLightingData.lightmapsShadowmask.Length > 0)
                {
                    if (lightmapData.sceneLightingData.lightmapsShadowmask[i] != null)
                    {
                        newLightmaps[i].shadowMask = lightmapData.sceneLightingData.lightmapsShadowmask[i];
                    }
                }
            }

            LightmapSettings.lightmaps = newLightmaps;

            stageInOperation = false;
        }

        private static void LoadRenderersData(StoredLightmapData lightmapData)
        {
            stageInOperation = true;

            List<MeshRenderer> renderers = new List<MeshRenderer>(GameObject.FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[]);

            for (int i = 0; i < renderers.Count; i++)
            {
                MLSStaticRenderer staticRenderer = renderers[i].GetComponent<MLSStaticRenderer>();

                if (staticRenderer != null)
                {
                    StoredLightmapData.RendererData rendererData = 
                        lightmapData.rendererDataDeserialized[staticRenderer.scriptId] as StoredLightmapData.RendererData;

                    renderers[i].lightmapIndex = rendererData.lightmapIndex;
                    renderers[i].lightmapScaleOffset = rendererData.lightmapScaleOffset;
                }
            }

            stageInOperation = false;
        }

        private static void LoadTerrainsData(StoredLightmapData lightmapData)
        {
            stageInOperation = true;

            Terrain[] terrains = GameObject.FindObjectsOfType(typeof(Terrain)) as Terrain[];
            int counter = 0;

            foreach (Terrain terrain in terrains)
            {
                terrain.lightmapIndex = lightmapData.sceneLightingData.terrainDatas[counter].lightmapIndex;
                terrain.lightmapScaleOffset = lightmapData.sceneLightingData.terrainDatas[counter].lightmapOffsetScale;

                counter++;
            }

            stageInOperation = false;
        }

        private static void LoadLightSourcesData(StoredLightmapData lightmapData)
        {
            stageInOperation = true;

            List<Light> lightSources = new List<Light>(GameObject.FindObjectsOfType(typeof(Light)) as Light[]);

            for (int i = 0; i < lightSources.Count; i++)
            {
                MLSLight mlsLight = lightSources[i].GetComponent<MLSLight>();

                if (mlsLight == null)
                {
                    continue;
                }

                StoredLightmapData.LightSourceData currentLightData =
                        System.Array.Find(lightmapData.sceneLightingData.lightSourceDatas, item => item.lightUID == mlsLight.lightGUID);

                if (currentLightData != null)
                {
                    lightSources[i].transform.position = currentLightData.position;
                    lightSources[i].transform.rotation = currentLightData.rotation;
                    lightSources[i].intensity = currentLightData.intensity;
                    lightSources[i].color = currentLightData.color;
                    lightSources[i].colorTemperature = currentLightData.temperature;
                    lightSources[i].range = currentLightData.range;
                    lightSources[i].shadows = (LightShadows)currentLightData.shadowType;
                    lightSources[i].spotAngle = currentLightData.spotAngle;
                    lightSources[i].shadowStrength = currentLightData.shadowStrength;
                }
            }

            stageInOperation = false;
        }

        private static void LoadReflectionProbesData(StoredLightmapData lightmapData)
        {
            stageInOperation = true;

            ReflectionProbe[] sceneReflectionProbes = GameObject.FindObjectsOfType<ReflectionProbe>();

            for (int i = 0; i < sceneReflectionProbes.Length; i++)
            {
                Cubemap reflectionTexture = lightmapData.storedReflectionProbeDataDeserialized[sceneReflectionProbes[i].name] as Cubemap;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                sceneReflectionProbes[i].GetComponent<HDAdditionalReflectionData>().bakedTexture = reflectionTexture;
#else
                sceneReflectionProbes[i].bakedTexture = reflectionTexture;
#endif
            }

            stageInOperation = false;
        }

        private static void LoadCustomBlendablesData(StoredLightmapData lightmapData)
        {
            stageInOperation = true;

            MLSCustomBlendable[] customBlendables = GameObject.FindObjectsOfType<MLSCustomBlendable>();

            for (int i = 0; i < customBlendables.Length; i++)
            {
                for (int j = 0; j < lightmapData.sceneLightingData.customBlendableDatas.Length; j++)
                {
                    //if (customBlendables[i].sourceScriptId == lightmapData.sceneLightingData.customBlendableDatas[j].sourceScriptId)
                    //{
                    //    FieldInfo[] sorceScriptFields = customBlendables[i].GetType().GetFields();

                    //    //for (int p = 0; p < lightmapData.sceneLightingData.customBlendableDatas[j].blendableFloatFields.Length; p++)
                    //    //{
                    //    //    if (sorceScriptFields[p].FieldType.Name == "Cubemap")
                    //    //    {
                    //    //        sorceScriptFields[p].SetValue(customBlendables[i], lightmapData.sceneLightingData.customBlendableDatas[j].blendableCubemapParametersValues[p]);
                    //    //    }
                    //    //    else if (sorceScriptFields[p].FieldType.Name == "Single")
                    //    //    {
                    //    //        sorceScriptFields[p].SetValue(customBlendables[i], lightmapData.sceneLightingData.customBlendableDatas[j].blendableFloatParametersValues[p]);
                    //    //    }
                    //    //}
                    //}
                }
            }

            stageInOperation = false;
        }

        private static void LoadLightProbesData(StoredLightmapData lightmapData)
        {
            stageInOperation = true;

            if (lightmapData.sceneLightingData.lightProbes == null || lightmapData.sceneLightingData.lightProbes.Length == 0)
            {
                stageInOperation = false;
                return;
            }

            SphericalHarmonicsL2[] sphericalHarmonicsArray = new SphericalHarmonicsL2[lightmapData.sceneLightingData.lightProbes.Length];

            for (int i = 0; i < sphericalHarmonicsArray.Length; i++)
            {
                SphericalHarmonicsL2 sphericalHarmonics = new SphericalHarmonicsL2();

                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        sphericalHarmonics[j, k] = lightmapData.sceneLightingData.lightProbes[i].coefficients[j * 9 + k];
                    }
                }

                sphericalHarmonicsArray[i] = sphericalHarmonics;
            }

            LightmapSettings.lightProbes.bakedProbes = sphericalHarmonicsArray;

            stageInOperation = false;
        }

        private static void LoadEnvironmentSettings(StoredLightmapData lightmapData)
        {
            stageInOperation = true;

            RenderSettings.fog = lightmapData.sceneLightingData.fogSettings.enabled;
            RenderSettings.fogColor = lightmapData.sceneLightingData.fogSettings.fogColor;
            RenderSettings.fogDensity = lightmapData.sceneLightingData.fogSettings.fogDensity;

            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetTexture("_Tex", lightmapData.sceneLightingData.skyboxSettings.skyboxTexture);
                RenderSettings.skybox.SetFloat("_Exposure", lightmapData.sceneLightingData.skyboxSettings.exposure);
                RenderSettings.skybox.SetColor("_Tint", lightmapData.sceneLightingData.skyboxSettings.tintColor);
            }

            RenderSettings.ambientMode = lightmapData.sceneLightingData.environmentSettings.source;
            RenderSettings.ambientIntensity = lightmapData.sceneLightingData.environmentSettings.intensityMultiplier;
            RenderSettings.ambientLight = lightmapData.sceneLightingData.environmentSettings.ambientColor;
            RenderSettings.ambientSkyColor = lightmapData.sceneLightingData.environmentSettings.skyColor;
            RenderSettings.ambientEquatorColor = lightmapData.sceneLightingData.environmentSettings.equatorColor;
            RenderSettings.ambientGroundColor = lightmapData.sceneLightingData.environmentSettings.groundColor;

            stageInOperation = false;
        }

        public static void LoadLightingData(MagicLightmapSwitcher parent, int lightmapDataIndex, LoadMode loadMode = LoadMode.Synchronously)
        {
            parent.SetBlendingOptionsGlobal(MagicLightmapSwitcher.BlendingOptions.None);
            parent.resetAffectedObjects = true;

            StoredLightmapData dataToLoad = parent.currentLightmapScenario.blendableLightmaps[lightmapDataIndex].lightingData;

            if (loadMode == LoadMode.Asynchronously)
            {
#if UNITY_EDITOR
                loadLightingDataRoutine = LoadLightingDataAsync(dataToLoad);
                EditorApplication.update += LoadLightingDataIteratorUpdate;
#else

                parent.StartCoroutine(LoadLightingDataAsync(dataToLoad)); 
#endif
            }
            else
            {
                LoadEnvironmentSettings(dataToLoad);
                LoadTextues(dataToLoad);
                LoadRenderersData(dataToLoad);
                LoadTerrainsData(dataToLoad);
                LoadLightSourcesData(dataToLoad);
                LoadReflectionProbesData(dataToLoad);
                LoadCustomBlendablesData(dataToLoad);
                LoadLightProbesData(dataToLoad);

#if UNITY_EDITOR
                EditorWindow view = EditorWindow.GetWindow<SceneView>();
                view.Repaint();
#endif
            }
        }
    }
}

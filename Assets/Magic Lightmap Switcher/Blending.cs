using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MagicLightmapSwitcher
{
    public class Blending
    {
        private static bool _propertyIdsInitialized = false;

        public static int _MLS_ENABLE_LIGHTMAPS_BLENDING;
        public static int _MLS_ENABLE_REFLECTIONS_BLENDING;
        public static int _MLS_ENABLE_SKY_CUBEMAPS_BLENDING;
        public static int _MLS_REFLECTIONS_FLAG;
        public static int _MLS_CURRENT_LIGHTMAP_PAIR;
        public static int _MLS_Cubemap_Array;
        public static int _MLS_Lightmap_Color_Array;
        public static int _MLS_Lightmap_Directional_Array;
        public static int _MLS_Lightmap_ShadowMask_Array;
        public static int _MLS_Lightmap_Color_Blend_From;
        public static int _MLS_Lightmap_Color_Blend_To;
        public static int _MLS_Lightmap_Directional_Blend_From;
        public static int _MLS_Lightmap_Directional_Blend_To;
        public static int _MLS_Lightmap_ShadowMask_Blend_From;
        public static int _MLS_Lightmap_ShadowMask_Blend_To;
        public static int _MLS_Reflection_Blend_From_0;
        public static int _MLS_Reflection_Blend_To_0;
        public static int _MLS_Reflection_Blend_From_1;
        public static int _MLS_Reflection_Blend_To_1;
        public static int _MLS_Lightmaps_Blend_Factor;
        public static int _MLS_Reflections_Blend_Factor;
        public static int _MLS_Sky_Cubemap_Blend_Factor;
        public static int _MLS_Sky_Cubemap_Blend_From;
        public static int _MLS_Sky_Cubemap_Blend_To;
        public static int _MLS_Sky_Blend_From_Exposure;
        public static int _MLS_Sky_Blend_To_Exposure;
        public static int _MLS_Sky_Blend_From_Tint;
        public static int _MLS_Sky_Blend_To_Tint;
        public static int _MLS_SkyReflection_Blend_From;
        public static int _MLS_SkyReflection_Blend_To;

#if BAKERY_INCLUDED
        public static int _MLS_BakeryRNM0_Array;
        public static int _MLS_BakeryRNM1_Array;
        public static int _MLS_BakeryRNM2_Array;
        
        public static int _MLS_BakeryRNM0_From;
        public static int _MLS_BakeryRNM0_To;
        public static int _MLS_BakeryRNM1_From;
        public static int _MLS_BakeryRNM1_To;
        public static int _MLS_BakeryRNM2_From;
        public static int _MLS_BakeryRNM2_To;
        public static int _MLS_BakeryVolume0_From;
        public static int _MLS_BakeryVolume0_To;
        public static int _MLS_BakeryVolume1_From;
        public static int _MLS_BakeryVolume1_To;
        public static int _MLS_BakeryVolume2_From;
        public static int _MLS_BakeryVolume2_To;
        public static int _MLS_BakeryVolumeMask_From;
        public static int _MLS_BakeryVolumMask_To;
        public static int _MLS_BakeryVolumeCompressed_From;
        public static int _MLS_BakeryVolumeCompressed_To;
#endif

        public static Dictionary<string, BlendingOperationalData> blendingOperationalDatas = new Dictionary<string, BlendingOperationalData>();
        public static List<MagicLightmapSwitcher.AffectedObject> resultStaticAffectedObjects;
        public static List<MagicLightmapSwitcher.AffectedObject> resultDynamicAffectedObjects;
        private static List<MLSLight> resultAffectedLights;
        private static bool lightProbesArrayProcessing;
        private static Queue<BlendProbesThreadData> blendProbesThreadsQueue = new Queue<BlendProbesThreadData>();
        private static Queue<ProbesReplacingThreadData> probesReplacingThreadsQueue = new Queue<ProbesReplacingThreadData>();
        private static ProbesReplacingThreadData lastReplacedProbesData = new ProbesReplacingThreadData();
        
        private static CubemapArray _cubemapArray;
        private static int _cubemapWidth;
        private static int _cubemapCount;
        private static GraphicsFormat _cubemapFormat;
        private static Texture2DArray _lightmapLightArray;
        private static Texture2DArray _lightmapDirArray;
        private static Texture2DArray _lightmapShadowMaskArray;
        
        #if BAKERY_INCLUDED
        private static Texture2DArray _lightmapBakeryRNM0Array;
        private static Texture2DArray _lightmapBakeryRNM1Array;
        private static Texture2DArray _lightmapBakeryRNM2Array;

        private static int _lightmapRNM_Width;
        private static int _lightmapRNM_Height;
        private static int _lightmapRNM_Depth;
        private static int _lightmapRNM_Count;
        #endif
        
        private static int _lightmapWidth;
        private static int _lightmapHeight;
        private static int _lightmapDepth;
        private static int _lightmapCount;

        private static int _lastFromIndex = -1;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
        public static HDRenderPipelineAsset hdRenderPipelineAsset;
        private static HDAdditionalReflectionData hdRelectionProbeData;
#endif
        private static bool isDeferredMode = false;
        
        public class BlendingOperationalData
        {
            public string sceneName;
            public int loadIndex;
            public int lightProbesArrayIndex;
        }

        public class BlendProbesThreadData
        {
            public bool isBusy;
            public MagicLightmapSwitcher switcherInstance;
            public int lightProbesArrayPosition;
            public float[] blendFromArray;
            public float[] blendToArray;
            public SphericalHarmonicsL2[] sphericalHarmonicsArray;
            public float blendFactor;
        }

        public class ProbesReplacingThreadData
        {
            public bool isBusy;
            public MagicLightmapSwitcher switcherInstance;
            public SphericalHarmonicsL2[] sphericalHarmonicsArray;
            public BlendProbesThreadData lastProbesData;
        }

        public static void InitiShaderProperties()
        {
            if (!_propertyIdsInitialized)
            {
                _MLS_ENABLE_LIGHTMAPS_BLENDING = Shader.PropertyToID("_MLS_ENABLE_LIGHTMAPS_BLENDING");
                _MLS_ENABLE_REFLECTIONS_BLENDING = Shader.PropertyToID("_MLS_ENABLE_REFLECTIONS_BLENDING");
                _MLS_ENABLE_SKY_CUBEMAPS_BLENDING = Shader.PropertyToID("_MLS_ENABLE_SKY_CUBEMAPS_BLENDING");
                _MLS_REFLECTIONS_FLAG = Shader.PropertyToID("_MLS_ReflectionsFlag");
                _MLS_CURRENT_LIGHTMAP_PAIR = Shader.PropertyToID("_MLS_CURRENT_LIGHTMAP_PAIR");
                _MLS_Cubemap_Array = Shader.PropertyToID("_MLS_Cubemap_Array");
                _MLS_Lightmap_Color_Array = Shader.PropertyToID("_MLS_Lightmap_Color_Array");
                _MLS_Lightmap_Directional_Array = Shader.PropertyToID("_MLS_Lightmap_Directional_Array");
                _MLS_Lightmap_ShadowMask_Array = Shader.PropertyToID("_MLS_Lightmap_ShadowMask_Array");
                _MLS_Lightmap_Color_Blend_From = Shader.PropertyToID("_MLS_Lightmap_Color_Blend_From");
                _MLS_Lightmap_Color_Blend_To = Shader.PropertyToID("_MLS_Lightmap_Color_Blend_To");
                _MLS_Lightmap_Directional_Blend_From = Shader.PropertyToID("_MLS_Lightmap_Dir_Blend_From");
                _MLS_Lightmap_Directional_Blend_To = Shader.PropertyToID("_MLS_Lightmap_Dir_Blend_To");
                _MLS_Lightmap_ShadowMask_Blend_From = Shader.PropertyToID("_MLS_Lightmap_ShadowMask_Blend_From");
                _MLS_Lightmap_ShadowMask_Blend_To = Shader.PropertyToID("_MLS_Lightmap_ShadowMask_Blend_To");
                _MLS_Reflection_Blend_From_0 = Shader.PropertyToID("_MLS_Reflection_Blend_From_0");
                _MLS_Reflection_Blend_To_0 = Shader.PropertyToID("_MLS_Reflection_Blend_To_0");
                _MLS_Reflection_Blend_From_1 = Shader.PropertyToID("_MLS_Reflection_Blend_From_1");
                _MLS_Reflection_Blend_To_1 = Shader.PropertyToID("_MLS_Reflection_Blend_To_1");
                _MLS_Lightmaps_Blend_Factor = Shader.PropertyToID("_MLS_Lightmaps_Blend_Factor");
                _MLS_Reflections_Blend_Factor = Shader.PropertyToID("_MLS_Reflections_Blend_Factor");
                _MLS_Sky_Cubemap_Blend_Factor = Shader.PropertyToID("_MLS_Sky_Cubemap_Blend_Factor");
                _MLS_Sky_Cubemap_Blend_From = Shader.PropertyToID("_MLS_Sky_Cubemap_Blend_From");
                _MLS_Sky_Cubemap_Blend_To = Shader.PropertyToID("_MLS_Sky_Cubemap_Blend_To");
                _MLS_Sky_Blend_From_Exposure = Shader.PropertyToID("_MLS_Sky_Blend_From_Exposure");
                _MLS_Sky_Blend_To_Exposure = Shader.PropertyToID("_MLS_Sky_Blend_To_Exposure");
                _MLS_Sky_Blend_From_Tint = Shader.PropertyToID("_MLS_Sky_Blend_From_Tint");
                _MLS_Sky_Blend_To_Tint = Shader.PropertyToID("_MLS_Sky_Blend_To_Tint");
                _MLS_SkyReflection_Blend_From = Shader.PropertyToID("_MLS_SkyReflection_Blend_From");
                _MLS_SkyReflection_Blend_To = Shader.PropertyToID("_MLS_SkyReflection_Blend_To");

#if BAKERY_INCLUDED
                _MLS_BakeryRNM0_Array = Shader.PropertyToID("_MLS_BakeryRNM_0_Array");
                _MLS_BakeryRNM1_Array = Shader.PropertyToID("_MLS_BakeryRNM_1_Array");
                _MLS_BakeryRNM2_Array = Shader.PropertyToID("_MLS_BakeryRNM_2_Array");
                
                _MLS_BakeryRNM0_From = Shader.PropertyToID("_MLS_BakeryRNM0_From");
                _MLS_BakeryRNM0_To = Shader.PropertyToID("_MLS_BakeryRNM0_To");
                _MLS_BakeryRNM1_From = Shader.PropertyToID("_MLS_BakeryRNM1_From");
                _MLS_BakeryRNM1_To = Shader.PropertyToID("_MLS_BakeryRNM1_To");
                _MLS_BakeryRNM2_From = Shader.PropertyToID("_MLS_BakeryRNM2_From");
                _MLS_BakeryRNM2_To = Shader.PropertyToID("_MLS_BakeryRNM2_To");
                
                _MLS_BakeryVolume0_From = Shader.PropertyToID("_MLS_BakeryVolume0_From");
                _MLS_BakeryVolume0_To = Shader.PropertyToID("_MLS_BakeryVolume0_To");
                _MLS_BakeryVolume1_From = Shader.PropertyToID("_MLS_BakeryVolume1_From");
                _MLS_BakeryVolume1_To = Shader.PropertyToID("_MLS_BakeryVolume1_To");
                _MLS_BakeryVolume2_From = Shader.PropertyToID("_MLS_BakeryVolume2_From");
                _MLS_BakeryVolume2_To = Shader.PropertyToID("_MLS_BakeryVolume2_To");
                _MLS_BakeryVolumeMask_From = Shader.PropertyToID("_MLS_BakeryVolumeMask_From");
                _MLS_BakeryVolumMask_To = Shader.PropertyToID("_MLS_BakeryVolumeMask_To");
                _MLS_BakeryVolumeCompressed_From = Shader.PropertyToID("_MLS_BakeryVolumeCompressed_From");
                _MLS_BakeryVolumeCompressed_To = Shader.PropertyToID("_MLS_BakeryVolumeCompressed_To");
#endif

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                hdRenderPipelineAsset = (HDRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;
#endif
                _propertyIdsInitialized = true;
            }
        }

        public static void UpdateBlendingOperationalData(string targetScene)
        {
#if UNITY_2020_1_OR_NEWER
            MagicLightmapSwitcher[] magicLightmapSwitchers = GameObject.FindObjectsOfType<MagicLightmapSwitcher>();
            int totalProbesOnScene = 0;

            for (int i = 0; i < magicLightmapSwitchers.Length; i++)
            {
                if (magicLightmapSwitchers[i].availableScenarios.Count > 0 && magicLightmapSwitchers[i].availableScenarios[0].blendableLightmaps.Count > 0)
                {
                    totalProbesOnScene += magicLightmapSwitchers[i].availableScenarios[0].blendableLightmaps[0].lightingData.sceneLightingData.initialLightProbesArrayPosition;
                    magicLightmapSwitchers[i].availableScenarios[0].lightProbesArrayPosition = totalProbesOnScene - magicLightmapSwitchers[i].availableScenarios[0].blendableLightmaps[0].lightingData.sceneLightingData.initialLightProbesArrayPosition;
                }
            }
#else
            if (!blendingOperationalDatas.ContainsKey(targetScene))
            {
                BlendingOperationalData blendingOperationalData = new BlendingOperationalData();

                blendingOperationalData.sceneName = targetScene;
                blendingOperationalData.loadIndex = blendingOperationalDatas.Count;
                blendingOperationalData.lightProbesArrayIndex = 0;

                blendingOperationalDatas.Add(targetScene, blendingOperationalData);
            }
#endif
        }

        public static void Blend(MagicLightmapSwitcher switcherInstance, float blendFactor, StoredLightingScenario storedLightmapScenario, string targetScene)
        {
            switcherInstance.currentLightmapScenario = storedLightmapScenario;

            if (!storedLightmapScenario.selfTestCompleted && !storedLightmapScenario.SelfTest())
            {
                Debug.LogErrorFormat("<color=cyan>MLS:</color> An error was detected in the stored lightmap data. " +
                    "Try reassigning the data in the blending queue. Scenario: " + storedLightmapScenario.name);
                return;
            }
            else
            {
                if (!switcherInstance.storedDataUpdated)
                {
                    switcherInstance.StartCoroutine(switcherInstance.UpdateStoredArray(SceneManager.GetActiveScene().name, true));
                }

                if (!storedLightmapScenario.selfTestSuccess)
                {
                    return;
                }
            }

            if (Camera.main == null)
            {
                Debug.LogErrorFormat("<color=cyan>MLS:</color> You have not installed the main camera. Tag the main camera with \"MainCamera\".");
                return;
            }

            if (switcherInstance.workflow == MagicLightmapSwitcher.Workflow.MultiScene)
            {
                switcherInstance.staticAffectedObjects.TryGetValue(targetScene, out resultStaticAffectedObjects);
                switcherInstance.dynamicAffectedObjects.TryGetValue(targetScene, out resultDynamicAffectedObjects);
                switcherInstance.storedLights.TryGetValue(targetScene, out resultAffectedLights);
            }
            else
            {
                resultStaticAffectedObjects = switcherInstance.sceneStaticAffectedObjects;
                resultDynamicAffectedObjects = switcherInstance.sceneDynamicAffectedObjects;
                resultAffectedLights = switcherInstance.sceneAffectedLightSources;
            }

            for (int i = 0; i < storedLightmapScenario.blendableLightmaps.Count; i++)
            {
                if (storedLightmapScenario.targetScene != storedLightmapScenario.blendableLightmaps[i].lightingData.dataPrefix)
                {
                    Debug.LogErrorFormat("<color=cyan>MLS:</color>The \"Blendable Lightmaps Queue\"" +
                        "contains invalid data. Make sure the queue contains the data stored for the current scene.");
                    return;
                }

                if (i < storedLightmapScenario.blendableLightmaps.Count - 2)
                {
                    if (blendFactor >= storedLightmapScenario.blendableLightmaps[i].startValue && blendFactor <= storedLightmapScenario.blendableLightmaps[i + 1].startValue)
                    {
                        storedLightmapScenario.lightingDataFromIndex =
                            storedLightmapScenario.blendableLightmaps[i].blendingIndex;
                        storedLightmapScenario.lightingDataToIndex =
                            storedLightmapScenario.blendableLightmaps[i + 1].blendingIndex;

                        storedLightmapScenario.localBlendFactor =
                            Mathf.Clamp((blendFactor - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataFromIndex].startValue) /
                            (storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].startValue - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataFromIndex].startValue), 0, 1);

                        break;
                    }
                }
                else
                {
                    if (blendFactor >= storedLightmapScenario.blendableLightmaps[i].startValue)
                    {
                        storedLightmapScenario.lightingDataFromIndex = storedLightmapScenario.blendableLightmaps[i].blendingIndex;
                        storedLightmapScenario.lightingDataToIndex = storedLightmapScenario.blendableLightmaps.Count - 1;

                        storedLightmapScenario.localBlendFactor =
                            Mathf.Clamp((blendFactor - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataFromIndex].startValue) /
                            (1 - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataFromIndex].startValue), 0, 1);

                        break;
                    }
                }
            }

            float reflectionsRangedBlend =
                    Mathf.Clamp((storedLightmapScenario.localBlendFactor - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].reflectionsBlendingRange.x) /
                    (storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].reflectionsBlendingRange.y - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].reflectionsBlendingRange.x), 0, 1);

            float lightmapsRangedBlend =
                    Mathf.Clamp((storedLightmapScenario.localBlendFactor - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].lightmapBlendingRange.x) /
                    (storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].lightmapBlendingRange.y - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].lightmapBlendingRange.x), 0, 1);

            storedLightmapScenario.reflectionsRangedBlendFactor = reflectionsRangedBlend;
            storedLightmapScenario.lightmapsRangedBlendFactor = lightmapsRangedBlend;
            
            BlendLightmapsData(switcherInstance, storedLightmapScenario);

            if ((storedLightmapScenario.blendingModules & (1 << 3)) > 0)
            {
                BlendLightProbesData(switcherInstance, storedLightmapScenario,
                        storedLightmapScenario.lightingDataFromIndex, storedLightmapScenario.lightingDataToIndex,
                        lightmapsRangedBlend);
            }
            
#if BAKERY_INCLUDED
            if ((storedLightmapScenario.blendingModules & (1 << 5)) > 0)
#else
            if ((storedLightmapScenario.blendingModules & (1 << 4)) > 0)
#endif
            {
                BlendLightSourcesData(storedLightmapScenario.localBlendFactor, blendFactor,
                    storedLightmapScenario.blendableLightmaps, storedLightmapScenario.lightingDataFromIndex,
                    storedLightmapScenario.lightingDataToIndex);
            }

#if BAKERY_INCLUDED
            if ((storedLightmapScenario.blendingModules & (1 << 6)) > 0)
#else
            if ((storedLightmapScenario.blendingModules & (1 << 5)) > 0)
#endif
            {
                BlendCustomData(storedLightmapScenario.localBlendFactor, blendFactor, reflectionsRangedBlend,
                    lightmapsRangedBlend, storedLightmapScenario, storedLightmapScenario.lightingDataFromIndex,
                    storedLightmapScenario.lightingDataToIndex);
            }

#if BAKERY_INCLUDED
            if ((storedLightmapScenario.blendingModules & (1 << 7)) > 0)
#else
            if ((storedLightmapScenario.blendingModules & (1 << 6)) > 0)
#endif
            {
                BlendGameObjectsData(storedLightmapScenario.localBlendFactor, blendFactor,
                    storedLightmapScenario.blendableLightmaps, storedLightmapScenario.lightingDataFromIndex,
                    storedLightmapScenario.lightingDataToIndex);
            }

#if BAKERY_INCLUDED
            if ((storedLightmapScenario.blendingModules & (1 << 8)) > 0)
#else
            if ((storedLightmapScenario.blendingModules & (1 << 7)) > 0)
#endif
            {
                BlendCommonLightingSettings(lightmapsRangedBlend, storedLightmapScenario.blendableLightmaps,
                    storedLightmapScenario.lightingDataFromIndex, storedLightmapScenario.lightingDataToIndex);
            }

            switcherInstance.lastLightmapScenario = storedLightmapScenario;
            switcherInstance.OnBlendingValueChanged[storedLightmapScenario.eventsListId].Invoke(storedLightmapScenario, blendFactor, reflectionsRangedBlend, lightmapsRangedBlend);
        }

        private static void SetReflectionsBlendingState(MagicLightmapSwitcher.AffectedObject targetObject, int val)
        {
            targetObject.SetShaderFloat(_MLS_ENABLE_REFLECTIONS_BLENDING, val);
        }

        public static void BlendReflectionProbes(
            MagicLightmapSwitcher.AffectedObject targetObject,
            List<StoredLightingScenario.LightmapData> storedLightmapDatas,
            List<ReflectionProbeBlendInfo> closestReflectionProbes,
            int fromIndex,
            int toIndex)
        {
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
            if (hdRelectionProbeData == null)
            {
                hdRelectionProbeData = closestReflectionProbes[0].probe.gameObject
                    .GetComponent<HDAdditionalReflectionData>();
            }

            if (closestReflectionProbes[0].probe == null || hdRelectionProbeData.mode == ProbeSettings.Mode.Realtime)
#else
            if (closestReflectionProbes[0].probe == null || closestReflectionProbes[0].probe.mode == ReflectionProbeMode.Realtime)
#endif
            {
                return;
            }
            
            var firstProbe = closestReflectionProbes[0].probe.name;

            Cubemap blendFrom_0 =
                storedLightmapDatas[fromIndex].lightingData
                    .storedReflectionProbeDataDeserialized[firstProbe] as Cubemap;
            Cubemap blendTo_0 =
                storedLightmapDatas[toIndex].lightingData.storedReflectionProbeDataDeserialized[firstProbe] as Cubemap;

            if (blendFrom_0 == null || blendTo_0 == null)
            {
                SetReflectionsBlendingState(targetObject, 0);
            }
            else
            {
                SetReflectionsBlendingState(targetObject, 1);

                if (isDeferredMode)
                {
                    Shader.SetGlobalTexture(_MLS_Reflection_Blend_From_0, blendFrom_0);
                    Shader.SetGlobalTexture(_MLS_Reflection_Blend_To_0, blendTo_0);
                }
                else
                {
                    targetObject.SetShaderTexture(_MLS_Reflection_Blend_From_0, blendFrom_0);
                    targetObject.SetShaderTexture(_MLS_Reflection_Blend_To_0, blendTo_0);
                }

                if (closestReflectionProbes.Count > 1)
                {
                    if (closestReflectionProbes[0].probe == null ||
                        closestReflectionProbes[1].probe.mode == ReflectionProbeMode.Realtime)
                    {
                        return;
                    }

                    var secondProbe = closestReflectionProbes[1].probe.name;

                    Cubemap blendFrom_1 = storedLightmapDatas[fromIndex].lightingData
                        .storedReflectionProbeDataDeserialized[secondProbe] as Cubemap;
                    Cubemap blendTo_1 = storedLightmapDatas[toIndex].lightingData
                        .storedReflectionProbeDataDeserialized[secondProbe] as Cubemap;

                    if (blendFrom_0 == null || blendFrom_1 == null || blendTo_0 == null || blendTo_1 == null)
                    {
                        SetReflectionsBlendingState(targetObject, 0);
                    }
                    else
                    {
                        SetReflectionsBlendingState(targetObject, 1);

                        if (isDeferredMode)
                        {
                            Shader.SetGlobalTexture(_MLS_Reflection_Blend_From_1, blendFrom_1);
                            Shader.SetGlobalTexture(_MLS_Reflection_Blend_To_1, blendTo_1);
                        }
                        else
                        {
                            targetObject.SetShaderTexture(_MLS_Reflection_Blend_From_1, blendFrom_1);
                            targetObject.SetShaderTexture(_MLS_Reflection_Blend_To_1, blendTo_1);
                        }
                    }
                }
            }
        }

        private static void BlendSkyboxReflectionProbes(
            MagicLightmapSwitcher.AffectedObject targetObject,
            List<StoredLightingScenario.LightmapData> storedLightmapDatas,
            int fromIndex,
            int toIndex)

        {
            targetObject.SetShaderTexture(_MLS_SkyReflection_Blend_From, storedLightmapDatas[fromIndex].lightingData.sceneLightingData.skyboxReflectionTexture[0]);
            targetObject.SetShaderTexture(_MLS_SkyReflection_Blend_To, storedLightmapDatas[fromIndex].lightingData.sceneLightingData.skyboxReflectionTexture[0]);
            targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 0);
        }
        
#if BAKERY_INCLUDED
        private static BakeryVolume[] sceneVolumes;
        
        private static BakeryVolume GetClosestBakeryVolume(MagicLightmapSwitcher.AffectedObject targetObject)
        {
            Dictionary<float, BakeryVolume> distances = new Dictionary<float, BakeryVolume>();

            //if (sceneVolumes == null)
            {
                sceneVolumes = Object.FindObjectsOfType<BakeryVolume>();
            }

            for (int i = 0; i < sceneVolumes.Length; i++)
            {
                distances.Add(Vector3.Distance(targetObject.meshRenderer.transform.position, sceneVolumes[i].transform.position), sceneVolumes[i]);
            }

            return distances.Count > 0 ? distances.Min().Value : null;
        }
        
        private static void ProcessBakeryVolumes(
            MagicLightmapSwitcher.AffectedObject targetObject,
            List<StoredLightingScenario.LightmapData> storedLightmapDatas,
            BakeryVolume closestVolume,
            int fromIndex,
            int toIndex)
        {
            List<Texture3D> blendFrom =
                storedLightmapDatas[fromIndex].lightingData.bakeryVolumeDataDeserialized[closestVolume.name] as List<Texture3D>;
            List<Texture3D> blendTo =
                storedLightmapDatas[toIndex].lightingData.bakeryVolumeDataDeserialized[closestVolume.name] as List<Texture3D>;

            if (blendFrom != null && blendTo != null)
            {
                targetObject.SetShaderTexture(_MLS_BakeryVolume0_From, blendFrom[0]);
                targetObject.SetShaderTexture(_MLS_BakeryVolume0_To, blendTo[0]);
                targetObject.SetShaderTexture(_MLS_BakeryVolume1_From, blendFrom[1]);
                targetObject.SetShaderTexture(_MLS_BakeryVolume1_To, blendTo[1]);
                targetObject.SetShaderTexture(_MLS_BakeryVolume2_From, blendFrom[2]);
                targetObject.SetShaderTexture(_MLS_BakeryVolume2_To, blendTo[2]);

                if (closestVolume.bakedMask != null)
                {
                    targetObject.SetShaderTexture(_MLS_BakeryVolumeMask_From, blendFrom[3]);
                    targetObject.SetShaderTexture(_MLS_BakeryVolumMask_To, blendTo[3]);
                }
                
                if (closestVolume.bakedTexture3 != null)
                {
                    targetObject.SetShaderTexture(_MLS_BakeryVolumeCompressed_From, blendFrom[4]);
                    targetObject.SetShaderTexture(_MLS_BakeryVolumeCompressed_To, blendTo[4]);
                }
            }
        }
#endif

        private static void ProcessReflectionProbes(
            ReflectionProbeUsage reflectionProbeUsage,
            MagicLightmapSwitcher.AffectedObject targetObject,
            List<StoredLightingScenario.LightmapData> storedLightmapDatas,
            int fromIndex,
            int toIndex)
        {
            if (targetObject.meshRenderer != null)
            {
                targetObject.meshRenderer.GetClosestReflectionProbes(targetObject.reflectionProbeBlendInfo);
            }
            else if (targetObject.terrain != null)
            {
                targetObject.terrain.GetClosestReflectionProbes(targetObject.reflectionProbeBlendInfo);
            }

            switch (reflectionProbeUsage)
            {
                case ReflectionProbeUsage.Off:
                    BlendSkyboxReflectionProbes(
                        targetObject,
                        storedLightmapDatas,
                        fromIndex,
                        toIndex);

                    targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 0);
                    break;
                case ReflectionProbeUsage.BlendProbes:
                case ReflectionProbeUsage.Simple:
                    if (targetObject.reflectionProbeBlendInfo.Count > 0)
                    {
                        BlendReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            targetObject.reflectionProbeBlendInfo,
                            fromIndex,
                            toIndex);

                        targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 1);
                    }
                    else
                    {
                        BlendSkyboxReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            fromIndex,
                            toIndex);

                        targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 0);
                    }
                    break;
                case ReflectionProbeUsage.BlendProbesAndSkybox:
                    if (targetObject.reflectionProbeBlendInfo.Count > 0)
                    {
                        BlendReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            targetObject.reflectionProbeBlendInfo,
                            fromIndex,
                            toIndex);

                        BlendSkyboxReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            fromIndex,
                            toIndex);

                        targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 2);
                    }
                    else
                    {
                        BlendSkyboxReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            fromIndex,
                            toIndex);

                        targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 0);
                    }
                    break;
            }
        }

        private static void BlendLightmapsData(
            MagicLightmapSwitcher switcherInstance,
            StoredLightingScenario storedLightingScenario
        )
        {
            InitiShaderProperties();

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
            if (hdRenderPipelineAsset != null)
            {
                if ((hdRenderPipelineAsset.currentPlatformRenderPipelineSettings.supportedLitShaderMode ==
                     RenderPipelineSettings.SupportedLitShaderMode.Both ||
                     hdRenderPipelineAsset.currentPlatformRenderPipelineSettings.supportedLitShaderMode ==
                     RenderPipelineSettings.SupportedLitShaderMode.DeferredOnly)
                )
                {
                    isDeferredMode = true;
                }
            }
            else
            {
                Debug.LogWarningFormat("<color=cyan>MLS:</color>" + 
                                       "MLS is trying to work in HDRP mode because you installed this package, " +
                                       "but you did not assign an asset in the Graphics settings of your project." +
                                       "Assign asset, remove HDRP package or remove \"MT_HDRP_XX_INCLUDED\" directive from " +
                                       "Player settings of your project.");

                return;
            }
#endif
            
            #region Process Reflection Probes

            if (switcherInstance.useTextureCubeArrays)
            {
                if ((storedLightingScenario.blendingModules & (1 << 1)) > 0)
                {
                    if (switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario ||
                        !switcherInstance.cubemapArrayInitialized)
                    {
                        switcherInstance.cubemapArrayInitialized = false;

                        if (storedLightingScenario.blendableLightmaps[0].lightingData
                            .sceneLightingData.reflectionProbes.cubeReflectionTexture != null &&
                            storedLightingScenario.blendableLightmaps[0].lightingData
                            .sceneLightingData.reflectionProbes.cubeReflectionTexture.Length > 0)
                        {
                            Cubemap referenceCubemap = storedLightingScenario.blendableLightmaps[0].lightingData
                                .sceneLightingData.reflectionProbes.cubeReflectionTexture[0];

                            _cubemapWidth = referenceCubemap.width;
                            _cubemapCount = storedLightingScenario.blendableLightmaps[0].lightingData
                                .storedReflectionProbeDataDeserialized.Count;
                            _cubemapFormat = referenceCubemap.graphicsFormat;
                            _cubemapArray = new CubemapArray(_cubemapWidth,
                                storedLightingScenario.blendableLightmaps.Count * _cubemapCount,
                                referenceCubemap.format,
                                true);

                            int globalCounter = 0;
                            int localCounter = 0;

                            for (int i = 0; i < storedLightingScenario.blendableLightmaps.Count; i++)
                            {
                                for (int j = 0; j < _cubemapCount; j++)
                                {
                                    Cubemap cubemap = storedLightingScenario.blendableLightmaps[i].lightingData
                                        .sceneLightingData.reflectionProbes.cubeReflectionTexture[j];

                                    for (int k = 0; k < 6; k++)
                                    {
                                        localCounter = 6 * j + k;
                                        Graphics.CopyTexture(cubemap, k, _cubemapArray, globalCounter + localCounter);
                                    }
                                }

                                globalCounter += localCounter + 1;
                            }

                            switcherInstance.cubemapArrayInitialized = true;
                        }
                    }

                    Shader.SetGlobalTexture(_MLS_Cubemap_Array, _cubemapArray);
                    Shader.SetGlobalFloat(_MLS_Reflections_Blend_Factor, storedLightingScenario.reflectionsRangedBlendFactor);
                }

#if BAKERY_INCLUDED
                #region Process Bakery Volumes

                if ((storedLightingScenario.blendingModules & (1 << 4)) > 0)
                {
                    for (int i = 0; i < resultDynamicAffectedObjects.Count; i++)
                    {
                        if (switcherInstance.lightmapper == MagicLightmapSwitcher.Lightmapper.BakeryLightmapper)
                        {
                            BakeryVolume closestVolume = GetClosestBakeryVolume(resultDynamicAffectedObjects[i]);

                            if (closestVolume != null)
                            {
                                ProcessBakeryVolumes(
                                    resultDynamicAffectedObjects[i],
                                    storedLightingScenario.blendableLightmaps,
                                    closestVolume,
                                    storedLightingScenario.lightingDataFromIndex,
                                    storedLightingScenario.lightingDataToIndex);
                            }
                        }
                    }
                }

                #endregion
#endif
            }
            else
            {
                switcherInstance.cubemapArrayInitialized = false;
                
                for (int i = 0; i < resultDynamicAffectedObjects.Count; i++)
                {
                    if (!resultDynamicAffectedObjects[i].meshRenderer.isVisible)
                    {
                        continue;
                    }

                    if (resultDynamicAffectedObjects[i].meshRenderer != null)
                    {
                        resultDynamicAffectedObjects[i].InitPropertyBlock();
                    }
                    else
                    {
                        resultDynamicAffectedObjects.RemoveAt(i);
                        return;
                    }

                    if (storedLightingScenario.blendableLightmaps.Count < 3 ||
                        (resultDynamicAffectedObjects[i].lastFromIndex !=
                         storedLightingScenario.lightingDataFromIndex ||
                         switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario))
                    {
                        if ((storedLightingScenario.blendingModules & (1 << 1)) > 0)
                        {
                            ProcessReflectionProbes(
                                resultDynamicAffectedObjects[i].meshRenderer.reflectionProbeUsage,
                                resultDynamicAffectedObjects[i],
                                storedLightingScenario.blendableLightmaps,
                                storedLightingScenario.lightingDataFromIndex,
                                storedLightingScenario.lightingDataToIndex);
                        }

#if BAKERY_INCLUDED
                        #region Process Bakery Volumes

                        if ((storedLightingScenario.blendingModules & (1 << 4)) > 0)
                        {
                            if (switcherInstance.lightmapper == MagicLightmapSwitcher.Lightmapper.BakeryLightmapper)
                            {
                                BakeryVolume closestVolume =
                                    GetClosestBakeryVolume(resultDynamicAffectedObjects[i]);

                                if (closestVolume != null)
                                {
                                    ProcessBakeryVolumes(
                                        resultDynamicAffectedObjects[i],
                                        storedLightingScenario.blendableLightmaps,
                                        closestVolume,
                                        storedLightingScenario.lightingDataFromIndex,
                                        storedLightingScenario.lightingDataToIndex);
                                }
                            }
                        }

                        #endregion
#endif
                    }

                    if (!isDeferredMode)
                    {
                        resultDynamicAffectedObjects[i].SetShaderInt(_MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                        resultDynamicAffectedObjects[i].SetShaderFloat(_MLS_Reflections_Blend_Factor,
                            storedLightingScenario.reflectionsRangedBlendFactor);
                    }

#if UNITY_EDITOR
                    resultDynamicAffectedObjects[i].ApplyPropertyBlock();
#endif

                    resultDynamicAffectedObjects[i].lastFromIndex = storedLightingScenario.lightingDataFromIndex;
                }
            }

            #endregion

            #region Process Lightmaps
            
            if (switcherInstance.useTexture2DArrays)
            {
                #region Global Lightmaps Array

                if ((storedLightingScenario.blendingModules & (1 << 0)) > 0)
                {
                    if (switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario ||
                        !switcherInstance.lightmapArrayInitialized)
                    {
                        switcherInstance.lightmapArrayInitialized = false;

                        _lightmapWidth = storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.
                            lightmapsLight[0].width;
                        _lightmapHeight = storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.
                            lightmapsLight[0].height;
                        _lightmapCount = storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.
                            lightmapsLight.Length;
                        _lightmapDepth = storedLightingScenario.blendableLightmaps.Count * _lightmapCount;
                        
#if BAKERY_INCLUDED
                        if (storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM0 != null && 
                            storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM0.Length > 0)
                        {
                            _lightmapRNM_Width = storedLightingScenario.blendableLightmaps[0].lightingData
                                .sceneLightingData
                                .lightmapsBakeryRNM0[0].width;
                            _lightmapRNM_Height = storedLightingScenario.blendableLightmaps[0].lightingData
                                .sceneLightingData.lightmapsBakeryRNM0[0].height;
                            _lightmapRNM_Count = storedLightingScenario.blendableLightmaps[0].lightingData
                                .sceneLightingData
                                .lightmapsBakeryRNM0.Length;
                            _lightmapRNM_Depth = storedLightingScenario.blendableLightmaps.Count * _lightmapRNM_Count;
                        }
#endif

                        if (storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData
                            .lightmapsLight[0] != null)
                        {
                            _lightmapLightArray = new Texture2DArray(
                                _lightmapWidth, _lightmapHeight, _lightmapDepth,
                                storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData
                                    .lightmapsLight[0].graphicsFormat,
                                TextureCreationFlags.None);
                        }

                        if (storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData
                            .lightmapsDirectional[0] != null)
                        {
                            _lightmapDirArray = new Texture2DArray(
                                _lightmapWidth, _lightmapHeight, _lightmapDepth,
                                storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData
                                    .lightmapsDirectional[0]
                                    .graphicsFormat,
                                TextureCreationFlags.None);
                        }

                        if (storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData
                            .lightmapsShadowmask[0] != null)
                        {
                            _lightmapShadowMaskArray = new Texture2DArray(
                                _lightmapWidth, _lightmapHeight, _lightmapDepth,
                                storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData
                                    .lightmapsShadowmask[0]
                                    .graphicsFormat,
                                TextureCreationFlags.None);
                        }

                        for (int i = 0; i < storedLightingScenario.blendableLightmaps.Count; i++)
                        {
                            if (_lightmapLightArray != null)
                            {
                                for (int j = 0;
                                    j < storedLightingScenario.blendableLightmaps[i].lightingData.sceneLightingData
                                        .lightmapsLight.Length;
                                    j++)
                                {
                                    Texture2D source = storedLightingScenario.blendableLightmaps[i].lightingData
                                        .sceneLightingData
                                        .lightmapsLight[j];

                                    Graphics.CopyTexture(
                                        source, 0, 0,
                                        _lightmapLightArray, (i * _lightmapCount) + j, 0);
                                }
                            }

                            if (_lightmapDirArray != null)
                            {
                                for (int j = 0;
                                    j < storedLightingScenario.blendableLightmaps[i].lightingData.sceneLightingData
                                        .lightmapsDirectional.Length;
                                    j++)
                                {
                                    Texture2D source = storedLightingScenario.blendableLightmaps[i].lightingData
                                        .sceneLightingData
                                        .lightmapsDirectional[j];

                                    Graphics.CopyTexture(
                                        source, 0, 0,
                                        _lightmapDirArray, (i * _lightmapCount) + j, 0);
                                }
                            }

                            if (_lightmapShadowMaskArray != null)
                            {
                                for (int j = 0;
                                    j < storedLightingScenario.blendableLightmaps[i].lightingData.sceneLightingData
                                        .lightmapsShadowmask.Length;
                                    j++)
                                {
                                    Texture2D source = storedLightingScenario.blendableLightmaps[i].lightingData
                                        .sceneLightingData
                                        .lightmapsShadowmask[j];

                                    Graphics.CopyTexture(
                                        source, 0, 0,
                                        _lightmapShadowMaskArray, (i * _lightmapCount) + j, 0);
                                }
                            }
                        }
                        
#if BAKERY_INCLUDED
                        if (storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM0 != null && 
                            storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM0.Length > 0 && 
                            storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM0[0] != null)
                        {
                            _lightmapBakeryRNM0Array = new Texture2DArray(
                                _lightmapRNM_Width, _lightmapRNM_Height, _lightmapRNM_Depth,
                                storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData
                                    .lightmapsBakeryRNM0[0].graphicsFormat,
                                TextureCreationFlags.None);
                        }
                        
                        if (storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM1 != null && 
                            storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM1.Length > 0 &&
                            storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM1[0] != null)
                        {
                            _lightmapBakeryRNM1Array = new Texture2DArray(
                                _lightmapRNM_Width, _lightmapRNM_Height, _lightmapRNM_Depth,
                                storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData
                                    .lightmapsBakeryRNM1[0].graphicsFormat,
                                TextureCreationFlags.None);
                        }
                        
                        if (storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM2 != null && 
                            storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM2.Length > 0 &&
                            storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData.lightmapsBakeryRNM2[0] != null)
                        {
                            _lightmapBakeryRNM2Array = new Texture2DArray(
                                _lightmapRNM_Width, _lightmapRNM_Height, _lightmapRNM_Depth,
                                storedLightingScenario.blendableLightmaps[0].lightingData.sceneLightingData
                                    .lightmapsBakeryRNM2[0].graphicsFormat,
                                TextureCreationFlags.None);
                        }

                        for (int i = 0; i < storedLightingScenario.blendableLightmaps.Count; i++)
                        {
                            if (_lightmapBakeryRNM0Array != null)
                            {
                                for (int j = 0;
                                    j < _lightmapRNM_Count;
                                    j++)
                                {
                                    Texture2D source = storedLightingScenario.blendableLightmaps[i].lightingData
                                        .sceneLightingData
                                        .lightmapsBakeryRNM0[j];

                                    Graphics.CopyTexture(
                                        source, 0, 0,
                                        _lightmapBakeryRNM0Array, (i * _lightmapRNM_Count) + j, 0);
                                }
                            }
                            
                            if (_lightmapBakeryRNM1Array != null)
                            {
                                for (int j = 0;
                                    j < _lightmapRNM_Count;
                                    j++)
                                {
                                    Texture2D source = storedLightingScenario.blendableLightmaps[i].lightingData
                                        .sceneLightingData
                                        .lightmapsBakeryRNM1[j];

                                    Graphics.CopyTexture(
                                        source, 0, 0,
                                        _lightmapBakeryRNM1Array, (i * _lightmapRNM_Count) + j, 0);
                                }
                            }
                            
                            if (_lightmapBakeryRNM2Array != null)
                            {
                                for (int j = 0;
                                    j < _lightmapRNM_Count;
                                    j++)
                                {
                                    Texture2D source = storedLightingScenario.blendableLightmaps[i].lightingData
                                        .sceneLightingData
                                        .lightmapsBakeryRNM2[j];

                                    Graphics.CopyTexture(
                                        source, 0, 0,
                                        _lightmapBakeryRNM2Array, (i * _lightmapRNM_Count) + j, 0);
                                }
                            }
                        }
#endif

                        switcherInstance.lightmapArrayInitialized = true;
                    }
                    
#if BAKERY_INCLUDED
                    Shader.SetGlobalTexture(_MLS_BakeryRNM0_Array, _lightmapBakeryRNM0Array);
                    Shader.SetGlobalTexture(_MLS_BakeryRNM1_Array, _lightmapBakeryRNM1Array);
                    Shader.SetGlobalTexture(_MLS_BakeryRNM2_Array, _lightmapBakeryRNM2Array);
#endif

                    Shader.SetGlobalTexture(_MLS_Lightmap_Color_Array, _lightmapLightArray);
                    Shader.SetGlobalTexture(_MLS_Lightmap_Directional_Array, _lightmapDirArray);
                    Shader.SetGlobalTexture(_MLS_Lightmap_ShadowMask_Array, _lightmapShadowMaskArray);
                    Shader.SetGlobalFloat(_MLS_Lightmaps_Blend_Factor, storedLightingScenario.lightmapsRangedBlendFactor);
                }

                if ((storedLightingScenario.blendingModules & (1 << 1)) > 0)
                {
                    for (int i = 0; i < resultStaticAffectedObjects.Count; i++)
                    {
                        if (resultStaticAffectedObjects[i].meshRenderer != null ||
                            resultStaticAffectedObjects[i].terrain != null)
                        {
                            resultStaticAffectedObjects[i].InitPropertyBlock();
                        }
                        else
                        {
                            resultStaticAffectedObjects.RemoveAt(i);
                            return;
                        }

                        if (resultStaticAffectedObjects[i].terrain == null)
                        {
                            if (!resultStaticAffectedObjects[i].meshRenderer.isVisible)
                            {
                                continue;
                            }

                            if (storedLightingScenario.blendableLightmaps.Count < 3 ||
                                resultStaticAffectedObjects[i].lastFromIndex !=
                                storedLightingScenario.lightingDataFromIndex ||
                                switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario)
                            {
                                ProcessReflectionProbes(
                                    resultStaticAffectedObjects[i].meshRenderer.reflectionProbeUsage,
                                    resultStaticAffectedObjects[i],
                                    storedLightingScenario.blendableLightmaps,
                                    storedLightingScenario.lightingDataFromIndex,
                                    storedLightingScenario.lightingDataToIndex);
                            }
                        }
                        else
                        {
                            if (resultStaticAffectedObjects[i].terrain.isActiveAndEnabled)
                            {
                                if (!isDeferredMode)
                                {
                                    if (resultStaticAffectedObjects[i].lastFromIndex !=
                                        storedLightingScenario.lightingDataFromIndex ||
                                        switcherInstance.lastLightmapScenario !=
                                        switcherInstance.currentLightmapScenario)
                                    {
                                        ProcessReflectionProbes(
                                            resultStaticAffectedObjects[i].terrain.reflectionProbeUsage,
                                            resultStaticAffectedObjects[i],
                                            storedLightingScenario.blendableLightmaps,
                                            storedLightingScenario.lightingDataFromIndex,
                                            storedLightingScenario.lightingDataToIndex);
                                    }
                                }
                            }
                        }

                        resultStaticAffectedObjects[i].lastFromIndex = storedLightingScenario.lightingDataFromIndex;

                        if ((storedLightingScenario.blendingModules & (1 << 1)) > 0)
                        {
                            resultStaticAffectedObjects[i].SetShaderInt(_MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                            resultStaticAffectedObjects[i].SetShaderFloat(_MLS_Reflections_Blend_Factor,
                                storedLightingScenario.reflectionsRangedBlendFactor);
                        }
                        else
                        {
                            resultStaticAffectedObjects[i].SetShaderInt(_MLS_ENABLE_REFLECTIONS_BLENDING, 0);
                        }
#if UNITY_EDITOR
                        resultStaticAffectedObjects[i].ApplyPropertyBlock();
#endif
                    }

                    if (isDeferredMode)
                    {
                        if ((storedLightingScenario.blendingModules & (1 << 1)) > 0)
                        {
                            Shader.SetGlobalFloat(_MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                            Shader.SetGlobalFloat(_MLS_Reflections_Blend_Factor,
                                storedLightingScenario.reflectionsRangedBlendFactor);
                        }
                        else
                        {
                            Shader.SetGlobalFloat(_MLS_ENABLE_REFLECTIONS_BLENDING, 0);
                        }

                        if ((storedLightingScenario.blendingModules & (1 << 0)) > 0)
                        {
                            Shader.SetGlobalFloat(_MLS_ENABLE_LIGHTMAPS_BLENDING, 1);
                            Shader.SetGlobalFloat(_MLS_Lightmaps_Blend_Factor,
                                storedLightingScenario.lightmapsRangedBlendFactor);
                        }
                        else
                        {
                            Shader.SetGlobalFloat(_MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                        }
                    }
                }

                #endregion
            }
            else
            {
                switcherInstance.lightmapArrayInitialized = false;
                
                for (int i = 0; i < resultStaticAffectedObjects.Count; i++)
                {
                    if (resultStaticAffectedObjects[i].meshRenderer != null ||
                        resultStaticAffectedObjects[i].terrain != null)
                    {
                        resultStaticAffectedObjects[i].InitPropertyBlock();
                    }
                    else
                    {
                        resultStaticAffectedObjects.RemoveAt(i);
                        return;
                    }

                    if (resultStaticAffectedObjects[i].terrain == null)
                    {
                        if (!resultStaticAffectedObjects[i].meshRenderer.isVisible)
                        {
                            continue;
                        }

                        if (storedLightingScenario.blendableLightmaps.Count < 3 ||
                            resultStaticAffectedObjects[i].lastFromIndex !=
                            storedLightingScenario.lightingDataFromIndex ||
                            switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario)
                        {
                            if ((storedLightingScenario.blendingModules & (1 << 1)) > 0)
                            {
                                ProcessReflectionProbes(
                                    resultStaticAffectedObjects[i].meshRenderer.reflectionProbeUsage,
                                    resultStaticAffectedObjects[i],
                                    storedLightingScenario.blendableLightmaps,
                                    storedLightingScenario.lightingDataFromIndex,
                                    storedLightingScenario.lightingDataToIndex);
                            }

                            if ((storedLightingScenario.blendingModules & (1 << 0)) > 0)
                            {
                                StoredLightmapData.RendererData rendererData =
                                    storedLightingScenario
                                            .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                            .lightingData
                                            .rendererDataDeserialized[resultStaticAffectedObjects[i].objectId] as
                                        StoredLightmapData.RendererData;

                                if (rendererData == null)
                                {
                                    resultStaticAffectedObjects.RemoveAt(i);

                                    Debug.LogWarningFormat("<color=cyan>MLS:</color> " +
                                                           "The object \"" +
                                                           resultStaticAffectedObjects[i].meshRenderer.name + "\" " +
                                                           "is not present in the \"" +
                                                           storedLightingScenario
                                                               .blendableLightmaps[
                                                                   storedLightingScenario.lightingDataFromIndex]
                                                               .lightingData.name +
                                                           "\" lighting data, it is automatically isolated " +
                                                           "and will not participate in blending or switching lightmaps. \r\n" +
                                                           "Why did this happen? \r\n" +
                                                           "The object was active and marked as static during baking of the \"" +
                                                           storedLightingScenario
                                                               .blendableLightmaps[
                                                                   storedLightingScenario.lightingDataFromIndex]
                                                               .lightingData.name +
                                                           "\" preset, " +
                                                           "but was deactivated or marked as dynamic in the \"" +
                                                           storedLightingScenario
                                                               .blendableLightmaps[
                                                                   storedLightingScenario.lightingDataFromIndex]
                                                               .lightingData.name +
                                                           "\" preset. " +
                                                           "Object \"" +
                                                           resultStaticAffectedObjects[i].meshRenderer.name +
                                                           "\" might be getting deactivated by some other script.");
                                    return;
                                }

                                if (rendererData.lightmapIndex > -1)
                                {
                                    if (resultStaticAffectedObjects[i].lastFromIndex !=
                                        storedLightingScenario.lightingDataFromIndex ||
                                        switcherInstance.lastLightmapScenario !=
                                        switcherInstance.currentLightmapScenario)
                                    {
                                        resultStaticAffectedObjects[i].SetShaderTexture(
                                            _MLS_Lightmap_Color_Blend_From,
                                            storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsLight[rendererData.lightmapIndex]);

                                        resultStaticAffectedObjects[i].SetShaderTexture(
                                            _MLS_Lightmap_Color_Blend_To,
                                            storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsLight[rendererData.lightmapIndex]);

                                        if (storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsDirectional.Length > 0 &&
                                            storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsDirectional[rendererData.lightmapIndex] != null &&
                                            storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                .lightingData
                                                .sceneLightingData.lightmapsDirectional
                                                .Length > 0 &&
                                            storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsDirectional[rendererData.lightmapIndex] != null)
                                        {
                                            resultStaticAffectedObjects[i].SetShaderTexture(
                                                _MLS_Lightmap_Directional_Blend_From,
                                                storedLightingScenario
                                                    .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                    .lightingData
                                                    .sceneLightingData
                                                    .lightmapsDirectional[rendererData.lightmapIndex]);

                                            resultStaticAffectedObjects[i].SetShaderTexture(
                                                _MLS_Lightmap_Directional_Blend_To,
                                                storedLightingScenario
                                                    .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                    .lightingData
                                                    .sceneLightingData
                                                    .lightmapsDirectional[rendererData.lightmapIndex]);
                                        }

                                        if (storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsShadowmask.Length > 0 &&
                                            storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsShadowmask[rendererData.lightmapIndex] != null &&
                                            storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                .lightingData
                                                .sceneLightingData.lightmapsShadowmask
                                                .Length > 0 &&
                                            storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsShadowmask[rendererData.lightmapIndex] != null)
                                        {
                                            resultStaticAffectedObjects[i].SetShaderTexture(
                                                _MLS_Lightmap_ShadowMask_Blend_From,
                                                storedLightingScenario
                                                    .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                    .lightingData
                                                    .sceneLightingData
                                                    .lightmapsShadowmask[rendererData.lightmapIndex]);

                                            resultStaticAffectedObjects[i].SetShaderTexture(
                                                _MLS_Lightmap_ShadowMask_Blend_To,
                                                storedLightingScenario
                                                    .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                    .lightingData
                                                    .sceneLightingData
                                                    .lightmapsShadowmask[rendererData.lightmapIndex]);
                                        }

#if BAKERY_INCLUDED
                                        if (switcherInstance.lightmapper ==
                                            MagicLightmapSwitcher.Lightmapper.BakeryLightmapper)
                                        {
                                            if (storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsBakeryRNM0.Length > 0)
                                            {
                                                if (storedLightingScenario
                                                    .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                    .lightingData
                                                    .sceneLightingData
                                                    .lightmapsBakeryRNM0[rendererData.lightmapIndex] != null)
                                                {
                                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                                        _MLS_BakeryRNM0_From,
                                                        storedLightingScenario
                                                            .blendableLightmaps[
                                                                storedLightingScenario.lightingDataFromIndex]
                                                            .lightingData.sceneLightingData
                                                            .lightmapsBakeryRNM0[rendererData.lightmapIndex]);
                                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                                        _MLS_BakeryRNM0_To,
                                                        storedLightingScenario
                                                            .blendableLightmaps[
                                                                storedLightingScenario.lightingDataToIndex]
                                                            .lightingData
                                                            .sceneLightingData
                                                            .lightmapsBakeryRNM0[rendererData.lightmapIndex]);
                                                }
                                            }

                                            if (storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsBakeryRNM1.Length > 0)
                                            {
                                                resultStaticAffectedObjects[i].SetShaderTexture(
                                                    _MLS_BakeryRNM1_From,
                                                    storedLightingScenario
                                                        .blendableLightmaps[
                                                            storedLightingScenario.lightingDataFromIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsBakeryRNM1[rendererData.lightmapIndex]);
                                                resultStaticAffectedObjects[i].SetShaderTexture(
                                                    _MLS_BakeryRNM1_To,
                                                    storedLightingScenario
                                                        .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsBakeryRNM1[rendererData.lightmapIndex]);
                                            }

                                            if (storedLightingScenario
                                                .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                .lightingData
                                                .sceneLightingData
                                                .lightmapsBakeryRNM2.Length > 0)
                                            {
                                                resultStaticAffectedObjects[i].SetShaderTexture(
                                                    _MLS_BakeryRNM2_From,
                                                    storedLightingScenario
                                                        .blendableLightmaps[
                                                            storedLightingScenario.lightingDataFromIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsBakeryRNM2[rendererData.lightmapIndex]);
                                                resultStaticAffectedObjects[i].SetShaderTexture(
                                                    _MLS_BakeryRNM2_To,
                                                    storedLightingScenario
                                                        .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsBakeryRNM2[rendererData.lightmapIndex]);
                                            }
                                        }
#endif
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (resultStaticAffectedObjects[i].terrain.isActiveAndEnabled)
                        {
                            if (!isDeferredMode)
                            {
                                if (resultStaticAffectedObjects[i].lastFromIndex !=
                                    storedLightingScenario.lightingDataFromIndex ||
                                    switcherInstance.lastLightmapScenario !=
                                    switcherInstance.currentLightmapScenario)
                                {
                                    if ((storedLightingScenario.blendingModules & (1 << 1)) > 0)
                                    {
                                        ProcessReflectionProbes(
                                            resultStaticAffectedObjects[i].terrain.reflectionProbeUsage,
                                            resultStaticAffectedObjects[i],
                                            storedLightingScenario.blendableLightmaps,
                                            storedLightingScenario.lightingDataFromIndex,
                                            storedLightingScenario.lightingDataToIndex);
                                    }

                                    if ((storedLightingScenario.blendingModules & (1 << 0)) > 0)
                                    {
                                        StoredLightmapData.TerrainData terrainData =
                                            storedLightingScenario
                                                    .blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                                                    .lightingData
                                                    .terrainDataDeserialized[resultStaticAffectedObjects[i].objectId] as
                                                StoredLightmapData.TerrainData;

                                        if (terrainData == null)
                                        {
                                            resultStaticAffectedObjects.RemoveAt(i);
                                            Debug.LogWarningFormat("<color=cyan>MLS:</color> " +
                                                                   "The object \"" +
                                                                   resultStaticAffectedObjects[i].meshRenderer.name +
                                                                   "\" " +
                                                                   "is not present in the \"" +
                                                                   storedLightingScenario
                                                                       .blendableLightmaps[
                                                                           storedLightingScenario.lightingDataFromIndex]
                                                                       .lightingData.name +
                                                                   "\" lighting data, it is automatically isolated " +
                                                                   "and will not participate in blending or switching lightmaps. \r\n" +
                                                                   "Why did this happen? \r\n" +
                                                                   "The object was active and marked as static during baking of the \"" +
                                                                   storedLightingScenario
                                                                       .blendableLightmaps[
                                                                           storedLightingScenario.lightingDataFromIndex]
                                                                       .lightingData.name +
                                                                   "\" preset, " +
                                                                   "but was deactivated or marked as dynamic in the \"" +
                                                                   storedLightingScenario
                                                                       .blendableLightmaps[
                                                                           storedLightingScenario.lightingDataFromIndex]
                                                                       .lightingData.name +
                                                                   "\" preset. " +
                                                                   "Object \"" +
                                                                   resultStaticAffectedObjects[i].meshRenderer.name +
                                                                   "\" might be getting deactivated by some other script.");
                                            return;
                                        }

                                        if (terrainData.lightmapIndex > -1)
                                        {
                                            if (resultStaticAffectedObjects[i].lastFromIndex !=
                                                storedLightingScenario.lightingDataFromIndex ||
                                                switcherInstance.lastLightmapScenario !=
                                                switcherInstance.currentLightmapScenario)
                                            {
                                                resultStaticAffectedObjects[i].SetShaderTexture(
                                                    _MLS_Lightmap_Color_Blend_From,
                                                    storedLightingScenario
                                                        .blendableLightmaps[
                                                            storedLightingScenario.lightingDataFromIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsLight[terrainData.lightmapIndex]);
                                                resultStaticAffectedObjects[i].SetShaderTexture(
                                                    _MLS_Lightmap_Color_Blend_To,
                                                    storedLightingScenario
                                                        .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsLight[terrainData.lightmapIndex]);

                                                if (storedLightingScenario
                                                        .blendableLightmaps[
                                                            storedLightingScenario.lightingDataFromIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsDirectional.Length > 0 &&
                                                    storedLightingScenario
                                                        .blendableLightmaps[
                                                            storedLightingScenario.lightingDataFromIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsDirectional[terrainData.lightmapIndex] != null &&
                                                    storedLightingScenario
                                                        .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsDirectional
                                                        .Length > 0 &&
                                                    storedLightingScenario
                                                        .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsDirectional[terrainData.lightmapIndex] != null)
                                                {
                                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                                        _MLS_Lightmap_Directional_Blend_From,
                                                        storedLightingScenario
                                                            .blendableLightmaps[
                                                                storedLightingScenario.lightingDataFromIndex]
                                                            .lightingData
                                                            .sceneLightingData
                                                            .lightmapsDirectional[terrainData.lightmapIndex]);
                                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                                        _MLS_Lightmap_Directional_Blend_To,
                                                        storedLightingScenario
                                                            .blendableLightmaps[
                                                                storedLightingScenario.lightingDataToIndex]
                                                            .lightingData
                                                            .sceneLightingData
                                                            .lightmapsDirectional[terrainData.lightmapIndex]);
                                                }

                                                if (storedLightingScenario
                                                        .blendableLightmaps[
                                                            storedLightingScenario.lightingDataFromIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsShadowmask.Length > 0 &&
                                                    storedLightingScenario
                                                        .blendableLightmaps[
                                                            storedLightingScenario.lightingDataFromIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsShadowmask[terrainData.lightmapIndex] != null &&
                                                    storedLightingScenario
                                                        .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsShadowmask
                                                        .Length > 0 &&
                                                    storedLightingScenario
                                                        .blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                                                        .lightingData
                                                        .sceneLightingData
                                                        .lightmapsShadowmask[terrainData.lightmapIndex] != null)
                                                {
                                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                                        _MLS_Lightmap_ShadowMask_Blend_From,
                                                        storedLightingScenario
                                                            .blendableLightmaps[
                                                                storedLightingScenario.lightingDataFromIndex]
                                                            .lightingData
                                                            .sceneLightingData
                                                            .lightmapsShadowmask[terrainData.lightmapIndex]);
                                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                                        _MLS_Lightmap_ShadowMask_Blend_To,
                                                        storedLightingScenario
                                                            .blendableLightmaps[
                                                                storedLightingScenario.lightingDataToIndex]
                                                            .lightingData
                                                            .sceneLightingData
                                                            .lightmapsShadowmask[terrainData.lightmapIndex]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    resultStaticAffectedObjects[i].lastFromIndex = storedLightingScenario.lightingDataFromIndex;

                    if ((storedLightingScenario.blendingModules & (1 << 1)) > 0)
                    {
                        resultStaticAffectedObjects[i].SetShaderInt(_MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                        resultStaticAffectedObjects[i].SetShaderFloat(_MLS_Reflections_Blend_Factor,
                            storedLightingScenario.reflectionsRangedBlendFactor);
                    }
                    else
                    {
                        resultStaticAffectedObjects[i].SetShaderInt(_MLS_ENABLE_REFLECTIONS_BLENDING, 0);
                    }

                    if ((storedLightingScenario.blendingModules & (1 << 0)) > 0)
                    {
                        resultStaticAffectedObjects[i].SetShaderInt(_MLS_ENABLE_LIGHTMAPS_BLENDING, 1);
                        resultStaticAffectedObjects[i].SetShaderFloat(_MLS_Lightmaps_Blend_Factor,
                            storedLightingScenario.lightmapsRangedBlendFactor);
                    }
                    else
                    {
                        resultStaticAffectedObjects[i].SetShaderInt(_MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                    }
#if UNITY_EDITOR
                    resultStaticAffectedObjects[i].ApplyPropertyBlock();
#endif
                }

                if (isDeferredMode)
                {
                    if ((storedLightingScenario.blendingModules & (1 << 1)) > 0)
                    {
                        Shader.SetGlobalFloat(_MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                        Shader.SetGlobalFloat(_MLS_Reflections_Blend_Factor, storedLightingScenario.reflectionsRangedBlendFactor);
                    }
                    else
                    {
                        Shader.SetGlobalFloat(_MLS_ENABLE_REFLECTIONS_BLENDING, 0);
                    }

                    if ((storedLightingScenario.blendingModules & (1 << 0)) > 0)
                    {
                        Shader.SetGlobalFloat(_MLS_ENABLE_LIGHTMAPS_BLENDING, 1);
                        Shader.SetGlobalFloat(_MLS_Lightmaps_Blend_Factor, storedLightingScenario.lightmapsRangedBlendFactor);
                    }
                    else
                    {
                        Shader.SetGlobalFloat(_MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                    }
                }
            }
            
            #endregion

            #region Process Skybox

            if ((storedLightingScenario.blendingModules & (1 << 3)) > 0)
            {
                Shader.SetGlobalFloat(
                    _MLS_Sky_Blend_From_Exposure,
                    QualitySettings.activeColorSpace ==
                    ColorSpace.Gamma
                        ? storedLightingScenario.blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                            .lightingData.sceneLightingData.skyboxSettings.exposure
                        : storedLightingScenario.blendableLightmaps[storedLightingScenario.lightingDataFromIndex]
                            .lightingData.sceneLightingData.skyboxSettings.exposure / 4);
                Shader.SetGlobalFloat(
                    _MLS_Sky_Blend_To_Exposure,
                    QualitySettings.activeColorSpace ==
                    ColorSpace.Gamma
                        ? storedLightingScenario.blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                            .lightingData
                            .sceneLightingData.skyboxSettings.exposure
                        : storedLightingScenario.blendableLightmaps[storedLightingScenario.lightingDataToIndex]
                            .lightingData
                            .sceneLightingData.skyboxSettings.exposure / 4);

                Shader.SetGlobalColor(_MLS_Sky_Blend_From_Tint,
                    storedLightingScenario.blendableLightmaps[storedLightingScenario.lightingDataFromIndex].lightingData
                        .sceneLightingData.skyboxSettings.tintColor);
                Shader.SetGlobalColor(_MLS_Sky_Blend_To_Tint,
                    storedLightingScenario.blendableLightmaps[storedLightingScenario.lightingDataToIndex].lightingData
                        .sceneLightingData.skyboxSettings.tintColor);
                Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_From,
                    storedLightingScenario.blendableLightmaps[storedLightingScenario.lightingDataFromIndex].lightingData
                        .sceneLightingData.skyboxSettings.skyboxTexture);
                Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_To,
                    storedLightingScenario.blendableLightmaps[storedLightingScenario.lightingDataToIndex].lightingData
                        .sceneLightingData.skyboxSettings.skyboxTexture);
                Shader.SetGlobalFloat(_MLS_Sky_Cubemap_Blend_Factor,
                    storedLightingScenario.reflectionsRangedBlendFactor);
            }

            #endregion

            Shader.SetGlobalVector(_MLS_CURRENT_LIGHTMAP_PAIR,
                new Vector4(storedLightingScenario.lightingDataFromIndex, storedLightingScenario.lightingDataToIndex,
                    _lightmapCount));

            Shader.SetGlobalInt(_MLS_ENABLE_LIGHTMAPS_BLENDING,
                (storedLightingScenario.blendingModules & (1 << 0)) > 0 ? 1 : 0);
            Shader.SetGlobalInt(_MLS_ENABLE_REFLECTIONS_BLENDING,
                (storedLightingScenario.blendingModules & (1 << 1)) > 0 ? 1 : 0);
            Shader.SetGlobalInt(_MLS_ENABLE_SKY_CUBEMAPS_BLENDING,
                (storedLightingScenario.blendingModules & (1 << 2)) > 0 ? 1 : 0);
        }

        private static void BlendCustomData(float localBlendFactor, float globalBlendFactor, float reflectionsBlendFactor, float lightmapsBlendFactor, StoredLightingScenario storedLightmapScenario, int fromIndex, int toIndex)
        {
            if (storedLightmapScenario.collectedCustomBlendableDatas.Count > 0)
            {
                if (storedLightmapScenario.collectedCustomBlendableDatas.Find(item => item.sourceScript == null) != null)
                {
                    storedLightmapScenario.SynchronizeCustomBlendableData();
                }
                else
                {
                    for (int i = 0; i < storedLightmapScenario.collectedCustomBlendableDatas.Count; i++)
                    {
                        if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableFloatFieldsDatas.Count > 0)
                        {
                            if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableFloatFieldsDatas.Find(item => item.sourceField == null) != null)
                            {
                                storedLightmapScenario.SynchronizeCustomBlendableData();
                            }
                        }

                        if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableColorFieldsDatas.Count > 0)
                        {
                            if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableColorFieldsDatas.Find(item => item.sourceField == null) != null)
                            {
                                storedLightmapScenario.SynchronizeCustomBlendableData();
                            }
                        }

                        if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas.Count > 0)
                        {
                            if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas.Find(item => item.sourceField == null) != null)
                            {
                                storedLightmapScenario.SynchronizeCustomBlendableData();
                            }
                        }
                    }
                }

                storedLightmapScenario.UpdateCustomBlendableData(localBlendFactor, globalBlendFactor, reflectionsBlendFactor, lightmapsBlendFactor, fromIndex, toIndex, 0);
            }
        }

        private static void BlendLightSourcesData(float localBlendFactor, float blendFactor, List<StoredLightingScenario.LightmapData> storedLightmapDatas, int fromIndex, int toIndex)
        {
            for (int i = 0; i < resultAffectedLights.Count; i++)
            {
                if (!resultAffectedLights[i].enabled)
                {
                    continue;
                }

                StoredLightmapData.LightSourceData lightFrom = 
                    storedLightmapDatas[fromIndex].lightingData.lightSourceDataDeserialized[resultAffectedLights[i].lightGUID] as StoredLightmapData.LightSourceData;
                StoredLightmapData.LightSourceData lightTo = 
                    storedLightmapDatas[toIndex].lightingData.lightSourceDataDeserialized[resultAffectedLights[i].lightGUID] as StoredLightmapData.LightSourceData;

                if (lightFrom == null || lightTo == null)
                {
                    continue;
                }

                resultAffectedLights[i].sourceLight.transform.position = Vector3.Lerp(
                    lightFrom.position,
                    lightTo.position,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.transform.rotation = Quaternion.Lerp(
                    lightFrom.rotation,
                    lightTo.rotation,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.intensity = Mathf.Lerp(
                    lightFrom.intensity,
                    lightTo.intensity,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.color = Color.Lerp(
                    lightFrom.color,
                    lightTo.color,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.colorTemperature = Mathf.Lerp(
                    lightFrom.temperature,
                    lightTo.temperature,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.range = Mathf.Lerp(
                    lightFrom.range,
                    lightTo.range,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.spotAngle = Mathf.Lerp(
                    lightFrom.spotAngle,
                    lightTo.spotAngle,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.shadows = 
                    localBlendFactor > resultAffectedLights[i].shadowTypeSwitchValue ? (LightShadows) lightTo.shadowType : (LightShadows) lightFrom.shadowType;
                resultAffectedLights[i].sourceLight.shadowStrength = Mathf.Lerp(
                    lightFrom.shadowStrength,
                    lightTo.shadowStrength,
                    localBlendFactor);
            }
        }

        private static void BlendGameObjectsData(float localBlendFactor, float blendFactor, List<StoredLightingScenario.LightmapData> storedLightmapDatas, int fromIndex, int toIndex)
        {
            for (int i = 0; i < resultStaticAffectedObjects.Count; i++)
            {
                if (resultStaticAffectedObjects[i].terrain != null)
                {
                    return;
                }
                
                if (!resultStaticAffectedObjects[i].meshRenderer.isVisible)
                {
                    continue;
                }

                StoredLightmapData.RendererData rendererDataFrom =
                    storedLightmapDatas[fromIndex].lightingData.rendererDataDeserialized[resultStaticAffectedObjects[i].objectId] as StoredLightmapData.RendererData;
                StoredLightmapData.RendererData rendererDataTo = 
                    storedLightmapDatas[toIndex].lightingData.rendererDataDeserialized[resultStaticAffectedObjects[i].objectId] as StoredLightmapData.RendererData;

                if (rendererDataFrom == null || rendererDataTo == null)
                {
                    resultStaticAffectedObjects.RemoveAt(i);
                    Debug.LogWarningFormat("<color=cyan>MLS:</color> " +
                        "The object \"" + resultStaticAffectedObjects[i].meshRenderer.name + "\" " +
                        "is not present in the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" lighting data, it is automatically isolated " +
                        "and will not participate in blending or switching lightmaps. \r\n" +
                        "Why did this happen? \r\n" +
                        "The object was active and marked as static during baking of the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" preset, " +
                        "but was deactivated or marked as dynamic in the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" preset. " +
                        "Object \"" + resultStaticAffectedObjects[i].meshRenderer.name + "\" might be getting deactivated by some other script.");
                    return;
                }

                if (rendererDataFrom.position != rendererDataTo.position)
                {
                    resultStaticAffectedObjects[i].meshRenderer.gameObject.transform.position = Vector3.Lerp(
                        rendererDataFrom.position,
                        rendererDataTo.position,
                        localBlendFactor);
                }

                if (rendererDataFrom.rotation != rendererDataTo.rotation)
                {
                    resultStaticAffectedObjects[i].meshRenderer.gameObject.transform.rotation = Quaternion.Lerp(
                        rendererDataFrom.rotation,
                        rendererDataTo.rotation,
                        localBlendFactor);
                }
            }
        }

        private static void BlendCommonLightingSettings(float blendFactor, List<StoredLightingScenario.LightmapData> storedLightmapDatas, int fromIndex, int toIndex)
        {
            RenderSettings.fogColor = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.fogSettings.fogColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.fogSettings.fogColor,
                blendFactor);
            RenderSettings.fogDensity = Mathf.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.fogSettings.fogDensity,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.fogSettings.fogDensity,
                blendFactor);
            RenderSettings.ambientMode = storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.source;
            RenderSettings.ambientIntensity = Mathf.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.intensityMultiplier,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.intensityMultiplier,
                blendFactor);
            RenderSettings.ambientLight = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.ambientColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.ambientColor,
                blendFactor);
            RenderSettings.ambientSkyColor = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.skyColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.skyColor,
                blendFactor);
            RenderSettings.ambientEquatorColor = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.equatorColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.equatorColor,
                blendFactor);
            RenderSettings.ambientGroundColor = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.groundColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.groundColor,
                blendFactor);
        }

        private static void BlendLightProbesThread(object data)
        {
            BlendProbesThreadData threadData = data as BlendProbesThreadData;

            int counter = 0;
            
            if (threadData != null)
            {
                float[] exit = new float[threadData.blendFromArray.Length];
                float[][] combinedTemp = new float[Mathf.RoundToInt(exit.Length / 27)][];

                Parallel.For(0, threadData.blendFromArray.Length, (i =>
                {
                    exit[i] = Mathf.Lerp(
                        threadData.blendFromArray[i],
                        threadData.blendToArray[i],
                        threadData.blendFactor);
                }));

                for (int i = 0; i < exit.Length; i += 27)
                {
                    float[] temp = new float[27];
                    Array.Copy(exit, i, temp, 0, 27);
                    combinedTemp[counter] = temp;
                    counter++;
                }

                Parallel.For(0, combinedTemp.Length, (i, state) =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            threadData.sphericalHarmonicsArray[i][j, k] = combinedTemp[i][j * 9 + k];
                        }
                    }
                });
            }

            blendProbesThreadsQueue.Enqueue(threadData);
            //System.GC.Collect(1, GCCollectionMode.Forced);
        }

        private static void LightProbesReplacingThread(object data)
        {
            ProbesReplacingThreadData threadData = data as ProbesReplacingThreadData;

            if (threadData != null)
            {
                SphericalHarmonicsL2[] finalArray = threadData.sphericalHarmonicsArray;

                Array.Copy(
                    threadData.lastProbesData.sphericalHarmonicsArray,
                    0,
                    finalArray,
                    threadData.lastProbesData.lightProbesArrayPosition,
                    threadData.lastProbesData.sphericalHarmonicsArray.Length);
            }

            probesReplacingThreadsQueue.Enqueue(threadData);
            //System.GC.Collect(1, GCCollectionMode.Forced);
        }

        private class ProbesReplacingThreadsPool
        {
            private List<ProbesReplacingThreadData> _objects;

            public ProbesReplacingThreadsPool(ProbesReplacingThreadData newObj)
            {
                _objects = new List<ProbesReplacingThreadData>();
            }

            public bool TryGet(out ProbesReplacingThreadData item)
            {
                if (_objects.Count > 0)
                {
                    int counter = -1;
                    
                    for (int i = 0; i < _objects.Count; i++)
                    {
                        if (!_objects[i].isBusy)
                        {
                            counter = i;
                        }
                    }

                    if (counter != -1)
                    {
                        item = _objects[counter];
                        return true;
                    }
                    else
                    {
                        _objects.Add(item = new ProbesReplacingThreadData());
                        return false;
                    }
                }
                else
                {
                    _objects.Add(item = new ProbesReplacingThreadData());
                    return false;
                }
            }
        }
        
        private class BlendProbesThreadsPool
        {
            private List<BlendProbesThreadData> _objects;

            public BlendProbesThreadsPool(BlendProbesThreadData newObj)
            {
                _objects = new List<BlendProbesThreadData>();
            }

            public bool TryGet(out BlendProbesThreadData item)
            {
                if (_objects.Count > 0)
                {
                    int counter = -1;
                    
                    for (int i = 0; i < _objects.Count; i++)
                    {
                        if (!_objects[i].isBusy)
                        {
                            counter = i;
                        }
                    }

                    if (counter != -1)
                    {
                        item = _objects[counter];
                        return true;
                    }
                    else
                    {
                        _objects.Add(item = new BlendProbesThreadData());
                        return false;
                    }
                }
                else
                {
                    _objects.Add(item = new BlendProbesThreadData());
                    return false;
                }
            }
        }

        private static ProbesReplacingThreadsPool _probesReplacingThreadDataPool = null;
        private static BlendProbesThreadsPool _blendProbesThreadDataPool = null;

        private static void BlendLightProbesData(MagicLightmapSwitcher switcherInstance,
            StoredLightingScenario storedLightmapScenario, int from, int to, float blendFactor)
        {
            LightProbes sceneProbesObject = LightmapSettings.lightProbes;
            
            if (_blendProbesThreadDataPool == null)
            {
                _blendProbesThreadDataPool = new BlendProbesThreadsPool(new BlendProbesThreadData());
            }

            if (_probesReplacingThreadDataPool == null)
            {
                _probesReplacingThreadDataPool = new ProbesReplacingThreadsPool(new ProbesReplacingThreadData());
            }

            if (sceneProbesObject == null)
            {
                return;
            }

            if (switcherInstance.stopProbesBlending)
            {
                return;
            }

            if (blendProbesThreadsQueue.Count > 3 ||
                probesReplacingThreadsQueue.Count > 3 ||
                sceneProbesObject == null)
            {
                blendProbesThreadsQueue.Clear();
                probesReplacingThreadsQueue.Clear();
                lightProbesArrayProcessing = false;
                return;
            }
  
            if (probesReplacingThreadsQueue.Count > 0)
            {
                lastReplacedProbesData = probesReplacingThreadsQueue.Dequeue();

                if (lastReplacedProbesData != null && lastReplacedProbesData.sphericalHarmonicsArray != null)
                {
                    if (LightmapSettings.lightProbes.bakedProbes.Length ==
                        lastReplacedProbesData.sphericalHarmonicsArray.Length)
                    {
                        LightmapSettings.lightProbes.bakedProbes = lastReplacedProbesData.sphericalHarmonicsArray;
                        lastReplacedProbesData.isBusy = false;
                    }
                }
            }

            if (!lightProbesArrayProcessing)
            {
                lightProbesArrayProcessing = true;

                if (blendProbesThreadsQueue.Count > 0)
                {
                    BlendProbesThreadData lastProbesData = blendProbesThreadsQueue.Dequeue();

                    if (lastProbesData != null)
                    {
                        lastProbesData.isBusy = false;
                            
                        //_probesReplacingThreadDataPool.TryGet(out var probesReplacingThreadData);

                        ProbesReplacingThreadData probesReplacingThreadData = new ProbesReplacingThreadData();
                        
                        probesReplacingThreadData.isBusy = true;
                        probesReplacingThreadData.switcherInstance = switcherInstance;
                        probesReplacingThreadData.lastProbesData = lastProbesData;
                        probesReplacingThreadData.sphericalHarmonicsArray = LightmapSettings.lightProbes.bakedProbes;
                        
                        ThreadPool.QueueUserWorkItem(LightProbesReplacingThread, probesReplacingThreadData);
                    }
                }

                lightProbesArrayProcessing = false;
            }

            //_blendProbesThreadDataPool.TryGet(out var blendProbesThreadData);

            BlendProbesThreadData blendProbesThreadData = new BlendProbesThreadData();

            blendProbesThreadData.isBusy = true;
            blendProbesThreadData.switcherInstance = switcherInstance;
            blendProbesThreadData.lightProbesArrayPosition = storedLightmapScenario.lightProbesArrayPosition;
            blendProbesThreadData.blendFromArray = storedLightmapScenario.blendableLightmaps[from].lightingData
                .sceneLightingData.lightProbes1D;
            blendProbesThreadData.blendToArray = storedLightmapScenario.blendableLightmaps[to].lightingData
                .sceneLightingData.lightProbes1D;
            blendProbesThreadData.sphericalHarmonicsArray = new SphericalHarmonicsL2[storedLightmapScenario
                .blendableLightmaps[to].lightingData.sceneLightingData.lightProbes.Length];
            blendProbesThreadData.blendFactor = blendFactor;

            ThreadPool.QueueUserWorkItem(BlendLightProbesThread, blendProbesThreadData);

            if (switcherInstance.lightingDataSwitching)
            {
                switcherInstance.lightingDataSwitching = false;
                switcherInstance.StartCoroutine(_DoLightprobesBlendQueue(switcherInstance));

                switcherInstance.lightprobesBlendingStarted = false;
            }
        }

        private static IEnumerator _DoLightprobesBlendQueue(MagicLightmapSwitcher switcherInstance)
        {
            while (blendProbesThreadsQueue.Count == 0)
            {
                yield return null;
            }

            BlendProbesThreadData lastProbesData = blendProbesThreadsQueue.Dequeue();

            if (lastProbesData != null)
            {
                ProbesReplacingThreadData probesReplacingThreadData = new ProbesReplacingThreadData();

                probesReplacingThreadData.switcherInstance = switcherInstance;
                probesReplacingThreadData.lastProbesData = lastProbesData;
                probesReplacingThreadData.sphericalHarmonicsArray = LightmapSettings.lightProbes.bakedProbes;

                ThreadPool.QueueUserWorkItem(LightProbesReplacingThread, probesReplacingThreadData);
            }

            while (probesReplacingThreadsQueue.Count == 0)
            {
                yield return null;
            }

            while (probesReplacingThreadsQueue.Count > 0)
            {
                lastReplacedProbesData = probesReplacingThreadsQueue.Dequeue();

                if (lastReplacedProbesData != null && lastReplacedProbesData.sphericalHarmonicsArray != null)
                {
                    if (LightmapSettings.lightProbes.bakedProbes.Length == lastReplacedProbesData.sphericalHarmonicsArray.Length)
                    {
                        LightmapSettings.lightProbes.bakedProbes = lastReplacedProbesData.sphericalHarmonicsArray;
                    }
                }

                yield return null;
            }
        }
    }
}
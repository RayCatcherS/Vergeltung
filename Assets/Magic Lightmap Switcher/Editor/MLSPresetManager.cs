using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MagicLightmapSwitcher
{
    static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : System.ICloneable
        {
            return listToClone.Select(item => (T) item.Clone()).ToList();
        }
    }

    public class MLSPresetManager : EditorWindow
    {

#if BAKERY_INCLUDED
        public enum BakeryLightType
        {
            Light,
            Skylight,
            CustomMesh
        }
#endif

        public static MagicLightmapSwitcher magicLightmapSwitcher;
        public static MLSPresetManager presetsManagerWindow;
        public static bool initialized;
        public static string targetScene;
        public static int setActivePreset;
        public static string presetName;
        public static bool directEditing;

        private static Vector2 scrollPos;

        public static void Init(bool forceActiveScene = false)
        {
            presetsManagerWindow = (MLSPresetManager) GetWindow(typeof(MLSPresetManager), false, "Preset Manager");
            presetsManagerWindow.name = "Preset Manager";
            presetsManagerWindow.minSize = new Vector2(500 * EditorGUIUtility.pixelsPerPoint, 150 * EditorGUIUtility.pixelsPerPoint);
            presetsManagerWindow.maxSize = new Vector2(500 * EditorGUIUtility.pixelsPerPoint, 1000 * EditorGUIUtility.pixelsPerPoint);
            presetsManagerWindow.Show();

            if (forceActiveScene)
            {
                magicLightmapSwitcher = RuntimeAPI.GetSwitcherInstanceStatic(EditorSceneManager.GetActiveScene().name);
            }
            else
            {
                magicLightmapSwitcher = RuntimeAPI.GetSwitcherInstanceStatic(targetScene);
            }

            SetMLSDataControl(true);
            MLSLightmapDataStoring.presetManager = presetsManagerWindow;

            initialized = true;
        }

        private static void SetMLSDataControl(bool value)
        {
            MLSLight[] mlsLights = FindObjectsOfType<MLSLight>();

            for (int i = 0; i < mlsLights.Length; i++)
            {
                mlsLights[i].presetManagerActive = value;
            }
        }

        private void OnDestroy()
        {
            SetMLSDataControl(false);
        }

        public static void CreateNewPreset(MagicLightmapSwitcher magicLightmapSwitcher, MagicLightmapSwitcher.SceneLightingPreset copyFrom = null)
        {
            MagicLightmapSwitcher.SceneLightingPreset lightingPreset = new MagicLightmapSwitcher.SceneLightingPreset();

            lightingPreset.name = "New Preset" + "_" + magicLightmapSwitcher.lightingPresets.Count.ToString();

            if (magicLightmapSwitcher.lightingPresets.Count > 0)
            {
                if (copyFrom != null)
                {
                    lightingPreset.lightSourceSettings = copyFrom.lightSourceSettings.Select(element => new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings() 
                    {
                        mlsLightUID = element.mlsLightUID,
                        mlsLight = element.mlsLight,
                        light = element.light,
                        lightType = element.lightType,
                        position = element.position,
                        rotation = element.rotation,
                        color = element.color,
                        colorTemperature = element.colorTemperature,
                        intensity = element.intensity,
                        indirectMultiplier = element.indirectMultiplier,
                        range = element.range,
                        spotOuterAngle = element.spotOuterAngle,
                        spotInnerAngle = element.spotInnerAngle,
                        areaWidth = element.areaWidth,
                        areaHeight = element.areaHeight,
                        shadowsType = element.shadowsType,
                        bakedShadowsRadius = element.bakedShadowsRadius,
                        globalFoldoutEnabled = element.globalFoldoutEnabled,
                        transformFoldoutEnabled = element.transformFoldoutEnabled,
                        settingsFoldoutEnabled = element.settingsFoldoutEnabled
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                        ,
                        additionalLightData = element.additionalLightData
                        #endif
                        #if BAKERY_INCLUDED
                        ,
                        bakeryDirectLightsSettings = element.bakeryDirectLightsSettings,
                        bakeryPointLightsSettings = element.bakeryPointLightsSettings, 
                        bakeryLightMeshesSettings = element.bakeryLightMeshesSettings
#endif
                    }).ToList();

                    List<MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings> clonedCustomBlendablesSettings = new List<MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings>(copyFrom.customBlendablesSettings.Count);

                    copyFrom.customBlendablesSettings.ForEach((item) =>
                        {
                        clonedCustomBlendablesSettings.Add(new MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings(item));
                    });

                    lightingPreset.customBlendablesSettings = clonedCustomBlendablesSettings;

                    lightingPreset.gameObjectsSettings = copyFrom.gameObjectsSettings.Select(element => new MagicLightmapSwitcher.SceneLightingPreset.GameObjectSettings()
                    {
                        gameObject = element.gameObject,
                        enabled = element.enabled,
                        instanceId = element.instanceId,
                        rotation = element.rotation,
                        position = element.position,
                        transformFoldoutEnabled = element.transformFoldoutEnabled,
                        globalFoldoutEnabled = element.globalFoldoutEnabled
                    }).ToList();

                    lightingPreset.skyboxSettings = new MagicLightmapSwitcher.SceneLightingPreset.SkyboxSettings(copyFrom.skyboxSettings);
                    lightingPreset.fogSettings = new MagicLightmapSwitcher.SceneLightingPreset.FogSettings(copyFrom.fogSettings);
                    lightingPreset.environmentSettings = new MagicLightmapSwitcher.SceneLightingPreset.EnvironmentSettings(copyFrom.environmentSettings);

#if BAKERY_INCLUDED
                    lightingPreset.bakeryLightMeshesSettings = copyFrom.bakeryLightMeshesSettings.Select(element => new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings()
                    {
                        parentGameObject = element.parentGameObject,
                        bakeryLightMesh = element.bakeryLightMesh,
                        UID = element.UID,
                        All = element.All,
                        color = element.color,
                        intensity = element.intensity,
                        texture = element.texture,
                        cutoff = element.cutoff,
                        samples = element.samples,
                        samples2 = element.samples2,
                        bitmask = element.bitmask,
                        selfShadow = element.selfShadow,
                        bakeToIndirect = element.bakeToIndirect,
                        indirectIntensity = element.indirectIntensity,
                        lmid = element.lmid,
                        bakeryLightMeshFoldoutEnabled = element.bakeryLightMeshFoldoutEnabled
                    }).ToList();                    
#endif
                }
                else
                {
                    copyFrom = magicLightmapSwitcher.lightingPresets[magicLightmapSwitcher.lightingPresets.Count - 1];
                    CreateNewPreset(magicLightmapSwitcher, copyFrom);
                    return;
                }
            }
            else
            {
                lightingPreset.environmentSettings.source = RenderSettings.ambientMode;
                lightingPreset.environmentSettings.intensityMultiplier = RenderSettings.ambientIntensity;
                lightingPreset.environmentSettings.ambientColor = RenderSettings.ambientLight;
                lightingPreset.environmentSettings.skyColor = RenderSettings.ambientSkyColor;
                lightingPreset.environmentSettings.equatorColor = RenderSettings.ambientEquatorColor;
                lightingPreset.environmentSettings.groundColor = RenderSettings.ambientGroundColor;
                    
                if (RenderSettings.skybox != null)
                {
                    if (RenderSettings.skybox.HasProperty("_Tex"))
                    {
                        lightingPreset.skyboxSettings.skyboxTexture =
                            RenderSettings.skybox.GetTexture("_Tex") as Cubemap;
                    }

                    lightingPreset.skyboxSettings.exposure = RenderSettings.skybox.GetFloat("_Exposure");

                    if (RenderSettings.skybox.HasProperty("_Tint"))
                    {
                        lightingPreset.skyboxSettings.tintColor = RenderSettings.skybox.GetColor("_Tint");
                    }
                    else if (RenderSettings.skybox.HasProperty("_SkyTint"))
                    {
                        lightingPreset.skyboxSettings.tintColor = RenderSettings.skybox.GetColor("_SkyTint");
                    }
                }

                #if BAKERY_INCLUDED
                BakerySkyLight bakerySkyLight = FindObjectOfType<BakerySkyLight>();

                if (bakerySkyLight != null)
                {
                    lightingPreset.skyboxSettings.bakerySkyLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.SkyboxSettings.BakerySkyLightsSettings();
                    lightingPreset.skyboxSettings.bakerySkyLightsSettings.bakerySky = bakerySkyLight;
                }

                lightingPreset.bakeryLightMeshesSettings = new List<MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings>();
                #endif

                lightingPreset.fogSettings.enabled = RenderSettings.fog;
                lightingPreset.fogSettings.fogColor = RenderSettings.fogColor;
                lightingPreset.fogSettings.fogDensity = RenderSettings.fogDensity;
            }

            magicLightmapSwitcher.lightingPresets.Add(lightingPreset);
        }

        public static void RemoveLightingPresetFromQueue(MagicLightmapSwitcher magicLightmapSwitcher, int index)
        {
            magicLightmapSwitcher.lightingPresets.RemoveAt(index);
        }

        private void DuplicatePreset(int pId)
        {
            magicLightmapSwitcher.lightingPresets[pId].UpdatePresetData();

            CreateNewPreset(magicLightmapSwitcher, magicLightmapSwitcher.lightingPresets[pId]);
            magicLightmapSwitcher.lightingPresets[pId + 1].MatchSceneSettings();
            directEditing = true;
            setActivePreset = magicLightmapSwitcher.lightingPresets.Count - 1;
            Init();
        }

        private void AddLightToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, Light[] lights)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] == null)
                {                    
                    continue;
                }

                if (lights[i].lightmapBakeType == LightmapBakeType.Realtime)
                {
                    Debug.LogFormat("<color=cyan>MLS:</color> Light source \"" + lights[i].name + "\" in Realtime mode skipped.");
                    continue;
                }

                AddLightToPreset(presets, lights[i]);
            }
        }

        private void AddLightToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, Light light)
        {
            if (light.lightmapBakeType == LightmapBakeType.Realtime)
            {
                EditorUtility.DisplayDialog("Magic Lightmap Switcher", "Realtime lights cannot be used in baking lightmaps.", "OK");
                return;
            }

            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i].lightSourceSettings.Find(item => item.light == light) != null)
                {
                    return;
                }

                if (magicLightmapSwitcher.workflow == MagicLightmapSwitcher.Workflow.MultiScene)
                {
                    if (light.gameObject.scene != magicLightmapSwitcher.gameObject.scene)
                    {
                        return;
                    }
                }

                MLSLight mlsLight = light.GetComponent<MLSLight>();                

                if (mlsLight == null)
                {
                    mlsLight = light.gameObject.AddComponent<MLSLight>();
                    mlsLight.UpdateGUID();
                }

                MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings lightSourceSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings();

                lightSourceSettings.mlsLightUID = mlsLight.lightGUID;
                lightSourceSettings.mlsLight = mlsLight;
                lightSourceSettings.light = light;

                if (light.transform.parent != null)
                {
                    lightSourceSettings.position = light.transform.localPosition;
                }
                else
                {
                    lightSourceSettings.position = light.transform.position;
                }

                lightSourceSettings.rotation = TransformUtils.GetInspectorRotation(light.transform);
                lightSourceSettings.color = light.color;                
                lightSourceSettings.colorTemperature = light.colorTemperature;
                lightSourceSettings.range = light.range;
                lightSourceSettings.spotOuterAngle = light.spotAngle;
                lightSourceSettings.spotInnerAngle = light.innerSpotAngle;
                lightSourceSettings.areaWidth = light.areaSize.x;
                lightSourceSettings.areaHeight = light.areaSize.y;
                lightSourceSettings.lightType = light.type;
                lightSourceSettings.intensity = light.intensity;
                lightSourceSettings.shadowsType = light.shadows;
                lightSourceSettings.shadowStrength = light.shadowStrength;
                lightSourceSettings.indirectMultiplier = light.bounceIntensity;
                lightSourceSettings.bakedShadowsRadius = light.shadowRadius;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                lightSourceSettings.additionalLightData = light.GetComponent<HDAdditionalLightData>();
#endif

                presets[i].lightSourceSettings.Add(lightSourceSettings);

#if BAKERY_INCLUDED
                AddBakeryLightToPreset(presets[i], light);
#endif
            }
        }

#if BAKERY_INCLUDED
        private void AddBakeryLightToPreset<T>(MagicLightmapSwitcher.SceneLightingPreset preset, T light)
        {
            Light currentLight = null;
            BakeryLightMesh bakeryCustomLightMesh = null;
            BakerySkyLight bakerySkyLight = null;

            if (light.GetType() == typeof(Light))
            {
                currentLight = light as Light;
            }
            else if (light.GetType() == typeof(BakeryLightMesh))
            {
                bakeryCustomLightMesh = light as BakeryLightMesh;
            }
            else if (light.GetType() == typeof(BakerySkyLight))
            {
                bakerySkyLight = light as BakerySkyLight;
            }

            if (currentLight != null)
            {
                switch (currentLight.type)
                {
                    case LightType.Directional:
                        BakeryDirectLight bakeryDirectLight = currentLight.gameObject.GetComponent<BakeryDirectLight>();

                        if (bakeryDirectLight != null)
                        {
                            MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryDirectLightsSettings bakeryDirectLightsSettings =
                                preset.lightSourceSettings.Find(item => item.light == currentLight).bakeryDirectLightsSettings;

                            if (bakeryDirectLightsSettings == null)
                            {
                                bakeryDirectLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryDirectLightsSettings();

                                bakeryDirectLightsSettings.parentGameObject = currentLight.gameObject;
                                bakeryDirectLightsSettings.bakeryDirect = bakeryDirectLight;
                                bakeryDirectLightsSettings.UID = bakeryDirectLight.UID;
                                bakeryDirectLightsSettings.color = bakeryDirectLight.color;
                                bakeryDirectLightsSettings.intensity = bakeryDirectLight.intensity;
                                bakeryDirectLightsSettings.shadowSpread = bakeryDirectLight.shadowSpread;
                                bakeryDirectLightsSettings.samples = bakeryDirectLight.samples;
                                bakeryDirectLightsSettings.bitmask = bakeryDirectLight.bitmask;
                                bakeryDirectLightsSettings.bakeToIndirect = bakeryDirectLight.bakeToIndirect;
                                bakeryDirectLightsSettings.shadowmask = bakeryDirectLight.shadowmask;
                                bakeryDirectLightsSettings.shadowmaskDenoise = bakeryDirectLight.shadowmaskDenoise;
                                bakeryDirectLightsSettings.indirectIntensity = bakeryDirectLight.indirectIntensity;
                                bakeryDirectLightsSettings.cloudShadow = bakeryDirectLight.cloudShadow;
                                bakeryDirectLightsSettings.cloudShadowTilingX = bakeryDirectLight.cloudShadowTilingX;
                                bakeryDirectLightsSettings.cloudShadowTilingY = bakeryDirectLight.cloudShadowTilingY;
                                bakeryDirectLightsSettings.cloudShadowOffsetX = bakeryDirectLight.cloudShadowOffsetX;
                                bakeryDirectLightsSettings.cloudShadowOffsetY = bakeryDirectLight.cloudShadowOffsetY;

                                preset.lightSourceSettings.Find(item => item.light == currentLight).bakeryDirectLightsSettings = bakeryDirectLightsSettings;
                            }
                        }
                        break;
                    case LightType.Point:
                    case LightType.Spot:
                        BakeryPointLight bakeryPointLight = currentLight.gameObject.GetComponent<BakeryPointLight>();

                        if (bakeryPointLight != null)
                        {
                            MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryPointLightsSettings bakeryPointLightsSettings =
                            preset.lightSourceSettings.Find(item => item.light == currentLight).bakeryPointLightsSettings;

                            if (bakeryPointLightsSettings == null)
                            {
                                bakeryPointLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryPointLightsSettings();

                                bakeryPointLightsSettings.parentGameObject = currentLight.gameObject;
                                bakeryPointLightsSettings.bakeryPoint = bakeryPointLight;
                                bakeryPointLightsSettings.UID = bakeryPointLight.UID;
                                bakeryPointLightsSettings.color = bakeryPointLight.color;
                                bakeryPointLightsSettings.intensity = bakeryPointLight.intensity;
                                bakeryPointLightsSettings.shadowSpread = bakeryPointLight.shadowSpread;
                                bakeryPointLightsSettings.cutoff = bakeryPointLight.cutoff;
                                bakeryPointLightsSettings.realisticFalloff = bakeryPointLight.realisticFalloff;
                                bakeryPointLightsSettings.samples = bakeryPointLight.samples;
                                bakeryPointLightsSettings.projMode = bakeryPointLight.projMode;
                                bakeryPointLightsSettings.cookie = bakeryPointLight.cookie;
                                bakeryPointLightsSettings.angle = bakeryPointLight.angle;
                                bakeryPointLightsSettings.innerAngle = bakeryPointLight.innerAngle;
                                bakeryPointLightsSettings.cubemap = bakeryPointLight.cubemap;
                                bakeryPointLightsSettings.iesFile = bakeryPointLight.iesFile;
                                bakeryPointLightsSettings.bitmask = bakeryPointLight.bitmask;
                                bakeryPointLightsSettings.bakeToIndirect = bakeryPointLight.bakeToIndirect;
                                bakeryPointLightsSettings.shadowmask = bakeryPointLight.shadowmask;
                                bakeryPointLightsSettings.indirectIntensity = bakeryPointLight.indirectIntensity;
                                bakeryPointLightsSettings.falloffMinRadius = bakeryPointLight.falloffMinRadius;

                                preset.lightSourceSettings.Find(item => item.light == currentLight).bakeryPointLightsSettings = bakeryPointLightsSettings;
                            }
                        }
                        break;
                    case LightType.Area:
                    case LightType.Disc:
                        BakeryLightMesh bakeryLightMesh = currentLight.gameObject.GetComponent<BakeryLightMesh>();

                        if (bakeryLightMesh != null)
                        {
                            MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings bakeryLightMeshesSettings =
                            preset.lightSourceSettings.Find(item => item.light == currentLight).bakeryLightMeshesSettings;

                            if (bakeryLightMeshesSettings == null)
                            {
                                bakeryLightMeshesSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings();

                                bakeryLightMeshesSettings.parentGameObject = currentLight.gameObject;
                                bakeryLightMeshesSettings.bakeryLightMesh = bakeryLightMesh;
                                bakeryLightMeshesSettings.UID = bakeryLightMesh.UID;
                                bakeryLightMeshesSettings.color = bakeryLightMesh.color;
                                bakeryLightMeshesSettings.intensity = bakeryLightMesh.intensity;
                                bakeryLightMeshesSettings.texture = bakeryLightMesh.texture;
                                bakeryLightMeshesSettings.cutoff = bakeryLightMesh.cutoff;
                                bakeryLightMeshesSettings.samples = bakeryLightMesh.samples;
                                bakeryLightMeshesSettings.samples2 = bakeryLightMesh.samples2;
                                bakeryLightMeshesSettings.bitmask = bakeryLightMesh.bitmask;
                                bakeryLightMeshesSettings.selfShadow = bakeryLightMesh.selfShadow;
                                bakeryLightMeshesSettings.bakeToIndirect = bakeryLightMesh.bakeToIndirect;
                                bakeryLightMeshesSettings.indirectIntensity = bakeryLightMesh.indirectIntensity;
                                bakeryLightMeshesSettings.lmid = bakeryLightMesh.lmid;

                                preset.lightSourceSettings.Find(item => item.light == currentLight).bakeryLightMeshesSettings = bakeryLightMeshesSettings;
                            }
                        }
                        break;
                }
            }
            else if (bakeryCustomLightMesh != null)
            {
                BakeryLightMesh bakeryLightMesh = bakeryCustomLightMesh.gameObject.GetComponent<BakeryLightMesh>();

                if (bakeryLightMesh != null)
                {
                    MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings bakeryLightMeshesSettings =
                            preset.bakeryLightMeshesSettings.Find(item => item.bakeryLightMesh == bakeryLightMesh);

                    if (bakeryLightMeshesSettings == null)
                    {
                        bakeryLightMeshesSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings();

                        bakeryLightMeshesSettings.parentGameObject = bakeryLightMesh.gameObject;
                        bakeryLightMeshesSettings.bakeryLightMesh = bakeryLightMesh;
                        bakeryLightMeshesSettings.UID = bakeryLightMesh.UID;
                        bakeryLightMeshesSettings.color = bakeryLightMesh.color;
                        bakeryLightMeshesSettings.intensity = bakeryLightMesh.intensity;
                        bakeryLightMeshesSettings.texture = bakeryLightMesh.texture;
                        bakeryLightMeshesSettings.cutoff = bakeryLightMesh.cutoff;
                        bakeryLightMeshesSettings.samples = bakeryLightMesh.samples;
                        bakeryLightMeshesSettings.samples2 = bakeryLightMesh.samples2;
                        bakeryLightMeshesSettings.bitmask = bakeryLightMesh.bitmask;
                        bakeryLightMeshesSettings.selfShadow = bakeryLightMesh.selfShadow;
                        bakeryLightMeshesSettings.bakeToIndirect = bakeryLightMesh.bakeToIndirect;
                        bakeryLightMeshesSettings.indirectIntensity = bakeryLightMesh.indirectIntensity;
                        bakeryLightMeshesSettings.lmid = bakeryLightMesh.lmid;

                        preset.bakeryLightMeshesSettings.Add(bakeryLightMeshesSettings);
                    }
                }
            }
            else if (bakerySkyLight != null)
            {
                if (preset.skyboxSettings.bakerySkyLightsSettings == null)
                {
                    preset.skyboxSettings.bakerySkyLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.SkyboxSettings.BakerySkyLightsSettings();
                    preset.skyboxSettings.bakerySkyLightsSettings.bakerySky = bakerySkyLight;
                }
            }
        }
#endif
        private void AddCustomBlendableToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, MLSCustomBlendable[] customBlendables)
        {
            for (int i = 0; i < customBlendables.Length; i++)
            {
                if (customBlendables[i] == null)
                {
                    continue;
                }

                AddCustomBlendableToPreset(presets, customBlendables[i]);
            }
        }

        private void AddCustomBlendableToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, MLSCustomBlendable customBlendable)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i].customBlendablesSettings.Find(item => item.sourceScriptId == customBlendable.sourceScriptId) != null)
                {
                    return;
                }

                MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings customBlendablesSettings = new MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings();

                customBlendable.GetSharedParameters();

                customBlendablesSettings.sourceScript = customBlendable;
                customBlendablesSettings.sourceScriptId = customBlendable.sourceScriptId;

                for (int j = 0; j < customBlendable.blendableFloatFields.Count; j++)
                {
                    customBlendablesSettings.blendableFloatParameters.Add(customBlendable.blendableFloatFields[j].Name);
                    customBlendablesSettings.blendableFloatParametersValues.Add((float) customBlendable.blendableFloatFields[j].GetValue(customBlendable));
                }

                for (int j = 0; j < customBlendable.blendableCubemapParameters.Count; j++)
                {
                    customBlendablesSettings.blendableCubemapParameters.Add(customBlendable.blendableCubemapParameters[j].Name);
                    customBlendablesSettings.blendableCubemapParametersValues.Add((Cubemap) customBlendable.blendableCubemapParameters[j].GetValue(customBlendable));
                }

                for (int j = 0; j < customBlendable.blendableColorParameters.Count; j++)
                {
                    customBlendablesSettings.blendableColorParameters.Add(customBlendable.blendableColorParameters[j].Name);
                    customBlendablesSettings.blendableColorParametersValues.Add((Color) customBlendable.blendableColorParameters[j].GetValue(customBlendable));
                }

                presets[i].customBlendablesSettings.Add(customBlendablesSettings);
            }
        }

        private void AddGameObjectToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, GameObject[] gameObjects)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i] == null)
                {
                    continue;
                }

                AddGameObjectToPreset(presets, gameObjects[i]);
            }
        }

        private void AddGameObjectToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, GameObject gameObject)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i].gameObjectsSettings.Find(item => item.gameObject == gameObject) != null)
                {
                    return;
                }

                MagicLightmapSwitcher.SceneLightingPreset.GameObjectSettings gameObjectSettings = new MagicLightmapSwitcher.SceneLightingPreset.GameObjectSettings();

                gameObjectSettings.gameObject = gameObject;
                gameObjectSettings.instanceId = gameObject.GetHashCode();
                gameObjectSettings.position = gameObject.transform.localPosition;
                gameObjectSettings.rotation = gameObject.transform.rotation;
                gameObjectSettings.enabled = gameObject.activeInHierarchy;

                presets[i].gameObjectsSettings.Add(gameObjectSettings);
            }
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

            if (magicLightmapSwitcher == null)
            {
                Init(true);
            }

            if (!MLSEditorUtils.stylesInitialized)
            {
                MLSEditorUtils.InitStyles();
            }     

            if (directEditing)
            {
                directEditing = false;

                for (int i = 0; i < magicLightmapSwitcher.lightingPresets.Count; i++)
                {
                    magicLightmapSwitcher.lightingPresets[i].foldoutEnabled = false;
                }

                magicLightmapSwitcher.lightingPresets[setActivePreset].foldoutEnabled = true;
            }

            GUILayout.Label("Presets For Scene: " + targetScene, MLSEditorUtils.captionStyle);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create New..."))
                {
                    CreateNewPreset(magicLightmapSwitcher);
                }
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                for (int lp = 0; lp < magicLightmapSwitcher.lightingPresets.Count; lp++)
                {
                    using (new GUILayout.VerticalScope(GUI.skin.box))
                    {
                        magicLightmapSwitcher.lightingPresets[lp].foldoutEnabled =
                            EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].foldoutEnabled, "Preset: " + magicLightmapSwitcher.lightingPresets[lp].name, true, MLSEditorUtils.presetMainFoldout);

                        if (magicLightmapSwitcher.lightingPresets[lp].foldoutEnabled)
                        {
                            EditorGUI.BeginChangeCheck();

                            for (int i = 0; i < magicLightmapSwitcher.lightingPresets.Count; i++)
                            {
                                if (magicLightmapSwitcher.lightingPresets[i] != magicLightmapSwitcher.lightingPresets[lp])
                                {
                                    magicLightmapSwitcher.lightingPresets[i].foldoutEnabled = false;
                                }
                            }

                            if ((focusedWindow != null && presetsManagerWindow != null && focusedWindow.titleContent.text == presetsManagerWindow.name) || 
                                EditorGUIUtility.GetObjectPickerObject() != null ||
                                (EditorWindow.mouseOverWindow != null && EditorWindow.mouseOverWindow.GetType().ToString() == "UnityEditor.ColorPicker"))
                            {
                                magicLightmapSwitcher.lightingPresets[lp].MatchSceneSettings();
                            }
                            else
                            {
                                magicLightmapSwitcher.lightingPresets[lp].UpdatePresetData();
                            }

                            using (new GUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Duplicate And Edit", GUILayout.MinWidth(150)))
                                {
                                    DuplicatePreset(lp);
                                }

                                if (GUILayout.Button("Remove", GUILayout.MinWidth(150)))
                                {
                                    magicLightmapSwitcher.lightingPresets.RemoveAt(lp);
                                    continue;
                                }
                            }

                            GUILayout.Space(10);

                            GUILayout.Label("General Settings", MLSEditorUtils.captionStyle);

                            magicLightmapSwitcher.lightingPresets[lp].name = EditorGUILayout.TextField("Name: ", magicLightmapSwitcher.lightingPresets[lp].name);
                            magicLightmapSwitcher.lightingPresets[lp].included = EditorGUILayout.Toggle("Included: ", magicLightmapSwitcher.lightingPresets[lp].included);

                            GUILayout.Space(10);

                            EditorGUI.BeginChangeCheck();

                            #region Game Objects
                            GUILayout.Label("Game Objects", MLSEditorUtils.captionStyle);

                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();

                                GameObject selectedGameObject = null;
                                GameObject[] selectedGameObjects = null;

                                if (Selection.activeObject != null)
                                {
                                    if (Selection.gameObjects.Length > 1)
                                    {
                                        selectedGameObjects = new GameObject[Selection.gameObjects.Length];

                                        for (int o = 0; o < Selection.gameObjects.Length; o++)
                                        {
                                            selectedGameObjects[o] = Selection.gameObjects[o];
                                        }
                                    }
                                    else
                                    {
                                        if (Selection.activeGameObject != null)
                                        {
                                            selectedGameObject = Selection.activeGameObject;
                                        }
                                    }
                                }

                                if (selectedGameObject == null && selectedGameObjects == null)
                                {
                                    GUI.enabled = false;
                                }

                                if (GUILayout.Button(selectedGameObject != null ? "Add Selected Object..." : selectedGameObjects != null ? "Add Selected Objects..." : "No Selected Objects", GUILayout.MaxWidth(150)))
                                {
                                    if (selectedGameObject != null)
                                    {
                                        AddGameObjectToPreset(magicLightmapSwitcher.lightingPresets, selectedGameObject);
                                    }
                                    else if (selectedGameObjects != null)
                                    {
                                        AddGameObjectToPreset(magicLightmapSwitcher.lightingPresets, selectedGameObjects);
                                    }

                                }

                                GUI.enabled = true;
                            }

                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                if (magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings.Count == 0)
                                {
                                    EditorGUILayout.HelpBox("There are no Game Objects in the preset.", MessageType.Info);
                                }

                                for (int i = 0; i < magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings.Count; i++)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].globalFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].globalFoldoutEnabled,
                                                    magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].gameObject.name, true);

                                            GUILayout.FlexibleSpace();

                                            if (GUILayout.Button("Remove"))
                                            {
                                                for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[j].gameObjectsSettings.RemoveAt(i);
                                                }

                                                continue;
                                            }
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].globalFoldoutEnabled)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].transformFoldoutEnabled =
                                                    EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].transformFoldoutEnabled, "Transform", true);

                                            if (magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].transformFoldoutEnabled)
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].position =
                                                    EditorGUILayout.Vector3Field("Position", magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].position);

                                                if (magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].justAdded)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].justAdded = false;
                                                    magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation =
                                                        TransformUtils.GetInspectorRotation(magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].gameObject.transform);
                                                }

                                                magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation =
                                                    EditorGUILayout.Vector3Field("Rotation", magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation);

                                                magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].rotation =
                                                    Quaternion.Euler(
                                                        magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation.x,
                                                        magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation.y,
                                                        magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation.z);
                                            }

                                            magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].enabled =
                                                EditorGUILayout.Toggle("Enabled", magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].enabled);
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Lights Settings
                            GUILayout.Label("Light Sources", MLSEditorUtils.captionStyle);

                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();

                                Light selectedLight = null;
                                Light[] selectedLights = null;

                                if (Selection.activeObject != null)
                                {                                    
                                    if (Selection.gameObjects.Length > 1)
                                    {
                                        selectedLights = new Light[Selection.gameObjects.Length];

                                        for (int o = 0; o < Selection.gameObjects.Length; o++)
                                        {
                                            selectedLights[o] = Selection.gameObjects[o].GetComponent<Light>();
                                        }
                                    }
                                    else
                                    {
                                        if (Selection.activeGameObject != null)
                                        {
                                            selectedLight = Selection.activeGameObject.GetComponent<Light>();
                                        }
                                    }
                                }                                

                                if (selectedLight == null && selectedLights == null)
                                {
                                    GUI.enabled = false;
                                }                                

                                if (GUILayout.Button(selectedLight != null ? "Add Selected Light..." : selectedLights != null ? "Add Selected Lights..." : "No Selected Lights", GUILayout.MaxWidth(150)))
                                {
                                    if (selectedLight != null)
                                    {
                                        AddLightToPreset(magicLightmapSwitcher.lightingPresets, selectedLight);
                                    }
                                    else if (selectedLights != null)
                                    {
                                        AddLightToPreset(magicLightmapSwitcher.lightingPresets, selectedLights);
                                    }
                                }

                                GUI.enabled = true;
                            }

                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings.Count == 0)
                                {
                                    EditorGUILayout.HelpBox("There are no Lights in the preset.", MessageType.Info);
                                }

                                for (int i = 0; i < magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings.Count; i++)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        string lightName = "_temp";
                                        bool externalConrol = false;

                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.GetComponent<MLSLight>() == null)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.gameObject.AddComponent<MLSLight>();
                                        }

                                        magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.GetComponent<MLSLight>().lastEditedBy = magicLightmapSwitcher.lightingPresets[lp].name;

                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].mlsLight.exludeFromStoring)
                                        {
                                            lightName = magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.name + " (External control - editing locked)";
                                            externalConrol = true;
                                        }
                                        else
                                        {
                                            lightName = magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.name;
                                        }

                                        using (new GUILayout.HorizontalScope())
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].globalFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].globalFoldoutEnabled,
                                                    lightName, true);

                                            GUILayout.FlexibleSpace();

                                            if (GUILayout.Button("Select On Scene"))
                                            {
                                                Selection.activeGameObject = magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.gameObject;
                                            }

                                            if (GUILayout.Button("Remove"))
                                            {
                                                for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                                {
                                                    MLSLight mlsLight = magicLightmapSwitcher.lightingPresets[j].lightSourceSettings[i].light.gameObject.GetComponent<MLSLight>();

                                                    if (mlsLight != null)
                                                    {
                                                        mlsLight.destroyedFromManager = true;
                                                        DestroyImmediate(magicLightmapSwitcher.lightingPresets[j].lightSourceSettings[i].light.gameObject.GetComponent<MLSLight>());
                                                    }

                                                    magicLightmapSwitcher.lightingPresets[j].lightSourceSettings.RemoveAt(i);
                                                }
                                                
                                                continue;
                                            }
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].globalFoldoutEnabled)
                                        {
                                            if (externalConrol)
                                            {
                                                GUI.enabled = false;
                                            }

                                            using (new GUILayout.VerticalScope(GUI.skin.box))
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].transformFoldoutEnabled =
                                                    EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].transformFoldoutEnabled, "Transform", true);

                                                if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].transformFoldoutEnabled)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].position =
                                                            EditorGUILayout.Vector3Field("Position", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].position);   
                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].rotation =
                                                        EditorGUILayout.Vector3Field("Rotation", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].rotation);
                                                }

                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].settingsFoldoutEnabled =
                                                    EditorGUILayout.Foldout(
                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].settingsFoldoutEnabled,
                                                        "General Settings", 
                                                        true);

                                                if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].settingsFoldoutEnabled)
                                                {
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.type =
                                                        (HDLightType) EditorGUILayout.EnumPopup("Type", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.type);

                                                    switch (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.type)
#else
                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].lightType =
                                                        (LightType) EditorGUILayout.EnumPopup("Type", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].lightType);
                                                    
                                                    switch (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].lightType)
#endif
                                                    {
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                        case UnityEngine.Rendering.HighDefinition.HDLightType.Directional:
#else
                                                        case LightType.Directional:
#endif
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Shape", MLSEditorUtils.boldLabelStyle);                                                            
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.angularDiameter =
                                                                EditorGUILayout.FloatField("Angular Diameter", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.angularDiameter);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.angularDiameter < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.angularDiameter = 0;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareSize =
                                                                EditorGUILayout.FloatField("Flare Size", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareSize);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareSize < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareSize = 0;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareFalloff =
                                                               EditorGUILayout.FloatField("Flare Falloff", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareFalloff);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareFalloff < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareFalloff = 0;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareTint =
                                                               EditorGUILayout.ColorField("Flare Tint", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.flareTint);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.distance =
                                                               EditorGUILayout.FloatField("Distance", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.distance);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.distance < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.distance = 0;
                                                            }

                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Emission", MLSEditorUtils.boldLabelStyle);
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature =
                                                                EditorGUILayout.FloatField("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity, 0f, 130000.0f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier);
                                                            
                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier = 0;
                                                            }

                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Volumetrics", MLSEditorUtils.boldLabelStyle);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricDimmer =
                                                                EditorGUILayout.Slider("Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricDimmer, 0f, 16.0f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricShadowDimmer =
                                                                EditorGUILayout.Slider("Shadow Dimmer", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricShadowDimmer, 0f, 1.0f);
#else
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color =
                                                                EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature, 1000f, 20000f);                                                            

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity, 0f, 100.0f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier = 0;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowsType =
                                                                (LightShadows) EditorGUILayout.EnumPopup("Shadow Type", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowsType);

                                                            switch (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowsType)
                                                            {
                                                                case LightShadows.Soft:
                                                                    break;
                                                                case LightShadows.Hard:
                                                                case LightShadows.None:
                                                                    GUI.enabled = false;
                                                                    break;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].directionalBakedShadowAngle =
                                                                EditorGUILayout.Slider("Baked Shadow Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].directionalBakedShadowAngle, 0f, 90.0f);
                                                            
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowStrength =
                                                                EditorGUILayout.Slider("Shadow Strength", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowStrength, 0f, 1.0f);
#endif
                                                            GUI.enabled = true;
                                                            break;
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                        case UnityEngine.Rendering.HighDefinition.HDLightType.Point:
#else
                                                        case LightType.Point:
#endif
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Shape", MLSEditorUtils.boldLabelStyle);
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius =
                                                                EditorGUILayout.FloatField("Radius", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.angularDiameter < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.angularDiameter = 0;
                                                            }

                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Emission", MLSEditorUtils.boldLabelStyle);   

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature =
                                                                EditorGUILayout.FloatField("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity, 0f, 3183.099f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.luxAtDistance =
                                                                EditorGUILayout.FloatField("At", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.luxAtDistance);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.luxAtDistance < 0.01f)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.luxAtDistance = 0.01f;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range < 0.001f)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range = 0.001f;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier = 0;
                                                            }

                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Volumetrics", MLSEditorUtils.boldLabelStyle);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricDimmer =
                                                                EditorGUILayout.Slider("Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricDimmer, 0f, 16.0f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricShadowDimmer =
                                                                EditorGUILayout.Slider("Shadow Dimmer", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricShadowDimmer, 0f, 1.0f);

#if MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricFadeDistance =
                                                                EditorGUILayout.FloatField("Fade Distance", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricFadeDistance);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricFadeDistance < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricFadeDistance = 0;
                                                            }
#endif
#else
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range < 0.001f)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range = 0.001f;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature, 1000f, 20000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity, 0f, 100f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier = 0;
                                                            }
                                                            
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowsType =
                                                                (LightShadows) EditorGUILayout.EnumPopup("Shadow Type", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowsType);

                                                            switch (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowsType)
                                                            {
                                                                case LightShadows.Soft:
                                                                    break;
                                                                case LightShadows.Hard:
                                                                case LightShadows.None:
                                                                    GUI.enabled = false;
                                                                    break;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowStrength =
                                                                EditorGUILayout.Slider("Shadow Strength", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowStrength, 0f, 1.0f);
#endif
                                                            break;
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                        case UnityEngine.Rendering.HighDefinition.HDLightType.Spot:
#else
                                                        case LightType.Spot:
#endif
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Shape", MLSEditorUtils.boldLabelStyle);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.spotLightShape =
                                                                (SpotLightShape) EditorGUILayout.EnumPopup("Shape", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.spotLightShape);

                                                            switch (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.spotLightShape)
                                                            {
                                                                case SpotLightShape.Cone:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].spotOuterAngle =
                                                                        EditorGUILayout.Slider("Outer Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].spotOuterAngle, 1, 179);

                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.innerSpotPercent =
                                                                        EditorGUILayout.Slider("Inner Angle (%)", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.innerSpotPercent, 0, 100f);

                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius =
                                                                        EditorGUILayout.FloatField("Radius", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius);

                                                                    if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius < 0)
                                                                    {
                                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius = 0;
                                                                    }
                                                                    break;
                                                                case SpotLightShape.Pyramid:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].spotOuterAngle =
                                                                        EditorGUILayout.Slider("Spot Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].spotOuterAngle, 1, 179);

                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.aspectRatio =
                                                                        EditorGUILayout.Slider("Aspect Ratio", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.aspectRatio, 0.05f, 20f);

                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius =
                                                                        EditorGUILayout.FloatField("Radius", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius);

                                                                    if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius < 0)
                                                                    {
                                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius = 0;
                                                                    }
                                                                    break;
                                                                case SpotLightShape.Box:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeWidth =
                                                                        EditorGUILayout.FloatField("Size X", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeWidth);

                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeHeight =
                                                                        EditorGUILayout.FloatField("Size Y", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeHeight);
                                                                    break;
                                                            }

                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Emission", MLSEditorUtils.boldLabelStyle);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature =
                                                                EditorGUILayout.FloatField("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity, 0f, 40000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range = 0;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier = 0;
                                                            }

                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Volumetrics", MLSEditorUtils.boldLabelStyle);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricDimmer =
                                                                EditorGUILayout.Slider("Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricDimmer, 0f, 16.0f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricShadowDimmer =
                                                                EditorGUILayout.Slider("Shadow Dimmer", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricShadowDimmer, 0f, 1.0f);
#if MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricFadeDistance =
                                                                EditorGUILayout.FloatField("Fade Distance", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricFadeDistance);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricFadeDistance < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.volumetricFadeDistance = 0;
                                                            }
#endif
#else
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range);
                                                            
#if MT_URP_7_INCLUDED || MT_URP_8_INCLUDED || MT_URP_9_INCLUDED || MT_URP_10_INCLUDED || MT_URP_11_INCLUDED || MT_URP_12_INCLUDED
                                                            EditorGUILayout.MinMaxSlider("Spot Angle", 
                                                                ref magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].spotInnerAngle, 
                                                                ref magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].spotOuterAngle, 1, 179);
#else
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].spotOuterAngle =
                                                                EditorGUILayout.Slider("Spot Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].spotOuterAngle, 1, 179);
#endif

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature, 1000f, 20000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity, 0f, 100f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier = 0;
                                                            }
                                                            
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowsType =
                                                                (LightShadows) EditorGUILayout.EnumPopup("Shadow Type", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowsType);

                                                            switch (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowsType)
                                                            {
                                                                case LightShadows.Soft:
                                                                    break;
                                                                case LightShadows.Hard:
                                                                case LightShadows.None:
                                                                    GUI.enabled = false;
                                                                    break;
                                                            }
                                                            
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakedShadowsRadius =
                                                                EditorGUILayout.FloatField("Baked Shadows Radius", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakedShadowsRadius);
                                                            
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowStrength =
                                                                EditorGUILayout.Slider("Shadow Strength", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].shadowStrength, 0f, 1.0f);
                                                            
#endif
                                                            break;
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                        case UnityEngine.Rendering.HighDefinition.HDLightType.Area:
#else
                                                        case LightType.Area:
#endif
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Shape", MLSEditorUtils.boldLabelStyle);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.areaLightShape =
                                                                (AreaLightShape) EditorGUILayout.EnumPopup("Shape", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.areaLightShape);

                                                            switch (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.areaLightShape)
                                                            {
                                                                case AreaLightShape.Rectangle:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeWidth =
                                                                        EditorGUILayout.FloatField("Size X", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeWidth);

                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeHeight =
                                                                        EditorGUILayout.FloatField("Size Y", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeHeight);
                                                                    break;
                                                                case AreaLightShape.Disc:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius =
                                                                        EditorGUILayout.FloatField("Radius", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].additionalLightData.shapeRadius);
                                                                    break;
                                                                case AreaLightShape.Tube:
                                                                    EditorGUILayout.HelpBox("Real-time light sources are not counted when baking.", MessageType.Warning);
                                                                    break;
                                                            }

                                                            GUILayout.Space(10);
                                                            GUILayout.Label("Emission", MLSEditorUtils.boldLabelStyle);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature =
                                                                EditorGUILayout.FloatField("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity, 0f, 40000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range = 0;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier = 0;
                                                            }
#else
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].areaWidth =
                                                                EditorGUILayout.FloatField("Width", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].areaWidth);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].areaHeight =
                                                                EditorGUILayout.FloatField("Height", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].areaHeight);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature, 1000f, 20000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity, 0f, 100f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier);
#endif
                                                            break;
#if !MT_HDRP_7_INCLUDED && !MT_HDRP_8_INCLUDED && !MT_HDRP_9_INCLUDED && !MT_HDRP_10_INCLUDED && !MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
#if MT_HDRP_12_INCLUDED
                                                        case (HDLightType) LightType.Disc:
#else
                                                        case LightType.Disc:
#endif
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].range);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].areaWidth =
                                                                EditorGUILayout.FloatField("Radius", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].areaWidth);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].colorTemperature, 1000f, 20000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity, 0f, 100f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].indirectMultiplier);
                                                            break;
#endif
                                                    }
                                                }
#if BAKERY_INCLUDED
                                                switch (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.type)
                                                {
                                                    case LightType.Directional:
                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.GetComponent<BakeryDirectLight>() == null)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings = null;
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[lp], magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light);
                                                        }

                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.bakeryDirectFoldoutEnabled =
                                                            EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.bakeryDirectFoldoutEnabled, "Bakery Settings", true);

                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.bakeryDirectFoldoutEnabled)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.color =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.intensity =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.shadowSpread =
                                                                EditorGUILayout.FloatField("Shadow Spread", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.shadowSpread);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.samples =
                                                                EditorGUILayout.IntField("Shadow Samples", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.samples);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.bitmask =
                                                                EditorGUILayout.IntField("Bitmask", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.bitmask);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.indirectIntensity =
                                                                EditorGUILayout.FloatField("Indirect Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.indirectIntensity);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadow =
                                                                EditorGUILayout.ObjectField("Texture Projection", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadow, typeof(Texture2D), false) as Texture2D;
                                                        
                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadow != null)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadowTilingX =
                                                                    EditorGUILayout.FloatField("Tiling U", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadowTilingX);

                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadowTilingY =
                                                                    EditorGUILayout.FloatField("Tiling V", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadowTilingY);

                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadowOffsetX =
                                                                   EditorGUILayout.FloatField("Offset X", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadowOffsetX);

                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadowOffsetY =
                                                                    EditorGUILayout.FloatField("Offset Y", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryDirectLightsSettings.cloudShadowOffsetY);
                                                            }
                                                        }
                                                        break;
                                                    case LightType.Point:
                                                    case LightType.Spot:
                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.GetComponent<BakeryPointLight>() == null)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings = null;
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[lp], magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light);
                                                        }

                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.bakeryPointFoldoutEnabled =
                                                            EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.bakeryPointFoldoutEnabled, "Bakery Settings", true);

                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.bakeryPointFoldoutEnabled)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.color =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.intensity =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.shadowSpread =
                                                                EditorGUILayout.FloatField("Shadow Spread", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.shadowSpread);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.realisticFalloff =
                                                                EditorGUILayout.Toggle("Physical  Falloff", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.realisticFalloff);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.realisticFalloff)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.falloffMinRadius =
                                                                    EditorGUILayout.FloatField("Falloff Min Size", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.falloffMinRadius);
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.projMode =
                                                                (BakeryPointLight.ftLightProjectionMode) EditorGUILayout.EnumPopup("Projection Mask", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.projMode);

                                                            switch (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.projMode)
                                                            {
                                                                case BakeryPointLight.ftLightProjectionMode.Omni:
                                                                    break;
                                                                case BakeryPointLight.ftLightProjectionMode.Cookie:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.cookie =
                                                                        EditorGUILayout.ObjectField("Cookie Texture", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.cookie, typeof(Texture2D), false) as Texture2D;
                                                                    break;
                                                                case BakeryPointLight.ftLightProjectionMode.Cubemap:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.cubemap =
                                                                        EditorGUILayout.ObjectField("Projection Cubemap", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.cubemap, typeof(Cubemap), false) as Cubemap;
                                                                    break;
                                                                case BakeryPointLight.ftLightProjectionMode.IES:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.iesFile =
                                                                        EditorGUILayout.ObjectField("IES File", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.iesFile, typeof(Object), false) as Object;
                                                                    break;
                                                                case BakeryPointLight.ftLightProjectionMode.Cone:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.angle =
                                                                        EditorGUILayout.Slider("Outer Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.angle, 1.0f, 180.0f);
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.innerAngle =
                                                                        EditorGUILayout.Slider("Inner Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.innerAngle, 0.0f, 100.0f);
                                                                    break;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.samples =
                                                                EditorGUILayout.IntField("Shadow Samples", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.samples);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.bitmask =
                                                                EditorGUILayout.IntField("Bitmask", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.bitmask);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.indirectIntensity =
                                                                EditorGUILayout.FloatField("Indirect Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryPointLightsSettings.indirectIntensity);
                                                        }
                                                        break;
                                                    case LightType.Area:
                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light.GetComponent<BakeryLightMesh>() == null)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings = null;
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[lp], magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].light);
                                                        }

                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.bakeryLightMeshFoldoutEnabled =
                                                            EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.bakeryLightMeshFoldoutEnabled, "Bakery Settings", true);

                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.bakeryLightMeshFoldoutEnabled)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.color =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].color;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.intensity =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].intensity;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.texture =
                                                                        EditorGUILayout.ObjectField(
                                                                            "Texture", 
                                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.texture, typeof(Texture2D), false) as Texture2D;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.cutoff =
                                                                    EditorGUILayout.FloatField("Cutoff", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.cutoff);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.samples =
                                                                    EditorGUILayout.IntField("Samples Near", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.samples);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.samples2 =
                                                                    EditorGUILayout.IntField("Samples Far", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.samples2);                                                            

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.bitmask =
                                                                EditorGUILayout.IntField("Bitmask", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.bitmask);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.selfShadow =
                                                                EditorGUILayout.Toggle("Self Shadow", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.selfShadow);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.indirectIntensity =
                                                                EditorGUILayout.FloatField("Indirect Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourceSettings[i].bakeryLightMeshesSettings.indirectIntensity);
                                                        }
                                                        break;
                                                }
                                                
#endif
                                            }

                                            GUI.enabled = true;
                                        }
                                    }
                                }
                            }
                            #endregion

#if BAKERY_INCLUDED
                            #region Bakery Light Meshes
                            GUILayout.Label("Bakery Light Meshes", MLSEditorUtils.captionStyle);

                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();

                                BakeryLightMesh selectedMesh = null;
                                BakeryLightMesh[] selectedMeshes = null;

                                if (Selection.activeObject != null)
                                {
                                    if (Selection.gameObjects.Length > 1)
                                    {
                                        selectedMeshes = new BakeryLightMesh[Selection.gameObjects.Length];

                                        for (int o = 0; o < Selection.gameObjects.Length; o++)
                                        {
                                            BakeryLightMesh lightMesh = Selection.gameObjects[o].GetComponent<BakeryLightMesh>();

                                            if (Selection.gameObjects[o].GetComponent<Light>() == null)
                                            {
                                                selectedMeshes[o] = lightMesh;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Selection.activeGameObject != null)
                                        {
                                            BakeryLightMesh lightMesh = Selection.activeGameObject.GetComponent<BakeryLightMesh>();

                                            if (Selection.activeGameObject.GetComponent<Light>() == null)
                                            {
                                                selectedMesh = Selection.activeGameObject.GetComponent<BakeryLightMesh>();
                                            }
                                        }
                                    }
                                }

                                if (selectedMesh == null && selectedMeshes == null)
                                {
                                    GUI.enabled = false;
                                }

                                if (GUILayout.Button(selectedMesh != null ? "Add Selected Object..." : selectedMeshes != null ? "Add Selected Objects..." : "No Selected Objects", GUILayout.MaxWidth(150)))
                                {
                                    if (selectedMesh != null)
                                    {
                                        for (int p = 0; p < magicLightmapSwitcher.lightingPresets.Count; p++)
                                        {
                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[p], selectedMesh);
                                        }                                        
                                    }
                                    else if (selectedMeshes != null)
                                    {
                                        for (int p = 0; p < magicLightmapSwitcher.lightingPresets.Count; p++)
                                        {
                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[p], selectedMeshes);
                                        }
                                    }
                                }

                                GUI.enabled = true;
                            } 
                            
                            using (new GUILayout.VerticalScope(GUI.skin.box)) 
                            {
                                if (magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings.Count == 0)
                                {
                                    EditorGUILayout.HelpBox("There are no Bakery Light Meshes in the preset.", MessageType.Info);
                                }

                                for (int i = 0; i < magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings.Count; i++)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        if (magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMesh == null)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings.RemoveAt(i);
                                            continue;
                                        }

                                        using (new GUILayout.HorizontalScope())
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMeshFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMeshFoldoutEnabled,
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMesh.name, true);

                                            GUILayout.FlexibleSpace();

                                            if (GUILayout.Button("Select On Scene"))
                                            {
                                                Selection.activeGameObject = magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMesh.gameObject;
                                            }

                                            if (GUILayout.Button("Remove"))
                                            {
                                                for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[j].bakeryLightMeshesSettings.RemoveAt(i);
                                                }

                                                continue;
                                            }
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMeshFoldoutEnabled)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].color =
                                                EditorGUILayout.ColorField(
                                                    "Color",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].color);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].intensity =
                                                EditorGUILayout.FloatField(
                                                    "Intensity",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].intensity);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].texture =
                                                EditorGUILayout.ObjectField(
                                                    "Texture",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].texture, typeof(Texture2D), false) as Texture2D;

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].cutoff =
                                                EditorGUILayout.FloatField(
                                                    "Cutoff",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].cutoff);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].samples =
                                                EditorGUILayout.IntField(
                                                    "Samples Near",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].samples);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].samples2 =
                                                EditorGUILayout.IntField(
                                                    "Samples Far",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].samples2);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bitmask =
                                                EditorGUILayout.IntField(
                                                    "Bitmask",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bitmask);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].selfShadow =
                                                EditorGUILayout.Toggle(
                                                    "Selfshadow",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].selfShadow);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].indirectIntensity =
                                                EditorGUILayout.FloatField(
                                                    "Indirect Intensity",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].indirectIntensity);
                                        }
                                    }
                                }
                            }
#endregion
#endif

                            #region Custom Blendable
                            GUILayout.Label("Custom Blendables", MLSEditorUtils.captionStyle);

                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();

                                MLSCustomBlendable selectedCustomBlendable = null;
                                MLSCustomBlendable[] selectedCustomBlendables = null;

                                if (Selection.activeObject != null)
                                {
                                    if (Selection.gameObjects.Length > 1)
                                    {
                                        selectedCustomBlendables = new MLSCustomBlendable[Selection.gameObjects.Length];

                                        for (int o = 0; o < Selection.gameObjects.Length; o++)
                                        {
                                            selectedCustomBlendables[o] = Selection.gameObjects[o].GetComponent<MLSCustomBlendable>();
                                        }
                                    }
                                    else
                                    {
                                        if (Selection.activeGameObject != null)
                                        {
                                            selectedCustomBlendable = Selection.activeGameObject.GetComponent<MLSCustomBlendable>();
                                        }
                                    }
                                }

                                if (selectedCustomBlendable == null && selectedCustomBlendables == null)
                                {
                                    GUI.enabled = false;
                                }

                                if (GUILayout.Button(selectedCustomBlendable != null ? "Add Selected Object..." : selectedCustomBlendables != null ? "Add Selected Objects..." : "No Selected Objects", GUILayout.MaxWidth(150)))
                                {
                                    if (selectedCustomBlendable != null)
                                    {
                                        AddCustomBlendableToPreset(magicLightmapSwitcher.lightingPresets, selectedCustomBlendable);
                                    }
                                    else if (selectedCustomBlendables != null)
                                    {
                                        AddCustomBlendableToPreset(magicLightmapSwitcher.lightingPresets, selectedCustomBlendables);
                                    }
                                    
                                }

                                GUI.enabled = true;
                            }

                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                if (magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings.Count == 0)
                                {
                                    EditorGUILayout.HelpBox("There are no Custom Blendable objects in the preset.", MessageType.Info);
                                }

                                for (int i = 0; i < magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings.Count; i++)
                                {
                                    if (magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].sourceScript == null)
                                    {
                                        for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                        {
                                            magicLightmapSwitcher.lightingPresets[j].customBlendablesSettings.RemoveAt(i);
                                        }

                                        continue;

                                        //magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].sourceScript =
                                        //    magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].sourceScript.UpdateScriptLink();
                                    }

                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].globalFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].globalFoldoutEnabled,
                                                    magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].sourceScript.name, true);

                                            GUILayout.FlexibleSpace();

                                            if (GUILayout.Button("Remove"))
                                            {
                                                for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[j].customBlendablesSettings.RemoveAt(i);
                                                }

                                                continue;
                                            }
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].globalFoldoutEnabled)
                                        {
                                            for (int j = 0; j < magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableCubemapParameters.Count; j++)
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableCubemapParametersValues[j] =
                                                    EditorGUILayout.ObjectField(
                                                        magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableCubemapParameters[j],
                                                        magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableCubemapParametersValues[j], 
                                                        typeof(Cubemap), false) as Cubemap;
                                            }

                                            for (int j = 0; j < magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableFloatParameters.Count; j++)
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableFloatParametersValues[j] =
                                                    EditorGUILayout.FloatField(
                                                        magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableFloatParameters[j],
                                                        magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableFloatParametersValues[j]);
                                            }

                                            for (int j = 0; j < magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableColorParameters.Count; j++)
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableColorParametersValues[j] =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent(magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableColorParameters[j]),
                                                        magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableColorParametersValues[j],
                                                        false, false, true);
                                            }
                                        }
                                    }
                                }
                            }
#endregion

                            #region Lighting Settings
                            GUILayout.Label("General Lighting", MLSEditorUtils.captionStyle);

                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.globalFoldoutEnabled =
                                        EditorGUILayout.Foldout(
                                            magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.globalFoldoutEnabled,
                                            "Skybox Settings", true);

                                if (magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.globalFoldoutEnabled)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.skyboxTexture =
                                            EditorGUILayout.ObjectField("Skybox Texture", magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.skyboxTexture, typeof(Cubemap), false) as Cubemap;
                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.exposure =
                                            EditorGUILayout.FloatField("Exposure", magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.exposure);
                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.tintColor =
                                            EditorGUILayout.ColorField("Tint", magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.tintColor);

#if BAKERY_INCLUDED
                                        BakerySkyLight bakerySkyLight = FindObjectOfType<BakerySkyLight>();

                                        if (bakerySkyLight == null)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings = null;
                                            continue;
                                        }
                                        else
                                        {
                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[lp], bakerySkyLight);
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings != null)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bakerySkylightFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bakerySkylightFoldoutEnabled,
                                                    "Bakery Sky Settings", true);

                                            if (magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bakerySkylightFoldoutEnabled)
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.color =
                                                    EditorGUILayout.ColorField(
                                                        "Color",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.color);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.intensity =
                                                    EditorGUILayout.FloatField(
                                                        "Intensity",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.intensity);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.cubemap =
                                                    EditorGUILayout.ObjectField(
                                                        "Skybox Texture", 
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.cubemap, typeof(Cubemap), false) as Cubemap;

                                                if (magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.cubemap != null)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.correctRotation =
                                                        EditorGUILayout.Toggle(
                                                            "Correct Rotation",
                                                            magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.correctRotation);
                                                }

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.samples =
                                                    EditorGUILayout.IntField(
                                                        "Samples",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.samples);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.hemispherical =
                                                    EditorGUILayout.Toggle(
                                                        "Hemispherical",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.hemispherical);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bitmask =
                                                    EditorGUILayout.IntField(
                                                        "Bitmask",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bitmask);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.indirectIntensity =
                                                    EditorGUILayout.FloatField(
                                                        "Indirect Intensty",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.indirectIntensity);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.tangentSH =
                                                    EditorGUILayout.Toggle(
                                                        "Tangent SH",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.tangentSH);
                                            }
                                        }
#endif
                                    }
                                }

                                magicLightmapSwitcher.lightingPresets[lp].fogSettings.globalFoldoutEnabled =
                                        EditorGUILayout.Foldout(
                                            magicLightmapSwitcher.lightingPresets[lp].fogSettings.globalFoldoutEnabled,
                                            "Fog Settings", true);

                                if (magicLightmapSwitcher.lightingPresets[lp].fogSettings.globalFoldoutEnabled)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        magicLightmapSwitcher.lightingPresets[lp].fogSettings.enabled =
                                            EditorGUILayout.Toggle("Fog Enabled", magicLightmapSwitcher.lightingPresets[lp].fogSettings.enabled);
                                        magicLightmapSwitcher.lightingPresets[lp].fogSettings.fogColor =
                                            EditorGUILayout.ColorField("Color", magicLightmapSwitcher.lightingPresets[lp].fogSettings.fogColor);
                                        magicLightmapSwitcher.lightingPresets[lp].fogSettings.fogDensity =
                                            EditorGUILayout.FloatField("Density", magicLightmapSwitcher.lightingPresets[lp].fogSettings.fogDensity);
                                    }
                                }

                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.globalFoldoutEnabled =
                                        EditorGUILayout.Foldout(
                                            magicLightmapSwitcher.lightingPresets[lp].environmentSettings.globalFoldoutEnabled,
                                            "Environment Settings", true);

                                if (magicLightmapSwitcher.lightingPresets[lp].environmentSettings.globalFoldoutEnabled)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        magicLightmapSwitcher.lightingPresets[lp].environmentSettings.source =
                                            (AmbientMode) EditorGUILayout.EnumPopup("Source", magicLightmapSwitcher.lightingPresets[lp].environmentSettings.source);

                                        switch (magicLightmapSwitcher.lightingPresets[lp].environmentSettings.source)
                                        {
                                            case AmbientMode.Trilight:
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.skyColor =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent("Sky Color"), magicLightmapSwitcher.lightingPresets[lp].environmentSettings.skyColor, true, false, true);
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.equatorColor =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent("Equator Color"), magicLightmapSwitcher.lightingPresets[lp].environmentSettings.equatorColor, true, false, true);
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.groundColor =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent("Ground Color"), magicLightmapSwitcher.lightingPresets[lp].environmentSettings.groundColor, true, false, true);
                                                break;
                                            case AmbientMode.Flat:
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.skyColor =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent("Ambient Color"), magicLightmapSwitcher.lightingPresets[lp].environmentSettings.skyColor, true, false, true);
                                                break;
                                            case AmbientMode.Skybox:
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.intensityMultiplier =
                                                    EditorGUILayout.Slider("Intensity Multiplier", magicLightmapSwitcher.lightingPresets[lp].environmentSettings.intensityMultiplier, 0f, 8f);
                                                break;
                                        }
                                    }
                                }

                                //magicLightmapSwitcher.lightingPresets[lp].lightmapParameters =
                                //    EditorGUILayout.ObjectField("Lightmap Parameters", magicLightmapSwitcher.lightingPresets[lp].lightmapParameters, typeof(LightmapParameters), false) as LightmapParameters;
                            }
#endregion

                            if (EditorGUI.EndChangeCheck())
                            {
                                magicLightmapSwitcher.lightingPresets[lp].MatchSceneSettings();
                            }
                        }

                        GUILayout.Space(10);

                        using (new GUILayout.HorizontalScope())
                        {
                            if (!magicLightmapSwitcher.lightingPresets[lp].foldoutEnabled)
                            {
                                GUILayout.FlexibleSpace();

                                if (GUILayout.Button("Load", GUILayout.MaxWidth(80)))
                                {
                                    magicLightmapSwitcher.lightingPresets[lp].MatchSceneSettings();
                                }

                                if (GUILayout.Button("Duplicate", GUILayout.MaxWidth(80)))
                                {
                                    DuplicatePreset(lp);
                                }

                                if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
                                {
                                    magicLightmapSwitcher.lightingPresets.RemoveAt(lp);
                                }
                            }
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        static void DrawColorTempSliderBackground(int sizeCorrection)
        {
            Rect test = new Rect(GUILayoutUtility.GetLastRect());
            GUI.Box(new Rect(test.x + 59 + sizeCorrection, test.y + 20, test.width - 20, test.height),
                CreateKelvinGradientTexture("cTemp", 367 - sizeCorrection, (int) test.height, 1000f, 20000f, 2));
        }

        static Texture2D CreateKelvinGradientTexture(string name, int width, int height, float minKelvin, float maxKelvin, float current)
        {
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true)
            {
                name = name,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[width * height];

            float mappedMax = Mathf.Pow(maxKelvin, 1f / current);
            float mappedMin = Mathf.Pow(minKelvin, 1f / current);

            for (int i = 0; i < width; i++)
            {
                float pixelfrac = i / (float)(width - 1);
                float mappedValue = (mappedMax - mappedMin) * pixelfrac + mappedMin;
                float kelvin = Mathf.Pow(mappedValue, current);
                Color kelvinColor = Mathf.CorrelatedColorTemperatureToRGB(kelvin);
                for (int j = 0; j < height; j++)
                    pixels[j * width + i] = kelvinColor.gamma;
            }

            texture.SetPixels32(pixels);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Object = UnityEngine.Object;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
using UnityEngine.Rendering.HighDefinition;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Callbacks;
#endif

namespace MagicLightmapSwitcher
{
    [ExecuteInEditMode, System.Serializable]
    public class MagicLightmapSwitcher : MonoBehaviour
    {
        #region Runtime Variables  
        public int MLS_USE_TEXTURE2D_ARRAYS = Shader.PropertyToID("MLS_TEXTURE2D_ARRAYS");
        public int MLS_USE_TEXTURECUBE_ARRAYS = Shader.PropertyToID("MLS_TEXTURECUBE_ARRAYS");

        public class DynamicRendererAddedEvent : UnityEvent<GameObject, MLSDynamicRenderer> { }
        public class DynamicRendererRemoveEvent : UnityEvent<GameObject, AffectedObject> { }

        public static DynamicRendererAddedEvent OnDynamicRendererAdded;
        public static DynamicRendererRemoveEvent OnDynamicRendererRemoved;

        [System.Serializable]
        public class BlendingValueChanged : UnityEvent<StoredLightingScenario, float, float, float> { }
        [System.Serializable]
        public class LoadedLightmapChanged : UnityEvent<StoredLightingScenario, int> { }

        [SerializeField]
        public List<BlendingValueChanged> OnBlendingValueChanged = new List<BlendingValueChanged>();
        [SerializeField]
        public List<LoadedLightmapChanged> OnLoadedLightmapChanged = new List<LoadedLightmapChanged>();

        public enum Lightmapper
        {
            UnityLightmapper,
            BakeryLightmapper
        }
        
        public enum Workflow
        {
            MultiScene,
            SingleScene
        }

        public enum BlendingState
        {
            Enabled,
            Disabled
        }

        public enum BlendingOptions
        {
            All,
            Lightmaps,
            Reflections,
            None
        }

        [System.Serializable]
        public class BakedGroup
        {
            [SerializeField]
            public GameObject rootObject;
            [SerializeField]
            public List<Light> affectedLights = new List<Light>();
            [SerializeField]
            public List<GameObject> affectedObjects = new List<GameObject>();
        }

        public class AffectedObject
        {
            public Terrain terrain;
            public MeshRenderer meshRenderer;
            public ReflectionProbeUsage reflectionProbeUsage;
            public Material material;
            public int materialsCount;
            public bool isStatic;
            public string objectId;
            public MaterialPropertyBlock _propBlock;
            public List<ReflectionProbeBlendInfo> reflectionProbeBlendInfo = new List<ReflectionProbeBlendInfo>();
            public int lastFromIndex = -1;

            public void InitPropertyBlock()
            {
#if UNITY_EDITOR
                if (_propBlock == null)
                {
                    _propBlock = new MaterialPropertyBlock();
                }

                if (meshRenderer != null)
                {
                    meshRenderer.GetPropertyBlock(_propBlock);
                }
                else if (terrain != null)
                {
                    terrain.GetSplatMaterialPropertyBlock(_propBlock);
                }
#endif
            }

            public void SetShaderFloat(string property, float value)
            {
                if (Application.isPlaying)
                {
                    if (meshRenderer != null)
                    {
                        if (materialsCount > 1)
                        {
                            for (int i = 0; i < materialsCount; i++)
                            {
                                if (meshRenderer.materials[i].name.StartsWith("Outline", System.StringComparison.Ordinal))
                                {
                                    continue;
                                }

                                meshRenderer.materials[i].SetFloat(property, value);
                            }
                        }
                        else
                        {
                            meshRenderer.material.SetFloat(property, value);
                        }
                    }
                    else if (terrain != null)
                    {
                        terrain.materialTemplate.SetFloat(property, value);
                    }
                }
                else
                {
                    _propBlock.SetFloat(property, value);
                }
            }

            public void SetShaderFloat(int nameID, float value)
            {
                if (Application.isPlaying)
                {
                    if (meshRenderer != null)
                    {
                        if (materialsCount > 1)
                        {
                            for (int i = 0; i < materialsCount; i++)
                            {
                                if (meshRenderer.materials[i].name.StartsWith("Outline", System.StringComparison.Ordinal))
                                {
                                    continue;
                                }
                
                                meshRenderer.materials[i].SetFloat(nameID, value);
                            }
                        }
                        else
                        {
                            meshRenderer.material.SetFloat(nameID, value);
                        }
                    }
                    else if (terrain != null)
                    {
                        terrain.materialTemplate.SetFloat(nameID, value);
                    }
                }
                else
                {
                    _propBlock.SetFloat(nameID, value);
                }
            }

            public void SetShaderInt(string property, int value)
            {
                if (Application.isPlaying)
                {
                    if (meshRenderer != null)
                    {
                        if (materialsCount > 1)
                        {
                            for (int i = 0; i < materialsCount; i++)
                            {
                                if (meshRenderer.materials[i].name.StartsWith("Outline", System.StringComparison.Ordinal))
                                {
                                    continue;
                                }

                                meshRenderer.materials[i].SetInt(property, value);
                            }
                        }
                        else
                        {
                            meshRenderer.material.SetInt(property, value);
                        }
                    }
                    else if (terrain != null)
                    {
                        terrain.materialTemplate.SetInt(property, value);
                    }
                }
                else
                {
                    _propBlock.SetFloat(property, value);
                }
            }

            public void SetShaderInt(int nameID, int value)
            {
                if (Application.isPlaying)
                {
                    if (meshRenderer != null)
                    {
                        if (materialsCount > 1)
                        {
                            for (int i = 0; i < materialsCount; i++)
                            {
                                if (meshRenderer.materials[i].name.StartsWith("Outline", System.StringComparison.Ordinal))
                                {
                                    continue;
                                }
                
                                meshRenderer.materials[i].SetInt(nameID, value);
                            }
                        }
                        else
                        {
                            meshRenderer.material.SetInt(nameID, value);
                        }
                    }
                    else if (terrain != null)
                    {
                        terrain.materialTemplate.SetInt(nameID, value);
                    }
                }
                else
                {
                    _propBlock.SetFloat(nameID, value);
                }
            }

            public void SetShaderTexture(string property, Texture value)
            {
                if (Application.isPlaying)
                {
                    if (meshRenderer != null)
                    {
                        if (materialsCount > 1)
                        {
                            for (int i = 0; i < materialsCount; i++)
                            {
                                if (meshRenderer.materials[i].name.StartsWith("Outline", System.StringComparison.Ordinal))
                                {
                                    continue;
                                }

                                meshRenderer.materials[i].SetTexture(property, value);
                            }
                        }
                        else
                        {
                            meshRenderer.material.SetTexture(property, value);
                        }
                    }
                    else if (terrain != null)
                    {
                        terrain.materialTemplate.SetTexture(property, value);
                    }
                }
                else
                {
                    _propBlock.SetTexture(property, value);
                }
            }

            public void SetShaderTexture(int nameID, Texture value)
            {
                if (Application.isPlaying)
                {
                    if (meshRenderer != null)
                    {
                        if (materialsCount > 1)
                        {
                            for (int i = 0; i < materialsCount; i++)
                            {
                                if (meshRenderer.materials[i].name.StartsWith("Outline", System.StringComparison.Ordinal))
                                {
                                    continue;
                                }
                
                                meshRenderer.materials[i].SetTexture(nameID, value);
                            }
                        }
                        else
                        {
                            meshRenderer.material.SetTexture(nameID, value);
                        }
                    }
                    else if (terrain != null)
                    {
                        terrain.materialTemplate.SetTexture(nameID, value);
                    }
                }
                else
                {
                    if (_propBlock != null)
                    {
                        _propBlock.SetTexture(nameID, value);
                    }
                }
            }

            public void ApplyPropertyBlock()
            {
#if UNITY_EDITOR
                if (_propBlock != null)
                {
                    if (meshRenderer != null)
                    {
                        meshRenderer.SetPropertyBlock(_propBlock);
                    }
                    else if (terrain != null)
                    {
                        terrain.SetSplatMaterialPropertyBlock(_propBlock);
                    }
                }
#endif
            }
        }

        #region Multi-Scene Workflow
        public Dictionary<string, List<AffectedObject>> staticAffectedObjects = new Dictionary<string, List<AffectedObject>>();
        public Dictionary<string, List<AffectedObject>> dynamicAffectedObjects = new Dictionary<string, List<AffectedObject>>();
        public Dictionary<string, List<StoredLightmapData>> orderedStoredLightmapDatas = new Dictionary<string, List<StoredLightmapData>>();
        public Dictionary<string, List<StoredLightmapData>> storedLightmapDatas = new Dictionary<string, List<StoredLightmapData>>();
        public Dictionary<string, List<StoredLightingScenario>> storedLightmapScenarios = new Dictionary<string, List<StoredLightingScenario>>();
        public Dictionary<string, List<MLSLight>> storedLights = new Dictionary<string, List<MLSLight>>();
        public Dictionary<string, List<ReflectionProbe>> storedReflectionProbes = new Dictionary<string, List<ReflectionProbe>>();
        #endregion

        #region Single Scene Workflow
        public List<AffectedObject> sceneStaticAffectedObjects = new List<AffectedObject>();
        public List<AffectedObject> sceneDynamicAffectedObjects = new List<AffectedObject>();
        public List<StoredLightmapData> sceneLightmapDatas = new List<StoredLightmapData>();
        public List<StoredLightingScenario> sceneLightmapScenarios = new List<StoredLightingScenario>();
        public List<MLSLight> sceneAffectedLightSources = new List<MLSLight>();
        public List<ReflectionProbe> sceneReflectionProbes = new List<ReflectionProbe>();
        public List<Vector3> sceneReflectionProbePositions = new List<Vector3>();
        #endregion

        public List<BakedGroup> bakedGroup = new List<BakedGroup>();
        public List<StoredLightingScenario> availableScenarios = new List<StoredLightingScenario>();
        public StoredLightingScenario currentLightmapScenario;
        public StoredLightingScenario lastLightmapScenario;

        public Lightmapper lightmapper = Lightmapper.UnityLightmapper;
        public Workflow workflow;        
        public Workflow lastWorkflow;
        public BlendingOptions currentBlendingState;
        private string workPath;
        public string currentDataPath;
        public bool loadFromAssetBundles;
        public int storedAssetsCount;
        public static Cubemap defaultCubeBlack;
        public bool resetAffectedObjects;
        public SystemProperties systemProperties;
        private int lastSceneCount;
        public int lastSelectedScene;
        public string sceneToUnload;
        public string lastLoadedscene;
        public bool storedDataUpdated = false;
        public bool storedDataUpdatingProcess = false;
        public bool lightingDataSwitching;
        public bool stopProbesBlending;
        public bool lightmapArrayInitialized;
        public bool cubemapArrayInitialized;
        public bool useTexture2DArrays;
        public bool useTextureCubeArrays;
        #endregion

#if UNITY_EDITOR
        #region Editor Variables 
        public SerializedObject switcherSerializedObject;

        public enum StoringMode
        {
            Once,
            Queue
        }               

        [System.Serializable]
        public class SceneLightingPreset
        {
            [System.Serializable]
            public class LightSourceSettings
            {
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                public HDAdditionalLightData additionalLightData;
#endif
                public Light light;
                public MLSLight mlsLight;
                public string mlsLightUID;
                public LightType lightType;
                public Vector3 position;
                public Vector3 rotation;
                public Color color;
                public float colorTemperature;
                public float intensity;
                public float indirectMultiplier;
                public float range;
                public float radius;
                public float spotOuterAngle;
                public LightShadows shadowsType;
                public float shadowStrength;
                public float spotInnerAngle;
                public float bakedShadowsRadius;
                public float directionalBakedShadowAngle;
                public float areaWidth;
                public float areaHeight;
                public bool globalFoldoutEnabled;
                public bool transformFoldoutEnabled;
                public bool settingsFoldoutEnabled;
                public bool justAdded = true;

#if BAKERY_INCLUDED
                [System.Serializable]
                public class BakeryDirectLightsSettings
                {
                    public GameObject parentGameObject;
                    public BakeryDirectLight bakeryDirect;
                    public int UID;
                    public Color color;
                    public float intensity;
                    public float shadowSpread;
                    public int samples;
                    public int bitmask;
                    public bool bakeToIndirect;
                    public bool shadowmask;
                    public bool shadowmaskDenoise;
                    public float indirectIntensity;
                    public Texture2D cloudShadow;
                    public float cloudShadowTilingX;
                    public float cloudShadowTilingY;
                    public float cloudShadowOffsetX, cloudShadowOffsetY;
                    public bool bakeryDirectFoldoutEnabled;
                }

                [System.Serializable]
                public class BakeryPointLightsSettings
                {
                    public GameObject parentGameObject;
                    public BakeryPointLight bakeryPoint;
                    public int UID;
                    public Color color;
                    public float intensity;
                    public float shadowSpread;
                    public float cutoff;
                    public bool realisticFalloff;
                    public int samples;
                    public BakeryPointLight.ftLightProjectionMode projMode;
                    public Texture2D cookie;
                    public float angle;
                    public float innerAngle;
                    public Cubemap cubemap;
                    public UnityEngine.Object iesFile;
                    public int bitmask;
                    public bool bakeToIndirect;
                    public bool shadowmask;
                    public float indirectIntensity;
                    public float falloffMinRadius;
                    public float screenRadius;
                    public bool bakeryPointFoldoutEnabled;
                }

                [System.Serializable]
                public class BakeryLightMeshesSettings
                {
                    public GameObject parentGameObject;
                    public BakeryLightMesh bakeryLightMesh;
                    public int UID;
                    public List<MeshFilter> All;
                    public Color color;
                    public float intensity;
                    public Texture2D texture;
                    public float cutoff;
                    public int samples;
                    public int samples2;
                    public int bitmask;
                    public bool selfShadow;
                    public bool bakeToIndirect;
                    public float indirectIntensity;
                    public int lmid;
                    public bool bakeryLightMeshFoldoutEnabled;
                }                

                [SerializeField]
                public BakeryDirectLightsSettings bakeryDirectLightsSettings;
                [SerializeField]
                public BakeryPointLightsSettings bakeryPointLightsSettings;
                [SerializeField]
                public BakeryLightMeshesSettings bakeryLightMeshesSettings;                
#endif
            }

            [System.Serializable]
            public class GameObjectSettings
            {
                [SerializeField]
                public GameObject gameObject;
                [SerializeField]
                public int instanceId;
                [SerializeField]
                public bool enabled;
                [SerializeField]
                public bool justAdded = true;
                [SerializeField]
                public Vector3 position;
                [SerializeField]
                public Quaternion rotation;
                [SerializeField]
                public Vector3 tempRotation;
                [SerializeField]
                public bool globalFoldoutEnabled;
                [SerializeField]
                public bool transformFoldoutEnabled;
            }

            [System.Serializable]
            public class FogSettings
            {
                [SerializeField]
                public bool enabled;
                [SerializeField]
                public Color fogColor;
                [SerializeField]
                public float fogDensity;
                [SerializeField]
                public bool globalFoldoutEnabled;

                public FogSettings() { }

                public FogSettings(FogSettings source)
                {
                    this.enabled = source.enabled;
                    this.fogColor = source.fogColor;
                    this.fogDensity = source.fogDensity;
                }
            }

            [System.Serializable]
            public class CustomBlendablesSettings
            {
                public CustomBlendablesSettings(CustomBlendablesSettings copyFrom = null)
                {
                    if (copyFrom != null)
                    {
                        sourceScript = copyFrom.sourceScript;
                        sourceScriptId = copyFrom.sourceScriptId;

                        List<string> blendableFloatParametersClone = new List<string>(copyFrom.blendableFloatParameters.Count);
                        List<float> blendableFloatParametersValuesClone = new List<float>(copyFrom.blendableFloatParametersValues.Count);
                        List<string> blendableCubemapParametersClone = new List<string>(copyFrom.blendableCubemapParameters.Count);
                        List<Cubemap> blendableCubemapParametersValuesClone = new List<Cubemap>(copyFrom.blendableCubemapParametersValues.Count);
                        List<string> blendableColorParametersClone = new List<string>(copyFrom.blendableColorParameters.Count);
                        List<Color> blendableColorParametersValuesClone = new List<Color>(copyFrom.blendableColorParametersValues.Count);

                        copyFrom.blendableFloatParameters.ForEach((item) =>
                        {
                            blendableFloatParametersClone.Add(item);
                        });                       
                        
                        copyFrom.blendableFloatParametersValues.ForEach((item) =>
                        {
                            blendableFloatParametersValuesClone.Add(item);
                        });

                        copyFrom.blendableCubemapParameters.ForEach((item) =>
                        {
                            blendableCubemapParametersClone.Add(item);
                        });

                        copyFrom.blendableCubemapParametersValues.ForEach((item) =>
                        {
                            blendableCubemapParametersValuesClone.Add(item);
                        });

                        copyFrom.blendableColorParameters.ForEach((item) =>
                        {
                            blendableColorParametersClone.Add(item);
                        });

                        copyFrom.blendableColorParametersValues.ForEach((item) =>
                        {
                            blendableColorParametersValuesClone.Add(item);
                        });

                        blendableFloatParameters = blendableFloatParametersClone;
                        blendableFloatParametersValues = blendableFloatParametersValuesClone;
                        blendableCubemapParameters = blendableCubemapParametersClone;
                        blendableCubemapParametersValues = blendableCubemapParametersValuesClone;
                        blendableColorParameters = blendableColorParametersClone;
                        blendableColorParametersValues = blendableColorParametersValuesClone;
                        globalFoldoutEnabled = copyFrom.globalFoldoutEnabled;
                    }
                }

                [SerializeField]
                public MLSCustomBlendable sourceScript;
                [SerializeField]
                public string sourceScriptId;
                [SerializeField]
                public List<string> blendableFloatParameters = new List<string>();
                [SerializeField]
                public List<float> blendableFloatParametersValues = new List<float>();
                [SerializeField]
                public List<string> blendableCubemapParameters = new List<string>();
                [SerializeField]
                public List<Cubemap> blendableCubemapParametersValues = new List<Cubemap>();
                [SerializeField]
                public List<string> blendableColorParameters = new List<string>();
                [SerializeField]
                public List<Color> blendableColorParametersValues = new List<Color>();
                [SerializeField]
                public bool globalFoldoutEnabled;
            }

            [System.Serializable]
            public class SkyboxSettings
            {
                public SkyboxSettings(SkyboxSettings copyFrom = null)
                {
                    if (copyFrom != null)
                    {
                        skyboxTexture = copyFrom.skyboxTexture;
                        exposure = copyFrom.exposure;
                        tintColor = copyFrom.tintColor;
                        globalFoldoutEnabled = copyFrom.globalFoldoutEnabled;
                    }
                }
                
                [SerializeField]
                public Cubemap skyboxTexture;
                [SerializeField]
                public float exposure;
                [SerializeField]
                public Color tintColor;
                [SerializeField]
                public bool globalFoldoutEnabled;

#if BAKERY_INCLUDED
                [System.Serializable]
                public class BakerySkyLightsSettings
                {
                    public GameObject parentGameObject;
                    public BakerySkyLight bakerySky;
                    public int UID;
                    public string texName;
                    public Color color;
                    public float intensity;
                    public int samples;
                    public bool hemispherical;
                    public int bitmask;
                    public bool bakeToIndirect;
                    public float indirectIntensity;
                    public bool tangentSH;
                    public Cubemap cubemap;
                    public bool correctRotation = false;                    
                    public Quaternion cubemapAngles;
                    public bool bakerySkylightFoldoutEnabled;
                }

                [SerializeField]
                public BakerySkyLightsSettings bakerySkyLightsSettings = null;
#endif
            }

            [System.Serializable]
            public class EnvironmentSettings
            {
                [SerializeField]
                public AmbientMode source;
                [SerializeField]
                public Color ambientColor;
                [SerializeField]
                public Color skyColor;
                [SerializeField]
                public Color equatorColor;
                [SerializeField]
                public Color groundColor;
                [SerializeField]
                public float intensityMultiplier;
                [SerializeField]
                public bool globalFoldoutEnabled;

                public EnvironmentSettings() { }

                public EnvironmentSettings(EnvironmentSettings source)
                {
                    this.source = source.source;
                    this.ambientColor = source.ambientColor;
                    this.skyColor = source.skyColor;
                    this.equatorColor = source.equatorColor;
                    this.groundColor = source.groundColor;
                    this.intensityMultiplier = source.intensityMultiplier;
                    this.globalFoldoutEnabled = source.globalFoldoutEnabled;
                }
            }

            [SerializeField]
            public string name;
            [SerializeField]
            public bool included = true;
            [SerializeField]
            public List<LightSourceSettings> lightSourceSettings = new List<LightSourceSettings>();
            [SerializeField]
            public List<CustomBlendablesSettings> customBlendablesSettings = new List<CustomBlendablesSettings>();
            [SerializeField]
            public List<GameObjectSettings> gameObjectsSettings = new List<GameObjectSettings>();
            [SerializeField]
            public SkyboxSettings skyboxSettings = new SkyboxSettings();
            [SerializeField]
            public EnvironmentSettings environmentSettings = new EnvironmentSettings();
            [SerializeField]
            public FogSettings fogSettings = new FogSettings();
            [SerializeField]
            public LightmapParameters lightmapParameters;
            [SerializeField]
            public bool foldoutEnabled;

#if BAKERY_INCLUDED
            [SerializeField]
            public List<LightSourceSettings.BakeryLightMeshesSettings> bakeryLightMeshesSettings = new List<LightSourceSettings.BakeryLightMeshesSettings>();
#endif

            public void MatchSceneSettings()
            {
                Shader.SetGlobalInt("_MLS_ENABLE_LIGHTMAPS_BLENDING", 0);
                Shader.SetGlobalInt("_MLS_ENABLE_REFLECTIONS_BLENDING", 0);
                Shader.SetGlobalInt("_MLS_ENABLE_SKY_CUBEMAPS_BLENDING", 0);

                GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                for (int i = 0; i < gameObjects.Length; i++)
                {
                    GameObjectSettings currentSettings = gameObjectsSettings.Find(item => item.gameObject == gameObjects[i]);

                    if (currentSettings != null)
                    {
                        gameObjects[i].SetActive(currentSettings.enabled);
                        gameObjects[i].transform.localPosition = currentSettings.position;
                        gameObjects[i].transform.rotation = currentSettings.rotation;
                    }
                }

                Light[] sceneLights = FindObjectsOfType<Light>();

                for (int i = 0; i < sceneLights.Length; i++)
                {                    
                    LightSourceSettings currentSettings = lightSourceSettings.Find(item => item.light == sceneLights[i]);

                    if (currentSettings != null)
                    {
                        if (!currentSettings.mlsLight.editedDirectly)
                        {
                            if (sceneLights[i].transform != null)
                            {
                                sceneLights[i].transform.localPosition = currentSettings.position;
                            }
                            else
                            {
                                sceneLights[i].transform.position = currentSettings.position;
                            }

                            TransformUtils.SetInspectorRotation(sceneLights[i].transform, currentSettings.rotation);
                            sceneLights[i].color = currentSettings.color;
                            sceneLights[i].colorTemperature = currentSettings.colorTemperature;
                            sceneLights[i].range = currentSettings.range;
                            sceneLights[i].spotAngle = currentSettings.spotOuterAngle;
                            sceneLights[i].innerSpotAngle = currentSettings.spotInnerAngle;
                            sceneLights[i].areaSize = new Vector2(currentSettings.areaWidth, currentSettings.areaHeight);
                            sceneLights[i].bounceIntensity = currentSettings.indirectMultiplier;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                            if (currentSettings.additionalLightData != null)
                            {
                                sceneLights[i].GetComponent<HDAdditionalLightData>().intensity = currentSettings.intensity;
                                sceneLights[i].GetComponent<HDAdditionalLightData>().type = currentSettings.additionalLightData.type;
                            }
#else                            
                            sceneLights[i].type = currentSettings.lightType;
                            sceneLights[i].intensity = currentSettings.intensity;
                            sceneLights[i].shadowRadius = currentSettings.bakedShadowsRadius;
                            sceneLights[i].shadowAngle = currentSettings.directionalBakedShadowAngle;
                            sceneLights[i].shadows = currentSettings.shadowsType;
                            sceneLights[i].shadowStrength = currentSettings.shadowStrength;
#endif

#if BAKERY_INCLUDED
                            if (currentSettings.bakeryDirectLightsSettings != null)
                            {
                                if (currentSettings.bakeryDirectLightsSettings.bakeryDirect != null)
                                {
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.color = currentSettings.color;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.intensity = currentSettings.intensity;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.shadowSpread = currentSettings.bakeryDirectLightsSettings.shadowSpread;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.samples = currentSettings.bakeryDirectLightsSettings.samples;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.bitmask = currentSettings.bakeryDirectLightsSettings.bitmask;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.bakeToIndirect = currentSettings.bakeryDirectLightsSettings.bakeToIndirect;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.shadowmask = currentSettings.bakeryDirectLightsSettings.shadowmask;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.shadowmaskDenoise = currentSettings.bakeryDirectLightsSettings.shadowmaskDenoise;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.indirectIntensity = currentSettings.bakeryDirectLightsSettings.indirectIntensity;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadow = currentSettings.bakeryDirectLightsSettings.cloudShadow;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadowTilingX = currentSettings.bakeryDirectLightsSettings.cloudShadowTilingX;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadowTilingY = currentSettings.bakeryDirectLightsSettings.cloudShadowTilingY;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadowOffsetX = currentSettings.bakeryDirectLightsSettings.cloudShadowOffsetX;
                                    currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadowOffsetY = currentSettings.bakeryDirectLightsSettings.cloudShadowOffsetY;

                                    //EditorUtility.SetDirty(currentSettings.bakeryDirectLightsSettings.bakeryDirect);
                                }
                                else
                                {
                                    currentSettings.bakeryDirectLightsSettings = null;
                                }
                            }

                            if (currentSettings.bakeryPointLightsSettings != null)
                            {
                                if (currentSettings.bakeryPointLightsSettings.bakeryPoint != null)
                                {
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.color = currentSettings.color;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.intensity = currentSettings.intensity;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.shadowSpread = currentSettings.bakeryPointLightsSettings.shadowSpread;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.cutoff = currentSettings.bakeryPointLightsSettings.cutoff;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.realisticFalloff = currentSettings.bakeryPointLightsSettings.realisticFalloff;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.samples = currentSettings.bakeryPointLightsSettings.samples;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.projMode = currentSettings.bakeryPointLightsSettings.projMode;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.cookie = currentSettings.bakeryPointLightsSettings.cookie;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.angle = currentSettings.bakeryPointLightsSettings.angle;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.innerAngle = currentSettings.bakeryPointLightsSettings.innerAngle;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.cubemap = currentSettings.bakeryPointLightsSettings.cubemap;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.iesFile = currentSettings.bakeryPointLightsSettings.iesFile;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.bitmask = currentSettings.bakeryPointLightsSettings.bitmask;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.bakeToIndirect = currentSettings.bakeryPointLightsSettings.bakeToIndirect;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.shadowmask = currentSettings.bakeryPointLightsSettings.shadowmask;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.indirectIntensity = currentSettings.bakeryPointLightsSettings.indirectIntensity;
                                    currentSettings.bakeryPointLightsSettings.bakeryPoint.falloffMinRadius = currentSettings.bakeryPointLightsSettings.falloffMinRadius;

                                    EditorUtility.SetDirty(currentSettings.bakeryPointLightsSettings.bakeryPoint);
                                }
                                else
                                {
                                    currentSettings.bakeryPointLightsSettings = null;
                                }
                            }

                            if (currentSettings.bakeryLightMeshesSettings != null)
                            {
                                if (currentSettings.bakeryLightMeshesSettings.bakeryLightMesh != null)
                                {
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.color = currentSettings.color;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.intensity = currentSettings.intensity;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.texture = currentSettings.bakeryLightMeshesSettings.texture;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.cutoff = currentSettings.bakeryLightMeshesSettings.cutoff;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.samples = currentSettings.bakeryLightMeshesSettings.samples;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.samples2 = currentSettings.bakeryLightMeshesSettings.samples2;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.bitmask = currentSettings.bakeryLightMeshesSettings.bitmask;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.selfShadow = currentSettings.bakeryLightMeshesSettings.selfShadow;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.bakeToIndirect = currentSettings.bakeryLightMeshesSettings.bakeToIndirect;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.indirectIntensity = currentSettings.bakeryLightMeshesSettings.indirectIntensity;
                                    currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.lmid = currentSettings.bakeryLightMeshesSettings.lmid;

                                    //EditorUtility.SetDirty(currentSettings.bakeryLightMeshesSettings.bakeryLightMesh);
                                }
                                else
                                {
                                    currentSettings.bakeryLightMeshesSettings = null;
                                }
                            }
#endif
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("Magic Lightmap Switcher", 
                                "The light source has settings unaccounted for by the preset manager. Update settings?", "Update", "Revert"))
                            {
                                currentSettings.mlsLight.editedDirectly = false;
                                UpdatePresetData();
                            }
                        }
                    }
                }

                if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Tex"))
                {
                    RenderSettings.skybox.SetTexture("_Tex", skyboxSettings.skyboxTexture);
                    RenderSettings.skybox.SetFloat("_Exposure", skyboxSettings.exposure);

                    if (RenderSettings.skybox.HasProperty("_Tint"))
                    {
                        RenderSettings.skybox.SetColor("_Tint", skyboxSettings.tintColor);
                    }
                    else if (RenderSettings.skybox.HasProperty("_SkyTint"))
                    {
                        RenderSettings.skybox.SetColor("_SkyTint", skyboxSettings.tintColor);
                    }
                }

                RenderSettings.fog = fogSettings.enabled;
                RenderSettings.fogColor = fogSettings.fogColor;
                RenderSettings.fogDensity = fogSettings.fogDensity;

                RenderSettings.ambientMode = environmentSettings.source;
                RenderSettings.ambientIntensity = environmentSettings.intensityMultiplier;
                RenderSettings.ambientLight = environmentSettings.ambientColor;
                RenderSettings.ambientSkyColor = environmentSettings.skyColor;
                RenderSettings.ambientEquatorColor = environmentSettings.equatorColor;
                RenderSettings.ambientGroundColor = environmentSettings.groundColor;

#if BAKERY_INCLUDED
                if (skyboxSettings.bakerySkyLightsSettings != null)
                {
                    if (skyboxSettings.bakerySkyLightsSettings.bakerySky != null)
                    {
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.texName = skyboxSettings.bakerySkyLightsSettings.texName;
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.color = skyboxSettings.bakerySkyLightsSettings.color;
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.intensity = skyboxSettings.bakerySkyLightsSettings.intensity;
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.samples = skyboxSettings.bakerySkyLightsSettings.samples;
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.hemispherical = skyboxSettings.bakerySkyLightsSettings.hemispherical;
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.bitmask = skyboxSettings.bakerySkyLightsSettings.bitmask;
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.bakeToIndirect = skyboxSettings.bakerySkyLightsSettings.bakeToIndirect;
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.indirectIntensity = skyboxSettings.bakerySkyLightsSettings.indirectIntensity;
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.tangentSH = skyboxSettings.bakerySkyLightsSettings.tangentSH;
                        skyboxSettings.bakerySkyLightsSettings.bakerySky.cubemap = skyboxSettings.bakerySkyLightsSettings.cubemap;

                        EditorUtility.SetDirty(skyboxSettings.bakerySkyLightsSettings.bakerySky);
                    }
                    else
                    {
                        skyboxSettings.bakerySkyLightsSettings = null;
                    }
                }

                BakeryLightMesh[] bakeryLightMeshes = FindObjectsOfType<BakeryLightMesh>();

                for (int i = 0; i < bakeryLightMeshes.Length; i++)
                {
                    LightSourceSettings.BakeryLightMeshesSettings currentSettings = bakeryLightMeshesSettings.Find(item => item.bakeryLightMesh == bakeryLightMeshes[i]);

                    if (currentSettings != null)
                    {
                        currentSettings.bakeryLightMesh.color = currentSettings.color;
                        currentSettings.bakeryLightMesh.intensity = currentSettings.intensity;
                        currentSettings.bakeryLightMesh.texture = currentSettings.texture;
                        currentSettings.bakeryLightMesh.cutoff = currentSettings.cutoff;
                        currentSettings.bakeryLightMesh.samples = currentSettings.samples;
                        currentSettings.bakeryLightMesh.samples2 = currentSettings.samples2;
                        currentSettings.bakeryLightMesh.bitmask = currentSettings.bitmask;
                        currentSettings.bakeryLightMesh.selfShadow = currentSettings.selfShadow;
                        currentSettings.bakeryLightMesh.bakeToIndirect = currentSettings.bakeToIndirect;
                        currentSettings.bakeryLightMesh.indirectIntensity = currentSettings.indirectIntensity;
                        currentSettings.bakeryLightMesh.lmid = currentSettings.lmid;

                        EditorUtility.SetDirty(currentSettings.bakeryLightMesh);
                    }
                }
#endif

                MLSCustomBlendable[] customBlendables = FindObjectsOfType<MLSCustomBlendable>();

                for (int i = 0; i < customBlendables.Length; i++)
                {
                    CustomBlendablesSettings currentSettings = customBlendablesSettings.Find(item => item.sourceScriptId == customBlendables[i].sourceScriptId);

                    if (currentSettings != null)
                    {
                        if (customBlendables[i].blendableFloatFields.Count == 0 || customBlendables[i].blendableCubemapParameters.Count == 0)
                        {
                            customBlendables[i].GetSharedParameters();
                        }

                        SerializedObject serializedObject = new SerializedObject(currentSettings.sourceScript);

                        for (int k = 0; k < currentSettings.blendableFloatParameters.Count; k++)
                        {
                            SerializedProperty floatProperty = serializedObject.FindProperty(currentSettings.blendableFloatParameters[k]);
                            floatProperty.floatValue = currentSettings.blendableFloatParametersValues[k];
                        }

                        for (int k = 0; k < currentSettings.blendableCubemapParameters.Count; k++)
                        {
                            SerializedProperty cubemapProperty = serializedObject.FindProperty(currentSettings.blendableCubemapParameters[k]);
                            cubemapProperty.objectReferenceValue = currentSettings.blendableCubemapParametersValues[k];
                        }

                        for (int k = 0; k < currentSettings.blendableColorParameters.Count; k++)
                        {
                            SerializedProperty cubemapProperty = serializedObject.FindProperty(currentSettings.blendableColorParameters[k]);
                            cubemapProperty.colorValue = currentSettings.blendableColorParametersValues[k];
                        }

                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            public void UpdatePresetData()
            {
                Shader.SetGlobalInt("_MLS_ENABLE_LIGHTMAPS_BLENDING", 0);
                Shader.SetGlobalInt("_MLS_ENABLE_REFLECTIONS_BLENDING", 0);
                Shader.SetGlobalInt("_MLS_ENABLE_SKY_CUBEMAPS_BLENDING", 0);

                GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                for (int i = 0; i < gameObjects.Length; i++)
                {
                    GameObjectSettings currentSettings = gameObjectsSettings.Find(item => item.gameObject == gameObjects[i]);

                    if (currentSettings != null)
                    {
                        currentSettings.enabled = gameObjects[i].activeSelf;
                        currentSettings.position = gameObjects[i].transform.localPosition;
                        currentSettings.rotation = gameObjects[i].transform.rotation;
                        currentSettings.tempRotation = TransformUtils.GetInspectorRotation(gameObjects[i].transform);
                    }
                }

                Light[] sceneLights = FindObjectsOfType<Light>();

                for (int i = 0; i < sceneLights.Length; i++)
                {
                    LightSourceSettings currentSettings = lightSourceSettings.Find(item => item.light == sceneLights[i]);

                    if (currentSettings != null)
                    {
                        currentSettings.mlsLight.editedDirectly = false;                        
                        currentSettings.light = sceneLights[i];

                        if (sceneLights[i].transform.parent != null)
                        {
                            currentSettings.position = sceneLights[i].transform.localPosition;
                        }
                        else
                        {
                            currentSettings.position = sceneLights[i].transform.position;
                        }

                        currentSettings.rotation = TransformUtils.GetInspectorRotation(sceneLights[i].transform);
                        currentSettings.color = sceneLights[i].color;
                        currentSettings.colorTemperature = sceneLights[i].colorTemperature;
                        currentSettings.intensity = sceneLights[i].intensity;
                        currentSettings.range = sceneLights[i].range;
                        currentSettings.spotOuterAngle = sceneLights[i].spotAngle;
                        currentSettings.spotInnerAngle = sceneLights[i].innerSpotAngle;
                        currentSettings.areaWidth = sceneLights[i].areaSize.x;
                        currentSettings.areaHeight = sceneLights[i].areaSize.y;
                        currentSettings.indirectMultiplier = sceneLights[i].bounceIntensity;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                        currentSettings.intensity = sceneLights[i].GetComponent<HDAdditionalLightData>().intensity;
                        currentSettings.additionalLightData.type = sceneLights[i].GetComponent<HDAdditionalLightData>().type;
#else
                        currentSettings.intensity = sceneLights[i].intensity;
                        currentSettings.lightType = sceneLights[i].type;
                        currentSettings.bakedShadowsRadius = sceneLights[i].shadowRadius;
                        currentSettings.directionalBakedShadowAngle = sceneLights[i].shadowAngle;
                        currentSettings.shadowsType = sceneLights[i].shadows;
                        currentSettings.shadowStrength = sceneLights[i].shadowStrength;
#endif

#if BAKERY_INCLUDED
                        if (currentSettings.bakeryDirectLightsSettings != null && currentSettings.bakeryDirectLightsSettings.bakeryDirect != null)
                        {
                            currentSettings.bakeryDirectLightsSettings.color = currentSettings.bakeryDirectLightsSettings.bakeryDirect.color;
                            currentSettings.bakeryDirectLightsSettings.intensity = currentSettings.bakeryDirectLightsSettings.bakeryDirect.intensity;
                            currentSettings.bakeryDirectLightsSettings.shadowSpread = currentSettings.bakeryDirectLightsSettings.bakeryDirect.shadowSpread;
                            currentSettings.bakeryDirectLightsSettings.samples = currentSettings.bakeryDirectLightsSettings.bakeryDirect.samples;
                            currentSettings.bakeryDirectLightsSettings.bitmask = currentSettings.bakeryDirectLightsSettings.bakeryDirect.bitmask;
                            currentSettings.bakeryDirectLightsSettings.bakeToIndirect = currentSettings.bakeryDirectLightsSettings.bakeryDirect.bakeToIndirect;
                            currentSettings.bakeryDirectLightsSettings.shadowmask = currentSettings.bakeryDirectLightsSettings.bakeryDirect.shadowmask;
                            currentSettings.bakeryDirectLightsSettings.shadowmaskDenoise = currentSettings.bakeryDirectLightsSettings.bakeryDirect.shadowmaskDenoise;
                            currentSettings.bakeryDirectLightsSettings.indirectIntensity = currentSettings.bakeryDirectLightsSettings.bakeryDirect.indirectIntensity;
                            currentSettings.bakeryDirectLightsSettings.cloudShadow = currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadow;
                            currentSettings.bakeryDirectLightsSettings.cloudShadowTilingX = currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadowTilingX;
                            currentSettings.bakeryDirectLightsSettings.cloudShadowTilingY = currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadowTilingY;
                            currentSettings.bakeryDirectLightsSettings.cloudShadowOffsetX = currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadowOffsetX;
                            currentSettings.bakeryDirectLightsSettings.cloudShadowOffsetY = currentSettings.bakeryDirectLightsSettings.bakeryDirect.cloudShadowOffsetY;
                        }

                        if (currentSettings.bakeryPointLightsSettings != null && currentSettings.bakeryPointLightsSettings.bakeryPoint != null)
                        {
                            currentSettings.bakeryPointLightsSettings.color = currentSettings.bakeryPointLightsSettings.bakeryPoint.color;
                            currentSettings.bakeryPointLightsSettings.intensity = currentSettings.bakeryPointLightsSettings.bakeryPoint.intensity;
                            currentSettings.bakeryPointLightsSettings.shadowSpread = currentSettings.bakeryPointLightsSettings.bakeryPoint.shadowSpread;
                            currentSettings.bakeryPointLightsSettings.cutoff = currentSettings.bakeryPointLightsSettings.bakeryPoint.cutoff;
                            currentSettings.bakeryPointLightsSettings.realisticFalloff = currentSettings.bakeryPointLightsSettings.bakeryPoint.realisticFalloff;
                            currentSettings.bakeryPointLightsSettings.samples = currentSettings.bakeryPointLightsSettings.bakeryPoint.samples;
                            currentSettings.bakeryPointLightsSettings.projMode = currentSettings.bakeryPointLightsSettings.bakeryPoint.projMode;
                            currentSettings.bakeryPointLightsSettings.cookie = currentSettings.bakeryPointLightsSettings.bakeryPoint.cookie;
                            currentSettings.bakeryPointLightsSettings.angle = currentSettings.bakeryPointLightsSettings.bakeryPoint.angle;
                            currentSettings.bakeryPointLightsSettings.innerAngle = currentSettings.bakeryPointLightsSettings.bakeryPoint.innerAngle;
                            currentSettings.bakeryPointLightsSettings.cubemap = currentSettings.bakeryPointLightsSettings.bakeryPoint.cubemap;
                            currentSettings.bakeryPointLightsSettings.iesFile = currentSettings.bakeryPointLightsSettings.bakeryPoint.iesFile;
                            currentSettings.bakeryPointLightsSettings.bitmask = currentSettings.bakeryPointLightsSettings.bakeryPoint.bitmask;
                            currentSettings.bakeryPointLightsSettings.bakeToIndirect = currentSettings.bakeryPointLightsSettings.bakeryPoint.bakeToIndirect;
                            currentSettings.bakeryPointLightsSettings.shadowmask = currentSettings.bakeryPointLightsSettings.bakeryPoint.shadowmask;
                            currentSettings.bakeryPointLightsSettings.indirectIntensity = currentSettings.bakeryPointLightsSettings.bakeryPoint.indirectIntensity;
                            currentSettings.bakeryPointLightsSettings.falloffMinRadius = currentSettings.bakeryPointLightsSettings.bakeryPoint.falloffMinRadius;
                        }

                        if (currentSettings.bakeryLightMeshesSettings != null && currentSettings.bakeryLightMeshesSettings.bakeryLightMesh != null)
                        {
                            currentSettings.bakeryLightMeshesSettings.color = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.color;
                            currentSettings.bakeryLightMeshesSettings.intensity = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.intensity;
                            currentSettings.bakeryLightMeshesSettings.texture = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.texture;
                            currentSettings.bakeryLightMeshesSettings.cutoff = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.cutoff;
                            currentSettings.bakeryLightMeshesSettings.samples = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.samples;
                            currentSettings.bakeryLightMeshesSettings.samples2 = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.samples2;
                            currentSettings.bakeryLightMeshesSettings.bitmask = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.bitmask;
                            currentSettings.bakeryLightMeshesSettings.selfShadow = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.selfShadow;
                            currentSettings.bakeryLightMeshesSettings.bakeToIndirect = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.bakeToIndirect;
                            currentSettings.bakeryLightMeshesSettings.indirectIntensity = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.indirectIntensity;
                            currentSettings.bakeryLightMeshesSettings.lmid = currentSettings.bakeryLightMeshesSettings.bakeryLightMesh.lmid;
                        }
#endif

                        sceneLights[i].GetComponent<MLSLight>().lastEditedBy = name;
                    }
                }

                if (RenderSettings.skybox != null)
                {
                    if (RenderSettings.skybox.HasProperty("_Tex"))
                    {
                        skyboxSettings.skyboxTexture = RenderSettings.skybox.GetTexture("_Tex") as Cubemap;
                        skyboxSettings.exposure = RenderSettings.skybox.GetFloat("_Exposure");
                        skyboxSettings.tintColor = RenderSettings.skybox.GetColor("_Tint");
                    }
                }

                fogSettings.enabled = RenderSettings.fog;
                fogSettings.fogColor = RenderSettings.fogColor;
                fogSettings.fogDensity = RenderSettings.fogDensity;

                environmentSettings.source = RenderSettings.ambientMode;
                environmentSettings.intensityMultiplier = RenderSettings.ambientIntensity;
                environmentSettings.ambientColor = RenderSettings.ambientLight;
                environmentSettings.skyColor = RenderSettings.ambientSkyColor;
                environmentSettings.equatorColor = RenderSettings.ambientEquatorColor;
                environmentSettings.groundColor = RenderSettings.ambientGroundColor;

#if BAKERY_INCLUDED
                if (skyboxSettings.bakerySkyLightsSettings != null && skyboxSettings.bakerySkyLightsSettings.bakerySky != null)
                {
                    skyboxSettings.bakerySkyLightsSettings.texName = skyboxSettings.bakerySkyLightsSettings.bakerySky.texName;
                    skyboxSettings.bakerySkyLightsSettings.color = skyboxSettings.bakerySkyLightsSettings.bakerySky.color;
                    skyboxSettings.bakerySkyLightsSettings.intensity = skyboxSettings.bakerySkyLightsSettings.bakerySky.intensity;
                    skyboxSettings.bakerySkyLightsSettings.samples = skyboxSettings.bakerySkyLightsSettings.bakerySky.samples;
                    skyboxSettings.bakerySkyLightsSettings.hemispherical = skyboxSettings.bakerySkyLightsSettings.bakerySky.hemispherical;
                    skyboxSettings.bakerySkyLightsSettings.bitmask = skyboxSettings.bakerySkyLightsSettings.bakerySky.bitmask;
                    skyboxSettings.bakerySkyLightsSettings.bakeToIndirect = skyboxSettings.bakerySkyLightsSettings.bakerySky.bakeToIndirect;
                    skyboxSettings.bakerySkyLightsSettings.indirectIntensity = skyboxSettings.bakerySkyLightsSettings.bakerySky.indirectIntensity;
                    skyboxSettings.bakerySkyLightsSettings.tangentSH = skyboxSettings.bakerySkyLightsSettings.bakerySky.tangentSH;
                    skyboxSettings.bakerySkyLightsSettings.cubemap = skyboxSettings.bakerySkyLightsSettings.bakerySky.cubemap;
                }

                BakeryLightMesh[] bakeryLightMeshes = FindObjectsOfType<BakeryLightMesh>();

                for (int i = 0; i < bakeryLightMeshes.Length; i++)
                {
                    LightSourceSettings.BakeryLightMeshesSettings currentSettings = bakeryLightMeshesSettings.Find(item => item.bakeryLightMesh == bakeryLightMeshes[i]);

                    if (currentSettings != null)
                    {
                        currentSettings.color = currentSettings.bakeryLightMesh.color;
                        currentSettings.intensity = currentSettings.bakeryLightMesh.intensity;
                        currentSettings.texture = currentSettings.bakeryLightMesh.texture;
                        currentSettings.cutoff = currentSettings.bakeryLightMesh.cutoff;
                        currentSettings.samples = currentSettings.bakeryLightMesh.samples;
                        currentSettings.samples2 = currentSettings.bakeryLightMesh.samples2;
                        currentSettings.bitmask = currentSettings.bakeryLightMesh.bitmask;
                        currentSettings.selfShadow = currentSettings.bakeryLightMesh.selfShadow;
                        currentSettings.bakeToIndirect = currentSettings.bakeryLightMesh.bakeToIndirect;
                        currentSettings.indirectIntensity = currentSettings.bakeryLightMesh.indirectIntensity;
                        currentSettings.lmid = currentSettings.bakeryLightMesh.lmid;
                    }
                }                
#endif

                MLSCustomBlendable[] customBlendables = FindObjectsOfType<MLSCustomBlendable>();

                for (int i = 0; i < customBlendables.Length; i++)
                {
                    CustomBlendablesSettings currentSettings = customBlendablesSettings.Find(item => item.sourceScriptId == customBlendables[i].sourceScriptId);

                    if (currentSettings != null)
                    {
                        if (customBlendables[i].blendableFloatFields.Count == 0 || customBlendables[i].blendableCubemapParameters.Count == 0)
                        {
                            customBlendables[i].GetSharedParameters();
                        }

                        for (int j = 0; j < customBlendables[i].blendableFloatFields.Count; j++)
                        {
                            for (int k = 0; k < currentSettings.blendableFloatParameters.Count; k++)
                            {
                                if (customBlendables[i].blendableFloatFields[j].Name == currentSettings.blendableFloatParameters[k])
                                {
                                    currentSettings.blendableFloatParametersValues[k] = (float) customBlendables[i].blendableFloatFields[k].GetValue(customBlendables[i]);
                                }
                            }
                        }

                        for (int j = 0; j < customBlendables[i].blendableCubemapParameters.Count; j++)
                        {
                            for (int k = 0; k < currentSettings.blendableCubemapParameters.Count; k++)
                            {
                                if (customBlendables[i].blendableCubemapParameters[j].Name == currentSettings.blendableCubemapParameters[k])
                                {
                                    currentSettings.blendableCubemapParametersValues[k] = (Cubemap) customBlendables[i].blendableCubemapParameters[k].GetValue(customBlendables[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public List<SceneLightingPreset> lightingPresets = new List<SceneLightingPreset>();
        public List<string> presetNames = new List<string>();
        public StoringMode storingMode;
        public bool deferredRenderingWarning;
        #endregion

        public static bool CheckIfStatic(GameObject gameObject)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            bool isStatic = false;

#if UNITY_2019_2_OR_NEWER
            if ((flags & StaticEditorFlags.ContributeGI) != 0)
            {
                isStatic = true;
            }
#else
            if ((flags & StaticEditorFlags.LightmapStatic) != 0)
            {
                isStatic = true;
            }
#endif

            return isStatic;
        }
#endif

        public void OnSceneLoadComplete(string targetScene)
        {
            stopProbesBlending = false;
            StartCoroutine(UpdateStoredArray(targetScene));
        }

        public void OnSceneUnloadComplete(string sceneToUnload)
        {
            //StartCoroutine(UpdateStoredArray(sceneToUnload, true));
        }

        public List<StoredLightmapData> DeserializeStoredData(string targetSceneName)
        {
            List<StoredLightmapData> tempLightmapDataList = new List<StoredLightmapData>();
            List<StoredLightmapData> exitLightmapDataList = new List<StoredLightmapData>();

            if (loadFromAssetBundles)
            {
                /*
                 * Use your own code here to load the assets from the AssetBundle. 
                 * Load Data Type: List<StoredLightmapData>.
                 * 
                 * tempLightmapDataList = AssetBundleData;
                 */

                Debug.LogFormat("<color=cyan>MLS:</color> Use your own code to load \"Stored Lightmap Data\" here.");

                if (tempLightmapDataList == null || tempLightmapDataList.Count == 0)
                {
                    return null;
                }
            }
            else
            {
                #if UNITY_EDITOR
                if (!Directory.Exists(currentDataPath))
                {
                    return null;
                }

                string[] storedLightmapDataPaths = Directory.GetFiles(currentDataPath);

                for (int i = 0; i < storedLightmapDataPaths.Length; i++)
                {
                    if (!storedLightmapDataPaths[i].EndsWith("meta", System.StringComparison.Ordinal))
                    {
                        StoredLightmapData data = AssetDatabase.LoadAssetAtPath<StoredLightmapData>(storedLightmapDataPaths[i]);

                        if (data != null)
                        {
                            tempLightmapDataList.Add(data);
                        }
                    }
                }
                #else
                tempLightmapDataList = sceneLightmapDatas;
                #endif
            }

            if (tempLightmapDataList.Count > 0)
            {
                for (int i = 0; i < tempLightmapDataList.Count; i++)
                {
                    if (tempLightmapDataList[i].workflow == workflow)
                    {
                        #if BAKERY_INCLUDED
                        if (lightmapper == Lightmapper.BakeryLightmapper)
                        {
                            tempLightmapDataList[i].bakeryVolumeDataDeserialized = new Hashtable();

                            for (int j = 0;
                                j < tempLightmapDataList[i].sceneLightingData.bakeryVolumes.name.Length;
                                j++)
                            {
                                try
                                {
                                    List<Texture3D> volumeTextures = new List<Texture3D>();

                                    volumeTextures.Add(tempLightmapDataList[i].sceneLightingData.bakeryVolumes
                                        .volumeTexture0[j]);
                                    volumeTextures.Add(tempLightmapDataList[i].sceneLightingData.bakeryVolumes
                                        .volumeTexture1[j]);
                                    volumeTextures.Add(tempLightmapDataList[i].sceneLightingData.bakeryVolumes
                                        .volumeTexture2[j]);

                                    if (tempLightmapDataList[i].sceneLightingData.bakeryVolumes.volumeTexture3.Length >
                                        0)
                                    {
                                        volumeTextures.Add(tempLightmapDataList[i].sceneLightingData.bakeryVolumes
                                            .volumeTexture3[j]);
                                    }
                                    else
                                    {
                                        volumeTextures.Add(new Texture3D(1, 1, 1, TextureFormat.RGBAHalf, false));
                                    }

                                    if (tempLightmapDataList[i].sceneLightingData.bakeryVolumes.volumeTexture4.Length >
                                        0)
                                    {
                                        volumeTextures.Add(tempLightmapDataList[i].sceneLightingData.bakeryVolumes
                                            .volumeTexture4[j]);
                                    }
                                    else
                                    {
                                        volumeTextures.Add(new Texture3D(1, 1, 1, TextureFormat.RGBAHalf, false));
                                    }

                                    tempLightmapDataList[i].bakeryVolumeDataDeserialized.Add(
                                        tempLightmapDataList[i].sceneLightingData.bakeryVolumes.name[j],
                                        volumeTextures);
                                }
                                catch (System.Exception ae)
                                {
                                    Debug.Log("The probability of this event is extremely small, " +
                                              "however, if it did happen, please report it to the developer.\r\n" +
                                              ae.ToString());
                                }
                            }
                        }
#endif
                        
                        tempLightmapDataList[i].storedReflectionProbeDataDeserialized = new Hashtable();

                        for (int j = 0; j < tempLightmapDataList[i].sceneLightingData.reflectionProbes.name.Length; j++)
                        {
                            try
                            {
                                tempLightmapDataList[i].storedReflectionProbeDataDeserialized.Add(
                                    tempLightmapDataList[i].sceneLightingData.reflectionProbes.name[j],
                                    tempLightmapDataList[i].sceneLightingData.reflectionProbes.cubeReflectionTexture[j]);
                            }
                            catch (System.Exception ae)
                            {
                                Debug.Log("The probability of this event is extremely small, " +
                                    "however, if it did happen, please report it to the developer.\r\n" +
                                    ae.ToString());
                            }
                        }

                        tempLightmapDataList[i].rendererDataDeserialized = new Hashtable();

                        for (int j = 0; j < tempLightmapDataList[i].sceneLightingData.rendererDatas.Length; j++)
                        {
                            try
                            {
                                tempLightmapDataList[i].rendererDataDeserialized.Add(
                                    tempLightmapDataList[i].sceneLightingData.rendererDatas[j].objectId,
                                    tempLightmapDataList[i].sceneLightingData.rendererDatas[j]);
                            }
                            catch (System.Exception ae)
                            {
                                Debug.Log("The probability of this event is extremely small, " +
                                    "however, if it did happen, please report it to the developer.\r\n" +
                                    ae.ToString());
                            }
                        }

                        tempLightmapDataList[i].terrainDataDeserialized = new Hashtable();

                        for (int j = 0; j < tempLightmapDataList[i].sceneLightingData.terrainDatas.Length; j++)
                        {
                            try
                            {
                                tempLightmapDataList[i].terrainDataDeserialized.Add(
                                    tempLightmapDataList[i].sceneLightingData.terrainDatas[j].objectId,
                                    tempLightmapDataList[i].sceneLightingData.terrainDatas[j]);
                            }
                            catch (System.Exception ae)
                            {
                                Debug.Log("The probability of this event is extremely small, " +
                                    "however, if it did happen, please report it to the developer.\r\n" +
                                    ae.ToString());
                            }
                        }

                        tempLightmapDataList[i].lightSourceDataDeserialized = new Hashtable();

                        for (int j = 0; j < tempLightmapDataList[i].sceneLightingData.lightSourceDatas.Length; j++)
                        {
                            try
                            {
                                tempLightmapDataList[i].lightSourceDataDeserialized.Add(
                                    tempLightmapDataList[i].sceneLightingData.lightSourceDatas[j].lightUID,
                                    tempLightmapDataList[i].sceneLightingData.lightSourceDatas[j]);
                            }
                            catch (System.Exception ae)
                            {
                                Debug.Log("The probability of this event is extremely small, " +
                                    "however, if it did happen, please report it to the developer.\r\n" +
                                    ae.ToString());
                            }
                        }

                        exitLightmapDataList.Add(tempLightmapDataList[i]);
                    }
                }
            }

            return exitLightmapDataList;
        }

        public List<StoredLightingScenario> UpdateLightingScenarios(string targetSceneName, bool forceUpdateStoredData = false)
        {
            List<StoredLightingScenario> tempLightmapScenariosList = new List<StoredLightingScenario>();
            List<StoredLightingScenario> exitLightmapScenariosList = new List<StoredLightingScenario>();
            Blending.BlendingOperationalData blendingOperationalData;

            if (loadFromAssetBundles)
            {
                /*
                 * Use your own code here to load the assets from the AssetBundle. 
                 * Load Data Type: List<StoredLightingScenario>
                 * 
                 * tempLightmapScenariosList = AssetBundleData;
                 */

                Debug.LogFormat("<color=cyan>MLS:</color> Use your own code to load \"Lightmap Scenarios\" here.");

                if (tempLightmapScenariosList == null || tempLightmapScenariosList.Count == 0)
                {
                    return null;
                }
            }
            else
            {
                #if UNITY_EDITOR
                if (!Directory.Exists(currentDataPath))
                {
                    return null;
                }

                string[] storedLightmapScenariosPaths = Directory.GetFiles(currentDataPath);

                for (int i = 0; i < storedLightmapScenariosPaths.Length; i++)
                {
                    if (!storedLightmapScenariosPaths[i].EndsWith("meta", System.StringComparison.Ordinal))
                    {
                        StoredLightingScenario scenario = AssetDatabase.LoadAssetAtPath<StoredLightingScenario>(storedLightmapScenariosPaths[i]);

                        if (scenario != null)
                        {
                            tempLightmapScenariosList.Add(scenario);
                        }
                    }
                }
                #else
                tempLightmapScenariosList = sceneLightmapScenarios;
                #endif
            }

            if (tempLightmapScenariosList.Count > 0)
            {                
                Blending.blendingOperationalDatas.TryGetValue(targetSceneName, out blendingOperationalData);
                
                for (int i = 0; i < tempLightmapScenariosList.Count; i++)
                {
                    // if (!OnBlendingValueChanged.Contains(tempLightmapScenariosList[i].OnBlendingValueChanged))
                    // {
                    //     OnBlendingValueChanged.Add(tempLightmapScenariosList[i].OnBlendingValueChanged);
                    // }
                    //
                    // if (!OnLoadedLightmapChanged.Contains(tempLightmapScenariosList[i].OnLoadedLightmapChanged))
                    // {
                    //     OnLoadedLightmapChanged.Add(tempLightmapScenariosList[i].OnLoadedLightmapChanged);
                    // }
                    
                    if (tempLightmapScenariosList[i].workflow == workflow)
                    {
                        if (tempLightmapScenariosList[i].blendableLightmaps.Count > 0)
                        {
                            if (LightmapSettings.lightProbes != null)
                            {
                                switch (workflow)
                                {
                                    case Workflow.MultiScene:
#if !UNITY_2020_1_OR_NEWER
                                        //if (blendingOperationalData.loadIndex > 0)
                                        //{
                                        //    if (blendingOperationalData.lightProbesArrayIndex == 0)
                                        //    {
                                        //        blendingOperationalData.lightProbesArrayIndex = LightmapSettings.lightProbes.bakedProbes.Length - tempLightmapScenariosList[i].blendableLightmaps[0].lightingData.sceneLightingData.lightProbes.Length;
                                        //        tempLightmapScenariosList[i].lightProbesArrayPosition = blendingOperationalData.lightProbesArrayIndex;
                                        //    }
                                        //    else
                                        //    {
                                        //        tempLightmapScenariosList[i].lightProbesArrayPosition = blendingOperationalData.lightProbesArrayIndex;
                                        //    }
                                        //}
                                        //else
                                        {
                                            tempLightmapScenariosList[i].lightProbesArrayPosition = 0;
                                        }
#endif
                                        break;
                                    case Workflow.SingleScene:
                                        tempLightmapScenariosList[i].lightProbesArrayPosition = 0;
                                        break;
                                }
                                
                            }
                        }

                        exitLightmapScenariosList.Add(tempLightmapScenariosList[i]);
                    }
                }

                availableScenarios = exitLightmapScenariosList;
            }

            return exitLightmapScenariosList;
        }

        private List<ReflectionProbe> UpdateReflectionProbes(string targetScene)
        {
            ReflectionProbe[] reflectionProbes = FindObjectsOfType<ReflectionProbe>();
            List<ReflectionProbe> resultProbesList = new List<ReflectionProbe>();

            for (int i = 0; i < reflectionProbes.Length; i++)
            {
                if (reflectionProbes[i].gameObject.scene.name == targetScene)
                {
                    resultProbesList.Add(reflectionProbes[i]);
                    sceneReflectionProbePositions.Add(reflectionProbes[i].transform.position);
                }
            }

            return resultProbesList;
        }

        private List<MLSLight> UpdateLights(string targetScene)
        {
            MLSLight[] lights = FindObjectsOfType<MLSLight>();
            List<MLSLight> resultLightsList = new List<MLSLight>();

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].gameObject.scene.name == targetScene)
                {
                    resultLightsList.Add(lights[i]);
                }
            }

            return resultLightsList;
        }

        public IEnumerator UpdateStoredArray(string targetScene, bool forceUpdateStoredData = false)
        {
            if (storedDataUpdatingProcess)
            {
                yield break;    
            }

            storedDataUpdatingProcess = true;
            storedDataUpdated = false;

            FindSystemProperies();

            if (systemProperties == null)
            {
                Debug.LogFormat("<color=cyan>MLS:</color> " +
                    "The \"System Properties\" file was not found. " +
                    "Go to Tools->Magic Tools->Magic Lightmap Switcher->Prepare Shaders menu item. " +
                    "The file will be created automatically.");

                yield break;
            }

#if UNITY_EDITOR
#if BAKERY_INCLUDED
            if (Lightmapping.isRunning || ftRenderLightmap.bakeInProgress)
            {
#else
            if (Lightmapping.isRunning)
            {
#endif

                storedDataUpdatingProcess = false;
                forceUpdateStoredData = true;
                yield break;
            }
#endif

            if (lastSceneCount != SceneManager.sceneCount || forceUpdateStoredData)
            {
                if (string.IsNullOrEmpty(targetScene))
                {
                    storedDataUpdatingProcess = false;
                    yield break;
                }

                bool removeOrAdd = false || lastSceneCount < SceneManager.sceneCount;

                lastSceneCount = SceneManager.sceneCount;  

                switch (workflow)
                {
                    case Workflow.MultiScene:
                        if (removeOrAdd || forceUpdateStoredData)
                        {
#if !UNITY_2020_1_OR_NEWER
                            Blending.UpdateBlendingOperationalData(targetScene);
#endif

                            if (!storedLightmapDatas.ContainsKey(targetScene))
                            {
                                storedLightmapDatas.Add(targetScene, DeserializeStoredData(targetScene));
                            }
                            else
                            {
                                storedLightmapDatas[targetScene] = DeserializeStoredData(targetScene);

                                if (storedLightmapDatas[targetScene].Count == 0)
                                {
                                    storedLightmapDatas.Remove(targetScene);
                                }
                            }

                            if (!storedLightmapScenarios.ContainsKey(targetScene))
                            {
                                storedLightmapScenarios.Add(targetScene, UpdateLightingScenarios(targetScene, forceUpdateStoredData));
                            }
                            else
                            {
                                storedLightmapScenarios[targetScene] = UpdateLightingScenarios(targetScene, forceUpdateStoredData);

                                if (storedLightmapScenarios[targetScene].Count == 0)
                                {
                                    storedLightmapScenarios.Remove(targetScene);
                                }
                            }

                            if (!storedReflectionProbes.ContainsKey(targetScene))
                            {
                                storedReflectionProbes.Add(targetScene, UpdateReflectionProbes(targetScene));
                            }
                            else
                            {
                                storedReflectionProbes[targetScene] = UpdateReflectionProbes(targetScene);
                            }

                            if (!storedLights.ContainsKey(targetScene))
                            {
                                storedLights.Add(targetScene, UpdateLights(targetScene));
                            }
                            else
                            {
                                storedLights[targetScene] = UpdateLights(targetScene);
                            }
                        }
                        else
                        {
                            storedLightmapScenarios.TryGetValue(targetScene, out List<StoredLightingScenario> sourceScenariosSet);

                            if (storedLightmapDatas.ContainsKey(targetScene))
                            {
                                storedLightmapDatas.Remove(targetScene);
                            }

                            if (storedLightmapScenarios.ContainsKey(targetScene))
                            {
                                storedLightmapScenarios.Remove(targetScene);
                            }                            

                            if (storedReflectionProbes.ContainsKey(targetScene))
                            {
                                 storedReflectionProbes.Remove(targetScene);
                            }

                            if (storedLights.ContainsKey(targetScene))
                            {
                                storedLights.Remove(targetScene);
                            }
                        }
                        break;
                    case Workflow.SingleScene:
                        sceneLightmapDatas = DeserializeStoredData(targetScene);
                        sceneLightmapScenarios = UpdateLightingScenarios(targetScene);
                        sceneAffectedLightSources = new List<MLSLight>(FindObjectsOfType<MLSLight>());
                        sceneReflectionProbes = UpdateReflectionProbes(targetScene);
                        break;
                }                
            }

            if (loadFromAssetBundles)
            {
                while (sceneLightmapDatas == null || sceneLightmapScenarios == null)
                {
                    Debug.LogFormat("<color=cyan>MLS:</color> Stored Data Array has not been updated.");
                    storedDataUpdatingProcess = false;
                    yield return null;
                }
            }
            else
            {
                if (sceneLightmapDatas == null || sceneLightmapScenarios == null)
                {
                    Debug.LogFormat("<color=cyan>MLS:</color> Stored Data Array has not been updated.");
                    storedDataUpdatingProcess = false;
                    yield break;
                }
            }

#if UNITY_2020_1_OR_NEWER
            Blending.UpdateBlendingOperationalData( targetScene);
#endif

            forceUpdateStoredData = false;
            ConfigureAffectedObjects(targetScene);
            storedDataUpdated = true;
            storedDataUpdatingProcess = false;
            SceneManagment.sceneProcessing = false;

            if (workflow == Workflow.MultiScene)
            {
                LightProbes.TetrahedralizeAsync();
            }

#if UNITY_EDITOR
            if (currentLightmapScenario != null)
            {
                if (currentLightmapScenario.blendableLightmaps != null && currentLightmapScenario.blendableLightmaps.Count > 0)
                {
                    Blending.Blend(this, 0, currentLightmapScenario, targetScene);
                }
            }
            else if (availableScenarios.Count > 0)
            {
                if (availableScenarios[0].blendableLightmaps != null && availableScenarios[0].blendableLightmaps.Count > 0)
                {
                    Blending.Blend(this, 0, availableScenarios[0], targetScene);
                }
            }
#endif
        }

        private void ConfigureAffectedObjects(string targetScene)
        {
            List<StoredLightmapData> sourceData = new List<StoredLightmapData>();

            if (workflow == Workflow.MultiScene)
            {
                storedLightmapDatas.TryGetValue(targetScene, out sourceData);
            }
            else
            {
                sourceData = sceneLightmapDatas;
            }

            if (sourceData != null && sourceData.Count > 0)
            {
                StoreAffectableObjects(targetScene);

                if (sceneStaticAffectedObjects.Count > 0)
                {
                    for (int i = 0; i < sceneStaticAffectedObjects.Count; i++)
                    {
                        if (sceneStaticAffectedObjects[i].terrain == null)
                        {
                            if (sceneStaticAffectedObjects[i].isStatic)
                            {
                                SetBlendingOptions(sceneStaticAffectedObjects[i], BlendingOptions.All);
                            }
                            else
                            {
                                SetBlendingOptions(sceneStaticAffectedObjects[i], BlendingOptions.Reflections);
                            }
                        }
                        else
                        {
                            SetBlendingOptions(sceneStaticAffectedObjects[i], BlendingOptions.All);
                        }
                    }
                }
            }
            else
            {
                if (sceneStaticAffectedObjects.Count > 0)
                {
                    for (int i = 0; i < sceneStaticAffectedObjects.Count; i++)
                    {
                        if (sceneStaticAffectedObjects[i].terrain == null)
                        {
                            SetBlendingOptions(sceneStaticAffectedObjects[i], BlendingOptions.None);
                        }
                        else
                        {
                            SetBlendingOptions(sceneStaticAffectedObjects[i], BlendingOptions.None);
                        }
                    }
                }
            }

            resetAffectedObjects = false;
        }        

        private void SetBlendingOptions(AffectedObject affectableObject, BlendingOptions blendingOptions)
        {
            Blending.InitiShaderProperties();

            if (affectableObject.meshRenderer != null || affectableObject.terrain != null)
            {
                affectableObject.InitPropertyBlock();
            }
            else
            {
                sceneStaticAffectedObjects.Remove(affectableObject);
                return;
            }

            if (defaultCubeBlack == null)
            {
                CreateDefaultCubemap();
            }

            if (availableScenarios.Count > 0)
            {
                currentLightmapScenario = availableScenarios[0];
                lastLightmapScenario = currentLightmapScenario;
            }

            if (currentLightmapScenario != null)
            {
                if (currentLightmapScenario.blendableLightmaps.Count > 1)
                {
                    switch (blendingOptions)
                    {
                        case BlendingOptions.All:
                            affectableObject.SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 1);
                            affectableObject.SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                            break;
                        case BlendingOptions.Lightmaps:
                            affectableObject.SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 1);
                            affectableObject.SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 0);

                            affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_From_0, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_To_0, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_From_1, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_To_1, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_SkyReflection_Blend_From, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_SkyReflection_Blend_To, defaultCubeBlack);
                            break;
                        case BlendingOptions.Reflections:
                            affectableObject.SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                            affectableObject.SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                            break;
                        case BlendingOptions.None:
                            affectableObject.SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                            affectableObject.SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 0);

                            affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_From_0, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_To_0, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_From_1, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_To_1, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_SkyReflection_Blend_From, defaultCubeBlack);
                            affectableObject.SetShaderTexture(Blending._MLS_SkyReflection_Blend_To, defaultCubeBlack);
                            break;
                    }
                }
                else
                {
                    affectableObject.SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                    affectableObject.SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 0);

                    affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_From_0, defaultCubeBlack);
                    affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_To_0, defaultCubeBlack);
                    affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_From_1, defaultCubeBlack);
                    affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_To_1, defaultCubeBlack);
                    affectableObject.SetShaderTexture(Blending._MLS_SkyReflection_Blend_From, defaultCubeBlack);
                    affectableObject.SetShaderTexture(Blending._MLS_SkyReflection_Blend_To, defaultCubeBlack);
                }
            }
            else
            {
                affectableObject.SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                affectableObject.SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 0);

                affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_From_0, defaultCubeBlack);
                affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_To_0, defaultCubeBlack);
                affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_From_1, defaultCubeBlack);
                affectableObject.SetShaderTexture(Blending._MLS_Reflection_Blend_To_1, defaultCubeBlack);
                affectableObject.SetShaderTexture(Blending._MLS_SkyReflection_Blend_From, defaultCubeBlack);
                affectableObject.SetShaderTexture(Blending._MLS_SkyReflection_Blend_To, defaultCubeBlack);
            }

            affectableObject.ApplyPropertyBlock();
        }

#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        public static void SetDefaultShaderValues()
        {
            Blending.InitiShaderProperties();

            if (defaultCubeBlack == null)
            {
                CreateDefaultCubemap();
            }
        }

        public void SetBlendingOptionsGlobal(BlendingOptions blendingOptions)
        {
            currentBlendingState = blendingOptions;

            Blending.InitiShaderProperties();

            switch (blendingOptions)
            {
                case BlendingOptions.All:
                    break;
                case BlendingOptions.None:
                    Shader.SetGlobalInt(Blending._MLS_REFLECTIONS_FLAG, 0);
                    Shader.SetGlobalInt(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 0);
                    Shader.SetGlobalInt(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                    Shader.SetGlobalInt(Blending._MLS_ENABLE_SKY_CUBEMAPS_BLENDING, 0);
                    Shader.SetGlobalTexture(Blending._MLS_Reflection_Blend_From_0, defaultCubeBlack);
                    Shader.SetGlobalTexture(Blending._MLS_Reflection_Blend_To_0, defaultCubeBlack);
                    Shader.SetGlobalTexture(Blending._MLS_Reflection_Blend_From_1, defaultCubeBlack);
                    Shader.SetGlobalTexture(Blending._MLS_Reflection_Blend_To_1, defaultCubeBlack);
                    Shader.SetGlobalTexture(Blending._MLS_SkyReflection_Blend_From, defaultCubeBlack);
                    Shader.SetGlobalTexture(Blending._MLS_SkyReflection_Blend_To, defaultCubeBlack);
                    break;
                case BlendingOptions.Lightmaps:
                    break;
                case BlendingOptions.Reflections:
                    break;
            }

#if UNITY_EDITOR
            StoreAffectableObjects(EditorSceneManager.GetActiveScene().name);
#endif

            for (int i = 0; i < sceneStaticAffectedObjects.Count; i++)
            {
                if ((sceneStaticAffectedObjects[i].meshRenderer != null || sceneStaticAffectedObjects[i].terrain != null) && sceneStaticAffectedObjects[i]._propBlock == null)
                {
                    sceneStaticAffectedObjects[i].InitPropertyBlock();
                }
                else
                {
                    sceneStaticAffectedObjects.RemoveAt(i);
                    return;
                }

                if (defaultCubeBlack == null)
                {
                    CreateDefaultCubemap();
                }

                switch (blendingOptions)
                {
                    case BlendingOptions.All:
                        sceneStaticAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 1);
                        sceneStaticAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                        break;
                    case BlendingOptions.Lightmaps:
                        sceneStaticAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 1);
                        sceneStaticAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 0);

                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_Reflection_Blend_From_0, defaultCubeBlack);
                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_Reflection_Blend_To_0, defaultCubeBlack);
                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_Reflection_Blend_From_1, defaultCubeBlack);
                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_Reflection_Blend_To_1, defaultCubeBlack);
                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_SkyReflection_Blend_From, defaultCubeBlack);
                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_SkyReflection_Blend_To, defaultCubeBlack);
                        break;
                    case BlendingOptions.Reflections:
                        sceneStaticAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                        sceneStaticAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                        break;
                    case BlendingOptions.None:
                        sceneStaticAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_LIGHTMAPS_BLENDING, 0);
                        sceneStaticAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 0);

                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_Reflection_Blend_From_0, defaultCubeBlack);
                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_Reflection_Blend_To_0, defaultCubeBlack);
                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_Reflection_Blend_From_1, defaultCubeBlack);
                        sceneStaticAffectedObjects[i].SetShaderTexture(Blending._MLS_Reflection_Blend_To_1, defaultCubeBlack);
                        break;
                }

                sceneStaticAffectedObjects[i].ApplyPropertyBlock();                
            }

            for (int i = 0; i < sceneDynamicAffectedObjects.Count; i++)
            {
                if (sceneDynamicAffectedObjects[i].meshRenderer != null || sceneDynamicAffectedObjects[i].terrain != null)
                {
                    sceneDynamicAffectedObjects[i].InitPropertyBlock();
                }
                else
                {
                    sceneDynamicAffectedObjects.RemoveAt(i);
                    return;
                }

                switch (blendingOptions)
                {
                    case BlendingOptions.All:
                        sceneDynamicAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                        break;
                    case BlendingOptions.Lightmaps:
                        sceneDynamicAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 0);
                        break;
                    case BlendingOptions.Reflections:
                        sceneDynamicAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 1);
                        break;
                    case BlendingOptions.None:
                        sceneDynamicAffectedObjects[i].SetShaderFloat(Blending._MLS_ENABLE_REFLECTIONS_BLENDING, 0);
                        break;
                }

                sceneDynamicAffectedObjects[i].ApplyPropertyBlock();
            }
        }

        private void StoreAffectableObjects(string targetScene)
        {
            List<AffectedObject> tempStaticAffectableObjects = new List<AffectedObject>();
            List<AffectedObject> tempDynamicAffectableObjects = new List<AffectedObject>();

            Object[] renderers = FindObjectsOfType<MeshRenderer>();

            foreach (MeshRenderer meshRenderer in renderers)
            {
                if (workflow == Workflow.MultiScene)
                {
                    if (meshRenderer.gameObject.scene.name != targetScene)
                    {
                        continue;
                    }
                }

                MLSStaticRenderer staticRenderer = meshRenderer.gameObject.GetComponent<MLSStaticRenderer>();
                MLSDynamicRenderer dynamicRenderer = meshRenderer.gameObject.GetComponent<MLSDynamicRenderer>();

                if (staticRenderer != null)
                {
#if UNITY_EDITOR
                    if (meshRenderer.lightmapIndex < 0 || meshRenderer.scaleInLightmap == 0 || !meshRenderer.enabled || meshRenderer.receiveGI == ReceiveGI.LightProbes)
#else
                    if (meshRenderer.lightmapIndex < 0 || !meshRenderer.enabled)
#endif
                    {
                        //if (meshRenderer.GetComponent<MLSStaticRenderer>() != null)
                        //{
                        //    GameObject.DestroyImmediate(meshRenderer.GetComponent<MLSStaticRenderer>());
                        //}

                        //continue;
                    }

                    AffectedObject affectableObject = new AffectedObject();

                    affectableObject.isStatic = true;
                    affectableObject.meshRenderer = meshRenderer;
                    affectableObject.reflectionProbeUsage = meshRenderer.reflectionProbeUsage;
                    affectableObject.materialsCount = meshRenderer.sharedMaterials.Length;
                    affectableObject.objectId = staticRenderer.scriptId;

                    tempStaticAffectableObjects.Add(affectableObject);
                }
                else if (dynamicRenderer != null)
                {
                    if (!meshRenderer.enabled)
                    {
                        if (meshRenderer.GetComponent<MLSStaticRenderer>() != null)
                        {
                            GameObject.DestroyImmediate(meshRenderer.GetComponent<MLSStaticRenderer>());
                        }

                        continue;
                    }

                    AffectedObject affectableObject = new AffectedObject();

                    affectableObject.meshRenderer = meshRenderer;
                    affectableObject.reflectionProbeUsage = meshRenderer.reflectionProbeUsage;
                    affectableObject.materialsCount = meshRenderer.sharedMaterials.Length;
                    affectableObject.objectId = dynamicRenderer.scriptId;

                    tempDynamicAffectableObjects.Add(affectableObject);
                }
            }

            Object[] terrains = FindObjectsOfType<Terrain>();

            foreach (Terrain terrain in terrains)
            {
                if (workflow == Workflow.MultiScene)
                {
                    if (terrain.gameObject.scene.name != targetScene)
                    {
                        continue;
                    }
                }

                if (terrain.lightmapIndex < 0 || terrain.lightmapScaleOffset.x == 0 || terrain.lightmapScaleOffset.y == 0 || !terrain.enabled)
                {
                    if (terrain.GetComponent<MLSStaticRenderer>() != null)
                    {
                        GameObject.DestroyImmediate(terrain.GetComponent<MLSStaticRenderer>());
                    }

                    continue;
                }

                MLSStaticRenderer staticRenderer = terrain.gameObject.GetComponent<MLSStaticRenderer>();
                MLSDynamicRenderer dynamicRenderer = terrain.gameObject.GetComponent<MLSDynamicRenderer>();

                if (staticRenderer != null)
                {
                    AffectedObject affectableObject = new AffectedObject();

                    affectableObject.isStatic = true;
                    affectableObject.terrain = terrain;
                    affectableObject.objectId = staticRenderer.scriptId;

                    tempStaticAffectableObjects.Add(affectableObject);
                }
                else if (dynamicRenderer != null)
                {
                    AffectedObject affectableObject = new AffectedObject();

                    affectableObject.terrain = terrain;
                    affectableObject.objectId = dynamicRenderer.scriptId;

                    tempDynamicAffectableObjects.Add(affectableObject);
                }
            }

            if (workflow == Workflow.MultiScene)
            {
                if (!staticAffectedObjects.ContainsKey(targetScene))
                {
                    staticAffectedObjects.Add(targetScene, tempStaticAffectableObjects);
                }
                else
                {
                    staticAffectedObjects[targetScene] = tempStaticAffectableObjects;
                }

                if (!dynamicAffectedObjects.ContainsKey(targetScene))
                {
                    dynamicAffectedObjects.Add(targetScene, tempDynamicAffectableObjects);
                }
                else
                {
                    dynamicAffectedObjects[targetScene] = tempDynamicAffectableObjects;
                }
            }
            else
            {
                sceneStaticAffectedObjects = tempStaticAffectableObjects;
                sceneDynamicAffectedObjects = tempDynamicAffectableObjects;
            }
        }

        private void TetrahedralizeProbesAsync()
        {
            needsRetetrahedralization = true;
            tetrahedralizationCompleted = false;

            //LightProbes.TetrahedralizeAsync();
        }

        private void TetrahedralizationCompleted()
        {
            needsRetetrahedralization = false;
            tetrahedralizationCompleted = true;
        }

        private bool tetrahedralizationCompleted;
        private bool needsRetetrahedralization;

        private void Awake() 
        {

        }

        public bool lightprobesBlendingStarted;
        private void OnEnable()
        {
            lightprobesBlendingStarted = false;
#if UNITY_EDITOR
            if (switcherSerializedObject == null)
            {
                switcherSerializedObject = new SerializedObject(this);
            }
#endif

            storedDataUpdated = false;
            cubemapArrayInitialized = false;
            lightmapArrayInitialized = false;
            
            if (useTexture2DArrays)
            {
                Shader.EnableKeyword("MLS_TEXTURE2D_ARRAYS_ON");
                Shader.DisableKeyword("MLS_TEXTURE2D_ARRAYS_OFF");
            }
            else
            {
                Shader.EnableKeyword("MLS_TEXTURE2D_ARRAYS_OFF");
                Shader.DisableKeyword("MLS_TEXTURE2D_ARRAYS_ON");
            }
            
            if (useTextureCubeArrays)
            {
                Shader.EnableKeyword("MLS_TEXTURECUBE_ARRAYS_ON");
                Shader.DisableKeyword("MLS_TEXTURECUBE_ARRAYS_OFF");
            }
            else
            {
                Shader.EnableKeyword("MLS_TEXTURECUBE_ARRAYS_OFF");
                Shader.DisableKeyword("MLS_TEXTURECUBE_ARRAYS_ON");
            }

            CreateDefaultCubemap();
            LoadDependentAssets();

            OnDynamicRendererAdded = new DynamicRendererAddedEvent();
            OnDynamicRendererRemoved = new DynamicRendererRemoveEvent();

            OnDynamicRendererAdded.AddListener(AddDynamicRenderer);
            OnDynamicRendererRemoved.AddListener(RemoveDynamicRenderer);

            switch (workflow)
            {
                case Workflow.SingleScene:
                    if (!storedDataUpdated)
                    {
                        StartCoroutine(UpdateStoredArray(SceneManager.GetActiveScene().name, true));
                    } 
                    break;
                case Workflow.MultiScene:
                    if (!storedDataUpdated)
                    {
                        StartCoroutine(UpdateStoredArray(SceneManager.GetActiveScene().name, true));
                    }
                    break;
            }

            //LightProbes.needsRetetrahedralization += TetrahedralizeProbesAsync;
            //LightProbes.tetrahedralizationCompleted += TetrahedralizationCompleted;
        }

        void Start()
        {

        }

        private void Update() 
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                //EditorApplication.update += SetDefaultShaderValuesLocal;
            }
#endif
        }

        private void AddDynamicRenderer(GameObject gameObject, MLSDynamicRenderer dynamicRenderer)
        {            
            AffectedObject affectableObject = new AffectedObject();

            affectableObject.meshRenderer = gameObject.GetComponent<MeshRenderer>();
            affectableObject.objectId = dynamicRenderer.scriptId;

            if (workflow == Workflow.MultiScene)
            {
                List<AffectedObject> currentAffectableObjects = new List<AffectedObject>();

                if (dynamicAffectedObjects.ContainsKey(gameObject.scene.name))
                {
                    dynamicAffectedObjects.TryGetValue(gameObject.scene.name, out currentAffectableObjects);
                    currentAffectableObjects.Add(affectableObject);
                    dynamicAffectedObjects[gameObject.scene.name] = currentAffectableObjects;
                }
            }
            else
            {
                sceneDynamicAffectedObjects.Add(affectableObject);
            }
        }

        private void RemoveDynamicRenderer(GameObject gameObject, AffectedObject affectableObject)
        {
            if (workflow == Workflow.MultiScene)
            {
                List<AffectedObject> currentAffectableObjects = new List<AffectedObject>();

                if (dynamicAffectedObjects.ContainsKey(gameObject.scene.name))
                {
                    dynamicAffectedObjects.TryGetValue(gameObject.scene.name, out currentAffectableObjects);
                    currentAffectableObjects.Remove(affectableObject);
                    dynamicAffectedObjects[gameObject.scene.name] = currentAffectableObjects;
                }
            }
            else
            {
                sceneDynamicAffectedObjects.Remove(affectableObject);
            }
        }

        public static void CreateDefaultCubemap()
        {
#if UNITY_EDITOR
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.WebGL:
                    defaultCubeBlack = new Cubemap(12, TextureFormat.RGBAHalf, false);
                    break;
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.Switch:
                    defaultCubeBlack = new Cubemap(12, UnityEngine.Experimental.Rendering.DefaultFormat.HDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                    break;
                case BuildTarget.Android:
                    defaultCubeBlack = new Cubemap(12, TextureFormat.ETC_RGB4, false);
                    break;
            }
#else
            switch (Application.platform)
            {
                case RuntimePlatform.WebGLPlayer:
                    defaultCubeBlack = new Cubemap(12, TextureFormat.RGBAHalf, false);
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.Switch:
                    defaultCubeBlack = new Cubemap(12, UnityEngine.Experimental.Rendering.DefaultFormat.HDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                    break;
                case RuntimePlatform.Android:
                    defaultCubeBlack = new Cubemap(12, TextureFormat.ETC_RGB4, false);
                    break;
            } 
#endif
        }

        private void FindSystemProperies()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(workPath))
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);

                for (int i = 0; i < directories.Length; i++)
                {
                    if (Directory.GetFiles(directories[i]).Length == 0)
                    {
                        continue;
                    }

                    if (!directories[i].Contains("Resources"))
                    {
                        workPath = directories[i];
                        break;
                    }
                }
            }

            int subIndex = workPath.IndexOf("Assets");
            string finalPath = workPath.Substring(subIndex + "Assets".Length + 1);
            systemProperties = AssetDatabase.LoadAssetAtPath<SystemProperties>("Assets/" + finalPath + "/Editor/SystemProperties.asset");
#endif
        }

        private void LoadDependentAssets()
        {
#if UNITY_EDITOR
            FindSystemProperies();

            if (systemProperties == null)
            {
                workPath = "";
                FindSystemProperies();
                Debug.LogFormat("<color=cyan>MLS:</color> Palgin's position in the project hierarchy has changed.");
            }
#endif
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightmapSwitcher
{    
    [System.Serializable]
    public class SystemProperties : ScriptableObject
    {
        [SerializeField]
        public bool standardRPActive;
        [SerializeField]
        public bool universalRPActive;
        [SerializeField]
        public bool highDefinitionRPActive;
        [SerializeField]
        public bool standardRPPatched;
        [SerializeField]
        public bool universalRPPatched;
        [SerializeField]
        public bool highDefinitionRPPatched;
        [SerializeField]
        public bool clearOriginalLightingData;
        [SerializeField]
        public bool batchByLightmapIndex;
        [SerializeField]
        public bool editorRestarted;
        [SerializeField]
        public double prevTimeSinceStartup;
        [SerializeField]
        public bool checkForPatchedRoutineInProcess;
        [SerializeField, HideInInspector]
        public bool restoring;
        [SerializeField]
        public bool srpPatching;
        [SerializeField]
        public bool waitForRestart;
        [SerializeField, HideInInspector]
        public string srpCoreDirFrom;        
        [SerializeField, HideInInspector]
        public string srpCoreDirTo;
        [SerializeField, HideInInspector]
        public string srpCommonDirFrom;
        [SerializeField, HideInInspector]
        public string srpCommonDirTo;
        
        [SerializeField, HideInInspector]
        public string shaderGraphDirFrom;
        [SerializeField, HideInInspector]
        public string ShaderGraphDirTo;
        [SerializeField, HideInInspector]
        public string VFXGraphDirFrom;
        [SerializeField, HideInInspector]
        public string VFXGraphDirTo;
        [SerializeField, HideInInspector]
        public string HDRPConfigDirFrom;
        [SerializeField, HideInInspector]
        public string HDRPConfigDirTo;
        [SerializeField]
        public string srpVersion;
        [SerializeField, HideInInspector]
        public bool srpReady;
        [SerializeField]
        public string storePath = "/MLS_DATA";
        [SerializeField]
        public bool deferredWarningConfirmed;
        [SerializeField]
        public bool useSwitchingOnly;

        #region Scriptable Render Pipeline Files
        #region Common
        [SerializeField, HideInInspector]
        public string srp_Core_SourcesPath;
        [SerializeField, HideInInspector]
        public string srp_Common_ModifySourcesPath;

        // SRP Common Default Source Files
        [SerializeField, HideInInspector]
        public string SRP__ENTITY_LIGHTING__DEFAULT_SOURCE_FILE = "EntityLighting.hlsl";
        [SerializeField, HideInInspector]
        public string SRP__COMMON__DEFAULT_SOURCE_FILE = "Common.hlsl";

        // SRP Common Serach Fragments            
        [SerializeField, HideInInspector]
        public string SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__SEARCH_STRING;
        [SerializeField, HideInInspector]
        public string SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__SEARCH_STRING;
        [SerializeField, HideInInspector]
        public string SRP__COMMON__COMMON_INCLUDES__SEARCH_STRING;

        // SRP Common Patch Source Files
        [SerializeField, HideInInspector]
        public string SRP__COMMON__COMMON_INCLUDES__MODYFI_SOURCE_FILE = "MLS_SRP_Common_Includes.hlsl";
        [SerializeField, HideInInspector]
        public string SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Single_Lightmap_Additions.txt";
        [SerializeField, HideInInspector]
        public string SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Directional_Lightmap_Additions.txt";
        [SerializeField, HideInInspector]
        public string SRP__LIT__COMMON_VARIABLES__MODYFI_SOURCE_FILE = "Variables_Additions.txt";

        // SRP Common Patch Lines
        [SerializeField, HideInInspector]
        public List<string> srp__Common__Common_Includes__Lines = new List<string>();
        [SerializeField, HideInInspector]
        public List<string> srp__Entity_Lighting__Sample_Single_Lightmap__Lines = new List<string>();
        [SerializeField, HideInInspector]
        public List<string> srp__Entity_Lighting__Sample_Directional_Lightmap__Lines = new List<string>();
        [SerializeField, HideInInspector]
        public List<string> srp__Lit__Common_Variables__Lines = new List<string>();

        // SRP Common Patched Signatures
        [SerializeField, HideInInspector]
        public string SRP_COMMON__COMMON_INCLUDES__SIGNATURE = "//<MLS_COMMON_INCLUDES>";
        [SerializeField, HideInInspector]
        public string SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__SIGNATURE = "//<MLS_ENTITY_LIGHTING_SAMLE_SINGLE_LIGHTMAP_ADDITIONS>";
        [SerializeField, HideInInspector]
        public string SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__SIGNATURE = "//<MLS_ENTITY_LIGHTING_SAMLE_DIRECTIONAL_LIGHTMAP_ADDITIONS>";
        [SerializeField, HideInInspector]
        public string SRP__LIT__COMMON_VARIABLES__SIGNATURE = "//<MLS_VARIABLES_ADDITIONS>";
        #endregion

        #region URP
        [SerializeField, HideInInspector]
        public string srp_URP_SourcesPath;
        [SerializeField, HideInInspector]
        public string srp_URP_ModifySourcesPath;

        // Universal RP Default Source Files
        [SerializeField, HideInInspector]
        public string URP__LIT__DEFAULT_SOURCE_FILE = "Shaders/Lit.shader";
        [SerializeField, HideInInspector]
        public string URP__TERRAIN_LIT__DEFAULT_SOURCE_FILE = "Shaders/Terrain/TerrainLit.shader";

#if MT_URP_12_INCLUDED
        [SerializeField, HideInInspector]
        public string URP__LIGHTING__DEFAULT_SOURCE_FILE = "GlobalIllumination.hlsl";
#else
        [SerializeField, HideInInspector]
        public string URP__LIGHTING__DEFAULT_SOURCE_FILE = "Lighting.hlsl";
#endif

        // Universal RP Search Fragments
        [SerializeField, HideInInspector]
        public string URP__LIT__COMMON_VARIABLES__SEARCH_STRING =
            "[HideInInspector] _GlossyReflections(\"EnvironmentReflections\", Float) = 0.0";
        [SerializeField, HideInInspector]
        public string URP__TERRAIN_LIT__COMMON_VARIABLES__SEARCH_STRING =
            "[ToggleUI] _EnableInstancedPerPixelNormal(\"Enable Instanced per-pixel normal\", Float) = 1.0";
#if MT_URP_12_INCLUDED
        [SerializeField, HideInInspector]
        public string URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__SEARCH_STRING =
            "half3 GlossyEnvironmentReflection(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion)";
#else
        [SerializeField, HideInInspector]
        public string URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__SEARCH_STRING =
            "half3 GlossyEnvironmentReflection(half3 reflectVector, half perceptualRoughness, half occlusion)";
#endif

        // URP Patch Source Files 
#if MT_URP_12_INCLUDED
        [SerializeField]
        public string URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__MODYFI_SOURCE_FILE = "Glossy_Environment_Reflection_Additions_12.txt";
#else
        [SerializeField, HideInInspector]
        public string URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__MODYFI_SOURCE_FILE = "Glossy_Environment_Reflection_Additions_7-11.txt";
#endif

        // URP RP Patch Lines       
        [SerializeField, HideInInspector]
        public List<string> urp__Lighting__Glossy_Environment_Reflection__Lines = new List<string>();

        // URP Patched Signatures 
#if MT_URP_12_INCLUDED
        [SerializeField, HideInInspector]
        public string URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__SIGNATURE = "//<MLS_GLOBAL_ILLUMINATION_GLOSSY_ENVIRONMENT_REFLECTION_ADDITIONS>";
#else
        [SerializeField, HideInInspector]
        public string URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__SIGNATURE = "//<MLS_LIGHTING_GLOSSY_ENVIRONMENT_REFLECTION_ADDITIONS>";
#endif
#endregion

        #region HDRP
        [SerializeField, HideInInspector]
        public string srp_HDRP_SourcesPath;
        [SerializeField, HideInInspector]
        public string srp_HDRP_ModifySourcesPath;

        // HDRP Default Source Files        
        [SerializeField, HideInInspector]
        public string HDRP__LIT__DEFAULT_SOURCE_FILE = "Runtime/Material/Lit/Lit.shader";
        [SerializeField, HideInInspector]
        public string HDRP__HDRI_SKY__DEFAULT_SOURCE_FILE = "Runtime/Sky/HDRISky/HDRISky.shader";
        [SerializeField, HideInInspector]
        public string HDRP__TERRAIN_LIT__DEFAULT_SOURCE_FILE = "Runtime/Material/TerrainLit/TerrainLit.shader";
        [SerializeField, HideInInspector]
        public string HDRP__LIGHTING_LOOP_DEF__DEFAULT_SOURCE_FILE = "Runtime/Lighting/LightLoop/LightLoopDef.hlsl";
        [SerializeField, HideInInspector]
        public string HDRP__BUILTINGI__DEFAULT_SOURCE_FILE = "Runtime/Material/BuiltInGIUtilities.hlsl";

        // HDRP Search Fragments
        [SerializeField, HideInInspector]
        public string HDRP__LIT__COMMON_VARIABLES__SEARCH_STRING =
            "[HideInInspector] _DiffusionProfileHash(\"Diffusion Profile Hash\", Float) = 0";
        [SerializeField, HideInInspector]
        public  string HDRP__HDRISKY__GET_SKY_COLOR__SEARCH_STRING =
            "float3 GetSkyColor(float3 dir)";
        [SerializeField, HideInInspector]
        public string HDRP__TERRAIN_LIT__COMMON_VARIABLES__SEARCH_STRING =
            "[HideInInspector] [ToggleUI] _AddPrecomputedVelocity(\"AddPrecomputedVelocity\", Float) = 0.0";
        [SerializeField, HideInInspector]
        public string HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__SEARCH_STRING;
        [SerializeField, HideInInspector]
        public string HDRP__BUILTINGI__SEARCH_STRING =
            "float4 SampleShadowMask(float3 positionRWS, float2 uvStaticLightmap) // normalWS not use for now";

        // HDRP Patch Source Files
        [SerializeField, HideInInspector]
        public string HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__MODYFI_SOURCE_FILE;
        [SerializeField, HideInInspector]
        public string HDRP__HDRI_SKY__GET_SKY_COLOR__MODYFI_SOURCE_FILE;
        [SerializeField, HideInInspector]
        public string HDRP__BUILTINGI__MODYFI_SOURCE_FILE;

        // HDRP Patch Lines    
        [SerializeField, HideInInspector]
        public List<string> hdrp_Lighting_Loop_Def__Sample_Env__Lines = new List<string>();
        [SerializeField, HideInInspector]
        public List<string> hdrp_HDRI_Sky__Get_Sky_Color__Lines = new List<string>();
        [SerializeField, HideInInspector]
        public List<string> hdrp_BuiltInGI__Lines = new List<string>();

        // HDRP Patched Signatures 
        [SerializeField, HideInInspector]
        public string HDRP__HDRI_SKY__GET_SKY_COLOR__SIGNATURE = "//<MLS_HDRI_SKY_GET_SKY_COLOR_ADDITIONS>";
        [SerializeField, HideInInspector]
        public string HDRP__BUILTINGI__SIGNATURE = "//<MLS_BUILTINGI_ADDITIONS>";

#endregion
#endregion
    }
}

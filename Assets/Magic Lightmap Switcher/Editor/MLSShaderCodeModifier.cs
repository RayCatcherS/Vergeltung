using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [ExecuteInEditMode]
    public class MLSShaderCodeModifier : EditorWindow
    {
        public enum RenderPipelineType
        {
            Standard,
            URP,
            HDRP
        }

        private struct CmdProcessArguments
        {
            public enum CommandType
            {
                Copy,
                Delete
            }

            public CmdProcessArguments(CmdProcessArguments data)
            {
                commandType = data.commandType;
                deleteFrom = data.deleteFrom;
                copyFrom = data.copyFrom;
                copyTo = data.copyTo;
            }

            public CommandType commandType;
            public string deleteFrom;
            public string copyFrom;
            public string copyTo;
        }

        private static SystemProperties systemProperties;
        private static MLSShaderCodeModifier shaderCodeModifierWindow;

        private static IEnumerator prepareSRPPackageRoutine;
        private static IEnumerator checkForPatchedRoutine;

        private static bool initialized;
        private static bool hasHDRPDefine;
        private static bool hasURPDefine;
        private const string HDRP_7_PackageDefine = "MT_HDRP_7_INCLUDED";
        private const string HDRP_8_PackageDefine = "MT_HDRP_8_INCLUDED";
        private const string HDRP_9_PackageDefine = "MT_HDRP_9_INCLUDED";
        private const string HDRP_10_PackageDefine = "MT_HDRP_10_INCLUDED";
        private const string HDRP_11_PackageDefine = "MT_HDRP_11_INCLUDED";
        private const string HDRP_12_PackageDefine = "MT_HDRP_12_INCLUDED";
        private const string URP_7_PackageDefine = "MT_URP_7_INCLUDED";
        private const string URP_8_PackageDefine = "MT_URP_9_INCLUDED";
        private const string URP_9_PackageDefine = "MT_URP_9_INCLUDED";
        private const string URP_10_PackageDefine = "MT_URP_10_INCLUDED";
        private const string URP_11_PackageDefine = "MT_URP_11_INCLUDED";
        private const string URP_12_PackageDefine = "MT_URP_12_INCLUDED";
        private static bool manual;
        private static bool standard_RP_Patched;  

        private static string patch_URP_Button_Text = "Patch URP Shaders";
        private static string patch_HDRP_Button_Text = "Patch HDRP Shaders";

        #region Standard Render Pipeline

        private static string standard_RP_SourcesPath;
        private static string standard_RP_ModifySourcesPath;

        // Standard RP Default Source Files
        private static string STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE = "UnityImageBasedLighting.cginc";
        private static string STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE = "UnityPBSLighting.cginc";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE = "UnityShadowLibrary.cginc";
        private static string STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE = "UnityGlobalIllumination.cginc";
        private static string STANDARD__CORE__DEFAULT_SOURCE_FILE = "UnityStandardCore.cginc";

        // Standard RP Additional Includes
        private static string STANDARD__CORE__COMMON__INCLUDE_FILE = "MLS_Standard_Common.cginc";

        // Standard RP Search Fragments
        private static string STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__SEARCH_STRING =
            "half3 Unity_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, Unity_GlossyEnvironmentData glossIn)";
        private static string STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD_GI__SEARCH_STRING =
            "gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__SEARCH_STRING =
            "fixed UnitySampleBakedOcclusion (float2 lightmapUV, float3 worldPos)";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__SEARCH_STRING =
            "fixed4 UnityGetRawBakedOcclusions(float2 lightmapUV, float3 worldPos)";
        private static string STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__SEARCH_STRING =
            "#include \"UnityImageBasedLighting.cginc\"";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__SEARCH_STRING =
            "half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, data.lightmapUV.xy);" +
            "fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER (unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__SEARCH_STRING =
            "half3 env0 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube0), data.probeHDR[0], glossIn);" +
            "half3 env1 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0), data.probeHDR[1], glossIn);";

        // Standard RP Patch Source Files
        private static string STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__MODYFI_SOURCE_FILE = "Unity_Image_Based_Lighting_Glossy_Environment.txt";
        private static string STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD__MODYFI_SOURCE_FILE = "Unity_PBS_Lighting_Standard_Lighting_Additions.txt";
        private static string STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__MODYFI_SOURCE_FILE = "Global_Illumination_Include_Section_Additions.txt";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__MODYFI_SOURCE_FILE = "Global_Illumination_UnityGI_IndirectSpecular_Additions.txt";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__MODYFI_SOURCE_FILE = "Global_Illumination_Unity_GI_Base_Additions.txt";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__MODYFI_SOURCE_FILE = "Unity_Shadow_Library_Unity_Sample_Baked_Occlusion_Additions.txt";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__MODYFI_SOURCE_FILE = "Unity_Shadow_Library_Unity_Get_Raw_Baked_Occlusions_Additions.txt";

        // Standard RP Patch Lines
        private List<string> standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines = new List<string>();
        private List<string> standard__Unity_PBS_Lighting__Lighting_Standard_Lines = new List<string>();
        private List<string> standard__Global_Illumination__Include_Section_Lines = new List<string>();
        private List<string> standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines = new List<string>();
        private List<string> standard__Global_Illumination__Unity_GI_Base_Lines = new List<string>();
        private List<string> standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines = new List<string>();
        private List<string> standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines = new List<string>();
        private List<string> includeSectionAdditions = new List<string>();
        private List<string> fragmentGIBlendReflectionProbes = new List<string>();

        // Standard MLS Patched Signatures
        private static string STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__SIGNATURE = "//<MLS_IMAGE_BASED_LIGHTING_ENVIRONMENT_CUSTOM>";
        private static string STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD__SIGNATURE = "//<MLS_UNITY_PBS_LIGHTING_LIGHTING_STANDARD_ADDITIONS>";
        private static string STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__SIGNATURE = "//<MLS_GLOBAL_ILLUMINATION_INCLUDE_SECTION>";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__SIGNATURE = "//<MLS_GLOBAL_ILLUMINATION_UNITY_GI_INDIRECT_SPECULAR_ADDITIONS>";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__SIGNATURE = "//<MLS_GLOBAL_ILLUMINATION_UNITY_GI_BASE_ADDITIONS>";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__SIGNATURE = "//<MLS_UNITY_SHADOW_LIBRARY_UNITY_SAMPLE_BAKED_OCCLUSION_ADDITIONS>";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__SIGNATURE = "//<MLS_UNITY_SHADOW_LIBRARY_UNITY_GET_RAW_BAKED_OCCLUSIONS_ADDITIONS>";
        #endregion        

        [MenuItem("Tools/Magic Tools/Magic Lightmap Switcher/Prepare Shaders...", priority = 1)]
        static void OpenWindow()
        {
            shaderCodeModifierWindow = (MLSShaderCodeModifier) GetWindow(typeof(MLSShaderCodeModifier), true, "Magic Lightmap Switcher - Patch Shaders");
            shaderCodeModifierWindow.maxSize = new Vector2(250 * EditorGUIUtility.pixelsPerPoint, 100 * EditorGUIUtility.pixelsPerPoint);
            shaderCodeModifierWindow.minSize = shaderCodeModifierWindow.maxSize;
            shaderCodeModifierWindow.position = new Rect(
                Screen.width + shaderCodeModifierWindow.minSize.x * 0.5f,
                Screen.height - shaderCodeModifierWindow.minSize.y * 0.5f,
                shaderCodeModifierWindow.minSize.x,
                shaderCodeModifierWindow.minSize.y);

            shaderCodeModifierWindow.Show();
        }        

        public static void PrepareSRPPackageSRPIteratorUpdate()
        {
            if (prepareSRPPackageRoutine != null && prepareSRPPackageRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= PrepareSRPPackageSRPIteratorUpdate;
        }

        public static void CheckForPatchedIteratorUpdate()
        {
            if (checkForPatchedRoutine != null && checkForPatchedRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= CheckForPatchedIteratorUpdate;
        }

        private static void GetSystemProperties()
        {
            if (systemProperties == null)
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);
                string projectRelativePath = directories[0].Split(new string[] { "Magic Lightmap Switcher" }, StringSplitOptions.None)[0];

                systemProperties = AssetDatabase.LoadAssetAtPath(FileUtil.GetProjectRelativePath(projectRelativePath + "/Magic Lightmap Switcher/Editor/SystemProperties.asset"), typeof(SystemProperties)) as SystemProperties;
            }

            if (systemProperties == null)
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);
                string projectRelativePath = directories[0].Split(new[] { "Magic Lightmap Switcher" }, StringSplitOptions.None)[0];
                
                systemProperties = ScriptableObject.CreateInstance<SystemProperties>();

                AssetDatabase.CreateAsset(systemProperties, FileUtil.GetProjectRelativePath(projectRelativePath + "/Magic Lightmap Switcher/Editor/SystemProperties.asset"));
                AssetDatabase.SaveAssets();
            }
        }

        private static void GetInstalledSRPVersion()
        {
            if (!systemProperties.srpPatching)
            {
                if (string.IsNullOrEmpty(systemProperties.srpVersion))
                {
                    string fullPathToSRP = Path.GetFullPath("Packages/com.unity.render-pipelines.core");

                    if (fullPathToSRP.Contains("@"))
                    {
                        systemProperties.srpVersion = fullPathToSRP.Split(new char[] { '@' })[1];
                    }
                }
            }
            else
            {
                if (!Directory.Exists(Application.dataPath + "/../MLS_SRP_TEMP"))
                {
                    systemProperties.srpPatching = false;
                    return;
                }

                string[] packageInfo = File.ReadAllLines(Application.dataPath + "/../MLS_SRP_TEMP/com.unity.render-pipelines.core/package.json");

                for (int i = 0; i < packageInfo.Length; i++)
                {
                    if (packageInfo[i].Contains("version"))
                    {
                        systemProperties.srpVersion = packageInfo[i].Split(new char[] { ':' })[1].Split(new char[] { '\"' })[1];
                    }
                }
            }            
        }

        [DidReloadScripts]
        static void CheckSRPState()
        {
            GetSystemProperties();
            GetInstalledSRPVersion();

            systemProperties.highDefinitionRPActive = DoesTypeExist("HDRenderPipelineAsset");
            systemProperties.universalRPActive = DoesTypeExist("UniversalRenderPipelineAsset");

            string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            string[] scriptingDefines = scriptingDefine.Split(';');

            if (scriptingDefines.Contains(HDRP_7_PackageDefine) ||
                scriptingDefines.Contains(HDRP_8_PackageDefine) ||
                scriptingDefines.Contains(HDRP_9_PackageDefine) ||
                scriptingDefines.Contains(HDRP_10_PackageDefine) ||
                scriptingDefines.Contains(HDRP_11_PackageDefine) ||
                scriptingDefines.Contains(HDRP_12_PackageDefine))
            {
                hasHDRPDefine = true;
            }

            if (scriptingDefines.Contains(URP_7_PackageDefine) ||
                scriptingDefines.Contains(URP_8_PackageDefine) ||
                scriptingDefines.Contains(URP_9_PackageDefine) ||
                scriptingDefines.Contains(URP_10_PackageDefine) ||
                scriptingDefines.Contains(URP_11_PackageDefine) ||
                scriptingDefines.Contains(URP_12_PackageDefine))
            {
                hasURPDefine = true;
            }

            // HDRP Package Check
            if (systemProperties.highDefinitionRPActive && !hasHDRPDefine)
            {
                if (systemProperties.srpVersion.StartsWith("7"))
                {
                    AddDefine(HDRP_7_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("8"))
                {
                    AddDefine(HDRP_8_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("9"))
                {
                    AddDefine(HDRP_9_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("10"))
                {
                    AddDefine(HDRP_10_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("11"))
                {
                    AddDefine(HDRP_11_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("12"))
                {
                    AddDefine(HDRP_12_PackageDefine, false);
                }
            }
            else if (!systemProperties.highDefinitionRPActive && hasHDRPDefine)
            {
                if (scriptingDefines.Contains(HDRP_7_PackageDefine))
                {
                    RemoveDefine(HDRP_7_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(HDRP_8_PackageDefine))
                {
                    RemoveDefine(HDRP_8_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(HDRP_9_PackageDefine))
                {
                    RemoveDefine(HDRP_9_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(HDRP_10_PackageDefine))
                {
                    RemoveDefine(HDRP_10_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(HDRP_11_PackageDefine))
                {
                    RemoveDefine(HDRP_11_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(HDRP_12_PackageDefine))
                {
                    RemoveDefine(HDRP_12_PackageDefine, false);
                }
            }

            // URP Package Check
            if (systemProperties.universalRPActive && !hasURPDefine)
            {
                if (systemProperties.srpVersion.StartsWith("7"))
                {
                    AddDefine(URP_7_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("8"))
                {
                    AddDefine(URP_8_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("9"))
                {
                    AddDefine(URP_9_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("10"))
                {
                    AddDefine(URP_10_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("11"))
                {
                    AddDefine(URP_11_PackageDefine, false);
                }
                else if (systemProperties.srpVersion.StartsWith("12"))
                {
                    AddDefine(URP_12_PackageDefine, false);
                }
            }
            else if (!systemProperties.universalRPActive && hasURPDefine)
            {
                if (scriptingDefines.Contains(URP_7_PackageDefine))
                {
                    RemoveDefine(URP_7_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(URP_8_PackageDefine))
                {
                    RemoveDefine(URP_8_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(URP_9_PackageDefine))
                {
                    RemoveDefine(URP_9_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(URP_10_PackageDefine))
                {
                    RemoveDefine(URP_10_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(URP_11_PackageDefine))
                {
                    RemoveDefine(URP_11_PackageDefine, false);
                }
                else if (scriptingDefines.Contains(URP_12_PackageDefine))
                {
                    RemoveDefine(URP_12_PackageDefine, false);
                }
            }            

            if (manual)
            {
                manual = false;
                EditorUtility.DisplayDialog("MLS Dependency Checker", "Scripting Define Symbols configured.", "OK");
            }
        }

        public static void Initialize() 
        {
            Application.runInBackground = true;            

            string managedDir = "";

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                managedDir = EditorApplication.applicationPath.Split(new string[] { "Unity.exe" }, StringSplitOptions.None)[0];
            }
            else
            {
                System.Reflection.Assembly entryAssembly = new System.Diagnostics.StackTrace().GetFrames().Last().GetMethod().Module.Assembly;
                managedDir = System.IO.Path.GetDirectoryName(entryAssembly.Location);
            }

            standard_RP_SourcesPath = managedDir + "Data/CGIncludes/";

            if (!Directory.Exists(standard_RP_SourcesPath))
            {
                standard_RP_SourcesPath = managedDir + "/../../CGIncludes/";
            }

            if (!Directory.Exists(standard_RP_SourcesPath))
            {
                UnityEngine.Debug.LogError("Can't find directory: " + standard_RP_SourcesPath);
                return;
            }

            string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);
            string projectRelativePath = Application.dataPath + directories[0].Split(new string[] { "Magic Lightmap Switcher" }, StringSplitOptions.None)[1];
            
            standard_RP_ModifySourcesPath = directories[0] + "/Editor/Dependent Resources/Shader Sources/Standard/";

            GetSystemProperties();
            CheckSRPState();

            if (systemProperties.universalRPActive || systemProperties.highDefinitionRPActive)
            {
                systemProperties.srp_Common_ModifySourcesPath = directories[0] + "/Editor/Dependent Resources/Shader Sources/SRP/Common/";

                GetInstalledSRPVersion();

                if (systemProperties.srpVersion.StartsWith("7"))
                {
                    systemProperties.SRP__COMMON__COMMON_INCLUDES__MODYFI_SOURCE_FILE = "MLS_SRP_Common_Includes_7_8.hlsl";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Single_Lightmap_Additions_7_8.txt";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Directional_Lightmap_Additions_7_8.txt";

                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__MODYFI_SOURCE_FILE = "Lighting_Loop_Def_Additions_7_8.txt";
                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__SEARCH_STRING =
                        "color.rgb = SAMPLE_TEXTURECUBE_ARRAY_LOD_ABSTRACT(_EnvCubemapTextures, s_trilinear_clamp_sampler, texCoord, _EnvSliceSize * index + sliceIdx, lod).rgb;";
                    systemProperties.HDRP__HDRI_SKY__GET_SKY_COLOR__MODYFI_SOURCE_FILE = "HDRI_Sky_Get_Sky_Color_Additions_7-9.txt";
                    systemProperties.HDRP__HDRISKY__GET_SKY_COLOR__SEARCH_STRING = "float3 GetSkyColor(float3 dir)";
                    systemProperties.HDRP__BUILTINGI__MODYFI_SOURCE_FILE = "BuiltInGI_Additions_7-9.txt";

                    systemProperties.SRP__COMMON__COMMON_INCLUDES__SEARCH_STRING =
                        "#endif // UNITY_COMMON_INCLUDED";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__SEARCH_STRING =
                        "real3 SampleSingleLightmap(TEXTURE2D_PARAM(lightmapTex, lightmapSampler), float2 uv, float4 transform, bool encodedLightmap, real4 decodeInstructions)";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__SEARCH_STRING =
                        "real3 SampleDirectionalLightmap(TEXTURE2D_PARAM(lightmapTex, lightmapSampler), TEXTURE2D_PARAM(lightmapDirTex, lightmapDirSampler), float2 uv, float4 transform, float3 normalWS, bool encodedLightmap, real4 decodeInstructions)";
                }
                else if (systemProperties.srpVersion.StartsWith("8"))
                {
                    systemProperties.SRP__COMMON__COMMON_INCLUDES__MODYFI_SOURCE_FILE = "MLS_SRP_Common_Includes_7_8.hlsl";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Single_Lightmap_Additions_7_8.txt";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Directional_Lightmap_Additions_7_8.txt";

                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__MODYFI_SOURCE_FILE = "Lighting_Loop_Def_Additions_7_8.txt";
                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__SEARCH_STRING =
                        "color.rgb = SAMPLE_TEXTURECUBE_ARRAY_LOD_ABSTRACT(_EnvCubemapTextures, s_trilinear_clamp_sampler, texCoord, _EnvSliceSize * index + sliceIdx, lod).rgb;";
                    systemProperties.HDRP__HDRI_SKY__GET_SKY_COLOR__MODYFI_SOURCE_FILE = "HDRI_Sky_Get_Sky_Color_Additions_7-9.txt";
                    systemProperties.HDRP__HDRISKY__GET_SKY_COLOR__SEARCH_STRING = "float3 GetSkyColor(float3 dir)";
                    systemProperties.HDRP__BUILTINGI__MODYFI_SOURCE_FILE = "BuiltInGI_Additions_7-9.txt";

                    systemProperties.SRP__COMMON__COMMON_INCLUDES__SEARCH_STRING =
                        "#endif // UNITY_COMMON_INCLUDED";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__SEARCH_STRING =
                        "real3 SampleSingleLightmap(TEXTURE2D_PARAM(lightmapTex, lightmapSampler), float2 uv, float4 transform, bool encodedLightmap, real4 decodeInstructions)";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__SEARCH_STRING =
                        "real3 SampleDirectionalLightmap(TEXTURE2D_PARAM(lightmapTex, lightmapSampler), TEXTURE2D_PARAM(lightmapDirTex, lightmapDirSampler), float2 uv, float4 transform, float3 normalWS, bool encodedLightmap, real4 decodeInstructions)";
                }
                else if (systemProperties.srpVersion.StartsWith("10"))
                {
                    systemProperties.SRP__COMMON__COMMON_INCLUDES__MODYFI_SOURCE_FILE = "MLS_SRP_Common_Includes_10.hlsl";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Single_Lightmap_Additions_10.txt";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Directional_Lightmap_Additions_10.txt";

                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__MODYFI_SOURCE_FILE = "Lighting_Loop_Def_Additions_10.txt";
                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__SEARCH_STRING =
                        "color.rgb = SAMPLE_TEXTURECUBE_ARRAY_LOD_ABSTRACT(_EnvCubemapTextures, s_trilinear_clamp_sampler, texCoord, _EnvSliceSize * index + sliceIdx, lod).rgb;";
                    systemProperties.HDRP__HDRI_SKY__GET_SKY_COLOR__MODYFI_SOURCE_FILE = "HDRI_Sky_Get_Sky_Color_Additions_10-11.txt";
                    systemProperties.HDRP__HDRISKY__GET_SKY_COLOR__SEARCH_STRING = "float3 GetSkyColor(float3 dir)";
                    systemProperties.HDRP__BUILTINGI__MODYFI_SOURCE_FILE = "BuiltInGI_Additions_10-11.txt";

                    systemProperties.SRP__COMMON__COMMON_INCLUDES__SEARCH_STRING =
                        "#endif // UNITY_COMMON_INCLUDED";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__SEARCH_STRING =
                        "real3 SampleSingleLightmap(TEXTURE2D_LIGHTMAP_PARAM(lightmapTex, lightmapSampler), LIGHTMAP_EXTRA_ARGS, float4 transform, bool encodedLightmap, real4 decodeInstructions)";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__SEARCH_STRING =
                        "void SampleDirectionalLightmap(TEXTURE2D_LIGHTMAP_PARAM(lightmapTex, lightmapSampler), TEXTURE2D_LIGHTMAP_PARAM(lightmapDirTex, lightmapDirSampler), LIGHTMAP_EXTRA_ARGS, float4 transform,";
                }
                else if (systemProperties.srpVersion.StartsWith("11"))
                {
                    systemProperties.SRP__COMMON__COMMON_INCLUDES__MODYFI_SOURCE_FILE = "MLS_SRP_Common_Includes_10.hlsl";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Single_Lightmap_Additions_10.txt";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Directional_Lightmap_Additions_11.txt";

                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__MODYFI_SOURCE_FILE = "Lighting_Loop_Def_Additions_10.txt";
                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__SEARCH_STRING =
                        "color.rgb = SAMPLE_TEXTURECUBE_ARRAY_LOD_ABSTRACT(_EnvCubemapTextures, s_trilinear_clamp_sampler, texCoord, _EnvSliceSize * index + sliceIdx, lod).rgb;";
                    systemProperties.HDRP__HDRI_SKY__GET_SKY_COLOR__MODYFI_SOURCE_FILE = "HDRI_Sky_Get_Sky_Color_Additions_10-11.txt";
                    systemProperties.HDRP__HDRISKY__GET_SKY_COLOR__SEARCH_STRING = "float3 GetSkyColor(float3 dir)";
                    systemProperties.HDRP__BUILTINGI__MODYFI_SOURCE_FILE = "BuiltInGI_Additions_10-11.txt";

                    systemProperties.SRP__COMMON__COMMON_INCLUDES__SEARCH_STRING =
                        "#endif // UNITY_COMMON_INCLUDED";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__SEARCH_STRING =
                        "real3 SampleSingleLightmap(TEXTURE2D_LIGHTMAP_PARAM(lightmapTex, lightmapSampler), LIGHTMAP_EXTRA_ARGS, float4 transform, bool encodedLightmap, real4 decodeInstructions)";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__SEARCH_STRING =
                        "void SampleDirectionalLightmap(TEXTURE2D_LIGHTMAP_PARAM(lightmapTex, lightmapSampler), TEXTURE2D_LIGHTMAP_PARAM(lightmapDirTex, lightmapDirSampler), LIGHTMAP_EXTRA_ARGS, float4 transform,";
                }
                else if (systemProperties.srpVersion.StartsWith("12"))
                {
                    systemProperties.SRP__COMMON__COMMON_INCLUDES__MODYFI_SOURCE_FILE = "MLS_SRP_Common_Includes_10.hlsl";
                    systemProperties.SRP__COMMON__COMMON_INCLUDES__SEARCH_STRING = "#endif // UNITY_COMMON_INCLUDED";

                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Single_Lightmap_Additions_10.txt";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__MODYFI_SOURCE_FILE = "Entity_Lighting_Sample_Directional_Lightmap_Additions_11.txt";

                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__MODYFI_SOURCE_FILE = "Lighting_Loop_Def_Additions_10.txt";
                    systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__SEARCH_STRING =
                        "color.rgb = SAMPLE_TEXTURECUBE_ARRAY_LOD_ABSTRACT(_EnvCubemapTextures, s_trilinear_clamp_sampler, texCoord, _EnvSliceSize * index + sliceIdx, lod).rgb;";

                    systemProperties.HDRP__HDRI_SKY__GET_SKY_COLOR__MODYFI_SOURCE_FILE = "HDRI_Sky_Get_Sky_Color_Additions_10-11.txt";
                    systemProperties.HDRP__HDRISKY__GET_SKY_COLOR__SEARCH_STRING = "float3 GetSkyColor(float3 dir)";

                    systemProperties.HDRP__BUILTINGI__MODYFI_SOURCE_FILE = "BuiltInGI_Additions_10-11.txt";


                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__SEARCH_STRING =
                        "real3 SampleSingleLightmap(TEXTURE2D_LIGHTMAP_PARAM(lightmapTex, lightmapSampler), LIGHTMAP_EXTRA_ARGS, float4 transform, bool encodedLightmap, real4 decodeInstructions)";
                    systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__SEARCH_STRING =
                        "void SampleDirectionalLightmap(TEXTURE2D_LIGHTMAP_PARAM(lightmapTex, lightmapSampler), TEXTURE2D_LIGHTMAP_PARAM(lightmapDirTex, lightmapDirSampler), LIGHTMAP_EXTRA_ARGS, float4 transform,";

                    systemProperties.URP__LIGHTING__DEFAULT_SOURCE_FILE = "GlobalIllumination.hlsl";
                    systemProperties.URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__SEARCH_STRING = "half3 GlossyEnvironmentReflection(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion)";
                    systemProperties.URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__MODYFI_SOURCE_FILE = "Glossy_Environment_Reflection_Additions_12.txt";
                    systemProperties.URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__SIGNATURE = "//<MLS_GLOBAL_ILLUMINATION_GLOSSY_ENVIRONMENT_REFLECTION_ADDITIONS>";
                }
                else
                {
                    EditorUtility.DisplayDialog("Magic Lightmap Switcher",
                        "The SRP version is not supported by the plugin. " +
                        "Contact the developer using any of the available " +
                        "communication channels to quickly fix this problem.",
                        "OK");
                }
            }
            else
            {
                standard_RP_Patched = CheckForStandardRPPatched(false);
                systemProperties.standardRPPatched = standard_RP_Patched;
            }

            if (systemProperties.universalRPActive)
            {
                systemProperties.srp_URP_ModifySourcesPath = directories[0] + "/Editor/Dependent Resources/Shader Sources/SRP/URP/";
            }

            if (systemProperties.highDefinitionRPActive)
            {
                systemProperties.srp_HDRP_ModifySourcesPath = directories[0] + "/Editor/Dependent Resources/Shader Sources/SRP/HDRP/";
            }

            EditorUtility.SetDirty(systemProperties);
            initialized = true;
        }        

        private static void PrintPatchReport(RenderPipelineType type, List<string> patchErrors = null)
        {
            if (patchErrors != null)
            {
                StringBuilder sb = new StringBuilder();
                string renderPipeline = "";

                for (int i = 0; i < patchErrors.Count; i++)
                {
                    if (i < patchErrors.Count - 1)
                    {
                        sb.Append(patchErrors[i] + ", ");
                    }
                    else
                    {
                        sb.Append(patchErrors[i]);
                    }
                }

                switch (type)
                {
                    case RenderPipelineType.Standard:
                        renderPipeline = "Render Pipeline: Standard";
                        break;
                    case RenderPipelineType.URP:
                        renderPipeline = "Render Pipeline: URP " + systemProperties.srpVersion;
                        break;
                    case RenderPipelineType.HDRP:
                        renderPipeline = "Render Pipeline: HDRP " + systemProperties.srpVersion;
                        break;
                }

                Debug.LogErrorFormat("" +
                    "<color=cyan>MLS:</color> Patch error of [" + sb.ToString() + "] " + "Unity version: " + Application.unityVersion.ToString() + ". " + renderPipeline +
                    "\r\n" +
                    "Show the text of this error to the developer.");
            }
            else
            {
                switch (type)
                {
                    case RenderPipelineType.Standard:
                        Debug.LogFormat("<color=cyan>MLS:</color> Standard Shaders patched successfully.");
                        break;
                    case RenderPipelineType.URP:
                        Debug.LogFormat("<color=cyan>MLS:</color> URP " + systemProperties.srpVersion + " patched successfully.");
                        break;
                    case RenderPipelineType.HDRP:
                        Debug.LogFormat("<color=cyan>MLS:</color> HDRP " + systemProperties.srpVersion + " patched successfully.");
                        break;
                }
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (!initialized)
            {
                Initialize();
            }
            else
            {
                CheckSRPState();
            }

            GetSystemProperties();

            MLSEditorUtils.InitStyles();

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("These changes will be applied to the base Unity shaders.", MLSEditorUtils.captionStyle);

            GUILayout.Label(
                "Changes do not affect the basic functionality of the shaders, " +
                "they only contain some extras to support real-time lightmap blending.", MLSEditorUtils.labelCenteredStyle);

            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUI.skin.box);

            if ((!systemProperties.universalRPActive && !systemProperties.highDefinitionRPActive) || systemProperties.standardRPActive)
            {
                GUILayout.Label("Standard Shaders", MLSEditorUtils.caption_1_Style);

                if (File.Exists(standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE))
                {
                    standard_RP_Patched = true;
                }
                else
                {
                    standard_RP_Patched = false;
                }

                if (standard_RP_Patched)
                {
                    systemProperties.standardRPPatched = true;

                    if (GUILayout.Button("Restore Standard RP Shaders", MLSEditorUtils.bigButtonStyle))
                    {
                        RestoreStandardRPShaders();
                        systemProperties.prevTimeSinceStartup = 0;
                        standard_RP_Patched = CheckForStandardRPPatched(false);
                    }
                }
                else
                {
                    systemProperties.standardRPPatched = false;

                    if (GUILayout.Button("Patch Standard RP Shaders", MLSEditorUtils.bigButtonStyle))
                    {
                        PatchStandardRPShaders();
                        systemProperties.editorRestarted = false;
                        systemProperties.prevTimeSinceStartup = 0;
                        standard_RP_Patched = CheckForStandardRPPatched(true);
                    }
                }

                systemProperties.standardRPActive = true;
            }
            else
            {
                if (GUILayout.Button("Use Standard RP", MLSEditorUtils.bigButtonStyle))
                {
                    systemProperties.standardRPActive = true;
                }

                GUILayout.Label("SRP Packages", MLSEditorUtils.caption_1_Style);
            }

            if (systemProperties.universalRPActive)
            {
                systemProperties.universalRPActive = true;

                EditorGUILayout.HelpBox(
                "Your project contains a URP " + systemProperties.srpVersion + " package.", MessageType.Info);

                if (systemProperties.universalRPPatched)
                {
                    if (GUILayout.Button("Restore URP Shaders", MLSEditorUtils.bigButtonStyle))
                    {
                        systemProperties.universalRPPatched = false;
                        RestoreSRPShaders(RenderPipelineType.URP, true);
                    }
                }
                else
                {
                    if (!File.Exists(systemProperties.srp_URP_ModifySourcesPath + "_URP_SUPPORT_IMPORTED_"))
                    {
                        EditorGUILayout.HelpBox(
                            "You have not imported the URP support package. " +
                            "It is located in the Magic Lightmap Switcher/Support Packages folder", MessageType.Error);
                    }
                    else
                    {
                        if (GUILayout.Button(patch_URP_Button_Text, MLSEditorUtils.bigButtonStyle))
                        {
                            systemProperties.standardRPActive = false;
                            systemProperties.editorRestarted = true;
                            prepareSRPPackageRoutine = PrepareSRPPackage(RenderPipelineType.URP);
                            EditorApplication.update += PrepareSRPPackageSRPIteratorUpdate;
                        }                        
                    }
                }
            }

            if (systemProperties.highDefinitionRPActive)
            {
                EditorGUILayout.HelpBox(
                "Your project contains a HDRP " + systemProperties.srpVersion + " package.", MessageType.Info);

                if (systemProperties.highDefinitionRPPatched)
                {
                    if (GUILayout.Button("Restore HDRP Shaders", MLSEditorUtils.bigButtonStyle))
                    {
                        systemProperties.highDefinitionRPPatched = false;
                        RestoreSRPShaders(RenderPipelineType.HDRP, true);
                    }
                }
                else
                {
                    if (!File.Exists(systemProperties.srp_HDRP_ModifySourcesPath + "_HDRP_SUPPORT_IMPORTED_"))
                    {
                        EditorGUILayout.HelpBox(
                            "You have not imported the HDRP support package. " +
                            "It is located in the Magic Lightmap Switcher/Support Packages folder", MessageType.Error);
                    }
                    else
                    {
                        if (GUILayout.Button(patch_HDRP_Button_Text, MLSEditorUtils.bigButtonStyle))
                        {
                            systemProperties.standardRPActive = false;
                            systemProperties.editorRestarted = true;
                            prepareSRPPackageRoutine = PrepareSRPPackage(RenderPipelineType.HDRP);
                            EditorApplication.update += PrepareSRPPackageSRPIteratorUpdate;
                        }
                    }
                }
            }

            EditorUtility.SetDirty(systemProperties);

            GUILayout.EndVertical();

            if ((Event.current.type == EventType.Repaint))
            {
                Rect templateEnd = GUILayoutUtility.GetLastRect();

                if (shaderCodeModifierWindow != null)
                {
                    shaderCodeModifierWindow.minSize = new Vector2(shaderCodeModifierWindow.minSize.x, templateEnd.position.y + templateEnd.height);
                }
            }
        }

        public static bool CheckForStandardRPPatched(bool printReport)
        {
            int patchedFragments = 0;
            int filePatchedFragments = 0;
            List<string> patchErrors = new List<string>();

            try
            {
                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__SIGNATURE == line.Trim() ||
                            STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__SIGNATURE == line.Trim() ||
                            STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                            filePatchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 3)
                    {
                        patchErrors.Add("GlobalIllumination.cginc");
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 1)
                    {
                        patchErrors.Add("ImageBasedLighting.cginc");
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 1)
                    {
                        patchErrors.Add("PBSLighting.cginc");
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 1)
                    {
                        patchErrors.Add("UnityShadowLibrary.cginc (Deferred Section)");
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 1)
                    {
                        patchErrors.Add("UnityShadowLibrary.cginc (Forward Section)");
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("The file could not be read: " + e.Message);
                return false;
            }

            if (patchedFragments == 10)
            {
                if (printReport)
                {
                    PrintPatchReport(RenderPipelineType.Standard);
                }

                return true;
            }
            else
            {
                if (printReport)
                {
                    PrintPatchReport(RenderPipelineType.Standard, patchErrors);
                }

                return false;
            }
        }

        private static IEnumerator PrepareSRPPackage(RenderPipelineType srp)
        { 
            string finalDir = "";
            string sourceDir = "";
            string destDir = "";

            switch (srp)
            {
                case RenderPipelineType.URP:
                    patch_URP_Button_Text = "Patching Process...";                    

                    yield return null;

                    sourceDir = Application.dataPath + "/../Library/PackageCache/com.unity.render-pipelines.core@" + systemProperties.srpVersion;
                    destDir = Application.dataPath + "/../Packages/com.unity.render-pipelines.core@" + systemProperties.srpVersion;

                    if (Directory.Exists(destDir))
                    {
                        Directory.Delete(destDir, true);
                    }

                    Directory.Move(sourceDir, destDir);

                    systemProperties.srp_Core_SourcesPath = destDir + "/ShaderLibrary/";

                    sourceDir = Application.dataPath + "/../Library/PackageCache/com.unity.render-pipelines.universal@" + systemProperties.srpVersion;
                    destDir = Application.dataPath + "/../Packages/com.unity.render-pipelines.universal@" + systemProperties.srpVersion;

                    systemProperties.srp_URP_SourcesPath = destDir + "/";

                    if (Directory.Exists(destDir))
                    {
                        Directory.Delete(destDir, true);
                    }

                    Directory.Move(sourceDir, destDir);

                    PatchSRPShaders(RenderPipelineType.URP);
                    break;
                case RenderPipelineType.HDRP:
                    patch_HDRP_Button_Text = "Patching Process...";

                    yield return null;

                    sourceDir = Application.dataPath + "/../Library/PackageCache/com.unity.render-pipelines.core@" + systemProperties.srpVersion;
                    destDir = Application.dataPath + "/../Packages/com.unity.render-pipelines.core@" + systemProperties.srpVersion;

                    if (Directory.Exists(destDir))
                    {
                        Directory.Delete(destDir, true);
                    }

                    Directory.Move(sourceDir, destDir);

                    systemProperties.srp_Core_SourcesPath = destDir + "/ShaderLibrary/";

                    sourceDir = Application.dataPath + "/../Library/PackageCache/com.unity.render-pipelines.high-definition@" + systemProperties.srpVersion;
                    destDir = Application.dataPath + "/../Packages/com.unity.render-pipelines.high-definition@" + systemProperties.srpVersion;

                    systemProperties.srp_HDRP_SourcesPath = destDir + "/";

                    if (Directory.Exists(destDir))
                    {
                        Directory.Delete(destDir, true);
                    }

                    Directory.Move(sourceDir, destDir);

                    PatchSRPShaders(RenderPipelineType.HDRP);
                    break;
            }
        }

        public static void CheckForSRPPatched(RenderPipelineType srp, bool printReport)
        { 
            int targetPatchetFragments = 0;
            int patchedFragments = 0;
            int filePatchedFragments = 0;
            List<string> patchErrors = new List<string>();

            switch (srp)
            {
                case RenderPipelineType.URP:
                    targetPatchetFragments = 6;
                    break;
                case RenderPipelineType.HDRP:
                    if (systemProperties.srpVersion.StartsWith("10") || systemProperties.srpVersion.StartsWith("11"))
                    {
                        targetPatchetFragments = 7;
                    }
                    else
                    {
                        targetPatchetFragments = 7;
                    }
                    break;
            }            

            try
            {
                using (StreamReader reader = new StreamReader(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__COMMON__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) && (systemProperties.SRP_COMMON__COMMON_INCLUDES__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                            filePatchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 1)
                    {
                        patchErrors.Add("Common.hlsl");
                    }
                }

                using (StreamReader reader = new StreamReader(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__ENTITY_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__SIGNATURE == line.Trim() ||
                            systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                            filePatchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 2)
                    {
                        patchErrors.Add("EntityLighting.hlsl");
                    }
                }

                switch (srp)
                {
                    case RenderPipelineType.URP:
                        using (StreamReader reader = new StreamReader(systemProperties.srp_URP_SourcesPath + "/ShaderLibrary/" + systemProperties.URP__LIGHTING__DEFAULT_SOURCE_FILE))
                        {
                            filePatchedFragments = 0;

                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();

                                if (!string.IsNullOrEmpty(line) && (systemProperties.URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__SIGNATURE == line.Trim()))
                                {
                                    patchedFragments++;
                                    filePatchedFragments++;
                                }
                            }

                            if (filePatchedFragments != 1)
                            {
                                if (systemProperties.srpVersion.StartsWith("12"))
                                {
                                    patchErrors.Add("GlobalIllumitation.hlsl");
                                }
                                else
                                {
                                    patchErrors.Add("Lighting.hlsl");
                                }
                            }
                        }

                        using (StreamReader reader = new StreamReader(systemProperties.srp_URP_SourcesPath + systemProperties.URP__LIT__DEFAULT_SOURCE_FILE))
                        {
                            filePatchedFragments = 0;


                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();

                                if (!string.IsNullOrEmpty(line) && (systemProperties.SRP__LIT__COMMON_VARIABLES__SIGNATURE == line.Trim()))
                                {
                                    patchedFragments++;
                                    filePatchedFragments++;
                                }
                            }

                            if (filePatchedFragments != 1)
                            {
                                patchErrors.Add("Lit.shader");
                            }
                        }

                        using (StreamReader reader = new StreamReader(systemProperties.srp_URP_SourcesPath + systemProperties.URP__TERRAIN_LIT__DEFAULT_SOURCE_FILE))
                        {
                            filePatchedFragments = 0;

                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();

                                if (!string.IsNullOrEmpty(line) && (systemProperties.SRP__LIT__COMMON_VARIABLES__SIGNATURE == line.Trim()))
                                {
                                    patchedFragments++;
                                    filePatchedFragments++;
                                }
                            }

                            if (filePatchedFragments != 1)
                            {
                                patchErrors.Add("TerrainLit.shader");
                            }
                        }
                        break;
                    case RenderPipelineType.HDRP:  
                        using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__LIT__DEFAULT_SOURCE_FILE))
                        {
                            filePatchedFragments = 0;

                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();

                                if (!string.IsNullOrEmpty(line) && (systemProperties.SRP__LIT__COMMON_VARIABLES__SIGNATURE == line.Trim()))
                                {
                                    patchedFragments++;
                                    filePatchedFragments++;
                                }
                            }

                            if (filePatchedFragments != 1)
                            {
                                patchErrors.Add("Lit.shader");
                            }
                        }

                        using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__TERRAIN_LIT__DEFAULT_SOURCE_FILE))
                        {
                            filePatchedFragments = 0;

                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();

                                if (!string.IsNullOrEmpty(line) && (systemProperties.SRP__LIT__COMMON_VARIABLES__SIGNATURE == line.Trim()))
                                {
                                    patchedFragments++;
                                    filePatchedFragments++;
                                }
                            }

                            if (filePatchedFragments != 1)
                            {
                                patchErrors.Add("TerrainLit.shader");
                            }
                        }

                        using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__BUILTINGI__DEFAULT_SOURCE_FILE))
                        {
                            filePatchedFragments = 0;

                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();

                                if (!string.IsNullOrEmpty(line) && (systemProperties.HDRP__BUILTINGI__SIGNATURE == line.Trim()))
                                {
                                    patchedFragments++;
                                    filePatchedFragments++;
                                }
                            }

                            if (filePatchedFragments != 1)
                            {
                                patchErrors.Add("BuiltInGI.hlsl");
                            }
                        }

                        using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__HDRI_SKY__DEFAULT_SOURCE_FILE))
                        {
                            filePatchedFragments = 0;

                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();

                                if (!string.IsNullOrEmpty(line) && (systemProperties.HDRP__HDRI_SKY__GET_SKY_COLOR__SIGNATURE == line.Trim()))
                                {
                                    patchedFragments++;
                                    filePatchedFragments++;
                                }
                            }

                            if (filePatchedFragments != 1)
                            {
                                patchErrors.Add("HDRISky.Shader");
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("The file could not be read: " + e.Message);

                switch (srp)
                {
                    case RenderPipelineType.URP:
                        systemProperties.universalRPPatched = false;
                        break;
                    case RenderPipelineType.HDRP:
                        systemProperties.highDefinitionRPPatched = false;
                        break;
                }
            }

            if (patchedFragments == targetPatchetFragments)
            {
                if (printReport)
                {
                    PrintPatchReport(srp);
                }

                switch (srp)
                {
                    case RenderPipelineType.URP:
                        systemProperties.universalRPPatched = true;  
                        break;
                    case RenderPipelineType.HDRP:
                        systemProperties.highDefinitionRPPatched = true;
                        break;
                }

                if (EditorUtility.DisplayDialog(
                        "Magic Lightmap Switcher",
                        "You must restart the editor for the changes to take effect.", "OK"))
                {
                    systemProperties.useSwitchingOnly = false;
                    systemProperties.editorRestarted = true;
                    EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                }
                else
                {
                    systemProperties.useSwitchingOnly = false;
                    systemProperties.editorRestarted = false;
                    Debug.LogFormat("<color=cyan>MLS:</color> You won't be able to use lightmap blending and switching until the editor is restarted.");
                }
            }
            else
            {
                if (printReport)
                {
                    PrintPatchReport(srp, patchErrors);
                }

                switch (srp)
                {
                    case RenderPipelineType.URP:
                        systemProperties.universalRPPatched = false;
                        break;
                    case RenderPipelineType.HDRP:
                        systemProperties.highDefinitionRPPatched = false;
                        break;
                }
            }

            systemProperties.restoring = false;
        }

        private static void RunAsAdmin(List<CmdProcessArguments> argumentsList)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(@"/c ");

            for (int i = 0; i < argumentsList.Count; i++)
            {
                switch (argumentsList[i].commandType)
                {
                    case CmdProcessArguments.CommandType.Copy:
                        stringBuilder.Append("copy /y \"" + argumentsList[i].copyFrom.Replace("/", @"\") + "\" \"" + argumentsList[i].copyTo.Replace("/", @"\") + "\" && ");
                        break;
                    case CmdProcessArguments.CommandType.Delete:
                        stringBuilder.Append("del /q \"" + argumentsList[i].deleteFrom.Replace("/", @"\") + "\" && ");
                        break;
                }
            }

            stringBuilder.Append("pause");

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.FileName = "cmd.exe";
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;
            startInfo.Arguments = stringBuilder.ToString();

            try
            {
                System.Diagnostics.Process process =  new System.Diagnostics.Process();

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void PatchStandardRPShaders()
        {
            CmdProcessArguments arguments = new CmdProcessArguments();
            List<CmdProcessArguments> processArguments = new List<CmdProcessArguments>();

            standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines.Clear();
            standard__Unity_PBS_Lighting__Lighting_Standard_Lines.Clear();
            standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines.Clear();
            standard__Global_Illumination__Include_Section_Lines.Clear();
            standard__Global_Illumination__Unity_GI_Base_Lines.Clear();
            standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines.Clear();
            standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines.Clear();
            includeSectionAdditions.Clear();
            fragmentGIBlendReflectionProbes.Clear();
            
            bool USEFTRACE = false;

            #region Read Modified Sources

            try
            {
                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Global_Illumination__Include_Section_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Unity_PBS_Lighting__Lighting_Standard_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Global_Illumination__Unity_GI_Base_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines.Add(reader.ReadLine());
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogErrorFormat("<color=cyan>MLS:</color> The file could not be read: " + e.Message);
            }

            #endregion

            #region Buckup Original Files

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_ModifySourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE, standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE);
                }
            }

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original";

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE, standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original");
                }
            }

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original";

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE, standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original");
                }
            }

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original";

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE, standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original");
                }
            }

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original";

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE, standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original");
                }
            }

            RunAsAdmin(processArguments);

            #endregion

            #region Patch Shader Files

            List<string> fileLines = new List<string>();
            processArguments.Clear();

            try
            {
                #region Global Illumination

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        
                        if (!USEFTRACE && !string.IsNullOrEmpty(line) && line.Trim() == "//<FTRACEV1.0>")
                        {
                            USEFTRACE = true;
                            
                            for (int i = 0; i < standard__Global_Illumination__Include_Section_Lines.Count; i++)
                            {
                                fileLines.Add(standard__Global_Illumination__Include_Section_Lines[i]);
                            }

                            fileLines.Add("");
                            fileLines.Add(line);
                            continue;
                        }

                        if (!USEFTRACE && !string.IsNullOrEmpty(line) && STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__SEARCH_STRING.Contains(line.Trim()))
                        {
                            for (int i = 0; i < standard__Global_Illumination__Include_Section_Lines.Count; i++)
                            {
                                fileLines.Add(standard__Global_Illumination__Include_Section_Lines[i]);
                            }

                            fileLines.Add("");
                            fileLines.Add(line);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__SEARCH_STRING.Contains(line.Trim()))
                        {
                            if (line.Trim().Contains("env0"))
                            {
                                fileLines.Add("");

                                for (int i = 0; i < 28; i++)
                                {
                                    fileLines.Add(standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines[i]);
                                }

                                fileLines.Add("");
                            }

                            if (line.Trim().Contains("env1"))
                            {
                                for (int i = 29; i < 56; i++)
                                {
                                    fileLines.Add(standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines[i]);
                                }

                                fileLines.Add("");
                            }
                            continue;
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__SEARCH_STRING.Contains(line.Trim()))
                        {
                            if (line.Trim().Contains("half4 bakedColorTex"))
                            {
                                fileLines.Add("");

                                for (int i = 0; i < 6; i++)
                                {
                                    fileLines.Add(standard__Global_Illumination__Unity_GI_Base_Lines[i]);
                                }

                                fileLines.Add("");
                            }

                            if (line.Trim().Contains("fixed4 bakedDirTex"))
                            {
                                for (int i = 7; i < 15; i++)
                                {
                                    fileLines.Add(standard__Global_Illumination__Unity_GI_Base_Lines[i]);
                                }

                                fileLines.Add("");
                            }
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                if (!File.Exists(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    File.Create(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE).Close();
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                USEFTRACE = false;
                fileLines.Clear();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(
                        standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE,
                        standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE);
                }

                #endregion

                #region PBS Lightnig

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    bool skipLine = false;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (skipLine)
                        {
                            skipLine = false;
                            continue;
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD_GI__SEARCH_STRING.Contains(line.Trim()))
                        {
                            skipLine = true;

                            fileLines.Add(line);
                            fileLines.Add("#endif");
                            fileLines.Add("");

                            for (int i = 0; i < standard__Unity_PBS_Lighting__Lighting_Standard_Lines.Count; i++)
                            {
                                fileLines.Add(standard__Unity_PBS_Lighting__Lighting_Standard_Lines[i]);
                            }

                            fileLines.Add("");
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                if (!File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    File.Create(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE).Close();
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                fileLines.Clear();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(
                        standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE,
                        standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE);
                }

                #endregion

                #region Image Based Lighting

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) && STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__SEARCH_STRING == line.Trim())
                        {
                            fileLines.Add("");

                            for (int i = 0; i < standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines.Count; i++)
                            {
                                fileLines.Add(standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines[i]);
                            }

                            fileLines.Add("");
                            fileLines.Add(line);
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                if (!File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    File.Create(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE).Close();
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                fileLines.Clear();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(
                        standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE,
                        standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE);
                }

                #endregion

                #region Shadow Library

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    bool skipLine = false;
                    bool fragmentFound = false;
                    bool replaceStartLine = false;
                    bool replaceEndLine = false;
                    List<string> tempLines = new List<string>();

                    while (!reader.EndOfStream)
                    {
                        if (skipLine)
                        {
                            skipLine = false;
                            continue;
                        }

                        string line = reader.ReadLine();

                        if (fragmentFound)
                        {
                            if (!replaceStartLine)
                            {
                                if (!string.IsNullOrEmpty(line) && line.Trim() == "#if defined (SHADOWS_SHADOWMASK)")
                                {
                                    replaceStartLine = true;

                                    for (int i = 0; i < standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines.Count; i++)
                                    {
                                        fileLines.Add(standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines[i]);
                                    }
                                }
                                else
                                {
                                    fileLines.Add(line);
                                    continue;
                                }
                            }

                            if (!replaceEndLine)
                            {
                                if (!string.IsNullOrEmpty(line) && line.Trim() == "return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));")
                                {
                                    fragmentFound = false;
                                    replaceEndLine = true;
                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__SEARCH_STRING == line.Trim())
                        {
                            fileLines.Add(line);
                            fragmentFound = true;
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                if (!File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    File.Create(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE).Close();
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                fileLines.Clear();

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    bool skipLine = false;
                    bool fragmentFound = false;
                    bool replaceStartLine = false;
                    bool replaceEndLine = false;
                    bool isNGSSModified = false;

                    string target = "";

                    while (!reader.EndOfStream)
                    { 
                        if (skipLine)
                        {
                            skipLine = false;
                            continue;
                        }

                        string line = reader.ReadLine();
                        
                        if (!string.IsNullOrEmpty(line) && line.Trim() == "#ifdef USEFTRACE")
                        {
                            USEFTRACE = true;
                        }

                        if (!isNGSSModified)
                        {
                            if (!string.IsNullOrEmpty(line) && line.Trim() == "//NGSS SUPPORT")
                            {
                                isNGSSModified = true;
                                target = "return UNITY_SAMPLE_TEX2D_SAMPLER(unity_ShadowMask, unity_Lightmap, lightmapUV.xy);//Unity 2017 and below";
                            }
                            else
                            {
                                target = "return UNITY_SAMPLE_TEX2D(unity_ShadowMask, lightmapUV.xy);";
                            }
                        }

                        if (fragmentFound)
                        {
                            if (!replaceStartLine)
                            {
                                if (!string.IsNullOrEmpty(line) && line.Trim() == target)
                                {
                                    replaceStartLine = true;

                                    for (int i = 0; i < standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines.Count; i++)
                                    {
                                        fileLines.Add(standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines[i]);
                                    }

                                    if (USEFTRACE)
                                    {
                                        fileLines.Add("#endif");
                                    }
                                }
                                else
                                {
                                    fileLines.Add(line);
                                    continue;
                                }
                            }

                            if (!replaceEndLine)
                            {
                                if (!string.IsNullOrEmpty(line) && line.Trim() == "#else")
                                {
                                    fileLines.Add(line);
                                    fragmentFound = false;
                                    replaceEndLine = true;
                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__SEARCH_STRING == line.Trim())
                        {
                            fileLines.Add(line);
                            fragmentFound = true;
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                fileLines.Clear();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(
                        standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE,
                        standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE);
                }

                #endregion

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    RunAsAdmin(processArguments);
                }

                if (File.Exists(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    File.Delete(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE);
                }

                if (File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    File.Delete(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE);
                }

                if (File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    File.Delete(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE);
                }

                if (File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    File.Delete(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("The file could not be read: " + e.Message);
            }
            #endregion

            if (EditorUtility.DisplayDialog(
                "Magic Lightmap Switcher",
                "You must restart the editor for the changes to take effect. Restart now?", "Yes", "No, I will do it later."))
            {
                systemProperties.editorRestarted = true;
                EditorApplication.OpenProject(Directory.GetCurrentDirectory());
            }
            else
            {
                systemProperties.editorRestarted = false;
                Debug.LogFormat("<color=cyan>MLS:</color> You won't be able to use lightmap blending and switching until the editor is restarted.");
            }
        }

        private static bool CheckForMLSTags(string path, string tag)
        {
            bool result = false;

            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (!string.IsNullOrEmpty(line) && line.Trim().Contains(tag))
                    {
                        //Debug.Log("File: " + path + " patched successfully.");
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private void RestoreStandardRPShaders()
        {
            CmdProcessArguments arguments;
            List<CmdProcessArguments> processArguments = new List<CmdProcessArguments>();

            if (File.Exists(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE);
                }
            }

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                RunAsAdmin(processArguments);
            }
        }

        private static void PatchSRPShaders(RenderPipelineType srp)
        {
            bool srpCommonPatched = false;
            bool srpEntityLightingPatched = false;

            bool urpLitShaderPatched = false;
            bool urpTerrainShaderPatched = false;
            bool urpLightingPatched = false;

            bool hdrpLitShaderPatched = false;
            bool hdrpTerrainShaderPatched = false;
            bool hdrpLightingLoopPatched = false;
            bool hdrpHDRISkyPatched = false;
            bool hdrpBuiltInGIPatched = false;

            GetInstalledSRPVersion();

            srpCommonPatched = CheckForMLSTags(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__COMMON__DEFAULT_SOURCE_FILE, "<MLS");
            srpEntityLightingPatched = CheckForMLSTags(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__ENTITY_LIGHTING__DEFAULT_SOURCE_FILE, "<MLS");

            switch (srp)
            {
                case RenderPipelineType.URP:
                    urpLitShaderPatched = CheckForMLSTags(systemProperties.srp_URP_SourcesPath + systemProperties.URP__LIT__DEFAULT_SOURCE_FILE, "<MLS");
                    urpTerrainShaderPatched = CheckForMLSTags(systemProperties.srp_URP_SourcesPath + systemProperties.URP__TERRAIN_LIT__DEFAULT_SOURCE_FILE, "<MLS");
                    urpLightingPatched = CheckForMLSTags(systemProperties.srp_URP_SourcesPath + "/ShaderLibrary/" + systemProperties.URP__LIGHTING__DEFAULT_SOURCE_FILE, "<MLS");
                    break;
                case RenderPipelineType.HDRP:
                    hdrpLitShaderPatched = CheckForMLSTags(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__LIT__DEFAULT_SOURCE_FILE, "<MLS");
                    hdrpTerrainShaderPatched = CheckForMLSTags(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__TERRAIN_LIT__DEFAULT_SOURCE_FILE, "<MLS");
                    hdrpLightingLoopPatched = CheckForMLSTags(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__LIGHTING_LOOP_DEF__DEFAULT_SOURCE_FILE, "<MLS");
                    hdrpBuiltInGIPatched = CheckForMLSTags(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__BUILTINGI__DEFAULT_SOURCE_FILE, "<MLS");
                    hdrpHDRISkyPatched = CheckForMLSTags(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__HDRI_SKY__DEFAULT_SOURCE_FILE, "<MLS");
                    break;
            }

            List<string> fileLines = new List<string>();
            systemProperties.srp__Common__Common_Includes__Lines.Clear();
            systemProperties.srp__Entity_Lighting__Sample_Single_Lightmap__Lines.Clear();
            systemProperties.srp__Entity_Lighting__Sample_Directional_Lightmap__Lines.Clear();
            systemProperties.srp__Lit__Common_Variables__Lines.Clear();
            systemProperties.urp__Lighting__Glossy_Environment_Reflection__Lines.Clear();
            systemProperties.hdrp_Lighting_Loop_Def__Sample_Env__Lines.Clear();
            systemProperties.hdrp_HDRI_Sky__Get_Sky_Color__Lines.Clear();
            systemProperties.hdrp_BuiltInGI__Lines.Clear();

            #region Read Modified Sources
            try
            {
                using (StreamReader reader = new StreamReader(systemProperties.srp_Common_ModifySourcesPath + systemProperties.SRP__COMMON__COMMON_INCLUDES__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        systemProperties.srp__Common__Common_Includes__Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(systemProperties.srp_Common_ModifySourcesPath + systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        systemProperties.srp__Entity_Lighting__Sample_Single_Lightmap__Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(systemProperties.srp_Common_ModifySourcesPath + systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        systemProperties.srp__Entity_Lighting__Sample_Directional_Lightmap__Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(systemProperties.srp_Common_ModifySourcesPath + systemProperties.SRP__LIT__COMMON_VARIABLES__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        systemProperties.srp__Lit__Common_Variables__Lines.Add(reader.ReadLine());
                    }
                }

                switch (srp)
                {
                    case RenderPipelineType.URP:
                        using (StreamReader reader = new StreamReader(systemProperties.srp_URP_ModifySourcesPath + systemProperties.URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__MODYFI_SOURCE_FILE))
                        {
                            while (!reader.EndOfStream)
                            {
                                systemProperties.urp__Lighting__Glossy_Environment_Reflection__Lines.Add(reader.ReadLine());
                            }
                        }
                        break;
                    case RenderPipelineType.HDRP:
                        using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_ModifySourcesPath + systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__MODYFI_SOURCE_FILE))
                        {
                            while (!reader.EndOfStream)
                            {
                                systemProperties.hdrp_Lighting_Loop_Def__Sample_Env__Lines.Add(reader.ReadLine());
                            }
                        }

                        using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_ModifySourcesPath + systemProperties.HDRP__BUILTINGI__MODYFI_SOURCE_FILE))
                        {
                            while (!reader.EndOfStream)
                            {
                                systemProperties.hdrp_BuiltInGI__Lines.Add(reader.ReadLine());
                            }
                        }

                        using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_ModifySourcesPath + systemProperties.HDRP__HDRI_SKY__GET_SKY_COLOR__MODYFI_SOURCE_FILE))
                        {
                            while (!reader.EndOfStream)
                            {
                                systemProperties.hdrp_HDRI_Sky__Get_Sky_Color__Lines.Add(reader.ReadLine());
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("The file could not be read: " + e.Message);
            }
            #endregion

            #region Patch Shader Files
            try
            {
                if (!srpCommonPatched)
                {
                    using (StreamReader reader = new StreamReader(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__COMMON__DEFAULT_SOURCE_FILE))
                    {
                        switch (srp)
                        {
                            case RenderPipelineType.URP:
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();

                                    if (!string.IsNullOrEmpty(line) && systemProperties.SRP__COMMON__COMMON_INCLUDES__SEARCH_STRING == line.Trim())
                                    {
                                        for (int i = 0; i < systemProperties.srp__Common__Common_Includes__Lines.Count; i++)
                                        {
                                            fileLines.Add(systemProperties.srp__Common__Common_Includes__Lines[i]);
                                        }

                                        fileLines.Add(line);
                                        fileLines.Add("");
                                        continue;
                                    }
                                    else
                                    {
                                        fileLines.Add(line);
                                    }
                                }
                                break;
                            case RenderPipelineType.HDRP:
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();

                                    if (!string.IsNullOrEmpty(line) && systemProperties.SRP__COMMON__COMMON_INCLUDES__SEARCH_STRING == line.Trim())
                                    {
                                        for (int i = 0; i < systemProperties.srp__Common__Common_Includes__Lines.Count; i++)
                                        {
                                            fileLines.Add(systemProperties.srp__Common__Common_Includes__Lines[i]);
                                        }

                                        fileLines.Add(line);
                                        fileLines.Add("");
                                        continue;
                                    }
                                    else
                                    {
                                        fileLines.Add(line);
                                    }
                                }
                                break;
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__COMMON__DEFAULT_SOURCE_FILE))
                    {
                        for (int i = 0; i < fileLines.Count; i++)
                        {
                            writer.WriteLine(fileLines[i]);
                        }
                    }

                    fileLines.Clear();
                }

                if (!srpEntityLightingPatched)
                {
                    using (StreamReader reader = new StreamReader(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__ENTITY_LIGHTING__DEFAULT_SOURCE_FILE))
                    {
                        bool skipLine = false;
                        bool fragmentFound = false;
                        bool replaceStartLine = false;
                        bool replaceEndLine = false;
                        List<string> tempLines = new List<string>();

                        while (!reader.EndOfStream)
                        {
                            if (skipLine)
                            {
                                skipLine = false;
                                continue;
                            }

                            string line = reader.ReadLine();

                            if (fragmentFound)
                            {
                                if (!replaceStartLine)
                                {
                                    if (!string.IsNullOrEmpty(line) && line.Trim() == "if (encodedLightmap)")
                                    {
                                        replaceStartLine = true;

                                        fileLines.Add("");

                                        for (int i = 0; i < systemProperties.srp__Entity_Lighting__Sample_Single_Lightmap__Lines.Count; i++)
                                        {
                                            fileLines.Add(systemProperties.srp__Entity_Lighting__Sample_Single_Lightmap__Lines[i]);
                                        }

                                        fileLines.Add("");
                                    }
                                    else
                                    {
                                        fileLines.Add(line);
                                        continue;
                                    }
                                }

                                if (!replaceEndLine)
                                {
                                    if (!string.IsNullOrEmpty(line) && line.Trim() == "return illuminance;")
                                    {
                                        fileLines.Add(line);
                                        fragmentFound = false;
                                        replaceEndLine = true;
                                        continue;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(line) && systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_SINGLE_LIGHTMAP__SEARCH_STRING == line.Trim())
                            {
                                fileLines.Add(line);
                                fragmentFound = true;
                                continue;
                            }

                            fileLines.Add(line);
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__ENTITY_LIGHTING__DEFAULT_SOURCE_FILE))
                    {
                        for (int i = 0; i < fileLines.Count; i++)
                        {
                            writer.WriteLine(fileLines[i]);
                        }
                    }

                    fileLines.Clear();

                    using (StreamReader reader = new StreamReader(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__ENTITY_LIGHTING__DEFAULT_SOURCE_FILE))
                    {
                        bool skipLine = false;
                        bool fragmentFound = false;
                        bool replaceStartLine = false;
                        bool replaceEndLine = false;
                        List<string> tempLines = new List<string>();

                        while (!reader.EndOfStream)
                        {
                            if (skipLine)
                            {
                                skipLine = false;
                                continue;
                            }

                            string line = reader.ReadLine();

                            if (fragmentFound)
                            {
                                if (!replaceStartLine)
                                {
                                    if (systemProperties.srpVersion.StartsWith("7") || systemProperties.srpVersion.StartsWith("8"))
                                    {
                                        if (!string.IsNullOrEmpty(line) && line.Trim() == "real4 direction = SAMPLE_TEXTURE2D(lightmapDirTex, lightmapDirSampler, uv);")
                                        {
                                            replaceStartLine = true;

                                            for (int i = 0; i < systemProperties.srp__Entity_Lighting__Sample_Directional_Lightmap__Lines.Count; i++)
                                            {
                                                fileLines.Add(systemProperties.srp__Entity_Lighting__Sample_Directional_Lightmap__Lines[i]);
                                            }

                                            fileLines.Add("");
                                        }
                                        else
                                        {
                                            fileLines.Add(line);
                                            continue;
                                        }
                                    }
                                    else if (systemProperties.srpVersion.StartsWith("10") || systemProperties.srpVersion.StartsWith("11") || systemProperties.srpVersion.StartsWith("12"))
                                    {
                                        if (!string.IsNullOrEmpty(line) && line.Trim() == "real4 direction = SAMPLE_TEXTURE2D_LIGHTMAP(lightmapDirTex, lightmapDirSampler, LIGHTMAP_EXTRA_ARGS_USE);")
                                        {
                                            replaceStartLine = true;

                                            for (int i = 0; i < systemProperties.srp__Entity_Lighting__Sample_Directional_Lightmap__Lines.Count; i++)
                                            {
                                                fileLines.Add(systemProperties.srp__Entity_Lighting__Sample_Directional_Lightmap__Lines[i]);
                                            }

                                            fileLines.Add("");
                                        }
                                        else
                                        {
                                            fileLines.Add(line);
                                            continue;
                                        }
                                    }
                                }

                                if (!replaceEndLine)
                                {
                                    if (!string.IsNullOrEmpty(line) && line.Trim() == "real halfLambert = dot(normalWS, direction.xyz - 0.5) + 0.5;")
                                    {
                                        fileLines.Add(line);
                                        fragmentFound = false;
                                        replaceEndLine = true;
                                        continue;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(line) && systemProperties.SRP__ENTITY_LIGHTING__SAMPLE_DIRECTIONAL_LIGHTMAP__SEARCH_STRING == line.Trim())
                            {
                                fileLines.Add(line);
                                fragmentFound = true;
                                continue;
                            }

                            fileLines.Add(line);
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(systemProperties.srp_Core_SourcesPath + systemProperties.SRP__ENTITY_LIGHTING__DEFAULT_SOURCE_FILE))
                    {
                        for (int i = 0; i < fileLines.Count; i++)
                        {
                            writer.WriteLine(fileLines[i]);
                        }
                    }

                    fileLines.Clear();
                }

                switch (srp)
                {
                    case RenderPipelineType.URP:
                        if (!urpLitShaderPatched)
                        {
                            using (StreamReader reader = new StreamReader(systemProperties.srp_URP_SourcesPath + systemProperties.URP__LIT__DEFAULT_SOURCE_FILE))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();

                                    if (!string.IsNullOrEmpty(line) && systemProperties.URP__LIT__COMMON_VARIABLES__SEARCH_STRING == line.Trim())
                                    {
                                        fileLines.Add(line);
                                        fileLines.Add("");

                                        for (int i = 0; i < systemProperties.srp__Lit__Common_Variables__Lines.Count; i++)
                                        {
                                            fileLines.Add(systemProperties.srp__Lit__Common_Variables__Lines[i]);
                                        }
                                        continue;
                                    }

                                    fileLines.Add(line);
                                }
                            }

                            using (StreamWriter writer = new StreamWriter(systemProperties.srp_URP_SourcesPath + systemProperties.URP__LIT__DEFAULT_SOURCE_FILE))
                            {
                                for (int i = 0; i < fileLines.Count; i++)
                                {
                                    writer.WriteLine(fileLines[i]);
                                }
                            }

                            fileLines.Clear();
                        }

                        if (!urpTerrainShaderPatched)
                        {
                            using (StreamReader reader = new StreamReader(systemProperties.srp_URP_SourcesPath + systemProperties.URP__TERRAIN_LIT__DEFAULT_SOURCE_FILE))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();

                                    if (!string.IsNullOrEmpty(line) && systemProperties.URP__TERRAIN_LIT__COMMON_VARIABLES__SEARCH_STRING == line.Trim())
                                    {
                                        fileLines.Add(line);
                                        fileLines.Add("");

                                        for (int i = 0; i < systemProperties.srp__Lit__Common_Variables__Lines.Count; i++)
                                        {
                                            fileLines.Add(systemProperties.srp__Lit__Common_Variables__Lines[i]);
                                        }
                                        continue;
                                    }

                                    fileLines.Add(line);
                                }
                            }

                            using (StreamWriter writer = new StreamWriter(systemProperties.srp_URP_SourcesPath + systemProperties.URP__TERRAIN_LIT__DEFAULT_SOURCE_FILE))
                            {
                                for (int i = 0; i < fileLines.Count; i++)
                                {
                                    writer.WriteLine(fileLines[i]);
                                }
                            }

                            fileLines.Clear();
                        }

                        if (!urpLightingPatched)
                        {
                            using (StreamReader reader = new StreamReader(systemProperties.srp_URP_SourcesPath + "/ShaderLibrary/" + systemProperties.URP__LIGHTING__DEFAULT_SOURCE_FILE))
                            {
                                bool skipLine = false;
                                bool fragmentFound = false;
                                bool replaceStartLine = false;
                                bool replaceEndLine = false;
                                List<string> tempLines = new List<string>();

                                while (!reader.EndOfStream)
                                {
                                    if (skipLine)
                                    {
                                        skipLine = false;
                                        continue;
                                    }

                                    string line = reader.ReadLine();

                                    if (fragmentFound)
                                    {
                                        #if MT_URP_12_INCLUDED
                                        if (!replaceStartLine)
                                        {
                                            replaceStartLine = true;

                                            for (int i = 0; i < systemProperties.urp__Lighting__Glossy_Environment_Reflection__Lines.Count; i++)
                                            {
                                                fileLines.Add(systemProperties.urp__Lighting__Glossy_Environment_Reflection__Lines[i]);
                                            }

                                            fileLines.Add("");
                                        }

                                        if (!replaceEndLine)
                                        {
                                            #if MT_URP_12_INCLUDED
                                            if (!string.IsNullOrEmpty(line) && line.Trim() == "#endif // _ENVIRONMENTREFLECTIONS_OFF")
                                            {
                                            #else
                                            if (!string.IsNullOrEmpty(line) && line.Trim() == "return irradiance * occlusion;")
                                            {                                                
                                            #endif
                                                fragmentFound = false;
                                                replaceEndLine = true;
                                                continue;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
#else
                                        if (!replaceStartLine)
                                        {                                            
                                            if (!string.IsNullOrEmpty(line) && line.Trim() == "{")
                                            {
                                                replaceStartLine = true;

                                                fileLines.Add(line);

                                                for (int i = 0; i < systemProperties.urp__Lighting__Glossy_Environment_Reflection__Lines.Count; i++)
                                                {
                                                    fileLines.Add(systemProperties.urp__Lighting__Glossy_Environment_Reflection__Lines[i]);
                                                }

                                                fileLines.Add("");
                                            }
                                            else
                                            {
                                                fileLines.Add(line);
                                                continue;
                                            }
                                        }

                                        if (!replaceEndLine)
                                        {
                                            if (!string.IsNullOrEmpty(line) && line.Trim() == "return irradiance * occlusion;")
                                            {
                                                fileLines.Add(line);
                                                fragmentFound = false;
                                                replaceEndLine = true;
                                                continue;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
#endif
                                        }

                                    if (!string.IsNullOrEmpty(line) && systemProperties.URP__LIGHTING__GLOSSY_ENVIRONMENT_REFLECTION__SEARCH_STRING == line.Trim())
                                    {
                                        #if !MT_URP_12_INCLUDED
                                        fileLines.Add(line);
                                        #endif
                                        fragmentFound = true;
                                        continue;
                                    }

                                    fileLines.Add(line);
                                }
                            }

                            using (StreamWriter writer = new StreamWriter(systemProperties.srp_URP_SourcesPath + "/ShaderLibrary/" + systemProperties.URP__LIGHTING__DEFAULT_SOURCE_FILE))
                            {
                                for (int i = 0; i < fileLines.Count; i++)
                                {
                                    writer.WriteLine(fileLines[i]);
                                }
                            }
                        }

                        //AssetDatabase.Refresh();
                        break;
                    case RenderPipelineType.HDRP:
                        if (!hdrpLitShaderPatched)
                        {
                            using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__LIT__DEFAULT_SOURCE_FILE))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();

                                    if (!string.IsNullOrEmpty(line) && systemProperties.HDRP__LIT__COMMON_VARIABLES__SEARCH_STRING == line.Trim())
                                    {
                                        fileLines.Add(line);
                                        fileLines.Add("");

                                        for (int i = 0; i < systemProperties.srp__Lit__Common_Variables__Lines.Count; i++)
                                        {
                                            fileLines.Add(systemProperties.srp__Lit__Common_Variables__Lines[i]);
                                        }
                                        continue;
                                    }

                                    fileLines.Add(line);
                                }
                            }

                            using (StreamWriter writer = new StreamWriter(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__LIT__DEFAULT_SOURCE_FILE))
                            {
                                for (int i = 0; i < fileLines.Count; i++)
                                {
                                    writer.WriteLine(fileLines[i]);
                                }
                            }

                            fileLines.Clear();
                        }

                        if (!hdrpTerrainShaderPatched)
                        {
                            using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__TERRAIN_LIT__DEFAULT_SOURCE_FILE))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();

                                    if (!string.IsNullOrEmpty(line) && systemProperties.HDRP__TERRAIN_LIT__COMMON_VARIABLES__SEARCH_STRING == line.Trim())
                                    {
                                        fileLines.Add(line);
                                        fileLines.Add("");

                                        for (int i = 0; i < systemProperties.srp__Lit__Common_Variables__Lines.Count; i++)
                                        {
                                            fileLines.Add(systemProperties.srp__Lit__Common_Variables__Lines[i]);
                                        }
                                        continue;
                                    }

                                    fileLines.Add(line);
                                }
                            }

                            using (StreamWriter writer = new StreamWriter(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__TERRAIN_LIT__DEFAULT_SOURCE_FILE))
                            {
                                for (int i = 0; i < fileLines.Count; i++)
                                {
                                    writer.WriteLine(fileLines[i]);
                                }
                            }

                            fileLines.Clear();
                        }

                        if (!hdrpLightingLoopPatched)
                        {
                            using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__LIGHTING_LOOP_DEF__DEFAULT_SOURCE_FILE))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();

                                    if (!string.IsNullOrEmpty(line) && systemProperties.HDRP__LIGHTING_LOOP_DEF__SAMPLE_ENV__SEARCH_STRING == line.Trim())
                                    {
                                        fileLines.Add("");

                                        for (int i = 0; i < systemProperties.hdrp_Lighting_Loop_Def__Sample_Env__Lines.Count; i++)
                                        {
                                            fileLines.Add(systemProperties.hdrp_Lighting_Loop_Def__Sample_Env__Lines[i]);
                                        }

                                        fileLines.Add("");
                                        continue;
                                    }

                                    fileLines.Add(line);
                                }
                            }

                            using (StreamWriter writer = new StreamWriter(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__LIGHTING_LOOP_DEF__DEFAULT_SOURCE_FILE))
                            {
                                for (int i = 0; i < fileLines.Count; i++)
                                {
                                    writer.WriteLine(fileLines[i]);
                                }
                            }

                            fileLines.Clear();
                        }

                        if (!hdrpHDRISkyPatched)
                        {
                            using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__HDRI_SKY__DEFAULT_SOURCE_FILE))
                            {
                                bool skipLine = false;
                                bool fragmentFound = false;
                                bool replaceStartLine = false;
                                bool replaceEndLine = false;
                                List<string> tempLines = new List<string>();

                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();

                                    if (skipLine)
                                    {
                                        skipLine = false;
                                        continue;
                                    }

                                    if (fragmentFound)
                                    {
                                        if (!replaceStartLine)
                                        {
                                            replaceStartLine = true;

                                            for (int i = 0; i < systemProperties.hdrp_HDRI_Sky__Get_Sky_Color__Lines.Count; i++)
                                            {
                                                fileLines.Add(systemProperties.hdrp_HDRI_Sky__Get_Sky_Color__Lines[i]);
                                            }

                                            fileLines.Add("");
                                        }

                                        if (!replaceEndLine)
                                        {
                                            if (!string.IsNullOrEmpty(line) && line.Trim() == "return SAMPLE_TEXTURECUBE_LOD(_Cubemap, sampler_Cubemap, dir, 0).rgb;")
                                            {
                                                fragmentFound = false;
                                                replaceEndLine = true;
                                                skipLine = true;
                                                continue;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(line) && systemProperties.HDRP__HDRISKY__GET_SKY_COLOR__SEARCH_STRING == line.Trim())
                                    {
                                        fragmentFound = true;
                                        continue;
                                    }

                                    fileLines.Add(line);
                                }
                            }

                            using (StreamWriter writer = new StreamWriter(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__HDRI_SKY__DEFAULT_SOURCE_FILE))
                            {
                                for (int i = 0; i < fileLines.Count; i++)
                                {
                                    writer.WriteLine(fileLines[i]);
                                }
                            }

                            fileLines.Clear();
                        }

                        if (!hdrpBuiltInGIPatched)
                        {
                            using (StreamReader reader = new StreamReader(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__BUILTINGI__DEFAULT_SOURCE_FILE))
                            {
                                bool skipLine = false;
                                bool fragmentFound = false;
                                bool replaceStartLine = false;
                                bool replaceEndLine = false;
                                List<string> tempLines = new List<string>();

                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();

                                    if (skipLine)
                                    {
                                        skipLine = false;
                                        continue;
                                    }

                                    if (fragmentFound)
                                    {
                                        if (!replaceStartLine)
                                        {
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED
                                            if (!string.IsNullOrEmpty(line) && line.Trim() == "float4 rawOcclusionMask = SAMPLE_TEXTURE2D(unity_ShadowMask, samplerunity_ShadowMask, uv); // Can't reuse sampler from Lightmap because with shader graph, the compile could optimize out the lightmaps if metal is 1")
#elif MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                            if (!string.IsNullOrEmpty(line) && line.Trim() == "float4 rawOcclusionMask = SAMPLE_TEXTURE2D_LIGHTMAP(SHADOWMASK_NAME, SHADOWMASK_SAMPLER_NAME, SHADOWMASK_SAMPLE_EXTRA_ARGS); // Can't reuse sampler from Lightmap because with shader graph, the compile could optimize out the lightmaps if metal is 1")
#endif
                                            {
                                                replaceStartLine = true;

                                                for (int i = 0; i < systemProperties.hdrp_BuiltInGI__Lines.Count; i++)
                                                {
                                                    fileLines.Add(systemProperties.hdrp_BuiltInGI__Lines[i]);
                                                }

                                                fileLines.Add("");
                                            }
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                                            else
                                            {
                                                fileLines.Add(line);
                                                continue;
                                            }
#endif
                                        }

                                        if (!replaceEndLine)
                                        {
                                            if (!string.IsNullOrEmpty(line) && line.Trim() == "#else")
                                            {
                                                fileLines.Add(line);
                                                fragmentFound = false;
                                                replaceEndLine = true;
                                                continue;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(line) && systemProperties.HDRP__BUILTINGI__SEARCH_STRING == line.Trim())
                                    {
                                        fileLines.Add(line);
                                        fragmentFound = true;
                                        continue;
                                    }

                                    fileLines.Add(line);
                                }
                            }

                            using (StreamWriter writer = new StreamWriter(systemProperties.srp_HDRP_SourcesPath + systemProperties.HDRP__BUILTINGI__DEFAULT_SOURCE_FILE))
                            {
                                for (int i = 0; i < fileLines.Count; i++)
                                {
                                    writer.WriteLine(fileLines[i]);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("The file could not be read: " + e.Message);
            }
#endregion

            CheckForSRPPatched(srp, true);
        }

        private void RestoreSRPShaders(RenderPipelineType srp, bool restoring)
        {
            string sourceDir = "";
            string destDir = "";

            switch (srp)
            {
                case RenderPipelineType.URP:
                    sourceDir = Application.dataPath + "/../Packages/com.unity.render-pipelines.core@" + systemProperties.srpVersion;
                    destDir = Application.dataPath + "/../Library/PackageCache/com.unity.render-pipelines.core@" + systemProperties.srpVersion;

                    Directory.Move(sourceDir, destDir);

                    sourceDir = Application.dataPath + "/../Packages/com.unity.render-pipelines.universal@" + systemProperties.srpVersion;
                    destDir = Application.dataPath + "/../Library/PackageCache/com.unity.render-pipelines.universal@" + systemProperties.srpVersion;

                    Directory.Move(sourceDir, destDir);

                    if (EditorUtility.DisplayDialog(
                        "Magic Lightmap Switcher",
                        "You must restart the editor for the changes to take effect.", "OK"))
                    {
                        systemProperties.editorRestarted = true;
                        EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                    }
                    else
                    {
                        systemProperties.editorRestarted = false;
                        Debug.LogFormat("<color=cyan>MLS:</color> You won't be able to use lightmap blending and switching until the editor is restarted.");
                    }                    
                    break;
                case RenderPipelineType.HDRP:
                    sourceDir = Application.dataPath + "/../Packages/com.unity.render-pipelines.core@" + systemProperties.srpVersion;
                    destDir = Application.dataPath + "/../Library/PackageCache/com.unity.render-pipelines.core@" + systemProperties.srpVersion;

                    Directory.Move(sourceDir, destDir);

                    sourceDir = Application.dataPath + "/../Packages/com.unity.render-pipelines.high-definition@" + systemProperties.srpVersion;
                    destDir = Application.dataPath + "/../Library/PackageCache/com.unity.render-pipelines.high-definition@" + systemProperties.srpVersion;

                    Directory.Move(sourceDir, destDir);

                    if (EditorUtility.DisplayDialog(
                        "Magic Lightmap Switcher",
                        "You must restart the editor for the changes to take effect.", "OK"))
                    {
                        systemProperties.editorRestarted = true;
                        EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                    }
                    else
                    {
                        systemProperties.editorRestarted = false;
                        Debug.LogFormat("<color=cyan>MLS:</color> You won't be able to use lightmap blending and switching until the editor is restarted.");
                    }
                    break;
            }            
        }        

        private static bool DoesTypeExist(string className)
        {
            var foundType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                             from type in GetTypesSafe(assembly)
                             where type.Name == className
                             select type).FirstOrDefault();

            return foundType != null;
        }

        private static IEnumerable<Type> GetTypesSafe(System.Reflection.Assembly assembly)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            return types.Where(x => x != null);
        }

        static private void RemoveDefine(string define, bool processLockFile = true)
        {
            if (processLockFile)
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);

                if (File.Exists(directories[0] + "/MLS_lock"))
                {
                    File.Delete(directories[0] + "/MLS_lock");
                }
            }

            string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            string[] scriptingDefines = scriptingDefine.Split(';');
            List<string> listDefines = scriptingDefines.ToList();
            listDefines.Remove(define);

            string newDefines = string.Join(";", listDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
        }

        static private void AddDefine(string define, bool processLockFile = true)
        {
            if (processLockFile)
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);

                if (!File.Exists(directories[0] + "/MLS_lock"))
                {
                    File.Create(directories[0] + "/MLS_lock");
                }
            }

            string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            string[] scriptingDefines = scriptingDefine.Split(';');
            List<string> listDefines = scriptingDefines.ToList();
            listDefines.Add(define);

            string newDefines = string.Join(";", listDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
        }
    }
}
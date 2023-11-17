#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class StoreLightmapTextures
    {
        public IEnumerator Execute(StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            MLSProgressBarHelper.StartNewStage("Storing Lightmap Textures...");
            
            string fullStorePath = "";

            lightmapData.sceneLightingData.lightmapsLight = new Texture2D[LightmapSettings.lightmaps.Length];
            lightmapData.sceneLightingData.lightmapsDirectional = new Texture2D[LightmapSettings.lightmaps.Length];
            lightmapData.sceneLightingData.lightmapsShadowmask = new Texture2D[LightmapSettings.lightmaps.Length];
            lightmapData.sceneLightingData.fogSettings = new StoredLightmapData.FogSettings();

#if BAKERY_INCLUDED
            if (mainComponent.lightmapper == MagicLightmapSwitcher.Lightmapper.BakeryLightmapper)
            {
                ftLightmapsStorage ftLightmaps = ftRenderLightmap.FindRenderSettingsStorage();

                lightmapData.sceneLightingData.lightmapsBakeryRNM0 = new Texture2D[ftLightmaps.rnmMaps0.Count];
                lightmapData.sceneLightingData.lightmapsBakeryRNM1 = new Texture2D[ftLightmaps.rnmMaps0.Count];
                lightmapData.sceneLightingData.lightmapsBakeryRNM2 = new Texture2D[ftLightmaps.rnmMaps0.Count];

                for (int i = 0; i < lightmapData.sceneLightingData.lightmapsBakeryRNM0.Length; i++)
                {
                    lightmapData.sceneLightingData.lightmapsBakeryRNM0[i] = SaveTexture(i,
                        MLSManager.LightmapType.BakeryRNM0, lightmapData.sceneLightingData.lightmapName, lightmapData,
                        mainComponent);
                }

                for (int i = 0; i < lightmapData.sceneLightingData.lightmapsBakeryRNM1.Length; i++)
                {
                    lightmapData.sceneLightingData.lightmapsBakeryRNM1[i] = SaveTexture(i,
                        MLSManager.LightmapType.BakeryRNM1, lightmapData.sceneLightingData.lightmapName, lightmapData,
                        mainComponent);
                }

                for (int i = 0; i < lightmapData.sceneLightingData.lightmapsBakeryRNM2.Length; i++)
                {
                    lightmapData.sceneLightingData.lightmapsBakeryRNM2[i] = SaveTexture(i,
                        MLSManager.LightmapType.BakeryRNM2, lightmapData.sceneLightingData.lightmapName, lightmapData,
                        mainComponent);
                }

                var bakeryVolumes = Object.FindObjectsOfType<BakeryVolume>();

                lightmapData.sceneLightingData.bakeryVolumes = new StoredLightmapData.BakeryVolumeData();
                lightmapData.sceneLightingData.bakeryVolumes.name = new string[bakeryVolumes.Length];
                lightmapData.sceneLightingData.bakeryVolumes.volumeTexture0 = new Texture3D[bakeryVolumes.Length];
                lightmapData.sceneLightingData.bakeryVolumes.volumeTexture1 = new Texture3D[bakeryVolumes.Length];
                lightmapData.sceneLightingData.bakeryVolumes.volumeTexture2 = new Texture3D[bakeryVolumes.Length];
                lightmapData.sceneLightingData.bakeryVolumes.volumeTexture3 = new Texture3D[bakeryVolumes.Length];
                lightmapData.sceneLightingData.bakeryVolumes.volumeTexture4 = new Texture3D[bakeryVolumes.Length];

                for (int i = 0; i < bakeryVolumes.Length; i++)
                {
                    if (!bakeryVolumes[i].gameObject.name.Contains("MLS"))
                    {
                        bakeryVolumes[i].gameObject.name = "MLS_" + bakeryVolumes[i].gameObject.name + "_" + i;
                    }

                    lightmapData.sceneLightingData.bakeryVolumes.name[i] = bakeryVolumes[i].name;
                    lightmapData.sceneLightingData.bakeryVolumes.volumeTexture0[i] = SaveTexture(
                        MLSManager.LightmapType.BakeryVolume0, lightmapData, mainComponent, bakeryVolumes[i], i);
                    lightmapData.sceneLightingData.bakeryVolumes.volumeTexture1[i] = SaveTexture(
                        MLSManager.LightmapType.BakeryVolume1, lightmapData, mainComponent, bakeryVolumes[i], i);
                    lightmapData.sceneLightingData.bakeryVolumes.volumeTexture2[i] = SaveTexture(
                        MLSManager.LightmapType.BakeryVolume2, lightmapData, mainComponent, bakeryVolumes[i], i);

                    if (bakeryVolumes[i].bakedMask != null)
                    {
                        lightmapData.sceneLightingData.bakeryVolumes.volumeTexture3[i] = SaveTexture(
                            MLSManager.LightmapType.BakeryVolumeMask, lightmapData, mainComponent, bakeryVolumes[i], i);
                    }

                    if (bakeryVolumes[i].bakedTexture3 != null)
                    {
                        lightmapData.sceneLightingData.bakeryVolumes.volumeTexture4[i] = SaveTexture(
                            MLSManager.LightmapType.BakeryVolumeCompressed, lightmapData, mainComponent,
                            bakeryVolumes[i], i);
                    }
                }
            }
#endif

            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                lightmapData.sceneLightingData.lightmapsLight[i] = SaveTexture(i, MLSManager.LightmapType.Color, lightmapData.sceneLightingData.lightmapName, lightmapData, mainComponent);
                lightmapData.sceneLightingData.lightmapsDirectional[i] = SaveTexture(i, MLSManager.LightmapType.Directional, lightmapData.sceneLightingData.lightmapName, lightmapData, mainComponent);
                lightmapData.sceneLightingData.lightmapsShadowmask[i] = SaveTexture(i, MLSManager.LightmapType.Shadowmask, lightmapData.sceneLightingData.lightmapName, lightmapData, mainComponent);

                if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Tex"))
                {
                    StoredLightmapData.SkyboxSettings skyboxSettings = new StoredLightmapData.SkyboxSettings();

                    skyboxSettings.skyboxTexture = RenderSettings.skybox.GetTexture("_Tex") as Cubemap;
                    skyboxSettings.exposure = RenderSettings.skybox.GetFloat("_Exposure");
                    skyboxSettings.tintColor = RenderSettings.skybox.GetColor("_Tint");

                    lightmapData.sceneLightingData.skyboxSettings = skyboxSettings;
                }

                lightmapData.sceneLightingData.fogSettings.enabled = RenderSettings.fog;
                lightmapData.sceneLightingData.fogSettings.fogColor = RenderSettings.fogColor;
                lightmapData.sceneLightingData.fogSettings.fogDensity = RenderSettings.fogDensity;

                lightmapData.sceneLightingData.environmentSettings.source = RenderSettings.ambientMode;
                lightmapData.sceneLightingData.environmentSettings.intensityMultiplier = RenderSettings.ambientIntensity;
                lightmapData.sceneLightingData.environmentSettings.ambientColor = RenderSettings.ambientLight;
                lightmapData.sceneLightingData.environmentSettings.skyColor = RenderSettings.ambientSkyColor;
                lightmapData.sceneLightingData.environmentSettings.equatorColor = RenderSettings.ambientEquatorColor;
                lightmapData.sceneLightingData.environmentSettings.groundColor = RenderSettings.ambientGroundColor;

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateProgress(LightmapSettings.lightmaps.Length, 0))
                    {
                        yield return null;
                    }
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.SetDirty(lightmapData);
            AssetDatabase.Refresh();

            MLSLightmapDataStoring.stageExecuting = false;
        }

        private Texture2D SaveTexture(int lightmapIndex, MLSManager.LightmapType lightmapType, string lightmapName, StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            string fullStorePath = "";

#if BAKERY_INCLUDED
            ftLightmapsStorage ftLightmaps = ftRenderLightmap.FindRenderSettingsStorage();
#endif

            switch (lightmapType) 
            {
                case MLSManager.LightmapType.Color:
                    fullStorePath = mainComponent.currentDataPath + "/LightmapLight_" + lightmapName + "_" + lightmapIndex + ".exr";

                    EditorUtility.SetDirty(lightmapData);

                    if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].lightmapColor), fullStorePath))
                    {
                        mainComponent.storedAssetsCount++;
                    }

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].lightmapColor));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.Directional:
                    if (LightmapSettings.lightmaps[lightmapIndex].lightmapDir != null)
                    {
                        fullStorePath = mainComponent.currentDataPath + "/LightmapDirectional_" + lightmapName + "_" + lightmapIndex + ".png";

                        EditorUtility.SetDirty(lightmapData);

                        if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].lightmapDir), fullStorePath))
                        {
                            mainComponent.storedAssetsCount++;
                        }

                        if (MLSManager.clearDefaultDataFolder)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].lightmapDir));
                        }

                        EditorUtility.SetDirty(lightmapData);
                    }
                    break;
                case MLSManager.LightmapType.Shadowmask:
                    if (LightmapSettings.lightmaps[lightmapIndex].shadowMask != null)
                    {
                        fullStorePath = mainComponent.currentDataPath + "/LightmapShadowmask_" + lightmapName + "_" + lightmapIndex + ".png";

                        EditorUtility.SetDirty(lightmapData);

                        if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].shadowMask), fullStorePath))
                        {
                            mainComponent.storedAssetsCount++;
                        }

                        if (MLSManager.clearDefaultDataFolder)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].shadowMask));
                        }

                        EditorUtility.SetDirty(lightmapData);
                    }
                    break;
#if BAKERY_INCLUDED
                case MLSManager.LightmapType.BakeryRNM0:
                    fullStorePath = mainComponent.currentDataPath + "/LightmapBakeryRNM0_" + lightmapName + "_" + lightmapIndex + ".hdr";

                    EditorUtility.SetDirty(lightmapData);

                    if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps0[lightmapIndex]), fullStorePath))
                    {
                        mainComponent.storedAssetsCount++;
                    }

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps0[lightmapIndex]));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.BakeryRNM1:
                    fullStorePath = mainComponent.currentDataPath + "/LightmapBakeryRNM1_" + lightmapName + "_" + lightmapIndex + ".hdr";

                    EditorUtility.SetDirty(lightmapData);

                    if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps1[lightmapIndex]), fullStorePath))
                    {
                        mainComponent.storedAssetsCount++;
                    }

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps1[lightmapIndex]));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.BakeryRNM2:
                    fullStorePath = mainComponent.currentDataPath + "/LightmapBakeryRNM2_" + lightmapName + "_" + lightmapIndex + ".hdr";

                    EditorUtility.SetDirty(lightmapData);

                    if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps2[lightmapIndex]), fullStorePath))
                    {
                        mainComponent.storedAssetsCount++;
                    }

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps2[lightmapIndex]));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
#endif
            }

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(lightmapData);

            return AssetDatabase.LoadAssetAtPath(fullStorePath, typeof(Texture2D)) as Texture2D;
        }
        
#if BAKERY_INCLUDED
        private Texture3D SaveTexture(MLSManager.LightmapType lightmapType, StoredLightmapData lightmapData,
            MagicLightmapSwitcher mainComponent, BakeryVolume bakeryVolume, int volumeIndex)
        {
            string fullStorePath = "";

            switch (lightmapType) 
            {
                case MLSManager.LightmapType.BakeryVolume0:
                    fullStorePath = mainComponent.currentDataPath + "/BakeryVolume0_" + lightmapData.sceneLightingData.lightmapName + "_" + volumeIndex + ".asset";

                    EditorUtility.SetDirty(lightmapData);
                    
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedTexture0), fullStorePath);

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedTexture0));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.BakeryVolume1:
                    fullStorePath = mainComponent.currentDataPath + "/BakeryVolume1_" + lightmapData.sceneLightingData.lightmapName + "_" + volumeIndex + ".asset";

                    EditorUtility.SetDirty(lightmapData);
                    
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedTexture1), fullStorePath);

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedTexture1));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.BakeryVolume2:
                    fullStorePath = mainComponent.currentDataPath + "/BakeryVolume2_" + lightmapData.sceneLightingData.lightmapName + "_" + volumeIndex + ".asset";

                    EditorUtility.SetDirty(lightmapData);
                    
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedTexture2), fullStorePath);

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedTexture2));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.BakeryVolumeMask:
                    fullStorePath = mainComponent.currentDataPath + "/BakeryVolumeMask_" + lightmapData.sceneLightingData.lightmapName + "_" + volumeIndex + ".asset";

                    EditorUtility.SetDirty(lightmapData);
                    
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedMask), fullStorePath);

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedMask));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.BakeryVolumeCompressed:
                    fullStorePath = mainComponent.currentDataPath + "/BakeryVolumeCompressed_" + lightmapData.sceneLightingData.lightmapName + "_" + volumeIndex + ".asset";

                    EditorUtility.SetDirty(lightmapData);
                    
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedTexture3), fullStorePath);
                    
                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bakeryVolume.bakedTexture3));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
            }

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(lightmapData);

            return AssetDatabase.LoadAssetAtPath(fullStorePath, typeof(Texture3D)) as Texture3D;
        }
#endif
    }
}
#endif
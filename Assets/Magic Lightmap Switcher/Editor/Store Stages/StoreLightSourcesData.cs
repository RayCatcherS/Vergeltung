#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MagicLightmapSwitcher
{
    public class StoreLightSourcesData
    {
        public IEnumerator Execute(StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            if (mainComponent == null)
            {
                mainComponent = RuntimeAPI.GetSwitcherInstanceStatic(EditorSceneManager.GetActiveScene().name);
            }

            MLSProgressBarHelper.StartNewStage("Storing Lights Data...");

            yield return null;

            Object[] lightSources = Object.FindObjectsOfType(typeof(Light));

            List< StoredLightmapData.LightSourceData> tempLightsList = new List<StoredLightmapData.LightSourceData>();

            int counter = 0;

            foreach (Light light in lightSources)
            {
                MLSLight mlsLight = light.GetComponent<MLSLight>();

                if (mlsLight != null && !mlsLight.exludeFromStoring)
                {
                    //if (mainComponent.lightingPresets[0].lightSourceSettings.Find(item => item.mlsLightUID == light.GetComponent<MLSLight>().lightGUID) != null)
                    {
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                        HDAdditionalLightData additionalLightData = light.gameObject.GetComponent<HDAdditionalLightData>();

                    if (additionalLightData.lightmapBakeType == LightmapBakeType.Baked || additionalLightData.lightmapBakeType == LightmapBakeType.Mixed)
                    {
#else
                        if (light.lightmapBakeType == LightmapBakeType.Baked || light.lightmapBakeType == LightmapBakeType.Mixed)
                        {
#endif
                            tempLightsList.Add(new StoredLightmapData.LightSourceData());

                            if (tempLightsList.Find(item => item.lightUID == light.GetComponent<MLSLight>().lightGUID) != null)
                            {
                                light.GetComponent<MLSLight>().UpdateGUID();
                            }

                            tempLightsList[counter].name = light.name;
                            tempLightsList[counter].instanceID = light.GetInstanceID().ToString();
                            tempLightsList[counter].lightUID = light.GetComponent<MLSLight>().lightGUID;
                            tempLightsList[counter].position = light.transform.position;
                            tempLightsList[counter].rotation = light.transform.rotation;
                            tempLightsList[counter].intensity = light.intensity;
                            tempLightsList[counter].color = light.color;
                            tempLightsList[counter].temperature = light.colorTemperature;
                            tempLightsList[counter].range = light.range;
                            tempLightsList[counter].spotAngle = light.spotAngle;
                            tempLightsList[counter].areaSize = light.areaSize;
                            tempLightsList[counter].shadowType = (int)light.shadows;
                            tempLightsList[counter].shadowStrength = light.shadowStrength;

                            counter++;
                        }
                    }
                }

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateProgress(lightSources.Length, 0))
                    {
                        yield return null;
                    }
                }
            }

            lightmapData.sceneLightingData.lightSourceDatas = tempLightsList.ToArray();
            MLSLightmapDataStoring.stageExecuting = false;
        }
    }
}
#endif
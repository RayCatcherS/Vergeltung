#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace MagicLightmapSwitcher
{
    public class StoreLightProbesData
    {
        public IEnumerator Execute(StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            MLSProgressBarHelper.StartNewStage("Storing Lightprobes Data...");

            yield return null;

            if (LightmapSettings.lightProbes == null)
            {
                Debug.Log("No light probes on the scene.");
                MLSLightmapDataStoring.stageExecuting = false;
                yield break;
            }

            SphericalHarmonicsL2[] sceneLightProbes = LightmapSettings.lightProbes.bakedProbes;            
            lightmapData.sceneLightingData.lightProbes = new StoredLightmapData.SphericalHarmonics[sceneLightProbes.Length];
            lightmapData.sceneLightingData.initialLightProbesArrayPosition = LightmapSettings.lightProbes.bakedProbes.Length;

            for (int i = 0; i < sceneLightProbes.Length; i++)
            {
                var SHCoeff = new StoredLightmapData.SphericalHarmonics();

                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        SHCoeff.coefficients[j * 9 + k] = sceneLightProbes[i][j, k];
                    }
                }

                lightmapData.sceneLightingData.lightProbes[i] = SHCoeff;

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateProgress(sceneLightProbes.Length, 100))
                    {
                        yield return null;
                    }
                }
            }

            lightmapData.sceneLightingData.lightProbes1D = new float[sceneLightProbes.Length * 27];

            int counter = 0;

            for (int i = 0; i < sceneLightProbes.Length; i++)
            {
                for (int j = 0; j < lightmapData.sceneLightingData.lightProbes[i].coefficients.Length; j++)
                {
                    lightmapData.sceneLightingData.lightProbes1D[counter] = 0;
                    lightmapData.sceneLightingData.lightProbes1D[counter] = lightmapData.sceneLightingData.lightProbes[i].coefficients[j];
                    counter++;
                }
            }

            MLSLightmapDataStoring.stageExecuting = false;
        }
    }
}
#endif

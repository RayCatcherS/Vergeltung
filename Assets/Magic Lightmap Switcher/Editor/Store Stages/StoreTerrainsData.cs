#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class StoreTerrainsData
    {
        public IEnumerator Execute(StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            MLSProgressBarHelper.StartNewStage("Storing Terrains Data...");

            yield return null;

            Object[] terrains = Object.FindObjectsOfType(typeof(Terrain));

            List<StoredLightmapData.TerrainData> terrainDatasTemp = new List<StoredLightmapData.TerrainData>();

            int counter = 0;

            foreach (Terrain terrain in terrains)
            {
                if (!CheckIfContributeGI(terrain.gameObject))
                {
                    continue;
                }

                if (terrain.lightmapIndex != -1 || !terrain.enabled)
                {
                    MLSStaticRenderer staticRenderer = terrain.gameObject.GetComponent<MLSStaticRenderer>();

                    if (staticRenderer == null)
                    {
                        staticRenderer = terrain.gameObject.AddComponent<MLSStaticRenderer>();
                        staticRenderer.UpdateGUID();
                    }

                    StoredLightmapData.TerrainData terrainData = new StoredLightmapData.TerrainData();

                    terrainData.objectId = staticRenderer.scriptId;
                    terrainData.lightmapIndex = terrain.lightmapIndex;
                    terrainData.lightmapOffsetScale = terrain.lightmapScaleOffset;

                    terrainDatasTemp.Add(terrainData);
                }

                counter++;

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateProgress(terrains.Length, 0))
                    {
                        yield return null;
                    }
                }
            }

            lightmapData.sceneLightingData.terrainDatas = terrainDatasTemp.ToArray();

            MLSLightmapDataStoring.stageExecuting = false;
        }

        public bool CheckIfContributeGI(GameObject gameObject)
        {
            bool isStatic = false;
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(gameObject);

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
    }
}
#endif
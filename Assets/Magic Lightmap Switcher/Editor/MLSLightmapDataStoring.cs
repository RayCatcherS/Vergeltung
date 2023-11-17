using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using static MagicLightmapSwitcher.MLSManager;
using System;

namespace MagicLightmapSwitcher
{
    public static class MLSLightmapDataStoring
    {
        public static IEnumerator bakingLightingDataQueueRoutine;
        public static IEnumerator bakingLightingDataRoutine;
        public static IEnumerator storingLightingDataRoutine;
        public static IEnumerator storingLightingDataSubRoutine;
        public static IEnumerator storingStageRoutine;
        public static List<IEnumerator> storingStages = new List<IEnumerator>();

        public static StoredLightmapData lightmapDataAsset;
        public static bool stageExecuting;
        public static bool storingLightingData;
        public static bool storeStagesExecuting;
        public static string currentBakingPreset;
        public static bool bakingLightingData = false;
        public static bool bakingLightingDataQueue = false;
        public static bool stopBakingQueue = false;
        public static bool waitingForAllAssetsImported = false;
        public static bool stopWaitingForAssetsSaved = false;
        public static bool waitingForAssetImporting = false;
        public static int presetsCount;
        public static int currentPreset;
        public static MagicLightmapSwitcher.Lightmapper currentLightmapper;
        public static MLSPresetManager presetManager;

        public static string lastTargetScene;
        public static int lastStaticAffectedRenderes;

        #region Enumerators
        public static void BakeAndStoreQueueIteratorUpdate()
        {
            if (bakingLightingDataQueueRoutine != null && bakingLightingDataQueueRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= BakeAndStoreQueueIteratorUpdate;
        }

        public static void BakeAndStoreIteratorUpdate()
        {
            if (bakingLightingDataRoutine != null && bakingLightingDataRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= BakeAndStoreIteratorUpdate;
        }

        public static void StoreLightmapIteratorUpdate()
        {
            if (storingLightingDataRoutine != null && storingLightingDataRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= StoreLightmapIteratorUpdate;
        }

        public static void StoringStageIteratorUpdate()
        {
            if (storingLightingDataSubRoutine != null && storingLightingDataSubRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= StoringStageIteratorUpdate;
        }

        public static void ExecuteStagesIteratorUpdate()
        {
            if (storingStageRoutine != null && storingStageRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= ExecuteStagesIteratorUpdate;
        }

        private static IEnumerator ExecuteAllStages()
        {
            for (int i = 0; i < storingStages.Count; i++)
            {
                storingLightingDataSubRoutine = null;
                stageExecuting = true;
                storingLightingDataSubRoutine = storingStages[i];
                EditorApplication.update += StoringStageIteratorUpdate;

                while (stageExecuting)
                {
                    yield return null;
                }

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateTotalProgress(storingStages.Count, 0))
                    {
                        yield return null;
                    }
                }
            }

            storeStagesExecuting = false;
        }

        private static IEnumerator BakeAndStoreLightmapQueue(MagicLightmapSwitcher activeSwitcherInstance, bool createLightmapScenario)
        {
            activeSwitcherInstance.StopAllCoroutines();

            if (presetManager != null)
            {
                presetManager.Close();
            }

            bakingLightingDataQueue = true;
            currentPreset = 0;
            presetsCount = 0;

            List<StoredLightmapData> tempStoredLightmapDatas = new List<StoredLightmapData>();

            if (activeSwitcherInstance.lightingPresets == null)
            {
                bakingLightingDataQueue = false;
                yield break;
            }

            currentLightmapper = activeSwitcherInstance.lightmapper;

            for (int i = 0; i < activeSwitcherInstance.lightingPresets.Count; i++)
            {
                if (activeSwitcherInstance.lightingPresets[i].included)
                {
                    presetsCount++;
                }
            }

            for (int i = 0; i < activeSwitcherInstance.lightingPresets.Count; i++)
            {
                currentPreset++;

                if (activeSwitcherInstance == null)
                {
                    activeSwitcherInstance = RuntimeAPI.GetSwitcherInstanceStatic(SceneManager.GetActiveScene().name);
                }
                
                if (!activeSwitcherInstance.lightingPresets[i].included)
                {
                    continue;
                }

                currentBakingPreset = activeSwitcherInstance.lightingPresets[i].name;

#if BAKERY_INCLUDED
                if (stopBakingQueue || ftRenderLightmap.userCanceled)
                {
                    stopBakingQueue = true;
                    ftRenderLightmap.userCanceled = false;
#else
                if (stopBakingQueue)
                {                    
#endif
                    bakingLightingDataQueue = false;
                    waitingForAllAssetsImported = false;
                    bakingLightingData = false;
                    waitingForAssetImporting = false;
                    break;
                }

                Lightmapping.Clear();
                activeSwitcherInstance.lightingPresets[i].MatchSceneSettings();
                EditorSceneManager.SaveOpenScenes();

#if BAKERY_INCLUDED
                switch (currentLightmapper)
                {
                    case MagicLightmapSwitcher.Lightmapper.UnityLightmapper:
                        Lightmapping.BakeAsync();

                        while (Lightmapping.isRunning)
                        {
                            yield return null;
                        }
                        break;
                    case MagicLightmapSwitcher.Lightmapper.BakeryLightmapper:
                        if (ftRenderLightmap.instance == null)
                        {
                            EditorDisplayMessages.ShowMessage("The Bakery's \"Render Lightmap...\" window should be open.");
                            
                            stopBakingQueue = true;
                            break;
                        }

                        if (activeSwitcherInstance.useTexture2DArrays || activeSwitcherInstance.useTextureCubeArrays)
                        {
                            var storage = ftRenderLightmap.FindRenderSettingsStorage();

                            if (storage.renderSettingsMinAutoResolution != storage.renderSettingsMaxAutoResolution)
                            {
                                EditorDisplayMessages.ShowMessage("" +
                                                                  "You are using texture arrays, this requires the same " +
                                                                  "atlas size for all lightmaps. You are currently using " +
                                                                  "Bakery, which uses the dynamic resolution of the lightmap " +
                                                                  "texture atlas. Set the same Min and Max resolution.");
                                
                                stopBakingQueue = true;
                                break;
                            }
                        }

                        ftRenderLightmap.instance.RenderButton(false);

                        while (ftRenderLightmap.bakeInProgress)
                        {
                            yield return null;
                        }

                        ftRenderLightmap.instance.RenderReflectionProbesButton(false);

                        while (ftRenderLightmap.bakeInProgress)
                        {
                            yield return null;
                        }

                        if (LightmapSettings.lightProbes != null && LightmapSettings.lightProbes.count > 0)
                        {
                            if (ftRenderLightmap.lightProbeMode == ftRenderLightmap.LightProbeMode.Legacy)
                            {
                                ftRenderLightmap.instance.RenderLightProbesButton(false);

                                while (ftRenderLightmap.bakeInProgress)
                                {
                                    yield return null;
                                }
                            }
                        }
                        break;
                }
#else
                Lightmapping.BakeAsync();

                while (Lightmapping.isRunning)
                {
                    yield return null;
                }
#endif

#if BAKERY_INCLUDED
                if (stopBakingQueue || ftRenderLightmap.userCanceled)
                {
                    stopBakingQueue = true;
                    ftRenderLightmap.userCanceled = false;
#else
                if (stopBakingQueue)
                {                    
#endif
                    bakingLightingDataQueue = false;
                    waitingForAllAssetsImported = false;
                    bakingLightingData = false;
                    waitingForAssetImporting = false;
                    break;
                }

                waitingForAllAssetsImported = true;

                while (waitingForAllAssetsImported)
                {
                    if (!waitingForAssetImporting)
                    {
                        yield return new WaitForSeconds(5);

                        if (waitingForAssetImporting)
                        {
                            yield return null;
                        }
                        else
                        {
                            waitingForAllAssetsImported = false;
                        }
                    }

                    yield return null;
                }

#if BAKERY_INCLUDED
                if (stopBakingQueue || ftRenderLightmap.userCanceled)
                {
                    stopBakingQueue = true;
                    ftRenderLightmap.userCanceled = false;
#else
                if (stopBakingQueue)
                {                    
#endif
                    bakingLightingDataQueue = false;
                    waitingForAllAssetsImported = false;
                    bakingLightingData = false;
                    waitingForAssetImporting = false;
                    break;
                }

                bakingLightingData = false;
                waitingForAssetImporting = false;
                storingLightingData = true;

                StartStoringProcess(activeSwitcherInstance, activeSwitcherInstance.lightingPresets[i].name);

                while (storingLightingData)
                {
                    yield return null;
                }

#if BAKERY_INCLUDED
                if (stopBakingQueue || ftRenderLightmap.userCanceled)
                {
                    stopBakingQueue = true;
                    ftRenderLightmap.userCanceled = false;
#else
                if (stopBakingQueue)
                {                    
#endif
                    bakingLightingDataQueue = false;
                    waitingForAllAssetsImported = false;
                    bakingLightingData = false;
                    waitingForAssetImporting = false;
                    break;
                }

                tempStoredLightmapDatas.Add(lightmapDataAsset);
            }

            if (activeSwitcherInstance == null)
            {
                activeSwitcherInstance = RuntimeAPI.GetSwitcherInstanceStatic(SceneManager.GetActiveScene().name);
            }

            if (!stopBakingQueue)
            {
                if (createLightmapScenario)
                {
                    if (activeSwitcherInstance.availableScenarios.Count > 0 && MLSManager.scenarioToChange > 0)
                    {
                        int removeIndex = MLSManager.scenarioToChange - 1;
                        MLSManager.RemoveStoredLightingScenario(removeIndex);
                        CreateLightmapScenario(activeSwitcherInstance, "New Lightmap Scenario_" + (MLSManager.scenarioToChange - 1).ToString(), tempStoredLightmapDatas);
                    }
                    else
                    {
                        CreateLightmapScenario(activeSwitcherInstance, "New Lightmap Scenario_" + activeSwitcherInstance.availableScenarios.Count, tempStoredLightmapDatas);
                    }
                }
                else
                {
                    for (int i = 0; i < activeSwitcherInstance.availableScenarios.Count; i++)
                    {
                        for (int j = 0; j < activeSwitcherInstance.availableScenarios[i].blendableLightmaps.Count; j++) 
                        {
                            for (int k = 0; k < tempStoredLightmapDatas.Count; k++)    
                            {
                                if (activeSwitcherInstance.availableScenarios[i].blendableLightmaps[j].lightingData.dataName == tempStoredLightmapDatas[k].dataName)
                                {
                                    activeSwitcherInstance.availableScenarios[i].blendableLightmaps[j].lightingData = tempStoredLightmapDatas[k];
                                }
                            }                            
                        }
                    }
                }

                activeSwitcherInstance.StartCoroutine(activeSwitcherInstance.UpdateStoredArray(SceneManager.GetSceneAt(MLSManager.selectedScene).name, true));
            }

            if (tempStoredLightmapDatas.Count > 0)
            {
                activeSwitcherInstance.StartCoroutine(activeSwitcherInstance.UpdateStoredArray(SceneManager.GetSceneAt(MLSManager.selectedScene).name, true));
            }

            bakingLightingDataQueue = false;
        }

        private static IEnumerator BakeAndStoreLightmap(MagicLightmapSwitcher activeSwitcherInstance, string lightmapName)
        {
            if (presetManager != null)
            {
                presetManager.Close();
            }

            bakingLightingData = true;

            currentLightmapper = activeSwitcherInstance.lightmapper;

            //List<GameObject> tempDisabledObjects = new List<GameObject>();
            //List<Light> tempDisabledLights = new List<Light>();

            //if (bakedGroup.Count > 0)
            //{
            //    GameObject[] meshRenderers = FindObjectsOfType<GameObject>();
            //    Light[] lights = FindObjectsOfType<Light>();

            //    for (int i = 0; i < meshRenderers.Length; i++)  
            //    {
            //        if (meshRenderers[i].name != "!ftraceLightmaps" && 
            //            meshRenderers[i].transform.childCount == 0 && 
            //            meshRenderers[i].GetComponent<Light>() == null && 
            //            !bakedGroup[0].affectedObjects.Contains(meshRenderers[i]) && 
            //            bakedGroup[0].rootObject != meshRenderers[i].gameObject &&
            //            ((meshRenderers[i].transform.parent != null && meshRenderers[i].transform.parent.gameObject != bakedGroup[0].rootObject) || meshRenderers[i].transform.parent == null))
            //        {
            //            meshRenderers[i].SetActive(false);
            //            tempDisabledObjects.Add(meshRenderers[i]);
            //        }
            //    }

            //    for (int i = 0; i < lights.Length; i++)
            //    {
            //        if (!bakedGroup[0].affectedLights.Contains(lights[i]))
            //        {
            //            lights[i].enabled = false;
            //            tempDisabledLights.Add(lights[i]);
            //        }
            //    }
            //}

            Lightmapping.Clear();

#if BAKERY_INCLUDED
            switch (currentLightmapper)
            {
                case MagicLightmapSwitcher.Lightmapper.UnityLightmapper:
                    Lightmapping.BakeAsync();

                    while (Lightmapping.isRunning)
                    {
                        yield return null;
                    }
                    break;
                case MagicLightmapSwitcher.Lightmapper.BakeryLightmapper:
                    if (ftRenderLightmap.instance == null)
                    {
                        EditorDisplayMessages.ShowMessage("The Bakery's \"Render Lightmap...\" window should be open.");

                        stopBakingQueue = true;
                        break;
                    }
                    
                    var storage = ftRenderLightmap.FindRenderSettingsStorage();

                    if (storage.renderSettingsMinAutoResolution != storage.renderSettingsMaxAutoResolution)
                    {
                        Debug.Log("sdsd");
                        yield break;
                    }
                    // storage.renderSettingsMinAutoResolution = 2048;
                    // storage.renderSettingsMaxAutoResolution = 2048;
                    // ftRenderLightmap.instance.LoadRenderSettings();

                    ftRenderLightmap.instance.RenderButton(false);

                    while (ftRenderLightmap.bakeInProgress)
                    {
                        yield return null;
                    }

                    ftRenderLightmap.instance.RenderReflectionProbesButton(false);

                    while (ftRenderLightmap.bakeInProgress)
                    {
                        yield return null;
                    }

                    if (ftRenderLightmap.lightProbeMode == ftRenderLightmap.LightProbeMode.Legacy)
                    {
                        ftRenderLightmap.instance.RenderLightProbesButton(false);

                        while (ftRenderLightmap.bakeInProgress)
                        {
                            yield return null;
                        }
                    }
                    break;
            }
#else
            Lightmapping.BakeAsync();

            while (Lightmapping.isRunning)
            {
                yield return null;
            }
#endif

            //if (bakedGroup.Count > 0)
            //{
            //    for (int i = 0; i < tempDisabledObjects.Count; i++)
            //    {
            //        tempDisabledObjects[i].SetActive(true);
            //    }

            //    for (int i = 0; i < tempDisabledLights.Count; i++)
            //    {
            //        tempDisabledLights[i].enabled = true;
            //    }
            //}

            waitingForAllAssetsImported = true;

            while (waitingForAllAssetsImported)
            {
                if (!waitingForAssetImporting)
                {
                    yield return new WaitForSeconds(5);

                    if (waitingForAssetImporting)
                    {
                        yield return null;
                    }
                    else
                    {
                        waitingForAllAssetsImported = false;
                    }
                }

                yield return null;
            }

            bakingLightingData = false;
            waitingForAssetImporting = false;
            storingLightingData = true;

            if (activeSwitcherInstance == null)
            {
                activeSwitcherInstance = RuntimeAPI.GetSwitcherInstanceStatic(SceneManager.GetActiveScene().name);
            }

            StartStoringProcess(activeSwitcherInstance, lightmapName);

            while (storingLightingData)
            {
                yield return null;
            }

            activeSwitcherInstance.StartCoroutine(activeSwitcherInstance.UpdateStoredArray(SceneManager.GetSceneAt(MLSManager.selectedScene).name, true));
        }

        public static IEnumerator StoreLightingData(MagicLightmapSwitcher activeSwitcherInstance, string lightmapName, bool manualBaking = false)
        {
            storingLightingData = true;

            //activeSwitcherInstance.SetBlendingOptionsGlobal(MagicLightmapSwitcher.BlendingOptions.None);

            StoreLightmapTextures storeLightmapTexturesStage = new StoreLightmapTextures();
            StoreCustomBlendablesData storeCustomBlendablesData = new StoreCustomBlendablesData();
            StoreRenderersData storeRenderersDataStage = new StoreRenderersData();
            StoreTerrainsData storeTerrainsDataStage = new StoreTerrainsData();
            StoreLightSourcesData storeLightSourcesDataStage = new StoreLightSourcesData();
            StoreReflectionProbesData storeReflectionProbesStage = new StoreReflectionProbesData();
            StoreLightProbesData storeLightProbesDataStage = new StoreLightProbesData();

            MLSProgressBarHelper.StartNewStage("Creating Asset Data File...");

            yield return null;

            string sceneLightmapDataPath = activeSwitcherInstance.currentDataPath;

            if (!Directory.Exists(sceneLightmapDataPath))
            {
                Directory.CreateDirectory(sceneLightmapDataPath);
            }

            if (File.Exists(sceneLightmapDataPath + "/" + EditorSceneManager.GetActiveScene().name + "_" + lightmapName + ".asset"))
            {
                File.Delete(sceneLightmapDataPath + "/" + EditorSceneManager.GetActiveScene().name + "_" + lightmapName + ".asset");
            }

            lightmapDataAsset = Editor.CreateInstance<StoredLightmapData>();
            AssetDatabase.CreateAsset(lightmapDataAsset, sceneLightmapDataPath + "/" + EditorSceneManager.GetActiveScene().name + "_" + lightmapName + ".asset");            
            AssetDatabase.SaveAssets();

            yield return null;

            lightmapDataAsset.sceneGUID = EditorSceneManager.GetActiveScene().GetHashCode().ToString();
            lightmapDataAsset.workflow = activeSwitcherInstance.workflow;
            lightmapDataAsset.dataPrefix = EditorSceneManager.GetActiveScene().name;
            lightmapDataAsset.dataName = lightmapName;
            lightmapDataAsset.sceneLightingData = new StoredLightmapData.SceneLightingData();
            lightmapDataAsset.sceneLightingData.lightmapName = lightmapName;
            lightmapDataAsset.blendingIndex = Resources.FindObjectsOfTypeAll(typeof(StoredLightmapData)).Length - 1;
            lightmapDataAsset.blendingRange = new Vector2(0, 1);
            
            EditorUtility.SetDirty(lightmapDataAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            storingStages.Add(storeReflectionProbesStage.Execute(lightmapDataAsset, activeSwitcherInstance));
            storingStages.Add(storeLightmapTexturesStage.Execute(lightmapDataAsset, activeSwitcherInstance));
            storingStages.Add(storeCustomBlendablesData.Execute(lightmapDataAsset, activeSwitcherInstance));
            storingStages.Add(storeRenderersDataStage.Execute(lightmapDataAsset, activeSwitcherInstance));
            storingStages.Add(storeTerrainsDataStage.Execute(lightmapDataAsset, activeSwitcherInstance));            
            storingStages.Add(storeLightSourcesDataStage.Execute(lightmapDataAsset, activeSwitcherInstance));
            storingStages.Add(storeLightProbesDataStage.Execute(lightmapDataAsset, activeSwitcherInstance));

            storeStagesExecuting = true;
            storingStageRoutine = ExecuteAllStages();
            EditorApplication.update += ExecuteStagesIteratorUpdate;

            while (storeStagesExecuting)
            {
                yield return null;
            }

            EditorUtility.SetDirty(lightmapDataAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!bakingLightingDataQueue)
            {
                EditorDisplayMessages.ShowMessage("Lighting Data storing finished.");
            }
            else
            {                
                if (currentPreset == presetsCount)
                {
                    currentPreset = 0;
                    EditorDisplayMessages.ShowMessage("Lighting Data storing finished.");
                }
            }

            Debug.LogFormat("<color=cyan>MLS:</color> Lighting Data for scene \"" + EditorSceneManager.GetActiveScene().name + "\" stored successfully. " +
                    "Data File Name: " + lightmapName);

            if (manualBaking)
            {
                activeSwitcherInstance.StartCoroutine(activeSwitcherInstance.UpdateStoredArray(SceneManager.GetSceneAt(MLSManager.selectedScene).name, true));
            }

            storingLightingData = false;
        }

        public static void StartBakeStoringProcessQueue(MagicLightmapSwitcher activeSwitcherInstance, bool createLightmapScenario)
        {
            stopBakingQueue = false;
            bakingLightingDataQueueRoutine = BakeAndStoreLightmapQueue(activeSwitcherInstance, createLightmapScenario);
            EditorApplication.update += BakeAndStoreQueueIteratorUpdate;
        }

        public static void StartBakeStoringProcess(string lightmapName)
        {
            if (activeSwitcherInstance == null)
            {
                activeSwitcherInstance = RuntimeAPI.GetSwitcherInstanceStatic(SceneManager.GetActiveScene().name);
            }

            bakingLightingDataRoutine = BakeAndStoreLightmap(activeSwitcherInstance, lightmapName);
            EditorApplication.update += BakeAndStoreIteratorUpdate;
        }

        public static void StartStoringProcess(MagicLightmapSwitcher activeSwitcherInstance, string lightmapName, bool manualBaking = false)
        {
            MLSProgressBarHelper.ResetProgress();
            storingStages.Clear();

            if (LightmapSettings.lightmaps.Length > 0)
            {
                storingLightingDataRoutine = StoreLightingData(activeSwitcherInstance, lightmapName, manualBaking);
                EditorApplication.update += StoreLightmapIteratorUpdate;
            }
            else
            {
                stopBakingQueue = true;
                bakingLightingDataQueue = false;
                waitingForAllAssetsImported = false;
                bakingLightingData = false;
                waitingForAssetImporting = false;
                storingLightingData = false;

                EditorDisplayMessages.ShowMessage("No baked lightmaps found in the project.");
            }

            for (int i = 0; i < activeSwitcherInstance.availableScenarios.Count; i++)
            {
                activeSwitcherInstance.availableScenarios[i].selfTestCompleted = false;
            }
        }

        public static void StopStoring()
        {
            MLSProgressBarHelper.ResetProgress();
            storingStages.Clear();

            storingLightingDataRoutine = null;
            EditorApplication.update -= StoreLightmapIteratorUpdate;
            storingLightingData = false;
        }

        public static void StopBakingQueue()
        {
            stopBakingQueue = true;
            bakingLightingDataQueue = false;
            waitingForAllAssetsImported = false;
            bakingLightingData = false;
            waitingForAssetImporting = false;

            Lightmapping.Cancel();
        }
#endregion
 
        public static void CreateLightmapScenario(MagicLightmapSwitcher lightmapSwitcher, string name, List<StoredLightmapData> storedLightmapData = null)
        {
            string sceneLightmapDataPath = lightmapSwitcher.currentDataPath;

            if (!Directory.Exists(sceneLightmapDataPath))
            {
                Directory.CreateDirectory(sceneLightmapDataPath);
            }

            if (lightmapSwitcher.storedLightmapScenarios == null)
            {
                lightmapSwitcher.storedLightmapScenarios = new Dictionary<string, List<StoredLightingScenario>>();
            }

            string prefix = EditorSceneManager.GetActiveScene().name + "_";

            StoredLightingScenario storedLightingScenario = AssetDatabase.LoadAssetAtPath<StoredLightingScenario>(sceneLightmapDataPath + "/" + prefix + name + ".asset");

            if (storedLightingScenario == null)
            {
                storedLightingScenario = Editor.CreateInstance<StoredLightingScenario>();

                storedLightingScenario.workflow = lightmapSwitcher.workflow;
                storedLightingScenario.sourceObject = lightmapSwitcher.gameObject;
                storedLightingScenario.targetScene = EditorSceneManager.GetActiveScene().name;
                storedLightingScenario.prefix = prefix;
                storedLightingScenario.scenarioName = name;
                storedLightingScenario.eventsListId = lightmapSwitcher.OnBlendingValueChanged.Count;

                if (storedLightmapData != null)
                {
                    for (int i = 0; i < storedLightmapData.Count; i++)
                    {
                        storedLightingScenario.AddLightmapData(storedLightmapData[i]);
                        storedLightingScenario.startValues.Add(0);
                    }

                    for (int i = 0; i < storedLightingScenario.startValues.Count; i++)
                    {
                        storedLightingScenario.startValues[i] = (float) Mathf.Round((float) i * ((float) 1 / ((float) storedLightingScenario.startValues.Count - 1)) * 100f) / 100f;
                    }
                }

                lightmapSwitcher.OnBlendingValueChanged.Add(new MagicLightmapSwitcher.BlendingValueChanged());
                lightmapSwitcher.OnLoadedLightmapChanged.Add(new MagicLightmapSwitcher.LoadedLightmapChanged());

                AssetDatabase.CreateAsset(storedLightingScenario, sceneLightmapDataPath + "/" + prefix + name + ".asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                lightmapSwitcher.StartCoroutine(lightmapSwitcher.UpdateStoredArray(EditorSceneManager.GetActiveScene().name, true));
            }
        }
    }
}

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
using UnityEngine.Rendering.HighDefinition;
#endif

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
using System.Collections;
#endif

namespace MagicLightmapSwitcher
{
    [CreateAssetMenu(fileName = "StoredLightingScenario", menuName = "Magic Tools/Magic Lightmap Switcher/Create New Scenario", order = 1)]
    public class StoredLightingScenario : ScriptableObject
    {
#if UNITY_EDITOR
        public IEnumerator editorBlendingPreviewRoutine;
        private RuntimeAPI runtimeAPI = new RuntimeAPI();

        public void EditorBlendingPreviewIteratorUpdate()
        {
            if (editorBlendingPreviewRoutine != null && editorBlendingPreviewRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= EditorBlendingPreviewIteratorUpdate;
        }

        private IEnumerator EditorBlendingPreview(float cycleLength)
        {
            while (!stopPreviewPlayback)
            {
                runtimeAPI.BlendLightmapsCyclic(cycleLength, this, RuntimeAPI.BlendingDirection.FirstToLast);

                yield return null;
            }
        }

        public void StopEditorBlendingPreview()
        {
            globalBlendFactor = 0;
            stopPreviewPlayback = true;
            editorBlendingPreviewRoutine = null;
            EditorApplication.update -= EditorBlendingPreviewIteratorUpdate;
        }
#endif
        public enum RangesSettingMode
        {
            Separated,
            Mirror,
            Copy
        }

        [System.Serializable]
        public class LightmapData
        {
            public int blendingIndex;
            public int prevBlendingIndex;
            public float startValue;
            public Vector2 reflectionsBlendingRange;
            public Vector2 lightmapBlendingRange;
            public StoredLightmapData lightingData;

            public LightmapData()
            {
            }

            public LightmapData(int _blendingIndex, int _prevBlendingIndex, float _startValue, Vector2 _reflectionsBlendingRange, Vector2 _lightmapsBlendingRange, StoredLightmapData _lightingData)
            {
                blendingIndex = _blendingIndex;
                prevBlendingIndex = _prevBlendingIndex;
                startValue = _startValue;
                reflectionsBlendingRange = _reflectionsBlendingRange;
                lightmapBlendingRange = _lightmapsBlendingRange;
                lightingData = _lightingData;
            }
        }

        [System.Serializable]
        public class CollectedCustomBlendableData
        {
            public enum BlendingSynchMode
            {
                Self,
                WithGlobalBlend,
                WithReflections,
                WithLightmaps
            }

            [System.Serializable]
            public class BlendableFloatFieldData
            {
                [System.Serializable]
                public class DataVariant
                {
                    [SerializeField]
                    public string lightmapName;
                    [SerializeField]
                    public float fieldValue;
                }

                [SerializeField]
                public BlendingSynchMode blendingSynchMode;
                [SerializeField]
                public FieldInfo sourceField;
                [SerializeField]
                public string fieldName;
                [SerializeField]
                public List<DataVariant> dataVariants = new List<DataVariant>();
                [SerializeField]
                public bool foldoutEnabled;
            }

            [System.Serializable]
            public class BlendableCubemapFieldData
            {
                [System.Serializable]
                public class DataVariant
                {
                    [SerializeField]
                    public string lightmapName;
                    [SerializeField]
                    public Cubemap fieldValue;
                }

                [SerializeField]
                public BlendingSynchMode blendingSynchMode;
                [SerializeField]
                public FieldInfo sourceField;
                [SerializeField]
                public string fieldName;
                [SerializeField]
                public Cubemap fieldValue;
                [SerializeField]
                public List<DataVariant> dataVariants = new List<DataVariant>();
                [SerializeField]
                public bool foldoutEnabled;
            }

            [System.Serializable]
            public class BlendableColorFieldData
            {
                [System.Serializable]
                public class DataVariant
                {
                    [SerializeField]
                    public string lightmapName;
                    [SerializeField]
                    public Color fieldValue;
                }

                [SerializeField]
                public BlendingSynchMode blendingSynchMode;
                [SerializeField]
                public FieldInfo sourceField;
                [SerializeField]
                public string fieldName;
                [SerializeField]
                public Color fieldValue;
                [SerializeField]
                public List<DataVariant> dataVariants = new List<DataVariant>();
                [SerializeField]
                public bool foldoutEnabled;
            }

            [SerializeField]
            public MLSCustomBlendable sourceScript;
            [SerializeField]
            public string sourceScriptName;
            [SerializeField]
            public string sourceScriptId;
            [SerializeField]
            public List<BlendableFloatFieldData> blendableFloatFieldsDatas = new List<BlendableFloatFieldData>();
            [SerializeField]
            public List<BlendableCubemapFieldData> blendableCubemapFieldsDatas = new List<BlendableCubemapFieldData>();
            [SerializeField]
            public List<BlendableColorFieldData> blendableColorFieldsDatas = new List<BlendableColorFieldData>();
            [SerializeField]
            public bool foldoutEnabled;
        }

        [SerializeField]
        public MagicLightmapSwitcher.Workflow workflow;
        [SerializeField]
        public GameObject sourceObject;
        [SerializeField]
        public string targetScene;
        [SerializeField]
        public string prefix;
        [SerializeField]
        public string scenarioName;
        [SerializeField]
        public List<LightmapData> blendableLightmaps = new List<LightmapData>();
        [SerializeField]
        public List<int> blendingOrder = new List<int>();
        [SerializeField]
        public List<float> startValues = new List<float>();
        [SerializeField]
        public List<CollectedCustomBlendableData> collectedCustomBlendableDatas = new List<CollectedCustomBlendableData>();
        [SerializeField]
        public bool cyclic;
        [SerializeField]
        public RangesSettingMode rangesSettingMode;
        [SerializeField]
        public int lastBlendableLightmapsCount;
        [SerializeField]
        public float globalBlendFactor;
        [SerializeField]
        public float localBlendFactor = 0;
        [SerializeField]
        public float reflectionsRangedBlendFactor;
        [SerializeField]
        public float lightmapsRangedBlendFactor;
        [SerializeField]
        public bool stopPreviewPlayback = true;
        [SerializeField]
        public int lightingDataFromIndex = 0;
        [SerializeField]
        public int lightingDataToIndex = 0;
        [SerializeField]
        public int lightProbesArrayPosition;
        [SerializeField]
        public int eventsListId;
        [SerializeField] 
        public LayerMask blendingModules = ~ 0;

        private MagicLightmapSwitcher lightmapSwitcher;
        
        public MagicLightmapSwitcher.BlendingValueChanged OnBlendingValueChanged;
        public MagicLightmapSwitcher.LoadedLightmapChanged OnLoadedLightmapChanged;

        public List<LightmapData> orderedStoredLightmapDatas = new List<LightmapData>();
        public List<StoredLightmapData> tempLightmapDataList = new List<StoredLightmapData>();
        public float prevBlendValue;
        public bool orderChanged;
        public bool removed;
        public bool selfTestCompleted;
        public bool selfTestSuccess;

        public bool initialized;
        private int lastFrom = -1;

        int _MLS_Sky_Cubemap_Blend_Factor;
        int _MLS_Sky_Cubemap_Blend_From;
        int _MLS_Sky_Cubemap_Blend_To;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
        HDRISky hdriSky;
        int _MLS_ENABLE_DISTORTION;
        int _MLS_USE_FLOWMAP;
#endif      

#if UNITY_EDITOR
        [SerializeField]
        public SerializedObject scenarioSerializedObject;
        public ReorderableList reorderableStoredLightmapDatas;

        public void StartEditorBlendingPreview(float cycleLength)
        {
            globalBlendFactor = 0;
            stopPreviewPlayback = false;
            editorBlendingPreviewRoutine = EditorBlendingPreview(cycleLength);
            EditorApplication.update += EditorBlendingPreviewIteratorUpdate;
        }
#endif

        public bool SelfTest()
        {
            bool result = true;
            selfTestSuccess = true;

            for (int i = 0; i < blendableLightmaps.Count; i++)
            {
                for (int j = 0; j < blendableLightmaps[i].lightingData.sceneLightingData.lightmapsLight.Length; j++)
                {
                    if (blendableLightmaps[i].lightingData.sceneLightingData.lightmapsLight[j] == null)
                    {
                        result = false;
                        selfTestSuccess = false;
                    }
                }
            }

            selfTestCompleted = true;
            return result;
        }

        //public void CalculateBlendingValues()
        //{
        //    for (int i = 0; i < blendableLightmaps.Count; i++)
        //    {
        //        if (i < blendableLightmaps.Count - 2)
        //        {
        //            if (globalBlendFactor >= blendableLightmaps[i].startValue && globalBlendFactor <= blendableLightmaps[i + 1].startValue)
        //            {
        //                lightingDataFromIndex = blendableLightmaps[i].blendingIndex;
        //                lightingDataToIndex = blendableLightmaps[i + 1].blendingIndex;

        //                localBlendFactor =
        //                    Mathf.Clamp((globalBlendFactor - blendableLightmaps[lightingDataFromIndex].startValue) /
        //                    (blendableLightmaps[lightingDataToIndex].startValue - blendableLightmaps[lightingDataFromIndex].startValue), 0, 1);

        //                break;
        //            }
        //        }
        //        else
        //        {
        //            if (globalBlendFactor >= blendableLightmaps[i].startValue)
        //            {
        //                lightingDataFromIndex = blendableLightmaps[i].blendingIndex;
        //                lightingDataToIndex = blendableLightmaps.Count - 1;

        //                localBlendFactor =
        //                    Mathf.Clamp((globalBlendFactor - blendableLightmaps[lightingDataFromIndex].startValue) /
        //                    (1 - blendableLightmaps[lightingDataFromIndex].startValue), 0, 1);

        //                break;
        //            }
        //        }
        //    }

        //    float reflectionsRangedBlend =
        //            Mathf.Clamp((localBlendFactor - blendableLightmaps[lightingDataToIndex].reflectionsBlendingRange.x) /
        //            (blendableLightmaps[lightingDataToIndex].reflectionsBlendingRange.y - blendableLightmaps[lightingDataToIndex].reflectionsBlendingRange.x), 0, 1);

        //    float lightmapsRangedBlend =
        //            Mathf.Clamp((localBlendFactor - blendableLightmaps[lightingDataToIndex].lightmapBlendingRange.x) /
        //            (blendableLightmaps[lightingDataToIndex].lightmapBlendingRange.y - blendableLightmaps[lightingDataToIndex].lightmapBlendingRange.x), 0, 1);
        //}

        private void UpdateFields(MLSCustomBlendable customBlendableSource, CollectedCustomBlendableData customBlendableData)
        {
            customBlendableData.sourceScript = customBlendableSource.UpdateScriptLink();

            for (int i = 0; i < customBlendableData.blendableFloatFieldsDatas.Count; i++)
            {
                if (customBlendableData.blendableFloatFieldsDatas[i].dataVariants != null &&
                    customBlendableData.blendableFloatFieldsDatas[i].dataVariants.Count > 0)
                {
                    FieldInfo[] sorceScriptFields = customBlendableData.sourceScript.GetType().GetFields();

                    foreach (FieldInfo field in sorceScriptFields)
                    {
                        if (customBlendableData.blendableFloatFieldsDatas[i].fieldName == field.Name)
                        {
                            customBlendableData.blendableFloatFieldsDatas[i].sourceField = field;
                        }
                    }
                }
            }

            for (int i = 0; i < customBlendableData.blendableColorFieldsDatas.Count; i++)
            {
                if (customBlendableData.blendableColorFieldsDatas[i].dataVariants != null &&
                    customBlendableData.blendableColorFieldsDatas[i].dataVariants.Count > 0)
                {
                    FieldInfo[] sorceScriptFields = customBlendableData.sourceScript.GetType().GetFields();

                    foreach (FieldInfo field in sorceScriptFields)
                    {
                        if (customBlendableData.blendableColorFieldsDatas[i].fieldName == field.Name)
                        {
                            customBlendableData.blendableColorFieldsDatas[i].sourceField = field;
                        }
                    }
                }
            }

            for (int i = 0; i < customBlendableData.blendableCubemapFieldsDatas.Count; i++)
            {
                if (customBlendableData.blendableCubemapFieldsDatas[i].dataVariants != null &&
                    customBlendableData.blendableCubemapFieldsDatas[i].dataVariants.Count > 0)
                {
                    FieldInfo[] sorceScriptFields = customBlendableData.sourceScript.GetType().GetFields();

                    foreach (FieldInfo field in sorceScriptFields)
                    {
                        if (customBlendableData.blendableCubemapFieldsDatas[i].fieldName == field.Name)
                        {
                            customBlendableData.blendableCubemapFieldsDatas[i].sourceField = field;
                        }
                    }
                }
            }
        }

        private void SynchronizeFloatData(int storedDataIndex, int customBlendableDataIndex, CollectedCustomBlendableData targetDataSet, bool update)
        {
            for (int k = 0; k < blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableFloatFieldsDatas.Length; k++)
            {
                CollectedCustomBlendableData.BlendableFloatFieldData.DataVariant dataVariant = new CollectedCustomBlendableData.BlendableFloatFieldData.DataVariant();
                dataVariant.fieldValue = blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableFloatFieldsDatas[k].fieldValue;
                dataVariant.lightmapName = blendableLightmaps[storedDataIndex].lightingData.dataName;

                CollectedCustomBlendableData.BlendableFloatFieldData currentFloatDataSet =
                    targetDataSet.blendableFloatFieldsDatas.Find(
                        item => item.fieldName == blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableFloatFieldsDatas[k].fieldName);

                if (currentFloatDataSet == null)
                {
                    CollectedCustomBlendableData.BlendableFloatFieldData blendableFloatFieldData = new CollectedCustomBlendableData.BlendableFloatFieldData();

                    blendableFloatFieldData.fieldName = blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableFloatFieldsDatas[k].fieldName;
                    blendableFloatFieldData.dataVariants.Add(dataVariant);
                    targetDataSet.blendableFloatFieldsDatas.Add(blendableFloatFieldData);
                }
                else
                {
                    if (update)
                    {
                        currentFloatDataSet.dataVariants.Add(dataVariant);
                    }
                    else
                    {
                        currentFloatDataSet.dataVariants.Add(dataVariant);
                        targetDataSet.blendableFloatFieldsDatas.Add(currentFloatDataSet);
                    }
                }
            }
        }

        private void SynchronizeColorData(int storedDataIndex, int customBlendableDataIndex, CollectedCustomBlendableData targetDataSet, bool update)
        {
            for (int k = 0; k < blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableColorFieldsDatas.Length; k++)
            {
                CollectedCustomBlendableData.BlendableColorFieldData.DataVariant dataVariant = new CollectedCustomBlendableData.BlendableColorFieldData.DataVariant();
                dataVariant.fieldValue = blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableColorFieldsDatas[k].fieldValue;
                dataVariant.lightmapName = blendableLightmaps[storedDataIndex].lightingData.dataName;

                CollectedCustomBlendableData.BlendableColorFieldData currentColorDataSet =
                    targetDataSet.blendableColorFieldsDatas.Find(
                        item => item.fieldName == blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableColorFieldsDatas[k].fieldName);

                if (currentColorDataSet == null)
                {
                    CollectedCustomBlendableData.BlendableColorFieldData blendableColorFieldData = new CollectedCustomBlendableData.BlendableColorFieldData();

                    blendableColorFieldData.fieldName = blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableColorFieldsDatas[k].fieldName;
                    blendableColorFieldData.dataVariants.Add(dataVariant);
                    targetDataSet.blendableColorFieldsDatas.Add(blendableColorFieldData);
                }
                else
                {
                    if (update)
                    {
                        currentColorDataSet.dataVariants.Add(dataVariant);
                    }
                    else
                    {
                        currentColorDataSet.dataVariants.Add(dataVariant);
                        targetDataSet.blendableColorFieldsDatas.Add(currentColorDataSet);
                    }
                }
            }
        }

        private void SynchronizeCubemapData(int storedDataIndex, int customBlendableDataIndex, CollectedCustomBlendableData targetDataSet, bool update)
        {
            for (int k = 0; k < blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableCubemapFieldsDatas.Length; k++)
            {
                CollectedCustomBlendableData.BlendableCubemapFieldData.DataVariant dataVariant = new CollectedCustomBlendableData.BlendableCubemapFieldData.DataVariant();
                dataVariant.fieldValue = blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableCubemapFieldsDatas[k].fieldValue;
                dataVariant.lightmapName = blendableLightmaps[storedDataIndex].lightingData.dataName;

                CollectedCustomBlendableData.BlendableCubemapFieldData currentCubemapDataSet =
                    targetDataSet.blendableCubemapFieldsDatas.Find(
                        item => item.fieldName == blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableCubemapFieldsDatas[k].fieldName);

                if (currentCubemapDataSet == null)
                {
                    CollectedCustomBlendableData.BlendableCubemapFieldData blendableCubemapFieldData = new CollectedCustomBlendableData.BlendableCubemapFieldData();

                    blendableCubemapFieldData.fieldName = blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[customBlendableDataIndex].blendableCubemapFieldsDatas[k].fieldName;
                    blendableCubemapFieldData.dataVariants.Add(dataVariant);
                    targetDataSet.blendableCubemapFieldsDatas.Add(blendableCubemapFieldData);
                }
                else
                {
                    if (update)
                    {
                        currentCubemapDataSet.dataVariants.Add(dataVariant);
                    }
                    else
                    {
                        currentCubemapDataSet.dataVariants.Add(dataVariant);
                        targetDataSet.blendableCubemapFieldsDatas.Add(currentCubemapDataSet);
                    }
                }
            }
        }

        private void BuildCollectedCustomBlendableData(int storedDataIndex, MLSCustomBlendable customBlendable)
        {
            CollectedCustomBlendableData currentData = collectedCustomBlendableDatas.Find(item => item.sourceScriptId == customBlendable.sourceScriptId);

            if (currentData == null)
            {
                CollectedCustomBlendableData collectedCustomBlendableData = new CollectedCustomBlendableData();

                collectedCustomBlendableData.sourceScript = customBlendable.UpdateScriptLink();
                collectedCustomBlendableData.sourceScriptName = customBlendable.name;
                collectedCustomBlendableData.sourceScriptId = customBlendable.sourceScriptId;

                collectedCustomBlendableData.blendableFloatFieldsDatas = new List<CollectedCustomBlendableData.BlendableFloatFieldData>();
                collectedCustomBlendableData.blendableColorFieldsDatas = new List<CollectedCustomBlendableData.BlendableColorFieldData>();
                collectedCustomBlendableData.blendableCubemapFieldsDatas = new List<CollectedCustomBlendableData.BlendableCubemapFieldData>();

                for (int j = 0; j < blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas.Length; j++)
                {
                    if (customBlendable.sourceScriptId == blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[j].sourceScriptId)
                    {
                        #region Synchronize Float Data
                        SynchronizeFloatData(storedDataIndex, j, collectedCustomBlendableData, false);
                        #endregion

                        #region Synchronize Color Data
                        SynchronizeColorData(storedDataIndex, j, collectedCustomBlendableData, false);
                        #endregion

                        #region Synchronize Cubemap Data
                        SynchronizeCubemapData(storedDataIndex, j, collectedCustomBlendableData, false);
                        #endregion
                    }
                }

                collectedCustomBlendableDatas.Add(collectedCustomBlendableData);
            }
            else
            {
                for (int j = 0; j < blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas.Length; j++)
                {
                    if (customBlendable.sourceScriptId == blendableLightmaps[storedDataIndex].lightingData.sceneLightingData.customBlendableDatas[j].sourceScriptId)
                    {
                        #region Synchronize Float Data
                        SynchronizeFloatData(storedDataIndex, j, currentData, true);
                        #endregion

                        #region Synchronize Color Data
                        SynchronizeColorData(storedDataIndex, j, currentData, true);
                        #endregion

                        #region Synchronize Cubemap Data
                        SynchronizeCubemapData(storedDataIndex, j, currentData, true);
                        #endregion
                    }
                }
            }
        }

        public void SynchronizeCustomBlendableData(bool rebuild = false)
        {
            if (rebuild)
            {
                collectedCustomBlendableDatas.Clear();
            }

            if (blendableLightmaps.Count > 1)
            {
                Object[] customBlendables = FindObjectsOfType<MLSCustomBlendable>();

                foreach (MLSCustomBlendable customBlendable in customBlendables)
                {
                    for (int i = 0; i < blendableLightmaps.Count; i++)
                    {
                        if (blendableLightmaps[i].lightingData == null)
                        {
                            continue;
                        }

                        if (rebuild)
                        {
                            if (workflow == MagicLightmapSwitcher.Workflow.MultiScene)
                            {
                                if (customBlendable.gameObject.scene != SceneManager.GetActiveScene())
                                {
                                    continue;
                                }
                                else
                                {
                                    BuildCollectedCustomBlendableData(i, customBlendable);
                                }
                            }
                            else
                            {
                                BuildCollectedCustomBlendableData(i, customBlendable);
                            }
                        }

                        for (int j = 0; j < collectedCustomBlendableDatas.Count; j++)
                        {
                            if (customBlendable.sourceScriptId == collectedCustomBlendableDatas[j].sourceScriptId)
                            {
                                UpdateFields(customBlendable, collectedCustomBlendableDatas[j]);
                            }
                        }
                    }
                }
            }

            if (rebuild)
            {
                rebuild = false;
            }
        }

        public void UpdateCustomBlendableData(float localBlendFactor, float blendFactor, float reflectionsRangedBlend, float lightmapsRangedBlend, int from, int to, int blendableLightmapsCount)
        {
            float segmentsRangedBlend = 0;

            for (int i = 0; i < collectedCustomBlendableDatas.Count; i++)
            {
                if (!initialized)
                {
                    initialized = true;

                    _MLS_Sky_Cubemap_Blend_Factor = Shader.PropertyToID("_MLS_Sky_Cubemap_Blend_Factor");
                    _MLS_Sky_Cubemap_Blend_From = Shader.PropertyToID("_MLS_Sky_Cubemap_Blend_From");
                    _MLS_Sky_Cubemap_Blend_To = Shader.PropertyToID("_MLS_Sky_Cubemap_Blend_To");

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED
                    hdriSky = collectedCustomBlendableDatas[i].sourceScript.attachedScript as HDRISky;
#endif
#if MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                    _MLS_ENABLE_DISTORTION = Shader.PropertyToID("_MLS_ENABLE_DISTORTION");
                    _MLS_USE_FLOWMAP = Shader.PropertyToID("_MLS_USE_FLOWMAP");
#endif
                }

                for (int j = 0; j < collectedCustomBlendableDatas[i].blendableFloatFieldsDatas.Count; j++)
                {
                    if (from == collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].dataVariants.Count ||
                        to == collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].dataVariants.Count)
                    {
                        Debug.LogErrorFormat("<color=cyan>MLS:</color> Error in Custom Blendable data. " +
                            "The variable \"" + collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].fieldName +
                            "\" of the object \"" + collectedCustomBlendableDatas[i].sourceScriptName + "\" has an incorrect blending index.");
                        continue;
                    }

                    CollectedCustomBlendableData.BlendableFloatFieldData.DataVariant fromData =
                        collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].dataVariants[from];
                    CollectedCustomBlendableData.BlendableFloatFieldData.DataVariant toData =
                        collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].dataVariants[to];

                    switch (collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].blendingSynchMode)
                    {
                        case CollectedCustomBlendableData.BlendingSynchMode.Self:
                            if (from > 0)
                            {
                                if (toData.fieldValue == 0)
                                {
                                    toData.fieldValue = 1;
                                }

                                segmentsRangedBlend =
                                    Mathf.Clamp((localBlendFactor - fromData.fieldValue) / (toData.fieldValue - fromData.fieldValue), 0, 1);
                            }
                            else
                            {
                                segmentsRangedBlend =
                                    Mathf.Clamp(localBlendFactor / toData.fieldValue, 0, 1);
                            }

                            collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].sourceField.SetValue(
                                collectedCustomBlendableDatas[i].sourceScript,
                                Mathf.Lerp(fromData.fieldValue, toData.fieldValue, localBlendFactor));
                            break;
                        case CollectedCustomBlendableData.BlendingSynchMode.WithGlobalBlend:
                            collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].sourceField.SetValue(
                                collectedCustomBlendableDatas[i].sourceScript, Mathf.Lerp(0, 1, blendFactor));
                            break;
                        case CollectedCustomBlendableData.BlendingSynchMode.WithReflections:
                            collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].sourceField.SetValue(
                                collectedCustomBlendableDatas[i].sourceScript, Mathf.Lerp(fromData.fieldValue, toData.fieldValue, reflectionsRangedBlend));
                            break;
                        case CollectedCustomBlendableData.BlendingSynchMode.WithLightmaps:
                            collectedCustomBlendableDatas[i].blendableFloatFieldsDatas[j].sourceField.SetValue(
                                collectedCustomBlendableDatas[i].sourceScript, Mathf.Lerp(fromData.fieldValue, toData.fieldValue, lightmapsRangedBlend));
                            break;
                    }
                }

                for (int j = 0; j < collectedCustomBlendableDatas[i].blendableColorFieldsDatas.Count; j++)
                {
                    if (from == collectedCustomBlendableDatas[i].blendableColorFieldsDatas[j].dataVariants.Count ||
                        to == collectedCustomBlendableDatas[i].blendableColorFieldsDatas[j].dataVariants.Count)
                    {
                        Debug.LogErrorFormat("<color=cyan>MLS:</color> Error in Custom Blendable data. " +
                            "The variable \"" + collectedCustomBlendableDatas[i].blendableColorFieldsDatas[j].fieldName +
                            "\" of the object \"" + collectedCustomBlendableDatas[i].sourceScriptName + "\" has an incorrect blending index.");
                        continue;
                    }

                    CollectedCustomBlendableData.BlendableColorFieldData.DataVariant fromData =
                        collectedCustomBlendableDatas[i].blendableColorFieldsDatas[j].dataVariants[from];
                    CollectedCustomBlendableData.BlendableColorFieldData.DataVariant toData =
                        collectedCustomBlendableDatas[i].blendableColorFieldsDatas[j].dataVariants[to];

                    switch (collectedCustomBlendableDatas[i].blendableColorFieldsDatas[j].blendingSynchMode)
                    {
                        case CollectedCustomBlendableData.BlendingSynchMode.Self:
                            collectedCustomBlendableDatas[i].blendableColorFieldsDatas[j].sourceField.SetValue(
                                collectedCustomBlendableDatas[i].sourceScript,
                                Color.Lerp(fromData.fieldValue, toData.fieldValue, localBlendFactor));
                            break;
                        case CollectedCustomBlendableData.BlendingSynchMode.WithGlobalBlend:
                            break;
                        case CollectedCustomBlendableData.BlendingSynchMode.WithReflections:
                            collectedCustomBlendableDatas[i].blendableColorFieldsDatas[j].sourceField.SetValue(
                                collectedCustomBlendableDatas[i].sourceScript, Color.Lerp(fromData.fieldValue, toData.fieldValue, reflectionsRangedBlend));
                            break;
                        case CollectedCustomBlendableData.BlendingSynchMode.WithLightmaps:
                            collectedCustomBlendableDatas[i].blendableColorFieldsDatas[j].sourceField.SetValue(
                                collectedCustomBlendableDatas[i].sourceScript, Color.Lerp(fromData.fieldValue, toData.fieldValue, lightmapsRangedBlend));
                            break;
                    }
                }

                for (int j = 0; j < collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas.Count; j++)
                {
                    if (from == collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas[j].dataVariants.Count ||
                        to == collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas[j].dataVariants.Count)
                    {
                        Debug.LogErrorFormat("<color=cyan>MLS:</color> Error in Custom Blendable data. " +
                            "The variable \"" + collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas[j].fieldName +
                            "\" of the object \"" + collectedCustomBlendableDatas[i].sourceScriptName + "\" has an incorrect blending index.");
                        continue;
                    }

                    CollectedCustomBlendableData.BlendableCubemapFieldData.DataVariant fromData =
                        collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas[j].dataVariants[from];
                    CollectedCustomBlendableData.BlendableCubemapFieldData.DataVariant toData =
                        collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas[j].dataVariants[to];

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED
                    Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_From, fromData.fieldValue);
                    Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_To, toData.fieldValue);
#endif
#if MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
                    if (hdriSky != null)
                    {
                        if (hdriSky.enableDistortion.value)
                        {
                            if (hdriSky.procedural.value)
                            {
                                Shader.SetGlobalInt(_MLS_ENABLE_DISTORTION, 1);
                                Shader.SetGlobalInt(_MLS_USE_FLOWMAP, 0);
                            }
                            else
                            {
                                Shader.SetGlobalInt(_MLS_ENABLE_DISTORTION, 1);
                                Shader.SetGlobalInt(_MLS_USE_FLOWMAP, 1);
                            }
                        }
                        else
                        {
                            Shader.SetGlobalInt(_MLS_ENABLE_DISTORTION, 0);
                            Shader.SetGlobalInt(_MLS_USE_FLOWMAP, 0);
                        }

                        Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_From, fromData.fieldValue);
                        Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_To, toData.fieldValue);
                    }
                    else
                    {
                        Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_From, fromData.fieldValue);
                        Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_To, toData.fieldValue);
                    }
#else
                    if (lastFrom != from)
                    {
                        Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_From, fromData.fieldValue);
                        Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_To, toData.fieldValue);
                    }
#endif

                    switch (collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas[j].blendingSynchMode)
                    {
                        case CollectedCustomBlendableData.BlendingSynchMode.WithGlobalBlend:
                            Shader.SetGlobalFloat(_MLS_Sky_Cubemap_Blend_Factor, blendFactor);
                            break;
                        case CollectedCustomBlendableData.BlendingSynchMode.WithReflections:
                            Shader.SetGlobalFloat(_MLS_Sky_Cubemap_Blend_Factor, reflectionsRangedBlend);
                            break;
                        case CollectedCustomBlendableData.BlendingSynchMode.WithLightmaps:
                            Shader.SetGlobalFloat(_MLS_Sky_Cubemap_Blend_Factor, lightmapsRangedBlend);
                            break;
                    }
                }
            }

            lastFrom = from;
        }

        public void RebuildSourceList()
        {
            if (cyclic)
            {
                startValues.Add(0);

                for (int i = 0; i < startValues.Count; i++)
                {
                    startValues[i] = (float) Mathf.Round((float) i * ((float) 1 / ((float) startValues.Count - 1)) * 100f) / 100f;
                }

                LightmapData cylicLightmap =
                new LightmapData(blendableLightmaps[0].blendingIndex,
                blendableLightmaps[0].prevBlendingIndex,
                blendableLightmaps[0].startValue,
                blendableLightmaps[0].reflectionsBlendingRange,
                blendableLightmaps[0].lightmapBlendingRange,
                blendableLightmaps[0].lightingData);

                blendableLightmaps.Add(cylicLightmap);
            }
            else
            {
                startValues.RemoveAt(startValues.Count - 1);

                for (int i = 0; i < startValues.Count; i++)
                {
                    startValues[i] = (float) Mathf.Round((float) i * ((float) 1 / ((float) startValues.Count - 1)) * 100f) / 100f;
                }

                if (blendableLightmaps[0].lightingData == blendableLightmaps[blendableLightmaps.Count - 1].lightingData)
                {
                    blendableLightmaps.RemoveAt(blendableLightmaps.Count - 1);
                }
            }
        }

#if UNITY_EDITOR
        public void AddLightmapData(StoredLightmapData storedLightmapData)
        {
            LightmapData newLigtmapData = new LightmapData();

            newLigtmapData.lightingData = storedLightmapData;
            newLigtmapData.blendingIndex = blendableLightmaps.Count;
            newLigtmapData.startValue = blendableLightmaps.Count * 0.25f;
            newLigtmapData.reflectionsBlendingRange = new Vector2(0, 1);
            newLigtmapData.lightmapBlendingRange = new Vector2(0, 1);

            if (blendableLightmaps.Find(item => item.lightingData.name == newLigtmapData.lightingData.name) == null)
            {
                blendableLightmaps.Add(newLigtmapData);
                blendingOrder.Add(blendableLightmaps.Count - 1);

                EditorUtility.SetDirty(this);
            }
            else
            {
                EditorUtility.DisplayDialog("Magic Lightmap Switcher", "This asset has already been added to the blending scenario.", "OK");
            }
        }
#endif

        public void SetActive(MagicLightmapSwitcher magicLightmapSwitcher)
        {
            magicLightmapSwitcher.currentLightmapScenario = this;
            magicLightmapSwitcher.lastLightmapScenario = magicLightmapSwitcher.currentLightmapScenario;
        }

#if UNITY_EDITOR
        public void BuildReorderableList()
        {
            GUIStyle labelCenteredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 0, 5),
                fontSize = 11,
                wordWrap = true,
                fontStyle = FontStyle.Bold
            };

            if (scenarioSerializedObject == null)
            {
                scenarioSerializedObject = new SerializedObject(this);
            }

            reorderableStoredLightmapDatas = new ReorderableList(scenarioSerializedObject, scenarioSerializedObject.FindProperty("blendableLightmaps"), true, true, true, true);

            reorderableStoredLightmapDatas.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y, 15, rect.height),
                    "");

                EditorGUI.LabelField(
                    new Rect(rect.x + 15, rect.y, 20, rect.height),
                    "№");

                EditorGUI.LabelField(
                    new Rect(rect.x + 35, rect.y, 80, rect.height),
                    "Short Name");

                EditorGUI.LabelField(
                    new Rect(rect.x + 115, rect.y, rect.width - 250, rect.height),
                    "Asset Name", labelCenteredStyle);

                EditorGUI.LabelField(
                    new Rect(rect.x + 100 + (rect.width - 260), rect.y, 75, rect.height),
                    "Target Value");
                EditorGUI.LabelField(
                    new Rect(rect.x + (rect.width - 65), rect.y, 75, rect.height),
                    "Switching");
            };

            reorderableStoredLightmapDatas.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = reorderableStoredLightmapDatas.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight),
                    index.ToString());

                StoredLightmapData asset = element.FindPropertyRelative("lightingData").objectReferenceValue as StoredLightmapData;

                EditorGUI.LabelField(
                        new Rect(rect.x + 20, rect.y, 80, EditorGUIUtility.singleLineHeight),
                        asset == null ? "-No Asset-" : asset.dataName);

                EditorGUI.PropertyField(
                    new Rect(rect.x + 100, rect.y, rect.width - 260, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("lightingData"),
                    GUIContent.none);

                if (index > 0 && index < reorderableStoredLightmapDatas.serializedProperty.arraySize - 1)
                {
                    startValues[index] = EditorGUI.FloatField(
                        new Rect(rect.x + 100 + (rect.width - 230), rect.y, 50, EditorGUIUtility.singleLineHeight),
                        startValues[index]);
                }
                else
                {
                    if (index == reorderableStoredLightmapDatas.serializedProperty.arraySize - 1)
                    {
                        EditorGUI.LabelField(
                            new Rect(rect.x + 100 + (rect.width - 230), rect.y, 50, EditorGUIUtility.singleLineHeight),
                            "1");
                    }
                    else
                    {
                        EditorGUI.LabelField(
                            new Rect(rect.x + 100 + (rect.width - 230), rect.y, 50, EditorGUIUtility.singleLineHeight),
                            "0");
                    }
                }

                if (GUI.Button(new Rect(rect.x + (rect.width - 75), rect.y, 75, EditorGUIUtility.singleLineHeight), "Load"))
                {
                    if (lightmapSwitcher == null)
                    {
                        lightmapSwitcher = runtimeAPI.GetSwitcherSource(this.targetScene);
                    }

                    lightmapSwitcher.lightingDataSwitching = true;

                    if (lightmapSwitcher.systemProperties.useSwitchingOnly)
                    {
                        Switching.LoadLightingData(lightmapSwitcher, index, Switching.LoadMode.Asynchronously);
                    }
                    else
                    {
                        Blending.Blend(lightmapSwitcher, startValues[index], this, this.targetScene);
                    }

                    lightmapSwitcher.OnLoadedLightmapChanged[eventsListId].Invoke(this, index);
                }
            };

            reorderableStoredLightmapDatas.onRemoveCallback = (ReorderableList l) =>
            {
                SerializedProperty startValues = scenarioSerializedObject.FindProperty("startValues");
                SerializedProperty blendableLightmaps = scenarioSerializedObject.FindProperty("blendableLightmaps");

                startValues.DeleteArrayElementAtIndex(l.index);
                blendableLightmaps.DeleteArrayElementAtIndex(l.index);

                for (int i = 0; i < startValues.arraySize; i++)
                {
                    startValues.GetArrayElementAtIndex(i).floatValue = (float) Mathf.Round((float) i * ((float) 1 / ((float) startValues.arraySize - 1)) * 100f) / 100f;
                }

                if (cyclic)
                {
                    if (EditorUtility.DisplayDialog("Magic Lightmap Switcher!",
                        "You cannot remove the last lightmap from the cyclic list.", "OK"))
                    {
                        return;
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Magic Lightmap Switcher!",
                        "Are you sure you want to delete the Asset?", "Yes", "No"))
                    {
                        //ReorderableList.defaultBehaviours.DoRemoveButton(l);
                    }
                }
            };

            reorderableStoredLightmapDatas.onAddCallback = (ReorderableList l) =>
            {
                ReorderableList.defaultBehaviours.DoAddButton(l);

                SerializedProperty startValues = scenarioSerializedObject.FindProperty("startValues");

                startValues.InsertArrayElementAtIndex(l.index);

                for (int i = 0; i < startValues.arraySize; i++)
                {
                    startValues.GetArrayElementAtIndex(i).floatValue = (float) Mathf.Round((float) i * ((float) 1 / ((float) startValues.arraySize - 1)) * 100f) / 100f;
                }
            };
            
            reorderableStoredLightmapDatas.onReorderCallback = (ReorderableList l) =>
            {
                scenarioSerializedObject.FindProperty("orderChanged").boolValue = true;
            };
        }
#endif
    }
}
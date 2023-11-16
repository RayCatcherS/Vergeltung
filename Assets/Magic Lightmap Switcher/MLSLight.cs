using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [RequireComponent(typeof(Light)), ExecuteInEditMode]
    public class MLSLight : MonoBehaviour
    {
        [SerializeField]
        public bool exludeFromStoring;
        [SerializeField]
        public string lightGUID;
        public bool destroyedFromManager = false;
        public string lastEditedBy;
        public bool editedDirectly;
        public bool presetManagerActive;
        public Light sourceLight;
        public float shadowTypeSwitchValue;

        private void Start()
        {
            sourceLight = GetComponent<Light>();
        }

        public void UpdateGUID()
        {
            lightGUID = Guid.NewGuid().ToString();
        }

#if UNITY_EDITOR

        private void OnDestroy()
        {
            if (!destroyedFromManager)
            {
                MagicLightmapSwitcher magicLightmapSwitcher = FindObjectOfType<MagicLightmapSwitcher>();

                if (magicLightmapSwitcher != null)
                {
                    List<MagicLightmapSwitcher.SceneLightingPreset> sceneLightingPresets = magicLightmapSwitcher.lightingPresets;

                    for (int i = 0; i < sceneLightingPresets.Count; i++)
                    {
                        sceneLightingPresets[i].lightSourceSettings.Remove(sceneLightingPresets[i].lightSourceSettings.Find(item => item.light == gameObject.GetComponent<Light>()));
                    }
                }
            }
        }
#endif
    }
}
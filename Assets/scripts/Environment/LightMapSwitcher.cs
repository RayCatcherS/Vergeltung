using MagicLightmapSwitcher;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace MagicLightmapSwitcher {
    public enum LigthMap {
        light,
        noLight
    }
    public class LightMapSwitcher : MonoBehaviour {
        

        [SerializeField] LigthMap StartingLightMap = LigthMap.light;

        void Start() {
            SwitchToLightmap(StartingLightMap);
        }

        public static void SwitchToLightmap(LigthMap lightMap) {


            MagicLightmapSwitcher lightmapSwitcher;
            RuntimeAPI runtimeAPI = new RuntimeAPI();
            List<StoredLightingScenario> availableScenarios = new List<StoredLightingScenario>();



            string sceneName = SceneManager.GetActiveScene().name;
            lightmapSwitcher = runtimeAPI.GetSwitcherSource(sceneName);
            //Debug.Log(lightmapSwitcher.sceneLightmapDatas.Count);

            Switching.LoadLightingData(lightmapSwitcher,
                lightMap == LigthMap.light ? 0 : 1,
                Switching.LoadMode.Asynchronously);

            ChangedLightMap.Invoke(lightMap);

        }

        public static Action<LigthMap> ChangedLightMap;
    }
}


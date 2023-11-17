using MagicLightmapSwitcher;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace MagicLightmapSwitcher {
    public class LightMapSwitcher : MonoBehaviour {

        void Start() {
            SwitchToLightmap();

        }

        public void SwitchToLightmap() {

            // switch lightmaps
            /*MagicLightmapSwitcher lightmapSwitcher;
            RuntimeAPI runtimeAPI = new RuntimeAPI();

            List<StoredLightingScenario> availableScenarios = new List<StoredLightingScenario>();*/


            //MagicLightmapSwitcher.UpdateLightingScenarios(SceneManager.GetActiveScene().name, true);



            //StoredLightingScenario
            string sceneName = SceneManager.GetActiveScene().name;
            /*lightmapSwitcher = runtimeAPI.GetSwitcherSource(sceneName);
            Debug.Log(lightmapSwitcher.sceneLightmapDatas.Count);*/
            //Switching.LoadLightingData(lightmapSwitcher, 0, Switching.LoadMode.Asynchronously);

        }
    }
}


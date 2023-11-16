using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLightmapSwitcher
{
    public static class SceneManagment
    {
        public static bool sceneProcessing;

        public static void LoadSceneAdditive(MonoBehaviour callerObject, string sceneName, bool setActiveOnLoad)
        {
            callerObject.StartCoroutine(_LoadSceneAdditive(sceneName, setActiveOnLoad));
        }

        public static void UnloadScene(MonoBehaviour callerObject, string sceneName)
        {
            callerObject.StartCoroutine(_UnloadScene(sceneName));
        }

        private static IEnumerator _LoadSceneAdditive(string sceneName, bool setActiveOnLoad)
        {
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                while (sceneProcessing)
                {
                    yield return null;
                }

                sceneProcessing = true;

#if !UNITY_2020_1_OR_NEWER
                MagicLightmapSwitcher current = RuntimeAPI.GetSwitcherInstanceStatic(SceneManager.GetActiveScene().name);

                if (current != null)
                {
                    current.stopProbesBlending = true;
                }

                LightmapSettings.lightProbes = null;
#endif
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                asyncOperation.completed += (AsyncOperation) =>
                {
                    if (setActiveOnLoad)
                    {
                        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
                    }

                    RuntimeAPI.GetSwitcherInstanceStatic(sceneName).OnSceneLoadComplete(sceneName);
                };
            }
        }

        private static IEnumerator _UnloadScene(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                while (sceneProcessing)
                {
                    yield return null;
                }

                //sceneProcessing = true;

                AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);

                asyncOperation.completed += (AsyncOperation) =>
                {
                    sceneProcessing = false;                    
                };
            }
        }
    }
}

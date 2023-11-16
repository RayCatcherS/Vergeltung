using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLightmapSwitcher
{
    public class CallAdditiveSceneLoading : MonoBehaviour
    {
        public string sceneName;
        public bool loadOnStart;
        public bool setActiveOnLoad;

        void Start()
        {
            if (loadOnStart)
            {
                SceneManagment.LoadSceneAdditive(this, sceneName, setActiveOnLoad);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            SceneManagment.LoadSceneAdditive(this, sceneName, setActiveOnLoad);
        }
    }
}

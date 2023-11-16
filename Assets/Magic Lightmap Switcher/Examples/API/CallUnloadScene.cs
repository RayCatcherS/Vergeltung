using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLightmapSwitcher
{
    public class CallUnloadScene : MonoBehaviour
    {
        public string sceneName;
        
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            SceneManagment.UnloadScene(this, sceneName);
        }
    }
}

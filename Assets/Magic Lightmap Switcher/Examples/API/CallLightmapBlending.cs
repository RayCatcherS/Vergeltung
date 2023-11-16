//Import Magic Lightmap Switcher class
using MagicLightmapSwitcher;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class CallLightmapBlending : MonoBehaviour
    {
        public enum BlendingType
        {
            Cyclic,
            Once
        }
        
        public StoredLightingScenario lightingScenario;
        public float blendingLength;
        public RuntimeAPI.BlendingDirection blendingDirection;
        public BlendingType blendingType;
        public bool startBlending;

        private RuntimeAPI runtimeAPI;

        // Start is called before the first frame update
        void Start()
        {
            runtimeAPI = new RuntimeAPI();
        }

        // Update is called once per frame
        void Update()
        {
            switch (blendingType)
            {
                case BlendingType.Cyclic:
                    runtimeAPI.BlendLightmapsCyclic(blendingLength, lightingScenario, blendingDirection);
                    break;
                case BlendingType.Once:
                    if (runtimeAPI.currentBlendingTime >= blendingLength)
                    {
                        startBlending = false;
                        runtimeAPI.ResetBlendingTime(blendingLength);
                    }
                    
                    if (startBlending)
                    {
                        runtimeAPI.BlendLightmaps(blendingLength, lightingScenario, blendingDirection);
                    }
                    break;
            }
        }
    }
}

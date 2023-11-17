using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class ChangeObjectStateOnEvent : MonoBehaviour
    {
        public void ChageObjectState(StoredLightingScenario storedLightingScenario, float globalBlend, float reflectionsBlend, float lightmapsBlend)
        {
            if (globalBlend > 0.5f)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void ChangeObjectState(StoredLightingScenario storedLightingScenario, int lightmapIndex)
        {
            if (lightmapIndex == 1)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }
}
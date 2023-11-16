using MagicLightmapSwitcher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallLightmapsSwitching : MonoBehaviour
{
    public StoredLightingScenario lightingScenario;
    public int lightmapIndex;

    private RuntimeAPI runtimeAPI;

    void Start()
    {
        runtimeAPI = new RuntimeAPI();
    }

    private void OnTriggerEnter(Collider other)
    {
        runtimeAPI.SwitchLightmap(lightmapIndex, lightingScenario);
    }
}

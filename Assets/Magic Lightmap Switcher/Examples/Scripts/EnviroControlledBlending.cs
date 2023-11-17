using MagicLightmapSwitcher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviroControlledBlending : MonoBehaviour
{
    public StoredLightingScenario lightingScenario;

    private RuntimeAPI runtimeAPI;

    // Start is called before the first frame update
    void Start()
    {
        runtimeAPI = new RuntimeAPI();
    }

    // Update is called once per frame
    void Update()
    {
#if ENVIRO_LW || ENVIRO_HD
        runtimeAPI.EnviroControlled(lightingScenario);
#endif
    }
}

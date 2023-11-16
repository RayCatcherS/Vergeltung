using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLightmapSwitcher;

[ExecuteInEditMode]
public class EnviroHours : MLSCustomBlendable
{
    [Range(0, 24)]
    public float mls_b_EnviroHours;

    void OnEnable()
    {
        base.OnEnable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying)
        {
#if ENVIRO_LW || ENVIRO_HD || ENVIRO_HDRP
            EnviroSkyMgr.instance.SetTimeOfDay(mls_b_EnviroHours);
#endif
        }
    }
}

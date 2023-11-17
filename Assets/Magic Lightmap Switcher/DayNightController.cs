using UnityEngine;
using MagicLightmapSwitcher;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
using UnityEngine.Rendering.HighDefinition;
#endif

[ExecuteInEditMode]
public class DayNightController : MLSCustomBlendable
{
    public Light sun;
    public Light moon;
    public Gradient gradient;
    public float maxSunIntensity;
    public float maxMoonIntensity;
    public float secondsInFullDay = 120f;
    [Range(0,1)]
    public float mls_b_currentTimeOfDay = 0;
    [HideInInspector]
    public float timeMultiplier = 1f;

    MLSLight sunObject;
    MLSLight moonObject;

    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    float sunIntensity;
    float moonIntensity;

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
    HDAdditionalLightData HdrpSun;
    HDAdditionalLightData HdrpMoon;
#endif

    void Start()
    {

    }

    void Update()
    {
        if (sun == null || moon == null)
        {
            return;
        }
        else
        {
            sunObject = sun.GetComponent<MLSLight>();
            moonObject = moon.GetComponent<MLSLight>();

            if (sunObject == null)
            {
                sunObject = sun.gameObject.AddComponent<MLSLight>();
                sunObject.exludeFromStoring = true;
            }

            if (moonObject == null)
            {
                moonObject = moon.gameObject.AddComponent<MLSLight>();
                moonObject.exludeFromStoring = true;
            }
        }
        
        UpdateSun();

        //if (Application.isPlaying)
        //{
        //    mls_b_currentTimeOfDay += (Time.deltaTime / secondsInFullDay) * timeMultiplier;

        //    if (mls_b_currentTimeOfDay >= 1)
        //    {
        //        mls_b_currentTimeOfDay = 0;
        //    }
        //}
    }

    void UpdateSun()
    {
#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
        if (HdrpSun == null || HdrpMoon == null)
        {
            HdrpSun = sun.gameObject.GetComponent<HDAdditionalLightData>();
            HdrpMoon = moon.gameObject.GetComponent<HDAdditionalLightData>();
        }
#endif

        sun.transform.localRotation = Quaternion.Euler((mls_b_currentTimeOfDay * 360f) - 90, (mls_b_currentTimeOfDay * 180f) - 90, 0);
        moon.transform.localRotation = Quaternion.Euler(-((mls_b_currentTimeOfDay * 360f) - 90), 0, 0);

        float sunIntensityMultiplier = 1;

        if (mls_b_currentTimeOfDay <= 0.23f || mls_b_currentTimeOfDay >= 0.75f)
        {
            sunIntensityMultiplier = 0;

            //sun.shadows = LightShadows.None;
            moon.shadows = LightShadows.Soft;
        }
        else if (mls_b_currentTimeOfDay <= 0.25f)
        {
            sunIntensityMultiplier = Mathf.Clamp01((mls_b_currentTimeOfDay - 0.23f) * (1 / 0.02f));
        }
        else if (mls_b_currentTimeOfDay >= 0.73f)
        {
            sunIntensityMultiplier = Mathf.Clamp01(1 - ((mls_b_currentTimeOfDay - 0.73f) * (1 / 0.02f)));
        }

        float moonIntensityMultiplier = 1;

        if (mls_b_currentTimeOfDay >= 0.22f && mls_b_currentTimeOfDay <= 0.74f)
        {
            moonIntensityMultiplier = 0;

            sun.shadows = LightShadows.Soft;
            //moon.shadows = LightShadows.None;
        }
        else if (mls_b_currentTimeOfDay >= 0.75f)
        {
            moonIntensityMultiplier = Mathf.Clamp01((mls_b_currentTimeOfDay - 0.75f) * (1 / 0.02f));
        }
        else if (mls_b_currentTimeOfDay <= 0.25f)
        {
            moonIntensityMultiplier = Mathf.Clamp01(1 - ((mls_b_currentTimeOfDay - 0.23f) * (1 / 0.02f)));
        }

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
        HdrpSun.intensity = maxSunIntensity * sunIntensityMultiplier;
        HdrpMoon.intensity = maxMoonIntensity * moonIntensityMultiplier;
        HdrpSun.color = gradient.Evaluate(mls_b_currentTimeOfDay);
#else
        sun.intensity = maxSunIntensity * sunIntensityMultiplier;
        moon.intensity = maxMoonIntensity * moonIntensityMultiplier;
        sun.color = gradient.Evaluate(mls_b_currentTimeOfDay);
#endif
    }
}

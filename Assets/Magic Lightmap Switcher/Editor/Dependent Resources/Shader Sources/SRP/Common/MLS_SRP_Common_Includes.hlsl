#ifndef MLS_BLENDING_STANDARD
#define MLS_BLENDING_STANDARD

SamplerState mls_bilinear_clamp_sampler;
SamplerState mls_trilinear_clamp_sampler;

// Lightmaps Processing
float _MLS_Lightmaps_Blend_Factor;

#ifdef MLS_TEXTURE2D_ARRAYS_ON
    UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_MLS_Lightmap_Color_Array);
    UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_MLS_Lightmap_Directional_Array);
    UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_MLS_Lightmap_ShadowMask_Array);

    UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_MLS_BakeryRNM_0_Array);
    UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_MLS_BakeryRNM_1_Array);
    UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_MLS_BakeryRNM_2_Array);
#endif

#ifdef MLS_TEXTURECUBE_ARRAYS_ON
    UNITY_DECLARE_TEXCUBEARRAY(_MLS_Cubemap_Array);
#endif

UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_Color_Blend_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_Color_Blend_To);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_Dir_Blend_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_Dir_Blend_To);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_ShadowMask_Blend_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_ShadowMask_Blend_To);

UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM0_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM0_To);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM1_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM1_To);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM2_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM2_To);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolume0_From);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolume0_To);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolume1_From);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolume1_To);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolume2_From);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolume2_To);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolumeMask_From);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolumeMask_To);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolumeCompressed_From);
UNITY_DECLARE_TEX3D_NOSAMPLER(_MLS_BakeryVolumeCompressed_To);

struct BakeryVolumeData
{
    float4 Volume0Blended;
    float4 Volume1Blended;
    float4 Volume2Blended;
    float4 VolumeMaskBlended;
};

// Reflections Prcessing
float _MLS_Reflections_Blend_Factor;
int _MLS_ReflectionsFlag;

UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_Reflection_Blend_From_0);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_Reflection_Blend_To_0);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_Reflection_Blend_From_1);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_Reflection_Blend_To_1);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_SkyReflection_Blend_From);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_SkyReflection_Blend_To);

// Sky Cubemap Processing
float _MLS_Sky_Cubemap_Blend_Factor;
float _MLS_Sky_Blend_From_Rotation;
float _MLS_Sky_Blend_To_Rotation;
float _MLS_Sky_Blend_From_Exposure;
float _MLS_Sky_Blend_To_Exposure;
half4 _MLS_Sky_Blend_From_Tint;
half4 _MLS_Sky_Blend_To_Tint;

UNITY_DECLARE_TEXCUBE(_MLS_Sky_Cubemap_Blend_From);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_Sky_Cubemap_Blend_To);

// General
int _MLS_ENABLE_LIGHTMAPS_BLENDING;
int _MLS_ENABLE_REFLECTIONS_BLENDING;
int _MLS_ENABLE_SKY_CUBEMAPS_BLENDING;
float4 _MLS_CURRENT_LIGHTMAP_PAIR;
float4 _MLS_OBJECT_BLENDING_DATA;


float4 BlendTwoTextures(int lightmapType, float2 uv, SamplerState samplerState)
{
    half4 textureFrom;
    half4 textureTo;

    switch (lightmapType)
    {
    case 0:
        textureFrom = SAMPLE_TEXTURE2D(_MLS_Lightmap_Color_Blend_From, samplerState, uv.xy);
        textureTo = SAMPLE_TEXTURE2D(_MLS_Lightmap_Color_Blend_To, samplerState, uv.xy);
        break;
    case 1:
        textureFrom = SAMPLE_TEXTURE2D(_MLS_Lightmap_Dir_Blend_From, samplerState, uv.xy);
        textureTo = SAMPLE_TEXTURE2D(_MLS_Lightmap_Dir_Blend_To, samplerState, uv.xy);
        break;
    }

    return lerp(textureFrom, textureTo, _MLS_Lightmaps_Blend_Factor);
}

float4 BlendTwoCubeTextures(int probeIndex, float3 reflection, half mip, SamplerState samplerState)
{
    float4 textureFrom;
    float4 textureTo;

    switch (probeIndex)
    {
    case 0:
        textureFrom = SAMPLE_TEXTURECUBE_LOD(_MLS_Reflection_Blend_From_0, samplerState, reflection, mip);
        textureTo = SAMPLE_TEXTURECUBE_LOD(_MLS_Reflection_Blend_To_0, samplerState, reflection, mip);
        break;
    case 1:
        textureFrom = SAMPLE_TEXTURECUBE_LOD(_MLS_Reflection_Blend_From_1, samplerState, reflection, mip);
        textureTo = SAMPLE_TEXTURECUBE_LOD(_MLS_Reflection_Blend_To_1, samplerState, reflection, mip);
        break;
    case 2:
        textureFrom = SAMPLE_TEXTURECUBE_LOD(_MLS_SkyReflection_Blend_From, samplerState, reflection, mip);
        textureTo = SAMPLE_TEXTURECUBE_LOD(_MLS_SkyReflection_Blend_To, samplerState, reflection, mip);
        break;
    }

    return lerp(textureFrom, textureTo, _MLS_Reflections_Blend_Factor);
}
//</MLS_COMMON_INCLUDES>
//<MLS_COMMON_INCLUDES>
#ifndef MLS_BLENDING_SRP
#define MLS_BLENDING_SRP
// Lightmaps Processing
float _MLS_Lightmaps_Blend_Factor;

TEXTURE2D(_MLS_Lightmap_Color_Blend_From);
TEXTURE2D(_MLS_Lightmap_Color_Blend_To); 
TEXTURE2D(_MLS_Lightmap_Dir_Blend_From);
TEXTURE2D(_MLS_Lightmap_Dir_Blend_To);
TEXTURE2D(_MLS_Lightmap_ShadowMask_Blend_From);
TEXTURE2D(_MLS_Lightmap_ShadowMask_Blend_To);
TEXTURE2D(_MLS_BakeryRNM0_From); SAMPLER(sampler_MLS_BakeryRNM0_From);
TEXTURE2D(_MLS_BakeryRNM0_To);
TEXTURE2D(_MLS_BakeryRNM1_From); SAMPLER(sampler_MLS_BakeryRNM1_From);
TEXTURE2D(_MLS_BakeryRNM1_To);
TEXTURE2D(_MLS_BakeryRNM2_From); SAMPLER(sampler_MLS_BakeryRNM2_From);
TEXTURE2D(_MLS_BakeryRNM2_To);

// Reflections Prcessing
float _MLS_Reflections_Blend_Factor = 0;
int _MLS_ReflectionsFlag = 0;

TEXTURECUBE(_MLS_Reflection_Blend_From_0);
TEXTURECUBE(_MLS_Reflection_Blend_To_0);
TEXTURECUBE(_MLS_Reflection_Blend_From_1);
TEXTURECUBE(_MLS_Reflection_Blend_To_1);
TEXTURECUBE(_MLS_SkyReflection_Blend_From);
TEXTURECUBE(_MLS_SkyReflection_Blend_To);

// Sky Cubemap Processing
float _MLS_Sky_Cubemap_Blend_Factor;

TEXTURECUBE(_MLS_Sky_Cubemap_Blend_From);
TEXTURECUBE(_MLS_Sky_Cubemap_Blend_To);

// General 
int _MLS_ENABLE_LIGHTMAPS_BLENDING = 0;
int _MLS_ENABLE_REFLECTIONS_BLENDING = 0;
int _MLS_ENABLE_SKY_CUBEMAPS_BLENDING = 0;

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
    case 2:
        textureFrom = SAMPLE_TEXTURE2D(_MLS_Lightmap_ShadowMask_Blend_From, samplerState, uv.xy);
        textureTo = SAMPLE_TEXTURE2D(_MLS_Lightmap_ShadowMask_Blend_To, samplerState, uv.xy);
        break;
    }

    return lerp(textureFrom, textureTo, _MLS_Lightmaps_Blend_Factor);
}

float4 BlendBakeryRNM(int lightmapType, float2 uv)
{
    half4 textureFrom;
    half4 textureTo;
    float4 result;

    switch (lightmapType)
    {
    case 0:
        textureFrom = SAMPLE_TEXTURE2D(_MLS_BakeryRNM0_From, sampler_MLS_BakeryRNM0_From, uv.xy);
        textureTo = SAMPLE_TEXTURE2D(_MLS_BakeryRNM0_To, sampler_MLS_BakeryRNM0_From, uv.xy);
        break;
    case 1:
        textureFrom = SAMPLE_TEXTURE2D(_MLS_BakeryRNM1_From, sampler_MLS_BakeryRNM1_From, uv.xy);
        textureTo = SAMPLE_TEXTURE2D(_MLS_BakeryRNM1_To, sampler_MLS_BakeryRNM1_From, uv.xy);
        break;
    case 2:
        textureFrom = SAMPLE_TEXTURE2D(_MLS_BakeryRNM2_From, sampler_MLS_BakeryRNM2_From, uv.xy);
        textureTo = SAMPLE_TEXTURE2D(_MLS_BakeryRNM2_To, sampler_MLS_BakeryRNM2_From, uv.xy);
        break;
    case 3:
        textureFrom = SAMPLE_TEXTURE2D(_MLS_Lightmap_Color_Blend_From, sampler_MLS_BakeryRNM0_From, uv.xy);
        textureTo = SAMPLE_TEXTURE2D(_MLS_Lightmap_Color_Blend_To, sampler_MLS_BakeryRNM0_From, uv.xy);
        break;
    }

    return lerp(textureFrom, textureTo, _MLS_Lightmaps_Blend_Factor);
}

float4 BlendBakeryRNMSampler(int lightmapType, float2 uv, SamplerState samplerState)
{
    half4 textureFrom;
    half4 textureTo;

    switch (lightmapType)
    {
    case 0:
        textureFrom = _MLS_BakeryRNM0_From.Sample(samplerState, uv.xy);
        textureTo = _MLS_BakeryRNM0_To.Sample(samplerState, uv.xy);
        break;
    case 1:
        textureFrom = _MLS_BakeryRNM1_From.Sample(samplerState, uv.xy);
        textureTo = _MLS_BakeryRNM1_To.Sample(samplerState, uv.xy);
        break;
    case 2:
        textureFrom = _MLS_BakeryRNM2_From.Sample(samplerState, uv.xy);
        textureTo = _MLS_BakeryRNM2_To.Sample(samplerState, uv.xy);
        break;
    }

    return lerp(textureFrom, textureTo, _MLS_Lightmaps_Blend_Factor);
}

float4 BlendTwoCubeTextures(int probeIndex, float3 reflection, half mip, SamplerState samplerState)
{
    float4 textureFrom;
    float4 textureTo;
    float blendFactor;

    switch (probeIndex)
    {
    case 0:
        textureFrom = SAMPLE_TEXTURECUBE_LOD(_MLS_Reflection_Blend_From_0, samplerState, reflection, mip);
        textureTo = SAMPLE_TEXTURECUBE_LOD(_MLS_Reflection_Blend_To_0, samplerState, reflection, mip);
        blendFactor = _MLS_Reflections_Blend_Factor;
        break;
    case 1:
        textureFrom = SAMPLE_TEXTURECUBE_LOD(_MLS_Reflection_Blend_From_1, samplerState, reflection, mip);
        textureTo = SAMPLE_TEXTURECUBE_LOD(_MLS_Reflection_Blend_To_1, samplerState, reflection, mip);
        blendFactor = _MLS_Reflections_Blend_Factor;
        break;
    case 2:
        textureFrom = SAMPLE_TEXTURECUBE_LOD(_MLS_SkyReflection_Blend_From, samplerState, reflection, mip);
        textureTo = SAMPLE_TEXTURECUBE_LOD(_MLS_SkyReflection_Blend_To, samplerState, reflection, mip);
        blendFactor = _MLS_Reflections_Blend_Factor;
        break;
    case 3:
        textureFrom = SAMPLE_TEXTURECUBE_LOD(_MLS_Sky_Cubemap_Blend_From, samplerState, reflection, mip);
        textureTo = SAMPLE_TEXTURECUBE_LOD(_MLS_Sky_Cubemap_Blend_To, samplerState, reflection, mip);
        blendFactor = _MLS_Sky_Cubemap_Blend_Factor;
        break;
    }

    return lerp(textureFrom, textureTo, blendFactor);
}
#endif //</MLS_COMMON_INCLUDES>
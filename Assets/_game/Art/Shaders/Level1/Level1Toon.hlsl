#ifndef SIMPLE_TOON_URP_INCLUDED
#define SIMPLE_TOON_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_EmissionMap);
SAMPLER(sampler_EmissionMap);
TEXTURE2D(_BumpMap);
SAMPLER(sampler_BumpMap);

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    half4 _Color;
    half4 _EmissionColor;
    half4 _ShnColor;
    half4 _OtlColor;
    float _ColIntense;
    float _ColBright;
    float _AmbientCol;
    float _Segmented;
    float _Steps;
    float _StpSmooth;
    float _Offset;
    float _Clipped;
    float _MinLight;
    float _MaxLight;
    float _Lumin;
    float _ShnOverlap;
    float _ShnIntense;
    float _ShnRange;
    float _ShnSmooth;
    float _OtlWidth;
    float _EmissionEnabled;
    float _UseNormalMap;
    float _BumpScale;
    float _AlphaClip;
    float _Cutoff;
    float _Cull;
CBUFFER_END

struct STAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    float2 lightmapUV : TEXCOORD1;
    float2 dynamicLightmapUV : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct STVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
    half4 tangentWS : TEXCOORD3;
    half fogFactor : TEXCOORD4;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 5);
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

STVaryings STVertex(STAttributes input)
{
    STVaryings output = (STVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = position.positionCS;
    output.positionWS = position.positionWS;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.tangentWS = half4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w * GetOddNegativeScale());
    output.uv = TRANSFORM_TEX(input.uv, _MainTex);
    output.fogFactor = ComputeFogFactor(output.positionCS.z);
    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS, output.vertexSH);
    return output;
}

half4 STSampleColor(float2 uv)
{
    half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;
    if (_AlphaClip > 0.5) clip(color.a - _Cutoff);
    return color;
}

half3 STNormal(STVaryings input)
{
    half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
#if defined(_NORMALMAP)
    half3 tangentWS = normalize(input.tangentWS.xyz);
    half3 bitangentWS = cross(normalWS, tangentWS) * input.tangentWS.w;
    half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
    normalWS = NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, half3x3(tangentWS, bitangentWS, normalWS)));
#endif
    return normalWS;
}

// Keep the pack's lighting controls, with guards for equal limits and zero smoothing.
half STToon(float normalDotLight, half attenuation)
{
    float steps = _Segmented > 0.5 ? max(1.0, floor(_Steps)) : 1.0;
    float smoothing = _Segmented > 0.5 ? saturate(_StpSmooth) : 1.0;
    float offset = clamp(_Offset, -0.99, 1.0);
    float intensity = saturate((normalDotLight + offset) / (1.0 + offset));
    float stepSize = rcp(steps);
    float band = ceil(intensity * steps) * stepSize;
    if (smoothing > 0.0)
    {
        float lower = max(0.0, band - stepSize);
        float transition = saturate((intensity - lower) / max(stepSize * smoothing, 0.0001));
        band = lerp(lower, band, smoothstep(0.0, 1.0, transition));
    }
    float minLight = saturate(_MinLight);
    float maxLight = max(minLight, _MaxLight) + _Lumin;
    float dimmed = band * saturate(attenuation);
    float value = _Clipped > 0.5
        ? saturate((dimmed - minLight) / max(_MaxLight - minLight, 0.0001)) : dimmed;
    return lerp(minLight, maxLight, value);
}

half3 STLightColor(half3 albedo, half3 normalWS, Light light)
{
    half distanceFade = saturate(light.distanceAttenuation);
    half normalDotLight = dot(normalWS, light.direction);
    half3 color = albedo * light.color * STToon(normalDotLight, light.shadowAttenuation) * distanceFade;
    if (_ShnIntense > 0.0 && _ShnRange > 0.0)
    {
        float distance = abs(normalDotLight - 1.0);
        float range = _ShnRange * 2.0;
        float shine = 1.0 - smoothstep(range * (1.0 - _ShnSmooth), range + 0.0001, distance);
        float visibility = _ShnOverlap > 0.5 ? 1.0 : light.shadowAttenuation;
        color = lerp(color, _ShnColor.rgb * light.color * distanceFade, _ShnIntense * shine * visibility);
    }
    return color;
}

half4 STFragment(STVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    half4 textureColor = STSampleColor(input.uv);
    half3 albedo = max(0.0, textureColor.rgb * _ColIntense + _ColBright);
    half3 normalWS = STNormal(input);
    InputData inputData = (InputData)0;
    inputData.positionWS = input.positionWS;
    inputData.normalWS = normalWS;
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(ST_TRANSPARENT)
    inputData.shadowCoord = ComputeScreenPos(TransformWorldToHClip(input.positionWS));
#else
    inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
#endif
    half4 shadowMask = unity_ProbesOcclusion;
    AmbientOcclusionFactor ao = CreateAmbientOcclusionFactor(inputData.normalizedScreenSpaceUV, 1.0);
    Light mainLight = GetMainLight(inputData, shadowMask, ao);
    half3 bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, normalWS);
    half3 color = albedo * max(_MinLight, bakedGI * _AmbientCol) * ao.indirectAmbientOcclusion;
    uint renderingLayers = GetMeshRenderingLayer();
    if (IsMatchingLightLayer(mainLight.layerMask, renderingLayers))
        color += STLightColor(albedo, normalWS, mainLight);

#if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
#if USE_CLUSTER_LIGHT_LOOP
    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, ao);
        if (IsMatchingLightLayer(light.layerMask, renderingLayers))
            color += STLightColor(albedo, normalWS, light);
    }
#endif
    uint lightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(lightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, ao);
        if (IsMatchingLightLayer(light.layerMask, renderingLayers))
            color += STLightColor(albedo, normalWS, light);
    LIGHT_LOOP_END
#endif

    color += SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb * _EmissionEnabled;
    color = MixFog(color, input.fogFactor);
#if defined(ST_TRANSPARENT)
    return half4(color, textureColor.a);
#else
    return half4(color, 1.0);
#endif
}

float3 _LightDirection;
float3 _LightPosition;

STVaryings STShadowVertex(STAttributes input)
{
    STVaryings output = STVertex(input);
#if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
    float3 lightDirection = normalize(_LightPosition - output.positionWS);
#else
    float3 lightDirection = _LightDirection;
#endif
    output.positionCS = TransformWorldToHClip(ApplyShadowBias(output.positionWS, output.normalWS, lightDirection));
    output.positionCS = ApplyShadowClamping(output.positionCS);
    return output;
}

half4 STDepthFragment(STVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    STSampleColor(input.uv);
    return 0;
}

half4 STDepthNormalsFragment(STVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    STSampleColor(input.uv);
    half3 normalWS = STNormal(input);
#if defined(_GBUFFER_NORMALS_OCT)
    float2 octNormal = PackNormalOctQuadEncode(normalWS);
    return half4(PackFloat2To888(saturate(octNormal * 0.5 + 0.5)), 0);
#else
    return half4(normalWS, 0);
#endif
}

STVaryings STOutlineVertex(STAttributes input)
{
    STVaryings output = STVertex(input);
    float2 normalCS = mul((float3x3)UNITY_MATRIX_VP, output.normalWS).xy;
    float2 pixelDirection = normalCS * _ScaledScreenParams.xy;
    pixelDirection *= rsqrt(max(dot(pixelDirection, pixelDirection), 0.000001));
    output.positionCS.xy += pixelDirection * (2.0 * _OtlWidth / _ScaledScreenParams.xy) * output.positionCS.w;
    return output;
}

half4 STOutlineFragment(STVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    clip(_OtlWidth - 0.0001);
    STSampleColor(input.uv);
    return half4(MixFog(_OtlColor.rgb, input.fogFactor), _OtlColor.a);
}

#endif

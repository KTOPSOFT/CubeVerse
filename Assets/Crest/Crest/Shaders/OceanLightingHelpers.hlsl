// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

#ifndef CREST_OCEAN_LIGHTING_HELPERS_H
#define CREST_OCEAN_LIGHTING_HELPERS_H

#if UNITY_VERSION < 202230
#define GetMeshRenderingLayer GetMeshRenderingLightLayer
#endif

#ifndef LIGHT_LOOP_BEGIN
#define LIGHT_LOOP_BEGIN(x) for (uint lightIndex = 0; lightIndex < x; ++lightIndex) {
#endif

#ifndef LIGHT_LOOP_END
#define LIGHT_LOOP_END }
#endif

#if UNITY_VERSION >= 202130
#define LIGHT_LAYERS 1
#endif

namespace WaveHarmonic
{
	namespace Crest
	{
#if defined(LIGHTING_INCLUDED)
		float3 WorldSpaceLightDir(float3 worldPos)
		{
			float3 lightDir = _WorldSpaceLightPos0.xyz;
			if (_WorldSpaceLightPos0.w > 0.)
			{
				// non-directional light - this is a position, not a direction
				lightDir = normalize(lightDir - worldPos.xyz);
			}
			return lightDir;
		}
#endif

		half3 AmbientLight()
		{
			return half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
		}

		half3 AdditionalSoftLighting(const float3 i_PositionWS, const float4 i_ScreenPosition)
		{
			half3 color = 0.0;

#if CREST_URP
#if defined(_ADDITIONAL_LIGHTS)

			// Shadowmask.
			half4 shadowMask = unity_ProbesOcclusion;

			uint pixelLightCount = GetAdditionalLightsCount();

#if LIGHT_LAYERS
#if _LIGHT_LAYERS
			uint meshRenderingLayers = GetMeshRenderingLayer();
#endif
#endif

#if USE_FORWARD_PLUS
			InputData inputData = (InputData)0;
			// For Foward+ LIGHT_LOOP_BEGIN macro uses inputData.normalizedScreenSpaceUV and inputData.positionWS.
			inputData.normalizedScreenSpaceUV = i_ScreenPosition.xy / i_ScreenPosition.w;
			inputData.positionWS = i_PositionWS;
#endif

			LIGHT_LOOP_BEGIN(pixelLightCount)
				// Includes shadows and cookies.
				Light light = GetAdditionalLight(lightIndex, i_PositionWS, shadowMask);
#if LIGHT_LAYERS
#if _LIGHT_LAYERS
				if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
#endif
				{
					color += light.color * (light.distanceAttenuation * light.shadowAttenuation);
				}
			LIGHT_LOOP_END
#endif // _ADDITIONAL_LIGHTS
#endif // CREST_URP

			return color;
		}

		half3 AdditionalHardLighting(const float3 i_PositionWS, const float4 i_ScreenPosition, const half3 i_n_pixel, const half3 i_view, const BRDFData brdf)
		{
			half3 color = 0.0;

#if CREST_URP
#if defined(_ADDITIONAL_LIGHTS)

			// Shadowmask.
			half4 shadowMask = unity_ProbesOcclusion;

			uint pixelLightCount = GetAdditionalLightsCount();

#if LIGHT_LAYERS
#if _LIGHT_LAYERS
			uint meshRenderingLayers = GetMeshRenderingLayer();
#endif
#endif

#if USE_FORWARD_PLUS
			InputData inputData = (InputData)0;
			// For Foward+ LIGHT_LOOP_BEGIN macro uses inputData.normalizedScreenSpaceUV and inputData.positionWS.
			inputData.normalizedScreenSpaceUV = i_ScreenPosition.xy / i_ScreenPosition.w;
			inputData.positionWS = i_PositionWS;
#endif

			LIGHT_LOOP_BEGIN(pixelLightCount)
				// Includes shadows and cookies.
				Light light = GetAdditionalLight(lightIndex, i_PositionWS, shadowMask);
#if LIGHT_LAYERS
#if _LIGHT_LAYERS
				if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
#endif
				{
					color += LightingPhysicallyBased(brdf, light, i_n_pixel, i_view);
				}
			LIGHT_LOOP_END
#endif // _ADDITIONAL_LIGHTS
#endif // CREST_URP

			return color;
		}
	}
}


#endif // CREST_OCEAN_LIGHTING_HELPERS_H

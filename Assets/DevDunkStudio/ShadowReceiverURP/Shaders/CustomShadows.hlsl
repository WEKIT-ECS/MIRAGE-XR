#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

/*MIT License
Copyright(c) 2020 Cyanilux
@Cyanilux | https://github.com/Cyanilux/URP_ShaderGraphCustomLighting

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions :

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

//------------------------------------------------------------------------------------------------------
// Main Light Shadows
//------------------------------------------------------------------------------------------------------

/*
- This undef (un-define) is required to prevent the "invalid subscript 'shadowCoord'" error,
  which occurs when _MAIN_LIGHT_SHADOWS is used with 1/No Shadow Cascades with the Unlit Graph.
- It's technically not required for the PBR/Lit graph, so I'm using the SHADERPASS_FORWARD to ignore it for the pass.
  (But it would probably still remove the interpolator for other passes in the PBR/Lit graph and use a per-pixel version)
*/
#ifndef SHADERGRAPH_PREVIEW
	#if VERSION_GREATER_EQUAL(9, 0)
		#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
		#if (SHADERPASS != SHADERPASS_FORWARD)
			#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
		#endif
	#else
		#ifndef SHADERPASS_FORWARD
			#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
		#endif
	#endif

	#if UNITY_VERSION < 202220
	/*
	GetMeshRenderingLayer() is only available in 2022.2+
	Previous versions need to use GetMeshRenderingLightLayer()
	*/
	uint GetMeshRenderingLayer(){
		return GetMeshRenderingLightLayer();
	}
	#endif

half GetMainShadow(float3 WorldPos){
	#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
		float4 shadowCoord = ComputeScreenPos(TransformWorldToHClip(WorldPos));
	#else
		float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
	#endif
	return MainLightShadow(shadowCoord, WorldPos, half4(1,1,1,1), _MainLightOcclusionProbes);
}

half GetMainShadow(half3 WorldPos){
	#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
		half4 shadowCoord = ComputeScreenPos(TransformWorldToHClip(WorldPos));
	#else
		half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
	#endif
	return MainLightShadow(shadowCoord, WorldPos, half4(1,1,1,1), _MainLightOcclusionProbes);
}

float GetAdditionalShadow(float3 WorldPosition, float3 Normal, half alpha){
	float ShadowAtten = 0;
	half one = half(1);
	half4 shadowMask = half4(one,one,one,one);

	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, shadowMask);
	#ifdef _LIGHT_LAYERS
	if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * length(light.color) * alpha;
		}
	LIGHT_LOOP_END

	return ShadowAtten;
}

half GetAdditionalShadow(half3 WorldPosition, half3 Normal, half alpha){
	half ShadowAtten = half(0);
	half one = half(1);
	half4 shadowMask = half4(one,one,one,one);

	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	half4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, shadowMask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * length(light.color) * alpha;
		}
	LIGHT_LOOP_END

	return ShadowAtten;
}
#endif

//Samples the Shadowmap for the Main Light, based on the World Position passed in. (Position node)
void MainLightShadows_float (float3 WorldPos, out half ShadowAtten){
	#ifdef SHADERGRAPH_PREVIEW
		ShadowAtten = 1;
	#else
		ShadowAtten = GetMainShadow(WorldPos);
	#endif
}

//Samples the Shadowmap for the Main Light, based on the World Position passed in. (Position node)
void MainLightShadows_half (half3 WorldPos, out half ShadowAtten){
	#ifdef SHADERGRAPH_PREVIEW
		ShadowAtten = 1;
	#else
		ShadowAtten = GetMainShadow(WorldPos);
	#endif
}

void AdditionalShadows_float (float3 WorldPosition, float3 Normal, half alpha, out float ShadowAtten){
	#ifdef SHADERGRAPH_PREVIEW
		ShadowAtten = 1;
	#else
		ShadowAtten = GetAdditionalShadow(WorldPosition, Normal, alpha);
	#endif
}

void AdditionalShadows_half (half3 WorldPosition, half3 Normal, half alpha, out half ShadowAtten){
	#ifdef SHADERGRAPH_PREVIEW
		ShadowAtten = 1;
	#else
		ShadowAtten = GetAdditionalShadow(WorldPosition, Normal, alpha);
	#endif
}
#endif // CUSTOM_LIGHTING_INCLUDED
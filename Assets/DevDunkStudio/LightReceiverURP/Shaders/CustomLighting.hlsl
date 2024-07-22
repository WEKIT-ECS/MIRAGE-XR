#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

// @Cyanilux | https://github.com/Cyanilux/URP_ShaderGraphCustomLighting
// Note this version of the package assumes v12+ due to usage of "Branch on Input Connection" node
// For older versions, see branches on github repo!

#ifndef SHADERGRAPH_PREVIEW
	#if UNITY_VERSION < 202220
	/*
	GetMeshRenderingLayer() is only available in 2022.2+
	Previous versions need to use GetMeshRenderingLightLayer()
	*/
	uint GetMeshRenderingLayer(){
		return GetMeshRenderingLightLayer();
	}
	#endif

	half3 LightingSpecularFixed(half3 lightColor, half3 lightDir, half3 normal, float3 viewDir, half4 specular, float smoothness)
	{
		float3 halfVec = normalize(lightDir + viewDir);
		half NdotH = half(saturate(dot(normal, halfVec)));
		half modifier = pow(NdotH, smoothness); // Half produces banding, need full precision
		half3 specularReflection = specular.rgb * modifier;
		return lightColor * specularReflection;
	}

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
#endif

/*
- Handles additional lights (e.g. additional directional, point, spotlights)
- For custom lighting, you may want to duplicate this and swap the LightingLambert / LightingSpecularFixed functions out. See Toon Example below!
- To work in the Unlit Graph, the following keywords must be defined in the blackboard :
	- Boolean Keyword, Global Multi-Compile "_ADDITIONAL_LIGHT_SHADOWS"
	- Boolean Keyword, Global Multi-Compile "_ADDITIONAL_LIGHTS"
- To support Forward+ path
	- Boolean Keyword, Global Multi-Compile "_FORWARD_PLUS" (2022.2+)
*/
void AdditionalLights_float(float3 SpecColor, float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView, bool UseMainLight,
							out float3 Diffuse, out float3 Specular) {
	float3 diffuseColor = 0;
	float3 specularColor = 0;
	half4 ShadowMask = half4(1,1,1,1);
#ifndef SHADERGRAPH_PREVIEW
	float NewSmoothness = exp2(10 * Smoothness + 1);
	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	#if USE_FORWARD_PLUS
		for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) {
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
		#ifdef _LIGHT_LAYERS
			if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		#endif
			{
			//PBR Estimate
				float3 attenuatedLightColor = light.color * (light.distanceAttenuation); 
				diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal); 
				specularColor += LightingSpecularFixed(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), NewSmoothness);
			}
		}
	#endif

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	if(UseMainLight){
		Light mainLight = GetMainLight();
		// Blinn-Phong
		float3 attenuatedLightColor = mainLight.color;
		specularColor += LightingSpecularFixed(attenuatedLightColor, mainLight.direction, WorldNormal, WorldView, float4(SpecColor, 0), NewSmoothness);
		specularColor *= Smoothness;
	}

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			//PBR Estimate
			float3 attenuatedLightColor = light.color * (light.distanceAttenuation);
			diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
			specularColor += LightingSpecularFixed(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), NewSmoothness);
		}
	LIGHT_LOOP_END
#endif

	Diffuse = diffuseColor;
	Specular = specularColor;
}

void AdditionalLights_half(half3 SpecColor, half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView, bool UseMainLight,
							out half3 Diffuse, out half3 Specular) {
	half3 diffuseColor = half3(0,0,0);
	half3 specularColor = half3(0,0,0);
	half4 ShadowMask = half4(1,1,1,1);
#ifndef SHADERGRAPH_PREVIEW
	half NewSmoothness = exp2(half(10) * Smoothness + half(1));
	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	#if USE_FORWARD_PLUS
		for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) {
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
		#ifdef _LIGHT_LAYERS
			if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		#endif
			{
				// BRP estimate
				half3 attenuatedLightColor = light.color * (light.distanceAttenuation); 
				diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal); 
				specularColor += LightingSpecularFixed(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), NewSmoothness);
			}
		}
	#endif

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	half4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	if(UseMainLight){
		Light mainLight = GetMainLight();
		half3 attenuatedLightColor = mainLight.color;
		specularColor += LightingSpecularFixed(attenuatedLightColor, mainLight.direction, WorldNormal, WorldView, half4(SpecColor, 0), NewSmoothness);
		specularColor *= Smoothness;
	}
	

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			// BRP estimate
			half3 attenuatedLightColor = light.color.rgb * (light.distanceAttenuation); 
			diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
			specularColor += LightingSpecularFixed(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), NewSmoothness);
		}
	LIGHT_LOOP_END
#endif

	Diffuse = diffuseColor;
	Specular = specularColor;
}

void AdditionalLightsNoSpecular_float(float3 WorldPosition, float3 WorldNormal,
							out float3 Diffuse) {
	float3 diffuseColor = 0;
	float3 specularColor = 0;
	half4 ShadowMask = half4(1,1,1,1);
#ifndef SHADERGRAPH_PREVIEW
	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	#if USE_FORWARD_PLUS
		for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) {
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
		#ifdef _LIGHT_LAYERS
			if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		#endif
			{
			//PBR Estimate
				half3 attenuatedLightColor = light.color * (light.distanceAttenuation); 
				diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
			}
		}
	#endif

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			//PBR Estimate
			half3 attenuatedLightColor = light.color * (light.distanceAttenuation); 
			diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
		}
	LIGHT_LOOP_END
#endif

	Diffuse = diffuseColor;
}

void AdditionalLightsNoSpecular_half(half3 WorldPosition, half3 WorldNormal,
							out half3 Diffuse) {
	half3 diffuseColor = half3(0,0,0);
	half3 specularColor = half3(0,0,0);
	half4 ShadowMask = half4(1,1,1,1);
#ifndef SHADERGRAPH_PREVIEW
	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	#if USE_FORWARD_PLUS
		for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) {
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
		#ifdef _LIGHT_LAYERS
			if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		#endif
		{
			// BRP estimate
			half3 attenuatedLightColor = light.color * (light.distanceAttenuation); 
			diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal); 
		}
	}
	#endif

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	half4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			// BRP estimate
			half3 attenuatedLightColor = light.color * (light.distanceAttenuation); 
			diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal); 
			
		}
	LIGHT_LOOP_END
#endif

	Diffuse = diffuseColor;
}

void AdditionalLightsAndShadow_float(float3 SpecColor, float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView, half alpha, bool UseMainLight,
							out float3 Diffuse, out float3 Specular, out float ShadowAtten, out float mainLightShadowAtten) {
	float3 diffuseColor = 0;
	float3 specularColor = 0;
	half4 ShadowMask = half4(1,1,1,1);
	half one = half(1);
	mainLightShadowAtten = 0;
	ShadowAtten = 0;
#ifndef SHADERGRAPH_PREVIEW
	float NewSmoothness = exp2(10 * Smoothness + 1);
	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	#if USE_FORWARD_PLUS
		for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) {
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
		#ifdef _LIGHT_LAYERS
			if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		#endif
			{
			//PBR Estimate
			float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
				diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
				specularColor += LightingSpecularFixed(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), NewSmoothness);
				ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * length(light.color);
			}
		}
	#endif

	if(UseMainLight){
		Light mainLight = GetMainLight();
		// Blinn-Phong
		float3 attenuatedLightColor = mainLight.color;
		specularColor += LightingSpecularFixed(attenuatedLightColor, mainLight.direction, WorldNormal, WorldView, float4(SpecColor, 0), NewSmoothness);
		specularColor *= Smoothness;

		mainLightShadowAtten = GetMainShadow(WorldPosition);
	}
	else{
		mainLightShadowAtten = 1;
	}

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			//PBR Estimate
			float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
			diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
			specularColor += LightingSpecularFixed(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), NewSmoothness);
			ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * length(light.color) * alpha;
	
		}
	LIGHT_LOOP_END
#endif
	Diffuse = diffuseColor;
	Specular = specularColor;
}

void AdditionalLightsAndShadow_half(half3 SpecColor, half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView, half alpha, bool UseMainLight,
							out half3 Diffuse, out half3 Specular, out half ShadowAtten, out half mainLightShadowAtten) {
	half3 diffuseColor = 0;
	half3 specularColor = 0;
	half one = half(1);
	half4 ShadowMask = half4(1,1,1,1);
	mainLightShadowAtten = 0;
	ShadowAtten = 0;
#ifndef SHADERGRAPH_PREVIEW
	half NewSmoothness = exp2(10 * Smoothness + 1);
	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	#if USE_FORWARD_PLUS
		for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) {
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
		#ifdef _LIGHT_LAYERS
			if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		#endif
			{
			//PBR Estimate
				float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
				diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
				specularColor += LightingSpecularFixed(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), NewSmoothness);
				ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * length(light.color) * alpha;
			}
		}
	#endif

	if(UseMainLight){
		Light mainLight = GetMainLight();
		// Blinn-Phong
		float3 attenuatedLightColor = mainLight.color;
		specularColor += LightingSpecularFixed(attenuatedLightColor, mainLight.direction, WorldNormal, WorldView, float4(SpecColor, 0), NewSmoothness);
		specularColor *= Smoothness;

		mainLightShadowAtten = GetMainShadow(WorldPosition);
	}
	else{
		mainLightShadowAtten = 1;
	}

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	half4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			//PBR Estimate
			float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
			diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
			specularColor += LightingSpecularFixed(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), NewSmoothness);
			ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * length(light.color) * alpha;
		}
	LIGHT_LOOP_END
#endif

	Diffuse = diffuseColor;
	Specular = specularColor;
}

void AdditionalLightsAndShadowNoSpecular_float(float3 WorldPosition, float3 WorldNormal, half alpha, bool UseMainLight,
							out float3 Diffuse, out float ShadowAtten, out float mainLightShadowAtten) {
	float3 diffuseColor = 0;
	half one = half(1);
	mainLightShadowAtten = 0;
	half4 ShadowMask = half4(1,1,1,1);
	ShadowAtten = 0;
#ifndef SHADERGRAPH_PREVIEW
	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	#if USE_FORWARD_PLUS
		for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) {
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
		#ifdef _LIGHT_LAYERS
			if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		#endif
			{
			//PBR Estimate
				float3 attenuatedLightColor = light.color.rgb * (light.distanceAttenuation); 
				diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
				ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * light.color * alpha;
			}
		}
	#endif

	if(UseMainLight){
		mainLightShadowAtten = GetMainShadow(WorldPosition);
	}
	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			//PBR Estimate
			float3 attenuatedLightColor = light.color.rgb * (light.distanceAttenuation);
			diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
			ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * light.color * alpha;
		}
	LIGHT_LOOP_END
#endif

	Diffuse = diffuseColor;
}

void AdditionalLightsAndShadowNoSpecular_half(half3 WorldPosition, half3 WorldNormal, half alpha, bool UseMainLight,
							out half3 Diffuse, out half ShadowAtten, out half mainLightShadowAtten) {
	half3 diffuseColor = 0;
	half one = half(1);
	half4 ShadowMask = half4(1,1,1,1);
	mainLightShadowAtten = 0;
	ShadowAtten = 0;
#ifndef SHADERGRAPH_PREVIEW
	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	#if USE_FORWARD_PLUS
		for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) {
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
		#ifdef _LIGHT_LAYERS
			if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		#endif
			{
			//PBR Estimate
				half3 attenuatedLightColor = light.color.rgb * (light.distanceAttenuation); 
				diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
				ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * light.color * alpha;
			}
		}
	#endif

	if(UseMainLight){
		mainLightShadowAtten = GetMainShadow(WorldPosition);
	}

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	half4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			//PBR Estimate
			half3 attenuatedLightColor = light.color.rgb * (light.distanceAttenuation);
			diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
			ShadowAtten += (one-light.shadowAttenuation) * light.distanceAttenuation * light.color * alpha;
		}
	LIGHT_LOOP_END
#endif

	Diffuse = diffuseColor;
}
#endif
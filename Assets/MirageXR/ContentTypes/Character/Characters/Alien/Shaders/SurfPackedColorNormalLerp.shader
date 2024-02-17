Shader "Sylphelabs/SurfPackedColorNormalLerp"
{
	Properties
	{
		_BaseColor("BaseColor", Color) = (0,0,0,0)
		_Metal("Metal", Range(0 , 1)) = 0
		_Smooth("Smooth", Range(0 , 1)) = 0
		_ColorABCD("ColorABCD", 2D) = "white" {}
		_ColorEFGH("ColorEFGH", 2D) = "white" {}

		_NormalAB("NormalAB", 2D) = "white" {}
		_NormalCD("NormalCD", 2D) = "white" {}
		_NormalEF("NormalEF", 2D) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
			#pragma target 3.0
			#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		
			uniform half _Metal, _Smooth;
			uniform sampler2D _ColorABCD, _ColorEFGH, _NormalAB, _NormalCD, _NormalEF;
			uniform half4 _BaseColor;
			uniform half _blendValues[4];
			
			struct Input { float2 uv_ColorABCD; };

			void surf( Input i , inout SurfaceOutputStandard o )
			{
				half4 greyAlbedoABCD	= tex2D(_ColorABCD, i.uv_ColorABCD);								//.r = A, .g = B, .b = C, .a = D
				half4 greyAlbedoEFGH	= tex2D(_ColorEFGH, i.uv_ColorABCD);								//.r = E, .g = F, .b = G, .a = H
				half4 normalAB			= tex2D(_NormalAB, i.uv_ColorABCD);									//.rg = A, .ba = B
				half4 normalCD			= tex2D(_NormalCD, i.uv_ColorABCD);									//.rg = C, .ba = D
				half4 normalEF			= tex2D(_NormalEF, i.uv_ColorABCD);									//.rg = E, .ba = F

				half lerpGreyAlbedoAB		= lerp(greyAlbedoABCD.r, greyAlbedoABCD.g, _blendValues[0]);	//Base (R) -> Angry (G)
				half lerpGreyAlbedoAB_C		= lerp(lerpGreyAlbedoAB, greyAlbedoABCD.b, _blendValues[1]);	//-> Happy (B)
				half lerpGreyAlbedoABC_D	= lerp(lerpGreyAlbedoAB_C, greyAlbedoABCD.a, _blendValues[2]);	//-> Sad (A)
				half lerpGreyAlbedoABCD_E	= lerp(lerpGreyAlbedoABC_D, greyAlbedoEFGH.r, _blendValues[3]);	//-> Surprise

				half2 lerpNormalAB		= lerp(normalAB.rg, normalAB.ba, _blendValues[0]);					//Base (RG) -> Angry (BA)
				half2 lerpNormalAB_C	= lerp(lerpNormalAB, normalCD.rg, _blendValues[1]);					//-> Happy(RG)
				half2 lerpNormalABC_D	= lerp(lerpNormalAB_C, normalCD.ba, _blendValues[2]);				//-> Sad (BA)
				half2 lerpNormalABCD_E	= lerp(lerpNormalABC_D, normalEF.rg, _blendValues[3]);				//-> Surprise (RG)
			
				o.Albedo = lerpGreyAlbedoABCD_E * _BaseColor.rgb;
				o.Normal.xy = lerpNormalABCD_E * 2 - 1; o.Normal.z = 1.0 - dot(o.Normal.xy, o.Normal.xy);
				o.Metallic = _Metal;
				o.Smoothness = _Smooth;
				o.Alpha = 1;
			}
		ENDCG
	}
}
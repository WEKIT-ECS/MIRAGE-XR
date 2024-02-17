﻿Shader "Obi/URP/Particles" {

Properties {
	_Color 		("Particle color", Color) = (1,1,1,1)
	_RadiusScale("Radius scale",float) = 1
}

	SubShader { 

        Tags{"RenderPipeline" = "UniversalRenderPipeline"}

		Pass { 

			Name "ParticleFwdBase"
			Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" "LightMode" = "UniversalForward"}
			Blend SrcAlpha OneMinusSrcAlpha  
			
			HLSLPROGRAM
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "../ObiUtils.cginc"
            #include "ObiLightingURP.cginc"
			#include "../ObiEllipsoids.cginc"

			float4 _Color;
			float4 _LightColor0; 

			struct vin{
				float4 vertex   : POSITION;
				float3 corner   : NORMAL;
				float4 color    : COLOR;
				float4 t0 : TEXCOORD0; // ellipsoid t1 vector
			    float4 t1 : TEXCOORD1; // ellipsoid t2 vector
			    float4 t2 : TEXCOORD2; // ellipsoid t3 vector
			};
			
			struct v2f
			{
				float4 pos   : SV_POSITION;
				float4 color    : COLOR;
				float4 mapping  : TEXCOORD0;
				float3 viewRay : TEXCOORD1;
				float3 lightDir : TEXCOORD2;
				float3 a2 : TEXCOORD3;
				float3 a3 : TEXCOORD4;
			};

            struct fout 
            {
                half4 color : SV_Target;
                float depth : SV_Depth;
            };

			v2f vert(vin v)
			{ 
				float3x3 P, IP;
				BuildParameterSpaceMatrices(v.t0,v.t1,v.t2,P,IP);
			
				float3 worldPos;
				float3 view;
				float3 eye;
				float radius = BuildEllipsoidBillboard(v.vertex,v.corner,P,IP,worldPos,view,eye);
			
				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, float4(worldPos,v.vertex.w));
				o.mapping = float4(v.corner.xy,1/length(eye),radius); // A[1]
				o.viewRay = mul((float3x3)UNITY_MATRIX_V,view); 	  // A[0]
				o.color = v.color * _Color;
			
				BuildAuxiliaryNormalVectors(v.vertex,worldPos,view,P,IP,o.a2,o.a3);
						
				o.lightDir = mul((float3x3)UNITY_MATRIX_MV, ObjSpaceLightDir(v.vertex));

				return o;
			}

			fout frag(v2f i) 
			{
				fout fo;

				fo.color =  half4(0,0,0,i.color.a); 

				// generate sphere normals:
				float3 p,n;
				IntersectEllipsoid(i.viewRay,i.mapping, i.a2,i.a3, p, n);

				// clip space position:
				float4 pos = mul(UNITY_MATRIX_P,float4(p,1.0));

				// simple lighting: ambient
				half3 amb = SampleSphereAmbient(n);

				// simple lighting: diffuse
		   	 	float ndotl = saturate( dot( n, normalize(i.lightDir) ) );
				float atten = Attenuation(p);

				// final lit color:
				fo.color.rgb = i.color * (_LightColor0 * ndotl * atten + amb);

				// normalized device coordinates:
				fo.depth = pos.z/pos.w;

				// in openGL calculated depth range is <-1,1> map it to <0,1>
				#if SHADER_API_OPENGL || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3	 
					fo.depth = 0.5 * fo.depth + 0.5;
				#endif

				return fo;
			}
			 
			ENDHLSL

		} 

	    Pass {

    	    Name "ShadowCaster"
	        Tags { "LightMode" = "ShadowCaster" }
	        Offset 1, 1
	       
	        Fog {Mode Off}
	        ZWrite On ZTest LEqual
	 
			HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

			#pragma vertex ellipsoidShadowVS
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma multi_compile_shadowcaster

            #include "../ObiUtils.cginc"
            #include "ObiLightingURP.cginc"
	        #include "../ObiEllipsoids.cginc"

			sampler3D  _DitherMaskLOD;
			float4 _Color;

			struct vin{
				float4 vertex   : POSITION;
				float3 corner   : NORMAL;
				float4 color    : COLOR;
				float4 t0 : TEXCOORD0; // ellipsoid t1 vector
			    float4 t1 : TEXCOORD1; // ellipsoid t2 vector
			    float4 t2 : TEXCOORD2; // ellipsoid t3 vector
			};
			
			struct v2f {
				float4 color    : COLOR;
				float4 mapping  : TEXCOORD0;
				float3 viewRay : TEXCOORD1;
                float3 lightDir : TEXCOORD2;
                float3 a2 : TEXCOORD3;
                float3 a3 : TEXCOORD4;
			};

            struct fout 
            {
                half4 color : SV_Target;
                float depth : SV_Depth;
            };

			v2f ellipsoidShadowVS( vin v , out float4 outpos : SV_POSITION )// clip space position output
			{
				float3x3 P, IP;
				BuildParameterSpaceMatrices(v.t0,v.t1,v.t2,P,IP);
			
				float3 worldPos;
				float3 view;
				float3 eye;
				float radius = BuildEllipsoidBillboard(v.vertex,v.corner,P,IP,worldPos,view,eye);
			
				v2f o;
				outpos = mul(UNITY_MATRIX_VP, float4(worldPos,v.vertex.w));
				o.mapping = float4(v.corner.xy,1/length(eye),radius); // A[1]
				o.viewRay = mul((float3x3)UNITY_MATRIX_V,view); 	  // A[0]
				o.color = v.color * _Color;

                BuildAuxiliaryNormalVectors(v.vertex,worldPos,view,P,IP,o.a2,o.a3);

                o.lightDir = WorldSpaceLightDir(v.vertex);
				return o;
			}
         			 
			fout frag( v2f i ) 
			{
				fout fo;

				float3 p,n;
				IntersectEllipsoid(i.viewRay,i.mapping,i.a2,i.a3,p, n);

				// calculate world space position and normal:
                float4 wnormal = mul(UNITY_MATRIX_I_V,float4(n,0));
                float4 wpos = mul(UNITY_MATRIX_I_V,float4(p,1));

                // calculate clip space position.
                float4 clipPos = TransformWorldToHClip(ApplyShadowBias(wpos, wnormal, normalize(i.lightDir)));

                #if UNITY_REVERSED_Z
                    clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                fo.color = clipPos.z/clipPos.w; //similar to what SHADOW_CASTER_FRAGMENT does in case there's no depth buffer.
                fo.depth = clipPos.z/clipPos.w; 

				// in openGL calculated depth range is <-1,1> map it to <0,1>
				#if SHADER_API_OPENGL || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3
					fo.depth = fo.depth*0.5+0.5;
				#endif
       
				return fo;
			}
			ENDHLSL
		 
	    }
        
	} 
}


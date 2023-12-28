Shader "Obi/EditorParticles" {

    Properties {
    	_Color 		("Particle color", Color) = (1,1,1,1)
    	_RadiusScale("Radius scale",float) = 1
    }

	SubShader { 

		Pass { 

                Name "EditorParticle"
                Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque"}
                Blend SrcAlpha OneMinusSrcAlpha  

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest

                #include "../../Resources/ObiMaterials/ObiEllipsoids.cginc"
                #include "../../Resources/ObiMaterials/ObiUtils.cginc"

                fixed4 _Color;

                struct vin{
                	float4 vertex   : POSITION;
                	float3 corner   : NORMAL;
                	fixed4 color    : COLOR;

                	float4 t0 : TEXCOORD0; // ellipsoid t1 vector
                    float4 t1 : TEXCOORD1; // ellipsoid t2 vector
                    float4 t2 : TEXCOORD2; // ellipsoid t3 vector
                };

                struct v2f
                {
                	float4 pos   : SV_POSITION;
                	fixed4 color    : COLOR;
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
                			
                	o.lightDir = mul((float3x3)UNITY_MATRIX_MV, float3(0.5f,0.5f,0.5f));

                	return o;
                }

                fout frag(v2f i) 
                {
                    fout fo;

                    fo.color =  half4(0,0,0,i.color.a); 

                    // generate sphere normals:
                    float3 p,n;
                    IntersectEllipsoid(i.viewRay,i.mapping, i.a2,i.a3,p, n);

                    // clip space position:
                    float4 pos = mul(UNITY_MATRIX_P,float4(p,1.0));

                    // simple lighting: ambient
                    float3 modelUp = mul (UNITY_MATRIX_IT_MV,float3(0,1,0));
                    float vecHemi = dot(n, modelUp) * 0.5f + 0.5f;
                    half3 amb = 0.6f * lerp(float3(0.5f,0.48f,0.45f), float3(0.8f,0.9f,1), vecHemi);

                    // simple lighting: diffuse
                    float ndotl = saturate( dot( n, normalize(i.lightDir) ) );

                    // final lit color:
                    fo.color.rgb = i.color * (0.5f * ndotl + amb);

                    // normalized device coordinates:
                    fo.depth = pos.z/pos.w;

                    // in openGL calculated depth range is <-1,1> map it to <0,1>
                    #if SHADER_API_OPENGL || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3	 
                    fo.depth = 0.5*fo.depth + 0.5;
                    #endif

                    return fo;
                }

                ENDCG

		} 

	} 
FallBack "Diffuse"
}


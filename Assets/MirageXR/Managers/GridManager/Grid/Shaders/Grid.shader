Shader "Mirage/Grid" {
    Properties {
        [MainTexture] _BaseMap ("BaseMap", 2D) = "white" {}
        [MainColor] _BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _SecondTex ("SecondTexture", 2D) = "white" {}
        _BorderTex ("BorderTexture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainThreshold ("MainThreshold", Range(0, 100)) = 10.0
        _SecondThreshold ("SecondThreshold", Range(0, 100)) = 10.0
        _Position ("Position", Vector) = (0, 0, 0, 0)

        // URP & PolySpatial compatibility properties
        [HideInInspector] _Surface ("__surface", Float) = 1.0
        [HideInInspector] _Blend ("__blend", Float) = 0.0
        [HideInInspector] _Cull ("__cull", Float) = 0.0
        [HideInInspector] [ToggleUI] _AlphaClip ("__clip", Float) = 0.0
        [HideInInspector] _QueueOffset ("Queue offset", Float) = 0.0

        // PolySpatial SimpleLit/Specular Fallback Properties
        [HideInInspector] [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
        [HideInInspector] _SpecColor ("Specular Color", Color) = (0, 0, 0, 0)
        [HideInInspector] _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _SpecularHighlights ("Specular Highlights", Float) = 0.0
    }

    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" "UniversalMaterialType" = "Unlit" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 second_uv : TEXCOORD1;
                float2 border_uv : TEXCOORD3;
                float3 world_position : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BorderTex);
            SAMPLER(sampler_BorderTex);
            TEXTURE2D(_SecondTex);
            SAMPLER(sampler_SecondTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BorderTex_ST;
                float4 _SecondTex_ST;
                float4 _Color;
                float3 _Position;
                float _MainThreshold;
                float _SecondThreshold;
            CBUFFER_END

            Varyings vert(Attributes input) {
                Varyings output = (Varyings)0;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.world_position = vertexInput.positionWS;
                output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                output.border_uv = input.uv * _BorderTex_ST.xy + _BorderTex_ST.zw;
                output.second_uv = input.uv * _SecondTex_ST.xy + _SecondTex_ST.zw;
                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                half4 col = half4(0, 0, 0, 0);
                float dis = distance(_Position, input.world_position);
                if (dis < _MainThreshold) {
                    half4 mainCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                    half4 borderCol = SAMPLE_TEXTURE2D(_BorderTex, sampler_BorderTex, input.border_uv);
                    col = saturate(mainCol + borderCol);
                    col.rgb *= _Color.rgb;
                    col.a *= _Color.a;
                }
                else if (dis < _SecondThreshold + _MainThreshold) {
                    col = SAMPLE_TEXTURE2D(_SecondTex, sampler_SecondTex, input.second_uv);
                    col.rgb *= _Color.rgb;
                    col.a *= _Color.a;
                }
                else {
                    col = half4(0, 0, 0, 0);
                }
                return col;
            }
            ENDHLSL
        }
    }

    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float2 second_uv : TEXCOORD1;
                float2 border_uv : TEXCOORD3;
                float4 vertex : SV_POSITION;
                float3 world_position : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _BorderTex;
            sampler2D _SecondTex;
            float4 _Color;
            float4 _MainTex_ST;
            float4 _BorderTex_ST;
            float4 _SecondTex_ST;
            float _MainThreshold;
            float _SecondThreshold;
            float3 _Position;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.border_uv = v.uv.xy * _BorderTex_ST.xy + _BorderTex_ST.zw;
                o.second_uv = v.uv.xy * _SecondTex_ST.xy + _SecondTex_ST.zw;
                o.world_position = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (const v2f i) : SV_Target {
                fixed4 col = fixed4(0, 0, 0, 0);
                const float dis = distance(_Position, i.world_position);
                if (dis < _MainThreshold) {
                    col = tex2D(_MainTex, i.uv);
                    col += tex2D(_BorderTex, i.border_uv);
                    col = clamp(col, 0, 1);
                    col.rgb *= _Color.rgb;
                    col.a *= _Color.a;
                }
                else if (dis < _SecondThreshold + _MainThreshold) {
                    col = tex2D(_SecondTex, i.second_uv);
                    col.rgb *= _Color.rgb;
                    col.a *= _Color.a;
                }
                else {
                    col = fixed4(0, 0, 0, 0);
                }
                return col;
            }
            ENDCG
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
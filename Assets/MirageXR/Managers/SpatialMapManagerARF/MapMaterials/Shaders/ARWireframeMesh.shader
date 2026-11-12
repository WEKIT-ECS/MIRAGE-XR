Shader "MirageXR/ARWireframeMesh"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.157, 1.0, 0.0, 0.12)
        _WireColor ("Wire Color", Color) = (0.157, 1.0, 0.0, 0.85)
        _GridSpacing ("Grid Spacing", Float) = 0.2
        _LineWidth ("Line Width", Range(0.001, 0.05)) = 0.006

        // URP & PolySpatial compatibility properties
        [HideInInspector] _Surface ("__surface", Float) = 1.0
        [HideInInspector] _Blend ("__blend", Float) = 0.0
        [HideInInspector] _Cull ("__cull", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 0.0
        [HideInInspector] [ToggleUI] _AlphaClip ("__clip", Float) = 0.0
        [HideInInspector] _QueueOffset ("Queue offset", Float) = 0.0

        // PolySpatial SimpleLit/Specular Fallback Properties
        [HideInInspector] [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
        [HideInInspector] _SpecColor ("Specular Color", Color) = (0, 0, 0, 0)
        [HideInInspector] _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _SpecularHighlights ("Specular Highlights", Float) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent-5" 
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _WireColor;
                float _GridSpacing;
                float _LineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            float GridFactor(float2 uv, float halfWidth)
            {
                float2 d = max(fwidth(uv), 0.0001);
                float2 g = abs(frac(uv - 0.5) - 0.5);
                float2 a = smoothstep(halfWidth + d, halfWidth - d, g);
                return max(a.x, a.y);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float spacing = max(_GridSpacing, 0.01);
                float halfWidth = (_LineWidth / spacing) * 0.5;

                float3 weights = pow(abs(input.worldNormal), 4.0);
                weights /= max(weights.x + weights.y + weights.z, 0.0001);

                float gx = GridFactor(input.worldPos.yz / spacing, halfWidth);
                float gy = GridFactor(input.worldPos.xz / spacing, halfWidth);
                float gz = GridFactor(input.worldPos.xy / spacing, halfWidth);

                float lineIntensity = gx * weights.x + gy * weights.y + gz * weights.z;
                lineIntensity = saturate(lineIntensity);

                half4 col = lerp(_BaseColor, _WireColor, lineIntensity);
                return col;
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-5" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            fixed4 _BaseColor;
            fixed4 _WireColor;
            float _GridSpacing;
            float _LineWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float spacing = max(_GridSpacing, 0.01);
                float halfWidth = (_LineWidth / spacing) * 0.5;

                float3 weights = pow(abs(i.worldNormal), 4.0);
                weights /= max(weights.x + weights.y + weights.z, 0.0001);

                float2 uvX = i.worldPos.yz / spacing;
                float2 uvY = i.worldPos.xz / spacing;
                float2 uvZ = i.worldPos.xy / spacing;

                float2 dX = max(fwidth(uvX), 0.0001);
                float2 gX = abs(frac(uvX - 0.5) - 0.5);
                float2 aX = smoothstep(halfWidth + dX, halfWidth - dX, gX);
                float gx = max(aX.x, aX.y);

                float2 dY = max(fwidth(uvY), 0.0001);
                float2 gY = abs(frac(uvY - 0.5) - 0.5);
                float2 aY = smoothstep(halfWidth + dY, halfWidth - dY, gY);
                float gy = max(aY.x, aY.y);

                float2 dZ = max(fwidth(uvZ), 0.0001);
                float2 gZ = abs(frac(uvZ - 0.5) - 0.5);
                float2 aZ = smoothstep(halfWidth + dZ, halfWidth - dZ, gZ);
                float gz = max(aZ.x, aZ.y);

                float lineIntensity = saturate(gx * weights.x + gy * weights.y + gz * weights.z);
                return lerp(_BaseColor, _WireColor, lineIntensity);
            }
            ENDCG
        }
    }
}

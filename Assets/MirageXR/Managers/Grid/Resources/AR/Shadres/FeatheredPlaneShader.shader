Shader "Unlit/FeatheredPlaneShader"
{
    Properties
    {
        [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _TexTintColor("Texture Tint Color", Color) = (1,1,1,1)
        _PlaneColor("Plane Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 10
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Float) = 0
        _Surface ("_Surface", Float) = 1.0
        _Blend ("_Blend", Float) = 0.0
        _AlphaClip ("Alpha Clip", Float) = 0.0
        _QueueOffset ("_QueueOffset", Float) = 0.0
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _SpecColor ("Specular", Color) = (0.5, 0.5, 0.5, 1)
        _SpecularHighlights ("Specular Highlights", Float) = 0.0
        [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        Cull [_Cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 uv2 : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 uv2 : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _TexTintColor;
            fixed4 _BaseColor;
            fixed4 _PlaneColor;
            float _ShortestUVMapping;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = v.uv2;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                fixed4 col = tex2D(_MainTex, i.uv) * _TexTintColor * _BaseColor;
                col = lerp( _PlaneColor, col, col.a);
                // Fade out from as we pass the edge.
                // uv2.x stores a mapped UV that will be "1" at the beginning of the feathering.
                // We fade until we reach at the edge of the shortest UV mapping.
                // This is the remmaped UV value at the vertex.
                // We choose the shorted one so that ll edges will fade out completely.
                // See ARFeatheredPlaneMeshVisualizer.cs for more details.
                col.a *=  1-smoothstep(1, _ShortestUVMapping, i.uv2.x);
                return col;
            }
            ENDCG
        }
    }
}

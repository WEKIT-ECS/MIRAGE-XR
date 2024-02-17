Shader "Mirage/Grid" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _SecondTex ("SecondTexture", 2D) = "white" {}
        _BorderTex ("BorderTexture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainThreshold ("MainThreshold", Range(0, 10)) = 0.5
        _SecondThreshold ("SecondThreshold", Range(0, 10)) = 0.5
        _Position ("Position", Vector) = (0, 0, 0, 0)
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
            float3 _ObjectPosition;

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
                fixed4 col;
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
                    col.a = 0.0;                    
                }
                return col;
            }
            ENDCG
        }
    }
}
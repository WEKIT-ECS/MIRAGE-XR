Shader "Obi/VoxelMaterial" {

	Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Size("Square size",Float) = 0.95
        _InsideColor("Inside color",Color) = (1,1,1,1)
        _OutsideColor("Outside color",Color) = (0,0,0,1)
    }

	SubShader { 

		Pass {
       
			Cull Back 
			Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			struct vin{
				float4 vertex   : POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};

            struct v2f {
                float4 pos: POSITION;
                fixed4 color    : COLOR;
			   float2 texcoord  : TEXCOORD0;
            };

		   sampler2D _MainTex;
            float _Size;
            fixed4 _InsideColor;
            fixed4 _OutsideColor;

            v2f vert(vin v) {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                o.texcoord = v.texcoord;
			   o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                
                float2 centered = i.texcoord * 2 - 1;
                float square = max(abs(centered.x), abs(centered.y)) - 0.05f/(2.0*1.4142);
                float width = fwidth(square);
                float alpha = smoothstep(_Size - width, _Size,  square);

                return lerp(_InsideColor,_OutsideColor,alpha);
            }

            ENDCG
        }
 
	} 
}


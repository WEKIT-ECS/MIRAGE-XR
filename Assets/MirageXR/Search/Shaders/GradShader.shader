// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:0,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33722,y:32495,varname:node_3138,prsc:2|emission-1236-OUT,alpha-7044-OUT,clip-4945-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32454,y:32442,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0.8758622,c3:1,c4:1;n:type:ShaderForge.SFN_TexCoord,id:3363,x:32043,y:33026,varname:node_3363,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Power,id:4945,x:33136,y:33076,varname:node_4945,prsc:2|VAL-7566-OUT,EXP-716-OUT;n:type:ShaderForge.SFN_Power,id:3949,x:33071,y:32886,varname:node_3949,prsc:2|VAL-7566-OUT,EXP-421-OUT;n:type:ShaderForge.SFN_Slider,id:421,x:32624,y:32814,ptovrint:False,ptlb:Gradient height,ptin:_Gradientheight,varname:node_421,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:3.501852,max:16;n:type:ShaderForge.SFN_Multiply,id:7044,x:33259,y:32756,varname:node_7044,prsc:2|A-2288-OUT,B-3949-OUT;n:type:ShaderForge.SFN_Slider,id:716,x:32464,y:33260,ptovrint:False,ptlb:Clip height,ptin:_Clipheight,varname:node_716,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1.304345,max:8;n:type:ShaderForge.SFN_Tex2d,id:766,x:32454,y:32635,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_766,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:1236,x:32735,y:32558,varname:node_1236,prsc:2|A-7241-RGB,B-766-RGB;n:type:ShaderForge.SFN_Slider,id:2288,x:32878,y:32692,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_2288,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1.786346,max:8;n:type:ShaderForge.SFN_OneMinus,id:8747,x:32720,y:32931,varname:node_8747,prsc:2|IN-7182-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:7566,x:32720,y:33087,ptovrint:False,ptlb:FlipDirection,ptin:_FlipDirection,varname:node_7566,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-7182-OUT,B-8747-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:7182,x:32413,y:32945,ptovrint:False,ptlb:VerticalDirection,ptin:_VerticalDirection,varname:_FlipDirection_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-3363-U,B-3363-V;proporder:766-7241-421-716-2288-7182-7566;pass:END;sub:END;*/

Shader "Shader Forge/GradShader" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0,0.8758622,1,1)
        _Gradientheight ("Gradient height", Range(0, 16)) = 3.501852
        _Clipheight ("Clip height", Range(0, 8)) = 1.304345
        _Opacity ("Opacity", Range(0, 8)) = 1.786346
        [MaterialToggle] _VerticalDirection ("VerticalDirection", Float ) = 0
        [MaterialToggle] _FlipDirection ("FlipDirection", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float _Gradientheight;
            uniform float _Clipheight;
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float _Opacity;
            uniform fixed _FlipDirection;
            uniform fixed _VerticalDirection;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float _VerticalDirection_var = lerp( i.uv0.r, i.uv0.g, _VerticalDirection );
                float _FlipDirection_var = lerp( _VerticalDirection_var, (1.0 - _VerticalDirection_var), _FlipDirection );
                clip(pow(_FlipDirection_var,_Clipheight) - 0.5);
////// Lighting:
////// Emissive:
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(i.uv0, _Texture));
                float3 emissive = (_Color.rgb*_Texture_var.rgb);
                float3 finalColor = emissive;
                return fixed4(finalColor,(_Opacity*pow(_FlipDirection_var,_Gradientheight)));
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float _Clipheight;
            uniform fixed _FlipDirection;
            uniform fixed _VerticalDirection;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float _VerticalDirection_var = lerp( i.uv0.r, i.uv0.g, _VerticalDirection );
                float _FlipDirection_var = lerp( _VerticalDirection_var, (1.0 - _VerticalDirection_var), _FlipDirection );
                clip(pow(_FlipDirection_var,_Clipheight) - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}

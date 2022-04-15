// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:34851,y:32420,varname:node_3138,prsc:2|emission-2808-RGB,alpha-9247-OUT;n:type:ShaderForge.SFN_TexCoord,id:7060,x:32753,y:32713,varname:node_7060,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_RemapRange,id:7350,x:32938,y:32713,varname:node_7350,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-7060-UVOUT;n:type:ShaderForge.SFN_Length,id:396,x:33136,y:32640,varname:node_396,prsc:2|IN-7350-OUT;n:type:ShaderForge.SFN_Slider,id:3894,x:33136,y:32808,ptovrint:False,ptlb:External_diameter,ptin:_External_diameter,varname:node_3894,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Length,id:8698,x:33136,y:32906,varname:node_8698,prsc:2|IN-7350-OUT;n:type:ShaderForge.SFN_Slider,id:5273,x:33136,y:33066,ptovrint:False,ptlb:Internal_diameter,ptin:_Internal_diameter,varname:node_5273,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.9294428,max:1;n:type:ShaderForge.SFN_Add,id:694,x:33724,y:32643,varname:node_694,prsc:2|A-396-OUT,B-3576-OUT;n:type:ShaderForge.SFN_Add,id:1341,x:33730,y:32909,varname:node_1341,prsc:2|A-8698-OUT,B-2222-OUT;n:type:ShaderForge.SFN_OneMinus,id:5754,x:34109,y:32642,varname:node_5754,prsc:2|IN-5473-OUT;n:type:ShaderForge.SFN_Floor,id:5473,x:33916,y:32642,varname:node_5473,prsc:2|IN-694-OUT;n:type:ShaderForge.SFN_Floor,id:7568,x:33918,y:32909,varname:node_7568,prsc:2|IN-1341-OUT;n:type:ShaderForge.SFN_Multiply,id:2385,x:34108,y:32909,varname:node_2385,prsc:2|A-5754-OUT,B-7568-OUT;n:type:ShaderForge.SFN_Color,id:2808,x:32753,y:32495,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_2808,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.9338235,c2:0.2197232,c3:0.2197232,c4:1;n:type:ShaderForge.SFN_Multiply,id:3650,x:34109,y:32398,varname:node_3650,prsc:2|A-5754-OUT,B-9923-OUT,C-6887-OUT;n:type:ShaderForge.SFN_Add,id:4285,x:34435,y:32698,varname:node_4285,prsc:2|A-3650-OUT,B-2385-OUT;n:type:ShaderForge.SFN_Slider,id:6958,x:34097,y:33072,ptovrint:False,ptlb:Total_opacity,ptin:_Total_opacity,varname:node_6958,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_OneMinus,id:3576,x:33516,y:32724,varname:node_3576,prsc:2|IN-3894-OUT;n:type:ShaderForge.SFN_OneMinus,id:2222,x:33512,y:33000,varname:node_2222,prsc:2|IN-5273-OUT;n:type:ShaderForge.SFN_Slider,id:3283,x:33266,y:32402,ptovrint:False,ptlb:Internal_opacity_size,ptin:_Internal_opacity_size,varname:node_3283,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:-0.5047702,max:1;n:type:ShaderForge.SFN_Add,id:9923,x:33795,y:32250,varname:node_9923,prsc:2|A-8698-OUT,B-3283-OUT;n:type:ShaderForge.SFN_Multiply,id:9247,x:34624,y:32698,varname:node_9247,prsc:2|A-4285-OUT,B-6958-OUT;n:type:ShaderForge.SFN_Slider,id:6887,x:33668,y:32434,ptovrint:False,ptlb:Internal_opacity,ptin:_Internal_opacity,varname:node_6887,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;proporder:2808-3894-5273-6958-3283-6887;pass:END;sub:END;*/

Shader "Shader Forge/PlacementTarget" {
    Properties {
        _Color ("Color", Color) = (0.9338235,0.2197232,0.2197232,1)
        _External_diameter ("External_diameter", Range(0, 1)) = 1
        _Internal_diameter ("Internal_diameter", Range(0, 1)) = 0.9294428
        _Total_opacity ("Total_opacity", Range(0, 1)) = 1
        _Internal_opacity_size ("Internal_opacity_size", Range(-1, 1)) = -0.5047702
        _Internal_opacity ("Internal_opacity", Range(0, 2)) = 0
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
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float _External_diameter;
            uniform float _Internal_diameter;
            uniform float4 _Color;
            uniform float _Total_opacity;
            uniform float _Internal_opacity_size;
            uniform float _Internal_opacity;
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
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float3 emissive = _Color.rgb;
                float3 finalColor = emissive;
                float2 node_7350 = (i.uv0*2.0+-1.0);
                float node_5754 = (1.0 - floor((length(node_7350)+(1.0 - _External_diameter))));
                float node_8698 = length(node_7350);
                return fixed4(finalColor,(((node_5754*(node_8698+_Internal_opacity_size)*_Internal_opacity)+(node_5754*floor((node_8698+(1.0 - _Internal_diameter)))))*_Total_opacity));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}

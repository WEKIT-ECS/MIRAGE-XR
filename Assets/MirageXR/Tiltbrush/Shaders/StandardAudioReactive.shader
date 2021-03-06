// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

Shader "Custom/Standard_AudioReactive" {
  Properties {
    _EmissionColor ("Color", Color) = (1,1,1,1)
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
    _Glossiness ("Smoothness", Range(0,1)) = 0.5
    _Metallic ("Metallic", Range(0,1)) = 0.0
  }
  SubShader {
    Tags { "Queue"="AlphaTest+20" }

    CGPROGRAM
    #pragma surface surf Standard fullforwardshadows nofog
    #pragma multi_compile __ AUDIO_REACTIVE
    #pragma target 3.0
    #include "./Include/Brush.cginc"

    sampler2D _MainTex;

    struct Input {
      float2 uv_MainTex;
    };

    half _Glossiness;
    half _Metallic;
    fixed4 _EmissionColor;
    float4 _AudioReactiveColor;

    void surf (Input IN, inout SurfaceOutputStandard o) {

      float index = IN.uv_MainTex.y;
#ifdef AUDIO_REACTIVE
      float wav = tex2D(_WaveFormTex, float2(index,0)).r - .5f;
      float4 c = tex2D(_MainTex, IN.uv_MainTex + half2(wav,0) * .1) * _EmissionColor;
      c += c * _BeatOutput.x * 10;
      o.Smoothness = .8;
#else
      float4 c = tex2D(_MainTex, IN.uv_MainTex) * _EmissionColor;
      o.Smoothness = _Glossiness;
#endif
      o.Emission = c.rgb;
      o.Albedo = 0;
      // Metallic and smoothness come from slider variables
      o.Metallic = _Metallic;
      o.Alpha = c.a;
    }
    ENDCG
  }
  FallBack "Diffuse"
}

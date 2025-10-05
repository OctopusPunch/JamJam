Shader "Custom/EyeShader_Layers_Array"
{
    Properties
    {
        [Toggle(PIXELSNAP_ON)] _PixelSnap ("Pixel Snap", Float) = 1
        _BackgroundTex ("Background", 2D) = "white" {}
        _ShadeTex      ("Shade", 2D) = "white" {}
        _HighlightTex  ("Highlight", 2D) = "white" {}
        _EyeTexArray   ("Eye Frames", 2DArray) = "" {}
        _EyeFrame      ("Eye Frame", Float) = 0
        _EyeOffsetPX   ("Eye Offset PX", Vector) = (64,64,0,0)
        _CenterPX      ("Center PX", Vector) = (64,64,0,0)
        _FisheyeStrength ("Fisheye Strength", Range(0,4)) = 0
        _FisheyePower    ("Fisheye Power", Range(1,6)) = 2
        _SnapSize      ("Snap Size", Float) = 64
        _EyeScale      ("Eye Scale", Float) = 2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            sampler2D _BackgroundTex; float4 _BackgroundTex_ST;
            sampler2D _ShadeTex;      float4 _ShadeTex_ST;
            sampler2D _HighlightTex;  float4 _HighlightTex_ST;

            UNITY_DECLARE_TEX2DARRAY(_EyeTexArray);

            float2 _EyeOffsetPX;
            float2 _CenterPX;
            float  _FisheyeStrength;
            float  _FisheyePower;
            float  _SnapSize;
            float  _EyeScale;
            float  _EyeFrame;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            float2 PixelSnapUV(float2 uv, float g){ return floor(uv*g)/g; }
            float2 FisheyeUV(float2 uv, float k, float p){ float2 c=uv-0.5; float r=length(c); float s=1.0+k*pow(r,p); return 0.5+c*s; }

            v2f vert(appdata v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex);
                #if defined(PIXELSNAP_ON)
                o.pos=UnityPixelSnap(o.pos);
                #endif
                o.uv=v.uv; return o;
            }

            fixed4 Over(fixed4 top, fixed4 bottom){ return top + (1.0 - top.a)*bottom; }

            fixed4 frag(v2f i):SV_Target
            {
                float2 uv = i.uv;
                float2 uvSnap = PixelSnapUV(uv, _SnapSize);

                fixed4 bg    = tex2D(_BackgroundTex, uvSnap * _BackgroundTex_ST.xy + _BackgroundTex_ST.zw);
                fixed4 shade = tex2D(_ShadeTex,      uvSnap * _ShadeTex_ST.xy      + _ShadeTex_ST.zw); shade.rgb *= shade.a;

                float2 uvFish = FisheyeUV(uv, _FisheyeStrength, _FisheyePower);
                float2 eyeOfs = (_EyeOffsetPX - _CenterPX) / _SnapSize;
                float2 uvEye  = PixelSnapUV(uvFish * _EyeScale + eyeOfs, _SnapSize);
                fixed4 eye = UNITY_SAMPLE_TEX2DARRAY(_EyeTexArray, float3(uvEye, _EyeFrame));
  
                float bgMask = bg.a;
                eye.a *= bgMask;
                eye.rgb *= eye.a;


                fixed4 hi     = tex2D(_HighlightTex, uv * _HighlightTex_ST.xy + _HighlightTex_ST.zw); hi.rgb *= hi.a;

                return Over(hi, Over(eye, Over(shade, bg)));
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}

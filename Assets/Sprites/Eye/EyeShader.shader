Shader "Custom/EyeShader"
{
    Properties
    {
        [Toggle(PIXELSNAP_ON)] _PixelSnap ("Pixel Snap", Float) = 1
        _EyeTex("Eye", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _OutlineTex("Outline", 2D) = "black" {}
        _ShadeTex("Shade Tex", 2D) = "white" {}
        _EyeOffsetPX("Eye Offset PX", Vector) = (64,64,0,0)
        _CenterPX("Center PX", Vector) = (64,64,0,0)
        _FisheyeStrength("Fisheye Strength", Range(0,4)) = 0
        _FisheyePower("Fisheye Power", Range(1,6)) = 2
        _SnapSize("Snap Size", Float) = 64
        _EyeScale("Eye Scale", Float) = 2
        _ShadeStrength("Shade Strength", Range(0,1)) = 0
        _MaskHard("Mask Hardness", Range(0,1)) = 1
        _MaskDilatePx("Mask Dilate Px", Range(0,2)) = 0
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
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            sampler2D _EyeTex;     float4 _EyeTex_ST;
            sampler2D _MaskTex;    float4 _MaskTex_ST;
            sampler2D _OutlineTex; float4 _OutlineTex_ST;
            sampler2D _ShadeTex;   float4 _ShadeTex_ST;

            float2 _EyeOffsetPX;
            float2 _CenterPX;
            float  _FisheyeStrength;
            float  _FisheyePower;
            float  _SnapSize;
            float  _EyeScale;
            float  _ShadeStrength;
            float  _MaskHard;
            float  _MaskDilatePx;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            float2 PixelSnapUV(float2 uv, float g) { return floor(uv * g) / g; }
            float2 FisheyeUV(float2 uv, float k, float p) { float2 c = uv - 0.5; float r = length(c); float s = 1.0 + k * pow(r, p); return 0.5 + c * s; }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                #if defined(PIXELSNAP_ON)
                o.pos = UnityPixelSnap(o.pos);
                #endif
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float2 uvOutline = PixelSnapUV(uv, _SnapSize);
                float4 outlineCol = tex2D(_OutlineTex, uvOutline * _OutlineTex_ST.xy + _OutlineTex_ST.zw);

                float2 mST = uvOutline * _MaskTex_ST.xy + _MaskTex_ST.zw;
                float  mA  = tex2D(_MaskTex, mST).a;
                if (_MaskDilatePx > 0.0)
                {
                    float px = _MaskDilatePx / _SnapSize;
                    float a1 = tex2D(_MaskTex, mST + float2( px,  0)).a;
                    float a2 = tex2D(_MaskTex, mST + float2(-px,  0)).a;
                    float a3 = tex2D(_MaskTex, mST + float2( 0 ,  px)).a;
                    float a4 = tex2D(_MaskTex, mST + float2( 0 , -px)).a;
                    mA = max(mA, max(max(a1,a2), max(a3,a4)));
                }
                float maskA = lerp(mA, step(0.5, mA), _MaskHard);

                float2 uvFish = FisheyeUV(uv, _FisheyeStrength, _FisheyePower);
                float2 eyeOfs = (_EyeOffsetPX - _CenterPX) / _SnapSize;
                float2 uvEye  = PixelSnapUV(uvFish * _EyeScale + eyeOfs, _SnapSize);
                float4 eyeCol = tex2D(_EyeTex, uvEye * _EyeTex_ST.xy + _EyeTex_ST.zw);

                float4 baseCol = lerp(0, eyeCol, maskA);

                float shadeMask = tex2D(_ShadeTex, uvOutline * _ShadeTex_ST.xy + _ShadeTex_ST.zw).a;
                float shadeA = saturate(_ShadeStrength * shadeMask) * maskA;
                float3 shadedRGB = lerp(baseCol.rgb, 0, shadeA);
                float4 shadedCol = float4(shadedRGB, baseCol.a);

                return outlineCol + (1.0 - outlineCol.a) * shadedCol;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}

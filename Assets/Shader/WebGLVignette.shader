Shader "Unlit/WebGLVignette"
{
    Properties
    {
        [PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Color", Color) = (0,0,0,1)
        _Center("Center", Vector) = (0.5,0.5,0,0)
        _Radius("Radius", Float) = 0.35
        _Softness("Softness", Float) = 0.25
        _Intensity("Intensity", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata { float4 vertex: POSITION; float2 uv: TEXCOORD0; };
            struct v2f     { float4 pos: SV_POSITION; float2 uv: TEXCOORD0; };

            // Image expects these even if we don't sample.
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float4 _Center;
            float  _Radius, _Softness, _Intensity;
            // _ScreenParams comes from Core.hlsl: x=width, y=height, z=1+1/width, w=1+1/height

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv  = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 d = i.uv - _Center.xy;

                // Aspect-correct so the mask is a circle in screen space
                float aspect = _ScreenParams.x / max(1.0, _ScreenParams.y);
                d.x *= aspect;

                float r  = _Radius;
                float s  = _Radius + max(1e-4, _Softness);
                float dd = dot(d, d);
                float r2 = r * r;
                float s2 = s * s;

                // Smooth ring from r..s
                float m = saturate((dd - r2) / max(1e-5, s2 - r2));
                float a = saturate(m * _Intensity) * _Color.a;

                return half4(_Color.rgb, a);
            }
            ENDHLSL
        }
    }
}

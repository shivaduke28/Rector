Shader "Rector/CrtWaveform"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,0)
        _Intensity ("Intensity", Range(0, 4)) = 1
        _Delta("Delta", Range(0, 1)) = 0.01
    }

    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            Name "Update"
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #include "RectorAudioWaveform.hlsl"

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            fixed4 _Color;
            fixed _Intensity;
            fixed _Delta;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                const float2 uv = IN.localTexcoord.xy;
                float x = uv.x;
                uint ind = floor(x * _RectorAudioWaveformSize);

                float data = RectorAudioWaveform(ind) + 0.5;
                float v = step(uv.y, data) * step(data, uv.y + _Delta) * _Intensity;
                return _Color + v;
            }
            ENDCG
        }
    }
}

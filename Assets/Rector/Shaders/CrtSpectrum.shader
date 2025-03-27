Shader "Rector/CrtSpectrum"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,0)
        _Intensity ("Intensity", Range(0, 4)) = 1
        _Delta("Delta", Range(0, 1)) = 0.01
        _BufferRatio("Buffer Ratio", Range(0, 1)) = 0.5
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

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            #include "AudioSpectrum.hlsl"
            fixed4 _Color;
            fixed _BufferRatio;
            fixed _Intensity;
            fixed _Delta;

            // ガウス重み計算関数
            float gaussian_weight(float x, float sigma)
            {
                return exp(-x * x / (2.0 * sigma * sigma));
            }

            float smoothingByGaussian(int index, float sigma)
            {
                const int radius = 2;

                float weightSum = 0;
                float sum = 0;

                for (int i = -radius; i <= radius; i++)
                {
                    float weight = gaussian_weight(i, sigma);
                    sum += AudioSpectrum(clamp(index + i, 0, RECTOR_AUDIO_SPECTRUM_BUFFER_SIZE - 1)) * weight;
                    weightSum += weight;
                }

                return sum / weightSum;
            }


            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                const float2 uv = IN.localTexcoord.xy;
                float x = uv.x;
                uint ind = floor(x * RECTOR_AUDIO_SPECTRUM_BUFFER_SIZE);

                float data = smoothingByGaussian(ind, exp(x * 8) - 1);
                float v = step(uv.y, data) * step(data, uv.y + _Delta) * _Intensity;
                return _Color + v;
                // float prev = tex2D(_SelfTexture2D, uv).r;
                // return saturate(v + prev * _BufferRatio);
            }
            ENDCG
        }
    }
}

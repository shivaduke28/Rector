Shader "Rector/GameOfLife"
{
    Properties
    {
        _Tex("Texture", 2D) = "black" {}
    }

    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            Name "Init"
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"

            #pragma vertex InitCustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            sampler2D _Tex;

            float4 frag(v2f_init_customrendertexture IN) : COLOR
            {
                const float2 uv = IN.texcoord.xy;
                float4 col = tex2D(_Tex, uv);
                float r = col.r * col.a;
                r = step(0.5,r);
                return float4(r, r, r, 1);
            }
            ENDCG
        }

        Pass
        {
            Name "Update"
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                int count = 0;
                const float2 uv = IN.localTexcoord.xy;
                float2 offset = 1.0 / float2(_CustomRenderTextureWidth, _CustomRenderTextureHeight);

                const bool alive = tex2D(_SelfTexture2D, uv).r > 0.5;

                count += tex2D(_SelfTexture2D, uv + float2(-offset.x, 0)).r > 0.5 ? 1 : 0;
                count += tex2D(_SelfTexture2D, uv + float2(offset.x, 0)).r > 0.5 ? 1 : 0;
                count += tex2D(_SelfTexture2D, uv + float2(0, offset.y)).r > 0.5 ? 1 : 0;
                count += tex2D(_SelfTexture2D, uv + float2(0, -offset.y)).r > 0.5 ? 1 : 0;
                count += tex2D(_SelfTexture2D, uv + float2(-offset.x, -offset.y)).r > 0.5 ? 1 : 0;
                count += tex2D(_SelfTexture2D, uv + float2(-offset.x, offset.y)).r > 0.5 ? 1 : 0;
                count += tex2D(_SelfTexture2D, uv + float2(offset.x, -offset.y)).r > 0.5 ? 1 : 0;
                count += tex2D(_SelfTexture2D, uv + float2(offset.x, offset.y)).r > 0.5 ? 1 : 0;

                int next = alive == 0 ? (count == 3 ? 1 : 0) : (count == 2 || count == 3 ? 1 : 0);
                return float4(next, next, next, 1);
            }
            ENDCG
        }
    }
}

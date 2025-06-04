Shader "Custom/CombinedTexture"
{
    Properties
    {
        _LeftTex ("Left Texture", 2D) = "black" {}
        _FrontTex ("Front Texture", 2D) = "black" {}
        _RightTex ("Right Texture", 2D) = "black" {}
        _Brightness ("Brightness", Range(0.5, 2.0)) = 1.0
        _Contrast ("Contrast", Range(0.5, 2.0)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _LeftTex;
            sampler2D _FrontTex;
            sampler2D _RightTex;
            float _Brightness;
            float _Contrast;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // UV 좌표에 따라 어떤 텍스처를 사용할지 결정
                float3 col;
                
                if (i.uv.x < 0.333) // 좌측 텍스처
                {
                    // 좌측 텍스처의 UV 좌표 조정 (0-0.333 범위를 0-1 범위로 변환)
                    float2 leftUV = float2(i.uv.x * 3.0, i.uv.y);
                    col = tex2D(_LeftTex, leftUV).rgb;
                }
                else if (i.uv.x < 0.667) // 정면 텍스처
                {
                    // 정면 텍스처의 UV 좌표 조정 (0.333-0.667 범위를 0-1 범위로 변환)
                    float2 frontUV = float2((i.uv.x - 0.333) * 3.0, i.uv.y);
                    col = tex2D(_FrontTex, frontUV).rgb;
                }
                else // 우측 텍스처
                {
                    // 우측 텍스처의 UV 좌표 조정 (0.667-1 범위를 0-1 범위로 변환)
                    float2 rightUV = float2((i.uv.x - 0.667) * 3.0, i.uv.y);
                    col = tex2D(_RightTex, rightUV).rgb;
                }
                
                // 밝기 및 대비 조정
                col = (col - 0.5) * _Contrast + 0.5;
                col = col * _Brightness;
                
                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
    
    FallBack "Unlit/Texture"
} 
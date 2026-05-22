Shader "UI/UniformDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale", Range(1, 30)) = 8
        _EdgeSoftness ("Edge Softness", Range(0, 0.2)) = 0.08
        _NoiseStrength ("Noise Strength", Range(0, 0.3)) = 0.15
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        
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
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            float4 _Color;
            float _DissolveAmount;
            float _NoiseScale;
            float _EdgeSoftness;
            float _NoiseStrength;
            
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            float noise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                
                float a = random(i);
                float b = random(i + float2(1, 0));
                float c = random(i + float2(0, 1));
                float d = random(i + float2(1, 1));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // 完全显示时直接返回
                if (_DissolveAmount >= 0.99f)
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                    return col;
                }
                
                // 完全隐藏时返回透明
                if (_DissolveAmount <= 0.01f)
                {
                    return fixed4(0, 0, 0, 0);
                }
                
                // 从左向右溶解
                float directionValue = i.uv.x;
                
                // 噪波
                float2 noiseUV = i.uv * _NoiseScale;
                float noiseValue = noise(noiseUV);
                float dissolveThreshold = directionValue + noiseValue * _NoiseStrength;
                dissolveThreshold = clamp(dissolveThreshold, 0, 1);
                
                // 计算可见度
                float isVisible = smoothstep(_DissolveAmount - _EdgeSoftness, _DissolveAmount, dissolveThreshold);
                isVisible = 1.0 - isVisible;
                
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                col.a *= isVisible;
                
                clip(col.a - 0.01);
                return col;
            }
            ENDCG
        }
    }
}
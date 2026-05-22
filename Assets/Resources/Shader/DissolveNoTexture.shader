// 文件：DissolveNoTexture.shader
Shader "Custom/DissolveNoTexture"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // 溶解效果参数
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveWidth ("Edge Width", Range(0, 0.3)) = 0.1
        _DissolveColor ("Edge Color", Color) = (1, 0.3, 0, 1)
        
        // 噪波参数
        _NoiseScale ("Noise Scale", Range(1, 20)) = 5
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.5
        
        // 边缘效果
        _EdgeGlow ("Edge Glow", Range(0, 2)) = 1.5
        _EdgeColor2 ("Second Edge Color", Color) = (1, 0.8, 0, 1)
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
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
            float4 _MainTex_ST;
            float _DissolveAmount;
            float _DissolveWidth;
            float4 _DissolveColor;
            float4 _Color;
            float _NoiseScale;
            float _NoiseStrength;
            float _EdgeGlow;
            float4 _EdgeColor2;
            
            // 简单的伪随机函数
            float random (float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898,78.233))) * 43758.5453123);
            }
            
            // 简单的噪波函数
            float noise (float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 生成程序化噪波
                float2 noiseUV = i.uv * _NoiseScale;
                float dissolveNoise = noise(noiseUV);
                
                // 添加时间变量让边缘闪烁（可选）
                // float time = _Time.y * 0.5;
                // dissolveNoise += sin(time + i.uv.x * 20) * 0.1;
                
                // 应用溶解强度
                dissolveNoise = dissolveNoise * (1 - _NoiseStrength) + _NoiseStrength;
                
                // 计算溶解阈值
                float threshold = _DissolveAmount;
                float isDissolved = step(dissolveNoise, threshold);
                
                // 计算边缘区域（带发光效果）
                float edgeRange = fwidth(dissolveNoise) * _DissolveWidth;
                float edgeFactor = smoothstep(threshold - edgeRange, threshold, dissolveNoise);
                
                // 采样主纹理
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                
                // 多重边缘颜色混合（更炫酷）
                float3 finalEdgeColor = lerp(_DissolveColor, _EdgeColor2, sin(dissolveNoise * 50) * 0.5 + 0.5);
                finalEdgeColor *= _EdgeGlow;
                
                // 混合边缘颜色
                col.rgb = lerp(col.rgb, finalEdgeColor, edgeFactor * (1 - isDissolved));
                
                // 应用透明度（溶解部分透明）
                col.a *= (1 - isDissolved);
                
                // 边缘发光增强
                col.a += edgeFactor * 0.5 * (1 - isDissolved);
                
                // 裁剪完全溶解的像素
                clip(col.a - 0.01);
                
                return col;
            }
            ENDCG
        }
    }
}
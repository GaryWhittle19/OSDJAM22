Shader "Hidden/CRTShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Bend ("CRT bend effect", Range(1,10)) = 4
        _Scan1Size("Scanline one size", Range(100, 300)) = 200
        _Scan1Speed ("Scanline one speed", Range(800, 1200)) = 1000
        _ScanLineAmount("Scanline amount", Range(0.01, 0.2)) = 0.05
        _VignetteSize("Vignette size", Range(0, 5)) = 1.9
        _VignetteSmooth("Vignette smoothness", Range(0.0, 2.0)) = 0.6
        _VignetteEdge("Vignette edge rounding", Range(0, 20)) = 8.0
        _NoiseSize("Noise size", Range(0, 100)) = 75
		_BlackoutAmount("Blackout amount", Range(0, 1.0)) = 0.05
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 crt_coords(float2 uv, float bend)
            {
                uv -= 0.5;
                uv *= 2.;
                uv.x *= 1. + pow(abs(uv.y)/bend, 2.);
                uv.y *= 1. + pow(abs(uv.x)/bend, 2.);
    
                uv /= 2.;
                return uv + .5;
            }

            float vignette(float2 uv, float size, float smoothness, float edgeRounding)
            {
 	            uv -= .5;
                uv *= size;
                float amount = sqrt(pow(abs(uv.x), edgeRounding) + pow(abs(uv.y), edgeRounding));
                amount = 1. - amount;
                return smoothstep(0., smoothness, amount);
            }

            float scanline(float2 uv, float lines, float speed)
            {
                return sin(uv.y * lines + _Time * speed);
            }

            float random(float2 uv)
            {
 	            return frac(sin(dot(uv, float2(15.5151, 42.2561))) * 12341.14122 * sin(_Time * 0.03));
            }

            float noise(float2 uv)
            {
 	            float2 i = floor(uv);
                float2 f = frac(uv);
    
                float a = random(i);
                float b = random(i + float2(1.0f,0.0f));
	            float c = random(i + float2(0.0f, 1.0f));
                float d = random(i + float2(1.0f, 1.0f));
    
                float2 u = smoothstep(0., 1., f);
    
                return lerp(a,b, u.x) + (c - a) * u.y * (1. - u.x) + (d - b) * u.x * u.y; 
                     
            }

            sampler2D _MainTex;
            float _Bend;
            float _Scan1Size;
            float _Scan1Speed;
            float _ScanLineAmount;
            float _VignetteSize;
            float _VignetteSmooth;
            float _VignetteEdge;
            float _NoiseSize;
            float _BlackoutAmount;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 crt_uv = crt_coords(i.uv, _Bend);
                float scan = scanline(i.uv, _Scan1Size, _Scan1Speed);
                fixed4 col = lerp(tex2D(_MainTex, crt_uv), fixed4(scan, scan, scan, 1.0f), _ScanLineAmount) * vignette(i.uv, 1.9f, 0.6f, 8.0f);
                //return lerp(col, fixed4(noise(i.uv * _NoiseSize), noise(i.uv * _NoiseSize), noise(i.uv * _NoiseSize), 1.0f), _NoiseAmount);
                return lerp(col, 0, _BlackoutAmount);
            }
            ENDCG
        }
    }
}

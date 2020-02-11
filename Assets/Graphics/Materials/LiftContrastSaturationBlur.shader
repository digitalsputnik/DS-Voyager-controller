Shader "Digital Sputnik/Lift Contrast Saturation Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Lift ("Lift", Float) = 0.0
        _Contrast ("Contrast", Float) = 1.0
        _Saturation ("Saturation", Float) = 1.0
        _BlurSize ("Blur", Range(0.00, 0.5)) = 0.0
        _StandardDeviation ("Standard Deviation", Range(0.00, 0.3)) = 0.02
    }
    SubShader
    {
		Cull Off
		ZWrite Off 
		ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#define PI 3.14159265359
			#define E 2.71828182846
            #define SAMPLES 30

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

            sampler2D _MainTex;
            float _Lift;
            float _Contrast;
            float _Saturation;
			float _BlurSize;
			float _StandardDeviation;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = 0;

				if (_StandardDeviation <= 0.0001)
                {
                    col = tex2D(_MainTex, i.uv);
                }
                else
                {
                    float invAspect = _ScreenParams.y / _ScreenParams.x;
					float stDevSquared = _StandardDeviation * _StandardDeviation;
                    float sum = 0;

				    for (float y = 0; y < SAMPLES; y++)
                    {
				        for (float x = 0; x < SAMPLES; x++)
                        {
					        float offsetX = (x/(SAMPLES-1) - 0.5) * _BlurSize * invAspect;
                            float offsetY = (y/(SAMPLES-1) - 0.5) * _BlurSize;

					        float2 uv = i.uv + float2(offsetX, offsetY);
					        float gauss = (1 / sqrt(2 * PI * stDevSquared)) * pow(E, -((offsetY*offsetY)/(2 * stDevSquared)));
					        sum += gauss;
					        col += tex2D(_MainTex, uv) * gauss;
				        }
				    }

				    col = col / sum;
                }

                col = ((1.0 - col) * _Lift) + col;
                col = (col - 0.5) * (_Contrast) + 0.5;
                float greyscale = dot(col.rgb, fixed3(.222, .707, .071));
                col.rgb = lerp(float3(greyscale, greyscale, greyscale), col.rgb, _Saturation);

                return col;
            }
            ENDCG
        }
    }
}

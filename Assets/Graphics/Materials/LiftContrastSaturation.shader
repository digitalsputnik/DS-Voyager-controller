Shader "Digital Sputnik/Lift Contrast Saturation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Lift ("Lift", Float) = 0.0
        _Contrast ("Contrast", Float) = 1.0
        _Saturation ("Saturation", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Lift;
            float _Contrast;
            float _Saturation;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // LIFT
                col = ((1.0 - col) * _Lift) + col;

                // CONTRAST
                col = (col - 0.5) * (_Contrast) + 0.5;

                // SATURATION
                float greyscale = dot(col.rgb, fixed3(.222, .707, .071));
                col.rgb = lerp(float3(greyscale, greyscale, greyscale), col.rgb, _Saturation);

                return col;
            }
            ENDCG
        }
    }
}

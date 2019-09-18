Shader "Voyager/Background"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _Resolution ("Resolution", Float) = 1.0
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
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Resolution;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = ComputeScreenPos(o.vertex) + float4(-_WorldSpaceCameraPos.xy / float2(unity_OrthoParams.x, unity_OrthoParams.y) / _WorldSpaceCameraPos.z, 0, 1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 coords = i.uv.xy / i.uv.w * _Resolution;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                coords.x = coords.x * aspect;
                fixed4 col = tex2D(_MainTex, coords);
                return col;
            }
            ENDCG
        }
    }
}

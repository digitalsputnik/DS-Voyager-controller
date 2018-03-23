Shader "Test/Kaspar"
{
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _visible ("Visible",range(0,1)) = 0
    }

    SubShader {
      Tags { "Queue"="Transparent" "RenderType"="Fade" }
      Blend SrcAlpha OneMinusSrcAlpha
      Cull Off Lighting Off ZWrite Off
      LOD 100

    pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float2 uv1 : TEXCOORD0;
			float2 uv2 : TEXCOORD1;
		};

	    struct v2f {
				float2 uv1 : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 vertex : SV_POSITION;
		};

		float4 _MainTex_ST;

		float _visible;
	    sampler2D _MainTex;

		v2f vert (appdata v) {
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv1 = TRANSFORM_TEX(v.uv1, _MainTex);
			o.uv2 = TRANSFORM_TEX(v.uv2, _MainTex);
			return o;
		}

	    fixed4 frag (v2f i) : SV_Target {
	    	clip(_visible-i.uv2.x);
			// sample the texture
			fixed4 col = tex2D(_MainTex, i.uv1);
			//col.a = 0.5;
			return col;
		}
	    ENDCG
    }
    }
}
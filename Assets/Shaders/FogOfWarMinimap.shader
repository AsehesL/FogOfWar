Shader "Hidden/FogOfWarMinimap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FogColor("FogColor", color) = (0,0,0,1)
		_MixValue("MixValue", float) = 0
	}
	SubShader
	{
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
			
			sampler2D _MainTex;
			half _MixValue;

			half4 _FogColor;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed3 mask = tex2D(_MainTex, i.uv).rgb;
				fixed4 col;
				col.rgb = _FogColor.rgb;
				col.a = saturate(1 - mask.r*_FogColor.a);

				fixed visual = lerp(mask.b, mask.g, _MixValue);
				col.a = lerp(col.a, 0, visual);
				return col;
			}
			ENDCG
		}
	}
}

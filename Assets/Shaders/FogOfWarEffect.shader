Shader "Hidden/FogOfWarEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FogColor("FogColor", color) = (0,0,0,1)
		_FogTex ("FogTex", 2D) = "black" {}
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
			float4 _MainTex_TexelSize;
			sampler2D _CameraDepthTexture;

			sampler2D _FogTex;

			half _MixValue;

			half4 _FogColor;

			float4x4 internal_CameraToProjector;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, i.uv);
#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
				i.uv.y = 1 - i.uv.y;
#endif
				fixed depth = tex2D(_CameraDepthTexture, i.uv).r;
				fixed4 pos = fixed4(i.uv.x * 2 - 1, i.uv.y * 2 - 1, -depth * 2 + 1, 1);
				pos = mul(unity_CameraInvProjection, pos);
				pos = mul(internal_CameraToProjector, pos);

				pos.xy /= pos.w;

				fixed3 tex = tex2D(_FogTex, pos.xy).rgb;

				float2 atten = saturate((0.5 - abs(pos.xy - 0.5)) / (1 - 0.9));

				fixed4 col;
				col.rgb = lerp(_FogColor.rgb, fixed3(1, 1, 1), tex.r*_FogColor.a);

				fixed visual = lerp(tex.b, tex.g, _MixValue);
				col.rgb = lerp(col.rgb, fixed3(1, 1, 1), visual)*atten.x*atten.y;

				c.rgb *= col.rgb;
				c.a = 1;
				return c;
			}
			ENDCG
		}
	}
}

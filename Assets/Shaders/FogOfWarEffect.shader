Shader "Hidden/FogOfWarEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FogTex ("FogTex", 2D) = "black" {}
		_MixValue("MixValue", float) = 0
		_BlurOffset("BlurOffset", float) = 0
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
			half _BlurOffset;

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

				//fixed4 tex = tex2Dproj(_FogTex, UNITY_PROJ_COORD(pos));
				fixed3 tex = tex2D(_FogTex, pos.xy).rgb*0.5;
				tex.rgb += tex2D(_FogTex, pos.xy + half2(_BlurOffset, 0)).rgb*0.125;
				tex.rgb += tex2D(_FogTex, pos.xy - half2(_BlurOffset, 0)).rgb*0.125;
				tex.rgb += tex2D(_FogTex, pos.xy + half2(0, _BlurOffset)).rgb*0.125;
				tex.rgb += tex2D(_FogTex, pos.xy - half2(0, _BlurOffset)).rgb*0.125;

				float2 atten = saturate((0.5 - abs(pos.xy - 0.5)) / (1 - 0.9));

				fixed4 col = fixed4(tex.r, tex.r, tex.r, 1)*0.5;
				fixed visual = lerp(tex.b, tex.g, _MixValue);
				col.rgb = lerp(col.rgb, fixed3(1, 1, 1), visual)*atten.x*atten.y;

				c.rgb *= col.rgb;
				return c;
			}
			ENDCG
		}
	}
}

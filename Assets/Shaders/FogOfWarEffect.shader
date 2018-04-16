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
				float2 uv_depth : TEXCOORD1;
				float3 interpolatedRay : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _CameraDepthTexture;

			sampler2D _FogTex;

			half _MixValue;

			half4 _FogColor;

			float4x4 _FrustumCorners;
			float4x4 internal_WorldToProjector;

			v2f vert (appdata v)
			{
				v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0.1;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;
				o.uv_depth = v.uv.xy;
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
				#endif
				o.interpolatedRay = _FrustumCorners[(int)index].xyz;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));

				fixed depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.uv_depth)));
				
				fixed4 pos = fixed4(depth*i.interpolatedRay, 1);
				
				pos.xyz += _WorldSpaceCameraPos;
				pos = mul(internal_WorldToProjector, pos);
				
				
				pos.xy /= pos.w;

				fixed3 tex = tex2D(_FogTex, pos.xy).rgb;

				float2 atten = saturate((0.5 - abs(pos.xy - 0.5)) / (1 - 0.9));

				fixed3 col;
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

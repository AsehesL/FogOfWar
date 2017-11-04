Shader "Hidden/FogOfWarBlur" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Offset ("Offset", float) = 0
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Offset;

			fixed4 frag(v2f_img i):COLOR{
				float4 uv01 = i.uv.xyxy + _Offset * float4(0, 1, 0, -1);
				float4 uv10 = i.uv.xyxy + _Offset * float4(1, 0, -1, 0);
				float4 uv23 = i.uv.xyxy + _Offset * float4(0, 1, 0, -1) * 2.0;
				float4 uv32 = i.uv.xyxy + _Offset * float4(1, 0, -1, 0) * 2.0;
				float4 uv45 = i.uv.xyxy + _Offset * float4(0, 1, 0, -1) * 3.0;
				float4 uv54 = i.uv.xyxy + _Offset * float4(1, 0, -1, 0) * 3.0;

				fixed4 c = fixed4(0, 0, 0, 0);
				c += 0.4 * tex2D(_MainTex, i.uv);
				c += 0.075 * tex2D(_MainTex, uv01.xy);
				c += 0.075 * tex2D(_MainTex, uv01.zw);
				c += 0.075 * tex2D(_MainTex, uv10.xy);
				c += 0.075 * tex2D(_MainTex, uv10.zw);
				c += 0.05 * tex2D(_MainTex, uv23.xy);
				c += 0.05 * tex2D(_MainTex, uv23.zw);
				c += 0.05 * tex2D(_MainTex, uv32.xy);
				c += 0.05 * tex2D(_MainTex, uv32.zw);
				c += 0.025 * tex2D(_MainTex, uv45.xy);
				c += 0.025 * tex2D(_MainTex, uv45.zw);
				c += 0.025 * tex2D(_MainTex, uv54.xy);
				c += 0.025 * tex2D(_MainTex, uv54.zw);

				return c;
			}

			ENDCG
		}
	} 
	FallBack off
}

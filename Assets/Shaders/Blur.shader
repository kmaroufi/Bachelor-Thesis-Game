// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Blur" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;

		half2 uv : TEXCOORD0;
		half2 taps[4] : TEXCOORD1; 
	};
	
	fixed2 dir;
	float strength;
	
	sampler2D _MainTex;
	
	v2f vert (appdata_img v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);

		o.uv = v.texcoord.xy;
		
		fixed hstep = dir.x;
		fixed vstep = dir.y;

		o.taps[0] =  half2(v.texcoord.x - 3.2307692308 * hstep * strength, v.texcoord.y - 3.2307692308 * vstep * strength);
		o.taps[1] =  half2(v.texcoord.x - 1.3846153846 * hstep * strength, v.texcoord.y - 1.3846153846 * vstep * strength);
		
		o.taps[2] =  half2(v.texcoord.x + 1.3846153846 * hstep * strength, v.texcoord.y + 1.3846153846 * vstep * strength);
		o.taps[3] =  half2(v.texcoord.x + 3.2307692308 * hstep * strength, v.texcoord.y + 3.2307692308 * vstep * strength);

		return o;
	}
	
	half4 frag (v2f i) : COLOR {
		
		half4 color = float4 (0,0,0,0);

		color += tex2D (_MainTex, i.uv) * 0.2270270270;
		color += tex2D (_MainTex, i.taps[0]) * 0.0702702703;
		color += tex2D (_MainTex, i.taps[1]) * 0.3162162162;
		color += tex2D (_MainTex, i.taps[2]) * 0.3162162162;
		color += tex2D (_MainTex, i.taps[3]) * 0.0702702703;
		
		return color;
	}

	ENDCG
	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }

      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
}

Fallback off


} // shader
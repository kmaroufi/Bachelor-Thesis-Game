Shader "BlitColor" {
Properties {    
    _MainTex ("Texture", 2D) = "white" { } 
	
	_Color ("Color", Color) = (0,0,0,0)
}
SubShader {
    Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag	
		#include "UnityCG.cginc"

		sampler2D _MainTex;		
		fixed4 _Color;
		float4 _MainTex_ST;

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		v2f vert (appdata_base v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			return o;
		}
		

		fixed4 frag (v2f i) : SV_Target {
			return _Color;
		}
		
		ENDCG

	}
}
Fallback off
} 
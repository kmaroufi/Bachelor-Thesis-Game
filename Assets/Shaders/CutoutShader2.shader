Shader "Unlit/CutoutShader2" {
Properties {    
    _MainTex ("Texture", 2D) = "white" { } 
	
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.04
	_Color0 ("Color 0", Color) = (1,1,1,1)
	_Color1 ("Color 1", Color) = (1,1,1,1)
	_Color2 ("Color 2", Color) = (1,1,1,1)
	_Color3 ("Color 3", Color) = (1,1,1,1)
}
SubShader {
	//Tags {"Queue"="Transparent"}
	//GrabPass{}
    Pass {
		//Blend SrcAlpha OneMinusSrcAlpha
		//Blend SrcColor OneMinusSrcColor    
		//Blend One One // Additive
		//Blend One OneMinusSrcAlpha
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag	
		#include "UnityCG.cginc"

		sampler2D _MainTex;		
		fixed _Cutoff;
		float4 _MainTex_ST;
		fixed4 _Color0;
		fixed4 _Color1;
		fixed4 _Color2;
		fixed4 _Color3;

		struct v2f {
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
		};	
			
		v2f vert (appdata_base v){
			v2f o;
			o.pos = UnityObjectToClipPos (v.vertex);
			o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
			return o;
		};
		

		fixed4 frag (v2f i) : SV_Target {		
			fixed4 texcol = tex2D (_MainTex, i.uv);
			
			clip(texcol.a - _Cutoff);
			texcol.a = 1;
			
			if (texcol.r > texcol.b && texcol.g > texcol.b && abs(texcol.r - texcol.g) < 0.05) {
				texcol.xyz = _Color3.xyz;
			} else if (texcol.g > texcol.r && texcol.g > texcol.b) {
				texcol.xyz = _Color0.xyz;
			} else if (texcol.b > texcol.r && texcol.b > texcol.g) {
				texcol.xyz = _Color1.xyz;
			} else if (texcol.r > texcol.g && texcol.r > texcol.b) {
				texcol.xyz = _Color2.xyz;
			}
			
			return texcol;
		}
		
		ENDCG

	}
}
Fallback off
} 
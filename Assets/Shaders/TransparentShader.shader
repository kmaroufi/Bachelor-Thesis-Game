﻿Shader "Unlit/TransparentShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
		Tags {"Queue"="Transparent"}
        //LOD 100

        Pass
        {
			//Blend One One
			Blend SrcAlpha OneMinusSrcAlpha
			//Blend srcalpha dstalpha
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				//clip(col.a - 0.1);
				//if (col.a < 0.01) col.rgb = fixed3(0, 0, 0);
				//if (col.r + col.g + col.b < 0.01) {
				//	discard;
				//}
				//clip(col.a - 0.99);
                return col;
            }
            ENDCG
        }
    }
}

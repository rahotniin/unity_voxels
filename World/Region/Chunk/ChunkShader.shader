Shader "Unlit/ChunkShader"
{
	Properties
	{
		//_MainTex ("Texture", 2D) = "white" {}
		_TextureArray ("TextureArray", 2DArray) = "white"{}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma require 2darray

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float3 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 uv : TEXCOORD0;
			};

			UNITY_DECLARE_TEX2DARRAY(_TextureArray);
			//sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = float3(TRANSFORM_TEX(v.uv.xy, _MainTex), 0);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv.xy);
				fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, i.uv);
				return col;
			}

			ENDCG
		}
	}
}

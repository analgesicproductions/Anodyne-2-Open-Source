Shader "Unlit/TranspUnlitSpriteMesh"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Transparency ("Transparency", Range(0,1)) = 0
		_ColorMultiplier ("ColorMultiplier", Range(0,1)) = 1
		SourceMode ("SourceMode",float) = 5
		DestinationMode ("DestinationMode",float) = 10
		//OpMode ("OpMode", float) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"}
		//BlendOp [OpMode]
		Blend [SourceMode] [DestinationMode]
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform float _Transparency;
			uniform float _ColorMultiplier;
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
				col.a *= _Transparency;
				col.r *= _ColorMultiplier;
				col.g *= _ColorMultiplier;
				col.b *= _ColorMultiplier;
				return col;
			}
			ENDCG
		}
	}
}

Shader "Hidden/Pixelize"
{

// Note this doesnt work unless at least one directional light casts shadows?
// My own implementation of fog that has blend modes, and is extensible...
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Strength ("Strength",float) = 1
		_Scale ("Scale",int) = 3
		_W ("W",float) = 1366
		_H ("H",float) = 1300
	}
	SubShader
	{
		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"


			struct appdata {
				
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
				// Shorthand for mul(UNITY_MATRIX_MVP,v.vertex)
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			uniform float _Strength;
			uniform int _Scale;
			uniform float _W;
			uniform float _H;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 cellSize = float2(_Strength*_Scale,_Strength*_Scale);
				float2 pixelspace = i.uv; 
				pixelspace.x *= _W; pixelspace.y *= _H;
				pixelspace /= cellSize;
				pixelspace = ceil(pixelspace);
				pixelspace *= cellSize;
				pixelspace.x /= _W; pixelspace.y /= _H;
				fixed4 col = tex2D(_MainTex,pixelspace);
				//col *= (1 - (_Strength/20));
				//col.a = 1;
				return col;
			}
			ENDCG
		}
	}
}

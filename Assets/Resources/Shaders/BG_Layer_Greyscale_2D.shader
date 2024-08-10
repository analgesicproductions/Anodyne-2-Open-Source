Shader "Unlit/TranspUnlitGreyscale"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Transparency ("Transparency", Range(0,1)) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"}
		
		GrabPass { }

		Blend One Zero
		Pass
		{
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform float _Transparency;
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION; // NOT interpolated
				float4 screenPos: TEXCOORD1; // Interpolated!
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _GrabTexture;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = o.vertex;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Clip space, -1 to 1
				float2 grabTexcoord = (i.screenPos.xy + 1)/2; // Converts to UV coords from 0 to 1 that will sample from the grab tex2D
				// the conversion works because the clip space coords of stuff fit to the camera view
				#if UNITY_UV_STARTS_AT_TOP
				grabTexcoord.y = 1.0 - grabTexcoord.y;
				#endif
				
				fixed4 sample = tex2D(_GrabTexture, grabTexcoord); 
				float grey = 0.21 * sample.r + 0.71 * sample.g + 0.07 * sample.b;
				float u_colorFactor = _Transparency;
				sample.r = sample.r * u_colorFactor + grey * (1.0 - u_colorFactor);
				sample.g = sample.g * u_colorFactor + grey * (1.0 - u_colorFactor);
				sample.b = sample.b * u_colorFactor + grey * (1.0 - u_colorFactor);
				sample.a = 1.0;
				return sample;	
			}
			ENDCG
		}



	}
}

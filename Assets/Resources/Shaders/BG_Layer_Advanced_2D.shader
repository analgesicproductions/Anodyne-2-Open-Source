Shader "Unlit/TranspUnlitADVANCEDSpriteMesh"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Transparency ("Transparency", Range(0,1)) = 0
		_OpMode ("OpMode", int) = 0 //Add by default, set to advanced things in script. Overrides "Blend" if advanced
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
			uniform int _OpMode;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = o.vertex;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			// a = bottom, b = top
			fixed4 Overlay(fixed4 a, fixed4 b) {
				fixed4 r = a < .5 ? 2.0 * a * b : 1.0 - 2.0 * (1.0 - a) * (1.0 - b);
				return r;
			}

			// Pegtop formula
			fixed4 SoftLight(fixed4 a, fixed4 b) {
				fixed4 r = (1 - 2*b)*a*a + 2*b*a;
				return r;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				// Clip space, -1 to 1
				float2 grabTexcoord = (i.screenPos.xy + 1)/2; // Converts to UV coords from 0 to 1 that will sample from the grab tex2D
				// the conversion works because the clip space coords of stuff fit to the camera view
				#if UNITY_UV_STARTS_AT_TOP
				grabTexcoord.y = 1.0 - grabTexcoord.y;
				#endif
				
				fixed4 grabColor = tex2D(_GrabTexture, grabTexcoord); 

				
				col.a *= _Transparency;
				if (_OpMode == 29){ // SoftLight 
					col *= col.a;
					col = SoftLight(grabColor,col);
				} else if (_OpMode == 23) { // Overlay
					col *= col.a;
					col = Overlay(grabColor,col);
				} else {
					col = col*col.a + grabColor*(1-col.a);
				}
				return col;
			}
			ENDCG
		}



	}
}

// Copyright Analgesic Productions LLC 2018- (Written by Sean Han Tani)
// Originally written for Anodyne 2: Return to Dust
// Feel free to adapt and change for any use, with attribution.

// This simple vertex shader can be adjusted to make a variety of interesting animations on a low-poly column shaped model -smooth, fleshy lava-lamp motions, or frightening, quick fleshy muscle movements.
// It isn't rotation-agnostic, so it only works with vertical columns, though I'm sure you could adjust it to fix that.
// Inspired by fleshy columns in Jabu-Jabu's belly of Ocarina of Time.

// Looks good with transparency and concentric cylinders.
// Also looks good with a texture-offsetter script.
// Join our newsletter and see https://analgesic.productions/code.html for more scripts, and follow us on Twitter at https://twitter.com/analgesicprod 

Shader "Unlit/WavyTranspAdditive"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Transparency ("Transparency",float) = 1
		// The strength of the effect that scales vertices inwards or outwards relative to the object-space model origin
		_Crunch ("Crunch",float) = 0.005
		// Strength of effect that pushes vertices outwards from the x-axis origin (x-scaling kinda)
		_XOscillation ("XOscillation",float) = 4
		// Strength of effect that offsets vertices from their origin position ("shaking")
		_Contraction ("Contraction",float) = 3
		// Affects how fast the effect happens
		_Speed ("Speed",float) = 50
	}
	SubShader
	{
		// Lets the model be transparent
		Tags { "Queue" = "Transparent+2" "RenderType"="Transparent" }
		CULL Off
		Blend SrcAlpha One
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		//#pragma fragment frag
		#pragma target 3.0

			float _Transparency;
			float _Crunch;
			float _XOscillation;
			float _Contraction;
			float _Speed;

			sampler2D _MainTex;

			struct Input {
			float2 uv_MainTex;
			};
			
			// Here's what you probably care about!
			void vert (inout appdata_full v)
			{
				float3 normal = normalize(v.normal);
				// All of these use 'v.vertex.y' to offset the oscillating pattern based on the vertex's y-position, giving the wavy effect.

				// In a column the normal points away from the object's center, so use that direction to scale the vertex relative to the center
				v.vertex.xyz += normal * _Contraction * (1+ sin(_Speed*_Time.x+ 0.06f*v.vertex.y));
				// Push/pull vertex relative to x-axis origin
				v.vertex.x *= 1+ _Crunch*sin((_Speed/10)*_Time.x+ v.vertex.y);
				// Offset vertex.
				v.vertex.x += _XOscillation*sin(_Speed*_Time.x+ 0.05*v.vertex.y);


				//v.vertex = UnityObjectToClipPos(v.vertex);
				//v.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(v,v.vertex);
			}
			
		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			c.a = _Transparency;
			o.Albedo = c.rgb;
			o.Alpha = 0.5f;
		}
			//fixed4 frag (v2f i) : SV_Target
			//{
				//fixed4 col = tex2D(_MainTex, i.uv);
				//UNITY_APPLY_FOG(i.fogCoord, col);
				//col.a = _Transparency;
				//return col;
			//}
			ENDCG
	}
}

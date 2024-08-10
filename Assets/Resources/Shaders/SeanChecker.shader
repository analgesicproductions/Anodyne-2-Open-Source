// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Sean Checker"
{


// Place inspector props here.
// Naming convention is start with _,to avoid duplicates... but not needed.
	Properties {
		// Name, type, default
		_Tint ("Tint", Color) = (1,1,1,1)
		_Color1 ("Color1",Color) = (0,0,0,1)
		_Color2 ("Color2",Color) = (1,1,1,1)
		_MainTex ("Texture",2D) = "white" {}
	}

	SubShader {


		Fog {
			Mode Global
		}

		Pass {

			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram
			#pragma multi_compile_fog

			#include "UnityCG.cginc"


			float4 _Tint;
			float4 _Color1;
			float4 _Color2;

			// sampler2D is the type to access texturse in a shader prog
			sampler2D _MainTex;
			// Adding a texture property to the shader also means we can
			// change tiling and offset.
			float4 _MainTex_ST; // ST mean scale/translation... due to
								// backwards compat we use this, vs "TO" :(

			


			// Used to group outputs / inputs for vert/frag progs
			struct Interpolators {
				float4 position: SV_POSITION;
				float2 uv: TEXCOORD0;
				UNITY_FOG_COORDS(1) // 1 means it will use TEXCOORD1
			};


			// Groups input params to vertex

			struct VertexData {
				float4 position: POSITION;
				float2 uv: TEXCOORD0;
			};

			// Returns transformed coordinates.
			// SV_POSITION means SYSTEM VALUE and FINAL VERTEX POSITION
			// POSITION gives us the object-space position of the vertex

			// Adding 'out' means the fragment program can use it. Note that the
		// name of this out param doesnt need to match, only the SEMANTIC matters

		// This works too, without the interpolator struct.
//			float4 MyVertexProgram ( ... ) : SVPOSITION
			Interpolators MyVertexProgram (
				VertexData v
			) {
				Interpolators i;

				// note that  .xx .yx etc work (.rgba .rg etc) - 'swizzle'
				// shorthand for mul(UNITY_MATRIX_MVP, v.position)
				// This transforms 'raw object vertices' (in object-space) into our world space.
				i.position = UnityObjectToClipPos(v.position);	

				// Scale by the tiling vector (stored as xy in the var)
				// Then  offset by the offset portion
				// How to do this manually
				//i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
				// With UnityCG help:
				i.uv = TRANSFORM_TEX(v.uv,_MainTex);

				UNITY_TRANSFER_FOG(i,i.position);
				return i;
			}


			// Fragment program, per triangle, gets all the interpolated 
			// vertices as pixels.

			// outputs an RGBA color for a pixel.
			// SV_TARGET means default shader target, or the frame buffer
			// which holds the image we're generating

			// Note the parameter needs a semantic as well.
			float4 MyFragmentProgram (
				Interpolators i
			) : SV_TARGET {
				// Because this is an opaque shader, alpha won't work here
				int u = i.uv.x;
				int v = i.uv.y;
				float4 c;
				if (u % 2 == 1) {
					if (v % 2 == 1) {
						c = _Color1;
					} else {
						c = _Color2;
					}
				} else {
					if (v % 2 == 1) {
						c = _Color2;
					}  else {
						c = _Color1;
					}
				}
				UNITY_APPLY_FOG(i.fogCoord,c);
				return c;
				// tex2D function samples a texture given uv coords.
				//return tex2D (_MainTex, i.uv) * _Tint;
			}

			ENDCG
		}
	}
}

// Upgrade NOTE: commented out 'float4x4 _Object2World', a built-in variable
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/World Gradient Y Taper"
{


	Properties {
		_Color1 ("Color1",Color) = (0,0,0,1)
		_Color2 ("Color2",Color) = (1,1,1,1)
		_Center ("Center",Vector) = (0,0,0,0)
		_Radius ("Radius",Float) = 5
		_YTaper ("YTaper",Float) = 7
		_YTaperStart("YTaperStart",Float)= -1
	}
	SubShader {
		Pass {

			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityCG.cginc"


			float4 _Color1;
			float4 _Color2;
			float4 _Center;
			float _Radius;
			float _YTaper;
			float _YTaperStart;

			// uniform float4x4 _Object2World;


			// Used to group outputs / inputs for vert/frag progs
			struct Interpolators {
				float4 position: SV_POSITION;
				float4 posWorld: TEXCOORD0;
			};


			// Groups input params to vertex

			struct VertexData {
				float4 position: POSITION;
			};

			Interpolators MyVertexProgram (
				VertexData v
			) {
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);	
				i.posWorld = mul(unity_ObjectToWorld,v.position);				
				return i;
			}


			float4 MyFragmentProgram (
				Interpolators i
			) : SV_TARGET {
				float2 a = i.posWorld.xz;
				float2 b = _Center.xz;
				float dis = clamp(distance(a,b) / _Radius,0,1);
				float4 c = lerp(_Color1,_Color2,dis);
				// Now lerp from center of object to some pt above
				if (i.posWorld.y > _YTaperStart) {
					dis = clamp((i.posWorld.y - _YTaperStart)/_YTaper,0,1);
					c = lerp(c,_Color2,dis);
				}
				c.w = 1;
				return c;
			}

			ENDCG
		}
	}
}

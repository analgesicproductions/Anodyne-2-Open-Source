Shader "Hidden/SeanFog"
{

// Note this doesnt work unless at least one directional light casts shadows?
// My own implementation of fog that has blend modes, and is extensible...
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FogColor ("FogColor",Color) = (1,1,1,1)
		_FinalColor ("FinalColor",Color) = (1,1,1,1)
		_FinalColorStart("FinalColorStart",float) = 0.7
		_FinalColorEnd("FinalColorEnd",float) = 0.9
		_UseScreen ("UseScreen",Int) = 0
		_UseAdd ("UseAdd",Int) = 0
		_UseMultiply ("UseMultiply",Int) = 0
		_FogFar ("FogFar",float) = 0
		_FogNear ("FogNear",float) = 0
		_FogLinear ("FogLinear",Int) = 0
		_FogExp ("FogExp",Int) = 0
		_FogExp2 ("FogExp2",Int) = 0
		_Density ("Density",float) = 0
		_MaxDistance("MaxDistance",float) = 800
		_Intensity ("Intensity",float) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		
		Pass
		{

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
				float4 scrPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				// Shorthand for mul(UNITY_MATRIX_MVP,v.vertex)
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.scrPos = ComputeScreenPos(o.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			uniform int _UseScreen;
			uniform int _UseAdd;
			uniform int _UseMultiply;
			uniform float _FogNear;
			uniform float _FogFar;
			uniform int _FogLinear;
			uniform int _FogExp;
			uniform int _FogExp2;
			uniform float _Density;
			uniform float _MaxDistance;
			uniform float _Intensity;
			uniform float4 _FinalColor;
			uniform float _FinalColorStart;
			uniform float _FinalColorEnd;

			// what the hell is SV_Target?
			fixed4 frag (v2f i) : SV_Target
			{
				float depthValue = Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
				if (depthValue == 1) depthValue = 0;
				// Skybox should have no fog

				float camRange = _MaxDistance - 0.4; // Far - near
				float fogCoord = 0.4 + depthValue*camRange;

				// 1 is far, 0 is near
				// Use bullshit equation here
				fixed4 col = tex2D(_MainTex, i.uv);

				float factor = 1;
				factor = (_FogFar - fogCoord) / (_FogFar-_FogNear);
				// Linear fog computation:
				// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
				//float factor = i.fogCoord * unity_FogParams.z + unity_FogParams.w;

				if (_FogLinear == 1) factor = (_FogFar - fogCoord) / (_FogFar-_FogNear);
					// density / ln(2)
				if (_FogExp == 1) factor =   exp2(-(_Density/.693) * fogCoord);
				if (_FogExp2 == 1) factor = exp2(-(_Density/.833) * fogCoord*(_Density/.833) * fogCoord);
			
			
				factor = _Intensity*saturate(1-factor);
				fixed4 one = 1;
				// 0 to 1, 0 = no fog
				if (_UseAdd == 1) {
					col = col + factor*unity_FogColor;
				} else if (_UseMultiply == 1) {
					col = lerp(col,col * unity_FogColor,factor);
				} else if (_UseScreen == 1) {
					//1 - (1-Target) * (1-Blend)  
					col = lerp(col,one - (one - col) * (one - unity_FogColor),factor);
				} else  {
					col.rgb = lerp(col.rgb,unity_FogColor.rgb,factor);
				} 
				col.a = 1;
				if (depthValue > _FinalColorStart) {
					if (depthValue > _FinalColorEnd) depthValue = _FinalColorEnd;
					col.rgb = lerp(col.rgb,_FinalColor.rgb,(depthValue-_FinalColorStart)/(_FinalColorEnd-_FinalColorStart));
				}
				// Use that to blend the fog color onto the pixel color according to blendmode setting

				// unity Fogcolor is builtin		
				//col.rgb = unity_FogColor.rgb;
				return col;
			}
			ENDCG
		}
	}
}

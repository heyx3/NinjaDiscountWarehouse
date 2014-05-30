//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Filter for lens distortion correction and chromatic aberration
//
//=============================================================================

Shader "Custom/SteamVR_Distort" {
	Properties {
		_MainTex ("Main", 2D) = "white" {}
		_Overlay ("Overlay", 2D) = "black" {}
	}

	// Shader code pasted into all further CGPROGRAM blocks
	CGINCLUDE

	#include "UnityCG.cginc"

	sampler2D _MainTex;
	sampler2D _Overlay;

	CBUFFER_START(Raytrace)
		uniform float4x4 invProj;
		uniform float4x4 rot;
		uniform float4 P0;
		uniform float4 coef;	// CURVED x: 2.0 * theta, y: aspect / scale, z: aspect
								// FLAT x: 1 / scale, y: aspect / scale, z: distance
		uniform float4 uvOffset;
		uniform float alpha;
		uniform float weight;
	CBUFFER_END

	float4 cylinder(float2 uv)
	{
		float4 ndc = float4(2.0f * uv.x - 1.0f, 2.0f * uv.y - 1.0f, 0, 1);
		ndc = mul(invProj, ndc); // generate a ray from normalized device coordinates
		ndc /= ndc.w; ndc.z *= -1.0f; ndc.w = 0.0f; // unity z is flipped
		float4 V = normalize(mul(rot, ndc));

		float a = dot(V.xz, V.xz);
		float b = 2.0f * dot(V.xz, P0.xz);
		float c = dot(P0.xz, P0.xz) - P0.w; // r^2 in P0.w

		float opacity = 0.0f;

		float det = b * b - 4.0f * a * c;
		if (det > 0.0f)
		{
			float t = (sqrt(det) - b) / (2.0f * a);
			if (t > 0.0f)
			{
				float3 P = P0.xyz + V.xyz * t;
				P.x = atan2(P.x, P.z) / coef.x;
				P.y *= coef.y;

				uv = P.xy + 0.5f;
				uv += uvOffset.xy;
				uv *= uvOffset.zw;

				float2 edge = abs(uv - 0.5f) > 0.5f;
				float bg = dot(edge,edge);
				opacity = alpha * saturate(1-bg);
			}
		}

		float4 color = tex2D(_Overlay, uv);
		color.a *= opacity;
		return color;
	}

	float4 plane(float2 uv)
	{
		float4 ndc = float4(2.0f * uv.x - 1.0f, 2.0f * uv.y - 1.0f, 0, 1);
		ndc = mul(invProj, ndc); // generate a ray from normalized device coordinates
		ndc /= ndc.w; ndc.z *= -1.0f; ndc.w = 0.0f; // unity z is flipped
		float4 V = normalize(mul(rot, ndc));

		float opacity = 0.0f;

		float t = -P0.z / V.z; // z-aligned plane at origin
		if (t > 0.0f)
		{
			float3 P = P0.xyz + V.xyz * t;
			P.x *= coef.x;
			P.y *= coef.y;

			uv = P.xy + 0.5f;
			uv += uvOffset.xy;
			uv *= uvOffset.zw;

			float2 edge = abs(uv - 0.5f) > 0.5f;
			float bg = dot(edge,edge);
			opacity = alpha * saturate(1-bg);
		}

		float4 color = tex2D(_Overlay, uv);
		color.a *= opacity;
		return color;
	}

	struct VS_OUTPUT {
		float4 position : SV_POSITION;
		float2 uvr : TEXCOORD0;
		float2 uvg : TEXCOORD1;
		float2 uvb : TEXCOORD2;
	};

	VS_OUTPUT VS(appdata_full v)
	{
		VS_OUTPUT o;
		o.position = mul(_Object2World, v.vertex);
		o.uvr = v.normal.xy;
		o.uvg = v.texcoord;
		o.uvb = v.texcoord1.xy;
		return o;
	}

	struct PS_OUTPUT {
		float4 color : COLOR;
	};
	
	PS_OUTPUT PS(VS_OUTPUT input)
	{
		PS_OUTPUT o;

		o.color.r = tex2D( _MainTex, input.uvr ).r;
		o.color.g = tex2D( _MainTex, input.uvg ).g;
		o.color.b = tex2D( _MainTex, input.uvb ).b;
		o.color.a = weight;

		float2 threshold = 0.05f; // fade edges over last five percent
		float2 edge = saturate(abs(input.uvg - 0.5f) + threshold - 0.5f) / threshold;
		o.color.xyz = lerp( o.color.xyz, float3(0,0,0), dot(edge,edge) );
		return o;
	}

	PS_OUTPUT PS_FLAT(VS_OUTPUT input)
	{
		PS_OUTPUT o;

		float4 rsamp = plane( input.uvr );
		float4 gsamp = plane( input.uvg );
		float4 bsamp = plane( input.uvb );

		o.color.r = lerp( tex2D( _MainTex, input.uvr ).r, rsamp.r, rsamp.a );
		o.color.g = lerp( tex2D( _MainTex, input.uvg ).g, gsamp.g, gsamp.a );
		o.color.b = lerp( tex2D( _MainTex, input.uvb ).b, bsamp.b, bsamp.a );
		o.color.a = weight;

		float2 threshold = 0.05f; // fade edges over last five percent
		float2 edge = saturate(abs(input.uvg - 0.5f) + threshold - 0.5f) / threshold;
		o.color.xyz = lerp( o.color.xyz, float3(0,0,0), dot(edge,edge) );
		return o;
	}

	PS_OUTPUT PS_CURVED(VS_OUTPUT input)
	{
		PS_OUTPUT o;

		float4 rsamp = cylinder( input.uvr );
		float4 gsamp = cylinder( input.uvg );
		float4 bsamp = cylinder( input.uvb );

		o.color.r = lerp( tex2D( _MainTex, input.uvr ).r, rsamp.r, rsamp.a );
		o.color.g = lerp( tex2D( _MainTex, input.uvg ).g, gsamp.g, gsamp.a );
		o.color.b = lerp( tex2D( _MainTex, input.uvb ).b, bsamp.b, bsamp.a );
		o.color.a = weight;

		float2 threshold = 0.05f; // fade edges over last five percent
		float2 edge = saturate(abs(input.uvg - 0.5f) + threshold - 0.5f) / threshold;
		o.color.xyz = lerp( o.color.xyz, float3(0,0,0), dot(edge,edge) );
		return o;
	}

	ENDCG

	SubShader {
		Pass {
			Name "0: Curved overlay with AA"
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex VS
			#pragma fragment PS_CURVED
			ENDCG
		}
		Pass {
			Name "1: Curved overlay no AA"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex VS
			#pragma fragment PS_CURVED
			ENDCG
		}
		Pass {
			Name "2: Flat overlay with AA"
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex VS
			#pragma fragment PS_FLAT
			ENDCG
		}
		Pass {
			Name "3: Flat overlay no AA"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex VS
			#pragma fragment PS_FLAT
			ENDCG
		}
		Pass {
			Name "4: No overlay with AA"
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex VS
			#pragma fragment PS
			ENDCG
		}
		Pass {
			Name "5: No overlay no AA"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex VS
			#pragma fragment PS
			ENDCG
		}
	}
	Fallback Off
}

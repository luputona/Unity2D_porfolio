﻿Shader "AnyPortrait/Transparent/Colored Texture (2X) SoftAdditive"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)
		_MainTex("Main Texture (RGBA)", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend OneMinusDstColor One//Soft Add
		//Cull Off//<<?

		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf SimpleColor alpha
		#pragma surface surf SimpleColor//AlphaBlend가 아닌 경우

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		half4 LightingSimpleColor(SurfaceOutput s, half3 lightDir, half atten)
		{
			half4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		half4 _Color;
		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			c.rgb *= _Color.rgb * 2.0f;

			o.Alpha = c.a * _Color.a;
			
			//Additive라면 RGB * Alpha를 Albedo에 넣어야한다.
			//o.Albedo = c.rgb;
			o.Albedo = c.rgb * o.Alpha;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

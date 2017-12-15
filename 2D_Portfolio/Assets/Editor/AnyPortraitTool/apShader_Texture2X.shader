Shader "AnyPortrait/Editor/Colored Texture (2X)" {
	Properties{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)
		_MainTex("Albedo (RGBA)", 2D) = "white" {}
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One//Add
		//Blend OneMinusDstColor One//Soft Add
		//Blend DstColor Zero//Multiply
		//Blend DstColor SrcColor//2X Multiply
		//Cull Off

		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf SimpleColor alpha //<<AlphaBlend인 경우
		//#pragma surface surf SimpleColor//AlphaBlend가 아닌 경우

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
		float4 _ScreenSize;

		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
			float4 screenPos;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			screenUV.y = 1.0f - screenUV.y;

			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			c.rgb *= _Color.rgb * 2.0f;
			c.rgb *= IN.color;

			
			o.Alpha = c.a * _Color.a;
			

			
			if (screenUV.x < _ScreenSize.x || screenUV.x > _ScreenSize.z)
			{
				o.Alpha = 0;
				discard;
			}
			if (screenUV.y < _ScreenSize.y || screenUV.y > _ScreenSize.w)
			{
				o.Alpha = 0;
				discard;
			}
			o.Albedo = c.rgb;
		}
		ENDCG
		}
		FallBack "Diffuse"
}

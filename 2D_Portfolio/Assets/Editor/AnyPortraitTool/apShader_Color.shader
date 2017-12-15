Shader "AnyPortrait/Editor/Color" {
	Properties{
		_Color("1X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One
		//Cull Off

		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf SimpleColor alpha

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
		float4 _ScreenSize;

		struct Input
		{
			//float2 uv_MainTex;
			float4 color : COLOR;
			float4 screenPos;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{
			//half4 c = tex2D(_MainTex, IN.uv_MainTex);
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			
			screenUV.y = 1.0f - screenUV.y;
			//float2 screenUV = IN.screenPos.xy;

			half4 c = IN.color;
			c.rgb *= _Color.rgb * 1.0f;

			

			o.Albedo = c.rgb;
			//o.Alpha = c.a * _Color.a * pow(IN.color.a, 0.1);
			o.Alpha = c.a * _Color.a;

			
			if (screenUV.x < _ScreenSize.x || screenUV.x > _ScreenSize.z)
			{
				o.Alpha = 0;
			}
			if (screenUV.y < _ScreenSize.y || screenUV.y > _ScreenSize.w)
			{
				o.Alpha = 0;
			}
			
		}
		ENDCG
	}
	FallBack "Diffuse"
}

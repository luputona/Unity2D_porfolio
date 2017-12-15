// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AnyPortrait/Editor/Masked Colored Texture (2X) Additive"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)
		_MainTex("Base Texture (RGBA)", 2D) = "white" {}
		_ClipTexture1("Clipped Texture 1 (RGBA)", 2D) = "clear" {}
		_ClipTexture2("Clipped Texture 2 (RGBA)", 2D) = "clear" {}
		_ClipTexture3("Clipped Texture 3 (RGBA)", 2D) = "clear" {}
		_Color1("2X Color 1 (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)
		_Color2("2X Color 2 (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)
		_Color3("2X Color 3 (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)
		_MaskRenderTexture("Mask Render Texture", 2D) = "clear" {}
		_vColorITP("Vertex Color Ratio (0~1)", Range(0, 1)) = 0
		_BlendOpt1("Blend Option 1 (Alpha,Add,Soft,Mul)", Color) = (1, 0, 0, 0)
		_BlendOpt2("Blend Option 2 (Alpha,Add,Soft,Mul)", Color) = (1, 0, 0, 0)
		_BlendOpt3("Blend Option 3 (Alpha,Add,Soft,Mul)", Color) = (1, 0, 0, 0)
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		//Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One//Additive

		LOD 500

		//----------------------------------------------------------------------------------
		//Pass 1
		//Stencil의 값만 넘기고 유효한 렌더링이 없는 단계
		//Base가 되는 Texture의 Alpha값(AlphaTest)이 Stencil 작성 여부가 된다.
		Pass
		{
			//ColorMask 0
			ZWrite off//<이 단계에서는 Zwrite를 하지 않는다.
			ZTest Always
			Lighting Off
			Cull Off

			Blend SrcAlpha OneMinusSrcAlpha

			//Stencil : "Z 테스트만 된다면 특정 값(53)을 저장해두자"
			stencil
			{
				ref 53
				comp Always
				pass replace
				fail zero
				zfail zero
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct vertexInput
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				/*float3 vertColor_Black : TEXCOORD1;
				float3 vertColor_Red : TEXCOORD2;*/
			};

			struct vertexOutput
			{
				float4 pos : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			half4 _Color;
			sampler2D _MainTex;

			float4 _MainTex_ST;

			float4 _ScreenSize;

			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(IN.vertex);
				o.color = IN.color;
				o.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				float itp_black = saturate(1.0f - (IN.color.r + IN.color.g + IN.color.b));
				half4 c = tex2D(_MainTex, IN.uv);
				//c.rgb *= 1;
				
				c.a *= _Color.a;
				c.a = (c.a * itp_black);
				
				float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
				screenUV.y = 1.0f - screenUV.y;

				if (screenUV.x < _ScreenSize.x || screenUV.x > _ScreenSize.z)
				{
					c.a = 0;
				}
				if (screenUV.y < _ScreenSize.y || screenUV.y > _ScreenSize.w)
				{
					c.a = 0;
				}
				//c.rgb = 0;
				//c.r = screenUV.x;
				//c.b = screenUV.y;
				if (c.a < 0.5f)
				{
					c.a = 0;
					//discard;
				}
				
				return c;
			}
			ENDCG
		}


		//----------------------------------------------------------------------------------
		//Pass 2
		//입력된 Stencil이 맞다면 이제 렌더링을 하자
		//기본이 되는 Base Texture를 포함해서 최대 4개의 텍스쳐를 렌더링 할 수 있다.
		//이미지의 보간은 VertexColor를 이용한다.(Black, R, G, B)
		Pass
		{
			ColorMask RGB
			ZWrite off
			//ZTest Always
			//Cull Off

			LOD 200

			//Blend SrcAlpha OneMinusSrcAlpha
			Blend One One//Additive

			//Stencil : "Alpha값이 저장된 Stencil 조건에 맞으면 렌더링을 하고, 버퍼는 그대로 둔다."
			//stencil
			//{
			//	ref 53
			//	//comp equal
			//	comp always
			//	//pass zero
			//	//fail zero
			//	//zfail zero
			//}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct vertexInput
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				float3 vertColor : TEXCOORD1;
				//float3 vertColor_Z : TEXCOORD1;
				//float3 vertColor_R : TEXCOORD2;
				//float3 vertColor_G : TEXCOORD3;
				//float3 vertColor_B : TEXCOORD4;
				/*float3 vertColor_Z : TEXCOORD1;
				float3 vertColor_R : TEXCOORD2;
				float3 vertColor_G : TEXCOORD3;
				float3 vertColor_B : TEXCOORD4;*/
			};

			struct vertexOutput
			{
				float4 pos : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
				float4 screenPos : TEXCOORD4;
				float4 worldPos : TEXCOORD5;
				float3 vertColor : TEXCOORD6;
				
			};

			half4 _Color;
			sampler2D _MainTex;

			sampler2D _ClipTexture1;
			sampler2D _ClipTexture2;
			sampler2D _ClipTexture3;

			half4 _Color1;
			half4 _Color2;
			half4 _Color3;

			float4 _MainTex_ST;
			float4 _ClipTexture1_ST;
			float4 _ClipTexture2_ST;
			float4 _ClipTexture3_ST;

			sampler2D _MaskRenderTexture;
			
			float4 _BlendOpt1;
			float4 _BlendOpt2;
			float4 _BlendOpt3;

			float _vColorITP;

			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(IN.vertex);

				/*float itp_black = saturate(1.0f - (IN.color.r + IN.color.g + IN.color.b));
				float itp_r = saturate(IN.color.r);
				float itp_g = saturate(IN.color.g);
				float itp_b = saturate(IN.color.b);*/

				o.color = IN.color;

				o.vertColor = IN.vertColor;
				//o.vertColor = (itp_black * IN.vertColor_Z) + (itp_r * IN.vertColor_R) + (itp_g * IN.vertColor_G) + (itp_b * IN.vertColor_B);

				o.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
				o.uv1 = TRANSFORM_TEX(IN.texcoord, _ClipTexture1);
				o.uv2 = TRANSFORM_TEX(IN.texcoord, _ClipTexture2);
				o.uv3 = TRANSFORM_TEX(IN.texcoord, _ClipTexture3);
				o.screenPos = ComputeScreenPos(o.pos);
				o.worldPos = o.pos;
				return o;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				float itp_black = saturate(1.0f - (IN.color.r + IN.color.g + IN.color.b));
				float itp_r = saturate(IN.color.r);
				float itp_g = saturate(IN.color.g);
				float itp_b = saturate(IN.color.b);

				half4 c_base = tex2D(_MainTex, IN.uv);
				half4 c_r = tex2D(_ClipTexture1, IN.uv1);
				half4 c_g = tex2D(_ClipTexture2, IN.uv2);
				half4 c_b = tex2D(_ClipTexture3, IN.uv3);

				float2 screenUV = IN.screenPos.xy;// *_ScreenParams.xy;
				//float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
				//screenUV.y = 1.0f - screenUV.y;

				half4 maskTexture = tex2D(_MaskRenderTexture, screenUV);

				//float vertItp = (_vColorITP_0 * itp_black) + (_vColorITP_1 * itp_r) + (_vColorITP_2 * itp_g) + (_vColorITP_3 * itp_b);

				half4 c = half4(0, 0, 0, 0);
				c_base = maskTexture;
				c_base.rgb *= _Color.rgb * 2;
				c_r.rgb *= _Color1.rgb * 2;
				c_g.rgb *= _Color2.rgb * 2;
				c_b.rgb *= _Color3.rgb * 2;

				//c_base.rgb *= IN.vertColor_Black;
				//c_r.rgb *= IN.vertColor_Red;
				//c_g.rgb *= _Color2.rgb * 2;
				//c_b.rgb *= _Color3.rgb * 2;

				c_base.a *= itp_black * _Color.a;
				c_r.a *= itp_r * _Color1.a;
				c_g.a *= itp_g * _Color2.a;
				c_b.a *= itp_b * _Color3.a;

				c = c_base;
				
				//Blend Option : Alpha, Add, Soft, Mul
				c.rgb = (c.rgb * (1.0f - c_r.a) + (c_r.rgb * c_r.a)) * _BlendOpt1.r +
					(c.rgb + (c_r.rgb * c_r.a)) * _BlendOpt1.g +
					(c.rgb + (c_r.rgb * c_r.a * (1.0f - c.rgb))) * _BlendOpt1.b +
					(c.rgb * (c_r.rgb * c_r.a + 0.5f * (1.0f - c_r.a)) * 2) * _BlendOpt1.a;


				//c.rgb = c.rgb * (1.0f - c_g.a) + (c_g.rgb * c_g.a);
				c.rgb = (c.rgb * (1.0f - c_g.a) + (c_g.rgb * c_g.a)) * _BlendOpt2.r +
					(c.rgb + (c_g.rgb * c_g.a)) * _BlendOpt2.g +
					(c.rgb + (c_g.rgb * c_g.a * (1.0f - c.rgb))) * _BlendOpt2.b +
					(c.rgb * (c_g.rgb * c_g.a + 0.5f * (1.0f - c_g.a)) * 2) * _BlendOpt2.a;


				//c.rgb = c.rgb * (1.0f - c_b.a) + (c_b.rgb * c_b.a);
				c.rgb = (c.rgb * (1.0f - c_b.a) + (c_b.rgb * c_b.a)) * _BlendOpt3.r +
					(c.rgb + (c_b.rgb * c_b.a)) * _BlendOpt3.g +
					(c.rgb + (c_b.rgb * c_b.a * (1.0f - c.rgb))) * _BlendOpt3.b +
					(c.rgb * (c_b.rgb * c_b.a + 0.5f * (1.0f - c_b.a)) * 2) * _BlendOpt3.a;
				
				
				c.rgb += IN.vertColor;
				c.rgb = IN.vertColor * _vColorITP + c.rgb * (1.0f - _vColorITP);

				//c.b = 1.0f;
				c.a = saturate(c_base.a + c_r.a + c_g.a + c_b.a) * maskTexture.a;
				
				//Additive라면..
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}

		//////----------------------------------------------------------------------------------
		////Pass 3
		////멀티 패스 렌더링에서 사용했던 Stencil을 초기화한다.
		////이 단계가 없으면, 동일한 Shader를 사용하는 객체끼리 영향을 주게된다.
		////(같은 Stencil 값을 공유하므로..)
		////렌더링에 영향을 주지 않도록 색상은 Clear로 둔다.
		//Pass
		//{
		//	ColorMask 0
		//	Lighting Off
		//	Cull Off

		//	//Stencil : "지금껏 처리되었던 Buffer를 Zero로 만든다."
		//	stencil
		//	{
		//		ref 53
		//		comp equal
		//		pass zero
		//		fail zero
		//		//zfail zero
		//	}

		//	CGPROGRAM
		//	#pragma vertex vert
		//	#pragma fragment frag
		//	#pragma target 3.0

		//	#include "UnityCG.cginc"

		//	//불필요한 변수는 다 빼자
		//	struct vertexInput
		//	{
		//		float4 vertex : POSITION;
		//	};

		//	struct vertexOutput
		//	{
		//		float4 pos : POSITION;
		//	};

		//	vertexOutput vert(vertexInput IN)
		//	{
		//		vertexOutput o;
		//		o.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
		//		return o;
		//	}

		//	half4 frag(vertexOutput IN) : COLOR
		//	{
		//		return half4(0, 0, 0, 0);
		//	}
		//	ENDCG
		//}
	}
	FallBack "Diffuse"
}

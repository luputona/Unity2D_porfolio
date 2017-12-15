// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AnyPortrait/Transparent/Clipped Texture (2X) SoftAdditive"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)
		_MainTex("Base Texture (RGBA)", 2D) = "white" {}
		_MaskColor("Mask Color (A)", Color) = (0.5, 0.5, 0.5, 1.0)
		_MaskTex("Mask Texture 1 (A)", 2D) = "clear" {}
	}
	
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		//Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One//Add
		//Blend OneMinusDstColor One//Soft Add
		//Blend DstColor Zero//Multiply
		//Blend DstColor SrcColor//2X Multiply

		LOD 200

		//----------------------------------------------------------------------------------
		//Pass 1
		//Stencil의 값만 넘기고 유효한 렌더링이 없는 단계
		//Base가 되는 Texture의 Alpha값(AlphaTest)이 Stencil 작성 여부가 된다.
		Pass
		{	
			ColorMask 0
			ZWrite off//<이 단계에서는 Zwrite를 하지 않는다.
			Lighting Off
			//Cull Off

			Blend SrcAlpha OneMinusSrcAlpha

			//Stencil : "Z 테스트만 된다면 특정 값(53)을 저장해두자"
			stencil
			{	
				ref 53
				comp Always
				pass Replace
				fail zero
				zfail zero
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			//VertexColor가 R=1인 경우는 Mask, 기본은 0이다.
			struct vertexInput
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 texcoord : TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 pos : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float2 uv_mask : TEXCOORD1;
			};

			
			half4 _Color;
			sampler2D _MainTex;

			half4 _MaskColor;
			sampler2D _MaskTex;

			float4 _MainTex_ST;
			float4 _MaskTex_ST;

			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(IN.vertex);
				o.color = IN.color;
				half maskItp = o.color.r;
				o.pos.z = 1 - IN.color.r;

				o.uv = TRANSFORM_TEX(IN.texcoord, _MaskTex);
				o.uv_mask = TRANSFORM_TEX(IN.texcoord, _MainTex);
				return o;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				float maskItp = IN.color.r;
				half4 c_mask = tex2D(_MaskTex, IN.uv_mask);
				half4 c_main = tex2D(_MainTex, IN.uv);

				half4 c = c_mask * maskItp + c_main * (1 - maskItp);
				//half4 c = c_mask * c_main;

				if (maskItp < 0.5f)
				{
					discard;
				}

				c.rgb *= 0;
				//c.a *= _MaskColor.a * maskItp;

				if (c.a < 0.1f)
				{
					discard;
				}

				//discard;

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
			ZWrite off
			//Cull Off

			LOD 200
			Blend OneMinusDstColor One//Soft Add

			//Stencil : "Alpha값이 저장된 Stencil 조건에 맞으면 렌더링을 하고, 버퍼는 그대로 둔다."
			stencil
			{
				ref 53
				comp Equal
				//pass keep
				//fail keep
				//zfail keep
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
			};

			struct vertexOutput
			{
				float4 pos : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			half4 _Color;
			sampler2D _MainTex;

			half4 _MaskColor;
			sampler2D _MaskTex;
			
			float4 _MainTex_ST;
			float4 _MaskTex_ST;

			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(IN.vertex);
				o.color = IN.color;
				o.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);

				o.pos.z = IN.color.r;
				return o;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv);

				//c.rgb = IN.color;
				//c.a = IN.color.a;
				
				c.rgb *= _Color.rgb * 2;
				c.a *= _Color.a * _MaskColor.a;

				//Additive인 경우
				c.rgb *= c.a;

				//Multiply인 경우
				//c.rgb = c.rgb * (c.a) + float4(0.5f, 0.5f, 0.5f, 1.0f) * (1.0f - c.a);

				if (IN.color.r > 0.2f)
				{
					c.rgb = 0;
					c.a = 0;
				}

				return c;
			}
			ENDCG
		}

		//----------------------------------------------------------------------------------
		//Pass 3
		//멀티 패스 렌더링에서 사용했던 Stencil을 초기화한다.
		//이 단계가 없으면, 동일한 Shader를 사용하는 객체끼리 영향을 주게된다.
		//(같은 Stencil 값을 공유하므로..)
		//렌더링에 영향을 주지 않도록 색상은 Clear로 둔다.
		Pass
		{
			ColorMask 0
			Lighting Off
			//Cull Off

			Blend SrcAlpha OneMinusSrcAlpha

			//Stencil : "지금껏 처리되었던 Buffer를 Zero로 만든다."
			stencil
			{
				ref 53
				comp equal
				pass zero
				fail zero
				//zfail zero
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			//불필요한 변수는 다 빼자
			struct vertexInput
			{
				float4 vertex : POSITION;
			};

			struct vertexOutput
			{
				float4 pos : POSITION;
			};

			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(IN.vertex);
				return o;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				discard;
				return half4(0, 0, 0, 0);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}

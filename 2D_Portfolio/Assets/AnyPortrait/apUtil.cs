/*
*	Copyright (c) 2017. RainyRizzle. All rights reserved
*	https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public static class apUtil
	{

		public static List<T> ResizeList<T>(List<T> srcList, int resizeSize)
		{
			if (resizeSize < 0)
			{
				return null;
			}
			List<T> resultList = new List<T>();
			for (int i = 0; i < resizeSize; i++)
			{
				if (i < srcList.Count)
				{
					resultList.Add(srcList[i]);
				}
				else
				{
					resultList.Add(default(T));
				}
			}

			return resultList;

		}

		// 색상 처리
		//------------------------------------------------------------------------------------------
		public static Color BlendColor_ITP(Color prevResult, Color nextResult, float nextWeight)
		{
			return (prevResult * (1.0f - nextWeight)) + (nextResult * nextWeight);
		}

		//public static Vector3 _color_2XTmp_Prev = new Vector3(0, 0, 0);
		//public static Vector3 _color_2XTmp_Next = new Vector3(0, 0, 0);

		public static Color BlendColor_Add(Color prevResult, Color nextResult, float nextWeight)
		{
			//_color_2XTmp_Prev.x = (float)(prevResult.r);
			//_color_2XTmp_Prev.y = (float)(prevResult.g);
			//_color_2XTmp_Prev.z = (float)(prevResult.b);

			//_color_2XTmp_Next.x = (float)(nextResult.r);
			//_color_2XTmp_Next.y = (float)(nextResult.g);
			//_color_2XTmp_Next.z = (float)(nextResult.b);

			//_color_2XTmp_Prev += (_color_2XTmp_Next * nextWeight);
			//_color_2XTmp_Next = _color_2XTmp_Prev * (1.0f - nextWeight) + ((_color_2XTmp_Prev + _color_2XTmp_Next) * nextWeight);



			//return new Color(	Mathf.Clamp01(_color_2XTmp_Prev.x + 0.5f),
			//					Mathf.Clamp01(_color_2XTmp_Prev.y + 0.5f),
			//					Mathf.Clamp01(_color_2XTmp_Prev.z + 0.5f),
			//					//Mathf.Clamp01(prevResult.a + (nextResult.a * nextWeight))
			//					Mathf.Clamp01(prevResult.a * (1.0f - nextWeight) + (prevResult.a * nextResult.a) * nextWeight)
			//				);

			//return new Color(	Mathf.Clamp01(_color_2XTmp_Next.x),
			//					Mathf.Clamp01(_color_2XTmp_Next.y),
			//					Mathf.Clamp01(_color_2XTmp_Next.z),
			//					//Mathf.Clamp01(prevResult.a + (nextResult.a * nextWeight))
			//					Mathf.Clamp01(prevResult.a * (1.0f - nextWeight) + (prevResult.a * nextResult.a) * nextWeight)
			//				);

			//return prevResult + (nextResult * nextWeight);

			nextResult.r = prevResult.r * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.r + nextResult.r - 0.5f) * nextWeight);
			nextResult.g = prevResult.g * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.g + nextResult.g - 0.5f) * nextWeight);
			nextResult.b = prevResult.b * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.b + nextResult.b - 0.5f) * nextWeight);
			//nextResult.a = prevResult.a * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.a + nextResult.a - 0.5f) * nextWeight);
			nextResult.a = prevResult.a * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.a * nextResult.a) * nextWeight);//Alpha는 Multiply 연산



			return nextResult;
		}
	}

}
﻿/*
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

	/// <summary>
	/// 보간을 위한 영역 값
	/// 2차원의 값을 가진다. (1차원은 굳이 영역 없이도 처리 가능하다)
	/// LT : XY의 Min 값, RT : XY의 Max값
	/// </summary>
	public class apOptCalculatedLerpArea
	{
		// Members
		//-----------------------------------------------
		public apOptCalculatedLerpPoint _pointLT, _pointRT, _pointLB, _pointRB;
		public Vector2 _posLT = Vector2.zero, _posRB = Vector2.zero;

		// Init
		//-----------------------------------------------
		public apOptCalculatedLerpArea(apOptCalculatedLerpPoint pointLT,
										apOptCalculatedLerpPoint pointRT,
										apOptCalculatedLerpPoint pointLB,
										apOptCalculatedLerpPoint pointRB)
		{
			_pointLT = pointLT;
			_pointRT = pointRT;
			_pointLB = pointLB;
			_pointRB = pointRB;

			SetRangeVector2(_pointLT._pos, _pointRB._pos);
		}

		public void SetRangeVector2(Vector2 posLT, Vector2 posRB)
		{
			_posLT = posLT;
			_posRB = posRB;
		}


		// Functions
		//-----------------------------------------------
		public void ReadyToCalculate()
		{
			_pointLT._calculatedWeight = 0.0f;
			_pointRT._calculatedWeight = 0.0f;
			_pointLB._calculatedWeight = 0.0f;
			_pointRB._calculatedWeight = 0.0f;
		}



		// Get / Set
		//-----------------------------------------------
		public bool IsInclude(Vector2 pos)
		{
			if (pos.x < _posLT.x || pos.x > _posRB.x ||
				pos.y < _posLT.y || pos.y > _posRB.y)
			{
				return false;
			}
			return true;
		}

	}

}
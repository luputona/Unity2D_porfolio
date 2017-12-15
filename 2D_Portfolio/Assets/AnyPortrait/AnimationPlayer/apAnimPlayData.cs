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

	/// <summary>
	/// AnimPlay에서 재생하기 위해 AnimClip (string 이름), MeshGroup + RootUnit의 세트를 저장한다.
	/// 직렬화가 되는 데이터와 그렇지 않은 데이터가 있어서 링크 작업 필요
	/// 플레이 참조시 사용된다. (일일이 검색하지 않도록 만듬)
	/// 이 데이터를 참조로 AnimPlayUnit이 생성된다.
	/// </summary>
	[Serializable]
	public class apAnimPlayData
	{
		// Members
		//----------------------------------------------
		[SerializeField]
		public int _animClipID = -1;

		[NonSerialized]
		public apAnimClip _linkedAnimClip = null;

		[SerializeField]
		public string _animClipName = "";


		[SerializeField]
		public int _meshGroupID = -1;

		[NonSerialized]
		public apOptRootUnit _linkedOptRootUnit = null;

		[NonSerialized]
		public bool _isValid = false;




		// Init
		//----------------------------------------------
		public apAnimPlayData(int animClipID, int meshGroupID, string animClipName)
		{
			_animClipID = animClipID;

			_meshGroupID = meshGroupID;

			_animClipName = animClipName;
		}

		public void Link(apAnimClip animClip, apOptRootUnit optRootUnit)
		{
			_linkedAnimClip = animClip;
			_linkedOptRootUnit = optRootUnit;
			_isValid = true;
		}


		// Functions
		//----------------------------------------------
	}

}
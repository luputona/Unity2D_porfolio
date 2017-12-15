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
	/// apMesh를 만들기 위한 데이터
	/// 기본적인 Vertex 정보 + 재질과 텍스쳐 정보
	/// + 연결된 본 정보가 포함되어 있다.
	/// 저장을 위한 정보 - Raw Data 형식으로 저장된다.
	/// </summary>
	public class apMeshData
	{
		// Members
		//-------------------------------------
		//기본 정보들
		public string _meshName = "";
		public string _texturePath = "";
		public string _materialPath = "";

		//apVertex 정보들
		public List<string> _vertexData = new List<string>();

		// Init
		//-------------------------------------



		// Functions
		//-------------------------------------
	}

}
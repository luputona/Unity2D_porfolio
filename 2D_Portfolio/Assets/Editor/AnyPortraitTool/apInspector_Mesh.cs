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
using UnityEditor;



//[CustomEditor(typeof(apMesh))]
//public class apInspector_Mesh : Editor
//{
//	private apMesh _targetMesh = null;
//	private apMesh _prevTargetMesh = null;

//	public override void OnInspectorGUI()
//	{
//		_targetMesh = target as apMesh;
//		if(_targetMesh != _prevTargetMesh)
//		{
//			Init();
//			_prevTargetMesh = _targetMesh;
//		}
		
//		if(GUILayout.Button("Make Sample Plane", GUILayout.Height(30)))
//		{
//			if (_targetMesh._portrait != null)
//			{
//				if (_targetMesh._vertexData == null || _targetMesh._indexBuffer == null)
//				{
//					List<apVertex> vertexData = new List<apVertex>();
//					List<int> indexBuffer = new List<int>();


//					// CW 방식
//					// 1		2(1, 1)
//					//
//					// 0(0, 0)	3
//					_targetMesh.RefreshVertexID();

//					vertexData.Add(new apVertex(0, _targetMesh._portrait.MakeUniqueID_Vertex(), new Vector3(-100, -100, 0), new Vector2(0, 0)));
//					vertexData.Add(new apVertex(1, _targetMesh._portrait.MakeUniqueID_Vertex(), new Vector3(-100, 100, 0), new Vector2(0, 1)));
//					vertexData.Add(new apVertex(2, _targetMesh._portrait.MakeUniqueID_Vertex(), new Vector3(100, 100, 0), new Vector2(1, 1)));
//					vertexData.Add(new apVertex(3, _targetMesh._portrait.MakeUniqueID_Vertex(), new Vector3(100, -100, 0), new Vector2(1, 0)));

//					indexBuffer.Add(0);
//					indexBuffer.Add(1);
//					indexBuffer.Add(3);

//					indexBuffer.Add(1);
//					indexBuffer.Add(2);
//					indexBuffer.Add(3);

//					_targetMesh.SetVertices(vertexData, indexBuffer);
//				}
//			}
//			_targetMesh.CheckAndMakeMesh(true);
			
//		}

//		base.OnInspectorGUI();

//	}

//	private void Init()
//	{

//	}
//}

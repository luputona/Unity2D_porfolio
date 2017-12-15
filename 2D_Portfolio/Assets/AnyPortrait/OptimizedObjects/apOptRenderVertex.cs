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

	[Serializable]
	public class apOptRenderVertex
	{
		// Members
		//----------------------------------------------
		//Parent MonoBehaviour
		public apOptTransform _parentTransform = null;
		public apOptMesh _parentMesh = null;


		//Vertex의 값에 해당하는 apVertex가 없으므로 바로 Index 접근을 한다.
		//기본 데이터
		public int _uniqueID = -1;
		public int _index;

		public Vector2 _pos_Local = Vector2.zero;
		//public Vector3 _pos3_Local = Vector3.zero;

		public Vector2 _uv = Vector2.zero;

		//업데이트 데이터
		public Vector3 _vertPos3_LocalUpdated = Vector3.zero;

		public Vector2 _vertPos_World = Vector2.zero;
		//public Vector3 _vertPos3_World = Vector3.zero;

		// Transform 데이터들
		//0. Rigging
		//리깅의 경우는 Additive없이 Weight, Pos로만 값을 가져온다.
		//레이어의 영향을 전혀 받지 않는다.
		public Vector2 _pos_Rigging = Vector2.zero;
		public float _weight_Rigging = 0.0f;//0이면 Vertex Pos를 사용, 1이면 posRigging을 사용한다. 기본값은 0


		//1. [Static] Vert -> Mesh (Pivot)
		[SerializeField]
		public apMatrix3x3 _matrix_Static_Vert2Mesh = apMatrix3x3.identity;

		[SerializeField]
		public apMatrix3x3 _matrix_Static_Vert2Mesh_Inverse = apMatrix3x3.identity;


		//2. [Cal] Vert Local - Blended
		public apMatrix3x3 _matrix_Cal_VertLocal = apMatrix3x3.identity;

		//3. [TF+Cal] 중첩된 Mesh/MeshGroup Transform
		public apMatrix3x3 _matrix_MeshTransform = apMatrix3x3.identity;

		//4. [Cal] Vert World - Blended
		public apMatrix3x3 _matrix_Cal_VertWorld = apMatrix3x3.identity;

		//private Vector2 _cal_VertWorld = Vector2.zero;

		// 계산 완료
		public apMatrix3x3 _matrix_ToWorld = apMatrix3x3.identity;
		//public apMatrix3x3 _matrix_ToVert = apMatrix3x3.identity;


		//계산 관련 변수
		private bool _isCalculated = false;


		//TODO : 물리 관련 지연 변수 추가 필요


		// Init
		//----------------------------------------------
		public apOptRenderVertex(apOptTransform parentTransform, apOptMesh parentMesh,
									int vertUniqueID, int vertIndex, Vector2 vertPosLocal,
									Vector2 vertUV)
		{
			_parentTransform = parentTransform;
			_parentMesh = parentMesh;
			_uniqueID = vertUniqueID;
			_index = vertIndex;
			_pos_Local = vertPosLocal;
			_uv = vertUV;


			//_pos3_Local = new Vector3(_pos_Local.x, _pos_Local.y, 0);
			//_pos3_Local.x = _pos_Local.x;
			//_pos3_Local.y = _pos_Local.y;
			//_pos3_Local.z = 0;

			_vertPos3_LocalUpdated.x = _pos_Local.x;
			_vertPos3_LocalUpdated.y = _pos_Local.y;
			_vertPos3_LocalUpdated.z = 0;

			_isCalculated = false;

			_pos_Rigging = Vector2.zero;
			_weight_Rigging = 0.0f;
		}

		// Functions
		//----------------------------------------------
		// 준비 + Matrix/Delta Pos 입력
		//---------------------------------------------------------
		public void ReadyToCalculate()
		{
			_matrix_Static_Vert2Mesh = apMatrix3x3.identity;
			_matrix_Static_Vert2Mesh_Inverse = apMatrix3x3.identity;

			_matrix_Cal_VertLocal = apMatrix3x3.identity;
			_matrix_MeshTransform = apMatrix3x3.identity;

			_matrix_Cal_VertWorld = apMatrix3x3.identity;
			_matrix_ToWorld = apMatrix3x3.identity;
			//_matrix_ToVert = apMatrix3x3.identity;
			_vertPos_World = Vector2.zero;

			//_cal_VertWorld = Vector2.zero;

			_vertPos3_LocalUpdated.x = _pos_Local.x;
			_vertPos3_LocalUpdated.y = _pos_Local.y;
			_vertPos3_LocalUpdated.z = 0;

			_pos_Rigging = Vector2.zero;
			_weight_Rigging = 0.0f;
		}

		public void SetRigging_0_LocalPosWeight(Vector2 posRiggingResult, float weight)
		{
			_pos_Rigging = posRiggingResult;
			_weight_Rigging = weight;
		}

		public void SetMatrix_1_Static_Vert2Mesh(apMatrix3x3 matrix_Vert2Local)
		{
			_matrix_Static_Vert2Mesh = matrix_Vert2Local;
			_matrix_Static_Vert2Mesh_Inverse = _matrix_Static_Vert2Mesh.inverse;
		}

		public void SetMatrix_2_Calculate_VertLocal(Vector2 deltaPos)
		{
			_matrix_Cal_VertLocal = apMatrix3x3.TRS(deltaPos, 0, Vector2.one);
		}

		public void SetMatrix_3_Transform_Mesh(apMatrix3x3 matrix_meshTransform)
		{
			_matrix_MeshTransform = matrix_meshTransform;
		}

		public void SetMatrix_4_Calculate_VertWorld(Vector2 deltaPos)
		{
			_matrix_Cal_VertWorld = apMatrix3x3.TRS(deltaPos, 0, Vector2.one);
			//_cal_VertWorld = deltaPos;
		}

		// Calculate
		//---------------------------------------------------------
		public void Calculate()
		{
			//역순으로 World Matrix를 계산하자
			_matrix_ToWorld = _matrix_Cal_VertWorld
							//* _matrix_TF_Cal_Parent 
							//* _matrix_Cal_Mesh 
							//* _matrix_TF_Mesh 
							* _matrix_MeshTransform
							* _matrix_Cal_VertLocal
							* _matrix_Static_Vert2Mesh;

			//_matrix_ToVert = _matrix_ToWorld.inverse;

			//이전 식
			//_vertPos3_World = _matrix_ToWorld.MultiplyPoint3x4(_pos3_Local);

			//리깅 포함한 식으로 변경
			//_weight_Rigging = Mathf.Clamp(_weight_Rigging, 0.0f, 0.5f);

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local * (1.0f - _weight_Rigging) + _pos_Rigging * _weight_Rigging);

			//_vertPos_World.x = _vertPos3_World.x;
			//_vertPos_World.y = _vertPos3_World.y;

			Vector2 posLocalUpdated = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			_vertPos3_LocalUpdated.x = posLocalUpdated.x;
			_vertPos3_LocalUpdated.y = posLocalUpdated.y;
			_vertPos3_LocalUpdated.z = 0;

			_isCalculated = true;
		}


		public void CalculateByComputeShader(ref apComputeShader.OutputVertexStruct_Opt outputVertex)
		{
			//_vertPos3_World = outputVertex._posWorld3;
			_vertPos_World = outputVertex._posWorld2;
			_vertPos3_LocalUpdated = outputVertex._posLocalOnMesh;
			_vertPos3_LocalUpdated.z = 0;
			_isCalculated = true;
			//Debug.Log("Update Compute Shader");

			//if(_cal_VertWorld.magnitude > 0.5f && _index == 4)
			//{
			//	Debug.Log("Opt Cal World : " + _cal_VertWorld + " >> VertWorld : " + _vertPos_World + " >> Mesh Transform : \r\n" + _matrix_MeshTransform.ToString());
			//}
		}

		// Get / Set
		//----------------------------------------------
		public bool IsCalculated { get { return _isCalculated; } }
	}
}
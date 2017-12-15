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
using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// 에디터의 apRenderUnit + [Transform_Mesh/Transform_MeshGroup]이 합쳐진 실행객체
	/// Transform (Mesh/MG) 데이터와 RenderUnit의 Update 기능들이 여기에 모두 포함된다.
	/// 
	/// </summary>
	public class apOptTransform : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		public apPortrait _portrait = null;
		public apOptRootUnit _rootUnit = null;

		public int _transformID = -1;



		[HideInInspector]
		public Transform _transform = null;

		[SerializeField]
		public apMatrix _defaultMatrix;


		//RenderUnit 데이터
		public enum UNIT_TYPE { Group = 0, Mesh = 1 }
		public UNIT_TYPE _unitType = UNIT_TYPE.Group;

		public int _meshGroupUniqueID = -1;//Group 타입이면 meshGroupUniqueID를 넣어주자.

		public int _level = -1;
		public int _depth = -1;

		[SerializeField]
		public bool _isVisible_Default = true;

		[SerializeField]
		public Color _meshColor2X_Default = Color.gray;


		public apOptTransform _parentTransform = null;

		public int _nChildTransforms = 0;
		public apOptTransform[] _childTransforms = null;

		//Mesh 타입인 경우
		public apOptMesh _childMesh = null;//실제 Mesh MonoBehaviour
										   //<참고>
										   //원래 apRenderVertex는 renderUnit에 있지만, 여기서는 apOptMesh에 직접 포함되어 있다.



		//Modifier의 값을 전달받는 Stack
		[NonSerialized]
		private apOptCalculatedResultStack _calculatedStack = null;

		public apOptCalculatedResultStack CalculatedStack
		{
			get
			{
				if (_calculatedStack == null)
				{ _calculatedStack = new apOptCalculatedResultStack(this); }
				return _calculatedStack;
			}
		}

		[SerializeField]
		public apOptModifierSubStack _modifierStack = new apOptModifierSubStack();

		//업데이트 되는 변수
		//[NonSerialized]
		//public apMatrix3x3 _matrix_TF_Cal_ToWorld = apMatrix3x3.identity;

		//private apMatrix _calculateTmpMatrix = new apMatrix();
		//public apMatrix CalculatedTmpMatrix {  get { return _calculateTmpMatrix; } }


		//World Transform을 구하기 위해선
		// World Transform = [Parent World] x [To Parent] x [Modified]

		[NonSerialized]
		public apMatrix _matrix_TF_ParentWorld = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TF_ParentWorld_NonModified = new apMatrix();

		//Opt Transform은 기본 좌표에 ToParent가 반영되어 있다.
		[NonSerialized]
		public apMatrix _matrix_TF_ToParent = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TF_LocalModified = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TFResult_World = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TFResult_WorldWithoutMod = new apMatrix();

		[NonSerialized]
		public Color _meshColor2X = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[NonSerialized]
		public bool _isVisible = false;

		private const float VISIBLE_ALPHA = 0.01f;



		// OptBone을 추가한다.
		//OptBone의 GameObject가 저장되는 Transform (내용은 없다)
		public Transform _boneGroup = null;

		public apOptBone[] _boneList_All = null;
		public apOptBone[] _boneList_Root = null;
		public bool _isBoneUpdatable = false;


		//[NonSerialized]
		//private bool _isTransformInit = false;

		// Init
		//------------------------------------------------
		void Awake()
		{

		}

		void Start()
		{

		}

		// Update
		//------------------------------------------------
		void Update()
		{

		}

		void LateUpdate()
		{

		}


		// Update (외부에서 업데이트를 한다.)
		//------------------------------------------------
		public void UpdateModifier_Pre(float tDelta)
		{
			if (_modifierStack != null)
			{
				_modifierStack.Update_Pre(tDelta);
			}

			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateModifier_Pre(tDelta);
				}
			}
		}

		public void UpdateModifier_Post(float tDelta)
		{
			if (_modifierStack != null)
			{
				_modifierStack.Update_Post(tDelta);
			}

			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateModifier_Post(tDelta);
				}
			}
		}


		public void ReadyToUpdate()
		{
			//1. Child Mesh와 기본 Reday
			if (_childMesh != null)
			{
				_childMesh.ReadyToUpdate();
			}

			//2. Calculate Stack Ready
			if (_calculatedStack != null)
			{
				_calculatedStack.ReadyToCalculate();
			}

			//3. 몇가지 변수 초기화
			_meshColor2X = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			_isVisible = true;

			//Editor에서는 기본 matrix가 들어가지만, 여기서는 아예 Transform(Mono)에 들어가기 때문에 Identity가 된다.
			//_matrix_TF_Cal_ToWorld = apMatrix3x3.identity;
			//_calculateTmpMatrix.SetIdentity();


			//변경
			//[Parent World x To Parent x Local TF] 조합으로 변경

			if (_matrix_TF_ParentWorld == null)
			{ _matrix_TF_ParentWorld = new apMatrix(); }
			if (_matrix_TF_ParentWorld_NonModified == null)
			{ _matrix_TF_ParentWorld_NonModified = new apMatrix(); }
			if (_matrix_TF_ToParent == null)
			{ _matrix_TF_ToParent = new apMatrix(); }
			if (_matrix_TF_LocalModified == null)
			{ _matrix_TF_LocalModified = new apMatrix(); }
			if (_matrix_TFResult_World == null)
			{ _matrix_TFResult_World = new apMatrix(); }
			if (_matrix_TFResult_WorldWithoutMod == null)
			{ _matrix_TFResult_WorldWithoutMod = new apMatrix(); }


			_matrix_TF_ParentWorld.SetIdentity();
			_matrix_TF_ParentWorld_NonModified.SetIdentity();
			//_matrix_TF_ToParent.SetIdentity();
			_matrix_TF_LocalModified.SetIdentity();

			//Editor에서는 기본 matrix가 들어가지만, 여기서는 아예 Transform(Mono)에 들어가기 때문에 Identity가 된다.
			_matrix_TF_ToParent.SetMatrix(_defaultMatrix);

			_matrix_TFResult_World.SetIdentity();
			_matrix_TFResult_WorldWithoutMod.SetIdentity();


			//3. 자식 호출
			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].ReadyToUpdate();
				}
			}
		}


		/// <summary>
		/// CalculateStack을 업데이트 한다.
		/// Pre-Update이다. Rigging, VertWorld는 제외된다.
		/// </summary>
		public void UpdateCalculate_Pre()
		{

#if UNITY_EDITOR
			Profiler.BeginSample("Transform - 1. Stack Calculate");
#endif

			//1. Calculated Stack 업데이트
			if (_calculatedStack != null)
			{
				_calculatedStack.Calculate_Pre();
			}

#if UNITY_EDITOR
			Profiler.EndSample();
#endif


#if UNITY_EDITOR
			Profiler.BeginSample("Transform - 2. Matrix / Color");
#endif

			//2. Calculated의 값 적용 + 계층적 Matrix 적용
			if (_calculatedStack.MeshWorldMatrixWrap != null)
			{
				//변경전
				//_calclateTmpMatrix.SRMultiply(_calculatedStack.MeshWorldMatrixWrap, true);
				//_matrix_TF_Cal_ToWorld = _calculateTmpMatrix.MtrxToSpace;

				//변경후
				_matrix_TF_LocalModified.SetMatrix(_calculatedStack.MeshWorldMatrixWrap);

				//if(_calculatedStack.MeshWorldMatrixWrap.Scale2.magnitude < 0.8f)
				//{
				//	Debug.Log(name + " : Low Scale : " + _calculatedStack.MeshWorldMatrixWrap.Scale2);
				//}

			}

			if (CalculatedStack.IsAnyColorCalculated)
			{
				_meshColor2X = CalculatedStack.MeshColor;
				_isVisible = CalculatedStack.IsMeshVisible;
			}
			else
			{
				_meshColor2X = _meshColor2X_Default;
				_isVisible = _isVisible_Default;
			}
			if (!_isVisible)
			{
				_meshColor2X.a = 0.0f;
			}


			if (_parentTransform != null)
			{
				//변경 전
				//_calculateTmpMatrix.SRMultiply(_parentTransform.CalculatedTmpMatrix, true);
				//_matrix_TF_Cal_ToWorld = _calculateTmpMatrix.MtrxToSpace;

				//변경 후
				_matrix_TF_ParentWorld.SetMatrix(_parentTransform._matrix_TFResult_World);
				_matrix_TF_ParentWorld_NonModified.SetMatrix(_parentTransform._matrix_TFResult_WorldWithoutMod);

				//색상은 2X 방식의 Add
				_meshColor2X.r = Mathf.Clamp01(((float)(_meshColor2X.r) - 0.5f) + ((float)(_parentTransform._meshColor2X.r) - 0.5f) + 0.5f);
				_meshColor2X.g = Mathf.Clamp01(((float)(_meshColor2X.g) - 0.5f) + ((float)(_parentTransform._meshColor2X.g) - 0.5f) + 0.5f);
				_meshColor2X.b = Mathf.Clamp01(((float)(_meshColor2X.b) - 0.5f) + ((float)(_parentTransform._meshColor2X.b) - 0.5f) + 0.5f);
				_meshColor2X.a *= _parentTransform._meshColor2X.a;
			}

			if (_meshColor2X.a < VISIBLE_ALPHA
				//|| !CalculatedStack.IsMeshVisible
				)
			{
				_isVisible = false;
				_meshColor2X.a = 0.0f;
			}

			//MakeTransformMatrix(); < 이 함수 부분
			//World Matrix를 만든다.
			_matrix_TFResult_World.RMultiply(_matrix_TF_ToParent);//변경 : ToParent -> LocalModified -> ParentWorld 순으로 바꾼다.
			_matrix_TFResult_World.RMultiply(_matrix_TF_LocalModified);//<<[R]


			//_matrix_TFResult_World.RMultiply(_matrix_TF_ToParent);//<<[R]

			_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorld);//<<[R]

			//_matrix_TFResult_WorldWithoutMod.SRMultiply(_matrix_TF_ToParent, true);//ToParent는 넣지 않는다.
			//_matrix_TFResult_WorldWithoutMod.SRMultiply(_matrix_TF_ParentWorld, true);//<<[SR]

			_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ToParent);//<<[R]
			_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorld_NonModified);//<<[R]

#if UNITY_EDITOR
			Profiler.EndSample();
#endif


			//[MeshUpdate]는 Post Update로 전달

			//3. 자식 호출
			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateCalculate_Pre();
				}
			}

		}



		/// <summary>
		/// CalculateStack을 업데이트 한다.
		/// Post-Update이다. Rigging, VertWorld만 처리된다.
		/// </summary>
		public void UpdateCalculate_Post()
		{

#if UNITY_EDITOR
			Profiler.BeginSample("Transform - 1. Stack Calculate");
#endif

			//1. Calculated Stack 업데이트
			if (_calculatedStack != null)
			{
				_calculatedStack.Calculate_Post();
			}

#if UNITY_EDITOR
			Profiler.EndSample();
#endif


#if UNITY_EDITOR
			Profiler.BeginSample("Transform - 3. Mesh Update");
#endif

			//3. Mesh 업데이트 - 중요
			//실제 Vertex의 위치를 적용
			if (_childMesh != null)
			{
				_childMesh.UpdateCalculate(_calculatedStack.IsRigging,
											_calculatedStack.IsVertexLocal,
											_calculatedStack.IsVertexWorld,
											_isVisible);
			}

#if UNITY_EDITOR
			Profiler.EndSample();
#endif

			//3. 자식 호출
			//자식 객체도 업데이트를 한다.
			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateCalculate_Post();
				}
			}

		}

		//본 관련 업데이트 코드
		public void ReadyToUpdateBones()
		{
			//if(!_isBoneUpdatable)
			//{
			//	return;
			//}
			if (_boneList_Root != null)
			{
				for (int i = 0; i < _boneList_Root.Length; i++)
				{
					_boneList_Root[i].ReadyToUpdate(true);
				}
			}

			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].ReadyToUpdateBones();
				}
			}
		}


		public void UpdateBonesWorldMatrix()
		{
			if (_boneList_Root != null)
			{
				for (int i = 0; i < _boneList_Root.Length; i++)
				{
					_boneList_Root[i].MakeWorldMatrix(true);
				}
			}

			if (_childTransforms != null)
			{
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					_childTransforms[i].UpdateBonesWorldMatrix();
				}
			}
		}

		// Editor Functions
		//------------------------------------------------
		public void Bake(apPortrait portrait, //apMeshGroup srcMeshGroup, 
							apOptTransform parentTransform,
							apOptRootUnit rootUnit,
							int transformID, int meshGroupUniqueID, apMatrix defaultMatrix,
							bool isMesh, int level, int depth,
							bool isVisible_Default,
							Color meshColor2X_Default
										)
		{
			_portrait = portrait;
			_rootUnit = rootUnit;
			_transformID = transformID;
			_meshGroupUniqueID = meshGroupUniqueID;

			_parentTransform = parentTransform;

			_defaultMatrix = new apMatrix(defaultMatrix);
			_transform = transform;

			_level = level;
			_depth = depth;

			_isVisible_Default = isVisible_Default;
			_meshColor2X_Default = meshColor2X_Default;

			if (parentTransform != null)
			{
				_depth -= parentTransform._depth;
			}

			//이부분 실험 중
			//1. Default Matrix를 Transform에 적용하고, Modifier 계산에서는 제외하는 경우
			//결과 : Bake시에는 "Preview"를 위해서 DefaultMatrix 위치로 이동을 시키지만, 실행시에는 원점으로 이동시킨다.
			//_transform.localPosition = _defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)_depth);
			//_transform.localRotation = Quaternion.Euler(0.0f, 0.0f, _defaultMatrix._angleDeg);
			//_transform.localScale = _defaultMatrix._scale;

			//2. Default Matrix를 Modifier에 포함시키고 Transform은 원점인 경우 (Editor와 동일)
			_transform.localPosition = -new Vector3(0.0f, 0.0f, (float)_depth);
			_transform.localRotation = Quaternion.identity;
			_transform.localScale = Vector3.one;

			if (isMesh)
			{
				_unitType = UNIT_TYPE.Mesh;
			}
			else
			{
				_unitType = UNIT_TYPE.Group;
			}

			_childTransforms = null;
			_childMesh = null;
		}

		public void BakeModifier(apPortrait portrait, apMeshGroup srcMeshGroup)
		{
			if (srcMeshGroup != null)
			{
				_modifierStack.Bake(srcMeshGroup._modifierStack, portrait);
			}
		}

		public void SetChildMesh(apOptMesh optMesh)
		{
			_childMesh = optMesh;
		}

		public void AddChildTransforms(apOptTransform childTransform)
		{
			if (_childTransforms == null)
			{
				_childTransforms = new apOptTransform[1];
				_childTransforms[0] = childTransform;
			}
			else
			{
				apOptTransform[] nextTransform = new apOptTransform[_childTransforms.Length + 1];
				for (int i = 0; i < _childTransforms.Length; i++)
				{
					nextTransform[i] = _childTransforms[i];
				}
				nextTransform[nextTransform.Length - 1] = childTransform;

				_childTransforms = new apOptTransform[nextTransform.Length];
				for (int i = 0; i < nextTransform.Length; i++)
				{
					_childTransforms[i] = nextTransform[i];
				}
			}
		}

		public void ClearResultParams()
		{
			if (_calculatedStack == null)
			{
				_calculatedStack = new apOptCalculatedResultStack(this);
			}

			//Debug.Log("Clear Param : " + _transformID);
			_calculatedStack.ClearResultParams();
			_modifierStack.ClearAllCalculateParam();
		}

		/// <summary>
		/// [핵심 코드]
		/// Modifier를 업데이트할 수 있도록 연결해준다.
		/// </summary>
		public void RefreshModifierLink()
		{

			if (_calculatedStack == null)
			{
				_calculatedStack = new apOptCalculatedResultStack(this);
			}
			_modifierStack.LinkModifierStackToRenderUnitCalculateStack();
		}



		// Functions
		//------------------------------------------------
		public void Show(bool isChildShow)
		{
			if (_childMesh != null)
			{
				_childMesh.Show();
			}

			if (isChildShow)
			{
				if (_childTransforms != null)
				{
					for (int i = 0; i < _childTransforms.Length; i++)
					{
						_childTransforms[i].Show(true);
					}
				}
			}
		}

		public void Hide(bool isChildHide)
		{
			if (_childMesh != null)
			{
				_childMesh.Hide();
			}

			if (isChildHide)
			{
				if (_childTransforms != null)
				{
					for (int i = 0; i < _childTransforms.Length; i++)
					{
						_childTransforms[i].Hide(true);
					}
				}
			}
		}


		// Get / Set
		//------------------------------------------------
		public apOptModifierUnitBase GetModifier(int uniqueID)
		{
			return _modifierStack.GetModifier(uniqueID);
		}



		public apOptTransform GetMeshTransform(int uniqueID)
		{
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				if (_childTransforms[i]._unitType == UNIT_TYPE.Mesh
					&& _childTransforms[i]._transformID == uniqueID)
				{
					return _childTransforms[i];
				}
			}
			return null;
		}

		public apOptTransform GetMeshGroupTransform(int uniqueID)
		{
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				if (_childTransforms[i]._unitType == UNIT_TYPE.Group
					&& _childTransforms[i]._transformID == uniqueID)
				{
					return _childTransforms[i];
				}
			}
			return null;
		}


		public apOptTransform GetMeshTransformRecursive(int uniqueID)
		{
			apOptTransform result = GetMeshTransform(uniqueID);
			if (result != null)
			{
				return result;
			}

			apOptTransform curGroupTransform = null;
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				curGroupTransform = _childTransforms[i];
				if (curGroupTransform._unitType != UNIT_TYPE.Group)
				{
					continue;
				}

				result = curGroupTransform.GetMeshTransformRecursive(uniqueID);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		public apOptTransform GetMeshGroupTransformRecursive(int uniqueID)
		{
			apOptTransform result = GetMeshGroupTransform(uniqueID);
			if (result != null)
			{
				return result;
			}

			apOptTransform curGroupTransform = null;
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				curGroupTransform = _childTransforms[i];
				if (curGroupTransform._unitType != UNIT_TYPE.Group)
				{
					continue;
				}

				result = curGroupTransform.GetMeshGroupTransformRecursive(uniqueID);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		public apOptBone GetBone(int uniqueID)
		{
			for (int i = 0; i < _boneList_All.Length; i++)
			{
				if (_boneList_All[i]._uniqueID == uniqueID)
				{
					return _boneList_All[i];
				}
			}

			return null;
		}

		public apOptBone GetBoneRecursive(int uniqueID)
		{
			for (int i = 0; i < _boneList_All.Length; i++)
			{
				if (_boneList_All[i]._uniqueID == uniqueID)
				{
					return _boneList_All[i];
				}
			}


			apOptBone result = null;
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				result = _childTransforms[i].GetBoneRecursive(uniqueID);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}
	}
}
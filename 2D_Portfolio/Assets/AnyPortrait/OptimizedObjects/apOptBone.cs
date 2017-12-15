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
	/// MeshGroup에 포함되는 apBone의 Opt 버전
	/// MeshGroup에 해당되는 Root OptTransform의 "Bones" GameObject에 포함된다.
	/// Matrix 계산은 Bone과 동일하며, Transform에 반영되지 않는다. (Transform은 Local Pos와 Rotation만 계산된다)
	/// Transform은 Rigging에 반영되지는 않지만, 만약 어떤 오브젝트를 Attachment 한다면 사용되어야 한다.
	/// Opt Bone의 Transform은 외부 입력은 무시하며, Attachment를 하는 용도로만 사용된다.
	/// Attachment를 하는 경우 하위에 Socket Transform을 생성한뒤, 거기서 WorldMatrix에 해당하는 TRS를 넣는다. (값 자체는 Local Matrix)
	/// </summary>
	public class apOptBone : MonoBehaviour
	{
		// Members
		//---------------------------------------------------------------
		// apBone 정보를 옮기자
		public int _uniqueID = -1;
		public int _meshGroupID = -1;

		//이건 Serialize가 된다.
		public apOptTransform _parentOptTransform = null;

		public apOptBone _parentBone = null;

		public apOptBone[] _childBones = null;//<ChildBones의 배열버전

		[SerializeField]
		public apMatrix _defaultMatrix = new apMatrix();

		[NonSerialized]
		private Vector2 _deltaPos = Vector2.zero;

		[NonSerialized]
		private float _deltaAngle = 0.0f;

		[NonSerialized]
		private Vector2 _deltaScale = Vector2.one;

		[NonSerialized]
		public apMatrix _localMatrix = new apMatrix();

		[NonSerialized]
		public apMatrix _worldMatrix = new apMatrix();

		[NonSerialized]
		public apMatrix _worldMatrix_NonModified = new apMatrix();


		//Shape 계열
		[SerializeField]
		public Color _color = Color.white;
		public int _shapeWidth = 30;
		public int _shapeLength = 50;//<<이 값은 생성할 때 Child와의 거리로 판단한다.
		public int _shapeTaper = 100;//기본값은 뾰족

#if UNITY_EDITOR
		private Vector2 _shapePoint_End = Vector3.zero;

		private Vector2 _shapePoint_Mid1 = Vector3.zero;
		private Vector2 _shapePoint_Mid2 = Vector3.zero;
		private Vector2 _shapePoint_End1 = Vector3.zero;
		private Vector2 _shapePoint_End2 = Vector3.zero;
#endif

		//IK 정보
		public apBone.OPTION_LOCAL_MOVE _optionLocalMove = apBone.OPTION_LOCAL_MOVE.Disabled;
		public apBone.OPTION_IK _optionIK = apBone.OPTION_IK.IKSingle;

		/// <summary>
		/// Parent로부터 IK의 대상이 되는가? IK Single일 때에도 Tail이 된다.
		/// (자신이 IK를 설정하는 것과는 무관함)
		/// </summary>
		public bool _isIKTail = false;

		//IK의 타겟과 Parent
		public int _IKTargetBoneID = -1;

		public apOptBone _IKTargetBone = null;

		public int _IKNextChainedBoneID = -1;

		public apOptBone _IKNextChainedBone = null;


		/// <summary>
		/// IK Tail이거나 IK Chained 상태라면 Header를 저장하고, Chaining 처리를 해야한다.
		/// </summary>
		public int _IKHeaderBoneID = -1;

		public apOptBone _IKHeaderBone = null;



		//IK시 추가 옵션

		/// <summary>IK 적용시, 각도를 제한을 줄 것인가 (기본값 False)</summary>
		public bool _isIKAngleRange = false;
		public float _IKAngleRange_Lower = -90.0f;//음수여야 한다.
		public float _IKAngleRange_Upper = 90.0f;//양수여야 한다.
		public float _IKAnglePreferred = 0.0f;//선호하는 각도 Offset


		/// <summary>IK 연산이 되었는가</summary>
		[NonSerialized]
		public bool _isIKCalculated = false;

		/// <summary>IK 연산이 발생했을 경우, World 좌표계에서 Angle을 어떻게 만들어야 하는지 계산 결과값</summary>
		[NonSerialized]
		public float _IKRequestAngleResult = 0.0f;


		/// <summary>
		/// IK 계산을 해주는 Chain Set.
		/// </summary>
		[SerializeField]
		private apOptBoneIKChainSet _IKChainSet = null;//<<이거 Opt 버전으로 만들자





		//추가 : 이건 나중에 세팅하자
		//Transform에 적용되는 Local Matrix 값 (Scale이 없다)
		[NonSerialized]
		public apMatrix _transformLocalMatrix = new apMatrix();

		//Attach시 만들어지는 Socket
		//Socket 옵션은 Bone에서 미리 세팅해야한다.
		public Transform _socketTransform = null;


		// Init
		//---------------------------------------------------------------
		void Start()
		{
			//업데이트 안합니더
			this.enabled = false;
		}


		//Link 함수의 내용은 Bake 시에 진행해야한다.
		public void Bake(apBone bone)
		{
			_uniqueID = bone._uniqueID;
			_meshGroupID = bone._meshGroupID;
			_defaultMatrix.SetMatrix(bone._defaultMatrix);


			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_localMatrix.SetIdentity();

			_worldMatrix.SetIdentity();

			_color = bone._color;
			_shapeWidth = bone._shapeWidth;
			_shapeLength = bone._shapeLength;
			_shapeTaper = bone._shapeTaper;

			_optionLocalMove = bone._optionLocalMove;
			_optionIK = bone._optionIK;

			_isIKTail = bone._isIKTail;

			_IKTargetBoneID = bone._IKTargetBoneID;
			_IKTargetBone = null;//<<나중에 링크

			_IKNextChainedBoneID = bone._IKNextChainedBoneID;
			_IKNextChainedBone = null;//<<나중에 링크


			_IKHeaderBoneID = bone._IKHeaderBoneID;
			_IKHeaderBone = null;//<<나중에 링크


			_isIKAngleRange = bone._isIKAngleRange;
			_IKAngleRange_Lower = bone._IKAngleRange_Lower;
			_IKAngleRange_Upper = bone._IKAngleRange_Upper;
			_IKAnglePreferred = bone._IKAnglePreferred;


			_isIKCalculated = false;
			_IKRequestAngleResult = 0.0f;



			_transformLocalMatrix.SetIdentity();
		}


		public void Link(apOptTransform targetOptTransform)
		{
			_parentOptTransform = targetOptTransform;
			if (_parentOptTransform == null)
			{
				//??
				Debug.LogError("[" + transform.name + "] OptBone의 ParentOptTransform이 Null이다. [" + _meshGroupID + "]");
				_IKTargetBone = null;
				_IKNextChainedBone = null;
				_IKHeaderBone = null;

				//LinkBoneChaining();


				return;
			}


			_IKTargetBone = _parentOptTransform.GetBone(_IKTargetBoneID);
			_IKNextChainedBone = _parentOptTransform.GetBone(_IKNextChainedBoneID);
			_IKHeaderBone = _parentOptTransform.GetBone(_IKHeaderBoneID);

			//LinkBoneChaining();

		}



		//여기서는 LinkBoneChaining만 진행
		/// <summary>
		/// Bone Chaining 직후에 재귀적으로 호출한다.
		/// Tail이 가지는 -> Head로의 IK 리스트를 만든다.
		/// 
		/// </summary>
		public void LinkBoneChaining()
		{
			if (_localMatrix == null)
			{
				_localMatrix = new apMatrix();
			}
			if (_worldMatrix == null)
			{
				_worldMatrix = new apMatrix();
			}
			if (_worldMatrix_NonModified == null)
			{
				_worldMatrix_NonModified = new apMatrix();
			}


			if (_isIKTail)
			{
				apOptBone curParentBone = _parentBone;
				apOptBone headBone = _IKHeaderBone;

				bool isParentExist = (curParentBone != null);
				bool isHeaderExist = (headBone != null);
				bool isHeaderIsInParents = false;
				if (isParentExist && isHeaderExist)
				{
					isHeaderIsInParents = (GetParentRecursive(headBone._uniqueID) != null);
				}


				if (isParentExist && isHeaderExist && isHeaderIsInParents)
				{
					if (_IKChainSet == null)
					{
						_IKChainSet = new apOptBoneIKChainSet(this);
					}
					//Chain을 Refresh한다.
					_IKChainSet.RefreshChain();
				}
				else
				{
					_IKChainSet = null;

					Debug.LogError("[" + transform.name + "] IK Chaining Error : Parent -> Chain List 연결시 데이터가 누락되었다. "
						+ "[ Parent : " + isParentExist
						+ " / Header : " + isHeaderExist
						+ " / IsHeader Is In Parent : " + isHeaderIsInParents + " ]");
				}
			}
			else
			{
				_IKChainSet = null;
			}

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].LinkBoneChaining();
				}
			}

		}


		// Update
		//---------------------------------------------------------------
		/// <summary>
		/// 1) Update Transform Matrix를 초기화한다.
		/// </summary>
		public void ReadyToUpdate(bool isRecursive)
		{
			//_localModifiedTransformMatrix.SetIdentity();

			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_isIKCalculated = false;
			_IKRequestAngleResult = 0.0f;

			//_worldMatrix.SetIdentity();
			if (isRecursive && _childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].ReadyToUpdate(true);
				}
			}
		}

		/// <summary>
		/// 2) Update된 TRS 값을 넣는다.
		/// </summary>
		/// <param name="deltaPos"></param>
		/// <param name="deltaAngle"></param>
		/// <param name="deltaScale"></param>
		public void UpdateModifiedValue(Vector2 deltaPos, float deltaAngle, Vector2 deltaScale)
		{
			_deltaPos = deltaPos;
			_deltaAngle = deltaAngle;
			_deltaScale = deltaScale;
		}


		public void AddIKAngle(float IKAngle)
		{
			_isIKCalculated = true;
			_IKRequestAngleResult += IKAngle;
		}

		/// <summary>
		/// 4) World Matrix를 만든다.
		/// 이 함수는 Parent의 MeshGroupTransform이 연산된 후 -> Vertex가 연산되기 전에 호출되어야 한다.
		/// </summary>
		public void MakeWorldMatrix(bool isRecursive)
		{
			_localMatrix.SetIdentity();
			_localMatrix._pos = _deltaPos;
			_localMatrix._angleDeg = _deltaAngle;
			_localMatrix._scale.x = _deltaScale.x;
			_localMatrix._scale.y = _deltaScale.y;

			_localMatrix.MakeMatrix();

			//World Matrix = ParentMatrix x LocalMatrix
			//Root인 경우에는 MeshGroup의 Matrix를 이용하자

			//_invWorldMatrix_NonModified.SetIdentity();

			if (_parentBone == null)
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정


				if (_parentOptTransform != null)
				{
					//Debug.Log("SetParentOptTransform Matrix : [" + _parentOptTransform.transform.name + "] : " + _parentOptTransform._matrix_TFResult_World.Scale2);
					//Non Modified도 동일하게 적용
					//렌더유닛의 WorldMatrix를 넣어주자
					_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_World);//RenderUnit의 WorldMatrixWrap의 Opt 버전

					_worldMatrix_NonModified.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod);

				}
			}
			else
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);
				_worldMatrix.RMultiply(_parentBone._worldMatrix);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정
				_worldMatrix_NonModified.RMultiply(_parentBone._worldMatrix_NonModified);
			}

			//World Matrix는 MeshGroup과 동일한 Space의 값을 가진다.
			//그러나 실제로 Bone World Matrix는
			//Root - MeshGroup...(Rec) - Bone Group - Bone.. (Rec <- 여기)
			//의 레벨을 가진다.
			//Root 밑으로는 모두 World에 대해서 동일한 Space를 가지므로
			//Root를 찾아서 Scale을 제어하자...?
			//일단 Parent에서 빼두자
			//_transformLocalMatrix.SetMatrix(_worldMatrix);


#if UNITY_EDITOR
			_shapePoint_End = new Vector2(0.0f, _shapeLength);


			_shapePoint_Mid1 = new Vector2(-_shapeWidth * 0.5f, _shapeLength * 0.2f);
			_shapePoint_Mid2 = new Vector2(_shapeWidth * 0.5f, _shapeLength * 0.2f);

			float taperRatio = Mathf.Clamp01((float)(100 - _shapeTaper) / 100.0f);

			_shapePoint_End1 = new Vector2(-_shapeWidth * 0.5f * taperRatio, _shapeLength);
			_shapePoint_End2 = new Vector2(_shapeWidth * 0.5f * taperRatio, _shapeLength);

			_shapePoint_End = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End);
			_shapePoint_Mid1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid1);
			_shapePoint_Mid2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid2);
			_shapePoint_End1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End1);
			_shapePoint_End2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End2);
#endif

			if (_socketTransform != null)
			{
				//소켓을 업데이트 하자
				_socketTransform.localPosition = new Vector3(_worldMatrix._pos.x, _worldMatrix._pos.y, 0);
				_socketTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, _worldMatrix._angleDeg);
				_socketTransform.localScale = _worldMatrix._scale;
			}
			//if (string.Equals(name, "Bone 2 Debug"))
			//{
			//	//디버그를 해보자
			//	Debug.Log("------- Bone Matrix [" + name + "] ------- (Runtime)");
			//	Debug.Log("Default Matrix [" + _defaultMatrix.ToString() + "]");
			//	Debug.Log("Local Matrix [" + _localMatrix.ToString() + "]");
			//	if (_parentBone != null)
			//	{
			//		Debug.Log("Parent(" + _parentBone.name + ")");
			//		Debug.Log(">> World Matrix [" + _parentBone._worldMatrix.ToString() + "]");
			//		Debug.Log(">> World Matrix No Mod [" + _parentBone._worldMatrix_NonModified.ToString() + "]");
			//	}
			//	Debug.Log("World Matrix [" + _worldMatrix.ToString() + "]");
			//	Debug.Log("World Matrix No Mod [" + _worldMatrix_NonModified.ToString() + "]");
			//	Debug.Log("-----------------------------------------");
			//}

			//Child도 호출해준다.
			if (isRecursive && _childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].MakeWorldMatrix(true);
				}
			}
		}



		// Functions
		//---------------------------------------------------------------



		// Get / Set
		//---------------------------------------------------------------
		/// <summary>
		/// boneID를 가지는 Bone을 자식 노드로 두고 있는가.
		/// 재귀적으로 찾는다.
		/// </summary>
		/// <param name="boneID"></param>
		/// <returns></returns>
		public apOptBone GetChildBoneRecursive(int boneID)
		{
			if (_childBones == null)
			{
				return null;
			}
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해보자

			for (int i = 0; i < _childBones.Length; i++)
			{
				apOptBone result = _childBones[i].GetChildBoneRecursive(boneID);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		/// <summary>
		/// 바로 아래의 자식 Bone을 검색한다.
		/// </summary>
		/// <param name="boneID"></param>
		/// <returns></returns>
		public apOptBone GetChildBone(int boneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			return null;
		}

		/// <summary>
		/// 자식 Bone 중에서 특정 Target Bone을 재귀적인 자식으로 가지는 시작 Bone을 찾는다.
		/// </summary>
		/// <param name="targetBoneID"></param>
		/// <returns></returns>
		public apOptBone FindNextChainedBone(int targetBoneID)
		{
			//바로 아래의 자식 노드를 검색
			if (_childBones == null)
			{
				return null;
			}
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == targetBoneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해서, 그 중에 실제로 Target Bone을 포함하는 Child Bone을 리턴하자

			for (int i = 0; i < _childBones.Length; i++)
			{
				apOptBone result = _childBones[i].GetChildBoneRecursive(targetBoneID);
				if (result != null)
				{
					//return result;
					return _childBones[i];//<<Result가 아니라, ChildBone을 리턴
				}
			}
			return null;
		}

		/// <summary>
		/// 요청한 boneID를 가지는 Bone을 부모 노드로 두고 있는가.
		/// 재귀적으로 찾는다.
		/// </summary>
		/// <param name="boneID"></param>
		/// <returns></returns>
		public apOptBone GetParentRecursive(int boneID)
		{
			if (_parentBone == null)
			{
				return null;
			}

			if (_parentBone._uniqueID == boneID)
			{
				return _parentBone;
			}

			//재귀적으로 검색해보자
			return _parentBone.GetParentRecursive(boneID);

		}



		// Gizmo Event
#if UNITY_EDITOR
		void OnDrawGizmosSelected()
		{
			Gizmos.color = _color;

			Matrix4x4 tfMatrix = transform.localToWorldMatrix;
			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_End));

			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_Mid1));
			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_Mid2));
			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_Mid1), tfMatrix.MultiplyPoint3x4(_shapePoint_End1));
			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_Mid2), tfMatrix.MultiplyPoint3x4(_shapePoint_End2));
			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_End1), tfMatrix.MultiplyPoint3x4(_shapePoint_End2));
		}
#endif
	}

}
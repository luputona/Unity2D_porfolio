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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// Editor
	/// </summary>
	public class apSelection
	{
		// Members
		//-------------------------------------
		public apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }

		public enum SELECTION_TYPE
		{
			None,
			ImageRes,
			Mesh,
			Face,
			MeshGroup,
			Animation,
			Overall,
			Param
		}

		private SELECTION_TYPE _selectionType = SELECTION_TYPE.None;

		public SELECTION_TYPE SelectionType { get { return _selectionType; } }

		private apPortrait _portrait = null;
		private apRootUnit _rootUnit = null;
		private apTextureData _image = null;
		private apMesh _mesh = null;
		private apMeshGroup _meshGroup = null;
		private apControlParam _param = null;
		private apAnimClip _animClip = null;

		//Overall 선택시, 선택가능한 AnimClip 리스트
		private List<apAnimClip> _rootUnitAnimClips = new List<apAnimClip>();
		private apAnimClip _curRootUnitAnimClip = null;


		//Anim Clip 내에서 단일 선택시
		private apAnimTimeline _subAnimTimeline = null;//<<타임라인 단일 선택시
		private apAnimTimelineLayer _subAnimTimelineLayer = null;//타임 라인의 레이어 단일 선택시
		private apAnimKeyframe _subAnimKeyframe = null;//단일 선택한 키프레임
		private apAnimKeyframe _subAnimWorkKeyframe = null;//<<자동으로 선택되는 키프레임이다. "현재 프레임"에 위치한 "레이어의 프레임"이다.
		private bool _isAnimTimelineLayerGUIScrollRequest = false;


		private List<apAnimKeyframe> _subAnimKeyframeList = new List<apAnimKeyframe>();//여러개의 키프레임을 선택한 경우 (주로 복불/이동 할때)
		private EX_EDIT _exAnimEditingMode = EX_EDIT.None;//<애니메이션 수정 작업을 하고 있는가
		public EX_EDIT ExAnimEditingMode { get { if (IsAnimEditable) { return _exAnimEditingMode; } return EX_EDIT.None; } }

		private bool _isAnimLock = false;


		private apTransform_Mesh _subMeshTransformOnAnimClip = null;
		private apTransform_MeshGroup _subMeshGroupTransformOnAnimClip = null;
		private apControlParam _subControlParamOnAnimClip = null;

		//AnimClip에서 ModMesh를 선택하고 Vertex 수정시
		private apModifiedMesh _modMeshOfAnim = null;
		private apRenderUnit _renderUnitOfAnim = null;
		private ModRenderVert _modRenderVertOfAnim = null;
		private List<ModRenderVert> _modRenderVertListOfAnim = new List<ModRenderVert>();//<<1개만 선택해도 리스트엔 들어가있다.
		private List<ModRenderVert> _modRenderVertListOfAnim_Weighted = new List<ModRenderVert>();//<<Soft Selection, Blur, Volume 등에 포함되는 "Weight가 포함된 리스트"
		private apModifiedBone _modBoneOfAnim = null;


		/// <summary>애니메이션 수정 작업이 가능한가?</summary>
		private bool IsAnimEditable
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _subAnimTimeline == null)
				{
					return false;
				}
				if (_animClip._targetMeshGroup == null)
				{
					return false;
				}
				return true;
			}
		}
		public bool IsAnimPlaying
		{
			get
			{
				if (AnimClip == null)
				{
					return false;
				}
				return AnimClip.IsPlaying;
			}
		}




		public apModifiedMesh ModMeshOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modMeshOfAnim; } return null; } }
		public apModifiedBone ModBoneOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modBoneOfAnim; } return null; } }
		public apRenderUnit RenderUnitOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _renderUnitOfAnim; } return null; } }
		public ModRenderVert ModRenderVertOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modRenderVertOfAnim; } return null; } }
		public List<ModRenderVert> ModRenderVertListOfAnim { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modRenderVertListOfAnim; } return null; } }
		public List<ModRenderVert> ModRenderVertListOfAnim_Weighted { get { if (_selectionType == SELECTION_TYPE.Animation) { return _modRenderVertListOfAnim_Weighted; } return null; } }

		public Vector2 ModRenderVertsCenterPosOfAnim
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.Animation || _modRenderVertListOfAnim.Count == 0)
				{
					return Vector2.zero;
				}
				Vector2 centerPos = Vector2.zero;
				for (int i = 0; i < _modRenderVertListOfAnim.Count; i++)
				{
					centerPos += _modRenderVertListOfAnim[i]._renderVert._pos_World;
				}
				centerPos /= _modRenderVertListOfAnim.Count;
				return centerPos;
			}
		}


		//Bone


		private apTransform_Mesh _subMeshTransformInGroup = null;
		private apTransform_MeshGroup _subMeshGroupTransformInGroup = null;

		private apModifierBase _modifier = null;

		//Modifier 작업시 선택하는 객체들
		private apModifierParamSet _paramSetOfMod = null;


		private apModifiedMesh _modMeshOfMod = null;
		private apModifiedBone _modBoneOfMod = null;//<추가
		private apRenderUnit _renderUnitOfMod = null;

		//추가
		//modBone으로 등록 가능한 apBone 리스트
		private List<apBone> _modRegistableBones = new List<apBone>();
		public List<apBone> ModRegistableBones { get { return _modRegistableBones; } }

		//Mod Vert와 Render Vert는 동시에 선택이 된다.
		public class ModRenderVert
		{
			public apModifiedVertex _modVert = null;
			public apRenderVertex _renderVert = null;
			//추가
			//ModVert가 아니라 ModVertRig가 매칭되는 경우도 있다.
			//Gizmo에서 주로 사용하는데 에러 안나게 주의할 것
			public apModifiedVertexRig _modVertRig = null;

			public apModifiedVertexWeight _modVertWeight = null;


			/// <summary>
			/// SoftSelection, Blur, Volume등의 "편집 과정에서의 Weight"를 임시로 결정하는 경우의 값
			/// </summary>
			public float _vertWeightByTool = 1.0f;

			public ModRenderVert(apModifiedVertex modVert, apRenderVertex renderVert)
			{
				_modVert = modVert;
				_modVertRig = null;
				_modVertWeight = null;

				_renderVert = renderVert;
				_vertWeightByTool = 1.0f;

			}

			public ModRenderVert(apModifiedVertexRig modVertRig, apRenderVertex renderVert)
			{
				_modVert = null;
				_modVertRig = modVertRig;
				_modVertWeight = null;

				_renderVert = renderVert;
				_vertWeightByTool = 1.0f;
			}

			public ModRenderVert(apModifiedVertexWeight modVertWeight, apRenderVertex renderVert)
			{
				_modVert = null;
				_modVertRig = null;
				_modVertWeight = modVertWeight;

				_renderVert = renderVert;
				_vertWeightByTool = _modVertWeight._weight;//<<이건 갱신해야할 것
			}

			//다음 World 좌표값을 받아서 ModifiedVertex의 값을 수정하자
			public void SetWorldPosToModifier_VertLocal(Vector2 nextWorldPos)
			{
				//NextWorld Pos에서 -> [VertWorld] -> [MeshTransform] -> Vert Local 적용 후의 좌표 -> Vert Local 적용 전의 좌표 
				//적용 전-후의 좌표 비교 = 그 차이값을 ModVert에 넣자
				apMatrix3x3 matToAfterVertLocal = (_renderVert._matrix_Cal_VertWorld * _renderVert._matrix_MeshTransform).inverse;
				Vector2 nextLocalMorphedPos = matToAfterVertLocal.MultiplyPoint(nextWorldPos);
				Vector2 beforeLocalMorphedPos = (_renderVert._matrix_Cal_VertLocal * _renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(_renderVert._vertex._pos);

				_modVert._deltaPos.x += (nextLocalMorphedPos.x - beforeLocalMorphedPos.x);
				_modVert._deltaPos.y += (nextLocalMorphedPos.y - beforeLocalMorphedPos.y);
			}
		}

		//버텍스에 대해서
		//단일 선택일때
		//복수개의 선택일때
		private ModRenderVert _modRenderVertOfMod = null;
		private List<ModRenderVert> _modRenderVertListOfMod = new List<ModRenderVert>();//<<1개만 선택해도 리스트엔 들어가있다.
		private List<ModRenderVert> _modRenderVertListOfMod_Weighted = new List<ModRenderVert>();//<<Soft Selection, Blur, Volume 등에 포함되는 "Weight가 포함된 리스트"


		//메시/메시그룹 트랜스폼에 대해서
		//복수 선택도 가능하게 해주자
		private List<apTransform_Mesh> _subMeshTransformListInGroup = new List<apTransform_Mesh>();
		private List<apTransform_MeshGroup> _subMeshGroupTransformListInGroup = new List<apTransform_MeshGroup>();



		/// <summary>Modifier에서 현재 선택중인 ParamSetGroup [주의 : Animated Modifier에서는 이 값을 사용하지 말고 다른 값을 사용해야한다]</summary>
		private apModifierParamSetGroup _subEditedParamSetGroup = null;

		/// <summary>Animated Modifier에서 현재 선택중인 ParamSetGroup의 Pack. [주의 : Animataed Modifier에서만 사용가능하다]</summary>
		private apModifierParamSetGroupAnimPack _subEditedParamSetGroupAnimPack = null;


		public apPortrait Portrait { get { return _portrait; } }

		public apRootUnit RootUnit { get { if (_selectionType == SELECTION_TYPE.Overall && _portrait != null) { return _rootUnit; } return null; } }
		public List<apAnimClip> RootUnitAnimClipList { get { if (_selectionType == SELECTION_TYPE.Overall && _portrait != null) { return _rootUnitAnimClips; } return null; } }
		public apAnimClip RootUnitAnimClip { get { if (_selectionType == SELECTION_TYPE.Overall && _portrait != null) { return _curRootUnitAnimClip; } return null; } }


		public apTextureData TextureData { get { if (_selectionType == SELECTION_TYPE.ImageRes) { return _image; } return null; } }
		public apMesh Mesh { get { if (_selectionType == SELECTION_TYPE.Mesh) { return _mesh; } return null; } }
		public apMeshGroup MeshGroup { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _meshGroup; } return null; } }
		public apControlParam Param { get { if (_selectionType == SELECTION_TYPE.Param) { return _param; } return null; } }
		public apAnimClip AnimClip { get { if (_selectionType == SELECTION_TYPE.Animation) { return _animClip; } return null; } }

		//Mesh Group에서 서브 선택
		//Mesh/MeshGroup Transform
		public apTransform_Mesh SubMeshInGroup { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _subMeshTransformInGroup; } return null; } }
		public apTransform_MeshGroup SubMeshGroupInGroup { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _subMeshGroupTransformInGroup; } return null; } }

		//ParamSetGroup / ParamSetGroupAnimPack
		/// <summary>Modifier에서 현재 선택중인 ParamSetGroup [주의 : Animated Modifier에서는 이 값을 사용하지 말고 다른 값을 사용해야한다]</summary>
		public apModifierParamSetGroup SubEditedParamSetGroup { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _subEditedParamSetGroup; } return null; } }
		/// <summary>Animated Modifier에서 현재 선택중인 ParamSetGroup의 Pack. [주의 : Animataed Modifier에서만 사용가능하다]</summary>
		public apModifierParamSetGroupAnimPack SubEditedParamSetGroupAnimPack { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _subEditedParamSetGroupAnimPack; } return null; } }


		//MeshGroup Setting에서 Pivot을 바꿀 때
		private bool _isMeshGroupSetting_ChangePivot = false;
		public bool IsMeshGroupSettingChangePivot { get { return _isMeshGroupSetting_ChangePivot; } }

		//현재 선택된 Modifier
		public apModifierBase Modifier { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modifier; } return null; } }

		//Modifier 작업식 선택하는 객체들
		public apModifierParamSet ParamSetOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _paramSetOfMod; } return null; } }
		public apModifiedMesh ModMeshOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modMeshOfMod; } return null; } }

		public apModifiedBone ModBoneOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modBoneOfMod; } return null; } }

		public apRenderUnit RenderUnitOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _renderUnitOfMod; } return null; } }

		//public apModifiedVertex ModVertOfMod { get { if(_selectionType == SELECTION_TYPE.MeshGroup) { return _modVertOfMod; } return null; } }
		//public apRenderVertex RenderVertOfMod { get { if(_selectionType == SELECTION_TYPE.MeshGroup) { return _renderVertOfMod; } return null; } }
		public ModRenderVert ModRenderVertOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modRenderVertOfMod; } return null; } }
		public List<ModRenderVert> ModRenderVertListOfMod { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modRenderVertListOfMod; } return null; } }
		public List<ModRenderVert> ModRenderVertListOfMod_Weighted { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _modRenderVertListOfMod_Weighted; } return null; } }

		public Vector2 ModRenderVertsCenterPosOfMod
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.MeshGroup || _modRenderVertListOfMod.Count == 0)
				{
					return Vector2.zero;
				}
				Vector2 centerPos = Vector2.zero;
				for (int i = 0; i < _modRenderVertListOfMod.Count; i++)
				{
					centerPos += _modRenderVertListOfMod[i]._renderVert._pos_World;
				}
				centerPos /= _modRenderVertListOfMod.Count;
				return centerPos;
			}
		}

		//public apControlParam ControlParamOfMod { get { if(_selectionType == SELECTION_TYPE.MeshGroup) { return _subControlParamOfMod; } return null; } }
		//public apControlParam ControlParamEditingMod { get { if(_selectionType == SELECTION_TYPE.MeshGroup) { return _subControlParamEditingMod; } return null; } }

		//Mesh Group을 본격적으로 수정할 땐, 다른 기능이 잠겨야 한다.
		public enum EX_EDIT_KEY_VALUE
		{
			None,//<<별 제한없이 컨트롤 가능하며 별도의 UI가 등장하지 않는다.
			ModMeshAndParamKey_ModVert,
			ParamKey_ModMesh,
			ParamKey_Bone
		}
		//private bool _isExclusiveModifierEdit = false;//<true이면 몇가지 기능이 잠긴다.
		private EX_EDIT_KEY_VALUE _exEditKeyValue = EX_EDIT_KEY_VALUE.None;
		public EX_EDIT_KEY_VALUE ExEditMode { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _exEditKeyValue; } return EX_EDIT_KEY_VALUE.None; } }

		/// <summary>
		/// Modifier / Animation 작업시 다른 Modifier/AnimLayer를 제외시킬 것인가에 대한 타입.
		/// </summary>
		public enum EX_EDIT
		{
			None,
			/// <summary>수동으로 제한시키지 않는한 최소한의 제한만 작동하는 모드</summary>
			General_Edit,
			/// <summary>수동으로 제한하여 1개의 Modifier(ParamSet)/AnimLayer만 허용하는 모드</summary>
			ExOnly_Edit,
		}
		private EX_EDIT _exclusiveEditing = EX_EDIT.None;//해당 모드에서 제한적 에디팅 중인가
		public EX_EDIT ExEditingMode { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _exclusiveEditing; } return EX_EDIT.None; } }




		private bool _isLockExEditKey = false;
		public bool IsLockExEditKey { get { return _isLockExEditKey; } }


		public bool IsExEditable
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.MeshGroup)
				{
					return false;
				}

				if (_meshGroup == null || _modifier == null)
				{
					return false;
				}

				switch (ExEditMode)
				{
					case EX_EDIT_KEY_VALUE.None:
						return false;

					case EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert:
						return (ExKey_ModParamSetGroup != null) && (ExKey_ModParamSet != null)
							//&& (ExKey_ModMesh != null)
							;

					case EX_EDIT_KEY_VALUE.ParamKey_Bone:
					case EX_EDIT_KEY_VALUE.ParamKey_ModMesh:
						return (ExKey_ModParamSetGroup != null) && (ExKey_ModParamSet != null);

					default:
						Debug.LogError("TODO : IsExEditable에 정의되지 않는 타입이 들어왔습니다. [" + ExEditMode + "]");
						break;
				}
				return false;
			}
		}

		//키값으로 사용할 것 - 키로 사용하는 것들
		public apModifierParamSetGroup ExKey_ModParamSetGroup
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert
					|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_Bone
					|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_ModMesh)
				{
					return SubEditedParamSetGroup;
				}
				return null;
			}
		}

		public apModifierParamSet ExKey_ModParamSet
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert
					|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_Bone
					|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_ModMesh)
				{
					return ParamSetOfMod;
				}
				return null;
			}
		}

		public apModifiedMesh ExKey_ModMesh
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert)
				{
					return ModMeshOfMod;
				}
				return null;
			}
		}

		public apModifiedMesh ExValue_ModMesh
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_ModMesh)
				{
					return ModMeshOfMod;
				}
				return null;
			}
		}

		public ModRenderVert ExValue_ModVert
		{
			get
			{
				if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert)
				{
					return ModRenderVertOfMod;
				}
				return null;
			}
		}
		//TODO : 여러개가 선택되었다면?


		//리깅 전용 변수 
		private bool _rigEdit_isBindingEdit = false;//Rig 작업중인가
		private bool _rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가
		public enum RIGGING_EDIT_VIEW_MODE
		{
			WeightColorOnly,
			WeightWithTexture,
		}
		public RIGGING_EDIT_VIEW_MODE _rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightWithTexture;
		public bool _rigEdit_isBoneColorView = true;
		private float _rigEdit_setWeightValue = 0.5f;
		private float _rigEdit_scaleWeightValue = 0.95f;
		public bool _rigEdit_isAutoNormalize = true;
		public bool IsRigEditTestPosing { get { return _rigEdit_isTestPosing; } }
		public bool IsRigEditBinding { get { return _rigEdit_isBindingEdit; } }

		private float _physics_setWeightValue = 0.5f;
		private float _physics_scaleWeightValue = 0.95f;
		private float _physics_windSimulationScale = 1000.0f;
		private Vector2 _physics_windSimulationDir = new Vector2(1.0f, 0.5f);

		/// <summary>
		/// Rigging 시에 "현재 Vertex에 연결된 Bone 정보"를 저장한다.
		/// 복수의 Vertex를 선택할 경우를 대비해서 몇가지 변수가 추가
		/// </summary>
		public class VertRigData
		{
			public apBone _bone = null;
			public int _nRig = 0;
			public float _weight = 0.0f;
			public float _weight_Min = 0.0f;
			public float _weight_Max = 0.0f;
			public VertRigData(apBone bone, float weight)
			{
				_bone = bone;
				_nRig = 1;
				_weight = weight;
				_weight_Min = _weight;
				_weight_Max = _weight;
			}
			public void AddRig(float weight)
			{
				_weight = ((_weight * _nRig) + weight) / (_nRig + 1);
				_nRig++;
				_weight_Min = Mathf.Min(weight, _weight_Min);
				_weight_Max = Mathf.Max(weight, _weight_Max);
			}
		}
		private List<VertRigData> _rigEdit_vertRigDataList = new List<VertRigData>();

		// 애니메이션 선택 정보
		public apAnimTimeline AnimTimeline { get { if (AnimClip != null) { return _subAnimTimeline; } return null; } }
		public apAnimTimelineLayer AnimTimelineLayer { get { if (AnimClip != null) { return _subAnimTimelineLayer; } return null; } }
		public apAnimKeyframe AnimKeyframe { get { if (AnimClip != null) { return _subAnimKeyframe; } return null; } }
		public apAnimKeyframe AnimWorkKeyframe { get { if (AnimTimelineLayer != null) { return _subAnimWorkKeyframe; } return null; } }
		public List<apAnimKeyframe> AnimKeyframes { get { if (AnimClip != null) { return _subAnimKeyframeList; } return null; } }
		public bool IsAnimKeyframeMultipleSelected { get { if (AnimClip != null) { return _subAnimKeyframeList.Count > 1; } return false; } }
		//public bool IsAnimAutoKey						{ get { return _isAnimAutoKey; } }
		//public bool IsAnimEditing { get { return _isAnimEditing; } }//<<ExEditing으로 변경
		public bool IsSelectedKeyframe(apAnimKeyframe keyframe)
		{
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				Debug.LogError("Not Animation Type");
				return false;
			}
			return _subAnimKeyframeList.Contains(keyframe);
		}

		public void CancelAnimEditing() { _exAnimEditingMode = EX_EDIT.None; _isAnimLock = false; }
		public void OnHotKey_AnimEditingLockToggle()
		{
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _exAnimEditingMode == EX_EDIT.None)
			{
				return;
			}
			_isAnimLock = !_isAnimLock;
		}

		public enum ANIM_SINGLE_PROPERTY_UI { Value, Curve }
		public ANIM_SINGLE_PROPERTY_UI _animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Value;

		public enum ANIM_SINGLE_PROPERTY_CURVE_UI { Prev, Next }
		public ANIM_SINGLE_PROPERTY_CURVE_UI _animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Prev;

		public apTransform_Mesh SubMeshTransformOnAnimClip { get { if (AnimClip != null) { return _subMeshTransformOnAnimClip; } return null; } }
		public apTransform_MeshGroup SubMeshGroupTransformOnAnimClip { get { if (AnimClip != null) { return _subMeshGroupTransformOnAnimClip; } return null; } }
		public apControlParam SubControlParamOnAnimClip { get { if (AnimClip != null) { return _subControlParamOnAnimClip; } return null; } }

		public bool IsAnimSelectionLock { get { if (AnimClip != null) { return _isAnimLock; } return false; } }


		//Bone 편집
		private apBone _bone = null;//현재 선택한 Bone (어떤 모드에서든지 참조 가능)
		public apBone Bone { get { return _bone; } }

		private bool _isBoneDefaultEditing = false;
		public bool IsBoneDefaultEditing { get { return _isBoneDefaultEditing; } }

		public enum BONE_EDIT_MODE
		{
			None,
			SelectOnly,
			Add,
			SelectAndTRS,
			Link
		}
		private BONE_EDIT_MODE _boneEditMode = BONE_EDIT_MODE.None;
		//public BONE_EDIT_MODE BoneEditMode { get { if (!_isBoneDefaultEditing) { return BONE_EDIT_MODE.None; } return _boneEditMode; } }
		public BONE_EDIT_MODE BoneEditMode { get { return _boneEditMode; } }

		public enum MESHGROUP_CHILD_HIERARCHY { ChildMeshes, Bones }
		public MESHGROUP_CHILD_HIERARCHY _meshGroupChildHierarchy = MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
		public MESHGROUP_CHILD_HIERARCHY _meshGroupChildHierarchy_Anim = MESHGROUP_CHILD_HIERARCHY.ChildMeshes;



		//public 

		// Init
		//-------------------------------------
		public apSelection(apEditor editor)
		{
			_editor = editor;
			Clear();
		}

		public void Clear()
		{
			_selectionType = SELECTION_TYPE.None;

			_portrait = null;
			_image = null;
			_mesh = null;
			_meshGroup = null;
			_param = null;
			_modifier = null;
			_animClip = null;

			_bone = null;

			_subMeshTransformInGroup = null;
			_subMeshGroupTransformInGroup = null;
			_subEditedParamSetGroup = null;
			_subEditedParamSetGroupAnimPack = null;

			_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
			_exclusiveEditing = EX_EDIT.None;
			_isLockExEditKey = false;

			_renderUnitOfMod = null;
			//_renderVertOfMod = null;

			_modRenderVertOfMod = null;
			_modRenderVertListOfMod.Clear();
			_modRenderVertListOfMod_Weighted.Clear();

			_subMeshTransformListInGroup.Clear();
			_subMeshGroupTransformListInGroup.Clear();

			_isMeshGroupSetting_ChangePivot = false;

			_subAnimTimeline = null;
			_subAnimTimelineLayer = null;
			_subAnimKeyframe = null;
			_subAnimWorkKeyframe = null;

			_subMeshTransformOnAnimClip = null;
			_subMeshGroupTransformOnAnimClip = null;
			_subControlParamOnAnimClip = null;

			_subAnimKeyframeList.Clear();
			//_isAnimEditing = false;
			_exAnimEditingMode = EX_EDIT.None;
			//_isAnimAutoKey = false;
			_isAnimLock = false;

			_modMeshOfAnim = null;
			_modBoneOfAnim = null;
			_renderUnitOfAnim = null;
			_modRenderVertOfAnim = null;
			_modRenderVertListOfAnim.Clear();
			_modRenderVertListOfAnim_Weighted.Clear();

			_isBoneDefaultEditing = false;


			_rigEdit_isBindingEdit = false;//Rig 작업중인가
			_rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가
										  //_rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightWithTexture//<<이건 초기화 안된다.
		}


		// Functions
		//-------------------------------------
		public void SetPortrait(apPortrait portrait)
		{
			if (portrait != _portrait)
			{
				Clear();
				_portrait = portrait;
			}
		}


		public void SetNone()
		{
			_selectionType = SELECTION_TYPE.None;

			//_portrait = null;
			_rootUnit = null;
			_rootUnitAnimClips.Clear();
			_curRootUnitAnimClip = null;

			_image = null;
			_mesh = null;
			_meshGroup = null;
			_param = null;
			_animClip = null;

			_subMeshTransformInGroup = null;
			_subMeshGroupTransformInGroup = null;

			_subMeshTransformListInGroup.Clear();
			_subMeshGroupTransformListInGroup.Clear();

			_modifier = null;

			_isMeshGroupSetting_ChangePivot = false;

			_paramSetOfMod = null;
			_modMeshOfMod = null;
			//_modVertOfMod = null;
			_modBoneOfMod = null;
			_modRegistableBones.Clear();
			//_subControlParamOfMod = null;
			//_subControlParamEditingMod = null;
			_subEditedParamSetGroup = null;
			_subEditedParamSetGroupAnimPack = null;

			_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
			_exclusiveEditing = EX_EDIT.None;
			_isLockExEditKey = false;

			_renderUnitOfMod = null;
			//_renderVertOfMod = null;

			_modRenderVertOfMod = null;
			_modRenderVertListOfMod.Clear();
			_modRenderVertListOfMod_Weighted.Clear();


			_subAnimTimeline = null;
			_subAnimTimelineLayer = null;
			_subAnimKeyframe = null;
			_subAnimWorkKeyframe = null;

			_subMeshTransformOnAnimClip = null;
			_subMeshGroupTransformOnAnimClip = null;
			_subControlParamOnAnimClip = null;

			_subAnimKeyframeList.Clear();
			//_isAnimEditing = false;
			_exAnimEditingMode = EX_EDIT.None;
			//_isAnimAutoKey = false;
			_isAnimLock = false;

			_modMeshOfAnim = null;
			_modBoneOfAnim = null;
			_renderUnitOfAnim = null;
			_modRenderVertOfAnim = null;
			_modRenderVertListOfAnim.Clear();
			_modRenderVertListOfAnim_Weighted.Clear();

			_bone = null;
			_isBoneDefaultEditing = false;

			_rigEdit_isBindingEdit = false;//Rig 작업중인가
			_rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가

			SetBoneRiggingTest();
			Editor.Hierarchy_MeshGroup.ResetSubUnits();

			if (Editor._portrait != null)
			{
				for (int i = 0; i < Editor._portrait._animClips.Count; i++)
				{
					Editor._portrait._animClips[i]._isSelectedInEditor = false;
				}
			}

			Editor.Gizmos.RefreshFFDTransformForce();//<추가

			//기즈모 일단 초기화
			Editor.Gizmos.Unlink();

			GUI.FocusControl(null);

			apEditorUtil.ResetUndo(Editor);//메뉴가 바뀌면 Undo 기록을 초기화한다.
		}


		public void SetImage(apTextureData image)
		{
			SetNone();

			_selectionType = SELECTION_TYPE.ImageRes;

			_image = image;

			//이미지의 Asset 정보는 매번 갱신한다. (언제든 바뀔 수 있으므로)
			if (image._image != null)
			{
				string fullPath = AssetDatabase.GetAssetPath(image._image);
				//Debug.Log("Image Path : " + fullPath);

				if (string.IsNullOrEmpty(fullPath))
				{
					image._assetFullPath = "";
					image._isPSDFile = false;
				}
				else
				{
					image._assetFullPath = fullPath;
					if (fullPath.Contains(".psd") || fullPath.Contains(".PSD"))
					{
						image._isPSDFile = true;
					}
					else
					{
						image._isPSDFile = false;
					}
				}
			}
			else
			{
				image._assetFullPath = "";
				image._isPSDFile = false;
			}
		}

		public void SetMesh(apMesh mesh)
		{
			SetNone();

			_selectionType = SELECTION_TYPE.Mesh;

			_mesh = mesh;
			//_prevMesh_Name = _mesh._name;


		}

		public void SetMeshGroup(apMeshGroup meshGroup)
		{
			SetNone();

			_selectionType = SELECTION_TYPE.MeshGroup;

			bool isChanged = false;
			if (_meshGroup != meshGroup)
			{
				isChanged = true;
			}
			_meshGroup = meshGroup;
			//_prevMeshGroup_Name = _meshGroup._name;

			//_meshGroup.SortRenderUnits(true);//Sort를 다시 해준다. (RenderUnit 세팅때문)
			//Mesh Group을 선택하면 이 초기화를 전부 실행해야한다.

			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_meshGroup);


			_meshGroup.SetDirtyToReset();
			_meshGroup.SetDirtyToSort();
			//_meshGroup.SetAllRenderUnitForceUpdate();
			_meshGroup.RefreshForce(true);//Depth 바뀌었다고 강제한다.



			Editor.Hierarchy_MeshGroup.ResetSubUnits();
			Editor.Hierarchy_MeshGroup.RefreshUnits();

			Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Setting;

			if (isChanged)
			{
				_meshGroup.LinkModMeshRenderUnits();
				_meshGroup.RefreshModifierLink();
				_meshGroup._modifierStack.InitModifierCalculatedValues();//<<값 초기화

				_meshGroup._modifierStack.RefreshAndSort(true);
				Editor.Gizmos.RefreshFFDTransformForce();
			}

			Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshGroupSetting());

			SetModifierExclusiveEditing(EX_EDIT.None);
			SetModifierExclusiveEditKeyLock(false);
			SetModifierEditMode(EX_EDIT_KEY_VALUE.None);
		}




		public void SetSubMeshInGroup(apTransform_Mesh subMeshTransformInGroup)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup)
			{
				_subMeshTransformInGroup = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();

				_subMeshTransformListInGroup.Clear();
				_subMeshGroupTransformListInGroup.Clear();

				return;
			}

			bool isChanged = (_subMeshTransformInGroup != subMeshTransformInGroup);

			_subMeshTransformInGroup = subMeshTransformInGroup;
			_subMeshGroupTransformInGroup = null;

			_subMeshTransformListInGroup.Clear();
			_subMeshTransformListInGroup.Add(_subMeshTransformInGroup);//<<MeshTransform 한개만 넣어주자

			_subMeshGroupTransformListInGroup.Clear();

			//여기서 만약 Modifier 선택중이며, 특정 ParamKey를 선택하고 있다면
			//자동으로 ModifierMesh를 선택해보자
			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}
		}

		public void SetSubMeshGroupInGroup(apTransform_MeshGroup subMeshGroupTransformInGroup)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup)
			{
				_subMeshTransformInGroup = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();

				_subMeshTransformListInGroup.Clear();
				_subMeshGroupTransformListInGroup.Clear();
				return;
			}

			bool isChanged = (_subMeshGroupTransformInGroup != subMeshGroupTransformInGroup);

			_subMeshTransformInGroup = null;
			_subMeshGroupTransformInGroup = subMeshGroupTransformInGroup;


			_subMeshTransformListInGroup.Clear();
			_subMeshGroupTransformListInGroup.Clear();
			_subMeshGroupTransformListInGroup.Add(_subMeshGroupTransformInGroup);//<<MeshGroupTransform 한개만 넣어주자

			//여기서 만약 Modifier 선택중이며, 특정 ParamKey를 선택하고 있다면
			//자동으로 ModifierMesh를 선택해보자
			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}
		}

		public void SetModifier(apModifierBase modifier)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup)
			{
				_modifier = null;
				return;
			}

			bool isChanged = false;
			if (_modifier != modifier || modifier == null)
			{
				_paramSetOfMod = null;
				_modMeshOfMod = null;
				//_modVertOfMod = null;
				_modBoneOfMod = null;
				_modRegistableBones.Clear();
				//_subControlParamOfMod = null;
				//_subControlParamEditingMod = null;
				_subEditedParamSetGroup = null;
				_subEditedParamSetGroupAnimPack = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();

				_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
				_exclusiveEditing = EX_EDIT.None;

				_modifier = modifier;
				isChanged = true;

				_rigEdit_isBindingEdit = false;//Rig 작업중인가
				_rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가



				SetBoneRiggingTest();

			}

			_modifier = modifier;

			if (modifier != null)
			{
				if ((int)(modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
				{
					SetModifierEditMode(EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert);
				}
				else
				{
					SetModifierEditMode(EX_EDIT_KEY_VALUE.ParamKey_ModMesh);
				}
				#region [미사용 코드]
				//switch (modifier.CalculatedValueType)
				//{
				//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.Vertex:
				//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.Vertex_World:
				//		SetModifierEditMode(EXCLUSIVE_EDIT_MODE.ModMeshAndParamKey_ModVert);
				//		break;

				//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.MeshGroup_Transform:
				//		SetModifierEditMode(EXCLUSIVE_EDIT_MODE.ParamKey_ModMesh);
				//		break;

				//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.MeshGroup_Color:
				//		SetModifierEditMode(EXCLUSIVE_EDIT_MODE.ParamKey_ModMesh);
				//		break;

				//	default:
				//		Debug.LogError("TODO : Modfier -> ExEditMode 세팅 필요");
				//		break;
				//} 
				#endregion


				//ParamSetGroup이 선택되어 있다면 Modifier와의 유효성 체크
				bool isSubEditedParamSetGroupInit = false;
				if (_subEditedParamSetGroup != null)
				{
					if (!_modifier._paramSetGroup_controller.Contains(_subEditedParamSetGroup))
					{
						isSubEditedParamSetGroupInit = true;

					}
				}
				else if (_subEditedParamSetGroupAnimPack != null)
				{
					if (!_modifier._paramSetGroupAnimPacks.Contains(_subEditedParamSetGroupAnimPack))
					{
						isSubEditedParamSetGroupInit = true;
					}
				}
				if (isSubEditedParamSetGroupInit)
				{
					_paramSetOfMod = null;
					_modMeshOfMod = null;
					//_modVertOfMod = null;
					_modBoneOfMod = null;
					_modRegistableBones.Clear();
					//_subControlParamOfMod = null;
					//_subControlParamEditingMod = null;
					_subEditedParamSetGroup = null;
					_subEditedParamSetGroupAnimPack = null;

					_renderUnitOfMod = null;
					//_renderVertOfMod = null;

					_modRenderVertOfMod = null;
					_modRenderVertListOfMod.Clear();
					_modRenderVertListOfMod_Weighted.Clear();
				}



				if (MeshGroup != null)
				{
					//Exclusive 모두 해제
					MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
				}


				//각 타입에 따라 Gizmo를 넣어주자
				if (_modifier is apModifier_Morph)
				{
					//Morph
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Morph());
				}
				else if (_modifier is apModifier_TF)
				{
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_TF());
				}
				else if (_modifier is apModifier_Rigging)
				{
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Rigging());
					_rigEdit_isTestPosing = false;//Modifier를 선택하면 TestPosing은 취소된다.

					SetBoneRiggingTest();
				}
				else if (_modifier is apModifier_Physic)
				{
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Physics());
				}
				else
				{
					if (!_modifier.IsAnimated)
					{
						Debug.LogError("Modifier를 선택하였으나 Animation 타입이 아닌데도 Gizmo에 지정되지 않은 타입 : " + _modifier.GetType());
					}
					//아니면 말고 >> Gizmo 초기화
					Editor.Gizmos.Unlink();
				}

				//AutoSelect하기 전에
				//현재 타입이 Static이라면
				//ParamSetGroup/ParamSet은 자동으로 선택한다.
				//ParamSetGroup, ParamSet은 각각 한개씩 존재한다.
				if (_modifier.SyncTarget == apModifierParamSetGroup.SYNC_TARGET.Static)
				{
					apModifierParamSetGroup paramSetGroup = null;
					apModifierParamSet paramSet = null;
					if (_modifier._paramSetGroup_controller.Count == 0)
					{
						Editor.Controller.AddStaticParamSetGroupToModifier();
					}

					paramSetGroup = _modifier._paramSetGroup_controller[0];

					if (paramSetGroup._paramSetList.Count == 0)
					{
						paramSet = new apModifierParamSet();
						paramSet.LinkParamSetGroup(paramSetGroup);
						paramSetGroup._paramSetList.Add(paramSet);
					}

					paramSet = paramSetGroup._paramSetList[0];

					SetParamSetGroupOfModifier(paramSetGroup);
					SetParamSetOfModifier(paramSet);
				}
				else if (!_modifier.IsAnimated)
				{
					if (_subEditedParamSetGroup == null)
					{
						if (_modifier._paramSetGroup_controller.Count > 0)
						{
							//마지막으로 입력된 PSG를 선택
							SetParamSetGroupOfModifier(_modifier._paramSetGroup_controller[_modifier._paramSetGroup_controller.Count - 1]);
						}
					}
					//맨 위의 ParamSetGroup을 선택하자
				}

				if (_modifier.SyncTarget == apModifierParamSetGroup.SYNC_TARGET.Controller)
				{
					Editor._tabLeft = apEditor.TAB_LEFT.Controller;
				}
			}
			else
			{
				if (MeshGroup != null)
				{
					//Exclusive 모두 해제
					MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
				}

				SetModifierEditMode(EX_EDIT_KEY_VALUE.None);

				//아니면 말고 >> Gizmo 초기화
				Editor.Gizmos.Unlink();
			}



			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
		}

		public void SetParamSetGroupOfModifier(apModifierParamSetGroup paramSetGroup)
		{
			//AnimPack 선택은 여기서 무조건 해제된다.
			_subEditedParamSetGroupAnimPack = null;

			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null)
			{
				_subEditedParamSetGroup = null;
				return;
			}
			bool isCheck = false;

			bool isChangedTarget = (_subEditedParamSetGroup != paramSetGroup);
			if (_subEditedParamSetGroup != paramSetGroup)
			{
				_paramSetOfMod = null;
				_modMeshOfMod = null;
				//_modVertOfMod = null;
				_modBoneOfMod = null;
				//_subControlParamOfMod = null;
				//_subControlParamEditingMod = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();

				//_exclusiveEditMode = EXCLUSIVE_EDIT_MODE.None;
				//_isExclusiveEditing = false;

				if (ExEditingMode == EX_EDIT.ExOnly_Edit)
				{
					//SetModifierExclusiveEditing(false);
					SetModifierExclusiveEditing(EX_EDIT.None);
				}

				isCheck = true;
			}
			_subEditedParamSetGroup = paramSetGroup;

			if (isCheck && SubEditedParamSetGroup != null)
			{
				bool isChanged = SubEditedParamSetGroup.RefreshSync();
				if (isChanged)
				{
					MeshGroup.LinkModMeshRenderUnits();//<<이걸 먼저 선언한다.
					MeshGroup.RefreshModifierLink();
				}
			}

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();

			AutoSelectModMeshOrModBone();

			if (isChangedTarget)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}
		}

		/// <summary>
		/// Animated Modifier인 경우, ParamSetGroup 대신 ParamSetGroupAnimPack을 선택하고 보여준다.
		/// </summary>
		public void SetParamSetGroupAnimPackOfModifier(apModifierParamSetGroupAnimPack paramSetGroupAnimPack)
		{
			//일반 선택은 여기서 무조건 해제된다.
			_subEditedParamSetGroup = null;

			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null)
			{
				_subEditedParamSetGroupAnimPack = null;
				return;
			}
			bool isCheck = false;

			bool isChangedTarget = (_subEditedParamSetGroupAnimPack != paramSetGroupAnimPack);
			if (_subEditedParamSetGroupAnimPack != paramSetGroupAnimPack)
			{
				_paramSetOfMod = null;
				_modMeshOfMod = null;
				//_modVertOfMod = null;
				_modBoneOfMod = null;
				//_subControlParamOfMod = null;
				//_subControlParamEditingMod = null;

				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();


				//_exclusiveEditMode = EXCLUSIVE_EDIT_MODE.None;
				//_isExclusiveEditing = false;
				if (ExEditingMode == EX_EDIT.ExOnly_Edit)
				{
					//SetModifierExclusiveEditing(false);
					SetModifierExclusiveEditing(EX_EDIT.None);
				}

				//SetModifierExclusiveEditing(false);

				isCheck = true;
			}
			_subEditedParamSetGroupAnimPack = paramSetGroupAnimPack;

			if (isCheck && SubEditedParamSetGroup != null)
			{
				bool isChanged = SubEditedParamSetGroup.RefreshSync();
				if (isChanged)
				{
					MeshGroup.LinkModMeshRenderUnits();//<<이걸 먼저 선언한다.
					MeshGroup.RefreshModifierLink();
				}
			}

			AutoSelectModMeshOrModBone();

			if (isChangedTarget)
			{
				Editor.Gizmos.RefreshFFDTransformForce();
			}
		}



		public void SetParamSetOfModifier(apModifierParamSet paramSetOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null)
			{
				_paramSetOfMod = null;
				return;
			}

			bool isChanged = false;
			if (_paramSetOfMod != paramSetOfMod)
			{

				//_subControlParamOfMod = null;
				//_subControlParamEditingMod = null;
				//_modMeshOfMod = null;
				//_modVertOfMod = null;
				//_modBoneOfMod = null;
				//_renderUnitOfMod = null;
				//_renderVertOfMod = null;
				isChanged = true;
			}
			_paramSetOfMod = paramSetOfMod;

			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				Editor.Gizmos.RefreshFFDTransformForce();//<추가
			}
		}

		/// <summary>
		/// MeshGroup->Modifier->ParamSetGroup을 선택한 상태에서 ParamSet을 선택하지 않았다면,
		/// Modifier의 종류에 따라 ParamSet을 선택한다. (라고 하지만 Controller 입력 타입만 해당한다..)
		/// </summary>
		public void AutoSelectParamSetOfModifier()
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _portrait == null
				|| _meshGroup == null
				|| _modifier == null
				|| _subEditedParamSetGroup == null
				|| _paramSetOfMod != null)//<<ParamSet이 이미 선택되어도 걍 리턴한다.
			{
				return;
			}
			apModifierParamSet targetParamSet = null;
			switch (_modifier.SyncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					{
						if (_subEditedParamSetGroup._keyControlParam != null)
						{
							apControlParam controlParam = _subEditedParamSetGroup._keyControlParam;
							//해당 ControlParam이 위치한 곳과 같은 값을 가지는 ParamSet이 있으면 이동한다.
							switch (_subEditedParamSetGroup._keyControlParam._valueType)
							{
								case apControlParam.TYPE.Int:
									{
										targetParamSet = _subEditedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return controlParam._int_Cur == a._conSyncValue_Int;
										});

										//선택할만한게 있으면 아예 Control Param값을 동기화
										if (targetParamSet != null)
										{
											controlParam._int_Cur = targetParamSet._conSyncValue_Int;
										}
									}
									break;

								case apControlParam.TYPE.Float:
									{
										float fSnapSize = Mathf.Abs(controlParam._float_Max - controlParam._float_Min) / controlParam._snapSize;
										targetParamSet = _subEditedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return Mathf.Abs(controlParam._float_Cur - a._conSyncValue_Float) < (fSnapSize * 0.25f);
										});

										//선택할만한게 있으면 아예 Control Param값을 동기화
										if (targetParamSet != null)
										{
											controlParam._float_Cur = targetParamSet._conSyncValue_Float;
										}
									}
									break;

								case apControlParam.TYPE.Vector2:
									{
										float vSnapSizeX = Mathf.Abs(controlParam._vec2_Max.x - controlParam._vec2_Min.x) / controlParam._snapSize;
										float vSnapSizeY = Mathf.Abs(controlParam._vec2_Max.y - controlParam._vec2_Min.y) / controlParam._snapSize;

										targetParamSet = _subEditedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return Mathf.Abs(controlParam._vec2_Cur.x - a._conSyncValue_Vector2.x) < (vSnapSizeX * 0.25f)
												&& Mathf.Abs(controlParam._vec2_Cur.y - a._conSyncValue_Vector2.y) < (vSnapSizeY * 0.25f);
										});

										//선택할만한게 있으면 아예 Control Param값을 동기화
										if (targetParamSet != null)
										{
											controlParam._vec2_Cur = targetParamSet._conSyncValue_Vector2;
										}
									}
									break;
							}
						}
					}
					break;
				default:
					//그 외에는.. 적용되는게 없어요
					break;
			}

			if (targetParamSet != null)
			{
				_paramSetOfMod = targetParamSet;

				AutoSelectModMeshOrModBone();

				//Editor.RefreshControllerAndHierarchy();
				Editor.Gizmos.RefreshFFDTransformForce();//<추가
			}

		}

		// Mod-Mesh, Vert, Bone 선택
		public bool SetModMeshOfModifier(apModifiedMesh modMeshOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null || _paramSetOfMod == null)
			{
				_modMeshOfMod = null;
				_modBoneOfMod = null;
				return false;
			}

			if (_modMeshOfMod != modMeshOfMod)
			{
				//_modVertOfMod = null;
				//_renderVertOfMod = null;
				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();
			}
			_modMeshOfMod = modMeshOfMod;
			_modBoneOfMod = null;
			return true;

		}

		public bool SetModBoneOfModifier(apModifiedBone modBoneOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null || _paramSetOfMod == null)
			{
				_modMeshOfMod = null;
				_modBoneOfMod = null;
				return false;
			}

			_modBoneOfMod = modBoneOfMod;

			_modMeshOfMod = null;
			_modRenderVertOfMod = null;
			_modRenderVertListOfMod.Clear();
			_modRenderVertListOfMod_Weighted.Clear();
			return true;
		}

		/// <summary>
		/// Mod-Render Vertex를 선택한다. [Modifier 수정작업시]
		/// ModVert, ModVertRig, ModVertWeight 중 값 하나를 넣어줘야 한다.
		/// </summary>
		/// <param name="modVertOfMod"></param>
		/// <param name="renderVertOfMod"></param>
		public void SetModVertexOfModifier(apModifiedVertex modVertOfMod, apModifiedVertexRig modVertRigOfMod, apModifiedVertexWeight modVertWeight, apRenderVertex renderVertOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| _modMeshOfMod == null)
			{
				//_modVertOfMod = null;
				//_renderVertOfMod = null;
				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();
				return;
			}

			AutoSelectModMeshOrModBone();

			bool isInitReturn = false;
			if (renderVertOfMod == null)
			{
				isInitReturn = true;
			}
			else if (modVertOfMod == null && modVertRigOfMod == null && modVertWeight == null)
			{
				isInitReturn = true;
			}

			//if (modVertOfMod == null || renderVertOfMod == null)
			if (isInitReturn)
			{
				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();
				return;
			}


			//_modVertOfMod = modVertOfMod;
			//_renderVertOfMod = renderVertOfMod;
			bool isChangeModVert = false;
			//기존의 ModRenderVert를 유지할 것인가 또는 새로 선택(생성)할 것인가
			if (_modRenderVertOfMod != null)
			{
				if (_modRenderVertOfMod._renderVert != renderVertOfMod)
				{
					isChangeModVert = true;
				}
				else if (modVertOfMod != null)
				{
					if (_modRenderVertOfMod._modVert != modVertOfMod)
					{
						isChangeModVert = true;
					}
				}
				else if (modVertRigOfMod != null)
				{
					if (_modRenderVertOfMod._modVertRig != modVertRigOfMod)
					{
						isChangeModVert = true;
					}
				}
				else if (modVertWeight != null)
				{
					if (_modRenderVertOfMod._modVertWeight != modVertWeight)
					{
						isChangeModVert = true;
					}
				}
			}
			else
			{
				isChangeModVert = true;
			}

			if (isChangeModVert)
			{
				if (modVertOfMod != null)
				{
					//Vert
					_modRenderVertOfMod = new ModRenderVert(modVertOfMod, renderVertOfMod);
				}
				else if (modVertRigOfMod != null)
				{
					//VertRig
					_modRenderVertOfMod = new ModRenderVert(modVertRigOfMod, renderVertOfMod);
				}
				else
				{
					//VertWeight
					_modRenderVertOfMod = new ModRenderVert(modVertWeight, renderVertOfMod);
				}

				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod.Add(_modRenderVertOfMod);

				_modRenderVertListOfMod_Weighted.Clear();
			}
		}

		/// <summary>
		/// Mod-Render Vertex를 추가한다. [Modifier 수정작업시]
		/// ModVert, ModVertRig, ModVertWeight 중 값 하나를 넣어줘야 한다.
		/// </summary>
		public void AddModVertexOfModifier(apModifiedVertex modVertOfMod, apModifiedVertexRig modVertRigOfMod, apModifiedVertexWeight modVertWeight, apRenderVertex renderVertOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| _modMeshOfMod == null)
			{
				return;
			}

			//AutoSelectModMesh();//<<여기선 생략

			if (renderVertOfMod == null)
			{
				return;
			}

			if (modVertOfMod == null && modVertRigOfMod == null && modVertWeight == null)
			{
				//셋다 없으면 안된다.
				return;
			}
			bool isExistSame = _modRenderVertListOfMod.Exists(delegate (ModRenderVert a)
			{
				return a._renderVert == renderVertOfMod
				|| (a._modVert == modVertOfMod && modVertOfMod != null)
				|| (a._modVertRig == modVertRigOfMod && modVertRigOfMod != null)
				|| (a._modVertWeight == modVertWeight && modVertWeight != null);
			});

			if (!isExistSame)
			{
				ModRenderVert newModRenderVert = null;
				//ModVert에 연동할지, ModVertRig와 연동할지 결정한다.
				if (modVertOfMod != null)
				{
					newModRenderVert = new ModRenderVert(modVertOfMod, renderVertOfMod);
				}
				else if (modVertRigOfMod != null)
				{
					newModRenderVert = new ModRenderVert(modVertRigOfMod, renderVertOfMod);
				}
				else
				{
					newModRenderVert = new ModRenderVert(modVertWeight, renderVertOfMod);
				}

				_modRenderVertListOfMod.Add(newModRenderVert);

				if (_modRenderVertListOfMod.Count == 1)
				{
					_modRenderVertOfMod = newModRenderVert;
				}
			}
		}



		/// <summary>
		/// Mod-Render Vertex를 삭제한다. [Modifier 수정작업시]
		/// ModVert, ModVertRig, ModVertWeight 중 값 하나를 넣어줘야 한다.
		/// </summary>
		public void RemoveModVertexOfModifier(apModifiedVertex modVertOfMod, apModifiedVertexRig modVertRigOfMod, apModifiedVertexWeight modVertWeight, apRenderVertex renderVertOfMod)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| _modMeshOfMod == null)
			{
				return;
			}

			//AutoSelectModMesh();//<<여기선 생략

			if (renderVertOfMod == null)
			{
				return;
			}
			if (modVertOfMod == null && modVertRigOfMod == null && modVertWeight == null)
			{
				//셋다 없으면 안된다.
				return;
			}
			//if (modVertOfMod == null || renderVertOfMod == null)
			//{
			//	return;
			//}

			_modRenderVertListOfMod.RemoveAll(delegate (ModRenderVert a)
			{
				return a._renderVert == renderVertOfMod
				|| (a._modVert == modVertOfMod && modVertOfMod != null)
				|| (a._modVertRig == modVertRigOfMod && modVertRigOfMod != null)
				|| (a._modVertWeight == modVertWeight && modVertWeight != null);
			});

			if (_modRenderVertListOfMod.Count == 1)
			{
				_modRenderVertOfMod = _modRenderVertListOfMod[0];
			}
			else if (_modRenderVertListOfMod.Count == 0)
			{
				_modRenderVertOfMod = null;
			}
			else if (!_modRenderVertListOfAnim.Contains(_modRenderVertOfMod))
			{
				_modRenderVertOfMod = null;
			}
		}




		//MeshTransform(MeshGroupT)이 선택되어있다면 자동으로 ParamSet 내부의 ModMesh를 선택한다.
		public void AutoSelectModMeshOrModBone()
		{
			//0. ParamSet까지 선택이 안되었다면 아무것도 선택 불가
			//1. ModMesh를 선택할 수 있는가
			//2. ModMesh의 유효한 선택이 없다면 ModBone 선택이 가능한가
			//거기에 맞게 처리


			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _meshGroup == null
				|| _modifier == null
				|| _paramSetOfMod == null
				|| _subEditedParamSetGroup == null
				)
			{
				//아무것도 선택하지 못할 경우
				_modMeshOfMod = null;
				_modBoneOfMod = null;
				//_modVertOfMod = null;
				_renderUnitOfMod = null;
				//_renderVertOfMod = null;

				_modRegistableBones.Clear();

				_modRenderVertOfMod = null;
				_modRenderVertListOfMod.Clear();
				_modRenderVertListOfMod_Weighted.Clear();
				//Debug.LogError("AutoSelectModMesh -> Clear 1");
				return;
			}

			//1. ModMesh부터 선택하자
			bool isModMeshSelected = false;

			if (_subMeshTransformInGroup != null || _subMeshGroupTransformInGroup != null)
			{
				//bool isModMeshValid = false;
				for (int i = 0; i < _paramSetOfMod._meshData.Count; i++)
				{
					apModifiedMesh modMesh = _paramSetOfMod._meshData[i];
					if (_subMeshTransformInGroup != null)
					{
						if (modMesh._transform_Mesh == _subMeshTransformInGroup)
						{
							if (SetModMeshOfModifier(modMesh))
							{
								//isModMeshValid = true;
								isModMeshSelected = true;
							}
							break;
						}
					}
					else if (_subMeshGroupTransformInGroup != null)
					{
						if (modMesh._transform_MeshGroup == _subMeshGroupTransformInGroup)
						{
							if (SetModMeshOfModifier(modMesh))
							{
								//isModMeshValid = true;
								isModMeshSelected = true;
							}
							break;
						}
					}
				}

				if (!isModMeshSelected)
				{
					//선택된 ModMesh가 없네용..
					_modMeshOfMod = null;
					_renderUnitOfMod = null;

					_modRenderVertOfMod = null;
					_modRenderVertListOfMod.Clear();
					_modRenderVertListOfMod_Weighted.Clear();
				}



				if (_subMeshTransformInGroup != null)
				{
					apRenderUnit nextSelectUnit = MeshGroup.GetRenderUnit(_subMeshTransformInGroup);
					if (nextSelectUnit != _renderUnitOfMod)
					{
						//_modVertOfMod = null;
						//_renderVertOfMod = null;
						_modRenderVertOfMod = null;
						_modRenderVertListOfMod.Clear();
						_modRenderVertListOfMod_Weighted.Clear();
					}
					_renderUnitOfMod = nextSelectUnit;
				}
				else if (_subMeshGroupTransformInGroup != null)
				{
					apRenderUnit nextSelectUnit = MeshGroup.GetRenderUnit(_subMeshGroupTransformInGroup);
					if (nextSelectUnit != _renderUnitOfMod)
					{
						//_modVertOfMod = null;
						//_renderVertOfMod = null;
						_modRenderVertOfMod = null;
						_modRenderVertListOfMod.Clear();
						_modRenderVertListOfMod_Weighted.Clear();
					}
					_renderUnitOfMod = nextSelectUnit;
				}
				else
				{
					_modMeshOfMod = null;
					//_modVertOfMod = null;
					_renderUnitOfMod = null;
					//_renderVertOfMod = null;

					_modRenderVertOfMod = null;
					_modRenderVertListOfMod.Clear();
					_modRenderVertListOfMod_Weighted.Clear();
					//Debug.LogError("AutoSelectModMesh -> Clear 2");
					isModMeshSelected = false;
				}
			}

			if (!isModMeshSelected)
			{
				_modMeshOfMod = null;
			}
			else
			{
				_modBoneOfMod = null;
			}

			//2. ModMesh 선택한게 없다면 ModBone을 선택해보자
			if (!isModMeshSelected)
			{
				_modBoneOfMod = null;

				if (Bone != null)
				{
					//선택한 Bone이 있다면
					for (int i = 0; i < _paramSetOfMod._boneData.Count; i++)
					{
						apModifiedBone modBone = _paramSetOfMod._boneData[i];
						if (modBone._bone == Bone)
						{
							if (SetModBoneOfModifier(modBone))
							{
								break;
							}
						}
					}
				}
			}

			//추가
			//ModBone으로 선택 가능한 Bone 리스트를 만들어준다.
			_modRegistableBones.Clear();

			for (int i = 0; i < _paramSetOfMod._boneData.Count; i++)
			{
				_modRegistableBones.Add(_paramSetOfMod._boneData[i]._bone);
			}



			//MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
		}



		public bool SetModifierEditMode(EX_EDIT_KEY_VALUE editMode)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null)
			{
				_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
				_exclusiveEditing = EX_EDIT.None;
				return false;
			}

			if (_exEditKeyValue != editMode)
			{
				_exclusiveEditing = EX_EDIT.None;
				_isLockExEditKey = false;

				if (MeshGroup != null)
				{
					//Exclusive 모두 해제
					MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
				}
			}
			_exEditKeyValue = editMode;
			return true;
		}

		public bool SetModifierExclusiveEditing(EX_EDIT exclusiveEditing)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _subEditedParamSetGroup == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				_exclusiveEditing = EX_EDIT.None;
				return false;
			}

			bool isExEditable = IsExEditable;
			if (MeshGroup == null || Modifier == null || SubEditedParamSetGroup == null)
			{
				isExEditable = false;
			}

			if (isExEditable)
			{
				_exclusiveEditing = exclusiveEditing;
			}
			else
			{
				_exclusiveEditing = EX_EDIT.None;
			}

			//작업중인 Modifier 외에는 일부 제외를 하자
			switch (_exclusiveEditing)
			{
				case EX_EDIT.None:
					//모든 Modifier를 활성화한다.
					{
						if (MeshGroup != null)
						{
							//Exclusive 모두 해제
							MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
							Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
						}

						//_modVertOfMod = null;
						//_renderVertOfMod = null;

						_modRenderVertOfMod = null;
						_modRenderVertListOfMod.Clear();
						_modRenderVertListOfMod_Weighted.Clear();
					}
					break;

				case EX_EDIT.General_Edit:
					//연동 가능한 Modifier를 활성화한다.
					MeshGroup._modifierStack.SetExclusiveModifierInEditingGeneral(_modifier);
					break;

				case EX_EDIT.ExOnly_Edit:
					//작업중인 Modifier만 활성화한다.
					MeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, SubEditedParamSetGroup);
					break;
			}



			return true;
		}


		/// <summary>
		/// 단축키 [Space]에 의해서도 SelectionLock(Modifier)를 바꿀 수 있다.
		/// </summary>
		public void OnHotKeyEvent_ToggleExclusiveEditKeyLock()
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				return;
			}
			_isLockExEditKey = !_isLockExEditKey;
		}

		public void SetModifierExclusiveEditKeyLock(bool isLock)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
			{
				_isLockExEditKey = false;
				return;
			}
			_isLockExEditKey = isLock;
		}


		public void SetOverall(apRootUnit rootUnit)
		{
			SetNone();

			if (_rootUnit != rootUnit)
			{
				_curRootUnitAnimClip = null;
			}

			_rootUnitAnimClips.Clear();

			_selectionType = SELECTION_TYPE.Overall;

			if (rootUnit != null)
			{
				_rootUnit = rootUnit;

				//이 RootUnit에 적용할 AnimClip이 뭐가 있는지 확인하자
				for (int i = 0; i < _portrait._animClips.Count; i++)
				{
					apAnimClip animClip = _portrait._animClips[i];
					if (_rootUnit._childMeshGroup == animClip._targetMeshGroup)
					{
						_rootUnitAnimClips.Add(animClip);//<<연동되는 AnimClip이다.
					}
				}

				if (_rootUnit._childMeshGroup != null)
				{
					//Mesh Group을 선택하면 이 초기화를 전부 실행해야한다.
					_rootUnit._childMeshGroup.SetDirtyToReset();
					_rootUnit._childMeshGroup.SetDirtyToSort();
					//_rootUnit._childMeshGroup.SetAllRenderUnitForceUpdate();
					_rootUnit._childMeshGroup.RefreshForce(true);

					_rootUnit._childMeshGroup.LinkModMeshRenderUnits();
					_rootUnit._childMeshGroup.RefreshModifierLink();
					_rootUnit._childMeshGroup._modifierStack.InitModifierCalculatedValues();//<<값 초기화

					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_rootUnit._childMeshGroup);

					_rootUnit._childMeshGroup._modifierStack.RefreshAndSort(true);


				}
			}

			if (_curRootUnitAnimClip != null)
			{
				if (!_rootUnitAnimClips.Contains(_curRootUnitAnimClip))
				{
					_curRootUnitAnimClip = null;//<<이건 포함되지 않습니더
				}
			}

			if (_curRootUnitAnimClip != null)
			{
				_curRootUnitAnimClip._isSelectedInEditor = true;
			}

			Editor.Gizmos.Unlink();

		}





		public void SetParam(apControlParam controlParam)
		{
			SetNone();

			_selectionType = SELECTION_TYPE.Param;

			_param = controlParam;
		}

		public void SetAnimClip(apAnimClip animClip)
		{
			SetNone();

			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip != animClip
				|| _animClip == null)
			{
				Editor.RefreshTimelineLayers(false);
			}

			bool isResetInfo = false;

			for (int i = 0; i < Editor._portrait._animClips.Count; i++)
			{
				Editor._portrait._animClips[i]._isSelectedInEditor = false;
			}

			bool isChanged = false;
			if (_animClip != animClip)
			{
				_animClip = animClip;

				_animClip.Pause_Editor();

				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;
				_subAnimWorkKeyframe = null;

				_subMeshTransformOnAnimClip = null;
				_subMeshGroupTransformOnAnimClip = null;
				_subControlParamOnAnimClip = null;

				_subAnimKeyframeList.Clear();
				//_isAnimEditing = false;
				_exAnimEditingMode = EX_EDIT.None;
				//_isAnimAutoKey = false;
				_isAnimLock = false;

				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				isResetInfo = true;
				isChanged = true;


				if (_animClip._targetMeshGroup != null)
				{
					//Mesh Group을 선택하면 이 초기화를 전부 실행해야한다.
					_animClip._targetMeshGroup.SetDirtyToReset();
					_animClip._targetMeshGroup.SetDirtyToSort();
					//_animClip._targetMeshGroup.SetAllRenderUnitForceUpdate();
					_animClip._targetMeshGroup.RefreshForce(true);

					_animClip._targetMeshGroup.LinkModMeshRenderUnits();
					_animClip._targetMeshGroup.RefreshModifierLink();
					_animClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();//<<값 초기화

					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup);

					_animClip._targetMeshGroup._modifierStack.RefreshAndSort(true);
				}


				Editor.Gizmos.RefreshFFDTransformForce();

			}
			else
			{
				//같은 거라면?
				//패스
			}
			_animClip = animClip;
			_animClip._isSelectedInEditor = true;

			_selectionType = SELECTION_TYPE.Animation;
			_prevAnimClipName = _animClip._name;

			if(isChanged && _animClip != null)
			{
				//타임라인을 자동으로 선택해주자
				if (_animClip._timelines.Count > 0)
				{
					apAnimTimeline firstTimeline = _animClip._timelines[0];
					SetAnimTimeline(firstTimeline, true, true);
				}
			}

			AutoSelectAnimWorkKeyframe();

			if (isResetInfo)
			{
				//Sync를 한번 돌려주자
				_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Value;
				_animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Prev;
				Editor.Controller.AddAndSyncAnimClipToModifier(_animClip);
			}

			Editor.RefreshTimelineLayers(isResetInfo);

			Editor.Hierarchy_AnimClip.ResetSubUnits();
			Editor.Hierarchy_AnimClip.RefreshUnits();

			//Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Morph());

			SetAnimClipGizmoEvent(isResetInfo);//Gizmo 이벤트 연결
		}

		/// <summary>
		/// AnimClip 상태에서 현재 상태에 맞는 GizmoEvent를 등록한다.
		/// </summary>
		private void SetAnimClipGizmoEvent(bool isForceReset)
		{
			if (_animClip == null)
			{
				Editor.Gizmos.Unlink();
				return;
			}

			if (isForceReset)
			{
				Editor.Gizmos.Unlink();

			}

			if (AnimTimeline == null)
			{
				//타임라인이 없으면 선택만 가능하다
				Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_OnlySelectTransform());
			}
			else
			{
				switch (AnimTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						if (AnimTimeline._linkedModifier != null)
						{
							if ((int)(AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
							{
								//Vertex와 관련된 Modifier다.
								Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditVertex());
							}
							else
							{
								//Transform과 관련된 Modifier다.
								Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditTransform());
							}
						}
						else
						{
							Debug.LogError("Error : 선택된 Timeline의 Modifier가 연결되지 않음");
							Editor.Gizmos.Unlink();
						}
						break;

					//이거 삭제하고, 
					//GetEventSet__Animation_EditTransform에서 Bone을 제어하는 코드를 추가하자
					//case apAnimClip.LINK_TYPE.Bone:
					//	Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditBone());
					//	break;

					case apAnimClip.LINK_TYPE.ControlParam:
						//Control Param일땐 선택만 가능
						Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_OnlySelectTransform());
						break;

					default:
						Debug.LogError("TODO : 알 수 없는 Timeline LinkType [" + AnimTimeline._linkType + "]");
						Editor.Gizmos.Unlink();
						break;
				}
			}

		}


		/// <summary>
		/// Animation 편집시 - AnimClip -> Timeline 을 선택한다. (단일 선택)
		/// </summary>
		/// <param name="timeLine"></param>
		public void SetAnimTimeline(apAnimTimeline timeLine, bool isKeyframeSelectReset, bool isIgnoreLock = false)
		{
			if (!isIgnoreLock)
			{
				//현재 작업중 + Lock이 걸리면 바꾸지 못한다.
				if (ExAnimEditingMode != EX_EDIT.None && IsAnimSelectionLock)
				{
					return;
				}
			}



			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				timeLine == null ||
				!_animClip.IsTimelineContain(timeLine))
			{
				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;
				_subAnimWorkKeyframe = null;

				_subAnimKeyframeList.Clear();
				_exAnimEditingMode = EX_EDIT.None;
				//_isAnimAutoKey = false;
				_isAnimLock = false;

				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				AutoSelectAnimWorkKeyframe();
				RefreshAnimEditing(true);

				Editor.RefreshTimelineLayers(false);
				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
				return;
			}

			if (_subAnimTimeline != timeLine)
			{
				_subAnimTimelineLayer = null;
				_subAnimWorkKeyframe = null;

				if (isKeyframeSelectReset)
				{
					_subAnimKeyframe = null;

					_subAnimKeyframeList.Clear();
				}

				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				AutoSelectAnimWorkKeyframe();

				//Editing에서 바꿀 수 있으므로 AnimEditing를 갱신한다.
				RefreshAnimEditing(true);
			}

			_subAnimTimeline = timeLine;


			AutoSelectAnimTimelineLayer();

			Editor.RefreshTimelineLayers(false);

			SetAnimClipGizmoEvent(false);//Gizmo 이벤트 연결

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
			Editor.Hierarchy_AnimClip.RefreshUnits();
		}

		public void SetAnimTimelineLayer(apAnimTimelineLayer timelineLayer, bool isKeyframeSelectReset, bool isAutoSelectTargetObject = false, bool isIgnoreLock = false)
		{
			//현재 작업중+Lock이 걸리면 바꾸지 못한다.
			if (!isIgnoreLock)
			{
				if (ExAnimEditingMode != EX_EDIT.None && IsAnimSelectionLock)
				{
					return;
				}
			}

			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				_subAnimTimeline == null ||
				timelineLayer == null ||
				!_subAnimTimeline.IsTimelineLayerContain(timelineLayer)
				)
			{
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;

				_subAnimKeyframeList.Clear();

				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				AutoSelectAnimWorkKeyframe();

				//Editing에서 바꿀 수 있으므로 AnimEditing를 갱신한다.
				RefreshAnimEditing(true);

				Editor.RefreshTimelineLayers(false);
				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

				return;
			}

			if (_subAnimTimelineLayer != timelineLayer && isKeyframeSelectReset)
			{
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_subAnimTimelineLayer = timelineLayer;

				AutoSelectAnimWorkKeyframe();

				RefreshAnimEditing(true);
			}

			_subAnimTimelineLayer = timelineLayer;

			if (isAutoSelectTargetObject)
			{
				//자동으로 타겟을 정하자
				_subControlParamOnAnimClip = null;
				_subMeshTransformOnAnimClip = null;
				_subMeshGroupTransformOnAnimClip = null;


				if (_subAnimTimelineLayer != null && _subAnimTimelineLayer._parentTimeline != null)
				{
					apAnimTimeline parentTimeline = _subAnimTimelineLayer._parentTimeline;
					switch (parentTimeline._linkType)
					{
						case apAnimClip.LINK_TYPE.AnimatedModifier:
							{
								switch (_subAnimTimelineLayer._linkModType)
								{
									case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
										if (_subAnimTimelineLayer._linkedMeshTransform != null)
										{
											_subMeshTransformOnAnimClip = _subAnimTimelineLayer._linkedMeshTransform;
										}
										break;

									case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
										if (_subAnimTimelineLayer._linkedMeshGroupTransform != null)
										{
											_subMeshGroupTransformOnAnimClip = _subAnimTimelineLayer._linkedMeshGroupTransform;
										}
										break;

									case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
										if (_subAnimTimelineLayer._linkedBone != null)
										{
											_bone = _subAnimTimelineLayer._linkedBone;
										}
										break;

									case apAnimTimelineLayer.LINK_MOD_TYPE.None:
										break;
								}
							}
							break;


						case apAnimClip.LINK_TYPE.ControlParam:
							if (_subAnimTimelineLayer._linkedControlParam != null)
							{
								_subControlParamOnAnimClip = _subAnimTimelineLayer._linkedControlParam;
							}
							break;

						default:
							Debug.LogError("에러 : 알 수 없는 타입 : [" + parentTimeline._linkType + "]");
							break;
					}
				}
			}

			Editor.RefreshTimelineLayers(false);

			SetAnimClipGizmoEvent(false);//Gizmo 이벤트 연결
		}

		/// <summary>
		/// Timeline GUI에서 Keyframe을 선택한다.
		/// AutoSelect를 켜면 선택한 Keyframe에 맞게 다른 TimelineLayer / Timeline을 선택한다.
		/// 단일 선택이므로 "다중 선택"은 항상 현재 선택한 것만 가지도록 한다.
		/// </summary>
		/// <param name="keyframe"></param>
		/// <param name="isTimelineAutoSelect"></param>
		public void SetAnimKeyframe(apAnimKeyframe keyframe, bool isTimelineAutoSelect, apGizmos.SELECT_TYPE selectType, bool isSelectLoopDummy = false)
		{
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;

				_subAnimKeyframeList.Clear();

				AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결

				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

				Editor.RefreshTimelineLayers(false);
				return;
			}

			if (selectType != apGizmos.SELECT_TYPE.New)
			{
				List<apAnimKeyframe> singleKeyframes = new List<apAnimKeyframe>();
				if (keyframe != null)
				{
					singleKeyframes.Add(keyframe);
				}

				SetAnimMultipleKeyframe(singleKeyframes, selectType, isTimelineAutoSelect);
				return;
			}

			if (keyframe == null)
			{
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
				return;
			}

			bool isKeyframeChanged = (keyframe != _subAnimKeyframe);

			if (isTimelineAutoSelect)
			{

				//자동으로 Timeline / Timelayer도 선택할 때
				//if (_subAnimTimelineLayer == null ||
				//	!_subAnimTimelineLayer.IsKeyframeContain(keyframe))
				{
					//Layer가 선택되지 않았거나, 선택된 Layer에 포함되지 않을 때
					apAnimTimelineLayer parentLayer = keyframe._parentTimelineLayer;
					if (parentLayer == null)
					{
						_subAnimKeyframe = null;
						_subAnimKeyframeList.Clear();

						AutoSelectAnimWorkKeyframe();
						return;
					}
					apAnimTimeline parentTimeline = parentLayer._parentTimeline;
					if (parentTimeline == null || !_animClip.IsTimelineContain(parentTimeline))
					{
						//유효하지 않은 타임라인일때
						_subAnimKeyframe = null;
						_subAnimKeyframeList.Clear();

						SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

						AutoSelectAnimWorkKeyframe();
						return;
					}

					//자동으로 체크해주자
					_subAnimTimeline = parentTimeline;
					_subAnimTimelineLayer = parentLayer;

					_subAnimKeyframe = keyframe;

					_subAnimKeyframeList.Clear();
					_subAnimKeyframeList.Add(keyframe);

					AutoSelectAnimWorkKeyframe();
					SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

					Editor.RefreshTimelineLayers(false);
				}
				//else
				//{
				//	_subAnimKeyframe = keyframe;

				//	_subAnimKeyframeList.Clear();
				//	_subAnimKeyframeList.Add(keyframe);

				//	AutoSelectAnimWorkKeyframe();
				//	SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
				//}
			}
			else
			{
				//TimelineLayer에 있는 키프레임만 선택할 때
				if (_subAnimTimeline == null ||
					_subAnimTimelineLayer == null)
				{
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();

					SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
					return;//처리 못함
				}


				if (_subAnimTimelineLayer.IsKeyframeContain(keyframe))
				{
					//Layer에 포함된 Keyframe이다.

					_subAnimKeyframe = keyframe;
					_subAnimKeyframeList.Clear();
					_subAnimKeyframeList.Add(_subAnimKeyframe);
				}
				else
				{
					//Layer에 포함되지 않은 Keyframe이다. => 처리 못함
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();
				}
				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결

			}

			_subAnimKeyframe._parentTimelineLayer.SortAndRefreshKeyframes();

			//if (_subAnimKeyframe != null && Editor._isAnimAutoScroll)
			if (_subAnimKeyframe != null)
			{
				int selectedFrameIndex = _subAnimKeyframe._frameIndex;
				if (_animClip.IsLoop &&
					(selectedFrameIndex < _animClip.StartFrame || selectedFrameIndex > _animClip.EndFrame))
				{
					selectedFrameIndex = _subAnimKeyframe._loopFrameIndex;
				}

				if (selectedFrameIndex >= _animClip.StartFrame
					&& selectedFrameIndex <= _animClip.EndFrame)
				{
					_animClip.SetFrame_Editor(selectedFrameIndex);
				}

				//if (isSelectLoopDummy)
				//{
				//	_animClip.SetFrame_Editor(_subAnimKeyframe._loopFrameIndex);
				//}
				//else
				//{
				//	_animClip.SetFrame_Editor(_subAnimKeyframe._frameIndex);
				//}

				SetAutoAnimScroll();
			}

			AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결
			SetAnimClipGizmoEvent(isKeyframeChanged);//Gizmo 이벤트 연결
		}


		/// <summary>
		/// Keyframe 다중 선택을 한다.
		/// 이때는 Timeline, Timelinelayer는 변동이 되지 않는다. (다만 다중 선택시에는 Timeline, Timelinelayer를 별도로 수정하지 못한다)
		/// </summary>
		/// <param name="keyframes"></param>
		/// <param name="selectType"></param>
		public void SetAnimMultipleKeyframe(List<apAnimKeyframe> keyframes, apGizmos.SELECT_TYPE selectType, bool isTimelineAutoSelect)
		{
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				_subAnimTimeline = null;
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;

				_subAnimKeyframeList.Clear();

				AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결

				SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
				return;
			}

			apAnimKeyframe curKeyframe = null;
			if (selectType == apGizmos.SELECT_TYPE.New)
			{
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();
			}


			//공통의 타임라인을 가지는가
			apAnimTimeline commonTimeline = null;
			apAnimTimelineLayer commonTimelineLayer = null;



			if (isTimelineAutoSelect)
			{
				List<apAnimKeyframe> checkCommonKeyframes = new List<apAnimKeyframe>();
				if (selectType == apGizmos.SELECT_TYPE.Add ||
					selectType == apGizmos.SELECT_TYPE.New)
				{
					for (int i = 0; i < keyframes.Count; i++)
					{
						checkCommonKeyframes.Add(keyframes[i]);
					}
				}

				if (selectType == apGizmos.SELECT_TYPE.Add ||
					selectType == apGizmos.SELECT_TYPE.Subtract)
				{
					//기존에 선택했던것도 추가하자
					for (int i = 0; i < _subAnimKeyframeList.Count; i++)
					{
						checkCommonKeyframes.Add(_subAnimKeyframeList[i]);
					}
				}

				if (selectType == apGizmos.SELECT_TYPE.Subtract)
				{
					//기존에 선택했던 것에서 빼자
					for (int i = 0; i < keyframes.Count; i++)
					{
						checkCommonKeyframes.Remove(keyframes[i]);
					}
				}


				for (int i = 0; i < checkCommonKeyframes.Count; i++)
				{
					curKeyframe = checkCommonKeyframes[i];
					if (commonTimelineLayer == null)
					{
						commonTimelineLayer = curKeyframe._parentTimelineLayer;
						commonTimeline = commonTimelineLayer._parentTimeline;
					}
					else
					{
						if (commonTimelineLayer != curKeyframe._parentTimelineLayer)
						{
							commonTimelineLayer = null;
							break;
						}
					}
				}
			}

			for (int i = 0; i < keyframes.Count; i++)
			{
				curKeyframe = keyframes[i];
				if (curKeyframe == null ||
					curKeyframe._parentTimelineLayer == null ||
					curKeyframe._parentTimelineLayer._parentAnimClip != _animClip)
				{
					continue;
				}

				if (selectType == apGizmos.SELECT_TYPE.Add ||
					selectType == apGizmos.SELECT_TYPE.New)
				{
					//Debug.Log("Add");
					_subAnimKeyframeList.Add(curKeyframe);
				}
				else
				{
					_subAnimKeyframeList.Remove(curKeyframe);
				}
			}

			if (_subAnimKeyframeList.Count > 0)
			{
				if (!_subAnimKeyframeList.Contains(_subAnimKeyframe))
				{
					_subAnimKeyframe = _subAnimKeyframeList[0];
				}
			}
			else
			{
				_subAnimKeyframe = null;
			}

			if (isTimelineAutoSelect)
			{

				if (commonTimelineLayer != null)
				{
					if (commonTimelineLayer != _subAnimTimelineLayer)
					{
						_subAnimTimelineLayer = commonTimelineLayer;

						if (ExAnimEditingMode == EX_EDIT.None)
						{
							_subAnimTimeline = commonTimeline;
						}

						Editor.RefreshTimelineLayers(false);
					}
				}
				else
				{
					_subAnimTimelineLayer = null;
					if (ExAnimEditingMode == EX_EDIT.None)
					{
						_subAnimTimeline = null;
					}

					Editor.RefreshTimelineLayers(false);
				}
			}
			else
			{
				Editor.RefreshTimelineLayers(false);
			}

			List<apAnimTimelineLayer> refreshLayer = new List<apAnimTimelineLayer>();
			for (int i = 0; i < _subAnimKeyframeList.Count; i++)
			{
				if (!refreshLayer.Contains(_subAnimKeyframeList[i]._parentTimelineLayer))
				{
					refreshLayer.Add(_subAnimKeyframeList[i]._parentTimelineLayer);
				}
			}
			for (int i = 0; i < refreshLayer.Count; i++)
			{
				refreshLayer[i].SortAndRefreshKeyframes();
			}

			AutoSelectAnimWorkKeyframe();//<Work+Mod 자동 연결
			SetAnimClipGizmoEvent(true);//Gizmo 이벤트 연결
		}

		private void SetAnimEditingToggle()
		{
			if (ExAnimEditingMode != EX_EDIT.None)
			{
				//>> Off
				//_isAnimEditing = false;
				_exAnimEditingMode = EX_EDIT.None;
				//_isAnimAutoKey = false;
				_isAnimLock = false;
			}
			else
			{
				if (IsAnimEditable)
				{
					//_isAnimEditing = true;//<<편집 시작!
					//_isAnimAutoKey = false;
					_exAnimEditingMode = EX_EDIT.ExOnly_Edit;//<<배타적 Mod 선택이 기본값이다.
					_isAnimLock = true;//기존의 False에서 True로 변경

					bool isVertexTarget = false;
					bool isControlParamTarget = false;
					bool isTransformTarget = false;
					bool isBoneTarget = false;

					//현재 객체가 현재 Timeline에 맞지 않다면 선택을 해제해야한다.
					if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						isControlParamTarget = true;
					}
					else if (_subAnimTimeline._linkedModifier != null)
					{
						if ((int)(_subAnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
						{
							isVertexTarget = true;
							isTransformTarget = true;
						}
						else if (_subAnimTimeline._linkedModifier.IsTarget_Bone)
						{
							isTransformTarget = true;
							isBoneTarget = true;
						}
						else
						{
							isTransformTarget = true;
						}
					}
					else
					{
						//?? 뭘 선택할까요.
						Debug.LogError("Anim Toggle Error : Animation Modifier 타입인데 Modifier가 연결 안됨");
					}

					if (!isVertexTarget)
					{
						_modRenderVertOfAnim = null;
						_modRenderVertListOfAnim.Clear();
					}
					if (!isControlParamTarget)
					{
						_subControlParamOnAnimClip = null;
					}
					if (!isTransformTarget)
					{
						_subMeshTransformOnAnimClip = null;
						_subMeshGroupTransformOnAnimClip = null;
					}
					if (!isBoneTarget)
					{
						_bone = null;
					}

					
				}
			}


			RefreshAnimEditing(true);
		}

		private void SetAnimEditingLayerLockToggle()
		{
			if (ExAnimEditingMode == EX_EDIT.None)
			{
				return;
			}

			if (ExAnimEditingMode == EX_EDIT.ExOnly_Edit)
			{
				_exAnimEditingMode = EX_EDIT.General_Edit;
			}
			else
			{
				_exAnimEditingMode = EX_EDIT.ExOnly_Edit;
			}

			RefreshAnimEditing(true);
		}

		/// <summary>
		/// 애니메이션 작업 도중 타임라인 추가/삭제, 키프레임 추가/삭제/이동과 같은 변동사항이 있을때 호출되어야 하는 함수
		/// </summary>
		public void RefreshAnimEditing(bool isGizmoEventReset)
		{
			if (_animClip == null)
			{
				return;
			}

			//Editing 상태에 따라 Refresh 코드가 다르다
			if (ExAnimEditingMode != EX_EDIT.None)
			{
				//현재 선택한 타임라인에 따라서 Modifier를 On/Off할지 결정한다.
				bool isExclusiveActive = false;
				if (_subAnimTimeline != null)
				{
					if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						if (_subAnimTimeline._linkedModifier != null && _animClip._targetMeshGroup != null)
						{
							if (ExAnimEditingMode == EX_EDIT.ExOnly_Edit)
							{
								//현재의 AnimTimeline에 해당하는 ParamSet만 선택하자
								List<apModifierParamSetGroup> exParamSetGroups = new List<apModifierParamSetGroup>();
								List<apModifierParamSetGroup> linkParamSetGroups = _subAnimTimeline._linkedModifier._paramSetGroup_controller;
								for (int iP = 0; iP < linkParamSetGroups.Count; iP++)
								{
									apModifierParamSetGroup linkPSG = linkParamSetGroups[iP];
									if (linkPSG._keyAnimTimeline == _subAnimTimeline &&
										linkPSG._keyAnimClip == _animClip)
									{
										exParamSetGroups.Add(linkPSG);
									}
								}

								//Debug.Log("Set Anim Editing > Exclusive Enabled [" + _subAnimTimeline._linkedModifier.DisplayName + "]");

								_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup(_subAnimTimeline._linkedModifier, exParamSetGroups);
								isExclusiveActive = true;
							}
							else if (ExAnimEditingMode == EX_EDIT.General_Edit)
							{
								//추가 : General Edit 모드
								//선택한 것과 허용되는 Modifier는 모두 허용한다.
								_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup_General(_subAnimTimeline._linkedModifier, _animClip);
								isExclusiveActive = true;
							}
						}
					}
				}

				if (!isExclusiveActive)
				{
					//Modifier와 연동된게 아니라면
					if (_animClip._targetMeshGroup != null)
					{
						_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
						Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup);
					}
				}
			}
			else
			{
				//모든 Modifier의 Exclusive 선택을 해제하고 모두 활성화한다.
				if (_animClip._targetMeshGroup != null)
				{
					_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup);
				}
			}

			AutoSelectAnimTimelineLayer();
			Editor.RefreshTimelineLayers(false);
			SetAnimClipGizmoEvent(isGizmoEventReset);
		}




		private int _timlineGUIWidth = -1;
		/// <summary>
		/// Is Auto Scroll 옵션이 켜져있으면 스크롤을 자동으로 선택한다.
		/// 재생중에도 스크롤을 움직인다.
		/// </summary>
		public void SetAutoAnimScroll()
		{
			int curFrame = 0;
			int startFrame = 0;
			int endFrame = 0;
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _timlineGUIWidth <= 0)
			{
				return;
			}

			curFrame = _animClip.CurFrame;
			startFrame = _animClip.StartFrame;
			endFrame = _animClip.EndFrame;

			int widthPerFrame = Editor.WidthPerFrameInTimeline;
			int nFrames = Mathf.Max((endFrame - startFrame) + 1, 1);
			int widthForTotalFrame = nFrames * widthPerFrame;
			int widthForScrollFrame = widthForTotalFrame;

			//화면에 보여지는 프레임 범위는?
			int startFrame_Visible = (int)((float)(_scroll_Timeline.x / (float)widthPerFrame) + startFrame);
			int endFrame_Visible = (int)(((float)_timlineGUIWidth / (float)widthPerFrame) + startFrame_Visible);

			int marginFrame = 10;
			int targetFrame = -1;


			startFrame_Visible += marginFrame;
			endFrame_Visible -= marginFrame;

			//"이동해야할 범위와 실제로 이동되는 범위는 다르다"
			if (curFrame < startFrame_Visible)
			{
				//커서가 화면 왼쪽에 붙도록 하자
				targetFrame = curFrame - marginFrame;
			}
			else if (curFrame > endFrame_Visible)
			{
				//커서가 화면 오른쪽에 붙도록 하자
				targetFrame = (curFrame + marginFrame) - (int)((float)_timlineGUIWidth / (float)widthPerFrame);
			}
			else
			{
				return;
			}

			targetFrame -= startFrame;
			float nextScroll = Mathf.Clamp((targetFrame * widthPerFrame), 0, widthForScrollFrame);
			//Debug.Log("Auto Scroll [Curframe : " + curFrame + " [, Scroll Frame : " + targetFrame + "[" + startFrame_Visible + " ~ " + endFrame_Visible + "], " +
			//	"Scroll : " + _scroll_Timeline.x + " >> " + nextScroll);

			_scroll_Timeline.x = nextScroll;
		}

		/// <summary>
		/// AnimClip 작업을 위해 MeshTransform을 선택한다.
		/// 해당 데이터가 Timeline에 없어도 선택 가능하다.
		/// </summary>
		/// <param name="meshTransform"></param>
		public void SetSubMeshTransformForAnimClipEdit(apTransform_Mesh meshTransform)
		{
			if (meshTransform != null)
			{
				_bone = null;
			}
			_subMeshGroupTransformOnAnimClip = null;
			_subControlParamOnAnimClip = null;

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_subMeshTransformOnAnimClip = null;
				return;
			}
			_subMeshTransformOnAnimClip = meshTransform;

			AutoSelectAnimTimelineLayer();
		}

		/// <summary>
		/// AnimClip 작업을 위해 MeshGroupTransform을 선택한다.
		/// 해당 데이터가 Timeline에 없어도 선택 가능하다.
		/// </summary>
		/// <param name="meshGroupTransform"></param>
		public void SetSubMeshGroupTransformForAnimClipEdit(apTransform_MeshGroup meshGroupTransform)
		{
			if (meshGroupTransform != null)
			{
				_bone = null;
			}
			_subMeshTransformOnAnimClip = null;
			_subControlParamOnAnimClip = null;

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_subMeshGroupTransformOnAnimClip = null;
				return;
			}

			_subMeshGroupTransformOnAnimClip = meshGroupTransform;

			AutoSelectAnimTimelineLayer();
		}

		/// <summary>
		/// AnimClip 작업을 위해 Control Param을 선택한다.
		/// 해당 데이터가 Timeline에 없어도 선택 가능하다
		/// </summary>
		/// <param name="controlParam"></param>
		public void SetSubControlParamForAnimClipEdit(apControlParam controlParam)
		{
			_bone = null;
			_subMeshTransformOnAnimClip = null;
			_subMeshGroupTransformOnAnimClip = null;

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_subControlParamOnAnimClip = null;
				return;
			}

			_subControlParamOnAnimClip = controlParam;

			AutoSelectAnimTimelineLayer();
		}

		/// <summary>
		/// 선택된 객체(Transform/Bone/ControlParam) 중에서 "현재 타임라인"이 선택할 수 있는 객체를 리턴한다.
		/// </summary>
		/// <returns></returns>
		public object GetSelectedAnimTimelineObject()
		{
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				_subAnimTimeline == null)
			{
				return null;
			}

			switch (_subAnimTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					if (SubMeshTransformOnAnimClip != null)
					{
						return SubMeshTransformOnAnimClip;
					}
					if (SubMeshGroupTransformOnAnimClip != null)
					{
						return SubMeshGroupTransformOnAnimClip;
					}
					if (Bone != null)
					{
						return Bone;
					}
					break;

				case apAnimClip.LINK_TYPE.ControlParam:
					if (SubControlParamOnAnimClip != null)
					{
						return SubControlParamOnAnimClip;
					}
					break;

			}
			return null;
		}


		/// <summary>
		/// 현재 선택한 Sub 객체 (Transform, Bone, ControlParam)에 따라서
		/// 자동으로 Timeline의 Layer를 선택해준다.
		/// </summary>
		public void AutoSelectAnimTimelineLayer()
		{
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				_subAnimTimeline == null)
			{
				
				
				_subAnimTimelineLayer = null;
				_subAnimKeyframe = null;
				_subAnimWorkKeyframe = null;

				_subAnimKeyframeList.Clear();

				AutoSelectAnimWorkKeyframe();
				return;
			}

			_subAnimWorkKeyframe = null;

			//timeline이 ControlParam계열이라면 에디터의 탭을 변경
			if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
			{
				Editor._tabLeft = apEditor.TAB_LEFT.Controller;
			}

			//자동으로 스크롤을 해주자
			_isAnimTimelineLayerGUIScrollRequest = true;

			//다중 키프레임 작업중에는 단일 선택 불가
			if (_subAnimKeyframeList.Count > 1)
			{
				
				AutoSelectAnimWorkKeyframe();
				return;
			}




			object selectedObject = GetSelectedAnimTimelineObject();
			if (selectedObject == null)
			{
				AutoSelectAnimWorkKeyframe();
				if (AnimWorkKeyframe == null)
				{
					SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
				}

				//Debug.LogError("Object Select -> 선택된 객체가 없다.");
				return;//선택된게 없다면 일단 패스
			}


			apAnimTimelineLayer nextLayer = null;
			nextLayer = _subAnimTimeline.GetTimelineLayer(selectedObject);
			if (nextLayer != null)
			{	
				SetAnimTimelineLayer(nextLayer, false);
			}

			#region [미사용 코드]
			////만약 이미 선택된 레이어가 있다면 "유효"한지 테스트한다
			//bool isLayerIsAlreadySelected = false;
			//if (_subAnimTimelineLayer != null)
			//{
			//	if (_subAnimTimelineLayer.IsContainTargetObject(selectedObject))
			//	{
			//		//현재 레이어에 포함되어 있다면 패스
			//		//AutoSelectAnimWorkKeyframe();
			//		//return;
			//		isLayerIsAlreadySelected = true;
			//	}
			//}
			//apAnimTimelineLayer nextLayer = null;
			//if (!isLayerIsAlreadySelected)
			//{
			//	nextLayer = _subAnimTimeline.GetTimelineLayer(selectedObject);

			//	if (nextLayer != null)
			//	{
			//		SetAnimTimelineLayer(nextLayer, false);
			//	}
			//}
			//else
			//{
			//	nextLayer = _subAnimTimelineLayer;
			//} 
			#endregion


			AutoSelectAnimWorkKeyframe();

			//여기서는 아예 Work Keyframe 뿐만아니라 Keyframe으로도 선택을 한다.
			SetAnimKeyframe(AnimWorkKeyframe, false, apGizmos.SELECT_TYPE.New);

			_modRegistableBones.Clear();//<<이것도 갱신해주자 [타입라인에 등록된 Bone]
			if (_subAnimTimeline != null)
			{
				for (int i = 0; i < _subAnimTimeline._layers.Count; i++)
				{
					apAnimTimelineLayer timelineLayer = _subAnimTimeline._layers[i];
					if (timelineLayer._linkedBone != null)
					{
						_modRegistableBones.Add(timelineLayer._linkedBone);
					}
				}

			}
		}

		/// <summary>
		/// 현재 재생중인 프레임에 맞게 WorkKeyframe을 자동으로 선택한다.
		/// 키프레임을 바꾸거나 레이어를 바꿀때 자동으로 호출한다.
		/// 수동으로 선택하는 키프레임과 다르다.
		/// </summary>
		public void AutoSelectAnimWorkKeyframe()
		{
			Editor.Gizmos.SetUpdate();

			apAnimKeyframe prevWorkKeyframe = _subAnimWorkKeyframe;
			if (_subAnimTimelineLayer == null || IsAnimPlaying)//<<플레이 중에는 모든 선택이 초기화된다.
			{
				
				if (_subAnimWorkKeyframe != null)
				{
					_subAnimWorkKeyframe = null;
					_modMeshOfAnim = null;
					_modBoneOfAnim = null;
					_renderUnitOfAnim = null;
					_modRenderVertOfAnim = null;
					_modRenderVertListOfAnim.Clear();
					_modRenderVertListOfAnim_Weighted.Clear();

					//추가 : 기즈모 갱신이 필요한 경우 (주로 FFD)
					Editor.Gizmos.RefreshFFDTransformForce();
				}

				Editor.Hierarchy_AnimClip.RefreshUnits();
				return;
			}
			int curFrame = _animClip.CurFrame;
			_subAnimWorkKeyframe = _subAnimTimelineLayer.GetKeyframeByFrameIndex(curFrame);

			if (_subAnimWorkKeyframe == null)
			{
				
				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();

				Editor.Gizmos.RefreshFFDTransformForce();//<기즈모 갱신

				Editor.Hierarchy_AnimClip.RefreshUnits();
				return;
			}

			bool isResetMod = true;
			//if (_subAnimWorkKeyframe != prevWorkKeyframe)//강제
			{
				if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					
					if (_subAnimTimeline._linkedModifier != null)
					{
						
						apModifierParamSet targetParamSet = _subAnimTimeline.GetModifierParamSet(_subAnimTimelineLayer, _subAnimWorkKeyframe);
						if (targetParamSet != null)
						{
							if (targetParamSet._meshData.Count > 0)
							{
								
								isResetMod = false;
								//중요!
								//>>여기서 Anim용 ModMesh를 선택한다.<<
								_modMeshOfAnim = targetParamSet._meshData[0];
								if (_modMeshOfAnim._transform_Mesh != null)
								{
									_renderUnitOfAnim = _animClip._targetMeshGroup.GetRenderUnit(_modMeshOfAnim._transform_Mesh);
								}
								else if (_modMeshOfAnim._transform_MeshGroup != null)
								{
									_renderUnitOfAnim = _animClip._targetMeshGroup.GetRenderUnit(_modMeshOfAnim._transform_MeshGroup);
								}
								else
								{
									_renderUnitOfAnim = null;
								}

								_modRenderVertOfAnim = null;
								_modRenderVertListOfAnim.Clear();
								_modRenderVertListOfAnim_Weighted.Clear();

								_modBoneOfAnim = null;//<<Mod Bone은 선택 해제
							}
							else if (targetParamSet._boneData.Count > 0)
							{
								
								isResetMod = false;

								//ModBone이 있다면 그걸 선택하자
								_modBoneOfAnim = targetParamSet._boneData[0];
								_renderUnitOfAnim = _modBoneOfAnim._renderUnit;
								if (_modBoneOfAnim != null)
								{
									_bone = _modBoneOfAnim._bone;
								}


								//Mod Mesh 변수는 초기화
								_modMeshOfAnim = null;
								_modRenderVertOfAnim = null;
								_modRenderVertListOfAnim.Clear();
								_modRenderVertListOfAnim_Weighted.Clear();
							}
						}
						

					}
				}
			}
			//else
			//{
			//	//변동된 것이 없다.
			//	isResetMod = false;
			//}

			if (isResetMod)
			{
				_modMeshOfAnim = null;
				_modBoneOfAnim = null;
				_renderUnitOfAnim = null;
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();
			}

			Editor.Gizmos.RefreshFFDTransformForce();//<기즈모 갱신

			Editor.Hierarchy_AnimClip.RefreshUnits();
		}


		/// <summary>
		/// Mod-Render Vertex를 선택한다. [Animation 수정작업시]
		/// </summary>
		/// <param name="modVertOfAnim">Modified Vertex of Anim Keyframe</param>
		/// <param name="renderVertOfAnim">Render Vertex of Anim Keyframe</param>
		public void SetModVertexOfAnim(apModifiedVertex modVertOfAnim, apRenderVertex renderVertOfAnim)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe == null
				|| ModMeshOfAnim == null)
			{
				return;
			}

			if (modVertOfAnim == null || renderVertOfAnim == null)
			{
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();
				return;
			}

			if (ModMeshOfAnim != modVertOfAnim._modifiedMesh)
			{
				_modRenderVertOfAnim = null;
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim_Weighted.Clear();
				return;
			}
			bool isChangeModVert = false;
			if (_modRenderVertOfAnim != null)
			{
				if (_modRenderVertOfAnim._modVert != modVertOfAnim || _modRenderVertOfAnim._renderVert != renderVertOfAnim)
				{
					isChangeModVert = true;
				}
			}
			else
			{
				isChangeModVert = true;
			}

			if (isChangeModVert)
			{
				_modRenderVertOfAnim = new ModRenderVert(modVertOfAnim, renderVertOfAnim);
				_modRenderVertListOfAnim.Clear();
				_modRenderVertListOfAnim.Add(_modRenderVertOfAnim);

				_modRenderVertListOfAnim_Weighted.Clear();

			}
		}



		/// <summary>
		/// Mod-Render Vertex를 추가한다. [Animation 수정작업시]
		/// </summary>
		public void AddModVertexOfAnim(apModifiedVertex modVertOfAnim, apRenderVertex renderVertOfAnim)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe == null
				|| ModMeshOfAnim == null)
			{
				return;
			}

			if (modVertOfAnim == null || renderVertOfAnim == null)
			{
				//추가/제거없이 생략
				return;
			}

			bool isExistSame = _modRenderVertListOfAnim.Exists(delegate (ModRenderVert a)
			{
				return a._modVert == modVertOfAnim || a._renderVert == renderVertOfAnim;
			});

			if (!isExistSame)
			{
				//새로 생성+추가해야할 필요가 있다.
				ModRenderVert newModRenderVert = new ModRenderVert(modVertOfAnim, renderVertOfAnim);
				_modRenderVertListOfAnim.Add(newModRenderVert);

				if (_modRenderVertListOfAnim.Count == 1)
				{
					_modRenderVertOfAnim = newModRenderVert;
				}
			}
		}

		/// <summary>
		/// Mod-Render Vertex를 삭제한다. [Animation 수정작업시]
		/// </summary>
		public void RemoveModVertexOfAnim(apModifiedVertex modVertOfAnim, apRenderVertex renderVertOfAnim)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe == null
				|| ModMeshOfAnim == null)
			{
				return;
			}

			if (modVertOfAnim == null || renderVertOfAnim == null)
			{
				//추가/제거없이 생략
				return;
			}

			_modRenderVertListOfAnim.RemoveAll(delegate (ModRenderVert a)
			{
				return a._modVert == modVertOfAnim || a._renderVert == renderVertOfAnim;
			});

			if (_modRenderVertListOfAnim.Count == 1)
			{
				_modRenderVertOfAnim = _modRenderVertListOfAnim[0];
			}
			else if (_modRenderVertListOfAnim.Count == 0)
			{
				_modRenderVertOfAnim = null;
			}
			else if (!_modRenderVertListOfAnim.Contains(_modRenderVertOfAnim))
			{
				_modRenderVertOfAnim = null;
			}

		}





		public void SetBone(apBone bone)
		{
			_bone = bone;
			if (SelectionType == SELECTION_TYPE.MeshGroup &&
				Modifier != null)
			{
				AutoSelectModMeshOrModBone();
			}
			if (SelectionType == SELECTION_TYPE.Animation && AnimClip != null)
			{
				AutoSelectAnimTimelineLayer();
			}
		}

		/// <summary>
		/// AnimClip 작업시 Bone을 선택하면 SetBone대신 이 함수를 호출한다.
		/// </summary>
		/// <param name="bone"></param>
		public void SetBoneForAnimClip(apBone bone)
		{
			_bone = bone;

			if (bone != null)
			{
				_subControlParamOnAnimClip = null;
				_subMeshTransformOnAnimClip = null;
				_subMeshGroupTransformOnAnimClip = null;
			}

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_bone = null;
				return;
			}

			SetAnimTimelineLayer(null, true);//TImelineLayer의 선택을 취소해야 AutoSelect가 정상작동한다.
			AutoSelectAnimTimelineLayer();
			if(_bone != bone && bone != null)
			{
				//bone은 유지하자
				_bone = bone;
				_modBoneOfAnim = null;
			}
		}

		/// <summary>
		/// isEditing : Default Matrix를 수정하는가
		/// isBoneMenu : 현재 Bone Menu인가
		/// </summary>
		/// <param name="isEditing"></param>
		/// <param name="isBoneMenu"></param>
		public void SetBoneEditing(bool isEditing, bool isBoneMenu)
		{
			bool isChanged = _isBoneDefaultEditing != isEditing;

			_isBoneDefaultEditing = isEditing;

			//if (isChanged)
			{
				if (_isBoneDefaultEditing)
				{
					SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, isBoneMenu);
					//Debug.LogError("TODO : Default Bone Tranform을 활성화할 때에는 다른 Rig Modifier를 꺼야한다.");

					//Editor.Gizmos.LinkObject()
				}
				else
				{
					if (isBoneMenu)
					{
						SetBoneEditMode(BONE_EDIT_MODE.SelectOnly, isBoneMenu);
					}
					else
					{
						SetBoneEditMode(BONE_EDIT_MODE.None, isBoneMenu);
					}
					//Debug.LogError("TODO : Default Bone Tranform을 종료할 때에는 다른 Rig Modifier를 켜야한다.");
				}
			}
		}

		public void SetBoneEditMode(BONE_EDIT_MODE boneEditMode, bool isBoneMenu)
		{
			_boneEditMode = boneEditMode;

			if (!_isBoneDefaultEditing)
			{
				if (isBoneMenu)
				{
					_boneEditMode = BONE_EDIT_MODE.SelectOnly;
				}
				else
				{
					_boneEditMode = BONE_EDIT_MODE.None;
				}
			}

			Editor.Controller.SetBoneEditInit();
			//Gizmo 이벤트를 설정하자
			switch (_boneEditMode)
			{
				case BONE_EDIT_MODE.None:
					Editor.Gizmos.Unlink();
					break;

				case BONE_EDIT_MODE.SelectOnly:
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Bone_SelectOnly());
					break;

				case BONE_EDIT_MODE.SelectAndTRS:
					//Select에서는 Gizmo 이벤트를 받는다.
					//Transform 제어를 해야하기 때문
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Bone_Default());
					break;

				case BONE_EDIT_MODE.Add:
					Editor.Gizmos.Unlink();
					break;

				case BONE_EDIT_MODE.Link:
					Editor.Gizmos.Unlink();
					break;
			}
		}

		/// <summary>
		/// Rigging시 Pose Test를 하는지 여부를 설정한다.
		/// 모든 MeshGroup에 대해서 설정한다.
		/// _rigEdit_isTestPosing값을 먼저 설정한다.
		/// </summary>
		public void SetBoneRiggingTest()
		{
			if (Editor._portrait == null)
			{
				return;
			}
			for (int i = 0; i < Editor._portrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[i];
				meshGroup.SetBoneRiggingTest(_rigEdit_isTestPosing);
			}
		}

		/// <summary>
		/// Rigging시, Test중인 Pose를 리셋한다.
		/// </summary>
		public void ResetRiggingTestPose()
		{
			if (Editor._portrait == null)
			{
				return;
			}
			for (int i = 0; i < Editor._portrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[i];
				meshGroup.ResetRiggingTestPose();
			}
			Editor.RefreshControllerAndHierarchy();
			Editor.SetRepaint();
		}

		// Editor View
		//-------------------------------------
		public bool DrawEditor(int width, int height)
		{
			if (_portrait == null)
			{
				//Debug.LogError("Selection Portrait is Null");
				return false;
			}
			//EditorGUILayout.LabelField("Properties");

			//EditorGUILayout.Space();
			EditorGUILayout.Space();
			switch (_selectionType)
			{
				case SELECTION_TYPE.None:
					Draw_None(width, height);
					break;

				case SELECTION_TYPE.ImageRes:
					Draw_ImageRes(width, height);
					break;
				case SELECTION_TYPE.Mesh:
					Draw_Mesh(width, height);
					break;
				case SELECTION_TYPE.Face:
					Draw_Face(width, height);
					break;
				case SELECTION_TYPE.MeshGroup:
					Draw_MeshGroup(width, height);
					break;
				case SELECTION_TYPE.Animation:
					Draw_Animation(width, height);
					break;
				case SELECTION_TYPE.Overall:
					Draw_Overall(width, height);
					break;
				case SELECTION_TYPE.Param:
					Draw_Param(width, height);
					break;
			}

			EditorGUILayout.Space();

			return true;
		}

		//private apPortrait _portrait = null;
		//private apTextureData _image = null;
		//private apMesh _mesh = null;
		public void DrawEditor_Header(int width, int height)
		{
			switch (_selectionType)
			{
				case SELECTION_TYPE.None:
					DrawTitle("Not Selected", width);
					break;

				case SELECTION_TYPE.ImageRes:
					DrawTitle("Image", width);
					break;
				case SELECTION_TYPE.Mesh:
					DrawTitle("Mesh", width);
					break;
				case SELECTION_TYPE.Face:
					DrawTitle("Face", width);
					break;
				case SELECTION_TYPE.MeshGroup:
					DrawTitle("Mesh Group", width);
					break;
				case SELECTION_TYPE.Animation:
					DrawTitle("Animation", width);
					break;
				case SELECTION_TYPE.Overall:
					DrawTitle("Overall", width);
					break;
				case SELECTION_TYPE.Param:
					DrawTitle("Parameter", width);
					break;
			}
		}

		//public apPortrait Portrait { get { return _portrait; } }
		//public apTextureData TextureData {  get { if (_selectionType == SELECTION_TYPE.ImageRes) { return _image; } return null; } }
		//public apMesh Mesh {  get { if (_selectionType 

		private void Draw_None(int width, int height)
		{
			//GUILayout.Box("Not Selected", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Not Selected", width);
			EditorGUILayout.Space();
		}

		private void Draw_ImageRes(int width, int height)
		{
			//GUILayout.Box("Image", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Image", width);
			EditorGUILayout.Space();

			apTextureData textureData = _image;
			if (textureData == null)
			{
				SetNone();
				return;
			}

			Texture2D prevImage = textureData._image;

			EditorGUILayout.LabelField("Image Asset");



			//textureData._image = EditorGUILayout.ObjectField(textureData._image, typeof(Texture2D), true, GUILayout.Width(width), GUILayout.Height(50)) as Texture2D;
			textureData._image = EditorGUILayout.ObjectField(textureData._image, typeof(Texture2D), true) as Texture2D;

			if (GUILayout.Button("Select Image", GUILayout.Height(30)))
			{
				_loadKey_SelectTextureAsset = apDialog_SelectTextureAsset.ShowDialog(Editor, textureData, OnTextureAssetSelected);
			}

			if (textureData._image != prevImage)
			{
				//이미지가 추가되었다.
				if (textureData._image != null)
				{
					//Undo
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Image_SettingChanged, Editor._portrait, textureData._image, false, Editor);

					textureData._name = textureData._image.name;
					textureData._width = textureData._image.width;
					textureData._height = textureData._image.height;

					//이미지 에셋의 Path를 확인하고, PSD인지 체크한다.
					if (textureData._image != null)
					{
						string fullPath = AssetDatabase.GetAssetPath(textureData._image);
						//Debug.Log("Image Path : " + fullPath);

						if (string.IsNullOrEmpty(fullPath))
						{
							textureData._assetFullPath = "";
							textureData._isPSDFile = false;
						}
						else
						{
							textureData._assetFullPath = fullPath;
							if (fullPath.Contains(".psd") || fullPath.Contains(".PSD"))
							{
								textureData._isPSDFile = true;
							}
							else
							{
								textureData._isPSDFile = false;
							}
						}
					}
					else
					{
						textureData._assetFullPath = "";
						textureData._isPSDFile = false;
					}
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy();
			}

			EditorGUILayout.Space();


			EditorGUILayout.LabelField("Name");
			string nextName = EditorGUILayout.DelayedTextField(textureData._name);

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Size");
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Width", GUILayout.Width(40));
			int nextWidth = EditorGUILayout.DelayedIntField(textureData._width);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Height", GUILayout.Width(40));
			int nextHeight = EditorGUILayout.DelayedIntField(textureData._height);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();


			//변경값이 있으면 Undo 등록하고 변경
			if (!string.Equals(nextName, textureData._name) ||
				nextWidth != textureData._width ||
				nextHeight != textureData._height)
			{
				//Undo
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Image_SettingChanged, Editor._portrait, textureData, false, Editor);

				textureData._name = nextName;
				textureData._width = nextWidth;
				textureData._height = nextHeight;
			}



			if (GUILayout.Button("Refresh Image Property", GUILayout.Height(30)))
			{
				//Undo
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Image_SettingChanged, Editor._portrait, textureData, false, Editor);

				if (textureData._image != null)
				{
					textureData._name = textureData._image.name;
					textureData._width = textureData._image.width;
					textureData._height = textureData._image.height;
				}
				else
				{
					textureData._name = "";
					textureData._width = 0;
					textureData._height = 0;
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy();
			}




			// Remove
			GUILayout.Space(30);
			if (GUILayout.Button("Remove Image"))
			{

				//bool isResult = EditorUtility.DisplayDialog("Remove Image", "Do you want to remove [" + textureData._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveImage_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveImage_Body, textureData._name),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel));


				if (isResult)
				{
					Editor.Controller.RemoveTexture(textureData);
					//_portrait._textureData.Remove(textureData);

					SetNone();
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy();
			}
		}

		private object _loadKey_SelectTextureAsset = null;
		private void OnTextureAssetSelected(bool isSuccess, apTextureData targetTextureData, object loadKey, Texture2D resultTexture2D)
		{
			if (_loadKey_SelectTextureAsset != loadKey || !isSuccess)
			{
				_loadKey_SelectTextureAsset = null;
				return;
			}
			_loadKey_SelectTextureAsset = null;
			if (targetTextureData == null)
			{
				return;
			}

			targetTextureData._image = resultTexture2D;

			//이미지가 추가되었다.
			if (targetTextureData._image != null)
			{
				//Undo
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Image_SettingChanged, Editor._portrait, targetTextureData._image, false, Editor);

				targetTextureData._name = targetTextureData._image.name;
				targetTextureData._width = targetTextureData._image.width;
				targetTextureData._height = targetTextureData._image.height;

				//이미지 에셋의 Path를 확인하고, PSD인지 체크한다.
				if (targetTextureData._image != null)
				{
					string fullPath = AssetDatabase.GetAssetPath(targetTextureData._image);
					//Debug.Log("Image Path : " + fullPath);

					if (string.IsNullOrEmpty(fullPath))
					{
						targetTextureData._assetFullPath = "";
						targetTextureData._isPSDFile = false;
					}
					else
					{
						targetTextureData._assetFullPath = fullPath;
						if (fullPath.Contains(".psd") || fullPath.Contains(".PSD"))
						{
							targetTextureData._isPSDFile = true;
						}
						else
						{
							targetTextureData._isPSDFile = false;
						}
					}
				}
				else
				{
					targetTextureData._assetFullPath = "";
					targetTextureData._isPSDFile = false;
				}
			}
			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy();
		}





		private bool _isShowTextureDataList = false;
		private void Draw_Mesh(int width, int height)
		{
			//GUILayout.Box("Mesh", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Mesh", width);
			EditorGUILayout.Space();

			if (_mesh == null)
			{
				SetNone();
				return;
			}

			//탭
			bool isEditMeshMode_None = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.Setting);
			bool isEditMeshMode_MakeMesh = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.MakeMesh);
			bool isEditMeshMode_Modify = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.Modify);

			//bool isEditMeshMode_AddVertex = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.AddVertex);
			//bool isEditMeshMode_LinkEdge = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.LinkEdge);

			bool isEditMeshMode_Pivot = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.PivotEdit);
			//bool isEditMeshMode_Volume = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.VolumeWeight);
			//bool isEditMeshMode_Physic = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.PhysicWeight);

			int subTabWidth = (width / 2) - 5;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//int tabBtnHeight = 30;
			if (apEditorUtil.ToggledButton("Setting", isEditMeshMode_None, subTabWidth))
			{
				if (!isEditMeshMode_None)
				{
					Editor.Controller.CheckMeshEdgeWorkRemained();
					Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Setting;
				}
			}
			if (apEditorUtil.ToggledButton("Mesh Edit", isEditMeshMode_MakeMesh, subTabWidth))
			{
				if (!isEditMeshMode_MakeMesh)
				{
					Editor.Controller.CheckMeshEdgeWorkRemained();
					Editor._meshEditMode = apEditor.MESH_EDIT_MODE.MakeMesh;
					Editor.Controller.StartMeshEdgeWork();
					Editor.VertController.SetMesh(_mesh);
					Editor.VertController.UnselectVertex();
				}
			}


			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			if (apEditorUtil.ToggledButton("Pivot", isEditMeshMode_Pivot, subTabWidth))
			{
				if (!isEditMeshMode_Pivot)
				{
					Editor.Controller.CheckMeshEdgeWorkRemained();
					Editor._meshEditMode = apEditor.MESH_EDIT_MODE.PivotEdit;
				}
			}

			if (apEditorUtil.ToggledButton("Modify", isEditMeshMode_Modify, subTabWidth))
			{
				if (!isEditMeshMode_Modify)
				{
					Editor.Controller.CheckMeshEdgeWorkRemained();
					Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Modify;
				}
			}

			#region [미사용 코드] Edge 수정은 Vertex와 통합되어 MakeMesh로 바뀜
			//if(apEditorUtil.ToggledButton("Edge", isEditMeshMode_LinkEdge, subTabWidth))
			//{
			//	if(!isEditMeshMode_LinkEdge)
			//	{
			//		Editor.Controller.StartMeshEdgeWork();
			//		Editor._meshEditMode = apEditor.MESH_EDIT_MODE.LinkEdge;
			//	}
			//} 
			#endregion
			EditorGUILayout.EndHorizontal();

			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

			//if (apEditorUtil.ToggledButton("Volume", isEditMeshMode_Volume, subTabWidth))
			//{
			//	if (!isEditMeshMode_Volume)
			//	{
			//		Editor.Controller.CheckMeshEdgeWorkRemained();
			//		Editor._meshEditMode = apEditor.MESH_EDIT_MODE.VolumeWeight;
			//	}
			//}
			//if (apEditorUtil.ToggledButton("Physic", isEditMeshMode_Physic, subTabWidth))
			//{
			//	if (!isEditMeshMode_Physic)
			//	{
			//		Editor.Controller.CheckMeshEdgeWorkRemained();
			//		Editor._meshEditMode = apEditor.MESH_EDIT_MODE.PhysicWeight;
			//	}
			//}
			//EditorGUILayout.EndHorizontal();


			switch (Editor._meshEditMode)
			{
				case apEditor.MESH_EDIT_MODE.Setting:
					MeshProperty_None(width, height);
					break;

				case apEditor.MESH_EDIT_MODE.Modify:
					MeshProperty_Modify(width, height);
					break;

				case apEditor.MESH_EDIT_MODE.MakeMesh:
					MeshProperty_MakeMesh(width, height);
					break;
				//case apEditor.MESH_EDIT_MODE.AddVertex:
				//	MeshProperty_AddVertex(width, height);
				//	break;

				//case apEditor.MESH_EDIT_MODE.LinkEdge:
				//	MeshProperty_LinkEdge(width, height);
				//	break;

				case apEditor.MESH_EDIT_MODE.PivotEdit:
					MeshProperty_Pivot(width, height);
					break;

					//case apEditor.MESH_EDIT_MODE.VolumeWeight:
					//	MeshProperty_Volume(width, height);
					//	break;

					//case apEditor.MESH_EDIT_MODE.PhysicWeight:
					//	MeshProperty_Physic(width, height);
					//	break;
			}
			#region [미사용 코드] Sub 코드로 옮겨졌다.
			////0. 기본 정보
			//EditorGUILayout.LabelField("Name");
			//_mesh.transform.name = EditorGUILayout.TextField(_mesh.transform.name);

			//EditorGUILayout.Space();

			////1. 어느 텍스쳐를 사용할 것인가
			//EditorGUILayout.LabelField("Image");
			//apTextureData textureData = _mesh._textureData;

			//string strTextureName = "(No Image)";
			//Texture2D curTextureImage = null;
			//int selectedImageHeight = 20;
			//if(_mesh._textureData != null)
			//{
			//	strTextureName = _mesh._textureData._name;
			//	curTextureImage = _mesh._textureData._image;

			//	if(curTextureImage != null && _mesh._textureData._width > 0 && _mesh._textureData._height > 0)
			//	{
			//		selectedImageHeight = (int)((float)(width * _mesh._textureData._height) / (float)(_mesh._textureData._width));
			//	}
			//}

			//if (curTextureImage != null)
			//{
			//	//EditorGUILayout.TextField(strTextureName);
			//	EditorGUILayout.LabelField(strTextureName);
			//	EditorGUILayout.ObjectField(curTextureImage, typeof(Texture2D), false, GUILayout.Height(selectedImageHeight));
			//}
			//else
			//{
			//	EditorGUILayout.LabelField("(No Image)");
			//}

			//if(GUILayout.Button("Change Image", GUILayout.Height(30)))
			//{
			//	_isShowTextureDataList = !_isShowTextureDataList;
			//}

			//EditorGUILayout.Space();
			//if(_isShowTextureDataList)
			//{
			//	int nImage = _portrait._textureData.Count;
			//	for (int i = 0; i < nImage; i++)
			//	{
			//		if (i % 2 == 0)
			//		{
			//			EditorGUILayout.BeginHorizontal();
			//		}

			//		EditorGUILayout.BeginVertical(GUILayout.Width((width / 2) - 4));

			//		apTextureData curTextureData = _portrait._textureData[i];
			//		if(curTextureData == null)
			//		{
			//			continue;
			//		}

			//		//EditorGUILayout.LabelField("[" + (i + 1) + "] : " + curTextureData._name);
			//		//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//		int imageHeight = 20;
			//		if(curTextureData._image != null && curTextureData._width > 0 && curTextureData._height > 0)
			//		{
			//			//w : h = w' : h'
			//			//(w ' * h) / w = h'
			//			imageHeight = (int)((float)((width / 2 - 4) * curTextureData._height) / (float)(curTextureData._width));
			//		}
			//		EditorGUILayout.ObjectField(curTextureData._image, typeof(Texture2D), false, GUILayout.Height(imageHeight));
			//		if(GUILayout.Button("Select", GUILayout.Height(25)))
			//		{
			//			apEditorUtil.SetRecord("Change Image of Mesh", _mesh);

			//			bool isCheckToResetVertex = false;
			//			if(_mesh._vertexData == null || _mesh._vertexData.Count == 0)
			//			{
			//				isCheckToResetVertex = true;
			//			}

			//			_mesh._textureData = curTextureData;
			//			_isShowTextureDataList = false;

			//			//if(isCheckToResetVertex)
			//			//{
			//			//	if (EditorUtility.DisplayDialog("Reset Vertex", "Do you want to make Vertices automatically?", "Reset", "Stay"))
			//			//	{
			//			//		_mesh._vertexData.Clear();
			//			//		_mesh._indexBuffer.Clear();

			//			//		_mesh.ResetVerticesByImageOutline();
			//			//	}
			//			//}
			//		}
			//		//EditorGUILayout.EndHorizontal();

			//		EditorGUILayout.EndVertical();


			//		if(i % 2 == 1)
			//		{
			//			EditorGUILayout.EndHorizontal();
			//			GUILayout.Space(10);
			//		}
			//	}
			//	if(nImage % 2 == 1)
			//	{
			//		EditorGUILayout.EndHorizontal();
			//		GUILayout.Space(10);
			//	}

			//}

			//EditorGUILayout.Space();
			////_mesh._textureData = EditorGUILayout.ObjectField(_mesh._textureData, typeof(apTextureData), false);

			////2. 버텍스 세팅
			//if(GUILayout.Button("Reset Vertices"))
			//{
			//	if(_mesh._textureData != null && _mesh._textureData._image != null)
			//	{
			//		bool isConfirmReset = false;
			//		if(_mesh._vertexData != null && _mesh._vertexData.Count > 0 &&
			//			_mesh._indexBuffer != null && _mesh._indexBuffer.Count > 0)
			//		{
			//			isConfirmReset = EditorUtility.DisplayDialog("Reset Vertex", "If you reset vertices, All data is reset. Do not undo.", "Reset", "Cancel");
			//		}
			//		else
			//		{
			//			isConfirmReset = true;
			//		}

			//		if (isConfirmReset)
			//		{
			//			apEditorUtil.SetRecord("Reset Vertex", _mesh);

			//			_mesh._vertexData.Clear();
			//			_mesh._indexBuffer.Clear();
			//			_mesh._edges.Clear();
			//			_mesh._polygons.Clear();
			//			_mesh.MakeEdgesToPolygonAndIndexBuffer();

			//			_mesh.ResetVerticesByImageOutline();
			//		}
			//	}
			//}

			//// Remove
			//GUILayout.Space(30);
			//if(GUILayout.Button("Remove Mesh"))
			//{
			//	if(EditorUtility.DisplayDialog("Remove Mesh", "Do you want to remove [" + _mesh.name + "]?", "Remove", "Cancel"))
			//	{
			//		//apEditorUtil.SetRecord("Remove Mesh", _portrait);

			//		//MonoBehaviour.DestroyImmediate(_mesh.gameObject);
			//		//_portrait._meshes.Remove(_mesh);
			//		Editor.Controller.RemoveMesh(_mesh);

			//		SetNone();
			//	}
			//} 
			#endregion
		}

		private void Draw_Face(int width, int height)
		{
			//GUILayout.Box("Face", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Face", width);
			EditorGUILayout.Space();

		}

		private void Draw_MeshGroup(int width, int height)
		{
			//DrawTitle("Mesh Group", width);
			EditorGUILayout.Space();

			if (_meshGroup == null)
			{
				SetNone();
				return;
			}

			bool isEditMeshGroupMode_Setting = (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting);
			bool isEditMeshGroupMode_Bone = (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Bone);
			bool isEditMeshGroupMode_Modifier = (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier);
			int subTabWidth = (width / 2) - 4;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			if (apEditorUtil.ToggledButton("Setting", isEditMeshGroupMode_Setting, subTabWidth))
			{
				if (!isEditMeshGroupMode_Setting)
				{
					Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Setting;



					SetModMeshOfModifier(null);
					SetSubMeshGroupInGroup(null);
					SetSubMeshInGroup(null);
					SetModifier(null);

					SetBoneEditing(false, false);//Bone 처리는 종료 

					//Gizmo 컨트롤 방식을 Setting에 맞게 바꾸자
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshGroupSetting());



					SetModifierEditMode(EX_EDIT_KEY_VALUE.None);

					_rigEdit_isBindingEdit = false;
					_rigEdit_isTestPosing = false;
					SetBoneRiggingTest();
				}
			}

			if (apEditorUtil.ToggledButton("Bone", isEditMeshGroupMode_Bone, subTabWidth))
			{
				if (!isEditMeshGroupMode_Bone)
				{
					Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Bone;

					SetModMeshOfModifier(null);
					SetSubMeshGroupInGroup(null);
					SetSubMeshInGroup(null);
					SetModifier(null);

					//일단 Gizmo 초기화
					Editor.Gizmos.Unlink();

					_meshGroupChildHierarchy = MESHGROUP_CHILD_HIERARCHY.Bones;//하단 UI도 변경

					SetModifierEditMode(EX_EDIT_KEY_VALUE.ParamKey_Bone);

					_rigEdit_isBindingEdit = false;
					_rigEdit_isTestPosing = false;
					SetBoneRiggingTest();

					SetBoneEditing(false, true);
				}
			}
			EditorGUILayout.EndHorizontal();

			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			if (apEditorUtil.ToggledButton("Modify", isEditMeshGroupMode_Modifier, width))
			{
				if (!isEditMeshGroupMode_Modifier)
				{
					SetBoneEditing(false, false);//Bone 처리는 종료 

					Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Modifier;

					bool isSelectMod = false;
					if (Modifier == null)
					{
						//이전에 선택했던 Modifier가 없다면..
						if (_meshGroup._modifierStack != null)
						{
							if (_meshGroup._modifierStack._modifiers.Count > 0)
							{
								//맨 위의 Modifier를 자동으로 선택해주자
								int nMod = _meshGroup._modifierStack._modifiers.Count;
								apModifierBase lastMod = _meshGroup._modifierStack._modifiers[nMod - 1];
								SetModifier(lastMod);
								isSelectMod = true;
							}
						}
					}
					else
					{
						SetModifier(Modifier);

						isSelectMod = true;
					}

					if (!isSelectMod)
					{
						SetModifier(null);
					}


				}
			}
			//EditorGUILayout.EndHorizontal();

			if (Editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Setting)
			{
				_isMeshGroupSetting_ChangePivot = false;
			}

			switch (Editor._meshGroupEditMode)
			{
				case apEditor.MESHGROUP_EDIT_MODE.Setting:
					MeshGroupProperty_Setting(width, height);
					break;

				case apEditor.MESHGROUP_EDIT_MODE.Bone:
					MeshGroupProperty_Bone(width, height);
					break;

				case apEditor.MESHGROUP_EDIT_MODE.Modifier:
					MeshGroupProperty_Modify(width, height);
					break;
			}
		}





		private string _prevAnimClipName = "";
		private object _loadKey_SelectMeshGroupToAnimClip = null;
		private object _loadKey_AddTimelineToAnimClip = null;

		private void Draw_Animation(int width, int height)
		{
			//GUILayout.Box("Animation", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Animation", width);
			EditorGUILayout.Space();

			if (_animClip == null)
			{
				SetNone();
				return;
			}

			//왼쪽엔 기본 세팅/ 우측 (Right2)엔 편집 도구들 + 생성된 Timeline리스트
			EditorGUILayout.LabelField("Name");
			string nextAnimClipName = EditorGUILayout.DelayedTextField(_animClip._name, GUILayout.Width(width));

			if (!string.Equals(nextAnimClipName, _animClip._name))
			{
				_animClip._name = nextAnimClipName;
				Editor.RefreshControllerAndHierarchy();
			}

			#region [미사용 코드] Delayed Text Field를 사용하지 않았을 경우
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

			//_prevAnimClipName = EditorGUILayout.TextField(_prevAnimClipName);
			//if (GUILayout.Button("Change", GUILayout.Width(80)))
			//{
			//	if (!string.IsNullOrEmpty(_prevAnimClipName))
			//	{
			//		_animClip._name = _prevAnimClipName;

			//		//Editor.Hierarchy.RefreshUnits();
			//		Editor.RefreshControllerAndHierarchy();
			//	}
			//}
			//EditorGUILayout.EndHorizontal(); 
			#endregion

			GUILayout.Space(5);
			//MeshGroup에 연동해야한다.

			GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			guiStyle_Box.alignment = TextAnchor.MiddleCenter;

			Color prevColor = GUI.color;

			if (_animClip._targetMeshGroup == null)
			{
				//GUI.color = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//GUILayout.Box("Linked Mesh Group\n[ None ]", guiStyle_Box, GUILayout.Width(width), GUILayout.Height(40));
				//GUI.color = prevColor;

				//GUILayout.Space(2);

				if (GUILayout.Button("Set Mesh Group", GUILayout.Width(width), GUILayout.Height(35)))
				{
					_loadKey_SelectMeshGroupToAnimClip = apDialog_SelectLinkedMeshGroup.ShowDialog(Editor, _animClip, OnSelectMeshGroupToAnimClip);
				}
			}
			else
			{
				//GUI.color = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				//GUILayout.Box("Linked Mesh Group\n[ " + _animClip._targetMeshGroup._name +" ]", guiStyle_Box, GUILayout.Width(width), GUILayout.Height(40));
				//GUI.color = prevColor;

				//GUILayout.Space(2);

				EditorGUILayout.LabelField("Target Mesh Group");
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				GUI.color = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				GUILayout.Box(_animClip._targetMeshGroup._name, guiStyle_Box, GUILayout.Width(width - (80 + 2)), GUILayout.Height(18));
				GUI.color = prevColor;
				if (GUILayout.Button("Change", GUILayout.Width(80)))
				{
					_loadKey_SelectMeshGroupToAnimClip = apDialog_SelectLinkedMeshGroup.ShowDialog(Editor, _animClip, OnSelectMeshGroupToAnimClip);
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);
				if (GUILayout.Button("Duplicate", GUILayout.Width(width)))
				{
					Editor.Controller.DuplicateAnimClip(_animClip);
					Editor.RefreshControllerAndHierarchy();
				}
				GUILayout.Space(5);

				//Timeline을 추가하자
				//Timeline은 ControlParam, Modifier, Bone에 연동된다.
				//TimelineLayer은 각 Timeline에서 어느 Transform(Mesh/MeshGroup), Bone, ControlParam 에 적용 될지를 결정한다.
				if (GUILayout.Button("Add Timeline", GUILayout.Width(width), GUILayout.Height(25)))
				{
					_loadKey_AddTimelineToAnimClip = apDialog_AddAnimTimeline.ShowDialog(Editor, _animClip, OnAddTimelineToAnimClip);
				}

				//등록된 Timeline 리스트를 보여주자
				GUILayout.Space(10);
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(2);
				EditorGUILayout.LabelField("Timelines", GUILayout.Height(25));

				GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
				GUILayout.Button("", guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));//<레이아웃 정렬을 위한의미없는 숨은 버튼
				EditorGUILayout.EndHorizontal();

				//등록된 Modifier 리스트를 출력하자
				if (_animClip._timelines.Count > 0)
				{
					for (int i = 0; i < _animClip._timelines.Count; i++)
					{
						DrawTimelineUnit(_animClip._timelines[i], width, 25);
					}
				}
			}

			GUILayout.Space(20);

			apEditorUtil.GUI_DelimeterBoxH(width - 10);

			//등등
			GUILayout.Space(30);
			if (GUILayout.Button("Remove Animation"))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Animation", "Do you want to remove [" + _animClip._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveAnimClip_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveAnimClip_Body, _animClip._name),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel));
				if (isResult)
				{
					Editor.Controller.RemoveAnimClip(_animClip);

					SetNone();
					Editor.RefreshControllerAndHierarchy();
					Editor.RefreshTimelineLayers(true);
				}
			}
		}

		private void OnSelectMeshGroupToAnimClip(bool isSuccess, object loadKey, apMeshGroup meshGroup, apAnimClip targetAnimClip)
		{
			if (!isSuccess || _loadKey_SelectMeshGroupToAnimClip != loadKey
				|| meshGroup == null || _animClip != targetAnimClip)
			{
				_loadKey_SelectMeshGroupToAnimClip = null;
				return;
			}

			_loadKey_SelectMeshGroupToAnimClip = null;

			if (_animClip._targetMeshGroup != null)
			{
				if (_animClip._targetMeshGroup == meshGroup)
				{
					//바뀐게 없다 => Pass
					return;
				}

				//bool isResult = EditorUtility.DisplayDialog("Is Change Mesh Group", "Is Change Mesh Group?", "Change", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimClipMeshGroupChanged_Title),
																Editor.GetText(apLocalization.TEXT.AnimClipMeshGroupChanged_Body),
																Editor.GetText(apLocalization.TEXT.Okay),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);
				if (!isResult)
				{
					//기존 것에서 변경을 하지 않는다 => Pass
					return;
				}
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_SetMeshGroup, Editor._portrait, meshGroup, false, Editor);

			//기존의 Timeline이 있다면 다 날리자

			//_isAnimAutoKey = false;
			//_isAnimEditing = false;
			_exAnimEditingMode = EX_EDIT.None;
			_isAnimLock = false;

			SetAnimTimeline(null, true);
			SetSubMeshTransformForAnimClipEdit(null);//하나만 null을 하면 모두 선택이 취소된다.

			_animClip._timelines.Clear();
			bool isChanged = _animClip._targetMeshGroup != meshGroup;
			_animClip._targetMeshGroup = meshGroup;
			_animClip._targetMeshGroupID = meshGroup._uniqueID;


			if (meshGroup != null)
			{
				meshGroup._modifierStack.RefreshAndSort(true);
			}
			if (isChanged)
			{
				//MeshGroup 선택 후 초기화
				if (_animClip._targetMeshGroup != null)
				{
					_animClip._targetMeshGroup.SetDirtyToReset();
					_animClip._targetMeshGroup.SetDirtyToSort();
					//_animClip._targetMeshGroup.SetAllRenderUnitForceUpdate();
					_animClip._targetMeshGroup.RefreshForce(true);

					_animClip._targetMeshGroup.LinkModMeshRenderUnits();
					_animClip._targetMeshGroup.RefreshModifierLink();

					_animClip._targetMeshGroup._modifierStack.RefreshAndSort(true);
				}


				Editor.Hierarchy_AnimClip.ResetSubUnits();
			}
			Editor.RefreshControllerAndHierarchy();

		}

		//Dialog 이벤트에 의해서 Timeline을 추가하자
		private void OnAddTimelineToAnimClip(bool isSuccess, object loadKey, apAnimClip.LINK_TYPE linkType, int modifierUniqueID, apAnimClip targetAnimClip)
		{
			if (!isSuccess || _loadKey_AddTimelineToAnimClip != loadKey ||
				_animClip != targetAnimClip)
			{
				_loadKey_AddTimelineToAnimClip = null;
				return;
			}

			_loadKey_AddTimelineToAnimClip = null;

			Editor.Controller.AddAnimTimeline(linkType, modifierUniqueID, targetAnimClip);
		}


		private void DrawTimelineUnit(apAnimTimeline timeline, int width, int height)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (AnimTimeline == timeline)
			{
				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

				GUI.Box(new Rect(lastRect.x, lastRect.y + height, width + 15, height), "");
				GUI.backgroundColor = prevColor;
			}

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = Color.black;

			apImageSet.PRESET iconType = apImageSet.PRESET.Anim_WithMod;
			switch (timeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					iconType = apImageSet.PRESET.Anim_WithMod;
					break;

				case apAnimClip.LINK_TYPE.ControlParam:
					iconType = apImageSet.PRESET.Anim_WithControlParam;
					break;

					//case apAnimClip.LINK_TYPE.Bone:
					//	iconType = apImageSet.PRESET.Anim_WithBone;
					//	break;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			GUILayout.Space(10);
			if (GUILayout.Button(new GUIContent(" " + timeline.DisplayName, Editor.ImageSet.Get(iconType)), guiStyle_None, GUILayout.Width(width - 40), GUILayout.Height(height)))
			{
				SetAnimTimeline(timeline, true);
				SetAnimTimelineLayer(null, true);
				SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
			}

			Texture2D activeBtn = null;
			bool isActiveMod = false;
			if (timeline._isActiveInEditing)
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Active);
				isActiveMod = true;
			}
			else
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Deactive);
				isActiveMod = false;
			}
			if (GUILayout.Button(activeBtn, guiStyle_None, GUILayout.Width(height), GUILayout.Height(height)))
			{
				//일단 토글한다.
				timeline._isActiveInEditing = !isActiveMod;
			}
			EditorGUILayout.EndHorizontal();
		}





		private void Draw_Overall(int width, int height)
		{
			//GUILayout.Box("Overall", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Overall", width);
			EditorGUILayout.Space();

			apRootUnit rootUnit = RootUnit;
			if (rootUnit == null)
			{
				SetNone();
				return;
			}

			Color prevColor = GUI.color;

			//1. 연결된 MeshGroup 설정 (+ 해제)
			apMeshGroup targetMeshGroup = rootUnit._childMeshGroup;
			string strMeshGroupName = "";
			Color bgColor = Color.black;
			if (targetMeshGroup != null)
			{
				strMeshGroupName = "[" + targetMeshGroup._name + "]";
				bgColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
			}
			else
			{
				strMeshGroupName = "Error! No MeshGroup Linked";
				bgColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
			}
			GUI.color = bgColor;

			GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
			guiStyleBox.alignment = TextAnchor.MiddleCenter;

			GUILayout.Box(strMeshGroupName, guiStyleBox, GUILayout.Width(width), GUILayout.Height(35));

			GUI.color = prevColor;

			GUILayout.Space(20);
			apEditorUtil.GUI_DelimeterBoxH(width - 10);
			GUILayout.Space(20);

			//2. 애니메이션 제어

			apAnimClip curAnimClip = RootUnitAnimClip;
			bool isAnimClipAvailable = (curAnimClip != null);


			Texture2D icon_FirstFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_FirstFrame);
			Texture2D icon_PrevFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_PrevFrame);

			Texture2D icon_NextFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_NextFrame);
			Texture2D icon_LastFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_LastFrame);

			Texture2D icon_PlayPause = null;
			if (curAnimClip != null)
			{
				if (curAnimClip.IsPlaying)
				{ icon_PlayPause = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Pause); }
				else
				{ icon_PlayPause = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Play); }
			}
			else
			{
				icon_PlayPause = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Play);
			}

			int btnSize = 30;
			int btnWidth_Play = 45;
			int btnWidth_PrevNext = 35;
			int btnWidth_FirstLast = (width - (btnWidth_Play + btnWidth_PrevNext * 2 + 4 * 3 + 5)) / 2;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(btnSize));
			GUILayout.Space(2);
			if (apEditorUtil.ToggledButton_2Side(icon_FirstFrame, false, isAnimClipAvailable, btnWidth_FirstLast, btnSize))
			{
				if (curAnimClip != null)
				{
					curAnimClip.SetFrame_Editor(curAnimClip.StartFrame);
					curAnimClip.Pause_Editor();
				}
			}
			if (apEditorUtil.ToggledButton_2Side(icon_PrevFrame, false, isAnimClipAvailable, btnWidth_PrevNext, btnSize))
			{
				if (curAnimClip != null)
				{
					int prevFrame = curAnimClip.CurFrame - 1;
					if (prevFrame < curAnimClip.StartFrame && curAnimClip.IsLoop)
					{
						prevFrame = curAnimClip.EndFrame;
					}
					curAnimClip.SetFrame_Editor(prevFrame);
					curAnimClip.Pause_Editor();
				}
			}
			if (apEditorUtil.ToggledButton_2Side(icon_PlayPause, false, isAnimClipAvailable, btnWidth_Play, btnSize))
			{
				if (curAnimClip != null)
				{
					if (curAnimClip.IsPlaying)
					{
						curAnimClip.Pause_Editor();
					}
					else
					{
						if (curAnimClip.CurFrame == curAnimClip.EndFrame &&
							!curAnimClip.IsLoop)
						{
							curAnimClip.SetFrame_Editor(curAnimClip.StartFrame);
						}

						curAnimClip.Play_Editor();
					}
				}
			}
			if (apEditorUtil.ToggledButton_2Side(icon_NextFrame, false, isAnimClipAvailable, btnWidth_PrevNext, btnSize))
			{
				if (curAnimClip != null)
				{
					int nextFrame = curAnimClip.CurFrame + 1;
					if (nextFrame > curAnimClip.EndFrame && curAnimClip.IsLoop)
					{
						nextFrame = curAnimClip.StartFrame;
					}
					curAnimClip.SetFrame_Editor(nextFrame);
					curAnimClip.Pause_Editor();
				}
			}
			if (apEditorUtil.ToggledButton_2Side(icon_LastFrame, false, isAnimClipAvailable, btnWidth_FirstLast, btnSize))
			{
				if (curAnimClip != null)
				{
					curAnimClip.SetFrame_Editor(curAnimClip.EndFrame);
					curAnimClip.Pause_Editor();
				}
			}

			EditorGUILayout.EndHorizontal();

			int curFrame = 0;
			int startFrame = 0;
			int endFrame = 10;
			if (curAnimClip != null)
			{
				curFrame = curAnimClip.CurFrame;
				startFrame = curAnimClip.StartFrame;
				endFrame = curAnimClip.EndFrame;
			}
			int sliderFrame = EditorGUILayout.IntSlider(curFrame, startFrame, endFrame, GUILayout.Width(width));
			if (sliderFrame != curFrame)
			{
				curAnimClip.SetFrame_Editor(sliderFrame);
				curAnimClip.Pause_Editor();
			}

			GUILayout.Space(20);
			apEditorUtil.GUI_DelimeterBoxH(width - 10);
			GUILayout.Space(20);

			//3. 애니메이션 리스트
			List<apAnimClip> subAnimClips = RootUnitAnimClipList;
			EditorGUILayout.LabelField("Animation Clips", GUILayout.Width(width));
			GUILayout.Space(5);
			if (subAnimClips != null && subAnimClips.Count > 0)
			{
				apAnimClip nextSelectedAnimClip = null;
				GUIStyle guiNone = new GUIStyle(GUIStyle.none);
				guiNone.normal.textColor = GUI.skin.label.normal.textColor;

				Rect lastRect = GUILayoutUtility.GetLastRect();

				int scrollWidth = width - 20;

				Texture2D icon_Anim = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

				for (int i = 0; i < subAnimClips.Count; i++)
				{
					apAnimClip subAnimClip = subAnimClips[i];
					if (subAnimClip == curAnimClip)
					{
						lastRect = GUILayoutUtility.GetLastRect();
						GUI.color = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//int offsetHeight = 20 + 3;
						int offsetHeight = 1 + 3;
						if (i == 0)
						{
							offsetHeight = 4 + 3;
						}

						GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 24), "");

						GUI.color = prevColor;
					}
					EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
					GUILayout.Space(5);
					if (GUILayout.Button(new GUIContent(" " + subAnimClip._name, icon_Anim),
									guiNone,
									GUILayout.Width(scrollWidth - 5), GUILayout.Height(24)))
					{
						nextSelectedAnimClip = subAnimClip;
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(4);

				}

				if (nextSelectedAnimClip != null)
				{
					for (int i = 0; i < Editor._portrait._animClips.Count; i++)
					{
						Editor._portrait._animClips[i]._isSelectedInEditor = false;
					}

					_curRootUnitAnimClip = nextSelectedAnimClip;
					_curRootUnitAnimClip.LinkEditor(Editor._portrait);
					_curRootUnitAnimClip.RefreshTimelines();
					_curRootUnitAnimClip.SetFrame_Editor(_curRootUnitAnimClip.StartFrame);
					_curRootUnitAnimClip.Pause_Editor();

					_curRootUnitAnimClip._isSelectedInEditor = true;

					//Debug.Log("Select Root Unit Anim Clip : " + _curRootUnitAnimClip._name);
				}
			}



			GUILayout.Space(20);
			apEditorUtil.GUI_DelimeterBoxH(width - 10);
			GUILayout.Space(20);
			//MainMesh에서 해제
			if (GUILayout.Button("Unregist Main MeshGroup", GUILayout.Width(width), GUILayout.Height(20)))
			{
				//Debug.LogError("TODO : MainMeshGroup 해제");
				apMeshGroup targetRootMeshGroup = rootUnit._childMeshGroup;
				if (targetRootMeshGroup != null)
				{
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Portrait_SetMeshGroup, _portrait, targetRootMeshGroup, false, Editor);

					_portrait._mainMeshGroupIDList.Remove(targetRootMeshGroup._uniqueID);
					_portrait._mainMeshGroupList.Remove(targetRootMeshGroup);

					_portrait._rootUnits.Remove(rootUnit);

					SetNone();

					Editor.RefreshControllerAndHierarchy();
					Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.RootUnit, true);
				}
			}
		}

		private apControlParam _prevParam = null;
		//private string _prevParamName = "";
		private void Draw_Param(int width, int height)
		{
			EditorGUILayout.Space();

			apControlParam cParam = _param;
			if (cParam == null)
			{
				SetNone();
				return;
			}
			if (_prevParam != cParam)
			{
				_prevParam = cParam;
				//_prevParamName = cParam._keyName;
			}
			if (cParam._isReserved)
			{
				GUIStyle guiStyle_RedTextColor = new GUIStyle(GUI.skin.label);
				guiStyle_RedTextColor.normal.textColor = Color.red;
				EditorGUILayout.LabelField("Reserved Parameter", guiStyle_RedTextColor);
				GUILayout.Space(10);
			}

			bool isChanged = false;
			apControlParam.CATEGORY next_category = cParam._category;
			apControlParam.ICON_PRESET next_iconPreset = cParam._iconPreset;
			apControlParam.TYPE next_valueType = cParam._valueType;

			string next_label_Min = cParam._label_Min;
			string next_label_Max = cParam._label_Max;
			int next_snapSize = cParam._snapSize;

			int next_int_Def = cParam._int_Def;
			float next_float_Def = cParam._float_Def;
			Vector2 next_vec2_Def = cParam._vec2_Def;
			int next_int_Min = cParam._int_Min;
			int next_int_Max = cParam._int_Max;
			float next_float_Min = cParam._float_Min;
			float next_float_Max = cParam._float_Max;
			Vector2 next_vec2_Min = cParam._vec2_Min;
			Vector2 next_vec2_Max = cParam._vec2_Max;





			EditorGUILayout.LabelField("Name (Unique)");

			if (cParam._isReserved)
			{
				EditorGUILayout.LabelField(cParam._keyName, GUI.skin.textField, GUILayout.Width(width));
			}
			else
			{
				string nextKeyName = EditorGUILayout.DelayedTextField(cParam._keyName, GUILayout.Width(width));
				if (!string.Equals(nextKeyName, cParam._keyName))
				{
					if (string.IsNullOrEmpty(nextKeyName))
					{
						//이름이 빈칸이다
						//EditorUtility.DisplayDialog("Error", "Empty Name is not allowed", "Okay");

						EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.ControlParamNameError_Title),
													Editor.GetText(apLocalization.TEXT.ControlParamNameError_Body_Wrong),
													Editor.GetText(apLocalization.TEXT.Close));
					}
					else if (Editor.ParamControl.FindParam(nextKeyName) != null)
					{
						//이미 사용중인 이름이다.
						//EditorUtility.DisplayDialog("Error", "It is used Name", "Okay");
						EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.ControlParamNameError_Title),
												Editor.GetText(apLocalization.TEXT.ControlParamNameError_Body_Used),
												Editor.GetText(apLocalization.TEXT.Close));
					}
					else
					{
						apEditorUtil.SetRecord(apUndoGroupData.ACTION.ControlParam_SettingChanged, Editor._portrait, null, false, Editor);

						Editor.Controller.ChangeParamName(cParam, nextKeyName);
						cParam._keyName = nextKeyName;
					}
				}
			}
			#region [미사용 코드] DelayedTextField를 사용하기 전
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//if (cParam._isReserved)
			//{
			//	//TextField의 Skin을 사용하지만 작동은 불가능한 Label
			//	EditorGUILayout.LabelField(cParam._keyName, GUI.skin.textField);
			//	//EditorGUILayout.TextField(cParam._keyName);
			//}
			//else
			//{
			//	_prevParamName = EditorGUILayout.TextField(_prevParamName);
			//	if (GUILayout.Button("Change", GUILayout.Width(60)))
			//	{
			//		if (!_prevParamName.Equals(cParam._keyName))
			//		{
			//			if (string.IsNullOrEmpty(_prevParamName))
			//			{
			//				EditorUtility.DisplayDialog("Error", "Empty Name is not allowed", "Okay");

			//				_prevParamName = cParam._keyName;
			//			}
			//			else
			//			{
			//				if (Editor.ParamControl.FindParam(_prevParamName) != null)
			//				{
			//					EditorUtility.DisplayDialog("Error", "It is used Name", "Okay");

			//					_prevParamName = cParam._keyName;
			//				}
			//				else
			//				{
			//					//cParam._keyName = _prevParamName;

			//					//수정
			//					//링크가 깨지지 않도록 전체적으로 검색하여 키 이름을 바꾸어주자
			//					Editor.Controller.ChangeParamName(cParam, _prevParamName);
			//					cParam._keyName = _prevParamName;
			//				}
			//			}


			//		}
			//		GUI.FocusControl("");
			//		//Editor.Hierarchy.RefreshUnits();
			//		Editor.RefreshControllerAndHierarchy();
			//	}
			//}
			//EditorGUILayout.EndHorizontal(); 

			#endregion
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Type");
			if (cParam._isReserved)
			{
				EditorGUILayout.EnumPopup(cParam._valueType);
			}
			else
			{
				next_valueType = (apControlParam.TYPE)EditorGUILayout.EnumPopup(cParam._valueType);
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Category");
			if (cParam._isReserved)
			{
				EditorGUILayout.EnumPopup(cParam._category);
			}
			else
			{
				next_category = (apControlParam.CATEGORY)EditorGUILayout.EnumPopup(cParam._category);
			}
			GUILayout.Space(10);

			int iconSize = 32;
			int iconPresetHeight = 32;
			int presetCategoryWidth = width - (iconSize + 8 + 5);
			Texture2D imgIcon = Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(cParam._iconPreset));

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconPresetHeight));
			GUILayout.Space(2);

			EditorGUILayout.BeginVertical(GUILayout.Width(presetCategoryWidth), GUILayout.Height(iconPresetHeight));

			EditorGUILayout.LabelField("Icon Preset", GUILayout.Width(presetCategoryWidth));
			next_iconPreset = (apControlParam.ICON_PRESET)EditorGUILayout.EnumPopup(cParam._iconPreset, GUILayout.Width(presetCategoryWidth));

			EditorGUILayout.EndVertical();
			GUILayout.Space(2);
			EditorGUILayout.LabelField(new GUIContent(imgIcon), GUILayout.Width(iconSize), GUILayout.Height(iconPresetHeight));


			EditorGUILayout.EndHorizontal();


			EditorGUILayout.Space();

			bool isRangeAvailable = true;

			string strRangeLabelName_Min = "Min";
			string strRangeLabelName_Max = "Max";
			switch (cParam._valueType)
			{
				case apControlParam.TYPE.Int:
					EditorGUILayout.LabelField("Integer Type");
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Default Value");
					next_int_Def = EditorGUILayout.DelayedIntField(cParam._int_Def);
					break;

				case apControlParam.TYPE.Float:
					EditorGUILayout.LabelField("Float Number Type");
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Default Value");
					next_float_Def = EditorGUILayout.DelayedFloatField(cParam._float_Def);
					break;

				case apControlParam.TYPE.Vector2:
					EditorGUILayout.LabelField("Vector2 Type");
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Default Value");

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					next_vec2_Def.x = EditorGUILayout.DelayedFloatField(cParam._vec2_Def.x, GUILayout.Width((width / 2) - 2));
					next_vec2_Def.y = EditorGUILayout.DelayedFloatField(cParam._vec2_Def.y, GUILayout.Width((width / 2) - 2));
					EditorGUILayout.EndHorizontal();

					strRangeLabelName_Min = "Axis 1";
					strRangeLabelName_Max = "Axis 2";
					break;
			}
			GUILayout.Space(25);

			if (isRangeAvailable)
			{
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(25);


				GUILayoutOption opt_Label = GUILayout.Width(50);
				GUILayoutOption opt_Data = GUILayout.Width(width - (50 + 5));
				GUILayoutOption opt_SubData2 = GUILayout.Width((width - (50 + 5)) / 2 - 2);
				GUIStyle guiStyle_LabelRight = new GUIStyle(GUI.skin.label);
				guiStyle_LabelRight.alignment = TextAnchor.MiddleRight;

				GUILayout.Space(25);
				EditorGUILayout.LabelField("Range Value Label");

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField(strRangeLabelName_Min, opt_Label);
				next_label_Min = EditorGUILayout.DelayedTextField(cParam._label_Min, opt_Data);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField(strRangeLabelName_Max, opt_Label);
				next_label_Max = EditorGUILayout.DelayedTextField(cParam._label_Max, opt_Data);
				EditorGUILayout.EndHorizontal();


				GUILayout.Space(25);

				EditorGUILayout.LabelField("Range Value");


				switch (cParam._valueType)
				{
					case apControlParam.TYPE.Int:
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField("Min", opt_Label);
						next_int_Min = EditorGUILayout.DelayedIntField(cParam._int_Min, opt_Data);
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField("Max", opt_Label);
						next_int_Max = EditorGUILayout.DelayedIntField(cParam._int_Max, opt_Data);
						EditorGUILayout.EndHorizontal();
						break;

					case apControlParam.TYPE.Float:
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField("Min", opt_Label);
						next_float_Min = EditorGUILayout.DelayedFloatField(cParam._float_Min, opt_Data);
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField("Max", opt_Label);
						next_float_Max = EditorGUILayout.DelayedFloatField(cParam._float_Max, opt_Data);
						EditorGUILayout.EndHorizontal();
						break;

					case apControlParam.TYPE.Vector2:
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField("", opt_Label);
						EditorGUILayout.LabelField("Min", opt_SubData2);
						EditorGUILayout.LabelField("Max", guiStyle_LabelRight, opt_SubData2);
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField("X", opt_Label);
						next_vec2_Min.x = EditorGUILayout.DelayedFloatField(cParam._vec2_Min.x, opt_SubData2);
						next_vec2_Max.x = EditorGUILayout.DelayedFloatField(cParam._vec2_Max.x, opt_SubData2);
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField("Y", opt_Label);
						next_vec2_Min.y = EditorGUILayout.DelayedFloatField(cParam._vec2_Min.y, opt_SubData2);
						next_vec2_Max.y = EditorGUILayout.DelayedFloatField(cParam._vec2_Max.y, opt_SubData2);
						EditorGUILayout.EndHorizontal();
						break;

				}


				if (cParam._valueType == apControlParam.TYPE.Float ||
					cParam._valueType == apControlParam.TYPE.Vector2)
				{
					GUILayout.Space(10);



					EditorGUILayout.LabelField("Snap Size");
					next_snapSize = EditorGUILayout.DelayedIntField(cParam._snapSize, GUILayout.Width(width));
					//if (next_snapSize != cParam._snapSize)
					//{
					//	cParam._snapSize = nextSnapSize;
					//	if (cParam._snapSize < 1)
					//	{
					//		cParam._snapSize = 1;
					//	}
					//	GUI.FocusControl(null);
					//}
				}



				if (next_category != cParam._category ||
					next_iconPreset != cParam._iconPreset ||
					next_valueType != cParam._valueType ||

					next_label_Min != cParam._label_Min ||
					next_label_Max != cParam._label_Max ||
					next_snapSize != cParam._snapSize ||

					next_int_Def != cParam._int_Def ||
					next_float_Def != cParam._float_Def ||
					next_vec2_Def.x != cParam._vec2_Def.x ||
					next_vec2_Def.y != cParam._vec2_Def.y ||

					next_int_Min != cParam._int_Min ||
					next_int_Max != cParam._int_Max ||

					next_float_Min != cParam._float_Min ||
					next_float_Max != cParam._float_Max ||

					next_vec2_Min.x != cParam._vec2_Min.x ||
					next_vec2_Min.y != cParam._vec2_Min.y ||
					next_vec2_Max.x != cParam._vec2_Max.x ||
					next_vec2_Max.y != cParam._vec2_Max.y
					)
				{
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.ControlParam_SettingChanged, Editor._portrait, null, false, Editor);

					if (next_snapSize < 1)
					{
						next_snapSize = 1;
					}

					if (cParam._iconPreset != next_iconPreset)
					{
						cParam._isIconChanged = true;
					}
					else if (cParam._category != next_category && !cParam._isIconChanged)
					{
						//아이콘을 한번도 바꾸지 않았더라면 자동으로 다음 아이콘을 추천해주자
						next_iconPreset = apEditorUtil.GetControlParamPresetIconTypeByCategory(next_category);
					}

					cParam._category = next_category;
					cParam._iconPreset = next_iconPreset;
					cParam._valueType = next_valueType;

					cParam._label_Min = next_label_Min;
					cParam._label_Max = next_label_Max;
					cParam._snapSize = next_snapSize;

					cParam._int_Def = next_int_Def;
					cParam._float_Def = next_float_Def;
					cParam._vec2_Def = next_vec2_Def;

					cParam._int_Min = next_int_Min;
					cParam._int_Max = next_int_Max;

					cParam._float_Min = next_float_Min;
					cParam._float_Max = next_float_Max;

					cParam._vec2_Min = next_vec2_Min;
					cParam._vec2_Max = next_vec2_Max;

					cParam.MakeInterpolationRange();
					GUI.FocusControl(null);
				}


				GUILayout.Space(30);

				if (!cParam._isReserved)
				{
					if (GUILayout.Button("Remove Parameter", GUILayout.Height(18)))
					{
						//bool isResult = EditorUtility.DisplayDialog("Warning", "If this param removed, some motion data may be not worked correctly", "Remove it!", "Cancel");
						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveControlParam_Title),
																		Editor.GetTextFormat(apLocalization.TEXT.RemoveControlParam_Body, cParam._keyName),
																		Editor.GetText(apLocalization.TEXT.Remove),
																		Editor.GetText(apLocalization.TEXT.Cancel));
						if (isResult)
						{
							Editor.Controller.RemoveParam(cParam);
						}
					}
				}
			}
		}


		private void DrawTitle(string strTitle, int width)
		{
			int titleWidth = width;
			bool isShowHideBtn = false;
			if (_selectionType == SELECTION_TYPE.MeshGroup || _selectionType == SELECTION_TYPE.Animation)
			{
				titleWidth = width - (25 + 2);
				isShowHideBtn = true;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(5);
			GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
			guiStyle.normal.textColor = Color.white;
			guiStyle.alignment = TextAnchor.MiddleCenter;

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);

			GUILayout.Box(strTitle, guiStyle, GUILayout.Width(titleWidth), GUILayout.Height(20));

			GUI.backgroundColor = prevColor;

			if (isShowHideBtn)
			{
				bool isOpened = (Editor._right_UpperLayout == apEditor.RIGHT_UPPER_LAYOUT.Show);
				Texture2D btnImage = null;
				if (isOpened)
				{ btnImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_OpenLayout); }
				else
				{ btnImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_HideLayout); }

				GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
				if (GUILayout.Button(btnImage, guiStyle_Btn, GUILayout.Width(25), GUILayout.Height(25)))
				{
					if (Editor._right_UpperLayout == apEditor.RIGHT_UPPER_LAYOUT.Show)
					{
						Editor._right_UpperLayout = apEditor.RIGHT_UPPER_LAYOUT.Hide;
					}
					else
					{
						Editor._right_UpperLayout = apEditor.RIGHT_UPPER_LAYOUT.Show;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		//---------------------------------------------------------------------------


		//---------------------------------------------------------------------
		/// <summary>
		/// Mesh Property GUI에서 "조작 방법"에 대한 안내 UI를 보여준다.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="msgMouseLeft">마우스 좌클릭에 대한 설명 (없을 경우 null)</param>
		/// <param name="msgMouseMiddle">마우스 휠클릭에 대한 설명 (없을 경우 null)</param>
		/// <param name="msgMouseRight">마우스 우클릭에 대한 설명 (없을 경우 null)</param>
		/// <param name="msgKeyboardList">키보드 입력에 대한 설명. 여러개 가능</param>
		private void DrawHowToControl(int width, string msgMouseLeft, string msgMouseMiddle, string msgMouseRight, string msgKeyboardDelete = null, string msgKeyboardCtrl = null, string msgKeyboardShift = null)
		{
			bool isMouseLeft = !string.IsNullOrEmpty(msgMouseLeft);
			bool isMouseMiddle = !string.IsNullOrEmpty(msgMouseMiddle);
			bool isMouseRight = !string.IsNullOrEmpty(msgMouseRight);
			bool isKeyDelete = !string.IsNullOrEmpty(msgKeyboardDelete);
			bool isKeyCtrl = !string.IsNullOrEmpty(msgKeyboardCtrl);
			bool isKeyShift = !string.IsNullOrEmpty(msgKeyboardShift);
			//int nKeyMsg = 0;
			//if (msgKeyboardList != null)
			//{
			//	nKeyMsg = msgKeyboardList.Length;
			//}

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.margin = GUI.skin.box.margin;

			GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
			guiStyle_Label.alignment = TextAnchor.LowerLeft;

			//GUILayout.Space(20);

			int labelSize = 30;
			int subTextWidth = width - (labelSize + 8);
			if (isMouseLeft)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_MouseLeft)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgMouseLeft, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			if (isMouseMiddle)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_MouseMiddle)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgMouseMiddle, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();


				EditorGUILayout.EndHorizontal();
			}

			if (isMouseRight)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_MouseRight)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgMouseRight, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			if (isKeyDelete)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyDelete)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgKeyboardDelete, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			if (isKeyCtrl)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyCtrl)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgKeyboardCtrl, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			if (isKeyShift)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyShift)), guiStyle_Icon, GUILayout.Width(labelSize), GUILayout.Height(labelSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(subTextWidth), GUILayout.Height(labelSize));
				GUILayout.Space(8);
				EditorGUILayout.LabelField(msgKeyboardShift, GUILayout.Width(subTextWidth), GUILayout.Height(20));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}
			//if (nKeyMsg > 0)
			//{
			//	for (int i = 0; i < nKeyMsg; i++)
			//	{
			//		EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
			//		GUILayout.Space(5);
			//		EditorGUILayout.LabelField(msgKeyboardList[i], GUILayout.Width(width - 10));
			//		EditorGUILayout.EndHorizontal();
			//	}
			//}
			GUILayout.Space(20);
		}

		//private string _prevMesh_Name = "";

		private void MeshProperty_None(int width, int height)
		{
			EditorGUILayout.LabelField("Name");
			string nextMeshName = EditorGUILayout.DelayedTextField(_mesh._name, GUILayout.Width(width));
			if (!string.Equals(nextMeshName, _mesh._name))
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_SettingChanged, _portrait, _mesh, false, Editor);
				_mesh._name = nextMeshName;
				Editor.RefreshControllerAndHierarchy();
			}

			#region [미사용 코드] DelayedTextField를 사용하기 전
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//_prevMesh_Name = EditorGUILayout.TextField(_prevMesh_Name);
			//if (GUILayout.Button("Change", GUILayout.Width(80)))
			//{
			//	if (!string.IsNullOrEmpty(_prevMesh_Name))
			//	{
			//		_mesh._name = _prevMesh_Name;
			//		Editor.RefreshControllerAndHierarchy();
			//	}
			//}

			//EditorGUILayout.EndHorizontal(); 
			#endregion

			EditorGUILayout.Space();

			//1. 어느 텍스쳐를 사용할 것인가
			//[수정]
			//다이얼로그를 보여주자

			EditorGUILayout.LabelField("Image");
			//apTextureData textureData = _mesh._textureData;

			string strTextureName = "(No Image)";
			Texture2D curTextureImage = null;
			int selectedImageHeight = 20;
			if (_mesh._textureData != null)
			{
				strTextureName = _mesh._textureData._name;
				curTextureImage = _mesh._textureData._image;

				if (curTextureImage != null && _mesh._textureData._width > 0 && _mesh._textureData._height > 0)
				{
					selectedImageHeight = (int)((float)(width * _mesh._textureData._height) / (float)(_mesh._textureData._width));
				}
			}

			if (curTextureImage != null)
			{
				//EditorGUILayout.TextField(strTextureName);
				EditorGUILayout.LabelField(strTextureName);
				GUILayout.Space(10);
				EditorGUILayout.LabelField(new GUIContent(curTextureImage), GUILayout.Height(selectedImageHeight));
				//EditorGUILayout.ObjectField(curTextureImage, typeof(Texture2D), false, GUILayout.Height(selectedImageHeight));
				GUILayout.Space(10);
			}
			else
			{
				EditorGUILayout.LabelField("(No Image)");
			}

			if (GUILayout.Button("Change Image", GUILayout.Height(30)))
			{
				//_isShowTextureDataList = !_isShowTextureDataList;
				_loadKey_SelectTextureDataToMesh = apDialog_SelectTextureData.ShowDialog(Editor, _mesh, OnSelectTextureDataToMesh);
			}

			EditorGUILayout.Space();
			#region [미사용 코드]
			//if(_isShowTextureDataList)
			//{
			//	int nImage = _portrait._textureData.Count;
			//	for (int i = 0; i < nImage; i++)
			//	{
			//		if (i % 2 == 0)
			//		{
			//			EditorGUILayout.BeginHorizontal();
			//		}

			//		EditorGUILayout.BeginVertical(GUILayout.Width((width / 2) - 4));

			//		apTextureData curTextureData = _portrait._textureData[i];
			//		if(curTextureData == null)
			//		{
			//			continue;
			//		}

			//		//EditorGUILayout.LabelField("[" + (i + 1) + "] : " + curTextureData._name);
			//		//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//		int imageHeight = 20;
			//		if(curTextureData._image != null && curTextureData._width > 0 && curTextureData._height > 0)
			//		{
			//			//w : h = w' : h'
			//			//(w ' * h) / w = h'
			//			imageHeight = (int)((float)((width / 2 - 4) * curTextureData._height) / (float)(curTextureData._width));
			//		}
			//		EditorGUILayout.ObjectField(curTextureData._image, typeof(Texture2D), false, GUILayout.Height(imageHeight));
			//		if(GUILayout.Button("Select", GUILayout.Height(25)))
			//		{
			//			apEditorUtil.SetRecord("Change Image of Mesh", _portrait);

			//			//bool isCheckToResetVertex = false;
			//			//if(_mesh._vertexData == null || _mesh._vertexData.Count == 0)
			//			//{
			//			//	isCheckToResetVertex = true;
			//			//}

			//			_mesh._textureData = curTextureData;
			//			_isShowTextureDataList = false;

			//			//if(isCheckToResetVertex)
			//			//{
			//			//	if (EditorUtility.DisplayDialog("Reset Vertex", "Do you want to make Vertices automatically?", "Reset", "Stay"))
			//			//	{
			//			//		_mesh._vertexData.Clear();
			//			//		_mesh._indexBuffer.Clear();

			//			//		_mesh.ResetVerticesByImageOutline();
			//			//	}
			//			//}
			//		}
			//		//EditorGUILayout.EndHorizontal();

			//		EditorGUILayout.EndVertical();


			//		if(i % 2 == 1)
			//		{
			//			EditorGUILayout.EndHorizontal();
			//			GUILayout.Space(10);
			//		}
			//	}
			//	if(nImage % 2 == 1)
			//	{
			//		EditorGUILayout.EndHorizontal();
			//		GUILayout.Space(10);
			//	}

			//} 
			#endregion

			//2. 버텍스 세팅
			if (GUILayout.Button("Reset Vertices"))
			{
				if (_mesh._textureData != null && _mesh._textureData._image != null)
				{
					bool isConfirmReset = false;
					if (_mesh._vertexData != null && _mesh._vertexData.Count > 0 &&
						_mesh._indexBuffer != null && _mesh._indexBuffer.Count > 0)
					{
						//isConfirmReset = EditorUtility.DisplayDialog("Reset Vertex", "If you reset vertices, All data is reset.", "Reset", "Cancel");
						isConfirmReset = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.ResetMeshVertices_Title),
																		Editor.GetText(apLocalization.TEXT.ResetMeshVertices_Body),
																		Editor.GetText(apLocalization.TEXT.ResetMeshVertices_Okay),
																		Editor.GetText(apLocalization.TEXT.Cancel));


					}
					else
					{
						isConfirmReset = true;
					}

					if (isConfirmReset)
					{
						apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_ResetVertices, _mesh, _mesh, false, Editor);

						_mesh._vertexData.Clear();
						_mesh._indexBuffer.Clear();
						_mesh._edges.Clear();
						_mesh._polygons.Clear();
						_mesh.MakeEdgesToPolygonAndIndexBuffer();

						_mesh.ResetVerticesByImageOutline();
						_mesh.MakeEdgesToPolygonAndIndexBuffer();

						Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
					}
				}
			}

			// Remove
			GUILayout.Space(30);
			if (GUILayout.Button("Remove Mesh"))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Mesh", "Do you want to remove [" + _mesh._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveMesh_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveMesh_Body, _mesh._name),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel));

				if (isResult)
				{
					//apEditorUtil.SetRecord("Remove Mesh", _portrait);

					//MonoBehaviour.DestroyImmediate(_mesh.gameObject);
					//_portrait._meshes.Remove(_mesh);
					Editor.Controller.RemoveMesh(_mesh);

					SetNone();
				}
			}
		}

		private object _loadKey_SelectTextureDataToMesh = null;
		private void OnSelectTextureDataToMesh(bool isSuccess, apMesh targetMesh, object loadKey, apTextureData resultTextureData)
		{
			if (!isSuccess || resultTextureData == null || _mesh != targetMesh || _loadKey_SelectTextureDataToMesh != loadKey)
			{
				_loadKey_SelectTextureDataToMesh = null;
				return;
			}

			_loadKey_SelectTextureDataToMesh = null;

			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_SetImage, targetMesh, resultTextureData, false, Editor);

			_mesh._textureData = resultTextureData;
			_isShowTextureDataList = false;

		}



		private void MeshProperty_Modify(int width, int height)
		{
			GUILayout.Space(10);
			//EditorGUILayout.LabelField("Left Click : Select Vertex");
			DrawHowToControl(width, "Select Vertex", null, null);

			EditorGUILayout.Space();

			Editor.SetGUIVisible("Mesh Property Modify UI", (Editor.VertController.Vertex != null));
			Editor.SetGUIVisible("Mesh Property Modify UI No Info", (Editor.VertController.Vertex == null));

			if (Editor.VertController.Vertex != null)
			{
				if (Editor.IsDelayedGUIVisible("Mesh Property Modify UI"))
				{
					EditorGUILayout.LabelField("Index : " + Editor.VertController.Vertex._index);

					EditorGUILayout.LabelField("Position");
					Vector3 pos3 = Editor.VertController.Vertex._pos;
					/*Vector2 pos2Edit = */
					//EditorGUILayout.Vector2Field("", new Vector2(pos3.x, pos3.y));//<<사용할때 풀자
					apEditorUtil.DelayedVector2Field(new Vector2(pos3.x, pos3.y), width);

					EditorGUILayout.LabelField("UV");
					/*Vector2 uv = */
					//EditorGUILayout.Vector2Field("", Editor.VertController.Vertex._uv);//<<사용할때 풀자
					apEditorUtil.DelayedVector2Field(Editor.VertController.Vertex._uv, width);

					//Vertex의 자체 Weight는 사용하지 않는다.
					//EditorGUILayout.LabelField("Volume Weight");
					//float volumeWeightX100 = EditorGUILayout.DelayedFloatField(Editor.VertController.Vertex._volumeWeight * 100.0f);
					//volumeWeightX100 = Mathf.Clamp(volumeWeightX100, 0.0f, 100.0f);
					//Editor.VertController.Vertex._volumeWeight = volumeWeightX100 * 0.01f;

					//EditorGUILayout.LabelField("Physic Weight");
					//float physicWeightX100 = EditorGUILayout.DelayedFloatField(Editor.VertController.Vertex._physicsWeight * 100.0f);
					//physicWeightX100 = Mathf.Clamp(physicWeightX100, 0.0f, 100.0f);
					//Editor.VertController.Vertex._physicsWeight = physicWeightX100 * 0.01f;
				}
			}
			else
			{
				if (Editor.IsDelayedGUIVisible("Mesh Property Modify UI No Info"))
				{
					EditorGUILayout.LabelField("No vertex selected");
				}
			}
		}

		private void MeshProperty_MakeMesh(int width, int height)
		{
			GUILayout.Space(10);

			switch (Editor._meshEditeMode_MakeMesh)
			{
				case apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge:
					DrawHowToControl(width, "Add or Move Vertex with Edges", "Move View", "Remove Vertex or Edge", null, "Snap to Vertex", "L:Cut Edge / R:Delete Vertex");
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly:
					DrawHowToControl(width, "Add or Move Vertex", "Move View", "Remove Vertex");
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly:
					DrawHowToControl(width, "Link Vertices / Turn Edge", "Move View", "Remove Edge", null, "Snap to Vertex", "Cut Edge");
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon:
					DrawHowToControl(width, "Select Polygon", "Move View", null, "Remove Polygon");
					break;
			}

			EditorGUILayout.Space();

			Texture2D icon_EditVertexWithEdge = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_VertexEdge);
			Texture2D icon_EditVertexOnly = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_VertexOnly);
			Texture2D icon_EditEdgeOnly = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_EdgeOnly);
			Texture2D icon_EditPolygon = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Polygon);

			bool isSubEditMode_VE = (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge);
			bool isSubEditMode_Vertex = (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly);
			bool isSubEditMode_Edge = (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly);
			bool isSubEditMode_Polygon = (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon);

			//int btnWidth = (width / 3) - 4;
			int btnWidth = (width / 4) - 4;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(45));
			GUILayout.Space(5);
			bool nextEditMode_VE = apEditorUtil.ToggledButton(icon_EditVertexWithEdge, isSubEditMode_VE, true, btnWidth, 35);
			bool nextEditMode_Vertex = apEditorUtil.ToggledButton(icon_EditVertexOnly, isSubEditMode_Vertex, true, btnWidth, 35);
			bool nextEditMode_Edge = apEditorUtil.ToggledButton(icon_EditEdgeOnly, isSubEditMode_Edge, true, btnWidth, 35);
			bool nextEditMode_Polygon = apEditorUtil.ToggledButton(icon_EditPolygon, isSubEditMode_Polygon, true, btnWidth, 35);

			EditorGUILayout.EndHorizontal();

			if (nextEditMode_VE && !isSubEditMode_VE)
			{
				Editor._meshEditeMode_MakeMesh = apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge;
				Editor.VertController.UnselectVertex();
			}

			if (nextEditMode_Vertex && !isSubEditMode_Vertex)
			{
				Editor._meshEditeMode_MakeMesh = apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly;
				Editor.VertController.UnselectVertex();
			}

			if (nextEditMode_Edge && !isSubEditMode_Edge)
			{
				Editor._meshEditeMode_MakeMesh = apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly;
				Editor.VertController.UnselectVertex();
			}

			if (nextEditMode_Polygon && !isSubEditMode_Polygon)
			{
				Editor._meshEditeMode_MakeMesh = apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon;
				Editor.VertController.UnselectVertex();
			}

			GUILayout.Space(5);

			Color makeMeshModeColor = Color.black;
			string strMakeMeshModeInfo = "";
			switch (Editor._meshEditeMode_MakeMesh)
			{
				case apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge:
					strMakeMeshModeInfo = "Add Vertex / Link Edge";
					makeMeshModeColor = new Color(0.87f, 0.57f, 0.92f, 1.0f);
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly:
					strMakeMeshModeInfo = "Add Vertex";
					makeMeshModeColor = new Color(0.57f, 0.82f, 0.95f, 1.0f);
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly:
					strMakeMeshModeInfo = "Link Edge";
					makeMeshModeColor = new Color(0.95f, 0.65f, 0.65f, 1.0f);
					break;

				case apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon:
					strMakeMeshModeInfo = "Polygon";
					makeMeshModeColor = new Color(0.65f, 0.95f, 0.65f, 1.0f);
					break;
			}
			//Polygon HotKey 이벤트 추가
			if (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
			{
				Editor.AddHotKeyEvent(Editor.Controller.RemoveSelectedMeshPolygon, "Remove Polygon", KeyCode.Delete, false, false, false);
			}


			GUIStyle guiStyle_Info = new GUIStyle(GUI.skin.box);
			guiStyle_Info.alignment = TextAnchor.MiddleCenter;

			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = makeMeshModeColor;
			GUILayout.Box(strMakeMeshModeInfo, guiStyle_Info, GUILayout.Width(width - 8), GUILayout.Height(34));

			GUI.backgroundColor = prevColor;

			GUILayout.Space(20);
			if (GUILayout.Button(new GUIContent("Auto Link Edge", Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_AutoLink)), GUILayout.Height(30)))
			{
				//Undo
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_MakeEdges, Editor.Select.Mesh, Editor.Select.Mesh, false, Editor);

				//Editor.VertController.StopEdgeWire();
				Editor.Select.Mesh.AutoLinkEdges();
			}
			GUILayout.Space(20);
			if (GUILayout.Button(new GUIContent("Make Polygons", Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MakePolygon)), GUILayout.Height(40)))
			{
				//Undo
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_MakeEdges, Editor.Select.Mesh, Editor.Select.Mesh, false, Editor);

				//Editor.VertController.StopEdgeWire();

				Editor.Select.Mesh.MakeEdgesToPolygonAndIndexBuffer();
				Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
				Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
			}

			GUILayout.Space(30);

			if (GUILayout.Button("Remove All Vertices"))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove All Vertices", "Do you want to remove All vertices? (Not Undo)", "Remove All", "Cancel");

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveMeshVertices_Title),
																Editor.GetText(apLocalization.TEXT.RemoveMeshVertices_Body),
																Editor.GetText(apLocalization.TEXT.RemoveMeshVertices_Okay),
																Editor.GetText(apLocalization.TEXT.Cancel));

				if (isResult)
				{
					//Undo
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_RemoveAllVertices, _mesh, null, false, Editor);

					_mesh._vertexData.Clear();
					_mesh._indexBuffer.Clear();
					_mesh._edges.Clear();
					_mesh._polygons.Clear();

					_mesh.MakeEdgesToPolygonAndIndexBuffer();

					Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

					Editor.VertController.UnselectVertex();
					Editor.VertController.UnselectNextVertex();
				}
			}

		}



		private void MeshProperty_Pivot(int width, int height)
		{
			GUILayout.Space(10);
			//EditorGUILayout.LabelField("Left Drag : Change Pivot To Origin");
			DrawHowToControl(width, "Move Pivot", null, null, null);

			EditorGUILayout.Space();

			if (GUILayout.Button("Reset Pivot", GUILayout.Height(40)))
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_SetPivot, _mesh, _mesh._offsetPos, false, Editor);

				Editor.Select.Mesh._offsetPos = Vector2.zero;

				Editor.Select.Mesh.MakeOffsetPosMatrix();//<<OffsetPos를 수정하면 이걸 바꿔주자
			}
		}




		//----------------------------------------------------------------------------
		//private string _prevMeshGroup_Name = "";

		private void MeshGroupProperty_Setting(int width, int height)
		{
			EditorGUILayout.LabelField("Name");
			string nextMeshGroupName = EditorGUILayout.DelayedTextField(_meshGroup._name, GUILayout.Width(width));
			if (!string.Equals(nextMeshGroupName, _meshGroup._name))
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, _meshGroup, null, false, Editor);
				_meshGroup._name = nextMeshGroupName;
				Editor.RefreshControllerAndHierarchy();
			}

			#region [미사용 코드] DelayedTextField 사용 전 코드
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//_prevMeshGroup_Name = EditorGUILayout.TextField(_prevMeshGroup_Name);
			//if (GUILayout.Button("Change", GUILayout.Width(80)))
			//{
			//	if (!string.IsNullOrEmpty(_prevMeshGroup_Name))
			//	{
			//		_meshGroup._name = _prevMeshGroup_Name;

			//		//Editor.Hierarchy.RefreshUnits();
			//		Editor.RefreshControllerAndHierarchy();
			//	}
			//}
			//EditorGUILayout.EndHorizontal(); 
			#endregion
			//EditorGUILayout.Space();

			GUILayout.Space(20);

			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_MeshGroupDefaultTransform),
				" Editing..",
				" Edit Default Transform",
				_isMeshGroupSetting_ChangePivot, true, width, 30))
			{
				_isMeshGroupSetting_ChangePivot = !_isMeshGroupSetting_ChangePivot;
				if (_isMeshGroupSetting_ChangePivot)
				{
					//Modifier 모두 비활성화
					MeshGroup._modifierStack.SetExclusiveModifierInEditing(null, null);
				}
				else
				{
					//Modifier 모두 활성화
					MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
				}
			}

			GUILayout.Space(20);

			//프리셋 타입은 사용하지 않는다.
			#region [미사용 코드]
			//EditorGUILayout.LabelField("Preset Type");
			//apMeshGroup.PRESET_TYPE nextPreset = (apMeshGroup.PRESET_TYPE)EditorGUILayout.EnumPopup(_meshGroup._presetType);
			//if (nextPreset != _meshGroup._presetType)
			//{
			//	_meshGroup._presetType = nextPreset;
			//	//Refresh?
			//}
			//EditorGUILayout.Space(); 
			#endregion


			//MainMesh에 포함되는가
			bool isMainMeshGroup = _portrait._mainMeshGroupList.Contains(MeshGroup);
			if (isMainMeshGroup)
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.alignment = TextAnchor.MiddleCenter;
				Color prevColor = GUI.backgroundColor;
				GUI.backgroundColor = new Color(0.5f, 0.7f, 0.9f, 1.0f);

				GUILayout.Box("Main Portrait", guiStyle, GUILayout.Width(width), GUILayout.Height(30));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				if (GUILayout.Button("Set Main Portrait", GUILayout.Width(width), GUILayout.Height(30)))
				{
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Portrait_SetMeshGroup, _portrait, MeshGroup, false, Editor);

					_portrait._mainMeshGroupIDList.Add(MeshGroup._uniqueID);
					_portrait._mainMeshGroupList.Add(MeshGroup);

					apRootUnit newRootUnit = new apRootUnit();
					newRootUnit.SetPortrait(_portrait);
					newRootUnit.SetMeshGroup(MeshGroup);

					_portrait._rootUnits.Add(newRootUnit);

					//_portrait._mainMeshGroup = MeshGroup;
					//_portrait._mainMeshGroupID = MeshGroup._uniqueID;
					//_portrait._rootUnit._childMeshGroup = MeshGroup;
					//_portrait._rootUnit.SetMeshGroup(MeshGroup);
					Editor.RefreshControllerAndHierarchy();

					//Root Hierarchy Filter를 활성화한다.
					Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.RootUnit, true);
				}
			}




			GUILayout.Space(20);

			apEditorUtil.GUI_DelimeterBoxH(width - 10);

			//등등
			GUILayout.Space(30);
			if (GUILayout.Button("Remove Mesh Group"))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Mesh Group", "Do you want to remove [" + _meshGroup._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveMeshGroup_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveMeshGroup_Body, _meshGroup._name),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);
				if (isResult)
				{
					Editor.Controller.RemoveMeshGroup(_meshGroup);

					SetNone();
				}
			}


		}



		private void MeshGroupProperty_Bone(int width, int height)
		{
			GUILayout.Space(10);

			Editor.SetGUIVisible("BoneEditMode - Editable", _isBoneDefaultEditing);

			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_EditMode),
												" Editing Bones", " Start Editing Bones",
												IsBoneDefaultEditing, true, width, 30))
			{
				//Bone을 수정할 수 있다.
				SetBoneEditing(!_isBoneDefaultEditing, true);
			}

			GUILayout.Space(5);

			//Add 툴과 Select 툴 On/Off

			Editor.SetGUIVisible("BoneEditMode - Select", _boneEditMode == BONE_EDIT_MODE.SelectAndTRS);
			Editor.SetGUIVisible("BoneEditMode - Add", _boneEditMode == BONE_EDIT_MODE.Add);
			Editor.SetGUIVisible("BoneEditMode - Link", _boneEditMode == BONE_EDIT_MODE.Link);

			bool isBoneEditable = Editor.IsDelayedGUIVisible("BoneEditMode - Editable");
			bool isBoneEditMode_Select = Editor.IsDelayedGUIVisible("BoneEditMode - Select");
			bool isBoneEditMode_Add = Editor.IsDelayedGUIVisible("BoneEditMode - Add");
			bool isBoneEditMode_Link = Editor.IsDelayedGUIVisible("BoneEditMode - Link");

			int subTabWidth = (width / 3) - 4;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));


			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Select),
											isBoneEditMode_Select, _isBoneDefaultEditing,
											subTabWidth, 40))
			{
				SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Add),
											isBoneEditMode_Add, _isBoneDefaultEditing,
											subTabWidth, 40))
			{
				SetBoneEditMode(BONE_EDIT_MODE.Add, true);
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Link),
											isBoneEditMode_Link, _isBoneDefaultEditing,
											subTabWidth, 40))
			{
				SetBoneEditMode(BONE_EDIT_MODE.Link, true);
			}

			EditorGUILayout.EndHorizontal();


			GUILayout.Space(5);

			if (isBoneEditable)
			{
				string strBoneEditInfo = "";
				Color prevColor = GUI.color;
				Color colorBoneEdit = Color.black;
				switch (_boneEditMode)
				{
					case BONE_EDIT_MODE.None:
						strBoneEditInfo = "No Editable";
						colorBoneEdit = new Color(0.6f, 0.6f, 0.6f, 1.0f);
						break;

					case BONE_EDIT_MODE.SelectOnly:
						strBoneEditInfo = "Select Bones";
						colorBoneEdit = new Color(0.6f, 0.9f, 0.9f, 1.0f);
						break;

					case BONE_EDIT_MODE.SelectAndTRS:
						strBoneEditInfo = "Select Bones";
						colorBoneEdit = new Color(0.5f, 0.9f, 0.6f, 1.0f);
						break;

					case BONE_EDIT_MODE.Add:
						strBoneEditInfo = "Add Bones";
						colorBoneEdit = new Color(0.95f, 0.65f, 0.65f, 1.0f);
						break;

					case BONE_EDIT_MODE.Link:
						strBoneEditInfo = "Link Bones";
						colorBoneEdit = new Color(0.57f, 0.82f, 0.95f, 1.0f);
						break;
				}

				GUIStyle guiStyle_Info = new GUIStyle(GUI.skin.box);
				guiStyle_Info.alignment = TextAnchor.MiddleCenter;

				GUI.color = colorBoneEdit;
				GUILayout.Box(strBoneEditInfo, guiStyle_Info, GUILayout.Width(width - 8), GUILayout.Height(34));

				GUI.color = prevColor;

				GUILayout.Space(5);
				switch (_boneEditMode)
				{
					case BONE_EDIT_MODE.None:
						DrawHowToControl(width, "None", "Move View", "None", null);
						break;

					case BONE_EDIT_MODE.SelectOnly:
						DrawHowToControl(width, "Select Bones", "Move View", "Deselect", null);//<<삭제 포함해야할 듯?
						break;

					case BONE_EDIT_MODE.SelectAndTRS:
						DrawHowToControl(width, "Select Bones", "Move View", "Deselect", null);
						break;

					case BONE_EDIT_MODE.Add:
						DrawHowToControl(width, "Add Bones", "Move View", "Deselect", null);
						break;

					case BONE_EDIT_MODE.Link:
						DrawHowToControl(width, "Select and Link Bones", "Move View", "Deselect", null);
						break;
				}

			}
			GUILayout.Space(20);

			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);

			if (GUILayout.Button("Remove All Bones", GUILayout.Width(width)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Bones", "Remove All Bones?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveBonesAll_Title),
																Editor.GetText(apLocalization.TEXT.RemoveBonesAll_Body),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);
				if (isResult)
				{
					Editor.Controller.RemoveAllBones(MeshGroup);
				}
			}
		}

		private object _loadKey_AddModifier = null;
		private void MeshGroupProperty_Modify(int width, int height)
		{
			//EditorGUILayout.LabelField("Presets");
			GUILayout.Space(10);

			if (GUILayout.Button("Add Modifier", GUILayout.Height(30)))
			{
				_loadKey_AddModifier = apDialog_AddModifier.ShowDialog(Editor, MeshGroup, OnAddModifier);
			}

			#region [미사용 코드] 여기서 만든 리스트 대신 다이얼로그로 대체합니다.
			//Rect lastRect = GUILayoutUtility.GetLastRect();
			//Color prevColor = GUI.backgroundColor;

			//string[] strModifiers = new string[] { "Volume", "Morph", "Animated Morph", "Rigging", "Physic" };

			//GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
			//GUI.Box(new Rect(lastRect.x + 5, lastRect.y + 10, width - 10, strModifiers.Length * 22 + 3), "");
			//GUI.backgroundColor = prevColor;

			//GUIStyle _guiStyle_None = new GUIStyle(GUIStyle.none);
			//_guiStyle_None.normal.textColor = Color.black;
			//_guiStyle_None.onHover.textColor = Color.cyan;

			//int iAddModifier = -1;
			//for (int i = 0; i < strModifiers.Length; i++)
			//{
			//	apModifierBase.MODIFIER_TYPE modType = (apModifierBase.MODIFIER_TYPE)(i + 1);
			//	apImageSet.PRESET iconType = apEditorUtil.GetModifierIconType(modType);

			//	EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
			//	GUILayout.Space(15);
			//	EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(iconType)), GUILayout.Width(20), GUILayout.Height(20));
			//	GUILayout.Space(5);

			//	EditorGUILayout.LabelField(strModifiers[i], GUILayout.Width(width - (25 + 50 + 25)));
			//	if(GUILayout.Button("Add", GUILayout.Width(45)))
			//	{
			//		iAddModifier = i;
			//	}
			//	EditorGUILayout.EndHorizontal();
			//}

			//if(iAddModifier >= 0)
			//{
			//	apModifierBase.MODIFIER_TYPE modType = apModifierBase.MODIFIER_TYPE.Base;
			//	switch (iAddModifier)
			//	{
			//		case 0: modType = apModifierBase.MODIFIER_TYPE.Volume; break;
			//		case 1: modType = apModifierBase.MODIFIER_TYPE.Morph; break;
			//		case 2: modType = apModifierBase.MODIFIER_TYPE.AnimatedMorph; break;
			//		case 3: modType = apModifierBase.MODIFIER_TYPE.Rigging; break;
			//		case 4: modType = apModifierBase.MODIFIER_TYPE.Physic; break;
			//	}

			//	if(modType != apModifierBase.MODIFIER_TYPE.Base)
			//	{
			//		Editor.Controller.AddModifier(modType);
			//	}
			//}
			//GUILayout.Space(15); 
			#endregion

			GUILayout.Space(20);
			//EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(2);
			EditorGUILayout.LabelField("Modifier Stack", GUILayout.Height(25));

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			GUILayout.Button("", guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));//<레이아웃 정렬을 위한의미없는 숨은 버튼
			EditorGUILayout.EndHorizontal();
			apModifierStack modStack = MeshGroup._modifierStack;


			//등록된 Modifier 리스트를 출력하자
			if (modStack._modifiers.Count > 0)
			{
				//int iLayerSortChange = -1;
				//bool isLayerUp = false;//<<Up : 레이어값을 올린다.

				//역순으로 출력한다.
				for (int i = modStack._modifiers.Count - 1; i >= 0; i--)
				{
					DrawModifierLayerUnit(modStack._modifiers[i], width, 25);

					#region [미사용 코드]
					////bool isLayerMoveUp = (i < modStack._modifiers.Count - 1);
					////bool isLayerMoveDown = i > 0;

					////int layerChangeResult = DrawModifierLayerUnit(modStack._modifiers[i], width, 25, isLayerMoveUp, isLayerMoveDown);
					//int layerChangeResult = DrawModifierLayerUnit(modStack._modifiers[i], width, 25);
					//if (layerChangeResult != 0)
					//{
					//	//iLayerSortChange = i;
					//	//if(layerChangeResult > 0)
					//	//{
					//	//	isLayerUp = true;
					//	//}
					//	//else
					//	//{
					//	//	isLayerUp = false;
					//	//}
					//} 
					#endregion
				}

				//레이어 바꾸는 기능은 다른 곳에서..
				//if(iLayerSortChange >= 0)
				//{
				//	Editor.Controller.LayerChange(modStack._modifiers[iLayerSortChange], isLayerUp);
				//}
			}


		}

		private void OnAddModifier(bool isSuccess, object loadKey, apModifierBase.MODIFIER_TYPE modifierType, apMeshGroup targetMeshGroup, int validationKey)
		{
			if (!isSuccess || _loadKey_AddModifier != loadKey || MeshGroup != targetMeshGroup)
			{
				_loadKey_AddModifier = null;
				return;
			}

			if (modifierType != apModifierBase.MODIFIER_TYPE.Base)
			{	
				Editor.Controller.AddModifier(modifierType, validationKey);
			}
			_loadKey_AddModifier = null;
		}

		//private int DrawModifierLayerUnit(apModifierBase modifier, int width, int height, bool isLayerUp, bool isLayerDown)
		private int DrawModifierLayerUnit(apModifierBase modifier, int width, int height)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();

			if (Modifier == modifier)
			{
				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

				GUI.Box(new Rect(lastRect.x, lastRect.y + height, width + 15, height), "");
				GUI.backgroundColor = prevColor;
			}

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = Color.black;

			apImageSet.PRESET iconType = apEditorUtil.GetModifierIconType(modifier.ModifierType);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			GUILayout.Space(10);
			if (GUILayout.Button(new GUIContent(" " + modifier.DisplayName, Editor.ImageSet.Get(iconType)), guiStyle_None, GUILayout.Width(width - 40), GUILayout.Height(height)))
			{
				SetModifier(modifier);
			}

			int iResult = 0;

			Texture2D activeBtn = null;
			bool isActiveMod = false;
			if (modifier._isActive && modifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled)
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Active);
				isActiveMod = true;
			}
			else
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Deactive);
				isActiveMod = false;
			}
			if (GUILayout.Button(activeBtn, guiStyle_None, GUILayout.Width(height), GUILayout.Height(height)))
			{
				//일단 토글한다.
				modifier._isActive = !isActiveMod;

				if (ExEditingMode != EX_EDIT.None)
				{
					if (modifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled)
					{
						//작업이 허용된 Modifier가 아닌데 Active를 제어했다면
						//ExEdit를 해제해야한다.
						SetModifierExclusiveEditing(EX_EDIT.None);
					}
				}


				//if (!ExEditingMode)
				//{
				//	//Debug.LogError("TODO : Active를 바꾸면, 녹화 기능이 무조건 비활성화되어야 한다.");
				//	SetModifierExclusiveEditing(false);
				//}
			}
			EditorGUILayout.EndHorizontal();

			return iResult;
		}

		//------------------------------------------------------------------------------------
		public void DrawEditor_Right2(int width, int height)
		{
			if (Editor == null || Editor.Select.Portrait == null)
			{
				return;
			}

			EditorGUILayout.Space();

			switch (_selectionType)
			{
				case SELECTION_TYPE.MeshGroup:
					{
						switch (Editor._meshGroupEditMode)
						{
							case apEditor.MESHGROUP_EDIT_MODE.Setting:
								DrawEditor_Right2_MeshGroupRight_Setting(width, height);
								break;

							case apEditor.MESHGROUP_EDIT_MODE.Bone:
								DrawEditor_Right2_MeshGroup_Bone(width, height);
								break;

							case apEditor.MESHGROUP_EDIT_MODE.Modifier:
								DrawEditor_Right2_MeshGroup_Modifier(width, height);
								break;
						}
					}
					break;

				case SELECTION_TYPE.Animation:
					{
						DrawEditor_Right2_Animation(width, height);
					}
					break;
			}
		}


		//--------------------------------------------------------------------------------------
		public void DrawEditor_Bottom2Edit(int width, int height)
		{
			if (Editor == null || Editor.Select.Portrait == null || Modifier == null)
			{
				return;
			}

			GUIStyle btnGUIStyle = new GUIStyle(GUI.skin.button);
			btnGUIStyle.alignment = TextAnchor.MiddleLeft;

			bool isRiggingModifier = (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging);
			bool isWeightedVertModifier = (int)(Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) != 0
										|| (int)(Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Volume) != 0;
			//기본 Modifier가 있고
			//Rigging용 Modifier UI가 따로 있다.
			//추가 : Weight값을 사용하는 Physic/Volume도 따로 설정
			if (isRiggingModifier)
			{
				//리깅 타입인 경우
				//리깅 편집 툴 / 보기 버튼들이 나온다.
				//1. Rigging On/Off
				//+ 선택된 Mesh Transform
				//2. View 모드
				//3. Test Posing On/Off
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_EditBinding),
														"  Binding..", "  Start Binding", _rigEdit_isBindingEdit, true, 140, height, btnGUIStyle))
				{
					_rigEdit_isBindingEdit = !_rigEdit_isBindingEdit;
					_rigEdit_isTestPosing = false;

					//작업중인 Modifier 외에는 일부 제외를 하자
					if (_rigEdit_isBindingEdit)
					{
						MeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, SubEditedParamSetGroup);
						_isLockExEditKey = true;
					}
					else
					{
						if (MeshGroup != null)
						{
							//Exclusive 모두 해제
							MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
							Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
						}
						_isLockExEditKey = false;
					}
				}
				GUILayout.Space(10);

			}
			else
			{
				//그외의 Modifier
				//편집 On/Off와 현재 선택된 Key/Value가 나온다.

				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Recording),
													Editor.ImageSet.Get(apImageSet.PRESET.Edit_Record),
													Editor.ImageSet.Get(apImageSet.PRESET.Edit_NoRecord),
													"  Editing..", "  Start Editing", "  Not Editiable",
													_exclusiveEditing != EX_EDIT.None, IsExEditable, 140, height, btnGUIStyle))
				{
					EX_EDIT nextResult = EX_EDIT.None;
					if (_exclusiveEditing == EX_EDIT.None && IsExEditable)
					{
						//None -> ExOnly로 바꾼다.
						//General은 특별한 경우
						nextResult = EX_EDIT.ExOnly_Edit;
					}
					//if (IsExEditable || !isNextResult)
					//{
					//	//SetModifierExclusiveEditing(isNextResult);
					//}
					SetModifierExclusiveEditing(nextResult);
					if (nextResult == EX_EDIT.ExOnly_Edit)
					{
						_isLockExEditKey = true;//처음 Editing 작업시 Lock을 거는 것으로 변경
					}
					else
					{
						_isLockExEditKey = false;//Editing 해제시 Lock 해제
					}
				}


				#region [미사용 코드]
				//if (GUILayout.Button(new GUIContent(strButtonName, editIcon), btnGUIStyle, GUILayout.Width(120), GUILayout.Height(height)))
				//{
				//	bool isNextResult = !_isExclusiveEditing;
				//	if (IsExEditable || !isNextResult)
				//	{
				//		SetModifierExclusiveEditing(isNextResult);
				//	}
				//} 
				#endregion

				GUILayout.Space(10);
				//Lock 걸린 키 / 수정중인 객체 / 그 값을 각각 표시하자

			}
			#region [미사용 코드]
			//GUIStyle guiStyle_Key = new GUIStyle(GUI.skin.label);
			//Texture2D lockIcon = null;
			//if (IsLockExEditKey)
			//{
			//	lockIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Lock);
			//	guiStyle_Key.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			//}
			//else
			//{
			//	lockIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Unlock);
			//}




			//GUIStyle guiStyle_NotSelected = new GUIStyle(GUI.skin.label);
			//guiStyle_NotSelected.normal.textColor = new Color(0.0f, 0.5f, 1.0f, 1.0f);

			//if (GUILayout.Button(lockIcon, GUILayout.Width(height), GUILayout.Height(height)))
			//{
			//	SetModifierExclusiveEditKeyLock(!IsLockExEditKey);
			//} 
			#endregion


			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionLock),
												Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionUnlock),
												IsLockExEditKey, true, height, height))
			{
				SetModifierExclusiveEditKeyLock(!IsLockExEditKey);
			}

			//토글 단축키를 입력하자 [Space]
			Editor.AddHotKeyEvent(OnHotKeyEvent_ToggleExclusiveEditKeyLock, "Toggle Selection Lock", KeyCode.Space, false, false, false);

			GUILayout.Space(10);

			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_ModLock),
												Editor.ImageSet.Get(apImageSet.PRESET.Edit_ModUnlock),
												_exclusiveEditing == EX_EDIT.ExOnly_Edit,
												IsExEditable && _exclusiveEditing != EX_EDIT.None,
												height, height))
			{
				//여기서 ExOnly <-> General 사이를 바꾼다.
				if (IsExEditable && _exclusiveEditing != EX_EDIT.None)
				{
					EX_EDIT nextEditMode = EX_EDIT.ExOnly_Edit;
					if (_exclusiveEditing == EX_EDIT.ExOnly_Edit)
					{
						nextEditMode = EX_EDIT.General_Edit;
					}
					SetModifierExclusiveEditing(nextEditMode);
				}
			}

			GUILayout.Space(10);

			apImageSet.PRESET modImagePreset = apEditorUtil.GetModifierIconType(Modifier.ModifierType);

			GUIStyle guiStyle_Key = new GUIStyle(GUI.skin.label);
			if (IsLockExEditKey)
			{
				guiStyle_Key.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			}

			GUIStyle guiStyle_NotSelected = new GUIStyle(GUI.skin.label);
			guiStyle_NotSelected.normal.textColor = new Color(0.0f, 0.5f, 1.0f, 1.0f);


			switch (_exEditKeyValue)
			{
				case EX_EDIT_KEY_VALUE.None:
					break;

				case EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert:
				case EX_EDIT_KEY_VALUE.ParamKey_ModMesh://ModVert와 ModMesh는 비슷하다
					{
						//Key
						EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(modImagePreset)), GUILayout.Width(height), GUILayout.Height(height));


						Texture2D selectedImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);

						string strKey_ParamSetGroup = "<No Parameter>";
						string strKey_ParamSet = "<No Key>";
						string strKey_ModMesh = "<Not Selected>";
						string strKey_ModMeshLabel = "Sub Object";

						GUIStyle guiStyle_ParamSetGroup = guiStyle_NotSelected;
						GUIStyle guiStyle_ParamSet = guiStyle_NotSelected;
						GUIStyle guiStyle_Transform = guiStyle_NotSelected;

						if (ExKey_ModParamSetGroup != null)
						{
							if (ExKey_ModParamSetGroup._keyControlParam != null)
							{
								strKey_ParamSetGroup = ExKey_ModParamSetGroup._keyControlParam._keyName;
								guiStyle_ParamSetGroup = guiStyle_Key;
							}
						}

						if (ExKey_ModParamSet != null)
						{
							//TODO : 컨트롤 타입이 아니면 다른 이름을 쓰자
							strKey_ParamSet = ExKey_ModParamSet.ControlParamValue;
							guiStyle_ParamSet = guiStyle_Key;
						}

						apModifiedMesh modMesh = null;
						if (_exEditKeyValue == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert)
						{
							modMesh = ExKey_ModMesh;
						}
						else
						{
							modMesh = ExValue_ModMesh;
						}

						if (modMesh != null)
						{
							if (modMesh._transform_Mesh != null)
							{
								strKey_ModMeshLabel = "Sub Mesh";
								strKey_ModMesh = modMesh._transform_Mesh._nickName;
								selectedImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
								guiStyle_Transform = guiStyle_Key;
							}
							else if (modMesh._transform_MeshGroup != null)
							{
								strKey_ModMeshLabel = "Sub MeshGroup";
								strKey_ModMesh = modMesh._transform_MeshGroup._nickName;
								selectedImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
								guiStyle_Transform = guiStyle_Key;
							}
						}
						else
						{
							if (ExKey_ModParamSet == null)
							{
								//Key를 먼저 선택할 것을 알려야한다.
								strKey_ModMesh = "<Select Key First>";
							}
						}

						if (Modifier.SyncTarget != apModifierParamSetGroup.SYNC_TARGET.Static)
						{
							EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(height));
							EditorGUILayout.LabelField(strKey_ParamSetGroup, guiStyle_ParamSetGroup, GUILayout.Width(100));
							EditorGUILayout.LabelField(strKey_ParamSet, guiStyle_ParamSet, GUILayout.Width(100));
							EditorGUILayout.EndVertical();
						}
						else
						{
							EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(height));
							EditorGUILayout.LabelField(Modifier.DisplayName, guiStyle_Key, GUILayout.Width(100));
							//EditorGUILayout.LabelField(strKey_ParamSet, guiStyle_ParamSet, GUILayout.Width(100));
							EditorGUILayout.EndVertical();
						}
						EditorGUILayout.LabelField(new GUIContent(selectedImage), GUILayout.Width(height), GUILayout.Height(height));
						EditorGUILayout.BeginVertical(GUILayout.Width(170), GUILayout.Height(height));
						EditorGUILayout.LabelField(strKey_ModMeshLabel, GUILayout.Width(170));
						EditorGUILayout.LabelField(strKey_ModMesh, guiStyle_Transform, GUILayout.Width(170));
						EditorGUILayout.EndVertical();


						GUILayout.Space(10);
						apEditorUtil.GUI_DelimeterBoxV(height - 6);
						GUILayout.Space(10);

						//Value
						//(선택한 Vert의 값을 출력하자. 단, Rigging Modifier가 아닐때)
						if (_exEditKeyValue == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert && !isRiggingModifier && !isWeightedVertModifier)
						{

							bool isModVertSelected = (ExValue_ModVert != null);
							Editor.SetGUIVisible("Bottom2 Transform Mod Vert", isModVertSelected);

							if (Editor.IsDelayedGUIVisible("Bottom2 Transform Mod Vert"))
							{
								EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Vertex)), GUILayout.Width(height), GUILayout.Height(height));
								EditorGUILayout.BeginVertical(GUILayout.Width(150), GUILayout.Height(height));
								EditorGUILayout.LabelField("Vertex : " + ExValue_ModVert._modVert._vertexUniqueID, GUILayout.Width(150));

								//Vector2 newDeltaPos = EditorGUILayout.Vector2Field("", ExValue_ModVert._modVert._deltaPos, GUILayout.Width(150));
								Vector2 newDeltaPos = apEditorUtil.DelayedVector2Field(ExValue_ModVert._modVert._deltaPos, 150);
								if (ExEditingMode != EX_EDIT.None)
								{
									ExValue_ModVert._modVert._deltaPos = newDeltaPos;
								}
								EditorGUILayout.EndVertical();
							}
						}


					}
					break;

				case EX_EDIT_KEY_VALUE.ParamKey_Bone:
					{
						EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(modImagePreset)), GUILayout.Width(height), GUILayout.Height(height));
					}
					break;

			}

			if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 타입이면 몇가지 제어 버튼이 추가된다.
				//2. View 모드
				//3. Test Posing On/Off
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_WeightColorWithTexture), _rigEdit_viewMode == RIGGING_EDIT_VIEW_MODE.WeightWithTexture, true, height + 5, height))
				{
					_rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightWithTexture;
				}

				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_WeightColorOnly), _rigEdit_viewMode == RIGGING_EDIT_VIEW_MODE.WeightColorOnly, true, height + 5, height))
				{
					_rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightColorOnly;
				}

				GUILayout.Space(2);

				if (apEditorUtil.ToggledButton_2Side("Bone Color", "Bone Color", _rigEdit_isBoneColorView, true, 80, height))
				{
					_rigEdit_isBoneColorView = !_rigEdit_isBoneColorView;
					Editor.SaveEditorPref();//<<이것도 Save 요건
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxV(height - 6);
				GUILayout.Space(10);

				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_TestPosing),
													"  Pose Test", "  Pose Test", _rigEdit_isTestPosing, _rigEdit_isBindingEdit, 110, height))
				{
					_rigEdit_isTestPosing = !_rigEdit_isTestPosing;
					SetBoneRiggingTest();

				}
				if (GUILayout.Button("Reset Pose", GUILayout.Width(90), GUILayout.Height(height)))
				{
					ResetRiggingTestPose();
				}
			}
			else if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
			{
				//테스트로 시뮬레이션을 할 수 있다.
				//바람을 켜고 끌 수 있다.
				EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(height));
				EditorGUILayout.LabelField("Direction", GUILayout.Width(100));
				_physics_windSimulationDir = apEditorUtil.DelayedVector2Field(_physics_windSimulationDir, 100 - 4);
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(height));
				EditorGUILayout.LabelField("Power", GUILayout.Width(100));
				_physics_windSimulationScale = EditorGUILayout.DelayedFloatField(_physics_windSimulationScale, GUILayout.Width(100));
				EditorGUILayout.EndVertical();

				if (GUILayout.Button("Wind On", GUILayout.Width(90), GUILayout.Height(height)))
				{
					GUI.FocusControl(null);

					if (_portrait != null)
					{
						_portrait.ClearForce();
						_portrait.AddForce_Direction(_physics_windSimulationDir,
							0.3f,
							0.3f,
							3, 5)
							.SetPower(_physics_windSimulationScale, _physics_windSimulationScale * 0.3f, 4.0f)
							.EmitLoop();
					}
				}
				if (GUILayout.Button("Wind Off", GUILayout.Width(90), GUILayout.Height(height)))
				{
					GUI.FocusControl(null);
					if (_portrait != null)
					{
						_portrait.ClearForce();
					}
				}


			}

			return;
		}
		//------------------------------------------------------------------------------------
		private Vector2 _scroll_Timeline = new Vector2();

		public void DrawEditor_Bottom(int width, int height, int layoutX, int layoutY, int windowWidth, int windowHeight)
		{
			if (Editor == null || Editor.Select.Portrait == null)
			{
				return;
			}

			switch (_selectionType)
			{
				case SELECTION_TYPE.Animation:
					{
						DrawEditor_Bottom_Animation(width, height, layoutX, layoutY, windowWidth, windowHeight);
					}
					break;


				case SELECTION_TYPE.MeshGroup:
					{

					}
					break;
			}

			return;
		}


		private bool _isTimelineWheelDrag = false;
		private Vector2 _prevTimelineWheelDragPos = Vector2.zero;
		private Vector2 _scrollPos_BottomAnimationRightProperty = Vector2.zero;

		private void DrawEditor_Bottom_Animation(int width, int height, int layoutX, int layoutY, int windowWidth, int windowHeight)
		{
			//좌우 두개의 탭으로 나뉜다. [타임라인 - 선택된 객체 정보]
			int rightTabWidth = 300;
			int margin = 5;
			int mainTabWidth = width - (rightTabWidth + margin);
			Rect lastRect = GUILayoutUtility.GetLastRect();

			List<apTimelineLayerInfo> timelineInfoList = Editor.TimelineInfoList;
			apTimelineLayerInfo nextSelectLayerInfo = null;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			EditorGUILayout.BeginVertical(GUILayout.Width(mainTabWidth), GUILayout.Height(height));
			//1. [좌측] 타임라인 레이아웃

			//1-1 요약부 : [레코드] + [타임과 통합 키프레임]
			//1-2 메인 타임라인 : [레이어] + [타임라인 메인]
			//1-3 하단 컨트롤과 스크롤 : [컨트롤러] + [스크롤 + 애니메이션 설정]
			int leftTabWidth = 280;
			int timelineWidth = mainTabWidth - (leftTabWidth + 4);

			if (Event.current.type == EventType.Repaint)
			{
				_timlineGUIWidth = timelineWidth;
			}

			int recordAndSummaryHeight = 45;
			int bottomControlHeight = 54;
			int timelineHeight = height - (recordAndSummaryHeight + bottomControlHeight + 4);
			int guiHeight = height - bottomControlHeight;

			//자동 스크롤 이벤트 요청이 들어왔다.
			//처리를 해주자
			if (_isAnimTimelineLayerGUIScrollRequest)
			{
				//_scroll_Timeline.y
				//일단 어느 TimelineInfo인지 찾고,
				//그 값으로 이동
				apTimelineLayerInfo targetInfo = null;
				if (_subAnimTimelineLayer != null)
				{
					targetInfo = timelineInfoList.Find(delegate (apTimelineLayerInfo a)
					{
						return a._layer == _subAnimTimelineLayer && a.IsVisibleLayer;
					});
				}
				else if (_subAnimTimeline != null)
				{
					targetInfo = timelineInfoList.Find(delegate (apTimelineLayerInfo a)
					{
						return a._timeline == _subAnimTimeline && a._isTimeline;
					});
				}

				if (targetInfo != null)
				{
					if (targetInfo._guiLayerPosY - _scroll_Timeline.y < 0 ||
						targetInfo._guiLayerPosY - _scroll_Timeline.y > timelineHeight)
					{
						_scroll_Timeline.y = targetInfo._guiLayerPosY;
					}
				}

				_isAnimTimelineLayerGUIScrollRequest = false;
			}




			//if(Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size1)
			//{
			//	guiHeight = viewAndSummaryHeight;
			//}

			bool isDrawMainTimeline = (Editor._timelineLayoutSize != apEditor.TIMELINE_LAYOUTSIZE.Size1);

			//스크롤 값을 넣어주자
			int startFrame = AnimClip.StartFrame;
			int endFrame = AnimClip.EndFrame;
			int widthPerFrame = Editor.WidthPerFrameInTimeline;
			int nFrames = Mathf.Max((endFrame - startFrame) + 1, 1);
			int widthForTotalFrame = nFrames * widthPerFrame;
			int widthForScrollFrame = widthForTotalFrame;


			//출력할 레이어 개수

			int timelineLayers = Mathf.Max(10, Editor.TimelineInfoList.Count);

			//레이어의 높이
			int heightPerTimeline = 24;
			//int heightPerLayer = 32;
			int heightPerLayer = 28;//조금 작게 만들자

			int heightForScrollLayer = (timelineLayers * heightPerLayer);

			//이벤트가 발생했다면 Repaint하자
			bool isEventOccurred = false;


			//GL에 크기값을 넣어주자
			apTimelineGL.SetLayoutSize(timelineWidth, recordAndSummaryHeight, timelineHeight,
											layoutX + leftTabWidth,
											layoutY, layoutY + recordAndSummaryHeight,
											windowWidth, windowHeight,
											isDrawMainTimeline, _scroll_Timeline);

			//GL에 마우스 값을 넣고 업데이트를 하자

			bool isLeftBtnPressed = false;
			bool isRightBtnPressed = false;
			if (Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseDrag)
			{
				if (Event.current.button == 0)
				{ isLeftBtnPressed = true; }
				else if (Event.current.button == 1)
				{ isRightBtnPressed = true; }
			}

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			apTimelineGL.SetMouseValue(isLeftBtnPressed,
										isRightBtnPressed,
										apMouse.PosNotBound,
										Event.current.shift, isCtrl, Event.current.alt,
										Event.current.rawType,
										this);


			//TODO

			//GUI의 배경 색상
			Color prevColor = GUI.color;
			GUI.color = Editor._guiMainEditorColor;
			Rect timelineRect = new Rect(lastRect.x + leftTabWidth + 4, lastRect.y, timelineWidth, guiHeight + 15);
			GUI.Box(timelineRect, "");


			GUI.color = Editor._guiSubEditorColor;
			Rect timelineBottomRect = new Rect(lastRect.x + leftTabWidth + 4, lastRect.y + guiHeight + 15, timelineWidth, height - (guiHeight));
			GUI.Box(timelineBottomRect, "");

			GUI.color = prevColor;

			//추가 : 하단 GUI도 넣어주자

			bool isWheelDrag = false;
			//마우스 휠 이벤트를 직접 주자
			if (Event.current.rawType == EventType.ScrollWheel)
			{
				//휠 드르륵..
				Vector2 mousePos = Event.current.mousePosition;

				if (mousePos.x > 0 && mousePos.x < lastRect.x + leftTabWidth + timelineWidth &&
					mousePos.y > lastRect.y + recordAndSummaryHeight && mousePos.y < lastRect.y + guiHeight)
				{
					_scroll_Timeline += Event.current.delta * 7;
					Event.current.Use();
					apTimelineGL.SetMouseUse();

					isEventOccurred = true;
				}
			}

			if (Event.current.isMouse && Event.current.type != EventType.Used)
			{
				//휠 클릭 후 드래그
				if (Event.current.button == 2)
				{
					if (Event.current.type == EventType.MouseDown)
					{
						Vector2 mousePos = Event.current.mousePosition;

						if (mousePos.x > leftTabWidth && mousePos.x < lastRect.x + leftTabWidth + timelineWidth &&
							mousePos.y > lastRect.y + recordAndSummaryHeight && mousePos.y < lastRect.y + guiHeight)
						{
							//휠클릭 드래그 시작
							_isTimelineWheelDrag = true;
							_prevTimelineWheelDragPos = mousePos;

							isWheelDrag = true;
							Event.current.Use();
							apTimelineGL.SetMouseUse();

							isEventOccurred = true;
						}
					}
					else if (Event.current.type == EventType.MouseDrag && _isTimelineWheelDrag)
					{
						Vector2 mousePos = Event.current.mousePosition;
						Vector2 deltaPos = mousePos - _prevTimelineWheelDragPos;

						//_scroll_Timeline -= deltaPos * 1.0f;
						_scroll_Timeline.x -= deltaPos.x * 1.0f;//X만 움직이자

						_prevTimelineWheelDragPos = mousePos;
						isWheelDrag = true;
						Event.current.Use();
						apTimelineGL.SetMouseUse();

						isEventOccurred = true;
					}
				}
			}

			if (!isWheelDrag && Event.current.isMouse)
			{
				_isTimelineWheelDrag = false;
			}

			// ┌──┬─────┬──┐
			// │ㅁㅁ│	  v      │ inf│
			// ├──┼─────┤    │
			// │~~~~│  ㅁ  ㅁ  │    │
			// │~~~~│    ㅁ    │    │
			// ├──┼─────┤    │
			// │ >  │Zoom      │    │
			// └──┴─────┴──┘

			//1-1 요약부 : [레코드] + [타임과 통합 키프레임]
			EditorGUILayout.BeginHorizontal(GUILayout.Width(mainTabWidth), GUILayout.Height(recordAndSummaryHeight));

			EditorGUILayout.BeginHorizontal(GUILayout.Width(leftTabWidth), GUILayout.Height(recordAndSummaryHeight));
			GUILayout.Space(5);

			//Texture2D imgAutoKey = null;
			//if (IsAnimAutoKey)	{ imgAutoKey = Editor.ImageSet.Get(apImageSet.PRESET.Anim_KeyOn); }
			//else				{ imgAutoKey = Editor.ImageSet.Get(apImageSet.PRESET.Anim_KeyOff); }

			Texture2D imgKeyLock = null;
			if (IsAnimSelectionLock)
			{ imgKeyLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionLock); }
			else
			{ imgKeyLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionUnlock); }

			Texture2D imgLayerLock = null;
			if (ExAnimEditingMode == EX_EDIT.General_Edit)
			{ imgLayerLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_ModUnlock); }
			else
			{ imgLayerLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_ModLock); }

			Texture2D imgAddKeyframe = Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddKeyframe);

			// 요약부 + 왼쪽의 [레코드] 부분
			//1. Start / Stop Editing (Toggle)
			//2. Auto Key (Toggle)
			//3. Set Key
			//4. Lock (Toggle)로 이루어져 있다.
			GUIStyle btnGUIStyle = new GUIStyle(GUI.skin.button);
			btnGUIStyle.alignment = TextAnchor.MiddleLeft;

			Texture2D editIcon = null;
			string strButtonName = "";
			bool isEditable = false;


			if (ExAnimEditingMode != EX_EDIT.None)
			{
				//현재 애니메이션 수정 작업중이라면..
				editIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Recording);
				strButtonName = " Editing";
				isEditable = true;
			}
			else
			{
				//현재 애니메이션 수정 작업을 하고 있지 않다면..
				if (IsAnimEditable)
				{
					editIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Record);
					strButtonName = " Start Edit";
					isEditable = true;
				}
				else
				{
					editIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_NoRecord);
					strButtonName = " No-Editable";
				}
			}

			// Anim 편집 On/Off
			//Animation Editing On / Off
			if (apEditorUtil.ToggledButton_2Side(editIcon, strButtonName, strButtonName, ExAnimEditingMode != EX_EDIT.None, isEditable, 105, 30, btnGUIStyle))
			{
				//AnimEditing을 On<->Off를 전환하고 기즈모 이벤트를 설정한다.
				SetAnimEditingToggle();
			}

			if (apEditorUtil.ToggledButton_2Side(imgAddKeyframe, "Add Key", "Add Key", false, ExAnimEditingMode != EX_EDIT.None, 85, 30))
			{
				//Debug.LogError("TODO : Set Key");
				if (AnimTimelineLayer != null)
				{
					Editor.Controller.AddAnimKeyframe(AnimClip.CurFrame, AnimTimelineLayer, true);
				}
			}

			if (apEditorUtil.ToggledButton_2Side(imgKeyLock, IsAnimSelectionLock, ExAnimEditingMode != EX_EDIT.None, 35, 30))
			{
				_isAnimLock = !_isAnimLock;

				Editor.RefreshTimelineLayers(false);
			}

			//단축키 [Space]에 의해서 Seletion Lock을 켜고 끌 수 있다.
			Editor.AddHotKeyEvent(OnHotKey_AnimEditingLockToggle, "Toggle Selection Lock", KeyCode.Space, false, false, false);

			if (apEditorUtil.ToggledButton_2Side(imgLayerLock, ExAnimEditingMode == EX_EDIT.ExOnly_Edit, ExAnimEditingMode != EX_EDIT.None, 35, 30))
			{
				SetAnimEditingLayerLockToggle();//Mod Layer Lock을 토글
			}

			//if(GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoZoom), GUILayout.Width(30), GUILayout.Height(30)))

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical(GUILayout.Width(timelineWidth), GUILayout.Height(recordAndSummaryHeight));

			// 요약부 + 오른쪽의 [시간 / 통합 키 프레임]
			// 이건 GUI로 해야한다.
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();



			//1-2 메인 타임라인 : [레이어] + [타임라인 메인]
			EditorGUILayout.BeginHorizontal(GUILayout.Width(mainTabWidth), GUILayout.Height(timelineHeight));

			EditorGUILayout.BeginVertical(GUILayout.Width(leftTabWidth), GUILayout.Height(timelineHeight));
			GUILayout.BeginArea(new Rect(lastRect.x, lastRect.y + recordAndSummaryHeight, leftTabWidth, timelineHeight));
			// 메인 + 왼쪽의 [레이어] 부분

			// 레이어에 대한 렌더링 (정보 부분)
			//--------------------------------------------------------------
			int nTimelines = AnimClip._timelines.Count;
			apAnimTimeline curTimeline = null;
			int curLayerY = 0;

			//GUIStyle guiStyle_layerInfoBox = new GUIStyle(GUI.skin.label);
			//GUIStyle guiStyle_layerInfoBox = new GUIStyle(GUI.skin.box);
			GUIStyle guiStyle_layerInfoBox = new GUIStyle(GUI.skin.label);
			guiStyle_layerInfoBox.alignment = TextAnchor.MiddleLeft;
			guiStyle_layerInfoBox.padding = GUI.skin.button.padding;

			int baseLeftPadding = GUI.skin.button.padding.left;

			int btnWidth_Layer = leftTabWidth + 4;
			Texture2D img_HideLayer = Editor.ImageSet.Get(apImageSet.PRESET.Anim_HideLayer);
			Texture2D img_TimelineFolded = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight);
			Texture2D img_TimelineNotFolded = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

			Texture2D img_CurFold = null;

			GUIStyle guiStyle_LeftBtn = new GUIStyle(GUI.skin.button);
			guiStyle_LeftBtn.padding = new RectOffset(0, 0, 0, 0);

			for (int iLayer = 0; iLayer < timelineInfoList.Count; iLayer++)
			{
				apTimelineLayerInfo info = timelineInfoList[iLayer];

				if (!info._isTimeline && !info.IsVisibleLayer)
				{
					//숨겨진 레이어이다.
					info._guiLayerPosY = 0.0f;
					continue;
				}
				int layerHeight = heightPerLayer;
				int leftPadding = baseLeftPadding + 20;
				if (info._isTimeline)
				{
					layerHeight = heightPerTimeline;
					leftPadding = baseLeftPadding;
				}

				//배경 / 텍스트 색상을 정하자
				Color layerBGColor = info.GUIColor;
				Color textColor = Color.black;

				info._guiLayerPosY = curLayerY;

				if (!info._isAvailable)
				{
					textColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else
				{
					float grayScale = (layerBGColor.r + layerBGColor.g + layerBGColor.b) / 3.0f;
					if (grayScale < 0.3f)
					{
						textColor = Color.white;
					}
				}

				//아이콘을 결정하자

				guiStyle_layerInfoBox.normal.textColor = textColor;
				guiStyle_layerInfoBox.padding.left = leftPadding;


				Texture2D layerIcon = Editor.ImageSet.Get(info.IconImgType);




				//[ 레이어 선택 ]

				if (info._isTimeline)
				{
					GUI.backgroundColor = layerBGColor;
					GUI.Box(new Rect(0, curLayerY - _scroll_Timeline.y, btnWidth_Layer, layerHeight), "");

					int yOffset = (layerHeight - 18) / 2;

					if (info.IsTimelineFolded)
					{
						img_CurFold = img_TimelineFolded;
					}
					else
					{
						img_CurFold = img_TimelineNotFolded;
					}
					if (GUI.Button(new Rect(2, (curLayerY + yOffset) - _scroll_Timeline.y, 18, 18), img_CurFold, guiStyle_LeftBtn))
					{
						if (info._timeline != null)
						{
							info._timeline._guiTimelineFolded = !info._timeline._guiTimelineFolded;
						}
					}

					GUI.backgroundColor = prevColor;

					if (GUI.Button(new Rect(19, curLayerY - _scroll_Timeline.y, btnWidth_Layer, layerHeight),
					new GUIContent("  " + info.DisplayName, layerIcon), guiStyle_layerInfoBox))
					{
						nextSelectLayerInfo = info;//<<선택!
					}
				}
				else
				{
					//[ Hide 버튼]
					//int xOffset = (btnWidth_Layer - (layerHeight + 4)) + 2;
					//int xOffset = 18;
					int yOffset = (layerHeight - 18) / 2;

					GUI.backgroundColor = layerBGColor;
					GUI.Box(new Rect(0, curLayerY - _scroll_Timeline.y, btnWidth_Layer, layerHeight), "");

					if (GUI.Button(new Rect(2, (curLayerY + yOffset) - _scroll_Timeline.y, 18, 18), "-", guiStyle_LeftBtn))
					{
						//Hide
						info._layer._guiLayerVisible = false;//<<숨기자!
					}

					GUI.backgroundColor = prevColor;


					//2 + 18 + 2 = 22
					if (GUI.Button(new Rect(19, curLayerY - _scroll_Timeline.y, btnWidth_Layer - 22, layerHeight),
					new GUIContent("  " + info.DisplayName, layerIcon), guiStyle_layerInfoBox))
					{
						nextSelectLayerInfo = info;//<<선택!
					}
				}





				//GUI.backgroundColor = prevColor;

				curLayerY += layerHeight;
			}




			//--------------------------------------------------------------

			GUILayout.EndArea();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(timelineWidth), GUILayout.Height(timelineHeight));
			GUILayout.BeginArea(timelineRect);
			// 메인 + 오른쪽의 [메인 타임라인]
			// 이건 GUI로 해야한다.

			//기본 타임라인 GL 세팅
			apTimelineGL.SetTimelineSetting(0, AnimClip.StartFrame, AnimClip.EndFrame, Editor.WidthPerFrameInTimeline, AnimClip.IsLoop);
			//apTimelineGL.DrawTimeBars_Header(new Color(0.4f, 0.4f, 0.4f, 1.0f));

			// 레이어에 대한 렌더링 (타임라인 부분 - BG)
			//--------------------------------------------------------------
			curLayerY = 0;
			for (int iLayer = 0; iLayer < timelineInfoList.Count; iLayer++)
			{
				apTimelineLayerInfo info = timelineInfoList[iLayer];
				int layerHeight = heightPerLayer;

				if (!info._isTimeline && !info.IsVisibleLayer)
				{
					continue;
				}
				if (info._isTimeline)
				{
					layerHeight = heightPerTimeline;
				}
				if (info._isSelected)
				{
					apTimelineGL.DrawTimeBars_MainBG(info.TimelineColor, curLayerY + layerHeight - (int)_scroll_Timeline.y, layerHeight);
				}
				curLayerY += layerHeight;
			}

			//Grid를 그린다.
			apTimelineGL.DrawTimelineAreaBG(ExAnimEditingMode != EX_EDIT.None);
			apTimelineGL.DrawTimeGrid(new Color(0.4f, 0.4f, 0.4f, 1.0f), new Color(0.3f, 0.3f, 0.3f, 1.0f), new Color(0.7f, 0.7f, 0.7f, 1.0f));
			apTimelineGL.DrawTimeBars_Header(new Color(0.4f, 0.4f, 0.4f, 1.0f));



			// 레이어에 대한 렌더링 (타임라인 부분 - Line + Frames)
			//--------------------------------------------------------------
			curLayerY = 0;
			bool isAnyHidedLayer = false;
			apTimelineGL.BeginKeyframeControl();

			for (int iLayer = 0; iLayer < timelineInfoList.Count; iLayer++)
			{
				apTimelineLayerInfo info = timelineInfoList[iLayer];
				if (!info._isTimeline && !info.IsVisibleLayer)
				{
					//숨겨진 레이어
					isAnyHidedLayer = true;
					continue;
				}
				int layerHeight = heightPerLayer;
				if (info._isTimeline)
				{
					layerHeight = heightPerTimeline;
				}
				apTimelineGL.DrawTimeBars_MainLine(new Color(0.3f, 0.3f, 0.3f, 1.0f), curLayerY + layerHeight - (int)_scroll_Timeline.y);

				if (!info._isTimeline)
				{
					Color curveEditColor = Color.black;
					if (_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Curve)
					{
						apAnimCurveResult curveResult = null;
						if (AnimKeyframe != null)
						{
							if (_animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
							{
								curveResult = AnimKeyframe._curveKey._prevCurveResult;
							}
							else
							{
								curveResult = AnimKeyframe._curveKey._nextCurveResult;
							}


						}
					}
					apTimelineGL.DrawKeyframes(info._layer,
												curLayerY + layerHeight / 2,
												info.GUIColor,
												info._isAvailable,
												layerHeight,
												(AnimTimelineLayer == info._layer),
												AnimClip.CurFrame,
												_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Curve,
												_animPropertyCurveUI
												//curveEditColor
												);
				}

				curLayerY += layerHeight;


			}
			bool isKeyframeEvent = apTimelineGL.EndKeyframeControl();//<<제어용 함수
			if (isKeyframeEvent)
			{ isEventOccurred = true; }


			// Play Bar를 그린다.
			//int prevClipFrame = AnimClip.CurFrame;
			//bool isAutoRefresh = false;
			bool isChangeFrame = apTimelineGL.DrawPlayBar(AnimClip.CurFrame);
			if (isChangeFrame)
			{
				AutoSelectAnimWorkKeyframe();
				//isAutoRefresh = true;
			}
			//if(prevClipFrame != AnimClip.CurFrame)
			//{
			//	Debug.Log("Frame Changed [" + isAutoRefresh + "] : " + (AnimWorkKeyframe != null));
			//}

			apTimelineGL.DrawAndUpdateSelectArea();


			//--------------------------------------------------------------

			GUILayout.EndArea();
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();


			//TODO : 스크롤은 현재 키프레임의 범위, 레이어의 개수에 따라 바뀐다.
			_scroll_Timeline.y = GUI.VerticalScrollbar(new Rect(lastRect.x + leftTabWidth + 4 + timelineWidth - 15, lastRect.y, 15, timelineHeight + recordAndSummaryHeight + 4), _scroll_Timeline.y, 20.0f, 0.0f, heightForScrollLayer);


			//Anim 레이어를 선택하자
			if (nextSelectLayerInfo != null)
			{
				if (nextSelectLayerInfo._isTimeline)
				{
					//Timeline을 선택하기 전에
					//Anim객체를 초기화한다. (안그러면 자동으로 선택된 오브젝트에 의해서 TimelineLayer를 선택하게 된다.)
					SetBoneForAnimClip(null);
					SetSubControlParamForAnimClipEdit(null);
					SetSubMeshTransformForAnimClipEdit(null);
					SetSubMeshGroupTransformForAnimClipEdit(null);

					SetAnimTimeline(nextSelectLayerInfo._timeline, true, true);
					SetAnimTimelineLayer(null, true, true, true);
					SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
				}
				else
				{
					SetAnimTimeline(nextSelectLayerInfo._parentTimeline, true, true);
					SetAnimTimelineLayer(nextSelectLayerInfo._layer, true, true, true);
					SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
				}
				AutoSelectAnimWorkKeyframe();

				Editor.RefreshControllerAndHierarchy();
			}

			_scroll_Timeline.x = GUI.HorizontalScrollbar(new Rect(lastRect.x + leftTabWidth + 4, lastRect.y + recordAndSummaryHeight + timelineHeight + 4, timelineWidth - 15, 15),
															_scroll_Timeline.x,
															20.0f, 0.0f,
															widthForScrollFrame);

			if (GUI.Button(new Rect(lastRect.x + leftTabWidth + 4 + timelineWidth - 15, lastRect.y + recordAndSummaryHeight + timelineHeight + 4, 15, 15), ""))
			{
				_scroll_Timeline.x = 0;
				_scroll_Timeline.y = 0;
			}

			//1-3 하단 컨트롤과 스크롤 : [컨트롤러] + [스크롤 + 애니메이션 설정]
			int ctrlBtnSize_Small = 30;
			int ctrlBtnSize_Large = 30;
			int ctrlBtnSize_LargeUnder = bottomControlHeight - (ctrlBtnSize_Large + 6);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(mainTabWidth), GUILayout.Height(bottomControlHeight));
			EditorGUILayout.BeginVertical(GUILayout.Width(leftTabWidth), GUILayout.Height(bottomControlHeight));
			EditorGUILayout.BeginHorizontal(GUILayout.Width(leftTabWidth), GUILayout.Height(ctrlBtnSize_Large + 2));
			GUILayout.Space(5);


			//플레이 제어
			if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_FirstFrame), GUILayout.Width(ctrlBtnSize_Large), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 첫 프레임으로 이동
				AnimClip.SetFrame_Editor(AnimClip.StartFrame);
				AutoSelectAnimWorkKeyframe();
			}

			if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_PrevFrame), GUILayout.Width(ctrlBtnSize_Large + 10), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 이전 프레임으로 이동
				int prevFrame = AnimClip.CurFrame - 1;
				if (prevFrame < AnimClip.StartFrame)
				{
					if (AnimClip.IsLoop)
					{
						prevFrame = AnimClip.EndFrame;
					}
				}
				AnimClip.SetFrame_Editor(prevFrame);
				AutoSelectAnimWorkKeyframe();
			}

			Texture2D playIcon = null;
			if (AnimClip.IsPlaying)
			{
				//플레이중 -> Pause 버튼
				playIcon = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Pause);
			}
			else
			{
				//일시 정지 -> 플레이 버튼
				playIcon = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Play);
			}

			if (GUILayout.Button(playIcon, GUILayout.Width(ctrlBtnSize_Large + 30), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 플레이 / 일시정지
				if (AnimClip.IsPlaying)
				{
					// 플레이 -> 일시 정지
					AnimClip.Pause_Editor();
				}
				else
				{
					//마지막 프레임이라면 첫 프레임으로 이동하여 재생한다.
					if (AnimClip.CurFrame == AnimClip.EndFrame)
					{
						AnimClip.SetFrame_Editor(AnimClip.StartFrame);
					}
					// 일시 정지 -> 플레이
					AnimClip.Play_Editor();
				}

				//Play 전환 여부에 따라서도 WorkKeyframe을 전환한다.
				AutoSelectAnimWorkKeyframe();
				Editor.SetRepaint();
				Editor.Gizmos.SetUpdate();

			}

			if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_NextFrame), GUILayout.Width(ctrlBtnSize_Large + 10), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 다음 프레임으로 이동
				int nextFrame = AnimClip.CurFrame + 1;
				if (nextFrame > AnimClip.EndFrame)
				{
					if (AnimClip.IsLoop)
					{
						nextFrame = AnimClip.StartFrame;
					}
				}
				AnimClip.SetFrame_Editor(nextFrame);
				AutoSelectAnimWorkKeyframe();
			}

			if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_LastFrame), GUILayout.Width(ctrlBtnSize_Large), GUILayout.Height(ctrlBtnSize_Large)))
			{
				//제어 : 마지막 프레임으로 이동
				AnimClip.SetFrame_Editor(AnimClip.EndFrame);
				AutoSelectAnimWorkKeyframe();
			}

			GUILayout.Space(10);
			bool isLoopPlay = AnimClip.IsLoop;
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_Loop), isLoopPlay, true, ctrlBtnSize_Large, ctrlBtnSize_Large))
			{
				//AnimClip._isLoop = !AnimClip._isLoop;
				AnimClip.SetOption_IsLoop(!AnimClip.IsLoop);
				AnimClip.SetFrame_Editor(AnimClip.CurFrame);
				Editor.RefreshTimelineLayers(false);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(leftTabWidth), GUILayout.Height(ctrlBtnSize_LargeUnder + 2));
			GUILayout.Space(5);
			//현재 프레임 + 세밀 조정
			EditorGUILayout.LabelField("Frame", GUILayout.Width(40), GUILayout.Height(ctrlBtnSize_LargeUnder));
			int curFrame = AnimClip.CurFrame;
			int nextCurFrame = EditorGUILayout.IntSlider(curFrame, AnimClip.StartFrame, AnimClip.EndFrame, GUILayout.Width(leftTabWidth - 55), GUILayout.Height(ctrlBtnSize_LargeUnder));
			if (nextCurFrame != curFrame)
			{
				AnimClip.SetFrame_Editor(nextCurFrame);
				AutoSelectAnimWorkKeyframe();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(mainTabWidth - leftTabWidth), GUILayout.Height(bottomControlHeight));
			GUILayout.Space(18);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(mainTabWidth - leftTabWidth), GUILayout.Height(bottomControlHeight - 18));

			//맨 하단은 키 복붙이나 View, 영역 등에 관련된 정보를 출력한다.
			GUILayout.Space(10);

			//Timeline 정렬
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_SortRegOrder), Editor._timelineInfoSortType == apEditor.TIMELINE_INFO_SORT.Registered, true, ctrlBtnSize_Small, ctrlBtnSize_Small))
			{
				Editor._timelineInfoSortType = apEditor.TIMELINE_INFO_SORT.Registered;
				Editor.SaveEditorPref();
				Editor.RefreshTimelineLayers(true);
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_SortABC), Editor._timelineInfoSortType == apEditor.TIMELINE_INFO_SORT.ABC, true, ctrlBtnSize_Small, ctrlBtnSize_Small))
			{
				Editor._timelineInfoSortType = apEditor.TIMELINE_INFO_SORT.ABC;
				Editor.SaveEditorPref();
				Editor.RefreshTimelineLayers(true);
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_SortDepth), Editor._timelineInfoSortType == apEditor.TIMELINE_INFO_SORT.Depth, true, ctrlBtnSize_Small, ctrlBtnSize_Small))
			{
				Editor._timelineInfoSortType = apEditor.TIMELINE_INFO_SORT.Depth;
				Editor.SaveEditorPref();
				Editor.RefreshTimelineLayers(true);

			}

			GUILayout.Space(20);

			if (apEditorUtil.ToggledButton("Unhide Layers", !isAnyHidedLayer, 120, ctrlBtnSize_Small))
			{
				Editor.ShowAllTimelineLayers();
			}

			GUILayout.Space(20);

			// 타임라인 사이즈 (1, 2, 3)
			apEditor.TIMELINE_LAYOUTSIZE nextLayoutSize = Editor._timelineLayoutSize;

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_TimelineSize1), Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size1, true, ctrlBtnSize_Small, ctrlBtnSize_Small))
			{
				nextLayoutSize = apEditor.TIMELINE_LAYOUTSIZE.Size1;
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_TimelineSize2), Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size2, true, ctrlBtnSize_Small, ctrlBtnSize_Small))
			{
				nextLayoutSize = apEditor.TIMELINE_LAYOUTSIZE.Size2;
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_TimelineSize3), Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size3, true, ctrlBtnSize_Small, ctrlBtnSize_Small))
			{
				nextLayoutSize = apEditor.TIMELINE_LAYOUTSIZE.Size3;
			}


			//Zoom
			GUILayout.Space(4);

			EditorGUILayout.BeginVertical(GUILayout.Width(90));
			//EditorGUILayout.LabelField("Zoom", GUILayout.Width(100), GUILayout.Height(15));
			GUILayout.Space(7);
			int timelineLayoutSize_Min = 0;
			int timelineLayoutSize_Max = Editor._timelineZoomWPFPreset.Length - 1;

			int nextTimelineIndex = (int)(GUILayout.HorizontalSlider(Editor._timelineZoom_Index, timelineLayoutSize_Min, timelineLayoutSize_Max, GUILayout.Width(90), GUILayout.Height(20)) + 0.5f);
			if (nextTimelineIndex != Editor._timelineZoom_Index)
			{
				if (nextTimelineIndex < timelineLayoutSize_Min)
				{ nextTimelineIndex = timelineLayoutSize_Min; }
				else if (nextTimelineIndex > timelineLayoutSize_Max)
				{ nextTimelineIndex = timelineLayoutSize_Max; }

				Editor._timelineZoom_Index = nextTimelineIndex;
			}
			EditorGUILayout.EndVertical();


			if (GUILayout.Button(new GUIContent(" Fit", Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoZoom)),
									GUILayout.Width(65), GUILayout.Height(ctrlBtnSize_Small)))
			{
				//Debug.LogError("TODO : Timeline AutoZoom");
				//Width / 전체 Frame수 = 목표 WidthPerFrame
				int numFrames = Mathf.Max(AnimClip.EndFrame - AnimClip.StartFrame, 1);
				int targetWidthPerFrame = (int)((float)timelineWidth / (float)numFrames + 0.5f);
				_scroll_Timeline.x = 0;
				//적절한 값을 찾자
				int optWPFIndex = -1;
				for (int i = 0; i < Editor._timelineZoomWPFPreset.Length; i++)
				{
					int curWPF = Editor._timelineZoomWPFPreset[i];
					if (curWPF < targetWidthPerFrame)
					{
						optWPFIndex = i;
						break;
					}
				}
				if (optWPFIndex < 0)
				{
					Editor._timelineZoom_Index = Editor._timelineZoomWPFPreset.Length - 1;
				}
				else
				{
					Editor._timelineZoom_Index = optWPFIndex;
				}
			}

			GUILayout.Space(4);

			//Auto Scroll
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoScroll),
													" Auto Scroll", " Auto Scroll",
													Editor._isAnimAutoScroll, true,
													110, ctrlBtnSize_Small))
			{
				Editor._isAnimAutoScroll = !Editor._isAnimAutoScroll;
			}



			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();


			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(rightTabWidth), GUILayout.Height(height));
			//2. [우측] 선택된 레이어/키 정보

			_scrollPos_BottomAnimationRightProperty = EditorGUILayout.BeginScrollView(_scrollPos_BottomAnimationRightProperty, false, true, GUILayout.Width(rightTabWidth), GUILayout.Height(height));

			int rightPropertyWidth = rightTabWidth - 24;

			EditorGUILayout.BeginVertical(GUILayout.Width(rightPropertyWidth));


			//프로퍼티 타이틀
			//프로퍼티는 (KeyFrame -> Layer -> Timeline -> None) 순으로 정보를 보여준다.
			string propertyTitle = "";
			int propertyType = 0;
			if (AnimKeyframe != null)
			{
				if (IsAnimKeyframeMultipleSelected)
				{
					propertyTitle = "Keyframes [ " + AnimKeyframes.Count + " Selected ]";
					propertyType = 1;
				}
				else
				{
					propertyTitle = "Keyframe [ " + AnimKeyframe._frameIndex + " ]";
					propertyType = 2;
				}

			}
			else if (AnimTimelineLayer != null)
			{
				propertyTitle = "Layer [" + AnimTimelineLayer.DisplayName + " ]";
				propertyType = 3;
			}
			else if (AnimTimeline != null)
			{
				propertyTitle = "Timeline [ " + AnimTimeline.DisplayName + " ]";
				propertyType = 4;
			}
			else
			{
				propertyTitle = "Not Selected";
			}

			GUIStyle guiStyleProperty = new GUIStyle(GUI.skin.box);
			guiStyleProperty.normal.textColor = Color.white;
			guiStyleProperty.alignment = TextAnchor.MiddleCenter;

			GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);

			GUILayout.Box(propertyTitle, guiStyleProperty, GUILayout.Width(rightPropertyWidth), GUILayout.Height(20));
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);


			Editor.SetGUIVisible("Animation Bottom Property - MK", propertyType == 1);
			Editor.SetGUIVisible("Animation Bottom Property - SK", propertyType == 2);
			Editor.SetGUIVisible("Animation Bottom Property - L", propertyType == 3);
			Editor.SetGUIVisible("Animation Bottom Property - T", propertyType == 4);

			switch (propertyType)
			{
				case 1:

					if (Editor.IsDelayedGUIVisible("Animation Bottom Property - MK"))
					{
						DrawEditor_Bottom_AnimationProperty_MultipleKeyframes(AnimKeyframes, rightPropertyWidth);
					}
					break;

				case 2:
					if (Editor.IsDelayedGUIVisible("Animation Bottom Property - SK"))
					{
						DrawEditor_Bottom_AnimationProperty_SingleKeyframe(
							AnimKeyframe,
							rightPropertyWidth,
							windowWidth,
							windowHeight,
							(layoutX + leftTabWidth + margin + mainTabWidth + margin),
							//layoutX + margin + mainTabWidth + margin, 
							//leftTabWidth + margin + mainTabWidth + margin, 
							(int)(layoutY),
							(int)(_scrollPos_BottomAnimationRightProperty.y)
							//(int)(layoutY)
							);
					}
					break;

				case 3:
					if (Editor.IsDelayedGUIVisible("Animation Bottom Property - L"))
					{
						DrawEditor_Bottom_AnimationProperty_TimelineLayer(AnimTimelineLayer, rightPropertyWidth);
					}
					break;

				case 4:
					if (Editor.IsDelayedGUIVisible("Animation Bottom Property - T"))
					{
						DrawEditor_Bottom_AnimationProperty_Timeline(AnimTimeline, rightPropertyWidth);
					}
					break;
			}



			GUILayout.Space(height);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();


			if (Editor._timelineLayoutSize != nextLayoutSize)
			{
				Editor._timelineLayoutSize = nextLayoutSize;
			}

			if (isEventOccurred)
			{
				Editor.SetRepaint();
			}
		}

		private apAnimKeyframe _tmpPrevSelectedAnimKeyframe = null;

		//화면 우측의 UI 중 : 키프레임을 "1개 선택할 때" 출력되는 UI
		private void DrawEditor_Bottom_AnimationProperty_SingleKeyframe(apAnimKeyframe keyframe, int width, int windowWidth, int windowHeight, int layoutX, int layoutY, int scrollValue)
		{
			//TODO : 커브 조절


			//프레임 이동
			//EditorGUILayout.LabelField("Frame [" + keyframe._frameIndex + "]", GUILayout.Width(width));
			//GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));

			GUILayout.Space(5);

			Texture2D imgPrev = Editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToPrevFrame);
			Texture2D imgNext = Editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToNextFrame);
			Texture2D imgCurKey = Editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToCurrentFrame);

			int btnWidthSide = ((width - (10 + 80)) / 2) - 4;
			int btnWidthCenter = 90;
			bool isPrevKey = false;
			bool isNextKey = false;
			bool isCurKey = (AnimClip.CurFrame == keyframe._frameIndex);
			if (keyframe._prevLinkedKeyframe != null)
			{
				isPrevKey = true;
			}
			if (keyframe._nextLinkedKeyframe != null)
			{
				isNextKey = true;
			}

			if (apEditorUtil.ToggledButton_2Side(imgPrev, false, isPrevKey, btnWidthSide, 25))
			{
				//연결된 이전 프레임으로 이동한다.
				if (isPrevKey)
				{
					AnimClip.SetFrame_Editor(keyframe._prevLinkedKeyframe._frameIndex);
					SetAnimKeyframe(keyframe._prevLinkedKeyframe, true, apGizmos.SELECT_TYPE.New);
				}
			}
			if (apEditorUtil.ToggledButton_2Side(imgCurKey, isCurKey, true, btnWidthCenter, 25))
			{
				//현재 프레임으로 이동한다.
				AnimClip.SetFrame_Editor(keyframe._frameIndex);
				AutoSelectAnimWorkKeyframe();
				SetAutoAnimScroll();
			}
			if (apEditorUtil.ToggledButton_2Side(imgNext, false, isNextKey, btnWidthSide, 25))
			{
				//연결된 다음 프레임으로 이동한다.
				if (isNextKey)
				{
					AnimClip.SetFrame_Editor(keyframe._nextLinkedKeyframe._frameIndex);
					SetAnimKeyframe(keyframe._nextLinkedKeyframe, true, apGizmos.SELECT_TYPE.New);
				}
			}


			EditorGUILayout.EndHorizontal();

			//Value / Curve에 따라서 다른 UI가 나온다.
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(22));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton("Transform",
											(_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Value),
											(width / 2) - 2
										))
			{
				_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Value;
			}
			if (apEditorUtil.ToggledButton("Curve",
											(_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Curve),
											(width / 2) - 2
										))
			{
				_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Curve;
			}

			EditorGUILayout.EndHorizontal();


			//키프레임 타입인 경우
			bool isControlParamUI = (AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam &&
									AnimTimelineLayer._linkedControlParam != null);
			bool isModifierUI = (AnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
									AnimTimeline._linkedModifier != null);

			Editor.SetGUIVisible("Bottom Right Anim Property - ControlParamUI", isControlParamUI);
			Editor.SetGUIVisible("Bottom Right Anim Property - ModifierUI", isModifierUI);

			bool isDrawControlParamUI = Editor.IsDelayedGUIVisible("Bottom Right Anim Property - ControlParamUI");
			bool isDrawModifierUI = Editor.IsDelayedGUIVisible("Bottom Right Anim Property - ModifierUI");


			apControlParam controlParam = AnimTimelineLayer._linkedControlParam;

			Editor.SetGUIVisible("Anim Property - SameKeyframe", _tmpPrevSelectedAnimKeyframe == keyframe);
			bool isSameKP = Editor.IsDelayedGUIVisible("Anim Property - SameKeyframe");

			if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
			{
				_tmpPrevSelectedAnimKeyframe = keyframe;
			}

			Color prevColor = GUI.color;


			if (_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Value)
			{
				//1. Value Mode
				if (isDrawControlParamUI && isSameKP)
				{
					#region Control Param UI 그리는 코드
					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(width);
					GUILayout.Space(10);

					GUI.color = new Color(0.4f, 1.0f, 0.5f, 1.0f);

					GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
					guiStyleBox.alignment = TextAnchor.MiddleCenter;

					GUILayout.Box("Control Parameter Value",
									guiStyleBox,
									GUILayout.Width(width), GUILayout.Height(30));

					GUI.color = prevColor;


					GUIStyle guiStyle_LableMin = new GUIStyle(GUI.skin.label);
					guiStyle_LableMin.alignment = TextAnchor.MiddleLeft;

					GUIStyle guiStyle_LableMax = new GUIStyle(GUI.skin.label);
					guiStyle_LableMax.alignment = TextAnchor.MiddleRight;
					int widthLabelRange = (width / 2) - 2;

					GUILayout.Space(5);

					bool isChanged = false;

					switch (controlParam._valueType)
					{
						case apControlParam.TYPE.Int:
							{
								int iNext = keyframe._conSyncValue_Int;

								EditorGUILayout.LabelField(controlParam._keyName, GUILayout.Width(width));
								EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
								EditorGUILayout.LabelField(controlParam._label_Min, guiStyle_LableMin, GUILayout.Width(widthLabelRange));
								EditorGUILayout.LabelField(controlParam._label_Max, guiStyle_LableMax, GUILayout.Width(widthLabelRange));
								EditorGUILayout.EndHorizontal();
								iNext = EditorGUILayout.IntSlider(keyframe._conSyncValue_Int, controlParam._int_Min, controlParam._int_Max, GUILayout.Width(width));


								if (iNext != keyframe._conSyncValue_Int)
								{
									apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor._portrait, keyframe, true, Editor);

									keyframe._conSyncValue_Int = iNext;
									isChanged = true;
								}
							}
							break;

						case apControlParam.TYPE.Float:
							{
								float fNext = keyframe._conSyncValue_Float;

								EditorGUILayout.LabelField(controlParam._keyName, GUILayout.Width(width));
								EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
								EditorGUILayout.LabelField(controlParam._label_Min, guiStyle_LableMin, GUILayout.Width(widthLabelRange));
								EditorGUILayout.LabelField(controlParam._label_Max, guiStyle_LableMax, GUILayout.Width(widthLabelRange));
								EditorGUILayout.EndHorizontal();
								fNext = EditorGUILayout.Slider(keyframe._conSyncValue_Float, controlParam._float_Min, controlParam._float_Max, GUILayout.Width(width));

								if (fNext != keyframe._conSyncValue_Float)
								{
									apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor._portrait, keyframe, true, Editor);

									keyframe._conSyncValue_Float = fNext;
									isChanged = true;
								}
							}
							break;

						case apControlParam.TYPE.Vector2:
							{
								Vector2 v2Next = keyframe._conSyncValue_Vector2;
								EditorGUILayout.LabelField(controlParam._keyName, GUILayout.Width(width));

								EditorGUILayout.LabelField(controlParam._label_Min, GUILayout.Width(width));
								v2Next.x = EditorGUILayout.Slider(keyframe._conSyncValue_Vector2.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x, GUILayout.Width(width));

								EditorGUILayout.LabelField(controlParam._label_Max, GUILayout.Width(width));
								v2Next.y = EditorGUILayout.Slider(keyframe._conSyncValue_Vector2.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y, GUILayout.Width(width));

								if (v2Next.x != keyframe._conSyncValue_Vector2.x ||
									v2Next.y != keyframe._conSyncValue_Vector2.y)
								{
									apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor._portrait, keyframe, true, Editor);

									keyframe._conSyncValue_Vector2 = v2Next;
									isChanged = true;
								}
							}
							break;

					}

					if (isChanged)
					{
						AnimClip.UpdateControlParam(true);
					}
					#endregion
				}

				if (isDrawModifierUI && isSameKP)
				{
					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(width);
					GUILayout.Space(10);

					GUI.color = new Color(0.4f, 1.0f, 0.5f, 1.0f);

					GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
					guiStyleBox.alignment = TextAnchor.MiddleCenter;


					apModifierBase linkedModifier = AnimTimeline._linkedModifier;


					string boxText = "";
					bool isMod_Morph = ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0);
					bool isMod_TF = ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix) != 0);
					bool isMod_Color = ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0);

					if (isMod_Morph)
					{
						boxText = "Morph Modifier Value";
					}
					else
					{
						boxText = "Transform Modifier Value";
					}

					GUILayout.Box(boxText,
									guiStyleBox,
									GUILayout.Width(width), GUILayout.Height(30));

					GUI.color = prevColor;

					apModifierParamSet paramSet = keyframe._linkedParamSet_Editor;
					apModifiedMesh modMesh = keyframe._linkedModMesh_Editor;
					apModifiedBone modBone = keyframe._linkedModBone_Editor;
					if (modMesh == null)
					{
						isMod_Morph = false;
						isMod_Color = false;
					}
					if (modBone == null && modMesh == null)
					{
						//TF 타입은 Bone 타입이 적용될 수 있다.
						isMod_TF = false;
					}
					//TODO : 여기서부터 작성하자

					bool isChanged = false;

					if (isMod_Morph)
					{
						GUILayout.Space(5);
					}

					if (isMod_TF)
					{
						GUILayout.Space(5);

						Texture2D img_Pos = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Move);
						Texture2D img_Rot = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Rotate);
						Texture2D img_Scale = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Scale);

						Vector2 nextPos = Vector2.zero;
						float nextAngle = 0.0f;
						Vector2 nextScale = Vector2.one;

						if (modMesh != null)
						{
							nextPos = modMesh._transformMatrix._pos;
							nextAngle = modMesh._transformMatrix._angleDeg;
							nextScale = modMesh._transformMatrix._scale;
						}
						else if (modBone != null)
						{
							nextPos = modBone._transformMatrix._pos;
							nextAngle = modBone._transformMatrix._angleDeg;
							nextScale = modBone._transformMatrix._scale;
						}

						int iconSize = 30;
						int propertyWidth = width - (iconSize + 8);

						//Position
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
						EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
						EditorGUILayout.LabelField(new GUIContent(img_Pos), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
						EditorGUILayout.LabelField("Position");
						//nextPos = EditorGUILayout.Vector2Field("", nextPos, GUILayout.Width(propertyWidth));
						nextPos = apEditorUtil.DelayedVector2Field(nextPos, propertyWidth);
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();

						//Rotation
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
						EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
						EditorGUILayout.LabelField(new GUIContent(img_Rot), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
						EditorGUILayout.LabelField("Rotation");

						nextAngle = EditorGUILayout.DelayedFloatField(nextAngle, GUILayout.Width(propertyWidth));
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();

						//Scaling
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
						EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
						EditorGUILayout.LabelField(new GUIContent(img_Scale), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
						EditorGUILayout.LabelField("Scaling");

						//nextScale = EditorGUILayout.Vector2Field("", nextScale, GUILayout.Width(propertyWidth));
						nextScale = apEditorUtil.DelayedVector2Field(nextScale, propertyWidth);
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();

						if (modMesh != null)
						{
							if (nextPos.x != modMesh._transformMatrix._pos.x ||
								nextPos.y != modMesh._transformMatrix._pos.y ||
								nextAngle != modMesh._transformMatrix._angleDeg ||
								nextScale.x != modMesh._transformMatrix._scale.x ||
								nextScale.y != modMesh._transformMatrix._scale.y)
							{
								isChanged = true;

								apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, _portrait, _animClip._targetMeshGroup, false, Editor);

								modMesh._transformMatrix.SetPos(nextPos);
								modMesh._transformMatrix.SetRotate(nextAngle);
								modMesh._transformMatrix.SetScale(nextScale);
								modMesh._transformMatrix.MakeMatrix();

								apEditorUtil.ReleaseGUIFocus();
							}
						}
						else if (modBone != null)
						{
							if (nextPos.x != modBone._transformMatrix._pos.x ||
								nextPos.y != modBone._transformMatrix._pos.y ||
								nextAngle != modBone._transformMatrix._angleDeg ||
								nextScale.x != modBone._transformMatrix._scale.x ||
								nextScale.y != modBone._transformMatrix._scale.y)
							{
								isChanged = true;

								apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, _portrait, _animClip._targetMeshGroup, false, Editor);

								modBone._transformMatrix.SetPos(nextPos);
								modBone._transformMatrix.SetRotate(nextAngle);
								modBone._transformMatrix.SetScale(nextScale);
								modBone._transformMatrix.MakeMatrix();

								apEditorUtil.ReleaseGUIFocus();
							}
						}

					}

					if (isMod_Color)
					{
						GUILayout.Space(5);

						if (linkedModifier._isColorPropertyEnabled)
						{
							Texture2D img_Color = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Color);

							Color nextColor = modMesh._meshColor;
							bool isMeshVisible = modMesh._isVisible;

							int iconSize = 30;
							int propertyWidth = width - (iconSize + 8);

							//Color
							EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
							EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
							EditorGUILayout.LabelField(new GUIContent(img_Color), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
							EditorGUILayout.EndVertical();

							EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
							EditorGUILayout.LabelField("Color (2X)");
							try
							{
								nextColor = EditorGUILayout.ColorField("", nextColor, GUILayout.Width(propertyWidth));
							}
							catch (Exception)
							{

							}

							EditorGUILayout.EndVertical();
							EditorGUILayout.EndHorizontal();


							//Visible
							EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
							GUILayout.Space(5);
							EditorGUILayout.LabelField("Is Visible : ", GUILayout.Width(propertyWidth));
							isMeshVisible = EditorGUILayout.Toggle(isMeshVisible, GUILayout.Width(iconSize));
							EditorGUILayout.EndHorizontal();



							if (nextColor.r != modMesh._meshColor.r ||
								nextColor.g != modMesh._meshColor.g ||
								nextColor.b != modMesh._meshColor.b ||
								nextColor.a != modMesh._meshColor.a ||
								isMeshVisible != modMesh._isVisible)
							{
								isChanged = true;

								apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, _portrait, _animClip._targetMeshGroup, false, Editor);

								modMesh._meshColor = nextColor;
								modMesh._isVisible = isMeshVisible;

								apEditorUtil.ReleaseGUIFocus();
							}
						}
						else
						{
							GUI.color = new Color(0.4f, 0.4f, 0.4f, 1.0f);

							GUILayout.Box("Color Property is disabled",
											guiStyleBox,
											GUILayout.Width(width), GUILayout.Height(25));

							GUI.color = prevColor;
						}
					}


					if (isChanged)
					{
						AnimClip.UpdateControlParam(true);
					}
				}
			}
			else
			{
				//2. Curve Mode
				//1) Prev 커브를 선택할 것인지, Next 커브를 선택할 것인지 결정해야한다.
				//2) 양쪽의 컨트롤 포인트의 설정을 결정한다. (Linear / Smooth / Constant(Stepped))
				//3) 커브 GUI

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));

				GUILayout.Space(5);

				int curveTypeBtnSize = 30;
				int curveBtnSize = (width - (curveTypeBtnSize * 3 + 2 * 5)) / 2 - 6;

				apAnimCurve curveA = null;
				apAnimCurve curveB = null;
				apAnimCurveResult curveResult = null;

				string strPrevKey = "";
				string strNextKey = "";

				Color colorLabel_Prev = Color.black;
				Color colorLabel_Next = Color.black;

				if (_animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
				{
					curveA = keyframe._curveKey._prevLinkedCurveKey;
					curveB = keyframe._curveKey;
					curveResult = keyframe._curveKey._prevCurveResult;

					if (keyframe._prevLinkedKeyframe != null)
					{
						strPrevKey = "Prev [" + keyframe._prevLinkedKeyframe._frameIndex + "]";
					}
					strNextKey = "Current [" + keyframe._frameIndex + "]";
					colorLabel_Next = Color.red;
				}
				else
				{
					curveA = keyframe._curveKey;
					curveB = keyframe._curveKey._nextLinkedCurveKey;
					curveResult = keyframe._curveKey._nextCurveResult;


					strPrevKey = "Current [" + keyframe._frameIndex + "]";
					colorLabel_Prev = Color.red;
					if (keyframe._nextLinkedKeyframe != null)
					{
						strNextKey = "Next [" + keyframe._nextLinkedKeyframe._frameIndex + "]";
					}

				}



				if (apEditorUtil.ToggledButton("Prev", _animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev, curveBtnSize, 30))
				{
					_animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Prev;
				}
				if (apEditorUtil.ToggledButton("Next", _animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Next, curveBtnSize, 30))
				{
					_animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Next;
				}
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Curve_Linear), curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Linear, true, curveTypeBtnSize, 30))
				{
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Linear);
				}
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Curve_Smooth), curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Smooth, true, curveTypeBtnSize, 30))
				{
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Smooth);
				}
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Curve_Stepped), curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Constant, true, curveTypeBtnSize, 30))
				{
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Constant);
				}



				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);

				if (curveA == null || curveB == null)
				{
					EditorGUILayout.LabelField("Keyframe is not linked");
				}
				else
				{

					int curveUI_Width = width - 1;
					int curveUI_Height = 200;
					prevColor = GUI.color;

					Rect lastRect = GUILayoutUtility.GetLastRect();

					GUI.color = Editor._guiMainEditorColor;
					Rect curveRect = new Rect(lastRect.x + 5, lastRect.y, curveUI_Width, curveUI_Height);

					curveUI_Width -= 2;
					curveUI_Height -= 4;

					int layoutY_Clip = layoutY - Mathf.Min(scrollValue, 115);
					//Debug.Log("Lyout Y / layoutY : " + layoutY + " / scrollValue : " + scrollValue + " => " + layoutY_Clip);
					apAnimCurveGL.SetLayoutSize(
						curveUI_Width,
						curveUI_Height,
						(int)(lastRect.x) + layoutX - (curveUI_Width + 10),
						(int)(lastRect.y) + layoutY_Clip,
						windowWidth, windowHeight);

					bool isLeftBtnPressed = false;
					if (Event.current.rawType == EventType.MouseDown ||
						Event.current.rawType == EventType.MouseDrag)
					{
						if (Event.current.button == 0)
						{ isLeftBtnPressed = true; }
					}

					apAnimCurveGL.SetMouseValue(isLeftBtnPressed, apMouse.PosNotBound, Event.current.rawType, this);

					GUI.Box(curveRect, "");
					//EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(curveUI_Height));
					GUILayout.BeginArea(new Rect(lastRect.x + 8, lastRect.y + 124, curveUI_Width - 2, curveUI_Height - 2));

					Color curveGraphColorA = Color.black;
					Color curveGraphColorB = Color.black;
					//if(_animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
					//{
					//	curveGraphColor = new Color(0.2f, 1.0f, 0.3f, 1.0f);
					//}
					//else
					//{
					//	curveGraphColor = new Color(0.2f, 0.5f, 1.0f, 1.0f);
					//}
					if (curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Linear)
					{
						curveGraphColorA = new Color(1.0f, 0.1f, 0.1f, 1.0f);
						curveGraphColorB = new Color(1.0f, 1.0f, 0.1f, 1.0f);
					}
					else if (curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
					{
						curveGraphColorA = new Color(0.2f, 0.2f, 1.0f, 1.0f);
						curveGraphColorB = new Color(0.2f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						curveGraphColorA = new Color(0.2f, 1.0f, 0.1f, 1.0f);
						curveGraphColorB = new Color(0.1f, 1.0f, 0.6f, 1.0f);
					}


					apAnimCurveGL.DrawCurve(curveA, curveB, curveResult, curveGraphColorA, curveGraphColorB);


					GUILayout.EndArea();
					//EditorGUILayout.EndVertical();



					//GUILayout.Space(10);

					GUI.color = prevColor;


					GUILayout.Space(curveUI_Height - 2);


					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));

					GUIStyle guiStyle_FrameLabel_Prev = new GUIStyle(GUI.skin.label);
					GUIStyle guiStyle_FrameLabel_Next = new GUIStyle(GUI.skin.label);
					guiStyle_FrameLabel_Next.alignment = TextAnchor.MiddleRight;

					guiStyle_FrameLabel_Prev.normal.textColor = colorLabel_Prev;
					guiStyle_FrameLabel_Next.normal.textColor = colorLabel_Next;

					GUILayout.Space(5);
					EditorGUILayout.LabelField(strPrevKey, guiStyle_FrameLabel_Prev, GUILayout.Width(width / 2 - 4));

					EditorGUILayout.LabelField(strNextKey, guiStyle_FrameLabel_Next, GUILayout.Width(width / 2 - 4));
					EditorGUILayout.EndHorizontal();

					if (curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
					{
						GUILayout.Space(5);

						if (GUILayout.Button("Reset Smooth Setting", GUILayout.Width(width), GUILayout.Height(25)))
						{
							apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, _portrait, _animClip._targetMeshGroup, false, Editor);
							curveResult.ResetSmoothSetting();

							Editor.SetRepaint();
							//Editor.Repaint();
						}
					}
					GUILayout.Space(5);
					if (GUILayout.Button("Copy Curve to All Keyframes", GUILayout.Width(width), GUILayout.Height(25)))
					{

						Editor.Controller.CopyAnimCurveToAllKeyframes(curveResult, keyframe._parentTimelineLayer, keyframe._parentTimelineLayer._parentAnimClip);
						Editor.SetRepaint();
					}


				}
			}



			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			if (isSameKP)
			{
				//복사 / 붙여넣기 / 삭제 // (복붙은 모든 타입에서 등장한다)
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
				GUILayout.Space(5);
				//int editBtnWidth = ((width) / 2) - 3;
				int editBtnWidth_Copy = 80;
				int editBtnWidth_Paste = width - (80 + 4);
				//if (GUILayout.Button(new GUIContent(" Copy", Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy)), GUILayout.Width(editBtnWidth), GUILayout.Height(25)))
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy), " Copy", " Copy", false, true, editBtnWidth_Copy, 25))
				{
					//Debug.LogError("TODO : Copy Keyframe");
					if (keyframe != null)
					{
						string copyName = "";
						if (keyframe._parentTimelineLayer != null)
						{
							copyName += keyframe._parentTimelineLayer.DisplayName + " ";
						}
						copyName += "[ " + keyframe._frameIndex + " ]";
						apSnapShotManager.I.Copy_Keyframe(keyframe, copyName);
					}
				}

				string pasteKeyName = apSnapShotManager.I.GetClipboardName_Keyframe();
				bool isPastable = apSnapShotManager.I.IsPastable(keyframe);
				if (string.IsNullOrEmpty(pasteKeyName) || !isPastable)
				{
					pasteKeyName = "Paste";
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste), " " + pasteKeyName, " " + pasteKeyName, false, isPastable, editBtnWidth_Paste, 25))
				{
					if (keyframe != null)
					{
						//붙여넣기
						apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, _portrait, _animClip._targetMeshGroup, false, Editor);
						apSnapShotManager.I.Paste_Keyframe(keyframe);
						RefreshAnimEditing(true);
					}
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);


				//키 삭제
				if (GUILayout.Button("Remove Keyframe", GUILayout.Width(width), GUILayout.Height(20)))
				{
					Editor.Controller.RemoveKeyframe(keyframe);
				}
			}

		}

		private void DrawEditor_Bottom_AnimationProperty_MultipleKeyframes(List<apAnimKeyframe> keyframes, int width)
		{
			EditorGUILayout.LabelField(keyframes.Count + " Keyframes Selected");

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);


			//키 삭제
			if (GUILayout.Button("Remove Keyframe", GUILayout.Width(width), GUILayout.Height(20)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Keyframes", "Remove " + keyframes.Count + "s Keyframes?", "Remove", "Cancel");

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveKeyframes_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveKeyframes_Body, keyframes.Count),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);

				if (isResult)
				{
					Editor.Controller.RemoveKeyframes(keyframes);
				}
			}
		}

		private void DrawEditor_Bottom_AnimationProperty_TimelineLayer(apAnimTimelineLayer timelineLayer, int width)
		{
			EditorGUILayout.LabelField("Timeline Layer");

			GUILayout.Space(10);
			if (timelineLayer._targetParamSetGroup != null &&
				timelineLayer._parentTimeline != null &&
				timelineLayer._parentTimeline._linkedModifier != null
				)
			{
				apModifierParamSetGroup keyParamSetGroup = timelineLayer._targetParamSetGroup;
				apModifierBase modifier = timelineLayer._parentTimeline._linkedModifier;
				apAnimTimeline timeline = timelineLayer._parentTimeline;

				//이름
				//설정
				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = timelineLayer._guiColor;
				GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				guiStyle_Box.alignment = TextAnchor.MiddleCenter;

				GUILayout.Box(timelineLayer.DisplayName, guiStyle_Box, GUILayout.Width(width), GUILayout.Height(30));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);

				//1. 색상 Modifier라면 색상 옵션을 설정한다.
				if ((int)(modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
				{

					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
															" Color Option On", " Color Option Off",
															keyParamSetGroup._isColorPropertyEnabled, true,
															width, 24))
					{
						apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, _portrait, _animClip._targetMeshGroup, false, Editor);
						keyParamSetGroup._isColorPropertyEnabled = !keyParamSetGroup._isColorPropertyEnabled;

						_animClip._targetMeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy();
					}

					GUILayout.Space(10);
				}

				//2. GUI Color를 설정
				try
				{
					EditorGUILayout.LabelField("Layer GUI Color");
					Color nextGUIColor = EditorGUILayout.ColorField(timelineLayer._guiColor, GUILayout.Width(width));
					if (nextGUIColor != timelineLayer._guiColor)
					{
						apEditorUtil.SetEditorDirty();
						timelineLayer._guiColor = nextGUIColor;
					}
				}
				catch (Exception) { }

				GUILayout.Space(10);
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);
		}

		private void DrawEditor_Bottom_AnimationProperty_Timeline(apAnimTimeline timeline, int width)
		{
			EditorGUILayout.LabelField("Timeline");

			GUILayout.Space(10);

			if (timeline._linkedModifier != null
				)
			{
				apModifierBase modifier = timeline._linkedModifier;


				//이름
				//설정
				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = timeline._guiColor;
				GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				guiStyle_Box.alignment = TextAnchor.MiddleCenter;

				GUILayout.Box(timeline.DisplayName, guiStyle_Box, GUILayout.Width(width), GUILayout.Height(30));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);

				//1. 색상 Modifier라면 색상 옵션을 설정한다.
				if ((int)(modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
				{
					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
															" Color Option On", " Color Option Off",
															modifier._isColorPropertyEnabled, true,
															width, 24))
					{
						apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, _portrait, _animClip._targetMeshGroup, false, Editor);
						modifier._isColorPropertyEnabled = !modifier._isColorPropertyEnabled;
						_animClip._targetMeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy();
					}
				}
				GUILayout.Space(10);

			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);
		}



		//------------------------------------------------------------------------------------
		private void DrawEditor_Right2_MeshGroupRight_Setting(int width, int height)
		{
			bool isMeshTransform = false;
			bool isValidSelect = false;

			if (SubMeshInGroup != null)
			{
				if (SubMeshInGroup._mesh != null)
				{
					isMeshTransform = true;
					isValidSelect = true;
				}
			}
			else if (SubMeshGroupInGroup != null)
			{
				if (SubMeshGroupInGroup._meshGroup != null)
				{
					isMeshTransform = false;
					isValidSelect = true;
				}
			}

			//if (isValidSelect)
			//{
			//	//1-1. 선택된 객체가 존재하여 [객체 정보]를 출력할 수 있다.
			//	Editor.SetGUIVisible("MeshGroupBottom_Setting", true);
			//}
			//else
			//{
			//	//1-2. 선택된 객체가 없어서 우측 상세 정보 UI를 출력하지 않는다.
			//	//수정 -> 기본 루트 MeshGroupTransform을 출력한다.
			//	Editor.SetGUIVisible("MeshGroupBottom_Setting", false);

			//	return; //바로 리턴
			//}

			////2. 출력할 정보가 있다 하더라도
			////=> 바로 출력 가능한게 아니라 경우에 따라 Hide 상태를 조금 더 유지할 필요가 있다.
			//if (!Editor.IsDelayedGUIVisible("MeshGroupBottom_Setting"))
			//{
			//	//아직 출력하면 안된다.
			//	return;
			//}

			Editor.SetGUIVisible("MeshGroupRight_Setting_ObjectSelected", isValidSelect);
			Editor.SetGUIVisible("MeshGroupRight_Setting_ObjectNotSelected", !isValidSelect);

			bool isSelectedObjectRender = Editor.IsDelayedGUIVisible("MeshGroupRight_Setting_ObjectSelected");
			bool isNotSelectedObjectRender = Editor.IsDelayedGUIVisible("MeshGroupRight_Setting_ObjectNotSelected");

			if (!isSelectedObjectRender && !isNotSelectedObjectRender)
			{
				return;
			}

			//1. 오브젝트가 선택이 되었다.
			if (isSelectedObjectRender)
			{
				string objectName = "";
				string strType = "";
				string prevNickName = "";
				if (isMeshTransform)
				{
					strType = "Sub Mesh";
					objectName = SubMeshInGroup._mesh._name;
					prevNickName = SubMeshInGroup._nickName;
				}
				else
				{
					strType = "Sub Mesh Group";
					objectName = SubMeshGroupInGroup._meshGroup._name;
					prevNickName = SubMeshGroupInGroup._nickName;
				}
				//EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));

				//1. 아이콘 / 타입
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
				GUILayout.Space(10);
				if (isMeshTransform)
				{
					EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh)), GUILayout.Width(50), GUILayout.Height(50));
				}
				else
				{
					EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)), GUILayout.Width(50), GUILayout.Height(50));
				}
				EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(strType, GUILayout.Width(width - (50 + 10)));
				EditorGUILayout.LabelField(objectName, GUILayout.Width(width - (50 + 10)));


				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				//2. 닉네임
				EditorGUILayout.LabelField("Name", GUILayout.Width(80));
				string nextNickName = EditorGUILayout.DelayedTextField(prevNickName, GUILayout.Width(width));
				if (!string.Equals(nextNickName, prevNickName))
				{
					if (isMeshTransform)
					{ SubMeshInGroup._nickName = nextNickName; }
					else
					{ SubMeshGroupInGroup._nickName = nextNickName; }

					Editor.RefreshControllerAndHierarchy();
				}

				GUILayout.Space(20);

				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(10);

				Editor.SetGUIVisible("Render Unit Detail Status - MeshTransform", isMeshTransform);
				Editor.SetGUIVisible("Render Unit Detail Status - MeshGroupTransform", !isMeshTransform);

				bool isMeshTransformDetailRendererable = Editor.IsDelayedGUIVisible("Render Unit Detail Status - MeshTransform");
				bool isMeshGroupTransformDetailRendererable = Editor.IsDelayedGUIVisible("Render Unit Detail Status - MeshGroupTransform");

				//3. Mesh Transform Setting
				if (isMeshTransform && isMeshTransformDetailRendererable)
				{
					EditorGUILayout.LabelField("Shader Setting");
					SubMeshInGroup._shaderType = (apPortrait.SHADER_TYPE)EditorGUILayout.EnumPopup(SubMeshInGroup._shaderType);
					GUILayout.Space(5);
					SubMeshInGroup._isCustomShader = EditorGUILayout.Toggle("Use Custom Shader", SubMeshInGroup._isCustomShader);
					if (SubMeshInGroup._isCustomShader)
					{
						GUILayout.Space(5);
						EditorGUILayout.LabelField("Custom Shader");
						SubMeshInGroup._customShader = (Shader)EditorGUILayout.ObjectField(SubMeshInGroup._customShader, typeof(Shader), false);
					}
					GUILayout.Space(20);

					GUIStyle guiStyle_ClipStatus = new GUIStyle(GUI.skin.box);
					guiStyle_ClipStatus.alignment = TextAnchor.MiddleCenter;

					Editor.SetGUIVisible("Mesh Transform Detail Status - Clipping Child", SubMeshInGroup._isClipping_Child);
					Editor.SetGUIVisible("Mesh Transform Detail Status - Clipping Parent", SubMeshInGroup._isClipping_Parent);
					Editor.SetGUIVisible("Mesh Transform Detail Status - Clipping None", (!SubMeshInGroup._isClipping_Parent && !SubMeshInGroup._isClipping_Child));

					if (SubMeshInGroup._isClipping_Parent)
					{
						if (Editor.IsDelayedGUIVisible("Mesh Transform Detail Status - Clipping Parent"))
						{
							//1. 자식 메시를 가지는 Clipping의 Base Parent이다.
							//- 자식 메시 리스트들을 보여준다.
							//-> 레이어 순서를 바꾼다. / Clip을 해제한다..
							GUILayout.Box("Parent Mask Mesh", guiStyle_ClipStatus, GUILayout.Width(width), GUILayout.Height(25));
							GUILayout.Space(5);

							//Texture2D btnImg_Down = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown);
							//Texture2D btnImg_Up = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp);
							Texture2D btnImg_Delete = Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey);

							int iBtn = -1;
							//int btnRequestType = -1;
							#region [미사용 코드]
							//for (int iChild = 0; iChild < SubMeshInGroup._clipChildMeshTransforms.Length; iChild++)
							//{
							//	apTransform_Mesh childMesh = SubMeshInGroup._clipChildMeshTransforms[iChild];
							//	EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
							//	if (childMesh != null)
							//	{
							//		EditorGUILayout.LabelField("[" + iChild + "] " + childMesh._nickName, GUILayout.Width(width - (20 + 5)), GUILayout.Height(20));
							//		if (GUILayout.Button(btnImg_Delete, GUILayout.Width(20), GUILayout.Height(20)))
							//		{
							//			iBtn = iChild;
							//			//btnRequestType = 2;//2 : Delete
							//			Debug.LogError("TODO : Mesh 삭제 버튼 기능");
							//		}
							//	}
							//	else
							//	{
							//		EditorGUILayout.LabelField("[" + iChild + "] <Empty>", GUILayout.Width(width), GUILayout.Height(20));
							//	}
							//	EditorGUILayout.EndHorizontal();
							//} 
							#endregion

							for (int iChild = 0; iChild < SubMeshInGroup._clipChildMeshes.Count; iChild++)
							{
								apTransform_Mesh childMesh = SubMeshInGroup._clipChildMeshes[iChild]._meshTransform;
								EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
								if (childMesh != null)
								{
									EditorGUILayout.LabelField("[" + iChild + "] " + childMesh._nickName, GUILayout.Width(width - (20 + 5)), GUILayout.Height(20));
									if (GUILayout.Button(btnImg_Delete, GUILayout.Width(20), GUILayout.Height(20)))
									{
										iBtn = iChild;
										//btnRequestType = 2;//2 : Delete
										Debug.LogError("TODO : Mesh 삭제 버튼 기능");
									}
								}
								else
								{
									EditorGUILayout.LabelField("[" + iChild + "] <Empty>", GUILayout.Width(width), GUILayout.Height(20));
								}
								EditorGUILayout.EndHorizontal();
							}


							if (iBtn >= 0)
							{
								Debug.LogError("TODO : Mesh 삭제");
							}
						}
					}
					else if (SubMeshInGroup._isClipping_Child)
					{


						if (Editor.IsDelayedGUIVisible("Mesh Transform Detail Status - Clipping Child"))
						{
							//2. Parent를 Mask로 삼는 자식 Mesh이다.
							//- 부모 메시를 보여준다.
							//-> 순서 바꾸기를 요청한다
							//-> Clip을 해제한다.
							GUILayout.Box("Child Clipped Mesh", guiStyle_ClipStatus, GUILayout.Width(width), GUILayout.Height(25));
							GUILayout.Space(5);

							string strParentName = "<No Mask Parent>";
							if (SubMeshInGroup._clipParentMeshTransform != null)
							{
								strParentName = SubMeshInGroup._clipParentMeshTransform._nickName;
							}
							EditorGUILayout.LabelField("Mask Parent : " + strParentName, GUILayout.Width(width));
							EditorGUILayout.LabelField("Clipped Index : " + SubMeshInGroup._clipIndexFromParent, GUILayout.Width(width));
							EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

							//int btnRequestType = -1;
							if (GUILayout.Button("Release", GUILayout.Width(width), GUILayout.Height(25)))
							{
								//btnRequestType = 2;//2 : Delete
								Editor.Controller.ReleaseClippingMeshTransform(MeshGroup, SubMeshInGroup);
							}
							EditorGUILayout.EndHorizontal();


						}
					}
					else
					{
						//3. 기본 상태의 Mesh이다.
						//Clip을 요청한다.
						if (GUILayout.Button("Clipping To Below Mesh", GUILayout.Width(width), GUILayout.Height(25)))
						{
							Editor.Controller.AddClippingMeshTransform(MeshGroup, SubMeshInGroup, true);
						}
					}
				}
				else if (!isMeshTransform && isMeshGroupTransformDetailRendererable)
				{

				}

				if (isMeshTransformDetailRendererable || isMeshGroupTransformDetailRendererable)
				{
					GUILayout.Space(20);
					//4. Detach

					apEditorUtil.GUI_DelimeterBoxH(width - 10);
					GUILayout.Space(10);

					if (GUILayout.Button("Detach " + strType))
					{
						//bool isResult = EditorUtility.DisplayDialog("Detach", "Detach it?", "Detach", "Cancel");
						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.Detach_Title),
																		Editor.GetText(apLocalization.TEXT.Detach_Body),
																		Editor.GetText(apLocalization.TEXT.Detach_Ok),
																		Editor.GetText(apLocalization.TEXT.Cancel)
																		);
						if (isResult)
						{
							if (isMeshTransform)
							{
								Editor.Controller.DetachMeshInMeshGroup(SubMeshInGroup, MeshGroup);
								Editor.Select.SetSubMeshInGroup(null);
							}
							else
							{
								Editor.Controller.DetachMeshGroupInMeshGroup(SubMeshGroupInGroup, MeshGroup);
								Editor.Select.SetSubMeshGroupInGroup(null);
							}
						}
						MeshGroup.SetDirtyToSort();//TODO : Sort에서 자식 객체 변한것 체크 : Clip 그룹 체크
						MeshGroup.RefreshForce();
						Editor.SetRepaint();
					}
					//EditorGUILayout.EndVertical();
				}
			}
			else if (isNotSelectedObjectRender)
			{
				//2. 오브젝트가 선택이 안되었다.
				//기본 정보를 출력하고, 루트 MeshGroupTransform의 Transform 값을 설정한다.
				apTransform_MeshGroup rootMeshGroupTransform = MeshGroup._rootMeshGroupTransform;

				//1. 아이콘 / 타입
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
				GUILayout.Space(10);
				EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)), GUILayout.Width(50), GUILayout.Height(50));

				EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
				GUILayout.Space(5);
				EditorGUILayout.LabelField("Mesh Group", GUILayout.Width(width - (50 + 12)));
				EditorGUILayout.LabelField(MeshGroup.name, GUILayout.Width(width - (50 + 12)));

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(10);


				EditorGUILayout.BeginVertical(GUILayout.Width(width));


				EditorGUILayout.LabelField("Root Transform");
				//Vector2 rootPos = rootMeshGroupTransform._matrix._pos;
				//float rootAngle = rootMeshGroupTransform._matrix._angleDeg;
				//Vector2 rootScale = rootMeshGroupTransform._matrix._scale;

				Texture2D img_Pos = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Move);
				Texture2D img_Rot = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Rotate);
				Texture2D img_Scale = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Scale);

				int iconSize = 30;
				int propertyWidth = width - (iconSize + 12);

				//Position
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
				EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
				EditorGUILayout.LabelField(new GUIContent(img_Pos), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
				EditorGUILayout.LabelField("Position", GUILayout.Width(propertyWidth));
				//nextPos = EditorGUILayout.Vector2Field("", nextPos, GUILayout.Width(propertyWidth));
				Vector2 rootPos = apEditorUtil.DelayedVector2Field(rootMeshGroupTransform._matrix._pos, propertyWidth);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				//Rotation
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
				EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
				EditorGUILayout.LabelField(new GUIContent(img_Rot), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
				EditorGUILayout.LabelField("Rotation", GUILayout.Width(propertyWidth));

				float rootAngle = EditorGUILayout.DelayedFloatField(rootMeshGroupTransform._matrix._angleDeg, GUILayout.Width(propertyWidth));
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				//Scaling
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(iconSize));
				EditorGUILayout.BeginVertical(GUILayout.Width(iconSize));
				EditorGUILayout.LabelField(new GUIContent(img_Scale), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(propertyWidth), GUILayout.Height(iconSize));
				EditorGUILayout.LabelField("Scaling", GUILayout.Width(propertyWidth));

				//nextScale = EditorGUILayout.Vector2Field("", nextScale, GUILayout.Width(propertyWidth));
				Vector2 rootScale = apEditorUtil.DelayedVector2Field(rootMeshGroupTransform._matrix._scale, propertyWidth);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();

				//테스트용
				//rootMeshGroupTransform._isVisible_Default = EditorGUILayout.Toggle("Is Visible", rootMeshGroupTransform._isVisible_Default, GUILayout.Width(width));
				//EditorGUILayout.ColorField("Color2x", rootMeshGroupTransform._meshColor2X_Default);


				if (rootPos != rootMeshGroupTransform._matrix._pos
					|| rootAngle != rootMeshGroupTransform._matrix._angleDeg
					|| rootScale != rootMeshGroupTransform._matrix._scale)
				{
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, MeshGroup, MeshGroup, false, Editor);
					rootMeshGroupTransform._matrix.SetTRS(rootPos.x, rootPos.y, rootAngle, rootScale.x, rootScale.y);
					MeshGroup.RefreshForce();
					apEditorUtil.ReleaseGUIFocus();
				}
			}
		}

		//private string _prevName_BoneProperty = "";
		private apBone _prevBone_BoneProperty = null;
		private int _prevChildBoneCount = 0;
		private void DrawEditor_Right2_MeshGroup_Bone(int width, int height)
		{
			//int subWidth = 250;
			apBone curBone = Bone;

			bool isRefresh = false;
			bool isAnyGUIAction = false;

			bool isChildBoneChanged = false;

			bool isBoneChanged = (_prevBone_BoneProperty != curBone);
			if (curBone != null)
			{
				isChildBoneChanged = (_prevChildBoneCount != curBone._childBones.Count);
			}


			if (_prevBone_BoneProperty != curBone)
			{
				_prevBone_BoneProperty = curBone;
				if (curBone != null)
				{
					//_prevName_BoneProperty = curBone._name;
					_prevChildBoneCount = curBone._childBones.Count;
				}
				else
				{
					//_prevName_BoneProperty = "";
					_prevChildBoneCount = 0;
				}

				Editor.SetGUIVisible("Update Child Bones", false);
			}

			if (curBone != null)
			{
				if (_prevChildBoneCount != curBone._childBones.Count)
				{
					Editor.SetGUIVisible("Update Child Bones", true);
					if (Editor.IsDelayedGUIVisible("Update Child Bones"))
					{
						//Debug.Log("Child Bone Count Changed : " + _prevChildBoneCount + " -> " + curBone._childBones.Count);
						_prevChildBoneCount = curBone._childBones.Count;
					}
				}
			}

			Editor.SetGUIVisible("MeshGroupRight2 Bone", curBone != null
				&& !isBoneChanged
				//&& !isChildBoneChanged
				);
			Editor.SetGUIVisible("MeshGroup Bone - Child Bone Drawable", true);
			if (!Editor.IsDelayedGUIVisible("MeshGroupRight2 Bone")
				//|| !Editor.IsDelayedGUIVisible("MeshGroup Bone - Child Bone Drawable")
				)
			{
				return;
			}



			//1. 아이콘 / 타입
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
			GUILayout.Space(10);

			//모디파이어 아이콘
			EditorGUILayout.LabelField(
				new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)),
				GUILayout.Width(50), GUILayout.Height(50));

			int nameWidth = width - (50 + 10);
			EditorGUILayout.BeginVertical(GUILayout.Width(nameWidth));
			GUILayout.Space(5);
			//EditorGUILayout.LabelField(curBone._name, GUILayout.Width(width - (50 + 10)));
			//EditorGUILayout.LabelField("Layer : " + Modifier._layer, GUILayout.Width(width - (50 + 10)));
			EditorGUILayout.LabelField("Bone", GUILayout.Width(nameWidth));

			string nextBoneName = EditorGUILayout.DelayedTextField(curBone._name, GUILayout.Width(nameWidth));
			if (!string.Equals(nextBoneName, curBone._name))
			{
				curBone._name = nextBoneName;
				isRefresh = true;
				isAnyGUIAction = true;
			}
			#region [미사용 코드] DelayedTextField 사용 전 코드
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(nameWidth));
			//_prevName_BoneProperty = EditorGUILayout.TextField(_prevName_BoneProperty, GUILayout.Width(nameWidth - 62));
			//if (GUILayout.Button("Change", GUILayout.Width(60)))
			//{
			//	curBone._name = _prevName_BoneProperty;
			//	isRefresh = true;
			//	isAnyGUIAction = true;
			//}

			//EditorGUILayout.EndHorizontal(); 
			#endregion

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20);

			//Default Matrix 설정
			Vector2 defPos = curBone._defaultMatrix._pos;
			float defAngle = curBone._defaultMatrix._angleDeg;
			Vector2 defScale = curBone._defaultMatrix._scale;

			EditorGUILayout.LabelField("Base Pose Transformation", GUILayout.Width(width));
			int widthValue = width - 72;

			if (!IsBoneDefaultEditing)
			{
				//여기서는 보여주기만
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField("Position", GUILayout.Width(70));
				EditorGUILayout.LabelField(defPos.x + ", " + defPos.y, GUILayout.Width(widthValue));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField("Rotation", GUILayout.Width(70));
				EditorGUILayout.LabelField(defAngle + "", GUILayout.Width(widthValue));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField("Scaling", GUILayout.Width(70));
				EditorGUILayout.LabelField(defScale.x + ", " + defScale.y, GUILayout.Width(widthValue));
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				//여기서는 설정이 가능하다

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField("Position", GUILayout.Width(70));
				//defPos.x = EditorGUILayout.FloatField(defPos.x, GUILayout.Width(widthValue / 2 - 2));
				//defPos.y = EditorGUILayout.FloatField(defPos.y, GUILayout.Width(widthValue / 2 - 2));

				//defPos = EditorGUILayout.Vector2Field("", defPos, GUILayout.Width(widthValue));
				defPos = apEditorUtil.DelayedVector2Field(defPos, widthValue);

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField("Rotation", GUILayout.Width(70));
				defAngle = EditorGUILayout.DelayedFloatField(defAngle, GUILayout.Width(widthValue));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				EditorGUILayout.LabelField("Scaling", GUILayout.Width(70));
				//defScale.x = EditorGUILayout.FloatField(defScale.x, GUILayout.Width(widthValue / 2 - 2));
				//defScale.y = EditorGUILayout.FloatField(defScale.y, GUILayout.Width(widthValue / 2 - 2));

				//defScale = EditorGUILayout.Vector2Field("", defScale, GUILayout.Width(widthValue));
				defScale = apEditorUtil.DelayedVector2Field(defScale, widthValue);

				EditorGUILayout.EndHorizontal();

				if (defPos != curBone._defaultMatrix._pos ||
					defAngle != curBone._defaultMatrix._angleDeg ||
					defScale != curBone._defaultMatrix._scale)
				{
					curBone._defaultMatrix.SetPos(defPos);
					curBone._defaultMatrix.SetRotate(defAngle);
					curBone._defaultMatrix.SetScale(defScale);

					curBone.MakeWorldMatrix(true);
					//isRefresh = true;
					isAnyGUIAction = true;
				}
			}
			GUILayout.Space(10);
			if (apEditorUtil.ToggledButton_2Side("Socket Enabled", "Socket Disabled", curBone._isSocketEnabled, true, width, 25))
			{
				curBone._isSocketEnabled = !curBone._isSocketEnabled;
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//IK 설정
			EditorGUILayout.LabelField("IK Setting", GUILayout.Width(width));

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(40));
			int IKModeBtnSize = (width / 4) - 4;
			//EditorGUILayout.LabelField("IK Option", GUILayout.Width(70));
			GUILayout.Space(5);
			apBone.OPTION_IK nextOptionIK = curBone._optionIK;

			//apBone.OPTION_IK nextOptionIK = (apBone.OPTION_IK)EditorGUILayout.EnumPopup(curBone._optionIK, GUILayout.Width(widthValue));

			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKSingle), curBone._optionIK == apBone.OPTION_IK.IKSingle, true, IKModeBtnSize, 40))
			{
				nextOptionIK = apBone.OPTION_IK.IKSingle;
				isAnyGUIAction = true;
			}
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKHead), curBone._optionIK == apBone.OPTION_IK.IKHead, true, IKModeBtnSize, 40))
			{
				nextOptionIK = apBone.OPTION_IK.IKHead;
				isAnyGUIAction = true;
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKChained), curBone._optionIK == apBone.OPTION_IK.IKChained, curBone._optionIK == apBone.OPTION_IK.IKChained, IKModeBtnSize, 40))
			{
				//nextOptionIK = apBone.OPTION_IK.IKSingle;//Chained는 직접 설정할 수 있는게 아니다.
				isAnyGUIAction = true;
			}
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKDisabled), curBone._optionIK == apBone.OPTION_IK.Disabled, true, IKModeBtnSize, 40))
			{
				nextOptionIK = apBone.OPTION_IK.Disabled;
				isAnyGUIAction = true;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			string strIKInfo = "";
			Color prevColor = GUI.color;

			Color boxColor = Color.black;
			switch (curBone._optionIK)
			{
				case apBone.OPTION_IK.IKSingle:
					strIKInfo = "[IK Single]\nIK is applied to One Child Bone";
					boxColor = new Color(1.0f, 0.6f, 0.5f, 1.0f);
					break;

				case apBone.OPTION_IK.IKHead:
					strIKInfo = "[IK Head]\nIK is applied to Chained Bones";
					boxColor = new Color(1.0f, 0.5f, 0.6f, 1.0f);
					break;

				case apBone.OPTION_IK.IKChained:
					strIKInfo = "[IK Chain]\nLocated in the middle of IK Chain";
					boxColor = new Color(0.7f, 0.5f, 1.0f, 1.0f);
					break;

				case apBone.OPTION_IK.Disabled:
					strIKInfo = "[Disabled]\nIK is not applied";
					boxColor = new Color(0.6f, 0.8f, 1.0f, 1.0f);
					break;
			}
			GUI.color = boxColor;
			GUIStyle guiStyleInfoBox = new GUIStyle(GUI.skin.box);
			guiStyleInfoBox.alignment = TextAnchor.MiddleCenter;

			GUILayout.Box(strIKInfo, guiStyleInfoBox, GUILayout.Width(width), GUILayout.Height(40));

			GUI.color = prevColor;

			GUILayout.Space(10);


			if (nextOptionIK != curBone._optionIK)
			{
				//Debug.Log("IK Change : " + curBone._optionIK + " > " + nextOptionIK);

				bool isIKOptionChangeValid = false;

				//이제 IK 옵션에 맞는지 체크해주자
				if (curBone._optionIK == apBone.OPTION_IK.IKChained)
				{
					//Chained 상태에서는 아예 바꿀 수 없다.
					//EditorUtility.DisplayDialog("IK Option Information",
					//	"<IK Chained> setting has been forced.\nTo Change, change the IK setting in the <IK Header>.",
					//	"Close");

					EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.IKOption_Title),
													Editor.GetText(apLocalization.TEXT.IKOption_Body_Chained),
													Editor.GetText(apLocalization.TEXT.Close));
				}
				else
				{
					//그외에는 변경이 가능하다
					switch (nextOptionIK)
					{
						case apBone.OPTION_IK.Disabled:
							//끄는 건 쉽다.
							isIKOptionChangeValid = true;
							break;

						case apBone.OPTION_IK.IKChained:
							//IK Chained는 직접 할 수 있는게 아니다.
							//EditorUtility.DisplayDialog("IK Option Information",
							//"<IK Chained> setting is set automatically.\nTo change, change the setting in the <IK Header>.",
							//"Close");

							EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.IKOption_Title),
												Editor.GetText(apLocalization.TEXT.IKOption_Body_Chained),
												Editor.GetText(apLocalization.TEXT.Close));
							break;

						case apBone.OPTION_IK.IKHead:
							{
								//자식으로 연결된게 없으면 일단 바로 아래 자식을 연결하자.
								//자식이 없으면 실패

								apBone nextChainedBone = curBone._IKNextChainedBone;
								apBone targetBone = curBone._IKTargetBone;

								bool isRefreshNeed = true;
								if (nextChainedBone != null && targetBone != null)
								{
									//이전에 연결된 값이 존재하고, 재귀적인 연결도 유효한 경우는 패스
									if (curBone.GetChildBone(nextChainedBone._uniqueID) != null
										&& curBone.GetChildBoneRecursive(targetBone._uniqueID) != null)
									{
										//유효한 설정이다.
										isRefreshNeed = false;
									}
								}

								if (isRefreshNeed)
								{
									//자식 Bone의 하나를 연결하자
									if (curBone._childBones.Count > 0)
									{
										curBone._IKNextChainedBone = curBone._childBones[0];
										curBone._IKTargetBone = curBone._childBones[0];

										curBone._IKNextChainedBoneID = curBone._IKNextChainedBone._uniqueID;
										curBone._IKTargetBoneID = curBone._IKTargetBone._uniqueID;

										isIKOptionChangeValid = true;//기본값을 넣어서 변경 가능
									}
									else
									{
										//EditorUtility.DisplayDialog("IK Option Information",
										//"<IK Head> setting requires one or more child Bones.",
										//"Close");

										EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.IKOption_Title),
													Editor.GetText(apLocalization.TEXT.IKOption_Body_Head),
													Editor.GetText(apLocalization.TEXT.Close));
									}
								}
								else
								{
									isIKOptionChangeValid = true;
								}
							}
							break;

						case apBone.OPTION_IK.IKSingle:
							{
								//IK Target과 NextChained가 다르면 일단 그것부터 같게 하자.
								//나머지는 Head와 동일
								curBone._IKTargetBone = curBone._IKNextChainedBone;
								curBone._IKTargetBoneID = curBone._IKNextChainedBoneID;

								apBone nextChainedBone = curBone._IKNextChainedBone;

								bool isRefreshNeed = true;
								if (nextChainedBone != null)
								{
									//이전에 연결된 값이 존재하고, 재귀적인 연결도 유효한 경우는 패스
									if (curBone.GetChildBone(nextChainedBone._uniqueID) != null)
									{
										//유효한 설정이다.
										isRefreshNeed = false;
									}
								}

								if (isRefreshNeed)
								{
									//자식 Bone의 하나를 연결하자
									if (curBone._childBones.Count > 0)
									{
										curBone._IKNextChainedBone = curBone._childBones[0];
										curBone._IKTargetBone = curBone._childBones[0];

										curBone._IKNextChainedBoneID = curBone._IKNextChainedBone._uniqueID;
										curBone._IKTargetBoneID = curBone._IKTargetBone._uniqueID;

										isIKOptionChangeValid = true;//기본값을 넣어서 변경 가능
									}
									else
									{
										//EditorUtility.DisplayDialog("IK Option Information",
										//"<IK Single> setting requires a child Bone.",
										//"Close");

										EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.IKOption_Title),
													Editor.GetText(apLocalization.TEXT.IKOption_Body_Single),
													Editor.GetText(apLocalization.TEXT.Close));
									}
								}
								else
								{
									isIKOptionChangeValid = true;
								}
							}
							break;
					}
				}



				if (isIKOptionChangeValid)
				{
					curBone._optionIK = nextOptionIK;

					isRefresh = true;
				}
				//TODO : 너무 자동으로 Bone Chain을 하는것 같다;
				//옵션이 적용이 안된다;
			}



			EditorGUILayout.LabelField("IK Header", GUILayout.Width(width));
			string headerBoneName = "<None>";
			if (curBone._IKHeaderBone != null)
			{
				headerBoneName = curBone._IKHeaderBone._name;
			}
			EditorGUILayout.LabelField(new GUIContent(" " + headerBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width));

			GUILayout.Space(5);
			EditorGUILayout.LabelField("IK Next Chain To Target", GUILayout.Width(width));
			string nextChainedBoneName = "<None>";
			if (curBone._IKNextChainedBone != null)
			{
				nextChainedBoneName = curBone._IKNextChainedBone._name;
			}
			EditorGUILayout.LabelField(new GUIContent(" " + nextChainedBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width));
			GUILayout.Space(5);


			if (curBone._optionIK != apBone.OPTION_IK.Disabled)
			{
				EditorGUILayout.LabelField("IK Target", GUILayout.Width(width));

				apBone targetBone = curBone._IKTargetBone;

				string targetBoneName = "<None>";

				if (targetBone != null)
				{
					targetBoneName = targetBone._name;
				}

				EditorGUILayout.LabelField(new GUIContent(" " + targetBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width));



				//Target을 설정하자.
				if (curBone._optionIK == apBone.OPTION_IK.IKHead)
				{
					if (GUILayout.Button("Change IK Target", GUILayout.Width(width), GUILayout.Height(20)))
					{
						//Debug.LogError("TODO : IK Target을 Dialog를 열어서 설정하자.");
						_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKTarget, OnDialogSelectBone);
						isAnyGUIAction = true;
					}
				}



				GUILayout.Space(15);
				EditorGUILayout.LabelField("IK Angle Constraint", GUILayout.Width(width));

				//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				//EditorGUILayout.LabelField("Is Angle Constraint", GUILayout.Width(70));
				if (apEditorUtil.ToggledButton_2Side("Constraint On", "Constraint Off", curBone._isIKAngleRange, true, width, 25))
				{
					curBone._isIKAngleRange = !curBone._isIKAngleRange;
					isAnyGUIAction = true;
				}
				//bool isNextIKAngle = EditorGUILayout.Toggle("Is Angle Constraint", curBone._isIKAngleRange, GUILayout.Width(width));
				//EditorGUILayout.EndHorizontal();

				if (curBone._isIKAngleRange)
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField("Min", GUILayout.Width(70));
					float nextLowerAngle = EditorGUILayout.Slider(curBone._IKAngleRange_Lower, -180, 0, GUILayout.Width(widthValue));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField("Max", GUILayout.Width(70));
					float nextUpperAngle = EditorGUILayout.Slider(curBone._IKAngleRange_Upper, 0, 180, GUILayout.Width(widthValue));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField("Preferred", GUILayout.Width(70));
					float nextPreferredAngle = EditorGUILayout.Slider(curBone._IKAnglePreferred, -180, 180, GUILayout.Width(widthValue));
					EditorGUILayout.EndHorizontal();

					if (nextLowerAngle != curBone._IKAngleRange_Lower ||
						nextUpperAngle != curBone._IKAngleRange_Upper ||
						nextPreferredAngle != curBone._IKAnglePreferred)
					{
						curBone._IKAngleRange_Lower = nextLowerAngle;
						curBone._IKAngleRange_Upper = nextUpperAngle;
						curBone._IKAnglePreferred = nextPreferredAngle;
						//isRefresh = true;
						isAnyGUIAction = true;
					}
				}

				//if(isNextIKAngle != curBone._isIKAngleRange)
				//{
				//	curBone._isIKAngleRange = isNextIKAngle;
				//	Debug.Log("IK Angle Changed : " + curBone._isIKAngleRange);
				//	//isRefresh = true;
				//}
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//Hierarchy 설정
			EditorGUILayout.LabelField("Hierarchy", GUILayout.Width(width));
			//Parent와 Child List를 보여주자.
			EditorGUILayout.LabelField("Parent Bone", GUILayout.Width(width));
			string parentName = "<None>";
			if (curBone._parentBone != null)
			{
				parentName = curBone._parentBone._name;
			}
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(new GUIContent(" " + parentName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width - 60));
			if (GUILayout.Button("Change", GUILayout.Width(58)))
			{
				//Debug.LogError("TODO : Change Parent Dialog 구현할 것");
				isAnyGUIAction = true;
				_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.ChangeParent, OnDialogSelectBone);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			int nChildList = curBone._childBones.Count;
			if (_prevChildBoneCount != nChildList)
			{
				Debug.Log("Count is not matched : " + _prevChildBoneCount + " > " + nChildList);
			}
			EditorGUILayout.LabelField("Children Bones [" + nChildList + "]", GUILayout.Width(width));

			//Detach가 
			apBone detachedBone = null;

			for (int iChild = 0; iChild < _prevChildBoneCount; iChild++)
			{
				if (iChild >= nChildList)
				{
					//리스트를 벗어났다.
					//더미 Layout을 그리자
					//유니티 레이아웃 처리방식때문..
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField("", GUILayout.Width(width - 60));
					if (GUILayout.Button("Detach", GUILayout.Width(58)))
					{

					}
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					apBone childBone = curBone._childBones[iChild];
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
					EditorGUILayout.LabelField(new GUIContent(" " + childBone._name, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width - 60));
					if (GUILayout.Button("Detach", GUILayout.Width(58)))
					{
						//Debug.LogError("TODO : Change Parent Dialog 구현할 것");
						//bool isResult = EditorUtility.DisplayDialog("Detach Child Bone", "Detach Bone? [" + childBone._name + "]", "Detach", "Cancel")
						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.DetachChildBone_Title),
																		Editor.GetTextFormat(apLocalization.TEXT.DetachChildBone_Body, childBone._name),
																		Editor.GetText(apLocalization.TEXT.Detach_Ok),
																		Editor.GetText(apLocalization.TEXT.Cancel)
																		);

						if (isResult)
						{
							//Debug.LogError("TODO : Detach Child Bone 구현할 것");
							//Detach Child Bone 선택
							detachedBone = childBone;
							isAnyGUIAction = true;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			if (GUILayout.Button("Attach Child Bone", GUILayout.Width(width), GUILayout.Height(20)))
			{
				isAnyGUIAction = true;
				_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.AttachChild, OnDialogSelectBone);
			}
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);


			//Shape 설정
			EditorGUILayout.LabelField("Shape", GUILayout.Width(width));

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Color", GUILayout.Width(70));
			try
			{
				curBone._color = EditorGUILayout.ColorField(curBone._color, GUILayout.Width(widthValue));
			}
			catch (Exception) { }
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Width", GUILayout.Width(70));
			curBone._shapeWidth = EditorGUILayout.DelayedIntField(curBone._shapeWidth, GUILayout.Width(widthValue));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Taper", GUILayout.Width(70));
			curBone._shapeTaper = EditorGUILayout.DelayedIntField(curBone._shapeTaper, GUILayout.Width(widthValue));
			EditorGUILayout.EndHorizontal();


			//Detach 요청이 있으면 수행 후 Refresh를 하자
			if (detachedBone != null)
			{
				isAnyGUIAction = true;
				Editor.Controller.DetachBoneFromChild(curBone, detachedBone);
				Editor.SetGUIVisible("MeshGroup Bone - Child Bone Drawable", false);
				isRefresh = true;
			}


			if (isAnyGUIAction)
			{
				//여기서 뭔가 처리를 했으면 Select 모드로 강제된다.
				if (_boneEditMode != BONE_EDIT_MODE.SelectAndTRS)
				{
					SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);
				}
			}

			if (isRefresh)
			{
				Editor.RefreshControllerAndHierarchy();
				Editor._portrait.LinkAndRefreshInEditor();
			}

			GUILayout.Space(20);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);
			if (GUILayout.Button("Remove Bone", GUILayout.Width(width)))
			{
				isAnyGUIAction = true;
				SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);

				int btnIndex = EditorUtility.DisplayDialogComplex("Remove Bone", "Remove Bone [" + curBone._name + "] ?", "Remove", "Remove All Child Bones", "Cancel");
				if (btnIndex == 0)
				{
					//Bone을 삭제한다.
					Editor.Controller.RemoveBone(curBone, false);
				}
				else if (btnIndex == 1)
				{
					//Bone과 자식을 모두 삭제한다.
					Editor.Controller.RemoveBone(curBone, true);
				}
			}

			//if (curBone != null && _prevChildBoneCount != curBone._childBones.Count)
			//{
			//	Editor.SetGUIVisible("Update Child Bones", true);
			//	if(Editor.IsDelayedGUIVisible("Update Child Bones"))
			//	//if (Event.current.type == EventType.layout)
			//	{
			//		Debug.Log("Child Bone Count Adapted [" + curBone._childBones.Count + " > " + _prevChildBoneCount + "] (" + Event.current.type + ")");
			//		_prevChildBoneCount = curBone._childBones.Count;
			//		Editor.SetGUIVisible("Update Child Bones", false);
			//	}
			//}
		}

		private object _loadKey_SelectBone = null;
		private void OnDialogSelectBone(bool isSuccess, object loadKey, bool isNullBone, apBone selectedBone, apBone targetBone, apDialog_SelectLinkedBone.REQUEST_TYPE requestType)
		{
			if (_loadKey_SelectBone != loadKey)
			{
				_loadKey_SelectBone = null;
				return;
			}
			if (!isSuccess)
			{
				_loadKey_SelectBone = null;
				return;
			}


			_loadKey_SelectBone = null;
			switch (requestType)
			{
				case apDialog_SelectLinkedBone.REQUEST_TYPE.AttachChild:
					{
						Editor.Controller.AttachBoneToChild(targetBone, selectedBone);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.ChangeParent:
					{
						Editor.Controller.SetBoneAsParent(targetBone, selectedBone);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKTarget:
					{
						Editor.Controller.SetBoneAsIKTarget(targetBone, selectedBone);
					}
					break;
			}
		}


		private void DrawEditor_Right2_MeshGroup_Modifier(int width, int height)
		{
			if (Modifier != null)
			{
				//1-1. 선택된 객체가 존재하여 [객체 정보]를 출력할 수 있다.
				Editor.SetGUIVisible("MeshGroupBottom_Modifier", true);
			}
			else
			{
				//1-2. 선택된 객체가 없어서 하단 UI를 출력하지 않는다.
				Editor.SetGUIVisible("MeshGroupBottom_Modifier", false);

				return; //바로 리턴
			}

			//2. 출력할 정보가 있다 하더라도
			//=> 바로 출력 가능한게 아니라 경우에 따라 Hide 상태를 조금 더 유지할 필요가 있다.
			if (!Editor.IsDelayedGUIVisible("MeshGroupBottom_Modifier"))
			{
				//아직 출력하면 안된다.
				return;
			}
			//1. 아이콘 / 타입
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
			GUILayout.Space(10);

			//모디파이어 아이콘
			EditorGUILayout.LabelField(
				new GUIContent(Editor.ImageSet.Get(apEditorUtil.GetModifierIconType(Modifier.ModifierType))),
				GUILayout.Width(50), GUILayout.Height(50));

			EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(Modifier.DisplayName, GUILayout.Width(width - (50 + 10)));
			EditorGUILayout.LabelField("Layer : " + Modifier._layer, GUILayout.Width(width - (50 + 10)));


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			//추가
			//만약 색상 옵션이 있는 경우 설정을 하자
			if ((int)(Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			{
				//Modifier._isColorPropertyEnabled = EditorGUILayout.Toggle("Color Property ", Modifier._isColorPropertyEnabled, GUILayout.Width(width));
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
														" Color Option On",
														" Color Option Off",
														Modifier._isColorPropertyEnabled, true,
														width, 24
													))
				{
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, Modifier, true, Editor);
					Modifier._isColorPropertyEnabled = !Modifier._isColorPropertyEnabled;
					Editor.RefreshControllerAndHierarchy();
				}
				GUILayout.Space(10);
			}


			//2. 기본 블렌딩 설정
			EditorGUILayout.LabelField("Blend", GUILayout.Width(width));

			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

			EditorGUILayout.LabelField("Method", GUILayout.Width(70));
			apModifierBase.BLEND_METHOD blendMethod = (apModifierBase.BLEND_METHOD)EditorGUILayout.EnumPopup(Modifier._blendMethod, GUILayout.Width(width - (70 + 5)));
			if (blendMethod != Modifier._blendMethod)
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, Modifier, true, Editor);
				Modifier._blendMethod = blendMethod;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Weight", GUILayout.Width(70));
			float layerWeight = EditorGUILayout.DelayedFloatField(Modifier._layerWeight, GUILayout.Width(width - (70 + 5)));

			layerWeight = Mathf.Clamp01(layerWeight);
			if (layerWeight != Modifier._layerWeight)
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, Modifier, true, Editor);
				Modifier._layerWeight = layerWeight;
			}
			EditorGUILayout.EndHorizontal();

			//레이어 이동
			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Layer Up", GUILayout.Width(width / 2 - 5)))
			{
				Editor.Controller.LayerChange(Modifier, true);
			}
			if (GUILayout.Button("Layer Down", GUILayout.Width(width / 2 - 5)))
			{
				Editor.Controller.LayerChange(Modifier, false);
			}
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(10);

			//3. 각 프로퍼티 렌더링
			// 수정
			//일괄적으로 호출하자
			DrawModifierPropertyGUI(width, height);
			//switch (Modifier.ModifierType)
			//{
			//	case apModifierBase.MODIFIER_TYPE.Morph:
			//		MeshGroupBottomStatus_Modifier(width, height);
			//		break;

			//	case apModifierBase.MODIFIER_TYPE.Volume:
			//		MeshGroupBottomStatus_Modifier_Volume(width, height);
			//		break;

			//		//TODO : 새로운 모디파이어에 맞게 프로퍼티를 렌더링해야한다.

			//	default:
			//		GUILayout.Space(5);
			//		break;
			//}

			GUILayout.Space(20);


			//4. Modifier 삭제
			apEditorUtil.GUI_DelimeterBoxH(width - 10);
			GUILayout.Space(10);

			if (GUILayout.Button("Remove Modifier"))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove", "Remove Modifier [" + Modifier.DisplayName + "]?", "Remove", "Cancel");

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveModifier_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveModifier_Body, Modifier.DisplayName),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);

				if (isResult)
				{
					Editor.Controller.RemoveModifier(Modifier);
				}
			}


			//삭제 직후라면 출력 에러가 발생한다.
			if (Modifier == null)
			{
				return;
			}

		}

		private Vector2 _scrollBottom_Status = Vector2.zero;

		//private object _controlPramDialog_LoadKey = null;

		private void DrawModifierPropertyGUI(int width, int height)
		{
			if (Modifier != null)
			{
				string strRecordName = Modifier.DisplayName;


				if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					//Rigging UI를 작성
					DrawModifierPropertyGUI_Rigging(width, height, strRecordName);
				}
				else if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
				{
					//Physic UI를 작성
					DrawModifierPropertyGUI_Physics(width, height);
				}
				else
				{
					//그 외에는 ParamSetGroup에 따라서 UI를 구성하면 된다.
					switch (Modifier.SyncTarget)
					{
						case apModifierParamSetGroup.SYNC_TARGET.Bones:
							break;

						case apModifierParamSetGroup.SYNC_TARGET.Controller:
							{
								//Control Param 리스트
								apDialog_SelectControlParam.PARAM_TYPE paramFilter = apDialog_SelectControlParam.PARAM_TYPE.All;
								DrawModifierPropertyGUI_ControllerParamSet(width, height, paramFilter, strRecordName);
							}
							break;

						case apModifierParamSetGroup.SYNC_TARGET.ControllerWithoutKey:
							break;

						case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
							{
								//Keyframe 리스트
								DrawModifierPropertyGUI_KeyframeParamSet(width, height, strRecordName);
							}
							break;

						case apModifierParamSetGroup.SYNC_TARGET.Static:
							break;
					}
				}

			}


			GUILayout.Space(20);


		}


		#region [미사용 코드]
		//private void MeshGroupBottomStatus_Modifier_Volume(int width, int height)
		//{
		//	apDialog_SelectControlParam.PARAM_TYPE paramFilter =
		//	apDialog_SelectControlParam.PARAM_TYPE.Float |
		//			apDialog_SelectControlParam.PARAM_TYPE.Int |
		//			apDialog_SelectControlParam.PARAM_TYPE.Vector2 |
		//			apDialog_SelectControlParam.PARAM_TYPE.Vector3;

		//	DrawModifierPropertyGUI_ControllerParamSet(width, height, paramFilter, "Volume");

		//	GUILayout.Space(20);
		//} 
		#endregion

		// Modifier 보조 함수들
		//------------------------------------------------------------------------------------
		private void DrawModifierPropertyGUI_ControllerParamSet(int width, int height, apDialog_SelectControlParam.PARAM_TYPE paramFilter, string recordName)
		{
			// SyncTarget으로 Control Param을 받아서 Modifier를 제어하는 경우
			GUIStyle guiNone = new GUIStyle(GUIStyle.none);
			guiNone.normal.textColor = GUI.skin.label.normal.textColor;

			EditorGUILayout.LabelField("Control Parameters", GUILayout.Width(width));

			GUILayout.Space(5);


			// 생성된 Morph Key (Parameter Group)를 선택하자
			//------------------------------------------------------------------
			// Control Param에 따른 Param Set Group 리스트
			//------------------------------------------------------------------
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(120));
			GUILayout.Space(5);

			Rect lastRect = GUILayoutUtility.GetLastRect();

			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

			GUI.Box(new Rect(lastRect.x + 5, lastRect.y, width, 120), "");
			GUI.backgroundColor = prevColor;

			//처리 역순으로 보여준다.
			List<apModifierParamSetGroup> paramSetGroups = new List<apModifierParamSetGroup>();
			if (Modifier._paramSetGroup_controller.Count > 0)
			{
				for (int i = Modifier._paramSetGroup_controller.Count - 1; i >= 0; i--)
				{
					paramSetGroups.Add(Modifier._paramSetGroup_controller[i]);
				}
			}

			//등록된 Control Param Group 리스트를 출력하자
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(120));
			_scrollBottom_Status = EditorGUILayout.BeginScrollView(_scrollBottom_Status, false, true);
			GUILayout.Space(2);
			int scrollWidth = width - (30);
			EditorGUILayout.BeginVertical(GUILayout.Width(scrollWidth), GUILayout.Height(120));
			GUILayout.Space(3);

			Texture2D paramIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);
			Texture2D visibleIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current);
			Texture2D nonvisibleIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current);

			//현재 선택중인 파라미터 그룹
			apModifierParamSetGroup curParamSetGroup = SubEditedParamSetGroup;

			for (int i = 0; i < paramSetGroups.Count; i++)
			{
				if (curParamSetGroup == paramSetGroups[i])
				{
					lastRect = GUILayoutUtility.GetLastRect();
					GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

					int offsetHeight = 18 + 3;
					if (i == 0)
					{
						offsetHeight = 1 + 3;
					}

					GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 20), "");
					GUI.backgroundColor = prevColor;

				}
				EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
				GUILayout.Space(5);
				if (GUILayout.Button(new GUIContent(" " + paramSetGroups[i]._keyControlParam._keyName, paramIconImage),
									guiNone,
									GUILayout.Width(scrollWidth - (5 + 25)), GUILayout.Height(20)))
				{
					//ParamSetGroup을 선택했다.
					SetParamSetGroupOfModifier(paramSetGroups[i]);
					AutoSelectParamSetOfModifier();//<자동 선택까지

					Editor.RefreshControllerAndHierarchy();
				}

				Texture2D imageVisible = visibleIconImage;

				if (!paramSetGroups[i]._isEnabled)
				{
					imageVisible = nonvisibleIconImage;
				}
				if (GUILayout.Button(imageVisible, guiNone, GUILayout.Width(20), GUILayout.Height(20)))
				{
					paramSetGroups[i]._isEnabled = !paramSetGroups[i]._isEnabled;
				}
				EditorGUILayout.EndHorizontal();
			}


			EditorGUILayout.EndVertical();

			GUILayout.Space(120);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			//------------------------------------------------------------------ < Param Set Group 리스트



			//-----------------------------------------------------------------------------------
			// Param Set Group 선택시 / 선택된 Param Set Group 정보와 포함된 Param Set 리스트
			//-----------------------------------------------------------------------------------



			GUILayout.Space(10);

			Editor.SetGUIVisible("CP Selected ParamSetGroup", (SubEditedParamSetGroup != null));

			if (!Editor.IsDelayedGUIVisible("CP Selected ParamSetGroup"))
			{
				return;
			}
			//ParamSetGroup에 레이어 옵션이 추가되었다.
			EditorGUILayout.LabelField("Parameters Setting");
			GUILayout.Space(2);
			EditorGUILayout.LabelField("Blend Method");
			apModifierParamSetGroup.BLEND_METHOD psgBlendMethod = (apModifierParamSetGroup.BLEND_METHOD)EditorGUILayout.EnumPopup(SubEditedParamSetGroup._blendMethod, GUILayout.Width(width));
			if (psgBlendMethod != SubEditedParamSetGroup._blendMethod)
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, null, false, Editor);
				SubEditedParamSetGroup._blendMethod = psgBlendMethod;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Weight", GUILayout.Width(80));
			float psgLayerWeight = EditorGUILayout.Slider(SubEditedParamSetGroup._layerWeight, 0.0f, 1.0f, GUILayout.Width(width - 85));
			if (psgLayerWeight != SubEditedParamSetGroup._layerWeight)
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, null, false, Editor);
				SubEditedParamSetGroup._layerWeight = psgLayerWeight;
			}

			EditorGUILayout.EndHorizontal();

			if ((int)(Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			{
				//색상 옵션을 넣어주자
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
													" Color Option On", " Color Option Off",
													SubEditedParamSetGroup._isColorPropertyEnabled, true,
													width, 24))
				{
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, null, false, Editor);
					SubEditedParamSetGroup._isColorPropertyEnabled = !SubEditedParamSetGroup._isColorPropertyEnabled;
					Editor.RefreshControllerAndHierarchy();
				}
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			if (GUILayout.Button("Layer Up", GUILayout.Width(width / 2 - 2)))
			{
				Modifier.ChangeParamSetGroupLayerIndex(SubEditedParamSetGroup, SubEditedParamSetGroup._layerIndex + 1);
			}
			if (GUILayout.Button("Layer Down", GUILayout.Width(width / 2 - 2)))
			{
				Modifier.ChangeParamSetGroupLayerIndex(SubEditedParamSetGroup, SubEditedParamSetGroup._layerIndex - 1);
			}
			EditorGUILayout.EndHorizontal();

			//TODO : ModMeshOfMod만 작성되어있다.
			//ModBoneOfMod도 작성되어야 한다.

			GUILayout.Space(5);
			//변경 : Copy&Paste는 ModMesh가 선택되어있느냐, ModBone이 선택되어있느냐에 따라 다르다

			bool isModMeshSelected = ModMeshOfMod != null;
			bool isModBoneSelected = ModBoneOfMod != null && Modifier.IsTarget_Bone;

			//복사 가능한가
			bool isModPastable = false;

			if (isModMeshSelected)
			{ isModPastable = apSnapShotManager.I.IsPastable(ModMeshOfMod); }
			else if (isModBoneSelected)
			{ isModPastable = apSnapShotManager.I.IsPastable(ModBoneOfMod); }

			//Color prevColor = GUI.backgroundColor;

			GUIStyle guiStyle_Center = new GUIStyle(GUI.skin.box);
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			if (isModPastable)
			{
				GUI.backgroundColor = new Color(0.2f, 0.5f, 0.7f, 1.0f);
				guiStyle_Center.normal.textColor = Color.white;
			}

			//Clipboard 이름 설정
			string strClipboardKeyName = "";

			if (isModMeshSelected)
			{ strClipboardKeyName = apSnapShotManager.I.GetClipboardName_ModMesh(); }
			else if (isModBoneSelected)
			{ strClipboardKeyName = apSnapShotManager.I.GetClipboardName_ModBone(); }

			if (string.IsNullOrEmpty(strClipboardKeyName))
			{
				strClipboardKeyName = "<Empty Clipboard>";
			}


			GUILayout.Box(strClipboardKeyName, guiStyle_Center, GUILayout.Width(width), GUILayout.Height(32));
			GUI.backgroundColor = prevColor;

			//추가
			//선택된 키가 있다면 => Copy / Paste / Reset 버튼을 만든다.
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			if (GUILayout.Button(new GUIContent(" Copy", Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy)), GUILayout.Width(width / 2 - 4), GUILayout.Height(24)))
			{
				//Debug.LogError("TODO : Copy Morph Key");
				//ModMesh를 복사할 것인지, ModBone을 복사할 것인지 결정
				if (SubEditedParamSetGroup != null && ParamSetOfMod != null)
				{
					if (isModMeshSelected && ParamSetOfMod._meshData.Contains(ModMeshOfMod))
					{
						//ModMesh 복사
						string clipboardName = "";
						if (ModMeshOfMod._transform_Mesh != null)
						{ clipboardName = ModMeshOfMod._transform_Mesh._nickName; }
						else if (ModMeshOfMod._transform_MeshGroup != null)
						{ clipboardName = ModMeshOfMod._transform_MeshGroup._nickName; }

						//clipboardName += "\n" + ParamSetOfMod._controlKeyName + "( " + ParamSetOfMod.ControlParamValue + " )";
						string controlParamName = "[Unknown Param]";
						if (SubEditedParamSetGroup._keyControlParam != null)
						{
							controlParamName = SubEditedParamSetGroup._keyControlParam._keyName;
						}
						clipboardName += "\n" + controlParamName + "( " + ParamSetOfMod.ControlParamValue + " )";

						apSnapShotManager.I.Copy_ModMesh(ModMeshOfMod, clipboardName);
					}
					else if (isModBoneSelected && ParamSetOfMod._boneData.Contains(ModBoneOfMod))
					{
						//ModBone 복사
						string clipboardName = "";
						if (ModBoneOfMod._bone != null)
						{ clipboardName = ModBoneOfMod._bone._name; }

						//clipboardName += "\n" + ParamSetOfMod._controlKeyName + "( " + ParamSetOfMod.ControlParamValue + " )";
						string controlParamName = "[Unknown Param]";
						if (SubEditedParamSetGroup._keyControlParam != null)
						{
							controlParamName = SubEditedParamSetGroup._keyControlParam._keyName;
						}
						clipboardName += "\n" + controlParamName + "( " + ParamSetOfMod.ControlParamValue + " )";

						apSnapShotManager.I.Copy_ModBone(ModBoneOfMod, clipboardName);
					}
				}
			}
			if (GUILayout.Button(new GUIContent(" Paste", Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste)), GUILayout.Width(width / 2 - 4), GUILayout.Height(24)))
			{
				//ModMesh를 복사할 것인지, ModBone을 복사할 것인지 결정
				if (SubEditedParamSetGroup != null && ParamSetOfMod != null)
				{
					object targetObj = ModMeshOfMod;
					if (isModBoneSelected)
					{
						targetObj = ModBoneOfMod;
					}
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_ModMeshValuePaste, MeshGroup, targetObj, false, Editor);

					if (isModMeshSelected && ParamSetOfMod._meshData.Contains(ModMeshOfMod))
					{
						//ModMesh 붙여넣기를 하자
						bool isResult = apSnapShotManager.I.Paste_ModMesh(ModMeshOfMod);
						if (!isResult)
						{
							//EditorUtility.DisplayDialog("Paste Failed", "Paste Failed", "Okay");
							Editor.Notification("Paste Failed", true, false);
						}
						//MeshGroup.AddForceUpdateTarget(ModMeshOfMod._renderUnit);
						MeshGroup.RefreshForce();
					}
					else if (isModBoneSelected && ParamSetOfMod._boneData.Contains(ModBoneOfMod))
					{
						//ModBone 붙여넣기를 하자
						bool isResult = apSnapShotManager.I.Paste_ModBone(ModBoneOfMod);
						if (!isResult)
						{
							//EditorUtility.DisplayDialog("Paste Failed", "Paste Failed", "Okay");
							Editor.Notification("Paste Failed", true, false);
						}
						//if(ModBoneOfMod._renderUnit != null)
						//{
						//	MeshGroup.AddForceUpdateTarget(ModBoneOfMod._renderUnit);
						//}
						MeshGroup.RefreshForce();
					}

				}
			}
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Reset Value", GUILayout.Width(width - 4), GUILayout.Height(20)))
			{
				if (ParamSetOfMod != null)
				{
					object targetObj = ModMeshOfMod;
					if (ModBoneOfMod != null)
					{
						targetObj = ModBoneOfMod;
					}

					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_ModMeshValueReset, MeshGroup, targetObj, false, Editor);

					if (ModMeshOfMod != null)
					{
						//ModMesh를 리셋한다.

						ModMeshOfMod.ResetValues();

						//MeshGroup.AddForceUpdateTarget(ModMeshOfMod._renderUnit);
						MeshGroup.RefreshForce();
					}
					else if (ModBoneOfMod != null)
					{
						//ModBone을 리셋한다.
						ModBoneOfMod._transformMatrix.SetIdentity();
						//if(ModBoneOfMod._renderUnit != null)
						//{
						//	MeshGroup.AddForceUpdateTarget(ModBoneOfMod._renderUnit);
						//}
						MeshGroup.RefreshForce();
					}
				}
			}
			GUILayout.Space(12);



			//--------------------------------------------------------------
			// Param Set 중 하나를 선택했을 때
			// 타겟을 등록 / 해제한다.
			// Transform 등록 / 해제
			//--------------------------------------------------------------
			bool isAnyTargetSelected = false;
			bool isContain = false;
			string strTargetName = "";
			object selectedObj = null;

			bool isTarget_Bone = Modifier.IsTarget_Bone;
			bool isTarget_MeshTransform = Modifier.IsTarget_MeshTransform;
			bool isTarget_MeshGroupTransform = Modifier.IsTarget_MeshGroupTransform;
			bool isTarget_ChildMeshTransform = Modifier.IsTarget_ChildMeshTransform;

			bool isBoneTarget = false;

			// 타겟을 선택하자
			bool isAddable = false;
			if (isTarget_Bone && !isAnyTargetSelected)
			{
				//1. Bone 선택
				//TODO : Bone 체크
				if (Bone != null)
				{
					isAnyTargetSelected = true;
					isAddable = true;
					isContain = SubEditedParamSetGroup.IsBoneContain(Bone);
					strTargetName = Bone._name;
					selectedObj = Bone;
					isBoneTarget = true;
				}
			}
			if (isTarget_MeshTransform && !isAnyTargetSelected)
			{
				//2. Mesh Transform 선택
				//Child 체크가 가능할까
				if (SubMeshInGroup != null)
				{
					apRenderUnit targetRenderUnit = null;
					//Child Mesh를 허용하는가
					if (isTarget_ChildMeshTransform)
					{
						//Child를 허용한다.
						targetRenderUnit = MeshGroup.GetRenderUnit(SubMeshInGroup);
					}
					else
					{
						//Child를 허용하지 않는다.
						targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(SubMeshInGroup);
					}
					if (targetRenderUnit != null)
					{
						//유효한 선택인 경우
						isContain = SubEditedParamSetGroup.IsMeshTransformContain(SubMeshInGroup);
						isAnyTargetSelected = true;
						strTargetName = SubMeshInGroup._nickName;
						selectedObj = SubMeshInGroup;

						isAddable = true;
					}
				}
			}
			if (isTarget_MeshGroupTransform && !isAnyTargetSelected)
			{
				if (SubMeshGroupInGroup != null)
				{
					//3. MeshGroup Transform 선택
					isContain = SubEditedParamSetGroup.IsMeshGroupTransformContain(SubMeshGroupInGroup);
					isAnyTargetSelected = true;
					strTargetName = SubMeshGroupInGroup._nickName;
					selectedObj = SubMeshGroupInGroup;

					isAddable = true;
				}
			}


			Editor.SetGUIVisible("Modifier_Add Transform Check", isAnyTargetSelected);
			Editor.SetGUIVisible("Modifier_Add Transform Check_Inverse", !isAnyTargetSelected);

			bool isGUI_TargetSelected = Editor.IsDelayedGUIVisible("Modifier_Add Transform Check");
			bool isGUI_TargetUnSelected = Editor.IsDelayedGUIVisible("Modifier_Add Transform Check_Inverse");

			if (isGUI_TargetSelected || isGUI_TargetUnSelected)
			{
				if (isGUI_TargetSelected)
				{
					//Color prevColor = GUI.backgroundColor;
					GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
					boxGUIStyle.alignment = TextAnchor.MiddleCenter;

					if (isContain)
					{
						GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
						GUILayout.Box("[" + strTargetName + "]\nSelected", GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;

						if (GUILayout.Button("Remove From Keys", GUILayout.Width(width), GUILayout.Height(35)))
						{

							//bool result = EditorUtility.DisplayDialog("Remove From Keys", "Remove From Keys [" + strTargetName + "]", "Remove", "Cancel");

							bool result = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveFromKeys_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveFromKeys_Body, strTargetName),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);

							if (result)
							{
								object targetObj = null;
								if (SubMeshInGroup != null && selectedObj == SubMeshInGroup)
								{
									targetObj = SubMeshInGroup;
								}
								else if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
								{
									targetObj = SubMeshGroupInGroup;
								}
								else if (Bone != null && selectedObj == Bone)
								{
									targetObj = Bone;
								}

								//Undo
								apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_RemoveModMeshFromParamSet, MeshGroup, targetObj, false, Editor);

								if (SubMeshInGroup != null && selectedObj == SubMeshInGroup)
								{
									SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshInGroup);
								}
								else if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
								{
									SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshGroupInGroup);
								}
								else if (Bone != null && selectedObj == Bone)
								{
									SubEditedParamSetGroup.RemoveModifierBones(Bone);
								}
								else
								{
									//?
								}

								Editor._portrait.LinkAndRefreshInEditor();
								AutoSelectModMeshOrModBone();

								Editor.SetRepaint();
							}
						}
					}
					else if (!isAddable)
					{
						//추가 가능하지 않다.
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						GUILayout.Box("[" + strTargetName + "]\nNot able to be Added", GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;
					}
					else
					{
						//아직 추가하지 않았다. 추가하자
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						GUILayout.Box("[" + strTargetName + "]\nNot Added to Edit", GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;

						if (GUILayout.Button("Start Editing", GUILayout.Width(width), GUILayout.Height(50)))
						{
							//ModMesh또는 ModBone으로 생성 후 추가한다.
							if (isBoneTarget)
							{
								//Bone
								Editor.Controller.AddModBone_WithSelectedBone();
							}
							else
							{
								//MeshTransform, MeshGroup
								Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();
							}

							Editor.SetRepaint();
						}
					}
					GUI.backgroundColor = prevColor;
				}

				EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(10));
				EditorGUILayout.EndVertical();
				GUILayout.Space(11);

				//ParamSetWeight를 사용하는 Modifier인가
				bool isUseParamSetWeight = Modifier.IsUseParamSetWeight;


				// Param Set 리스트를 출력한다.
				//-------------------------------------
				int iRemove = -1;
				for (int i = 0; i < SubEditedParamSetGroup._paramSetList.Count; i++)
				{
					bool isRemove = DrawModParamSetProperty(i, SubEditedParamSetGroup, SubEditedParamSetGroup._paramSetList[i], width - 10, ParamSetOfMod, isUseParamSetWeight);
					if (isRemove)
					{
						iRemove = i;
					}
				}
				if (iRemove >= 0)
				{
					Editor.Controller.RemoveRecordKey(SubEditedParamSetGroup._paramSetList[iRemove], null);
				}
			}


			//-----------------------------------------------------------------------------------
		}

		private bool DrawModParamSetProperty(int index, apModifierParamSetGroup paramSetGroup, apModifierParamSet paramSet, int width, apModifierParamSet selectedParamSet, bool isUseParamSetWeight)
		{
			bool isRemove = false;
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Color prevColor = GUI.backgroundColor;

			bool isSelect = false;
			if (paramSet == selectedParamSet)
			{
				GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);
				isSelect = true;
			}
			else
			{
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
			}

			int heightOffset = 18;
			if (index == 0)
			{
				//heightOffset = 5;
				heightOffset = 9;
			}

			GUI.Box(new Rect(lastRect.x, lastRect.y + heightOffset, width + 10, 30), "");
			GUI.backgroundColor = prevColor;



			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));

			GUILayout.Space(10);

			int compWidth = width - (55 + 20 + 5 + 10);
			if (isUseParamSetWeight)
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.textField);
				guiStyle.alignment = TextAnchor.MiddleLeft;

				//ParamSetWeight를 출력/수정할 수 있게 한다.
				float paramSetWeight = EditorGUILayout.DelayedFloatField(paramSet._overlapWeight, guiStyle, GUILayout.Width(30), GUILayout.Height(20));
				if (paramSetWeight != paramSet._overlapWeight)
				{
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, null, false, Editor);
					paramSet._overlapWeight = Mathf.Clamp01(paramSetWeight);
					apEditorUtil.ReleaseGUIFocus();
					MeshGroup.RefreshForce();
					Editor.RefreshControllerAndHierarchy();
				}
				compWidth -= 34;
			}

			switch (paramSetGroup._keyControlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	{
				//		GUIStyle guiStyle = new GUIStyle(GUI.skin.toggle);
				//		guiStyle.alignment = TextAnchor.MiddleLeft;
				//		paramSet._conSyncValue_Bool = EditorGUILayout.Toggle(paramSet._conSyncValue_Bool, guiStyle, GUILayout.Width(compWidth), GUILayout.Height(20));
				//	}

				//	break;

				case apControlParam.TYPE.Int:
					{
						GUIStyle guiStyle = new GUIStyle(GUI.skin.textField);
						guiStyle.alignment = TextAnchor.MiddleLeft;
						int conInt = EditorGUILayout.DelayedIntField(paramSet._conSyncValue_Int, guiStyle, GUILayout.Width(compWidth), GUILayout.Height(20));
						if (conInt != paramSet._conSyncValue_Int)
						{
							//이건 Dirty만 하자
							apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, null, false, Editor);
							paramSet._conSyncValue_Int = conInt;
							apEditorUtil.ReleaseGUIFocus();
						}

					}
					break;

				case apControlParam.TYPE.Float:
					{
						GUIStyle guiStyle = new GUIStyle(GUI.skin.textField);
						guiStyle.alignment = TextAnchor.MiddleLeft;
						float conFloat = EditorGUILayout.DelayedFloatField(paramSet._conSyncValue_Float, guiStyle, GUILayout.Width(compWidth), GUILayout.Height(20));
						if (conFloat != paramSet._conSyncValue_Float)
						{
							apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, null, false, Editor);
							paramSet._conSyncValue_Float = conFloat;
							apEditorUtil.ReleaseGUIFocus();
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						GUIStyle guiStyle = new GUIStyle(GUI.skin.textField);
						guiStyle.alignment = TextAnchor.MiddleLeft;
						float conVec2X = EditorGUILayout.DelayedFloatField(paramSet._conSyncValue_Vector2.x, guiStyle, GUILayout.Width(compWidth / 2 - 2), GUILayout.Height(20));
						float conVec2Y = EditorGUILayout.DelayedFloatField(paramSet._conSyncValue_Vector2.y, guiStyle, GUILayout.Width(compWidth / 2 - 2), GUILayout.Height(20));
						if (conVec2X != paramSet._conSyncValue_Vector2.x || conVec2Y != paramSet._conSyncValue_Vector2.y)
						{
							apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Modifier._meshGroup, null, false, Editor);
							paramSet._conSyncValue_Vector2.x = conVec2X;
							paramSet._conSyncValue_Vector2.y = conVec2Y;
							apEditorUtil.ReleaseGUIFocus();
						}

					}
					break;
			}

			if (isSelect)
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = Color.white;
				guiStyle.alignment = TextAnchor.UpperCenter;
				GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
				GUILayout.Box("Editing", guiStyle, GUILayout.Width(55), GUILayout.Height(20));
				GUI.backgroundColor = prevColor;
			}
			else
			{
				if (GUILayout.Button("Select", GUILayout.Width(55), GUILayout.Height(20)))
				{
					if (Editor._tabLeft != apEditor.TAB_LEFT.Controller)
					{
						Editor._tabLeft = apEditor.TAB_LEFT.Controller;
					}
					SetParamSetOfModifier(paramSet);
					if (ParamSetOfMod != null)
					{
						apControlParam targetControlParam = paramSetGroup._keyControlParam;
						if (targetControlParam != null)
						{
							//switch (ParamSetOfMod._controlParam._valueType)
							switch (targetControlParam._valueType)
							{
								//case apControlParam.TYPE.Bool:
								//	targetControlParam._bool_Cur = paramSet._conSyncValue_Bool;
								//	break;

								case apControlParam.TYPE.Int:
									targetControlParam._int_Cur = paramSet._conSyncValue_Int;
									//if (targetControlParam._isRange)
									{
										targetControlParam._int_Cur =
											Mathf.Clamp(targetControlParam._int_Cur,
														targetControlParam._int_Min,
														targetControlParam._int_Max);
									}
									break;

								case apControlParam.TYPE.Float:
									targetControlParam._float_Cur = paramSet._conSyncValue_Float;
									//if (targetControlParam._isRange)
									{
										targetControlParam._float_Cur =
											Mathf.Clamp(targetControlParam._float_Cur,
														targetControlParam._float_Min,
														targetControlParam._float_Max);
									}
									break;

								case apControlParam.TYPE.Vector2:
									targetControlParam._vec2_Cur = paramSet._conSyncValue_Vector2;
									//if (targetControlParam._isRange)
									{
										targetControlParam._vec2_Cur.x =
											Mathf.Clamp(targetControlParam._vec2_Cur.x,
														targetControlParam._vec2_Min.x,
														targetControlParam._vec2_Max.x);

										targetControlParam._vec2_Cur.y =
											Mathf.Clamp(targetControlParam._vec2_Cur.y,
														targetControlParam._vec2_Min.y,
														targetControlParam._vec2_Max.y);
									}
									break;


							}
						}
					}
				}
			}

			if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey), GUILayout.Width(20), GUILayout.Height(20)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Record Key", "Remove Record Key?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveRecordKey_Title),
																Editor.GetText(apLocalization.TEXT.RemoveRecordKey_Body),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel));
				if (isResult)
				{
					//삭제시 true 리턴
					isRemove = true;
				}
			}



			EditorGUILayout.EndHorizontal();
			GUILayout.Space(20);

			return isRemove;
		}



		private void DrawModifierPropertyGUI_KeyframeParamSet(int width, int height, string recordName)
		{
			GUIStyle guiNone = new GUIStyle(GUIStyle.none);
			guiNone.normal.textColor = GUI.skin.label.normal.textColor;

			EditorGUILayout.LabelField("Animation Clips", GUILayout.Width(width));

			GUILayout.Space(5);

			// 생성된 ParamSet Group을 선택하자
			//------------------------------------------------------------------
			// AnimClip에 따른 Param Set Group Anim Pack 리스트
			//------------------------------------------------------------------
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(120));
			GUILayout.Space(5);

			Rect lastRect = GUILayoutUtility.GetLastRect();

			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

			GUI.Box(new Rect(lastRect.x + 5, lastRect.y, width, 120), "");
			GUI.backgroundColor = prevColor;

			#region [미사용 코드] 이건 처리 역순 필요 없다. 동적 순서 배열이므로
			////처리 역순으로 보여준다. // 
			//List<apModifierParamSetGroup> paramSetGroups = new List<apModifierParamSetGroup>();
			//if (Modifier._paramSetGroup_controller.Count > 0)
			//{
			//	for (int i = Modifier._paramSetGroup_controller.Count - 1; i >= 0; i--)
			//	{
			//		paramSetGroups.Add(Modifier._paramSetGroup_controller[i]);
			//	}
			//} 
			#endregion
			List<apModifierParamSetGroupAnimPack> paramSetGroupAnimPacks = Modifier._paramSetGroupAnimPacks;


			//등록된 Keyframe Param Group 리스트를 출력하자
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(120));
			_scrollBottom_Status = EditorGUILayout.BeginScrollView(_scrollBottom_Status, false, true);
			GUILayout.Space(2);
			int scrollWidth = width - (30);
			EditorGUILayout.BeginVertical(GUILayout.Width(scrollWidth), GUILayout.Height(120));
			GUILayout.Space(3);

			Texture2D animIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

			//현재 선택중인 파라미터 그룹
			apModifierParamSetGroupAnimPack curParamSetGroupAnimPack = SubEditedParamSetGroupAnimPack;

			for (int i = 0; i < paramSetGroupAnimPacks.Count; i++)
			{
				if (curParamSetGroupAnimPack == paramSetGroupAnimPacks[i])
				{
					lastRect = GUILayoutUtility.GetLastRect();
					GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

					int offsetHeight = 18 + 3;
					if (i == 0)
					{
						offsetHeight = 1 + 3;
					}

					GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 20), "");
					GUI.backgroundColor = prevColor;

				}
				EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
				GUILayout.Space(5);
				if (GUILayout.Button(new GUIContent(" " + paramSetGroupAnimPacks[i].LinkedAnimClip._name, animIconImage),
									guiNone,
									GUILayout.Width(scrollWidth - (5)), GUILayout.Height(20)))
				{
					SetParamSetGroupAnimPackOfModifier(paramSetGroupAnimPacks[i]);

					Editor.RefreshControllerAndHierarchy();
				}
				EditorGUILayout.EndHorizontal();
			}


			EditorGUILayout.EndVertical();

			GUILayout.Space(120);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			//------------------------------------------------------------------ < Param Set Group 리스트

			//-----------------------------------------------------------------------------------
			// Param Set Group 선택시 / 선택된 Param Set Group 정보와 포함된 Param Set 리스트
			//-----------------------------------------------------------------------------------

			GUILayout.Space(10);

			//>> 여기서 ParamSetGroup 설정을 할 순 없다. (ParamSetGroup이 TimelineLayer이므로.
			//AnimClip 기준으로는 ParamSetGroup을 묶은 가상의 그룹(SubEditedParamSetGroupAnimPack)을 설정해야하는데,
			//이건 묶음이므로 실제로는 Animation 설정에서 Timeline에서 해야한다. (Timelinelayer = ParamSetGroup이므로)
			//Editor.SetGUIVisible("Anim Selected ParamSetGroup", (SubEditedParamSetGroupAnimPack. != null));

			//if (!Editor.IsDelayedGUIVisible("Anim Selected ParamSetGroup"))
			//{
			//	return;
			//}


			//EditorGUILayout.LabelField("Selected Animation Clip");
			//EditorGUILayout.LabelField(_subEditedParamSetGroupAnimPack._keyAnimClip._name);
			//GUILayout.Space(5);

			//if ((int)(Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			//{
			//	//색상 옵션을 넣어주자
			//	EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//	EditorGUILayout.LabelField("Color Option", GUILayout.Width(160));
			//	_subEditedParamSetGroupAnimPack._isColorPropertyEnabled = EditorGUILayout.Toggle(_subEditedParamSetGroupAnimPack._isColorPropertyEnabled, GUILayout.Width(width - 85));
			//	EditorGUILayout.EndHorizontal();
			//}
		}


		private object _riggingModifier_prevSelectedTransform = null;
		private bool _riggingModifier_prevIsContained = false;
		private int _riggingModifier_prevNumBoneWeights = 0;
		//Rigging Modifier UI를 출력한다.
		private void DrawModifierPropertyGUI_Rigging(int width, int height, string recordName)
		{
			GUIStyle guiNone = new GUIStyle(GUIStyle.none);
			guiNone.normal.textColor = GUI.skin.label.normal.textColor;
			guiNone.alignment = TextAnchor.MiddleLeft;

			EditorGUILayout.LabelField("Target Mesh Transform", GUILayout.Width(width));
			//1. Mesh Transform 등록 체크
			//2. Weight 툴
			// 선택한 Vertex
			// Auto Normalize
			// Set Weight, +/- Weight, * Weight
			// Blend, Auto Rigging, Normalize, Prune,
			// Copy / Paste
			// Bone (Color, Remove)

			bool isTarget_MeshTransform = Modifier.IsTarget_MeshTransform;
			bool isTarget_ChildMeshTransform = Modifier.IsTarget_ChildMeshTransform;

			bool isContainInParamSetGroup = false;
			string strTargetName = "";
			object selectedObj = null;
			bool isAnyTargetSelected = false;
			bool isAddable = false;

			apTransform_Mesh targetMeshTransform = SubMeshInGroup;
			apModifierParamSetGroup paramSetGroup = SubEditedParamSetGroup;
			if (paramSetGroup == null)
			{
				//? Rigging에서는 ParamSetGroup이 있어야 한다.
				Editor.Controller.AddStaticParamSetGroupToModifier();

				if (Modifier._paramSetGroup_controller.Count > 0)
				{
					SetParamSetGroupOfModifier(Modifier._paramSetGroup_controller[0]);
				}
				paramSetGroup = SubEditedParamSetGroup;
				if (paramSetGroup == null)
				{
					Debug.LogError("ParamSet Group Is Null (" + Modifier._paramSetGroup_controller.Count + ")");
					return;
				}

				AutoSelectModMeshOrModBone();
			}
			apModifierParamSet paramSet = ParamSetOfMod;
			if (paramSet == null)
			{
				//Rigging에서는 1개의 ParamSetGroup과 1개의 ParamSet이 있어야 한다.
				//선택된게 없다면, ParamSet이 1개 있는지 확인
				//그후 선택한다.

				if (paramSetGroup._paramSetList.Count == 0)
				{
					paramSet = new apModifierParamSet();
					paramSet.LinkParamSetGroup(paramSetGroup);
					paramSetGroup._paramSetList.Add(paramSet);
				}
				else
				{
					paramSet = paramSetGroup._paramSetList[0];
				}
				SetParamSetOfModifier(paramSet);
			}



			//1. Mesh Transform 등록 체크
			if (targetMeshTransform != null)
			{
				apRenderUnit targetRenderUnit = null;
				//Child Mesh를 허용하는가
				if (isTarget_ChildMeshTransform)
				{
					//Child를 허용한다.
					targetRenderUnit = MeshGroup.GetRenderUnit(targetMeshTransform);
				}
				else
				{
					//Child를 허용하지 않는다.
					targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(targetMeshTransform);
				}
				if (targetRenderUnit != null)
				{
					//유효한 선택인 경우
					isContainInParamSetGroup = paramSetGroup.IsMeshTransformContain(targetMeshTransform);
					isAnyTargetSelected = true;
					strTargetName = targetMeshTransform._nickName;
					selectedObj = targetMeshTransform;

					isAddable = true;
				}
			}

			if (Event.current.type == EventType.Layout ||
				Event.current.type == EventType.Repaint)
			{
				_riggingModifier_prevSelectedTransform = targetMeshTransform;
				_riggingModifier_prevIsContained = isContainInParamSetGroup;
			}
			bool isSameSetting = (targetMeshTransform == _riggingModifier_prevSelectedTransform)
								&& (isContainInParamSetGroup == _riggingModifier_prevIsContained);


			Editor.SetGUIVisible("Modifier_Add Transform Check [Rigging]", isSameSetting);



			if (!Editor.IsDelayedGUIVisible("Modifier_Add Transform Check [Rigging]"))
			{
				return;
			}

			Color prevColor = GUI.backgroundColor;

			GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
			boxGUIStyle.alignment = TextAnchor.MiddleCenter;

			if (targetMeshTransform == null)
			{
				//선택된 MeshTransform이 없다.
				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				GUILayout.Box("No Mesh is Selected", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;
			}
			else if (isContainInParamSetGroup)
			{
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				GUILayout.Box("[" + strTargetName + "]\nSelected", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (GUILayout.Button("Remove From Rigging", GUILayout.Width(width), GUILayout.Height(30)))
				{

					//bool result = EditorUtility.DisplayDialog("Remove From Rigging", "Remove From Rigging [" + strTargetName + "]", "Remove", "Cancel");

					bool result = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveFromRigging_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveFromRigging_Body, strTargetName),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);

					if (result)
					{
						object targetObj = SubMeshInGroup;
						if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
						{
							targetObj = SubMeshGroupInGroup;
						}

						apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_RemoveBoneRigging, Modifier._meshGroup, targetObj, false, Editor);

						if (SubMeshInGroup != null && selectedObj == SubMeshInGroup)
						{
							SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshInGroup);
						}
						else if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
						{
							SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshGroupInGroup);
						}
						else
						{
							//TODO : Bone 제거
						}

						Editor._portrait.LinkAndRefreshInEditor();
						AutoSelectModMeshOrModBone();

						Editor.Hierarchy_MeshGroup.RefreshUnits();

						Editor.SetRepaint();
					}
				}
			}
			else if (!isAddable)
			{
				//추가 가능하지 않다.
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				GUILayout.Box("[" + strTargetName + "]\nNot able to be Added", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				//아직 추가하지 않았다. 추가하자
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				GUILayout.Box("[" + strTargetName + "]\nNot Added to Edit", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (GUILayout.Button("Add Rigging", GUILayout.Width(width), GUILayout.Height(30)))
				{
					Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();

					Editor.Hierarchy_MeshGroup.RefreshUnits();

					Editor.SetRepaint();
				}
			}
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			List<ModRenderVert> selectedVerts = Editor.Select.ModRenderVertListOfMod;
			bool isAnyVertSelected = (selectedVerts != null && selectedVerts.Count > 0);


			//2. Weight 툴
			// 선택한 Vertex
			// Auto Normalize
			// Set Weight, +/- Weight, * Weight
			// Blend, Auto Rigging, Normalize, Prune,
			// Copy / Paste
			// Bone (Color, Remove)

			//어떤 Vertex가 선택되었는지 표기한다.

			_rigEdit_vertRigDataList.Clear();

			if (!isAnyTargetSelected)
			{
				//선택된게 없다.
				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				GUILayout.Box("No Vetex is Selected", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;


			}
			else if (selectedVerts.Count == 1)
			{
				//1개의 Vertex
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				GUILayout.Box("[Vertex " + selectedVerts[0]._renderVert._vertex._index + "] is Selected", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;

			}
			else
			{
				GUI.backgroundColor = new Color(0.4f, 0.5f, 1.0f, 1.0f);
				GUILayout.Box(selectedVerts.Count + " Verts are Selected", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;
			}
			int nSelectedVerts = 0;
			if (isAnyTargetSelected)
			{
				nSelectedVerts = selectedVerts.Count;

				//리스트에 넣을 Rig 리스트를 완성하자
				for (int i = 0; i < selectedVerts.Count; i++)
				{
					apModifiedVertexRig modVertRig = selectedVerts[i]._modVertRig;
					if (modVertRig == null)
					{
						// -ㅅ-?
						continue;
					}
					for (int iPair = 0; iPair < modVertRig._weightPairs.Count; iPair++)
					{
						apModifiedVertexRig.WeightPair pair = modVertRig._weightPairs[iPair];
						VertRigData sameBoneData = _rigEdit_vertRigDataList.Find(delegate (VertRigData a)
						{
							return a._bone == pair._bone;
						});
						if (sameBoneData != null)
						{
							sameBoneData.AddRig(pair._weight);
						}
						else
						{
							_rigEdit_vertRigDataList.Add(new VertRigData(pair._bone, pair._weight));
						}
					}
				}
			}


			// 기본 토대는 3ds Max와 유사하게 가자

			// Edit가 활성화되지 않으면 버튼 선택불가
			bool isBtnAvailable = _rigEdit_isBindingEdit;

			int CALCULATE_SET = 0;
			int CALCULATE_ADD = 1;
			int CALCULATE_MULTIPLY = 2;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			//고정된 Weight 값
			//0, 0.1, 0.3, 0.5, 0.7, 0.9, 1 (7개)
			int widthPresetWeight = ((width - 2 * 7) / 7) - 2;
			bool isPresetAdapt = false;
			float presetWeight = 0.0f;
			if (apEditorUtil.ToggledButton("0", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.0f;
			}
			if (apEditorUtil.ToggledButton(".1", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.1f;
			}
			if (apEditorUtil.ToggledButton(".3", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.3f;
			}
			if (apEditorUtil.ToggledButton(".5", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.5f;
			}
			if (apEditorUtil.ToggledButton(".7", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.7f;
			}
			if (apEditorUtil.ToggledButton(".9", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.9f;
			}
			if (apEditorUtil.ToggledButton("1", false, isBtnAvailable, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 1f;
			}
			EditorGUILayout.EndHorizontal();

			if (isPresetAdapt)
			{
				Editor.Controller.SetBoneWeight(presetWeight, CALCULATE_SET);
			}

			int heightSetWeight = 25;
			int widthSetBtn = 90;
			int widthIncDecBtn = 30;
			int widthValue = width - (widthSetBtn + widthIncDecBtn * 2 + 2 * 5 + 5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightSetWeight));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(widthValue), GUILayout.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_rigEdit_setWeightValue = EditorGUILayout.DelayedFloatField(_rigEdit_setWeightValue);
			EditorGUILayout.EndVertical();

			if (apEditorUtil.ToggledButton("Set Weight", false, isBtnAvailable, widthSetBtn, heightSetWeight))
			{
				//Debug.LogError("TODO : Weight 적용 - Set");
				Editor.Controller.SetBoneWeight(_rigEdit_setWeightValue, CALCULATE_SET);
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton("+", false, isBtnAvailable, widthIncDecBtn, heightSetWeight))
			{
				////0.05 단위로 올라가거나 내려온다. (5%)
				////현재 값에서 "int형 반올림"을 수행하고 처리
				//_rigEdit_setWeightValue = Mathf.Clamp01((float)((int)(_rigEdit_setWeightValue * 20.0f + 0.5f) + 1) / 20.0f);
				//이게 아니었다..
				//0.05 추가
				Editor.Controller.SetBoneWeight(0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton("-", false, isBtnAvailable, widthIncDecBtn, heightSetWeight))
			{
				//0.05 단위로 올라가거나 내려온다. (5%)
				//현재 값에서 "int형 반올림"을 수행하고 처리
				//_rigEdit_setWeightValue = Mathf.Clamp01((float)((int)(_rigEdit_setWeightValue * 20.0f + 0.5f) - 1) / 20.0f);
				//0.05 빼기
				Editor.Controller.SetBoneWeight(-0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightSetWeight));
			GUILayout.Space(5);


			EditorGUILayout.BeginVertical(GUILayout.Width(widthValue), GUILayout.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_rigEdit_scaleWeightValue = EditorGUILayout.DelayedFloatField(_rigEdit_scaleWeightValue);
			EditorGUILayout.EndVertical();

			if (apEditorUtil.ToggledButton("Scale Weight", false, isBtnAvailable, widthSetBtn, heightSetWeight))
			{
				//Debug.LogError("TODO : Weight 적용 - Set");
				Editor.Controller.SetBoneWeight(_rigEdit_scaleWeightValue, CALCULATE_MULTIPLY);//Multiply 방식
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton("+", false, isBtnAvailable, widthIncDecBtn, heightSetWeight))
			{
				//0.01 단위로 올라가거나 내려온다. (1%)
				//현재 값에서 반올림을 수행하고 처리
				//Scale은 Clamp가 걸리지 않는다.
				//_rigEdit_scaleWeightValue = (float)((int)(_rigEdit_scaleWeightValue * 100.0f + 0.5f) + 1) / 100.0f;
				//x1.05
				Editor.Controller.SetBoneWeight(1.05f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton("-", false, isBtnAvailable, widthIncDecBtn, heightSetWeight))
			{
				//0.01 단위로 올라가거나 내려온다. (1%)
				//현재 값에서 반올림을 수행하고 처리
				//_rigEdit_scaleWeightValue = (float)((int)(_rigEdit_scaleWeightValue * 100.0f + 0.5f) - 1) / 100.0f;
				//if(_rigEdit_scaleWeightValue < 0.0f)
				//{
				//	_rigEdit_scaleWeightValue = 0.0f;
				//}
				//x0.95
				Editor.Controller.SetBoneWeight(0.95f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(8);

			int heightToolBtn = 25;
			int width4Btn = ((width - 5) / 4) - (2);

			//Blend, Prune, Normalize, Auto Rigging
			//Normalize On/Off
			//Copy / Paste

			int width2Btn = (width - 5) / 2;

			//Auto Rigging
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_AutoNormalize), "  Auto Normalize", "  Auto Normalize", _rigEdit_isAutoNormalize, isBtnAvailable, width, 28))
			{
				_rigEdit_isAutoNormalize = !_rigEdit_isAutoNormalize;

				//Off -> On 시에 Normalize를 적용하자
				if (_rigEdit_isAutoNormalize)
				{
					Editor.Controller.SetBoneWeightNormalize();
				}
				//Auto Normalize는 에디터 옵션으로 저장된다.
				Editor.SaveEditorPref();
			}


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Blend), " Blend", false, isBtnAvailable, width2Btn, heightToolBtn))
			{
				//Blend
				Editor.Controller.SetBoneWeightBlend();
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Normalize), " Normalize", false, isBtnAvailable, width2Btn, heightToolBtn))
			{
				//Normalize
				Editor.Controller.SetBoneWeightNormalize();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Prune), " Prune", false, isBtnAvailable, width2Btn, heightToolBtn))
			{
				//Prune
				Editor.Controller.SetBoneWeightPrune();
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Auto), " Auto Rig", false, isBtnAvailable, width2Btn, heightToolBtn))
			{
				//Auto
				Editor.Controller.SetBoneAutoRig();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Grow), " Grow", false, isBtnAvailable, width2Btn, heightToolBtn))
			{
				Editor.Controller.SelectVertexRigGrowOrShrink(true);
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Shrink), " Shrink", false, isBtnAvailable, width2Btn, heightToolBtn))
			{
				Editor.Controller.SelectVertexRigGrowOrShrink(false);
			}
			EditorGUILayout.EndHorizontal();


			bool isCopyAvailable = isBtnAvailable && selectedVerts.Count == 1;
			bool isPasteAvailable = false;
			if (isCopyAvailable)
			{
				if (apSnapShotManager.I.IsPastable(selectedVerts[0]._modVertRig))
				{
					isPasteAvailable = true;
				}
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy), " Copy", false, isCopyAvailable, width2Btn, heightToolBtn))
			{
				//Copy	
				apSnapShotManager.I.Copy_VertRig(selectedVerts[0]._modVertRig, "Mod Vert Rig");
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste), " Paste", false, isPasteAvailable, width2Btn, heightToolBtn))
			{
				//Paste
				if (apSnapShotManager.I.Paste_VertRig(selectedVerts[0]._modVertRig))
				{
					MeshGroup.RefreshForce();
				}
			}
			EditorGUILayout.EndHorizontal();
			//이제 리스트를 불러오자

			int nRigDataList = _rigEdit_vertRigDataList.Count;
			if (_riggingModifier_prevNumBoneWeights != nRigDataList)
			{
				Editor.SetGUIVisible("Rig Mod - RigDataCount Refreshed", true);
				if (Editor.IsDelayedGUIVisible("Rig Mod - RigDataCount Refreshed"))
				{
					_riggingModifier_prevNumBoneWeights = nRigDataList;
				}
			}
			else
			{
				Editor.SetGUIVisible("Rig Mod - RigDataCount Refreshed", false);
			}

			//if(selectedVerts)
			//List<apModifiedVertexRig> vertRigList = 
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(200));
			GUILayout.Space(5);

			Rect lastRect = GUILayoutUtility.GetLastRect();

			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

			GUI.Box(new Rect(lastRect.x + 5, lastRect.y, width, 200), "");
			GUI.backgroundColor = prevColor;


			//Weight 리스트를 출력하자
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(200));
			_scrollBottom_Status = EditorGUILayout.BeginScrollView(_scrollBottom_Status, false, true);
			GUILayout.Space(2);
			int scrollWidth = width - (30);
			EditorGUILayout.BeginVertical(GUILayout.Width(scrollWidth), GUILayout.Height(200));
			GUILayout.Space(3);

			Texture2D imgRemove = Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey);

			VertRigData vertRigData = null;
			string strLabel = "";

			VertRigData removeRigData = null;
			int widthLabel_Name = scrollWidth - (5 + 25 + 14 + 2 + 60);
			//for (int i = 0; i < _rigEdit_vertRigDataList.Count; i++)
			for (int i = 0; i < _riggingModifier_prevNumBoneWeights; i++)
			{
				if (i < _rigEdit_vertRigDataList.Count)
				{
					vertRigData = _rigEdit_vertRigDataList[i];
					if (vertRigData._bone == Bone)
					{
						lastRect = GUILayoutUtility.GetLastRect();
						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

						int offsetHeight = 18 + 3;
						if (i == 0)
						{
							offsetHeight = 1 + 3;
						}

						GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 20), "");
						GUI.backgroundColor = prevColor;
					}
					EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
					GUILayout.Space(5);

					//Bone의 색상, 이름, Weight, X를 출력
					GUI.backgroundColor = vertRigData._bone._color;
					GUILayout.Box("", GUILayout.Width(14), GUILayout.Height(14));
					GUI.backgroundColor = prevColor;


					if (nSelectedVerts > 1 && (vertRigData._weight_Max - vertRigData._weight_Min) > 0.01f)
					{
						//여러개가 섞여서 Weight가 의미가 없어졌다.
						//Min + 로 표현하자
						int iMin = (int)vertRigData._weight_Min;
						int iMax = (int)vertRigData._weight_Max;
						int iMin_Float = (int)(vertRigData._weight_Min * 10.0f + 0.5f) % 10;
						int iMax_Float = (int)(vertRigData._weight_Max * 10.0f + 0.5f) % 10;

						strLabel = string.Format("{0:N2}~{1:N2}", vertRigData._weight_Min, vertRigData._weight_Max);
						//strLabel = ((int)vertRigData._weight_Min) + "." + ((int)(vertRigData._weight_Min * 10.0f + 0.5f) % 10)
						//	+ "~" + ((int)vertRigData._weight_Max) + "." + ((int)(vertRigData._weight_Max * 10.0f + 0.5f) % 10);
					}
					else
					{
						//Weight를 출력한다.
						//strLabel = ((int)vertRigData._weight) + "." + ((int)(vertRigData._weight * 1000.0f + 0.5f) % 1000);
						strLabel = string.Format("{0:N3}", vertRigData._weight);
					}

					string rigName = vertRigData._bone._name;
					if (rigName.Length > 14)
					{
						rigName = rigName.Substring(0, 12) + "..";
					}
					if (GUILayout.Button(rigName,
										guiNone,
										GUILayout.Width(widthLabel_Name), GUILayout.Height(20)))
					{
						Editor.Select.SetBone(vertRigData._bone);
					}
					if (GUILayout.Button(strLabel,
										guiNone,
										GUILayout.Width(60), GUILayout.Height(20)))
					{
						Editor.Select.SetBone(vertRigData._bone);
					}

					if (GUILayout.Button(imgRemove, guiNone, GUILayout.Width(20), GUILayout.Height(20)))
					{
						//Debug.LogError("TODO : Bone Remove From Rigging");
						removeRigData = vertRigData;
					}

					EditorGUILayout.EndHorizontal();
				}
				else
				{
					//GUI 렌더 문제로 더미 렌더링
					EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth - 5));
					GUILayout.Space(5);

					GUILayout.Box("", GUILayout.Width(14), GUILayout.Height(14));

					if (GUILayout.Button("",
										guiNone,
										GUILayout.Width(widthLabel_Name), GUILayout.Height(20)))
					{
						//Dummy
					}
					if (GUILayout.Button("",
										guiNone,
										GUILayout.Width(60), GUILayout.Height(20)))
					{
						//Dummy
					}

					if (GUILayout.Button(imgRemove, guiNone, GUILayout.Width(20), GUILayout.Height(20)))
					{
						//Debug.LogError("TODO : Bone Remove From Rigging");
						//removeRigData = vertRigData;
						//Dummy
					}


					EditorGUILayout.EndHorizontal();
				}
			}


			if (removeRigData != null)
			{
				Editor.Controller.RemoveVertRigData(selectedVerts, removeRigData._bone);
			}

			EditorGUILayout.EndVertical();

			GUILayout.Space(120);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}


		private object _physicModifier_prevSelectedTransform = null;
		private bool _physicModifier_prevIsContained = false;

		private void DrawModifierPropertyGUI_Physics(int width, int height)
		{
			GUIStyle guiNone = new GUIStyle(GUIStyle.none);
			guiNone.normal.textColor = GUI.skin.label.normal.textColor;
			guiNone.alignment = TextAnchor.MiddleLeft;

			EditorGUILayout.LabelField("Target Mesh Transform", GUILayout.Width(width));
			//1. Mesh Transform 등록 체크
			//2. Weight 툴
			//3. Mesh Physics 툴

			bool isTarget_MeshTransform = Modifier.IsTarget_MeshTransform;
			bool isTarget_ChildMeshTransform = Modifier.IsTarget_ChildMeshTransform;

			bool isContainInParamSetGroup = false;
			string strTargetName = "";
			object selectedObj = null;
			bool isAnyTargetSelected = false;
			bool isAddable = false;

			apTransform_Mesh targetMeshTransform = SubMeshInGroup;
			apModifierParamSetGroup paramSetGroup = SubEditedParamSetGroup;
			if (paramSetGroup == null)
			{
				//? Physics에서는 1개의 ParamSetGroup이 있어야 한다.
				Editor.Controller.AddStaticParamSetGroupToModifier();

				if (Modifier._paramSetGroup_controller.Count > 0)
				{
					SetParamSetGroupOfModifier(Modifier._paramSetGroup_controller[0]);
				}
				paramSetGroup = SubEditedParamSetGroup;
				if (paramSetGroup == null)
				{
					Debug.LogError("ParamSet Group Is Null (" + Modifier._paramSetGroup_controller.Count + ")");
					return;
				}

				AutoSelectModMeshOrModBone();
			}

			apModifierParamSet paramSet = ParamSetOfMod;
			if (paramSet == null)
			{
				//Rigging에서는 1개의 ParamSetGroup과 1개의 ParamSet이 있어야 한다.
				//선택된게 없다면, ParamSet이 1개 있는지 확인
				//그후 선택한다.

				if (paramSetGroup._paramSetList.Count == 0)
				{
					paramSet = new apModifierParamSet();
					paramSet.LinkParamSetGroup(paramSetGroup);
					paramSetGroup._paramSetList.Add(paramSet);
				}
				else
				{
					paramSet = paramSetGroup._paramSetList[0];
				}
				SetParamSetOfModifier(paramSet);
			}

			//1. Mesh Transform 등록 체크
			if (targetMeshTransform != null)
			{
				apRenderUnit targetRenderUnit = null;
				//Child Mesh를 허용하는가
				if (isTarget_ChildMeshTransform)
				{
					//Child를 허용한다.
					targetRenderUnit = MeshGroup.GetRenderUnit(targetMeshTransform);
				}
				else
				{
					//Child를 허용하지 않는다.
					targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(targetMeshTransform);
				}
				if (targetRenderUnit != null)
				{
					//유효한 선택인 경우
					isContainInParamSetGroup = paramSetGroup.IsMeshTransformContain(targetMeshTransform);
					isAnyTargetSelected = true;
					strTargetName = targetMeshTransform._nickName;
					selectedObj = targetMeshTransform;

					isAddable = true;
				}
			}

			//if (Event.current.type == EventType.Layout
			//	|| Event.current.type == EventType.Repaint
			//	)
			//{
			//	_physicModifier_prevSelectedTransform = targetMeshTransform;
			//	_physicModifier_prevIsContained = isContainInParamSetGroup;
			//}

			//bool isSameSetting = (targetMeshTransform == _physicModifier_prevSelectedTransform)
			//					&& (isContainInParamSetGroup == _physicModifier_prevIsContained);


			Editor.SetGUIVisible("Modifier_Add Transform Check [Physic] Valid", targetMeshTransform != null);
			Editor.SetGUIVisible("Modifier_Add Transform Check [Physic] Invalid", targetMeshTransform == null);


			bool isMeshTransformValid = Editor.IsDelayedGUIVisible("Modifier_Add Transform Check [Physic] Valid");
			bool isMeshTransformInvalid = Editor.IsDelayedGUIVisible("Modifier_Add Transform Check [Physic] Invalid");

			//if (!Editor.IsDelayedGUIVisible("Modifier_Add Transform Check [Physic]"))
			//{
			//	Debug.Log("Physic Not Same Setting - [" + Event.current.type + "]");
			//	return;
			//}

			bool isDummyTransform = false;

			if (!isMeshTransformValid && !isMeshTransformInvalid)
			{
				//둘중 하나는 true여야 GUI를 그릴 수 있다.
				//Debug.Log("Physic Not Same Setting - [" + Event.current.type + "]");
				isDummyTransform = true;//<<더미로 출력해야한다...
										//return;
			}
			else
			{
				_physicModifier_prevSelectedTransform = targetMeshTransform;
				_physicModifier_prevIsContained = isContainInParamSetGroup;
			}



			Color prevColor = GUI.backgroundColor;

			GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
			boxGUIStyle.alignment = TextAnchor.MiddleCenter;

			if (targetMeshTransform == null)
			{
				//선택된 MeshTransform이 없다.
				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				GUILayout.Box("No Mesh is Selected", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (isDummyTransform)
				{
					if (GUILayout.Button("Add Physics", GUILayout.Width(width), GUILayout.Height(25)))
					{
						//더미용 버튼
					}
				}
			}
			else if (isContainInParamSetGroup)
			{
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				GUILayout.Box("[" + strTargetName + "]\nSelected", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (!isDummyTransform)
				{
					//더미 처리 중이 아닐때 버튼이 등장한다.
					if (GUILayout.Button("Remove From Physics", GUILayout.Width(width), GUILayout.Height(30)))
					{

						//bool result = EditorUtility.DisplayDialog("Remove From Physics", "Remove From Physics [" + strTargetName + "]", "Remove", "Cancel");

						bool result = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveFromPhysics_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveFromPhysics_Body, strTargetName),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);

						if (result)
						{
							object targetObj = SubMeshInGroup;
							if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
							{
								targetObj = SubMeshGroupInGroup;
							}

							apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_RemovePhysics, Modifier._meshGroup, targetObj, false, Editor);

							if (SubMeshInGroup != null && selectedObj == SubMeshInGroup)
							{
								SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshInGroup);
								SetModMeshOfModifier(null);
							}
							else if (SubMeshGroupInGroup != null && selectedObj == SubMeshGroupInGroup)
							{
								SubEditedParamSetGroup.RemoveModifierMeshes(SubMeshGroupInGroup);
								SetModMeshOfModifier(null);

							}



							if (MeshGroup != null)
							{
								MeshGroup.RefreshModifierLink();
							}

							SetSubMeshGroupInGroup(null);
							SetSubMeshInGroup(null);

							Editor._portrait.LinkAndRefreshInEditor();
							AutoSelectModMeshOrModBone();

							SetModifierExclusiveEditing(EX_EDIT.None);

							if (ModMeshOfMod != null)
							{
								ModMeshOfMod.RefreshVertexWeights(Editor._portrait, true, false);
							}

							Editor.Hierarchy_MeshGroup.RefreshUnits();


							Editor.SetRepaint();

							isContainInParamSetGroup = false;
						}
					}
				}
			}
			else if (!isAddable)
			{
				//추가 가능하지 않다.
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				GUILayout.Box("[" + strTargetName + "]\nNot able to be Added", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (isDummyTransform)
				{
					if (GUILayout.Button("Add Physics", GUILayout.Width(width), GUILayout.Height(25)))
					{
						//더미용 버튼
					}
				}
			}
			else
			{
				//아직 추가하지 않았다. 추가하자
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				GUILayout.Box("[" + strTargetName + "]\nNot Added to Edit", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(35));

				GUI.backgroundColor = prevColor;

				if (!isDummyTransform)
				{
					//더미 처리 중이 아닐때 버튼이 등장한다.
					if (GUILayout.Button("Add Physics", GUILayout.Width(width), GUILayout.Height(30)))
					{
						Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();

						Editor.Hierarchy_MeshGroup.RefreshUnits();

						Editor.SetRepaint();
					}
				}
			}
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			List<ModRenderVert> selectedVerts = Editor.Select.ModRenderVertListOfMod;
			bool isAnyVertSelected = (selectedVerts != null && selectedVerts.Count > 0);

			bool isExEditMode = ExEditingMode != EX_EDIT.None;

			//2. Weight 툴
			// 선택한 Vertex
			// Set Weight, +/- Weight, * Weight
			// Blend
			// Grow, Shrink

			//어떤 Vertex가 선택되었는지 표기한다.
			if (!isAnyTargetSelected || selectedVerts.Count == 0)
			{
				//선택된게 없다.
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				GUILayout.Box("No Vetex is Selected", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;


			}
			else if (selectedVerts.Count == 1)
			{
				//1개의 Vertex
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				GUILayout.Box("[Vertex " + selectedVerts[0]._renderVert._vertex._index + "] : " + selectedVerts[0]._modVertWeight._weight, boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;

			}
			else
			{
				GUI.backgroundColor = new Color(0.4f, 1.0f, 1.0f, 1.0f);
				GUILayout.Box(selectedVerts.Count + " Verts are Selected", boxGUIStyle, GUILayout.Width(width), GUILayout.Height(25));

				GUI.backgroundColor = prevColor;
			}
			int nSelectedVerts = selectedVerts.Count;

			bool isMainVert = false;
			bool isMainVertSwitchable = false;
			if (nSelectedVerts == 1)
			{
				if (selectedVerts[0]._modVertWeight._isEnabled)
				{
					isMainVert = selectedVerts[0]._modVertWeight._physicParam._isMain;
					isMainVertSwitchable = true;
				}
			}
			else if (nSelectedVerts > 1)
			{
				//전부다 MainVert인가
				bool isAllMainVert = true;
				for (int iVert = 0; iVert < selectedVerts.Count; iVert++)
				{
					if (!selectedVerts[iVert]._modVertWeight._physicParam._isMain)
					{
						isAllMainVert = false;
						break;
					}
				}
				isMainVert = isAllMainVert;
				isMainVertSwitchable = true;
			}
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Physic_SetMainVertex),
												" Important Vertex", " Set Important",
												isMainVert, isMainVertSwitchable && isExEditMode, width, 25))
			{
				if (isMainVertSwitchable)
				{
					for (int i = 0; i < selectedVerts.Count; i++)
					{
						selectedVerts[i]._modVertWeight._physicParam._isMain = !isMainVert;
					}

					ModMeshOfMod.RefreshVertexWeights(Editor._portrait, true, false);
				}
			}

			//Weight Tool
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			//고정된 Weight 값
			//0, 0.1, 0.3, 0.5, 0.7, 0.9, 1 (7개)
			int CALCULATE_SET = 0;
			int CALCULATE_ADD = 1;
			int CALCULATE_MULTIPLY = 2;

			int widthPresetWeight = ((width - 2 * 7) / 7) - 2;
			bool isPresetAdapt = false;
			float presetWeight = 0.0f;

			if (apEditorUtil.ToggledButton("0", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.0f;
			}
			if (apEditorUtil.ToggledButton(".1", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.1f;
			}
			if (apEditorUtil.ToggledButton(".3", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.3f;
			}
			if (apEditorUtil.ToggledButton(".5", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.5f;
			}
			if (apEditorUtil.ToggledButton(".7", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.7f;
			}
			if (apEditorUtil.ToggledButton(".9", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 0.9f;
			}
			if (apEditorUtil.ToggledButton("1", false, isExEditMode, widthPresetWeight, 30))
			{
				isPresetAdapt = true;
				presetWeight = 1f;
			}
			EditorGUILayout.EndHorizontal();

			if (isPresetAdapt)
			{
				//고정 Weight를 지정하자
				Editor.Controller.SetPhyVolWeight(presetWeight, CALCULATE_SET);
				isPresetAdapt = false;
			}



			int heightSetWeight = 25;
			int widthSetBtn = 90;
			int widthIncDecBtn = 30;
			int widthValue = width - (widthSetBtn + widthIncDecBtn * 2 + 2 * 5 + 5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightSetWeight));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(widthValue), GUILayout.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_physics_setWeightValue = EditorGUILayout.DelayedFloatField(_physics_setWeightValue);
			EditorGUILayout.EndVertical();

			if (apEditorUtil.ToggledButton("Set Weight", false, isExEditMode, widthSetBtn, heightSetWeight))
			{
				Editor.Controller.SetPhyVolWeight(_physics_setWeightValue, CALCULATE_SET);
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton("+", false, isExEditMode, widthIncDecBtn, heightSetWeight))
			{
				////0.05 단위로 올라가거나 내려온다. (5%)
				Editor.Controller.SetPhyVolWeight(0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton("-", false, isExEditMode, widthIncDecBtn, heightSetWeight))
			{
				//0.05 단위로 올라가거나 내려온다. (5%)
				Editor.Controller.SetPhyVolWeight(-0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightSetWeight));
			GUILayout.Space(5);


			EditorGUILayout.BeginVertical(GUILayout.Width(widthValue), GUILayout.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_physics_scaleWeightValue = EditorGUILayout.DelayedFloatField(_physics_scaleWeightValue);
			EditorGUILayout.EndVertical();

			if (apEditorUtil.ToggledButton("Scale Weight", false, isExEditMode, widthSetBtn, heightSetWeight))
			{
				Editor.Controller.SetPhyVolWeight(_physics_scaleWeightValue, CALCULATE_MULTIPLY);//Multiply 방식
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton("+", false, isExEditMode, widthIncDecBtn, heightSetWeight))
			{
				//x1.05
				//Debug.LogError("TODO : Physic Weight 적용 - x1.05");
				Editor.Controller.SetPhyVolWeight(1.05f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton("-", false, isExEditMode, widthIncDecBtn, heightSetWeight))
			{
				//x0.95
				//Debug.LogError("TODO : Physic Weight 적용 - x0.95");
				//Editor.Controller.SetBoneWeight(0.95f, CALCULATE_MULTIPLY);//Multiply 방식
				Editor.Controller.SetPhyVolWeight(0.95f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(8);

			int heightToolBtn = 25;
			int width2Btn = (width - 5) / 2;
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Blend), " Blend", false, isExEditMode, width, heightToolBtn))
			{
				//Blend
				Editor.Controller.SetPhyVolWeightBlend();
			}

			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightToolBtn));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Grow), " Grow", false, isExEditMode, width2Btn, heightToolBtn))
			{
				//Grow
				Editor.Controller.SelectVertexWeightGrowOrShrink(true);
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Shrink), " Shrink", false, isExEditMode, width2Btn, heightToolBtn))
			{
				//Shrink
				Editor.Controller.SelectVertexWeightGrowOrShrink(false);
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

			//추가
			//Viscosity를 위한 그룹
			int viscosityGroupID = 0;
			bool isViscosityAvailable = false;
			if (isExEditMode && nSelectedVerts > 0)
			{
				for (int i = 0; i < selectedVerts.Count; i++)
				{
					viscosityGroupID |= selectedVerts[i]._modVertWeight._physicParam._viscosityGroupID;
				}
				isViscosityAvailable = true;
			}
			int iViscosityChanged = -1;
			bool isViscosityAdd = false;

			int heightVisTool = 20;
			int widthVisTool = ((width - 5) / 5) - 2;

			//5줄씩 총 10개 (0은 모두 0으로 만든다.)

			EditorGUILayout.LabelField("Viscostiy Group ID");
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightVisTool));
			GUILayout.Space(5);
			for (int i = 0; i < 10; i++)
			{
				if (i == 5)
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(heightVisTool));
					GUILayout.Space(5);
				}


				string label = "";
				int iResult = 0;
				switch (i)
				{
					case 0:
						label = "X";
						iResult = 0;
						break;
					case 1:
						label = "1";
						iResult = 1;
						break;
					case 2:
						label = "2";
						iResult = 2;
						break;
					case 3:
						label = "3";
						iResult = 4;
						break;
					case 4:
						label = "4";
						iResult = 8;
						break;
					case 5:
						label = "5";
						iResult = 16;
						break;
					case 6:
						label = "6";
						iResult = 32;
						break;
					case 7:
						label = "7";
						iResult = 64;
						break;
					case 8:
						label = "8";
						iResult = 128;
						break;
					case 9:
						label = "9";
						iResult = 256;
						break;
				}
				bool isSelected = (viscosityGroupID & iResult) != 0;
				if (apEditorUtil.ToggledButton_2Side(label, label, isSelected, isViscosityAvailable, widthVisTool, heightVisTool))
				{
					iViscosityChanged = iResult;
					isViscosityAdd = !isSelected;
				}
			}
			EditorGUILayout.EndHorizontal();

			if (iViscosityChanged > -1)
			{
				Editor.Controller.SetPhysicsViscostyGroupID(iViscosityChanged, isViscosityAdd);
			}



			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			//메시 설정
			apPhysicsMeshParam physicMeshParam = null;
			if (ModMeshOfMod != null && ModMeshOfMod.PhysicParam != null)
			{
				physicMeshParam = ModMeshOfMod.PhysicParam;
			}
			if ((physicMeshParam == null && !isDummyTransform)
				|| (physicMeshParam != null && isDummyTransform))
			{
				//Mesh도 없고, Dummy도 없으면..
				//또는 Mesh가 있는데도 Dummy 판정이 났다면.. 
				return;
			}

			//여기서부턴 Dummy가 있으면 그 값을 이용한다.
			if (physicMeshParam != null)
			{
				isDummyTransform = false;
			}

			if (isDummyTransform && (_physicModifier_prevSelectedTransform == null || !_physicModifier_prevIsContained))
			{
				return;
			}

			int labelHeight = 30;

			apPhysicsPresetUnit presetUnit = null;
			if (!isDummyTransform)
			{
				if (physicMeshParam._presetID >= 0)
				{
					presetUnit = Editor.PhysicsPreset.GetPresetUnit(physicMeshParam._presetID);
					if (presetUnit == null)
					{
						physicMeshParam._presetID = -1;
					}
				}
			}
			//EditorGUILayout.LabelField("Physical Material");
			GUIStyle guiStyle_BoxStyle = new GUIStyle(GUI.skin.box);
			guiStyle_BoxStyle.alignment = TextAnchor.MiddleCenter;
			if (presetUnit != null)
			{
				bool isPropertySame = presetUnit.IsSameProperties(physicMeshParam);
				if (isPropertySame)
				{
					GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				}
				else
				{
					GUI.backgroundColor = new Color(0.4f, 1.0f, 1.1f, 1.0f);
				}

				GUILayout.Box(
					new GUIContent("  " + presetUnit._name,
									Editor.ImageSet.Get(apEditorUtil.GetPhysicsPresetIconType(presetUnit._icon))),
					guiStyle_BoxStyle, GUILayout.Width(width), GUILayout.Height(30));

				GUI.backgroundColor = prevColor;
			}
			else
			{

				GUILayout.Box("Physical Material", guiStyle_BoxStyle, GUILayout.Width(width), GUILayout.Height(30));
			}

			GUILayout.Space(5);
			//TODO : Preset
			//값이 바뀌었으면 Dirty

			EditorGUILayout.LabelField(new GUIContent("  Basic Setting", Editor.ImageSet.Get(apImageSet.PRESET.Physic_BasicSetting)), GUILayout.Height(labelHeight));

			float nextMass = EditorGUILayout.DelayedFloatField("Mass", (!isDummyTransform) ? physicMeshParam._mass : 0.0f);
			float nextDamping = EditorGUILayout.DelayedFloatField("Damping", (!isDummyTransform) ? physicMeshParam._damping : 0.0f);
			float nextAirDrag = EditorGUILayout.DelayedFloatField("Air Drag", (!isDummyTransform) ? physicMeshParam._airDrag : 0.0f);
			bool nextIsRestrictMoveRange = EditorGUILayout.Toggle("Set Move Range", (!isDummyTransform) ? physicMeshParam._isRestrictMoveRange : false);
			float nextMoveRange = (!isDummyTransform) ? physicMeshParam._moveRange : 0.0f;
			if (nextIsRestrictMoveRange)
			{
				nextMoveRange = EditorGUILayout.DelayedFloatField("Move Range", (!isDummyTransform) ? physicMeshParam._moveRange : 0.0f);
			}
			else
			{
				EditorGUILayout.LabelField("Move Range : Unlimited");
			}

			GUILayout.Space(5);

			int valueWidth = 74;//캬... 꼼꼼하다
			int labelWidth = width - (valueWidth + 2 + 5);
			int leftMargin = 3;
			int topMargin = 10;


			EditorGUILayout.LabelField(new GUIContent("  Stretchiness", Editor.ImageSet.Get(apImageSet.PRESET.Physic_Stretch)), GUILayout.Height(labelHeight));
			EditorGUILayout.BeginVertical(GUILayout.Width(valueWidth), GUILayout.Height(labelHeight));
			float nextStretchK = EditorGUILayout.DelayedFloatField("K-Value", (!isDummyTransform) ? physicMeshParam._stretchK : 0.0f);
			bool nextIsRestrictStretchRange = EditorGUILayout.Toggle("Set Stretch Range", (!isDummyTransform) ? physicMeshParam._isRestrictStretchRange : false);
			float nextStretchRange_Max = (!isDummyTransform) ? physicMeshParam._stretchRangeRatio_Max : 0.0f;
			if (nextIsRestrictStretchRange)
			{
				nextStretchRange_Max = EditorGUILayout.DelayedFloatField("Lengthen Ratio", (!isDummyTransform) ? physicMeshParam._stretchRangeRatio_Max : 0.0f);
			}
			else
			{
				EditorGUILayout.LabelField("Lengthen Ratio : Unlimited");
			}
			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelHeight));
			GUILayout.Space(leftMargin);
			EditorGUILayout.LabelField(new GUIContent("  Inertia", Editor.ImageSet.Get(apImageSet.PRESET.Physic_Inertia)), GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
			EditorGUILayout.BeginVertical(GUILayout.Width(valueWidth), GUILayout.Height(labelHeight));
			GUILayout.Space(topMargin);
			float nextInertiaK = EditorGUILayout.DelayedFloatField((!isDummyTransform) ? physicMeshParam._inertiaK : 0.0f, GUILayout.Width(valueWidth));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelHeight));
			GUILayout.Space(leftMargin);
			EditorGUILayout.LabelField(new GUIContent("  Restoring", Editor.ImageSet.Get(apImageSet.PRESET.Physic_Recover)), GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
			EditorGUILayout.BeginVertical(GUILayout.Width(valueWidth), GUILayout.Height(labelHeight));
			GUILayout.Space(topMargin);
			float nextRestoring = EditorGUILayout.DelayedFloatField((!isDummyTransform) ? physicMeshParam._restoring : 0.0f, GUILayout.Width(valueWidth));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(labelHeight));
			GUILayout.Space(leftMargin);
			EditorGUILayout.LabelField(new GUIContent("  Viscosity", Editor.ImageSet.Get(apImageSet.PRESET.Physic_Viscosity)), GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
			EditorGUILayout.BeginVertical(GUILayout.Width(valueWidth), GUILayout.Height(labelHeight));
			GUILayout.Space(topMargin);
			float nextViscosity = EditorGUILayout.DelayedFloatField((!isDummyTransform) ? physicMeshParam._viscosity : 0.0f, GUILayout.Width(valueWidth));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

			//Bend는 삭제 -> Elastic을 추가한다.
			//EditorGUILayout.LabelField(new GUIContent("  Bendiness", Editor.ImageSet.Get(apImageSet.PRESET.Physic_Bend)), GUILayout.Height(labelHeight));
			//physicMeshParam._bendK = EditorGUILayout.DelayedFloatField("Constant", physicMeshParam._bendK);
			//physicMeshParam._bendRange = EditorGUILayout.DelayedFloatField("Range", physicMeshParam._bendRange);


			//값이 바뀌었으면 적용
			if (!isDummyTransform)
			{
				if (nextMass != physicMeshParam._mass
					|| nextDamping != physicMeshParam._damping
					|| nextAirDrag != physicMeshParam._airDrag
					|| nextMoveRange != physicMeshParam._moveRange
					|| nextStretchK != physicMeshParam._stretchK
					//|| nextStretchRange_Min != physicMeshParam._stretchRangeRatio_Min
					|| nextStretchRange_Max != physicMeshParam._stretchRangeRatio_Max
					|| nextInertiaK != physicMeshParam._inertiaK
					|| nextRestoring != physicMeshParam._restoring
					|| nextViscosity != physicMeshParam._viscosity
					|| nextIsRestrictStretchRange != physicMeshParam._isRestrictStretchRange
					|| nextIsRestrictMoveRange != physicMeshParam._isRestrictMoveRange)
				{
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, MeshGroup, ModMeshOfMod, false, Editor);
					physicMeshParam._mass = nextMass;
					physicMeshParam._damping = nextDamping;
					physicMeshParam._airDrag = nextAirDrag;
					physicMeshParam._moveRange = nextMoveRange;
					physicMeshParam._stretchK = nextStretchK;

					//physicMeshParam._stretchRangeRatio_Min = Mathf.Clamp01(nextStretchRange_Min);
					physicMeshParam._stretchRangeRatio_Max = nextStretchRange_Max;
					if (physicMeshParam._stretchRangeRatio_Max < 0.0f)
					{
						physicMeshParam._stretchRangeRatio_Max = 0.0f;
					}

					physicMeshParam._isRestrictStretchRange = nextIsRestrictStretchRange;
					physicMeshParam._isRestrictMoveRange = nextIsRestrictMoveRange;


					physicMeshParam._inertiaK = nextInertiaK;
					physicMeshParam._restoring = nextRestoring;
					physicMeshParam._viscosity = nextViscosity;

					apEditorUtil.ReleaseGUIFocus();
				}
			}

			//GUILayout.Space(5);

			EditorGUILayout.LabelField(new GUIContent("  Gravity", Editor.ImageSet.Get(apImageSet.PRESET.Physic_Gravity)), GUILayout.Height(labelHeight));
			EditorGUILayout.LabelField("Input Type");
			apPhysicsMeshParam.ExternalParamType nextGravityParam = (apPhysicsMeshParam.ExternalParamType)EditorGUILayout.EnumPopup((!isDummyTransform) ? physicMeshParam._gravityParamType : apPhysicsMeshParam.ExternalParamType.Constant);

			Vector2 nextGravityConstValue = (!isDummyTransform) ? physicMeshParam._gravityConstValue : Vector2.zero;

			apPhysicsMeshParam.ExternalParamType curGravityParam = (physicMeshParam != null) ? physicMeshParam._gravityParamType : apPhysicsMeshParam.ExternalParamType.Constant;

			if (curGravityParam == apPhysicsMeshParam.ExternalParamType.Constant)
			{
				nextGravityConstValue = apEditorUtil.DelayedVector2Field((!isDummyTransform) ? physicMeshParam._gravityConstValue : Vector2.zero, width - 4);
			}
			else
			{
				//?
				//TODO : GravityControlParam 링크할 것
				apControlParam controlParam = physicMeshParam._gravityControlParam;
				if (controlParam == null && physicMeshParam._gravityControlParamID > 0)
				{
					physicMeshParam._gravityControlParam = Editor._portrait._controller.FindParam(physicMeshParam._gravityControlParamID);
					controlParam = physicMeshParam._gravityControlParam;
					if (controlParam == null)
					{
						physicMeshParam._gravityControlParamID = -1;
					}
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				if (controlParam != null)
				{
					GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
					GUILayout.Box("[" + controlParam._keyName + "]", boxGUIStyle, GUILayout.Width(width - 34), GUILayout.Height(25));

					GUI.backgroundColor = prevColor;
				}
				else
				{
					GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
					GUILayout.Box("No ControlParam", boxGUIStyle, GUILayout.Width(width - 34), GUILayout.Height(25));

					GUI.backgroundColor = prevColor;
				}

				if (GUILayout.Button("Set", GUILayout.Width(30), GUILayout.Height(25)))
				{
					//Control Param을 선택하는 Dialog를 호출하자
					_loadKey_SelectControlParamToPhyGravity = apDialog_SelectControlParam.ShowDialog(Editor, apDialog_SelectControlParam.PARAM_TYPE.Vector2, OnSelectControlParamToPhysicGravity);
				}
				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(5);

			EditorGUILayout.LabelField(new GUIContent("  Wind", Editor.ImageSet.Get(apImageSet.PRESET.Physic_Wind)), GUILayout.Height(labelHeight));
			EditorGUILayout.LabelField("Input Type");
			apPhysicsMeshParam.ExternalParamType nextWindParamType = (apPhysicsMeshParam.ExternalParamType)EditorGUILayout.EnumPopup((!isDummyTransform) ? physicMeshParam._windParamType : apPhysicsMeshParam.ExternalParamType.Constant);

			Vector2 nextWindConstValue = (!isDummyTransform) ? physicMeshParam._windConstValue : Vector2.zero;
			Vector2 nextWindRandomRange = (!isDummyTransform) ? physicMeshParam._windRandomRange : Vector2.zero;

			apPhysicsMeshParam.ExternalParamType curWindParamType = (physicMeshParam != null) ? physicMeshParam._windParamType : apPhysicsMeshParam.ExternalParamType.Constant;

			if (curWindParamType == apPhysicsMeshParam.ExternalParamType.Constant)
			{
				nextWindConstValue = apEditorUtil.DelayedVector2Field((!isDummyTransform) ? physicMeshParam._windConstValue : Vector2.zero, width - 4);
			}
			else
			{
				//?
				//TODO : GravityControlParam 링크할 것
				apControlParam controlParam = physicMeshParam._windControlParam;
				if (controlParam == null && physicMeshParam._windControlParamID > 0)
				{
					physicMeshParam._windControlParam = Editor._portrait._controller.FindParam(physicMeshParam._windControlParamID);
					controlParam = physicMeshParam._windControlParam;
					if (controlParam == null)
					{
						physicMeshParam._windControlParamID = -1;
					}
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				if (controlParam != null)
				{
					GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
					GUILayout.Box("[" + controlParam._keyName + "]", boxGUIStyle, GUILayout.Width(width - 34), GUILayout.Height(25));

					GUI.backgroundColor = prevColor;
				}
				else
				{
					GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
					GUILayout.Box("No ControlParam", boxGUIStyle, GUILayout.Width(width - 34), GUILayout.Height(25));

					GUI.backgroundColor = prevColor;
				}

				if (GUILayout.Button("Set", GUILayout.Width(30), GUILayout.Height(25)))
				{
					//Control Param을 선택하는 Dialog를 호출하자
					_loadKey_SelectControlParamToPhyWind = apDialog_SelectControlParam.ShowDialog(Editor, apDialog_SelectControlParam.PARAM_TYPE.Vector2, OnSelectControlParamToPhysicWind);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.LabelField("Wind Random Range Size");
			nextWindRandomRange = apEditorUtil.DelayedVector2Field((!isDummyTransform) ? physicMeshParam._windRandomRange : Vector2.zero, width - 4);

			GUILayout.Space(10);




			//Preset 창을 열자
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Physic_Palette), " Physics Presets", " Physics Presets", false, physicMeshParam != null, width, 32))
			{
				_loadKey_SelectPhysicsParam = apDialog_PhysicsPreset.ShowDialog(Editor, ModMeshOfMod, OnSelectPhysicsPreset);
			}


			if (!isDummyTransform)
			{
				if (nextGravityParam != physicMeshParam._gravityParamType
					|| nextGravityConstValue.x != physicMeshParam._gravityConstValue.x
					|| nextGravityConstValue.y != physicMeshParam._gravityConstValue.y
					|| nextWindParamType != physicMeshParam._windParamType
					|| nextWindConstValue.x != physicMeshParam._windConstValue.x
					|| nextWindConstValue.y != physicMeshParam._windConstValue.y
					|| nextWindRandomRange.x != physicMeshParam._windRandomRange.x
					|| nextWindRandomRange.y != physicMeshParam._windRandomRange.y
					)
				{
					apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, MeshGroup, ModMeshOfMod, false, Editor);
					physicMeshParam._gravityParamType = nextGravityParam;
					physicMeshParam._gravityConstValue = nextGravityConstValue;
					physicMeshParam._windParamType = nextWindParamType;
					physicMeshParam._windConstValue = nextWindConstValue;
					physicMeshParam._windRandomRange = nextWindRandomRange;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
		}


		//Physic Modifier에서 Gravity/Wind를 Control Param에 연결할 때, Dialog를 열어서 선택하도록 한다.
		private object _loadKey_SelectControlParamToPhyGravity = null;
		public void OnSelectControlParamToPhysicGravity(bool isSuccess, object loadKey, apControlParam resultControlParam)
		{
			Debug.Log("Select Control Param : OnSelectControlParamToPhysicGravity (" + isSuccess + ")");
			if (_loadKey_SelectControlParamToPhyGravity != loadKey || !isSuccess)
			{
				Debug.LogError("잘못된 loadKey");
				_loadKey_SelectControlParamToPhyGravity = null;
				return;
			}

			_loadKey_SelectControlParamToPhyGravity = null;
			if (Modifier == null
				|| (Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) == 0
				|| ModMeshOfMod == null)
			{
				return;
			}
			if (ModMeshOfMod.PhysicParam == null)
			{
				return;
			}

			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, MeshGroup, ModMeshOfMod, false, Editor);

			ModMeshOfMod.PhysicParam._gravityControlParam = resultControlParam;
			if (resultControlParam == null)
			{
				ModMeshOfMod.PhysicParam._gravityControlParamID = -1;
			}
			else
			{
				ModMeshOfMod.PhysicParam._gravityControlParamID = resultControlParam._uniqueID;
			}
		}

		private object _loadKey_SelectControlParamToPhyWind = null;
		public void OnSelectControlParamToPhysicWind(bool isSuccess, object loadKey, apControlParam resultControlParam)
		{
			if (_loadKey_SelectControlParamToPhyWind != loadKey || !isSuccess)
			{
				_loadKey_SelectControlParamToPhyWind = null;
				return;
			}

			_loadKey_SelectControlParamToPhyWind = null;
			if (Modifier == null
				|| (Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) == 0
				|| ModMeshOfMod == null)
			{
				return;
			}
			if (ModMeshOfMod.PhysicParam == null)
			{
				return;
			}

			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, MeshGroup, ModMeshOfMod, false, Editor);

			ModMeshOfMod.PhysicParam._windControlParam = resultControlParam;
			if (resultControlParam == null)
			{
				ModMeshOfMod.PhysicParam._windControlParamID = -1;
			}
			else
			{
				ModMeshOfMod.PhysicParam._windControlParamID = resultControlParam._uniqueID;
			}
		}

		private object _loadKey_SelectPhysicsParam = null;
		private void OnSelectPhysicsPreset(bool isSuccess, object loadKey, apPhysicsPresetUnit physicsUnit, apModifiedMesh targetModMesh)
		{
			if (!isSuccess || physicsUnit == null || targetModMesh == null || loadKey != _loadKey_SelectPhysicsParam || targetModMesh != ModMeshOfMod)
			{
				_loadKey_SelectPhysicsParam = null;
				return;
			}
			_loadKey_SelectPhysicsParam = null;
			if (targetModMesh.PhysicParam == null || SelectionType != SELECTION_TYPE.MeshGroup)
			{
				return;
			}
			//값 복사를 해주자
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SetPhysicsProperty, targetModMesh._meshGroupOfModifier, targetModMesh._meshGroupOfTransform, false, Editor);
			apPhysicsMeshParam physicsMeshParam = targetModMesh.PhysicParam;

			physicsMeshParam._presetID = physicsUnit._uniqueID;
			physicsMeshParam._moveRange = physicsUnit._moveRange;

			physicsMeshParam._isRestrictMoveRange = physicsUnit._isRestrictMoveRange;
			physicsMeshParam._isRestrictStretchRange = physicsUnit._isRestrictStretchRange;

			//physicsMeshParam._stretchRangeRatio_Min = physicsUnit._stretchRange_Min;
			physicsMeshParam._stretchRangeRatio_Max = physicsUnit._stretchRange_Max;
			physicsMeshParam._stretchK = physicsUnit._stretchK;
			physicsMeshParam._inertiaK = physicsUnit._inertiaK;
			physicsMeshParam._damping = physicsUnit._damping;
			physicsMeshParam._mass = physicsUnit._mass;

			physicsMeshParam._gravityConstValue = physicsUnit._gravityConstValue;
			physicsMeshParam._windConstValue = physicsUnit._windConstValue;
			physicsMeshParam._windRandomRange = physicsUnit._windRandomRange;

			physicsMeshParam._airDrag = physicsUnit._airDrag;
			physicsMeshParam._viscosity = physicsUnit._viscosity;
			physicsMeshParam._restoring = physicsUnit._restoring;

		}

		// Animation Right 2 GUI
		//------------------------------------------------------------------------------------
		private void DrawEditor_Right2_Animation(int width, int height)
		{
			// 상단부는 AnimClip의 정보를 출력하며,
			// 하단부는 선택된 Timeline의 정보를 출력한다.

			// AnimClip 정보 출력 부분

			Editor.SetGUIVisible("AnimationRight2GUI_AnimClip", (AnimClip != null));
			Editor.SetGUIVisible("AnimationRight2GUI_Timeline", (AnimTimeline != null));

			if (AnimClip == null)
			{
				return;
			}

			if (!Editor.IsDelayedGUIVisible("AnimationRight2GUI_AnimClip"))
			{
				//아직 출력하면 안된다.
				return;
			}

			apAnimClip animClip = AnimClip;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
			GUILayout.Space(10);

			EditorGUILayout.LabelField(
				new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation)),
				GUILayout.Width(50), GUILayout.Height(50));

			EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(animClip._name, GUILayout.Width(width - (50 + 10)));

			if (animClip._targetMeshGroup != null)
			{
				EditorGUILayout.LabelField("Target : " + animClip._targetMeshGroup._name, GUILayout.Width(width - (50 + 10)));
			}
			else
			{
				EditorGUILayout.LabelField("Target : [No MeshGroup]", GUILayout.Width(width - (50 + 10)));
			}


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			//애니메이션 기본 정보
			EditorGUILayout.LabelField("Animation Settings", GUILayout.Width(width));
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Start Frame", GUILayout.Width(110));
			int nextStartFrame = EditorGUILayout.DelayedIntField(animClip.StartFrame, GUILayout.Width(width - (110 + 5)));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("End Frame", GUILayout.Width(110));
			int nextEndFrame = EditorGUILayout.DelayedIntField(animClip.EndFrame, GUILayout.Width(width - (110 + 5)));
			EditorGUILayout.EndHorizontal();

			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//EditorGUILayout.LabelField("Loop", GUILayout.Width(110));
			//bool isNextLoop = EditorGUILayout.Toggle("", animClip.IsLoop, GUILayout.Width(width - (110 + 5)));
			//EditorGUILayout.EndHorizontal();
			bool isNextLoop = animClip.IsLoop;
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_Loop), " Loop On", " Loop Off", animClip.IsLoop, true, width, 24))
			{
				isNextLoop = !animClip.IsLoop;
				//값 적용은 아래에서
			}

			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("FPS", GUILayout.Width(110));
			int nextFPS = EditorGUILayout.DelayedIntField(animClip.FPS, GUILayout.Width(width - (110 + 5)));
			//int nextFPS = EditorGUILayout.IntSlider("FPS", animClip._FPS, 1, 240, GUILayout.Width(width));
			EditorGUILayout.EndHorizontal();

			if (nextStartFrame != animClip.StartFrame
				|| nextEndFrame != animClip.EndFrame
				|| nextFPS != animClip.FPS
				|| isNextLoop != animClip.IsLoop)
			{
				//바뀌었다면 타임라인 GUI를 세팅할 필요가 있을 수 있다.
				//Debug.Log("Anim Setting Changed");

				apEditorUtil.SetEditorDirty();

				//Start Frame과 Next Frame의 값이 뒤집혀져있는지 확인
				if (nextStartFrame > nextEndFrame)
				{
					int tmp = nextStartFrame;
					nextStartFrame = nextEndFrame;
					nextEndFrame = tmp;
				}

				animClip.SetOption_StartFrame(nextStartFrame);
				animClip.SetOption_EndFrame(nextEndFrame);
				animClip.SetOption_FPS(nextFPS);
				animClip.SetOption_IsLoop(isNextLoop);
			}



			GUILayout.Space(20);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);


			// Timeline 정보 출력 부분

			if (AnimTimeline == null)
			{
				return;
			}

			if (!Editor.IsDelayedGUIVisible("AnimationRight2GUI_Timeline"))
			{
				//아직 출력하면 안된다.
				return;
			}

			apAnimTimeline animTimeline = AnimTimeline;
			apAnimTimelineLayer animTimelineLayer = AnimTimelineLayer;


			//Timeline 정보 출력
			Texture2D iconTimeline = null;
			switch (animTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					iconTimeline = Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithMod);
					break;

				//case apAnimClip.LINK_TYPE.Bone:
				//	iconTimeline = Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithBone);
				//	break;

				case apAnimClip.LINK_TYPE.ControlParam:
					iconTimeline = Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithControlParam);
					break;

				default:
					iconTimeline = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy);//<<이상한 걸 넣어서 나중에 수정할 수 있게 하자
					break;
			}

			//1. 아이콘 / 타입
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(50));
			GUILayout.Space(10);

			EditorGUILayout.LabelField(new GUIContent(iconTimeline), GUILayout.Width(50), GUILayout.Height(50));

			EditorGUILayout.BeginVertical(GUILayout.Width(width - (50 + 10)));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(animTimeline.DisplayName, GUILayout.Width(width - (50 + 10)));


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			//GUILayout.Space(10);


			//현재 선택한 객체를 레이어로 만들 수 있다.
			//상태 : 선택한게 없다. / 선택은 했으나 레이어에 등록이 안되었다. (등록할 수 있다) / 선택한게 이미 등록한 객체다. (
			bool isAnyTargetObjectSelected = false;
			bool isAddableType = false;
			bool isAddable = false;
			string targetObjectName = "";
			object targetObject = null;
			bool isAddingLayerOnce = false;
			bool isAddChildTransformAddable = false;
			switch (animTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					//Transform이 속해있는지 확인하자
					if (SubMeshTransformOnAnimClip != null)
					{
						isAnyTargetObjectSelected = true;
						targetObjectName = SubMeshTransformOnAnimClip._nickName;
						targetObject = SubMeshTransformOnAnimClip;

						//레이어로 등록가능한가
						isAddableType = animTimeline.IsLayerAddableType(SubMeshTransformOnAnimClip);
						isAddable = !animTimeline.IsObjectAddedInLayers(SubMeshTransformOnAnimClip);
					}
					else if (SubMeshGroupTransformOnAnimClip != null)
					{
						isAnyTargetObjectSelected = true;
						targetObjectName = SubMeshGroupTransformOnAnimClip._nickName;
						targetObject = SubMeshGroupTransformOnAnimClip;

						//레이어로 등록가능한가.
						isAddableType = animTimeline.IsLayerAddableType(SubMeshGroupTransformOnAnimClip);
						isAddable = !animTimeline.IsObjectAddedInLayers(SubMeshGroupTransformOnAnimClip);
					}
					else if (Bone != null)
					{
						isAnyTargetObjectSelected = true;
						targetObjectName = Bone._name;
						targetObject = Bone;

						isAddableType = animTimeline.IsLayerAddableType(Bone);
						isAddable = !animTimeline.IsObjectAddedInLayers(Bone);
					}
					isAddingLayerOnce = true;//한번에 레이어를 추가할 수 있다.
					isAddChildTransformAddable = animTimeline._linkedModifier.IsTarget_ChildMeshTransform;
					break;


				case apAnimClip.LINK_TYPE.ControlParam:
					if (SubControlParamOnAnimClip != null)
					{
						isAnyTargetObjectSelected = true;
						targetObjectName = SubControlParamOnAnimClip._keyName;
						targetObject = SubControlParamOnAnimClip;

						isAddableType = animTimeline.IsLayerAddableType(SubControlParamOnAnimClip);
						isAddable = !animTimeline.IsObjectAddedInLayers(SubControlParamOnAnimClip);
					}

					isAddingLayerOnce = false;
					break;
			}
			bool isRemoveTimeline = false;

			bool isRemoveTimelineLayer = false;
			apAnimTimelineLayer removeLayer = null;

			//추가 : 추가 가능한 모든 객체에 대해서 TimelineLayer를 추가한다.
			if (isAddingLayerOnce)
			{
				string strTargetObject = "";
				bool isTargetTF = true;
				if (_meshGroupChildHierarchy_Anim == MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
				{
					strTargetObject = "Meshes";
					isTargetTF = true;
				}
				else
				{
					strTargetObject = "Bones";
					isTargetTF = false;
				}
				if (GUILayout.Button("All " + strTargetObject + " to Layers"))
				{
					//bool isResult = EditorUtility.DisplayDialog("Add to Timelines", "All " + strTargetObject + " are added to Timeline Layers?", "Add All", "Cancel");

					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AddAllObjects2Timeline_Title),
																Editor.GetTextFormat(apLocalization.TEXT.AddAllObjects2Timeline_Body, strTargetObject),
																Editor.GetText(apLocalization.TEXT.Okay),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);

					if (isResult)
					{
						//모든 객체를 TimelineLayer로 등록한다.
						Editor.Controller.AddAnimTimelineLayerForAllTransformObject(animClip._targetMeshGroup,
																						isTargetTF,
																						isAddChildTransformAddable,
																						animTimeline);
					}
				}
			}

			if (GUILayout.Button("Remove Timeline"))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Timeline", "Is Really Remove Timeline?", "Remove", "Cancel");

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveTimeline_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveTimeline_Body, animTimeline.DisplayName),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);

				if (isResult)
				{
					isRemoveTimeline = true;
				}
			}
			GUILayout.Space(20);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);

			Color prevColor = GUI.backgroundColor;



			Editor.SetGUIVisible("AnimationRight2GUI_Timeline_SelectedObject", isAnyTargetObjectSelected);
			Editor.SetGUIVisible("AnimationRight2GUI_Timeline_SelectedObject_Inverse", !isAnyTargetObjectSelected);

			bool isGUI_TargetSelected = Editor.IsDelayedGUIVisible("AnimationRight2GUI_Timeline_SelectedObject");
			bool isGUI_TargetUnSelected = Editor.IsDelayedGUIVisible("AnimationRight2GUI_Timeline_SelectedObject_Inverse");

			// -----------------------------------------
			if (isGUI_TargetSelected || isGUI_TargetUnSelected)
			{
				if (isGUI_TargetSelected)
				{


					GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
					boxGUIStyle.alignment = TextAnchor.MiddleCenter;

					if (isAddableType)
					{
						if (isAddable)
						{
							//아직 레이어로 추가가 되지 않았다.
							GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
							GUILayout.Box("[" + targetObjectName + "]\nNot Added to Edit", GUILayout.Width(width), GUILayout.Height(35));

							GUI.backgroundColor = prevColor;

							if (GUILayout.Button("Add Timeline Layer to Edit", GUILayout.Height(35)))
							{
								//Debug.LogError("TODO ; Layer 추가하기");
								Editor.Controller.AddAnimTimelineLayer(targetObject, animTimeline);
							}
						}
						else
						{
							//레이어에 이미 있다.
							GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
							GUILayout.Box("[" + targetObjectName + "]\nSelected", GUILayout.Width(width), GUILayout.Height(35));

							GUI.backgroundColor = prevColor;

							if (GUILayout.Button("Remove Timeline Layer", GUILayout.Height(25)))
							{
								//bool isResult = EditorUtility.DisplayDialog("Remove TimelineLayer", "Is Really Remove Timeline Layer?", "Remove", "Cancel");

								bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveTimelineLayer_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveTimelineLayer_Body, animTimelineLayer.DisplayName),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);

								if (isResult)
								{
									isRemoveTimelineLayer = true;
									removeLayer = animTimelineLayer;
								}
							}
						}
					}
					else
					{
						//추가할 수 있는 타입이 아니다.
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						GUILayout.Box("[" + targetObjectName + "]\nUnable to be Added", GUILayout.Width(width), GUILayout.Height(35));

						GUI.backgroundColor = prevColor;
					}
				}



				EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(10));
				EditorGUILayout.EndVertical();
				GUILayout.Space(11);



				EditorGUILayout.LabelField("Timeline Layers");
				GUILayout.Space(8);


				//현재의 타임라인 레이어 리스트를 만들어야한다.
				List<apAnimTimelineLayer> timelineLayers = animTimeline._layers;
				apAnimTimelineLayer curLayer = null;

				for (int i = 0; i < timelineLayers.Count; i++)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();

					curLayer = timelineLayers[i];
					if (animTimelineLayer == curLayer)
					{
						//선택된 레이어다.
						GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);
					}
					else
					{
						//선택되지 않은 레이어다.
						GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
					}

					int heightOffset = 18;
					if (i == 0)
					{
						heightOffset = 8;//9
					}

					GUI.Box(new Rect(lastRect.x, lastRect.y + heightOffset, width + 10, 30), "");
					GUI.backgroundColor = prevColor;

					int compWidth = width - (55 + 20 + 5 + 10);

					GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
					guiStyle_Label.alignment = TextAnchor.MiddleLeft;

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
					GUILayout.Space(10);
					EditorGUILayout.LabelField(curLayer.DisplayName, guiStyle_Label, GUILayout.Width(compWidth), GUILayout.Height(20));

					if (animTimelineLayer == curLayer)
					{
						GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
						guiStyle.normal.textColor = Color.white;
						guiStyle.alignment = TextAnchor.UpperCenter;
						GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
						GUILayout.Box("Selected", guiStyle, GUILayout.Width(55), GUILayout.Height(20));
						GUI.backgroundColor = prevColor;
					}
					else
					{
						if (GUILayout.Button("Select", GUILayout.Width(55), GUILayout.Height(20)))
						{
							SetAnimTimelineLayer(curLayer, true);
						}
					}

					if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey), GUILayout.Width(20), GUILayout.Height(20)))
					{
						//bool isResult = EditorUtility.DisplayDialog("Remove Timeline Layer", "Remove Timeline Layer?", "Remove", "Cancel");

						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveTimelineLayer_Title),
																Editor.GetTextFormat(apLocalization.TEXT.RemoveTimelineLayer_Body, curLayer.DisplayName),
																Editor.GetText(apLocalization.TEXT.Remove),
																Editor.GetText(apLocalization.TEXT.Cancel)
																);

						if (isResult)
						{
							isRemoveTimelineLayer = true;
							removeLayer = curLayer;
						}
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(20);
				}



			}


			//----------------------------------
			// 삭제 플래그가 있다.
			if (isRemoveTimelineLayer)
			{
				Editor.Controller.RemoveAnimTimelineLayer(removeLayer);
				SetAnimTimelineLayer(null, true, true);
				SetAnimClipGizmoEvent(true);
			}
			else if (isRemoveTimeline)
			{
				Editor.Controller.RemoveAnimTimeline(animTimeline);
				SetAnimTimeline(null, true);
				SetAnimClipGizmoEvent(true);

			}

		}
	}
}
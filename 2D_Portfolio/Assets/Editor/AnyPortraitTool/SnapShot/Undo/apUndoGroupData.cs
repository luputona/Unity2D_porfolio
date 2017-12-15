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
	/// Unity의 Undo 기능을 사용할 때, 불필요한 호출을 막는 용도
	/// "연속된 동일한 요청"을 방지한다.
	/// 중복 체크만 하는 것이므로 1개의 값만 가진다.
	/// </summary>
	public class apUndoGroupData
	{
		// Singletone
		//---------------------------------------------------
		private static apUndoGroupData _instance = new apUndoGroupData();
		public static apUndoGroupData I { get { return _instance; } }

		// Members
		//--------------------------------------------------

		private ACTION _action = ACTION.None;

		private MonoBehaviour _parentMonoObject = null;
		private object _targetObject = null;
		private bool _isMultiple = false;//여러 항목을 동시에 처리하는 Batch 액션 중인가

		public enum ACTION
		{
			None,
			Main_AddImage,
			Main_RemoveImage,
			Main_AddMesh,
			Main_RemoveMesh,
			Main_AddMeshGroup,
			Main_RemoveMeshGroup,
			Main_AddAnimation,
			Main_RemoveAnimation,
			Main_AddParam,
			Main_RemoveParam,

			Portrait_BakeOptionChanged,
			Portrait_SetMeshGroup,
			Portrait_ReleaseMeshGroup,


			Image_SettingChanged,

			MeshEdit_AddVertex,
			MeshEdit_EditVertex,
			MeshEdit_RemoveVertex,
			MeshEdit_ResetVertices,
			MeshEdit_RemoveAllVertices,
			MeshEdit_AddEdge,
			MeshEdit_EditEdge,
			MeshEdit_RemoveEdge,
			MeshEdit_MakeEdges,
			MeshEdit_EditPolygons,
			MeshEdit_SetImage,
			MeshEdit_SetPivot,
			MeshEdit_SettingChanged,

			MeshGroup_AttachMesh,
			MeshGroup_AttachMeshGroup,
			MeshGroup_DetachMesh,
			MeshGroup_DetachMeshGroup,
			MeshGroup_AddBone,
			MeshGroup_RemoveBone,
			MeshGroup_RemoveAllBones,
			MeshGroup_BoneDefaultEdit,
			MeshGroup_AttachBoneToChild,
			MeshGroup_DetachBoneFromChild,
			MeshGroup_SetBoneAsParent,
			MeshGroup_SetBoneAsIKTarget,


			MeshGroup_Gizmo_MoveTransform,
			MeshGroup_Gizmo_RotateTransform,
			MeshGroup_Gizmo_ScaleTransform,
			MeshGroup_Gizmo_Color,

			MeshGroup_AddModifier,
			MeshGroup_RemoveModifier,

			MeshGroup_DefaultSettingChanged,


			Modifier_LinkControlParam,
			Modifier_UnlinkControlParam,
			Modifier_AddStaticParamSetGroup,

			Modifier_LayerChanged,
			Modifier_SettingChanged,
			Modifier_SetBoneWeight,
			Modifier_RemoveBoneWeight,
			Modifier_RemoveBoneRigging,
			Modifier_RemovePhysics,
			Modifier_SetPhysicsWeight,
			Modifier_SetVolumeWeight,
			Modifier_SetPhysicsProperty,

			Modifier_Gizmo_MoveTransform,
			Modifier_Gizmo_RotateTransform,
			Modifier_Gizmo_ScaleTransform,
			Modifier_Gizmo_MoveVertex,
			Modifier_Gizmo_RotateVertex,
			Modifier_Gizmo_ScaleVertex,
			Modifier_Gizmo_FFDVertex,
			Modifier_Gizmo_Color,

			Modifier_ModMeshValuePaste,
			Modifier_ModMeshValueReset,
			Modifier_AddModMeshToParamSet,
			Modifier_RemoveModMeshFromParamSet,

			Anim_SetMeshGroup,
			Anim_DupAnimClip,
			Anim_AddTimeline,
			Anim_RemoveTimeline,
			Anim_AddTimelineLayer,
			Anim_RemoveTimelineLayer,
			Anim_AddKeyframe,
			Anim_RemoveKeyframe,
			Anim_DupKeyframe,
			Anim_KeyframeValueChanged,

			Anim_Gizmo_MoveTransform,
			Anim_Gizmo_RotateTransform,
			Anim_Gizmo_ScaleTransform,

			Anim_Gizmo_MoveVertex,
			Anim_Gizmo_RotateVertex,
			Anim_Gizmo_ScaleVertex,
			Anim_Gizmo_FFDVertex,

			Anim_Gizmo_Color,

			ControlParam_SettingChanged,

		}


		public static string GetLabel(ACTION action)
		{
			switch (action)
			{
				case ACTION.None:
					return "None";
				case ACTION.Main_AddImage:
					return "Add Image";
				case ACTION.Main_RemoveImage:
					return "Remove Image";
				case ACTION.Main_AddMesh:
					return "Add Mesh";
				case ACTION.Main_RemoveMesh:
					return "Remove Mesh";
				case ACTION.Main_AddMeshGroup:
					return "Add MeshGroup";
				case ACTION.Main_RemoveMeshGroup:
					return "Remove MeshGroup";
				case ACTION.Main_AddAnimation:
					return "Add Animation";
				case ACTION.Main_RemoveAnimation:
					return "Remove Animation";
				case ACTION.Main_AddParam:
					return "Add Parameter";
				case ACTION.Main_RemoveParam:
					return "Remove Parameter";

				case ACTION.Portrait_BakeOptionChanged:
					return "Bake Option Changed";
				case ACTION.Portrait_SetMeshGroup:
					return "Set Main MeshGroup";
				case ACTION.Portrait_ReleaseMeshGroup:
					return "Release Main MeshGroup";

				case ACTION.Image_SettingChanged:
					return "Set Image Property";

				case ACTION.MeshEdit_AddVertex:
					return "Add Vertex";
				case ACTION.MeshEdit_EditVertex:
					return "Edit Vertex";
				case ACTION.MeshEdit_RemoveVertex:
					return "Remove Vertex";
				case ACTION.MeshEdit_ResetVertices:
					return "Reset Vertices";
				case ACTION.MeshEdit_RemoveAllVertices:
					return "Remove All Vertices";
				case ACTION.MeshEdit_AddEdge:
					return "Add Edge";
				case ACTION.MeshEdit_EditEdge:
					return "Edit Edge";
				case ACTION.MeshEdit_RemoveEdge:
					return "Remove Edge";
				case ACTION.MeshEdit_MakeEdges:
					return "Make Edges";
				case ACTION.MeshEdit_EditPolygons:
					return "Edit Polygons";
				case ACTION.MeshEdit_SetImage:
					return "Set Image";
				case ACTION.MeshEdit_SetPivot:
					return "Set Mesh Pivot";
				case ACTION.MeshEdit_SettingChanged:
					return "Mesh Setting Changed";

				case ACTION.MeshGroup_AttachMesh:
					return "Attach Mesh";
				case ACTION.MeshGroup_AttachMeshGroup:
					return "Attach MeshGroup";
				case ACTION.MeshGroup_DetachMesh:
					return "Detach Mesh";
				case ACTION.MeshGroup_DetachMeshGroup:
					return "Detach MeshGroup";
				case ACTION.MeshGroup_AddBone:
					return "Add Bone";
				case ACTION.MeshGroup_RemoveBone:
					return "Remove Bone";
				case ACTION.MeshGroup_RemoveAllBones:
					return "Remove All Bones";
				case ACTION.MeshGroup_BoneDefaultEdit:
					return "Bone Edit";
				case ACTION.MeshGroup_AttachBoneToChild:
					return "Attach Bone to Child";
				case ACTION.MeshGroup_DetachBoneFromChild:
					return "Detach Bone from Child";
				case ACTION.MeshGroup_SetBoneAsParent:
					return "Set Bone as Parent";
				case ACTION.MeshGroup_SetBoneAsIKTarget:
					return "Set Bone as IK target";

				case ACTION.MeshGroup_Gizmo_MoveTransform:
					return "Default Position";
				case ACTION.MeshGroup_Gizmo_RotateTransform:
					return "Default Rotation";
				case ACTION.MeshGroup_Gizmo_ScaleTransform:
					return "Default Scaling";
				case ACTION.MeshGroup_Gizmo_Color:
					return "Default Color";

				case ACTION.MeshGroup_AddModifier:
					return "Add Modifier";
				case ACTION.MeshGroup_RemoveModifier:
					return "Remove Modifier";

				case ACTION.MeshGroup_DefaultSettingChanged:
					return "Default Setting Changed";

				case ACTION.Modifier_LinkControlParam:
					return "Link Control Parameter";
				case ACTION.Modifier_UnlinkControlParam:
					return "Unlink Control Parameter";
				case ACTION.Modifier_AddStaticParamSetGroup:
					return "Add StaticPSG";

				case ACTION.Modifier_LayerChanged:
					return "Change Layer Order";
				case ACTION.Modifier_SettingChanged:
					return "Change Layer Setting";
				case ACTION.Modifier_SetBoneWeight:
					return "Set Bone Weight";
				case ACTION.Modifier_RemoveBoneWeight:
					return "Remove Bone Weight";
				case ACTION.Modifier_RemoveBoneRigging:
					return "Remove Bone Rigging";
				case ACTION.Modifier_RemovePhysics:
					return "Remove Physics";
				case ACTION.Modifier_SetPhysicsWeight:
					return "Set Physics Weight";
				case ACTION.Modifier_SetVolumeWeight:
					return "Set Volume Weight";
				case ACTION.Modifier_SetPhysicsProperty:
					return "Set Physics Property";

				case ACTION.Modifier_Gizmo_MoveTransform:
					return "Move Transform";
				case ACTION.Modifier_Gizmo_RotateTransform:
					return "Rotate Transform";
				case ACTION.Modifier_Gizmo_ScaleTransform:
					return "Scale Transform";
				case ACTION.Modifier_Gizmo_MoveVertex:
					return "Move Vertex";
				case ACTION.Modifier_Gizmo_RotateVertex:
					return "Rotate Vertex";
				case ACTION.Modifier_Gizmo_ScaleVertex:
					return "Scale Vertex";
				case ACTION.Modifier_Gizmo_FFDVertex:
					return "Freeform Vertices";
				case ACTION.Modifier_Gizmo_Color:
					return "Set Color";

				case ACTION.Modifier_ModMeshValuePaste:
					return "Paste Modified Value";
				case ACTION.Modifier_ModMeshValueReset:
					return "Reset Modified Value";

				case ACTION.Modifier_AddModMeshToParamSet:
					return "Add To Key";
				case ACTION.Modifier_RemoveModMeshFromParamSet:
					return "Remove From Key";

				case ACTION.Anim_SetMeshGroup:
					return "Set MeshGroup";
				case ACTION.Anim_DupAnimClip:
					return "Duplicate AnimClip";
				case ACTION.Anim_AddTimeline:
					return "Add Timeline";
				case ACTION.Anim_RemoveTimeline:
					return "Remove Timeline";
				case ACTION.Anim_AddTimelineLayer:
					return "Add Timeline Layer";
				case ACTION.Anim_RemoveTimelineLayer:
					return "Remove Timeline Layer";
				case ACTION.Anim_AddKeyframe:
					return "Add Keyframe";
				case ACTION.Anim_RemoveKeyframe:
					return "Remove Keyframe";
				case ACTION.Anim_DupKeyframe:
					return "Duplicate Keyframe";

				case ACTION.Anim_KeyframeValueChanged:
					return "Keyframe Value Changed";

				case ACTION.Anim_Gizmo_MoveTransform:
					return "Move Transform";
				case ACTION.Anim_Gizmo_RotateTransform:
					return "Rotate Transform";
				case ACTION.Anim_Gizmo_ScaleTransform:
					return "Scale Transform";

				case ACTION.Anim_Gizmo_MoveVertex:
					return "Move Vertex";
				case ACTION.Anim_Gizmo_RotateVertex:
					return "Rotate Vertex";
				case ACTION.Anim_Gizmo_ScaleVertex:
					return "Scale Vertex";
				case ACTION.Anim_Gizmo_FFDVertex:
					return "Freeform Vertices";
				case ACTION.Anim_Gizmo_Color:
					return "Set Color";

				case ACTION.ControlParam_SettingChanged:
					return "Control Param Setting";

				default:
					Debug.LogError("정의되지 않은 Undo Action");
					return action.ToString();
			}
		}

		// Init
		//--------------------------------------------------
		private apUndoGroupData()
		{

		}

		public void Clear()
		{
			_action = ACTION.None;
			_parentMonoObject = null;
			_targetObject = null;
			_isMultiple = false;//여러 항목을 동시에 처리하는 Batch 액션 중인가
		}


		// Functions
		//--------------------------------------------------
		/// <summary>
		/// Undo 전에 중복을 체크하기 위해 Action을 등록한다.
		/// 리턴값이 True이면 "새로운 Action"이므로 Undo 등록을 해야한다.
		/// 만약 Action 타입이 Add, New.. 계열이면 targetObject가 null일 수 있다. (parent는 null이 되어선 안된다)
		/// </summary>
		/// <param name="label"></param>
		/// <param name="parentObject"></param>
		/// <param name="targtObject"></param>
		/// <param name="isMultiple"></param>
		public bool SetAction(ACTION action, MonoBehaviour parentMonoObject, object targtObject, bool isMultiple)
		{
			if (_action != ACTION.None && _parentMonoObject != null)
			{
				if (_action == action && _parentMonoObject == parentMonoObject && isMultiple == _isMultiple)
				{
					if (_isMultiple)
					{
						//다중 처리 타입이면 -> targetObject가 달라도 연속된 액션이다.
						return false;
					}
					else
					{
						//Multiple 타입이 아니라면 targetObject도 동일해야한다.
						//단, 둘다 Null이라면 연속된 타입일 수 없다.
						if (targtObject == _targetObject && targtObject != null && _targetObject != null)
						{
							return false;//연속된 Action이다.
						}
					}
				}
			}
			_action = action;
			_parentMonoObject = parentMonoObject;
			_targetObject = targtObject;
			_isMultiple = isMultiple;
			return true;
		}

	}
}
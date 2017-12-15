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

	[Serializable]
	public class apModifierStack
	{
		// Members
		//----------------------------------------------------
		//저장되는 Modifier들
		//Serialize는 다형성 저장이 안되어서 타입별로 따로 만들고, 실행중에 부모 클래스 리스트에 합친다.
		[SerializeField, HideInInspector]
		private List<apModifier_Volume> _modifiers_Volume = new List<apModifier_Volume>();

		[SerializeField, HideInInspector]
		private List<apModifier_Morph> _modifiers_Morph = new List<apModifier_Morph>();

		[SerializeField, HideInInspector]
		private List<apModifier_AnimatedMorph> _modifiers_AnimatedMorph = new List<apModifier_AnimatedMorph>();

		[SerializeField, HideInInspector]
		private List<apModifier_Rigging> _modifiers_Rigging = new List<apModifier_Rigging>();

		[SerializeField, HideInInspector]
		private List<apModifier_Physic> _modifiers_Physic = new List<apModifier_Physic>();

		[SerializeField, HideInInspector]
		private List<apModifier_TF> _modifiers_TF = new List<apModifier_TF>();

		[SerializeField, HideInInspector]
		private List<apModifier_AnimatedTF> _modifiers_AnimatedTF = new List<apModifier_AnimatedTF>();

		[SerializeField, HideInInspector]
		private List<apModifier_FFD> _modifiers_FFD = new List<apModifier_FFD>();

		[SerializeField, HideInInspector]
		private List<apModifier_AnimatedFFD> _modifiers_AnimatedFFD = new List<apModifier_AnimatedFFD>();


		//실제로 작동하는 Modifier 리스트 (Layer 순서에 맞게 Sort)
		[NonSerialized]
		public List<apModifierBase> _modifiers = new List<apModifierBase>();

		[NonSerialized]
		public apPortrait _parentPortrait = null;

		[NonSerialized]
		public apMeshGroup _parentMeshGroup = null;

		[NonSerialized]
		private bool _isSorted = false;


		// Init
		//----------------------------------------------------
		public apModifierStack()
		{

		}



		public void RefreshAndSort(bool isSetActiveAllModifier)
		{
			_modifiers.Clear();

			for (int i = 0; i < _modifiers_Volume.Count; i++)
			{
				_modifiers.Add(_modifiers_Volume[i]);
			}

			for (int i = 0; i < _modifiers_Morph.Count; i++)
			{
				_modifiers.Add(_modifiers_Morph[i]);
			}

			for (int i = 0; i < _modifiers_AnimatedMorph.Count; i++)
			{
				_modifiers.Add(_modifiers_AnimatedMorph[i]);
			}

			for (int i = 0; i < _modifiers_Rigging.Count; i++)
			{
				_modifiers.Add(_modifiers_Rigging[i]);
			}

			for (int i = 0; i < _modifiers_Physic.Count; i++)
			{
				_modifiers.Add(_modifiers_Physic[i]);
			}

			for (int i = 0; i < _modifiers_TF.Count; i++)
			{
				_modifiers.Add(_modifiers_TF[i]);
			}

			for (int i = 0; i < _modifiers_AnimatedTF.Count; i++)
			{
				_modifiers.Add(_modifiers_AnimatedTF[i]);
			}

			for (int i = 0; i < _modifiers_FFD.Count; i++)
			{
				_modifiers.Add(_modifiers_FFD[i]);
			}

			for (int i = 0; i < _modifiers_AnimatedFFD.Count; i++)
			{
				_modifiers.Add(_modifiers_AnimatedFFD[i]);
			}


			_modifiers.Sort(delegate (apModifierBase a, apModifierBase b)
			{
				return (a._layer * 10) - (b._layer * 10);
			});

			for (int i = 0; i < _modifiers.Count; i++)
			{
				_modifiers[i]._layer = i;
			}

			_isSorted = true;

			if (isSetActiveAllModifier)
			{
				ActiveAllModifierFromExclusiveEditing();
			}
		}



		// Functions
		//----------------------------------------------------
		public void Update_Pre(float tDelta)
		{
			if (_modifiers.Count == 0 && !_isSorted)
			{
				Debug.Log("Sort Modifier");
				RefreshAndSort(false);
			}

			Profiler.BeginSample("Modifier Calculate");
			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (!_modifiers[i].IsPreUpdate)
				{
					//Post-Update라면 패스
					continue;
				}
				if (_modifiers[i]._isActive
#if UNITY_EDITOR
				&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled//<<이건 에디터에서만 작동한다.
#endif
				)

				{
					_modifiers[i].Calculate(tDelta);
				}
				else
				{
					//Debug.Log("Not Update Mod Stack : " + _modifiers[i].DisplayName + " / " + _parentMeshGroup._name);
					_modifiers[i].InitCalculate(tDelta);
				}
			}

			Profiler.EndSample();
		}


		public void Update_Post(float tDelta)
		{
			Profiler.BeginSample("Modifier Calculate - Post");
			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (_modifiers[i].IsPreUpdate)
				{
					//Pre-Update라면 패스
					continue;
				}
				if (_modifiers[i]._isActive
#if UNITY_EDITOR
				&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled//<<이건 에디터에서만 작동한다.
#endif
				)

				{
					_modifiers[i].Calculate(tDelta);
				}
				else
				{
					//Debug.Log("Not Update Mod Stack : " + _modifiers[i].DisplayName + " / " + _parentMeshGroup._name);
					_modifiers[i].InitCalculate(tDelta);
				}
			}

			Profiler.EndSample();
		}



		// 에디터 관련 코드
		//----------------------------------------------------
		public void ActiveAllModifierFromExclusiveEditing()
		{
			apModifierBase modifier = null;
			for (int i = 0; i < _modifiers.Count; i++)
			{
				modifier = _modifiers[i];
				modifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled;

				for (int iP = 0; iP < modifier._paramSetGroup_controller.Count; iP++)
				{
					modifier._paramSetGroup_controller[iP]._isEnabledExclusive = true;
				}

				//List<apCalculatedResultParam> calParamList = modifier._calculatedResultParams;
				//for (int iCal = 0; iCal < calParamList.Count; iCal++)
				//{
				//	calParamList[iCal].ActiveAllParamList();
				//}
			}

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				if (_parentMeshGroup._childMeshGroupTransforms != null)
				{
					for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
						}
					}
				}
			}
		}



		/// <summary>
		/// [선택한 Modifier]와 [선택한 ParamSetGroup]만 활성화하고 나머지는 비활성한다.
		/// 한개의 ParamSetGroup만 활성화하므로 "한개의 ControlParam만 작업할 때" 호출된다.
		/// </summary>
		/// <param name="modifier"></param>
		/// <param name="paramSetGroup"></param>
		public void SetExclusiveModifierInEditing(apModifierBase modifier, apModifierParamSetGroup paramSetGroup)
		{
			//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;

			//추가
			//요청한 Modifier가 BoneTransform을 지원하는 경우
			//Rigging은 비활성화 되어서는 안된다.
			bool isRiggingAvailable = false;
			if (modifier != null && modifier.IsTarget_Bone && modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				isRiggingAvailable = true;//Rigging은 허용하자
			}

			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (_modifiers[i] == modifier && modifier != null && paramSetGroup != null)
				{
					//_modifiers[i]._isActive_InEditorExclusive = true;
					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						if (paramSetGroup == _modifiers[i]._paramSetGroup_controller[iP])
						{
							_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = true;
						}
						else
						{
							_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = false;
						}

					}

					//Active라 하더라도, ParamSet에도 처리 여부를 결정해야한다.

					//Modifier를 False 했다면 CalculateParam은 True로 해도 된다.
					//List<apCalculatedResultParam> calParamList = _modifiers[i]._calculatedResultParams;
					//for (int iCal = 0; iCal < calParamList.Count; iCal++)
					//{
					//	calParamList[iCal].ActiveConditionParamList(paramSetGroup._paramSetList);
					//}
				}
				else if (isRiggingAvailable && _modifiers[i].ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					//만약 Rigging 타입은 예외로 친다면..
					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = true;
					}

					////Active라 하더라도, ParamSet에도 처리 여부를 결정해야한다.

					////Modifier를 False 했다면 CalculateParam은 True로 해도 된다.
					//List<apCalculatedResultParam> calParamList = _modifiers[i]._calculatedResultParams;
					//for (int iCal = 0; iCal < calParamList.Count; iCal++)
					//{
					//	calParamList[iCal].ActiveConditionParamList(paramSetGroup._paramSetList);
					//}
				}
				else
				{
					////타입이 같은가 (다른건 해도 괜찮다.)
					//일단 다 빼보자
					//_modifiers[i]._isActive_InEditorExclusive = false;
					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						if (paramSetGroup == _modifiers[i]._paramSetGroup_controller[iP])
						{
							_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = false;
						}
					}

					////Modifier를 False 했다면 CalculateParam은 True로 해도 된다.
					//List<apCalculatedResultParam> calParamList = _modifiers[i]._calculatedResultParams;
					//for (int iCal = 0; iCal < calParamList.Count; iCal++)
					//{
					//	calParamList[iCal].ActiveAllParamList();
					//}
				}
			}

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				if (_parentMeshGroup._childMeshGroupTransforms != null)
				{
					for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing(null, null);
						}
					}
				}
			}
		}



		public void SetExclusiveModifierInEditing_MultipleParamSetGroup(apModifierBase modifier,
																		List<apModifierParamSetGroup> paramSetGroups)
		{
			//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;
			//추가
			//요청한 Modifier가 BoneTransform을 지원하는 경우
			//Rigging은 비활성화 되어서는 안된다.
			bool isRiggingAvailable = false;
			if (modifier != null && modifier.IsTarget_Bone && modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				isRiggingAvailable = true;//Rigging은 허용하자
			}

			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (_modifiers[i] == modifier && modifier != null && paramSetGroups != null && paramSetGroups.Count > 0)
				{
					//_modifiers[i]._isActive_InEditorExclusive = true;
					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
						if (paramSetGroups.Contains(paramSetGroup))
						{
							paramSetGroup._isEnabledExclusive = true;
						}
						else
						{
							paramSetGroup._isEnabledExclusive = false;
						}


					}

					//Active라 하더라도, ParamSet에도 처리 여부를 결정해야한다.

					//Modifier를 False 했다면 CalculateParam은 True로 해도 된다.
					//List<apCalculatedResultParam> calParamList = _modifiers[i]._calculatedResultParams;
					//for (int iCal = 0; iCal < calParamList.Count; iCal++)
					//{
					//	for (int iParamSetGroups = 0; iParamSetGroups < paramSetGroups.Count; iParamSetGroups++)
					//	{
					//		apModifierParamSetGroup paramSetGroup = paramSetGroups[iParamSetGroups];
					//		calParamList[iCal].ActiveConditionParamList(paramSetGroup._paramSetList);
					//	}
					//}
				}
				else if (isRiggingAvailable && _modifiers[i].ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					//만약 Rigging 타입은 예외로 친다면..
					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = true;
					}
				}
				else
				{
					////타입이 같은가 (다른건 해도 괜찮다.)
					//if(_modifiers[i].CalculatedResultType == targetResultType)
					//{
					//	_modifiers[i]._isActive_InEditorExclusive = false;
					//}
					//else
					//{
					//	_modifiers[i]._isActive_InEditorExclusive = true;
					//}
					//일단 다 빼보자
					//_modifiers[i]._isActive_InEditorExclusive = false;

					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
						paramSetGroup._isEnabledExclusive = false;
					}


					////Modifier를 False 했다면 CalculateParam은 True로 해도 된다.
					//List<apCalculatedResultParam> calParamList = _modifiers[i]._calculatedResultParams;
					//for (int iCal = 0; iCal < calParamList.Count; iCal++)
					//{
					//	calParamList[iCal].ActiveAllParamList();
					//}
				}
			}

			//Child MeshGroup에도 모두 적용하자 - False로..
			if (_parentMeshGroup != null)
			{
				if (_parentMeshGroup._childMeshGroupTransforms != null)
				{
					for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing(null, null);
						}
					}
				}
			}
		}





		/// <summary>
		/// [선택한 Modifier] + [해당 Modifier가 허용하는 다른 Modifier]만 허용한다.
		/// 모든 ParamSetGroup을 허용하므로 에디팅이 조금 다를 수는 있다.
		/// Animation 버전은 따로 만들 것
		/// </summary>
		/// <param name="modifier"></param>
		public void SetExclusiveModifierInEditingGeneral(apModifierBase modifier)
		{
			//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;
			apModifierBase.MODIFIER_TYPE[] exGeneralTypes = modifier.GetGeneralExEditableModTypes();
			if (exGeneralTypes == null)
			{
				exGeneralTypes = new apModifierBase.MODIFIER_TYPE[] { modifier.ModifierType };
			}

			//추가
			//요청한 Modifier가 BoneTransform을 지원하는 경우
			//Rigging은 비활성화 되어서는 안된다.
			for (int i = 0; i < _modifiers.Count; i++)
			{
				bool isValidType = false;
				for (int iGT = 0; iGT < exGeneralTypes.Length; iGT++)
				{
					if (exGeneralTypes[iGT] == _modifiers[i].ModifierType)
					{
						isValidType = true;
						break;
					}
				}

				if (isValidType)
				{
					//_modifiers[i]._isActive_InEditorExclusive = true;
					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						//ParamSetGroup도 모두다 허용
						_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = true;
					}
				}
				else
				{
					//불가불가
					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = false;
					}
				}
			}

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				if (_parentMeshGroup._childMeshGroupTransforms != null)
				{
					for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditingGeneral(modifier);
						}
					}
				}
			}
		}


		/// <summary>
		/// AnimTimeline을 선택하고, 그 안의 AnimTimeLayer를 모두 활성화한다.
		/// 일반적으로 [선택하지 않은 AnimTimeline]들을 모두 해제하는 반면에, 
		/// 여기서는 해당 ParamSetGroup에 연동된 AnimTimeline이 AnimClip에 포함된다면 모두 포함시킨다.
		/// </summary>
		/// <param name="modifier"></param>
		/// <param name="paramSetGroups"></param>
		public void SetExclusiveModifierInEditing_MultipleParamSetGroup_General(apModifierBase modifier, apAnimClip targetAnimClip)
		{
			//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;
			//추가
			//요청한 Modifier가 BoneTransform을 지원하는 경우
			//Rigging은 비활성화 되어서는 안된다.
			apModifierBase.MODIFIER_TYPE[] exGeneralTypes = modifier.GetGeneralExEditableModTypes();
			if (exGeneralTypes == null)
			{
				exGeneralTypes = new apModifierBase.MODIFIER_TYPE[] { modifier.ModifierType };
			}

			for (int i = 0; i < _modifiers.Count; i++)
			{
				bool isValidType = false;
				for (int iGT = 0; iGT < exGeneralTypes.Length; iGT++)
				{
					if (exGeneralTypes[iGT] == _modifiers[i].ModifierType)
					{
						isValidType = true;
						break;
					}
				}

				if (isValidType)
				{
					//AnimClip을 포함하는 ParamSetGroup에 한해서 
					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
						if (paramSetGroup._keyAnimClip == targetAnimClip)
						{
							paramSetGroup._isEnabledExclusive = true;
						}
						else
						{
							paramSetGroup._isEnabledExclusive = false;
						}
					}
				}
				else
				{
					//지원하는 타입이 아니다.
					//모두 Disabled한다.
					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
					{
						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
						paramSetGroup._isEnabledExclusive = false;
					}
				}
			}

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				if (_parentMeshGroup._childMeshGroupTransforms != null)
				{
					for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup_General(modifier, targetAnimClip);
						}
					}
				}
			}
		}



		// Add / Remove
		//----------------------------------------------------
		public void AddModifier(apModifierBase modifier, apModifierBase.MODIFIER_TYPE modifierType)
		{
			switch (modifierType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:

					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					_modifiers_Volume.Add((apModifier_Volume)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					_modifiers_Morph.Add((apModifier_Morph)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					_modifiers_AnimatedMorph.Add((apModifier_AnimatedMorph)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					_modifiers_Rigging.Add((apModifier_Rigging)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Physic:
					_modifiers_Physic.Add((apModifier_Physic)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					_modifiers_TF.Add((apModifier_TF)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					_modifiers_AnimatedTF.Add((apModifier_AnimatedTF)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					_modifiers_FFD.Add((apModifier_FFD)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					_modifiers_AnimatedFFD.Add((apModifier_AnimatedFFD)modifier);
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입 [" + modifier + "]");
					break;
			}
		}


		public void RemoveModifier(apModifierBase modifier)
		{
			apModifierBase.MODIFIER_TYPE modType = modifier.ModifierType;

			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:

					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					_modifiers_Volume.Remove((apModifier_Volume)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					_modifiers_Morph.Remove((apModifier_Morph)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					_modifiers_AnimatedMorph.Remove((apModifier_AnimatedMorph)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					_modifiers_Rigging.Remove((apModifier_Rigging)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Physic:
					_modifiers_Physic.Remove((apModifier_Physic)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					_modifiers_TF.Remove((apModifier_TF)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					_modifiers_AnimatedTF.Remove((apModifier_AnimatedTF)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					_modifiers_FFD.Remove((apModifier_FFD)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					_modifiers_AnimatedFFD.Remove((apModifier_AnimatedFFD)modifier);
					break;
			}
		}


		// Link
		//----------------------------------------------------
		public void ClearAllCalculateParams()
		{
			for (int i = 0; i < _modifiers.Count; i++)
			{
				_modifiers[i]._calculatedResultParams.Clear();
			}
			//렌더 유닛들의 Stack도 리셋한다.
			for (int i = 0; i < _parentMeshGroup._renderUnits_All.Count; i++)
			{
				apRenderUnit renderUnit = _parentMeshGroup._renderUnits_All[i];
				renderUnit._calculatedStack.ClearResultParams();
			}
		}


		public void LinkModifierStackToRenderUnitCalculateStack(bool isRoot = true, apMeshGroup rootMeshGroup = null)
		{
			//전체 Modifier중에서 RenderUnit을 포함한 Modifer를 찾는다.
			//그 중, RenderUnit에 대한것만 처리할 CalculateResultParam을 만들고 연동한다.
			//ResultParam을 RenderUnit의 CalculateStack에 넣는다.

			//Debug.Log("--------------------------------------------------------------");
			//Debug.Log("LinkModifierStackToRenderUnitCalculateStack [" + _parentMeshGroup._name + "]");
			//Debug.Log("--------------------------------------------------------------");

			//수정
			//각 ModMesh에서 계층적인 Link를 할 수 있도록
			//RenderUnit을 매번 바꾸어주자
			if (isRoot)
			{
				rootMeshGroup = _parentMeshGroup;

				//Modifier-ParamSetGroup-ParamSet + ModMesh가 "실제 RenderUnit"과 링크되지 않으므로
				//Calculate Param을 만들기 전에 이 링크를 먼저 해주어야 한다.
			}



			//Modifier를 돌면서 ParamSet 데이터를 Calculated 데이터로 변환해서 옮긴다.
			for (int iMod = 0; iMod < _modifiers.Count; iMod++)
			{
				//Modifier ->..
				apModifierBase modifier = _modifiers[iMod];

				List<apModifierParamSetGroup> paramSetGroups = modifier._paramSetGroup_controller;

				for (int iGroup = 0; iGroup < paramSetGroups.Count; iGroup++)
				{
					//Modifier -> ParamSetGroup ->..
					apModifierParamSetGroup paramSetGroup = paramSetGroups[iGroup];

					List<apModifierParamSet> paramSets = paramSetGroup._paramSetList;

					for (int iParam = 0; iParam < paramSets.Count; iParam++)
					{
						//Modifier -> ParamSetGroup -> ParamSet ->...
						apModifierParamSet paramSet = paramSets[iParam];

						List<apModifiedMesh> modMeshes = paramSet._meshData;
						List<apModifiedBone> modBones = paramSet._boneData;

						//1. Mod Mesh => Calculate Param으로 연결한다.
						for (int iModMesh = 0; iModMesh < modMeshes.Count; iModMesh++)
						{
							//[핵심]
							//Modifier -> ParamSetGroup -> ParamSet -> ModMeh 
							//이제 이 ModMesh와 타겟 Transform을 연결하자.
							//연결할땐 Calculated 오브젝트를 만들어서 연결
							apModifiedMesh modMesh = modMeshes[iModMesh];

							#region [미사용 코드]
							//여기서 수정
							//Root가 아닐때 > RenderUnit을 자체적으로 세팅할게 아니라, Root MeshGroup을 기준으로 RenderUnit을 찾자
							//if(!isRoot)
							//{	
							//	apRenderUnit recursiveRenderUnit = null;
							//	if(modMesh._isMeshTransform)
							//	{
							//		recursiveRenderUnit = rootMeshGroup.GetRenderUnit(modMesh._transform_Mesh);
							//	}
							//	else
							//	{
							//		recursiveRenderUnit = rootMeshGroup.GetRenderUnit(modMesh._transform_MeshGroup);
							//	}
							//	if(recursiveRenderUnit != null)
							//	{	
							//		//Debug.Log("Link ModStack -> Child Render Unit Changed [Modifier : " + modifier.DisplayName + "] / RenderUnit Name : " + recursiveRenderUnit.Name + " / is Changed : " + (modMesh._renderUnit != recursiveRenderUnit));
							//		//if(modMesh._renderUnit == null)
							//		//{
							//		//	Debug.LogError("기존 RenderUnit이 Null이다.");
							//		//}
							//		//else if(modMesh._renderUnit._meshGroup == null)
							//		//{
							//		//	Debug.LogError("기존 RenderUnit의 MeshGroup이 Null이다.");
							//		//}
							//		//else
							//		//{
							//		//	Debug.Log("[" + modMesh._renderUnit._meshGroup._name + " > " + rootMeshGroup._name + "]");
							//		//}

							//		modMesh._renderUnit = recursiveRenderUnit;
							//	}
							//	else
							//	{
							//		Debug.LogError("Re Link Failed");
							//	}
							//}

							#endregion

							if (modMesh._renderUnit == null)
							{
								continue;
							}
							//이미 만든 Calculate Param이 있는지 확인
							apCalculatedResultParam existParam = modifier.GetCalculatedResultParam(modMesh._renderUnit);

							//추가 : 만약 Calculated Param을 찾지 못했다면..
							//Parent의 누군가가 이미 만들었을 수 있다!
							//Root Parent MeshGroup에 요청해서 한번 더 확인하자
							//(Calculated Result Param을 공유할 수 있기 때문)
							if (existParam == null && rootMeshGroup != null)
							{
								//rootMeshGroup._modifierStack
								//?? 이거 해야하나
							}

							apModifierParamSetGroupVertWeight weightedVertexData = null;
							if (modMesh._transform_Mesh != null)
							{
								weightedVertexData = paramSetGroup.GetWeightVertexData(modMesh._transform_Mesh);
							}

							if (existParam != null)
							{
								//Debug.Log("> ModMesh [" + iModMesh + "] : " + modMesh._transformUniqueID + "< Add >");
								existParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, modMesh, null);
								existParam.LinkWeightedVertexData(weightedVertexData);

								//if(!isRoot)
								//{
								//	Debug.LogWarning("Child Modifier의 CalculateParam을 찾아서 적용 : "
								//		+ modifier.DisplayName + " / " + existParam._debugID + " / " + existParam._targetRenderUnit.Name);
								//}
							}
							else
							{
								//Debug.Log("> ModMesh [" + iModMesh + "] : " + modMesh._transformUniqueID + "< New >");
								//새로 Calculate Param을 만들고..
								apCalculatedResultParam newCalParam = new apCalculatedResultParam(
									modifier.CalculatedValueType,
									modifier.CalculatedSpace,
									modifier,
									modMesh._renderUnit,
									null,//<Bone은 없으닝께..
									weightedVertexData
									);

								newCalParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, modMesh, null);

								// Modifier에 등록하고
								modifier._calculatedResultParams.Add(newCalParam);

								//RenderUnit에도 등록을 하자
								modMesh._renderUnit._calculatedStack.AddCalculatedResultParam(newCalParam);


								//if(!isRoot)
								//{
								//	Debug.LogWarning("Child Modifier의 CalculateParam을 찾아서 적용 : "
								//		+ modifier.DisplayName + " / " + newCalParam._debugID + " / " + newCalParam._targetRenderUnit.Name);
								//}
							}
							//else
							//{
							//	Debug.LogError("Link Modifier Stack Error : No Render Unit (isRoot : " + isRoot + ")");
							//}
						}

						//2. Mod Bone => Calculate Param으로 연결한다.
						for (int iModBone = 0; iModBone < modBones.Count; iModBone++)
						{
							apModifiedBone modBone = modBones[iModBone];

							if (modBone._bone == null || modBone._renderUnit == null)
							{
								Debug.LogError("ModBone -> Calculate Link 실패 : [Bone : " + (modBone._bone != null) + ", RenderUnit : " + (modBone._renderUnit != null) + "]");
								continue;
							}

							//이미 만든 Calculate Param이 있는지 확인
							apCalculatedResultParam existParam = modifier.GetCalculatedResultParam_Bone(modBone._renderUnit, modBone._bone);

							if (existParam != null)
							{
								//이미 있다면 ModBone만 추가해주자
								existParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, null, modBone);
							}
							else
							{
								//Debug.Log("Mod Bone -> Calculate Param 등록");
								//새로 CalculateParam을 만들고
								apCalculatedResultParam newCalParam = new apCalculatedResultParam(
									modifier.CalculatedValueType,
									modifier.CalculatedSpace,
									modifier,
									modBone._renderUnit,
									modBone._bone,
									null//WeightedVertex
									);

								newCalParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, null, modBone);

								// Modifier에 등록하고
								modifier._calculatedResultParams.Add(newCalParam);

								//RenderUnit에도 등록을 하자
								modBone._renderUnit._calculatedStack.AddCalculatedResultParam(newCalParam);
							}
						}
					}
				}


				//SubList를 한번 정렬하자
				for (int iCal = 0; iCal < modifier._calculatedResultParams.Count; iCal++)
				{
					modifier._calculatedResultParams[iCal].SortSubList();
				}
			}

			//추가>>
			//하위 객체에 대해서도 Link를 자동으로 수행한다.
			//다 끝나고 Sort
			List<apTransform_MeshGroup> childMeshGroupTransforms = _parentMeshGroup._childMeshGroupTransforms;

			apTransform_MeshGroup childMeshGroup = null;

			if (childMeshGroupTransforms != null && childMeshGroupTransforms.Count > 0)
			{
				for (int i = 0; i < childMeshGroupTransforms.Count; i++)
				{
					childMeshGroup = childMeshGroupTransforms[i];
					if (childMeshGroup._meshGroup != null && childMeshGroup._meshGroup != _parentMeshGroup)
					{
						childMeshGroup._meshGroup._modifierStack.LinkModifierStackToRenderUnitCalculateStack(false, rootMeshGroup);//<<여기서도 같이 수행
					}
				}
			}

			if (isRoot)
			{
				//Debug.Log("Start Sort : " + _parentMeshGroup._name);
				//Root인 경우
				//RenderUnit들을 검사하면서 Calculated Stack에 대해서 Sort를 해주자
				List<apRenderUnit> renderUnits = _parentMeshGroup._renderUnits_All;
				for (int i = 0; i < renderUnits.Count; i++)
				{
					renderUnits[i]._calculatedStack.Sort();
				}
			}

		}


		/// <summary>
		/// Modifier들의 계산 값들을 초기화한다.
		/// </summary>
		public void InitModifierCalculatedValues()
		{
			for (int iMod = 0; iMod < _modifiers.Count; iMod++)
			{
				//Modifier ->..
				apModifierBase modifier = _modifiers[iMod];

				List<apModifierParamSetGroup> paramSetGroups = modifier._paramSetGroup_controller;

				for (int iGroup = 0; iGroup < paramSetGroups.Count; iGroup++)
				{
					//Modifier -> ParamSetGroup ->..
					apModifierParamSetGroup paramSetGroup = paramSetGroups[iGroup];

					List<apModifierParamSet> paramSets = paramSetGroup._paramSetList;

					for (int iParam = 0; iParam < paramSets.Count; iParam++)
					{
						//Modifier -> ParamSetGroup -> ParamSet ->...
						apModifierParamSet paramSet = paramSets[iParam];

						List<apModifiedMesh> modMeshes = paramSet._meshData;
						List<apModifiedBone> modBones = paramSet._boneData;

						for (int iModMesh = 0; iModMesh < modMeshes.Count; iModMesh++)
						{
							apModifiedMesh modMesh = modMeshes[iModMesh];
							if (modMesh._vertices != null && modMesh._vertices.Count > 0)
							{
								//ModVert 초기화 => 현재는 초기화 할게 없다.

							}
							if (modMesh._vertRigs != null && modMesh._vertRigs.Count > 0)
							{
								//ModVertRig 초기화 => 현재는 초기화 할게 없다.
							}
							if (modMesh._vertWeights != null && modMesh._vertWeights.Count > 0)
							{
								apModifiedVertexWeight vertWeight = null;
								for (int iVW = 0; iVW < modMesh._vertWeights.Count; iVW++)
								{
									vertWeight = modMesh._vertWeights[iVW];
									vertWeight.InitCalculatedValue();//<<초기화를 하자. (여기서는 물리값)
								}
							}
						}

						for (int iModBone = 0; iModBone < modBones.Count; iModBone++)
						{
							apModifiedBone modBone = modBones[iModBone];
							//ModBone도 현재는 초기화 할게 없다.
						}
					}
				}
			}
		}


		// Get / Set
		//----------------------------------------------------
		public int GetNewModifierID(int modifierType, int validationKey)
		{
			return apVersion.I.GetNextModifierID(modifierType, validationKey, IsModifierExist);
		}

		public apModifierBase GetModifier(int uniqueID)
		{
			return _modifiers.Find(delegate (apModifierBase a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public bool IsModifierExist(int uniqueID)
		{
			return _modifiers.Exists(delegate (apModifierBase a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public int GetLastLayer()
		{
			int maxLayer = -1;
			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (maxLayer < _modifiers[i]._layer)
				{
					maxLayer = _modifiers[i]._layer;
				}
			}
			return maxLayer;

		}



	}
}
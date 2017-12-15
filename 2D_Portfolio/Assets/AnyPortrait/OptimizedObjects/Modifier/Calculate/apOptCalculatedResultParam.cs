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

	public class apOptCalculatedResultParam
	{
		// Members
		//--------------------------------------------
		public apModifierParamSetGroup.SYNC_TARGET _inputType = apModifierParamSetGroup.SYNC_TARGET.Controller;

		public apCalculatedResultParam.CALCULATED_VALUE_TYPE _calculatedValueType = apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos;
		public apCalculatedResultParam.CALCULATED_SPACE _calculatedSpace = apCalculatedResultParam.CALCULATED_SPACE.Local;

		//연결된 모디파이어
		public apOptModifierUnitBase _linkedModifier = null;

		//타겟 Opt Transform
		public apOptTransform _targetOptTransform = null;

		//타겟 Opt의 Child Mesh (존재한다면)
		public apOptMesh _targetOptMesh = null;

		//추가 : 타겟 Bone
		public apOptBone _targetBone = null;


		//Vertex 가중치 적용 데이터
		public apOptParamSetGroupVertWeight _weightedVertexData = null;

		//결과값
		public List<Vector2> _result_Positions = null;
		public apMatrix _result_Matrix = new apMatrix();
		public Color _result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		public bool _result_IsVisible = true;

		//처리를 위한 임시값
		public List<Vector2> _tmp_Positions = null;

		public bool _isAvailable = true;

		public bool _isColorCalculated = true;//Color 계산이 이루어졌는가

		public bool _isAnimModifier = false;

		public float _totalParamSetGroupWeight = 0.0f;

		public bool IsColorValueEnabled
		{
			get
			{
				if (_linkedModifier != null)
				{
					return _linkedModifier._isColorPropertyEnabled;
				}
				return false;
			}
		}




		/// <summary>
		/// 키프레임 / ControlParam에 따라 적용되는 데이터 Set
		/// 에디터에서의 ParamKeyValueSet에 해당한다.
		/// </summary>
		public class OptParamKeyValueSet
		{
			public apOptParamSetGroup _keyParamSetGroup = null;
			public apOptParamSet _paramSet = null;
			public apOptModifiedMesh _modifiedMesh = null;
			public apOptModifiedBone _modifiedBone = null;

			public float _dist = -1.0f;
			public float _weight = -1.0f;
			public bool _isCalculated = false;
			public int _layerIndex = -1;

			/// <summary>
			/// ModMesh와 연동되는 ParamKeyValue 생성
			/// </summary>
			public OptParamKeyValueSet(apOptParamSetGroup keyParamSetGroup, apOptParamSet paramSet, apOptModifiedMesh modifiedMesh)
			{
				_keyParamSetGroup = keyParamSetGroup;
				_paramSet = paramSet;
				_modifiedMesh = modifiedMesh;
				_layerIndex = _keyParamSetGroup._layerIndex;

				_modifiedBone = null;
			}


			/// <summary>
			/// ModBone과 연동되는 ParamKeyValue 생성
			/// </summary>
			public OptParamKeyValueSet(apOptParamSetGroup keyParamSetGroup, apOptParamSet paramSet, apOptModifiedBone modifiedBone)
			{
				_keyParamSetGroup = keyParamSetGroup;
				_paramSet = paramSet;
				_modifiedMesh = null;
				_layerIndex = _keyParamSetGroup._layerIndex;

				_modifiedBone = modifiedBone;
			}

			public void ReadyToCalculate()
			{
				_dist = -1.0f;
				_weight = -1.0f;
				_isCalculated = false;
			}
		}

		public List<OptParamKeyValueSet> _paramKeyValues = new List<OptParamKeyValueSet>();
		public List<apOptCalculatedResultParamSubList> _subParamKeyValueList = new List<apOptCalculatedResultParamSubList>();

		// Init
		//--------------------------------------------
		public apOptCalculatedResultParam(apCalculatedResultParam.CALCULATED_VALUE_TYPE calculatedValueType,
											apCalculatedResultParam.CALCULATED_SPACE calculatedSpace,
											apOptModifierUnitBase linkedModifier,
											apOptTransform targetOptTranform,
											apOptMesh targetOptMesh,
											apOptBone targetBone,//<<추가
											apOptParamSetGroupVertWeight weightedVertData)
		{
			_calculatedValueType = calculatedValueType;
			_calculatedSpace = calculatedSpace;

			//TODO 여기서부터 작업하자
			_linkedModifier = linkedModifier;
			_targetOptTransform = targetOptTranform;
			_targetOptMesh = targetOptMesh;
			_targetBone = targetBone;//<<추가



			_weightedVertexData = weightedVertData;

			//Vertex 데이터가 들어간 경우 Vert 리스트를 만들어주자
			if ((int)(_calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
			{
				int nPos = 0;
				if (_targetOptMesh.LocalVertPositions != null)
				{
					nPos = _targetOptMesh.LocalVertPositions.Length;
				}

				_result_Positions = new List<Vector2>();
				_tmp_Positions = new List<Vector2>();
				for (int i = 0; i < nPos; i++)
				{
					_result_Positions.Add(Vector2.zero);
					_tmp_Positions.Add(Vector2.zero);
				}
			}

		}

		public void LinkWeightedVertexData(apOptParamSetGroupVertWeight weightedVertData)
		{
			_weightedVertexData = weightedVertData;
		}


		/// <summary>
		/// ParamSet을 받아서 SubList와 연동한다.
		/// </summary>
		/// <param name="paramSet"></param>
		/// <returns></returns>
		public void AddParamSetAndModifiedValue(apOptParamSetGroup paramSetGroup,
												apOptParamSet paramSet,
												apOptModifiedMesh modifiedMesh,
												apOptModifiedBone modifiedBone)
		{
			OptParamKeyValueSet existSet = GetParamKeyValue(paramSet);

			if (existSet != null)
			{
				//이미 존재한 값이라면 패스
				return;
			}

			//새로운 KeyValueSet을 만들어서 리스트에 추가하자
			//Mod Mesh 또는 Mod Bone 둘중 하나를 넣어서 ParamKeyValueSet을 구성하자
			OptParamKeyValueSet newKeyValueSet = null;
			if (modifiedMesh != null)
			{
				newKeyValueSet = new OptParamKeyValueSet(paramSetGroup, paramSet, modifiedMesh);
			}
			else if (modifiedBone != null)
			{
				newKeyValueSet = new OptParamKeyValueSet(paramSetGroup, paramSet, modifiedBone);
			}
			else
			{
				Debug.LogError("AddParamSetAndModifiedMesh Error : ModifiedMesh와 ModifiedBone이 모두 Null이다.");
				return;
			}

			_paramKeyValues.Add(newKeyValueSet);

			apOptCalculatedResultParamSubList targetSubList = null;

			apOptCalculatedResultParamSubList existSubList = _subParamKeyValueList.Find(delegate (apOptCalculatedResultParamSubList a)
		   {
			   return a._keyParamSetGroup == paramSetGroup;
		   });

			//같이 묶여서 작업할 SubList가 있는가
			if (existSubList != null)
			{
				targetSubList = existSubList;
			}
			else
			{
				//없으면 만든다.
				targetSubList = new apOptCalculatedResultParamSubList(this);
				targetSubList.SetParamSetGroup(paramSetGroup);

				_subParamKeyValueList.Add(targetSubList);
			}

			//해당 SubList에 위에서 만든 KeyValueSet을 추가하자
			if (targetSubList != null)
			{
				targetSubList.AddParamKeyValueSet(newKeyValueSet);
			}

			_isAnimModifier = (paramSetGroup._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame);
		}

		public void SortSubList()
		{
			_subParamKeyValueList.Sort(delegate (apOptCalculatedResultParamSubList a, apOptCalculatedResultParamSubList b)
			{
				if (a._keyParamSetGroup == null || b._keyParamSetGroup == null)
				{
					return 0;
				}

				return a._keyParamSetGroup._layerIndex - b._keyParamSetGroup._layerIndex;//오른차순 정렬
		});

			for (int i = 0; i < _subParamKeyValueList.Count; i++)
			{
				_subParamKeyValueList[i].MakeMetaData();
			}
		}



		// Functions
		//--------------------------------------------
		public void InitCalculate()
		{
			for (int i = 0; i < _subParamKeyValueList.Count; i++)
			{
				_subParamKeyValueList[i].InitCalculate();
			}

			_totalParamSetGroupWeight = 0.0f;
		}

		/// <summary>
		/// Calculate Result 계산을 한다. (키프레임이나 컨트롤 파라미터 가중치)
		/// </summary>
		/// <returns>True : 이 CalResult는 업데이트 해야한다. / False : 모든 Sub ParamValue가 업데이트 되지 않는다.</returns>
		public bool Calculate()
		{
			bool isUpdatable = false;

			_totalParamSetGroupWeight = 0.0f;

#if UNITY_EDITOR
			Profiler.BeginSample("Calcualte Result Param - Calculate");
#endif
			bool isResult = false;

			if (_isAnimModifier)
			{
				bool isNeedSort = false;
				//추가
				//애니메이션 타입인 경우
				//재정렬이 필요한지 체크한다.
				for (int i = 0; i < _subParamKeyValueList.Count; i++)
				{
					//여기서 애니메이션을 계산하고 UnitWeight를 LayerWeight로 저장한다.
					if (_subParamKeyValueList[i].UpdateAnimLayer())
					{
						//Layer의 변화가 있었다.
						//Sort를 하자
						isNeedSort = true;
					}
				}
				if (isNeedSort)
				{
					//Debug.Log("Reorder / AnimClip");
					//정렬을 다시 하자
					SortSubList();
				}
			}


			for (int i = 0; i < _subParamKeyValueList.Count; i++)
			{
				isResult = _subParamKeyValueList[i].Calculate();
				if (isResult)
				{
					isUpdatable = true;
				}
			}

#if UNITY_EDITOR
			Profiler.EndSample();
#endif

			return isUpdatable;
		}





		// Get / Set
		//--------------------------------------------
		public int ModifierLayer { get { return _linkedModifier._layer; } }
		public apModifierBase.BLEND_METHOD ModifierBlendMethod { get { return _linkedModifier._blendMethod; } }
		public float ModifierWeight
		{
			get
			{
				//return _linkedModifier._layerWeight;

				//수정 >> 
				return Mathf.Clamp01(_linkedModifier._layerWeight * Mathf.Clamp01(_totalParamSetGroupWeight));
			}
		}
		public bool IsModifierAvailable { get { return _isAvailable; } }

		public OptParamKeyValueSet GetParamKeyValue(apOptParamSet paramSet)
		{
			return _paramKeyValues.Find(delegate (OptParamKeyValueSet a)
			{
				return a._paramSet == paramSet;
			});
		}
	}

}
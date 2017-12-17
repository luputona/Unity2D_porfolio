﻿/*
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
	public class apAnimKeyframe
	{
		// Members
		//-----------------------------------------------------------------------
		public int _uniqueID = -1;//<<키프레임마다 unique ID가 있다.

		public int _frameIndex = -1;//<<어디에 배치되는가 (겹치면 안되며, 겹치면 하나가 삭제되어야함)

		[NonSerialized]
		public apAnimTimelineLayer _parentTimelineLayer = null;

		[SerializeField]
		public apAnimCurve _curveKey = new apAnimCurve();

		[NonSerialized]
		public apAnimKeyframe _prevLinkedKeyframe = null;

		[NonSerialized]
		public apAnimKeyframe _nextLinkedKeyframe = null;

		/// <summary>애니메이션 보간을 위해 연동된 값을 입력했는가 (그렇지 않다면 상대적인 보간 처리가 들어간다)</summary>
		public bool _isKeyValueSet = false;

		/// <summary>
		/// 이 키프레임은 활성화되어있는가 [AnimClip의 재생 영역 밖이면 비활성화되며 링크되지 않는다]
		/// </summary>
		public bool _isActive = false;

		//루프 양쪽의 프레임인 경우
		//해당 프레임은 더미 프레임으로 설정될 수 있다.
		//(Start Frame은) OverEndDummy로 설정된다. -> DummyIndex가 EndIndex 또는 그 이상에 붙는다.
		//(End Frame은) UnderStartDummy로 설정된다. -> DummyIndex가 StartIndex 또는 그 이하에 붙는다.
		public bool _isLoopAsStart = false;
		public bool _isLoopAsEnd = false;

		public int _loopFrameIndex = -1;

		//값이 적용되는 프레임 범위
		public int _activeFrameIndexMin = 0;
		public int _activeFrameIndexMax = 0;

		public int _activeFrameIndexMin_Dummy = 0;
		public int _activeFrameIndexMax_Dummy = 0;

		//Control Param 타입이면 
		//Control Param의 어떤 값에 동기화되는가
		//public bool _conSyncValue_Bool = false;
		public int _conSyncValue_Int = 0;
		public float _conSyncValue_Float = 0.0f;
		public Vector2 _conSyncValue_Vector2 = Vector2.zero;
		//public Vector3 _conSyncValue_Vector3 = Vector3.zero;
		//public Color _conSyncValue_Color = Color.black;


		//에디터용 빠른 접근 위한 변수
		[NonSerialized]
		public apModifierParamSet _linkedParamSet_Editor = null;

		[NonSerialized]
		public apModifiedMesh _linkedModMesh_Editor = null;

		[NonSerialized]
		public apModifiedBone _linkedModBone_Editor = null;

		// Init
		//-----------------------------------------------------------------------
		public apAnimKeyframe()
		{

		}


		public void Init(int uniqueID, int frameIndex)
		{
			_uniqueID = uniqueID;
			_frameIndex = frameIndex;

			_isLoopAsStart = false;
			_isLoopAsEnd = false;
			_loopFrameIndex = -1;

			//_conSyncValue_Bool = false;
			_conSyncValue_Int = 0;
			_conSyncValue_Float = 0.0f;
			_conSyncValue_Vector2 = Vector2.zero;
			//_conSyncValue_Vector3 = Vector3.zero;
			//_conSyncValue_Color = Color.black;

			_linkedParamSet_Editor = null;
			_linkedModMesh_Editor = null;
			_linkedModBone_Editor = null;
		}



		public void LinkModMesh_Editor(apModifierParamSet paramSet, apModifiedMesh modMesh)
		{
			_linkedParamSet_Editor = paramSet;
			_linkedModMesh_Editor = modMesh;
			_linkedModBone_Editor = null;
		}

		public void LinkModBone_Editor(apModifierParamSet paramSet, apModifiedBone modBone)
		{
			_linkedParamSet_Editor = paramSet;
			_linkedModMesh_Editor = null;
			_linkedModBone_Editor = modBone;
		}


		public void Link(apAnimTimelineLayer parentTimelineLayer)
		{
			_parentTimelineLayer = parentTimelineLayer;
			_parentTimelineLayer._parentAnimClip._portrait.RegistUniqueID(apIDManager.TARGET.AnimKeyFrame, _uniqueID);
		}

		public void SetInactive()
		{
			_isActive = false;
			_prevLinkedKeyframe = null;
			_nextLinkedKeyframe = null;
			_curveKey.SetLinkedCurveKey(null, null, _frameIndex, _frameIndex);


			_isLoopAsStart = false;
			_isLoopAsEnd = false;

			_loopFrameIndex = -1;
		}


		//public void SetLinkedKeyframes(apAnimKeyframe prevKeyframe, apAnimKeyframe nextKeyframe, bool isPrevDummyIndex, bool isNextDummyIndex)
		public void SetLinkedKeyframes(apAnimKeyframe prevKeyframe, apAnimKeyframe nextKeyframe, int prevFrameIndex, int nextFrameIndex)
		{
			_isActive = true;
			_prevLinkedKeyframe = prevKeyframe;
			_nextLinkedKeyframe = nextKeyframe;

			apAnimCurve prevCurveKey = null;
			apAnimCurve nextCurveKey = null;

			if (_prevLinkedKeyframe != null)
			{
				prevCurveKey = _prevLinkedKeyframe._curveKey;
			}
			if (_nextLinkedKeyframe != null)
			{
				nextCurveKey = _nextLinkedKeyframe._curveKey;
			}

			//_isLoopAsStart = false;
			//_isLoopAsEnd = false;
			//_loopFrameIndex = -1;

			//_curveKey.Set
			//_curveKey.SetLinkedCurveKey(prevCurveKey, nextCurveKey, isPrevDummyIndex, isNextDummyIndex);
			_curveKey.SetLinkedCurveKey(prevCurveKey, nextCurveKey, prevFrameIndex, nextFrameIndex);
		}


		/// <summary>
		/// 해당 프레임은 루프의 양쪽에 위치하여 더미프레임이 생성된다.
		/// StartFrame은 OverEnd 더미를 생성한다. (파라미터 True이며 인덱스를 +Length한다.
		/// EndFrame은 UnderStart 더미를 생성한다. (파라미터 False이며 인덱스를 -Length한다.
		/// </summary>
		/// <param name="isLoopAsStart"></param>
		/// <param name="dummyFrameIndex"></param>
		public void SetLoopFrame(bool isLoopAsStart, int dummyFrameIndex)
		{
			if (isLoopAsStart)
			{
				_isLoopAsStart = true;
				_isLoopAsEnd = false;
			}
			else
			{
				_isLoopAsStart = false;
				_isLoopAsEnd = true;
			}

			_loopFrameIndex = dummyFrameIndex;
			//if(isLoopAsStart)
			//{
			//	Debug.Log("Loop Start [" + _frameIndex + " > " + _loopFrameIndex + " ]");
			//}
			//if(_isLoopAsEnd)
			//{
			//	Debug.Log("Loop End [" + _frameIndex + " > " + _loopFrameIndex + " ]");
			//}

			//_curveKey.SetKeyIndex(_frameIndex, _loopFrameIndex);
			_curveKey.SetKeyIndex(_frameIndex);
		}

		public void SetDummyDisable()
		{
			_isLoopAsStart = false;
			_isLoopAsEnd = false;
			_loopFrameIndex = _frameIndex;
		}

		public bool IsFrameIn(int curFrame, bool isPrev)
		{
			if (isPrev)
			{
				if (_activeFrameIndexMin <= curFrame && curFrame <= _frameIndex)
				{
					return true;
				}
				if (_isLoopAsStart || _isLoopAsEnd)
				{
					if (_activeFrameIndexMin_Dummy <= curFrame && curFrame <= _loopFrameIndex)
					{
						return true;
					}
				}
				return false;
			}
			else
			{
				if (_frameIndex <= curFrame && curFrame <= _activeFrameIndexMax)
				{
					return true;
				}
				if (_isLoopAsStart || _isLoopAsEnd)
				{
					if (_loopFrameIndex <= curFrame && curFrame <= _activeFrameIndexMax_Dummy)
					{
						return true;
					}
				}
				return false;
			}
			//return false;
		}



		// Functions
		//-----------------------------------------------------------------------
		// 키프레임에서 "연동된 데이터"의 표면적인 값을 넣거나 상대적 처리임을 명시해주자
		public void SetKeyValue(float keyValue)
		{
			//_curveKey.SetKeyValue()
		}
		public void SetKeyValueRelative()
		{
			_isKeyValueSet = false;
		}



		public void RefreshCurveKey()
		{
			int dummyFrameIndex = _frameIndex;
			if (_isLoopAsStart || _isLoopAsEnd)
			{
				dummyFrameIndex = _loopFrameIndex;
			}
			//_curveKey.SetKeyIndex(_frameIndex, dummyFrameIndex);
			_curveKey.SetKeyIndex(_frameIndex);
			//_curveKey.CalculateSmooth();
		}
		// Get / Set
		//-----------------------------------------------------------------------
	}

}
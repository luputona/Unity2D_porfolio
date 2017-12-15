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
	/// AnimPlayUnit을 가지고 있는 유닛
	/// Queue
	/// </summary>
	public class apAnimPlayQueue
	{
		// Members
		//----------------------------------------------
		public apPortrait _portrait = null;
		public apAnimPlayManager _playManager = null;

		public List<apAnimPlayUnit> _animPlayUnits = new List<apAnimPlayUnit>();

		//TODO : 블렌드하자 + Request 받고 천천히 처리 방식

		private int _layer = -1;
		private int _nPlayedUnit = 0;


		private bool _isAnyUnitChanged = false;

		private apAnimPlayUnit _tmpCurPlayUnit = null;
		//private float _totalWeight = 0.0f;

		private List<apAnimPlayRequest> _requests_Live = new List<apAnimPlayRequest>();

		//RequestPool을 만들어서 관리한다.
		private List<apAnimPlayRequest> _requests_Total = new List<apAnimPlayRequest>();
		private List<apAnimPlayRequest> _requests_Remained = new List<apAnimPlayRequest>();

		private const int NUM_REQUEST_POOL_SIZE = 20;//기본 20개 사이즈를 가진다.

		private const float DEFAULT_FADE_TIME = 0.2f;

		// Init
		//----------------------------------------------
		public apAnimPlayQueue(int layer, apPortrait portrait, apAnimPlayManager playManager)
		{
			_layer = layer;
			_portrait = portrait;
			_playManager = playManager;


			Clear();
			_isAnyUnitChanged = false;

			_requests_Live.Clear();
			_requests_Total.Clear();
			_requests_Remained.Clear();

			AddRequestPool();//<<Pool을 만든다.

		}

		/// <summary>
		/// 초기화의 Clear이다. 
		/// 애니메이션을 정지시킬때는 Stop 함수를 써야한다. (그래야 Modifier에서 인식을 한다)
		/// </summary>
		public void Clear()
		{
			_animPlayUnits.Clear();
			_nPlayedUnit = 0;

			PushAllRequests();
		}

		// Functions
		//----------------------------------------------
		// 기본 함수들
		private void AddRequestPool()
		{
			for (int i = 0; i < NUM_REQUEST_POOL_SIZE; i++)
			{
				apAnimPlayRequest newRequest = new apAnimPlayRequest();
				_requests_Total.Add(newRequest);
				_requests_Remained.Add(newRequest);
			}
		}

		private void PushAllRequests()
		{
			apAnimPlayRequest curRequest = null;
			for (int i = 0; i < _requests_Live.Count; i++)
			{
				curRequest = _requests_Live[i];
				curRequest.Clear();

				if (!_requests_Total.Contains(curRequest))
				{
					_requests_Total.Add(curRequest);
				}
				if (!_requests_Remained.Contains(curRequest))
				{
					_requests_Remained.Add(curRequest);
				}
			}

			_requests_Live.Clear();
		}

		private apAnimPlayRequest PopRequest()
		{
			apAnimPlayRequest popRequest = null;
			if (_requests_Remained.Count == 0)
			{
				AddRequestPool();//<<Pool Size를 늘리자
			}
			popRequest = _requests_Remained[0];

			//Remained -> Live
			_requests_Remained.RemoveAt(0);
			_requests_Live.Add(popRequest);

			return popRequest;
		}

		private void PushRequest(apAnimPlayRequest request)
		{
			request.ReleasePlayUnitLink();
			request.Clear();
			_requests_Live.Remove(request);

			if (!_requests_Total.Contains(request))
			{
				_requests_Total.Add(request);
			}
			if (!_requests_Remained.Contains(request))
			{
				_requests_Remained.Add(request);
			}
		}



		private apAnimPlayUnit MakePlayUnit(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop)
		{
			//새로 만들고
			//그 전에..
			//재생중인 PlayUnit이 있으면 그걸 사용하자
			//레이어는 같아야 한다.
			apAnimPlayUnit existPlayUnit = null;
			for (int i = 0; i < _animPlayUnits.Count; i++)
			{
				if (_animPlayUnits[i]._linkedAnimClip == playData._linkedAnimClip
					&& _animPlayUnits[i].IsUpdatable

					)
				{
					existPlayUnit = _animPlayUnits[i];
					break;
				}
			}
			if (existPlayUnit != null)
			{
				//Debug.Log("아직 재생중인 PlayUnit을 다시 재생하는 요청이 왔다. [" + existPlayUnit._linkedAnimClip._name + "]");
				existPlayUnit.SetSubOption(blendMethod, isAutoEndIfNotloop);

				_nPlayedUnit = _animPlayUnits.Count;
				return existPlayUnit;
			}

			apAnimPlayUnit newPlayUnit = new apAnimPlayUnit(this);
			newPlayUnit.SetAnimClip(playData, _layer, blendMethod, isAutoEndIfNotloop, false);

			//리스트에 넣자
			_animPlayUnits.Add(newPlayUnit);

			_nPlayedUnit = _animPlayUnits.Count;
			return newPlayUnit;
		}

		//----------------------------------------------------
		// 재생/정지 요청 함수들
		//----------------------------------------------------

		/// <summary>
		/// AnimClip을 PlayUnit에 담아서 재생한다.
		/// Queue에 저장된 모든 클립은 무시되며 블렌드되지 않는다.
		/// </summary>
		/// <param name="blendMethod"></param>
		/// <param name="isAutoEndIfNotloop">True이면 Clip의 재생 후 자동으로 종료한다. (Loop일 경우 무시됨)</param>
		public apAnimPlayUnit Play(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, float blendTime = 0.0f, bool isAutoEndIfNotloop = true)
		{

			//Request를 생성한다.
			apAnimPlayRequest request = PopRequest();
			request.SetCurrentPlayedUnits(this, _animPlayUnits);


			apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);

			//newPlayUnit.Play();

			//Play 명령을 준다.
			request.PlayNew(newPlayUnit, blendTime);

			#region [미사용 코드]
			//TODO : 이 AnimClip을 CalculatedParam에 연결해야한다.
			//Debug.LogError("TODO : 이 AnimClip을 CalculatedParam에 연결해야한다");

			////플레이 유닛은 플레이 시작
			////나머지는 End로 만든다.
			//for (int i = 0; i < _animPlayUnits.Count; i++)
			//{
			//	if (newPlayUnit != _animPlayUnits[i])
			//	{
			//		_animPlayUnits[i].SetEnd();
			//	}
			//} 
			#endregion

			_nPlayedUnit = _animPlayUnits.Count;



			//Debug.Log("Next Play Units [" + _nPlayedUnit + "]");
			return newPlayUnit;
		}


		/// <summary>
		/// AnimClip을 PlayUnit에 담아서 재생한다.
		/// Queue에 저장된 클립들이 모두 끝나면 블렌드 없이 바로 실행된다.
		/// </summary>
		/// <param name="blendMethod"></param>
		/// <param name="isAutoEndIfNotloop">True이면 Clip의 재생 후 자동으로 종료한다. (Loop일 경우 무시됨)</param>
		/// <returns></returns>
		public apAnimPlayUnit PlayQueued(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, float blendTime = 0.0f, bool isAutoEndIfNotloop = true)
		{
			//현재 재생되는 플레이 유닛 중에서 "가장 많이 남은 플레이 시간"을 기준으로 타이머를 잡자
			//Fade 타임은 없고, 자동 삭제 타이머 + 자동 재생 대기 타이머를 지정

			//현재 Queue에 있는 객체가 없다면 Play와 동일하다
			if (_nPlayedUnit == 0)
			{
				return Play(playData, blendMethod, blendTime, isAutoEndIfNotloop);
			}

			//마지막 PlayUnit을 가져오자
			apAnimPlayUnit lastPlayUnit = _animPlayUnits[_animPlayUnits.Count - 1];
			if (lastPlayUnit.IsLoop)
			{
				//만약 마지막 PlayUnit이 Loop라면 => Queued 되지 않는다. 자동으로 [Play]로 바뀜
				return Play(playData, blendMethod, blendTime, isAutoEndIfNotloop);
			}

			//Request를 생성한다.
			apAnimPlayRequest request = PopRequest();
			request.SetCurrentPlayedUnits(this, _animPlayUnits);


			apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);
			newPlayUnit.Pause();

			//Play Queued 명령을 준다.
			request.PlayQueued(newPlayUnit, lastPlayUnit, blendTime);




			#region [미사용 코드] 미리 시간을 예상해서 처리하는 것은 문제가 많다.
			//float maxRemainPlayTime = -1.0f;
			//float curRemainPlayTime = 0.0f;
			//bool isAnyOnceAnimClip = false;
			//for (int i = 0; i < _nPlayedUnit; i++)
			//{
			//	_tmpCurPlayUnit = _animPlayUnits[i];
			//	if(_tmpCurPlayUnit.IsLoop)
			//	{
			//		//하나라도 루프이면 실패다.
			//		//Queue에 넣어도 작동하지 않는다.

			//		//>수정
			//		//루프는 무시하고 Queue 시간을 잡자
			//		//만약 Loop를 만나고 Queue가 있다면 그냥 기본값인 0.5초를 Queue 시간으로 쓴다.

			//		//Debug.LogError("PlayQueued Failed : Any Clip has Loop Option. Adding to Queue will be ignored");
			//		//return null;
			//		continue;
			//	}

			//	isAnyOnceAnimClip = true;
			//	curRemainPlayTime = _tmpCurPlayUnit.GetRemainPlayTime;
			//	if(maxRemainPlayTime < curRemainPlayTime)
			//	{
			//		maxRemainPlayTime = curRemainPlayTime;
			//	}
			//}

			//if(!isAnyOnceAnimClip)
			//{
			//	maxRemainPlayTime = 0.5f;
			//}
			//if(maxRemainPlayTime < 0.0f)
			//{
			//	maxRemainPlayTime = 0.0f;
			//}

			////최대 RemainPlayTime 만큼 Delay한다.
			//// Delay후 신규 플레이 또는 플레이 종료를 한다.
			////Fade 시간은 0

			//apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);
			//newPlayUnit.Play(0.0f, maxRemainPlayTime);

			////Debug.LogError("TODO : 이 AnimClip을 CalculatedParam에 연결해야한다");

			//for (int i = 0; i < _nPlayedUnit; i++)
			//{
			//	_tmpCurPlayUnit = _animPlayUnits[i];
			//	if (newPlayUnit != _tmpCurPlayUnit)
			//	{	
			//		_tmpCurPlayUnit.FadeOut(0.0f, maxRemainPlayTime);
			//	}
			//} 
			#endregion

			_nPlayedUnit = _animPlayUnits.Count;

			return newPlayUnit;
		}





		#region [미사용 코드] CrossFade 대신 Play에서 BlendTime을 넣자
		///// <summary>
		///// AnimClip을 PlayUnit에 담아서 바로 재생한다.
		///// Queue에 저장된 모든 클립에 바로 FadeOut을 지정하여 자연스럽게 종료하도록 한다.
		///// </summary>
		///// <param name="blendMethod"></param>
		///// <param name="isAutoEndIfNotloop">True이면 Clip의 재생 후 자동으로 종료한다. (Loop일 경우 무시됨)</param>
		//public apAnimPlayUnit CrossFade(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop, float fadeTime)
		//{
		//	apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);
		//	newPlayUnit.Play(fadeTime, 0.0f);

		//	//TODO : 이 AnimClip을 CalculatedParam에 연결해야한다.
		//	//Debug.LogError("TODO : 이 AnimClip을 CalculatedParam에 연결해야한다");

		//	//플레이 유닛은 플레이 시작
		//	//나머지는 End로 만든다.
		//	for (int i = 0; i < _animPlayUnits.Count; i++)
		//	{
		//		if (newPlayUnit != _animPlayUnits[i])
		//		{
		//			_animPlayUnits[i].FadeOut(fadeTime);
		//		}
		//	}

		//	_nPlayedUnit = _animPlayUnits.Count;

		//	return newPlayUnit;
		//}


		///// <summary>
		///// AnimClip을 PlayUnit에 담아서 기다린 뒤 재생한다.
		///// Queue에 저장된 클립들이 모두 끝나면 Fade Time만큼 섞어서 재생한다.
		///// </summary>
		///// <param name="animClip"></param>
		///// <param name="blendMethod"></param>
		///// <param name="isAutoEndIfNotloop">True이면 Clip의 재생 후 자동으로 종료한다. (Loop일 경우 무시됨)</param>
		///// <returns></returns>
		//public apAnimPlayUnit CrossFadeQueued(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop, float fadeTime)
		//{
		//	//현재 재생되는 플레이 유닛 중에서 "가장 많이 남은 플레이 시간"을 기준으로 타이머를 잡자
		//	//Fade 타임은 없고, 자동 삭제 타이머 + 자동 재생 대기 타이머를 지정

		//	//현재 Queue에 있는 객체가 없다면 CrossFade와 동일하다
		//	if(_nPlayedUnit == 0)
		//	{	
		//		return CrossFade(playData, blendMethod, isAutoEndIfNotloop, fadeTime);
		//	}

		//	float maxRemainPlayTime = -1.0f;
		//	float curRemainPlayTime = 0.0f;
		//	bool isAnyOnceAnimClip = false;
		//	for (int i = 0; i < _nPlayedUnit; i++)
		//	{
		//		_tmpCurPlayUnit = _animPlayUnits[i];
		//		if(_tmpCurPlayUnit.IsLoop)
		//		{
		//			//하나라도 루프이면 실패다. > 수정
		//			//루프는 무시하고 Queue 시간을 잡자
		//			//만약 Loop를 만나고 Queue가 있다면 그냥 기본값인 0.5초를 Queue 시간으로 쓴다.
		//			//Queue에 넣어도 작동하지 않는다.
		//			//Debug.LogError("PlayQueued Failed : Any Clip has Loop Option. Adding to Queue will be ignored");
		//			//return null;
		//			continue;
		//		}

		//		isAnyOnceAnimClip = true;
		//		curRemainPlayTime = _tmpCurPlayUnit.GetRemainPlayTime;
		//		if(maxRemainPlayTime < curRemainPlayTime)
		//		{
		//			maxRemainPlayTime = curRemainPlayTime;
		//		}
		//	}

		//	if(!isAnyOnceAnimClip)
		//	{
		//		maxRemainPlayTime = 0.5f;
		//	}
		//	if(maxRemainPlayTime < 0.0f)
		//	{
		//		maxRemainPlayTime = 0.0f;
		//	}

		//	//딜레이 시간 = 최대 "남은 시간" - "페이드아웃 시간"
		//	//-----------------..............--->
		//	//[    딜레이    ] + [ 페이드아웃 ] 

		//	//Debug.Log("------------------------------------------------------------");
		//	//Debug.Log("CrossFadeQueued Request [" + playData._animClipName + "]");
		//	float delayTime = maxRemainPlayTime - fadeTime;

		//	//Debug.Log("Max Remain Time : " + maxRemainPlayTime);
		//	//Debug.Log("Fade Time : " + fadeTime);
		//	//Debug.Log("Delay Time : " + delayTime);

		//	if(delayTime < 0.0f)
		//	{
		//		// 만약 남은 시간이 적어서 Delay 시간이 음수가 된다면
		//		//Delay Time = 0으로 두고
		//		//남은 시간이 모두 FadeTime이다.
		//		fadeTime = maxRemainPlayTime;
		//		delayTime = 0.0f;

		//		//Debug.LogError("Adjusted > Fade Time : " + fadeTime + " / Delay Time : 0");
		//	}

		//	//Debug.Log("------------------------------------------------------------");

		//	//최대 RemainPlayTime 만큼 Delay한다.
		//	// Delay후 신규 플레이 또는 플레이 종료를 한다.
		//	//Fade 시간은 0

		//	apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);
		//	newPlayUnit.Play(fadeTime, delayTime);

		//	//Debug.LogError("TODO : 이 AnimClip을 CalculatedParam에 연결해야한다");

		//	for (int i = 0; i < _nPlayedUnit; i++)
		//	{
		//		_tmpCurPlayUnit = _animPlayUnits[i];
		//		if (newPlayUnit != _tmpCurPlayUnit)
		//		{
		//			_tmpCurPlayUnit.FadeOut(fadeTime, delayTime);
		//		}
		//	}

		//	_nPlayedUnit = _animPlayUnits.Count;

		//	return newPlayUnit;
		//}

		#endregion


		/// <summary>
		/// 모든 PlayUnit을 종료한다. Clear와 달리 blendTime을 지원한다.
		/// 이 프레임에서 바로 종료하는게 아니므로, 만약 바로 정리를 하고자 한다면 ReleaseForce를 호출하자
		/// </summary>
		public void StopAll(float blendTime)
		{
			//if(blendTime < 0.001f)
			//{
			//	Clear();
			//	return;
			//}

			//Stop을 하면서 서서히 줄어드는 걸 요청한다.
			apAnimPlayRequest request = PopRequest();
			request.SetCurrentPlayedUnits(this, _animPlayUnits);
			request.Stop(blendTime);

			//for (int i = 0; i < _nPlayedUnit; i++)
			//{
			//	_tmpCurPlayUnit = _animPlayUnits[i];
			//	_tmpCurPlayUnit.FadeOut(fadeTime, delayTime);
			//}
		}

		/// <summary>
		/// 각 AnimClip을 강제로 Stop시킴과 동시에 Calculated와의 연동을 바로 끊어버린다.
		/// StopAll과 유사하지만 연동을 바로 끊는 점에서 강제력이 있고 업데이트시 처리에 문제가 있을 수 있음
		/// </summary>
		public void ReleaseForce()
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_animPlayUnits[i].ReleaseLink();
			}

			_animPlayUnits.Clear();
			_nPlayedUnit = _animPlayUnits.Count;
		}


		/// <summary>
		/// AnimClip중에 요청된 RootUnit에 대한 것이 아니면 강제로 종료한다.
		/// </summary>
		/// <param name="usingRootUnit"></param>
		public void StopWithInvalidRootUnit(apOptRootUnit usingRootUnit)
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_tmpCurPlayUnit = _animPlayUnits[i];
				if (_tmpCurPlayUnit._targetRootUnit != usingRootUnit)
				{
					_animPlayUnits[i].SetEnd();
				}
			}
		}

		public void Pause()
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_tmpCurPlayUnit = _animPlayUnits[i];
				_tmpCurPlayUnit.Pause();
			}
		}

		public void Resume()
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_tmpCurPlayUnit = _animPlayUnits[i];
				_tmpCurPlayUnit.Resume();
			}
		}

		// Update
		//----------------------------------------------

		public void Update(float tDelta)
		{
			//현재 재생중인 유닛이 있다면 시작
			if (_nPlayedUnit > 0)
			{
				//업데이트..
				//Debug.Log("Update Queue");
				for (int i = 0; i < _nPlayedUnit; i++)
				{
					_tmpCurPlayUnit = _animPlayUnits[i];
					_tmpCurPlayUnit.SetWeight(0.0f, false);//<<일단 Weight를 0으로 둔다.
					_tmpCurPlayUnit.Update(tDelta);

					if (_tmpCurPlayUnit.IsRemovable)
					{
						//TODO : 이 객체와 연결된 CalculatedParam에 AnimClip이 사라졌음을 알려야한다.
						//Debug.LogError("TODO : 이 객체와 연결된 CalculatedParam에 AnimClip이 사라졌음을 알려야한다");
						_tmpCurPlayUnit.SetWeight(0.0f, true);
						_isAnyUnitChanged = true;
					}
				}
			}

			apAnimPlayRequest curRequest = null;

			//Request를 업데이트하고
			//각 Request별로 연관된 PlayUnit의 Weight를 지정해주자
			for (int iCur = 0; iCur < _requests_Live.Count; iCur++)
			{
				curRequest = _requests_Live[iCur];
				if (curRequest.IsEnded)
				{
					continue;
				}
				curRequest.Update(tDelta);
			}



			//이제 Weight를 지정해준다.
			//1. Request를 자체적으로 업데이트하여 UnitWeight를 계산한다.

			//업데이트 방식
			//- 앞에서부터 설정한다.
			//- 일단 Weight를 1로 설정
			//- 이전 Prev 시간 영역 (-Start ~ +End)을 비교하여 겹치는 시간이 BlendTime보다 긴지 짧은지 판별한다.
			//- 겹친 시간계산하고, 현재의 ITP를 구한다.
			//- 현재 Request에 ITP를 곱하고, 이전 "모든 Weight"에 (1-ITP)를 곱한다.
			// 겹친 시간 : [ tStart <- tCur ] + [ tCur -> tEnd ]

			//2. 현재 시점에서 중복된 Request들 간의 RequestWeight를 계산한다.
			//3. Request를 돌면서 Prev/Next에 대해서 Weight를 지정해준다.


			float prevCurrent2EndTime = -1.0f;
			float tmpOverlapTime = 0.0f;
			float tmpOverlapITP = 0.0f;

			bool isAnyRequestChanged = false;

			for (int iCur = 0; iCur < _requests_Live.Count; iCur++)
			{
				curRequest = _requests_Live[iCur];
				//Ready => Weight : 1
				//End => Weight : 0
				if (!curRequest.IsLive)
				{

					if (curRequest.IsEnded)
					{
						curRequest.SetRequestWeight(0.0f);
						isAnyRequestChanged = true;
					}
					else
					{
						curRequest.SetRequestWeight(1.0f);
					}
					continue;
				}

				curRequest.SetRequestWeight(1.0f);//일단 1을 넣는다.

				if (iCur == 0)
				{
					prevCurrent2EndTime = curRequest.Current2EndTime;
					continue;
				}

				//BlendTime보다 짧다면 Overlap 시간이 짧아진다.
				//CurTime을 기준으로 [tStart <- tCur] 시간과 [tCur -> tEnd] 시간을 나누어 더하여 계산하는데,
				//[tCur -> tEnd] 시간은 이전 Request와 길이를 비교한다.
				tmpOverlapTime = curRequest.Current2StartTime + Mathf.Min(prevCurrent2EndTime, curRequest.Current2EndTime);
				if (tmpOverlapTime < 0.001f)
				{
					tmpOverlapITP = 1.0f;
				}
				else
				{
					tmpOverlapITP = curRequest.Current2StartTime / tmpOverlapTime;
				}

				for (int iPrev = 0; iPrev < iCur; iPrev++)
				{
					_requests_Live[iPrev].MultiplyRequestWeight(1.0f - tmpOverlapITP);
				}
				curRequest.MultiplyRequestWeight(tmpOverlapITP);


				prevCurrent2EndTime = curRequest.Current2EndTime;
			}


			//마지막으로 다시 돌면서 Request에서 계산된 UnitWeight * RequestWeight를 넣어서
			//
			for (int iCur = 0; iCur < _requests_Live.Count; iCur++)
			{
				curRequest = _requests_Live[iCur];
				//curRequest.AdaptWeightToPlayUnits();

				if (curRequest.IsEnded)
				{
					continue;
				}

				curRequest.AdaptWeightToPlayUnits();
			}


			//변화값이 있으면 삭제 여부를 판단하자
			if (_isAnyUnitChanged)
			{
				_animPlayUnits.RemoveAll(delegate (apAnimPlayUnit a)
				{
					return a.IsRemovable;
				});

				_isAnyUnitChanged = false;
				_nPlayedUnit = _animPlayUnits.Count;


				_playManager.OnAnyAnimPlayUnitEnded();
			}


			if (isAnyRequestChanged)
			{
				//끝난 Request를 Pool에 돌려놓는다.
				List<apAnimPlayRequest> endedRequests = new List<apAnimPlayRequest>();
				for (int i = 0; i < _requests_Live.Count; i++)
				{
					if (_requests_Live[i].IsEnded)
					{
						endedRequests.Add(_requests_Live[i]);
					}
				}

				for (int i = 0; i < endedRequests.Count; i++)
				{
					PushRequest(endedRequests[i]);
				}
			}
		}



		public int RefreshPlayOrders(int startOrder)
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_animPlayUnits[i].SetPlayOrder(startOrder);
				startOrder++;
			}
			return startOrder;
		}


		// Event
		//-----------------------------------------------------------------
		public void OnAnimPlayUnitPlayStart(apAnimPlayUnit playUnit)
		{
			//Play Unit이 재생을 시작했다.
			//Delay 이후에 업데이트되는 첫 프레임에 이 이벤트가 호출된다.

			// > Root Unit이 바뀔 수 있으므로 Play Manager에도 신고를 해야한다.
			_playManager.OnAnimPlayUnitPlayStart(playUnit, this);
		}


		public void OnAnimPlayUnitEnded(apAnimPlayUnit playUnit)
		{
			_playManager.OnAnimPlayUnitEnded(playUnit, this);
		}

		// Get / Set
		//-----------------------------------------------------------------
		public apAnimPlayUnit GetPlayUnit(string animClipName)
		{
			return _animPlayUnits.Find(delegate (apAnimPlayUnit a)
			{
				return (a._linkedAnimClip != null) && string.Equals(a._linkedAnimClip._name, animClipName);
			});
		}

		public apAnimPlayUnit GetPlayUnit(apAnimClip animClip)
		{
			return _animPlayUnits.Find(delegate (apAnimPlayUnit a)
			{
				return (a._linkedAnimClip == animClip);
			});
		}


		//디버그용
		public List<apAnimPlayUnit> PlayUnitList
		{
			get
			{
				return _animPlayUnits;
			}
		}
	}
}
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
	/// AnimClip을 감싸고 Runtime에서 재생이 되는 유닛.
	/// Layer 정보를 가지고 블렌딩의 기준이 된다.
	/// Queue의 실행 순서에 따라서 대기->페이드인(재생)->재생->페이드아웃(재생)->끝의 생명 주기를 가진다.
	/// "자동 재생 종료"옵션에 따라 "Loop가 아닌 AnimClip"은 자동으로 재생이 끝나기도 한다.
	/// </summary>
	public class apAnimPlayUnit
	{
		// Members
		//-----------------------------------------------
		public apAnimPlayQueue _parentQueue = null;

		public apAnimClip _linkedAnimClip = null;
		public apOptRootUnit _targetRootUnit = null;//렌더링 대상이 되는 루트 유닛

		//최종적으로 제어하고 있는 Request를 저장한다.
		//Weight 지정은 여러 Request에서 중첩적으로 하지만, 스테이트 제어는 마지막에 생성된 Request만 가능하다.
		public apAnimPlayRequest _ownerRequest = null;

		public int _layer = -1;
		public int _playOrder = -1;//<<이게 재생 순서. (Layer에 따라 증가하며, 동일 Layer에서는 Queue의 재생 순서에 따라 매겨진다.

		//수정 : Weight 계산은 위에서 하며, 그 외의 스테이트 계산은 빠진다.
		//플레이 시간만 계산을 하며, 이를 리턴하는 정도.
		//FadeIn/FadeOut의 상태는 삭제. <- 이게 버그의 가장 큰 이유

		/// <summary>
		/// 대기/페이드/플레이 상태
		/// (Pause는 별도의 변수로 체크하며 여기서는 Play에 포함된다)
		/// </summary>
		public enum PLAY_STATUS
		{
			/// <summary>Ready : 등록만 되고 아무런 처리가 되지 않았다. Queue 대기 상태인 경우</summary>
			Ready = 0,
			/// <summary>Play : 플레이가 되고 있는 중</summary>
			Play = 1,
			/// <summary>End : 플레이가 모두 끝났다. (삭제 대기)</summary>
			End = 2
		}

		private PLAY_STATUS _playStatus = PLAY_STATUS.Ready;
		public PLAY_STATUS PlayStatus { get { return _playStatus; } }

		private bool _isPause = false;


		//Fade/Delay 관련 코드 삭제. 이건 외부에서 제어할겁니다.
		//private float _fadeInTime = 0.0f;
		//private float _fadeOutTime = 0.0f;
		//private float _delayToPlayTime = 0.0f;
		//private float _delayToEndTime = 0.0f;

		//private bool _isDelayIn = false;
		//private bool _isDelayOut = false;

		//private float _tDelay = 0.0f;

		public enum BLEND_METHOD
		{
			Interpolation = 0,
			Additive = 1
		}

		private BLEND_METHOD _blendMethod = BLEND_METHOD.Interpolation;
		public BLEND_METHOD BlendMethod { get { return _blendMethod; } }

		/// <summary>
		/// AnimClip이 Loop 타입이 아니라면 자동으로 종료한다.
		/// </summary>
		private bool _isAutoEnd = false;

		/// <summary>배속 비율 (기본값 1)</summary>
		private float _speedRatio = 1.0f;



		// 내부 스테이트 처리 변수
		private PLAY_STATUS _nextPlayStatus = PLAY_STATUS.Ready;
		private bool _isFirstFrame = false;
		//private float _tFade = 0.0f;

		//총 재생 시간.
		private float _tAnimClipLength = 0.0f;

		private float _unitWeight = 0.0f;
		private bool _isWeightCalculated = false;
		private float _totalRequestWeights = 0.0f;

		//private float _prevUnitWeight = 0.0f;
		public float UnitWeight
		{
			get
			{
				if (_playStatus != PLAY_STATUS.Play)
				{
					return 0.0f;
				}


				if (_totalRequestWeights > 0.0f)
				{
					return _unitWeight / _totalRequestWeights;
				}
				if (_isWeightCalculated)
				{
					//일단 빼자
					//Debug.LogError("Calculated가 된 Play Unit : " + _unitWeight + " / Total : " + _totalRequestWeights);
				}

				return 1.0f;
			}
		}

		private bool _tmpIsEnd = false;

		private bool _isLoop = false;

		private bool _isPlayStartEventCalled = false;
		private bool _isEndEventCalled = false;

		//public float FadeInTime { get { return _fadeInTime; } }
		//public float FadeOutTime { get { return _fadeOutTime; } }
		//public float DelayToPlayTime { get { return _delayToPlayTime; } }
		//public float DelayToEndTime {  get { return _delayToEndTime; } }

		// Init
		//-----------------------------------------------
		public apAnimPlayUnit(apAnimPlayQueue parentQueue)
		{
			_parentQueue = parentQueue;
		}


		public void SetAnimClip(apAnimPlayData playData, int layer, BLEND_METHOD blendMethod, bool isAutoEndIfNotLoop, bool isEditor)
		{
			_linkedAnimClip = playData._linkedAnimClip;
			_targetRootUnit = playData._linkedOptRootUnit;

			//추가
			if (_linkedAnimClip._parentPlayUnit != null
				&& _linkedAnimClip._parentPlayUnit != this)
			{
				//이미 다른 PlayUnit이 사용중이었다면..
				_linkedAnimClip._parentPlayUnit.SetEnd();
				//_linkedAnimClip._parentPlayUnit._linkedAnimClip = null;
			}
			_linkedAnimClip._parentPlayUnit = this;

			_layer = layer;

			_isLoop = _linkedAnimClip.IsLoop;
			_isAutoEnd = isAutoEndIfNotLoop;
			if (_isLoop)
			{
				_isAutoEnd = false;//<<Loop일때 AutoEnd는 불가능하다
			}


			_blendMethod = blendMethod;

			_isPause = false;
			_playStatus = PLAY_STATUS.Ready;
			_isPlayStartEventCalled = false;
			_isEndEventCalled = false;

			//_fadeInTime = 0.0f;
			//_fadeOutTime = 0.0f;

			//_delayToPlayTime = 0.0f;
			//_delayToEndTime = 0.0f;

			_speedRatio = 1.0f;

			_isFirstFrame = true;
			_nextPlayStatus = _playStatus;

			if (isEditor)
			{
				_linkedAnimClip.Stop_Editor(false);//Stop은 하되 업데이트는 하지 않는다. (false)
			}
			else
			{
				_linkedAnimClip.Stop_Opt(false);
			}

			_tAnimClipLength = _linkedAnimClip.TimeLength;
			_unitWeight = 0.0f;
			_isWeightCalculated = false;
			_totalRequestWeights = 0.0f;
			//_prevUnitWeight = 0.0f;

			//_isDelayIn = false;
			//_isDelayOut = false;

			//_tDelay = 0.0f;

		}


		public void SetSubOption(BLEND_METHOD blendMethod, bool isAutoEndIfNotLoop)
		{
			_blendMethod = blendMethod;
			_isAutoEnd = isAutoEndIfNotLoop;
			if (_isLoop)
			{
				_isAutoEnd = false;//<<Loop일때 AutoEnd는 불가능하다
			}
		}

		public void SetOwnerRequest(apAnimPlayRequest request)
		{
			_ownerRequest = request;
		}

		// Update
		//-----------------------------------------------
		#region [미사용 코드] UnitWeight를 계산하는건 외부에서 일괄적으로 한다. 자체적으로 하면 문제가 많다.
		///// <summary>
		///// Update 직전에 UnitWeight를 계산한다.
		///// 유효하지 않을 경우 -1 리턴.
		///// 꼭 Update 직전에 호출해야한다.
		///// 실제 Clip 업데이트 전에 타이머/스테이트 처리등을 수행한다.
		///// </summary>
		///// <returns></returns>
		//public float CalculateUnitWeight(float tDelta)
		//{
		//	_tmpIsEnd = false;

		//	if(_linkedAnimClip._parentPlayUnit != this)
		//	{
		//		return -1.0f;
		//	}

		//	PLAY_STATUS requestedNextPlayStatus = _nextPlayStatus;

		//	switch (_playStatus)
		//	{
		//		case PLAY_STATUS.Ready:
		//			{
		//				if (_isFirstFrame)
		//				{
		//					_unitWeight = 0.0f;
		//					//_prevUnitWeight = 0.0f;
		//				}
		//				//if (!_isPause)
		//				//{
		//				//	if (_isDelayIn)
		//				//	{
		//				//		//딜레이 후에 플레이된다.
		//				//		_tDelay += tDelta;
		//				//		if (_tDelay > _delayToPlayTime)
		//				//		{
		//				//			_unitWeight = 0.0f;
		//				//			_isDelayIn = false;
		//				//			ChangeNextStatus(PLAY_STATUS.PlayWithFadeIn);//<<플레이 된다.
		//				//		}
		//				//	}
		//				//}
		//			}
		//			break;


		//		//case PLAY_STATUS.PlayWithFadeIn:
		//		//	{
		//		//		if(_isFirstFrame)
		//		//		{
		//		//			_tFade = 0.0f;
		//		//			_prevUnitWeight = _unitWeight;
		//		//		}
		//		//		if (!_isPause)
		//		//		{
		//		//			_tFade += tDelta;

		//		//			if (_tFade < _fadeInTime)
		//		//			{
		//		//				_unitWeight = (_prevUnitWeight * (_fadeInTime - _tFade) + 1.0f * _tFade) / _fadeInTime;
		//		//			}
		//		//			else
		//		//			{
		//		//				_unitWeight = 1.0f;
		//		//				//Fade가 끝났으면 Play
		//		//				ChangeNextStatus(PLAY_STATUS.Play);
		//		//			}
		//		//		}
		//		//	}
		//		//	break;

		//		case PLAY_STATUS.Play:
		//			{
		//				if(_isFirstFrame)
		//				{
		//					_unitWeight = 1.0f;
		//					//_prevUnitWeight = 1.0f;
		//				}

		//				if (!_isPause)
		//				{
		//					//if (_isDelayOut)
		//					//{
		//					//	//딜레이 후에 FadeOut된다.
		//					//	_tDelay += tDelta;
		//					//	if (_tDelay > _delayToEndTime)
		//					//	{
		//					//		_isDelayOut = false;
		//					//		_unitWeight = 1.0f;
		//					//		ChangeNextStatus(PLAY_STATUS.PlayWithFadeOut);//<<플레이 종료를 위한 FadeOut
		//					//	}
		//					//}
		//				}
		//			}
		//			break;

		//		case PLAY_STATUS.PlayWithFadeOut:
		//			{
		//				if(_isFirstFrame)
		//				{
		//					_tFade = 0.0f;
		//					_prevUnitWeight = _unitWeight;
		//				}

		//				if (!_isPause)
		//				{
		//					_tFade += tDelta;

		//					if (_tFade < _fadeOutTime)
		//					{
		//						_unitWeight = (_prevUnitWeight * (_fadeOutTime - _tFade) + 0.0f * _tFade) / _fadeOutTime;
		//					}
		//					else
		//					{
		//						_unitWeight = 0.0f;
		//						ChangeNextStatus(PLAY_STATUS.End);
		//					}
		//				}
		//			}
		//			break;


		//		case PLAY_STATUS.End:
		//			{
		//				//아무것도 안합니더
		//				if(_isFirstFrame)
		//				{
		//					//Debug.Log("End");
		//					_unitWeight = 0.0f;
		//				}

		//			}
		//			break;
		//	}

		//	if(_playOrder == 0)
		//	{
		//		return 1.0f;
		//	}
		//	return _unitWeight;
		//} 
		#endregion

		public void SetWeight(float weight, bool isCalculated)
		{
			//외부에서 Weight를 지정한다.
			_unitWeight = weight;
			_isWeightCalculated = isCalculated;
			_totalRequestWeights = 0.0f;
		}

		public void AddWeight(float multiplyUnitWeight, float requestWeight)
		{
			//외부에서 Weight를 지정한다.
			//_unitWeight = Mathf.Clamp01(_unitWeight * multiplyRatio);
			//_unitWeight = Mathf.Clamp01((_unitWeight * multiplyUnitWeight * requestWeight) + (_unitWeight * (1-requestWeight)));
			_unitWeight = _unitWeight + (multiplyUnitWeight * requestWeight);
			_totalRequestWeights += requestWeight;

			_isWeightCalculated = true;
		}

		public void Update(float tDelta)
		{
			_tmpIsEnd = false;

			//_unitWeight *= weightCorrectRatio;//<<이거 안해요

			if (_linkedAnimClip._parentPlayUnit != this)
			{
				//PlayUnit이 더이상 이 AnimClip을 제어할 수 없게 되었다
				//Link Release를 하고 업데이트도 막는다.
				//Debug.LogError("AnimPlayUnit Invalid End");
				ReleaseLink();
				return;
			}

			PLAY_STATUS requestedNextPlayStatus = _nextPlayStatus;

			switch (_playStatus)
			{
				case PLAY_STATUS.Ready:
					{
						if (_isFirstFrame)
						{
							//_unitWeight = 0.0f;
							//_prevUnitWeight = 0.0f;
							_linkedAnimClip.SetFrame_Opt(_linkedAnimClip.StartFrame);
							//Debug.Log("Ready");
						}
						//if (!_isPause)
						//{
						//	if (_isDelayIn)
						//	{
						//		//딜레이 후에 플레이된다.
						//		_tDelay += tDelta;
						//		if (_tDelay > _delayToPlayTime)
						//		{
						//			_unitWeight = 0.0f;
						//			_isDelayIn = false;
						//			ChangeNextStatus(PLAY_STATUS.PlayWithFadeIn);//<<플레이 된다.
						//		}
						//	}
						//}
					}
					break;


				//case PLAY_STATUS.PlayWithFadeIn:
				//	{
				//		if(_isFirstFrame)
				//		{
				//			//_tFade = 0.0f;
				//			//_prevUnitWeight = _unitWeight;

				//			//플레이 시작했다고 알려주자
				//			if (!_isPlayStartEventCalled)
				//			{
				//				_parentQueue.OnAnimPlayUnitPlayStart(this);
				//				_isPlayStartEventCalled = true;
				//			}
				//			//Debug.Log("Play With Fade In");
				//		}
				//		if (!_isPause)
				//		{
				//			//_tFade += tDelta;

				//			if (_tFade < _fadeInTime)
				//			{
				//				//_unitWeight = (_prevUnitWeight * (_fadeInTime - _tFade) + 1.0f * _tFade) / _fadeInTime;

				//				_tmpIsEnd = _linkedAnimClip.Update_Opt(tDelta * _speedRatio);
				//			}
				//			//else
				//			//{
				//			//	_unitWeight = 1.0f;
				//			//	//Fade가 끝났으면 Play
				//			//	ChangeNextStatus(PLAY_STATUS.Play);
				//			//}
				//		}
				//	}
				//	break;

				case PLAY_STATUS.Play:
					{
						if (_isFirstFrame)
						{
							//_unitWeight = 1.0f;
							//_prevUnitWeight = 1.0f;

							//플레이 시작했다고 알려주자
							if (!_isPlayStartEventCalled)
							{
								_parentQueue.OnAnimPlayUnitPlayStart(this);
								_isPlayStartEventCalled = true;
							}
							//Debug.Log("Play");
						}

						if (!_isPause)
						{
							_tmpIsEnd = _linkedAnimClip.Update_Opt(tDelta * _speedRatio);

							//if (_isDelayOut)
							//{
							//	//딜레이 후에 FadeOut된다.
							//	_tDelay += tDelta;
							//	if (_tDelay > _delayToEndTime)
							//	{
							//		_isDelayOut = false;
							//		_unitWeight = 1.0f;
							//		ChangeNextStatus(PLAY_STATUS.PlayWithFadeOut);//<<플레이 종료를 위한 FadeOut
							//	}
							//}
						}
					}
					break;

				//case PLAY_STATUS.PlayWithFadeOut:
				//	{
				//		if(_isFirstFrame)
				//		{
				//			//_tFade = 0.0f;
				//			//_prevUnitWeight = _unitWeight;
				//			//Debug.Log("Play With Fade Out");
				//		}

				//		if (!_isPause)
				//		{
				//			//_tFade += tDelta;

				//			if (_tFade < _fadeOutTime)
				//			{
				//				//_unitWeight = (_prevUnitWeight * (_fadeOutTime - _tFade) + 0.0f * _tFade) / _fadeOutTime;

				//				_tmpIsEnd = _linkedAnimClip.Update_Opt(tDelta * _speedRatio);
				//			}
				//			//else
				//			//{
				//			//	_unitWeight = 0.0f;
				//			//	ChangeNextStatus(PLAY_STATUS.End);
				//			//}
				//		}
				//	}
				//	break;


				case PLAY_STATUS.End:
					{
						//아무것도 안합니더
						if (_isFirstFrame)
						{
							//Debug.Log("End");
							//_unitWeight = 0.0f;
							ReleaseLink();
						}

					}
					break;
			}

			if (_tmpIsEnd && _isAutoEnd)
			{
				//종료가 되었다면 (일단 Loop는 아니라는 것)
				//조건에 따라 End로 넘어가자
				SetEnd();
			}

			//스테이트 처리
			//if(_nextPlayStatus != _playStatus)
			if (requestedNextPlayStatus != _playStatus)
			{
				_playStatus = requestedNextPlayStatus;
				_nextPlayStatus = _playStatus;
				_isFirstFrame = true;
			}
			else if (_isFirstFrame)
			{
				_isFirstFrame = false;
			}
		}


		private void ChangeNextStatus(PLAY_STATUS nextStatus)
		{
			_nextPlayStatus = nextStatus;
		}





		// Functions
		//-----------------------------------------------

		public void Play()
		{
			if (_playStatus == PLAY_STATUS.Ready)
			{
				//Debug.Log(_linkedAnimClip._name + " >> Play [ Fade : " + fadeTime + " / Delay : " + delayTime + " ]");
				_isPause = false;
				//_fadeInTime = fadeTime;
				_unitWeight = 0.0f;
				//_delayToPlayTime = delayTime;

				//_isDelayIn = true;
				//_isDelayOut = false;

				//_tDelay = 0.0f;

				_isPlayStartEventCalled = false;
				_isEndEventCalled = false;

				//if (delayTime < 0.001f)
				//{
				//	_delayToPlayTime = 0.0f;
				//	_isDelayIn = false;

				//	//딜레이가 없으면 바로 스테이트를 이동한다.
				//	if (_fadeInTime > 0.001f)
				//	{
				//		//Fade In을 하며 시작한다.
				//		ChangeNextStatus(PLAY_STATUS.PlayWithFadeIn);
				//	}
				//	else
				//	{
				//		//Debug.Log("Direct Play");
				//		//바로 시작
				//		ChangeNextStatus(PLAY_STATUS.Play);
				//	}
				//}

				//Debug.Log("Direct Play");
				//바로 시작
				ChangeNextStatus(PLAY_STATUS.Play);
			}
		}

		/// <summary>
		/// 일반적인 Play와 달리 강제로 재시작을 한다.
		/// </summary>
		public void ResetPlay()
		{
			_isPause = false;
			_isPlayStartEventCalled = false;
			_isEndEventCalled = false;
			ChangeNextStatus(PLAY_STATUS.Play);
			_isFirstFrame = true;

			_linkedAnimClip.SetFrame_Opt(_linkedAnimClip.StartFrame);
		}

		public void Resume()
		{
			_isPause = false;
		}

		public void Pause()
		{
			_isPause = true;
		}

		public void SetSpeed(float speedRatio)
		{
			_speedRatio = speedRatio;
		}


		//public void FadeOut(float fadeTime = 0.3f, float delayTime = 0.0f)
		//{
		//	//Debug.Log(_linkedAnimClip._name + " >> Fade Out [ Fade : " + fadeTime + " / Delay : " + delayTime + " ]");
		//	_isPause = false;
		//	_fadeOutTime = fadeTime;
		//	_delayToEndTime = delayTime;

		//	_isDelayOut = true;

		//	_tDelay = 0.0f;

		//	if (delayTime < 0.001f)
		//	{
		//		_delayToEndTime = 0.0f;
		//		_isDelayOut = false;

		//		//딜레이가 없으면 바로 종료를 한다.
		//		if (_fadeOutTime > 0.001f)
		//		{
		//			//Fade Out을 하며 끝난다
		//			ChangeNextStatus(PLAY_STATUS.PlayWithFadeOut);
		//		}
		//		else
		//		{
		//			//바로 끝
		//			ChangeNextStatus(PLAY_STATUS.End);
		//		}
		//	}
		//}

		public void SetEnd()
		{
			_unitWeight = 0.0f;
			_totalRequestWeights = 1.0f;
			_isWeightCalculated = true;

			_isPlayStartEventCalled = false;
			ChangeNextStatus(PLAY_STATUS.End);
		}


		public void ReleaseLink()
		{
			//연결된 Calculate와 연동을 끊는다.
			if (!_isEndEventCalled)
			{
				_parentQueue.OnAnimPlayUnitEnded(this);
				_isEndEventCalled = true;
				_playStatus = PLAY_STATUS.End;
			}
		}


		// Get / Set
		//-----------------------------------------------
		/// <summary>재생이 끝나고 삭제를 해야하는가</summary>
		public bool IsRemovable { get { return _playStatus == PLAY_STATUS.End; } }
		public bool IsUpdatable
		{
			get
			{
				return _playStatus == PLAY_STATUS.Ready ||
					//_playStatus == PLAY_STATUS.PlayWithFadeIn ||
					_playStatus == PLAY_STATUS.Play;
				//_playStatus == PLAY_STATUS.PlayWithFadeOut;
			}
		}

		public bool IsLoop { get { return _isLoop; } }

		/// <summary>
		/// PlayUnit이 자동으로 종료가 되는가. 이게 True여야 Queued Play가 가능하다
		/// [Loop가 아니어야 하며, isAutoEndIfNotLoop = true여야 한다]
		/// </summary>
		public bool IsEndAutomaticallly
		{
			get
			{
				if (_isLoop)
				{
					return false;
				}
				return _isAutoEnd;
			}
		}
		public float RemainPlayTime
		{
			get
			{
				if (_isLoop)
				{
					return -1.0f;
				}
				return _linkedAnimClip.TimeLength - _linkedAnimClip.TotalPlayTime;
			}
		}
		public float TotalPlayTime
		{
			get
			{
				return _linkedAnimClip.TotalPlayTime;
			}
		}

		public float TimeLength
		{
			get
			{
				return _linkedAnimClip.TimeLength;
			}
		}


		public void SetPlayOrder(int playOrder)
		{
			_playOrder = playOrder;
		}
	}

}
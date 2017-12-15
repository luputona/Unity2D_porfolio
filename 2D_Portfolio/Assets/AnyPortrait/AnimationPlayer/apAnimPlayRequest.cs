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


	/// <summary>
	/// Play / PlayQueued 요청이 들어왔을때, 그 시간을 기록하여 Weight를 계산하거나 재생을 제어하는 역할을 하는 클래스
	/// Layer마다 지정되므로, 각각의 Layer에 대한 요청이 들어온 경우 이 객체를 여러개 생성한다.
	/// Layer별로 가장 먼저 요청된 Request부터 Weight를 계산한다.
	/// 이 데이터는 AnimPlayQueue에 포함된다.
	/// 만약 대기중인 종료 시점보다 먼저 종료되는게 뒤로 온 경우, 어색하지 않게 Weight를 급격히 낮추는 것도 중요.
	/// </summary>
	public class apAnimPlayRequest
	{
		// Members
		//--------------------------------------------------
		private apAnimPlayQueue _parentAnimPlayQueue = null;

		//시간 계산법. 호출한 시간으로부터 타이머가 작동을 한다. (절대 시간을 알 수 없으므로)
		//BlendTime이 있는 경우, NextPlayStart 타임이 바뀌는데
		//New 타입인 경우 : 
		//private float _tNextPlayStart = 0.0f;//다음에 플레이가 시작하는 시점 (Blend가 끝나는 시점이다.)
		private float _tBlend = 0.0f;//시간 간격
		private float _tLive = 0.0f;
		private bool _isNextUnitPlayed = false;
		private bool _isNextUnitFrameReset = false;

		private float _tActiveStart = 0.0f;//생성 시간을 0으로 둘때의 Active 시작 시점
		private float _tActiveEnd = 0.0f;//생성 시간을 0으로 둘때의 Active 끝 시점
		private float _tActiveMiddle = 0.0f;//생성 시간을 0으로 둘때의 Active의 중간 시점

		public enum STATUS
		{
			/// <summary>
			/// Queued 타입의 경우, 바로 Active하지 못하고 시작 시점을 기다려야한다.
			/// </summary>
			Ready,
			Active,
			End
		}
		private STATUS _status = STATUS.Ready;

		public enum REQUEST_TYPE
		{
			New,
			Queued,//<<시간으로 재는 것이 아니라, 현재 대기중인 PlayData의 재생 시간을 보고 결정한다.
			Stop,//New와 비슷하게 처리를 하지만, 다음에 재생되는 Unit은 없다.
		}
		private REQUEST_TYPE _requestType = REQUEST_TYPE.New;

		//다음에 플레이하게될 PlayUnit (필수)
		private apAnimPlayUnit _nextPlayUnit = null;

		//Queue에 들어간 PlayData 중 마지막 데이터.
		//만약 requestType이 Queued라면, Queue 상태로 저장된다. 
		private apAnimPlayUnit _prevWaitingPlayUnit = null;

		//요청이 들어왔을때, 그 직전에 Queue에 존재했던 Unit들..
		private List<apAnimPlayUnit> _prevPlayUnits = new List<apAnimPlayUnit>();


		//Request 자체에 대한 Weight.
		//Request끼리 중첩되는 경우 (PlayUnit이 중첩되는게 아니라..)
		//Request간의 보간도 있어야 한다.
		//이때 Weight 주도권은 나중에 선언된 Request에 있다.
		private float _requestWeight = 1.0f;
		private float _nextUnitWeight = 1.0f;
		private float _prevUnitWeight = 1.0f;
		private float _nextUnitWeight_Overlap = 0.0f;


		/// <summary>새로 재생할 PlayUnit이 PrevUnit에 포함되어있는가? (포함된다면 해당 Unit의 Blend는 다르게 제어된다.</summary>
		private bool _isNextPlayUnitIsInPrevUnit = false;



		private const float BIAS_ZERO = 0.001f;
		//Pool 관련
		//-----------------------------------------------------------------



		// Init
		//--------------------------------------------------
		public apAnimPlayRequest()
		{
			Clear();
		}

		public void Clear()
		{
			_parentAnimPlayQueue = null;

			//_tNextPlayStart = 0.0f;
			_tBlend = 0.0f;
			_tLive = 0.0f;
			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.New;

			_nextPlayUnit = null;
			_prevWaitingPlayUnit = null;
			_prevPlayUnits.Clear();

			_requestWeight = 1.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = 0.0f;
			_tActiveEnd = 0.0f;
			_tActiveMiddle = 0.0f;
			_status = STATUS.Ready;

			_isNextPlayUnitIsInPrevUnit = false;

		}

		public void SetCurrentPlayedUnits(apAnimPlayQueue parentAnimPlayQueue, List<apAnimPlayUnit> prevPlayUnits)
		{
			_parentAnimPlayQueue = parentAnimPlayQueue;

			_prevPlayUnits.Clear();
			for (int i = 0; i < prevPlayUnits.Count; i++)
			{
				prevPlayUnits[i]._ownerRequest = this;
				_prevPlayUnits.Add(prevPlayUnits[i]);
			}
		}

		public void PlayNew(apAnimPlayUnit nextPlayUnit, float tBlend)
		{


			//_tNextPlayStart = tNextPlay;//<<이게 필요한가?
			_tBlend = tBlend;
			_tLive = 0.0f;

			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.New;
			_nextPlayUnit = nextPlayUnit;
			_nextPlayUnit._ownerRequest = this;

			_prevWaitingPlayUnit = null;

			_requestWeight = 0.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = 0.0f;
			_tActiveEnd = _tBlend;
			_tActiveMiddle = _tBlend * 0.5f;

			_status = STATUS.Active;//<<바로 시작

			_isNextPlayUnitIsInPrevUnit = false;

			for (int i = 0; i < _prevPlayUnits.Count; i++)
			{
				if (nextPlayUnit == _prevPlayUnits[i])
				{
					_isNextPlayUnitIsInPrevUnit = true;
					break;
				}
			}

			_nextUnitWeight_Overlap = 1.0f;

			if (_isNextPlayUnitIsInPrevUnit)
			{
				_nextUnitWeight_Overlap = _nextPlayUnit.UnitWeight;
			}

		}

		public void PlayQueued(apAnimPlayUnit nextPlayUnit, apAnimPlayUnit prevLastPlayUnit, float tBlend)
		{
			//_tNextPlayStart = -1;//Queued 타입은 플레이 시간을 받지 않는다.
			_tBlend = tBlend;
			_tLive = 0.0f;

			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.Queued;
			_nextPlayUnit = nextPlayUnit;
			_nextPlayUnit._ownerRequest = this;

			_prevWaitingPlayUnit = prevLastPlayUnit;

			_requestWeight = 0.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = -1.0f;
			_tActiveEnd = -1.0f;//<<알 수 없다.
			_tActiveMiddle = -1.0f;

			_status = STATUS.Ready;//<<일단 대기

			_isNextPlayUnitIsInPrevUnit = false;

			for (int i = 0; i < _prevPlayUnits.Count; i++)
			{
				if (nextPlayUnit == _prevPlayUnits[i])
				{
					_isNextPlayUnitIsInPrevUnit = true;
					break;
				}
			}

			_nextUnitWeight_Overlap = 1.0f;
			if (_isNextPlayUnitIsInPrevUnit)
			{
				_nextUnitWeight_Overlap = _nextPlayUnit.UnitWeight;
			}
		}

		public void Stop(float tBlend)
		{
			//_tNextPlayStart = -1;
			_tBlend = tBlend;
			_tLive = 0.0f;

			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.Stop;
			_nextPlayUnit = null;
			_prevWaitingPlayUnit = null;

			_requestWeight = 0.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = 0.0f;
			_tActiveEnd = _tBlend;
			_tActiveMiddle = _tBlend * 0.5f;
			_status = STATUS.Active;//<<바로 시작

			_isNextPlayUnitIsInPrevUnit = false;

		}

		// Update
		//--------------------------------------------------
		public void Update(float tDelta)
		{
			switch (_requestType)
			{
				case REQUEST_TYPE.New:
					{
						switch (_status)
						{
							case STATUS.Ready:
							case STATUS.Active:
								//Ready 상태가 없다. 있어도 Active로 처리
								if (!_isNextUnitPlayed)
								{
									if (_nextPlayUnit != null && _nextPlayUnit._ownerRequest == this)
									{
										_nextPlayUnit.Play();
									}
									_isNextUnitPlayed = true;
								}

								_tLive += tDelta;
								if (_tLive >= _tActiveEnd || _tActiveEnd < BIAS_ZERO)
								{
									_status = STATUS.End;//끝!
									_nextUnitWeight = 1.0f;
									_prevUnitWeight = 0.0f;


									for (int i = 0; i < _prevPlayUnits.Count; i++)
									{
										if (_nextPlayUnit != _prevPlayUnits[i] && _prevPlayUnits[i]._ownerRequest == this)
										{
											_prevPlayUnits[i].SetEnd();
										}
									}
								}
								else
								{
									if (_isNextPlayUnitIsInPrevUnit)
									{
										//만약 Prev 유닛에 이미 재생중이었다면
										if (_tLive < _tActiveMiddle)
										{
											//절반 동안은 서서히 내려가고 (이미 재생중이었으므로)
											_nextUnitWeight = (1.0f - (_tLive / _tActiveMiddle)) * _nextUnitWeight_Overlap;
										}
										else
										{
											//그 나머지는 1로 올라간다.
											_nextUnitWeight = ((_tLive - _tActiveMiddle) / _tActiveMiddle);
											if (!_isNextUnitFrameReset)
											{
												//프레임을 여기서 리셋한다.
												if (_nextPlayUnit != null && _nextPlayUnit._ownerRequest == this)
												{
													_nextPlayUnit.ResetPlay();
												}
												_isNextUnitFrameReset = true;
											}
										}
									}
									else
									{
										//새로운 NextUnit이 재생을 시작했다면 (기본)
										_nextUnitWeight = _tLive / _tActiveEnd;
									}

									_prevUnitWeight = 1.0f - (_tLive / _tActiveEnd);
								}
								break;

							case STATUS.End:
								_nextUnitWeight = 1.0f;
								_prevUnitWeight = 0.0f;
								break;
						}
					}
					break;

				case REQUEST_TYPE.Queued:
					{
						switch (_status)
						{
							case STATUS.Ready:
								{
									//여기가 중요
									//대기중인 AnimPlayUnit의 종료를 기다린다.
									//if(_prevWaitingPlayUnit == null)
									//{
									//	_status = STATUS.End;
									//	_nextUnitWeight = 0.0f;
									//	break;
									//}

									_tLive += tDelta;
									float remainTime = 0.0f;

									if (_prevWaitingPlayUnit != null)
									{
										remainTime = _prevWaitingPlayUnit.RemainPlayTime;
									}

									if (remainTime <= _tBlend + BIAS_ZERO)
									{
										_status = STATUS.Active;
										// Blend 시간을 포함하여 다음 PlayUnit을 실행할 수 있게 되었다.
										//Debug.LogError("Queue Ready >> Active (Remain : " + remainTime + " / Blend Time : " + _tBlend + ")");

										//현재 시간을 기점으로 Start-End 시간을 만든다.
										_tActiveStart = _tLive;
										_tActiveEnd = _tActiveStart + _tBlend;
										_tActiveMiddle = (_tActiveStart + _tActiveEnd) * 0.5f;

										_nextUnitWeight = 0.0f;//<<아직은 0
										_prevUnitWeight = 1.0f;
									}
									else
									{
										//대기..
										//Debug.Log("Queue Ready (Remain : " + remainTime + " / Blend Time : " + _tBlend + ")");
										_nextUnitWeight = 0.0f;
										_prevUnitWeight = 1.0f;
									}

								}
								break;

							case STATUS.Active:
								if (!_isNextUnitPlayed)
								{
									if (_nextPlayUnit != null && _nextPlayUnit._ownerRequest == this)
									{
										_nextPlayUnit.Play();
									}
									_isNextUnitPlayed = true;
								}

								_tLive += tDelta;
								if (_tLive >= _tActiveEnd || _tActiveEnd < BIAS_ZERO)
								{
									_status = STATUS.End;//끝!
									_nextUnitWeight = 1.0f;
									_prevUnitWeight = 0.0f;

									for (int i = 0; i < _prevPlayUnits.Count; i++)
									{
										if (_nextPlayUnit != _prevPlayUnits[i] && _prevPlayUnits[i]._ownerRequest == this)
										{
											_prevPlayUnits[i].SetEnd();
										}
									}
								}
								else
								{
									if (_isNextPlayUnitIsInPrevUnit)
									{
										//만약 Prev 유닛에 이미 재생중이었다면
										if (_tLive < _tActiveMiddle)
										{
											//절반 동안은 서서히 내려가고 (이미 재생중이었으므로)
											_nextUnitWeight = (1.0f - (_tLive / _tActiveMiddle)) * _nextUnitWeight_Overlap;
										}
										else
										{
											//그 나머지는 1로 올라간다.
											_nextUnitWeight = ((_tLive - _tActiveMiddle) / _tActiveMiddle);
											if (!_isNextUnitFrameReset)
											{
												//프레임을 여기서 리셋한다.
												if (_nextPlayUnit != null && _nextPlayUnit._ownerRequest == this)
												{
													_nextPlayUnit.ResetPlay();
												}
												_isNextUnitFrameReset = true;
											}
										}
									}
									else
									{
										//새로운 NextUnit이 재생을 시작했다면 (기본)
										_nextUnitWeight = _tLive / _tActiveEnd;
									}

									_prevUnitWeight = 1.0f - (_tLive / _tActiveEnd);
								}
								break;

							case STATUS.End:
								_nextUnitWeight = 1.0f;
								_prevUnitWeight = 0.0f;
								break;
						}
					}
					break;

				case REQUEST_TYPE.Stop:
					{
						switch (_status)
						{
							case STATUS.Ready:
							case STATUS.Active:
								//Ready 상태가 없다. 있어도 Active로 처리


								_tLive += tDelta;
								if (_tLive >= _tActiveEnd || _tActiveEnd < BIAS_ZERO)
								{
									_status = STATUS.End;//끝!
									_nextUnitWeight = 1.0f;
									_prevUnitWeight = 0.0f;


									for (int i = 0; i < _prevPlayUnits.Count; i++)
									{
										if (_prevPlayUnits[i] != null && _prevPlayUnits[i]._ownerRequest == this)
										{
											_prevPlayUnits[i].SetEnd();
										}
									}
								}
								else
								{
									_nextUnitWeight = _tLive / _tActiveEnd;
									_prevUnitWeight = 1.0f - _nextUnitWeight;
								}
								break;

							case STATUS.End:
								_nextUnitWeight = 1.0f;
								_prevUnitWeight = 0.0f;
								break;
						}
					}
					break;

			}

		}






		// Functions
		//--------------------------------------------------
		public void AdaptWeightToPlayUnits()
		{
			//float weight2Next = _nextUnitWeight * _requestWeight;
			//float weight2Prev = (1 - _nextUnitWeight) * (_requestWeight);
			//float weight2Prev = _prevUnitWeight * _requestWeight;

			if (_nextPlayUnit != null)
			{
				_nextPlayUnit.AddWeight(_nextUnitWeight, _requestWeight);
			}

			for (int i = 0; i < _prevPlayUnits.Count; i++)
			{
				if (_prevPlayUnits[i] != null)
				{
					if (_nextPlayUnit != _prevPlayUnits[i])
					{
						_prevPlayUnits[i].AddWeight(_prevUnitWeight, _requestWeight);
					}
				}
			}
		}

		public void ReleasePlayUnitLink()
		{
			for (int i = 0; i < _prevPlayUnits.Count; i++)
			{
				if (_prevPlayUnits[i] != null && _prevPlayUnits[i]._ownerRequest == this)
				{
					_prevPlayUnits[i]._ownerRequest = null;
				}
			}
		}

		// Get / Set
		//--------------------------------------------------
		public bool IsLive { get { return _status == STATUS.Active; } }
		public bool IsEnded { get { return _status == STATUS.End; } }
		//public bool 
		//private float _tBlend = 0.0f;
		//private float _tLive = 0.0f;
		//private bool _isLive = false;
		//private bool _isFirstLive = false;

		//public enum REQUEST_TYPE
		//{
		//	New,
		//	Queued,//<<시간으로 재는 것이 아니라, 현재 대기중인 PlayData의 재생 시간을 보고 결정한다.
		//	Stop,//New와 비슷하게 처리를 하지만, 다음에 재생되는 Unit은 없다.
		//}
		//private REQUEST_TYPE _requestType = REQUEST_TYPE.New;

		////다음에 플레이하게될 PlayUnit (필수)
		//private apAnimPlayUnit _nextPlayUnit = null;

		////Queue에 들어간 PlayData 중 마지막 데이터.
		////만약 requestType이 Queued라면, Queue 상태로 저장된다. 
		//private apAnimPlayUnit _prevWaitingPlayUnit = null;




		////NextPlayUnit에 대한 Weight. 선형으로 계산된다.
		////이전 PlayUnit "전체"에 대해서는 (1-_playUnitWeight)의 값이 곱해진다.
		//private float _playUnitWeight = 0.0f;

		////Request 자체에 대한 Weight.
		////Request끼리 중첩되는 경우 (PlayUnit이 중첩되는게 아니라..)
		////Request간의 보간도 있어야 한다.
		////이때 Weight 주도권은 나중에 선언된 Request에 있다.
		//private float _requestWeight = 1.0f;

		public void SetRequestWeight(float requestWeight)
		{
			_requestWeight = requestWeight;
		}

		public void MultiplyRequestWeight(float decreaseRatio)
		{
			_requestWeight = Mathf.Clamp01(_requestWeight * decreaseRatio);
		}

		public float RequestWeight { get { return _requestWeight; } }
		public float Current2StartTime { get { return Mathf.Max(_tLive - _tActiveStart, 0); } }
		public float Current2EndTime { get { return Mathf.Max(_tActiveEnd - _tLive, 0); } }

		public REQUEST_TYPE RequestType { get { return _requestType; } }

	}
}
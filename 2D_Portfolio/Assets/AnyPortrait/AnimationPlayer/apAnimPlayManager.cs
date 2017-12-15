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
	/// 애니메이션을 통합하여 관리하는 매니저
	/// AnimClip을 재생할 때, 레이어와 블렌딩을 수행한다.
	/// MeshGroup에 포함된게 아니라 Portrait에 속하는 것이다.
	/// 어떤 MeshGroup이 출력할지도 이 매니저가 결정한다.
	/// </summary>
	[Serializable]
	public class apAnimPlayManager
	{
		// Members
		//-------------------------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;


		//런타임)
		// [Play Data]
		//      +
		// [Play Unit] -> [Play Queue] -> [Play Layer] -> 여기서 병합하여 AnimClip 각각의 Blend, Layer Index, Weight를 결정한다.
		[SerializeField]
		public List<apAnimPlayData> _animPlayDataList = new List<apAnimPlayData>();

		//에디터)
		//에디터에서는
		//단일 RootUnit 선택 + AnimClip 선택 정도
		//블렌딩은 없으며, 선택된 RootUnit과 AnimClip의 재생을 대신 하는 정도다.
		[NonSerialized]
		private apRootUnit _curRootUnitInEditor = null;

		[NonSerialized]
		private apAnimClip _curAnimClipInEditor = null;


		public bool IsPlaying_Editor
		{
			get
			{
				if (_curAnimClipInEditor == null)
				{
					return false;
				}
				return _curAnimClipInEditor.IsPlaying;
			}
		}

		[NonSerialized]
		private List<apAnimPlayQueue> _animPlayQueues = new List<apAnimPlayQueue>();

		public const int MIN_LAYER_INDEX = 0;
		public const int MAX_LAYER_INDEX = 20;//<<20까지나 필요할까나..


		public enum PLAY_OPTION
		{
			/// <summary>
			/// 플레이 시작시, 같은 레이어의 AnimClip만 중단시킨다.
			/// Fade에서도 정상 종료된다.
			/// </summary>
			StopSameLayer = 0,
			/// <summary>
			/// 플레이 시작시, 다른 레이어의 AnimClip을 모두 중단시킨다.
			/// 요청된 레이어를 기준으로 Delay, Fade시간을 계산하고, 다른 레이어에 적용한다.
			/// </summary>
			StopAllLayers = 1,
		}


		// <플레이에 대한 주석>
		// 각 애니메이션 클립은 루트 유닛에 연결된다.
		// "같은 루트 유닛"에서는 Queue, Layer가 정상적으로 작동을 한다.
		// "다른 루트 유닛"에서는, 가장 마지막에 호출된 루트 유닛을 기준으로 재생되며, 그 외에는 바로 Stop되며 무시된다.
		public apOptRootUnit _curPlayedRootUnit = null;
		private bool _isInitAndLink = false;

		// Init
		//-------------------------------------------------------
		public apAnimPlayManager()
		{
			_isInitAndLink = false;
		}

		/// <summary>
		/// 리스트를 초기화하자.
		/// </summary>
		public void InitAndLink()
		{
			if (_animPlayDataList == null)
			{
				_animPlayDataList = new List<apAnimPlayData>();
			}
			//_animPlayDataList.Clear();

			if (_animPlayQueues == null)
			{
				_animPlayQueues = new List<apAnimPlayQueue>();
			}
			else
			{
				//일단 모두 Release를 한다.
				for (int i = 0; i < _animPlayQueues.Count; i++)
				{
					_animPlayQueues[i].ReleaseForce();
				}

				_animPlayQueues.Clear();
			}

			_animPlayQueues.Clear();
			for (int i = MIN_LAYER_INDEX; i <= MAX_LAYER_INDEX; i++)
			{
				apAnimPlayQueue newPlayQueue = new apAnimPlayQueue(i, _portrait, this);
				_animPlayQueues.Add(newPlayQueue);
			}

			_isInitAndLink = true;
		}

		/// <summary>
		/// [런타임에서] Portrait를 연결하고, Portrait를 검색하여 animPlayData를 세팅한다.
		/// 다른 Link가 모두 끝난뒤에 호출하자
		/// </summary>
		/// <param name="portrait"></param>
		public void LinkPortrait(apPortrait portrait)
		{
			_portrait = portrait;

			InitAndLink();

			if (_animPlayDataList == null)
			{
				_animPlayDataList = new List<apAnimPlayData>();
			}

			apAnimPlayData animPlayData = null;

			for (int i = 0; i < _animPlayDataList.Count; i++)
			{
				animPlayData = _animPlayDataList[i];
				animPlayData._isValid = false;//일단 유효성 초기화 (나중에 값 넣으면 자동으로 true)

				apAnimClip animClip = _portrait.GetAnimClip(animPlayData._animClipID);


				apOptRootUnit rootUnit = _portrait._optRootUnitList.Find(delegate (apOptRootUnit a)
				{
					if (a._rootOptTransform != null)
					{
						if (a._rootOptTransform._meshGroupUniqueID == animPlayData._meshGroupID)
						{
							return true;
						}
					}
					return false;
				});

				if (animClip != null && rootUnit != null)
				{
					animPlayData.Link(animClip, rootUnit);

					//추가 : 여기서 ControlParamResult를 미리 만들어서 이후에 AnimClip이 미리 만들수 있게 해주자
					animClip.MakeAndLinkControlParamResults();
				}
			}
		}


		// [Runtime] Functions
		//-------------------------------------------------------
		// 제어 함수

		/// <summary>
		/// 업데이트를 한다
		/// 1차적으로 키프레임을 업데이트하고, 2차로 컨트롤 Param을 업데이트 한다.
		/// 
		/// </summary>
		/// <param name="tDelta"></param>
		public void Update(float tDelta)
		{
			if (!_isInitAndLink)
			{
				Debug.LogError("Not _isInitAndLink");
				return;
			}

			//컨트롤러 초기화 먼저
			_portrait._controller.ReadyToLayerUpdate();

			//float totalWeight = 0.0f;

			//for (int i = MIN_LAYER_INDEX; i <= MAX_LAYER_INDEX; i++)
			//{
			//	totalWeight += _animPlayQueues[i].CalculateWeight(tDelta);
			//}
			//Debug.Log("Anim PM Weight : " + totalWeight);
			//if (totalWeight > 0.0f)
			{
				for (int i = MIN_LAYER_INDEX; i <= MAX_LAYER_INDEX; i++)
				{
					//Play Queue 업데이트
					//_animPlayQueues[i].Update(tDelta, 1.0f / totalWeight);
					_animPlayQueues[i].Update(tDelta);
				}
			}

			//컨트롤러 적용
			_portrait._controller.CompleteLayerUpdate();
		}

		/// <summary>
		/// AnimPlayData가 생성 또는 삭제 되었을 때, PlayOrder를 다시 매겨준다.
		/// </summary>
		private void RefreshPlayOrders()
		{
			int playOrder = 0;
			for (int i = MIN_LAYER_INDEX; i <= MAX_LAYER_INDEX; i++)
			{
				playOrder = _animPlayQueues[i].RefreshPlayOrders(playOrder);
			}
		}

		/// <summary>
		/// AnimClip 이름을 인자로 받아서 애니메이션을 재생한다.
		/// 해당 큐에서 재생중인 모든 클립은 자동으로 종료한다.
		/// </summary>
		/// <param name="animClipName">Clip 이름. 맞지 않을 경우 처리 실패</param>
		/// <param name="layer">실행되는 레이어. 0부터 실행되며 최대값은 20</param>
		/// <param name="blendMethod">다른 레이어와 블렌드시 옵션</param>
		/// <param name="playOption">StopAllLayers인 경우 요청된 레이어 외의 Clip도 모두 종료된다.</param>
		/// <param name="isAutoEndIfNotloop">[Loop 타입이 아닌 경우] True이면 재생 종료시 자동으로 처리 데이터가 삭제된다.</param>
		/// <returns></returns>
		public apAnimPlayData Play(string animClipName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("Play Failed : No AnimClip [" + animClipName + "]");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("Play Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.Play(playData, blendMethod, 0.0f, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{
				return null;
			}

			if (playOption == PLAY_OPTION.StopAllLayers)
			{
				//다른 레이어를 모두 정지시킨다.
				for (int i = 0; i < _animPlayQueues.Count; i++)
				{
					if (i == layer)
					{ continue; }
					_animPlayQueues[i].StopAll(0.0f);
				}
			}

			RefreshPlayOrders();

			return playData;
			;
		}


		/// <summary>
		/// AnimClip 이름을 받아서 애니메이션을 이어서 재생한다.
		/// 해당 큐에서 재생 중인 "마지막 클립"이 종료하면 바로 실행할 수 있도록 저장한다.
		/// </summary>
		/// <param name="animClipName">Clip 이름. 맞지 않을 경우 처리 실패</param>
		/// <param name="layer">실행되는 레이어. 0부터 실행되며 최대값은 20</param>
		/// <param name="blendMethod">다른 레이어와 블렌드시 옵션</param>
		/// <param name="isAutoEndIfNotloop">[Loop 타입이 아닌 경우] True이면 재생 종료시 자동으로 처리 데이터가 삭제된다.</param>
		/// <returns></returns>
		public apAnimPlayData PlayQueued(string animClipName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("PlayQueued Failed : No AnimClip [" + animClipName + "]");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("PlayQueued Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.PlayQueued(playData, blendMethod, 0.0f, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{ return null; }


			//float delayTime = resultPlayUnit.DelayToPlayTime;
			//float delayTime = resultPlayUnit.RemainPlayTime;
			float delayTime = Mathf.Clamp01(resultPlayUnit.RemainPlayTime);

			//if (playOption == PLAY_OPTION.StopAllLayers)
			//{
			//	//다른 레이어를 모두 정지시킨다. - 단, 딜레이를 준다. 
			//	for (int i = 0; i < _animPlayQueues.Count; i++)
			//	{
			//		if (i == layer)
			//		{ continue; }
			//		_animPlayQueues[i].StopAll(delayTime);
			//	}
			//}

			RefreshPlayOrders();

			return playData;
		}



		/// <summary>
		/// AnimClip 이름을 인자로 받아서 애니메이션을 재생한다.
		/// Fade 타임을 받아서 다른 모든 클립에서 자연스럽게 변환된다.
		/// </summary>
		/// <param name="animClipName">Clip 이름. 맞지 않을 경우 처리 실패</param>
		/// <param name="layer">실행되는 레이어. 0부터 실행되며 최대값은 20</param>
		/// <param name="blendMethod">다른 레이어와 블렌드시 옵션</param>
		/// <param name="playOption">StopAllLayer인 경우 요청된 레이어 외의 Clip들이 다음 Clip이 실행되는 순간 같이 종료된다.</param>
		/// <param name="isAutoEndIfNotloop">[Loop 타입이 아닌 경우] True이면 재생 종료시 자동으로 처리 데이터가 삭제된다.</param>
		/// <param name="fadeTime">페이드 시간. 기본값은 0.3</param>
		/// <returns></returns>
		public apAnimPlayData CrossFade(string animClipName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("CrossFade Failed : No AnimClip [" + animClipName + "]");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("CrossFade Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			if (fadeTime < 0.0f)
			{
				fadeTime = 0.0f;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.Play(playData, blendMethod, fadeTime, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{ return null; }


			//float fadeInTime = resultPlayUnit.FadeInTime;

			if (playOption == PLAY_OPTION.StopAllLayers)
			{
				//다른 레이어를 모두 정지시킨다. - 단, 딜레이를 준다.
				for (int i = 0; i < _animPlayQueues.Count; i++)
				{
					if (i == layer)
					{ continue; }
					_animPlayQueues[i].StopAll(fadeTime);
				}
			}

			RefreshPlayOrders();

			return playData;
		}



		/// <summary>
		/// AnimClip 이름을 받아서 애니메이션을 이어서 재생한다.
		/// 해당 큐에서 재생 중인 "마지막 클립"이 종료 시점에서 Fade 시간만큼 부그럽게 이어서 실행할 수 있도록 저장한다.
		/// </summary>
		/// <param name="animClipName">Clip 이름. 맞지 않을 경우 처리 실패</param>
		/// <param name="layer">실행되는 레이어. 0부터 실행되며 최대값은 20</param>
		/// <param name="blendMethod">다른 레이어와 블렌드시 옵션</param>
		/// <param name="isAutoEndIfNotloop">[Loop 타입이 아닌 경우] True이면 재생 종료시 자동으로 처리 데이터가 삭제된다.</param>
		/// <param name="fadeTime">페이드 시간. 기본값은 0.3</param>
		/// <returns></returns>
		public apAnimPlayData CrossFadeQueued(string animClipName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("CrossFade Failed : No AnimClip [" + animClipName + "]");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("CrossFade Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			if (fadeTime < 0.0f)
			{
				fadeTime = 0.0f;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.PlayQueued(playData, blendMethod, fadeTime, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{ return null; }

			//float delayTime = resultPlayUnit.DelayToPlayTime;
			float delayTime = Mathf.Clamp01(resultPlayUnit.RemainPlayTime - fadeTime);

			//if (playOption == PLAY_OPTION.StopAllLayers)
			//{
			//	//다른 레이어를 모두 정지시킨다. - 단, 딜레이를 준다.
			//	for (int i = 0; i < _animPlayQueues.Count; i++)
			//	{
			//		if (i == layer)
			//		{ continue; }
			//		_animPlayQueues[i].StopAll(delayTime);
			//	}
			//}

			RefreshPlayOrders();

			return playData;
		}




		public void StopLayer(int layer, float blendTime = 0.0f)
		{
			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{ return; }

			_animPlayQueues[layer].StopAll(blendTime);
		}

		public void StopAll(float blendTime = 0.0f)
		{
			for (int i = 0; i < _animPlayQueues.Count; i++)
			{
				_animPlayQueues[i].StopAll(blendTime);
			}
		}

		public void PauseLayer(int layer)
		{
			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{ return; }
			_animPlayQueues[layer].Pause();
		}

		public void PauseAll()
		{
			for (int i = 0; i < _animPlayQueues.Count; i++)
			{
				_animPlayQueues[i].Pause();
			}
		}

		public void ResumeLayer(int layer)
		{
			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{ return; }
			_animPlayQueues[layer].Resume();
		}

		public void ResumeAll()
		{
			for (int i = 0; i < _animPlayQueues.Count; i++)
			{
				_animPlayQueues[i].Resume();
			}
		}




		public apAnimPlayData GetAnimPlayData_Opt(string animClipName)
		{
			//Debug.Log("Request : [" + animClipName + "]");
			//for (int i = 0; i < _animPlayDataList.Count; i++)
			//{
			//	Debug.Log("[" + i + "] : [" + _animPlayDataList[i]._animClipName + "]");
			//}
			return _animPlayDataList.Find(delegate (apAnimPlayData a)
			{
				return string.Equals(a._animClipName, animClipName);
			});
		}

		public apAnimPlayData GetAnimPlayData_Opt(apAnimClip animClip)
		{
			return _animPlayDataList.Find(delegate (apAnimPlayData a)
			{
				return a._linkedAnimClip == animClip;
			});
		}


		public void SetOptRootUnit(apOptRootUnit rootUnit)
		{
			//Debug.LogError("SetOptRootUnit [" + rootUnit.transform.name + "]");
			if (_curPlayedRootUnit != rootUnit)
			{
				_curPlayedRootUnit = rootUnit;
				_portrait.ShowRootUnit(_curPlayedRootUnit);

				//AnimQueue를 돌면서 해당 RootUnit이 아닌 PlayUnit은 강제 종료한다.
				for (int i = 0; i < _animPlayQueues.Count; i++)
				{
					_animPlayQueues[i].StopWithInvalidRootUnit(_curPlayedRootUnit);
				}
			}
		}


		public void OnAnimPlayUnitPlayStart(apAnimPlayUnit playUnit, apAnimPlayQueue playQueue)
		{
			//Play Unit이 재생을 시작했다.
			//Delay 이후에 업데이트되는 첫 프레임에 이 이벤트가 호출된다.

			// > Root Unit이 바뀔 수 있으므로 Play Manager에도 신고를 해야한다.
			SetOptRootUnit(playUnit._targetRootUnit);
		}


		public void OnAnimPlayUnitEnded(apAnimPlayUnit playUnit, apAnimPlayQueue playQueue)
		{
			//Play Unit이 재생을 종료했다
			//1. apAnimPlayUnit을 사용하고 있던 Modifier와의 연동을 해제한다.
			//??

		}

		public void OnAnyAnimPlayUnitEnded()
		{
			//Debug.Log("Anim End And Refresh Order");
			RefreshPlayOrders();
		}

		// [에디터] Functions
		//-------------------------------------------------------
		// 업데이트 함수
		/// <summary>
		/// 프레임이 변동되었다면 True를 리턴한다.
		/// </summary>
		/// <param name="tDelta"></param>
		/// <returns></returns>
		public bool Update_Editor(float tDelta)
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return false;
			}

			int curFrame = _curAnimClipInEditor.CurFrame;
			_curAnimClipInEditor.Update_Editor(tDelta, false);

			return curFrame != _curAnimClipInEditor.CurFrame;
		}


		// 제어 함수
		public void SetRootUnit_Editor(apRootUnit rootUnit)
		{
			bool isChanged = (_curRootUnitInEditor != rootUnit);
			_curRootUnitInEditor = rootUnit;

			if (isChanged)
			{
				_curAnimClipInEditor = null;
			}
		}

		public bool SetAnimClip_Editor(apAnimClip animClip)
		{
			if (_curRootUnitInEditor == null)
			{
				//선택한 RootUnit이 없다.
				return false;
			}

			if (_curRootUnitInEditor._childMeshGroup != animClip._targetMeshGroup)
			{
				//이 RootUnit을 위한 AnimClip이 아니다.
				return false;
			}
			_curAnimClipInEditor = animClip;

			_curAnimClipInEditor.Stop_Editor();
			return true;
		}


		public void Play_Editor()
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return;
			}

			_curAnimClipInEditor.Play_Editor();
		}

		public void Pause_Editor()
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return;
			}

			_curAnimClipInEditor.Pause_Editor();
		}

		public void Stop_Editor()
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return;
			}

			_curAnimClipInEditor.Stop_Editor();
		}

		public void SetFrame_Editor(int frame)
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return;
			}

			int startFrame = _curAnimClipInEditor.StartFrame;
			int endFrame = _curAnimClipInEditor.EndFrame;

			int nextFrame = Mathf.Clamp(frame, startFrame, endFrame);

			_curAnimClipInEditor.SetFrame_Editor(nextFrame);
		}

		public float CurAnimFrameFloat_Editor
		{
			get
			{
				if (_curAnimClipInEditor == null)
				{ return 0.0f; }
				return _curAnimClipInEditor.CurFrameFloat;
			}
		}

		public apAnimClip CurAnimClip_Editor { get { return _curAnimClipInEditor; } }
		public apRootUnit CurRootUnit_Editor { get { return _curRootUnitInEditor; } }



		// Get / Set
		//-------------------------------------------------------
		//디버그용
		public List<apAnimPlayQueue> PlayQueueList
		{
			get
			{
				return _animPlayQueues;
			}
		}

		public List<apAnimPlayData> PlayDataList
		{
			get
			{
				return _animPlayDataList;
			}
		}
	}

}
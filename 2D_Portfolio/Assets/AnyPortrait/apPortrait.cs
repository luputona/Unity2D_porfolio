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

	public class apPortrait : MonoBehaviour
	{
		// Members
		//-----------------------------------------------------
		//public int _testVar = 0;

		//텍스쳐 등록 정보
		[SerializeField]
		public List<apTextureData> _textureData = new List<apTextureData>();

		//메시 등록 정보
		[SerializeField]
		public List<apMesh> _meshes = new List<apMesh>();

		//메시 그룹 등록 정보
		[SerializeField]
		public List<apMeshGroup> _meshGroups = new List<apMeshGroup>();


		//컨트롤 파라미터 등록 정보 [이건 Editor / Opt Realtime에 모두 적용된다.]
		[SerializeField]
		public apController _controller = new apController();

		//애니메이션 등록 정보 [이건 Editor / Opt Runtime에 모두 적용된다]
		[SerializeField]
		public List<apAnimClip> _animClips = new List<apAnimClip>();



		[SerializeField]
		public apAnimPlayManager _animPlayManager = new apAnimPlayManager();

		//메인 MeshGroup
		//[SerializeField]
		//public int _mainMeshGroupID = -1;

		//[NonSerialized]
		//public apMeshGroup _mainMeshGroup = null;//<<이건 ID에 맞게 체크한다.

		//>>수정 : RootUnit으로 적용되는 MainMeshGroup을 여러개를 둔다.
		[SerializeField]
		public List<int> _mainMeshGroupIDList = new List<int>();

		[NonSerialized]
		public List<apMeshGroup> _mainMeshGroupList = new List<apMeshGroup>();



		// 변경) 루트 유닛을 여러 개를 둔다 (루트 유닛은 애니메이션이 적용되는 MeshGroup이다)
		//[SerializeField]
		//public apRootUnit _rootUnit = new apRootUnit();//<<이건 단 하나 있습니다.

		[SerializeField]
		public List<apRootUnit> _rootUnits = new List<apRootUnit>();



		//MeshGroup에 등록된 Tranform과 RenderUnit
		//[NonSerialized]
		//public List<apTransform_MeshGroup> _transforms_MeshGroup = new List<apTransform_MeshGroup>();

		//[NonSerialized]
		//public List<apTransform_Mesh> _transforms_Mesh = new List<apTransform_Mesh>();

		//[NonSerialized]
		//public List<apRenderUnit> _renderUnits = new List<apRenderUnit>();


		// 유니크 IDs
		#region [미사용 코드] ID Manager로 통합했다.
		//[NonSerialized]
		//private List<int> _registeredUniqueIDs_Texture = new List<int>();

		//[NonSerialized]
		//private List<int> _registeredUniqueIDs_Vert = new List<int>();

		//[NonSerialized]
		//private List<int> _registeredUniqueIDs_Mesh = new List<int>();

		//[NonSerialized]
		//private List<int> _registeredUniqueIDs_MeshGroup = new List<int>();

		//[NonSerialized]
		//private List<int> _registeredUniqueIDs_Transform = new List<int>();//Transform은 mesh, Meshgroup 구분하지 않고 다 넣어준다.

		//[NonSerialized]
		//private List<int> _registeredUniqueIDs_Modifier = new List<int>();

		//[NonSerialized]
		//private List<int> _registeredUniqueIDs_ControlParam = new List<int>();

		//[NonSerialized]
		//private List<int> _registeredUniqueIDs_AnimClip = new List<int>(); 
		#endregion
		private apIDManager _IDManager = new apIDManager();



		//Runtime 계열 Members
		// 이후 "최적화" 버전에서는 이하의 Member만 적용한다.
		// Runtime 계열의 인스턴스들은 모두 opt를 앞에 붙인다.
		//---------------------------------------------
		[SerializeField]
		public List<apOptRootUnit> _optRootUnitList = new List<apOptRootUnit>();

		[NonSerialized]
		public apOptRootUnit _curPlayingOptRootUnit = null;//<<현재 재생중인 OptRootUnit

		//public apOptRootUnit _optRootUnit = null;

		[SerializeField]
		public List<apOptTransform> _optTransforms = null;

		[SerializeField]
		public List<apOptMesh> _optMeshes = null;

		[SerializeField]
		public List<apOptMesh> _optMaskedMeshes = null;//<<마스크 메시는 따로 관리하여 업데이트를 해야한다.

		[SerializeField]
		public List<apOptMesh> _optClippedMeshes = null;//<<클립 메시는 따로 관리하여 업데이트를 해야한다.

		public bool _isAnyMaskedMeshes = false;

		[NonSerialized]
		private bool _isInitLink = false;



		//기본 데이터가 저장될 하위 GameObject
		public GameObject _subObjectGroup = null;
		//작업 저장 효율성을 위해서 일부 메인 데이터를 GameObject 형식으로 저장한다. (크윽 Undo)
		//저장되는 타겟은 Mesh와 MeshGroup
		//- Mesh는 Vertex 데이터가 많아서 저장 필요성이 있다.
		//- MeshGroup은 ModMesh의 데이터가 많아서 저장 필요성이 있다.
		//직렬화가 가능하거나 연동 전용 객체인 RootUnit과 Image은 GameObject로 만들지 않는다.
		//AnimClip과 Param은 Realtime과 연동하는 객체이므로 별도로 분리하지 않는다.
		public GameObject _subObjectGroup_Mesh = null;
		public GameObject _subObjectGroup_MeshGroup = null;



		[SerializeField]
		public int _FPS = 30;

		private float _timePerFrame = 1.0f / 30.0f;
		private float _tDelta = 0.0f;

		[SerializeField, HideInInspector]
		public float _bakeScale = 0.01f;//Bake시 0.01을 곱한다.


		//물리 크기 삭제
		//[SerializeField, HideInInspector]
		//public float _physicBakeScale = 1f;//Bake시 물리 값들을 일괄적으로 약간 더 Scale을 할 수 있다. (에디터와 씬에서 다르게 작동하기 때문)


		//이미지 저장 경로를 저장하자
		[SerializeField, HideInInspector]
		public string _imageFilePath_Thumbnail = "";

		[NonSerialized]
		public Texture2D _thumbnailImage = null;




		public enum SHADER_TYPE
		{
			/// <summary>(기본값) 알파블렌드</summary>
			AlphaBlend = 0,
			/// <summary>더하기</summary>
			Additive = 1,
			/// <summary>부드럽게 더하기</summary>
			SoftAdditive = 2,
			/// <summary>곱하기</summary>
			Multiplicative = 3
		}

		[SerializeField]
		public bool _isGPUAccel = true;

		//물리 옵션 - Editor / Opt (기본값은 On)
		[SerializeField]
		public bool _isPhysicsPlay_Editor = true;

		[NonSerialized]
		public bool _isPhysicsSupport_Editor = true;//<<옵션과 관계없이 지금 물리를 지원하는가



		[NonSerialized]
		public bool _isPhysicsPlay_Opt = true;

		[NonSerialized]
		public int _updateCount = 0;

		//[NonSerialized]
		//private int _updateKeyIndex = 0;

		//public int UpdateKeyIndex { get { return _updateKeyIndex; } }


		//물리에 주는 외력을 관리하는 객체
		//저장되는 값은 없고, API만 제공한다.
		//Editor/Runtime 모두 사용 가능
		private apForceManager _forceManager = new apForceManager();
		public apForceManager ForceManager { get { return _forceManager; } }


		// Init
		//-----------------------------------------------------
		void Awake()
		{
			if (Application.isPlaying)
			{
				_isInitLink = false;
				if (_FPS < 10)
				{
					_FPS = 10;
				}
				_timePerFrame = 1.0f / (float)_FPS;
				_tDelta = _timePerFrame;
				//_updateKeyIndex = 0;
			}
		}



		void Start()
		{
			if (Application.isPlaying)
			{
				if (_FPS < 10)
				{
					_FPS = 10;
				}
				_timePerFrame = 1.0f / (float)_FPS;
				_tDelta = _timePerFrame;

				if (!_isInitLink)
				{
					LinkOpt();

					//첫 RootUnit을 기본으로 잡자
					if (_optRootUnitList.Count > 0)
					{
						ShowRootUnit(_optRootUnitList[0]);
					}

					_updateCount = 0;
					//_updateKeyIndex = 0;
				}
			}
		}

		// Update
		//-----------------------------------------------------
		void Update()
		{
#if UNITY_EDITOR
			try
			{
				if (Application.isPlaying)
				{
#endif
					#region [미사용 코드 : 삭제 예정]
					//if (_optRootUnit == null)
					//{
					//	return;
					//}
					//if(_curPlayingOptRootUnit == null)
					//{
					//	return;
					//}


					//testTimer += Time.deltaTime;
					//if(testTimer > 1.0f)
					//{
					//	testTimer -= 1.0f;
					//} 
					#endregion

					#region [미사용 코드]
					//if (Time.deltaTime > 0)
					//{
					//	_updateCount++;
					//	//_updateKeyIndex++;
					//	//if (_updateKeyIndex > 9999)
					//	//{
					//	//	_updateKeyIndex = 0;
					//	//}
					//} 
					#endregion

					////힘 관련 업데이트
					//ForceManager.Update(Time.deltaTime);

					////TODO : Animation이 실행중이라면 AnimPlayerManager가 관리하고,
					////그게 아니면 그냥 현재 RootUnit를 자체적으로 업데이트 한다.
					//_animPlayManager.Update(Time.deltaTime);

					if (_curPlayingOptRootUnit == null)
					{
						return;
					}


#region [미사용 코드 : LateUpdate로 넘어감]
//					_tDelta += Time.deltaTime;
//					//if (_tDelta > _timePerFrame)
//					//if(true)
//					{
//						//_tDelta -= _timePerFrame;//아래에 갱신한 부분이 있다.

//						//전체 업데이트하는 코드

//						//일정 프레임마다 업데이트를 한다.
//#if UNITY_EDITOR
//						Profiler.BeginSample("Portrait - Update Transform");
//#endif




//						//_optRootUnit.UpdateTransforms(_tDelta);
//						//_curPlayingOptRootUnit.UpdateTransforms(_tDelta);
//						//_curPlayingOptRootUnit.UpdateTransforms(_timePerFrame);


//						//원래는 이 코드
//						_curPlayingOptRootUnit.UpdateTransforms(Time.deltaTime);//<

//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif


//						//mask Mesh의 업데이트는 모든 Mesh 처리가 끝나고 한다.
//						if (_isAnyMaskedMeshes)
//						{

//#if UNITY_EDITOR
//							Profiler.BeginSample("Portrait - Post Update <Mask>");
//#endif

//							//Mask Parent 중심의 업데이트 삭제 -> Child 중심의 업데이트로 변경
//							//for (int i = 0; i < _optMaskedMeshes.Count; i++)
//							//{
//							//	_optMaskedMeshes[i].RefreshMaskedMesh();
//							//}

//							for (int i = 0; i < _optClippedMeshes.Count; i++)
//							{
//								_optClippedMeshes[i].RefreshClippedMesh();
//							}

//#if UNITY_EDITOR
//							Profiler.EndSample();
//#endif

//						}

//						_tDelta -= _timePerFrame;
//						//_tDelta = 0.0f;//Delatyed tDelta라면 0으로 바꾸자 


//					}
#endregion


#if UNITY_EDITOR
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Portrait Exception : " + ex.ToString());
			}
#endif
		}



		void LateUpdate()
		{
			if (Application.isPlaying)
			{
				//if(_curPlayingOptRootUnit == null)
				//{
				//	return;
				//}

#region [Update에서 넘어온 코드]
					_tDelta += Time.deltaTime;
					//if (_tDelta > _timePerFrame)
					//if(true)
					if(_curPlayingOptRootUnit != null)
					{
						//_tDelta -= _timePerFrame;//아래에 갱신한 부분이 있다.

						//전체 업데이트하는 코드

						//일정 프레임마다 업데이트를 한다.
#if UNITY_EDITOR
						Profiler.BeginSample("Portrait - Update Transform");
#endif




						//_optRootUnit.UpdateTransforms(_tDelta);
						//_curPlayingOptRootUnit.UpdateTransforms(_tDelta);
						//_curPlayingOptRootUnit.UpdateTransforms(_timePerFrame);


						//원래는 이 코드
						_curPlayingOptRootUnit.UpdateTransforms(Time.deltaTime);//<

#if UNITY_EDITOR
						Profiler.EndSample();
#endif


						//mask Mesh의 업데이트는 모든 Mesh 처리가 끝나고 한다.
						if (_isAnyMaskedMeshes)
						{

#if UNITY_EDITOR
							Profiler.BeginSample("Portrait - Post Update <Mask>");
#endif

							//Mask Parent 중심의 업데이트 삭제 -> Child 중심의 업데이트로 변경
							//for (int i = 0; i < _optMaskedMeshes.Count; i++)
							//{
							//	_optMaskedMeshes[i].RefreshMaskedMesh();
							//}

							for (int i = 0; i < _optClippedMeshes.Count; i++)
							{
								_optClippedMeshes[i].RefreshClippedMesh();
							}

#if UNITY_EDITOR
							Profiler.EndSample();
#endif

						}

						_tDelta -= _timePerFrame;
						//_tDelta = 0.0f;//Delatyed tDelta라면 0으로 바꾸자 


					}

				//이걸 여기다둬서 1프레임 지연을 하자
				//힘 관련 업데이트
				ForceManager.Update(Time.deltaTime);

				//TODO : Animation이 실행중이라면 AnimPlayerManager가 관리하고,
				//그게 아니면 그냥 현재 RootUnit를 자체적으로 업데이트 한다.
				_animPlayManager.Update(Time.deltaTime);
#endregion
			}
		}


		public void UpdateForce()
		{
			//강제로 업데이트를 한다.
#if UNITY_EDITOR
			try
			{
#endif
				if (!_isInitLink)
				{
					LinkOpt();
				}

				//_updateKeyIndex++;
				//if(_updateKeyIndex > 9999)
				//{
				//	_updateKeyIndex = 0;
				//}
				//TODO : Animation이 실행중이라면 AnimPlayerManager가 관리하고,
				//그게 아니면 그냥 현재 RootUnit를 자체적으로 업데이트 한다.

				//if (_optRootUnit == null)
				//{
				//	return;
				//}

				if (_animPlayManager.IsPlaying_Editor)
				{
					bool isFrameChanged = _animPlayManager.Update_Editor(0.0f);

				}
				else
				{
					if (_curPlayingOptRootUnit != null)
					{
						_curPlayingOptRootUnit.UpdateTransforms(0.0f);
					}
				}

				//일정 프레임마다 업데이트를 한다.
				//_optRootUnit.UpdateTransforms(_tDelta);


				//mask Mesh의 업데이트는 모든 Mesh 처리가 끝나고 한다.
				if (_isAnyMaskedMeshes)
				{
					//for (int i = 0; i < _optMaskedMeshes.Count; i++)
					//{
					//	_optMaskedMeshes[i].RefreshMaskedMesh();
					//}
					for (int i = 0; i < _optClippedMeshes.Count; i++)
					{
						_optClippedMeshes[i].RefreshClippedMesh();
					}
				}

#if UNITY_EDITOR
			}
			catch (Exception ex)
			{
				Debug.LogError("Portrait Exception : " + ex.ToString());
			}
#endif
		}



		// Functions
		//-----------------------------------------------------
		public void ShowRootUnit(apOptRootUnit targetOptRootUnit)
		{
			_curPlayingOptRootUnit = null;
			apOptRootUnit optRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				optRootUnit = _optRootUnitList[i];
				if (optRootUnit == targetOptRootUnit)
				{
					//이건 Show를 하자
					optRootUnit.Show();
					_curPlayingOptRootUnit = targetOptRootUnit;
				}
				else
				{
					//이건 Hide
					optRootUnit.Hide();
				}
			}
		}

		public void HideRootUnits()
		{
			_curPlayingOptRootUnit = null;

			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				_optRootUnitList[i].Hide();
			}
		}

		//public void IncreaseUpdateKeyIndex()
		//{
		//	_updateKeyIndex++;
		//	if(_updateKeyIndex > 9999)
		//	{
		//		_updateKeyIndex = 0;
		//	}
		//}

		public void SetPhysicEnabled(bool isPhysicEnabled)
		{
			_isPhysicsPlay_Opt = isPhysicEnabled;
		}
		//--------------------------------------------------------------------------------------
		// Runtime Optimized
		//--------------------------------------------------------------------------------------
		/// <summary>
		/// 첫 Bake 후 또는 시작후 로딩시 Modifier -> 해당 OptTransform을 연결한다.
		/// </summary>
		public void LinkOpt()
		{
			//Debug.Log("LinkModifierAndMeshGroups_Opt");

			_isInitLink = true;


			//MeshGroup -> OptTransform을 돌면서 처리
			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				apOptTransform optTransform = _optTransforms[iOptTransform];
				optTransform.ClearResultParams();
			}

			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].InitMesh(true);
			}

			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				apOptTransform optTransform = _optTransforms[iOptTransform];

				List<apOptModifierUnitBase> modifiers = optTransform._modifierStack._modifiers;
				for (int iMod = 0; iMod < modifiers.Count; iMod++)
				{
					apOptModifierUnitBase mod = modifiers[iMod];

					//추가 : Portrait를 연결해준다.
					mod.Link(this);

					//mod._meshGroup = GetMeshGroup(mod._meshGroupUniqueID);

					////삭제 조건1 - MeshGroup이 없다
					//if (mod._meshGroup == null)
					//{
					//	continue;
					//}

					List<apOptParamSetGroup> paramSetGroups = mod._paramSetGroupList;
					for (int iPSGroup = 0; iPSGroup < paramSetGroups.Count; iPSGroup++)
					{
						apOptParamSetGroup paramSetGroup = paramSetGroups[iPSGroup];

						//List<apModifierParamSet> paramSets = mod._paramSetList;
						//1. Key를 세팅해주자
						switch (paramSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.Static:
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								//paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamName);
								paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamID);
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								//Debug.LogError("TODO : KeyFrame 방식 연동");
								break;
						}


						List<apOptParamSet> paramSets = paramSetGroup._paramSetList;

						for (int iParamSet = 0; iParamSet < paramSets.Count; iParamSet++)
						{
							apOptParamSet paramSet = paramSets[iParamSet];

							//Link를 해주자
							paramSet.LinkParamSetGroup(paramSetGroup, this);



							#region [미사용 코드] Editor와 달리 여기서는 Monobehaviour인 것도 있고, Bake에서 이미 1차적으로 연결을 완료했다.
							//List<apOptModifiedMesh> meshData = paramSet._meshData;
							//for (int iMesh = 0; iMesh < meshData.Count; iMesh++)
							//{
							//	apOptModifiedMesh modMesh = meshData[iMesh];

							//	//modMesh._meshGroupUniqueID = meshGroup._uniqueID;

							//	switch (modMesh._targetType)
							//	{
							//		case apModifiedMesh.TARGET_TYPE.VertexMorph:
							//			{
							//				modMesh.Link
							//				meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
							//				if (meshTransform != null)
							//				{
							//					renderUnit = meshGroup.GetRenderUnit(meshTransform);
							//					modMesh.Link_VertexMorph(meshGroup, meshTransform, renderUnit);
							//				}
							//			}
							//			break;

							//		case apModifiedMesh.TARGET_TYPE.MeshTransform:
							//			{
							//				meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
							//				if (meshTransform != null)
							//				{
							//					renderUnit = meshGroup.GetRenderUnit(meshTransform);
							//					modMesh.Link_MeshTransform(meshGroup, meshTransform, renderUnit);
							//				}
							//			}
							//			break;

							//		case apModifiedMesh.TARGET_TYPE.MeshGroupTransform:
							//			{
							//				meshGroupTransform = meshGroup.GetMeshGroupTransform(modMesh._transformUniqueID);
							//				if (meshGroupTransform != null)
							//				{
							//					renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
							//					modMesh.Link_MeshGroupTransform(meshGroup, meshGroupTransform, renderUnit);
							//				}
							//			}
							//			break;

							//		case apModifiedMesh.TARGET_TYPE.Bone:
							//			{
							//				//TODO : Bone 처리도 해주자
							//				modMesh.Link_Bone();
							//			}
							//			break;
							//	}

							//}

							//paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
							//{
							//	return a._meshGroup == null;//<<연동이 안된..
							//});

							//List<apModifiedBone> boneData = paramSet._boneData;
							////TODO : 본 연동 
							#endregion
						}
					}
				}

				optTransform.RefreshModifierLink();
			}

			for (int i = 0; i < _animClips.Count; i++)
			{
				_animClips[i].LinkOpt(this);
			}

			//추가) AnimPlayer를 추가했다.
			_animPlayManager.LinkPortrait(this);

			//추가) Compute Shader를 지정하자.
			if (apComputeShader.I.IsComputeShaderNeedToLoad_Opt)
			{
				apComputeShader.I.SetComputeShaderAsset_Opt(Resources.Load<ComputeShader>("AnyPortraitShader/apCShader_OptVertWorldTransform"));
			}
		}



		//--------------------------------------------------------------------------------------
		// Editor
		//--------------------------------------------------------------------------------------


		// Get / Set
		//-----------------------------------------------------



		//--------------------------------------------------------------------------------------
		// API
		//--------------------------------------------------------------------------------------
		// Play
		//--------------------------------------------------------------------------------------
		public apAnimPlayData Play(string animClipName,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			return _animPlayManager.Play(animClipName, layer, blendMethod, playOption, isAutoEndIfNotloop);
		}

		public apAnimPlayData PlayQueued(string animClipName,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			return _animPlayManager.PlayQueued(animClipName, layer, blendMethod, isAutoEndIfNotloop);
		}

		public apAnimPlayData CrossFade(string animClipName,
											float fadeTime = 0.3f,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			return _animPlayManager.CrossFade(animClipName, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
		}

		public apAnimPlayData CrossFadeQueued(string animClipName,
												float fadeTime = 0.3f,
												int layer = 0,
												apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
												bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			return _animPlayManager.CrossFadeQueued(animClipName, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
		}

		public void StopLayer(int layer, float fadeTime = 0.0f)
		{
			if (_animPlayManager == null)
			{ return; }

			_animPlayManager.StopLayer(layer, fadeTime);
		}

		public void StopAll(float fadeTime = 0.0f)
		{
			if (_animPlayManager == null)
			{ return; }

			_animPlayManager.StopAll(fadeTime);
		}

		public void PauseLayer(int layer)
		{
			if (_animPlayManager == null)
			{ return; }

			_animPlayManager.PauseLayer(layer);
		}

		public void PauseAll()
		{
			if (_animPlayManager == null)
			{ return; }

			_animPlayManager.PauseAll();
		}

		public void ResumeLayer(int layer)
		{
			if (_animPlayManager == null)
			{ return; }

			_animPlayManager.ResumeLayer(layer);
		}

		public void ResumeAll()
		{
			if (_animPlayManager == null)
			{ return; }

			_animPlayManager.ResumeAll();
		}

		public apAnimPlayManager PlayManager
		{
			get
			{
				return _animPlayManager;
			}
		}

		//---------------------------------------------------------------------------------------
		// 물리 제어
		//---------------------------------------------------------------------------------------
		public void ClearForceAndTouch()
		{
			_forceManager.ClearAll();
		}
		public void ClearForce()
		{
			_forceManager.ClearForce();
		}

		public apForceUnit AddForce_Point(Vector2 pointPosW, float radius)
		{
			return _forceManager.AddForce_Point(pointPosW, radius);
		}

		public apForceUnit AddForce_Direction(Vector2 directionW)
		{
			return _forceManager.AddForce_Direction(directionW);
		}

		public apForceUnit AddForce_Direction(Vector2 directionW, float waveSizeX, float waveSizeY, float waveTimeX, float waveTimeY)
		{
			return _forceManager.AddForce_Direction(directionW, new Vector2(waveSizeX, waveSizeY), new Vector2(waveTimeX, waveTimeY));
		}

		public bool IsAnyForceEvent
		{
			get { return _forceManager.IsAnyForceEvent; }
		}

		public Vector2 GetForce(Vector2 targetPosW)
		{
			return _forceManager.GetForce(targetPosW);
		}


		public apPullTouch AddTouch(Vector2 posW, float radius)
		{
			return _forceManager.AddTouch(posW, radius);
		}

		public void ClearTouch()
		{
			_forceManager.ClearTouch();
		}

		public void RemoveTouch(int touchID)
		{
			_forceManager.RemoveTouch(touchID);
		}

		public apPullTouch GetTouch(int touchID)
		{
			return _forceManager.GetTouch(touchID);
		}

		public void SetTouchPosition(int touchID, Vector2 posW)
		{
			_forceManager.SetTouchPosition(touchID, posW);
		}

		public void SetTouchPosition(apPullTouch touch, Vector2 posW)
		{
			_forceManager.SetTouchPosition(touch, posW);
		}

		public bool IsAnyTouchEvent { get { return _forceManager.IsAnyTouchEvent; } }
		public int TouchProcessCode { get { return _forceManager.TouchProcessCode; } }

		//--------------------------------------------------------------------------------------
		// Control Param
		//--------------------------------------------------------------------------------------
		
		public bool SetControlParamInt(string controlParamName, int intValue)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			controlParam._int_Cur = intValue;
			//if(controlParam._isRange)
			{
				controlParam._int_Cur = Mathf.Clamp(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);
			}

			return true;
		}

		public bool SetControlParamFloat(string controlParamName, float floatValue)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			controlParam._float_Cur = floatValue;
			//if(controlParam._isRange)
			{
				controlParam._float_Cur = Mathf.Clamp(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);
			}

			return true;
		}

		public bool SetControlParamVector2(string controlParamName, Vector2 vec2Value)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			controlParam._vec2_Cur = vec2Value;
			//if(controlParam._isRange)
			{
				controlParam._vec2_Cur.x = Mathf.Clamp(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
				controlParam._vec2_Cur.y = Mathf.Clamp(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);
			}

			return true;
		}



		public bool SetControlParamInt(apControlParam controlParam, int intValue)
		{
			if (controlParam == null)
			{ return false; }

			controlParam._int_Cur = intValue;
			//if(controlParam._isRange)
			{
				controlParam._int_Cur = Mathf.Clamp(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);
			}

			return true;
		}

		public bool SetControlParamFloat(apControlParam controlParam, float floatValue)
		{
			if (controlParam == null)
			{ return false; }

			controlParam._float_Cur = floatValue;
			//if(controlParam._isRange)
			{
				controlParam._float_Cur = Mathf.Clamp(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);
			}

			return true;
		}

		public bool SetControlParamVector2(apControlParam controlParam, Vector2 vec2Value)
		{
			if (controlParam == null)
			{ return false; }

			controlParam._vec2_Cur = vec2Value;
			//if(controlParam._isRange)
			{
				controlParam._vec2_Cur.x = Mathf.Clamp(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
				controlParam._vec2_Cur.y = Mathf.Clamp(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);
			}

			return true;
		}



		public bool IsControlParamExist(string controlParamName)
		{
			return GetControlParam(controlParamName) != null;
		}

		public bool SetControlParamDefaultValue(string controlParamName)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					controlParam._int_Cur = controlParam._int_Def;
					break;

				case apControlParam.TYPE.Float:
					controlParam._float_Cur = controlParam._float_Def;
					break;

				case apControlParam.TYPE.Vector2:
					controlParam._vec2_Cur = controlParam._vec2_Def;
					break;
			}

			return true;
		}
		




		// 초기화
		//----------------------------------------------------------------

		public void ReadyToEdit()
		{

			//ID 리스트 일단 리셋
			ClearRegisteredUniqueIDs();

			//컨트롤 / 컨트롤 파라미터 리셋
			_controller.Ready(this);
			_controller.SetDefaultAll();


			for (int iTexture = 0; iTexture < _textureData.Count; iTexture++)
			{
				_textureData[iTexture].ReadyToEdit(this);
			}

			for (int iMeshes = 0; iMeshes < _meshes.Count; iMeshes++)
			{
				//내부 MeshComponent들의 레퍼런스를 연결하자
				_meshes[iMeshes].ReadyToEdit(this);

				//텍스쳐를 연결하자
				int textureID = -1;
				if (_meshes[iMeshes]._textureData != null)
				{
					textureID = _meshes[iMeshes]._textureData._uniqueID;
					_meshes[iMeshes]._textureData = GetTexture(textureID);
				}

				_meshes[iMeshes].LinkEdgeAndVertex();
			}

			//메시 그룹도 비슷하게 해주자
			//1. 메시/메시 그룹을 먼저 연결
			//2. Parent-Child는 그 다음에 연결 (Child 먼저 / Parent는 나중에)
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				meshGroup.Init(this);

				//1. Mesh 연결
				for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
				{
					meshGroup._childMeshTransforms[iChild].RegistIDToPortrait(this);//추가 : ID를 알려주자

					int childIndex = meshGroup._childMeshTransforms[iChild]._meshUniqueID;
					if (childIndex >= 0)
					{
						apMesh existMesh = GetMesh(childIndex);
						if (existMesh != null)
						{
							meshGroup._childMeshTransforms[iChild]._mesh = existMesh;
						}
						else
						{
							meshGroup._childMeshTransforms[iChild]._mesh = null;
						}
					}
					else
					{
						meshGroup._childMeshTransforms[iChild]._mesh = null;
					}
				}

				//1-2. MeshGroup 연결
				for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					meshGroup._childMeshGroupTransforms[iChild].RegistIDToPortrait(this);//추가 : ID를 알려주자

					int childIndex = meshGroup._childMeshGroupTransforms[iChild]._meshGroupUniqueID;
					if (childIndex >= 0)
					{
						apMeshGroup existMeshGroup = GetMeshGroup(childIndex);
						if (existMeshGroup != null)
						{
							meshGroup._childMeshGroupTransforms[iChild]._meshGroup = existMeshGroup;
						}
						else
						{
							meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
						}
					}
					else
					{
						meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
					}
				}
			}

			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//2. 하위 MeshGroup 연결
				for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					apTransform_MeshGroup childMeshGroupTransform = meshGroup._childMeshGroupTransforms[iChild];

					if (childMeshGroupTransform._meshGroupUniqueID >= 0)
					{
						apMeshGroup existMeshGroup = GetMeshGroup(childMeshGroupTransform._meshGroupUniqueID);
						if (existMeshGroup != null)
						{
							childMeshGroupTransform._meshGroup = existMeshGroup;

							childMeshGroupTransform._meshGroup._parentMeshGroupID = meshGroup._uniqueID;
							childMeshGroupTransform._meshGroup._parentMeshGroup = meshGroup;


						}
						else
						{
							childMeshGroupTransform._meshGroup = null;
						}
					}
				}

				//다만, 없어진 Mesh Group은 정리해주자
				meshGroup._childMeshTransforms.RemoveAll(delegate (apTransform_Mesh a)
				{
					return a._mesh == null;
				});
				meshGroup._childMeshGroupTransforms.RemoveAll(delegate (apTransform_MeshGroup a)
				{
					return a._meshGroup == null;
				});
			}


			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//추가) Clipping Layer를 위해서 Mesh Transform끼리 연결을 해준다.
				for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
				{
					//연결하기 전에
					//Child는 초기화해준다.
					apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iChild];
					meshTransform._isClipping_Child = false;
					meshTransform._clipIndexFromParent = -1;
					meshTransform._clipParentMeshTransform = null;

					if (meshTransform._clipChildMeshes == null)
					{
						meshTransform._clipChildMeshes = new List<apTransform_Mesh.ClipMeshSet>();
					}

					meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
					{
						return a._transformID < 0;
					});



					#region [미사용 코드]
					//if (meshTransform._clipChildMeshTransformIDs == null || meshTransform._clipChildMeshTransformIDs.Length != 3)
					//{
					//	meshTransform._clipChildMeshTransformIDs = new int[] { -1, -1, -1 };
					//}
					//if (meshTransform._clipChildMeshTransforms == null || meshTransform._clipChildMeshTransforms.Length != 3)
					//{
					//	meshTransform._clipChildMeshTransforms = new apTransform_Mesh[] { null, null, null };
					//}
					//if (meshTransform._clipChildRenderUnits == null || meshTransform._clipChildRenderUnits.Length != 3)
					//{
					//	meshTransform._clipChildRenderUnits = new apRenderUnit[] { null, null, null };
					//} 
					#endregion
				}

				for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
				{
					apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iChild];
					if (meshTransform._isClipping_Parent)
					{
						//최대 3개의 하위 Mesh를 검색해서 연결한다.
						//찾은 이후엔 Sort를 해준다.

						for (int iClip = 0; iClip < meshTransform._clipChildMeshes.Count; iClip++)
						{
							apTransform_Mesh.ClipMeshSet clipSet = meshTransform._clipChildMeshes[iClip];
							int childMeshID = clipSet._transformID;
							apTransform_Mesh childMeshTF = meshGroup.GetMeshTransform(childMeshID);
							if (childMeshTF != null)
							{
								clipSet._meshTransform = childMeshTF;
								clipSet._renderUnit = meshGroup.GetRenderUnit(childMeshTF);
							}
							else
							{
								clipSet._meshTransform = null;
								clipSet._transformID = -1;
								clipSet._renderUnit = null;
							}
						}

						#region [미사용 코드]
						//for (int iChildMesh = 0; iChildMesh < 3; iChildMesh++)
						//{
						//	int childMeshID = meshTransform._clipChildMeshTransformIDs[iChildMesh];
						//	apTransform_Mesh childMeshTF = meshGroup.GetMeshTransform(childMeshID);
						//	if (childMeshTF != null)
						//	{
						//		meshTransform._clipChildMeshTransforms[iChildMesh] = childMeshTF;
						//		meshTransform._clipChildRenderUnits[iChildMesh] = meshGroup.GetRenderUnit(childMeshTF);
						//	}
						//	else
						//	{
						//		meshTransform._clipChildMeshTransforms[iChildMesh] = null;
						//		meshTransform._clipChildMeshTransformIDs[iChildMesh] = -1;
						//		meshTransform._clipChildRenderUnits[iChildMesh] = null;
						//	}
						//} 
						#endregion
					}
					else
					{
						meshTransform._clipChildMeshes.Clear();

						#region [미사용 코드]
						//for (int iChildMesh = 0; iChildMesh < 3; iChildMesh++)
						//{
						//	meshTransform._clipChildMeshTransformIDs[iChildMesh] = -1;
						//	meshTransform._clipChildMeshTransforms[iChildMesh] = null;
						//	meshTransform._clipChildRenderUnits[iChildMesh] = null;
						//} 
						#endregion
					}

					meshTransform.SortClipMeshTransforms();
				}

			}



			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//2. 상위 MeshGroup 연결
				int parentUniqueID = meshGroup._parentMeshGroupID;
				if (parentUniqueID >= 0)
				{
					meshGroup._parentMeshGroup = GetMeshGroup(parentUniqueID);
					if (meshGroup._parentMeshGroup == null)
					{
						meshGroup._parentMeshGroupID = -1;
					}
				}
				else
				{
					meshGroup._parentMeshGroup = null;
				}
			}

			//Bone 연결 
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//Root 리스트는 일단 날리고 BoneAll 리스트를 돌면서 필요한걸 넣어주자
				//이후엔 Root -> Child 방식으로 순회
				meshGroup._boneList_Root.Clear();
				if (meshGroup._boneList_All != null)
				{
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						apBone bone = meshGroup._boneList_All[iBone];

						//먼저 ID를 ID Manager에 등록한다.
						RegistUniqueID(apIDManager.TARGET.Bone, bone._uniqueID);

						apBone parentBone = null;
						if (bone._parentBoneID >= 0)
						{
							parentBone = meshGroup.GetBone(bone._parentBoneID);
						}

						bone.Link(meshGroup, parentBone);

						if (parentBone == null)
						{
							//Parent가 없다면 Root 본이다.
							meshGroup._boneList_Root.Add(bone);
						}
					}
				}

				int curBoneIndex = 0;
				for (int iRoot = 0; iRoot < meshGroup._boneList_Root.Count; iRoot++)
				{
					apBone rootBone = meshGroup._boneList_Root[iRoot];
					//TODO : MeshGroup이 Transform으로 있는 경우에 Transform Matrix를 넣어줘야한다.
					rootBone.LinkRecursive(0);
					curBoneIndex = rootBone.SetBoneIndex(curBoneIndex) + 1;
				}
			}

			//본 계층 / IK Chain도 다시 점검
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

			}

			//Render Unit도 체크해주자
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];
				//meshGroup.SetAllRenderUnitForceUpdate();
				meshGroup.RefreshForce();
				meshGroup.SortRenderUnits(true);
			}

			//Anim Clip 준비도 하자
			for (int i = 0; i < _animClips.Count; i++)
			{
				_animClips[i].LinkEditor(this);
				_animClips[i].RemoveUnlinkedTimeline();
			}


			//5. Modifier 세팅
			#region [미사용 코드] ReLinkModifierAndMeshGroups 이 함수로 대체하자
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];
			//	meshGroup._modifierStack.RefreshAndSort();

			//	List<apModifierBase> modifiers = meshGroup._modifierStack._modifiers;
			//	for (int iMod = 0; iMod < modifiers.Count; iMod++)
			//	{
			//		apModifierBase mod = modifiers[iMod];

			//		//추가 : Portrait를 연결해준다.
			//		mod.LinkPortrait(this);

			//		mod._meshGroup = GetMeshGroup(mod._meshGroupUniqueID);

			//		//삭제 조건1 - MeshGroup이 없다
			//		if (mod._meshGroup == null)
			//		{
			//			continue;
			//		}
			//		List<apModifierParamSetGroup> paramSetGroups = mod._paramSetGroup_controller;
			//		for (int iPSGroup = 0; iPSGroup < paramSetGroups.Count; iPSGroup++)
			//		{
			//			apModifierParamSetGroup paramSetGroup = paramSetGroups[iPSGroup];

			//			//List<apModifierParamSet> paramSets = mod._paramSetList;
			//			//1. Key를 세팅해주자
			//			switch (paramSetGroup._syncTarget)
			//			{
			//				case apModifierParamSetGroup.SYNC_TARGET.Static:
			//					break;

			//				case apModifierParamSetGroup.SYNC_TARGET.Controller:
			//					paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamName);
			//					break;

			//				case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
			//					Debug.LogError("TODO : KeyFrame 방식 연동");
			//					break;
			//			}


			//			List<apModifierParamSet> paramSets = paramSetGroup._paramSetList;

			//			for (int iParamSet = 0; iParamSet < paramSets.Count; iParamSet++)
			//			{
			//				apModifierParamSet paramSet = paramSets[iParamSet];

			//				//Link를 해주자
			//				paramSet.LinkParamSetGroup(paramSetGroup);



			//				List<apModifiedMesh> meshData = paramSet._meshData;
			//				apTransform_Mesh meshTransform = null;
			//				apTransform_MeshGroup meshGroupTransform = null;
			//				apRenderUnit renderUnit = null;
			//				for (int iMesh = 0; iMesh < meshData.Count; iMesh++)
			//				{
			//					apModifiedMesh modMesh = meshData[iMesh];

			//					modMesh._meshGroupUniqueID = meshGroup._uniqueID;

			//					switch (modMesh._targetType)
			//					{
			//						case apModifiedMesh.TARGET_TYPE.VertexMorph:
			//							{
			//								meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
			//								if (meshTransform != null)
			//								{
			//									renderUnit = meshGroup.GetRenderUnit(meshTransform);
			//									modMesh.Link_VertexMorph(meshGroup, meshTransform, renderUnit);
			//								}
			//							}
			//							break;

			//						case apModifiedMesh.TARGET_TYPE.MeshTransform:
			//							{
			//								meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
			//								if (meshTransform != null)
			//								{
			//									renderUnit = meshGroup.GetRenderUnit(meshTransform);
			//									modMesh.Link_MeshTransform(meshGroup, meshTransform, renderUnit);
			//								}
			//							}
			//							break;

			//						case apModifiedMesh.TARGET_TYPE.MeshGroupTransform:
			//							{
			//								meshGroupTransform = meshGroup.GetMeshGroupTransform(modMesh._transformUniqueID);
			//								if (meshGroupTransform != null)
			//								{
			//									renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
			//									modMesh.Link_MeshGroupTransform(meshGroup, meshGroupTransform, renderUnit);
			//								}
			//							}
			//							break;

			//						case apModifiedMesh.TARGET_TYPE.Bone:
			//							{
			//								//TODO : Bone 처리도 해주자
			//								modMesh.Link_Bone();
			//							}
			//							break;
			//					}

			//				}

			//				paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
			//				{
			//					return a._meshGroup == null;//<<연동이 안된..
			//				});

			//				List<apModifiedBone> boneData = paramSet._boneData;
			//				//TODO : 본 연동
			//			}
			//		}

			//		mod.RefreshParamSet();
			//	}

			//	meshGroup._modifierStack._modifiers.RemoveAll(delegate (apModifierBase a)
			//	{
			//		return a._meshGroup == null;
			//	});


			//	//ModStack의 CalculateParam을 모두 지우고 다시 만들자
			//	meshGroup._modifierStack.ClearAllCalculateParams();

			//	Debug.LogError("Portrait =>LinkModifierStackToRenderUnitCalculateStack");
			//	meshGroup._modifierStack.LinkModifierStackToRenderUnitCalculateStack();
			//} 
			#endregion
			LinkAndRefreshInEditor();

			// Main MeshGroup 연결
			// 수정) "다중" MainMeshGroup으로 변경

			if (_mainMeshGroupList == null)
			{ _mainMeshGroupList = new List<apMeshGroup>(); }
			else
			{ _mainMeshGroupList.Clear(); }

			if (_mainMeshGroupIDList == null)
			{
				_mainMeshGroupIDList = new List<int>();
			}


			for (int iMGID = 0; iMGID < _mainMeshGroupIDList.Count; iMGID++)
			{
				int mainMeshGroupID = _mainMeshGroupIDList[iMGID];
				bool isValidMeshGroupID = false;

				if (mainMeshGroupID >= 0)
				{
					apMeshGroup mainMeshGroup = GetMeshGroup(mainMeshGroupID);
					if (mainMeshGroup != null)
					{
						if (!_mainMeshGroupList.Contains(mainMeshGroup))
						{
							_mainMeshGroupList.Add(mainMeshGroup);
							isValidMeshGroupID = true;
						}
					}
				}
				if (!isValidMeshGroupID)
				{
					_mainMeshGroupIDList[iMGID] = -1;//<<이건 삭제하자
				}
			}

			//일단 유효하지 못한 ID는 삭제하자
			_mainMeshGroupIDList.RemoveAll(delegate (int a)
			{
				return a < 0;
			});

			//_mainMeshGroup = null;
			//if (_mainMeshGroupID >= 0)
			//{
			//	_mainMeshGroup = GetMeshGroup(_mainMeshGroupID);
			//	if (_mainMeshGroup == null)
			//	{
			//		_mainMeshGroupID = -1;
			//	}
			//}


			//이전 코드
			//_rootUnit._portrait = this;
			//_rootUnit.SetMeshGroup(_mainMeshGroup);

			//변경) 다중 RootUnit으로 바꾸자

			_rootUnits.Clear();

			for (int iMainMesh = 0; iMainMesh < _mainMeshGroupList.Count; iMainMesh++)
			{
				apMeshGroup meshGroup = _mainMeshGroupList[iMainMesh];

				apRootUnit newRootUnit = new apRootUnit();

				newRootUnit.SetPortrait(this);
				newRootUnit.SetMeshGroup(meshGroup);

				_rootUnits.Add(newRootUnit);
			}
		}


		/// <summary>
		/// Editor 상태에서
		/// MeshGroup을 참조하는 객체들 간의 레퍼런스를 연결하고 갱신한다.
		/// Editor 실행시와 객체 추가/삭제시 호출해주자
		/// </summary>
		public void LinkAndRefreshInEditor()
		{
			_controller.Ready(this);

			for (int iMesh = 0; iMesh < _meshes.Count; iMesh++)
			{
				_meshes[iMesh].LinkEdgeAndVertex();
			}

			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];
				meshGroup._modifierStack.RefreshAndSort(false);


				//Bone 연결 
				//Root 리스트는 일단 날리고 BoneAll 리스트를 돌면서 필요한걸 넣어주자
				//이후엔 Root -> Child 방식으로 순회
				meshGroup._boneList_Root.Clear();
				if (meshGroup._boneList_All != null)
				{
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						apBone bone = meshGroup._boneList_All[iBone];
						if (bone._childBones == null)
						{
							bone._childBones = new List<apBone>();
						}
						bone._childBones.Clear();
					}
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						apBone bone = meshGroup._boneList_All[iBone];

						apBone parentBone = null;
						if (bone._parentBoneID >= 0)
						{
							parentBone = meshGroup.GetBone(bone._parentBoneID);
						}

						bone.Link(meshGroup, parentBone);

						if (parentBone == null)
						{
							//Parent가 없다면 Root 본이다.
							meshGroup._boneList_Root.Add(bone);
						}
					}
				}

				int curBoneIndex = 0;
				for (int iRoot = 0; iRoot < meshGroup._boneList_Root.Count; iRoot++)
				{
					apBone rootBone = meshGroup._boneList_Root[iRoot];
					//TODO : MeshGroup이 Transform으로 있는 경우에 Transform Matrix를 넣어줘야한다.
					rootBone.LinkRecursive(0);
					curBoneIndex = rootBone.SetBoneIndex(curBoneIndex) + 1;
				}




				List<apModifierBase> modifiers = meshGroup._modifierStack._modifiers;
				for (int iMod = 0; iMod < modifiers.Count; iMod++)
				{
					apModifierBase mod = modifiers[iMod];

					//추가 : Portrait를 연결해준다.
					mod.LinkPortrait(this);



					mod._meshGroup = GetMeshGroup(mod._meshGroupUniqueID);

					//삭제 조건1 - MeshGroup이 없다
					if (mod._meshGroup == null)
					{
						continue;
					}
					List<apModifierParamSetGroup> paramSetGroups = mod._paramSetGroup_controller;
					for (int iPSGroup = 0; iPSGroup < paramSetGroups.Count; iPSGroup++)
					{
						apModifierParamSetGroup paramSetGroup = paramSetGroups[iPSGroup];

						//List<apModifierParamSet> paramSets = mod._paramSetList;
						//1. Key를 세팅해주자
						switch (paramSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.Static:
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								//paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamName);
								paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamID);
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								{
									//Debug.LogError("TODO : KeyFrame 방식 연동");
									//추가 : AnimClip과 연동을 먼저 한다.
									// ParamSetGroup -> AnimClip과 연동
									paramSetGroup._keyAnimClip = GetAnimClip(paramSetGroup._keyAnimClipID);
									if (paramSetGroup._keyAnimClip == null)
									{
										paramSetGroup._keyAnimClipID = -1;//<<삭제 하자
										break;
									}

									paramSetGroup._keyAnimTimeline = paramSetGroup._keyAnimClip.GetTimeline(paramSetGroup._keyAnimTimelineID);

									if (paramSetGroup._keyAnimTimeline == null)
									{
										paramSetGroup._keyAnimTimelineID = -1;
										break;
									}

									paramSetGroup._keyAnimTimelineLayer = paramSetGroup._keyAnimTimeline.GetTimelineLayer(paramSetGroup._keyAnimTimelineLayerID);

									if (paramSetGroup._keyAnimTimelineLayer == null)
									{
										paramSetGroup._keyAnimTimelineLayerID = -1;
										break;
									}

									//추가) 상호 연동을 해주자
									paramSetGroup._keyAnimTimelineLayer.LinkParamSetGroup(paramSetGroup);

									//키프레임이면 여기서 한번더 링크를 해주자
									for (int iPS = 0; iPS < paramSetGroup._paramSetList.Count; iPS++)
									{
										apModifierParamSet paramSet = paramSetGroup._paramSetList[iPS];
										int keyframeID = paramSet._keyframeUniqueID;

										apAnimKeyframe targetKeyframe = paramSetGroup._keyAnimTimelineLayer.GetKeyframeByID(keyframeID);
										if (targetKeyframe != null)
										{
											paramSet.LinkSyncKeyframe(targetKeyframe);
										}
										else
										{
											//Debug.LogError("Keyframe 연동 에러 [" + keyframeID + "]");
											paramSet._keyframeUniqueID = -1;//못찾았다.
										}

									}
									int nPrevParamSet = paramSetGroup._paramSetList.Count;
									//"키프레임 연동" 방식에서 비어있는 키프레임이라면?
									int nRemoved = paramSetGroup._paramSetList.RemoveAll(delegate (apModifierParamSet a)
									{
										return a._keyframeUniqueID < 0;
									});
									if (nRemoved > 0)
									{
										Debug.LogError(nPrevParamSet + "개 중 " + nRemoved + "개의 Keyframe 과 연동되지 못한 ParamSet 삭제");
									}



									//추가
								}
								break;
						}


						List<apModifierParamSet> paramSets = paramSetGroup._paramSetList;

						for (int iParamSet = 0; iParamSet < paramSets.Count; iParamSet++)
						{
							apModifierParamSet paramSet = paramSets[iParamSet];

							//Link를 해주자
							paramSet.LinkParamSetGroup(paramSetGroup);



							List<apModifiedMesh> meshData = paramSet._meshData;
							apTransform_Mesh meshTransform = null;
							apTransform_MeshGroup meshGroupTransform = null;
							apRenderUnit renderUnit = null;

							//1. ModMesh
							for (int iMesh = 0; iMesh < meshData.Count; iMesh++)
							{
								apModifiedMesh modMesh = meshData[iMesh];

								//추가 : Modifier의 meshGroup과 Transform의 MeshGroup을 분리한다.
								apMeshGroup meshGroupOfTransform = null;
								if (modMesh._isRecursiveChildTransform)
								{
									//MeshGroup이 다르다
									//Debug.Log("Link RecursiveChildTransform : " + modMesh._meshGroupUniqueID_Transform);

									meshGroupOfTransform = GetMeshGroup(modMesh._meshGroupUniqueID_Transform);
									if (meshGroupOfTransform == null)
									{
										Debug.LogError("Recursive Child Transfrom Missing");
									}
								}
								else
								{
									//동일한 MeshGroup이다.
									meshGroupOfTransform = meshGroup;
								}

								modMesh._meshGroupUniqueID_Modifier = meshGroup._uniqueID;

								//변경 : 타입 대신 값을 보고 판단한다.
								if (modMesh._transformUniqueID >= 0 && meshGroupOfTransform != null)
								{
									if (modMesh._isMeshTransform)
									{
										meshTransform = meshGroupOfTransform.GetMeshTransform(modMesh._transformUniqueID);
										if (meshTransform != null)
										{
											renderUnit = meshGroup.GetRenderUnit(meshTransform);
											modMesh.Link_MeshTransform(meshGroup, meshGroupOfTransform, meshTransform, renderUnit, this);
										}
										else
										{
											Debug.LogError("No MeshTransform In MeshGroup / MeshGroup : " + meshGroupOfTransform._name + " / Transform ID : " + modMesh._transformUniqueID);
										}
									}
									else
									{
										meshGroupTransform = meshGroupOfTransform.GetMeshGroupTransform(modMesh._transformUniqueID);
										if (meshGroupTransform != null)
										{
											renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
											modMesh.Link_MeshGroupTransform(meshGroup, meshGroupOfTransform, meshGroupTransform, renderUnit);
										}
										else
										{
											Debug.LogError("No MeshGroupTransform In MeshGroup / MeshGroup : " + meshGroupOfTransform._name + " / Transform ID : " + modMesh._transformUniqueID);
										}
									}
								}
								else
								{
									Debug.LogError("Unknown ModMesh / Transform ID : " + modMesh._transformUniqueID + " / Is MeshGroupOfTransform Is Null : " + (meshGroupOfTransform == null));
								}



								#region [미사용 코드]
								//switch (modMesh._targetType)
								//{
								//	case apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform:
								//		{
								//			meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
								//			if (meshTransform != null)
								//			{
								//				renderUnit = meshGroup.GetRenderUnit(meshTransform);
								//				modMesh.Link_VertexMorph(meshGroup, meshTransform, renderUnit);
								//			}
								//		}
								//		break;

								//	case apModifiedMesh.TARGET_TYPE.MeshTransformOnly:
								//		{
								//			meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
								//			if (meshTransform != null)
								//			{
								//				renderUnit = meshGroup.GetRenderUnit(meshTransform);
								//				modMesh.Link_MeshTransform(meshGroup, meshTransform, renderUnit);
								//			}
								//		}
								//		break;

								//	case apModifiedMesh.TARGET_TYPE.MeshGroupTransformOnly:
								//		{
								//			meshGroupTransform = meshGroup.GetMeshGroupTransform(modMesh._transformUniqueID);
								//			if (meshGroupTransform != null)
								//			{
								//				renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
								//				modMesh.Link_MeshGroupTransform(meshGroup, meshGroupTransform, renderUnit);
								//			}
								//		}
								//		break;

								//	case apModifiedMesh.TARGET_TYPE.Bone:
								//		{
								//			//TODO : Bone 처리도 해주자
								//			modMesh.Link_Bone();
								//		}
								//		break;
								//} 
								#endregion

							}

							//연동이 안된 MeshData는 삭제한다.
							//디버그 코드
							//---------------------------------------------------------------------------------
							for (int iCheckMM = 0; iCheckMM < paramSet._meshData.Count; iCheckMM++)
							{
								if (paramSet._meshData[iCheckMM]._meshGroupOfModifier == null ||
									paramSet._meshData[iCheckMM]._meshGroupOfTransform == null)
								{
									int nModMesh = paramSet._meshData.Count;
									bool isMesh = paramSet._meshData[iCheckMM]._isMeshTransform;
									string animTimelineName = "[No AnimTimelineLayer]";

									string transformName = "[No Transform]";
									if (paramSet._meshData[iCheckMM]._transform_Mesh != null)
									{
										transformName = paramSet._meshData[iCheckMM]._transform_Mesh._nickName;
									}
									else if (paramSet._meshData[iCheckMM]._transform_MeshGroup != null)
									{
										transformName = paramSet._meshData[iCheckMM]._transform_MeshGroup._nickName;
									}

									if (paramSetGroup._keyAnimTimelineLayer != null)
									{
										animTimelineName = paramSetGroup._keyAnimTimelineLayer.DisplayName;
									}

									string strNullComp = "";
									if (paramSet._meshData[iCheckMM]._meshGroupOfModifier == null)
									{
										strNullComp += "MeshGroupOfModifier is Null / ";
									}
									if (paramSet._meshData[iCheckMM]._meshGroupOfTransform == null)
									{
										strNullComp += "MeshGroupOfTransform is Null / ";
									}
									Debug.LogError("Find Removable (" + nModMesh + ") / Is Mesh : " + isMesh + " / " + animTimelineName + " / " + transformName + " / Null : " + strNullComp);
								}
							}
							//---------------------------------------------------------------------------------


							int nRemoveModMesh = paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
							{
								return a._meshGroupOfModifier == null || a._meshGroupOfTransform == null;
							});

							if (nRemoveModMesh > 0)
							{
								Debug.LogError("Remove unlinked ModMesh (LinkAndRefreshInEditor) : " + nRemoveModMesh);
							}


							//---------------------------------------------------------------------------------
							//2. Bone 연동을 하자
							List<apModifiedBone> boneData = paramSet._boneData;
							apModifiedBone modBone = null;

							for (int iModBone = 0; iModBone < boneData.Count; iModBone++)
							{
								modBone = boneData[iModBone];
								apMeshGroup meshGroupOfBone = GetMeshGroup(modBone._meshGropuUniqueID_Bone);
								apMeshGroup meshGroupOfModifier = GetMeshGroup(modBone._meshGroupUniqueID_Modifier);
								if (meshGroupOfBone == null || meshGroupOfModifier == null)
								{
									//Debug.LogError("Link Error : Mod Bone 링크 실패 [MeshGroup]");
									continue;
								}

								apBone bone = meshGroupOfBone.GetBone(modBone._boneID);
								if (bone == null)
								{
									//Debug.LogError("Link Error : Mod Bone 링크 실패");
									continue;
								}

								meshGroupTransform = meshGroupOfModifier.GetMeshGroupTransformRecursive(modBone._transformUniqueID);
								//meshGroupTransform = meshGroupOfBone.GetMeshGroupTransform();
								if (meshGroupTransform == null)
								{
									//Debug.LogError("Link Error : Mod Bone 링크 실패 [MeshGroup Transform]");
									continue;
								}

								//renderUnit = meshGroupOfBone.GetRenderUnit(meshGroupTransform);
								renderUnit = meshGroupOfModifier.GetRenderUnit(meshGroupTransform._transformUniqueID, false);
								if (renderUnit == null)
								{
									//Debug.LogError("Link Error : Mod Bone 링크 실패 [Render Unit]");
									//continue;
								}

								modBone.Link(meshGroupOfModifier, meshGroupOfBone, bone, renderUnit, meshGroupTransform);
							}

							//연동 안된 ModBone은 삭제하자
							//---------------------------------------------------------------------------------
							int nRemovedModBone = boneData.RemoveAll(delegate (apModifiedBone a)
							{
								return a._bone == null || a._meshGroup_Bone == null || a._meshGroup_Modifier == null;
							});

							//if(nRemovedModBone > 0)
							//{

							//}
						}
					}

					mod.RefreshParamSet();

				}

				meshGroup._modifierStack._modifiers.RemoveAll(delegate (apModifierBase a)
				{
					return a._meshGroup == null;
				});


				//ModStack의 CalculateParam을 모두 지우고 다시 만들자
				meshGroup.RefreshModifierLink();
			}

			for (int i = 0; i < _animClips.Count; i++)
			{
				_animClips[i].LinkEditor(this);
				_animClips[i].RemoveUnlinkedTimeline();
			}

			System.GC.Collect();
		}

		// Bake
		//----------------------------------------------------------------



		// 참조용 리스트 관리
		//----------------------------------------------------------------



		// ID 관리
		//----------------------------------------------------------------
		//유니크 아이디는 몇가지 타입에 맞게 통합해서 관리한다.
		public void ClearRegisteredUniqueIDs()
		{
			//_registeredUniqueIDs_Texture.Clear();
			//_registeredUniqueIDs_Vert.Clear();
			//_registeredUniqueIDs_Mesh.Clear();
			//_registeredUniqueIDs_MeshGroup.Clear();
			//_registeredUniqueIDs_Transform.Clear();
			//_registeredUniqueIDs_Modifier.Clear();
			//_registeredUniqueIDs_ControlParam.Clear();
			//_registeredUniqueIDs_AnimClip.Clear();

			_IDManager.Clear();
		}

		// 발급된 ID는 관리를 위해 회수한다.
		public void RegistUniqueID(apIDManager.TARGET target, int ID)
		{
			_IDManager.RegistID(target, ID);
		}
		#region [미사용 코드]
		//public void RegistUniqueID_Texture(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Texture.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Texture.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Vertex(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Vert.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Vert.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Mesh(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Mesh.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Mesh.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_MeshGroup(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_MeshGroup.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_MeshGroup.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Transform(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Transform.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Transform.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Moifier(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Modifier.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Modifier.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_ControlParam(int uniqueID)
		//{
		//	if(!_registeredUniqueIDs_ControlParam.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_ControlParam.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_AnimClip(int uniqueID)
		//{
		//	if(!_registeredUniqueIDs_AnimClip.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_AnimClip.Add(uniqueID);
		//	}
		//} 
		#endregion



		// 새로운 ID를 발급한다.
		public int MakeUniqueID(apIDManager.TARGET target)
		{
			return _IDManager.MakeUniqueID(target);
		}
		#region [미사용 코드]
		//private int MakeUniqueID(List<int> IDList)
		//{
		//	int nextID = -1;
		//	int cntCheck = 0;
		//	while(true)
		//	{
		//		nextID = UnityEngine.Random.Range(1000, 99999999);
		//		if(!IDList.Contains(nextID))
		//		{
		//			IDList.Add(nextID);
		//			return nextID;
		//		}

		//		cntCheck++;
		//		//회수 제한에 걸렸다.
		//		if(cntCheck > 100)
		//		{
		//			break;
		//		}
		//	}

		//	for (int i = 1; i < 99999999; i++)
		//	{
		//		if(!IDList.Contains(i))
		//		{
		//			IDList.Add(i);
		//			return i;
		//		}
		//	}
		//	return -1;//<< 실패
		//}
		//public int MakeUniqueID_Texture()		{ return MakeUniqueID(_registeredUniqueIDs_Texture); }
		//public int MakeUniqueID_Vertex()		{ return MakeUniqueID(_registeredUniqueIDs_Vert); }
		//public int MakeUniqueID_Mesh()			{ return MakeUniqueID(_registeredUniqueIDs_Mesh); }
		//public int MakeUniqueID_MeshGroup()		{ return MakeUniqueID(_registeredUniqueIDs_MeshGroup); }
		//public int MakeUniqueID_Transform()		{ return MakeUniqueID(_registeredUniqueIDs_Transform); }
		//public int MakeUniqueID_Modifier()		{ return MakeUniqueID(_registeredUniqueIDs_Modifier); }
		//public int MakeUniqueID_ControlParam()	{ return MakeUniqueID(_registeredUniqueIDs_ControlParam); }
		//public int MakeUniqueID_AnimClip()		{ return MakeUniqueID(_registeredUniqueIDs_AnimClip); } 
		#endregion


		// 객체 삭제시 ID 회수
		public void PushUnusedID(apIDManager.TARGET target, int unusedID)
		{
			_IDManager.PushUnusedID(target, unusedID);
		}
		#region [미사용 코드]
		//public void PushUniqueID_Texture(int uniquedID)			{ _registeredUniqueIDs_Texture.Remove(uniquedID); }
		//public void PushUniqueID_Vertex(int uniquedID)			{ _registeredUniqueIDs_Vert.Remove(uniquedID); }
		//public void PushUniqueID_Mesh(int uniquedID)			{ _registeredUniqueIDs_Mesh.Remove(uniquedID); }
		//public void PushUniqueID_MeshGroup(int uniquedID)		{ _registeredUniqueIDs_MeshGroup.Remove(uniquedID); }
		//public void PushUniqueID_Transform(int uniquedID)		{ _registeredUniqueIDs_Transform.Remove(uniquedID); }
		//public void PushUniqueID_Modifier(int uniquedID)		{ _registeredUniqueIDs_Modifier.Remove(uniquedID); }
		//public void PushUniqueID_ControlParam(int uniquedID)	{ _registeredUniqueIDs_ControlParam.Remove(uniquedID); }
		//public void PushUniqueID_AnimClip(int uniquedID)		{ _registeredUniqueIDs_AnimClip.Remove(uniquedID); } 
		#endregion


		// ID로 오브젝트 참조
		//-------------------------------------------------------------------------------------------------------
		public apTextureData GetTexture(int uniqueID)
		{
			return _textureData.Find(delegate (apTextureData a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public apMesh GetMesh(int uniqueID)
		{
			return _meshes.Find(delegate (apMesh a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public apMeshGroup GetMeshGroup(int uniqueID)
		{
			return _meshGroups.Find(delegate (apMeshGroup a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public apControlParam GetControlParam(int uniqueID)
		{
			return _controller._controlParams.Find(delegate (apControlParam a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public apControlParam GetControlParam(string controlParamName)
		{
			return _controller._controlParams.Find(delegate (apControlParam a)
			{
				return string.Equals(a._keyName, controlParamName);
			});
		}

		public apAnimClip GetAnimClip(int uniqueID)
		{
			return _animClips.Find(delegate (apAnimClip a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		// ID로 오브젝트 참조 - RealTime
		//-------------------------------------------------------------------------------------------------------
		public apOptTransform GetOptTransform(int transformID)
		{
			if (transformID < -1)
			{
				return null;
			}

			if (_optTransforms == null)
			{
				return null;
			}
			return _optTransforms.Find(delegate (apOptTransform a)
			{
				return a._transformID == transformID;
			});
		}

		public apOptTransform GetOptTransformAsMeshGroup(int meshGroupUniqueID)
		{
			//Debug.Log("GetOptTransformAsMeshGroup [" + meshGroupUniqueID + "]");
			if (meshGroupUniqueID < 0)
			{
				Debug.LogError("ID < 0");
				return null;
			}
			if (_optTransforms == null)
			{
				Debug.LogError("OptTranforms is Null");
				return null;
			}

			//for (int i = 0; i < _optTransforms.Count; i++)
			//{
			//	Debug.Log("[" + i + "] : " + _optTransforms[i]._transformID + " / " + _optTransforms[i]._meshGroupUniqueID);
			//}

			return _optTransforms.Find(delegate (apOptTransform a)
			{
				return a._meshGroupUniqueID == meshGroupUniqueID;
			});
		}



	}

}
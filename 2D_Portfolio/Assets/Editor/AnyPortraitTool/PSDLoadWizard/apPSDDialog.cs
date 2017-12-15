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
using UnityEditor;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using Ntreev.Library.Psd;
using System.Threading;

using AnyPortrait;

namespace AnyPortrait
{

	public class apPSDDialog : EditorWindow
	{
		// Menu
		//----------------------------------------------------------
		private static apPSDDialog s_window = null;



		public static object ShowWindow(apEditor editor, FUNC_PSD_LOAD_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return null;
			}
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apPSDDialog), false, "PSD Load");
			apPSDDialog curTool = curWindow as apPSDDialog;
			if (curTool != null && curTool != s_window)
			{

				int width = 1000;
				int height = 700;

				object loadKey = new object();

				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, funcResult, loadKey);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		// Members
		//----------------------------------------------------------
		private apEditor _editor = null;
		private object _loadKey = null;
		private FUNC_PSD_LOAD_RESULT _funcResult = null;
		public delegate void FUNC_PSD_LOAD_RESULT(bool isSuccess, object loadKey, string fileName, List<apPSDLayerData> layerDataList, float scaleRatio, int totalWidth, int totalHeight, int padding);//<<나중에 처리 결과에 따라서 더 넣어주자



		public enum LOAD_STEP
		{
			Step1_FileLoad,
			Step2_LayerCheck,
			Step3_AtlasSetting,
		}

		private LOAD_STEP _step = LOAD_STEP.Step1_FileLoad;


		//파일 정보
		private bool _isFileLoaded = false;
		private string _fileFullPath = "";
		private string _fileNameOnly = "";

		private int _imageWidth = -1;
		private int _imageHeight = -1;
		private Vector2 _imageCenterPosOffset = Vector2.zero;


		//레이어 리스트
		private List<apPSDLayerData> _layerDataList = new List<apPSDLayerData>();
		private apPSDLayerData _selectedLayerData = null;


		//Bake 정보
		//Bake 할때
		// 이미지 크기 + 이미지 개수 + Padding을 지정한다.
		// 리사이즈가 안되면 -> 이미지 개수가 부족할때 에러 (크기를 늘리거나 이미지 개수를 늘려야한다.)
		// 리사이즈가 되면 -> 자동으로 비율을 조절한다. 단, 더 늘리진 않음
		//private bool _isBakeResizable = false;//<<크기가 안맞으면 자동으로 리사이즈를 할 것인가 (이건 넓이 비교로 리사이즈를 하자)
		private BAKE_SIZE _bakeWidth = BAKE_SIZE.s1024;
		private BAKE_SIZE _bakeHeight = BAKE_SIZE.s1024;
		private string _bakeDstFilePath = "";//저장될 기본 경로 (폴더만 지정한다. 나머지는 파일 + 이미지 번호)
		private string _bakeDstFileRelativePath = "";
		private int _bakeMaximumNumAtlas = 2;
		private int _bakePadding = 4;
		private bool _isBakeWarning = false;
		private string _bakeWarningMsg = "";

		private bool _bakeBlurOption = true;
		//private int _bakeResizeRatioX100 = 100;





		[Serializable]
		public enum BAKE_SIZE
		{
			s256,
			s512,
			s1024,
			s2048,
			s4096
		}
		public string[] _bakeDescription = new string[] { "256", "512", "1024", "2048", "4096" };

		//Bake 리스트
		private List<apPSDBakeData> _bakeDataList = new List<apPSDBakeData>();
		private apPSDBakeData _selectedBakeData = null;

		//Bake Param
		//Bake 전에 어떻게 배치할 지 결정하는 파라미터
		//LayerData + PosOffset으로 구성되어 있다.
		//일단 Scale 상관없이 위치만 계산한다.
		private class LayerBakeParam
		{
			public apPSDLayerData _targetLayer = null;
			public int _atlasIndex = 0;
			public int _posOffset_X = 0;
			public int _posOffset_Y = 0;

			public LayerBakeParam(apPSDLayerData targetLayer,
									int atlasIndex,
									int posOffset_X,
									int posOffset_Y
									)
			{
				_targetLayer = targetLayer;
				_atlasIndex = atlasIndex;
				_posOffset_X = posOffset_X;
				_posOffset_Y = posOffset_Y;
			}
		}

		//Bake 처리 중에 사용되는 변수 => 이건 Bake 전에 결정된다.
		private bool _isNeedBakeCheck = true;
		//private int _needBakeResizeX100 = 100;
		private List<LayerBakeParam> _bakeParams = new List<LayerBakeParam>();
		private int _realBakeSizePerIndex = 0;
		private int _realBakedAtlasCount = 0;//실제로 Bake된 Atlas
		private int _realBakeResizeX100 = 100;
		private object _loadKey_CheckBake = null;//<<체크가 끝났을때의 키
		private object _loadKey_Bake = null;//Bake가 끝났을 때의 키


		private int _resultAtlasCount = 0;
		private int _resultBakeResizeX100 = 0;
		private int _resultPadding = 0;


		// GUI
		public int _iZoomX100 = 11;//11 => 100
		public const int ZOOM_INDEX_DEFAULT = 11;
		public int[] _zoomListX100 = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 85, 90, 95, 100/*(11)*/, 105, 110, 120, 140, 160, 180, 200, 250, 300, 350, 400, 450, 500 };
		private Vector2 _scroll_MainCenter = Vector2.zero;

		private apPSDMouse _mouse = new apPSDMouse();
		private apPSDGL _gl = new apPSDGL();

		private Color _glBackGroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

		private const int PSD_IMAGE_FILE_MAX_SIZE = 5000;


		//중요!
		//스레드로 처리해야할 것들
		//public Thread _thread = null;
		//private bool _isSaveImageThreadStart = false;
		//private bool _isBakeThreadStart = false;
		//private enum THREAD_WORK_TYPE
		//{
		//	None,
		//	Calculate,
		//	BakeImage,
		//	SaveImageToEditor
		//}
		//private THREAD_WORK_TYPE _threadWorkType = THREAD_WORK_TYPE.None;


		//private bool _isThreadProcess = false;
		//private bool _isThreadProcessSuccess = false;
		//private int _threadProcessX100 = 0;
		private string _threadProcessName = "";

		private WorkProcess _workProcess = new WorkProcess();
		private bool _isImageBaking = false;


		//Thread - Bake
		//private int _iRequestSaveBakeDataList = -1;
		//private bool _isRequestSaveBakeDataList = false;
		//private bool _isRequestSaveBakeDataListComplete = false;

		//private int _iRequestReimportImage = -1;
		//private bool _isRequestReimportImage = false;
		//private bool _isRequestReimportImageComplete = false;


		private bool IsGUIUsable { get { return (!_workProcess.IsRunning); } }
		private bool IsProcessRunning { get { return _workProcess.IsRunning; } }

		/// <summary>
		/// 스레드를 모방한 비동기 프로세스
		/// </summary>
		public class WorkProcess
		{
			public class WorkProcessUnit
			{
				public delegate bool FUNC_PROCESS_UNIT(int index);
				private FUNC_PROCESS_UNIT _funcUnit = null;
				public int _count = -1;

				public WorkProcessUnit(int count)
				{
					_count = count;
				}

				public void AddProcess(FUNC_PROCESS_UNIT funcProcess)
				{
					_funcUnit = funcProcess;
				}

				public bool Run(int index)
				{
					if (_funcUnit == null)
					{
						return false;
					}
					return _funcUnit(index);
				}
				public void ChangeCount(int count)
				{
					_count = count;
				}
			}
			private List<WorkProcessUnit> _units = new List<WorkProcessUnit>();
			private int _totalProcessCount = 0;
			private int _curProcessX100 = 0;

			private bool _isRunning = false;
			private bool _isSuccess = false;

			private int _iCurUnit = -1;
			private int _iSubProcess = -1;
			private int _iTotalProcess = -1;
			private string _strProcessLabel;


			public bool IsRunning { get { return _isRunning; } }
			public bool IsSuccess { get { return !_isRunning && _isSuccess; } }
			public int ProcessX100 { get { return _curProcessX100; } }


			public WorkProcess()
			{
				Clear();
			}

			public void Clear()
			{
				_units.Clear();

				_totalProcessCount = 0;

				_isRunning = false;
				_isSuccess = false;

				_iCurUnit = -1;
				_iSubProcess = -1;
				_iTotalProcess = 0;
				_curProcessX100 = 0;
			}

			public void Add(WorkProcessUnit.FUNC_PROCESS_UNIT funcProcess, int count)
			{
				WorkProcessUnit newUnit = new WorkProcessUnit(count);
				newUnit.AddProcess(funcProcess);
				_units.Add(newUnit);

				_totalProcessCount += count;//전체 카운트를 높인다. (나중에 퍼센트 체크를 위함)
			}

			public void ChangeCount(int workIndex, int count)
			{
				if (workIndex < 0 || workIndex >= _units.Count)
				{
					return;
				}
				_units[workIndex].ChangeCount(count);

				_totalProcessCount = 0;
				//전체 카운트 갱신
				for (int i = 0; i < _units.Count; i++)
				{
					_totalProcessCount += _units[i]._count;
				}
			}

			public void StartRun(string strProcessLabel)
			{
				_curProcessX100 = 0;

				_isRunning = true;
				_isSuccess = false;

				_iCurUnit = 0;
				_iSubProcess = 0;
				_iTotalProcess = 0;
				_strProcessLabel = strProcessLabel;
			}

			public void Run()
			{
				if (!_isRunning)
				{
					return;
				}
				if (_iCurUnit >= _units.Count)
				{
					//끝. 성공!
					_isRunning = false;
					_isSuccess = true;
					_curProcessX100 = 100;

					//Debug.Log("Process Success : " + _strProcessLabel);
					return;
				}
				WorkProcessUnit curUnit = _units[_iCurUnit];



				//실행하고 퍼센트를 높이자
				if (!curUnit.Run(_iSubProcess))
				{
					//실패 했네염..
					_isRunning = false;
					_isSuccess = false;
					_curProcessX100 = 0;
					Debug.LogError("Process Failed : " + _strProcessLabel + " : " + _iCurUnit + " / " + _iSubProcess);
					return;
				}

				_iTotalProcess++;
				_iSubProcess++;

				if (_iSubProcess >= curUnit._count)
				{
					_iSubProcess = 0;
					_iCurUnit++;
				}

				_curProcessX100 = (int)Mathf.Clamp((((float)_iTotalProcess * 100.0f) / (float)_totalProcessCount), 0, 100);
			}



		}

		// Init
		//----------------------------------------------------------
		private void Init(apEditor editor, FUNC_PSD_LOAD_RESULT funcResult, object loadKey)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_step = LOAD_STEP.Step1_FileLoad;

			Shader[] shaderSet_Normal = new Shader[4];
			Shader[] shaderSet_VertAdd = new Shader[4];
			Shader[] shaderSet_Mask = new Shader[4];
			Shader[] shaderSet_Clipped = new Shader[4];
			for (int i = 0; i < 4; i++)
			{
				shaderSet_Normal[i] = editor._mat_Texture_Normal[i].shader;
				shaderSet_VertAdd[i] = editor._mat_Texture_VertAdd[i].shader;
				shaderSet_Mask[i] = editor._mat_MaskedTexture[i].shader;
				shaderSet_Clipped[i] = editor._mat_Clipped[i].shader;
			}

			//_gl.SetMaterial(editor._mat_Color, editor._mat_Texture, editor._mat_MaskedTexture);
			_gl.SetShader(editor._mat_Color.shader,
							shaderSet_Normal,
							shaderSet_VertAdd,
							shaderSet_Mask,
							editor._mat_MaskOnly.shader,
							shaderSet_Clipped);

			wantsMouseMove = true;

			//_isThreadProcess = false;
			_workProcess.Clear();
		}

		public static void CloseDialog()
		{
			if (s_window != null)
			{
				s_window.CloseThread();

				try
				{
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);

				}

				s_window = null;
			}
		}



		public void CloseThread()
		{
			try
			{
				//if (_thread != null)
				//{
				//	_thread.Abort();
				//}
				_workProcess.Clear();
			}
			catch (Exception ex)
			{
				Debug.LogError("CloseThread Exception : " + ex);
			}

			//_thread = null;
			//_threadWorkType = THREAD_WORK_TYPE.None;
			//_threadProcessX100 = 0;
			//_isThreadProcess = false;
			_isImageBaking = false;
		}

		void OnDestroy()
		{
			//Debug.Log("PSD Dialog Destroy");
			CloseThread();
		}

		// Update
		//----------------------------------------------------------
		void Update()
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}

			Repaint();


			if (_workProcess.IsRunning)
			{
				_workProcess.Run();
				if (!_workProcess.IsRunning)
				{
					_isImageBaking = false;
				}
			}

			//switch (_threadWorkType)
			//{
			//	case THREAD_WORK_TYPE.None:
			//		//??
			//		break;

			//	case THREAD_WORK_TYPE.Calculate:
			//		break;

			//	case THREAD_WORK_TYPE.BakeImage:
			//		break;

			//	case THREAD_WORK_TYPE.SaveImageToEditor:
			//		{
			//			if (_isThreadProcess)
			//			{
			//				//아직 Thread 작동 중
			//				if (_isRequestSaveBakeDataList)
			//				{
			//					//Debug.Log("Save : " + _iRequestSaveBakeDataList);
			//					SaveBakeImage(_iRequestSaveBakeDataList);
			//					_iRequestSaveBakeDataList = -1;
			//					_isRequestSaveBakeDataList = false;
			//					_isRequestSaveBakeDataListComplete = true;
			//				}

			//				if (_isRequestReimportImage)
			//				{
			//					//Debug.Log("Reimport : " + _iRequestReimportImage);
			//					ReimportBakedImage(_iRequestReimportImage);
			//					_iRequestReimportImage = -1;
			//					_isRequestReimportImage = false;
			//					_isRequestReimportImageComplete = true;
			//				}
			//			}
			//			else
			//			{
			//				//_isSaveImageThreadStart = false;
			//				_threadWorkType = THREAD_WORK_TYPE.None;
			//				CloseThread();
			//				AssetDatabase.Refresh();

			//				if (_isThreadProcessSuccess)
			//				{
			//					//Debug.Log("성공!");
			//					OnLoadComplete(true);
			//				}
			//				else
			//				{
			//					EditorUtility.DisplayDialog("Save Failed", "Save Images Failed", "Okay");
			//					CloseThread();

			//					_isThreadProcessSuccess = false;
			//				}
			//			}
			//		}
			//		break;
			//}

		}


		private bool IsMouseInGUI(Vector2 mousePos, Rect mainGUIRect)
		{
			if (mousePos.x < 0 || mousePos.x > mainGUIRect.width
				|| mousePos.y < 0 || mousePos.y > mainGUIRect.height)
			{
				return false;
			}
			return true;
		}

		private void MouseUpdate(Rect mainGUIRect)
		{
			bool isMouseEvent = Event.current.rawType == EventType.ScrollWheel ||
				Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseDrag ||
				Event.current.rawType == EventType.MouseMove ||
				Event.current.rawType == EventType.MouseUp;

			if (!isMouseEvent)
			{
				return;
			}

			Vector2 mousePos = Event.current.mousePosition - new Vector2(mainGUIRect.x, mainGUIRect.y);
			_mouse.SetMousePos(mousePos, Event.current.mousePosition);
			_mouse.ReadyToUpdate();

			if (Event.current.rawType == EventType.ScrollWheel)
			{
				Vector2 deltaValue = Event.current.delta;
				_mouse.Update_Wheel((int)(deltaValue.y * 10.0f));

			}
			else
			{
				int iMouse = -1;
				switch (Event.current.button)
				{
					case 0://Left
						iMouse = 0;
						break;

					case 1://Right
						iMouse = 1;
						break;

					case 2://Middle
						iMouse = 2;
						break;
				}


				if (iMouse >= 0)
				{
					_mouse.SetMouseBtn(iMouse);

					//GUI 기준 상대 좌표
					switch (Event.current.rawType)
					{
						case EventType.MouseDown:
							{
								if (IsMouseInGUI(mousePos, mainGUIRect))
								{
									//Editor._mouseBtn[iMouse].Update_Pressed(mousePos);
									_mouse.Update_Pressed();
								}
							}
							break;

						case EventType.MouseUp:
							{
								//Editor._mouseBtn[iMouse].Update_Released(mousePos);
								_mouse.Update_Released();
							}
							break;

						case EventType.MouseMove:
						case EventType.MouseDrag:
							{
								//Editor._mouseBtn[iMouse].Update_Moved(deltaValue);
								_mouse.Update_Moved();

							}
							break;

							//case EventType.ScrollWheel:
							//	{

							//	}
							//break;
					}

					_mouse.EndUpdate();
				}
			}
		}

		private bool MouseScrollUpdate(Rect mainGUIRect)
		{
			if (_mouse.Wheel != 0)
			{
				//if(IsMouseInGUI(Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosLast))
				if (IsMouseInGUI(_mouse.PosLast, mainGUIRect))
				{
					if (_mouse.Wheel > 0)
					{
						//줌 아웃 = 인덱스 감소
						_iZoomX100--;
						if (_iZoomX100 < 0)
						{ _iZoomX100 = 0; }
					}
					else if (_mouse.Wheel < 0)
					{
						//줌 인 = 인덱스 증가
						_iZoomX100++;
						if (_iZoomX100 >= _zoomListX100.Length)
						{
							_iZoomX100 = _zoomListX100.Length - 1;
						}
					}

					//Editor.Repaint();
					//SetRepaint();
					//Debug.Log("Zoom [" + _zoomListX100[_iZoomX100] + "]");

					_mouse.UseWheel();
					return true;
				}
			}

			if (_mouse.ButtonIndex == 2)
			{
				if (_mouse.Status == apPSDMouse.MouseBtnStatus.Down ||
					_mouse.Status == apPSDMouse.MouseBtnStatus.Pressed)
				{
					//if (IsMouseInGUI(Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosLast))
					if (IsMouseInGUI(_mouse.PosLast, mainGUIRect))
					{
						Vector2 moveDelta = _mouse.PosDelta;
						//RealX = scroll * windowWidth * 0.1

						Vector2 sensative = new Vector2(
							1.0f / (mainGUIRect.width * 0.1f),
							1.0f / (mainGUIRect.height * 0.1f));

						_scroll_MainCenter.x -= moveDelta.x * sensative.x;
						_scroll_MainCenter.y -= moveDelta.y * sensative.y;


						_mouse.UseMouseDrag();
						return true;
					}
				}
			}
			return false;
		}


		// GUI
		//----------------------------------------------------------
		void OnGUI()
		{
			try
			{
				if (_editor == null || _editor._portrait == null)
				{
					CloseDialog();
					return;
				}



				int windowWidth = (int)position.width;
				int windowHeight = (int)position.height;

				int topHeight = 28;
				int bottomHeight = 46;
				int margin = 4;

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(windowHeight));

				int centerHeight = windowHeight - (topHeight + bottomHeight + margin * 2);

				// Top UI : Step을 차례로 보여준다.
				//-----------------------------------------------
				GUI.Box(new Rect(0, 0, windowWidth, topHeight), "");

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(topHeight));

				GUI_Top(windowWidth, topHeight - 6);

				EditorGUILayout.EndVertical();
				// Center UI : Step별 설정을 보여준다.
				//-----------------------------------------------
				GUILayout.Space(margin);

				//EditorGUILayout.BeginVertical();
				Rect centerRect = new Rect(0, topHeight + margin, windowWidth, centerHeight);

				int centerRectWidth_List = 170;
				int centerRectWidth_Property = 250;
				int centerRectWidth_Main = windowWidth - (margin + centerRectWidth_List + margin + centerRectWidth_Property);

				Rect centerRect_Main = new Rect(0, topHeight + margin, centerRectWidth_Main, centerHeight);
				Rect centerRect_List = new Rect(centerRectWidth_Main + margin, topHeight + margin, centerRectWidth_List, centerHeight);
				Rect centerRect_Property = new Rect(centerRectWidth_Main + margin + centerRectWidth_List + margin, topHeight + margin, centerRectWidth_Property, centerHeight);

				//GUILayout.BeginArea(centerRect);

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));
				//EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));


				MouseUpdate(centerRect_Main);
				MouseScrollUpdate(centerRect_Main);

				if (_step == LOAD_STEP.Step1_FileLoad)
				{
					GUI.Box(centerRect, "");
				}
				else
				{
					Color guiBasicColor = GUI.backgroundColor;

					GUI.backgroundColor = _glBackGroundColor;
					GUI.Box(centerRect_Main, "");

					GUI.backgroundColor = guiBasicColor;


					GUI.Box(centerRect_List, "");
					GUI.Box(centerRect_Property, "");

					int scrollHeightOffset = 32;
					_scroll_MainCenter.y = GUI.VerticalScrollbar(new Rect(centerRect_Main.width - 15, scrollHeightOffset, 20, centerRect_Main.height - 15), _scroll_MainCenter.y, 5.0f, -100.0f, 100.0f + 5.0f);
					_scroll_MainCenter.x = GUI.HorizontalScrollbar(new Rect(0, (centerRect_Main.height - 15) + scrollHeightOffset, centerRect_Main.width - 15, 20), _scroll_MainCenter.x, 5.0f, -100.0f, 100.0f + 5.0f);

					if (GUI.Button(new Rect(centerRect_Main.width - 15, (centerRect_Main.height - 15) + scrollHeightOffset, 15, 15), ""))
					{
						_scroll_MainCenter = Vector2.zero;
						_iZoomX100 = ZOOM_INDEX_DEFAULT;
					}

					_gl.SetWindowSize(
						(int)centerRect_Main.width, (int)centerRect_Main.height,
						_scroll_MainCenter, (float)(_zoomListX100[_iZoomX100]) * 0.01f,
						(int)centerRect_Main.x, (int)centerRect_Main.y,
						(int)position.width, (int)position.height);


				}

				switch (_step)
				{
					case LOAD_STEP.Step1_FileLoad:
						{
							//1개의 레이아웃
							EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));

							EditorGUILayout.BeginVertical();
							GUILayout.Space(10);
							GUI_Center_FileLoad(windowWidth - 10, centerHeight - 26);
							EditorGUILayout.EndVertical();

							EditorGUILayout.EndHorizontal();
						}
						break;

					case LOAD_STEP.Step2_LayerCheck:
						{
							//3개의 레이아웃
							EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_Main.width), GUILayout.Height(centerHeight));
							//GUI_Center_LayerCheck_GUI(windowWidth - 10, centerHeight - 26);
							GUI_Center_LayerCheck_GUI((int)(centerRect_Main.width - 20), centerHeight - 26);
							EditorGUILayout.EndVertical();

							GUILayout.Space(margin);

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_List.width), GUILayout.Height(centerHeight - 6));
							//GUI_Center_LayerCheck_List((int)(centerRect_List.width - 4), centerHeight - 26);
							GUI_Center_LayerCheck_List((int)(centerRect_List.width), centerHeight - 6);
							EditorGUILayout.EndVertical();

							GUILayout.Space(margin);

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_Property.width - 2), GUILayout.Height(centerHeight));
							//GUI_Center_LayerCheck_Property((int)(centerRect_Property.width - 4), centerHeight - 26);
							GUI_Center_LayerCheck_Property((int)(centerRect_Property.width - 2), centerHeight);
							EditorGUILayout.EndVertical();

							EditorGUILayout.EndHorizontal();
						}

						break;

					case LOAD_STEP.Step3_AtlasSetting:
						{
							//3개의 레이아웃
							EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_Main.width), GUILayout.Height(centerHeight));
							//GUI_Center_LayerCheck_GUI(windowWidth - 10, centerHeight - 26);
							GUI_Center_AtlasSetting_GUI((int)(centerRect_Main.width - 20), centerHeight - 26);
							EditorGUILayout.EndVertical();

							GUILayout.Space(margin);

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_List.width), GUILayout.Height(centerHeight - 6));
							GUI_Center_AtlasSetting_List((int)(centerRect_List.width), centerHeight - 6);
							EditorGUILayout.EndVertical();

							GUILayout.Space(margin);

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_Property.width - 2), GUILayout.Height(centerHeight));
							GUI_Center_AtlasSetting_Property((int)(centerRect_Property.width - 2), centerHeight);
							EditorGUILayout.EndVertical();

							EditorGUILayout.EndHorizontal();
						}
						break;
				}
				//EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();

				//if(bottomMargin > 0)
				//{
				//	GUILayout.Space(bottomMargin);
				//}

				GUILayout.Space(4);

				//GUILayout.EndArea();
				//EditorGUILayout.EndVertical();
				// Bottom UI : 스텝 이동/확인/취소를 제어할 수 있다.
				//--------------------------------------------
				GUI.Box(new Rect(0, topHeight + margin + centerHeight + margin, windowWidth, bottomHeight), "");
				GUILayout.Space(margin);
				//GUILayout.Space(margin - 2);

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(bottomHeight));
				GUI_Bottom(windowWidth, bottomHeight - 12);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndVertical();
			}
			catch (Exception ex)
			{
				Debug.LogError("PSD Dialog Exception : " + ex);
				CloseDialog();
			}
		}



		// Top
		private void GUI_Top(int width, int height)
		{
			int stepWidth = (width / 5) - 10;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			//GUILayout.Space(20);
			int totalContentWidth = (stepWidth + 2) + 50 + (stepWidth + 2) + 50 + (stepWidth + 2);
			GUILayout.Space((width / 2) - (totalContentWidth / 2));

			Color prevColor = GUI.backgroundColor;

			GUIStyle guiStyle_Center = GUI.skin.box;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;
			guiStyle_Center.normal.textColor = Color.black;

			GUIStyle guiStyle_Next = GUI.skin.label;
			guiStyle_Next.alignment = TextAnchor.MiddleCenter;

			Color selectedColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
			Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			//Step 1
			if (_step == LOAD_STEP.Step1_FileLoad)
			{ GUI.backgroundColor = selectedColor; }
			else
			{ GUI.backgroundColor = unselectedColor; }
			GUILayout.Box("Load", guiStyle_Center, GUILayout.Width(stepWidth), GUILayout.Height(height));

			GUILayout.Space(10);
			GUILayout.Box(">>", guiStyle_Next, GUILayout.Width(30), GUILayout.Height(height));
			GUILayout.Space(10);

			//Step 2
			if (_step == LOAD_STEP.Step2_LayerCheck)
			{ GUI.backgroundColor = selectedColor; }
			else
			{ GUI.backgroundColor = unselectedColor; }
			GUILayout.Box("Layers", guiStyle_Center, GUILayout.Width(stepWidth), GUILayout.Height(height));

			GUILayout.Space(10);
			GUILayout.Box(">>", guiStyle_Next, GUILayout.Width(30), GUILayout.Height(height));
			GUILayout.Space(10);

			//Step 3
			if (_step == LOAD_STEP.Step3_AtlasSetting)
			{ GUI.backgroundColor = selectedColor; }
			else
			{ GUI.backgroundColor = unselectedColor; }
			GUILayout.Box("Atlas", guiStyle_Center, GUILayout.Width(stepWidth), GUILayout.Height(height));


			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = prevColor;
		}

		private Vector2 _guiScroll_FileLoad = Vector2.zero;

		// Center - File Load
		private void GUI_Center_FileLoad(int width, int height)
		{
			_guiScroll_FileLoad = EditorGUILayout.BeginScrollView(_guiScroll_FileLoad, false, true, GUILayout.Width(width), GUILayout.Height(height));

			width -= 20;
			EditorGUILayout.BeginVertical(GUILayout.Width(width));

			if (!_isFileLoaded)
			{
				int btnWidth = 120;
				int btnHeight = 40;

				GUILayout.Space((height / 2) - (btnHeight / 2));
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(btnHeight));

				GUILayout.Space((width / 2) - (btnWidth / 2));

				if (GUILayout.Button("Load PSD File", GUILayout.Width(btnWidth), GUILayout.Height(btnHeight)))
				{
					if (IsGUIUsable)
					{
						try
						{
							string filePath = EditorUtility.OpenFilePanel("Open PSD File", "", "psd");
							if (!string.IsNullOrEmpty(filePath))
							{
								LoadPsdFile(filePath);
							}
						}
						catch (Exception ex)
						{
							Debug.LogError("GUI_Center_FileLoad Exception : " + ex);
						}
					}
				}

				EditorGUILayout.EndHorizontal();
			}
			else
			{


				width -= 20;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				GUILayout.Space(20);

				width -= 40;
				EditorGUILayout.BeginVertical(GUILayout.Width(width));

				//기본 정보를 보여준다.
				EditorGUILayout.LabelField("PSD File Information", GUILayout.Width(width));
				GUILayout.Space(10);
				EditorGUILayout.LabelField("  PSD File Path : " + _fileFullPath, GUILayout.Width(width));
				GUILayout.Space(10);
				EditorGUILayout.LabelField("  Image Size : " + _imageWidth + " x " + _imageHeight, GUILayout.Width(width));
				EditorGUILayout.LabelField("  Image Layers : " + _layerDataList.Count, GUILayout.Width(width));

				GUILayout.Space(20);
				EditorGUILayout.LabelField("Layers", GUILayout.Width(width));

				GUILayout.Space(10);
				//거꾸로 출력한다. (0번 백그라운드가 맨 밑으로)
				if (_layerDataList.Count > 0)
				{
					for (int i = _layerDataList.Count - 1; i >= 0; i--)
					{
						apPSDLayerData curLayer = _layerDataList[i];
						string strLayerInfo = "  [" + curLayer._layerIndex + "] : " + curLayer._name + "  ( " + curLayer._width + " x " + curLayer._height;
						if (curLayer._isClipping)
						{
							strLayerInfo += " / Clipping";
						}
						strLayerInfo += " )";
						EditorGUILayout.LabelField(strLayerInfo, GUILayout.Width(width));
					}
				}

				GUILayout.Space(30);
				GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(4));
				GUILayout.Space(30);
				if (GUILayout.Button("Reset", GUILayout.Width(width), GUILayout.Height(15)))
				{
					if (IsGUIUsable)
					{
						//bool result = EditorUtility.DisplayDialog("Reset", "Reset PSD Import Process? (Data is not Saved)", "Reset", "Cancel");
						bool result = EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.ResetPSDImport_Title),
																	_editor.GetText(apLocalization.TEXT.ResetPSDImport_Body),
																	_editor.GetText(apLocalization.TEXT.ResetPSDImport_Okay),
																	_editor.GetText(apLocalization.TEXT.Cancel)
																	);
						if (result)
						{
							ClearPsdFile();
						}
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();

				GUILayout.Space(height);


			}


			GUILayout.Space(height + 20);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

		}



		// Center - Layer Check
		private void GUI_Center_LayerCheck_GUI(int width, int height)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_gl.DrawGrid();
			if (_layerDataList.Count > 0)
			{
				apPSDLayerData curImageLayer = null;
				//TODO : 선택한 이미지는 Outline이 나오도록 (true) 하자
				for (int i = 0; i < _layerDataList.Count; i++)
				{
					curImageLayer = _layerDataList[i];
					if (curImageLayer._image == null)
					{
						continue;
					}

					bool isOutline = (curImageLayer == _selectedLayerData);
					_gl.DrawTexture(curImageLayer._image,
										//curImageLayer._posOffset - _imageCenterPosOffset + imgPosOffset, 
										curImageLayer._posOffset - _imageCenterPosOffset,
										//curImageLayer._posOffsetWorld - _imageCenterPosOffset, 
										curImageLayer._width, curImageLayer._height,
										curImageLayer._transparentColor2X,
										isOutline);
				}
			}
			EditorGUILayout.EndVertical();
		}

		private Vector2 _scroll_LayerCheckList = Vector2.zero;

		private void GUI_Center_LayerCheck_List(int width, int height)
		{
			//Texture2D icon_Image = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);
			Texture2D icon_Clipping = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			Texture2D icon_Folder = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Folder);

			int itemHeight = 30;
			int levelMargin = 15;
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_NotBake = new GUIStyle(GUIStyle.none);
			guiStyle_Btn_NotBake.normal.textColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
			guiStyle_Btn_NotBake.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			if (GUILayout.Button("Deselect", GUILayout.Width(width - 7), GUILayout.Height(18)))
			{
				if (IsGUIUsable)
				{
					_selectedLayerData = null;
				}
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			_scroll_LayerCheckList = EditorGUILayout.BeginScrollView(_scroll_LayerCheckList, false, true, GUILayout.Width(width), GUILayout.Height(height - 33));
			width -= 24;
			//EditorGUILayout.BeginVertical(GUILayout.Width(width));

			GUILayout.Space(1);
			int iList = 0;

			if (_layerDataList.Count > 0)
			{

				apPSDLayerData curLayer = null;
				for (int i = _layerDataList.Count - 1; i >= 0; i--)
				{
					curLayer = _layerDataList[i];
					if (_selectedLayerData == curLayer)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						Color prevColor = GUI.backgroundColor;

						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);


						//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
						int xPos = (int)(_scroll_LayerCheckList.x + 0.5f);
						if (iList == 0)
						{
							GUI.Box(new Rect(lastRect.x + 1 + xPos, lastRect.y + 1, width + 10, itemHeight), "");
						}
						else
						{
							GUI.Box(new Rect(lastRect.x + 1 + xPos, lastRect.y + 30, width + 10, itemHeight), "");
						}

						GUI.backgroundColor = prevColor;
					}

					int level = curLayer._hierarchyLevel;
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width + (levelMargin * level)), GUILayout.Height(itemHeight));

					GUILayout.Space(5 + (levelMargin * level));


					bool isClipped = false;
					if (curLayer._isImageLayer)
					{
						isClipped = curLayer._isClipping;
						if (isClipped)
						{
							EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(itemHeight / 2), GUILayout.Height(itemHeight - 5));
						}

						EditorGUILayout.LabelField(new GUIContent(curLayer._image), guiStyle_Icon, GUILayout.Width(itemHeight - 5), GUILayout.Height(itemHeight - 5));

					}
					else
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Folder), guiStyle_Icon, GUILayout.Width(itemHeight - 5), GUILayout.Height(itemHeight - 5));
					}

					GUIStyle curGUIStyle = guiStyle_Btn;
					if (!curLayer._isBakable)
					{
						curGUIStyle = guiStyle_Btn_NotBake;
					}

					//if(GUILayout.Button(" [" + curLayer._layerIndex + "] " + curLayer._name, guiStyle_Btn, GUILayout.Width(width), GUILayout.Height(20)))
					int btnWidth = width - (5 + itemHeight);
					if (isClipped)
					{
						btnWidth -= (itemHeight / 2) + 2;
					}
					if (GUILayout.Button("  " + curLayer._name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(itemHeight)))
					{
						if (IsGUIUsable)
						{
							_selectedLayerData = curLayer;
						}
					}

					EditorGUILayout.EndHorizontal();

					iList++;
				}
			}

			//EditorGUILayout.EndVertical();

			GUILayout.Space(height + 20);

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}


		private Vector2 _scroll_LayerCheckProperty = Vector2.zero;

		private void GUI_Center_LayerCheck_Property(int width, int height)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_scroll_LayerCheckProperty = EditorGUILayout.BeginScrollView(_scroll_LayerCheckProperty, false, true, GUILayout.Width(width), GUILayout.Height(height));
			width -= 24;
			EditorGUILayout.BeginVertical(GUILayout.Width(width));

			GUILayout.Space(10);

			if (_selectedLayerData != null)
			{
				//현재 레이어 정보를 표시한다.
				//적용 여부도 설정
				bool prev_isClipping = _selectedLayerData._isClipping;
				bool prev_isBakable = _selectedLayerData._isBakable;

				EditorGUILayout.LabelField("Layer " + _selectedLayerData._layerIndex, GUILayout.Width(width));

				GUILayout.Space(5);
				EditorGUILayout.LabelField(_selectedLayerData._name, GUILayout.Width(width));
				EditorGUILayout.LabelField("Rect (LBRT : " + _selectedLayerData._posOffset_Left + ", " + _selectedLayerData._posOffset_Top + ", " + _selectedLayerData._posOffset_Right + ", " + _selectedLayerData._posOffset_Bottom + " )", GUILayout.Width(width));
				EditorGUILayout.LabelField("Position : " + _selectedLayerData._posOffset.x + ", " + _selectedLayerData._posOffset.y, GUILayout.Width(width));
				EditorGUILayout.LabelField("Size : " + _selectedLayerData._width + " x " + _selectedLayerData._height, GUILayout.Width(width));

				GUILayout.Space(5);
				EditorGUILayout.LabelField("Level : " + _selectedLayerData._hierarchyLevel, GUILayout.Width(width));
				if (_selectedLayerData._parentLayer != null)
				{
					EditorGUILayout.LabelField("Parent: " + _selectedLayerData._parentLayer._name, GUILayout.Width(width));
				}



				GUILayout.Space(5);
				bool next_isClipping = EditorGUILayout.Toggle("Clipping", _selectedLayerData._isClipping, GUILayout.Width(width));

				GUILayout.Space(5);
				bool next_isBakable = EditorGUILayout.Toggle("Bake Target", _selectedLayerData._isBakable, GUILayout.Width(width));

				if (IsGUIUsable)
				{
					//스레드가 작동 안될때에만 적용하자
					_selectedLayerData._isClipping = next_isClipping;
					_selectedLayerData._isBakable = next_isBakable;
				}

				if (_selectedLayerData._isClipping && !_selectedLayerData._isClippingValid)
				{
					GUILayout.Space(10);

					Color prevColor = GUI.color;
					GUI.color = new Color(1.0f, 0.6f, 0.6f, 1.0f);

					GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
					guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
					GUILayout.Box("Warning\nClipped Layers are Over 3", guiStyle_WarningBox, GUILayout.Width(width), GUILayout.Height(70));

					GUI.color = prevColor;
				}

				if (prev_isClipping != _selectedLayerData._isClipping
					|| prev_isBakable != _selectedLayerData._isBakable)
				{
					_isNeedBakeCheck = true;//<<다시 Bake할 수 있도록 하자
					CheckClippingValidation();
				}
			}
			else
			{
				//이미지 전체 정보를 표시한다.
				EditorGUILayout.LabelField("Image Name", GUILayout.Width(width));
				EditorGUILayout.LabelField(_fileNameOnly, GUILayout.Width(width));

				GUILayout.Space(5);
				EditorGUILayout.LabelField("File Path", GUILayout.Width(width));
				EditorGUILayout.TextField(_fileFullPath, GUILayout.Width(width));

				GUILayout.Space(5);
				EditorGUILayout.LabelField("Size", GUILayout.Width(width));
				EditorGUILayout.LabelField(_imageWidth + " x " + _imageHeight, GUILayout.Width(width));
			}

			EditorGUILayout.EndVertical();
			GUILayout.Space(height + 20);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}



		// Center - Atlas Setting
		private void GUI_Center_AtlasSetting_GUI(int width, int height)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_gl.DrawGrid();

			if (_bakeDataList.Count > 0 && !_isImageBaking)
			{
				if (_selectedBakeData == null)
				{
					apPSDBakeData curBakedData = null;
					//TODO : 선택한 이미지는 Outline이 나오도록 (true) 하자
					for (int i = 0; i < _bakeDataList.Count; i++)
					{
						curBakedData = _bakeDataList[i];
						Vector2 imgPosOffset = new Vector2(curBakedData._width * i, 0);

						_gl.DrawTexture(curBakedData._bakedImage,
											new Vector2(curBakedData._width / 2, curBakedData._height / 2) + imgPosOffset,
											curBakedData._width, curBakedData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f),
											false);
					}
				}
				else
				{
					_gl.DrawTexture(_selectedBakeData._bakedImage,
											new Vector2(_selectedBakeData._width / 2, _selectedBakeData._height / 2),
											_selectedBakeData._width, _selectedBakeData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f),
											true);
				}
			}
			EditorGUILayout.EndVertical();
		}


		private Vector2 _scroll_AtlasSettingList = Vector2.zero;
		private void GUI_Center_AtlasSetting_List(int width, int height)
		{
			Texture2D icon_Image = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);

			int itemHeight = 30;
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;



			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			if (GUILayout.Button("Deselect", GUILayout.Width(width - 7), GUILayout.Height(18)))
			{
				if (IsGUIUsable)
				{
					_selectedBakeData = null;
				}
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			_scroll_AtlasSettingList = EditorGUILayout.BeginScrollView(_scroll_AtlasSettingList, false, true, GUILayout.Width(width), GUILayout.Height(height - 33));
			width -= 24;
			//EditorGUILayout.BeginVertical(GUILayout.Width(width));

			GUILayout.Space(1);
			int iList = 0;
			if (_bakeDataList.Count > 0)
			{
				apPSDBakeData curBakeData = null;
				for (int i = 0; i < _bakeDataList.Count; i++)
				{
					curBakeData = _bakeDataList[i];
					if (_selectedBakeData == curBakeData)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						Color prevColor = GUI.backgroundColor;

						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);


						//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
						if (iList == 0)
						{
							GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 1, width + 10, itemHeight), "");
						}
						else
						{
							GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 30, width + 10, itemHeight), "");
						}

						GUI.backgroundColor = prevColor;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(itemHeight));

					GUILayout.Space(5);
					EditorGUILayout.LabelField(new GUIContent(icon_Image), guiStyle_Icon, GUILayout.Width(itemHeight - 5), GUILayout.Height(itemHeight - 5));
					if (GUILayout.Button("  Atlas " + curBakeData._atlasIndex, guiStyle_Btn, GUILayout.Width(width - (5 + itemHeight)), GUILayout.Height(itemHeight)))
					{
						if (IsGUIUsable)
						{
							_selectedBakeData = curBakeData;
						}
					}

					EditorGUILayout.EndHorizontal();

					iList++;
				}
			}

			GUILayout.Space(height + 20);

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}


		private Vector2 _scroll_AtlasSettingProperty = Vector2.zero;
		private void GUI_Center_AtlasSetting_Property(int width, int height)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_scroll_AtlasSettingProperty = EditorGUILayout.BeginScrollView(_scroll_AtlasSettingProperty, false, true, GUILayout.Width(width), GUILayout.Height(height));
			width -= 24;
			EditorGUILayout.BeginVertical(GUILayout.Width(width));

			GUILayout.Space(10);
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Asset Name", GUILayout.Width(width));
			string next_fileNameOnly = EditorGUILayout.DelayedTextField(_fileNameOnly, GUILayout.Width(width));
			if (IsGUIUsable)
			{
				_fileNameOnly = next_fileNameOnly;
			}

			GUILayout.Space(5);
			EditorGUILayout.LabelField("Save Path", GUILayout.Width(width));
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			string prev_bakeDstFilePath = _bakeDstFilePath;
			string next_bakeDstFilePath = EditorGUILayout.DelayedTextField(_bakeDstFilePath, GUILayout.Width(width - 44));
			if (IsGUIUsable)
			{
				_bakeDstFilePath = next_bakeDstFilePath;
			}
			if (GUILayout.Button("Set", GUILayout.Width(40)))
			{
				if (IsGUIUsable)
				{
					_bakeDstFilePath = EditorUtility.SaveFolderPanel("Save Path Folder", "Assets", _fileNameOnly);

					if (!_bakeDstFilePath.StartsWith(Application.dataPath))
					{

						//EditorUtility.DisplayDialog("Bake Destination Path Error", "Bake Destination Path is have to be in Asset Folder", "Okay");
						EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.PSDBakeError_Title_WrongDst),
														_editor.GetText(apLocalization.TEXT.PSDBakeError_Body_WrongDst),
														_editor.GetText(apLocalization.TEXT.Close)
														);

						_bakeDstFilePath = "";
						_bakeDstFileRelativePath = "";
					}
					else
					{
						//앞의 걸 빼고 나면 (..../Assets) + ....가 된다.
						//Releatives는 "Assets/..."로 시작해야한다.
						int subStartLength = Application.dataPath.Length;
						_bakeDstFileRelativePath = "Assets";
						if (_bakeDstFilePath.Length > subStartLength)
						{
							_bakeDstFileRelativePath += _bakeDstFilePath.Substring(subStartLength);
						}
					}
				}
				//_bakeDstFilePath = EditorUtility.SaveFilePanelInProject("Set Atlas Save Path with File Name", _fileNameOnly, "png", "Please enter a file name to save the atlas set.\nFiles are name with an index.");
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);

			//EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Atlas Baking Option", GUILayout.Width(width));
			GUILayout.Space(10);

			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//bool prev_isBakeResizable = _isBakeResizable;
			//EditorGUILayout.LabelField("Auto Resize : ", GUILayout.Width(120));
			//_isBakeResizable = EditorGUILayout.Toggle(_isBakeResizable, GUILayout.Width(width - 124));
			//EditorGUILayout.EndHorizontal();
			//GUILayout.Space(5);

			BAKE_SIZE prev_bakeWidth = _bakeWidth;
			BAKE_SIZE prev_bakeHeight = _bakeHeight;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Atlas Width : ", GUILayout.Width(120));
			BAKE_SIZE next_bakeWidth = (BAKE_SIZE)EditorGUILayout.Popup((int)_bakeWidth, _bakeDescription, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakeWidth = next_bakeWidth;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Atlas Height : ", GUILayout.Width(120));
			BAKE_SIZE next_bakeHeight = (BAKE_SIZE)EditorGUILayout.Popup((int)_bakeHeight, _bakeDescription, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakeHeight = next_bakeHeight;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			int prev_bakeMaximumNumAtlas = _bakeMaximumNumAtlas;
			int prev_bakePadding = _bakePadding;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Maximum Atlas : ", GUILayout.Width(120));
			int next_bakeMaximumNumAtlas = EditorGUILayout.DelayedIntField(_bakeMaximumNumAtlas, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakeMaximumNumAtlas = next_bakeMaximumNumAtlas;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Padding : ", GUILayout.Width(120));
			int next_bakePadding = EditorGUILayout.DelayedIntField(_bakePadding, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakePadding = next_bakePadding;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			bool prev_bakeBlurOption = _bakeBlurOption;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField("Fix Border Problem : ", GUILayout.Width(120));
			bool next_bakeBlurOption = EditorGUILayout.Toggle(_bakeBlurOption, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakeBlurOption = next_bakeBlurOption;
			}
			EditorGUILayout.EndHorizontal();


			//이제 Bake 가능한지 체크하자
			GUILayout.Space(10);
			if (
				//prev_isBakeResizable != _isBakeResizable ||
				prev_bakeWidth != _bakeWidth ||
				prev_bakeHeight != _bakeHeight ||
				prev_bakeMaximumNumAtlas != _bakeMaximumNumAtlas ||
				prev_bakePadding != _bakePadding ||
				!string.Equals(prev_bakeDstFilePath, _bakeDstFilePath) ||
				prev_bakeBlurOption != _bakeBlurOption)
			{
				_isNeedBakeCheck = true;
			}

			if (_isNeedBakeCheck)
			{
				CheckBakable();
			}

			Color prevColor = GUI.color;

			GUIStyle guiStyle_Result = new GUIStyle(GUI.skin.box);
			guiStyle_Result.alignment = TextAnchor.MiddleLeft;


			if (_isBakeWarning)
			{
				GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
				guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;


				GUI.color = new Color(1.0f, 0.6f, 0.6f, 1.0f);

				GUILayout.Box("Warning\n" + _bakeWarningMsg, guiStyle_WarningBox, GUILayout.Width(width), GUILayout.Height(70));

				GUI.color = prevColor;
			}
			else
			{
				if (GUILayout.Button("Bake", GUILayout.Width(width), GUILayout.Height(40)))
				{
					if (IsGUIUsable)
					{
						StartBake();
					}
				}
				if (_loadKey_CheckBake != _loadKey_Bake)
				{
					GUILayout.Space(10);
					GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
					guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;

					GUI.color = new Color(0.6f, 0.6f, 1.0f, 1.0f);

					GUILayout.Box("[ Settings are changed ]"
									+ "\n  Expected Scale : " + _realBakeResizeX100 + " %"
									+ "\n  Expected Atlas : " + _realBakedAtlasCount,
									guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));

					GUI.color = prevColor;

				}


			}
			GUILayout.Space(10);
			if (_loadKey_Bake != null)
			{
				//Bake가 되었다면 => 그 정보를 넣어주자
				GUILayout.Box("[ Baked Result ]"
								+ "\n  Scale Percent : " + _resultBakeResizeX100 + " %"
								+ "\n  Atlas : " + _resultAtlasCount,
								guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));

			}


			GUILayout.Space(20);

			if (IsProcessRunning)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();

				Rect barRect = new Rect(lastRect.x + 5, lastRect.y + 30, width - 5, 20);
				float barRatio = Mathf.Clamp01((float)_workProcess.ProcessX100 / 100.0f);

				//EditorGUI.ProgressBar(barRect, barRatio, "Convert PSD Data To Editor..");
				EditorGUI.ProgressBar(barRect, barRatio, _threadProcessName);

			}

			EditorGUILayout.EndVertical();
			GUILayout.Space(height + 20);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}


		// Bottom
		private void GUI_Bottom(int width, int height)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			int btnWidth = 120;
			int btnWidth_Cancel = 60;
			int margin = width - (btnWidth * 2 + btnWidth_Cancel + 12 + 30 + 200 + 10 + 2 + 20);

			GUILayout.Space(10);
			EditorGUILayout.BeginVertical(GUILayout.Width(200));
			GUILayout.Space(2);
			EditorGUILayout.LabelField("Background Color", GUILayout.Width(200));
			try
			{
				_glBackGroundColor = EditorGUILayout.ColorField(_glBackGroundColor, GUILayout.Width(80));
			}
			catch (Exception) { }

			EditorGUILayout.EndVertical();
			GUILayout.Space(margin);

			if (_step == LOAD_STEP.Step1_FileLoad)
			{
				GUILayout.Space(btnWidth + 4);
			}
			else
			{
				if (GUILayout.Button("< Back", GUILayout.Width(btnWidth), GUILayout.Height(height)))
				{
					if (IsGUIUsable)
					{
						MoveStep(false);
					}
				}
			}

			if (_step == LOAD_STEP.Step3_AtlasSetting)
			{
				if (GUILayout.Button("Complete", GUILayout.Width(btnWidth), GUILayout.Height(height)))
				{
					if (IsGUIUsable)
					{
						StartBakedImageSave();
						//bool isTextureResult = SaveBakedImages();
						//if(!isTextureResult)
						//{
						//	EditorUtility.DisplayDialog("Texture Save Failed", "Texture Save Failed", "Okay");
						//}
						//else
						//{
						//	OnLoadComplete(true);
						//}

					}
				}
			}
			else
			{
				if (_isFileLoaded)
				{
					if (GUILayout.Button("Next >", GUILayout.Width(btnWidth), GUILayout.Height(height)))
					{
						if (IsGUIUsable)
						{
							MoveStep(true);
						}
					}
				}
				else
				{
					Color prevColor = GUI.backgroundColor;

					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					GUILayout.Box("Next >", GUILayout.Width(btnWidth), GUILayout.Height(height));

					GUI.backgroundColor = prevColor;
				}
			}

			GUILayout.Space(30);

			if (GUILayout.Button("Close", GUILayout.Width(btnWidth_Cancel), GUILayout.Height(height)))
			{
				if (IsGUIUsable)
				{
					//bool result = EditorUtility.DisplayDialog("Close", "Close PSD Load? (Data is Not Saved)", "Close", "Cancel");

					bool result = EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.ClosePSDImport_Title),
																_editor.GetText(apLocalization.TEXT.ClosePSDImport_Body),
																_editor.GetText(apLocalization.TEXT.Close),
																_editor.GetText(apLocalization.TEXT.Cancel));

					if (result)
					{
						OnLoadComplete(false);
						CloseDialog();
					}
				}
			}



			EditorGUILayout.EndHorizontal();
		}

		// Functions
		//----------------------------------------------------------
		private void ClearPsdFile()
		{
			_isFileLoaded = false;
			_fileFullPath = "";
			_fileNameOnly = "";
			_imageWidth = -1;
			_imageHeight = -1;
			_imageCenterPosOffset = Vector2.zero;

			_layerDataList.Clear();
			_selectedLayerData = null;

			_bakeDataList.Clear();
			_selectedBakeData = null;

			//_isBakeResizable = false;//<<크기가 안맞으면 자동으로 리사이즈를 할 것인가 (이건 넓이 비교로 리사이즈를 하자)
			_bakeWidth = BAKE_SIZE.s1024;
			_bakeHeight = BAKE_SIZE.s1024;
			_bakeDstFilePath = "";//저장될 기본 경로 (폴더만 지정한다. 나머지는 파일 + 이미지 번호)
			_bakeMaximumNumAtlas = 2;
			_bakePadding = 4;
			_bakeBlurOption = true;

			_isNeedBakeCheck = true;
			//_needBakeResizeX100 = 100;
			_bakeParams.Clear();

			_loadKey_CheckBake = null;
			_loadKey_Bake = null;

			_resultAtlasCount = 0;
			_resultBakeResizeX100 = 0;
			_resultPadding = 0;
		}


		private bool LoadPsdFile(string filePath)
		{

			PsdDocument psdDoc = null;
			try
			{
				ClearPsdFile();

				psdDoc = PsdDocument.Create(filePath);
				if (psdDoc == null)
				{
					//EditorUtility.DisplayDialog("PSD Load Failed", "No File Loaded [" + filePath + "]", "Okay");
					EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.PSDBakeError_Title_Load),
													_editor.GetTextFormat(apLocalization.TEXT.PSDBakeError_Body_LoadPath, filePath),
													_editor.GetText(apLocalization.TEXT.Close)
													);
					return false;
				}
				_fileFullPath = filePath;

				_fileNameOnly = "";

				if (_fileFullPath.Length > 4)
				{
					for (int i = _fileFullPath.Length - 5; i >= 0; i--)
					{
						string curChar = _fileFullPath.Substring(i, 1);
						if (curChar == "\\" || curChar == "/")
						{
							break;
						}
						_fileNameOnly = curChar + _fileNameOnly;
					}
				}
				_imageWidth = psdDoc.FileHeaderSection.Width;
				_imageHeight = psdDoc.FileHeaderSection.Height;
				_imageCenterPosOffset = new Vector2((float)_imageWidth * 0.5f, (float)_imageHeight * 0.5f);

				if (_imageWidth > PSD_IMAGE_FILE_MAX_SIZE || _imageHeight > PSD_IMAGE_FILE_MAX_SIZE)
				{
					//EditorUtility.DisplayDialog("PSD Load Failed", 
					//	"Image File is Too Large [ " + _imageWidth + " x " + _imageHeight + " ] (Maximum 5000 x 5000)", 
					//	"Okay");

					EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.PSDBakeError_Title_Load),
													_editor.GetTextFormat(apLocalization.TEXT.PSDBakeError_Body_LoadSize, _imageWidth, _imageHeight),
													_editor.GetText(apLocalization.TEXT.Close)
													);
					ClearPsdFile();
					return false;
				}

				int curLayerIndex = 0;

				RecursiveAddLayer(psdDoc.Childs, 0, null, curLayerIndex);

				//클리핑이 가능한가 체크
				CheckClippingValidation();

				_isFileLoaded = true;

				psdDoc.Dispose();
				psdDoc = null;
				System.GC.Collect();

				return true;
			}
			catch (Exception ex)
			{
				ClearPsdFile();

				if (psdDoc != null)
				{
					psdDoc.Dispose();
					System.GC.Collect();
				}

				Debug.LogError("Load PSD File Exception : " + ex);

				//EditorUtility.DisplayDialog("PSD Load Failed", "Error Occured [" + ex.ToString() + "]", "Okay");
				EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.PSDBakeError_Title_Load),
												_editor.GetTextFormat(apLocalization.TEXT.PSDBakeError_Body_ErrorCode, ex.ToString()),
												_editor.GetText(apLocalization.TEXT.Close)
												);

			}

			return false;
		}





		private int RecursiveAddLayer(IPsdLayer[] layers, int level, apPSDLayerData parentLayerData, int curLayerIndex)
		{
			for (int i = 0; i < layers.Length; i++)
			{
				IPsdLayer curLayer = layers[i];
				if (curLayer == null)
				{
					continue;
				}

				apPSDLayerData newLayerData = new apPSDLayerData(curLayerIndex, curLayer, _imageWidth, _imageHeight);
				newLayerData.SetLevel(level);
				if (parentLayerData != null)
				{
					parentLayerData.AddChildLayer(newLayerData);
				}

				curLayerIndex++;

				//재귀 호출을 하자
				if (curLayer.Childs != null && curLayer.Childs.Length > 0)
				{
					curLayerIndex = RecursiveAddLayer(curLayer.Childs, level + 1, newLayerData, curLayerIndex);
				}

				_layerDataList.Add(newLayerData);
			}
			return curLayerIndex;
		}

		private void MakePosOffsetLocals(List<apPSDLayerData> layerList, int curLevel, apPSDLayerData parentLayer)
		{
			for (int i = 0; i < layerList.Count; i++)
			{
				apPSDLayerData curLayer = layerList[i];
				if (curLayer._hierarchyLevel != curLevel)
				{
					continue;
				}

				if (parentLayer != null)
				{
					curLayer._posOffsetLocal = curLayer._posOffset - parentLayer._posOffset;
				}
				else
				{
					curLayer._posOffsetLocal = curLayer._posOffset;
				}
			}
		}

		private void CheckClippingValidation()
		{
			//Debug.Log("CheckClippingValidation");
			//클리핑이 가능한가 체크
			//어떤 클리핑 옵션이 나올때
			//"같은 레벨에서" ㅁ CC[C] 까지는 Okay / ㅁCCC..[C]는 No
			for (int i = 0; i < _layerDataList.Count; i++)
			{
				apPSDLayerData curLayerData = _layerDataList[i];
				curLayerData._isClippingValid = true;

				if (curLayerData._isImageLayer && curLayerData._isClipping)
				{
					//앞으로 체크해보자.
					int curLevel = curLayerData._hierarchyLevel;

					apPSDLayerData prev1_Layer = null;
					apPSDLayerData prev2_Layer = null;
					apPSDLayerData prev3_Layer = null;

					if (i - 1 >= 0)
					{ prev1_Layer = _layerDataList[i - 1]; }
					if (i - 2 >= 0)
					{ prev2_Layer = _layerDataList[i - 2]; }
					if (i - 3 >= 0)
					{ prev3_Layer = _layerDataList[i - 3]; }

					bool isValiePrev1 = (prev1_Layer != null && prev1_Layer._isBakable && prev1_Layer._isImageLayer && !prev1_Layer._isClipping && prev1_Layer._hierarchyLevel == curLevel);
					bool isValiePrev2 = (prev2_Layer != null && prev2_Layer._isBakable && prev2_Layer._isImageLayer && !prev2_Layer._isClipping && prev2_Layer._hierarchyLevel == curLevel);
					bool isValiePrev3 = (prev3_Layer != null && prev3_Layer._isBakable && prev3_Layer._isImageLayer && !prev3_Layer._isClipping && prev3_Layer._hierarchyLevel == curLevel);
					if (isValiePrev1 || isValiePrev2 || isValiePrev3)
					{
						curLayerData._isClippingValid = true;
					}
					else
					{
						//Clipping의 대상이 없다면 문제가 있다.
						//Debug.LogError("Find Invalid Clipping [" + curLayerData._name + "]");
						curLayerData._isClippingValid = false;
					}
				}
			}
		}


		public void CheckBakable()
		{
			_isNeedBakeCheck = false;
			_isBakeWarning = false;//<이게 True이면 Bake가 불가능하다.
			_bakeWarningMsg = "";
			//_bakeResizeRatioX100 = 100;
			_realBakeSizePerIndex = -1;
			_loadKey_CheckBake = null;
			_realBakedAtlasCount = 0;
			_realBakeResizeX100 = 100;

			CheckClippingValidation();

			if (_bakeMaximumNumAtlas <= 0)
			{
				_isBakeWarning = true;
				_bakeWarningMsg = "[Maximum Atlas] is less than 0";
				return;
			}

			if (_bakePadding < 0)
			{
				_isBakeWarning = true;
				_bakeWarningMsg = "[Padding] is less than 0";
				return;
			}

			//1. Path 미지정
			if (string.IsNullOrEmpty(_bakeDstFilePath))
			{
				_isBakeWarning = true;
				_bakeWarningMsg = "Save Path is empty";

				return;
			}

			//2. 크기를 비교하자
			//W,H 합계, 최대값, 최소값, 영역 전체의 합
			int nLayer = 0;
			int sumWidth = 0;
			int sumHeight = 0;
			int maxWidth = -1;
			int maxHeight = -1;
			int minWidth = -1;
			int minHeight = -1;
			double sumArea = 0;

			apPSDLayerData curLayer = null;
			List<apPSDLayerData> bakableLayersX = new List<apPSDLayerData>(); //X축 큰거부터 체크
			List<apPSDLayerData> bakableLayersY = new List<apPSDLayerData>(); //Y축 큰거부터 체크
			for (int i = 0; i < _layerDataList.Count; i++)
			{
				curLayer = _layerDataList[i];
				if (!curLayer._isBakable || curLayer._image == null)
				{
					continue;
				}
				bakableLayersX.Add(curLayer);
				bakableLayersY.Add(curLayer);
				nLayer++;
				int curWidth = curLayer._width + (_bakePadding * 2);
				int curHeight = curLayer._height + (_bakePadding * 2);

				sumWidth += curWidth;
				sumHeight += curHeight;

				if (maxWidth < 0 || curWidth > maxWidth)
				{
					maxWidth = curWidth;
				}
				if (maxHeight < 0 || curHeight > maxHeight)
				{
					maxHeight = curHeight;
				}
				if (minWidth < 0 || curWidth < minWidth)
				{
					minWidth = curWidth;
				}
				if (minHeight < 0 || curHeight < minHeight)
				{
					minHeight = curHeight;
				}
				sumArea += (curWidth * curHeight);
			}

			//_needBakeResizeX100 = 100;

			_bakeParams.Clear();

			if (sumWidth < 10 || sumHeight < 10)
			{
				_isBakeWarning = true;//<이게 True이면 Bake가 불가능하다.
				_bakeWarningMsg = "Too Small Image";
				return;
			}

			//이제 본격적으로 만들어보자
			//Slot이라는 개념을 만들자.
			//Slot의 최소 크기는 최소 W,H의 크기값을 기준으로 한다.
			//minWH의 1/10의 값을 기준으로 4~32의 값을 가진다.


			//시작 Resize 값을 결정한다. (기본 100)
			//만약 maxWH가 요청한 Bake사이즈보다 크다면 -> 그만큼 리사이즈를 먼저 한다.
			//반복 수행
			//만약 maxWH가 요청한 Bake 사이즈보다 작다면 리사이즈가 없다는 가정으로 정사각형으로 만든다. (최대 Atlas 결과값이 오버가 되면 리사이즈를 해서 다시 수행한다.)
			int curResizeX100 = 100;
			float curResizeRatio = 1.0f;
			int slotSize = Mathf.Clamp((Mathf.Min(minWidth, minHeight) / 10), 4, 32);
			_realBakeSizePerIndex = slotSize;//<<이 값을 곱해서 실제 위치를 구한다.

			int numSlotAxisX = GetBakeSize(_bakeWidth) / slotSize;
			int numSlotAxisY = GetBakeSize(_bakeHeight) / slotSize;

			bool isSuccess = false;

			float baseRatioW = (float)GetBakeSize(_bakeWidth) / (float)maxWidth;
			float baseRatioH = (float)GetBakeSize(_bakeHeight) / (float)maxHeight;
			int baseRatioX100 = (int)((Mathf.Max(baseRatioW, baseRatioH) + 0.5f) * 100.0f);

			if (baseRatioX100 % 5 != 0)
			{
				baseRatioX100 = ((baseRatioX100 + 5) / 5) * 5;
			}

			if (baseRatioX100 < 100)
			{
				//maxW 또는 maxH가 이미지 크기를 넘었다.
				//리사이즈를 해야한다.
				//스케일은 5단위로 한다.
				curResizeX100 = baseRatioX100;
			}

			List<int[,]> atlasSlots = new List<int[,]>();
			for (int i = 0; i < _bakeMaximumNumAtlas; i++)
			{
				atlasSlots.Add(new int[numSlotAxisX, numSlotAxisY]);
			}

			//크기가 큰 이미지부터 내림차순
			bakableLayersX.Sort(delegate (apPSDLayerData a, apPSDLayerData b)
			{
				return b._width - a._width;
			});

			bakableLayersY.Sort(delegate (apPSDLayerData a, apPSDLayerData b)
			{
				return b._height - a._height;
			});


			List<apPSDLayerData> checkLayersX = new List<apPSDLayerData>();
			List<apPSDLayerData> checkLayersY = new List<apPSDLayerData>();
			while (true)
			{
				curResizeRatio = (float)curResizeX100 / 100.0f;
				if (curResizeX100 < 10)
				{
					isSuccess = false;
					break;//실패다.
				}

				//일단 슬롯을 비워두자
				for (int iAtlas = 0; iAtlas < atlasSlots.Count; iAtlas++)
				{
					int[,] slots = atlasSlots[iAtlas];
					for (int iX = 0; iX < numSlotAxisX; iX++)
					{
						for (int iY = 0; iY < numSlotAxisY; iY++)
						{
							slots[iX, iY] = -1;//<인덱스 할당 정보 초기화
						}
					}
				}

				//계산할 LayerData를 다시 리셋하자.
				checkLayersX.Clear();
				checkLayersY.Clear();
				for (int i = 0; i < bakableLayersX.Count; i++)
				{
					checkLayersX.Add(bakableLayersX[i]);
					checkLayersY.Add(bakableLayersY[i]);
				}

				_bakeParams.Clear();

				bool isCheckX = true;
				//X, Y축을 번갈아가면서 체크한다.
				//X일땐 X축부터 체크하면서 빈칸을 채운다.


				apPSDLayerData nextLayer = null;

				isSuccess = false;
				while (true)
				{
					if (checkLayersX.Count == 0 || checkLayersY.Count == 0)
					{
						//다 넣었다.
						isSuccess = true;
						break;
					}

					//다음 Layer를 꺼내서 슬롯을 체크한다.
					if (isCheckX)
					{
						nextLayer = checkLayersX[0];
					}
					else
					{
						nextLayer = checkLayersY[0];
					}

					//꺼낸 값은 Layer에서 삭제한다.
					checkLayersX.Remove(nextLayer);
					checkLayersY.Remove(nextLayer);


					int layerIndex = nextLayer._layerIndex;
					//Slot Width, Height를 계산하자
					int slotWidth = (int)(((float)nextLayer._width * curResizeRatio) + (_bakePadding * 2)) / slotSize;
					int slotHeight = (int)(((float)nextLayer._height * curResizeRatio) + (_bakePadding * 2)) / slotSize;
					//이제 빈칸을 찾자!

					//Atlas 앞부터 시작해서
					//Check X인 경우는 : Y -> X순서
					//Check Y인 경우는 : X -> Y순서
					bool isAddedSuccess = false;
					int iAddedX = -1;
					int iAddedY = -1;
					int iAddedAtlas = -1;
					for (int iAtlas = 0; iAtlas < atlasSlots.Count; iAtlas++)
					{
						int[,] slots = atlasSlots[iAtlas];


						bool addResult = false;


						if (isCheckX)
						{
							//X먼저 계산할 때
							for (int iY = 0; iY < numSlotAxisY; iY++)
							{
								for (int iX = 0; iX < numSlotAxisX; iX++)
								{
									addResult = AddToSlot(iX, iY, slotWidth, slotHeight, slots, numSlotAxisX, numSlotAxisY, layerIndex);
									if (addResult)
									{
										iAddedX = iX;
										iAddedY = iY;
										iAddedAtlas = iAtlas;
										break;
									}
								}

								if (addResult)
								{ break; }
							}
						}
						else
						{
							//Y먼저 계산할 때
							for (int iX = 0; iX < numSlotAxisX; iX++)
							{
								for (int iY = 0; iY < numSlotAxisY; iY++)
								{
									addResult = AddToSlot(iX, iY, slotWidth, slotHeight, slots, numSlotAxisX, numSlotAxisY, layerIndex);
									if (addResult)
									{
										iAddedX = iX;
										iAddedY = iY;
										iAddedAtlas = iAtlas;
										break;
									}
								}

								if (addResult)
								{ break; }
							}
						}

						if (addResult)
						{
							isAddedSuccess = true;
							break;
						}
					}

					if (isAddedSuccess)
					{
						//적당히 넣었다.
						LayerBakeParam newBakeParam = new LayerBakeParam(nextLayer, iAddedAtlas, iAddedX, iAddedY);
						_bakeParams.Add(newBakeParam);

						//실제로 작성된 Atlas의 개수를 확장한다.
						if (iAddedAtlas + 1 > _realBakedAtlasCount)
						{
							_realBakedAtlasCount = iAddedAtlas + 1;
						}
					}
					else
					{
						//하나라도 실패하면 돌아간다.
						isSuccess = false;
						break;
					}


					isCheckX = !isCheckX;//토글!
										 //다음 이미지를 넣어보자 -> 루프
				}

				//모두 넣었다면
				if (isSuccess)
				{
					break;
				}

				curResizeX100 -= 5;
			}

			if (nLayer > 0 && _realBakedAtlasCount == 0)
			{
				isSuccess = false;
				_isBakeWarning = true;
				_bakeWarningMsg = "No Baked Atlas";
			}


			if (!isSuccess)
			{
				_isBakeWarning = true;//<이게 True이면 Bake가 불가능하다.
				_bakeWarningMsg = "Need to increase [Number of Maximum Atlas]";

				//if(!_isBakeResizable)
				//{
				//	_bakeWarningMsg = "Need to turn on [Auto Resize] \n or [Increase Number of Maximum Atlas]";
				//}
				//else
				//{
				//	_bakeWarningMsg = "Need to increase [Number of Maximum Atlas]";
				//}
				return;
			}

			_realBakeResizeX100 = curResizeX100;
			_loadKey_CheckBake = new object();//마지막으로 Bake Check가 끝났다는 Key를 만들어주자
		}

		private int GetBakeSize(BAKE_SIZE bakeSize)
		{
			switch (bakeSize)
			{
				case BAKE_SIZE.s256:
					return 256;
				case BAKE_SIZE.s512:
					return 512;
				case BAKE_SIZE.s1024:
					return 1024;
				case BAKE_SIZE.s2048:
					return 2048;
				case BAKE_SIZE.s4096:
					return 4096;
			}
			return 4096;
		}

		//슬롯에 레이어를 넣을 수 있는지 확인하자
		private bool AddToSlot(int startPosX, int startPosY, int slotWidth, int slotHeight, int[,] targetSlot, int slotSizeX, int slotSizeY, int addedLayerIndex)
		{
			if (targetSlot[startPosX, startPosY] >= 0)
			{
				//시작점에 뭔가가 있다.
				return false;
			}

			if (startPosX + slotWidth >= slotSizeX ||
				startPosY + slotHeight >= slotSizeY)
			{
				//영역을 벗어난다.
				return false;
			}

			for (int iX = startPosX; iX <= startPosX + slotWidth; iX++)
			{
				for (int iY = startPosY; iY <= startPosY + slotHeight; iY++)
				{
					if (targetSlot[iX, iY] >= 0)
					{
						return false;//뭔가가 있다.
					}
				}
			}

			//넣어봤는데 괜찮네요
			for (int iX = startPosX; iX <= startPosX + slotWidth; iX++)
			{
				for (int iY = startPosY; iY <= startPosY + slotHeight; iY++)
				{
					targetSlot[iX, iY] = addedLayerIndex;
				}
			}
			return true;
		}



		//중요! Bake!
		private bool StartBake()
		{
			if (_loadKey_CheckBake == null)
			{
				return false;
			}
			if (_realBakeSizePerIndex <= 0 || _realBakeResizeX100 <= 0)
			{
				return false;
			}


			CloseThread();

			//_threadProcessX100 = 0;
			//_isThreadProcess = true;
			//_isSaveImageThreadStart = false;
			//_isThreadProcessSuccess = false;

			//_threadWorkType = THREAD_WORK_TYPE.BakeImage;
			_threadProcessName = "Bake Atlas..";

			_bakeDataList.Clear();

			_workProcess.Add(Work_Bake_1, _realBakedAtlasCount);
			_workProcess.Add(Work_Bake_2, _bakeParams.Count);
			_workProcess.Add(Work_Bake_3, _bakeDataList.Count);
			_workProcess.Add(Work_Bake_4, 1);

			_workProcess.StartRun("Bake Atlas");
			//_thread = new Thread(new ThreadStart(Thread_Bake));
			//_thread.Start();

			_isImageBaking = true;

			return true;
		}

		private bool Work_Bake_1(int index)
		{
			if (_loadKey_CheckBake == null)
			{
				return false;
			}
			if (_realBakeSizePerIndex <= 0 || _realBakeResizeX100 <= 0)
			{
				return false;
			}
			if (index >= _realBakedAtlasCount)
			{
				Debug.LogError("Work_Bake_1 Exception : Index Over (" + index + " / " + _realBakedAtlasCount + ")");
				return false;
			}

			apPSDBakeData newBakeData = new apPSDBakeData(index, GetBakeSize(_bakeWidth), GetBakeSize(_bakeHeight));
			newBakeData.ReadyToBake();
			_bakeDataList.Add(newBakeData);

			//WorkProcess 갱신
			_workProcess.ChangeCount(2, _bakeDataList.Count);
			return true;

		}

		private bool Work_Bake_2(int index)
		{
			if (_loadKey_CheckBake == null)
			{
				return false;
			}
			if (_realBakeSizePerIndex <= 0 || _realBakeResizeX100 <= 0)
			{
				return false;
			}
			if (index >= _bakeParams.Count)
			{
				Debug.LogError("Work_Bake_2 Exception : Index Over (" + index + " / " + _bakeParams.Count + ")");
				return false;
			}

			float bakeResizeRatio = Mathf.Clamp01(((float)_realBakeResizeX100 / 100.0f));

			LayerBakeParam bakeParam = _bakeParams[index];
			apPSDLayerData targetLayer = bakeParam._targetLayer;
			if (targetLayer._image == null)
			{
				Debug.LogError("Work_Bake_2 : No Image");
				return true;
			}

			//일단 레이어에 Bake 정보를 입력하자
			targetLayer._bakedAtalsIndex = bakeParam._atlasIndex;
			targetLayer._bakedImagePos_Left = bakeParam._posOffset_X * _realBakeSizePerIndex;
			targetLayer._bakedImagePos_Top = bakeParam._posOffset_Y * _realBakeSizePerIndex;
			targetLayer._bakedWidth = (int)((float)targetLayer._width * bakeResizeRatio + 0.5f);
			targetLayer._bakedHeight = (int)((float)targetLayer._height * bakeResizeRatio + 0.5f);

			//Bake Image에 값을 넣자
			apPSDBakeData targetBakeData = _bakeDataList[bakeParam._atlasIndex];
			bool isResult = targetBakeData.AddImage(targetLayer,
														targetLayer._bakedImagePos_Left,
														targetLayer._bakedImagePos_Top,
														bakeResizeRatio,
														targetLayer._bakedWidth,
														targetLayer._bakedHeight,
														_bakePadding);

			//Debug.Log("Bake [AddImage] : " + index + " >> " + bakeParam._atlasIndex);

			return isResult;
		}

		private bool Work_Bake_3(int index)
		{
			if (_loadKey_CheckBake == null)
			{
				return false;
			}
			if (_realBakeSizePerIndex <= 0 || _realBakeResizeX100 <= 0)
			{
				return false;
			}
			if (index >= _bakeDataList.Count)
			{
				Debug.LogError("Work_Bake_3 Exception : Index Over (" + index + " / " + _bakeDataList.Count + ")");
				return false;
			}

			//이제 실제로 Texture2D로 바꾸어주자
			_bakeDataList[index].EndToBake(_bakeBlurOption, _bakePadding);
			//Debug.Log("EndToBake : " + index);
			return true;
		}

		private bool Work_Bake_4(int index)
		{
			_loadKey_Bake = _loadKey_CheckBake;//체크했던 Bake 값이 같음을 설정해주자
			_resultAtlasCount = _realBakedAtlasCount;
			_resultBakeResizeX100 = _realBakeResizeX100;
			_resultPadding = _bakePadding;
			return true;
		}

		//private void Thread_Bake()
		//{
		//	Thread.Sleep(10);

		//	_selectedBakeData = null;
		//	_threadProcessX100 = 0;

		//	//일단 Atlas를 만들어주자 - 이건 메인 스레드에서 해야한다.
		//	//이 과정이 전체의 10%
		//	for (int i = 0; i < _realBakedAtlasCount; i++)
		//	{
		//		apPSDBakeData newBakeData = new apPSDBakeData(i, GetBakeSize(_bakeWidth), GetBakeSize(_bakeHeight));
		//		newBakeData.ReadyToBake();
		//		_bakeDataList.Add(newBakeData);
		//	}

		//	try
		//	{

		//		float bakeResizeRatio = Mathf.Clamp01(((float)_realBakeResizeX100 / 100.0f));
		//		for (int i = 0; i < _bakeParams.Count; i++)
		//		{
		//			LayerBakeParam bakeParam = _bakeParams[i];
		//			apPSDLayerData targetLayer = bakeParam._targetLayer;
		//			if (targetLayer._image == null)
		//			{
		//				continue;
		//			}

		//			//일단 레이어에 Bake 정보를 입력하자
		//			targetLayer._bakedAtalsIndex = bakeParam._atlasIndex;
		//			targetLayer._bakedImagePos_Left = bakeParam._posOffset_X * _realBakeSizePerIndex;
		//			targetLayer._bakedImagePos_Top = bakeParam._posOffset_Y * _realBakeSizePerIndex;
		//			targetLayer._bakedWidth = (int)((float)targetLayer._width * bakeResizeRatio + 0.5f);
		//			targetLayer._bakedHeight = (int)((float)targetLayer._height * bakeResizeRatio + 0.5f);

		//			//Bake Image에 값을 넣자
		//			apPSDBakeData targetBakeData = _bakeDataList[bakeParam._atlasIndex];
		//			bool isResult = targetBakeData.AddImage(targetLayer,
		//														targetLayer._bakedImagePos_Left,
		//														targetLayer._bakedImagePos_Top,
		//														bakeResizeRatio,
		//														targetLayer._bakedWidth,
		//														targetLayer._bakedHeight,
		//														_bakePadding);
		//			if (!isResult)
		//			{
		//				_isThreadProcess = false;
		//				_isThreadProcessSuccess = false;//실패함
		//				return;
		//			}

		//			Thread.Sleep(100);
		//			_threadProcessX100 = (int)((float)i / (float)_bakeParams.Count) * 70;
		//		}

		//		Thread.Sleep(100);
		//		_threadProcessX100 = 70;

		//		for (int i = 0; i < _bakeDataList.Count; i++)
		//		{
		//			//이제 실제로 Texture2D로 바꾸어주자
		//			_bakeDataList[i].EndToBake(_bakeBlurOption, _bakePadding);

		//			Thread.Sleep(100);
		//			_threadProcessX100 = 70 + (int)((float)i / (float)_bakeParams.Count) * 30;
		//		}

		//		Thread.Sleep(100);

		//		_loadKey_Bake = _loadKey_CheckBake;//체크했던 Bake 값이 같음을 설정해주자
		//		_resultAtlasCount = _realBakedAtlasCount;
		//		_resultBakeResizeX100 = _realBakeResizeX100;
		//		_resultPadding = _bakePadding;
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.LogError("Bake Exception : " + ex);
		//		_isThreadProcess = false;
		//		_isThreadProcessSuccess = false;
		//		throw;
		//	}


		//	_isThreadProcess = false;
		//	_isThreadProcessSuccess = true;//<<성공함
		//	_threadProcessX100 = 100;

		//}


		private bool SaveBakedImages()
		{
			if (_bakeDataList.Count == 0)
			{
				return false;
			}
			if (_loadKey_Bake == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(_bakeDstFilePath) || string.IsNullOrEmpty(_fileNameOnly) || string.IsNullOrEmpty(_bakeDstFileRelativePath))
			{
				return false;
			}


			try
			{
				for (int i = 0; i < _bakeDataList.Count; i++)
				{
					apPSDBakeData curBakeData = _bakeDataList[i];
					byte[] data = curBakeData._bakedImage.EncodeToPNG();
					//F:/MainWorks/UnityProjects/AnyPortrait/AnyPortrait/Assets/Sample
					string path = _bakeDstFilePath + "/" + _fileNameOnly + "_" + i + ".png";
					string relPath = _bakeDstFileRelativePath + "/" + _fileNameOnly + "_" + i + ".png";

					for (int iLayer = 0; iLayer < curBakeData._bakedLayerData.Count; iLayer++)
					{
						curBakeData._bakedLayerData[iLayer]._textureAssetPath = relPath;
						curBakeData._bakedLayerData[iLayer]._bakedData = curBakeData;
					}

					File.WriteAllBytes(path, data);

					AssetDatabase.SaveAssets();

					AssetDatabase.Refresh();

					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

					Debug.Log("Save And Reimport : " + relPath);



					//AssetDatabase.ImportAsset(relPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
					AssetDatabase.ImportAsset(relPath);

					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

					TextureImporter ti = TextureImporter.GetAtPath(relPath) as TextureImporter;
					if (ti == null)
					{
						Debug.LogError("Bake Error : Path : " + relPath);
					}
					else
					{
						TextureImporterSettings tiSetting = new TextureImporterSettings();
						tiSetting.filterMode = FilterMode.Bilinear;
						tiSetting.mipmapEnabled = false;
						//tiSetting.textureFormat = TextureImporterFormat.RGBA32;//Deprecated
						tiSetting.wrapMode = TextureWrapMode.Clamp;
						tiSetting.alphaSource = TextureImporterAlphaSource.FromInput;


						//tipSetting.


						ti.SetTextureSettings(tiSetting);
						ti.maxTextureSize = 4096;
						TextureImporterPlatformSettings tipSet = ti.GetDefaultPlatformTextureSettings();

						//TextureImporterFormat prevFormat = tipSet.format;

						//tipSet.compressionQuality = 
						//tipSet.format = TextureImporterFormat.RGBA32;


						//ti.SetPlatformTextureSettings()

						ti.SaveAndReimport();
						AssetDatabase.ImportAsset(relPath);

						//tipSet.format = prevFormat;
					}
				}
				AssetDatabase.Refresh();


			}
			catch (Exception ex)
			{
				Debug.LogError("SaveBakedImages Exception : " + ex);
				return false;
			}


			return true;
		}



		// Thread
		//-------------------------------------------------------------------
		private void StartBakedImageSave()
		{
			CloseThread();

			//Debug.Log("Start Baked Image Save");
			//_threadProcessX100 = 0;
			//_isThreadProcess = true;
			//_isSaveImageThreadStart = true;
			//_threadWorkType = THREAD_WORK_TYPE.SaveImageToEditor;
			//_isThreadProcessSuccess = false;
			_threadProcessName = "Convert PSD Data To Editor..";
			//_thread = new Thread(new ThreadStart(Thread_BakedImageSave));
			//_thread.Start();

			_workProcess.Add(Work_BakedImageSave_1, _bakeDataList.Count);
			_workProcess.Add(Work_BakdImageSave_2, 1);
			_workProcess.StartRun("Convert PSD Data To Editor");
		}

		private bool Work_BakedImageSave_1(int index)
		{
			if (_bakeDataList.Count == 0 || _loadKey_Bake == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(_bakeDstFilePath) || string.IsNullOrEmpty(_fileNameOnly) || string.IsNullOrEmpty(_bakeDstFileRelativePath))
			{
				return false;
			}
			if (index >= _bakeDataList.Count)
			{
				Debug.LogError("Work BakedImageSave - 1 : Index Over (" + index + " / " + _bakeDataList.Count + ")");
				return false;
			}

			SaveBakeImage(index);
			ReimportBakedImage(index);

			return true;
		}

		private bool Work_BakdImageSave_2(int index)
		{
			OnLoadComplete(true);
			return true;
		}

		//private void Thread_BakedImageSave()
		//{
		//	Thread.Sleep(100);

		//	if(_bakeDataList.Count == 0 || _loadKey_Bake == null)
		//	{
		//		_isThreadProcess = false;
		//		_isThreadProcessSuccess = false;
		//		return;
		//	}
		//	if(string.IsNullOrEmpty(_bakeDstFilePath) || string.IsNullOrEmpty(_fileNameOnly) || string.IsNullOrEmpty(_bakeDstFileRelativePath))
		//	{
		//		_isThreadProcess = false;
		//		_isThreadProcessSuccess = false;
		//		return;
		//	}


		//	try
		//	{
		//		int nBakeDataList = _bakeDataList.Count;
		//		float processRatioPerItem = 1.0f / (float)nBakeDataList;

		//		for (int i = 0; i < _bakeDataList.Count; i++)
		//		{



		//			_iRequestSaveBakeDataList = -1;
		//			_isRequestSaveBakeDataList = false;
		//			_isRequestSaveBakeDataListComplete = false;

		//			_iRequestReimportImage = -1;
		//			_isRequestReimportImage = false;
		//			_isRequestReimportImageComplete = false;

		//			Thread.Sleep(10);

		//			_threadProcessX100 = (int)(((i + 0.1f) * processRatioPerItem) * 100.0f + 0.5f);

		//			//Save를 요청하자
		//			_iRequestSaveBakeDataList = i;
		//			_isRequestSaveBakeDataList = true;
		//			_isRequestSaveBakeDataListComplete = false;

		//			Thread.Sleep(10);

		//			//Save를 요청하자
		//			while(true)
		//			{
		//				//끝났는지 체크
		//				if(_isRequestSaveBakeDataListComplete)
		//				{
		//					break;
		//				}
		//				Thread.Sleep(50);
		//			}



		//			for (int iSubLoad = 0; iSubLoad < 5; iSubLoad++)
		//			{
		//				Thread.Sleep(200);
		//				_threadProcessX100 = (int)(((i + 0.2f + (iSubLoad * 0.1f)) * processRatioPerItem) * 100.0f + 0.5f);
		//			}

		//			////Save는 끝났고
		//			//_iRequestSaveBakeDataList = -1;
		//			//_isRequestSaveBakeDataList = false;
		//			//_isRequestSaveBakeDataListComplete = false;


		//			////Reimport 요청
		//			//_iRequestReimportImage = i;
		//			//_isRequestReimportImage = true;
		//			//_isRequestReimportImageComplete = false;



		//			Thread.Sleep(1000);
		//			_threadProcessX100 = (int)(((i + 0.8f) * processRatioPerItem) * 100.0f + 0.5f);


		//			Thread.Sleep(10);
		//			while(true)
		//			{
		//				if(_isRequestReimportImageComplete)
		//				{
		//					break;
		//				}

		//				Thread.Sleep(50);
		//			}


		//			Thread.Sleep(200);
		//			_threadProcessX100 = (int)(((i + 0.9f) * processRatioPerItem) * 100.0f + 0.5f);
		//		}
		//		_threadProcessX100 = 100;
		//		Thread.Sleep(200);
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.LogError("SaveBakedImages Exception : " + ex);
		//		_isThreadProcess = false;
		//		_isThreadProcessSuccess = false;
		//		return;
		//	}

		//	_isThreadProcess = false;
		//	_isThreadProcessSuccess = true;//<!
		//	return;
		//}


		private void SaveBakeImage(int iBakeDataList)
		{
			try
			{
				apPSDBakeData curBakeData = _bakeDataList[iBakeDataList];
				byte[] data = curBakeData._bakedImage.EncodeToPNG();

				//F:/MainWorks/UnityProjects/AnyPortrait/AnyPortrait/Assets/Sample
				string path = _bakeDstFilePath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";
				string relPath = _bakeDstFileRelativePath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";

				for (int iLayer = 0; iLayer < curBakeData._bakedLayerData.Count; iLayer++)
				{
					curBakeData._bakedLayerData[iLayer]._textureAssetPath = relPath;
					curBakeData._bakedLayerData[iLayer]._bakedData = curBakeData;
				}

				File.WriteAllBytes(path, data);

				//AssetDatabase.CreateAsset(curBakeData._bakedImage, relPath);
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			}
			catch (Exception ex)
			{
				return;
			}
		}

		private void ReimportBakedImage(int iBakeDataList)
		{
			try
			{
				string path = _bakeDstFilePath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";
				string relPath = _bakeDstFileRelativePath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";

				AssetDatabase.SaveAssets();

				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);


				//-------------------------------------------------------------------
				// Unity 5.5부터는 TextureImporter를 호출하기 전에
				// AssetDatabase에서 한번 열어서 Apply를 해줘야 한다.
				//-------------------------------------------------------------------
				Texture2D tex2D = AssetDatabase.LoadAssetAtPath<Texture2D>(relPath);
				if (tex2D != null)
				{
					tex2D.Apply();
				}
				//-------------------------------------------------------------------

				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);


				TextureImporter ti = TextureImporter.GetAtPath(relPath) as TextureImporter;

				if (ti == null)
				{
					Debug.LogError("Bake Error : Path : " + relPath);
				}
				else
				{
					TextureImporterSettings tiSetting = new TextureImporterSettings();
					tiSetting.filterMode = FilterMode.Bilinear;
					tiSetting.mipmapEnabled = false;
					//tiSetting.textureFormat = TextureImporterFormat.RGBA32;//Deprecated
					tiSetting.wrapMode = TextureWrapMode.Clamp;
					tiSetting.alphaSource = TextureImporterAlphaSource.FromInput;

					ti.SetTextureSettings(tiSetting);
					ti.maxTextureSize = 4096;

					ti.SaveAndReimport();
				}

			}
			catch (Exception ex)
			{

			}
		}

		// Return Event
		//----------------------------------------------------------


		public void OnLoadComplete(bool isResult)
		{
			if (_funcResult != null)
			{
				if (isResult)
				{
					_funcResult(isResult, _loadKey, _fileNameOnly, _layerDataList, (float)_resultBakeResizeX100 / 100.0f, _imageWidth, _imageHeight, _resultPadding);
				}
				else
				{
					_funcResult(isResult, _loadKey, _fileNameOnly, null, (float)_resultBakeResizeX100 / 100.0f, _imageWidth, _imageHeight, _resultPadding);
				}
			}
			CloseDialog();
		}

		public void MoveStep(bool isMoveNext)
		{
			LOAD_STEP nextStep = _step;
			switch (_step)
			{
				case LOAD_STEP.Step1_FileLoad:
					if (!isMoveNext)
					{ nextStep = LOAD_STEP.Step1_FileLoad; }
					else
					{ nextStep = LOAD_STEP.Step2_LayerCheck; }
					break;

				case LOAD_STEP.Step2_LayerCheck:
					if (!isMoveNext)
					{ nextStep = LOAD_STEP.Step1_FileLoad; }
					else
					{ nextStep = LOAD_STEP.Step3_AtlasSetting; }
					break;

				case LOAD_STEP.Step3_AtlasSetting:
					if (!isMoveNext)
					{ nextStep = LOAD_STEP.Step2_LayerCheck; }
					else
					{ nextStep = LOAD_STEP.Step3_AtlasSetting; }
					break;
			}

			if (isMoveNext)
			{
				if (nextStep == LOAD_STEP.Step2_LayerCheck)
				{

				}
				else if (nextStep == LOAD_STEP.Step3_AtlasSetting)
				{

				}
			}

			_step = nextStep;
		}



	}

}
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
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{

	public class apDialog_CaptureScreen : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_CaptureScreen s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		private object _loadKey = null;

		private Vector2 _scroll = Vector2.zero;

		private apAnimClip _selectedAnimClip = null;
		private apRootUnit _curRootUnit = null;
		private List<apAnimClip> _animClips = new List<apAnimClip>();

		private enum EXPORT_TYPE
		{
			None,
			Thumbnail,
			PNG,
			GIFAnimation
		}
		private EXPORT_TYPE _exportRequestType = EXPORT_TYPE.None;
		private EXPORT_TYPE _exportProcessType = EXPORT_TYPE.None;
		private int _exportProcessX100 = -1;
		private int _iProcess = 0;
		private int _iProcessCount = 0;

		private bool IsGUIUsable { get { return _exportProcessType == EXPORT_TYPE.None; } }

		private string _prevFilePath = "";
		private string _prevFilePath_Directory = "";

		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait)
		{
			//Debug.Log("Show Dialog - Portrait Setting");
			CloseDialog();


			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_CaptureScreen), true, "Capture", true);
			apDialog_CaptureScreen curTool = curWindow as apDialog_CaptureScreen;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 700;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);


				s_window.Init(editor, portrait, loadKey);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		public static void CloseDialog()
		{
			if (s_window != null)
			{
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

		// Init
		//------------------------------------------------------------------
		public void Init(apEditor editor, apPortrait portrait, object loadKey)
		{
			_editor = editor;
			_loadKey = loadKey;
			_targetPortrait = portrait;
			_selectedAnimClip = null;

			_exportRequestType = EXPORT_TYPE.None;
			_exportProcessType = EXPORT_TYPE.None;
			_exportProcessX100 = 0;
		}

		// Update
		//------------------------------------------------------------------
		void Update()
		{
			if (Application.isPlaying)
			{
				return;
			}

			//Debug.Log("Update");
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetPortrait == null)
			{
				Debug.LogError("Exit - Editor / Portrait is Null");
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자 + Overall Menu가 아니라면..
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait || _editor.Select.SelectionType != apSelection.SELECTION_TYPE.Overall)
			{
				Debug.LogError("Exit - Editor / Portrait Missmatch");
				CloseDialog();
				return;
			}

			//여기서 체크 및 실행하자
			//Request => Process => Process 처리
			if (_exportProcessType == EXPORT_TYPE.None && _exportRequestType != EXPORT_TYPE.None)
			{
				_iProcess = 0;
				switch (_exportRequestType)
				{
					case EXPORT_TYPE.None:
						break;

					case EXPORT_TYPE.Thumbnail:
						_exportProcessType = EXPORT_TYPE.Thumbnail;
						break;
					case EXPORT_TYPE.PNG:
						_exportProcessType = EXPORT_TYPE.PNG;
						break;
					case EXPORT_TYPE.GIFAnimation:
						_exportProcessType = EXPORT_TYPE.GIFAnimation;
						break;
				}

				_exportRequestType = EXPORT_TYPE.None;

			}
			switch (_exportProcessType)
			{
				case EXPORT_TYPE.None:
					break;
				case EXPORT_TYPE.Thumbnail:
					Process_MakeThumbnail();
					break;
				case EXPORT_TYPE.PNG:
					Process_PNGScreenShot();
					break;
				case EXPORT_TYPE.GIFAnimation:
					Process_MakeGIF();
					break;
			}

			_scroll = EditorGUILayout.BeginScrollView(_scroll, false, true, GUILayout.Width(width), GUILayout.Height(height));
			width -= 24;
			EditorGUILayout.BeginVertical(GUILayout.Width(width));

			int settingWidth = ((width - 10) / 3) - 4;
			int settingWidth_Label = 50;
			int settingWidth_Value = settingWidth - (50 + 8);
			int settingHeight = 70;
			EditorGUILayout.LabelField("Setting");
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(settingHeight));
			GUILayout.Space(5);

			//Position
			//------------------------
			EditorGUILayout.BeginVertical(GUILayout.Width(settingWidth), GUILayout.Height(settingHeight));


			EditorGUILayout.LabelField("Position", GUILayout.Width(settingWidth));
			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			EditorGUILayout.LabelField("X", GUILayout.Width(settingWidth_Label));
			int posX = EditorGUILayout.DelayedIntField(_editor._captureFrame_PosX, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			EditorGUILayout.LabelField("Y", GUILayout.Width(settingWidth_Label));
			int posY = EditorGUILayout.DelayedIntField(_editor._captureFrame_PosY, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
			//------------------------


			//Capture Size
			//------------------------
			EditorGUILayout.BeginVertical(GUILayout.Width(settingWidth), GUILayout.Height(settingHeight));

			EditorGUILayout.LabelField("Capture Size", GUILayout.Width(settingWidth));
			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			EditorGUILayout.LabelField("Width", GUILayout.Width(settingWidth_Label));
			int srcSizeWidth = EditorGUILayout.DelayedIntField(_editor._captureFrame_SrcWidth, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			EditorGUILayout.LabelField("Height", GUILayout.Width(settingWidth_Label));
			int srcSizeHeight = EditorGUILayout.DelayedIntField(_editor._captureFrame_SrcHeight, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();


			if (srcSizeWidth < 8)
			{ srcSizeWidth = 8; }
			if (srcSizeHeight < 8)
			{ srcSizeHeight = 8; }

			EditorGUILayout.EndVertical();


			//------------------------

			//File Size
			//-------------------------------
			EditorGUILayout.BeginVertical(GUILayout.Width(settingWidth), GUILayout.Height(settingHeight));

			EditorGUILayout.LabelField("File Size", GUILayout.Width(settingWidth));

			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			EditorGUILayout.LabelField("Width", GUILayout.Width(settingWidth_Label));
			int dstSizeWidth = EditorGUILayout.DelayedIntField(_editor._captureFrame_DstWidth, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			EditorGUILayout.LabelField("Height", GUILayout.Width(settingWidth_Label));
			int dstSizeHeight = EditorGUILayout.DelayedIntField(_editor._captureFrame_DstHeight, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();

			if (dstSizeWidth < 8)
			{ dstSizeWidth = 8; }
			if (dstSizeHeight < 8)
			{ dstSizeHeight = 8; }



			EditorGUILayout.EndVertical();
			//-------------------------------

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			int setting2CompWidth = ((width - 10) / 2) - 8;

			//Color와 AspectRatio
			EditorGUILayout.LabelField("BG Color", GUILayout.Width(80));
			Color prevCaptureColor = _editor._captureFrame_Color;
			try
			{
				_editor._captureFrame_Color = EditorGUILayout.ColorField(_editor._captureFrame_Color, GUILayout.Width(setting2CompWidth - 86));
			}
			catch (Exception) { }


			GUILayout.Space(30);
			if (apEditorUtil.ToggledButton_2Side("Aspect Ratio Fixed", "Aspect Ratio Not Fixed", _editor._isCaptureAspectRatioFixed, true, setting2CompWidth - 20, 20))
			{
				_editor._isCaptureAspectRatioFixed = !_editor._isCaptureAspectRatioFixed;

				if (_editor._isCaptureAspectRatioFixed)
				{
					//AspectRatio를 굳혔다.
					//Dst계열 변수를 Src에 맞춘다.
					//Height를 고정, Width를 맞춘다.
					_editor._captureFrame_DstWidth = GetAspectRatio_Width(_editor._captureFrame_DstHeight, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
					dstSizeWidth = _editor._captureFrame_DstWidth;
				}

				_editor.SaveEditorPref();


				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();


			//AspectRatio를 맞추어보자
			if (_editor._isCaptureAspectRatioFixed)
			{
				if (srcSizeWidth != _editor._captureFrame_SrcWidth)
				{
					//Width가 바뀌었다. => Height를 맞추자
					srcSizeHeight = GetAspectRatio_Height(srcSizeWidth, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
					//>> Dst도 바꾸자 => Width
					dstSizeWidth = GetAspectRatio_Width(dstSizeHeight, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
				}
				else if (srcSizeHeight != _editor._captureFrame_SrcHeight)
				{
					//Height가 바뀌었다. => Width를 맞추자
					srcSizeWidth = GetAspectRatio_Width(srcSizeHeight, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
					//>> Dst도 바꾸자 => Height
					dstSizeHeight = GetAspectRatio_Height(dstSizeWidth, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
				}
				else if (dstSizeWidth != _editor._captureFrame_DstWidth)
				{
					//Width가 바뀌었다. => Height를 맞추자
					dstSizeHeight = GetAspectRatio_Height(dstSizeWidth, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);
					//>> Src도 바꾸다 => Width
					srcSizeWidth = GetAspectRatio_Width(srcSizeHeight, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);
				}
				else if (dstSizeHeight != _editor._captureFrame_DstHeight)
				{
					//Height가 바뀌었다. => Width를 맞추자
					dstSizeWidth = GetAspectRatio_Width(dstSizeHeight, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);
					//>> Dst도 바꾸자 => Height
					srcSizeHeight = GetAspectRatio_Height(srcSizeWidth, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);
				}
			}

			if (posX != _editor._captureFrame_PosX
				|| posY != _editor._captureFrame_PosY
				|| srcSizeWidth != _editor._captureFrame_SrcWidth
				|| srcSizeHeight != _editor._captureFrame_SrcHeight
				|| dstSizeWidth != _editor._captureFrame_DstWidth
				|| dstSizeHeight != _editor._captureFrame_DstHeight
				)
			{
				_editor._captureFrame_PosX = posX;
				_editor._captureFrame_PosY = posY;
				_editor._captureFrame_SrcWidth = srcSizeWidth;
				_editor._captureFrame_SrcHeight = srcSizeHeight;
				_editor._captureFrame_DstWidth = dstSizeWidth;
				_editor._captureFrame_DstHeight = dstSizeHeight;

				_editor.SaveEditorPref();
				apEditorUtil.ReleaseGUIFocus();
			}

			if (prevCaptureColor.r != _editor._captureFrame_Color.r
				|| prevCaptureColor.g != _editor._captureFrame_Color.g
				|| prevCaptureColor.b != _editor._captureFrame_Color.b)
			{
				_editor.SaveEditorPref();
				//색상은 GUIFocus를 null로 만들면 안되기에..
			}



			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			EditorGUILayout.LabelField("Thumbnail Capture");
			GUILayout.Space(5);

			EditorGUILayout.LabelField("File Path");
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(5);
			GUILayout.Box(_editor._portrait._thumbnailImage, GUI.skin.label, GUILayout.Width(50), GUILayout.Height(25));
			_editor._portrait._imageFilePath_Thumbnail = EditorGUILayout.TextField(_editor._portrait._imageFilePath_Thumbnail, GUILayout.Width(width - (130)));
			if (GUILayout.Button("Change", GUILayout.Width(60)))
			{
				string fileName = EditorUtility.SaveFilePanelInProject("Thumbnail File Path", _editor._portrait.name + "_Thumb.png", "png", "Please Enter a file name to save Thumbnail to");
				if (!string.IsNullOrEmpty(fileName))
				{
					_editor._portrait._imageFilePath_Thumbnail = fileName;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("Make Thumbnail", GUILayout.Width(width), GUILayout.Height(30)))
			{
				if (string.IsNullOrEmpty(_editor._portrait._imageFilePath_Thumbnail))
				{
					//EditorUtility.DisplayDialog("Thumbnail Creating Failed", "File Name is Empty", "Close");
					EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.ThumbCreateFailed_Title),
													_editor.GetText(apLocalization.TEXT.ThumbCreateFailed_Body_NoFile),
													_editor.GetText(apLocalization.TEXT.Close)
													);
				}
				else
				{
					RequestExport(EXPORT_TYPE.Thumbnail);
					#region [비동기 스타일로 변경]
					//int thumbnailWidth = 256;
					//int thumbnailHeight = 128;

					//float preferAspectRatio = (float)thumbnailWidth / (float)thumbnailHeight;

					//float srcAspectRatio = (float)_editor._captureFrame_SrcWidth / (float)_editor._captureFrame_SrcHeight;
					////긴쪽으로 캡쳐 크기를 맞춘다.
					//int srcThumbWidth = _editor._captureFrame_SrcWidth;
					//int srcThumbHeight = _editor._captureFrame_SrcHeight;
					////AspectRatio = W / H
					//if(srcAspectRatio < preferAspectRatio)
					//{
					//	//가로가 더 길군요.
					//	//가로를 자릅시다.

					//	//H = W / AspectRatio;
					//	srcThumbHeight = (int)((srcThumbWidth / preferAspectRatio) + 0.5f);
					//}
					//else
					//{
					//	//세로가 더 길군요.
					//	//세로를 자릅시다.
					//	//W = AspectRatio * H
					//	srcThumbWidth = (int)((srcThumbHeight * preferAspectRatio) + 0.5f);
					//}


					//Texture2D result = _editor.Exporter.RenderToTexture(_editor.Select.RootUnit._childMeshGroup,
					//												(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), (int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
					//												srcThumbWidth, srcThumbHeight,
					//												thumbnailWidth, thumbnailHeight,
					//												_editor._captureFrame_Color
					//												);

					//if (result != null)
					//{
					//	//이미지를 저장하자
					//	//이건 Asset으로 자동 저장
					//	string filePathWOExtension = _editor._portrait._imageFilePath_Thumbnail.Substring(0, _editor._portrait._imageFilePath_Thumbnail.Length - 4);
					//	bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(result, filePathWOExtension, true);

					//	if(isSaveSuccess)
					//	{
					//		AssetDatabase.Refresh();

					//		_editor._portrait._thumbnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(_editor._portrait._imageFilePath_Thumbnail);
					//	}
					//} 
					#endregion
				}
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//Screenshot을 찍자
			//-----------------------------------------------------------------------------------------------------------------------
			//-----------------------------------------------------------------------------------------------------------------------
			EditorGUILayout.LabelField("Screenshot Capture");
			GUILayout.Space(5);
			if (GUILayout.Button("Take a Screenshot", GUILayout.Width(width), GUILayout.Height(30)))
			{
				RequestExport(EXPORT_TYPE.PNG);
			}

			//-----------------------------------------------------------------------------------------------------------------------
			//-----------------------------------------------------------------------------------------------------------------------

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);


			//GIF Animation을 만들자
			//-----------------------------------------------------------------------------------------------------------------------
			//-----------------------------------------------------------------------------------------------------------------------
			EditorGUILayout.LabelField("GIF Animation");
			GUILayout.Space(5);

			apRootUnit curRootUnit = _editor.Select.RootUnit;

			if (_curRootUnit != curRootUnit)
			{
				//AnimList 리셋
				_animClips.Clear();

				_curRootUnit = curRootUnit;
				if (_curRootUnit != null)
				{
					for (int i = 0; i < _editor._portrait._animClips.Count; i++)
					{
						apAnimClip animClip = _editor._portrait._animClips[i];
						if (animClip._targetMeshGroup == _curRootUnit._childMeshGroup)
						{
							_animClips.Add(animClip);
						}
					}
				}

				_selectedAnimClip = null;
			}
			if (curRootUnit == null)
			{
				_selectedAnimClip = null;
			}
			else
			{
				if (_selectedAnimClip != null && _animClips.Count > 0)
				{
					if (!_animClips.Contains(_selectedAnimClip))
					{
						_selectedAnimClip = null;
					}
				}
				else
				{
					_selectedAnimClip = null;
				}
			}

			string animName = "< Animation is not selected >";
			Color animBGColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
			if (_selectedAnimClip != null)
			{
				animName = _selectedAnimClip._name;
				animBGColor = new Color(0.7f, 1.0f, 0.7f, 1.0f);
			}

			Color prevGUIColor = GUI.backgroundColor;
			GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
			guiStyleBox.alignment = TextAnchor.MiddleCenter;

			GUI.backgroundColor = animBGColor;

			GUILayout.Box(animName, guiStyleBox, GUILayout.Width(width), GUILayout.Height(30));

			GUI.backgroundColor = prevGUIColor;

			GUILayout.Space(5);
			int width_GIFSetting = (width - 32) / 2;

			int gifQuality = 256 - _editor._captureFrame_GIFSampleQuality;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			string strQuality = "";
			if (gifQuality > 200)
			{
				strQuality = "Quality [ High ]";
			}
			else if (gifQuality > 120)
			{
				strQuality = "Quality [ Medium ]";
			}
			else
			{
				strQuality = "Quality [ Low ]";
			}
			EditorGUILayout.LabelField(strQuality, GUILayout.Width(width_GIFSetting));
			GUILayout.Space(20);

			EditorGUILayout.LabelField("Loop Count", GUILayout.Width(width_GIFSetting));

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			//10 ~ 256
			//246 ~ 0
			gifQuality = EditorGUILayout.IntSlider(gifQuality, 0, 246, GUILayout.Width(width_GIFSetting));

			gifQuality = 256 - gifQuality;
			if (_editor._captureFrame_GIFSampleQuality != gifQuality)
			{
				_editor._captureFrame_GIFSampleQuality = gifQuality;
				_editor.SaveEditorPref();
			}

			GUILayout.Space(20);

			int loopCount = EditorGUILayout.DelayedIntField(_editor._captureFrame_GIFSampleLoopCount, GUILayout.Width(width_GIFSetting));

			if (loopCount != _editor._captureFrame_GIFSampleLoopCount)
			{
				loopCount = Mathf.Clamp(loopCount, 1, 10);
				_editor._captureFrame_GIFSampleLoopCount = loopCount;
				_editor.SaveEditorPref();
			}

			EditorGUILayout.EndHorizontal();

			//GUILayout.Space(10);

			//Rect lastRect_Progress = GUILayoutUtility.GetLastRect();

			//Rect barRect = new Rect(lastRect_Progress.x + 5, lastRect_Progress.y + 10, width - 5, 16);
			//float barRatio = 0.0f;
			//string strProcessName = "";
			//if(_exportProcessType == EXPORT_TYPE.GIFAnimation)
			//{
			//	barRatio = Mathf.Clamp01((float)_exportProcessX100 / 100.0f);
			//	strProcessName = "Exporting.. [ " + _exportProcessX100 + "% ]";
			//}

			////EditorGUI.ProgressBar(barRect, barRatio, "Convert PSD Data To Editor..");

			//EditorGUI.ProgressBar(barRect, barRatio, strProcessName);
			//GUILayout.Space(20);


			if (apEditorUtil.ToggledButton_2Side("Take a GIF Animation", "Take a GIF Animation", false, (_selectedAnimClip != null), width, 30))
			{
				//RequestExport(EXPORT_TYPE.GIFAnimation);//리퀘스트 안할래..
				string defFileName = "GIF_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".gif";
				string saveFilePath = EditorUtility.SaveFilePanel("Save GIF Animation", _prevFilePath_Directory, defFileName, "gif");
				if (!string.IsNullOrEmpty(saveFilePath))
				{
					bool result = _editor.Exporter.MakeGIFAnimation(saveFilePath,
																		_editor.Select.RootUnit._childMeshGroup,
																		_selectedAnimClip, _editor._captureFrame_GIFSampleLoopCount,
																		(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), (int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
																		_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
																		_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
																		_editor._captureFrame_Color,
																		_editor._captureFrame_GIFSampleQuality
																	);
					if (result)
					{
						System.IO.FileInfo fi = new System.IO.FileInfo(saveFilePath);

						Application.OpenURL("file://" + fi.Directory.FullName);
						Application.OpenURL("file://" + saveFilePath);

						_prevFilePath = _editor.Exporter.GIF_FilePath;
						_prevFilePath_Directory = fi.Directory.FullName;
					}
				}
			}

			GUILayout.Space(10);

			GUIStyle guiStyle = new GUIStyle(GUIStyle.none);
			guiStyle.normal.textColor = GUI.skin.label.normal.textColor;

			GUILayout.Button("Animation Clips", guiStyle, GUILayout.Width(width), GUILayout.Height(20));//투명 버튼


			//애니메이션 클립 리스트를 만들어야 한다.
			if (_animClips.Count > 0)
			{

				Texture2D iconImage = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

				apAnimClip nextSelectedAnimClip = null;
				for (int i = 0; i < _animClips.Count; i++)
				{
					apAnimClip animClip = _animClips[i];

					if (animClip == _selectedAnimClip)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						prevCaptureColor = GUI.backgroundColor;

						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

						GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
						GUI.backgroundColor = prevGUIColor;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
					GUILayout.Space(15);
					if (GUILayout.Button(new GUIContent(" " + animClip._name, iconImage), guiStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
					{
						nextSelectedAnimClip = animClip;
					}

					EditorGUILayout.EndHorizontal();
				}

				if (nextSelectedAnimClip != null)
				{
					for (int i = 0; i < _editor._portrait._animClips.Count; i++)
					{
						_editor._portrait._animClips[i]._isSelectedInEditor = false;
					}

					nextSelectedAnimClip.LinkEditor(_editor._portrait);
					nextSelectedAnimClip.RefreshTimelines();
					nextSelectedAnimClip.SetFrame_Editor(nextSelectedAnimClip.StartFrame);
					nextSelectedAnimClip.Pause_Editor();
					nextSelectedAnimClip._isSelectedInEditor = true;
					_selectedAnimClip = nextSelectedAnimClip;

					_editor._portrait._animPlayManager.SetAnimClip_Editor(_selectedAnimClip);
				}
			}

			EditorGUILayout.EndVertical();
			GUILayout.Space(500);

			EditorGUILayout.EndScrollView();


			//-----------------------------------------------------------------------------------------------------------------------
			//-----------------------------------------------------------------------------------------------------------------------
		}


		private int GetAspectRatio_Height(int srcWidth, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect
			//H = W / Aspect <<

			return (int)(((float)srcWidth / targetAspectRatio) + 0.5f);
		}

		private int GetAspectRatio_Width(int srcHeight, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect <<
			//H = W / Aspect

			return (int)(((float)srcHeight * targetAspectRatio) + 0.5f);
		}


		//-------------------------------------------------------------------------------
		private bool RequestExport(EXPORT_TYPE exportType)
		{
			if (_exportProcessType != EXPORT_TYPE.None || _exportRequestType != EXPORT_TYPE.None)
			{
				return false;
			}

			_exportRequestType = exportType;
			_exportProcessType = EXPORT_TYPE.None;
			return true;
		}

		private void Process_MakeThumbnail()
		{
			//if (curEvent.type == EventType.Repaint)
			//{
			//	return;
			//}

			try
			{
				int thumbnailWidth = 256;
				int thumbnailHeight = 128;

				float preferAspectRatio = (float)thumbnailWidth / (float)thumbnailHeight;

				float srcAspectRatio = (float)_editor._captureFrame_SrcWidth / (float)_editor._captureFrame_SrcHeight;
				//긴쪽으로 캡쳐 크기를 맞춘다.
				int srcThumbWidth = _editor._captureFrame_SrcWidth;
				int srcThumbHeight = _editor._captureFrame_SrcHeight;
				//AspectRatio = W / H
				if (srcAspectRatio < preferAspectRatio)
				{
					//가로가 더 길군요.
					//가로를 자릅시다.

					//H = W / AspectRatio;
					srcThumbHeight = (int)((srcThumbWidth / preferAspectRatio) + 0.5f);
				}
				else
				{
					//세로가 더 길군요.
					//세로를 자릅시다.
					//W = AspectRatio * H
					srcThumbWidth = (int)((srcThumbHeight * preferAspectRatio) + 0.5f);
				}


				Texture2D result = _editor.Exporter.RenderToTexture(_editor.Select.RootUnit._childMeshGroup,
																(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), (int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
																srcThumbWidth, srcThumbHeight,
																thumbnailWidth, thumbnailHeight,
																_editor._captureFrame_Color
																);

				if (result != null)
				{
					//이미지를 저장하자
					//이건 Asset으로 자동 저장
					string filePathWOExtension = _editor._portrait._imageFilePath_Thumbnail.Substring(0, _editor._portrait._imageFilePath_Thumbnail.Length - 4);
					bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(result, filePathWOExtension, true);

					if (isSaveSuccess)
					{
						AssetDatabase.Refresh();

						_editor._portrait._thumbnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(_editor._portrait._imageFilePath_Thumbnail);
					}
				}

				_exportRequestType = EXPORT_TYPE.None;
				_exportProcessType = EXPORT_TYPE.None;//<<끝
				_exportProcessX100 = 0;
				_iProcess = 0;
			}
			catch (Exception ex)
			{
				Debug.LogError("Make Thumbnail Exception : " + ex);
			}
		}

		private void Process_PNGScreenShot()
		{
			try
			{
				string defFileName = "ScreenShot_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".png";
				string saveFilePath = EditorUtility.SaveFilePanel("Save Screenshot as PNG", _prevFilePath_Directory, defFileName, "png");
				if (!string.IsNullOrEmpty(saveFilePath))
				{
					Texture2D result = _editor.Exporter.RenderToTexture(_editor.Select.RootUnit._childMeshGroup,
																	(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), (int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
																	_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
																	_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
																	_editor._captureFrame_Color
																	);

					if (result != null)
					{
						//이미지를 저장하자
						//이건 Asset으로 자동 저장
						string filePathWOExtension = saveFilePath.Substring(0, saveFilePath.Length - 4);

						bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(result, filePathWOExtension, true);

						System.IO.FileInfo fi = new System.IO.FileInfo(saveFilePath);

						Application.OpenURL("file://" + fi.Directory.FullName);
						Application.OpenURL("file://" + saveFilePath);

						_prevFilePath = saveFilePath;
						_prevFilePath_Directory = fi.Directory.FullName;
					}


				}

				_exportRequestType = EXPORT_TYPE.None;
				_exportProcessType = EXPORT_TYPE.None;//<<끝
				_exportProcessX100 = 0;
				_iProcess = 0;
			}
			catch (Exception ex)
			{
				Debug.LogError("PNG Screenshot Exception : " + ex);
			}
		}

		private void Process_MakeGIF()
		{
			if (_iProcess == 0)
			{
				string defFileName = "GIF_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".gif";
				string saveFilePath = EditorUtility.SaveFilePanel("Save GIF Animation", _prevFilePath_Directory, defFileName, "gif");
				if (!string.IsNullOrEmpty(saveFilePath))
				{
					int result = _editor.Exporter.MakeGIFAnimation_Ready(saveFilePath,
																		_editor.Select.RootUnit._childMeshGroup,
																		_selectedAnimClip, _editor._captureFrame_GIFSampleLoopCount,
																		(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), (int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
																		_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
																		_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
																		_editor._captureFrame_Color,
																		_editor._captureFrame_GIFSampleQuality
																	);

					if (result < 0)
					{
						//EditorUtility.DisplayDialog("Make GIF Creating Failed", "Request is rejected", "Close");
						EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.GIFFailed_Title),
														_editor.GetText(apLocalization.TEXT.GIFFailed_Body_Reject),
														_editor.GetText(apLocalization.TEXT.Close));

						_exportProcessType = EXPORT_TYPE.None;
						_exportRequestType = EXPORT_TYPE.None;
						_exportProcessX100 = 0;
						return;
					}

					_iProcessCount = result;

				}
			}

			float processRatio = _editor.Exporter.MakeGIFAnimation_Step(_iProcess);
			if (processRatio < 0.0f)
			{
				//EditorUtility.DisplayDialog("Make GIF Creating Failed", "Request is rejected", "Close");

				EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.GIFFailed_Title),
												_editor.GetText(apLocalization.TEXT.GIFFailed_Body_Reject),
												_editor.GetText(apLocalization.TEXT.Close));


				_exportProcessType = EXPORT_TYPE.None;
				_exportRequestType = EXPORT_TYPE.None;
				_exportProcessX100 = 0;
				return;
			}

			_exportProcessX100 = (int)(processRatio + 0.5f);

			_iProcess++;
			if (_iProcess >= _iProcessCount)
			{
				_exportProcessX100 = 100;
				_exportProcessType = EXPORT_TYPE.None;
				_exportRequestType = EXPORT_TYPE.None;

				System.IO.FileInfo fi = new System.IO.FileInfo(_editor.Exporter.GIF_FilePath);

				Application.OpenURL("file://" + fi.Directory.FullName);
				Application.OpenURL("file://" + _editor.Exporter.GIF_FilePath);

				_prevFilePath = _editor.Exporter.GIF_FilePath;
				_prevFilePath_Directory = fi.Directory.FullName;
			}
		}
	}

}
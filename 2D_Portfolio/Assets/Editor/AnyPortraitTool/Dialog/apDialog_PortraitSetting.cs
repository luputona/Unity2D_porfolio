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

	public class apDialog_PortraitSetting : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_PortraitSetting s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		private object _loadKey = null;


		private enum TAB
		{
			PortriatSetting,
			EditorSetting,
			About
		}

		private TAB _tab = TAB.PortriatSetting;
		private Vector2 _scroll = Vector2.zero;

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



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_PortraitSetting), true, "Setting", true);
			apDialog_PortraitSetting curTool = curWindow as apDialog_PortraitSetting;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 400;
				int height = 500;
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

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
			{
				Debug.LogError("Exit - Editor / Portrait Missmatch");
				CloseDialog();
				return;

			}

			int tabBtnHeight = 25;
			int tabBtnWidth = ((width - 10) / 3) - 4;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(tabBtnHeight));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton("Portrait", _tab == TAB.PortriatSetting, tabBtnWidth, tabBtnHeight))
			{
				_tab = TAB.PortriatSetting;
			}
			if (apEditorUtil.ToggledButton("Editor", _tab == TAB.EditorSetting, tabBtnWidth, tabBtnHeight))
			{
				_tab = TAB.EditorSetting;
			}
			if (apEditorUtil.ToggledButton("About", _tab == TAB.About, tabBtnWidth, tabBtnHeight))
			{
				_tab = TAB.About;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			int scrollHeight = height - 40;
			_scroll = EditorGUILayout.BeginScrollView(_scroll, false, true, GUILayout.Width(width), GUILayout.Height(scrollHeight));
			width -= 25;
			GUILayout.BeginVertical(GUILayout.Width(width));

			switch (_tab)
			{
				case TAB.PortriatSetting:
					{
						//Portrait 설정
						EditorGUILayout.LabelField("Portrait Settings");
						GUILayout.Space(10);
						string nextName = EditorGUILayout.DelayedTextField("Name", _targetPortrait.name);
						if (nextName != _targetPortrait.name)
						{
							_targetPortrait.name = nextName;
						}

						int nextFPS = EditorGUILayout.DelayedIntField("Update FPS", _targetPortrait._FPS);
						if (_targetPortrait._FPS != nextFPS)
						{
							if (nextFPS < 10)
							{
								nextFPS = 10;
							}
							_targetPortrait._FPS = nextFPS;
						}

						bool nextGPUAcc = EditorGUILayout.Toggle("GPU Acceleration", _targetPortrait._isGPUAccel);
						if (nextGPUAcc != _targetPortrait._isGPUAccel)
						{
							_targetPortrait._isGPUAccel = nextGPUAcc;
						}
					}
					break;

				case TAB.EditorSetting:
					{
						EditorGUILayout.LabelField("Editor Settings");
						GUILayout.Space(10);

						apEditor.LANGUAGE prevLanguage = _editor._language;
						bool prevGUIFPS = _editor._guiOption_isFPSVisible;

						Color prevColor_Background = _editor._colorOption_Background;
						Color prevColor_GridCenter = _editor._colorOption_GridCenter;
						Color prevColor_Grid = _editor._colorOption_Grid;

						Color prevColor_MeshEdge = _editor._colorOption_MeshEdge;
						Color prevColor_MeshHiddenEdge = _editor._colorOption_MeshHiddenEdge;
						Color prevColor_Outline = _editor._colorOption_Outline;
						Color prevColor_TFBorder = _editor._colorOption_TransformBorder;
						Color prevColor_VertNotSelected = _editor._colorOption_VertColor_NotSelected;
						Color prevColor_VertSelected = _editor._colorOption_VertColor_Selected;

						Color prevColor_GizmoFFDLine = _editor._colorOption_GizmoFFDLine;
						Color prevColor_GizmoFFDInnerLine = _editor._colorOption_GizmoFFDInnerLine;

						_editor._language = (apEditor.LANGUAGE)EditorGUILayout.EnumPopup("Language", _editor._language);

						GUILayout.Space(10);
						_editor._guiOption_isFPSVisible = EditorGUILayout.Toggle("Show FPS", _editor._guiOption_isFPSVisible);


						GUILayout.Space(10);
						try
						{
							int width_Btn = 65;
							int width_Color = width - (width_Btn + 8);

							int height_Color = 18;
							EditorGUILayout.LabelField("Background Colors");

							//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Color));
							//GUILayout.Space(5);
							//_editor._colorOption_Background = EditorGUILayout.ColorField("Background", _editor._colorOption_Background, GUILayout.Width(width_Color), GUILayout.Height(height_Color));
							//if(GUILayout.Button("Default", GUILayout.Width(width_Btn), GUILayout.Height(height_Color)))
							//{
							//	_editor._colorOption_Background = apEditor.DefaultColor_Background;
							//}
							//EditorGUILayout.EndHorizontal();

							//_editor._colorOption_GridCenter = EditorGUILayout.ColorField("Grid Center", _editor._colorOption_GridCenter);
							//_editor._colorOption_Grid = EditorGUILayout.ColorField("Grid", _editor._colorOption_Grid);
							//_editor._colorOption_AtlasBorder = EditorGUILayout.ColorField("Atlas Border", _editor._colorOption_AtlasBorder);

							_editor._colorOption_Background = ColorUI("Background", _editor._colorOption_Background, width, apEditor.DefaultColor_Background);
							_editor._colorOption_GridCenter = ColorUI("Grid Center", _editor._colorOption_GridCenter, width, apEditor.DefaultColor_GridCenter);
							_editor._colorOption_Grid = ColorUI("Grid", _editor._colorOption_Grid, width, apEditor.DefaultColor_Grid);
							_editor._colorOption_AtlasBorder = ColorUI("Atlas Border", _editor._colorOption_AtlasBorder, width, apEditor.DefaultColor_AtlasBorder);


							GUILayout.Space(5);
							EditorGUILayout.LabelField("Mesh GUI Colors");
							//_editor._colorOption_MeshEdge = EditorGUILayout.ColorField("Mesh Edge", _editor._colorOption_MeshEdge);
							//_editor._colorOption_MeshHiddenEdge = EditorGUILayout.ColorField("Mesh Hidden Edge", _editor._colorOption_MeshHiddenEdge);
							//_editor._colorOption_Outline = EditorGUILayout.ColorField("Outline", _editor._colorOption_Outline);
							//_editor._colorOption_TransformBorder = EditorGUILayout.ColorField("Transform Border", _editor._colorOption_TransformBorder);
							//_editor._colorOption_VertColor_NotSelected = EditorGUILayout.ColorField("Vertex", _editor._colorOption_VertColor_NotSelected);
							//_editor._colorOption_VertColor_Selected = EditorGUILayout.ColorField("Selected Vertex", _editor._colorOption_VertColor_Selected);

							_editor._colorOption_MeshEdge = ColorUI("Mesh Edge", _editor._colorOption_MeshEdge, width, apEditor.DefaultColor_MeshEdge);
							_editor._colorOption_MeshHiddenEdge = ColorUI("Mesh Hidden Edge", _editor._colorOption_MeshHiddenEdge, width, apEditor.DefaultColor_MeshHiddenEdge);
							_editor._colorOption_Outline = ColorUI("Outline", _editor._colorOption_Outline, width, apEditor.DefaultColor_Outline);
							_editor._colorOption_TransformBorder = ColorUI("Transform Border", _editor._colorOption_TransformBorder, width, apEditor.DefaultColor_TransformBorder);
							_editor._colorOption_VertColor_NotSelected = ColorUI("Vertex", _editor._colorOption_VertColor_NotSelected, width, apEditor.DefaultColor_VertNotSelected);
							_editor._colorOption_VertColor_Selected = ColorUI("Selected Vertex", _editor._colorOption_VertColor_Selected, width, apEditor.DefaultColor_VertSelected);


							GUILayout.Space(5);
							EditorGUILayout.LabelField("Gizmo Colors");
							//_editor._colorOption_GizmoFFDLine = EditorGUILayout.ColorField("FFD Line", _editor._colorOption_GizmoFFDLine);
							//_editor._colorOption_GizmoFFDInnerLine = EditorGUILayout.ColorField("FFD Inner Line", _editor._colorOption_GizmoFFDInnerLine);

							_editor._colorOption_GizmoFFDLine = ColorUI("FFD Line", _editor._colorOption_GizmoFFDLine, width, apEditor.DefaultColor_GizmoFFDLine);
							_editor._colorOption_GizmoFFDInnerLine = ColorUI("FFD Inner Line", _editor._colorOption_GizmoFFDInnerLine, width, apEditor.DefaultColor_GizmoFFDInnerLine);


						}
						catch (Exception)
						{

						}

						GUILayout.Space(20);
						if (GUILayout.Button("Restore Editor Default Setting", GUILayout.Height(20)))
						{
							_editor.RestoreEditorPref();
						}


						if (prevLanguage != _editor._language ||
							prevGUIFPS != _editor._guiOption_isFPSVisible ||
								prevColor_Background != _editor._colorOption_Background ||
								prevColor_GridCenter != _editor._colorOption_GridCenter ||
								prevColor_Grid != _editor._colorOption_Grid ||

								prevColor_MeshEdge != _editor._colorOption_MeshEdge ||
								prevColor_MeshHiddenEdge != _editor._colorOption_MeshHiddenEdge ||
								prevColor_Outline != _editor._colorOption_Outline ||
								prevColor_TFBorder != _editor._colorOption_TransformBorder ||
								prevColor_VertNotSelected != _editor._colorOption_VertColor_NotSelected ||
								prevColor_VertSelected != _editor._colorOption_VertColor_Selected ||

								prevColor_GizmoFFDLine != _editor._colorOption_GizmoFFDLine ||
								prevColor_GizmoFFDInnerLine != _editor._colorOption_GizmoFFDInnerLine)
						{
							_editor.SaveEditorPref();
						}
					}
					break;

				case TAB.About:
					{
						EditorGUILayout.LabelField("About");

						GUILayout.Space(20);
						apEditorUtil.GUI_DelimeterBoxH(width);
						GUILayout.Space(20);

						EditorGUILayout.LabelField("[AnyPortrait]");
						GUILayout.Space(10);
						EditorGUILayout.LabelField("Copyright (c) 2017 RainyRizzle. All right reserved.");

						GUILayout.Space(20);
						apEditorUtil.GUI_DelimeterBoxH(width);
						GUILayout.Space(20);

						EditorGUILayout.LabelField("[PSD File Import Library]");
						GUILayout.Space(10);
						EditorGUILayout.LabelField("Ntreev Photoshop Document Parser for .Net");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("Released under the MIT License.");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("Copyright (c) 2015 Ntreev Soft co., Ltd.");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("Permission is hereby granted, free of charge,");
						EditorGUILayout.LabelField("to any person obtaining a copy of this software");
						EditorGUILayout.LabelField("and associated documentation files (the \"Software\"),");
						EditorGUILayout.LabelField("to deal in the Software without restriction,");
						EditorGUILayout.LabelField("including without limitation the rights ");
						EditorGUILayout.LabelField("to use, copy, modify, merge, publish, distribute,");
						EditorGUILayout.LabelField("sublicense, and/or sell copies of the Software, ");
						EditorGUILayout.LabelField("and to permit persons to whom the Software is furnished");
						EditorGUILayout.LabelField("to do so, subject to the following conditions:");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("The above copyright notice and ");
						EditorGUILayout.LabelField("this permission notice shall be included");
						EditorGUILayout.LabelField("in all copies or substantial portions of the Software.");
						GUILayout.Space(10);

						EditorGUILayout.LabelField("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT ");
						EditorGUILayout.LabelField("WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, ");
						EditorGUILayout.LabelField("INCLUDING BUT NOT LIMITED TO THE WARRANTIES ");
						EditorGUILayout.LabelField("OF MERCHANTABILITY, FITNESS FOR A PARTICULAR ");
						EditorGUILayout.LabelField("PURPOSE AND NONINFRINGEMENT. ");
						EditorGUILayout.LabelField("IN NO EVENT SHALL THE AUTHORS OR ");
						EditorGUILayout.LabelField("COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES ");
						EditorGUILayout.LabelField("OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, ");
						EditorGUILayout.LabelField("TORT OR OTHERWISE, ARISING FROM, OUT OF OR ");
						EditorGUILayout.LabelField("IN CONNECTION WITH THE SOFTWARE OR ");
						EditorGUILayout.LabelField("THE USE OR OTHER DEALINGS IN THE SOFTWARE.");

						GUILayout.Space(20);
						apEditorUtil.GUI_DelimeterBoxH(width);
						GUILayout.Space(20);

						EditorGUILayout.LabelField("[GIF Export Library]");
						GUILayout.Space(10);
						EditorGUILayout.LabelField("NGif, Animated GIF Encoder for .NET");
						GUILayout.Space(10);
						EditorGUILayout.LabelField("Released under the CPOL 1.02.");
						GUILayout.Space(10);



					}
					break;
			}



			GUILayout.Space(height);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}


		private Color ColorUI(string label, Color srcColor, int width, Color defaultColor)
		{
			int width_Btn = 65;
			int width_Color = width - (width_Btn + 8);

			int height_Color = 18;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Color));
			GUILayout.Space(5);
			Color result = EditorGUILayout.ColorField(label, srcColor, GUILayout.Width(width_Color), GUILayout.Height(height_Color));
			if (GUILayout.Button("Default", GUILayout.Width(width_Btn), GUILayout.Height(height_Color)))
			{
				result = defaultColor;
			}
			EditorGUILayout.EndHorizontal();
			return result;
		}


	}


}
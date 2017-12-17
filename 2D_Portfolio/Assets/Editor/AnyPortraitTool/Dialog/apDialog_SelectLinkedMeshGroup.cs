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
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{

	public class apDialog_SelectLinkedMeshGroup : EditorWindow
	{
		// Members
		//------------------------------------------------------------------------
		public delegate void FUNC_SELECT_MESHGROUP(bool isSuccess, object loadKey, apMeshGroup meshGroup, apAnimClip targetAnimClip);

		private static apDialog_SelectLinkedMeshGroup s_window = null;

		private apEditor _editor = null;
		private object _loadKey = null;

		private FUNC_SELECT_MESHGROUP _funcResult;
		private apAnimClip _targetAnimClip = null;

		private List<apMeshGroup> _selectableMeshGroups = new List<apMeshGroup>();
		private apMeshGroup _selectedMeshGroup = null;

		private Vector2 _scrollList = new Vector2();

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apAnimClip targetAnimClip, FUNC_SELECT_MESHGROUP funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectLinkedMeshGroup), true, "Select Mesh Group", true);
			apDialog_SelectLinkedMeshGroup curTool = curWindow as apDialog_SelectLinkedMeshGroup;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 250;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetAnimClip, funcResult);

				return loadKey;
			}
			else
			{
				return null;
			}

		}

		private static void CloseDialog()
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
		//------------------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, apAnimClip targetAnimGroup, FUNC_SELECT_MESHGROUP funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_targetAnimClip = targetAnimGroup;

			_selectedMeshGroup = null;
			_selectableMeshGroups.Clear();

			for (int i = 0; i < _editor._portrait._meshGroups.Count; i++)
			{
				_selectableMeshGroups.Add(_editor._portrait._meshGroups[i]);
			}

		}


		// GUI
		//------------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				return;
			}

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 35, width, height - (90)), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			Texture2D iconMeshGroup = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);

			GUIStyle guiStyle = new GUIStyle(GUIStyle.none);
			guiStyle.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);
			GUILayout.Button("Select Mesh Group to Link", guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - (90)));

			GUILayout.Button(new GUIContent("Mesh Groups", iconImageCategory), guiStyle, GUILayout.Height(20));//<투명 버튼

			//GUILayout.Space(10);
			for (int i = 0; i < _selectableMeshGroups.Count; i++)
			{
				if (_selectableMeshGroups[i] == _selectedMeshGroup)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					prevColor = GUI.backgroundColor;

					GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

					GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
					GUI.backgroundColor = prevColor;
				}


				EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
				GUILayout.Space(15);
				if (GUILayout.Button(new GUIContent(" " + _selectableMeshGroups[i]._name, iconMeshGroup), guiStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
				{
					_selectedMeshGroup = _selectableMeshGroups[i];
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();


			bool isClose = false;
			if (GUILayout.Button("Select", GUILayout.Height(30)))
			{
				if (_selectedMeshGroup != null)
				{
					_funcResult(true, _loadKey, _selectedMeshGroup, _targetAnimClip);
				}
				else
				{
					_funcResult(false, _loadKey, null, null);
				}
				isClose = true;
			}
			if (GUILayout.Button("Close", GUILayout.Height(30)))
			{
				_funcResult(false, _loadKey, null, null);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}
	}

}
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

	public class apDialog_Bake : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_Bake s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		private object _loadKey = null;

		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_Bake), true, "Bake", true);
			apDialog_Bake curTool = curWindow as apDialog_Bake;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 350;
				int height = 170;
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
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
			{
				CloseDialog();
				return;
			}

			//Bake 설정
			EditorGUILayout.LabelField("Bake Setting");
			GUILayout.Space(5);

			EditorGUILayout.ObjectField("Portait", _targetPortrait, typeof(apPortrait), true);

			GUILayout.Space(5);

			float nextRootScale = EditorGUILayout.DelayedFloatField("Bake Scale", _targetPortrait._bakeScale);

			//GUILayout.Space(5);
			//float nextPhysicsScale = EditorGUILayout.DelayedFloatField("Physic Scale", _targetPortrait._physicBakeScale);


			CheckChangedProperties(nextRootScale);

			GUILayout.Space(10);


			if (GUILayout.Button("Bake", GUILayout.Height(30)))
			{
				GUI.FocusControl(null);

				CheckChangedProperties(nextRootScale);

				_editor.Controller.PortraitBake();
				_editor.Notification("[" + _targetPortrait.name + "] is Baked", false, false);
			}

			GUILayout.Space(5);
			if (GUILayout.Button("Optimized Bake", GUILayout.Height(30)))
			{
				GUI.FocusControl(null);

				CheckChangedProperties(nextRootScale);

				Debug.LogError("TODO : Opt Bake");
				_editor.Notification("[" + _targetPortrait.name + "] is Baked (Optimized)", false, false);
			}


		}

		private void CheckChangedProperties(float nextRootScale)
		{
			bool isChanged = false;
			if (nextRootScale != _targetPortrait._bakeScale)
			{
				isChanged = true;
			}

			if (isChanged)
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Portrait_BakeOptionChanged, _targetPortrait, _targetPortrait, false, _editor);

				if (nextRootScale < 0.0001f)
				{
					nextRootScale = 0.0001f;
				}
				_targetPortrait._bakeScale = nextRootScale;

				//if(nextPhysicsScale < 0.0f)
				//{
				//	nextPhysicsScale = 0.0f;
				//}
				//_targetPortrait._physicBakeScale = nextPhysicsScale;

				GUI.FocusControl(null);
			}
		}
	}

}
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
	//[데모용] Start Page 다이얼로그
	//에디터 시작시 나온다.
	//데모 버전 : 매 시작시마다 나온다.
	
	//내용
	//데모 버전 : 로고+데모 / 버전 / 데모와 정품 차이 안내 / 닫기

	public class apDialog_StartPage : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_StartPage s_window = null;

		private apEditor _editor = null;
		private Texture2D _img_Logo = null;

		// Show Window
		//------------------------------------------------------------------
		public static void ShowDialog(apEditor editor, Texture2D img_Logo)
		{
			
			CloseDialog();

			if (editor == null)
			{
				return;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_StartPage), true, "Demo Start Page", true);
			apDialog_StartPage curTool = curWindow as apDialog_StartPage;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 250;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, img_Logo);
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
		public void Init(apEditor editor, Texture2D img_Logo)
		{
			_editor = editor;
			_img_Logo = img_Logo;
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			width -= 10;
			//if (_editor == null)
			//{
			//	CloseDialog();
			//	return;
			//}

			////만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			//if (_editor != apEditor.CurrentEditor)
			//{
			//	CloseDialog();
			//	return;
			//}


			//1. 로고
			//2. 버전
			//3. 데모 기능 제한 확인하기

			int logoWidth = _img_Logo.width;
			int logoHeight = _img_Logo.height;
			int boxHeight = (int)((float)width * ((float)logoHeight / (float)logoWidth));
			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = Color.black;
			GUILayout.Box(_img_Logo, GUILayout.Width(width), GUILayout.Height(boxHeight));

			GUI.backgroundColor = prevColor;
			GUILayout.Space(5);

			EditorGUILayout.LabelField("Demo Version : " + apVersion.I.APP_VERSION);

			GUILayout.Space(10);

			if(GUILayout.Button("Check Limitations", GUILayout.Width(width), GUILayout.Height(40)))
			{
				//홈페이지로 갑시다.
				Application.OpenURL("https://www.rainyrizzle.com/ap-demodownload");
				CloseDialog();
			}
			GUILayout.Space(5);
			if(GUILayout.Button("Close", GUILayout.Width(width), GUILayout.Height(25)))
			{
				CloseDialog();
			}
		}
	}

}
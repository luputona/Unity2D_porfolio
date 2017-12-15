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

	public class apEditorTroubleShooting : EditorWindow
	{
		//private static apEditor2 s_window = null;

		//--------------------------------------------------------------
		//우선순위가 10 이상 차이가 나면 구분자가 생긴다.

		[MenuItem("Window/AnyPortrait/Editor Reset", false, 21)]
		public static void ShowWindow()
		{
			apEditor.CloseEditor();
			Debug.LogWarning("AnyPortrait Editor is Closed.");
			//      EditorWindow curWindow = EditorWindow.GetWindow(typeof(apEditor2), false, "AnyPortrait2");
			//      apEditor2 curTool = curWindow as apEditor2;
			//      //if(curTool != null && curTool != s_window)
			//if(curTool != null)
			//      {
			//          s_window = curTool;
			//	s_window.position = new Rect(0, 0, 200, 200);
			//          //s_window.Init();
			//      }
		}

		//[MenuItem("Window/AnyPortrait/Editor Reset", false)]
		//public static void ShowWindowDummy()
		//{
		//	Debug.Log("Dummy");
		//}
		//--------------------------------------------------------------

		[MenuItem("Window/AnyPortrait/Homepage", false, 41)]
		public static void OpenHomepage()
		{
			Application.OpenURL("https://www.rainyrizzle.com/");
		}

		[MenuItem("Window/AnyPortrait/Getting Started", false, 42)]
		public static void OpenGettingStarted()
		{
			Application.OpenURL("https://www.rainyrizzle.com/ap-gettingstarted");
		}

		[MenuItem("Window/AnyPortrait/Scripting", false, 43)]
		public static void OpenScripting()
		{
			Application.OpenURL("https://www.rainyrizzle.com/ap-scripting");
		}


		//이 기능은 뺍시다.
		//[MenuItem("Window/AnyPortrait/Submit a Survey (Demo)", false, 81)]
		//public static void OpenSubmitASurvey()
		//{
		//	Application.OpenURL("https://goo.gl/forms/xZqTaXTesYq6v1Ba2");
		//}


		[MenuItem("Window/AnyPortrait/Report a Bug or Feature", false, 82)]
		public static void OpenReportABug()
		{
			Application.OpenURL("https://goo.gl/forms/f03CdFRr58VTCqv53");
		}
	}

}
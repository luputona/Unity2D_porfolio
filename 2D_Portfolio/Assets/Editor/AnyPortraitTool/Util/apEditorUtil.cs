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
using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{

	public static class apEditorUtil
	{

		public static void GUI_DelimeterBoxV(int height)
		{
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
			GUILayout.Box("", GUILayout.Width(4), GUILayout.Height(height));
			GUI.backgroundColor = prevColor;
		}

		public static void GUI_DelimeterBoxH(int width)
		{
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
			GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(4));
			GUI.backgroundColor = prevColor;
		}

		public static int _recordRand = 0;
		private static int _lastUndoID = -1;
		//public static void SetRecord(string strRecord, UnityEngine.Object targetObj)

		public static void ClearRecord()
		{

		}

		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		/// <param name="parentObject">변동되는 오브젝트의 Parent</param>
		/// <param name="targetObject">변동되는 오브젝트</param>
		/// <param name="isMultiple">한번에 여러개의 객체를 동시에 처리하는가</param>
		public static void SetRecord(apUndoGroupData.ACTION action,
										MonoBehaviour parentMonoObject, //<<이게 중요
										object targetObject,
										bool isMultiple,
										apEditor editor)
		{
			if (editor._portrait == null)
			{
				return;
			}
			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, parentMonoObject, targetObject, isMultiple);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				_lastUndoID = Undo.GetCurrentGroup();
			}

			Undo.RecordObject(parentMonoObject, apUndoGroupData.GetLabel(action));//MonoObject별로 다르게 Undo를 등록하자

			if (targetObject != null && targetObject != parentMonoObject as object && targetObject is MonoBehaviour)
			{
				//타겟 오브젝트도 Monobehaviour라면 같이 저장
				Undo.RecordObject(targetObject as MonoBehaviour, apUndoGroupData.GetLabel(action));
			}

			//테스트를 위해서 Undo 기록이 발생하면 노티를 띄운다. (Undo ID를 같이)
			//editor.Notification("U:" + apUndoGroupData.GetLabel(action) + " - " + Undo.GetCurrentGroup(), true);

		}

		/// <summary>
		/// Monobehaviour 객체가 생성되니 Undo로 기록할 때 호출하는 함수
		/// </summary>
		/// <param name="createdMonoObject"></param>
		/// <param name="label"></param>
		public static void SetRecordCreateMonoObject(MonoBehaviour createdMonoObject, string label)
		{
			if (createdMonoObject == null)
			{
				return;
			}
			Undo.RegisterCreatedObjectUndo(createdMonoObject.gameObject, label);
		}


		public static void SetEditorDirty()
		{
			EditorSceneManager.MarkAllScenesDirty();
		}

		/// <summary>
		/// Undo는 "같은 메뉴"에서만 가능하다. 메뉴를 전환할 때에는 Undo를 
		/// </summary>
		public static void ResetUndo(apEditor editor)
		{
			//apUndoManager.I.Clear();
			if (editor._portrait != null)
			{
				Undo.ClearUndo(editor._portrait);
			}
		}


		public static void OnUndoRedoPerformed()
		{
			apUndoGroupData.I.Clear();
		}

		//public static apUndoUnitBase MakeUndo(apUndoManager.COMMAND command, object keyObj, apUndoManager.ACTION_TYPE actionType, string label, apPortrait portrait)
		//{
		//	EditorSceneManager.MarkAllScenesDirty();
		//	return apUndoManager.I.MakeUndo(command, keyObj, actionType, label, portrait);
		//}

		//public static void Undo(apEditor editor)
		//{
		//	Debug.Log("Undo");
		//	apUndoUnitBase executedUnit = apUndoManager.I.Undo(editor._portrait, editor);
		//	if(executedUnit != null)
		//	{
		//		editor.Notification("Undo [" + executedUnit._label + "]", true);
		//	}
		//	else
		//	{
		//		editor.Notification("No Undo", true);
		//	}
		//}

		//public static void Redo(apEditor editor)
		//{
		//	apUndoUnitBase executedUnit = apUndoManager.I.Redo(editor._portrait, editor);
		//	if(executedUnit != null)
		//	{
		//		editor.Notification("Redo [" + executedUnit._label + "]", true);
		//	}
		//}

		public static void ReleaseGUIFocus()
		{
			GUI.FocusControl(null);
		}

		//public static apPortrait MakeNewPortraitWithDialog()
		//{
		//	string path = EditorUtility.SaveFilePanelInProject("New Portrait Prefab", "noname.prefab", "prefab", "Please Enter a file name to save Prefab to");
		//	if (string.IsNullOrEmpty(path))
		//	{

		//		EditorUtility.DisplayDialog("New File Failed", "Creating process is canceled", "Okay");
		//		return null;
		//	}
		//	else
		//	{
		//		//path = path.Replace(".prefab", "");
		//		int iExp = path.LastIndexOf('.');
		//		string strPrefabName = "";
		//		for (int i = iExp; i >= 0; i--)
		//		{
		//			string curStr = path.Substring(i, 1);
		//			if (curStr == ".")
		//			{

		//			}
		//			else if (curStr == "/" || curStr == "\\")
		//			{
		//				break;
		//			}
		//			else
		//			{
		//				strPrefabName = curStr + strPrefabName;
		//			}

		//		}

		//		GameObject newPrefab = new GameObject(strPrefabName);
		//		newPrefab.transform.position = Vector3.zero;
		//		newPrefab.transform.rotation = Quaternion.identity;
		//		newPrefab.transform.localScale = Vector3.one;

		//		newPrefab.AddComponent<apPortrait>();

		//		PrefabUtility.CreatePrefab(path, newPrefab, ReplacePrefabOptions.ConnectToPrefab);

		//		MonoBehaviour.DestroyImmediate(newPrefab);

		//		GameObject savedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

		//		AssetDatabase.Refresh();

		//		Selection.activeObject = savedAsset;

		//		Debug.Log("Create : [" + path + "]");
		//		return savedAsset.GetComponent<apPortrait>();
		//	}
		//}

		//public static apPortrait OpenPortraitWithDialog()
		//{
		//	string path = EditorUtility.OpenFilePanel("Open apPortrait Prefab", "", "prefab");
		//	if (string.IsNullOrEmpty(path))
		//	{
		//		//EditorUtility.DisplayDialog("Open File Failed", "canceled", "Okay");
		//		return null;
		//	}
		//	else
		//	{
		//		Debug.Log("Open : [" + path + "]");

		//		int iAssets = path.LastIndexOf("Assets/");
		//		if (iAssets < 0)
		//		{
		//			EditorUtility.DisplayDialog("Open File Failed", "Is Wrong path", "Okay");
		//			return null;
		//		}

		//		path = path.Substring(iAssets);

		//		GameObject loadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
		//		if (loadedPrefab == null)
		//		{
		//			EditorUtility.DisplayDialog("Open File Failed", "There is no Prefab", "Okay");
		//			return null;
		//		}

		//		Selection.activeObject = loadedPrefab;

		//		apPortrait portrait = loadedPrefab.GetComponent<apPortrait>();
		//		if (portrait == null)
		//		{
		//			EditorUtility.DisplayDialog("Open File Failed", "There is no apPortrait Component", "Okay");
		//			return null;
		//		}
		//		return portrait;
		//	}
		//}

		//public static apPortrait SavePortraitWithDialog(apPortrait portrait)
		//{
		//	if (portrait == null)
		//	{
		//		EditorUtility.DisplayDialog("Save File Failed", "No Portrait", "Okay");
		//		return null;
		//	}



		//	string assetPath = AssetDatabase.GetAssetPath(portrait.gameObject);
		//	Debug.Log("Save : " + assetPath);

		//	GameObject copiedObj = MonoBehaviour.Instantiate<GameObject>(portrait.gameObject);
		//	GameObject targetObj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);


		//	if (targetObj == null)
		//	{
		//		PrefabUtility.CreatePrefab(assetPath, copiedObj, ReplacePrefabOptions.ConnectToPrefab);
		//	}
		//	else
		//	{
		//		PrefabUtility.ReplacePrefab(copiedObj, targetObj, ReplacePrefabOptions.Default);
		//	}

		//	MonoBehaviour.DestroyImmediate(copiedObj);

		//	AssetDatabase.Refresh();

		//	GameObject savedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

		//	Selection.activeObject = savedAsset;

		//	return savedAsset.GetComponent<apPortrait>();
		//}


		public static bool ToggledButton(string strText, bool isSelected, int width)
		{
			return ToggledButton(strText, isSelected, width, 20);
		}

		public static bool ToggledButton(string strText, bool isSelected, int width, int height)
		{
			if (isSelected)
			{
				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = Color.white;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				Color prevColor = GUI.backgroundColor;
				GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);

				GUILayout.Box(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				return GUILayout.Button(strText, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton(string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else if (isSelected)
				{
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
				}


				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = Color.white;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				//GUILayout.Box(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Button(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));//더미 버튼

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.alignment = TextAnchor.MiddleCenter;

				return GUILayout.Button(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton(Texture2D texture, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else if (isSelected)
				{
					GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
				}

				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = Color.red;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.alignment = TextAnchor.MiddleCenter;

				return GUILayout.Button(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton(Texture2D texture, string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else if (isSelected)
				{
					GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
				}


				//GUI.skin.box
				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.normal.textColor = Color.white;
				guiStyle.alignment = TextAnchor.MiddleCenter;
				guiStyle.margin = GUI.skin.button.margin;

				GUILayout.Box(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.alignment = TextAnchor.MiddleCenter;

				return GUILayout.Button(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D texture, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else if (isSelected)
				{
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				return GUILayout.Button(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else if (isSelected)
				{
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(textureSelected, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				return GUILayout.Button(textureNotSelected, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton_2Side(Texture2D texture, string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height, GUIStyle alignmentStyle = null)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else if (isSelected)
				{
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = Color.white;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}



				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}


				return GUILayout.Button(new GUIContent(strTextNotSelected, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height, GUIStyle alignmentStyle = null)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else if (isSelected)
				{
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.red;

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = Color.white;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}



				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(strTextSelected, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}


				return GUILayout.Button(strTextNotSelected, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, Texture2D textureNotAvailable,
												string strTextSelected, string strTextNotSelected, string strTextNotAvailable,
												bool isSelected, bool isAvailable, int width, int height, GUIStyle alignmentStyle = null)
		{
			Color prevColor = GUI.backgroundColor;

			if (!isAvailable)
			{
				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

				GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = Color.white;
				guiStyle.margin = GUI.skin.button.margin;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}

				GUILayout.Box(new GUIContent(strTextNotAvailable, textureNotAvailable), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else if (isSelected)
			{
				GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);

				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;
				guiStyle.normal.textColor = Color.white;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, textureSelected), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtn;
			}
			else
			{
				GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				guiStyle.padding = GUI.skin.box.padding;

				if (alignmentStyle != null)
				{
					guiStyle.alignment = alignmentStyle.alignment;
				}

				return GUILayout.Button(new GUIContent(strTextNotSelected, textureNotSelected), guiStyle, GUILayout.Width(width), GUILayout.Height(height));
			}
		}



		public static Vector2 DelayedVector2Field(Vector2 vectorValue, int width)
		{
			Vector2 result = vectorValue;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			if (width > 100)
			{
				int widthLabel = 15;
				int widthData = ((width - ((15 + 2) * 2)) / 2) - 2;
				EditorGUILayout.LabelField("X", GUILayout.Width(widthLabel));
				result.x = EditorGUILayout.DelayedFloatField(vectorValue.x, GUILayout.Width(widthData));

				EditorGUILayout.LabelField("Y", GUILayout.Width(widthLabel));
				result.y = EditorGUILayout.DelayedFloatField(vectorValue.y, GUILayout.Width(widthData));
			}
			else
			{
				int widthData = (width / 2) - 2;
				result.x = EditorGUILayout.DelayedFloatField(vectorValue.x, GUILayout.Width(widthData));
				result.y = EditorGUILayout.DelayedFloatField(vectorValue.y, GUILayout.Width(widthData));
			}



			EditorGUILayout.EndHorizontal();

			return result;
		}

		public static float DistanceFromLine(Vector2 posA, Vector2 posB, Vector2 posTarget)
		{
			//float lineLen = Vector2.Distance(posA, posB);
			//if(lineLen < 0.1f)
			//{
			//	return Vector2.Distance(posA, posTarget);
			//}

			//float proj = (posTarget.x - posA.x) * (posB.x - posA.x) + (posTarget.y - posA.y) * (posB.y - posA.y);
			//if(proj < 0)
			//{
			//	return Vector2.Distance(posA, posTarget);
			//}
			//else if(proj > lineLen)
			//{
			//	return Vector2.Distance(posB, posTarget);
			//}

			//return Mathf.Abs((-1) * (posTarget.x - posA.x) * (posB.y - posA.y) + (posTarget.y - posA.y) * (posB.x - posA.x)) / lineLen;

			//float lineLen = Vector2.Distance(posA, posB);
			float dotA = Vector2.Dot(posTarget - posA, (posB - posA).normalized);
			float dotB = Vector2.Dot(posTarget - posB, (posA - posB).normalized);

			if (dotA < 0.0f)
			{
				return Vector2.Distance(posA, posTarget);
			}

			if (dotB < 0.0f)
			{
				return Vector2.Distance(posB, posTarget);
			}

			return Vector2.Distance((posA + (posB - posA).normalized * dotA), posTarget);
		}

		public static bool IsMouseInMesh(Vector2 mousePos, apMesh targetMesh)
		{
			Vector2 mousePosW = apGL.GL2World(mousePos);

			Vector2 mousePosL = mousePosW + targetMesh._offsetPos;//<<이걸 추가해줘야 Local Pos가 된다.

			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					if (tri.IsPointInTri(mousePosL))
					{
						return true;
					}
				}
			}
			return false;
		}


		public static bool IsMouseInMesh(Vector2 mousePos, apMesh targetMesh, apMatrix3x3 matrixWorldToMeshLocal)
		{
			Vector2 mousePosW = apGL.GL2World(mousePos);

			Vector2 mousePosL = matrixWorldToMeshLocal.MultiplyPoint(mousePosW);

			//Vector2 mousePosL = mousePosW + targetMesh._offsetPos;//<<이걸 추가해줘야 Local Pos가 된다.

			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					if (tri.IsPointInTri(mousePosL))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsMouseInRenderUnitMesh(Vector2 mousePos, apRenderUnit meshRenderUnit)
		{
			if (meshRenderUnit._meshTransform == null)
			{
				return false;
			}

			if (meshRenderUnit._meshTransform._mesh == null || meshRenderUnit._renderVerts.Count == 0)
			{
				return false;
			}

			apMesh targetMesh = meshRenderUnit._meshTransform._mesh;
			List<apRenderVertex> rVerts = meshRenderUnit._renderVerts;

			Vector2 mousePosW = apGL.GL2World(mousePos);

			apRenderVertex rVert0, rVert1, rVert2;
			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					rVert0 = rVerts[tri._verts[0]._index];
					rVert1 = rVerts[tri._verts[1]._index];
					rVert2 = rVerts[tri._verts[2]._index];

					if (apMeshTri.IsPointInTri(mousePosW,
												rVert0._pos_World,
												rVert1._pos_World,
												rVert2._pos_World))
					{
						return true;
					}
				}
			}
			return false;
		}


		public static bool IsPointInTri(Vector2 point, Vector2 triPoint0, Vector2 triPoint1, Vector2 triPoint2)
		{
			float s = triPoint0.y * triPoint2.x - triPoint0.x * triPoint2.y + (triPoint2.y - triPoint0.y) * point.x + (triPoint0.x - triPoint2.x) * point.y;
			float t = triPoint0.x * triPoint1.y - triPoint0.y * triPoint1.x + (triPoint0.y - triPoint1.y) * point.x + (triPoint1.x - triPoint0.x) * point.y;

			if ((s < 0) != (t < 0))
			{
				return false;
			}

			var A = -triPoint1.y * triPoint2.x + triPoint0.y * (triPoint2.x - triPoint1.x) + triPoint0.x * (triPoint1.y - triPoint2.y) + triPoint1.x * triPoint2.y;
			if (A < 0.0)
			{
				s = -s;
				t = -t;
				A = -A;
			}
			return s > 0 && t > 0 && (s + t) <= A;

		}
		//----------------------------------------------------------------------------------------------------
		public static apImageSet.PRESET GetModifierIconType(apModifierBase.MODIFIER_TYPE modType)
		{
			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					return apImageSet.PRESET.Modifier_Volume;

				case apModifierBase.MODIFIER_TYPE.Volume:
					return apImageSet.PRESET.Modifier_Volume;

				case apModifierBase.MODIFIER_TYPE.Morph:
					return apImageSet.PRESET.Modifier_Morph;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					return apImageSet.PRESET.Modifier_AnimatedMorph;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					return apImageSet.PRESET.Modifier_Rigging;

				case apModifierBase.MODIFIER_TYPE.Physic:
					return apImageSet.PRESET.Modifier_Physic;

				case apModifierBase.MODIFIER_TYPE.TF:
					return apImageSet.PRESET.Modifier_TF;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					return apImageSet.PRESET.Modifier_AnimatedTF;

				case apModifierBase.MODIFIER_TYPE.FFD:
					return apImageSet.PRESET.Modifier_FFD;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					return apImageSet.PRESET.Modifier_AnimatedFFD;

			}
			return apImageSet.PRESET.Modifier_Volume;
		}

		public static apImageSet.PRESET GetPhysicsPresetIconType(apPhysicsPresetUnit.ICON iconType)
		{
			switch (iconType)
			{
				case apPhysicsPresetUnit.ICON.Cloth1:
					return apImageSet.PRESET.Physic_PresetCloth1;
				case apPhysicsPresetUnit.ICON.Cloth2:
					return apImageSet.PRESET.Physic_PresetCloth2;
				case apPhysicsPresetUnit.ICON.Cloth3:
					return apImageSet.PRESET.Physic_PresetCloth3;
				case apPhysicsPresetUnit.ICON.Flag:
					return apImageSet.PRESET.Physic_PresetFlag;
				case apPhysicsPresetUnit.ICON.Hair:
					return apImageSet.PRESET.Physic_PresetHair;
				case apPhysicsPresetUnit.ICON.Ribbon:
					return apImageSet.PRESET.Physic_PresetRibbon;
				case apPhysicsPresetUnit.ICON.RubberHard:
					return apImageSet.PRESET.Physic_PresetRubberHard;
				case apPhysicsPresetUnit.ICON.RubberSoft:
					return apImageSet.PRESET.Physic_PresetRubberSoft;
				case apPhysicsPresetUnit.ICON.Custom1:
					return apImageSet.PRESET.Physic_PresetCustom1;
				case apPhysicsPresetUnit.ICON.Custom2:
					return apImageSet.PRESET.Physic_PresetCustom2;
				case apPhysicsPresetUnit.ICON.Custom3:
					return apImageSet.PRESET.Physic_PresetCustom3;
			}
			return apImageSet.PRESET.Physic_PresetCustom3;
		}


		public static apImageSet.PRESET GetControlParamPresetIconType(apControlParam.ICON_PRESET iconType)
		{
			switch (iconType)
			{
				case apControlParam.ICON_PRESET.None:
					return apImageSet.PRESET.Hierarchy_Param;
				case apControlParam.ICON_PRESET.Head:
					return apImageSet.PRESET.ParamPreset_Head;
				case apControlParam.ICON_PRESET.Body:
					return apImageSet.PRESET.ParamPreset_Body;
				case apControlParam.ICON_PRESET.Hand:
					return apImageSet.PRESET.ParamPreset_Hand;
				case apControlParam.ICON_PRESET.Face:
					return apImageSet.PRESET.ParamPreset_Face;
				case apControlParam.ICON_PRESET.Eye:
					return apImageSet.PRESET.ParamPreset_Eye;
				case apControlParam.ICON_PRESET.Hair:
					return apImageSet.PRESET.ParamPreset_Hair;
				case apControlParam.ICON_PRESET.Equipment:
					return apImageSet.PRESET.ParamPreset_Equip;
				case apControlParam.ICON_PRESET.Cloth:
					return apImageSet.PRESET.ParamPreset_Cloth;
				case apControlParam.ICON_PRESET.Force:
					return apImageSet.PRESET.ParamPreset_Force;
				case apControlParam.ICON_PRESET.Etc:
					return apImageSet.PRESET.ParamPreset_Etc;
			}
			return apImageSet.PRESET.ParamPreset_Etc;
		}

		public static apControlParam.ICON_PRESET GetControlParamPresetIconTypeByCategory(apControlParam.CATEGORY category)
		{
			switch (category)
			{
				case apControlParam.CATEGORY.Head:
					return apControlParam.ICON_PRESET.Head;
				case apControlParam.CATEGORY.Body:
					return apControlParam.ICON_PRESET.Body;
				case apControlParam.CATEGORY.Face:
					return apControlParam.ICON_PRESET.Face;
				case apControlParam.CATEGORY.Hair:
					return apControlParam.ICON_PRESET.Hair;
				case apControlParam.CATEGORY.Equipment:
					return apControlParam.ICON_PRESET.Equipment;
				case apControlParam.CATEGORY.Force:
					return apControlParam.ICON_PRESET.Force;
				case apControlParam.CATEGORY.Etc:
					return apControlParam.ICON_PRESET.Etc;
			}
			return apControlParam.ICON_PRESET.Etc;

		}

		//----------------------------------------------------------------------------------------------------

		public class NameAndIndexPair
		{
			public string _strName = "";
			public int _index = 0;
			public int _indexStrLength = 0;
			public NameAndIndexPair(string strName, string strIndex)
			{
				_strName = strName;
				if (strIndex.Length > 0)
				{
					_index = Int32.Parse(strIndex);
					_indexStrLength = strIndex.Length;
				}
				else
				{
					_index = 0;
					_indexStrLength = 0;
				}
			}
			public string MakeNewName(int index)
			{
				string strIndex = index + "";
				if (strIndex.Length < _indexStrLength)
				{
					int dLength = _indexStrLength - strIndex.Length;
					//0을 붙여주자
					for (int i = 0; i < dLength; i++)
					{
						strIndex = "0" + strIndex;
					}
				}

				return _strName + strIndex;
			}
		}

		public static NameAndIndexPair ParseNumericName(string srcName)
		{
			if (string.IsNullOrEmpty(srcName))
			{
				return new NameAndIndexPair("<None>", "");
			}

			//1. 이름 내에 "숫자로 된 부분"이 있다면, 그중 가장 "뒤의 숫자"를 1 올려서 갱신한다.
			string strName_First = "", strName_Index = "";
			int strMode = 1;//0 : First, 1 : Index
			for (int i = srcName.Length - 1; i >= 0; i--)
			{
				string curStr = srcName.Substring(i, 1);
				switch (strMode)
				{
					case 1:
						{
							if (IsNumericString(curStr))
							{
								strName_Index = curStr + strName_Index;
							}
							else
							{
								strName_First = curStr + strName_First;
								strMode = 0;
							}
						}
						break;

					case 0:
						strName_First = curStr + strName_First;
						break;
				}
			}
			return new NameAndIndexPair(strName_First, strName_Index);
		}


		private static bool IsNumericString(string str)
		{
			if (str == "0" || str == "1" || str == "2" ||
				str == "3" || str == "4" || str == "5" ||
				str == "6" || str == "7" || str == "8" ||
				str == "9")
			{
				return true;
			}
			return false;
		}


		//---------------------------------------------------------------------------------------
		public static T[] AddItemToArray<T>(T addItem, T[] srcArray)
		{
			if (srcArray == null || srcArray.Length == 0)
			{
				return new T[] { addItem };
			}

			int prevArraySize = srcArray.Length;
			int nextArraySize = prevArraySize + 1;

			T[] nextArray = new T[nextArraySize];
			for (int i = 0; i < prevArraySize; i++)
			{
				nextArray[i] = srcArray[i];
			}
			nextArray[nextArraySize - 1] = addItem;
			return nextArray;
		}



		//---------------------------------------------------------------------------------------
		//private static System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
		//private static string _stopWatchMsg = "";
		//public static void StartCodePerformanceCheck(string stopWatchMsg)
		//{
		//	_stopWatchMsg = stopWatchMsg;
		//	_stopwatch.Reset();
		//	_stopwatch.Start();
		//}

		//public static void StopCodePerformanceCheck()
		//{
		//	_stopwatch.Stop();
		//	long mSec = _stopwatch.ElapsedMilliseconds;
		//	Debug.LogError("Performance [" + _stopWatchMsg + "] : " + (mSec / 1000) + "." + (mSec % 1000) + " secs");
		//	//return _stopwatch.ElapsedTicks + " Ticks";
		//}

	}
}
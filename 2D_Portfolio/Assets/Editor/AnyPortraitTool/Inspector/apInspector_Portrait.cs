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
using UnityEditor;
using UnityEngine.Profiling;

using AnyPortrait;

namespace AnyPortrait
{

	[CustomEditor(typeof(apPortrait))]
	public class apInspector_Portrait : Editor
	{
		private apPortrait _targetPortrait = null;
		private apControlParam.CATEGORY _curControlCategory = apControlParam.CATEGORY.Etc;
		private bool _showBaseInspector = false;
		private List<apControlParam> _controlParams = null;

		void OnEnable()
		{
			_targetPortrait = null;
		}

		public override void OnInspectorGUI()
		{
			//return;

			//base.OnInspectorGUI();
			apPortrait targetPortrait = target as apPortrait;

			if (targetPortrait != _targetPortrait)
			{
				_targetPortrait = targetPortrait;
				Init();
			}
			if (_targetPortrait == null)
			{
				//Profiler.EndSample();
				return;
			}

			Profiler.BeginSample("anyPortrait Inspector GUI");


			//return;
			if (apEditor.IsOpen())
			{
				//에디터가 작동중에는 안보이도록 하자
				EditorGUILayout.LabelField("Editor is opened");

				Profiler.EndSample();

				return;
			}

			try
			{
				EditorGUILayout.LabelField("Control Parameters");
				_curControlCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumMaskPopup(new GUIContent("Category"), _curControlCategory);

				EditorGUILayout.Space();
				//1. 컨르롤러를 제어할 수 있도록 하자
				bool isChanged = false;
				if (_controlParams != null)
				{
					for (int i = 0; i < _controlParams.Count; i++)
					{
						if ((int)(_controlParams[i]._category & _curControlCategory) != 0)
						{
							if (GUI_ControlParam(_controlParams[i]))
							{
								isChanged = true;
							}
						}
					}
				}

				GUILayout.Space(30);

				//2. 토글 버튼을 두어서 기본 Inspector 출력 여부를 결정하자.
				string strBaseButton = "Show All Properties";
				if (_showBaseInspector)
				{
					strBaseButton = "Hide Properties";
				}

				if (GUILayout.Button(strBaseButton, GUILayout.Height(20)))
				{
					_showBaseInspector = !_showBaseInspector;
				}

				if (_showBaseInspector)
				{
					base.OnInspectorGUI();
				}


				if (!Application.isPlaying && isChanged)
				{
					//플레이 중이라면 자동으로 업데이트 될 것이다.
					_targetPortrait.UpdateForce();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("apInspector_Portrait Exception : " + ex);
			}

			Profiler.EndSample();
		}

		private void Init()
		{
			_curControlCategory = apControlParam.CATEGORY.Head |
									apControlParam.CATEGORY.Body |
									apControlParam.CATEGORY.Face |
									apControlParam.CATEGORY.Hair |
									apControlParam.CATEGORY.Equipment |
									apControlParam.CATEGORY.Force |
									apControlParam.CATEGORY.Etc;

			_showBaseInspector = false;

			_controlParams = null;
			if (_targetPortrait._controller != null)
			{
				_controlParams = _targetPortrait._controller._controlParams;
			}
		}

		private bool GUI_ControlParam(apControlParam controlParam)
		{
			if (controlParam == null)
			{ return false; }

			bool isChanged = false;

			EditorGUILayout.LabelField(controlParam._keyName);

			switch (controlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	{
				//		bool bPrev = controlParam._bool_Cur;
				//		controlParam._bool_Cur = EditorGUILayout.Toggle(controlParam._bool_Cur);
				//		if(bPrev != controlParam._bool_Cur)
				//		{
				//			isChanged = true;
				//		}
				//	}
				//	break;

				case apControlParam.TYPE.Int:
					{
						int iPrev = controlParam._int_Cur;
						controlParam._int_Cur = EditorGUILayout.IntSlider(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);

						if (iPrev != controlParam._int_Cur)
						{
							isChanged = true;
						}
					}
					break;

				case apControlParam.TYPE.Float:
					{
						float fPrev = controlParam._float_Cur;
						controlParam._float_Cur = EditorGUILayout.Slider(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);

						if (Mathf.Abs(fPrev - controlParam._float_Cur) > 0.0001f)
						{
							isChanged = true;
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						Vector2 v2Prev = controlParam._vec2_Cur;
						controlParam._vec2_Cur.x = EditorGUILayout.Slider(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						controlParam._vec2_Cur.y = EditorGUILayout.Slider(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);

						if (Mathf.Abs(v2Prev.x - controlParam._vec2_Cur.x) > 0.0001f ||
							Mathf.Abs(v2Prev.y - controlParam._vec2_Cur.y) > 0.0001f)
						{
							isChanged = true;
						}
					}
					break;

			}

			GUILayout.Space(5);

			return isChanged;
		}
	}

}
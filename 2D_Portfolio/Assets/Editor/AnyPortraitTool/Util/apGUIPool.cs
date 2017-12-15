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
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{

	public static class apGUIPool
	{
		//// Members
		////-----------------------------------------------
		//public enum STYLE_TYPE
		//{
		//	None, Label, Button
		//}
		//private static Dictionary<STYLE_TYPE, List<GUIStyle>> _guiStyle_Remained = new Dictionary<STYLE_TYPE, List<GUIStyle>>();
		//private static Dictionary<STYLE_TYPE, List<GUIStyle>> _guiStyle_Using = new Dictionary<STYLE_TYPE, List<GUIStyle>>();
		//private static Dictionary<STYLE_TYPE, List<GUIStyle>> _guiStyle_Total = new Dictionary<STYLE_TYPE, List<GUIStyle>>();

		//private static List<GUIContent> _guiContent_Remained = new List<GUIContent>();
		//private static List<GUIContent> _guiContent_Using = new List<GUIContent>();
		//private static List<GUIContent> _guiContent_Total = new List<GUIContent>();

		//private static bool _isInit = false;
		//private const int NUM_INIT_STYLE = 64;
		//private const int NUM_INIT_CONTENT = 64;
		//private const int NUM_ENUM = 3;


		//// Init
		////-----------------------------------------------
		//public static void Init()
		//{
		//	if(_isInit)
		//	{
		//		return;
		//	}

		//	_guiStyle_Remained.Clear();
		//	_guiStyle_Using.Clear();
		//	_guiStyle_Total.Clear();

		//	_guiContent_Remained.Clear();
		//	_guiContent_Using.Clear();
		//	_guiContent_Total.Clear();

		//	for (int i = 0; i < NUM_ENUM; i++)
		//	{
		//		STYLE_TYPE styleType = (STYLE_TYPE)i;

		//		_guiStyle_Remained.Add(styleType, new List<GUIStyle>());
		//		_guiStyle_Using.Add(styleType, new List<GUIStyle>());
		//		_guiStyle_Total.Add(styleType, new List<GUIStyle>());

		//		GUIStyle refStyle = null;
		//		switch (styleType)
		//		{
		//			case STYLE_TYPE.None:
		//				refStyle = GUI.skin.name;
		//				break;

		//			case STYLE_TYPE.Label:
		//				refStyle = GUI.skin.label;
		//				break;

		//			case STYLE_TYPE.Button:
		//				refStyle = GUI.skin.button;
		//				break;
		//		}
		//		for (int iUnit = 0; iUnit < NUM_INIT_STYLE; iUnit++)
		//		{
		//			GUIStyle guiStyle = new GUIStyle(refStyle);

		//			//Remain + Total에 넣는다.
		//			_guiStyle_Remained[styleType].Add(guiStyle);
		//			_guiStyle_Total[styleType].Add(guiStyle);
		//		}
		//	}

		//	for (int i = 0; i < NUM_INIT_CONTENT; i++)
		//	{
		//		GUIContent guiContent = new GUIContent();

		//		_guiContent_Remained.Add(guiContent);
		//		_guiContent_Total.Add(guiContent);
		//	}

		//	_isInit = true;
		//}

		//public static void Reset()
		//{
		//	for (int i = 0; i < NUM_ENUM; i++)
		//	{
		//		STYLE_TYPE styleType = (STYLE_TYPE)i;

		//		_guiStyle_Remained[styleType].Clear();
		//		_guiStyle_Using[styleType].Clear();
		//		int nStyle = _guiStyle_Total[styleType].Count;
		//		for (int iUnit = 0; iUnit < nStyle; iUnit++)
		//		{
		//			_guiStyle_Remained[styleType].Add(_guiStyle_Total[styleType][i]);
		//		}
		//	}

		//	_guiContent_Remained.Clear();
		//	_guiContent_Using.Clear();

		//	int nContent = _guiContent_Total.Count;
		//	for (int i = 0; i < nContent; i++)
		//	{
		//		_guiContent_Remained.Add(_guiContent_Total[i]);
		//	}

		//}


		//// Push / Pop
		////-----------------------------------------------
		//private static GUIStyle PopStyle(STYLE_TYPE styleType)
		//{
		//	GUIStyle refStyle = null;
		//	switch (styleType)
		//	{
		//		case STYLE_TYPE.None:
		//			refStyle = GUI.skin.name;
		//			break;

		//		case STYLE_TYPE.Label:
		//			refStyle = GUI.skin.label;
		//			break;

		//		case STYLE_TYPE.Button:
		//			refStyle = GUI.skin.button;
		//			break;
		//	}

		//	GUIStyle popUnit = null;
		//	if(_guiStyle_Remained[styleType].Count > 0)
		//	{
		//		popUnit = _guiStyle_Remained[styleType][0];
		//		_guiStyle_Remained[styleType].RemoveAt(0);
		//	}
		//	else
		//	{
		//		popUnit = new GUIStyle(refStyle);
		//		_guiStyle_Total[styleType].Add(popUnit);
		//	}

		//	popUnit.alignment =			refStyle.alignment;
		//	popUnit.border =			refStyle.border;
		//	popUnit.normal.textColor =	refStyle.normal.textColor;
		//	popUnit.onHover.textColor =	refStyle.onHover.textColor;
		//	popUnit.margin =			refStyle.margin;

		//	_guiStyle_Using[styleType].Add(popUnit);
		//	return popUnit;
		//}

		//public static GUIStyle PopLabel()
		//{
		//	return PopStyle(STYLE_TYPE.Label);
		//}

		//public static GUIStyle PopButton()
		//{
		//	return PopStyle(STYLE_TYPE.Button);
		//}

		//public static GUIStyle PopNone()
		//{
		//	return PopStyle(STYLE_TYPE.None);
		//}
		////Push는 없애고 일괄적 리턴만 하자

		//private static GUIContent PopContent()
		//{
		//	GUIContent popContent = null;
		//	if(_guiContent_Remained.Count > 0)
		//	{
		//		popContent = _guiContent_Remained[0];
		//		_guiContent_Remained.RemoveAt(0);
		//	}
		//	else
		//	{
		//		popContent = new GUIContent();
		//		_guiContent_Total.Add(popContent);
		//	}

		//	_guiContent_Using.Add(popContent);

		//	return popContent;
		//}

		//public static GUIContent PopContent(Texture image)
		//{
		//	GUIContent popContent = PopContent();
		//	popContent.image = image;
		//	return popContent;
		//}

		//public static GUIContent PopContent(string text)
		//{
		//	GUIContent popContent = PopContent();
		//	popContent.text = text;
		//	return popContent;
		//}

		//public static GUIContent PopContent(string text, Texture image)
		//{
		//	GUIContent popContent = PopContent();
		//	popContent.image = image;
		//	popContent.text = text;
		//	return popContent;
		//}

		//public static GUIContent PopContent(Texture image, string toolTip)
		//{
		//	GUIContent popContent = PopContent();
		//	popContent.image = image;
		//	popContent.tooltip = toolTip;
		//	return popContent;
		//}

		//public static GUIContent PopContent(string text, string toolTip)
		//{
		//	GUIContent popContent = PopContent();
		//	popContent.text = text;
		//	popContent.tooltip = toolTip;
		//	return popContent;
		//}

		//public static GUIContent PopContent(string text, Texture image, string toolTip)
		//{
		//	GUIContent popContent = PopContent();
		//	popContent.image = image;
		//	popContent.text = text;
		//	popContent.tooltip = toolTip;
		//	return popContent;
		//}

	}
}
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

using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// 텍스트를 설정에 맞게 번역하는 클래스
	/// Editor의 멤버로 존재하며, Editor에서 Language 옵션을 넣어준다.
	/// </summary>
	public class apLocalization
	{
		// Member
		//------------------------------------------------
		//텍스트를 받는다.
		public enum TEXT
		{
			None = 0,
			Cancel = 1,
			Close = 2,
			Okay = 3,
			Remove = 4,
			Detach_Title = 5,
			Detach_Body = 6,
			Detach_Ok = 7,
			ThumbCreateFailed_Title = 8,
			ThumbCreateFailed_Body_NoFile = 9,
			GIFFailed_Title = 10,
			GIFFailed_Body_Reject = 11,
			PSDBakeError_Title_WrongDst = 12,
			PSDBakeError_Body_WrongDst = 13,
			PSDBakeError_Title_Load = 14,
			PSDBakeError_Body_LoadPath = 15,
			PSDBakeError_Body_LoadSize = 16,
			PSDBakeError_Body_ErrorCode = 17,
			AddTextureFailed_Title = 18,
			AddTextureFailed_Body = 19,
			MeshCreationFailed_Title = 20,
			MeshCreationFailed_Body = 21,
			MeshAddFailed_Title = 22,
			MeshAddFailed_Body = 23,
			AnimCreateFailed_Title = 24,
			AnimCreateFailed_Body = 25,
			AnimDuplicatedFailed_Title = 26,
			AnimDuplicatedFailed_Body = 27,
			AnimTimelineAddFailed_Title = 28,
			AnimTimelineAddFailed_Body = 29,
			AnimTimelineLayerAddFailed_Title = 30,
			AnimTimelineLayerAddFailed_Body = 31,
			AnimKeyframeAddFailed_Title = 32,
			AnimKeyframeAddFailed_Body_Already = 33,
			AnimKeyframeAddFailed_Body_Error = 34,
			MeshGroupAddFailed_Title = 35,
			MeshGroupAddFailed_Body = 36,
			BoneAddFailed_Title = 37,
			BoneAddFailed_Body = 38,
			MeshAttachFailed_Title = 39,
			MeshAttachFailed_Body = 40,
			MeshGroupAttachFailed_Title = 41,
			MeshGroupAttachFailed_Body = 42,
			ModifierAddFailed_Title = 43,
			ModifierAddFailed_Body = 44,
			ControlParamNameError_Title = 45,
			ControlParamNameError_Body_Wrong = 46,
			ControlParamNameError_Body_Used = 47,
			IKOption_Title = 48,
			IKOption_Body_Chained = 49,
			IKOption_Body_Head = 50,
			IKOption_Body_Single = 51,
			PhysicPreset_Regist_Title = 52,
			PhysicPreset_Regist_Body = 53,
			PhysicPreset_Regist_Okay = 54,
			PhysicPreset_Remove_Title = 55,
			PhysicPreset_Remove_Body = 56,
			ResetPSDImport_Title = 57,
			ResetPSDImport_Body = 58,
			ResetPSDImport_Okay = 59,
			ClosePSDImport_Title = 60,
			ClosePSDImport_Body = 61,
			MeshEditChanged_Title = 62,
			MeshEditChanged_Body = 63,
			MeshEditChanged_Okay = 64,
			ControlParamDefaultAll_Title = 65,
			ControlParamDefaultAll_Body = 66,
			ControlParamDefaultAll_Okay = 67,
			RemoveRecordKey_Title = 68,
			RemoveRecordKey_Body = 69,
			AdaptFFDTransformEdit_Title = 70,
			AdaptFFDTransformEdit_Body = 71,
			AdaptFFDTransformEdit_Okay = 72,
			AdaptFFDTransformEdit_No = 73,
			RemoveImage_Title = 74,
			RemoveImage_Body = 75,
			RemoveAnimClip_Title = 76,
			RemoveAnimClip_Body = 77,
			AnimClipMeshGroupChanged_Title = 78,
			AnimClipMeshGroupChanged_Body = 79,
			RemoveControlParam_Title = 80,
			RemoveControlParam_Body = 81,
			ResetMeshVertices_Title = 82,
			ResetMeshVertices_Body = 83,
			ResetMeshVertices_Okay = 84,
			RemoveMesh_Title = 85,
			RemoveMesh_Body = 86,
			RemoveMeshVertices_Title = 87,
			RemoveMeshVertices_Body = 88,
			RemoveMeshVertices_Okay = 89,
			RemoveMeshGroup_Title = 90,
			RemoveMeshGroup_Body = 91,
			RemoveBonesAll_Title = 92,
			RemoveBonesAll_Body = 93,
			RemoveKeyframes_Title = 94,
			RemoveKeyframes_Body = 95,
			DetachChildBone_Title = 96,
			DetachChildBone_Body = 97,
			RemoveModifier_Title = 98,
			RemoveModifier_Body = 99,
			RemoveFromKeys_Title = 100,
			RemoveFromKeys_Body = 101,
			RemoveFromRigging_Title = 102,
			RemoveFromRigging_Body = 103,
			RemoveFromPhysics_Title = 104,
			RemoveFromPhysics_Body = 105,
			AddAllObjects2Timeline_Title = 106,
			AddAllObjects2Timeline_Body = 107,
			RemoveTimeline_Title = 108,
			RemoveTimeline_Body = 109,
			RemoveTimelineLayer_Title = 110,
			RemoveTimelineLayer_Body = 111,
			DemoLimitation_Title = 112,
			DemoLimitation_Body = 113,
			DemoLimitation_Body_AddParam = 114,
			DemoLimitation_Body_AddAnimation = 115,
		}




		private bool _isLoaded = false;
		public bool IsLoaded { get { return _isLoaded; } }
		private apEditor.LANGUAGE _language = apEditor.LANGUAGE.English;
		public apEditor.LANGUAGE Language { get { return _language; } }


		private class TextSet
		{
			public TEXT _textType = TEXT.None;
			public Dictionary<apEditor.LANGUAGE, string> _textSet = new Dictionary<apEditor.LANGUAGE, string>();

			public TextSet(TEXT textType)
			{
				_textType = textType;
			}

			public void SetText(apEditor.LANGUAGE language, string text)
			{
				text = text.Replace("\t", "");
				text = text.Replace("[]", "\r\n");
				text = text.Replace("[c]", ",");
				text = text.Replace("[u]", "\"");


				//Debug.Log("언어팩 : " + language + " : " + text);
				_textSet.Add(language, text);
			}
		}
		private Dictionary<TEXT, TextSet> _textSets = new Dictionary<TEXT, TextSet>();

		// Function
		//------------------------------------------------
		public apLocalization()
		{
			_isLoaded = false;
			_textSets.Clear();
		}
		public void SetTextAsset(TextAsset textAsset)
		{
			if (_isLoaded)
			{
				return;
			}
			_textSets.Clear();
			string[] strParseLines = textAsset.text.Split(new string[] { "\n" }, StringSplitOptions.None);
			string strCurParseLine = null;
			for (int i = 1; i < strParseLines.Length; i++)
			{
				//첫줄(index 0)은 빼고 읽는다.
				strCurParseLine = strParseLines[i].Replace("\r", "");
				string[] strSubParseLine = strCurParseLine.Split(new string[] { "," }, StringSplitOptions.None);
				//Parse 순서
				//0 : TEXT 타입 (string) - 파싱 안한다.
				//1 : TEXT 타입 (int)
				//2 : English (영어)
				//3 : Korean (한국어)
				//4 : French (프랑스어)
				//5 : German (독일어)
				//6 : Spanish (스페인어)
				//7 : Italian (이탈리아어)
				//8 : Danish (덴마크어)
				//9 : Japanese (일본어)
				//10 : Chinese_Traditional (중국어-번체)
				//11 : Chinese_Simplified (중국어-간체)
				if (strSubParseLine.Length < 12)
				{
					//Debug.LogError("인식할 수 없는 Text (" + i + " : " + strCurParseLine + ")");
					continue;
				}
				try
				{
					TEXT textType = (TEXT)(int.Parse(strSubParseLine[1]));
					TextSet newTextSet = new TextSet(textType);

					newTextSet.SetText(apEditor.LANGUAGE.English, strSubParseLine[2]);
					newTextSet.SetText(apEditor.LANGUAGE.Korean, strSubParseLine[3]);
					newTextSet.SetText(apEditor.LANGUAGE.French, strSubParseLine[4]);
					newTextSet.SetText(apEditor.LANGUAGE.German, strSubParseLine[5]);
					newTextSet.SetText(apEditor.LANGUAGE.Spanish, strSubParseLine[6]);
					newTextSet.SetText(apEditor.LANGUAGE.Italian, strSubParseLine[7]);
					newTextSet.SetText(apEditor.LANGUAGE.Danish, strSubParseLine[8]);
					newTextSet.SetText(apEditor.LANGUAGE.Japanese, strSubParseLine[9]);
					newTextSet.SetText(apEditor.LANGUAGE.Chinese_Traditional, strSubParseLine[10]);
					newTextSet.SetText(apEditor.LANGUAGE.Chinese_Simplified, strSubParseLine[11]);

					_textSets.Add(textType, newTextSet);
				}
				catch (Exception ex)
				{
					Debug.LogError("Parsing 실패 (" + i + " : " + strCurParseLine + ")");
				}


			}



			_isLoaded = true;
		}
		public void SetLanguage(apEditor.LANGUAGE language)
		{
			_language = language;
		}

		public string GetText(TEXT textType)
		{
			return (_textSets[textType])._textSet[_language];
		}
	}

}
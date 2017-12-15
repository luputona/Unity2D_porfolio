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

	public static class apTimelineGL
	{
		private static int _layoutPosX = 0;
		private static int _layoutPosY_Header = 0;
		private static int _layoutPosY_Main = 0;
		private static int _layoutWidth = 0;
		private static int _layoutHeight_Header = 0;
		private static int _layoutHeight_Main = 0;
		private static int _layoutHeight_Total = 0;
		private static Vector4 _glScreenClippingSize_Total = Vector4.zero;
		private static Vector4 _glScreenClippingSize_Main = Vector4.zero;
		private static Vector4 _glScreenClippingSize_Header = Vector4.zero;
		private static Vector2 _scrollPos = Vector2.zero;

		private static apGL.MaterialBatch _matBatch_Total = new apGL.MaterialBatch();
		private static apGL.MaterialBatch _matBatch_Main = new apGL.MaterialBatch();
		private static apGL.MaterialBatch _matBatch_Header = new apGL.MaterialBatch();


		private static Texture2D _img_Keyframe = null;
		private static Texture2D _img_KeyframeDummy = null;
		private static Texture2D _img_KeySummary = null;
		private static Texture2D _img_PlayBarHead = null;
		private static Texture2D _img_KeyLoopLeft = null;
		private static Texture2D _img_KeyLoopRight = null;
		private static Texture2D _img_CurKeyframe = null;
		private static Texture2D _img_KeyframeCursor = null;
		//이동/복사용
		private static Texture2D _img_KeyframeMoveSrc = null;
		private static Texture2D _img_KeyframeMove = null;
		private static Texture2D _img_KeyframeCopy = null;

		private static Texture2D _img_TimelineBGStart = null;
		private static Texture2D _img_TimelineBGEnd = null;


		private static bool _isMouseEvent = false;
		private static bool _isMouseEventUsed = false;
		private static apMouse.MouseBtnStatus _leftBtnStatus = apMouse.MouseBtnStatus.Released;
		private static apMouse.MouseBtnStatus _rightBtnStatus = apMouse.MouseBtnStatus.Released;
		private static Vector2 _mousePos = Vector2.zero;
		private static Vector2 _mousePos_Down = Vector2.zero;
		private static Vector2 _scroll_Down = Vector2.zero;

		private static float _keyDragStartPos = 0.0f;
		private static float _keyDragCurPos = 0.0f;
		private static int _keyDragFrameIndex_Down = -1;
		private static int _keyDragFrameIndex_Cur = -1;

		private static EventType _curEventType;
		private static apSelection _selection;

		private static bool _isShift = false;
		private static bool _isCtrl = false;
		private static bool _isAlt = false;


		private static bool _isMainVisible = false;

		public enum CLIP_TYPE
		{
			Header,
			Main,
			Total
		}

		private static GUIStyle _textStyle = GUIStyle.none;

		//마우스 이벤트
		private enum TIMELINE_EVENT
		{
			None,//아무것도 안하고 있다.
			Select,//빈칸에서 클릭 -> 영역 선택
			ReadyToDrag,//선택을 하였으나 계속 마우스 입력이 되는 중
			DragFrame,//키 프레임 선택 -> 드래그 이동
			DragPlayBar,//플레이 바 선택 -> 드래그 이동
		}

		private static TIMELINE_EVENT _timelineEvent = TIMELINE_EVENT.None;

		private enum SELECT_TYPE
		{
			New, Add, Subtract
		}

		private static SELECT_TYPE _selectType = SELECT_TYPE.New;
		private static CLIP_TYPE _downClipArea = CLIP_TYPE.Main;

		//외부에서 입력이 들어온 상태에서는 입력을 무시하고 있어야 한다.
		private static bool _isMouseInputIgnored = false;

		//private enum KEYFRAME_STATUS { Inactive, Normal, Selected, Working, }

		private static Color _keyColor_Normal = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		private static Color _keyColor_Selected = new Color(1.0f, 0.1f, 0.1f, 1.0f);
		private static Color _keyColor_Working = new Color(0.1f, 0.3f, 1.0f, 1.0f);
		private static Color _keyColor_Inactive = new Color(0.3f, 0.3f, 0.3f, 1.0f);

		private static Color _keyColor_Copy = new Color(1.0f, 0.1f, 1.0f, 1.0f);
		private static Color _keyColor_Move = new Color(0.1f, 1.0f, 0.1f, 1.0f);
		private static Color _keyColor_Copy_Line = new Color(1.0f, 0.1f, 1.0f, 0.7f);
		private static Color _keyColor_Move_Line = new Color(0.1f, 1.0f, 0.1f, 0.7f);


		//Keyframe을 이동/복사 할때
		//한번에 하는게 아니라 임시 데이터를 만들어서 
		//드래그 중에는 임시 값을 이용한다.
		private class MoveKeyframe
		{
			public apAnimKeyframe _srcKeyframe = null;
			public int _startFrameIndex = -1;//처음 데이터가 입력되었을 때의 FrameIndex
			public int _nextFrameIndex = -1;
			public apAnimTimelineLayer ParentLayer { get { return _srcKeyframe._parentTimelineLayer; } }

			public MoveKeyframe(apAnimKeyframe srcKeyframe)
			{
				_srcKeyframe = srcKeyframe;
				_startFrameIndex = _srcKeyframe._frameIndex;
				_nextFrameIndex = _startFrameIndex;
			}
		}
		private static List<MoveKeyframe> _moveKeyframeList = new List<MoveKeyframe>();
		private static MoveKeyframe GetMoveKeyframe(apAnimKeyframe srcKeyframe)
		{
			if (_moveKeyframeList.Count == 0)
			{
				return null;
			}

			return _moveKeyframeList.Find(delegate (MoveKeyframe a)
			{
				return a._srcKeyframe == srcKeyframe;
			});
		}

		// Init
		//-------------------------------------------------------------------------------------------------------
		public static void SetShader(Shader shader_Color,
									Shader[] shader_Texture_Normal_Set,
									Shader[] shader_Texture_VColorAdd_Set,
									Shader[] shader_MaskedTexture_Set,
									Shader shader_MaskOnly,
									Shader[] shader_Clipped_Set)
		{
			_matBatch_Total.SetShader(shader_Color, shader_Texture_Normal_Set, shader_Texture_VColorAdd_Set, shader_MaskedTexture_Set, shader_MaskOnly, shader_Clipped_Set);
			_matBatch_Main.SetShader(shader_Color, shader_Texture_Normal_Set, shader_Texture_VColorAdd_Set, shader_MaskedTexture_Set, shader_MaskOnly, shader_Clipped_Set);
			_matBatch_Header.SetShader(shader_Color, shader_Texture_Normal_Set, shader_Texture_VColorAdd_Set, shader_MaskedTexture_Set, shader_MaskOnly, shader_Clipped_Set);
		}

		public static void SetTexture(Texture2D img_KeyFrame,
										Texture2D img_KeyFrameDummy,
										Texture2D img_KeySummary,
										Texture2D img_PlayBarHead,
										Texture2D img_KeyLoopLeft,
										Texture2D img_KeyLoopRight,
										Texture2D img_TimelineBGStart,
										Texture2D img_TimelineBGEnd,
										Texture2D img_CurKeyframe,
										Texture2D img_KeyframeCursor,
										Texture2D img_KeyframeMoveSrc,
										Texture2D img_KeyframeMove,
										Texture2D img_KeyframeCopy
										)
		{
			_img_Keyframe = img_KeyFrame;
			_img_KeyframeDummy = img_KeyFrameDummy;
			_img_KeySummary = img_KeySummary;
			_img_PlayBarHead = img_PlayBarHead;
			_img_KeyLoopLeft = img_KeyLoopLeft;
			_img_KeyLoopRight = img_KeyLoopRight;
			_img_CurKeyframe = img_CurKeyframe;
			_img_KeyframeCursor = img_KeyframeCursor;

			_img_KeyframeMoveSrc = img_KeyframeMoveSrc;
			_img_KeyframeMove = img_KeyframeMove;
			_img_KeyframeCopy = img_KeyframeCopy;

			_img_TimelineBGStart = img_TimelineBGStart;
			_img_TimelineBGEnd = img_TimelineBGEnd;
		}

		public static void SetLayoutSize(int layoutWidth, int layoutHeight_Header, int layoutHeight_Main,
											int posX, int posY_Header, int posY_Main,
											int totalEditorWidth, int totalEditorHeight,
											bool isMainVisible,
											Vector2 scrollPos)
		{
			_layoutPosX = posX;
			_layoutPosY_Header = posY_Header;
			_layoutPosY_Main = posY_Main;
			_layoutWidth = layoutWidth;
			_layoutHeight_Header = layoutHeight_Header + 1;
			_layoutHeight_Main = layoutHeight_Main + 2;

			_layoutHeight_Total = _layoutHeight_Header + _layoutHeight_Main;

			_isMainVisible = isMainVisible;

			_scrollPos = scrollPos;

			//원래는 30
			totalEditorHeight += 28;
			posY_Header += 28;
			posY_Main += 28;

			posX += 5;
			//layoutWidth -= 25;
			layoutWidth -= 17;
			//layoutHeight -= 20; //?

			//헤더
			_glScreenClippingSize_Header.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize_Header.y = (float)(posY_Header) / (float)totalEditorHeight;
			_glScreenClippingSize_Header.z = (float)(posX + layoutWidth) / (float)totalEditorWidth;
			_glScreenClippingSize_Header.w = (float)(posY_Header + _layoutHeight_Header) / (float)totalEditorHeight;

			//메인
			_glScreenClippingSize_Main.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize_Main.y = (float)(posY_Main) / (float)totalEditorHeight;
			_glScreenClippingSize_Main.z = (float)(posX + layoutWidth) / (float)totalEditorWidth;
			_glScreenClippingSize_Main.w = (float)(posY_Main + _layoutHeight_Main) / (float)totalEditorHeight;

			//전체
			_glScreenClippingSize_Total.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize_Total.y = (float)(posY_Header) / (float)totalEditorHeight;
			_glScreenClippingSize_Total.z = (float)(posX + layoutWidth) / (float)totalEditorWidth;
			_glScreenClippingSize_Total.w = (float)(posY_Header + _layoutHeight_Total) / (float)totalEditorHeight;

			//_isNeedPreRender = true;
			_isMouseEvent = false;
			_isMouseEventUsed = false;
		}


		public static void SetMouseValue(bool isLeftBtnPressed,
											bool isRightBtnPressed,
											Vector2 mousePos,
											bool isShift, bool isCtrl, bool isAlt,
											EventType curEventType,
											apSelection selection)
		{
			_isMouseEvent = true;
			_isMouseEventUsed = false;

			_mousePos = mousePos;

			_mousePos.x -= _layoutPosX;
			_mousePos.y -= _layoutPosY_Header;

			_isShift = isShift;
			_isCtrl = isCtrl;
			_isAlt = isAlt;

			bool isMouseEvent = (Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseMove ||
				Event.current.rawType == EventType.MouseUp ||
				Event.current.rawType == EventType.MouseDrag);
			if (isMouseEvent)
			{
				if (isLeftBtnPressed || isRightBtnPressed)
				{
					if (_isMouseInputIgnored)
					{
						//무시..
					}
					else
					{
						//첫 클릭때 체크하자
						bool isMouseDown = false;
						if (isLeftBtnPressed && (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released))
						{
							isMouseDown = true;
						}

						if (isRightBtnPressed && (_rightBtnStatus == apMouse.MouseBtnStatus.Up || _rightBtnStatus == apMouse.MouseBtnStatus.Released))
						{
							isMouseDown = true;
						}

						//막 눌리기 시작했을때
						if (isMouseDown)
						{
							if (IsMouseInLayout(_mousePos))
							{
								//패스
							}
							else
							{
								_isMouseInputIgnored = true;//<<이 부분에서 마우스 입력이 시작되지 않았다. 다음 업데이트는 무시
							}
						}
					}
				}
				else
				{
					if (_isMouseInputIgnored)
					{
						_isMouseInputIgnored = false;
					}
				}
			}



			if (isLeftBtnPressed)
			{
				if (_leftBtnStatus == apMouse.MouseBtnStatus.Down || _leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					_leftBtnStatus = apMouse.MouseBtnStatus.Pressed;
				}
				else
				{
					_leftBtnStatus = apMouse.MouseBtnStatus.Down;
				}
			}
			else
			{
				if (_leftBtnStatus == apMouse.MouseBtnStatus.Down || _leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					_leftBtnStatus = apMouse.MouseBtnStatus.Up;
				}
				else
				{
					_leftBtnStatus = apMouse.MouseBtnStatus.Released;
				}
			}


			if (isRightBtnPressed)
			{
				if (_rightBtnStatus == apMouse.MouseBtnStatus.Down || _rightBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					_rightBtnStatus = apMouse.MouseBtnStatus.Pressed;
				}
				else
				{
					_rightBtnStatus = apMouse.MouseBtnStatus.Down;
				}
			}
			else
			{
				if (_rightBtnStatus == apMouse.MouseBtnStatus.Down || _rightBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					_rightBtnStatus = apMouse.MouseBtnStatus.Up;
				}
				else
				{
					_rightBtnStatus = apMouse.MouseBtnStatus.Released;
				}
			}





			if (_isShift || _isCtrl)
			{
				_selectType = SELECT_TYPE.Add;
			}
			else if (_isAlt)
			{
				_selectType = SELECT_TYPE.Subtract;
			}
			else
			{
				_selectType = SELECT_TYPE.New;
			}

			_curEventType = curEventType;
			_selection = selection;

			if (curEventType != EventType.MouseDown &&
				curEventType != EventType.MouseDrag &&
				curEventType != EventType.MouseMove &&
				curEventType != EventType.MouseUp)
			{
				_isMouseEvent = false;
			}

			if (_selection.SelectionType != apSelection.SELECTION_TYPE.Animation)
			{
				_isMouseEvent = false;
			}

			if (_isMouseInputIgnored)
			{
				_isMouseEvent = false;
				_isMouseEventUsed = true;
			}
		}


		public static void SetMouseUse()
		{
			_isMouseEventUsed = true;
		}

		//public static void UpdateMouseEvent()
		//{
		//	if(_isMouseEventUsed || !_isMouseEvent
		//		|| _selection == null
		//		|| _selection.AnimClip == null)
		//	{
		//		_timelineEvent = TIMELINE_EVENT.None;//걍 취소
		//		return;
		//	}

		//	switch (_timelineEvent)
		//	{
		//		case TIMELINE_EVENT.None:
		//			{
		//				if(_leftBtnStatus == apMouse.MouseBtnStatus.Down)
		//				{
		//					//Down 체크 순서
		//					//PlayBar -> Frame -> 빈칸

		//				}
		//			}
		//			break;

		//		case TIMELINE_EVENT.Select:
		//			{

		//			}
		//			break;

		//		case TIMELINE_EVENT.DragFrame:
		//			{

		//			}
		//			break;

		//		case TIMELINE_EVENT.DragPlayBar:
		//			{

		//			}
		//			break;
		//	}
		//}


		//-------------------------------------------------------------------------------------------------------
		private static apGL.MaterialBatch GetMatBatch(CLIP_TYPE clipType)
		{
			switch (clipType)
			{
				case CLIP_TYPE.Total:
					return _matBatch_Total;
				case CLIP_TYPE.Main:
					return _matBatch_Main;
				case CLIP_TYPE.Header:
					return _matBatch_Header;
			}
			return null;
		}

		// Draw Line
		//-------------------------------------------------------------------------------------------------------
		public static void DrawLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null)
			{ return; }
			if (matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1, pos2))
			{
				return;
			}

			if (isNeedResetMat)
			{
				matBatch.SetPass_Color();
				switch (clipType)
				{
					case CLIP_TYPE.Header:
						matBatch.SetClippingSize(_glScreenClippingSize_Header);
						break;

					case CLIP_TYPE.Main:
						matBatch.SetClippingSize(_glScreenClippingSize_Main);
						break;

					case CLIP_TYPE.Total:
						matBatch.SetClippingSize(_glScreenClippingSize_Total);
						break;
				}


				GL.Begin(GL.LINES);


			}

			GL.Color(color);
			GL.Vertex(new Vector3(pos1.x, pos1.y, 0.0f));
			GL.Vertex(new Vector3(pos2.x, pos2.y, 0.0f));

			if (isNeedResetMat)
			{
				GL.End();
			}
		}

		// Draw Box
		//---------------------------------------------------------------------------------------------------------
		public static void DrawBox(Vector2 pos, float width, float height, Color color, bool isNeedResetMat, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null)
			{ return; }
			if (matBatch.IsNotReady())
			{ return; }

			float halfWidth = width * 0.5f;
			float halfHeight = height * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector3 pos_0 = new Vector3(pos.x - halfWidth, pos.y - halfHeight, 0);
			Vector3 pos_1 = new Vector3(pos.x + halfWidth, pos.y - halfHeight, 0);
			Vector3 pos_2 = new Vector3(pos.x + halfWidth, pos.y + halfHeight, 0);
			Vector3 pos_3 = new Vector3(pos.x - halfWidth, pos.y + halfHeight, 0);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2

			if (isNeedResetMat)
			{
				matBatch.SetPass_Color();
				switch (clipType)
				{
					case CLIP_TYPE.Header:
						matBatch.SetClippingSize(_glScreenClippingSize_Header);
						break;

					case CLIP_TYPE.Main:
						matBatch.SetClippingSize(_glScreenClippingSize_Main);
						break;

					case CLIP_TYPE.Total:
						matBatch.SetClippingSize(_glScreenClippingSize_Total);
						break;
				}

				GL.Begin(GL.TRIANGLES);
			}
			GL.Color(color);
			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_2); // 2

			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_0); // 0

			if (isNeedResetMat)
			{
				GL.End();
			}
		}



		// Draw Texture
		//--------------------------------------------------------------------------------------------------
		public static void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, bool isNeedResetMat, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null)
			{ return; }
			if (matBatch.IsNotReady())
			{ return; }

			float realWidth = width;
			float realHeight = height;

			float realWidth_Half = realWidth * 0.5f;
			float realHeight_Half = realHeight * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_1 = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_2 = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
			Vector2 pos_3 = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);

			float widthResize = (pos_1.x - pos_0.x);
			float heightResize = (pos_3.y - pos_0.y);

			if (widthResize < 1.0f || heightResize < 1.0f)
			{
				return;
			}

			float u_left = 0.0f;
			float u_right = 1.0f;

			float v_top = 0.0f;
			float v_bottom = 1.0f;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			if (isNeedResetMat)
			{
				matBatch.SetPass_Texture_Normal(color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
				switch (clipType)
				{
					case CLIP_TYPE.Header:
						matBatch.SetClippingSize(_glScreenClippingSize_Header);
						break;

					case CLIP_TYPE.Main:
						matBatch.SetClippingSize(_glScreenClippingSize_Main);
						break;

					case CLIP_TYPE.Total:
						matBatch.SetClippingSize(_glScreenClippingSize_Total);
						break;
				}

				GL.Begin(GL.TRIANGLES);
			}

			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0
			GL.TexCoord(uv_1);
			GL.Vertex(new Vector3(pos_1.x, pos_1.y, 0)); // 1
			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2

			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2
			GL.TexCoord(uv_3);
			GL.Vertex(new Vector3(pos_3.x, pos_3.y, 0)); // 3
			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0

			if (isNeedResetMat)
			{
				GL.End();
			}

			//GL.Flush();
		}


		public static void DrawBoldLine(Vector2 pos1, Vector2 pos2, float width, Color color, bool isNeedResetMat, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = null;
			if (isNeedResetMat)
			{
				matBatch = GetMatBatch(clipType);
				if (matBatch == null)
				{ return; }
				if (matBatch.IsNotReady())
				{ return; }
			}

			if (pos1 == pos2)
			{
				return;
			}

			//float halfWidth = width * 0.5f / _zoom;
			float halfWidth = width * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)

			// -------->
			// |    1
			// | 0     2
			// | 
			// | 
			// | 
			// | 5     3
			// |    4

			Vector2 dir = (pos1 - pos2).normalized;
			Vector2 dirRev = new Vector2(-dir.y, dir.x);

			Vector2 pos_0 = pos1 - dirRev * halfWidth;
			Vector2 pos_1 = pos1 + dir * halfWidth;
			//Vector2 pos_1 = pos1;
			Vector2 pos_2 = pos1 + dirRev * halfWidth;

			Vector2 pos_3 = pos2 + dirRev * halfWidth;
			Vector2 pos_4 = pos2 - dir * halfWidth;
			//Vector2 pos_4 = pos2;
			Vector2 pos_5 = pos2 - dirRev * halfWidth;

			if (isNeedResetMat)
			{
				//_mat_Color.SetPass(0);
				//_mat_Color.SetVector("_ScreenSize", _glScreenClippingSize);
				matBatch.SetPass_Color();
				switch (clipType)
				{
					case CLIP_TYPE.Header:
						matBatch.SetClippingSize(_glScreenClippingSize_Header);
						break;

					case CLIP_TYPE.Main:
						matBatch.SetClippingSize(_glScreenClippingSize_Main);
						break;

					case CLIP_TYPE.Total:
						matBatch.SetClippingSize(_glScreenClippingSize_Total);
						break;
				}


				GL.Begin(GL.TRIANGLES);
			}

			GL.Color(color);
			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_2); // 2

			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_0); // 0

			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_3); // 3

			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_0); // 0

			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_0); // 0

			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_3); // 3

			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_4); // 4
			GL.Vertex(pos_5); // 5

			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_4); // 4
			GL.Vertex(pos_3); // 3

			if (isNeedResetMat)
			{
				GL.End();
			}
		}


		public static void DrawBoldArea(Vector2 startPos, Vector2 endPos, float lineThickness, Color color, CLIP_TYPE clipType)
		{
			apGL.MaterialBatch matBatch = GetMatBatch(clipType);
			if (matBatch == null)
			{ return; }
			if (matBatch.IsNotReady())
			{ return; }

			float min_X = Mathf.Max(startPos.x, endPos.x);
			float max_X = Mathf.Min(startPos.x, endPos.x);

			float min_Y = Mathf.Max(startPos.y, endPos.y);
			float max_Y = Mathf.Min(startPos.y, endPos.y);

			matBatch.SetPass_Color();
			switch (clipType)
			{
				case CLIP_TYPE.Header:
					matBatch.SetClippingSize(_glScreenClippingSize_Header);
					break;

				case CLIP_TYPE.Main:
					matBatch.SetClippingSize(_glScreenClippingSize_Main);
					break;

				case CLIP_TYPE.Total:
					matBatch.SetClippingSize(_glScreenClippingSize_Total);
					break;
			}


			GL.Begin(GL.TRIANGLES);

			DrawBoldLine(new Vector2(min_X, min_Y), new Vector2(max_X, min_Y), lineThickness, color, false, clipType);
			DrawBoldLine(new Vector2(max_X, min_Y), new Vector2(max_X, max_Y), lineThickness, color, false, clipType);
			DrawBoldLine(new Vector2(max_X, max_Y), new Vector2(min_X, max_Y), lineThickness, color, false, clipType);
			DrawBoldLine(new Vector2(min_X, max_Y), new Vector2(min_X, min_Y), lineThickness, color, false, clipType);

			GL.End();
		}

		//----------------------------------------------------------------------------------------
		// Draw Text
		//----------------------------------------------------------------------------------------
		public static void DrawText(string text, Vector2 pos, float width, Color color, CLIP_TYPE clipType)
		{
			if (pos.x < 0)
			{
				return;
			}

			if (pos.x + (width) > _layoutWidth)
			{
				return;
			}
			_textStyle.normal.textColor = color;
			GUI.Label(new Rect(pos.x, pos.y, width, 30), text, _textStyle);
		}

		public static void DrawNumber(string number, Vector2 pos, Color color, CLIP_TYPE clipType)
		{
			//float width = GetIntWidth(number);
			float width = number.Length * 7;
			DrawText(number, pos - new Vector2(width / 2, 0), width, color, clipType);
		}

		#region [미사용 코드]
		//private static float GetIntWidth(int number)
		//{
		//	float length = 0.0f;

		//	int subNumber = 0;
		//	float baseWidth = 7.0f;
		//	while(true)
		//	{
		//		subNumber = number % 10;
		//		switch (subNumber)
		//		{	
		//			case 1: length += 1.0f;
		//				break;
		//			case 0:
		//			case 2:
		//			case 3:
		//			case 4:
		//			case 5:
		//			case 6:
		//			case 7:
		//			case 8:
		//			case 9:
		//				length += 1.0f;
		//				break;
		//		}

		//		if(number < 10)
		//		{
		//			break;
		//		}

		//		number /= 10;
		//	}

		//	return length * baseWidth;
		//} 
		#endregion

		//----------------------------------------------------------------------------------------

		//----------------------------------------------------------------------------------------
		private static int _curFrame = 0;
		private static int _startFrame = 0;
		private static int _endFrame = 0;
		private static int _widthPerFrame = 0;
		private static bool _isLoop = false;
		private static int _mainFrameUnit = 5;
		public static void SetTimelineSetting(int curFrame, int startFrame, int endFrame, int widthPerFrame, bool isLoop)
		{
			_curFrame = curFrame;
			_startFrame = startFrame;
			_endFrame = endFrame;
			_widthPerFrame = widthPerFrame;
			_isLoop = isLoop;

			//4자리 수를 기준으로 하자 //(4자리수 숫자의 길이 = 4 x 7 = 28)
			if (28 < widthPerFrame * 1)
			{
				_mainFrameUnit = 1;
			}
			else if (28 < widthPerFrame * 2)
			{
				_mainFrameUnit = 2;
			}
			else if (28 < widthPerFrame * 5)
			{
				_mainFrameUnit = 5;
			}
			else if (28 < widthPerFrame * 10)
			{
				_mainFrameUnit = 10;
			}
			else if (28 < widthPerFrame * 50)
			{
				_mainFrameUnit = 50;
			}
			else
			{
				_mainFrameUnit = 100;
			}

		}

		private const int X_OFFSET = 30;


		public static void DrawTimelineAreaBG(bool isEditing)
		{
			float startPosX = X_OFFSET - _scrollPos.x;
			float endPosX = FrameToPosX_Main(_endFrame);

			if (endPosX < startPosX)
			{
				return;
			}

			float baseSize = 120;
			if (endPosX - startPosX < baseSize)
			{
				baseSize = endPosX - startPosX;
			}

			Vector2 startPos = new Vector2(startPosX + baseSize / 2, _layoutHeight_Header / 2);
			Vector2 endPos = new Vector2(endPosX - baseSize / 2, _layoutHeight_Header / 2);
			Color bgColor = new Color(0.1f, 0.3f, 0.6f, 0.7f);
			if (isEditing)
			{
				bgColor = new Color(0.6f, 0.1f, 0.1f, 0.7f);
			}
			DrawTexture(_img_TimelineBGStart, startPos, baseSize, _layoutHeight_Header, bgColor, true, CLIP_TYPE.Header);
			DrawTexture(_img_TimelineBGEnd, endPos, baseSize, _layoutHeight_Header, bgColor, true, CLIP_TYPE.Header);
		}


		public static void DrawTimeGrid(Color lineColorMain, Color lineColorSub, Color numberColor)
		{
			Vector2 startPos = new Vector2(X_OFFSET - _scrollPos.x, 0);
			Vector2 curPos = startPos;
			int nCnt = 0;
			int startNumber = _startFrame;
			int curNumber = startNumber;
			//Batch를 하자

			_matBatch_Total.SetPass_Color();
			_matBatch_Total.SetClippingSize(_glScreenClippingSize_Total);
			GL.Begin(GL.LINES);


			Vector3 linePos1 = Vector3.zero, linePos2 = Vector3.zero;
			while (true)
			{
				if (curPos.x > 0)
				{
					if (curNumber % _mainFrameUnit == 0)
					{
						//DrawNumber(curNumber.ToString(), curPos, numberColor, CLIP_TYPE.Header);
						//DrawLine(curPos + new Vector2(0, 14), curPos + new Vector2(0, _layoutHeight_Total), lineColor, false, CLIP_TYPE.Total);
						GL.Color(lineColorMain);
						linePos1 = curPos + new Vector2(0, 14);
						linePos2 = curPos + new Vector2(0, _layoutHeight_Total);
					}
					else
					{
						//DrawLine(curPos + new Vector2(0, 20), curPos + new Vector2(0, _layoutHeight_Total), lineColor, false, CLIP_TYPE.Total);
						GL.Color(lineColorSub);
						linePos1 = curPos + new Vector2(0, 20);
						linePos2 = curPos + new Vector2(0, _layoutHeight_Total);
					}

					GL.Vertex(linePos1);
					GL.Vertex(linePos2);

					nCnt++;
				}

				curPos.x += _widthPerFrame;

				curNumber++;

				if (nCnt > 500)
				{
					break;
				}

				if (curPos.x > _layoutWidth)
				{
					break;
				}
			}

			GL.End();


			//Number도 출력하자
			curPos = startPos;
			nCnt = 0;
			startNumber = _startFrame;
			curNumber = startNumber;

			while (true)
			{
				if (curPos.x > 0)
				{
					if (curNumber % _mainFrameUnit == 0)
					{
						DrawNumber(curNumber.ToString(), curPos, numberColor, CLIP_TYPE.Header);
					}

					nCnt++;
				}

				curPos.x += _widthPerFrame;

				curNumber++;

				if (nCnt > 500)
				{
					break;
				}

				if (curPos.x > _layoutWidth)
				{
					break;
				}
			}

		}
		public static void DrawTimeBars_Header(Color lineColor)
		{
			DrawBox(new Vector2(_layoutWidth / 2, _layoutHeight_Header - 2), _layoutWidth, 4, lineColor, true, CLIP_TYPE.Header);

			//DrawBox(new Vector2(_layoutWidth / 2, _layoutHeight_Header / 2 - _scrollPos.y), _layoutWidth, _layoutHeight_Header, Color.green, true, CLIP_TYPE.Header);
			//DrawBox(new Vector2(_layoutWidth / 2, _layoutHeight_Header + (_layoutHeight_Main / 2) - _scrollPos.y), _layoutWidth, _layoutHeight_Main, Color.red, true, CLIP_TYPE.Main);

		}

		public static void DrawTimeBars_MainBG(Color bgColor, int posY, int height)
		{
			DrawBox(new Vector2(_layoutWidth / 2, posY + _layoutHeight_Header - (height / 2)), _layoutWidth, height - 1, bgColor, true, CLIP_TYPE.Main);
		}

		public static void DrawTimeBars_MainLine(Color lineColor, int posY)
		{
			DrawLine(new Vector2(0, posY + _layoutHeight_Header), new Vector2(_layoutWidth, posY + _layoutHeight_Header), lineColor, true, CLIP_TYPE.Main);
		}

		private static Vector2 _dragAreaStartPos = Vector2.zero;
		private static Vector2 _dragAreaEndPos = Vector2.zero;


		public static void DrawAndUpdateSelectArea()
		{
			if (_isMouseEventUsed)
			{
				//무시
				return;
			}

			//DrawBox(_mousePos, 20, 20, Color.red, true, CLIP_TYPE.Total);
			//Vector2 areaStartPos = Vector2.zero;
			//Vector2 areaEndPos = Vector2.zero;

			if (_timelineEvent == TIMELINE_EVENT.Select)
			{
				_dragAreaStartPos = _mousePos_Down - (_scrollPos - _scroll_Down);
				_dragAreaEndPos = _mousePos;

				if (_downClipArea == CLIP_TYPE.Header)
				{
					_dragAreaStartPos.y = Mathf.Clamp(_dragAreaStartPos.y, 0, _layoutHeight_Header - 3);
					_dragAreaEndPos.y = Mathf.Clamp(_dragAreaEndPos.y, 0, _layoutHeight_Header - 3);
				}
				else if (_downClipArea == CLIP_TYPE.Main)
				{
					//areaStartPos.y = Mathf.Max(areaStartPos.y, _layoutHeight_Header + 2);
					_dragAreaEndPos.y = Mathf.Max(_dragAreaEndPos.y, _layoutHeight_Header + 2);
				}
			}

			if (_isMouseEvent && !_isMouseEventUsed)
			{
				if (_timelineEvent == TIMELINE_EVENT.None)
				{
					//1. 선택이 안되었을 때 -> 
					if (IsMouseUpdatable_Down(true))
					{
						SetTimelineEvent(TIMELINE_EVENT.Select);
					}
				}
				else if (_timelineEvent == TIMELINE_EVENT.Select || _timelineEvent == TIMELINE_EVENT.ReadyToDrag)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
					{
						SetTimelineEvent(TIMELINE_EVENT.None);
					}
					else
					{
						//프레임 선택을 해야한다.
						//일단 그리기부터
					}
				}
			}

			if (_timelineEvent == TIMELINE_EVENT.Select && !_isKeyframeClicked)
			{
				Color lineColor = Color.black;
				switch (_selectType)
				{
					case SELECT_TYPE.New:
						lineColor = new Color(0.0f, 1.0f, 0.5f, 0.9f);
						break;

					case SELECT_TYPE.Add:
						lineColor = new Color(0.0f, 0.5f, 1.0f, 0.9f);
						break;

					case SELECT_TYPE.Subtract:
						lineColor = new Color(1.0f, 0.0f, 0.0f, 0.9f);
						break;
				}

				DrawBoldArea(_dragAreaStartPos, _dragAreaEndPos, 3.0f, lineColor, _downClipArea);
			}
		}

		//--------------------------------------------------------------------------
		/// <summary>
		/// PlayBar를 출력한다.
		/// 만약, 마우스 입력으로 Frame을 바꾸었다면 True를 리턴한다.
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public static bool DrawPlayBar(int frame)
		{
			Vector2 pos = FrameToPos_Main(frame, 0);
			pos.y = 0;
			//14:30
			//12:26
			int imgSizeWidth = 12;
			int imgSizeHeight = 26;
			//int yOffset = 12;
			int yOffset = 18;
			pos.y += (imgSizeHeight / 2) + yOffset;

			Color playBarColor = new Color(0.2f, 1.0f, 0.2f, 1.0f);
			Color lineColor = new Color(0.2f, 1.0f, 0.2f, 1.0f);

			if (_timelineEvent == TIMELINE_EVENT.DragPlayBar)
			{
				playBarColor = new Color(1.0f, 0.2f, 0.2f, 1.0f);
				lineColor = new Color(1.0f, 0.2f, 0.2f, 1.0f);
			}

			bool isChangeFrame = false;


			DrawLine(pos, pos + new Vector2(0.0f, _layoutHeight_Total), lineColor, true, CLIP_TYPE.Total);
			DrawTexture(_img_PlayBarHead, pos, imgSizeWidth, imgSizeHeight, playBarColor, true, CLIP_TYPE.Total);


			Vector2 cursorSelectPos = pos + new Vector2(0.0f, -yOffset / 2);

			float cursorSelectWidth = imgSizeWidth * 4;
			float cursorSelectHeight = imgSizeHeight + 8 + (yOffset / 2);
			AddCursorRect(cursorSelectPos, cursorSelectWidth, cursorSelectHeight, MouseCursor.ResizeHorizontal);

			if (_isMouseEvent && !_isMouseEventUsed)
			{
				//1. 선택이 안될때 -> 선택 체크
				if (_timelineEvent == TIMELINE_EVENT.None)
				{
					if (IsMouseUpdatable_Down(true))
					{
						if (IsTargetSelectable(_mousePos, cursorSelectPos, cursorSelectWidth, cursorSelectHeight))
						{
							SetTimelineEvent(TIMELINE_EVENT.DragPlayBar);
						}
					}
				}

				//2. 드래그를 해보자
				else if (_timelineEvent == TIMELINE_EVENT.DragPlayBar)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
					{
						SetTimelineEvent(TIMELINE_EVENT.None);
					}
					else
					{
						int posToFrame = Mathf.Clamp(PosToFrame(_mousePos.x), _startFrame, _endFrame);
						if (_selection != null && _selection.AnimClip != null)
						{
							int prevFrame = _selection.AnimClip.CurFrame;
							if (prevFrame != posToFrame)
							{
								_selection.AnimClip.SetFrame_Editor(posToFrame);
								isChangeFrame = true;
							}
						}
					}
				}
			}

			return isChangeFrame;
		}

		private enum KEYFRAME_CONTROL_TYPE
		{
			None,
			SingleSelect,
			MultipleSelect,
			MoveCopy,
			//Copy
		}
		private static KEYFRAME_CONTROL_TYPE _keyframeControlType = KEYFRAME_CONTROL_TYPE.None;
		//private static bool _isKeyframeSingleSelect = false;
		//private static bool _isKeyframeMultipleSelect = false;
		//private static bool _isKeyframeMove = false;
		//private static bool _isKeyframeCopy = false;
		private static List<apAnimKeyframe> _targetSelectableKeyframes = new List<apAnimKeyframe>();
		private static List<apAnimKeyframe> _targetSelectedKeyframes = new List<apAnimKeyframe>();
		private static bool _isSelectedKeyframeClick = false;
		private static bool _isKeyframeClicked = false;//단순 클릭으로 선택한게 있다면 => 영역 선택은 무시된다.
		private static bool _isSelectLoopDummy = false;

		public static void BeginKeyframeControl()
		{
			_keyframeControlType = KEYFRAME_CONTROL_TYPE.None;

			//Select? Move? Copy? 체크
			// Event.None -> [단일 선택] 체크 [Select] -> (성공시) Event.DragFrame (클릭한 위치의 Frame)
			// Event.Select -> [복수 선택] 체크 [Select] (바로 결과가 나오는건 아니고, 대상 프레임에 넣는다)
			// Event.DragFrame -> 드래그 처리 중(클릭했을때의 Frame

			if (_isMouseEvent && !_isMouseEventUsed)
			{
				if (_timelineEvent == TIMELINE_EVENT.DragFrame || _timelineEvent == TIMELINE_EVENT.ReadyToDrag)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Up ||
						_leftBtnStatus == apMouse.MouseBtnStatus.Released)
					{
						//Debug.Log("Mouse Up : " + _timelineEvent + " -> None");
						if (_timelineEvent == TIMELINE_EVENT.DragFrame)
						{
							OnDragKeyframeUp();
						}
						SetTimelineEvent(TIMELINE_EVENT.None);

						_isSelectedKeyframeClick = false;
					}
				}

				if (_timelineEvent == TIMELINE_EVENT.None)
				{
					//1. 선택이 안될때 -> 선택 체크 -> (이어서 Drag 할 수 있다.)
					if (IsMouseUpdatable_Down(true, true))
					{
						_keyframeControlType = KEYFRAME_CONTROL_TYPE.SingleSelect;
						_targetSelectableKeyframes.Clear();
						_isSelectedKeyframeClick = false;
						_isSelectLoopDummy = false;
					}
				}
				else if (_timelineEvent == TIMELINE_EVENT.Select)
				{
					_keyframeControlType = KEYFRAME_CONTROL_TYPE.MultipleSelect;
					_targetSelectableKeyframes.Clear();
					_isSelectedKeyframeClick = false;
					_isSelectLoopDummy = false;
				}
				else if (_timelineEvent == TIMELINE_EVENT.DragFrame)
				{
					_keyframeControlType = KEYFRAME_CONTROL_TYPE.MoveCopy;
				}


				//TODO
			}
		}

		//public static void DrawKeyframes(List<apAnimKeyframe> keyFrames, float posY, Color baseColor, bool isAvailable, int lineHeight)
		public static void DrawKeyframes(apAnimTimelineLayer timelineLayer,
											float posY,
											Color baseColor,
											bool isAvailable,
											int lineHeight,
											bool isSelectedTimelineLayer,
											int curFrame,
											bool isCurveEdit,
											apSelection.ANIM_SINGLE_PROPERTY_CURVE_UI curveEditUIType

											)
		{
			List<apAnimKeyframe> keyFrames = timelineLayer._keyframes;
			apAnimKeyframe firstFrame = timelineLayer._firstKeyFrame;
			apAnimKeyframe lastFrame = timelineLayer._lastKeyFrame;
			bool isDummyFirstFrame = false;
			bool isDummyLastFrame = false;
			if (firstFrame != null && firstFrame._isLoopAsStart)
			{
				isDummyFirstFrame = true;
			}

			if (lastFrame != null && lastFrame._isLoopAsEnd)
			{
				isDummyLastFrame = true;
			}


			float sizeH = (lineHeight - 2);
			float sizeW = (sizeH / 4) + 3;

			float halfSizeW = sizeW / 2.0f;
			float halfSizeH = sizeH / 2.0f;

			apAnimKeyframe curKeyFrame = null;

			baseColor.r *= 0.8f;
			baseColor.g *= 0.8f;
			baseColor.b *= 0.8f;
			baseColor.a = 1.0f;

			//Color selectedColor = new Color(1.0f, 0.05f, 0.05f, 1.0f);
			//Color workSelectedColor = new Color(0.2f, 0.3f, 0.7f, 1.0f);
			//Color inactiveColor = new Color(baseColor.r * 0.4f, baseColor.g * 0.4f, baseColor.b * 0.4f, 1.0f);

			//if (!isAvailable)
			//{
			//	baseColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
			//}

			posY = (posY + _layoutHeight_Header) - _scrollPos.y;
			//Vector2 keyPos = Vector2.zero;
			//Color keyColor = Color.black;
			bool isDrawY = false;

			int loopIconSize = 32;
			Color grayColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			if (posY + halfSizeH >= _layoutHeight_Header && posY - halfSizeH < _layoutHeight_Total)
			{
				isDrawY = true;
			}
			//영역에 포함되는 경우에만 렌더링을 하자
			float posX = -1;
			bool isWorkingKeyframeExist = false;

			if (isCurveEdit && isSelectedTimelineLayer)
			{
				apAnimKeyframe selectedKeyframe = null;
				if (_selection.AnimKeyframes.Count == 1 && _selection.AnimKeyframe != null && keyFrames.Contains(_selection.AnimKeyframe))
				{
					selectedKeyframe = _selection.AnimKeyframe;
				}

				Color curveColor = Color.black;
				for (int i = 0; i < keyFrames.Count; i++)
				{
					curKeyFrame = keyFrames[i];

					//시작 키는 Prev + Next Curve를 출력한다.
					//나머지는 Next만 출력한다.
					if (curKeyFrame == firstFrame)
					{
						DrawCurve(curKeyFrame, true, selectedKeyframe, curveEditUIType, posY, false);
					}

					DrawCurve(curKeyFrame, false, selectedKeyframe, curveEditUIType, posY, ((curKeyFrame == lastFrame) && isDummyLastFrame));
				}
			}

			for (int i = 0; i < keyFrames.Count; i++)
			{
				curKeyFrame = keyFrames[i];

				if (_selection.AnimWorkKeyframe == curKeyFrame)
				{
					isWorkingKeyframeExist = true;
				}

				//DrawSingleKeyframe(curKeyFrame, isAvailable, sizeW, sizeH, isDrawY, posY, selectedColor, inactiveColor, baseColor, grayColor, workSelectedColor, loopIconSize, false);
				DrawSingleKeyframe(curKeyFrame, isAvailable, sizeW, sizeH, isDrawY, posY, baseColor, loopIconSize, false);
			}

			if (isDummyLastFrame)
			{
				//더미 프레임은 조금 더 신중하게 처리한다.
				DrawSingleKeyframe(lastFrame, isAvailable, sizeW, sizeH, isDrawY, posY, baseColor, loopIconSize, true);
			}


			if (isDummyFirstFrame)
			{
				//더미 프레임은 조금 더 신중하게 처리한다.
				DrawSingleKeyframe(firstFrame, isAvailable, sizeW, sizeH, isDrawY, posY, baseColor, loopIconSize, true);
			}

			if (!isWorkingKeyframeExist && isSelectedTimelineLayer)
			{
				//현재 타임라인에서 + 재생중인 키프레임이 없다면 => 임시 키프레임을 보여주자
				DrawCurrentKeyframe(curFrame, sizeW, sizeH, posY, new Color(0.5f, 0.5f, 0.5f, 1.0f));
			}

			bool isMoveOrCopy = (_moveKeyframeList.Count != 0);
			if (isMoveOrCopy)
			{
				for (int i = 0; i < keyFrames.Count; i++)
				{
					curKeyFrame = keyFrames[i];

					MoveKeyframe moveCopyKeyframe = GetMoveKeyframe(curKeyFrame);
					if (moveCopyKeyframe != null)
					{
						DrawMoveCopyKeyframe(moveCopyKeyframe._startFrameIndex, moveCopyKeyframe._nextFrameIndex, sizeW, sizeH, posY, (_selectType != SELECT_TYPE.Add));
					}
				}
			}

			//GL.End();
		}



		private static void DrawSingleKeyframe(apAnimKeyframe keyframe,
												bool isAvailable,
												float sizeW, float sizeH,
												bool isDrawY, float posY,
												Color baseColor,
												//Color selectedColor, Color inactiveColor, Color baseColor, Color grayColor, Color workSelectedColor,
												int loopIconSize,
												bool isDummyFrame
											)
		{
			float posX = 0.0f;
			if (!isDummyFrame)
			{
				posX = FrameToPosX_Main(keyframe._frameIndex);
			}
			else
			{
				posX = FrameToPosX_Main(keyframe._loopFrameIndex);
			}

			bool isDrawX = false;
			if (posX < -sizeW || posX > _layoutWidth + sizeW)
			{
				isDrawX = false;
			}
			else
			{
				isDrawX = true;
			}
			Vector2 keyPos = new Vector2(posX - 0.5f, posY);
			Color keyColor = Color.black;

			//선택되었는지 여부
			bool isCursorDraw = false;
			if (!keyframe._isActive)
			{
				keyColor = _keyColor_Inactive;
			}
			else if (_selection.IsSelectedKeyframe(keyframe))
			{
				keyColor = _keyColor_Selected;
			}
			else if (_selection.AnimWorkKeyframe == keyframe)
			{
				keyColor = _keyColor_Working;
			}
			else
			{
				keyColor = baseColor;
			}

			if (_selection.AnimWorkKeyframe == keyframe)
			{
				isCursorDraw = true;
			}

			//이동/복사중이면 거기에 맞게 다르게 표현해야한다.
			bool isMoveOrCopy = (_moveKeyframeList.Count != 0);
			bool isDrawMoveCopyKey = isMoveOrCopy && (!isDummyFrame);//더미키가 아닌 Render에서 Move/Copy 상태일때


			//DrawTexture(_img_Keyframe, keyPos, sizeW, sizeH, keyColor, false, CLIP_TYPE.Main);
			if (isDrawX && isDrawY)
			{
				if (!isDummyFrame)
				{
					//더미가 아닐때 -> 복사 정보를 같이 출력해야한다.
					Texture2D img_key = _img_Keyframe;
					if (isMoveOrCopy)
					{
						//현재 프레임이 이동/복사 중이다.
						if (GetMoveKeyframe(keyframe) != null)
						{
							img_key = _img_KeyframeMoveSrc;
						}
					}
					DrawTexture(img_key, keyPos, sizeW, sizeH, keyColor, true, CLIP_TYPE.Main);
				}
				else
				{
					DrawTexture(_img_KeyframeDummy, keyPos, sizeW, sizeH, keyColor, true, CLIP_TYPE.Main);
				}
				if (keyframe._isLoopAsStart && keyframe._prevLinkedKeyframe != null)
				{
					DrawTexture(_img_KeyLoopLeft, keyPos, loopIconSize, loopIconSize, _keyColor_Normal, true, CLIP_TYPE.Main);
				}
				if (keyframe._isLoopAsEnd && keyframe._nextLinkedKeyframe != null)
				{
					DrawTexture(_img_KeyLoopRight, keyPos, loopIconSize, loopIconSize, _keyColor_Normal, true, CLIP_TYPE.Main);
				}

				if (isCursorDraw)
				{
					float cursizeW = 22.0f * (sizeW / 14.0f);
					float cursizeH = 56.0f * (sizeH / 48.0f);
					DrawTexture(_img_KeyframeCursor, keyPos, cursizeW, cursizeH, _keyColor_Normal, true, CLIP_TYPE.Main);
				}

				if (isAvailable)
				{
					AddCursorRect(keyPos, sizeW, sizeH + 4, MouseCursor.MoveArrow);
				}
			}

			//if(isDrawMoveCopyKey)
			//{
			//	MoveKeyframe moveCopyKeyframe = GetMoveKeyframe(keyframe);
			//	if(moveCopyKeyframe != null)
			//	{
			//		DrawMoveCopyKeyframe(moveCopyKeyframe._startFrameIndex, moveCopyKeyframe._nextFrameIndex, sizeW, sizeH, posY, (_selectType != SELECT_TYPE.Add));
			//	}
			//}

			if (_isMouseEvent && isAvailable && !_isMouseEventUsed)
			{
				if (_keyframeControlType == KEYFRAME_CONTROL_TYPE.SingleSelect)
				{
					//클릭으로 선택
					if (_targetSelectableKeyframes.Count == 0)
					{
						//아무것도 아직 선택된게 없을 때
						if (_mousePos.y > _layoutHeight_Header)
						{
							if (_leftBtnStatus == apMouse.MouseBtnStatus.Down)
							{
								if (IsTargetSelectable(_mousePos, keyPos, sizeW + 6, sizeH + 4))
								{
									_targetSelectableKeyframes.Add(keyframe);
									//선택된 키프레임을 다시 클릭했는가
									if (_selection.IsSelectedKeyframe(keyframe))
									{
										_isSelectedKeyframeClick = true;
									}
									else
									{
										_isSelectedKeyframeClick = false;
									}
									_isKeyframeClicked = true;
									_isSelectLoopDummy = isDummyFrame;
								}
							}
						}
					}
				}
				else if (_keyframeControlType == KEYFRAME_CONTROL_TYPE.MultipleSelect)
				{
					//영역으로 선택
					if (_downClipArea == CLIP_TYPE.Main)
					{
						bool isAnySelectd = false;


						if (!_isKeyframeClicked)
						{
							if (IsTargetSelectable_Area(_dragAreaStartPos, _dragAreaEndPos, keyPos, sizeW, sizeH))
							{
								_targetSelectableKeyframes.Add(keyframe);
								isAnySelectd = true;
							}
						}

						if (!isAnySelectd)
						{
							if (_leftBtnStatus == apMouse.MouseBtnStatus.Down || _leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
							{
								if (IsTargetSelectable(_dragAreaEndPos, keyPos, sizeW + 6, sizeH + 4) && _mousePos.y > _layoutHeight_Header)
								{
									_targetSelectableKeyframes.Add(keyframe);

									if (_selection.IsSelectedKeyframe(keyframe))
									{
										_isSelectedKeyframeClick = true;
									}
									//_isKeyframeClicked = true;
									isAnySelectd = true;

									if (IsTargetSelectable(_mousePos_Down, keyPos, sizeW + 6, sizeH + 4) && _mousePos.y > _layoutHeight_Header)
									{
										//클릭한 위치에서 선택된 것이라면
										//=> 단순 클릭
										_isKeyframeClicked = true;
									}
								}
							}
						}


					}
				}
			}
		}


		/// <summary>
		/// 현재 재생중인 Frame에 대해서 선택된 키프레임이 없을 경우, 가상의 키프레임 이미지를 출력한다.
		/// </summary>
		/// <param name="keyframe"></param>
		/// <param name="sizeW"></param>
		/// <param name="sizeH"></param>
		/// <param name="posY"></param>
		/// <param name="color"></param>
		private static void DrawCurrentKeyframe(int frameIndex,
													float sizeW, float sizeH,
													float posY,
													Color color
											)
		{
			float posX = FrameToPosX_Main(frameIndex);

			if (posX < -sizeW || posX > _layoutWidth + sizeW)
			{
				return;
			}

			Vector2 keyPos = new Vector2(posX - 0.5f, posY);
			//선택되었는지 여부

			DrawTexture(_img_CurKeyframe, keyPos, sizeW, sizeH, color, true, CLIP_TYPE.Main);
		}

		private static void DrawMoveCopyKeyframe(int srcKeyframeIndex, int targetKeyframeIndex, float sizeW, float sizeH, float posY, bool isMove)
		{
			if (srcKeyframeIndex == targetKeyframeIndex)
			{
				return;
			}
			float posX_Src = FrameToPosX_Main(srcKeyframeIndex) - 0.5f;
			float posX_Target = FrameToPosX_Main(targetKeyframeIndex) - 0.5f;

			if (posX_Src + sizeW < 0 && posX_Target + sizeW < 0)
			{
				return;
			}

			if (posX_Src - sizeW > _layoutWidth && posX_Target - sizeW > _layoutWidth)
			{
				return;
			}


			Vector2 keyPos = new Vector2(posX_Target, posY);
			Vector2 srcPos = new Vector2(posX_Src, posY);

			if (isMove)
			{
				DrawBoldLine(srcPos, keyPos, 3, _keyColor_Move_Line, true, CLIP_TYPE.Main);
				DrawTexture(_img_KeyframeMove, keyPos, sizeW, sizeH, _keyColor_Move, true, CLIP_TYPE.Main);
			}
			else
			{
				DrawBoldLine(srcPos, keyPos, 3, _keyColor_Copy_Line, true, CLIP_TYPE.Main);
				DrawTexture(_img_KeyframeCopy, keyPos, sizeW, sizeH, _keyColor_Copy, true, CLIP_TYPE.Main);
			}

		}

		private static Color _curveColor_Linear = new Color(1.0f, 0.2f, 0.2f, 1.0f);
		private static Color _curveColor_Smooth = new Color(0.2f, 0.5f, 1.0f, 1.0f);
		private static Color _curveColor_Constant = new Color(0.2f, 1.0f, 0.2f, 1.0f);

		private static void DrawCurve(apAnimKeyframe keyframe, bool isPrevDraw, apAnimKeyframe selectedKeyframe, apSelection.ANIM_SINGLE_PROPERTY_CURVE_UI curveEditUIType, float posY, bool isDummy)
		{
			apAnimCurveResult curveResult = null;
			if (isPrevDraw)
			{
				if (keyframe._curveKey._prevLinkedCurveKey == null)
				{ return; }
				curveResult = keyframe._curveKey._prevCurveResult;
			}
			else
			{
				if (keyframe._curveKey._nextLinkedCurveKey == null)
				{ return; }
				curveResult = keyframe._curveKey._nextCurveResult;
			}
			if (curveResult == null)
			{ return; }
			if (curveResult._curveKeyA == null || curveResult._curveKeyB == null)
			{ return; }

			int frameIndex_Start = keyframe._frameIndex;

			float posX_CurveStart = FrameToPosX_Main(keyframe._frameIndex);
			float posX_CurveEnd = 0.0f;
			bool isCurveDrawable = false;
			if (isPrevDraw)
			{
				posX_CurveEnd = FrameToPosX_Main(keyframe._curveKey._prevIndex);
			}
			else
			{
				posX_CurveEnd = FrameToPosX_Main(keyframe._curveKey._nextIndex);
			}




			Color curveColor = Color.black;
			switch (curveResult.CurveTangentType)
			{
				case apAnimCurve.TANGENT_TYPE.Linear:
					curveColor = _curveColor_Linear;
					break;

				case apAnimCurve.TANGENT_TYPE.Smooth:
					curveColor = _curveColor_Smooth;
					break;

				case apAnimCurve.TANGENT_TYPE.Constant:
					curveColor = _curveColor_Constant;
					break;
			}

			bool isSelected = false;
			if (curveEditUIType == apSelection.ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
			{
				//[ <- ]으로 그릴때
				//CurveResult의 Next가 Selected Keyframe이라면 Selected
				if (selectedKeyframe != null)
				{
					isSelected = (curveResult._curveKeyB == selectedKeyframe._curveKey);
				}
			}
			else
			{
				//[ -> ]으로 그릴때
				//CurveResult의 Prev가 Selected Keyframe이라면 Selected
				if (selectedKeyframe != null)
				{
					isSelected = (curveResult._curveKeyA == selectedKeyframe._curveKey);
				}
			}

			if (!isSelected)
			{
				curveColor.a = 0.3f;
			}

			DrawBoldLine(new Vector2(posX_CurveStart, posY),
									new Vector2(posX_CurveEnd, posY),
									7, curveColor, true, CLIP_TYPE.Main);


			if (isDummy)
			{
				float posX_Loop = FrameToPosX_Main(keyframe._loopFrameIndex);
				posX_CurveEnd = (posX_CurveEnd - posX_CurveStart) + posX_Loop;

				DrawBoldLine(new Vector2(posX_Loop, posY),
									new Vector2(posX_CurveEnd, posY),
									7, curveColor, true, CLIP_TYPE.Main);
			}
		}


		public static bool EndKeyframeControl()
		{
			bool isEventOccurred = false;
			if (_isMouseEvent && !_isMouseEventUsed)
			{
				switch (_keyframeControlType)
				{
					case KEYFRAME_CONTROL_TYPE.None:
						//패스
						break;


					case KEYFRAME_CONTROL_TYPE.SingleSelect:
						{
							if (_targetSelectableKeyframes.Count > 0)
							{
								//선택한게 있다.
								//선택한 이후엔 바로 이어서 DragFrame 상태로 넘어간다.

								//하나만 추가
								apGizmos.SELECT_TYPE selectType = apGizmos.SELECT_TYPE.Add;
								switch (_selectType)
								{
									case SELECT_TYPE.New:
										selectType = apGizmos.SELECT_TYPE.New;
										break;
									case SELECT_TYPE.Add:
										selectType = apGizmos.SELECT_TYPE.Add;
										break;
									case SELECT_TYPE.Subtract:
										selectType = apGizmos.SELECT_TYPE.Subtract;
										break;
								}

								if (_isSelectedKeyframeClick)
								{
									//"선택되었던 걸" 다시 눌렀다면 얘기가 다르다.
									//Add/Subtract를 제외하고는 Drag를 해야함
									//Drag를 해야할때는 Selection에 선택 이벤트를 날리지 않는다.
									//그 외에는 동일
									if (selectType != apGizmos.SELECT_TYPE.New)
									{
										_selection.SetAnimKeyframe(_targetSelectableKeyframes[0], true, selectType);
									}


								}
								else
								{
									//Selection Type에 맞게 "키프레임 한개"를 선택 (또는 해제) 한다.
									_selection.SetAnimKeyframe(_targetSelectableKeyframes[0], true, selectType, _isSelectLoopDummy);
								}


								////선택된걸 누르면
								////AnimClip의 재생 키프레임을 옮긴다.
								//int selectedFrameIndex = _targetSelectableKeyframes[0]._frameIndex;
								//if(_selection.AnimClip.IsLoop &&
								//	(
								//	selectedFrameIndex < _selection.AnimClip.StartFrame ||
								//	selectedFrameIndex > _selection.AnimClip.EndFrame
								//	))
								//{
								//	selectedFrameIndex = _targetSelectableKeyframes[0]._loopFrameIndex;
								//}

								//if (selectedFrameIndex >= _selection.AnimClip.StartFrame && selectedFrameIndex <= _selection.AnimClip.EndFrame)
								//{
								//	_selection.AnimClip.SetFrame_Editor(selectedFrameIndex);
								//}


								if (_leftBtnStatus != apMouse.MouseBtnStatus.Up && _leftBtnStatus != apMouse.MouseBtnStatus.Released)
								{
									if (_selectType != SELECT_TYPE.Subtract)
									{
										if (_selection.AnimKeyframes.Count == 1 ||
											(_selection.AnimKeyframes.Count > 1 && _isSelectedKeyframeClick)
											)
										{
											//if (_isSelectedKeyframeClick)//이걸 키고 else를 켜면 "첫 클릭시에는 Drag가 안되고, 다시 클릭할때 Drag"
											//if(true)//이 상태에서는 바로 Drag가 허용된다.
											{
												//_dragAreaStartPos = _mousePos_Down - (_scrollPos - _scroll_Down);
												_keyDragStartPos = _mousePos.x - (_scrollPos.x - _scroll_Down.x);
												_keyDragFrameIndex_Down = PosToFrame(_keyDragStartPos);
												_keyDragFrameIndex_Cur = _keyDragFrameIndex_Down;
												//Debug.Log("Start Drag Keyframe [Down : " + _keyDragFrameIndex_Down + "]");

												_moveKeyframeList.Clear();
												for (int i = 0; i < _selection.AnimKeyframes.Count; i++)
												{
													_moveKeyframeList.Add(new MoveKeyframe(_selection.AnimKeyframes[i]));
												}


												SetTimelineEvent(TIMELINE_EVENT.DragFrame);
											}
										}
									}
								}
								else
								{
									SetTimelineEvent(TIMELINE_EVENT.None);
									_isMouseEventUsed = true;
								}
							}
							else
							{
								//선택한게 없다.
								if (_timelineEvent != TIMELINE_EVENT.None)
								{
									SetTimelineEvent(TIMELINE_EVENT.None);
								}
							}
							isEventOccurred = true;
						}
						break;

					case KEYFRAME_CONTROL_TYPE.MultipleSelect:
						{
							if (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
							{
								//Debug.Log("Multiple Select [" + _targetSelectableKeyframes.Count + "]");
								//마우스를 떼었으면 끝
								switch (_selectType)
								{
									case SELECT_TYPE.New:
										_selection.SetAnimMultipleKeyframe(_targetSelectableKeyframes, apGizmos.SELECT_TYPE.New, true);
										break;

									case SELECT_TYPE.Add:
										_selection.SetAnimMultipleKeyframe(_targetSelectableKeyframes, apGizmos.SELECT_TYPE.Add, true);
										break;

									case SELECT_TYPE.Subtract:
										_selection.SetAnimMultipleKeyframe(_targetSelectableKeyframes, apGizmos.SELECT_TYPE.Subtract, true);
										break;
								}

								isEventOccurred = true;

								//SetTimelineEvent(TIMELINE_EVENT.None);
								//_isMouseEventUsed = true;
							}
						}
						break;

					case KEYFRAME_CONTROL_TYPE.MoveCopy:
						//드래그해서 키 복사/이동
						if (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
						{
							Debug.Log("Drag -> Up");

							OnDragKeyframeUp();
							SetTimelineEvent(TIMELINE_EVENT.None);
							_isMouseEventUsed = true;

							isEventOccurred = true;
						}
						else
						{
							_keyDragCurPos = _mousePos.x - (_scrollPos.x - _scroll_Down.x);
							int curFrame = PosToFrame(_keyDragCurPos);

							//if(_selection.AnimKeyframes != null && _selection.AnimKeyframes.Count > 0)
							if (_moveKeyframeList.Count > 0)
							{
								if (curFrame != _keyDragFrameIndex_Cur)
								{
									//변동 사항이 있었다.
									//Debug.Log("Next Move Keyframe [" + _keyDragFrameIndex_Cur + " > " + curFrame + "]");

									//int deltaKeyframeFromDown = Mathf.Clamp(curFrame, _startFrame, _endFrame) - Mathf.Clamp(_keyDragFrameIndex_Down, _startFrame, _endFrame);
									int deltaKeyframeFromDown = curFrame - _keyDragFrameIndex_Down;

									//체크 : deltaKeyframe대로 이동하다가 Start / End Frame을 벗어나면 안된다.
									//다중 선택을 포함해서 "한계점에선 다같이 이동을 못함" 상태로 만들어야 한다.
									int maxDeltaMove = deltaKeyframeFromDown;

									//1. 체크하여 얼마나 이동 가능한지 먼저 본다.
									//2. 그 Delta값 만큼 이동 또는 복사를 한다.

									for (int iKeyframe = 0; iKeyframe < _moveKeyframeList.Count; iKeyframe++)
									{
										MoveKeyframe moveKey = _moveKeyframeList[iKeyframe];

										//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
										int nextFrameIndex = moveKey._startFrameIndex + deltaKeyframeFromDown;

										if (nextFrameIndex < _startFrame)
										{
											//startFrame = frameIndex + deltaX
											//deltaX = startFrame - frameIndex;
											int deltaLimit = _startFrame - moveKey._startFrameIndex;
											if (Mathf.Abs(deltaLimit) <= Mathf.Abs(maxDeltaMove))
											{
												maxDeltaMove = deltaLimit;
											}
										}
										else if (nextFrameIndex > _endFrame)
										{
											//_endFrame = frameIndex + deltaX
											//deltaX = _endFrame - frameIndex;
											int deltaLimit = _endFrame - moveKey._startFrameIndex;
											if (Mathf.Abs(deltaLimit) <= Mathf.Abs(maxDeltaMove))
											{
												maxDeltaMove = deltaLimit;
											}
										}
									}

									//만약 이동거리 조절 후 부호가 바뀌거나 0이 되면 처리를 안한다.
									if (deltaKeyframeFromDown * maxDeltaMove <= 0)
									{
										maxDeltaMove = 0;
									}

									for (int iKeyframe = 0; iKeyframe < _moveKeyframeList.Count; iKeyframe++)
									{
										MoveKeyframe moveKey = _moveKeyframeList[iKeyframe];

										//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
										int nextFrameIndex = moveKey._startFrameIndex + maxDeltaMove;
										moveKey._nextFrameIndex = nextFrameIndex;

										apAnimTimelineLayer parentLayer = moveKey.ParentLayer;
										if (parentLayer == null)
										{
											continue;
										}
									}
								}
								_keyDragFrameIndex_Cur = curFrame;
							}
						}
						break;
				}

				if (_isKeyframeClicked &&
					(_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released)
					)
				{
					_isKeyframeClicked = false;
				}


			}

			return isEventOccurred;
		}


		private static void OnDragKeyframeUp()
		{
			List<apAnimTimelineLayer> refreshLayer = new List<apAnimTimelineLayer>();

			//Debug.Log("OnDragKeyframeUp [" + _moveKeyframeList.Count + "] (" + _selectType + ")");
			//이동한 이후에
			//Frame Index가 겹친게 있다면 Selection을 기준으로 남기고 나머지는 삭제해야한다.
			//if (_selection.AnimKeyframes != null && _selection.AnimKeyframes.Count > 0)
			if (_moveKeyframeList.Count > 0)
			{
				if (_selectType != SELECT_TYPE.Add)
				{
					//Debug.LogError(">>> Keryframe Move <<<");
					//선택한 프레임중 "현재 재생 프레임"과 같은게 있다면
					//"현재 재생 프레임"을 이동해야한다.
					int movePlayFrame = -1;
					bool isMovePlayFrame = false;
					if (_selection.AnimClip != null)
					{
						movePlayFrame = _selection.AnimClip.CurFrame;
					}


					for (int iKeyframe = 0; iKeyframe < _moveKeyframeList.Count; iKeyframe++)
					{
						MoveKeyframe moveKeyframe = _moveKeyframeList[iKeyframe];
						//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
						apAnimTimelineLayer parentLayer = moveKeyframe.ParentLayer;

						if (parentLayer == null)
						{ continue; }

						//키프레임 이동
						if (movePlayFrame >= 0 && moveKeyframe._srcKeyframe._frameIndex == movePlayFrame && !isMovePlayFrame)
						{
							movePlayFrame = moveKeyframe._nextFrameIndex;
							isMovePlayFrame = true;
						}
						moveKeyframe._srcKeyframe._frameIndex = moveKeyframe._nextFrameIndex;



						//겹쳐있고, 현재 키프레임 + 선택되지 않은 키프레임은 삭제한다.
						int nRemoved = parentLayer._keyframes.RemoveAll(delegate (apAnimKeyframe a)
						{
							if (a._frameIndex == moveKeyframe._nextFrameIndex)
							{
								if (a != moveKeyframe._srcKeyframe && !_selection.AnimKeyframes.Contains(a))
								{
									return true;
								}
							}
							return false;
						});

						if (nRemoved > 0)
						{
							parentLayer.SortAndRefreshKeyframes();//여기서 일시적으로 Refresh를 해주자
						}

						if (!refreshLayer.Contains(parentLayer))
						{
							refreshLayer.Add(parentLayer);
						}
					}

					if (isMovePlayFrame)
					{
						_selection.AnimClip.SetFrame_Editor(movePlayFrame);
					}
				}
				else
				{
					//Debug.LogError(">>> Keryframe Copy <<<");
					//이동한 키에 맞게 복사하자
					//단, 이동 키값이 같으면 무시
					for (int iKeyframe = 0; iKeyframe < _moveKeyframeList.Count; iKeyframe++)
					{
						MoveKeyframe moveKeyframe = _moveKeyframeList[iKeyframe];

						if (moveKeyframe._startFrameIndex == moveKeyframe._nextFrameIndex)
						{
							continue;
						}

						//apAnimKeyframe movedKeyframe = _selection.AnimKeyframes[iKeyframe];
						apAnimTimelineLayer parentLayer = moveKeyframe.ParentLayer;
						if (parentLayer == null)
						{ continue; }

						//겹쳐있고, 현재 키프레임 + 선택되지 않은 키프레임은 삭제한다.
						int nRemoved = parentLayer._keyframes.RemoveAll(delegate (apAnimKeyframe a)
						{
							if (a._frameIndex == moveKeyframe._nextFrameIndex)
							{
								if (a != moveKeyframe._srcKeyframe && !_selection.AnimKeyframes.Contains(a))
								{
									return true;
								}
							}
							return false;
						});

						if (nRemoved > 0)
						{
							parentLayer.SortAndRefreshKeyframes();//여기서 일시적으로 Refresh를 해주자
						}

						if (!refreshLayer.Contains(parentLayer))
						{
							refreshLayer.Add(parentLayer);
						}

						//복사한다.
						_selection.Editor.Controller.AddCopiedAnimKeyframe(
												moveKeyframe._nextFrameIndex,
												parentLayer,
												true,
												moveKeyframe._srcKeyframe,
												false);
					}
				}
			}

			for (int i = 0; i < refreshLayer.Count; i++)
			{
				refreshLayer[i].SortAndRefreshKeyframes();
			}

			_moveKeyframeList.Clear();

			//_selection.Editor.RefreshControllerAndHierarchy();

			//Refresh 추가
			//_selection.RefreshAnimEditing(true);
			//_selection.RefreshAnimEditing(false);
		}

		private static Vector2 FrameToPos_Main(int frame, int posY)
		{
			return new Vector2(
				X_OFFSET + (((frame - _startFrame) * _widthPerFrame) - _scrollPos.x),
				(_layoutHeight_Header + posY) - _scrollPos.y
				);
		}

		private static float FrameToPosX_Main(int frame)
		{
			return X_OFFSET + (((frame - _startFrame) * _widthPerFrame) - _scrollPos.x);
		}


		private static int PosToFrame(float posX)
		{
			//posX = X_OFFSET + (((frame - _startFrame) * _widthPerFrame) - _scrollPos.x)
			//posX - X_OFFSET + _scrollPos.X = ((frame - _startFrame) * _widthPerFrame)
			//(posX - X_OFFSET + _scrollPos.X) / _widthPerFrame = frame - _startFrame
			//(posX - X_OFFSET + _scrollPos.X) / _widthPerFrame + _startFrame = frame

			return (int)(((posX - X_OFFSET + _scrollPos.x) / (float)_widthPerFrame) + 0.5f) + _startFrame;
		}



		//------------------------------------------------------------------
		private static bool IsMouseUpdatable_Down(bool isLeft, bool isPressedAllowed = false)
		{
			if (!_isMouseEvent || _isMouseEventUsed || _selection == null)
			{
				return false;
			}

			if (isLeft)
			{
				if (isPressedAllowed)
				{
					if (_leftBtnStatus != apMouse.MouseBtnStatus.Down &&
						_leftBtnStatus != apMouse.MouseBtnStatus.Pressed)
					{
						return false;
					}
				}
				else
				{
					if (_leftBtnStatus != apMouse.MouseBtnStatus.Down)
					{
						return false;
					}
				}

			}
			else
			{
				if (isPressedAllowed)
				{
					if (_rightBtnStatus != apMouse.MouseBtnStatus.Down &&
						_rightBtnStatus != apMouse.MouseBtnStatus.Pressed)
					{
						return false;
					}
				}
				else
				{
					if (_rightBtnStatus != apMouse.MouseBtnStatus.Down)
					{
						return false;
					}
				}
			}

			if (_mousePos.x > 0 && _mousePos.x < _layoutWidth &&
				_mousePos.y > 0 && _mousePos.y < _layoutHeight_Total)
			{
				return true;
			}
			return false;
		}


		private static bool IsTargetSelectable(Vector2 mousePos, Vector2 targetPos, float width, float height)
		{
			float halfWidth = width / 2.0f;
			float halfHeight = height / 2.0f;
			if (mousePos.x >= (targetPos.x - (halfWidth + 0.5f)) && mousePos.x <= (targetPos.x + halfWidth + 0.5f) &&
				mousePos.y >= (targetPos.y - (halfHeight + 0.5f)) && mousePos.y <= (targetPos.y + halfHeight + 0.5f))
			{
				return true;
			}
			return false;
		}

		private static bool IsTargetSelectable_Area(Vector2 startPos, Vector2 endPos, Vector2 targetPos, float targetSizeW, float targetSizeH)
		{
			float halfWidth = targetSizeW / 2.0f;
			float halfHeight = targetSizeH / 2.0f;

			//조금이라도 걸치면 선택되어야 한다.

			float min_X = Mathf.Min(startPos.x, endPos.x);
			float max_X = Mathf.Max(startPos.x, endPos.x);
			float min_Y = Mathf.Min(startPos.y, endPos.y);
			float max_Y = Mathf.Max(startPos.y, endPos.y);

			if (min_X < targetPos.x + halfWidth && targetPos.x - halfWidth < max_X &&
				min_Y < targetPos.y + halfHeight && targetPos.y - halfHeight < max_Y)
			{
				return true;
			}
			return false;
		}

		private static void SetTimelineEvent(TIMELINE_EVENT timelineEvent)
		{
			//if(timelineEvent == TIMELINE_EVENT.DragFrame)
			//{
			//	Debug.LogError("Start Drag Frame : " + _timelineEvent + " >> Drag");
			//}

			_timelineEvent = timelineEvent;

			if (_timelineEvent == TIMELINE_EVENT.None)
			{
				_moveKeyframeList.Clear();
			}

			if (timelineEvent != TIMELINE_EVENT.None)
			{
				_mousePos_Down = _mousePos;

				_scroll_Down = _scrollPos;

				if (_mousePos_Down.y < _layoutHeight_Header)
				{
					_downClipArea = CLIP_TYPE.Header;
				}
				else
				{
					_downClipArea = CLIP_TYPE.Main;
				}
				_isMouseEventUsed = true;
			}
		}


		public static void AddCursorRect(Vector2 pos, float width, float height, MouseCursor cursorType)
		{
			if (pos.x < 0 || pos.x > _layoutWidth || pos.y < 0 || pos.y > _layoutHeight_Total)
			{
				return;
			}
			//pos.x += _layoutPosX;
			//pos.y += _layoutPosY_Header;
			pos.x -= width / 2;
			pos.y -= height / 2;

			//Debug.Log("AddCursorRect [ " + pos + " ]");
			//EditorGUI.DrawRect(new Rect(pos.x, pos.y, width, height), Color.yellow);
			EditorGUIUtility.AddCursorRect(new Rect(pos.x, pos.y, width, height), cursorType);
		}


		private static bool IsMouseInLayout(Vector2 mousePos)
		{
			if (mousePos.x < 0.0f || mousePos.x > _layoutWidth ||
				mousePos.y < 0.0f || mousePos.y > _layoutHeight_Total)
			{
				return false;
			}

			return true;
		}
	}

}
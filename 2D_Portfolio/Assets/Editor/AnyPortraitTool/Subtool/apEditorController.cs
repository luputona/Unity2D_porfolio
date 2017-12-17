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

	public class apEditorController
	{
		// Member
		//--------------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }

		// Init
		//--------------------------------------------------
		public apEditorController()
		{

		}

		public void SetEditor(apEditor editor)
		{
			_editor = editor;

		}


		// Functions
		//--------------------------------------------------
		// 1. 입력 관련한 함수들
		//--------------------------------------------------
		public void CheckInputEvent()
		{
			//bool isMouseEvent = (Event.current.type == EventType.ScrollWheel || Event.current.isMouse);



			if (Event.current.rawType == EventType.Used)
			{
				return;
			}



			//Event.current.rawType으로 해야 Editor 외부에서의 MouseUp 이벤트를 가져올 수 있다.
			bool isMouseEvent = Event.current.rawType == EventType.ScrollWheel ||
				Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseDrag ||
				Event.current.rawType == EventType.MouseMove ||
				Event.current.rawType == EventType.MouseUp;

			Vector2 mousePos = Vector2.zero;
			if (isMouseEvent || Event.current.type == EventType.Repaint)
			{
				mousePos = Event.current.mousePosition - new Vector2(Editor._mainGUIRect.x, Editor._mainGUIRect.y);
				apMouse.SetMousePos(mousePos, Event.current.mousePosition);


			}

			if (isMouseEvent)
			{
				//Vector2 mousePos = Event.current.mousePosition - new Vector2(Editor._mainGUIRect.x, Editor._mainGUIRect.y);
				//apMouse.SetMousePos(mousePos, Event.current.mousePosition);

				for (int i = 0; i < 4; i++)
				{
					Editor._mouseBtn[i].ReadyToUpdate();
				}

				if (Event.current.rawType == EventType.ScrollWheel)
				{
					Vector2 deltaValue = Event.current.delta;
					Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Update_Wheel((int)(deltaValue.y * 10.0f));
				}
				else//if (Event.current.isMouse)
				{
					int iMouse = -1;
					switch (Event.current.button)
					{
						case 0://Left
							iMouse = 0;
							break;

						case 1://Right
							iMouse = 1;
							break;

						case 2://Middle
							iMouse = 2;
							break;
					}
					if (iMouse >= 0)
					{
						//GUI 기준 상대 좌표
						//Vector2 mousePos = Event.current.mousePosition - new Vector2(Editor._mainGUIRect.x, Editor._mainGUIRect.y);
						//Vector2 deltaValue = Event.current.delta;

						//for (int i = 0; i < 3; i++)
						//{
						//	Editor._mouseBtn[i].SetLastMousePos(mousePos);
						//}


						switch (Event.current.rawType)
						{
							case EventType.MouseDown:
								{
									if (IsMouseInGUI(mousePos))
									{
										//Editor._mouseBtn[iMouse].Update_Pressed(mousePos);
										Editor._mouseBtn[iMouse].Update_Pressed();
									}
									if (iMouse == 0)
									{
										//범위에 상관없이 왼쪽 클릭 체크
										Editor._mouseBtn[Editor.MOUSE_BTN_LEFT_NOT_BOUND].Update_Pressed();
									}
								}
								break;

							case EventType.MouseUp:
								{
									//Editor._mouseBtn[iMouse].Update_Released(mousePos);
									Editor._mouseBtn[iMouse].Update_Released();

									if (iMouse == 0)
									{
										//범위에 상관없이 왼쪽 클릭 체크
										//Debug.Log("Mouse Up");
										Editor._mouseBtn[Editor.MOUSE_BTN_LEFT_NOT_BOUND].Update_Released();
									}
								}
								break;

							case EventType.MouseMove:
							case EventType.MouseDrag:
								{
									//Editor._mouseBtn[iMouse].Update_Moved(deltaValue);
									Editor._mouseBtn[iMouse].Update_Moved();

									if (iMouse == 0)
									{
										//범위에 상관없이 왼쪽 클릭 체크
										Editor._mouseBtn[Editor.MOUSE_BTN_LEFT_NOT_BOUND].Update_Moved();
									}
								}
								break;

								//case EventType.ScrollWheel:
								//	{

								//	}
								//break;
						}

						if (Editor._curMouseBtn != iMouse)
						{
							//이전에 누른 마우스 버튼이 다 무효
							//전부다 초기화한다.

							Editor._curMouseBtn = iMouse;
							for (int i = 0; i < 4; i++)
							{
								if (i != iMouse)
								{
									Editor._mouseBtn[i].EndUpdate();
								}
							}
						}
					}
				}
			}

			if (Event.current.rawType == EventType.KeyDown)
			{
#if UNITY_EDITOR_OSX
			Editor.OnHotKeyDown(Event.current.keyCode, Event.current.command, Event.current.alt, Event.current.shift);
#else
				Editor.OnHotKeyDown(Event.current.keyCode, Event.current.control, Event.current.alt, Event.current.shift);
#endif
			}
			else if (Event.current.rawType == EventType.KeyUp)
			{
				Editor.OnHotKeyUp();
			}
		}

		public bool IsMouseInGUI(Vector2 mousePos)
		{
			if (mousePos.x < 0 || mousePos.x > Editor._mainGUIRect.width)
			{
				return false;
			}

			if (mousePos.y < 0 || mousePos.y > Editor._mainGUIRect.height)
			{
				return false;
			}
			return true;
		}

		public void GUI_Input_CheckClickInCenter()
		{
			apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			apMouse.MouseBtnStatus middleBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Status;

			Vector2 mousePos = apMouse.Pos;

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down
				|| rightBtnStatus == apMouse.MouseBtnStatus.Down
				|| middleBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				if (IsMouseInGUI(mousePos))
				{
					apEditorUtil.ReleaseGUIFocus();
				}
			}


		}

		public bool GUI_Input_ZoomAndScroll()
		{
			if (Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Wheel != 0)
			{
				//if(IsMouseInGUI(Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosLast))
				if (IsMouseInGUI(apMouse.PosLast))
				{
					//현재 위치에서 마우스의 World 좌표를 구한다.
					Vector2 mouseW_Relative = apGL.GL2World(apMouse.PosLast);
					float zoomPrev = Editor._zoomListX100[Editor._iZoomX100] * 0.01f;


					if (Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Wheel > 0)
					{
						//줌 아웃 = 인덱스 감소
						Editor._iZoomX100--;
						if (Editor._iZoomX100 < 0)
						{ Editor._iZoomX100 = 0; }
					}
					else if (Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Wheel < 0)
					{
						//줌 인 = 인덱스 증가
						Editor._iZoomX100++;
						if (Editor._iZoomX100 >= Editor._zoomListX100.Length)
						{
							Editor._iZoomX100 = Editor._zoomListX100.Length - 1;
						}
					}
					//마우스의 World 좌표는 같아야 한다.
					float zoomNext = Editor._zoomListX100[Editor._iZoomX100] * 0.01f;

					//Editor._scroll_MainCenter += ((mouseW_Relative / zoomNext) - (mouseW_Relative / zoomPrev)) * zoomNext;
					//중심점의 위치를 구하자 (Editor GL 기준)
					Vector2 scroll = new Vector2(Editor._scroll_MainCenter.x * 0.1f * apGL.WindowSize.x,
													Editor._scroll_MainCenter.y * 0.1f * apGL.WindowSize.y);
					Vector2 guiCenterPos = apGL.WindowSizeHalf - scroll;

					Vector2 deltaMousePos = apMouse.PosLast - guiCenterPos;
					Vector2 nextDeltaMousePos = deltaMousePos * (zoomNext / zoomPrev);

					//마우스를 기준으로 확대/축소를 할 수 있도록 줌 상태에 따라서 Scroll을 자동으로 조정하자
					//Delta = Mouse - GUICenter
					//GUICenter = Mouse - Delta
					//WindowSizeHalf - Scroll = Mouse - Delta
					//Scroll - WindowSizeHalf = Delta - Mouse
					//Scroll = (Delta - Mouse) + WindowSizeHalf
					//ScrollCenter * 0.1f * SizeXY = (Delta - Mouse) + WindowSizeHalf
					//ScrollCenter = ((Delta - Mouse) + WindowSizeHalf) / (0.1f * SizeXY)
					float nextScrollX = ((nextDeltaMousePos.x - apMouse.PosLast.x) + apGL.WindowSizeHalf.x) / (0.1f * apGL.WindowSize.x);
					float nextScrollY = ((nextDeltaMousePos.y - apMouse.PosLast.y) + apGL.WindowSizeHalf.y) / (0.1f * apGL.WindowSize.y);

					nextScrollX = Mathf.Clamp(nextScrollX, -100.0f, 100.0f);
					nextScrollY = Mathf.Clamp(nextScrollY, -100.0f, 100.0f);

					Editor._scroll_MainCenter.x = nextScrollX;
					Editor._scroll_MainCenter.y = nextScrollY;

					//Debug.Log("GUI Zoom / Center Pos : " + guiCenterPos + " / Mouse : " + apMouse.PosLast);

					//Editor.Repaint();
					Editor.SetRepaint();
					//Debug.Log("Zoom [" + _zoomListX100[_iZoomX100] + "]");

					Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].UseWheel();
					return true;
				}
			}

			if (Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Status == apMouse.MouseBtnStatus.Down ||
				Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Status == apMouse.MouseBtnStatus.Pressed)
			{
				//if (IsMouseInGUI(Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosLast))
				if (IsMouseInGUI(apMouse.PosLast))
				{
					Vector2 moveDelta = Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosDelta;
					//RealX = scroll * windowWidth * 0.1

					Vector2 sensative = new Vector2(
						1.0f / (Editor._mainGUIRect.width * 0.1f),
						1.0f / (Editor._mainGUIRect.height * 0.1f));

					Editor._scroll_MainCenter.x -= moveDelta.x * sensative.x;
					Editor._scroll_MainCenter.y -= moveDelta.y * sensative.y;

					//Editor.Repaint();
					Editor.SetRepaint();

					Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].UseMouseDrag();
					return true;
				}
			}
			return false;
		}


		public void GUI_Input_Modify()
		{
			apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			//Vector2 mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Pos;
			Vector2 mousePos = apMouse.Pos;

			if (Editor.VertController.Mesh == null || Editor.Select.Mesh != Editor.VertController.Mesh)
			{
				if (Editor.Select.Mesh != null)
				{
					Editor.VertController.SetMesh(Editor.Select.Mesh);
				}
				else
				{
					return;
				}

			}

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				if (IsMouseInGUI(mousePos))
				{
					if (Editor.Select.Mesh != null)
					{
						for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
						{
							apVertex vertex = Editor.Select.Mesh._vertexData[i];
							Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

							Vector2 posGL = apGL.World2GL(vPos);

							//어떤 버텍스를 선택했다.
							if (IsVertexClickable(posGL, mousePos))
							{
								Editor.VertController.SelectVertex(vertex);
								break;
							}
						}

						//Editor.Repaint();
						Editor.SetRepaint();
					}
				}
			}
		}

		private bool _isAnyVertexMoved = false;
		private bool _isHiddenEdgeTurnable = false;
		private bool _isMeshVertMovable = false;//< Vertex를 이동할 수 있는 조건 (1. null -> 새로 클릭 / 2. 기존꺼 다시 클릭) ~ 불가 조건 (기존꺼 -> 다른거 클릭)
		public void GUI_Input_MakeMesh(apEditor.MESH_EDIT_MODE_MAKEMESH makeMeshMode)
		{
			if (Event.current.type == EventType.Used)
			{
				return;
			}
			//if (!Event.current.isMouse)
			//{
			//	return;
			//}

			apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			//Vector2 mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Pos;
			Vector2 mousePos = apMouse.Pos;

			bool isShift = Event.current.shift;

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			//추가
			//Ctrl을 누르면 Vertex를 선택하는게 "범위 이내"에서 "가장 가까운"으로 바뀐다. (Snap)
			//Shift를 누르면 1개가 아닌 여러개의 충돌 점을 검색하고, Edge를 만들때 아예 충돌점에 Vertex를 추가하며 강제로 만든다.


			bool isNearestVertexCheckable = false;


			bool isVertEdgeRemovalble = false;

			if (Editor.VertController.Vertex == null)
			{
				isVertEdgeRemovalble = true;//<<이전에 선택한게 없으면 다음 선택시 삭제 가능
			}



			if (Editor.VertController.Mesh == null || Editor.Select.Mesh != Editor.VertController.Mesh)
			{
				if (Editor.Select.Mesh != null)
				{
					Editor.VertController.SetMesh(Editor.Select.Mesh);
				}
				else
				{
					return;
				}
			}


			if (Event.current.isMouse)
			{





				if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
				{
					_isMeshVertMovable = false;//일단 이동 불가


					if (IsMouseInGUI(mousePos))
					{
						if (Editor.Select.Mesh != null)
						{
							if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
							{
								apVertex prevSelectedVert = Editor.VertController.Vertex;

								if (prevSelectedVert == null)
								{
									_isMeshVertMovable = true;//새로 선택하면 -> 다음에 Vert 이동 가능 (1)
								}

								bool isAnySelect = false;
								bool isAnyAddVertOrMesh = false;
								//Ctrl을 누르는 경우 -> 가장 가까운 Vertex를 선택한다. (즉, Vertex 추가는 안된다.)
								if (isCtrl &&
									(makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge || makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly))
								{
									apVertex nearestVert = null;
									float minDistToVert = 0.0f;
									for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
									{
										apVertex vertex = Editor.Select.Mesh._vertexData[i];
										Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

										Vector2 posGL = apGL.World2GL(vPos);
										float distToMouse = Vector2.Distance(posGL, mousePos);
										if (nearestVert == null || distToMouse < minDistToVert)
										{
											nearestVert = vertex;
											minDistToVert = distToMouse;
										}
									}
									if (nearestVert != null)
									{
										//가장 가까운 Vert를 찾았다.
										if (prevSelectedVert == nearestVert)
										{
											//같은걸 선택했다.
											//이동 가능
											_isMeshVertMovable = true;
										}

										Editor.VertController.SelectVertex(nearestVert);
										isAnySelect = true;

										//추가 : 이전 버텍스에서 새로운 버텍스로 자동으로 Edge를 생성해주자
										if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly)
										{
											if (Editor.VertController.Vertex != prevSelectedVert
												&& prevSelectedVert != null
												&& Editor.VertController.Vertex != null)
											{
												//Undo - Add Edge
												apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_AddEdge, Editor.Select.Mesh, null, false, Editor);

												Editor.Select.Mesh.MakeNewEdge(prevSelectedVert, Editor.VertController.Vertex, isShift);
												isAnyAddVertOrMesh = true;
											}
										}
									}
								}
								else
								{
									for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
									{
										apVertex vertex = Editor.Select.Mesh._vertexData[i];
										Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

										Vector2 posGL = apGL.World2GL(vPos);

										//어떤 버텍스를 선택했다.
										if (IsVertexClickable(posGL, mousePos))
										{
											if (prevSelectedVert == vertex)
											{
												//같은걸 선택했다.
												//이동 가능
												_isMeshVertMovable = true;
											}

											Editor.VertController.SelectVertex(vertex);
											isAnySelect = true;

											//추가 : 이전 버텍스에서 새로운 버텍스로 자동으로 Edge를 생성해주자
											if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly)
											{
												if (Editor.VertController.Vertex != prevSelectedVert
													&& prevSelectedVert != null
													&& Editor.VertController.Vertex != null)
												{
													//Undo - Add Edge
													apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_AddEdge, Editor.Select.Mesh, null, false, Editor);

													Editor.Select.Mesh.MakeNewEdge(prevSelectedVert, Editor.VertController.Vertex, isShift);
													isAnyAddVertOrMesh = true;
												}
											}

											break;
										}
									}
								}


								if (!isAnySelect)
								{
									Editor.VertController.UnselectVertex();

									//아무 버텍스를 선택하지 않았다.
									//새로 추가한다. => Vertex 모드일 때
									if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly ||
										makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge)
									{
										if (Editor.VertController.Vertex == null)
										{
											//Undo - Vertex 추가
											apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_AddVertex, Editor.Select.Mesh, null, false, Editor);
											//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
											//apUndoUnitBase undo = apEditorUtil.MakeUndo(	apUndoManager.COMMAND.MeshVertex, 
											//												Editor.Select.Mesh, 
											//												apUndoManager.ACTION_TYPE.Add, 
											//												"Add Vertex", 
											//												Editor._portrait);
											//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


											Vector2 mouseWorld = apGL.GL2World(mousePos) + Editor.Select.Mesh._offsetPos;
											apVertex addedVert = Editor.Select.Mesh.AddVertexAutoUV(mouseWorld);
											if (addedVert != null)
											{
												Editor.VertController.SelectVertex(addedVert);
											}


											if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge)
											{
												apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_AddEdge, Editor.Select.Mesh, null, false, Editor);
												//만약 이전에 선택한 버텍스가 있다면
												//Edge를 연결하자
												if (prevSelectedVert != null)
												{
													Editor.Select.Mesh.MakeNewEdge(prevSelectedVert, addedVert, isShift);
												}
											}

											//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
											//if(undo != null) { undo.Refresh(); }
											//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
										}
									}
									else
									{
										//Edge 선택 모드에서
										//만약 HiddenEdge를 선택한다면
										//Turn을 하자
										Editor.VertController.UnselectVertex();
										Editor.VertController.UnselectNextVertex();

										if (_isHiddenEdgeTurnable)
										{
											//Undo - Vertex 추가
											apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_EditEdge, Editor.Select.Mesh, null, false, Editor);

											//아무것도 선택하지 않았다면..
											//Hidden Edge의 Trun을 한번 해보자
											apMeshEdge minHiddenEdge = null;
											float minDist = float.MaxValue;
											apMeshPolygon minPoly = null;

											List<apMeshPolygon> polygons = Editor.Select.Mesh._polygons;
											for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
											{
												apMeshPolygon curPoly = polygons[iPoly];

												List<apMeshEdge> hiddenEdges = curPoly._hidddenEdges;

												for (int iHide = 0; iHide < hiddenEdges.Count; iHide++)
												{
													apMeshEdge hiddenEdge = hiddenEdges[iHide];

													Vector2 vPos1 = new Vector2(hiddenEdge._vert1._pos.x, hiddenEdge._vert1._pos.y) - Editor.Select.Mesh._offsetPos;
													Vector2 vPos2 = new Vector2(hiddenEdge._vert2._pos.x, hiddenEdge._vert2._pos.y) - Editor.Select.Mesh._offsetPos;

													float distEdge = apEditorUtil.DistanceFromLine(
																		apGL.World2GL(vPos1),
																		apGL.World2GL(vPos2),
																		mousePos);

													if (distEdge < 5.0f)
													{
														if (minHiddenEdge == null || distEdge < minDist)
														{
															minDist = distEdge;
															minHiddenEdge = hiddenEdge;
															minPoly = curPoly;
														}
													}
												}
											}

											if (minHiddenEdge != null)
											{
												//Debug.Log("Try Hidden Edge Turn");
												if (minPoly.TurnHiddenEdge(minHiddenEdge))
												{
													Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
												}
											}

											_isHiddenEdgeTurnable = false;
										}
									}
								}
								else
								{
									if (!isAnyAddVertOrMesh)
									{
										//Debug.Log("Start Vertex Edit");
										//Undo - MeshEdit Vertex Pos Changed
										apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_EditVertex, Editor.Select.Mesh, Editor.VertController.Vertex, false, Editor);
									}
								}
							}
							else
							{
								//추가 : Polygon 모드
								//apMeshPolygon prevPolygon = Editor.VertController.Polygon;
								List<apMeshPolygon> polygons = Editor.Select.Mesh._polygons;
								Vector2 meshOffsetPos = Editor.Select.Mesh._offsetPos;
								bool isAnyPolygonSelect = false;
								for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
								{
									apMeshPolygon polygon = polygons[iPoly];
									if (IsPolygonClickable(polygon, meshOffsetPos, mousePos))
									{
										Editor.VertController.SelectPolygon(polygon);
										isAnyPolygonSelect = true;
										break;
									}
								}
								if (!isAnyPolygonSelect)
								{
									Editor.VertController.UnselectVertex();//<<이걸 호출하면 Polygon도 선택 해제됨
								}
							}
							//Editor.Repaint();
							Editor.SetRepaint();
						}
					}
				}
				else if (leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly &&
						makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
					{
						if (Editor.VertController.Vertex != null)
						{
							if (IsMouseInGUI(mousePos))
							{
								if (_isMeshVertMovable)
								{
									if (!_isAnyVertexMoved)
									{

									}

									//Undo - MeshEdit Vertex Pos Changed
									apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_EditVertex, Editor.Select.Mesh, Editor.VertController.Vertex, false, Editor);

									Editor.VertController.Vertex._pos = apGL.GL2World(mousePos) + Editor.Select.Mesh._offsetPos;
									Editor.VertController.Mesh.RefreshVertexAutoUV(Editor.VertController.Vertex);

									_isAnyVertexMoved = true;
								}
							}
							else
							{
								Editor.VertController.UnselectVertex();
							}

							//Editor.Repaint();
							Editor.SetRepaint();
						}
					}
				}
				else if (leftBtnStatus == apMouse.MouseBtnStatus.Up ||
						leftBtnStatus == apMouse.MouseBtnStatus.Released)
				{
					Editor.VertController.StopEdgeWire();
					//Editor.Repaint();

					_isHiddenEdgeTurnable = true;

					if (_isAnyVertexMoved)
					{
						//apEditorUtil.SetRecord("Vertex Pos Change", Editor._selection.Mesh);
						_isAnyVertexMoved = false;
					}
					//if (Editor.VertController.Vertex != null)
					//{
					//	//마우스를 뗐을때 Unselect?
					//	//일단 냅두자
					//	//Editor.VertController.UnselectVertex();

					//	//Editor.Repaint();
					//	Editor.SetRepaint();
					//}
				}

				//mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Pos;

				if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
				{
					Editor.VertController.UnselectVertex();

					if (IsMouseInGUI(mousePos))
					{
						if (Editor.Select.Mesh != null)
						{
							bool isAnyRemoved = false;

							if (isVertEdgeRemovalble)
							{
								if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly ||
									makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge)
								{
									//1. 버텍스 제거
									for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
									{
										apVertex vertex = Editor.Select.Mesh._vertexData[i];
										Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

										Vector2 posGL = apGL.World2GL(vPos);

										//어떤 버텍스를 선택했다.
										if (IsVertexClickable(posGL, mousePos))
										{
											// Undo - MeshEdit_VertexRemoved
											apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_RemoveVertex, Editor.Select.Mesh, vertex, false, Editor);

											Editor.Select.Mesh.RemoveVertex(vertex, isShift);

											//Editor.Repaint();
											Editor.SetRepaint();
											isAnyRemoved = true;
											isVertEdgeRemovalble = false;
											break;
										}
									}
								}
							}

							if (isVertEdgeRemovalble)
							{
								if (!isAnyRemoved)
								{
									if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge ||
										makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly)
									{
										//2. Edge 제거
										apMeshEdge selectEdge = null;
										float minDist = float.MaxValue;

										for (int i = 0; i < Editor.Select.Mesh._edges.Count; i++)
										{
											apMeshEdge edge = Editor.Select.Mesh._edges[i];

											if (edge._vert1 == null || edge._vert2 == null)
											{
												continue;
											}

											Vector2 vPos1 = new Vector2(edge._vert1._pos.x, edge._vert1._pos.y) - Editor.Select.Mesh._offsetPos;
											Vector2 vPos2 = new Vector2(edge._vert2._pos.x, edge._vert2._pos.y) - Editor.Select.Mesh._offsetPos;

											float distEdge = apEditorUtil.DistanceFromLine(
												apGL.World2GL(vPos1),
												apGL.World2GL(vPos2),
												mousePos);

											if (distEdge < 5.0f)
											{
												if (selectEdge == null || distEdge < minDist)
												{
													minDist = distEdge;
													selectEdge = edge;
												}
											}

										}

										if (selectEdge != null)
										{
											//삭제합시더
											// Undo - MeshEdit_EdgeRemoved
											apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_RemoveEdge, Editor.Select.Mesh, selectEdge, false, Editor);

											Editor.Select.Mesh.RemoveEdge(selectEdge);
											isVertEdgeRemovalble = false;
										}

										Editor.VertController.UnselectVertex();
										Editor.VertController.UnselectNextVertex();
										//Editor.Repaint();
										Editor.SetRepaint();
									}
								}
							}
						}
					}

				}
			}

			isNearestVertexCheckable = false;

			if (Editor.VertController.Vertex != null)
			{
				if (Editor.Select.Mesh != null && IsMouseInGUI(mousePos) && makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
				{
					if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge ||
									makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly)
					{
						if (rightBtnStatus == apMouse.MouseBtnStatus.Up ||
							rightBtnStatus == apMouse.MouseBtnStatus.Released)
						{
							if (IsMouseInGUI(mousePos))
							{
								isNearestVertexCheckable = true;
							}
						}
					}
				}

				Editor.VertController.UpdateEdgeWire(mousePos, isShift);
				Editor.VertController.UnselectNextVertex();

				//마우스에서 가까운 Vertex를 찾는다.
				//Ctrl을 누르면 : 가장 가까운거 무조건
				//기본 : Vertex 영역안에 있는거
				if (isNearestVertexCheckable)
				{
					if (isCtrl)
					{
						apVertex nearestVert = null;
						float minDistToVert = 0.0f;
						for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
						{
							apVertex vertex = Editor.Select.Mesh._vertexData[i];
							Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

							Vector2 posGL = apGL.World2GL(vPos);
							float distToMouse = Vector2.Distance(posGL, mousePos);
							if (nearestVert == null || distToMouse < minDistToVert)
							{
								nearestVert = vertex;
								minDistToVert = distToMouse;
							}
						}
						if (nearestVert != null)
						{
							Editor.VertController.SelectNextVertex(nearestVert);

							Vector2 vPos = new Vector2(nearestVert._pos.x, nearestVert._pos.y) - Editor.Select.Mesh._offsetPos;
							Vector2 posGL = apGL.World2GL(vPos);


							Editor.VertController.UpdateEdgeWire(posGL, isShift);
						}
					}
					else
					{
						for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
						{
							apVertex vertex = Editor.Select.Mesh._vertexData[i];
							Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

							Vector2 posGL = apGL.World2GL(vPos);

							//어떤 버텍스를 선택했다.
							if (IsVertexClickable(posGL, mousePos))
							{
								Editor.VertController.SelectNextVertex(vertex);

								Editor.VertController.UpdateEdgeWire(posGL, isShift);
								break;
							}
						}
					}
				}
			}
			else
			{
				Editor.VertController.StopEdgeWire();
				Editor.VertController.UnselectNextVertex();
			}
		}


		#region [미사용 코드]
		//public void GUI_Input_LinkEdge()
		//{
		//	apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
		//	apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
		//	//Vector2 mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Pos;
		//	Vector2 mousePos = apMouse.Pos;

		//	if (Editor.VertController.Mesh == null || Editor.Select.Mesh != Editor.VertController.Mesh)
		//	{
		//		if (Editor.Select.Mesh != null)
		//		{
		//			Editor.VertController.SetMesh(Editor.Select.Mesh);
		//		}
		//		else
		//		{
		//			return;
		//		}

		//	}

		//	//Down -> Select
		//	//Pressed -> (Select가 있으면) 임시 Wire + 위치 파악해서 두번째 점 (예상 도달 점 체크)
		//	//Up -> (1) (Select가 있으면 + 두번째 점이 있으면) : 연결 -> 두개의 점 모두 해제
		//	//Up -> (2) (Select가 있으면 + 두번째 점이 없으면) => (그래도 좀 가까운 점 찾아서) : 연결 -> 두개의 점 모두 해제 

		//	#region [코드 1 : 드래그 드롭으로 연결하기]
		//	//if(leftBtnStatus == apMouse.MouseBtnStatus.Down)
		//	//{	
		//	//	if(IsMouseInGUI(mousePos))
		//	//	{
		//	//		if(Editor._selection.Mesh != null)
		//	//		{
		//	//			Editor.VertController.UnselectVertex();

		//	//			for (int i = 0; i < Editor._selection.Mesh._vertexData.Count; i++)
		//	//			{
		//	//				apVertex vertex = Editor._selection.Mesh._vertexData[i];
		//	//				Vector2 posGL = apGL.World2GL(vertex._pos);

		//	//				//어떤 버텍스를 선택했다.
		//	//				if(IsVertexClickable(posGL, mousePos))
		//	//				{
		//	//					Editor.VertController.SelectVertex(vertex);
		//	//					break;
		//	//				}
		//	//			}
		//	//			Editor.Repaint();
		//	//		}
		//	//	}
		//	//}
		//	//else if(leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
		//	//{
		//	//	if (Editor.VertController.Vertex != null)
		//	//	{
		//	//		if (IsMouseInGUI(mousePos))
		//	//		{
		//	//			Editor.VertController.UpdateEdgeWire(mousePos);
		//	//		}

		//	//		Editor.VertController.UnselectNextVertex();

		//	//		if (Editor._selection.Mesh != null)
		//	//		{
		//	//			//Pressed -> (Select가 있으면) 임시 Wire + 위치 파악해서 두번째 점 (예상 도달 점 체크)
		//	//			//TODO
		//	//			Vector2 mousePosW = apGL.GL2World(mousePos);
		//	//			apVertex minVert = null;
		//	//			float minDist = float.MaxValue;
		//	//			for (int i = 0; i < Editor._selection.Mesh._vertexData.Count; i++)
		//	//			{
		//	//				apVertex vert = Editor._selection.Mesh._vertexData[i];
		//	//				if (vert == Editor.VertController.Vertex)
		//	//				{
		//	//					continue;
		//	//				}

		//	//				float dist = Vector2.Distance(vert._pos, mousePosW);
		//	//				if (dist < minDist || minVert == null)
		//	//				{
		//	//					minVert = vert;
		//	//					minDist = dist;
		//	//				}
		//	//			}
		//	//			if (minVert != null)
		//	//			{
		//	//				if(minDist / apGL.Zoom < 30.0f)
		//	//				{
		//	//					Editor.VertController.SelectNextVertex(minVert);
		//	//				}
		//	//			}
		//	//		}

		//	//		Editor.Repaint();
		//	//	}
		//	//}
		//	//else
		//	//{
		//	//	if(Editor.VertController.Vertex != null)
		//	//	{
		//	//		//Up -> (1) (Select가 있으면 + 두번째 점이 있으면) : 연결 -> 두개의 점 모두 해제
		//	//		if(Editor.VertController.LinkedNextVertex != null)
		//	//		{
		//	//			if(Editor._selection.Mesh != null)
		//	//			{
		//	//				//새로운 Edge를 추가하자
		//	//				Editor._selection.Mesh.MakeNewEdge(Editor.VertController.Vertex, Editor.VertController.LinkedNextVertex);
		//	//			}
		//	//		}

		//	//		Editor.VertController.StopEdgeWire();
		//	//		Editor.VertController.UnselectVertex();
		//	//		Editor.Repaint();
		//	//	}
		//	//} 
		//	#endregion



		//	//mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Pos;

		//	#region [코드 1 : 우클릭 = 완성]
		//	//if(rightBtnStatus == apMouse.MouseBtnStatus.Down)
		//	//{
		//	//	Editor.VertController.UnselectVertex();

		//	//	if (IsMouseInGUI(mousePos))
		//	//	{
		//	//		if (Editor._selection.Mesh != null)
		//	//		{
		//	//			//삭제할까.. 아니면 중단할까..
		//	//			//중단을 해보자 (X) 아니다. 삭제를 하자
		//	//			Editor._selection.Mesh.MakeEdgesToIndexBuffer();
		//	//			Editor._meshEditMode = apEditor.MESH_EDIT_MODE.None;
		//	//		}
		//	//	}

		//	//} 
		//	#endregion

		//	//mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Pos;

		//	if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
		//	{
		//		if (IsMouseInGUI(mousePos))
		//		{
		//			//일단 뭔가의 버텍스를 선택한다.
		//			apVertex clickedVert = null;
		//			for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
		//			{
		//				apVertex vertex = Editor.Select.Mesh._vertexData[i];
		//				Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

		//				Vector2 posGL = apGL.World2GL(vPos);

		//				if (IsVertexClickable(posGL, mousePos))
		//				{
		//					clickedVert = vertex;
		//					break;
		//				}
		//			}

		//			if (clickedVert == null)
		//			{
		//				//아무것도 선택하지 않았다.
		//				Editor.VertController.UnselectVertex();
		//				Editor.VertController.UnselectNextVertex();

		//				if (_isHiddenEdgeTurnable)
		//				{
		//					//아무것도 선택하지 않았다면..
		//					//Hidden Edge의 Trun을 한번 해보자
		//					apMeshEdge minHiddenEdge = null;
		//					float minDist = float.MaxValue;
		//					apMeshPolygon minPoly = null;

		//					List<apMeshPolygon> polygons = Editor.Select.Mesh._polygons;
		//					for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
		//					{
		//						apMeshPolygon curPoly = polygons[iPoly];

		//						List<apMeshEdge> hiddenEdges = curPoly._hidddenEdges;

		//						for (int iHide = 0; iHide < hiddenEdges.Count; iHide++)
		//						{
		//							apMeshEdge hiddenEdge = hiddenEdges[iHide];

		//							Vector2 vPos1 = new Vector2(hiddenEdge._vert1._pos.x, hiddenEdge._vert1._pos.y) - Editor.Select.Mesh._offsetPos;
		//							Vector2 vPos2 = new Vector2(hiddenEdge._vert2._pos.x, hiddenEdge._vert2._pos.y) - Editor.Select.Mesh._offsetPos;

		//							float distEdge = apEditorUtil.DistanceFromLine(
		//												apGL.World2GL(vPos1),
		//												apGL.World2GL(vPos2),
		//												mousePos);

		//							if (distEdge < 5.0f)
		//							{
		//								if (minHiddenEdge == null || distEdge < minDist)
		//								{
		//									minDist = distEdge;
		//									minHiddenEdge = hiddenEdge;
		//									minPoly = curPoly;
		//								}
		//							}
		//						}
		//					}

		//					if (minHiddenEdge != null)
		//					{
		//						//Debug.Log("Try Hidden Edge Turn");
		//						if (minPoly.TurnHiddenEdge(minHiddenEdge))
		//						{
		//							Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
		//						}
		//					}

		//					_isHiddenEdgeTurnable = false;
		//				}
		//			}
		//			else
		//			{
		//				//뭔가를 선택했다.
		//				apVertex prevSelected = Editor.VertController.Vertex;

		//				//기존에 선택한게 있다
		//				//-> 연결한다.
		//				if (prevSelected != null)
		//				{
		//					// Undo - MeshEdit_AddEdge
		//					//apEditorUtil.SetRecord("MakeEdge", Editor._portrait);
		//					apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_AddEdge, Editor.Select.Mesh, null, false, Editor);
		//					//bool isSuccess = Editor.Select.Mesh.MakeNewEdge(prevSelected, clickedVert);
		//					Editor.Select.Mesh.MakeNewEdge(prevSelected, clickedVert);

		//				}

		//				//선택
		//				Editor.VertController.SelectVertex(clickedVert);
		//				Editor.VertController.UnselectNextVertex();
		//			}

		//			//Editor.Repaint();
		//			Editor.SetRepaint();
		//		}
		//	}
		//	else if (leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
		//	{
		//		//Pressed 없음
		//		//if (Editor.VertController.Vertex != null)
		//		//{
		//		//	if (IsMouseInGUI(mousePos))
		//		//	{
		//		//		Editor.VertController.UpdateEdgeWire(mousePos);
		//		//	}
		//		//	Editor.Repaint();
		//		//}
		//	}
		//	else if (leftBtnStatus == apMouse.MouseBtnStatus.Up)
		//	{
		//		Editor.VertController.StopEdgeWire();
		//		//Editor.Repaint();

		//		_isHiddenEdgeTurnable = true;
		//	}
		//	else
		//	{
		//		//Released 상태에서도 업데이트
		//		//if (Editor.VertController.Vertex != null)
		//		//{
		//		//	if (IsMouseInGUI(mousePos))
		//		//	{
		//		//		Editor.VertController.UpdateEdgeWire(mousePos);

		//		//		//마우스 근처에서 NextVertex를 선택하자 (롤오버 효과)
		//		//		Vector2 mousePosW = apGL.GL2World(mousePos);
		//		//		apVertex minVert = null;
		//		//		float minDist = float.MaxValue;
		//		//		for (int i = 0; i < Editor._selection.Mesh._vertexData.Count; i++)
		//		//		{
		//		//			apVertex vert = Editor._selection.Mesh._vertexData[i];
		//		//			if (vert == Editor.VertController.Vertex)
		//		//			{
		//		//				continue;
		//		//			}

		//		//			float dist = Vector2.Distance(vert._pos, mousePosW);
		//		//			if (dist < minDist || minVert == null)
		//		//			{
		//		//				minVert = vert;
		//		//				minDist = dist;
		//		//			}
		//		//		}
		//		//		if (minVert != null)
		//		//		{
		//		//			if (minDist / apGL.Zoom < 30.0f)
		//		//			{
		//		//				Editor.VertController.SelectNextVertex(minVert);
		//		//			}
		//		//		}

		//		//		Editor.Repaint();
		//		//	}
		//		//}
		//	}


		//	//mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Pos;
		//	if (rightBtnStatus == apMouse.MouseBtnStatus.Up)
		//	{
		//		if (IsMouseInGUI(mousePos))
		//		{
		//			//Edge를 지우자
		//			apMeshEdge selectEdge = null;
		//			float minDist = float.MaxValue;

		//			for (int i = 0; i < Editor.Select.Mesh._edges.Count; i++)
		//			{
		//				apMeshEdge edge = Editor.Select.Mesh._edges[i];

		//				if (edge._vert1 == null || edge._vert2 == null)
		//				{
		//					continue;
		//				}

		//				Vector2 vPos1 = new Vector2(edge._vert1._pos.x, edge._vert1._pos.y) - Editor.Select.Mesh._offsetPos;
		//				Vector2 vPos2 = new Vector2(edge._vert2._pos.x, edge._vert2._pos.y) - Editor.Select.Mesh._offsetPos;

		//				float distEdge = apEditorUtil.DistanceFromLine(
		//					apGL.World2GL(vPos1),
		//					apGL.World2GL(vPos2),
		//					mousePos);

		//				if (distEdge < 5.0f)
		//				{
		//					if (selectEdge == null || distEdge < minDist)
		//					{
		//						minDist = distEdge;
		//						selectEdge = edge;
		//					}
		//				}

		//			}

		//			if (selectEdge != null)
		//			{
		//				//삭제합시더
		//				//Editor._selection.Mesh._edges.Remove(selectEdge);
		//				// Undo - Remove Edge
		//				apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_RemoveEdge, Editor.Select.Mesh, selectEdge, false, Editor);

		//				Editor.Select.Mesh.RemoveEdge(selectEdge);


		//			}

		//			Editor.VertController.UnselectVertex();
		//			Editor.VertController.UnselectNextVertex();
		//			//Editor.Repaint();
		//			Editor.SetRepaint();
		//		}

		//	}
		//} 
		#endregion



		private bool _isMeshPivotEdit_Moved = false;
		private Vector2 _mouseDownPos_PivotEdit = Vector2.zero;
		public void GUI_Input_PivotEdit(float tDelta)
		{
			//이거 이상하다 수정하자
			apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;

			Vector2 mousePos = apMouse.Pos;

			if (Editor.VertController.Mesh == null || Editor.Select.Mesh != Editor.VertController.Mesh)
			{
				if (Editor.Select.Mesh != null)
				{
					Editor.VertController.SetMesh(Editor.Select.Mesh);
				}
				else
				{
					_isMeshPivotEdit_Moved = false;
					return;
				}
			}

			//순서는 Gizmo Transform -> Mouse 위치 체크 -> GizmoUpdate -> 결과 봐서 나머지 처리
			//Editor.Gizmos.Select(Editor.Select.Mesh);
			//Editor.Gizmos.SetTransform(Vector2.zero, Editor.Select.Mesh.Matrix_VertToLocal, apMatrix3x3.identity, apMatrix3x3.identity);

			if (!IsMouseInGUI(mousePos))
			{
				_isMeshPivotEdit_Moved = false;
				return;
			}

#if UNITY_EDITOR_OSX
				bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			//Gizmo 업데이트
			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				if (apEditorUtil.IsMouseInMesh(mousePos, Editor.Select.Mesh))
				{
					_isMeshPivotEdit_Moved = true;
					_mouseDownPos_PivotEdit = mousePos;
				}
			}
			else if (leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				if (_isMeshPivotEdit_Moved)
				{
					Vector2 posDownW = apGL.GL2World(_mouseDownPos_PivotEdit);
					Vector2 posCurW = apGL.GL2World(apMouse.Pos);

					Editor.Select.Mesh._offsetPos -= (posCurW - posDownW);
					Editor.Select.Mesh.MakeOffsetPosMatrix();//<<OffsetPos를 수정하면 이걸 바꿔주자

					_mouseDownPos_PivotEdit = mousePos;
					//Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].UseMouseDrag();
				}
			}
			else
			{
				_isMeshPivotEdit_Moved = false;
			}
		}

		//public void GUI_Input_VolumeWeight()
		//{
		//	if (Editor.IsHotKeyEvent)
		//	{
		//		bool isBrushSizeChange = false;
		//		bool isInc = true;
		//		if (Editor.HotKeyCode == KeyCode.RightBracket)
		//		{
		//			isBrushSizeChange = true;
		//			isInc = true;
		//		}
		//		else if (Editor.HotKeyCode == KeyCode.LeftBracket)
		//		{
		//			isBrushSizeChange = true;
		//			isInc = false;
		//		}

		//		if (isBrushSizeChange)
		//		{
		//			Editor._brushSize_VolumeEdit = GetNextBrushSize(Editor._brushSize_VolumeEdit, isInc);
		//			Editor.UseHotKey();

		//			return;
		//		}
		//	}
		//	apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
		//	apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
		//	//Vector2 mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Pos;
		//	Vector2 mousePos = apMouse.Pos;

		//	if (!IsMouseInGUI(mousePos))
		//	{
		//		return;
		//	}

		//	if (Editor.Select.Mesh == null)
		//	{
		//		return;
		//	}



		//	if (Editor.DeltaFrameTime <= 0.0001f)
		//	{
		//		return;
		//	}
		//	//float deltaTime = Editor.DeltaFrameTime;

		//	List<apVertex> vertices = Editor.Select.Mesh._vertexData;

		//	Vector2 mousePosW = apGL.GL2World(mousePos);

		//	bool isLeftBtn = false;
		//	bool isRightBtn = false;

		//	if (leftBtnStatus == apMouse.MouseBtnStatus.Down ||
		//		leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
		//	{
		//		isLeftBtn = true;
		//	}
		//	else if (rightBtnStatus == apMouse.MouseBtnStatus.Down ||
		//		rightBtnStatus == apMouse.MouseBtnStatus.Pressed)
		//	{
		//		isRightBtn = true;
		//	}

		//	if (isLeftBtn || isRightBtn)
		//	{
		//		float alpha = Mathf.Clamp01(Editor._brushAlpha_VolumeEdit * 0.01f * Editor.DeltaFrameTime);

		//		for (int i = 0; i < vertices.Count; i++)
		//		{
		//			apVertex vert = vertices[i];

		//			Vector2 vPos = new Vector2(vert._pos.x, vert._pos.y) - Editor.Select.Mesh._offsetPos;

		//			float dist = Vector2.Distance(vPos, mousePosW);
		//			float brushValue = GetBrushValue(
		//				dist,
		//				Editor._brushSize_VolumeEdit,
		//				Editor._brushValue_VolumeEdit,
		//				Editor._brushHardness_VolumeEdit
		//				);

		//			if (brushValue > 0.0f)
		//			{
		//				switch (Editor._brushType_VolumeEdit)
		//				{
		//					case apEditor.BRUSH_TYPE.SetValue:
		//						if (isLeftBtn)
		//						{
		//							vert._volumeWeight = vert._volumeWeight * (1.0f - alpha) + brushValue * alpha;
		//							vert._volumeWeight = Mathf.Clamp01(vert._volumeWeight);
		//						}
		//						break;

		//					case apEditor.BRUSH_TYPE.AddSubtract:
		//						{
		//							if (isLeftBtn)
		//							{
		//								vert._volumeWeight += brushValue * alpha;
		//							}
		//							else
		//							{
		//								vert._volumeWeight -= brushValue * alpha;
		//							}

		//							vert._volumeWeight = Mathf.Clamp01(vert._volumeWeight);
		//						}
		//						break;
		//				}
		//			}
		//		}
		//	}
		//}

		//public void GUI_Input_PhysicWeight()
		//{

		//}

		public void GUI_PrintBrushCursor(float radius, Color color)
		{
			apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			apMouse.MouseBtnStatus midBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Status;
			//Vector2 mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Pos;
			Vector2 mousePos = apMouse.Pos;

			if (!IsMouseInGUI(mousePos))
			{
				//Editor.Repaint();
				Editor.SetRepaint();
				return;
			}
			if (midBtnStatus == apMouse.MouseBtnStatus.Down ||
				midBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				//Editor.Repaint();
				Editor.SetRepaint();
				return;
			}

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down
				|| leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				color = Color.red;
			}
			else if (rightBtnStatus == apMouse.MouseBtnStatus.Down
				|| rightBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				color = Color.blue;
			}


			apGL.DrawCircle(apGL.GL2World(mousePos), radius, color, true);

			//Editor.Repaint();
			Editor.SetRepaint();
		}

		public bool IsVertexClickable(Vector2 vertPos, Vector2 mousePos)
		{
			if (!IsMouseInGUI(vertPos))
			{
				return false;
			}

			Vector2 difPos = mousePos - vertPos;
			if (Mathf.Abs(difPos.x) < 6.0f && Mathf.Abs(difPos.y) < 6.0f)
			{
				return true;
			}
			return false;
		}

		public bool IsPolygonClickable(apMeshPolygon polygon, Vector2 meshOffsetPos, Vector2 mousePos)
		{
			//Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;
			//Vector2 posGL = apGL.World2GL(vPos);
			//Tri 체크를 해보자
			for (int iTri = 0; iTri < polygon._tris.Count; iTri++)
			{
				apMeshTri tri = polygon._tris[iTri];
				Vector2 vPos0 = apGL.World2GL(new Vector2(tri._verts[0]._pos.x, tri._verts[0]._pos.y) - meshOffsetPos);
				Vector2 vPos1 = apGL.World2GL(new Vector2(tri._verts[1]._pos.x, tri._verts[1]._pos.y) - meshOffsetPos);
				Vector2 vPos2 = apGL.World2GL(new Vector2(tri._verts[2]._pos.x, tri._verts[2]._pos.y) - meshOffsetPos);

				if (apEditorUtil.IsPointInTri(mousePos, vPos0, vPos1, vPos2))
				{
					return true;
				}
			}
			return false;

		}



		//------------------------------------------------------------------------------
		public void GUI_Input_MeshGroup_Setting(float tDelta)
		{
			apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			Vector2 mousePos = apMouse.Pos;

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);

		}



		//Bone Edit : 이전에 클릭했던 위치
		private Vector2 _boneEdit_PrevClickPos = Vector2.zero;
		private bool _boneEdit_isFirstState = true;
		private bool _boneEdit_isMouseClickable = false;
		private bool _boneEdit_isDrawBoneGhost = false;//GUI에서 표시할 Ghost상태의 임시 Bone
		private Vector2 _boneEdit_PrevClickPosW = Vector2.zero;
		private Vector2 _boneEdit_NextGhostBonePosW = Vector2.zero;
		private apBone _boneEdit_PrevSelectedBone = null;



		public bool IsBoneEditGhostBoneDraw { get { return _boneEdit_isDrawBoneGhost; } }
		public Vector2 BoneEditGhostBonePosW_Start { get { return _boneEdit_PrevClickPosW; } }
		public Vector2 BoneEditGhostBonePosW_End { get { return _boneEdit_NextGhostBonePosW; } }
		public apBone BoneEditRollOverBone { get { return _boneEdit_rollOverBone; } }

		private apBone _boneEdit_rollOverBone = null;
		private Vector2 _boneEdit_PrevMousePosWToCheck = Vector2.zero;

		/// <summary>
		/// GUI Input에서 Bone 편집을 하기 전에 모드가 바뀌면 호출해야하는 함수.
		/// 몇가지 변수가 초기화된다.
		/// </summary>
		public void SetBoneEditInit()
		{
			_boneEdit_isFirstState = true;
			_boneEdit_PrevClickPos = Vector2.zero;
			_boneEdit_isMouseClickable = false;
			_boneEdit_isDrawBoneGhost = false;
			_boneEdit_PrevClickPosW = Vector2.zero;
			_boneEdit_NextGhostBonePosW = Vector2.zero;

			_boneEdit_rollOverBone = null;
			_boneEdit_PrevMousePosWToCheck = Vector2.zero;

			//Debug.Log("_boneEdit_PrevClickPosW -> Zero");

			//_boneEdit_PrevSelectedBone = Editor.Select.Bone;
			_boneEdit_PrevSelectedBone = null;
		}

		public void GUI_Input_MeshGroup_Bone(float tDelta)
		{
			//본 작업을 하자
			apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			Vector2 mousePos = apMouse.Pos;

			if (!_boneEdit_isMouseClickable)
			{
				if ((leftBtnStatus == apMouse.MouseBtnStatus.Up ||
					leftBtnStatus == apMouse.MouseBtnStatus.Released)
					&&
					(rightBtnStatus == apMouse.MouseBtnStatus.Up ||
					rightBtnStatus == apMouse.MouseBtnStatus.Released))
				{
					_boneEdit_isMouseClickable = true;
				}
			}

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			apSelection.BONE_EDIT_MODE boneEditMode = Editor.Select.BoneEditMode;
			switch (boneEditMode)
			{
				case apSelection.BONE_EDIT_MODE.None:
					//아무것도 안합니더
					break;

				case apSelection.BONE_EDIT_MODE.SelectOnly:
				case apSelection.BONE_EDIT_MODE.SelectAndTRS:
					//선택 + TRS
					if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
					{
						if (_boneEdit_isMouseClickable)
						{
							Editor.GizmoController._isBoneSelect_MovePosReset = true;//클릭시에는 리셋을 해주자
							_boneEdit_isMouseClickable = false;
						}

					}
					Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);
					if (rightBtnStatus == apMouse.MouseBtnStatus.Down && IsMouseInGUI(mousePos))
					{
						if (_boneEdit_isMouseClickable)
						{
							Editor.Select.SetBone(null);
							_boneEdit_isMouseClickable = false;
						}
					}
					break;

				case apSelection.BONE_EDIT_MODE.Add:
					//"선택된 Bone"을 Parent로 하여 추가하기.
					{
						apBone curSelectedBone = Editor.Select.Bone;
						apMeshGroup curMeshGroup = Editor.Select.MeshGroup;

						if (curMeshGroup == null)
						{
							break;
						}

						bool isMouseInGUI = IsMouseInGUI(mousePos);


						//1) 처음 Add할때 : 클릭한 곳이 Start 포인트 (Parent 여부는 상관없음)
						//2) 2+ Add할때 : 클릭한 곳이 End 포인트. Start -> End로 Bone을 생성하고, End를 Start로 교체.
						//우클릭하면 (1) 상태로 돌아간다.
						//(1)에서 우클릭을 하면 Add 모드에서 해제되고 Select 모드가 된다.


						//처음 추가할때에는 선택된 본을 Parent으로 한다. (Select 모드에서 선택해야함)
						//추가한 이후에는 추가된 본을 Parent로 하여 계속 수행한다.

						//- Add는 여기서 직접 처리하자
						//- "생성 중"일때는 Ghost 본을 GUI에 출력하자

						if (_boneEdit_isFirstState)
						{
							//_boneEdit_isDrawBoneGhost = false;
							//_boneEdit_isDrawBoneGhostOnMouseMove = false;

							//만약 마우스 입력 없이
							//외부에 의해서 Bone을 바꾸었다면
							//-> Parent를 바꾸려고 한 것.
							//Parent가 바뀌었으면 위치를 자동으로 잡아주자.
							if (curSelectedBone != _boneEdit_PrevSelectedBone)
							{
								_boneEdit_PrevSelectedBone = curSelectedBone;
								_boneEdit_isFirstState = false;
								_boneEdit_isMouseClickable = false;

								curSelectedBone.MakeWorldMatrix(false);
								curSelectedBone.GUIUpdate();
								Vector3 endPosW = curSelectedBone._shapePoint_End;

								_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

								_boneEdit_PrevClickPos = apGL.World2GL(endPosW);
								_boneEdit_PrevClickPosW = endPosW;
								_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
							}
							else
							{
								if (_boneEdit_isMouseClickable && isMouseInGUI)
								{

									if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
									{
										//좌클릭
										//1) 처음 Add할때 : 클릭한 곳이 Start 포인트 (Parent 여부는 상관없음)
										_boneEdit_PrevClickPos = mousePos;//마우스 위치를 잡고
										_boneEdit_isFirstState = false;//두번째 스테이트로 바꾼다.
										_boneEdit_isMouseClickable = false;

										_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

										_boneEdit_PrevClickPosW = apGL.GL2World(mousePos);
										_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
									}
									else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
									{
										//우클릭
										//(1)에서 우클릭을 하면 Add 모드에서 해제되고 Select 모드가 된다.
										Editor.Select.SetBoneEditMode(apSelection.BONE_EDIT_MODE.SelectAndTRS, true);
										Editor.Select.SetBone(null);
										_boneEdit_isMouseClickable = false;
										_boneEdit_isDrawBoneGhost = false;


									}
								}
							}
						}
						else
						{
							_boneEdit_NextGhostBonePosW = apGL.GL2World(mousePos);


							if (curSelectedBone != _boneEdit_PrevSelectedBone)
							{
								_boneEdit_PrevSelectedBone = curSelectedBone;
								_boneEdit_isFirstState = false;
								_boneEdit_isMouseClickable = false;

								curSelectedBone.MakeWorldMatrix(false);
								curSelectedBone.GUIUpdate();
								Vector3 endPosW = curSelectedBone._shapePoint_End;

								_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

								_boneEdit_PrevClickPos = apGL.World2GL(endPosW);
								_boneEdit_PrevClickPosW = endPosW;
								_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
							}



							if (_boneEdit_isMouseClickable && isMouseInGUI)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//좌클릭
									//2) 2+ Add할때 : 클릭한 곳이 End 포인트. Start -> End로 Bone을 생성하고, End를 Start로 교체.
									Vector2 startPosW = _boneEdit_PrevClickPosW;
									Vector2 endPosW = _boneEdit_NextGhostBonePosW;


									//Distance가 0이면 안된다.
									if (Vector2.Distance(startPosW, endPosW) > 0.00001f)
									{

										apBone newBone = AddBone(curMeshGroup, curSelectedBone);
										//newBone._defaultMatrix

										apMatrix parentWorldMatrix = null;
										//설정을 복사하자
										if (curSelectedBone != null)
										{
											newBone._shapeWidth = curSelectedBone._shapeWidth;
											newBone._shapeTaper = curSelectedBone._shapeTaper;

											parentWorldMatrix = curSelectedBone._worldMatrix;
										}
										else
										{
											if (curMeshGroup._rootRenderUnit != null)
											{
												parentWorldMatrix = curMeshGroup._rootRenderUnit.WorldMatrixWrap;
												//parentWorldMatrix = new apMatrix();
											}
											else
											{
												parentWorldMatrix = new apMatrix();
											}
										}

										//Parent 기준으로 로컬 좌표계를 구한다.
										Vector2 startPosL = parentWorldMatrix.InvMulPoint2(startPosW);
										Vector2 endPosL = parentWorldMatrix.InvMulPoint2(endPosW);

										float length = (endPosL - startPosL).magnitude;
										float angle = 0.0f;
										//start -> pos를 +Y로 삼도록 각도를 설정한다.
										if (Vector2.Distance(startPosL, endPosL) == 0.0f)
										{
											angle = -0.0f;
										}
										else
										{
											angle = Mathf.Atan2(endPosL.y - startPosL.y, endPosL.x - startPosL.x) * Mathf.Rad2Deg;
											angle += 90.0f;
										}

										angle += 180.0f;
										if (angle > 180.0f)
										{
											angle -= 360.0f;
										}
										else if (angle < -180.0f)
										{
											angle += 360.0f;
										}



										if (curSelectedBone != null)
										{
											curSelectedBone.LinkRecursive(curSelectedBone._level);

											//현재 본에 Child가 추가되었으므로
											//IK를 설정해주자
											if (curSelectedBone._childBones.Count > 0 && curSelectedBone._optionIK == apBone.OPTION_IK.Disabled)
											{
												curSelectedBone._optionIK = apBone.OPTION_IK.IKSingle;
												curSelectedBone._IKTargetBone = curSelectedBone._childBones[0];
												curSelectedBone._IKNextChainedBone = curSelectedBone._childBones[0];

												curSelectedBone._IKTargetBoneID = curSelectedBone._IKTargetBone._uniqueID;
												curSelectedBone._IKNextChainedBoneID = curSelectedBone._IKNextChainedBone._uniqueID;
											}
										}
										else
										{
											newBone.Link(curMeshGroup, null);
										}

										newBone.InitTransform();

										newBone._shapeLength = (int)length;

										newBone._defaultMatrix.SetIdentity();
										newBone._defaultMatrix.SetPos(startPosL);
										newBone._defaultMatrix.SetRotate(angle);
										newBone._defaultMatrix.SetScale(1.0f);

										newBone.MakeWorldMatrix(false);
										newBone.GUIUpdate(false);

										//Debug.Log("-Pos : L " + startPosL + ", W " + startPosW + "\n-Angle : " + angle
										//	+ "\n-End Pos : L " + endPosL + ", W " + endPosW + "\n-Delta : L " + (endPosL - startPosL) + ", W " + (endPosW - startPosW)
										//	+ "\n-Parent World Matrix : Pos : W " + parentWorldMatrix._pos + ", Length : " + length);


										//Select 에 선택해주자
										Editor.Select.SetBone(newBone);

										RefreshBoneHierarchy(curMeshGroup);
										RefreshBoneChaining(curMeshGroup);

										Editor.Select.SetBone(newBone);

										_boneEdit_PrevSelectedBone = Editor.Select.Bone;

										Editor.RefreshControllerAndHierarchy();
										curMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
										curMeshGroup.RefreshForce();

										//GUI가 바로 출력되면 에러가 있다.
										//다음 Layout까지 출력하지 말도록 제한하자
										Editor.SetGUIVisible("GUI MeshGroup Hierarchy Delayed", false);
										//Editor.SetGUIVisible("GUI MeshGroup Sub Bone Hierarchy Delayed", false);
									}
									//다음을 위해서 마우스 위치 갱신
									_boneEdit_PrevClickPos = mousePos;//마우스 위치를 잡고
									_boneEdit_PrevClickPosW = apGL.GL2World(mousePos);
									_boneEdit_isMouseClickable = false;

									_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

									Editor.SetRepaint();
								}
								else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//우클릭
									//우클릭하면 (1) 상태로 돌아간다.
									_boneEdit_isFirstState = true;
									_boneEdit_isMouseClickable = false;

									_boneEdit_isDrawBoneGhost = false;//Ghost Draw 종료

									Editor.Select.SetBone(null);
									_boneEdit_PrevSelectedBone = Editor.Select.Bone;
									Editor.RefreshControllerAndHierarchy();


								}
							}
						}

						//Editor.SetRepaint();
						//Editor.SetUpdateSkip();//<<이번 업데이트는 Skip을 한다.
					}


					break;

				case apSelection.BONE_EDIT_MODE.Link:
					//선택 + 2번째 선택으로 Parent 연결
					//(Child -> Parent)
					//연결한 후에는 연결 해제
					//우클릭으로 선택 해제
					{
						apBone curSelectedBone = Editor.Select.Bone;
						apMeshGroup curMeshGroup = Editor.Select.MeshGroup;

						if (curMeshGroup == null)
						{
							break;
						}

						bool isMouseInGUI = IsMouseInGUI(mousePos);

						//1) (현재 선택한 Bone 없이) 처음 Bone을 선택할 때 : 클릭한 Bone의 World Matrix + EndPos의 중점을 시작점으로 삼는다.
						//2) 다음 Bone을 선택할 때 : 
						//            이전에 선택한 Bone -> 지금 선택한 Bone으로 Parent 연결을 시도해본다. 
						//            (실패시 Noti)
						//            본 자체 선택을 Null로 지정

						if (curSelectedBone != _boneEdit_PrevSelectedBone)
						{
							_boneEdit_PrevSelectedBone = curSelectedBone;
							_boneEdit_isFirstState = false;
							_boneEdit_isMouseClickable = false;

							if (curSelectedBone != null)
							{
								curSelectedBone.MakeWorldMatrix(false);
								curSelectedBone.GUIUpdate();
								Vector2 midPosW = (curSelectedBone._shapePoint_End + curSelectedBone._worldMatrix._pos) * 0.5f;

								_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

								_boneEdit_PrevClickPos = apGL.World2GL(midPosW);
								_boneEdit_PrevClickPosW = midPosW;
								_boneEdit_NextGhostBonePosW = midPosW;
							}
							else
							{
								_boneEdit_isDrawBoneGhost = false;
							}
						}

						if (curSelectedBone == null)
						{
							//이전에 선택한 Bone이 없다.
							//새로 선택을 하자

							_boneEdit_isDrawBoneGhost = false;
							_boneEdit_rollOverBone = null;

							if (_boneEdit_isMouseClickable && isMouseInGUI)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//좌클릭
									//Bone을 선택할게 있는가
									List<apBone> boneList = curMeshGroup._boneList_All;
									apBone bone = null;
									for (int i = 0; i < boneList.Count; i++)
									{
										bone = boneList[i];
										if (Editor.GizmoController.IsBoneClick(bone, apGL.GL2World(mousePos), mousePos, Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.RenderOutline))
										{
											//Debug.Log("Selected : " + bone._name);
											Editor.Select.SetBone(bone);
											break;
										}
									}

									if (Editor.Select.Bone != null)
									{
										//새로 선택을 했다.
										curSelectedBone = Editor.Select.Bone;

										Editor.RefreshControllerAndHierarchy();

										_boneEdit_PrevClickPos = mousePos;//마우스 위치를 잡고
										_boneEdit_isMouseClickable = false;

										_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

										Vector2 midPosW = (curSelectedBone._shapePoint_End + curSelectedBone._worldMatrix._pos) * 0.5f;

										_boneEdit_PrevClickPosW = midPosW;
										_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
									}
								}
								else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//우클릭
									//(1)에서 우클릭을 하면 Add 모드에서 해제되고 Select 모드가 된다.
									Editor.Select.SetBoneEditMode(apSelection.BONE_EDIT_MODE.SelectAndTRS, true);
									Editor.Select.SetBone(null);
									_boneEdit_isMouseClickable = false;
									_boneEdit_isDrawBoneGhost = false;
								}
							}
						}
						else
						{
							//이전에 선택한 Bone이 있다.
							//다른 Bone을 선택한 후 Parent 연결을 시도하자.
							//연결 후에는 Link를 종료. (Link 여러번 할게 있나?)

							_boneEdit_isDrawBoneGhost = true;
							Vector2 curMousePosW = apGL.GL2World(mousePos);
							float deltaMousePos = Vector2.Distance(curMousePosW, _boneEdit_PrevMousePosWToCheck);
							_boneEdit_NextGhostBonePosW = curMousePosW;

							if (deltaMousePos > 2.0f)
							{
								_boneEdit_PrevMousePosWToCheck = curMousePosW;

								//다시 "가까운 롤오버된 Bone 찾기"
								List<apBone> boneList = curMeshGroup._boneList_All;
								apBone bone = null;
								_boneEdit_rollOverBone = null;
								for (int i = 0; i < boneList.Count; i++)
								{
									bone = boneList[i];
									if (Editor.GizmoController.IsBoneClick(bone, apGL.GL2World(mousePos), mousePos, Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.RenderOutline))
									{
										_boneEdit_rollOverBone = bone;
										break;
									}
								}
							}

							//여기서 클릭을 하면 Parent를 바꾸고 -> CurSelectBone을 교체하자
							//우클릭시 단순히 선택 Bone 해제
							if (_boneEdit_isMouseClickable && isMouseInGUI)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//Parent로 이을 Bone을 검색하자
									List<apBone> boneList = curMeshGroup._boneList_All;
									apBone bone = null;
									apBone targetBone = null;
									for (int i = 0; i < boneList.Count; i++)
									{
										bone = boneList[i];
										if (Editor.GizmoController.IsBoneClick(bone, apGL.GL2World(mousePos), mousePos, Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.RenderOutline))
										{
											targetBone = bone;
											break;
										}
									}
									if (targetBone != null)
									{
										//TODO : 가능한지 체크하자
										//Parent를 바꿀때에는
										//targetBone이 재귀적인 Child이면 안된다
										bool isChangeAvailable = true;
										if (curSelectedBone == targetBone)
										{
											isChangeAvailable = false;
										}
										else if (curSelectedBone._parentBone == targetBone)
										{
											isChangeAvailable = false;
										}
										else if (curSelectedBone.GetChildBoneRecursive(targetBone._uniqueID) != null)
										{
											isChangeAvailable = false;
										}

										//가능 여부에 따라서 처리
										if (isChangeAvailable)
										{
											//교체한다.
											SetBoneAsParent(curSelectedBone, targetBone);
											Editor.Notification(curSelectedBone._name + " became a child of " + targetBone._name, true, false);
										}
										else
										{
											//안된다고 에디터 노티로 띄워주자
											Editor.Notification("A Bone that can not be selected as a Parent. Detach first.", true, false);
										}

									}

									if (targetBone != null)
									{
										//처리가 끝났으면 Bone 교체
										Editor.Select.SetBone(targetBone);

										curSelectedBone.MakeWorldMatrix(false);
										curSelectedBone.GUIUpdate();
										Vector3 endPosW = curSelectedBone._shapePoint_End;

										_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

										_boneEdit_PrevClickPos = apGL.World2GL(endPosW);
										_boneEdit_PrevClickPosW = endPosW;
										_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
									}
									else
									{
										//우클릭 한것과 동일하게 작동
										Editor.Select.SetBone(null);
										_boneEdit_isDrawBoneGhost = false;
										_boneEdit_rollOverBone = null;
									}

									_boneEdit_isMouseClickable = false;

								}
								else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									_boneEdit_isMouseClickable = false;
									_boneEdit_isDrawBoneGhost = false;

									Editor.Select.SetBone(null);
									_boneEdit_rollOverBone = null;
								}
							}
						}

						//Editor.SetRepaint();
						//Editor.SetUpdateSkip();//<<이번 업데이트는 Skip을 한다.

						//우클릭을 한번 하면 선택 취소.
						//선택 취소된 상태에서 누르면 모드 취소
					}
					break;
			}
		}



		public bool IsBoneClickable(Vector2 posGL, apBone bone)
		{

			Vector2 posW = apGL.GL2World(posGL);

			//5각형 (Taper가 100일땐 4각형)의 클릭 체크
			if (apEditorUtil.IsPointInTri(posW, bone._worldMatrix._pos, bone._shapePoint_Mid1, bone._shapePoint_End1))
			{
				return true;
			}

			if (apEditorUtil.IsPointInTri(posW, bone._worldMatrix._pos, bone._shapePoint_Mid2, bone._shapePoint_End2))
			{
				return true;
			}

			if (bone._shapeTaper < 100)
			{
				if (apEditorUtil.IsPointInTri(posW, bone._worldMatrix._pos, bone._shapePoint_End1, bone._shapePoint_End2))
				{
					return true;
				}
			}

			return false;

		}


		public void GUI_Input_MeshGroup_Modifier(apModifierBase.MODIFIER_TYPE modifierType, float tDelta)
		{
			apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			Vector2 mousePos = apMouse.Pos;

			if (modifierType == apModifierBase.MODIFIER_TYPE.Base)
			{
				return;
			}

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);
		}

		public void GUI_Input_Animation(float tDelta)
		{
			apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			Vector2 mousePos = apMouse.Pos;

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);
		}

		//--------------------------------------------------
		// 2. 임시 변수 제어
		//--------------------------------------------------
		public void InitTmpValues()
		{
			Editor._tmpValues.Clear();
		}

		public int GetTmpValue(string keyName, int defaultValue)
		{
			if (Editor._tmpValues.ContainsKey(keyName))
			{
				return Editor._tmpValues[keyName];
			}

			Editor._tmpValues.Add(keyName, defaultValue);
			return defaultValue;
		}

		public void SetTmpValue(string keyName, int setvalue)
		{
			if (Editor._tmpValues.ContainsKey(keyName))
			{
				Editor._tmpValues[keyName] = setvalue;
				return;
			}
			Editor._tmpValues.Add(keyName, setvalue);
		}

		//--------------------------------------------------
		// 3-0. 초기화
		//--------------------------------------------------
		public void PortraitReadyToEdit()
		{
			if (Editor._portrait == null)
			{
				return;
			}
			Editor._portrait.ReadyToEdit();

			//추가
			//썸네일을 찾아서 연결해보자
			string thumnailPath = Editor._portrait._imageFilePath_Thumbnail;
			if (string.IsNullOrEmpty(thumnailPath))
			{
				Editor._portrait._thumbnailImage = null;
			}
			else
			{
				Texture2D thumnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumnailPath);
				Editor._portrait._thumbnailImage = thumnailImage;
			}

			RefreshMeshGroups();

			//Selection.activeGameObject = Editor.Select.Portrait.gameObject;
			Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져
		}

		//--------------------------------------------------
		// 3-1. 객체 참조
		//--------------------------------------------------
		public apTextureData GetTextureData(int uniqueID)
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			return Editor._portrait._textureData.Find(delegate (apTextureData a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public apMesh GetMesh(int uniqueID)
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			return Editor._portrait._meshes.Find(delegate (apMesh a)
			{
				return a._uniqueID == uniqueID;
			});
		}


		public apMeshGroup GetMeshGroup(int uniqueID)
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			return Editor._portrait._meshGroups.Find(delegate (apMeshGroup a)
			{
				return a._uniqueID == uniqueID;
			});
		}


		public apControlParam GetControlParam(string controlKeyName)
		{
			if (Editor._portrait == null)
			{
				return null;
			}
			return Editor._portrait._controller.FindParam(controlKeyName);
		}
		//--------------------------------------------------
		// 3. 객체의 추가 / 삭제
		//--------------------------------------------------
		/// <summary>
		/// Mesh와 MeshGroup은 Monobehaviour로 저장해야한다.
		/// 해당 GameObject가 포함될 Group이 있어야 Monobehaviour를 추가할 수 있다.
		/// 존재하면 추가하지 않는다.
		/// 모든 AddMesh/AddMeshGroup 함수 전에 호출한다.
		/// </summary>
		public void CheckAndMakeObjectGroup()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			apPortrait portrait = Editor._portrait;

			if (portrait._subObjectGroup == null)
			{
				portrait._subObjectGroup = new GameObject("EditorObjects");
				portrait._subObjectGroup.transform.parent = portrait.transform;
				portrait._subObjectGroup.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup.transform.localScale = Vector3.one;
				portrait._subObjectGroup.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_Mesh == null)
			{
				portrait._subObjectGroup_Mesh = new GameObject("Meshes");
				portrait._subObjectGroup_Mesh.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_Mesh.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_Mesh.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_Mesh.transform.localScale = Vector3.one;
				portrait._subObjectGroup_Mesh.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_MeshGroup == null)
			{
				portrait._subObjectGroup_MeshGroup = new GameObject("MeshGroups");
				portrait._subObjectGroup_MeshGroup.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_MeshGroup.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_MeshGroup.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_MeshGroup.transform.localScale = Vector3.one;
				portrait._subObjectGroup_MeshGroup.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}
		}


		#region [미사용 코드]
		///// <summary>
		///// 이미지를 삭제한다.
		///// </summary>
		///// <param name="iRemove"></param>
		//public void RemoveImage(int iRemove)
		//{
		//	if (iRemove >= 0 && iRemove < Editor._portrait._textureData.Count)
		//	{
		//		apEditorUtil.SetRecord("Remove Image", Editor._portrait);

		//		apTextureData removedTextureData = Editor._portrait._textureData[iRemove];
		//		if (removedTextureData == Editor.Select.TextureData)
		//		{
		//			Editor.Select.SetNone();//Select된 이미지라면 None으로 바꾸자
		//		}
		//		//int removedID = 
		//		Editor._portrait._textureData.RemoveAt(iRemove);
		//		Editor._portrait.SortTextureData();
		//	}
		//} 
		#endregion

		public void RemoveTexture(apTextureData textureData)
		{
			//Undo - Remove Image
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Main_RemoveImage, Editor._portrait, textureData, false, Editor);


			if (textureData == Editor.Select.TextureData)
			{
				Editor.Select.SetNone();//Select된 이미지라면 None으로 바꾸자
			}

			int removedUniqueID = textureData._uniqueID;

			//Editor._portrait.PushUniqueID_Texture(removedUniqueID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Texture, removedUniqueID);


			Editor._portrait._textureData.Remove(textureData);
			//Editor._portrait.SortTextureData();

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy();

		}

		/// <summary>
		/// 이미지를 추가한다.
		/// </summary>
		public apTextureData AddImage()
		{
			//int nextID = Editor._portrait.MakeUniqueID_Texture();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Texture);
			if (nextID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Texture Add Failed. Please Retry", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AddTextureFailed_Title),
												Editor.GetText(apLocalization.TEXT.AddTextureFailed_Body),
												Editor.GetText(apLocalization.TEXT.Close));

				return null;
			}

			//Undo - Add Image
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Main_AddImage, Editor._portrait, null, false, Editor);

			//apTextureData newTexture = new apTextureData(Editor._portrait._textureData.Count);

			apTextureData newTexture = new apTextureData(nextID);

			Editor._portrait._textureData.Add(newTexture);
			Editor.Select.SetImage(newTexture);//<<Selection에도 추가
											   //Editor._portrait.SortTextureData();

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy();

			return newTexture;
		}


		private object _psdDialogLoadKey = null;

		/// <summary>
		/// PSD 툴을 호출하여 자동 생성기를 돌린다.
		/// </summary>
		public void ShowPSDLoadDialog()
		{
			if (apVersion.I.IsDemo)
			{
				//데모 버전에서는 PSD Load를 지원하지 않습니다.
				EditorUtility.DisplayDialog(
					Editor.GetText(apLocalization.TEXT.DemoLimitation_Title),
					Editor.GetText(apLocalization.TEXT.DemoLimitation_Body),
					Editor.GetText(apLocalization.TEXT.Okay)
					);
			}
			else
			{
				_psdDialogLoadKey = apPSDDialog.ShowWindow(_editor, OnPSDImageLoad);
			}
		}
		public void OnPSDImageLoad(bool isSuccess, object loadKey,
									string fileName, List<apPSDLayerData> layerDataList,
									float scaleRatio, int totalWidth, int totalHeight, int padding)
		{
			if (_psdDialogLoadKey != loadKey)
			{
				_psdDialogLoadKey = null;
				return;
			}

			_psdDialogLoadKey = null;

			if (Editor._portrait == null || !isSuccess)
			{ return; }

			//이제 만들어봅시다.
			//일단 먼저 -> Image를 로드해야함
			//로드하고 TextureData에도 추가를 한 뒤, LayerData와 연동 맵을 만든다.
			Dictionary<string, Texture2D> savedAtlasPath = new Dictionary<string, Texture2D>();
			Dictionary<apPSDLayerData, apTextureData> layerTextureMapping = new Dictionary<apPSDLayerData, apTextureData>();


			Vector2 centerPosOffset = new Vector2((float)totalWidth * 0.5f * scaleRatio, (float)totalHeight * 0.5f * scaleRatio);

			for (int i = 0; i < layerDataList.Count; i++)
			{
				if (!layerDataList[i]._isImageLayer || !layerDataList[i]._isBakable)
				{
					continue;
				}

				string assetPath = layerDataList[i]._textureAssetPath;
				Texture2D savedImage = null;

				//Debug.Log("<" + layerDataList[i]._name + "> Image Path [" + assetPath + "]");
				if (savedAtlasPath.ContainsKey(assetPath))
				{
					savedImage = savedAtlasPath[assetPath];
				}
				else
				{
					savedImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
					if (savedImage == null)
					{
						Debug.LogError("Image Is Null Path [" + assetPath + "]");
					}
					savedAtlasPath.Add(assetPath, savedImage);
				}

				if (savedImage != null)
				{
					apTextureData textureData = Editor._portrait._textureData.Find(delegate (apTextureData a)
					{
						return a._image == savedImage;
					});

					if (textureData == null)
					{
						textureData = AddImage();
						textureData.SetImage(savedImage, savedImage.width, savedImage.height);
						//textureData._isPSDFile = true;
						textureData._assetFullPath = AssetDatabase.GetAssetPath(savedImage);

						//Debug.Log("Add Imaged : " + savedImage.name);
					}

					layerTextureMapping.Add(layerDataList[i], textureData);
				}
			}


			//Editor._portrait._textureData.Find()

			//리스트에선 계층적으로 되진 않았으나, 계층적으로 만들어야 한다.
			//List<apPSDLayerData> usedLayerData = new List<apPSDLayerData>();//<<중복 처리가 되지 않게 하자

			//1. Root가 될 MeshGroup을 만든다.
			apMeshGroup rootMeshGroup = AddMeshGroup();
			rootMeshGroup._name = fileName;


			//2. Parent가 없는 LayerData를 찾으면서 Mesh 또는 MeshGroup을 만들어주자
			//<추가> Depth는 LayerIndex와 같다.

			//3. Child가 있으면 재귀적으로 생성해준다.
			RecursiveParsePSDLayers(layerDataList, 0, rootMeshGroup, layerTextureMapping, scaleRatio, centerPosOffset, padding);

			rootMeshGroup.SortRenderUnits(true);
			//rootMeshGroup.SetAllRenderUnitForceUpdate();
			rootMeshGroup.RefreshForce();

			RefreshMeshGroups();


			//추가
			//Clipping 처리


			Editor._portrait.LinkAndRefreshInEditor();
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy();

		}

		private void RecursiveParsePSDLayers(List<apPSDLayerData> layerDataList,
												int curLevel,
												apMeshGroup parentMeshGroup,
												Dictionary<apPSDLayerData, apTextureData> layerTextureMapping,
												float scaleRatio, Vector2 centerPosOffset, int padding)
		{
			for (int i = 0; i < layerDataList.Count; i++)
			{
				apPSDLayerData curLayer = layerDataList[i];
				if (curLayer._hierarchyLevel != curLevel || !curLayer._isBakable)
				{
					continue;
				}
				if (curLayer._isImageLayer)
				{
					//이미지 레이어인 경우)
					//Mesh로 만들고 MeshTransform으로서 추가한다.
					//미리 Vertex를 Atlas 정보에 맞게 만들어주자
					apMesh newMesh = AddMesh();
					if (newMesh == null)
					{
						Debug.LogError("PSD Load Error : No Mesh Created");
						continue;
					}

					if (layerTextureMapping.ContainsKey(curLayer))
					{
						apTextureData textureData = layerTextureMapping[curLayer];
						newMesh._textureData = textureData;
						float resizeRatioW = (float)textureData._width / (float)curLayer._bakedData._width;
						float resizeRatioH = (float)textureData._height / (float)curLayer._bakedData._height;

						//실제 텍스쳐 에셋의 크기와 저장할때의 원본 이미지 크기는 다를 수 있다.
						//텍스쳐 에셋 크기를 존중하는게 기본이다.
						Vector2 offsetPos = new Vector2(
							(float)curLayer._bakedImagePos_Left + ((float)curLayer._bakedWidth * 0.5f),
							(float)curLayer._bakedImagePos_Top + ((float)curLayer._bakedHeight * 0.5f));



						offsetPos.x *= resizeRatioW;
						offsetPos.y *= resizeRatioH;

						float atlasPos_Left = curLayer._bakedImagePos_Left * resizeRatioW;
						float atlasPos_Right = (curLayer._bakedImagePos_Left + curLayer._bakedWidth) * resizeRatioW;
						float atlasPos_Top = curLayer._bakedImagePos_Top * resizeRatioH;
						float atlasPos_Bottom = (curLayer._bakedImagePos_Top + curLayer._bakedHeight) * resizeRatioH;

						float halfSize_W = (float)textureData._width * 0.5f;
						float halfSize_H = (float)textureData._height * 0.5f;

						atlasPos_Left -= halfSize_W;
						atlasPos_Right -= halfSize_W;
						atlasPos_Top -= halfSize_H;
						atlasPos_Bottom -= halfSize_H;

						//Padding도 적용하자
						//atlasPos_Left -= padding;
						atlasPos_Right += padding * 2;
						//atlasPos_Top -= padding * 2;
						atlasPos_Bottom += padding * 2;

						offsetPos.x -= halfSize_W;
						offsetPos.y -= halfSize_H;

						offsetPos.x += padding;
						offsetPos.y += padding;

						//PSD용 이므로 Atlas정보를 넣어주자
						newMesh._isPSDParsed = true;
						newMesh._atlasFromPSD_LT = new Vector2(atlasPos_Left, atlasPos_Top);
						newMesh._atlasFromPSD_RB = new Vector2(atlasPos_Right, atlasPos_Bottom);

						newMesh.ResetVerticesByRect(offsetPos, atlasPos_Left, atlasPos_Top, atlasPos_Right, atlasPos_Bottom);
						Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
					}

					newMesh._name = curLayer._name + "_Mesh";

					//Parent에 MeshTransform을 등록하자
					apTransform_Mesh meshTransform = AddMeshToMeshGroup(newMesh, parentMeshGroup);

					if (meshTransform == null)
					{
						//EditorUtility.DisplayDialog("Error", "Creating Mesh is failed", "Close");
						EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.MeshCreationFailed_Title),
														Editor.GetText(apLocalization.TEXT.MeshCreationFailed_Body),
														Editor.GetText(apLocalization.TEXT.Close));
						return;
					}

					meshTransform._nickName = curLayer._name;

					//기준 위치를 잡아주자
					meshTransform._matrix = new apMatrix();
					if (curLevel == 0)
					{
						meshTransform._matrix.SetPos(curLayer._posOffsetLocal * scaleRatio - centerPosOffset);
					}
					else
					{
						meshTransform._matrix.SetPos(curLayer._posOffsetLocal * scaleRatio);
					}

					meshTransform._matrix.MakeMatrix();

					if (curLayer._isClipping && curLayer._isClippingValid && parentMeshGroup != null)
					{
						AddClippingMeshTransform(parentMeshGroup, meshTransform, false);
					}

				}
				else
				{
					//폴더 레이어인 경우)
					//MeshGroup으로 만들고 MeshGroupTransform으로서 추가한다.
					//재귀적으로 하위 호출을 한다.
					apMeshGroup newMeshGroup = AddMeshGroup();
					if (newMeshGroup == null)
					{
						Debug.LogError("PSD Load Error : No MeshGroup Created");
						continue;
					}

					newMeshGroup._name = curLayer._name + "_MeshGroup";

					apTransform_MeshGroup meshGroupTransform = AddMeshGroupToMeshGroup(newMeshGroup, parentMeshGroup);

					meshGroupTransform._nickName = curLayer._name;

					//기준 위치를 잡아주자
					meshGroupTransform._matrix = new apMatrix();
					if (curLevel == 0)
					{
						meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * scaleRatio - centerPosOffset);
					}
					else
					{
						meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * scaleRatio);
					}
					meshGroupTransform._matrix.MakeMatrix();

					//자식 노드를 검색해서 처리하자
					if (curLayer._childLayers != null)
					{
						RecursiveParsePSDLayers(curLayer._childLayers, curLayer._hierarchyLevel + 1, newMeshGroup, layerTextureMapping, scaleRatio, centerPosOffset, padding);
					}


					newMeshGroup.SortRenderUnits(true);
					//newMeshGroup.SetAllRenderUnitForceUpdate();
					newMeshGroup.RefreshForce();
				}
			}
		}



		#region [미사용 코드]
		///// <summary>
		///// 메시를 삭제한다.
		///// </summary>
		///// <param name="iRemove"></param>
		//public void RemoveMesh(int iRemove)
		//{
		//	if (iRemove >= 0 && iRemove < Editor._portrait._meshes.Count)
		//	{
		//		apEditorUtil.SetRecord("Remove Mesh", Editor._portrait);

		//		apMesh removedMesh = Editor._portrait._meshes[iRemove];

		//		if (removedMesh == Editor.Select.Mesh)
		//		{
		//			Editor.Select.SetNone();
		//		}

		//		Editor._portrait._meshes.RemoveAt(iRemove);

		//		if (removedMesh != null)
		//		{	
		//			MonoBehaviour.DestroyImmediate(removedMesh.gameObject);
		//		}
		//	}
		//} 
		#endregion





		public apMesh AddMesh()
		{
			//ObjectGroup을 체크하여 만들어주자
			CheckAndMakeObjectGroup();

			//int nextID = Editor._portrait.MakeUniqueID_Mesh();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Mesh);
			if (nextID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Mesh Add Failed. Please Retry", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.MeshAddFailed_Title),
												Editor.GetText(apLocalization.TEXT.MeshAddFailed_Body),
												Editor.GetText(apLocalization.TEXT.Close));
				return null;
			}

			//Undo - Add Mesh
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Main_AddMesh, Editor._portrait, null, false, Editor);

			int nMeshes = Editor._portrait._meshes.Count;

			//GameObject로 만드는 경우
			string newName = "New Mesh (" + nMeshes + ")";
			GameObject newGameObj = new GameObject(newName);
			newGameObj.transform.parent = Editor._portrait._subObjectGroup_Mesh.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			apMesh newMesh = newGameObj.AddComponent<apMesh>();
			//apMesh newMesh = new apMesh();


			newMesh._uniqueID = nextID;
			newMesh._name = newName;

			newMesh.ReadyToEdit(Editor._portrait);

			Editor._portrait._meshes.Add(newMesh);
			Editor.Select.SetMesh(newMesh);

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy();

			//Mesh Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Mesh, true);


			//Undo - Create 추가
			apEditorUtil.SetRecordCreateMonoObject(newMesh, "Create Mesh");

			return newMesh;
		}


		public void RemoveMesh(apMesh mesh)
		{
			if (mesh == Editor.Select.Mesh)
			{
				Editor.Select.SetNone();
			}

			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Main_RemoveMesh, Editor._portrait, mesh, false, Editor);

			Editor._portrait.PushUnusedID(apIDManager.TARGET.Mesh, mesh._uniqueID);

			Editor._portrait._meshes.Remove(mesh);

			//if (mesh != null)
			//{	
			//	MonoBehaviour.DestroyImmediate(mesh.gameObject);
			//}

			//추가
			if (mesh != null)
			{
				Undo.DestroyObjectImmediate(mesh.gameObject);
			}

			//Editor.Hierarchy.RefreshUnits();
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy();
		}


		public void AddParam()
		{

			

			//Undo - Add Param
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Main_AddParam, Editor._portrait, null, false, Editor);


			//int iNextIndex = Editor._portrait.MakeUniqueID_ControlParam();
			int iNextIndex = 0;
			while (true)
			{
				apControlParam existParam = Editor.ParamControl.FindParam("New Param " + iNextIndex);
				if (existParam != null)
				{
					iNextIndex++;
				}
				else
				{
					break;
				}
			}
			string strNewName = "New Param (" + iNextIndex + ")";
			
			Editor.ParamControl.Ready(Editor._portrait);
			apControlParam newParam = Editor.ParamControl.AddParam_Float(strNewName, false, apControlParam.CATEGORY.Etc, 0.0f);
			if(newParam == null)
			{
				EditorUtility.DisplayDialog(
									Editor.GetText(apLocalization.TEXT.DemoLimitation_Title),
									Editor.GetText(apLocalization.TEXT.DemoLimitation_Body_AddParam),
									Editor.GetText(apLocalization.TEXT.Okay)
									);
				return;
			}
			newParam.Ready(Editor._portrait);
			if (newParam != null)
			{
				Editor.Select.SetParam(newParam);
			}

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy();

			//Param Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Param, true);
		}

		public void ChangeParamName(apControlParam cParam, string strNextName)
		{
			if (Editor._portrait == null)
			{
				return;
			}
			Debug.Log("TODO : Param 이름 바꿀때 키값을 설정하자 [MeshGroup - Modifier의 ControlParam 작업됨]");
			string prevName = cParam._keyName;


			#region [미사용 코드] string 값으로 Param을 저장했을때의 코드(지금은 int형 ID)
			//for (int iMeshGroup = 0; iMeshGroup < Editor._portrait._meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = Editor._portrait._meshGroups[iMeshGroup];
			//	for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
			//	{
			//		apModifierBase mod = meshGroup._modifierStack._modifiers[iMod];

			//		for (int iParamSetGroup = 0; iParamSetGroup < mod._paramSetGroup_controller.Count; iParamSetGroup++)
			//		{
			//			apModifierParamSetGroup paramSetGroup = mod._paramSetGroup_controller[iParamSetGroup];

			//			//같다면 이름을 바꾸자
			//			if(paramSetGroup._keyControlParam != null && 
			//				paramSetGroup._keyControlParam == cParam)
			//			{
			//				paramSetGroup._keyControlParamName = strNextName;
			//			}
			//		}
			//	}
			//} 
			#endregion

			cParam._keyName = strNextName;
			Editor.RefreshControllerAndHierarchy();
		}

		public void RemoveParam(apControlParam cParam)
		{
			if (Editor.Select.Param == cParam)
			{
				Editor.Select.SetNone();
			}

			int removedParamID = cParam._uniqueID;

			//Editor._portrait.PushUniqueID_ControlParam(removedParamID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.ControlParam, removedParamID);

			Editor.ParamControl._controlParams.Remove(cParam);
			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy();
		}




		public apAnimClip AddAnimClip(bool isSetRecord = true)
		{
			if (isSetRecord)
			{
				//Undo - Add Animation
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Main_AddAnimation, Editor._portrait, null, false, Editor);
			}

			//int iNextIndex = Editor._portrait.MakeUniqueID_AnimClip();
			int iNextIndex = 0;
			//int iNextUniqueID = Editor._portrait.MakeUniqueID_AnimClip();
			int iNextUniqueID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimClip);

			if (iNextUniqueID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Anim Clip Creating Failed", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimCreateFailed_Title),
												Editor.GetText(apLocalization.TEXT.AnimCreateFailed_Body),
												Editor.GetText(apLocalization.TEXT.Close));
				return null;
			}

			while (true)
			{
				bool isExist = Editor._portrait._animClips.Exists(delegate (apAnimClip a)
				{
					return a._name.Equals("New AnimClip (" + iNextIndex + ")");
				});

				if (isExist)
				{
					iNextIndex++;
				}
				else
				{
					break;
				}
			}

			string strNewName = "New AnimClip (" + iNextIndex + ")";

			apAnimClip newAnimClip = new apAnimClip();
			newAnimClip.Init(Editor._portrait, strNewName, iNextUniqueID);
			newAnimClip.LinkEditor(Editor._portrait);

			Editor._portrait._animClips.Add(newAnimClip);


			Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
			Editor.RefreshControllerAndHierarchy();

			//Animation Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Animation, true);

			return newAnimClip;
		}


		public void RemoveAnimClip(apAnimClip animClip)
		{
			//Remove - Animation
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Main_RemoveAnimation, Editor._portrait, animClip, false, Editor);
			if (Editor.Select.AnimClip == animClip)
			{
				Editor.Select.SetNone();
			}

			int removedAnimClipID = animClip._uniqueID;

			//Editor._portrait.PushUniqueID_AnimClip(removedAnimClipID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimClip, removedAnimClipID);

			Editor._portrait._animClips.Remove(animClip);
			

			//TODO : MeshGroup의 각 Modifier의 "Animated" 계열의 링크를 모두 끊어야 한다.
			Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋

			Editor._portrait.LinkAndRefreshInEditor();
			Editor.RefreshControllerAndHierarchy();
		}


		/// <summary>
		/// AnimClip을 복제하자
		/// </summary>
		/// <param name="srcAnimClip"></param>
		public void DuplicateAnimClip(apAnimClip srcAnimClip)
		{
			if (apVersion.I.IsDemo)
			{
				if (Editor._portrait._animClips.Count >= 2)
				{
					//이미 2개를 넘었다.
					//복사할 수 없다.
					EditorUtility.DisplayDialog(
						Editor.GetText(apLocalization.TEXT.DemoLimitation_Title),
						Editor.GetText(apLocalization.TEXT.DemoLimitation_Body_AddAnimation),
						Editor.GetText(apLocalization.TEXT.Okay)
						);

					return;
				}
			}


			//Undo - Anim Clip 복사
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_DupAnimClip, Editor._portrait, null, false, Editor);

			//일단 복사
			apAnimClip dupAnimClip = AddAnimClip(true);

			if (dupAnimClip == null)
			{
				return;
			}

			//1. AnimClip의 기본 정보를 복사한다.
			string dupAnimClipName = srcAnimClip._name + " Copy";

			//중복되지 않은 이름을 찾는다.
			if (Editor._portrait._animClips.Exists(delegate (apAnimClip a)
			{ return string.Equals(dupAnimClipName, a._name); }))
			{
				//오잉 똑같은 이름이 있네염...
				int copyIndex = -1;
				for (int iCopyIndex = 1; iCopyIndex < 1000; iCopyIndex++)
				{
					if (!Editor._portrait._animClips.Exists(delegate (apAnimClip a)
					{ return string.Equals(dupAnimClipName + " (" + iCopyIndex + ")", a._name); }))
					{
						copyIndex = iCopyIndex;
						break;
					}
				}
				if (copyIndex < 0)
				{ dupAnimClipName += " (1000+)"; }
				else
				{ dupAnimClipName += " (" + copyIndex + ")"; }
			}


			dupAnimClip._name = dupAnimClipName;
			dupAnimClip._portrait = srcAnimClip._portrait;
			dupAnimClip._targetMeshGroupID = srcAnimClip._targetMeshGroupID;
			dupAnimClip._targetMeshGroup = srcAnimClip._targetMeshGroup;
			dupAnimClip._targetOptTranform = srcAnimClip._targetOptTranform;

			dupAnimClip.SetOption_FPS(srcAnimClip.FPS);
			dupAnimClip.SetOption_StartFrame(srcAnimClip.StartFrame);
			dupAnimClip.SetOption_EndFrame(srcAnimClip.EndFrame);
			dupAnimClip.SetOption_IsLoop(srcAnimClip.IsLoop);

			//어떤 Src로 복사할지를 연결해둔다.
			Dictionary<apAnimTimeline, apAnimTimeline> dupTimelinePairs = new Dictionary<apAnimTimeline, apAnimTimeline>();
			Dictionary<apAnimTimelineLayer, apAnimTimelineLayer> dupTimelineLayerPairs = new Dictionary<apAnimTimelineLayer, apAnimTimelineLayer>();
			Dictionary<apAnimKeyframe, apAnimKeyframe> dupKeyframePairs = new Dictionary<apAnimKeyframe, apAnimKeyframe>();


			//2. Timeline을 하나씩 복사한다.
			for (int iT = 0; iT < srcAnimClip._timelines.Count; iT++)
			{
				apAnimTimeline srcTimeline = srcAnimClip._timelines[iT];
				apAnimTimeline dupTimeline = AddAnimTimeline(srcTimeline._linkType, srcTimeline._modifierUniqueID, dupAnimClip, false, false);
				if (dupTimeline == null)
				{
					//EditorUtility.DisplayDialog("Error", "Timeline Adding Failed.", "Close");
					EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimDuplicatedFailed_Title),
													Editor.GetText(apLocalization.TEXT.AnimDuplicatedFailed_Body),
													Editor.GetText(apLocalization.TEXT.Close));
					return;
				}

				dupTimeline._guiColor = srcTimeline._guiColor;
				dupTimeline._linkedModifier = srcTimeline._linkedModifier;
				dupTimeline._linkedOptModifier = srcTimeline._linkedOptModifier;

				//Dup - Src 순으로 복사된 Timeline을 저장하자.
				dupTimelinePairs.Add(dupTimeline, srcTimeline);

			}

			//Sync를 한번 해두자
			AddAndSyncAnimClipToModifier(dupAnimClip);

			foreach (KeyValuePair<apAnimTimeline, apAnimTimeline> timelinePair in dupTimelinePairs)
			{
				apAnimTimeline dupTimeline = timelinePair.Key;
				apAnimTimeline srcTimeline = timelinePair.Value;

				//3. TimelineLayer를 하나씩 복사한다.
				for (int iTL = 0; iTL < srcTimeline._layers.Count; iTL++)
				{
					apAnimTimelineLayer srcLayer = srcTimeline._layers[iTL];

					int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
					if (nextLayerID < 0)
					{
						//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
						EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimDuplicatedFailed_Title),
														Editor.GetText(apLocalization.TEXT.AnimDuplicatedFailed_Body),
														Editor.GetText(apLocalization.TEXT.Close));

						return;
					}

					apAnimTimelineLayer dupLayer = new apAnimTimelineLayer();
					dupLayer.Link(dupTimeline._parentAnimClip, dupTimeline);

					//if (dupTimeline._linkType == apAnimClip.LINK_TYPE.Bone)
					//{
					//	Debug.LogError("TODO : Bone 타입의 Timeline 복제가 구현되지 않았다.");
					//}
					dupLayer._uniqueID = nextLayerID;
					dupLayer._parentAnimClip = srcLayer._parentAnimClip;
					dupLayer._parentTimeline = srcLayer._parentTimeline;
					//dupLayer._isMeshTransform = srcLayer._isMeshTransform;

					dupLayer._linkModType = srcLayer._linkModType;

					dupLayer._transformID = srcLayer._transformID;
					dupLayer._linkedMeshTransform = srcLayer._linkedMeshTransform;
					dupLayer._linkedMeshGroupTransform = srcLayer._linkedMeshGroupTransform;
					dupLayer._linkedOptTransform = srcLayer._linkedOptTransform;
					dupLayer._guiColor = srcLayer._guiColor;
					dupLayer._targetParamSetGroup = srcLayer._targetParamSetGroup;
					dupLayer._boneID = srcLayer._boneID;
					dupLayer._controlParamID = srcLayer._controlParamID;
					dupLayer._linkedControlParam = srcLayer._linkedControlParam;
					dupLayer._linkType = srcLayer._linkType;
					dupLayer._linkedBone = srcLayer._linkedBone;//<<Bone 추가

					//Debug.LogError("TODO : Timeline Layer복사시 Linked Opt Bone도 복사할 것");


					dupTimeline._layers.Add(dupLayer);


					dupTimelineLayerPairs.Add(dupLayer, srcLayer);

					//이게 왜 필요했지????? @ㅅ@??
					////여기서 Link 한번
					//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
					//Editor._portrait.LinkAndRefreshInEditor();

					//apModifierParamSetGroup modParamSetGroup = null;
					//if (dupTimeline._linkedModifier != null)
					//{
					//	modParamSetGroup = dupTimeline._linkedModifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
					//	{
					//		return a._keyAnimTimelineLayer == dupLayer;
					//	});
					//}
				}
			}

			AddAndSyncAnimClipToModifier(dupAnimClip);

			//여기서 리프레시 한번 더
			dupAnimClip.RefreshTimelines();


			foreach (KeyValuePair<apAnimTimelineLayer, apAnimTimelineLayer> layerPair in dupTimelineLayerPairs)
			{
				apAnimTimelineLayer dupLayer = layerPair.Key;
				apAnimTimelineLayer srcLayer = layerPair.Value;

				//4. 키프레임도 복사하자.
				for (int iK = 0; iK < srcLayer._keyframes.Count; iK++)
				{
					apAnimKeyframe srcKeyframe = srcLayer._keyframes[iK];

					apAnimKeyframe dupKeyframe = AddAnimKeyframe(srcKeyframe._frameIndex, dupLayer, false, false, false, false);
					if (dupKeyframe == null)
					{
						//EditorUtility.DisplayDialog("Error", "Keyframe Adding Failed", "Closed");
						EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimDuplicatedFailed_Title),
														Editor.GetText(apLocalization.TEXT.AnimDuplicatedFailed_Body),
														Editor.GetText(apLocalization.TEXT.Close));
						return;
					}

					//Curkey 복사
					dupKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, srcKeyframe._frameIndex);
					dupKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;
					dupKeyframe._isActive = srcKeyframe._isActive;



					//Control Type 복사
					//dupKeyframe._conSyncValue_Bool = srcKeyframe._conSyncValue_Bool;
					dupKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
					dupKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
					dupKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;
					//dupKeyframe._conSyncValue_Vector3 = srcKeyframe._conSyncValue_Vector3;
					//dupKeyframe._conSyncValue_Color = srcKeyframe._conSyncValue_Color;

					dupKeyframe.Link(dupLayer);



					dupKeyframePairs.Add(dupKeyframe, srcKeyframe);
				}
			}

			//각 Keyframe<->Modifier Param 을 일괄 연결한 뒤에..
			AddAndSyncAnimClipToModifier(dupAnimClip);

			//여기서부터 ModMesh/ModBone 를 복사하도록 하자
			foreach (KeyValuePair<apAnimKeyframe, apAnimKeyframe> keyframePair in dupKeyframePairs)
			{
				apAnimKeyframe dupKeyframe = keyframePair.Key;
				apAnimKeyframe srcKeyframe = keyframePair.Value;

				apAnimTimelineLayer dupLayer = dupKeyframe._parentTimelineLayer;
				apAnimTimeline dupTimeline = dupKeyframe._parentTimelineLayer._parentTimeline;

				if (dupKeyframe._linkedModMesh_Editor != null && srcKeyframe._linkedModMesh_Editor != null)
				{
					//ModMesh 복사
					apModifiedMesh srcModMesh = srcKeyframe._linkedModMesh_Editor;
					apModifiedMesh dupModMesh = dupKeyframe._linkedModMesh_Editor;


					if (srcModMesh._vertices != null && srcModMesh._vertices.Count > 0)
					{
						if (dupModMesh._vertices == null)
						{ dupModMesh._vertices = new List<apModifiedVertex>(); }
						dupModMesh._vertices.Clear();

						apModifiedVertex srcModVert = null;
						apModifiedVertex dupModVert = null;
						for (int i = 0; i < srcModMesh._vertices.Count; i++)
						{
							srcModVert = srcModMesh._vertices[i];
							dupModVert = new apModifiedVertex();

							dupModVert._modifiedMesh = dupModMesh;
							dupModVert._vertexUniqueID = srcModVert._vertexUniqueID;
							dupModVert._vertIndex = srcModVert._vertIndex;

							dupModVert._mesh = srcModVert._mesh;
							dupModVert._vertex = srcModVert._vertex;
							dupModVert._deltaPos = srcModVert._deltaPos;

							dupModVert._overlapWeight = srcModVert._overlapWeight;

							dupModMesh._vertices.Add(dupModVert);
						}
					}

					if (dupModMesh._transformMatrix == null)
					{ dupModMesh._transformMatrix = new apMatrix(); }
					dupModMesh._transformMatrix.SetMatrix(srcModMesh._transformMatrix);

					dupModMesh._meshColor = srcModMesh._meshColor;
					dupModMesh._isVisible = srcModMesh._isVisible;
				}
				else if (dupKeyframe._linkedModBone_Editor != null && srcKeyframe._linkedModBone_Editor != null)
				{
					//Mod Bone 복사
					apModifiedBone srcModBone = srcKeyframe._linkedModBone_Editor;
					apModifiedBone dupModBone = dupKeyframe._linkedModBone_Editor;

					if (dupModBone._transformMatrix == null)
					{
						dupModBone._transformMatrix = new apMatrix();
					}

					dupModBone._transformMatrix.SetMatrix(srcModBone._transformMatrix);
				}
			}

			foreach (KeyValuePair<apAnimTimelineLayer, apAnimTimelineLayer> layerPair in dupTimelineLayerPairs)
			{
				//DupLayer를 Sort하자.
				int nRefreshed = layerPair.Key.SortAndRefreshKeyframes();

				//Debug.Log("Dup Layer Keyframe Refreshed [" + layerPair.Key.DisplayName + "] : " + nRefreshed);
			}

			#region [미사용 코드] ModMesh 복사 코드였으나 제대로 작동하지 않음
			//foreach (KeyValuePair<apAnimKeyframe, apAnimKeyframe> keyframePair in dupKeyframePairs)
			//{
			//	apAnimKeyframe dupKeyframe = keyframePair.Key;
			//	apAnimKeyframe srcKeyframe = keyframePair.Value;

			//	apAnimTimelineLayer dupLayer = dupKeyframe._parentTimelineLayer;
			//	apAnimTimeline dupTimeline = dupKeyframe._parentTimelineLayer._parentTimeline;

			//	//Anim Modifier 타입이면
			//	//1) Sync를 먼저하고
			//	//2) 등록된 ParamSet을 복사한다.
			//	if (dupTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			//	{
			//		//Modifier 자동 연결

			//		if (dupLayer._targetParamSetGroup != null)
			//		{

			//			apModifierParamSet paramSet = dupLayer._targetParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
			//				{
			//					return a.SyncKeyframe == dupKeyframe;
			//				});

			//			if (paramSet != null && paramSet._meshData.Count > 0)
			//			{
			//				dupKeyframe.LinkModMesh_Editor(paramSet, paramSet._meshData[0]);
			//			}
			//			else
			//			{
			//				if (paramSet == null)
			//				{
			//					Debug.LogError("Anim Mod Param Set 링크 실패 : Param Set이 Null이다. (" + dupLayer._targetParamSetGroup._paramSetList.Count + ")");
			//				}
			//				else
			//				{
			//					Debug.LogError("Anim Mod Param Set 링크 실패 : Mesh Data가 없다.");
			//				}
			//			}
			//		}



			//		if (dupKeyframe._linkedModMesh_Editor != null &&
			//			srcKeyframe._linkedModMesh_Editor != null)
			//		{
			//			apModifiedMesh srcModMesh = srcKeyframe._linkedModMesh_Editor;
			//			apModifiedMesh dupModMesh = dupKeyframe._linkedModMesh_Editor;

			//			dupModMesh.RefreshModifiedValues();

			//			if (srcModMesh._transformUniqueID != dupModMesh._transformUniqueID ||
			//				srcModMesh._isMeshTransform != dupModMesh._isMeshTransform)
			//			{
			//				Debug.LogError("Anim Modifier 타입의 Keyframe Mod Mesh 복사 실패 > Link된 Transform이 다르다.");
			//			}
			//			else
			//			{
			//				int nVertSrc = srcModMesh._vertices.Count;
			//				int nVertDup = dupModMesh._vertices.Count;
			//				if (nVertSrc == nVertDup)
			//				{
			//					for (int iVert = 0; iVert < nVertSrc; iVert++)
			//					{
			//						dupModMesh._vertices[iVert]._deltaPos = srcModMesh._vertices[iVert]._deltaPos;
			//						dupModMesh._vertices[iVert]._overlapWeight = srcModMesh._vertices[iVert]._overlapWeight;
			//					}
			//				}
			//				else
			//				{
			//					Debug.LogError("Anim Modifier 타입의 Keyframe Mod Mesh 복사시 Vertex 개수가 다르다");
			//				}

			//				dupModMesh._transformMatrix.SetMatrix(srcModMesh._transformMatrix);
			//				dupModMesh._meshColor = srcModMesh._meshColor;
			//				dupModMesh._isVisible = srcModMesh._isVisible;
			//			}

			//		}
			//		else
			//		{
			//			Debug.LogError("Anim Modifier 타입의 Keyframe Mod Mesh 복사 실패 > Null 발생 [" +
			//				"Src ModMesh is Null : " + (srcKeyframe._linkedModMesh_Editor == null)
			//				+ " / Dup ModMesh is Null : " + (dupKeyframe._linkedModMesh_Editor == null)
			//				+ " / Link Mod is Null : " + (dupTimeline._linkedModifier == null)
			//				+ " / Link Param Set Group is Null : " + (dupLayer._targetParamSetGroup == null) + "]");
			//		}

			//	}

			//} 
			#endregion





			dupAnimClip.RefreshTimelines();

			Editor._portrait.LinkAndRefreshInEditor();

			Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
			Editor.RefreshControllerAndHierarchy();

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

			Editor.Notification(srcAnimClip._name + " > " + dupAnimClip._name + " Duplicated", true, false);
		}


		public apAnimTimeline AddAnimTimeline(apAnimClip.LINK_TYPE linkType, int modifierUniqueID, apAnimClip targetAnimClip, bool errorMsg = true, bool isSetRecordAndRefresh = true)
		{
			if (targetAnimClip == null)
			{
				return null;
			}

			//Timeline을 추가해야한다.
			if (isSetRecordAndRefresh)
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_AddTimeline, Editor._portrait, null, false, Editor);
			}

			int nextTimelineID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimeline);
			if (nextTimelineID < 0)
			{
				if (errorMsg)
				{
					//EditorUtility.DisplayDialog("Error", "Timeline Adding Failed", "Close");
					EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimTimelineAddFailed_Title),
													Editor.GetText(apLocalization.TEXT.AnimTimelineAddFailed_Body),
													Editor.GetText(apLocalization.TEXT.Close));
				}
				return null;
			}



			apAnimTimeline newTimeline = new apAnimTimeline();
			newTimeline.Init(linkType, nextTimelineID, modifierUniqueID, targetAnimClip);

			targetAnimClip._timelines.Add(newTimeline);

			newTimeline.Link(targetAnimClip);

			if (isSetRecordAndRefresh)
			{
				//바로 Timeline을 선택한다.
				Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋

				Editor.Select.SetAnimTimeline(newTimeline, true);
				Editor.RefreshControllerAndHierarchy();

				//Refresh 추가
				Editor.Select.RefreshAnimEditing(true);
			}

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
			Editor.Hierarchy_AnimClip.RefreshUnits();

			return newTimeline;
		}

		public void RemoveAnimTimeline(apAnimTimeline animTimeline)
		{
			if (animTimeline == null)
			{
				return;
			}
			//Undo - Remove AnimTimeline
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_RemoveTimeline, Editor._portrait, animTimeline, false, Editor);

			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimTimeline, animTimeline._uniqueID);


			//선택중이면 제외
			Editor.Select.CancelAnimEditing();
			Editor.Select.SetAnimTimeline(null, true);
			if (Editor.Select.AnimTimeline != null)
			{
				Debug.LogError("Error!!! : AnimTimeline 해제가 안되었다!!");
			}

			apAnimClip parentAnimClip = animTimeline._parentAnimClip;
			if (parentAnimClip == null)
			{
				//?? 없네요.. 에러가..
				Debug.LogError("Error : AnimClip이 연결되지 않은 Timeline 제거");
			}
			else
			{
				animTimeline._linkedModifier = null;
				animTimeline._modifierUniqueID = -1;
				//뭔가 더 있어야하지 않으려나..

				parentAnimClip._timelines.Remove(animTimeline);

				//자동 삭제도 수행한다.
				parentAnimClip.RemoveUnlinkedTimeline();
			}

			//전체 Refresh를 해야한다.
			Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋

			Editor._portrait.LinkAndRefreshInEditor();
			Editor.RefreshControllerAndHierarchy();

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

		}



		public apAnimTimelineLayer AddAnimTimelineLayer(object targetObject, apAnimTimeline parentTimeline)
		{
			if (targetObject == null || parentTimeline == null)
			{
				return null;
			}

			//Undo - Add TimelineLayer
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_AddTimelineLayer, Editor._portrait, null, false, Editor);


			//이미 추가되었으면 리턴
			if (parentTimeline.IsObjectAddedInLayers(targetObject))
			{
				return null;
			}

			int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
			if (nextLayerID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimTimelineLayerAddFailed_Title),
												Editor.GetText(apLocalization.TEXT.AnimTimelineLayerAddFailed_Body),
												Editor.GetText(apLocalization.TEXT.Close));
				return null;
			}





			apAnimTimelineLayer newLayer = new apAnimTimelineLayer();
			newLayer.Link(parentTimeline._parentAnimClip, parentTimeline);

			switch (parentTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						int transformID = -1;
						bool isMeshTransform = false;
						if (targetObject is apTransform_Mesh)
						{
							apTransform_Mesh meshTransform = targetObject as apTransform_Mesh;
							transformID = meshTransform._transformUniqueID;
							isMeshTransform = true;

							newLayer.Init_TransformOfModifier(parentTimeline, nextLayerID, transformID, isMeshTransform);


						}
						else if (targetObject is apTransform_MeshGroup)
						{
							apTransform_MeshGroup meshGroupTransform = targetObject as apTransform_MeshGroup;
							transformID = meshGroupTransform._transformUniqueID;
							isMeshTransform = false;

							newLayer.Init_TransformOfModifier(parentTimeline, nextLayerID, transformID, isMeshTransform);
						}
						else if (targetObject is apBone)
						{
							apBone bone = targetObject as apBone;
							newLayer.Init_Bone(parentTimeline, nextLayerID, bone._uniqueID);
						}
						else
						{
							//?
							Debug.LogError(">> [Unknown Type]");
						}
					}
					break;


				case apAnimClip.LINK_TYPE.ControlParam:
					{
						int controlParamID = -1;
						if (targetObject is apControlParam)
						{
							apControlParam controlParam = targetObject as apControlParam;
							controlParamID = controlParam._uniqueID;
						}
						newLayer.Init_ControlParam(parentTimeline, nextLayerID, controlParamID);
					}
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입의 Layer 추가 코드 필요[" + parentTimeline._linkType + "]");
					break;
			}

			parentTimeline._layers.Add(newLayer);

			//전체 Refresh를 해야한다.
			if(parentTimeline._parentAnimClip._targetMeshGroup != null)
			{
				parentTimeline._parentAnimClip._targetMeshGroup.LinkModMeshRenderUnits();
				parentTimeline._parentAnimClip._targetMeshGroup.RefreshModifierLink();
				parentTimeline._parentAnimClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();
			}

			
			Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋




			Editor._portrait.LinkAndRefreshInEditor();
			Editor.RefreshControllerAndHierarchy();

			if(parentTimeline._linkedModifier != null)
			{
				parentTimeline._linkedModifier.RefreshParamSet();
			}

			Editor.Select.SetAnimTimelineLayer(newLayer, true);

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

			Editor.Select.AutoSelectAnimWorkKeyframe();

			return newLayer;
		}


		public apAnimTimelineLayer AddAnimTimelineLayerForAllTransformObject(apMeshGroup parentMeshGroup, bool isTargetTransform, bool isAddChildTransformAddable, apAnimTimeline parentTimeline)
		{
			if (parentMeshGroup == null || parentTimeline == null)
			{
				return null;
			}

			//Undo - Add TimelineLayer
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_AddTimelineLayer, Editor._portrait, null, false, Editor);

			List<object> targetObjects = new List<object>();

			//목표를 리스트로 찾자
			FindChildTransformsOrBones(parentMeshGroup, parentMeshGroup._rootMeshGroupTransform, isTargetTransform, targetObjects, isAddChildTransformAddable);

			apAnimTimelineLayer firstLayer = null;
			int startFrame = parentTimeline._parentAnimClip.StartFrame;

			List<apAnimTimelineLayer> addedLayers = new List<apAnimTimelineLayer>();
			for (int iTargetObjects = 0; iTargetObjects < targetObjects.Count; iTargetObjects++)
			{
				object targetObject = targetObjects[iTargetObjects];

				//이미 추가되었으면 리턴
				if (parentTimeline.IsObjectAddedInLayers(targetObject))
				{
					continue;
				}

				int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
				if (nextLayerID < 0)
				{
					//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
					EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimTimelineLayerAddFailed_Title),
													Editor.GetText(apLocalization.TEXT.AnimTimelineLayerAddFailed_Body),
													Editor.GetText(apLocalization.TEXT.Close));

					return null;
				}





				apAnimTimelineLayer newLayer = new apAnimTimelineLayer();
				newLayer.Link(parentTimeline._parentAnimClip, parentTimeline);

				if (firstLayer == null)
				{
					firstLayer = newLayer;
				}

				switch (parentTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						{
							int transformID = -1;
							bool isMeshTransform = false;
							if (targetObject is apTransform_Mesh)
							{
								apTransform_Mesh meshTransform = targetObject as apTransform_Mesh;
								transformID = meshTransform._transformUniqueID;
								isMeshTransform = true;

								newLayer.Init_TransformOfModifier(parentTimeline, nextLayerID, transformID, isMeshTransform);


							}
							else if (targetObject is apTransform_MeshGroup)
							{
								apTransform_MeshGroup meshGroupTransform = targetObject as apTransform_MeshGroup;
								transformID = meshGroupTransform._transformUniqueID;
								isMeshTransform = false;

								newLayer.Init_TransformOfModifier(parentTimeline, nextLayerID, transformID, isMeshTransform);
							}
							else if (targetObject is apBone)
							{
								apBone bone = targetObject as apBone;
								newLayer.Init_Bone(parentTimeline, nextLayerID, bone._uniqueID);
							}
							else
							{
								//?
								Debug.LogError(">> [Unknown Type]");
							}
						}
						break;


					case apAnimClip.LINK_TYPE.ControlParam:
						{
							int controlParamID = -1;
							if (targetObject is apControlParam)
							{
								apControlParam controlParam = targetObject as apControlParam;
								controlParamID = controlParam._uniqueID;
							}
							newLayer.Init_ControlParam(parentTimeline, nextLayerID, controlParamID);
						}
						break;

					default:
						Debug.LogError("TODO : 정의되지 않은 타입의 Layer 추가 코드 필요[" + parentTimeline._linkType + "]");
						break;
				}

				parentTimeline._layers.Add(newLayer);


				//시작 프레임에 Keyframe을 추가하자
				//AddAnimKeyframe(startFrame, newLayer, false, false, false, false);
				//>> 이걸 Refresh 후로 미루자
				addedLayers.Add(newLayer);
			}


			//전체 Refresh를 해야한다.
			if(parentTimeline._parentAnimClip._targetMeshGroup != null)
			{
				parentTimeline._parentAnimClip._targetMeshGroup.LinkModMeshRenderUnits();
				parentTimeline._parentAnimClip._targetMeshGroup.RefreshModifierLink();
				parentTimeline._parentAnimClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();
			}


			Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋

			Editor._portrait.LinkAndRefreshInEditor();
			Editor.RefreshControllerAndHierarchy();

			//if(parentTimeline._linkedModifier != null)
			//{
			//	Debug.Log("AnimLayer Add -> RefreshParamSet");
			//	parentTimeline._linkedModifier.RefreshParamSet();
			//}

			for (int i = 0; i < addedLayers.Count; i++)
			{
				AddAnimKeyframe(startFrame, addedLayers[i], false, false, false, false);
			}
			//다시 Refresh
			Editor._portrait.LinkAndRefreshInEditor();
			Editor.RefreshControllerAndHierarchy();

			Editor.Select.SetAnimTimelineLayer(firstLayer, true);

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

			Editor.Select.AutoSelectAnimWorkKeyframe();

			return firstLayer;
		}

		/// <summary>
		/// AnimTimelineLayer에 오브젝트를 추가하기 위해 "모든 Mesh/MeshGroup Transform"을 찾거나 "Bone"을 찾는 함수
		/// </summary>
		public void FindChildTransformsOrBones(apMeshGroup meshGroup, apTransform_MeshGroup meshGroupTransform, bool isTargetTransform, List<object> resultList, bool isChildTransformSupport)
		{
			if (isTargetTransform)
			{
				if (meshGroup != meshGroupTransform._meshGroup)
				{
					resultList.Add(meshGroupTransform);
				}


				for (int i = 0; i < meshGroupTransform._meshGroup._childMeshTransforms.Count; i++)
				{
					resultList.Add(meshGroupTransform._meshGroup._childMeshTransforms[i]);
				}
			}
			else
			{
				List<apBone> bones = new List<apBone>();
				for (int i = 0; i < meshGroupTransform._meshGroup._boneList_Root.Count; i++)
				{
					MakeRecursiveList(meshGroupTransform._meshGroup._boneList_Root[i], bones);
				}

				for (int i = 0; i < bones.Count; i++)
				{
					resultList.Add(bones[i]);
				}

			}

			if (isChildTransformSupport)
			{
				for (int i = 0; i < meshGroupTransform._meshGroup._childMeshGroupTransforms.Count; i++)
				{
					apTransform_MeshGroup childMeshGroup = meshGroupTransform._meshGroup._childMeshGroupTransforms[i];
					FindChildTransformsOrBones(meshGroup, childMeshGroup, isTargetTransform, resultList, isChildTransformSupport);
				}
			}
		}

		private void MakeRecursiveList(apBone targetBone, List<apBone> resultList)
		{
			resultList.Add(targetBone);
			if (targetBone._childBones != null)
			{
				for (int i = 0; i < targetBone._childBones.Count; i++)
				{
					MakeRecursiveList(targetBone._childBones[i], resultList);
				}
			}
		}


		public void RemoveAnimTimelineLayer(apAnimTimelineLayer animTimelineLayer)
		{
			if (animTimelineLayer == null)
			{
				return;
			}
			//Undo - Remove Anim Timeline Layer
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_RemoveTimelineLayer, Editor._portrait, animTimelineLayer, false, Editor);

			//ID 반납
			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimTimelineLayer, animTimelineLayer._uniqueID);



			//선택중이면 제외
			if (Editor.Select.AnimTimelineLayer == animTimelineLayer)
			{
				Editor.Select.SetAnimTimelineLayer(null, true);
			}

			apAnimTimeline parnetTimeline = animTimelineLayer._parentTimeline;
			if (parnetTimeline != null)
			{
				parnetTimeline._layers.Remove(animTimelineLayer);

				animTimelineLayer._transformID = -1;
				animTimelineLayer._boneID = -1;
				animTimelineLayer._controlParamID = -1;

				//자동 삭제도 해준다.
				parnetTimeline.RemoveUnlinkedLayer();
			}
			else
			{
				Debug.LogError("Error : Parent Timeline이 없는 Layer 제거 시도");
			}

			//전체 Refresh를 해야한다.
			Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋

			Editor._portrait.LinkAndRefreshInEditor();
			Editor.RefreshControllerAndHierarchy();

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);
		}

		//TODO : 이거 사용해야한다. + 추가하면 Modifier와 연동바로 할 것
		public apAnimKeyframe AddAnimKeyframe(int targetFrame, apAnimTimelineLayer parentLayer, bool isMakeCurrentBlendData, bool isErrorMsg = true, bool isSetRecord = true, bool isRefresh = true)
		{
			if (parentLayer == null)
			{
				return null;
			}

			if (isSetRecord)
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_AddKeyframe, Editor._portrait, null, false, Editor);
			}

			apAnimKeyframe existFrame = parentLayer.GetKeyframeByFrameIndex(targetFrame);
			if (existFrame != null)
			{
				//이미 해당 프레임에 값이 있다.
				//EditorUtility.DisplayDialog("Error", "Keyframe is already Added", "Closed");
				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimKeyframeAddFailed_Title),
												Editor.GetText(apLocalization.TEXT.AnimKeyframeAddFailed_Body_Already),
												Editor.GetText(apLocalization.TEXT.Close));

				return null;
			}

			int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
			if (nextKeyframeID < 0)
			{
				if (isErrorMsg)
				{
					//EditorUtility.DisplayDialog("Error", "Keyframe Adding Failed", "Closed");
					EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimKeyframeAddFailed_Title),
													Editor.GetText(apLocalization.TEXT.AnimKeyframeAddFailed_Body_Error),
													Editor.GetText(apLocalization.TEXT.Close));

				}
				return null;
			}



			apAnimKeyframe newKeyframe = new apAnimKeyframe();
			newKeyframe.Init(nextKeyframeID, targetFrame);
			newKeyframe.Link(parentLayer);

			if (isMakeCurrentBlendData)
			{
				if (apEditor.IS_DEBUG_DETAILED)
				{
					Debug.LogError("TODO : Set Key -> isMakeCurrentBlendData 현재 중간 값을 이용해서 ModMesh 값을 세팅한다.");
				}
			}


			parentLayer._keyframes.Add(newKeyframe);

			if (parentLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				//Modifier에 연동되는 타입이라면
				//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				AddAndSyncAnimClipToModifier(parentLayer._parentTimeline._parentAnimClip);
			}

			//전체 Refresh를 해야한다.
			if (isRefresh)
			{
				Editor._portrait.LinkAndRefreshInEditor();
				Editor.RefreshControllerAndHierarchy();

				//Refresh 추가
				Editor.Select.RefreshAnimEditing(true);
			}
			return newKeyframe;
		}



		public apAnimKeyframe AddCopiedAnimKeyframe(int targetFrameIndex, apAnimTimelineLayer parentLayer, bool isMakeCurrentBlendData, apAnimKeyframe srcKeyframe, bool isRefresh)
		{
			if (parentLayer == null)
			{
				return null;
			}

			//Undo - 키프레임 복사
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_DupKeyframe, Editor._portrait, null, false, Editor);

			int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
			if (nextKeyframeID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Keyframe Adding Failed", "Closed");
				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.AnimKeyframeAddFailed_Title),
												Editor.GetText(apLocalization.TEXT.AnimKeyframeAddFailed_Body_Error),
												Editor.GetText(apLocalization.TEXT.Close));
				return null;
			}



			apAnimKeyframe newKeyframe = new apAnimKeyframe();
			newKeyframe.Init(nextKeyframeID, targetFrameIndex);
			newKeyframe.Link(parentLayer);

			if (isMakeCurrentBlendData)
			{
				//Debug.LogError("TODO : Set Key -> isMakeCurrentBlendData");
			}


			parentLayer._keyframes.Add(newKeyframe);

			if (parentLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				//Modifier에 연동되는 타입이라면
				//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				//Debug.Log("Start Sync To Animated Modifier");
				AddAndSyncAnimClipToModifier(parentLayer._parentTimeline._parentAnimClip);
			}
			Editor.RefreshTimelineLayers(false);

			//값을 넣어서 복사하자
			if (srcKeyframe != null)
			{
				newKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, newKeyframe._frameIndex);
				newKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;

				//newKeyframe._conSyncValue_Bool = srcKeyframe._conSyncValue_Bool;
				newKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
				newKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
				newKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;
				//newKeyframe._conSyncValue_Vector3 = srcKeyframe._conSyncValue_Vector3;
				//newKeyframe._conSyncValue_Color = srcKeyframe._conSyncValue_Color;


				if (newKeyframe._linkedModMesh_Editor != null && srcKeyframe._linkedModMesh_Editor != null)
				{
					//Mod Mesh 값을 복사하자
					List<apModifiedVertex> srcVertList = srcKeyframe._linkedModMesh_Editor._vertices;
					apMatrix srcTransformMatrix = srcKeyframe._linkedModMesh_Editor._transformMatrix;
					Color srcMeshColor = srcKeyframe._linkedModMesh_Editor._meshColor;
					bool isVisible = srcKeyframe._linkedModMesh_Editor._isVisible;

					newKeyframe._linkedModMesh_Editor._transformMatrix.SetMatrix(srcTransformMatrix);
					newKeyframe._linkedModMesh_Editor._meshColor = srcMeshColor;
					newKeyframe._linkedModMesh_Editor._isVisible = isVisible;

					apModifiedVertex srcModVert = null;
					apModifiedVertex dstModVert = null;
					for (int i = 0; i < srcVertList.Count; i++)
					{
						srcModVert = srcVertList[i];
						dstModVert = newKeyframe._linkedModMesh_Editor._vertices.Find(delegate (apModifiedVertex a)
						{
							return a._vertexUniqueID == srcModVert._vertexUniqueID;
						});
						if (dstModVert != null)
						{
							dstModVert._deltaPos = srcModVert._deltaPos;
						}
					}
				}

				if (newKeyframe._linkedModBone_Editor != null && srcKeyframe._linkedModBone_Editor != null)
				{
					//ModBone도 복사하자
					if (newKeyframe._linkedModBone_Editor._transformMatrix == null)
					{
						newKeyframe._linkedModBone_Editor._transformMatrix = new apMatrix();
					}
					newKeyframe._linkedModBone_Editor._transformMatrix.SetMatrix(srcKeyframe._linkedModBone_Editor._transformMatrix);
				}

				//else
				//{
				//	//만약 Src만 있다면 체크해볼 필요가 있다. 연동이 안된 상태에서 복사를 시도했기 때문
				//	if (srcKeyframe._linkedModMesh_Editor != null)
				//	{
				//		Debug.LogError("Copy Keyframe Error : No Linked ModMesh");
				//	}
				//}
			}



			//전체 Refresh를 해야한다.
			Editor._portrait.LinkAndRefreshInEditor();
			if (isRefresh)
			{
				Editor.RefreshControllerAndHierarchy();

				//Refresh 추가
				Editor.Select.RefreshAnimEditing(true);
			}

			return newKeyframe;
		}

		public void RemoveKeyframe(apAnimKeyframe animKeyframe)
		{
			if (animKeyframe == null)
			{
				return;
			}
			//Undo - Remove Keyframe
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_RemoveKeyframe, Editor._portrait, animKeyframe, false, Editor);

			//ID 반탑
			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimKeyFrame, animKeyframe._uniqueID);



			//선택중이면 제외
			if (Editor.Select.AnimKeyframe == animKeyframe || Editor.Select.AnimKeyframes.Contains(animKeyframe))
			{
				Editor.Select.SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
			}

			//Timeline Layer에서 삭제
			apAnimTimelineLayer parentLayer = animKeyframe._parentTimelineLayer;
			if (parentLayer != null)
			{
				parentLayer._keyframes.Remove(animKeyframe);
			}

			//전체 Refresh를 해야한다.
			Editor._portrait.LinkAndRefreshInEditor();
			Editor.RefreshControllerAndHierarchy();
			Editor.RefreshTimelineLayers(false);

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);
		}

		public void RemoveKeyframes(List<apAnimKeyframe> animKeyframes)
		{
			if (animKeyframes == null || animKeyframes.Count == 0)
			{
				return;
			}

			//Undo - Remove Keyframes : 여러개를 동시에 삭제하지만 Multiple은 아니고 리스트값을 넣어주자
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_RemoveKeyframe, Editor._portrait, animKeyframes, false, Editor);


			List<apAnimKeyframe> targetKeyframes = new List<apAnimKeyframe>();
			for (int i = 0; i < animKeyframes.Count; i++)
			{
				targetKeyframes.Add(animKeyframes[i]);
			}



			for (int i = 0; i < targetKeyframes.Count; i++)
			{
				apAnimKeyframe animKeyframe = targetKeyframes[i];

				//ID 반납
				Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimKeyFrame, animKeyframe._uniqueID);

				//선택중이면 제외
				if (Editor.Select.AnimKeyframe == animKeyframe || Editor.Select.AnimKeyframes.Contains(animKeyframe))
				{
					Editor.Select.SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
				}

				//Timeline Layer에서 삭제
				apAnimTimelineLayer parentLayer = animKeyframe._parentTimelineLayer;
				if (parentLayer != null)
				{
					parentLayer._keyframes.Remove(animKeyframe);
				}
			}


			//전체 Refresh를 해야한다.
			Editor._portrait.LinkAndRefreshInEditor();
			Editor.RefreshControllerAndHierarchy();
			Editor.RefreshTimelineLayers(false);

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);
		}


		//AnimClip / Timeline / TimelineLayer / Keyframe과 Modifier 연동
		/// <summary>
		/// 이미 생성된 Timeline/Timelinelayer에 대해서 Modifier 내부의 ParamSetGroup/ParamSet 까지 만들어서
		/// ModMesh를 생성하게 한다. > 수정) 링크된 값에 따라 ModBone을 만든다.
		/// (중복을 체크하여 자동으로 만드므로 Refresh에 가깝다)
		/// </summary>
		/// <param name="animClip"></param>
		public void AddAndSyncAnimClipToModifier(apAnimClip animClip, bool isPrintLog = false)
		{
			apMeshGroup targetMeshGroup = animClip._targetMeshGroup;
			if (targetMeshGroup == null)
			{
				//if (apEditor.IS_DEBUG_DETAILED)
				//{
				//	Debug.LogError("AddAndSyncAnimClipToModifier Error : Target Mesh Group이 없다. [" + animClip._name + "]");
				//}
				return;
			}
			apModifierStack modStack = targetMeshGroup._modifierStack;
			List<apAnimTimeline> timelines = animClip._timelines;

			apAnimTimeline curTimeline = null;
			for (int iTimeline = 0; iTimeline < timelines.Count; iTimeline++)
			{
				curTimeline = timelines[iTimeline];

				//if(isPrintLog)
				//{
				//	Debug.Log("Check Timeline Link [" + curTimeline.DisplayName + "]");
				//}

				if (curTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					//Modifier 연동 타입일 때
					apModifierBase modifier = curTimeline._linkedModifier;//링크가 되어있어야 한다.
					if (!modStack._modifiers.Contains(modifier))
					{
						//Modifier가 없는데요?
						if (apEditor.IS_DEBUG_DETAILED)
						{
							Debug.LogError("Anim-Modifier Sync 문제 : Modifier를 찾을 수 없다. [Is Null : " + (modifier == null) + "]");
						}
						continue;
					}

					if (isPrintLog) { Debug.Log("Anim Modifier Check"); }

					apAnimTimelineLayer curLayer = null;
					for (int iLayer = 0; iLayer < curTimeline._layers.Count; iLayer++)
					{
						curLayer = curTimeline._layers[iLayer];

						//if (isPrintLog)
						//{
						//	Debug.Log("Layer > ParamSetGroup [Clip ID : " + animClip._uniqueID 
						//		+ " / Timeline ID : " + curTimeline._uniqueID 
						//		+ " / Layer ID : " + curLayer._uniqueID);
						//}

						List<apModifierParamSetGroup> paramSetGroupList = modifier._paramSetGroup_controller;
						apModifierParamSetGroup paramSetGroup = paramSetGroupList.Find(delegate (apModifierParamSetGroup a)
						{
							if (a._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame)
							{
								if (a._keyAnimClipID == animClip._uniqueID &&
									a._keyAnimTimelineID == curTimeline._uniqueID &&
									a._keyAnimTimelineLayerID == curLayer._uniqueID)
								{
								//ID가 모두 동일하다 (링크가 될 paramSetGroup이 이미 만들어졌다.
								return true;
								}
							}

							return false;
						});

						bool isAddParamSetGroup = false;
						if (paramSetGroup == null)
						{
							//if (isPrintLog)
							//{
							//	Debug.LogError("Not Param Set Group => Make ParamSetGroup");
							//}
							isAddParamSetGroup = true;
							//Debug.LogError("Add ParamSetGroup <- " + curLayer.DisplayName);
							//TODO : 여기서부터 작업을 하자
							paramSetGroup = new apModifierParamSetGroup(Editor._portrait, modifier, modifier.GetNextParamSetLayerIndex());
							paramSetGroup.SetTimeline(animClip, curTimeline, curLayer);
							paramSetGroup.LinkPortrait(Editor._portrait, modifier);

							modifier._paramSetGroup_controller.Add(paramSetGroup);
							//여기서는 ParamSet을 등록하진 않는다.
						}
						else
						{
							//if (isPrintLog)
							//{
							//	Debug.Log("Exist Param Set Group => Link ParamSetGroup");
							//}

							//Debug.Log("Link ParamSetGroup <- " + curLayer.DisplayName);
							//연동을 한번 더 해주자
							paramSetGroup._keyAnimClip = animClip;
							paramSetGroup._keyAnimTimeline = curTimeline;
							paramSetGroup._keyAnimTimelineLayer = curLayer;
							curLayer.LinkParamSetGroup(paramSetGroup);
						}

						//연동될 Tranform이 있는지 확인하자
						int linkedTransformID = -1;
						apTransform_Mesh linkedMeshTransform = null;
						apTransform_MeshGroup linkedMeshGroupTransform = null;
						apBone linkedBone = null;
						int linkedBoneID = -1;

						if (curLayer._linkedMeshTransform != null)
						{
							linkedMeshTransform = curLayer._linkedMeshTransform;
							linkedTransformID = linkedMeshTransform._transformUniqueID;
						}
						else if (curLayer._linkedMeshGroupTransform != null)
						{
							linkedMeshGroupTransform = curLayer._linkedMeshGroupTransform;
							linkedTransformID = linkedMeshGroupTransform._transformUniqueID;
						}
						else if (curLayer._linkedBone != null)
						{
							linkedBone = curLayer._linkedBone;
							linkedBoneID = linkedBone._uniqueID;
						}
						//TODO : Bone에 대해서도 처리해야한다.
						else
						{
							if (isPrintLog)
							{
								Debug.LogError("Link Transform Failed : Null in Layer");
							}
						}




						//Key를 추가해준다.
						List<apModifierParamSet> paramSetList = paramSetGroup._paramSetList;
						apAnimKeyframe curKeyframe = null;
						for (int iKeyframe = 0; iKeyframe < curLayer._keyframes.Count; iKeyframe++)
						{
							curKeyframe = curLayer._keyframes[iKeyframe];
							apModifierParamSet targetParamSet = paramSetList.Find(delegate (apModifierParamSet a)
							{
								return a.SyncKeyframe == curKeyframe;
							});

							if (targetParamSet == null)
							{
								//Debug.LogError("Add ParamSet <- Keyframe " + curKeyframe._frameIndex);
								//없다면 생성
								targetParamSet = new apModifierParamSet();
								targetParamSet.LinkParamSetGroup(paramSetGroup);
								targetParamSet.LinkSyncKeyframe(curKeyframe);

								paramSetList.Add(targetParamSet);
							}
							else
							{
								//Debug.Log("Link ParamSet <- Keyframe " + curKeyframe._frameIndex);
								//이미 있다면 서로 연결
								targetParamSet.LinkParamSetGroup(paramSetGroup);
								targetParamSet.LinkSyncKeyframe(curKeyframe);
							}


							if (linkedTransformID >= 0)
							{
								apModifiedMesh addedModMesh = null;
								//Modifier에 연동을 해주자
								if (linkedMeshTransform != null)
								{
									if (isAddParamSetGroup)
									{
										//Debug.Log("Add ModMesh > MeshTransform");
									}
									//ModeMesh는 추가하되, Refresh는 나중에 하자 (마지막 인자를 false로 둔다)
									addedModMesh = modifier.AddMeshTransform(targetMeshGroup, linkedMeshTransform, targetParamSet, true, true, false);
								}
								else if (linkedMeshGroupTransform != null)
								{
									if (isAddParamSetGroup)
									{
										//Debug.Log("Add ModMesh > MeshGroupTransform");
									}
									//ModeMesh는 추가하되, Refresh는 나중에 하자 (마지막 인자를 false로 둔다)
									addedModMesh = modifier.AddMeshGroupTransform(targetMeshGroup, linkedMeshGroupTransform, targetParamSet, true, true, false);
								}
								if (addedModMesh == null)
								{
									//Debug.LogError("Add Mod Mesh Failed");
									curKeyframe.LinkModMesh_Editor(targetParamSet, null);
								}
								else
								{
									curKeyframe.LinkModMesh_Editor(targetParamSet, addedModMesh);
								}
							}
							else if (linkedBoneID > 0)
							{
								//Bone 추가
								apModifiedBone addModBone = modifier.AddBone(linkedBone, targetParamSet, true, false);
								if (addModBone == null)
								{
									curKeyframe.LinkModBone_Editor(targetParamSet, null);
								}
								else
								{
									curKeyframe.LinkModBone_Editor(targetParamSet, addModBone);
								}
							}

						}

						
						paramSetGroup.RefreshSync();
					}





				}
			}
			//ModMesh 링크를 여기서 일괄적으로 처리
			targetMeshGroup.RefreshModifierLink();
		}




		public void CopyAnimCurveToAllKeyframes(apAnimCurveResult srcCurveResult, apAnimTimelineLayer animLayer, apAnimClip animClip)
		{
			if(srcCurveResult == null || animLayer == null || animClip == null)
			{
				Debug.LogError("CopyAnimCurveToAllKeyframes 실패1");
				return;
			}
			if(srcCurveResult._curveKeyA == null || srcCurveResult._curveKeyB == null)
			{
				Debug.LogError("CopyAnimCurveToAllKeyframes 실패2");
				return;
			}

			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor._portrait, animClip._targetMeshGroup, false, Editor);

			
			for (int i = 0; i < animLayer._keyframes.Count; i++)
			{
				apAnimKeyframe keyframe = animLayer._keyframes[i];
				if(keyframe._curveKey._prevCurveResult != srcCurveResult)
				{
					keyframe._curveKey._prevCurveResult.CopyCurve(srcCurveResult);
				}
				

				if(keyframe._curveKey._nextCurveResult != srcCurveResult)
				{
					keyframe._curveKey._nextCurveResult.CopyCurve(srcCurveResult);
				}
			}
			
		}



		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------


		public apMeshGroup AddMeshGroup()
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			//연결할 GameObjectGroup을 체크하자
			CheckAndMakeObjectGroup();


			//Undo - Add Mesh Group
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Main_AddMeshGroup, Editor._portrait, null, false, Editor);


			//int nextID = Editor._portrait.MakeUniqueID_MeshGroup();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.MeshGroup);
			int nextRootTransformID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);

			if (nextID < 0 || nextRootTransformID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Mesh Add Group Failed. Please Retry", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.MeshGroupAddFailed_Title),
												Editor.GetText(apLocalization.TEXT.MeshGroupAddFailed_Body),
												Editor.GetText(apLocalization.TEXT.Close));
				return null;
			}


			int nMeshGroups = Editor._portrait._meshGroups.Count;

			//GameObject로 만드는 경우
			string newName = "New Mesh Group (" + nMeshGroups + ")";
			GameObject newGameObj = new GameObject(newName);
			newGameObj.transform.parent = Editor._portrait._subObjectGroup_MeshGroup.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			apMeshGroup newGroup = newGameObj.AddComponent<apMeshGroup>();

			//apMeshGroup newGroup = new apMeshGroup();

			newGroup._uniqueID = nextID;
			newGroup._presetType = apMeshGroup.PRESET_TYPE.Default;
			newGroup._name = newName;

			newGroup.MakeRootTransform(nextRootTransformID);//<<추가 : Root Transform 생성

			newGroup.Init(Editor._portrait);

			Editor._portrait._meshGroups.Add(newGroup);
			//Debug.Log("MeshGroup Added");

			newGroup.RefreshModifierLink();
			Editor._portrait.LinkAndRefreshInEditor();

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy();
			//Editor.Hierarchy.RefreshUnits();

			//MeshGroup Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.MeshGroup, true);

			//Undo - Create 추가
			apEditorUtil.SetRecordCreateMonoObject(newGroup, "Create MeshGroup");

			return newGroup;
		}

		public void RemoveMeshGroup(apMeshGroup meshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}

			//Undo - Remove MeshGroup
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Main_RemoveMeshGroup, Editor._portrait, meshGroup, false, Editor);

			int meshGroupID = meshGroup._uniqueID;
			//Editor._portrait.PushUniqueID_MeshGroup(meshGroupID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.MeshGroup, meshGroupID);

			Editor._portrait._meshGroups.Remove(meshGroup);

			if (meshGroup != null)
			{
				Undo.DestroyObjectImmediate(meshGroup.gameObject);
			}

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy();
			//Editor.Hierarchy.RefreshUnits();
		}

		public void DetachMeshInMeshGroup(apTransform_Mesh targetMeshTransform, apMeshGroup parentMeshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}
			//Undo - Detach
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_DetachMesh, parentMeshGroup, targetMeshTransform, false, Editor);

			int removedUniqueID = targetMeshTransform._transformUniqueID;
			//Editor._portrait.PushUniqueID_Transform(removedUniqueID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, removedUniqueID);

			parentMeshGroup._childMeshTransforms.Remove(targetMeshTransform);

			parentMeshGroup.ResetRenderUnits();
			parentMeshGroup.RefreshModifierLink();
			parentMeshGroup.SortRenderUnits(true);

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy();
		}

		public void DetachMeshGroupInMeshGroup(apTransform_MeshGroup targetMeshGroupTransform, apMeshGroup parentMeshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}
			//Undo - Detach
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_DetachMeshGroup, parentMeshGroup, targetMeshGroupTransform, false, Editor);

			if (targetMeshGroupTransform._meshGroup != null)
			{
				targetMeshGroupTransform._meshGroup._parentMeshGroup = null;
				targetMeshGroupTransform._meshGroup._parentMeshGroupID = -1;

				//targetMeshGroupTransform._meshGroup.SortRenderUnits(true);
			}

			int removedUniqueID = targetMeshGroupTransform._transformUniqueID;
			//Editor._portrait.PushUniqueID_Transform(removedUniqueID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, removedUniqueID);

			parentMeshGroup._childMeshGroupTransforms.Remove(targetMeshGroupTransform);

			parentMeshGroup.ResetRenderUnits();
			parentMeshGroup.RefreshModifierLink();
			parentMeshGroup.SortRenderUnits(true);

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy();
		}

		public void RefreshMeshGroups()
		{
			List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;
			for (int i = 0; i < meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = meshGroups[i];

				List<apRenderUnit> removableRenderUnits = new List<apRenderUnit>();

				if (meshGroup._rootRenderUnit == null)
				{
					continue;
				}
				CheckRemovableRenderUnit(meshGroup, meshGroup._rootRenderUnit, removableRenderUnits);

				if (removableRenderUnits.Count > 0)
				{
					meshGroup._renderUnits_All.RemoveAll(delegate (apRenderUnit a)
					{
						return removableRenderUnits.Contains(a);
					});

					meshGroup.SetDirtyToReset();
					meshGroup.RefreshForce();
					meshGroup.SortRenderUnits(true);
				}

				//Bone Refresh도 여기서 하자
				RefreshBoneHierarchy(meshGroup);
				RefreshBoneChaining(meshGroup);
			}
		}




		private void CheckRemovableRenderUnit(apMeshGroup parentMeshGroup, apRenderUnit curRenderUnit, List<apRenderUnit> removableRenderUnits)
		{
			bool isRemovable = false;
			if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
			{
				//메시가 존재하는가
				if (curRenderUnit._meshTransform == null)
				{ isRemovable = true; Debug.LogError("R 1"); }
				else if (curRenderUnit._meshTransform._mesh == null)
				{ isRemovable = true; Debug.LogError("R 2"); }
				else if (GetMesh(curRenderUnit._meshTransform._mesh._uniqueID) == null)
				{ isRemovable = true; Debug.LogError("R 3"); }
			}
			else
			{
				//메시 그룹이 존재하는가?
				//if(curRenderUnit._meshGroupTransform == null)											{ isRemovable = true; Debug.LogError("R 4"); }
				if (curRenderUnit._meshGroup == null)
				{ isRemovable = true; Debug.LogError("R 5"); }
				else if (GetMeshGroup(curRenderUnit._meshGroup._uniqueID) == null)
				{
					isRemovable = true;
					Debug.LogError("R 6 - " + curRenderUnit._meshGroup._name);
				}
			}

			if (isRemovable)
			{
				//이후 모든 Child는 다 Remove한다.
				AddChildRenderUnitsToRemove(parentMeshGroup, curRenderUnit, removableRenderUnits);
			}
			else
			{
				for (int i = 0; i < curRenderUnit._childRenderUnits.Count; i++)
				{
					CheckRemovableRenderUnit(parentMeshGroup, curRenderUnit._childRenderUnits[i], removableRenderUnits);
				}
			}
		}

		private void AddChildRenderUnitsToRemove(apMeshGroup parentMeshGroup, apRenderUnit curRenderUnit, List<apRenderUnit> removableRenderUnits)
		{
			if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode)
			{
				if (curRenderUnit._meshGroup != null)
				{
					if (curRenderUnit._meshGroup._parentMeshGroup == parentMeshGroup)
					{
						Debug.LogError("Removable Unit : " + curRenderUnit._meshGroup._name + " In " + parentMeshGroup._name);
						curRenderUnit._meshGroup._parentMeshGroup = null;
						curRenderUnit._meshGroup._parentMeshGroupID = -1;
					}
				}
			}
			removableRenderUnits.Add(curRenderUnit);

			for (int i = 0; i < curRenderUnit._childRenderUnits.Count; i++)
			{
				AddChildRenderUnitsToRemove(parentMeshGroup, curRenderUnit._childRenderUnits[i], removableRenderUnits);
			}
		}

		/// <summary>
		/// Mesh의 Vertex가 바뀌면 이 함수를 호출한다.
		/// 모든 Render Unit들의 Vertex Buffer를 다시 리셋하게 만든다.
		/// </summary>
		public void ResetAllRenderUnitsVertexIndex()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			for (int iMG = 0; iMG < Editor._portrait._meshGroups.Count; iMG++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[iMG];
				for (int iRU = 0; iRU < meshGroup._renderUnits_All.Count; iRU++)
				{
					apRenderUnit renderUnit = meshGroup._renderUnits_All[iRU];
					renderUnit.ResetVertexIndex();
				}
			}
		}

		//------------------------------------------------------------------------------------------
		// 본
		//------------------------------------------------------------------------------------------

		/// <summary>
		/// 본을 생성하여 TargetMeshGroup에 추가한다.
		/// 만약 루트 본이 아닌 경우 : ParentBone에 값을 넣어주면 자동으로 Child에 포함된다.
		/// null을 넣으면 루트 본으로 설정되어 MeshGroup에서 따로 관리하도록 한다.
		/// 그외 설정은 리턴값을 받아서 처리하자
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="parentBone"></param>
		/// <returns></returns>
		public apBone AddBone(apMeshGroup targetMeshGroup, apBone parentBone)
		{
			if (Editor._portrait == null || targetMeshGroup == null)
			{
				return null;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_AddBone, targetMeshGroup, null, false, Editor);

			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Bone);
			int meshGroupID = targetMeshGroup._uniqueID;

			if (nextID < 0 || meshGroupID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Adding Bone is Failed. Please Retry", "Close");

				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.BoneAddFailed_Title),
												Editor.GetText(apLocalization.TEXT.BoneAddFailed_Body),
												Editor.GetText(apLocalization.TEXT.Close));
				return null;
			}



			//이름을 지어주자
			string parentName = "Bone ";
			if (parentBone != null)
			{
				parentName = parentBone._name;
			}
			apEditorUtil.NameAndIndexPair nameIndexPair = apEditorUtil.ParseNumericName(parentName);

			int curIndex = nameIndexPair._index;
			//이제 index를 올려가면서 겹치는게 있는지 확인한다.

			curIndex++;
			string resultName = "";
			int nCnt = 0;
			while (true)
			{
				resultName = nameIndexPair.MakeNewName(curIndex);

				bool isAnySameName = false;
				if (parentBone == null)
				{
					for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
					{
						if (string.Equals(targetMeshGroup._boneList_Root[i]._name, resultName))
						{
							//같은 이름이 있다.
							isAnySameName = true;
							break;
						}
					}
				}
				else
				{
					for (int i = 0; i < parentBone._childBones.Count; i++)
					{
						if (string.Equals(parentBone._childBones[i]._name, resultName))
						{
							//같은 이름이 있다.
							isAnySameName = true;
							break;
						}
					}
				}
				if (isAnySameName)
				{
					//똑같은 이름이 있네염..
					curIndex++;

					nCnt++;
					if (nCnt > 100)
					{
						Debug.Log("Error");
						break;
					}
				}
				else
				{
					//다른 이름 찾음
					break;
				}
			}

			#region [미사용 코드]
			//string baseName = "Bone ";
			//int nameNumber = 0;

			//string resultName = baseName + nameNumber;
			//if(parentBone == null)
			//{
			//	//동일한 이름이 Root에 있는지 체크하자
			//	resultName = baseName + nameNumber;

			//	if (targetMeshGroup._boneList_Root.Count > 0)
			//	{
			//		while (true)
			//		{
			//			bool isAnySameName = false;
			//			for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			//			{
			//				if(string.Equals(targetMeshGroup._boneList_Root[i]._name, resultName))
			//				{
			//					//같은 이름이 있다.
			//					isAnySameName = true;
			//					break;
			//				}
			//			}
			//			if(isAnySameName)
			//			{
			//				//똑같은 이름이 있네염..
			//				nameNumber++;
			//				resultName = baseName + nameNumber;
			//			}
			//			else
			//			{
			//				//다른 이름 찾음
			//				break;
			//			}
			//		}
			//	}

			//}
			//else
			//{
			//	baseName = parentBone._name + "";
			//	resultName = baseName + nameNumber;

			//	//동일한 이름이 Child에 있는지 체크하자
			//	if(parentBone._childBones.Count > 0)
			//	{
			//		while (true)
			//		{
			//			bool isAnySameName = false;
			//			for (int i = 0; i < parentBone._childBones.Count; i++)
			//			{
			//				if(string.Equals(parentBone._childBones[i]._name, resultName))
			//				{
			//					//같은 이름이 있다.
			//					isAnySameName = true;
			//					break;
			//				}
			//			}

			//			if(isAnySameName)
			//			{
			//				//똑같은 이름이 있네염..
			//				nameNumber++;
			//				resultName = baseName + nameNumber;
			//			}
			//			else
			//			{
			//				//다른 이름 찾음
			//				break;
			//			}
			//		}

			//	}
			//} 
			#endregion

			apBone newBone = new apBone(nextID, meshGroupID, resultName);

			if (parentBone != null)
			{
				//색상이 일정하도록 만든다.
				Color boneGUIColor = parentBone._color;
				boneGUIColor.r *= 0.9f;
				boneGUIColor.g *= 0.9f;
				boneGUIColor.b *= 0.9f;
				newBone._color = boneGUIColor;
			}
			//ParentBone을 포함해서 Link를 한다.
			//ParentBone이 있다면 이 Bone이 ChildList로 자동으로 추가된다.
			newBone.Link(targetMeshGroup, parentBone);
			newBone.InitTransform();

			targetMeshGroup._boneList_All.Add(newBone);//<<새로운 Bone을 추가하자

			if (newBone._parentBone == null)
			{
				//Root Bone이라면
				targetMeshGroup._boneList_Root.Add(newBone);//Root List에도 추가한다.
			}

			newBone.Link(targetMeshGroup, parentBone);


			return newBone;
		}


		public void RemoveAllBones(apMeshGroup targetMeshGroup)
		{
			if (targetMeshGroup == null)
			{
				return;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_RemoveAllBones, targetMeshGroup, null, false, Editor);

			//일단 ID 반납
			int nBones = targetMeshGroup._boneList_All.Count;
			for (int i = 0; i < nBones; i++)
			{
				Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, targetMeshGroup._boneList_All[i]._uniqueID);
			}

			targetMeshGroup._boneList_All.Clear();
			targetMeshGroup._boneList_Root.Clear();

			Editor.Select.SetBone(null);

			Editor._portrait.LinkAndRefreshInEditor();
			Editor.RefreshControllerAndHierarchy();

			Editor.Notification("All Bones of [" + targetMeshGroup._name + "] are removed", true, false);
		}


		public void RemoveBone(apBone bone, bool isRemoveChildren)
		{
			if (bone == null)
			{
				return;
			}

			//Debug.Log("Remove Bone : " + bone._name + " / Remove Children : " + isRemoveChildren);

			apMeshGroup meshGroup = bone._meshGroup;
			apBone parentBone = bone._parentBone;

			List<string> removedNames = new List<string>();


			if (!isRemoveChildren)
			{
				//< Children을 삭제하지 않을때 >
				//1. Parent에서 Bone을 삭제한다.
				//2. Bone의 Child를 Parent (또는 Null)에 연결한다.

				//Parent - [삭제할 Bone] - Child에서
				//Child를 삭제하지 않는다면
				//Parent <- Child로 연결한다.

				//3.연결할 때, 각각의 Child의 Matrix를 갱신한다.

				//4. MeshGroup에서 Bone을 삭제하고 Selection에서 해제한다.
				//5. Refresh

				removedNames.Add(bone._name);

				//1)
				if (parentBone != null)
				{
					parentBone._childBones.Remove(bone);
					parentBone._childBoneIDs.Remove(bone._uniqueID);
				}

				//2, 3)
				for (int i = 0; i < bone._childBones.Count; i++)
				{
					apBone childBone = bone._childBones[i];

					apMatrix nextParent_Matrix = null;
					if (parentBone != null)
					{
						childBone._parentBone = parentBone;
						childBone._parentBoneID = parentBone._uniqueID;

						if (!parentBone._childBones.Contains(childBone))
						{
							parentBone._childBones.Add(childBone);
						}
						if (!parentBone._childBoneIDs.Contains(childBone._uniqueID))
						{
							parentBone._childBoneIDs.Add(childBone._uniqueID);
						}

						nextParent_Matrix = parentBone._worldMatrix;
					}
					else
					{
						childBone._parentBone = null;
						childBone._parentBoneID = -1;

						if (bone._renderUnit != null)
						{
							nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
						}
						else
						{
							nextParent_Matrix = new apMatrix();
						}
					}

					//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
					apMatrix localBoneMatrix = apMatrix.RMultiply(childBone._defaultMatrix, childBone._localMatrix);

					//현재의 worldMatrix
					apMatrix worldBoneMatrix = childBone._worldMatrix;

					//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
					//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

					// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
					// Default
					apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix, nextParent_Matrix), childBone._localMatrix);
					childBone._defaultMatrix.SetMatrix(newDefaultMatrix);
				}


				//IK Option은 바꾸지 않는다.

				//4. MeshGroup에서 Bone을 삭제하고 Selection에서 해제한다.
				//혹시 모를 연동을 위해 에러를 발생하도록 하자
				Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, bone._uniqueID);

				bone._parentBone = null;
				bone._parentBoneID = -1;
				bone._meshGroup = null;
				bone._meshGroupID = -1;
				bone._childBones.Clear();
				bone._childBoneIDs.Clear();

				meshGroup._boneList_All.Remove(bone);
				meshGroup._boneList_Root.Remove(bone);

			}
			else
			{
				//< 모든 Children을 삭제한다. >
				//Parent에서 bone 연결 끊고 삭제하면 되므로 간단.
				if (parentBone != null)
				{
					parentBone._childBones.Remove(bone);
					parentBone._childBoneIDs.Remove(bone._uniqueID);
				}

				//재귀적으로 삭제를 해주자
				RemoveBoneWithChildrenRecursive(bone, meshGroup, removedNames);
			}

			Editor.Select.SetBone(null);

			Editor._portrait.LinkAndRefreshInEditor();

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);


			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.Hierarchy_AnimClip.ResetSubUnits();
			Editor.RefreshControllerAndHierarchy();

			if (removedNames.Count == 1)
			{
				Editor.Notification("[" + removedNames[0] + "] is removed", true, false);
			}
			else
			{
				string strRemoved = "";
				int nNames = removedNames.Count;
				if (nNames > 3)
				{
					nNames = 3;
				}
				for (int i = 0; i < nNames; i++)
				{
					if (i != 0)
					{
						strRemoved += ", ";
					}
					strRemoved += removedNames[i];

				}
				if (removedNames.Count > 3)
				{
					strRemoved += "...";
				}

				Editor.Notification("[" + strRemoved + "] are removed", true, false);
			}
		}

		private void RemoveBoneWithChildrenRecursive(apBone bone, apMeshGroup meshGroup, List<string> removedNames)
		{
			for (int i = 0; i < bone._childBones.Count; i++)
			{
				RemoveBoneWithChildrenRecursive(bone._childBones[i], meshGroup, removedNames);
			}

			Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, bone._uniqueID);
			meshGroup._boneList_All.Remove(bone);
			meshGroup._boneList_Root.Remove(bone);

			bone._parentBone = null;
			bone._parentBoneID = -1;
			bone._meshGroup = null;
			bone._meshGroupID = -1;
			bone._childBones.Clear();
			bone._childBoneIDs.Clear();

			removedNames.Add(bone._name);
		}

		public void AttachBoneToChild(apBone bone, apBone attachedBone)
		{
			if (bone == null || attachedBone == null)
			{
				return;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_AttachBoneToChild, bone._meshGroup, bone, false, Editor);

			if (bone.GetParentRecursive(attachedBone._uniqueID) != null)
			{
				//Parent가 오면 Loop가 발생한다.
				return;
			}
			Debug.Log("Attach Bone To Child : " + attachedBone._name + " > " + bone._name);



			//Child로 추가한다.
			//Child의 Parent를 연결한다.
			//기존의 Child의 Parent에서는 Child를 제외한다.
			//IK가 Disabled -> Single로 가능하면 Single로 만들어준다.
			//Child의 Default Matrix를 보정해준다.


			if (!bone._childBones.Contains(attachedBone))
			{
				bone._childBones.Add(attachedBone);
			}
			if (!bone._childBoneIDs.Contains(attachedBone._uniqueID))
			{
				bone._childBoneIDs.Add(attachedBone._uniqueID);
			}

			attachedBone._parentBone = bone;
			attachedBone._parentBoneID = bone._uniqueID;

			apBone prevParentBoneOffAttachedBone = attachedBone._parentBone;
			if (prevParentBoneOffAttachedBone != null)
			{
				prevParentBoneOffAttachedBone._childBones.Remove(attachedBone);
				prevParentBoneOffAttachedBone._childBoneIDs.Remove(attachedBone._uniqueID);
			}

			if (bone._childBones.Count == 1 && bone._optionIK == apBone.OPTION_IK.Disabled)
			{
				bone._optionIK = apBone.OPTION_IK.IKSingle;
				bone._IKTargetBone = attachedBone;
				bone._IKTargetBoneID = attachedBone._uniqueID;

				bone._IKNextChainedBone = attachedBone;
				bone._IKNextChainedBoneID = attachedBone._uniqueID;
			}


			//Attached Bone의 Default Matrix를 갱신하자

			//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
			apMatrix localBoneMatrix = apMatrix.RMultiply(attachedBone._defaultMatrix, attachedBone._localMatrix);

			//현재의 worldMatrix
			apMatrix worldBoneMatrix = attachedBone._worldMatrix;

			//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
			//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

			apMatrix nextParent_Matrix = null;
			if (bone._renderUnit != null)
			{
				nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
			}
			else
			{
				nextParent_Matrix = new apMatrix();
			}


			// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
			// Default
			apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix, nextParent_Matrix), attachedBone._localMatrix);
			attachedBone._defaultMatrix.SetMatrix(newDefaultMatrix);

			bone._meshGroup.RefreshForce();
			Editor._portrait.LinkAndRefreshInEditor();

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);


			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.RefreshControllerAndHierarchy();
		}

		public void DetachBoneFromChild(apBone bone, apBone detachedBone)
		{
			if (bone == null || detachedBone == null)
			{
				return;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_DetachBoneFromChild, bone._meshGroup, bone, false, Editor);

			//Child를 제거한다.
			//Child는 Parent가 없어졌으므로 Root가 된다.
			//Default Matrix 보존해줄 것


			bone._childBones.Remove(detachedBone);
			bone._childBoneIDs.Remove(detachedBone._uniqueID);

			detachedBone._parentBone = null;
			detachedBone._parentBoneID = -1;


			//Detached Bone의 Default Matrix를 갱신하자

			//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
			apMatrix localBoneMatrix = apMatrix.RMultiply(detachedBone._defaultMatrix, detachedBone._localMatrix);

			//현재의 worldMatrix
			apMatrix worldBoneMatrix = detachedBone._worldMatrix;

			//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
			//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

			apMatrix nextParent_Matrix = null;
			if (bone._renderUnit != null)
			{
				nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
			}
			else
			{
				nextParent_Matrix = new apMatrix();
			}


			// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
			// Default
			apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix, nextParent_Matrix), detachedBone._localMatrix);
			detachedBone._defaultMatrix.SetMatrix(newDefaultMatrix);

			bone._meshGroup.RefreshForce();
			Editor._portrait.LinkAndRefreshInEditor();

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);


			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.RefreshControllerAndHierarchy();
		}


		public void SetBoneAsParent(apBone bone, apBone parentBone)
		{
			if (bone == null)
			{
				return;
			}

			if (parentBone == bone._parentBone)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_SetBoneAsParent, bone._meshGroup, bone, false, Editor);

			//Parent Bone은 Null이 될 수 있다. (Root가 되는 경우)


			//1. 기존의 Parent에서 지금 Bone을 Child에서 뺀다.
			//2. 새로운 Parent에서 지금 Bone을 추가한다.
			//3. 새로운 Parent를 지금 Bone의 Parent로 등록한다.
			//4. Refresh
			//중요 > WorldMatrix를 보존해야한다.
			apMatrix prevParent_Matrix = null;
			apMatrix nextParent_Matrix = null;


			apBone prevParent = bone._parentBone;
			if (prevParent != null)
			{
				prevParent._childBones.Remove(bone);
				prevParent._childBoneIDs.Remove(bone._uniqueID);

				prevParent_Matrix = prevParent._worldMatrix;
			}
			else
			{
				if (bone._renderUnit != null)
				{
					prevParent_Matrix = bone._renderUnit.WorldMatrixWrap;
				}
				else
				{
					prevParent_Matrix = new apMatrix();
				}
			}

			if (parentBone != null)
			{
				parentBone._childBones.Add(bone);
				parentBone._childBoneIDs.Add(bone._uniqueID);

				bone._parentBone = parentBone;
				bone._parentBoneID = parentBone._uniqueID;

				if (parentBone._optionIK == apBone.OPTION_IK.Disabled && parentBone._childBones.Count == 1)
				{
					//처음 들어간거라면 자동으로 IK를 설정해주자.
					parentBone._optionIK = apBone.OPTION_IK.IKSingle;
					parentBone._IKTargetBone = parentBone._childBones[0];
					parentBone._IKTargetBoneID = parentBone._childBones[0]._uniqueID;

					parentBone._IKNextChainedBone = parentBone._childBones[0];
					parentBone._IKNextChainedBoneID = parentBone._childBones[0]._uniqueID;
				}

				nextParent_Matrix = parentBone._worldMatrix;
			}
			else
			{
				bone._parentBone = null;
				bone._parentBoneID = -1;

				if (bone._renderUnit != null)
				{
					nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
				}
				else
				{
					nextParent_Matrix = new apMatrix();
				}
			}

			//Default Matrix를 갱신하자

			//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
			apMatrix localBoneMatrix = apMatrix.RMultiply(bone._defaultMatrix, bone._localMatrix);

			//현재의 worldMatrix
			apMatrix worldBoneMatrix = bone._worldMatrix;

			//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
			//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

			// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
			// Default
			apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix, nextParent_Matrix), bone._localMatrix);
			bone._defaultMatrix.SetMatrix(newDefaultMatrix);

			bone._meshGroup.RefreshForce();
			Editor._portrait.LinkAndRefreshInEditor();

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);


			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.RefreshControllerAndHierarchy();
		}


		public void SetBoneAsIKTarget(apBone bone, apBone IKTargetBone)
		{
			if (bone == null || IKTargetBone == null)
			{
				return;
			}

			if (bone.GetChildBoneRecursive(IKTargetBone._uniqueID) == null)
			{
				return;
			}

			apBone nextChainedBone = bone.FindNextChainedBone(IKTargetBone._uniqueID);
			if (nextChainedBone == null)
			{
				return;
			}

			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_SetBoneAsIKTarget, bone._meshGroup, bone, false, Editor);

			string prevIKTargetBoneName = "<None>";
			if (bone._IKTargetBone != null)
			{
				prevIKTargetBoneName = bone._IKTargetBone._name;
			}

			//Debug.Log("Set Bone As IK Target : [" + prevIKTargetBoneName + " >> " + IKTargetBone._name + "] (" + bone._name + ")");



			bone._IKTargetBone = IKTargetBone;
			bone._IKTargetBoneID = IKTargetBone._uniqueID;

			bone._IKNextChainedBone = nextChainedBone;
			bone._IKNextChainedBoneID = nextChainedBone._uniqueID;


			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);

			Editor.RefreshControllerAndHierarchy();

			string curIKTargetBoneName = "<None>";
			if (bone._IKTargetBone != null)
			{
				curIKTargetBoneName = bone._IKTargetBone._name;
			}

			//Debug.Log("Set Bone As IK Target : Refresh Finined [ Cur IK Target : " + curIKTargetBoneName + " / Request IK Target : " + IKTargetBone._name + "]");
		}


		/// <summary>
		/// 해당 MeshGroup의 본들의 계층 연결 관계를 다시 갱신한다.
		/// IK Chaining은 호출되지 않으므로 별도로 (RefreshBoneChaining)를 호출하자
		/// 이 함수는 Link 이후에 호출해주자
		/// </summary>
		/// <param name="meshGroup"></param>
		public void RefreshBoneHierarchy(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			for (int i = 0; i < meshGroup._boneList_Root.Count; i++)
			{
				//Root 부터 재귀적으로 호출한다.
				RefreshBoneHierarchyUnit(meshGroup, meshGroup._boneList_Root[i], null);
			}

			if (meshGroup._childMeshGroupTransformsWithBones.Count > 0)
			{
				for (int i = 0; i < meshGroup._childMeshGroupTransformsWithBones.Count; i++)
				{
					apTransform_MeshGroup meshGroupTransform = meshGroup._childMeshGroupTransformsWithBones[i];
					if (meshGroupTransform._meshGroup != null)
					{
						RefreshBoneHierarchy(meshGroupTransform._meshGroup);
					}
				}
			}
		}


		private void RefreshBoneHierarchyUnit(apMeshGroup meshGroup, apBone bone, apBone parentBone)
		{
			if (bone == null)
			{
				return;
			}
			if (parentBone == null)
			{
				bone._parentBone = null;
				bone._parentBoneID = -1;
			}
			else
			{
				bone._parentBone = parentBone;
				bone._parentBoneID = parentBone._uniqueID;
			}

			bone._childBoneIDs.Clear();//ID 리스트는 일단 Clear
			bone._childBones.RemoveAll(delegate (apBone a)
			{
				return a == null;
			});

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				apBone childBone = bone._childBones[i];
				//ID 연결해주고..
				bone._childBoneIDs.Add(childBone._uniqueID);

				//계층적으로 호출하며 이어가자
				RefreshBoneHierarchyUnit(meshGroup, childBone, bone);

			}
		}


		/// <summary>
		/// 해당 본의 IK를 포함한 Chain 갱신을 한다.
		/// 기본 Link이후에 수행해야한다.
		/// IK 설정을 변경하면 한번씩 호출하자
		/// 초기화 이후에도 호출
		/// </summary>
		/// <param name="meshGroup"></param>
		public void RefreshBoneChaining(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			for (int i = 0; i < meshGroup._boneList_Root.Count; i++)
			{
				//Root 부터 재귀적으로 호출한다.
				RefreshBoneChainingUnit(meshGroup, meshGroup._boneList_Root[i]);
			}

			//추가
			//내부적으로도 BoneChaining을 다시 연결해주자
			for (int i = 0; i < meshGroup._boneList_Root.Count; i++)
			{
				meshGroup._boneList_Root[i].LinkBoneChaining();
			}

			if (meshGroup._childMeshGroupTransformsWithBones.Count > 0)
			{
				for (int i = 0; i < meshGroup._childMeshGroupTransformsWithBones.Count; i++)
				{
					apTransform_MeshGroup meshGroupTransform = meshGroup._childMeshGroupTransformsWithBones[i];
					if (meshGroupTransform._meshGroup != null)
					{
						RefreshBoneChaining(meshGroupTransform._meshGroup);
					}
				}
			}
		}

		private void RefreshBoneChainingUnit(apMeshGroup meshGroup, apBone bone)
		{
			if (bone == null)
			{
				return;
			}

			//1. Parent의 값에 따라서 IK Tail / IK Chained 처리를 갱신한다.
			bool isLocalMovable = false;
			if (bone._parentBone != null)
			{
				//Parent의 IK 옵션에 따라서 Tail 처리를 한다.
				switch (bone._parentBone._optionIK)
				{
					case apBone.OPTION_IK.Disabled:
						//Parent의 IK가 꺼져있다.
						//Chained라면 해제해준다.
						if (bone._optionIK == apBone.OPTION_IK.IKChained)
						{
							bone._optionIK = apBone.OPTION_IK.IKSingle;
							bone._IKTargetBone = null;
							bone._IKTargetBoneID = -1;

							bone._IKNextChainedBone = null;
							bone._IKNextChainedBoneID = -1;
						}

						bone._isIKTail = false;
						bone._IKHeaderBone = null;
						bone._IKHeaderBoneID = -1;
						isLocalMovable = true;
						break;

					case apBone.OPTION_IK.IKHead:
					case apBone.OPTION_IK.IKSingle:
					case apBone.OPTION_IK.IKChained:
						{
							//1) Parent가 자신을 타겟으로 삼고 있다면 Tail 처리
							//2) Parent가 자신의 자식을 타겟으로 삼고 있다면 Chained + Tail 처리
							//3) Parent가 자신 또는 자신의 자식을 타겟으로 삼고있지 않다면 IK 타겟이 아니다.
							int IKTargetBoneID = bone._parentBone._IKTargetBoneID;
							apBone IKTargetBone = bone._parentBone._IKTargetBone;
							int IKNextChainedBoneID = bone._parentBone._IKNextChainedBoneID;

							if (bone._uniqueID == IKTargetBoneID)
							{
								//1) 자신을 타겟으로 삼고 있는 경우
								//자신이 Chained였다면 이를 풀어줘야 한다.
								if (bone._optionIK == apBone.OPTION_IK.IKChained)
								{
									bone._optionIK = apBone.OPTION_IK.IKSingle;
									bone._IKTargetBone = null;
									bone._IKTargetBoneID = -1;

									bone._IKNextChainedBone = null;
									bone._IKNextChainedBoneID = -1;
								}

								bone._isIKTail = true;

								//bone._IKHeaderBone = bone._parentBone;
								//bone._IKHeaderBoneID = bone._parentBone._uniqueID;
								if (bone._parentBone._optionIK == apBone.OPTION_IK.IKHead
									|| bone._parentBone._optionIK == apBone.OPTION_IK.IKSingle)
								{
									bone._IKHeaderBone = bone._parentBone;
									bone._IKHeaderBoneID = bone._parentBone._uniqueID;
								}
								else
								{
									bone._IKHeaderBone = bone._parentBone._IKHeaderBone;
									bone._IKHeaderBoneID = bone._parentBone._IKHeaderBoneID;
								}
							}
							else if (bone._uniqueID == IKNextChainedBoneID)
							{
								//2) Parent가 자신의 자식을 타겟으로 삼고 있다면 Chained + Tail 처리
								bone._optionIK = apBone.OPTION_IK.IKChained;
								bone._isIKTail = true;

								//Parent가 Header로 삼고있는 Bone을 Header로 연결하여 공유한다.
								if (bone._parentBone._optionIK == apBone.OPTION_IK.IKHead
									|| bone._parentBone._optionIK == apBone.OPTION_IK.IKSingle)
								{
									bone._IKHeaderBone = bone._parentBone;
									bone._IKHeaderBoneID = bone._parentBone._uniqueID;
								}
								else
								{
									bone._IKHeaderBone = bone._parentBone._IKHeaderBone;
									bone._IKHeaderBoneID = bone._parentBone._IKHeaderBoneID;
								}


								if (bone._IKHeaderBone == null)
								{
									Debug.LogError("Bone Chaining Error : Header를 찾을 수 없다. [" + bone._parentBone._name + " -> " + bone._name + "]");
								}
								else
								{
									//Debug.Log("Chained : " + bone._IKHeaderBone._name + " >> " + bone._parentBone._name + " >> " + bone._name);
								}


								bone._IKNextChainedBone = bone.FindNextChainedBone(IKTargetBoneID);
								if (bone._IKNextChainedBone == null)
								{
									bone._IKNextChainedBoneID = -1;
									Debug.LogError("Bone Chaining Error : IK Chained가 이어지지 않았다. [" + bone._parentBone._name + " -> " + bone._name + " -> (끊김) -> " + IKTargetBone._name);
								}
								else
								{
									bone._IKNextChainedBoneID = bone._IKNextChainedBone._uniqueID;
								}

								//타겟을 공유한다.
								bone._IKTargetBone = IKTargetBone;
								bone._IKTargetBoneID = IKTargetBoneID;


							}
							else
							{
								//3) Parent가 자신 또는 자신의 자식을 타겟으로 삼고있지 않다면 IK 타겟이 아니다.
								//IK Chain이 끊겼다.

								if (bone._optionIK == apBone.OPTION_IK.IKChained)
								{
									bone._optionIK = apBone.OPTION_IK.IKSingle;
									bone._IKTargetBone = null;
									bone._IKTargetBoneID = -1;

									bone._IKNextChainedBone = null;
									bone._IKNextChainedBoneID = -1;
								}

								bone._isIKTail = false;
								bone._IKHeaderBone = null;
								bone._IKHeaderBoneID = -1;
								isLocalMovable = true;
							}
						}
						break;
				}
			}
			else
			{
				bone._isIKTail = false;
				bone._IKHeaderBone = null;
				bone._IKHeaderBoneID = -1;

				isLocalMovable = true;
			}


			//2. Child로의 IK 설정에 따라서 이어지는 Chain 처리를 한다.

			switch (bone._optionIK)
			{
				case apBone.OPTION_IK.Disabled:
					{
						//IK가 꺼져있으니 값을 날리자
						bone._IKTargetBoneID = -1;
						bone._IKTargetBone = null;

						bone._IKNextChainedBoneID = -1;
						bone._IKNextChainedBone = null;
					}
					break;

				case apBone.OPTION_IK.IKChained:
					//Chain 처리는 위의 Parent 처리에서 연동해서 이미 수행했다.
					break;

				case apBone.OPTION_IK.IKHead:
					{
						int targetIKBoneID = bone._IKTargetBoneID;
						int nextChainedBoneID = bone._IKNextChainedBoneID;

						//갱신 작업이 필요한지 체크
						bool isRefreshNeed = false;
						if (bone._IKTargetBone == null || bone._IKNextChainedBone == null)
						{
							//ID는 있는데 연결이 안되었네요
							//다시 연결 필요
							isRefreshNeed = true;
						}
						else
						{
							//검색속도가 빠른 -> Parent로의 함수를 이용하여 유효한 링크인지 판단한다.
							if (bone._IKTargetBone.GetParentRecursive(bone._uniqueID) == null
								|| bone._IKNextChainedBone.GetParentRecursive(bone._uniqueID) == null)
							{
								isRefreshNeed = true;
							}
						}

						if (isRefreshNeed)
						{
							//Target을 기준으로 ID와 레퍼런스 연동을 하자
							apBone targetBone = bone.GetChildBoneRecursive(targetIKBoneID);
							apBone nextChainedBone = bone.FindNextChainedBone(nextChainedBoneID);

							if (targetBone == null || nextChainedBone == null)
							{
								//못찾았네요...
								Debug.LogError("Bone Chaining Error : IK Header가 적절한 타겟을 찾지 못했다. [" + bone._name + "] > IK 해제됨");

								//에러로 인해 초기화 할때는
								//Child Bone이 1개라면 Single로 초기화
								//Child Bone이 여러개라면 Disabled
								if (bone._childBones.Count == 1 && bone._childBones[0] != null)
								{
									//IKSingle로 초기화하자
									apBone childBone = bone._childBones[0];

									bone._IKTargetBoneID = childBone._uniqueID;
									bone._IKTargetBone = childBone;

									bone._IKNextChainedBoneID = childBone._uniqueID;
									bone._IKNextChainedBone = childBone;

									bone._optionIK = apBone.OPTION_IK.IKSingle;
								}
								else
								{
									//Disabled로 초기화하자
									bone._IKTargetBoneID = -1;
									bone._IKTargetBone = null;

									bone._IKNextChainedBoneID = -1;
									bone._IKNextChainedBone = null;

									bone._optionIK = apBone.OPTION_IK.Disabled;
								}
							}
							else
							{
								//타겟이 있다. 마저 연결하자
								bone._IKTargetBoneID = targetIKBoneID;
								bone._IKTargetBone = targetBone;

								bone._IKNextChainedBoneID = nextChainedBoneID;
								bone._IKNextChainedBone = nextChainedBone;
							}
						}
					}
					break;

				case apBone.OPTION_IK.IKSingle:
					{
						//연결이 유효하면 -> 지속
						//연결이 유효하지 않으면 무조건 Disabled로 바꾼다.
						//자동 연결은 하지 말자

						int targetIKBoneID = bone._IKTargetBoneID;
						int nextChainedBoneID = bone._IKNextChainedBoneID;

						//갱신 작업이 필요한지 체크
						bool isRefreshNeed = false;
						if (bone._IKTargetBone == null || bone._IKNextChainedBone == null)
						{
							//ID는 있는데 연결이 안되었네요
							//다시 연결 필요
							isRefreshNeed = true;
						}
						else
						{
							//Parent/Child 연결 관계가 유효한가
							if (bone._IKTargetBone._parentBone != bone
								|| bone._IKNextChainedBone._parentBone != bone)
							{
								//직접 연결이 안되어있다.
								isRefreshNeed = true;
							}
						}

						if (isRefreshNeed)
						{
							apBone targetBone = bone.GetChildBone(targetIKBoneID);
							apBone nextChainedBone = bone.GetChildBone(targetIKBoneID);

							bool isInvalid = false;
							if (targetBone == null || nextChainedBone == null || targetBone != nextChainedBone)
							{
								isInvalid = true;
							}

							if (!isInvalid)
							{
								//유효한 연결
								bone._IKTargetBoneID = targetIKBoneID;
								bone._IKTargetBone = targetBone;

								bone._IKNextChainedBoneID = nextChainedBoneID;
								bone._IKNextChainedBone = nextChainedBone;
							}
							else
							{
								//유효하지 않은 연결
								//Disabled로 바꾸자
								bone._IKTargetBoneID = -1;
								bone._IKTargetBone = null;

								bone._IKNextChainedBoneID = -1;
								bone._IKNextChainedBone = null;

								bone._optionIK = apBone.OPTION_IK.Disabled;
							}

						}
					}
					break;
			}

			//3. IK 값에 따라서 Local Move 처리를 확인한다.
			if (!isLocalMovable)
			{
				if (bone._optionLocalMove == apBone.OPTION_LOCAL_MOVE.Enabled)
				{
					//IK Tail로 세팅된 상태라면 LocalMove는 불가능하다
					bone._optionLocalMove = apBone.OPTION_LOCAL_MOVE.Disabled;
				}
			}

			//4. Child도 Bone 체크를 하자
			for (int i = 0; i < bone._childBones.Count; i++)
			{
				RefreshBoneChainingUnit(meshGroup, bone._childBones[i]);
			}

		}



		/// <summary>
		/// 현재 선택한 ModMesh의 ModVertRig의 Weight 리스트에 "현재 선택한 Bone"을 선택한 Weight와 함께 추가한다.
		/// 만약 선택한 ModVertRig(1개 이상)가 없고 Bone을 선택하지 않았다면 패스한다.
		/// Bone이 등록되지 않았다면 자동으로 등록하며, AutoNormalize가 되어있다면 같이 수행한다.
		/// </summary>
		/// <param name="calculateType">연산 타입. 0 : 대입, 1 : 더하기, 2 : 곱하기</param>
		/// <param name="weight"></param>
		public void SetBoneWeight(float weight, int calculateType)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.Bone == null
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//Undo - 연속 입력 가능
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor.Select.MeshGroup, null, true, Editor);

			bool isAutoNormalize = Editor.Select._rigEdit_isAutoNormalize;

			apModifiedVertexRig vertRig = null;
			apModifiedVertexRig.WeightPair targetWeightPair = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];



				//Bone이 있는가?
				targetWeightPair = null;
				for (int iPair = 0; iPair < vertRig._weightPairs.Count; iPair++)
				{
					if (vertRig._weightPairs[iPair]._bone == bone)
					{
						targetWeightPair = vertRig._weightPairs[iPair];
						break;
					}
				}
				//없으면 추가
				if (targetWeightPair == null)
				{
					targetWeightPair = new apModifiedVertexRig.WeightPair(bone);
					vertRig._weightPairs.Add(targetWeightPair);
				}
				switch (calculateType)
				{
					case 0://대입
						targetWeightPair._weight = weight;
						break;

					case 1://더하기
						targetWeightPair._weight += weight;
						break;

					case 2://곱하기
						targetWeightPair._weight *= weight;
						break;
				}
				//if (isMultiply)
				//{
				//	targetWeightPair._weight *= weight;
				//}
				//else
				//{
				//	targetWeightPair._weight = weight;
				//}

				vertRig.CalculateTotalWeight();

				if (isAutoNormalize)
				{
					//Normalize를 하자
					vertRig.NormalizeExceptPair(targetWeightPair);
				}
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy();
		}


		/// <summary>
		/// Bone Weight를 Normalize한다.
		/// </summary>
		public void SetBoneWeightNormalize()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.Bone == null
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor.Select.MeshGroup, null, false, Editor);

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];

				vertRig.Normalize();
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy();
		}

		/// <summary>
		/// Bone Weight를 Prune한다.
		/// </summary>
		public void SetBoneWeightPrune()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.Bone == null
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor.Select.MeshGroup, false, false, Editor);

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];

				vertRig.Prune();
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy();
		}



		/// <summary>
		/// Bone Weight를 Blend한다.
		/// Blend시 주의점
		/// Blend는 내부의 Weight가 아니라 "주변의 Weight를 비교"하여 Blend한다.
		/// Mesh를 참조하여 "연결된 Vertex"를 가지는 "VertRigs"를 모두 검색한 뒤,
		/// "주변 Weight의 평균값" 10% + "자신의 Weight" + 90%를 적용한다.
		/// 연산 결과가 요청된 다른 Vertex에 영향을 주지 않도록 결과를 따로 저장했다가 일괄 적용한다.
		/// </summary>
		public void SetBoneWeightBlend()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				//|| Editor.Select.Bone == null<<여기선 Bone이 없어도 됩니다.
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor.Select.MeshGroup, null, false, Editor);

			//요청된 VertRig와 연결된 외부의 Weight 평균값
			Dictionary<apModifiedVertexRig, List<apModifiedVertexRig.WeightPair>> linkedWeightAvgs = new Dictionary<apModifiedVertexRig, List<apModifiedVertexRig.WeightPair>>();

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];

				//연결된 Vertex
				List<apVertex> linkedVerts = vertRig._mesh.GetLinkedVertex(vertRig._vertex, true);

				if (linkedVerts == null)
				{
					continue;
				}

				List<apModifiedVertexRig> linkedVertRigs = modMesh._vertRigs.FindAll(delegate (apModifiedVertexRig a)
				{
					return a != vertRig && linkedVerts.Contains(a._vertex);
				});

				if (linkedVertRigs.Count == 0)
				{
					continue;
				}

				//평균값을 내자
				//Bone+Weight 조합으로 다 더한 뒤
				//연결된 VertRig 개수만큼 나누자
				List<apModifiedVertexRig.WeightPair> weightPairList = new List<apModifiedVertexRig.WeightPair>();
				int nLinkedVertRigs = linkedVertRigs.Count;
				for (int iLink = 0; iLink < nLinkedVertRigs; iLink++)
				{
					apModifiedVertexRig linkedVertRig = linkedVertRigs[iLink];
					for (int iWP = 0; iWP < linkedVertRig._weightPairs.Count; iWP++)
					{
						apModifiedVertexRig.WeightPair weightPair = linkedVertRig._weightPairs[iWP];

						apModifiedVertexRig.WeightPair existPair = weightPairList.Find(delegate (apModifiedVertexRig.WeightPair a)
						{
							return a._bone == weightPair._bone;
						});

						if (existPair != null)
						{
							//이미 존재하는 Bone이다.
							//Weight만 추가하자.
							existPair._weight += weightPair._weight;
						}
						else
						{
							//등록되지 않은 Bone이다.
							//새로 추가하자
							apModifiedVertexRig.WeightPair newPair = new apModifiedVertexRig.WeightPair(weightPair._bone);
							newPair._weight = weightPair._weight;
							weightPairList.Add(newPair);
						}
					}
				}
				//평균값을 내자
				for (int iWP = 0; iWP < weightPairList.Count; iWP++)
				{
					weightPairList[iWP]._weight /= nLinkedVertRigs;
				}

				//연산 결과에 등록[요청 VertRig + 주변의 Rig 데이터]
				linkedWeightAvgs.Add(vertRig, weightPairList);
			}

			//값을 넣어주자
			//비율은 20% + 80%
			float ratio_Src = 0.8f;
			float ratio_Link = 0.2f;
			foreach (KeyValuePair<apModifiedVertexRig, List<apModifiedVertexRig.WeightPair>> rigPair in linkedWeightAvgs)
			{
				apModifiedVertexRig targetVertRig = rigPair.Key;
				List<apModifiedVertexRig.WeightPair> linkedWeightPairs = rigPair.Value;

				//1) Bone이 없다면 추가해준다.
				//2) targetVertRig 기준으로 : 90% 10% 비율로 계산
				for (int i = 0; i < linkedWeightPairs.Count; i++)
				{
					apModifiedVertexRig.WeightPair linkedPair = linkedWeightPairs[i];

					if (!targetVertRig._weightPairs.Exists(delegate (apModifiedVertexRig.WeightPair a)
					 {
						 return a._bone == linkedPair._bone;
					 }))
					{
						//새로 추가해준다.
						apModifiedVertexRig.WeightPair newPair = new apModifiedVertexRig.WeightPair(linkedPair._bone);
						newPair._weight = 0.0f;
						targetVertRig._weightPairs.Add(newPair);
					}
				}

				for (int i = 0; i < targetVertRig._weightPairs.Count; i++)
				{
					apModifiedVertexRig.WeightPair targetWeight = targetVertRig._weightPairs[i];
					apModifiedVertexRig.WeightPair linkedWeight = linkedWeightPairs.Find(delegate (apModifiedVertexRig.WeightPair a)
					{
						return a._bone == targetWeight._bone;
					});

					if (linkedWeight != null)
					{
						targetWeight._weight = targetWeight._weight * ratio_Src + linkedWeight._weight * ratio_Link;
					}
					else
					{
						targetWeight._weight = targetWeight._weight * ratio_Src;
					}
				}
			}



			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy();
		}

		/// <summary>
		/// 선택한 Vertex Rig에 대해서 Grow 또는 Select 선택을 한다.
		/// </summary>
		/// <param name="isGrow"></param>
		public void SelectVertexRigGrowOrShrink(bool isGrow)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				//|| Editor.Select.Bone == null<<여기선 Bone이 없어도 됩니다.
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//apEditorUtil.SetRecord("Select Vertex Rig Grow Or Shrink", Editor._portrait);


			apModifiedVertexRig vertRig = null;
			if (isGrow)
			{
				//Grow인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져오고, 기존의 리스트에 없는 Vertex를 선택해준다.
				List<apModifiedVertexRig> addVertRigs = new List<apModifiedVertexRig>();
				for (int iVR = 0; iVR < vertRigs.Count; iVR++)
				{
					vertRig = vertRigs[iVR];

					List<apVertex> linkedVerts = vertRig._mesh.GetLinkedVertex(vertRig._vertex, false);
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertRigs.Exists(delegate (apModifiedVertexRig a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (isExist)
						{
							//하나라도 이미 선택된 거네요.
							//패스
							continue;
						}

						apVertex addVertex = linkedVerts[iVert];
						//하나도 선택되지 않은 Vertex라면
						//새로 추가해주자
						apModifiedVertexRig addVertRig = modMesh._vertRigs.Find(delegate (apModifiedVertexRig a)
						{
							return a._vertex == addVertex;
						});
						if (addVertRig != null)
						{
							if (!addVertRigs.Contains(addVertRig))
							{
								addVertRigs.Add(addVertRig);
							}
						}
					}
				}
				//일괄적으로 추가해주자
				for (int i = 0; i < addVertRigs.Count; i++)
				{
					Editor.Select.AddModVertexOfModifier(null, addVertRigs[i], null, addVertRigs[i]._renderVertex);
				}
			}
			else
			{
				//Shirink인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져온다.
				//1) Linked Vert가 없으면 : 삭제 리스트에 넣는다.
				//2) Linked Vert 중에서 "지금 선택 중인 Vert"에 해당하지 않는 Vert가 하나라도 있으면 : 삭제 리스트에 넣는다.
				//3) 모든 Linked Vert가 "지금 선택중인 Vert"에 해당된다면 유지
				List<apModifiedVertexRig> removeVertRigs = new List<apModifiedVertexRig>();
				for (int iVR = 0; iVR < vertRigs.Count; iVR++)
				{
					vertRig = vertRigs[iVR];

					List<apVertex> linkedVerts = vertRig._mesh.GetLinkedVertex(vertRig._vertex, false);
					if (linkedVerts == null || linkedVerts.Count == 0)
					{
						//1) 연결된게 없으면 삭제
						removeVertRigs.Add(vertRig);
						continue;
					}

					//모든 Vertex가 현재 선택된 상태인지 확인하자
					bool isAllSelected = true;
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertRigs.Exists(delegate (apModifiedVertexRig a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (!isExist)
						{
							//선택되지 않은 Vertex를 발견!
							isAllSelected = false;
							break;
						}
					}
					if (!isAllSelected)
					{
						//2) 하나라도 선택되지 않은 Link Vertex가 발견되었다면 이건 삭제 대상이다.
						if (!removeVertRigs.Contains(vertRig))
						{
							removeVertRigs.Add(vertRig);
						}
					}
					else
					{
						//추가)
						//만약 외곽선에 위치한 Vertex라면 우선순위에 포함된다.
						bool isOutlineVertex = vertRig._mesh.IsOutlineVertex(vertRig._vertex);
						if (isOutlineVertex)
						{
							if (!removeVertRigs.Contains(vertRig))
							{
								removeVertRigs.Add(vertRig);
							}
						}
					}
				}

				if (removeVertRigs.Count > 0)
				{
					//하나씩 삭제하자

					for (int i = 0; i < removeVertRigs.Count; i++)
					{
						vertRig = removeVertRigs[i];
						Editor.Select.RemoveModVertexOfModifier(null, vertRig, null, vertRig._renderVertex);
					}
				}

			}

		}


		public void SetBoneAutoRig()
		{
			//선택한 Vertex에 대해서 자동으로 Rigging을 해주자
			//VertRig에 등록한 Vertex에 대해서만 수행하자.
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				//|| Editor.Select.Bone == null<<여기선 Bone이 없어도 됩니다.
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor.Select.MeshGroup, null, false, Editor);


			apModifiedVertexRig vertRig = null;

			float bias_zero = 0.0001f;
			for (int iVR = 0; iVR < vertRigs.Count; iVR++)
			{
				vertRig = vertRigs[iVR];

				if (vertRig._renderVertex == null)
				{
					continue;
				}

				Vector2 posW = vertRig._renderVertex._pos_World;
				//이제 영역 비교 및 거리 역으로 Weight를 계산
				//값을 넣고 나중에 Normalize하자
				for (int iW = 0; iW < vertRig._weightPairs.Count; iW++)
				{
					apBone bone = vertRig._weightPairs[iW]._bone;

					Vector2 bonePos_Start = bone._worldMatrix._pos;
					Vector2 bonePos_End = bone._shapePoint_End;

					Vector2 bonePos_Mid1 = bone._shapePoint_Mid1;
					Vector2 bonePos_Mid2 = bone._shapePoint_Mid2;

					float distLimit_Min = Vector2.Distance(bonePos_Mid1, bonePos_Mid2) * 0.25f;
					float distLimit_Max = distLimit_Min * 8.0f;

					if (distLimit_Max == 0.0f)
					{
						vertRig._weightPairs[iW]._weight = bias_zero;
					}
					else
					{
						//Bone 선분과 Vertex사이의 거리가 Min 이내로 들어오면 1의 값을 가진다.
						//Min-Max 사이에 들어오면 
						//Min일때 1, Max일때 0의 Weight 값(선형)을 가진다.
						//Max 밖일때는 0
						//0대신 최소값을 넣자.
						//Normalize를 수행한 후, 너무 작은 값을 0으로 만든 뒤에 다시 Normalize 반복

						float distToVert = apEditorUtil.DistanceFromLine(bonePos_Start, bonePos_End, posW);
						float weight = 0.0f;
						if (distToVert < distLimit_Min)
						{
							weight = 1.0f;
						}
						else if (distToVert < distLimit_Max)
						{
							float itp = distToVert - distLimit_Min;
							float length = distLimit_Max - distLimit_Min;


							weight = (1.0f * (length - itp) + bias_zero * itp) / length;
						}
						else
						{
							weight = bias_zero;
						}
						vertRig._weightPairs[iW]._weight = weight;
					}
				}

				vertRig.Normalize();

				//너무 작은 Weight를 0으로 만들자
				for (int iW = 0; iW < vertRig._weightPairs.Count; iW++)
				{
					if (vertRig._weightPairs[iW]._weight < 0.01f)
					{
						vertRig._weightPairs[iW]._weight = 0.0f;
					}
				}

				//다시 Normalize
				vertRig.Normalize();
			}

			Editor.Select.MeshGroup.RefreshForce();
		}

		public void RemoveVertRigData(List<apSelection.ModRenderVert> selectedVerts, apBone targetBone)
		{
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_RemoveBoneWeight, Editor.Select.MeshGroup, null, false, Editor);

			for (int iVert = 0; iVert < selectedVerts.Count; iVert++)
			{
				apSelection.ModRenderVert modRenderVert = selectedVerts[iVert];
				if (modRenderVert._modVertRig != null)
				{
					modRenderVert._modVertRig._weightPairs.RemoveAll(delegate (apModifiedVertexRig.WeightPair a)
					{
						return a._bone == targetBone;
					});

					modRenderVert._modVertRig.CalculateTotalWeight();
				}
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy();
		}

		//Rigging과 유사하게 Physic/Volume Weight도 처리하자
		/// <summary>
		/// Physic / Volume Modifier의 Vertex Weight를 지정한다.
		/// </summary>
		/// <param name="weight"></param>
		/// <param name="calculateType">연산 타입. 0 : 대입, 1 : 더하기, 2 : 곱하기</param>
		public void SetPhyVolWeight(float weight, int calculateType)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				//Debug.LogError("Failed..");
				return;
			}

			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			//Undo - 연속 입력 가능
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SetPhysicsWeight, Editor.Select.MeshGroup, null, true, Editor);


			for (int i = 0; i < vertWeights.Count; i++)
			{
				float curWeight = vertWeights[i]._weight;
				switch (calculateType)
				{
					case 0://대입
						curWeight = weight;
						break;

					case 1://더하기
						curWeight += weight;
						break;

					case 2://곱하기
						curWeight *= weight;
						break;
				}
				curWeight = Mathf.Clamp01(curWeight);
				vertWeights[i]._weight = curWeight;
			}

			//Weight Refresh
			Editor.Select.ModMeshOfMod.RefreshVertexWeights(Editor._portrait, Editor.Select.Modifier.IsPhysics, Editor.Select.Modifier.IsVolume);

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy();
		}

		public void SetPhyVolWeightBlend()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				|| Editor.Select.SubMeshInGroup == null
				|| Editor.Select.ModMeshOfMod == null)
			{
				Debug.LogError("Failed..");
				return;
			}

			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;

			apMesh targetMesh = Editor.Select.SubMeshInGroup._mesh;
			if (targetMesh == null)
			{
				Debug.LogError("Mesh is Null");
				return;
			}



			//평균값 로직은 문제가 많다.
			//모든 VertWeight를 대상으로
			//해당 Mesh에서 연결된 1Level Vert를 일일이 검색한뒤,
			//검색된 Vert의 ModVertWeight를 구하고,
			//그 ModVertWeight의 Weight의 평균을 구해서 Dictionary로 상태로 저장한다.

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			//Undo - 연속 입력 가능
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SetPhysicsWeight, Editor.Select.MeshGroup, null, true, Editor);


			Dictionary<apModifiedVertexWeight, float> avgWeights = new Dictionary<apModifiedVertexWeight, float>();

			for (int iModVert = 0; iModVert < vertWeights.Count; iModVert++)
			{
				apModifiedVertexWeight modVertWeight = vertWeights[iModVert];



				float totalWeight = 0.0f;
				int nWeight = 0;

				//자기 자신도 추가
				totalWeight += modVertWeight._weight;
				nWeight++;

				List<apVertex> linkedVert = targetMesh.GetLinkedVertex(modVertWeight._vertex, true);
				for (int iLV = 0; iLV < linkedVert.Count; iLV++)
				{
					apModifiedVertexWeight linkedModVertWeight = modMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
					{
						return a._vertex == linkedVert[iLV];
					});
					if (linkedModVertWeight != null && linkedModVertWeight != modVertWeight)
					{
						totalWeight += linkedModVertWeight._weight;
						nWeight++;
					}
					else
					{
						Debug.LogError("Link Vert에 해당하는 ModVert를 찾을 수 없다.");
					}
				}
				if (nWeight > 0)
				{
					totalWeight /= nWeight;
					avgWeights.Add(modVertWeight, totalWeight);
				}
			}
			//계산된 평균값을 넣어주자
			float ratio_Src = 0.8f;
			float ratio_Avg = 0.2f;
			foreach (KeyValuePair<apModifiedVertexWeight, float> vertWeightPair in avgWeights)
			{
				vertWeightPair.Key._weight = vertWeightPair.Key._weight * ratio_Src + vertWeightPair.Value * ratio_Avg;
			}


			//평균값을 두고, 기존 80%, 평균 20%로 넣어주자
			//float avgWeight = 0.0f;
			//for (int i = 0; i < vertWeights.Count; i++)
			//{
			//	avgWeight += vertWeights[i]._weight;
			//}
			//avgWeight /= vertWeights.Count;


			//for (int i = 0; i < vertWeights.Count; i++)
			//{
			//	vertWeights[i]._weight = (vertWeights[i]._weight * ratio_Src) + (avgWeight * ratio_Avg);
			//}

			//Weight Refresh
			Editor.Select.ModMeshOfMod.RefreshVertexWeights(Editor._portrait, Editor.Select.Modifier.IsPhysics, Editor.Select.Modifier.IsVolume);

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy();
		}

		public void SelectVertexWeightGrowOrShrink(bool isGrow)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				|| Editor.Select.ModMeshOfMod == null)
			{
				//Debug.LogError("Failed..");
				return;
			}

			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			apModifiedVertexWeight vertWeight = null;
			if (isGrow)
			{
				//Grow인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져오고, 기존의 리스트에 없는 Vertex를 선택해준다.
				List<apModifiedVertexWeight> addVertWeights = new List<apModifiedVertexWeight>();
				for (int iVW = 0; iVW < vertWeights.Count; iVW++)
				{
					vertWeight = vertWeights[iVW];

					List<apVertex> linkedVerts = vertWeight._mesh.GetLinkedVertex(vertWeight._vertex, false);
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertWeights.Exists(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (isExist)
						{
							//하나라도 이미 선택된 거네요.
							//패스
							continue;
						}

						apVertex addVertex = linkedVerts[iVert];
						//하나도 선택되지 않은 Vertex라면
						//새로 추가해주자
						apModifiedVertexWeight addVertWeight = modMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == addVertex;
						});
						if (addVertWeight != null)
						{
							if (!addVertWeights.Contains(addVertWeight))
							{
								addVertWeights.Add(addVertWeight);
							}
						}
					}
				}
				//일괄적으로 추가해주자
				for (int i = 0; i < addVertWeights.Count; i++)
				{
					Editor.Select.AddModVertexOfModifier(null, null, addVertWeights[i], addVertWeights[i]._renderVertex);
				}
			}
			else
			{
				//Shirink인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져온다.
				//1) Linked Vert가 없으면 : 삭제 리스트에 넣는다.
				//2) Linked Vert 중에서 "지금 선택 중인 Vert"에 해당하지 않는 Vert가 하나라도 있으면 : 삭제 리스트에 넣는다.
				//3) 모든 Linked Vert가 "지금 선택중인 Vert"에 해당된다면 유지
				List<apModifiedVertexWeight> removeVertWeights = new List<apModifiedVertexWeight>();
				for (int iVW = 0; iVW < vertWeights.Count; iVW++)
				{
					vertWeight = vertWeights[iVW];

					List<apVertex> linkedVerts = vertWeight._mesh.GetLinkedVertex(vertWeight._vertex, false);
					if (linkedVerts == null || linkedVerts.Count == 0)
					{
						//1) 연결된게 없으면 삭제
						removeVertWeights.Add(vertWeight);
						continue;
					}

					//모든 Vertex가 현재 선택된 상태인지 확인하자
					bool isAllSelected = true;
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertWeights.Exists(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (!isExist)
						{
							//선택되지 않은 Vertex를 발견!
							isAllSelected = false;
							break;
						}
					}
					if (!isAllSelected)
					{
						//2) 하나라도 선택되지 않은 Link Vertex가 발견되었다면 이건 삭제 대상이다.
						if (!removeVertWeights.Contains(vertWeight))
						{
							removeVertWeights.Add(vertWeight);
						}
					}
					else
					{
						//추가)
						//만약 외곽선에 위치한 Vertex라면 우선순위에 포함된다.
						bool isOutlineVertex = vertWeight._mesh.IsOutlineVertex(vertWeight._vertex);
						if (isOutlineVertex)
						{
							if (!removeVertWeights.Contains(vertWeight))
							{
								removeVertWeights.Add(vertWeight);
							}
						}
					}
				}

				if (removeVertWeights.Count > 0)
				{
					//하나씩 삭제하자

					for (int i = 0; i < removeVertWeights.Count; i++)
					{
						vertWeight = removeVertWeights[i];
						Editor.Select.RemoveModVertexOfModifier(null, null, vertWeight, vertWeight._renderVertex);
					}
				}

			}
		}

		public void SetPhysicsViscostyGroupID(int iViscosityID, bool isViscosityAdd)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				|| Editor.Select.ModMeshOfMod == null)
			{
				//Debug.LogError("Failed..");
				return;
			}

			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SetPhysicsProperty, Editor.Select.MeshGroup, null, true, Editor);

			apModifiedVertexWeight vertWeight = null;
			for (int i = 0; i < vertWeights.Count; i++)
			{
				vertWeight = vertWeights[i];
				if (iViscosityID == 0)
				{
					//0으로 초기화
					vertWeight._physicParam._viscosityGroupID = 0;
				}
				else
				{
					if (isViscosityAdd)
					{
						//추가한다.
						vertWeight._physicParam._viscosityGroupID |= iViscosityID;
					}
					else
					{
						//삭제한다.
						vertWeight._physicParam._viscosityGroupID &= ~iViscosityID;

					}
				}

			}
		}

		public void ResetPhysicsValues()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			for (int iMG = 0; iMG < Editor._portrait._meshGroups.Count; iMG++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[iMG];

				List<apModifierBase> modifiers = meshGroup._modifierStack._modifiers;
				for (int iMod = 0; iMod < modifiers.Count; iMod++)
				{
					apModifierBase mod = modifiers[iMod];

					if (!mod.IsPhysics)
					{
						continue;
					}

					for (int iPSG = 0; iPSG < mod._paramSetGroup_controller.Count; iPSG++)
					{
						apModifierParamSetGroup paramSetGroup = mod._paramSetGroup_controller[iPSG];

						for (int iPS = 0; iPS < paramSetGroup._paramSetList.Count; iPS++)
						{
							apModifierParamSet paramSet = paramSetGroup._paramSetList[iPS];

							for (int iModMesh = 0; iModMesh < paramSet._meshData.Count; iModMesh++)
							{
								apModifiedMesh modMesh = paramSet._meshData[iModMesh];

								List<apModifiedVertexWeight> vertWeights = modMesh._vertWeights;
								for (int iVW = 0; iVW < vertWeights.Count; iVW++)
								{
									apModifiedVertexWeight vertWeight = vertWeights[iVW];
									if (vertWeight == null)
									{
										continue;
									}

									vertWeight._calculatedDeltaPos = Vector2.zero;
									vertWeight.DampPhysicVertex();
								}
							}
						}
					}

				}
			}
		}


		//--------------------------------------------------
		// 4. 메시 작업
		//--------------------------------------------------
		public void StartMeshEdgeWork()
		{
			if (Editor.Select.Mesh == null)
			{
				return;
			}

			Editor.Select.Mesh.StartNewEdgeWork();

		}

		public void CheckMeshEdgeWorkRemained()
		{
			Editor.VertController.StopEdgeWire();
			if (Editor.Select.Mesh == null)
			{
				return;
			}

			if (Editor.Select.Mesh.IsEdgeWorking() && Editor.Select.Mesh.IsAnyWorkedEdge())
			{
				//Undo
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_MakeEdges, Editor._portrait, Editor.Select.Mesh, false, Editor);

				//bool isResult = EditorUtility.DisplayDialog("Confirm Edges", "Edge working is not completed.\nDo you want to complete it?", "Make Edges", "Remove");
				bool isResult = EditorUtility.DisplayDialog(_editor.GetText(apLocalization.TEXT.MeshEditChanged_Title),
																_editor.GetText(apLocalization.TEXT.MeshEditChanged_Body),
																_editor.GetText(apLocalization.TEXT.MeshEditChanged_Okay),
																_editor.GetText(apLocalization.TEXT.Cancel)
																);
				if (isResult)
				{
					//Editor._selection.Mesh.MakeEdgesToIndexBuffer();
					Editor.Select.Mesh.MakeEdgesToPolygonAndIndexBuffer();
					Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
				}
				else
				{
					//Editor._selection.Mesh.CancelEdgeWork();
				}
			}
		}

		/// <summary>
		/// [Deprecated]
		/// </summary>
		/// <param name="volumeValue"></param>
		public void PaintVolumeValue(float volumeValue)
		{
			//이 코드는 사용하지 않습니다.
			//if(Editor.Select.Mesh == null)
			//{
			//	return;
			//}
			//apEditorUtil.SetRecord("Paint Volume Value", Editor._portrait);

			//List<apVertex> vertices = Editor.Select.Mesh._vertexData;
			//for (int i = 0; i < vertices.Count; i++)
			//{
			//	vertices[i]._volumeWeight = volumeValue / 100.0f;
			//}

			////Editor.Repaint();
			//Editor.SetRepaint();
		}

		public float GetBrushValue(float dist, float brushRadius, float value, float hardness)
		{
			if (dist > brushRadius)
			{
				return 0.0f;
			}
			if (dist < 0.0f)
			{
				dist = 0.0f;
			}

			value *= 0.01f;
			hardness *= 0.01f;

			hardness = Mathf.Clamp01(hardness);

			float softValue = 1.0f * (brushRadius - dist) / brushRadius;

			float resultValue = 1.0f * (hardness) + softValue * (1.0f - hardness);
			return resultValue * value;

		}

		public int GetNearestBrushSizeIndex(float brushSize)
		{
			int iNearest = -1;
			float minDiff = float.MaxValue;

			for (int i = 0; i < Editor._brushPreset_Size.Length; i++)
			{
				float diff = Mathf.Abs(Editor._brushPreset_Size[i] - brushSize);
				if (iNearest < 0 || diff < minDiff)
				{
					minDiff = diff;
					iNearest = i;
				}
			}
			return iNearest;
		}

		public float GetNextBrushSize(float brushSize, bool isIncrease)
		{
			int iNearest = GetNearestBrushSizeIndex(brushSize);
			if (isIncrease)
			{
				iNearest++;
			}
			else
			{
				iNearest--;
			}

			if (iNearest >= Editor._brushPreset_Size.Length)
			{
				iNearest = Editor._brushPreset_Size.Length - 1;
			}
			else if (iNearest < 0)
			{
				iNearest = 0;
			}
			return Editor._brushPreset_Size[iNearest];
		}

		public void RemoveSelectedMeshPolygon()
		{
			if (Editor._portrait == null)
			{
				return;
			}
			if (Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh ||
				Editor._meshEditeMode_MakeMesh != apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
			{
				return;
			}
			if (Editor.Select.Mesh == null || Editor.VertController.Polygon == null)
			{
				return;
			}

			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_EditPolygons, Editor._portrait, Editor.Select.Mesh, false, Editor);
			Editor.Select.Mesh._polygons.Remove(Editor.VertController.Polygon);
			Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
			Editor.VertController.UnselectVertex();

			Editor.Controller.ResetAllRenderUnitsVertexIndex();
		}

		//--------------------------------------------------
		// 5. 메시 그룹 작업
		//--------------------------------------------------
		public apTransform_Mesh AddMeshToMeshGroup(apMesh addedMesh)
		{
			if (Editor == null)
			{
				//EditorUtility.DisplayDialog("Error", "Adding Mesh is failed", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.MeshAttachFailed_Title),
												Editor.GetText(apLocalization.TEXT.MeshAttachFailed_Body),
												Editor.GetText(apLocalization.TEXT.Close));
				return null;
			}
			return AddMeshToMeshGroup(addedMesh, Editor.Select.MeshGroup);
		}
		public apTransform_Mesh AddMeshToMeshGroup(apMesh addedMesh, apMeshGroup targetMeshGroup)
		{
			if (Editor == null || targetMeshGroup == null || addedMesh == null)
			{
				return null;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_AttachMesh, targetMeshGroup, addedMesh, false, Editor);


			int nSameMesh = targetMeshGroup._childMeshTransforms.FindAll(delegate (apTransform_Mesh a)
			{
				return a._meshUniqueID == addedMesh._uniqueID;
			}).Count;

			string newNickName = addedMesh._name;
			if (nSameMesh > 0)
			{
				newNickName += " (" + (nSameMesh + 1) + ")";
			}

			//int nextID = Editor._portrait.MakeUniqueID_Transform();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);

			if (nextID < 0)
			{
				return null;
			}
			apTransform_Mesh newMeshTransform = new apTransform_Mesh(nextID);

			newMeshTransform._meshUniqueID = addedMesh._uniqueID;
			newMeshTransform._nickName = newNickName;
			newMeshTransform._mesh = addedMesh;
			newMeshTransform._matrix = new apMatrix();
			newMeshTransform._isVisible_Default = true;

			//Depth는 가장 큰 값으로 들어간다.
			int maxDepth = targetMeshGroup.GetLastDepth();
			newMeshTransform._depth = maxDepth + 1;


			targetMeshGroup._childMeshTransforms.Add(newMeshTransform);
			targetMeshGroup.SetDirtyToReset();
			//targetMeshGroup.SetAllRenderUnitForceUpdate();
			targetMeshGroup.RefreshForce();

			Editor.RefreshControllerAndHierarchy();
			//Editor.Repaint();
			Editor.SetRepaint();

			return newMeshTransform;
		}

		public apTransform_MeshGroup AddMeshGroupToMeshGroup(apMeshGroup addedMeshGroup)
		{
			if (Editor == null)
			{
				return null;
			}
			return AddMeshGroupToMeshGroup(addedMeshGroup, Editor.Select.MeshGroup);
		}

		public apTransform_MeshGroup AddMeshGroupToMeshGroup(apMeshGroup addedMeshGroup, apMeshGroup targetMeshGroup)
		{
			if (Editor == null || targetMeshGroup == null || addedMeshGroup == null)
			{
				return null;
			}


			bool isExist = targetMeshGroup._childMeshGroupTransforms.Exists(delegate (apTransform_MeshGroup a)
			{
				return a._meshGroupUniqueID == addedMeshGroup._uniqueID;
			});

			if (isExist)
			{
				return null;
			}

			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_AttachMeshGroup, targetMeshGroup, addedMeshGroup, false, Editor);

			string newNickName = addedMeshGroup._name;

			//int nextID = Editor._portrait.MakeUniqueID_Transform();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);
			if (nextID < 0)
			{
				return null;
			}

			apTransform_MeshGroup newMeshGroupTransform = new apTransform_MeshGroup(nextID);

			newMeshGroupTransform._meshGroupUniqueID = addedMeshGroup._uniqueID;
			newMeshGroupTransform._nickName = newNickName;
			newMeshGroupTransform._meshGroup = addedMeshGroup;
			newMeshGroupTransform._matrix = new apMatrix();
			newMeshGroupTransform._isVisible_Default = true;

			//Depth는 가장 큰 값으로 들어간다.
			int maxDepth = targetMeshGroup.GetLastDepth();
			newMeshGroupTransform._depth = maxDepth + 1;

			targetMeshGroup._childMeshGroupTransforms.Add(newMeshGroupTransform);

			newMeshGroupTransform._meshGroup._parentMeshGroup = targetMeshGroup;
			newMeshGroupTransform._meshGroup._parentMeshGroupID = targetMeshGroup._uniqueID;

			targetMeshGroup.SetDirtyToReset();
			//targetMeshGroup.SetAllRenderUnitForceUpdate();
			targetMeshGroup.RefreshForce();

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy();
			//Editor.Repaint();
			Editor.SetRepaint();

			return newMeshGroupTransform;
		}




		public bool AddClippingMeshTransform(apMeshGroup meshGroup, apTransform_Mesh meshTransform, bool isShowErrorDialog)
		{
			if (meshGroup == null || meshTransform == null)
			{
				return false;
			}

			//if(meshTransform._isClipping_Parent)
			//{
			//	//자기 자신이 이미 Parent라면 취소
			//	if (isShowErrorDialog)
			//	{
			//		EditorUtility.DisplayDialog("Mask Mesh", "Mask Mesh cannot be Clipped Mesh", "Close");
			//	}
			//	return false;
			//}
			//Parent도 Clip을 지정한 뒤 -> Refresh만 잘 하면 된다.

			if (meshGroup.GetMeshTransform(meshTransform._transformUniqueID) == null)
			{
				//해당 메시 그룹에 존재하지 않는 트랜스폼이다.
				return false;
			}
			//이미 Clip 상태이면 패스
			//if(meshTransform._isClipping_Child || meshTransform._isClipping_Parent)
			//{
			//	return false;
			//}

			meshTransform._isClipping_Child = true;
			meshGroup.SetDirtyToSort();
			meshGroup.SortRenderUnits(true);
			#region [미사용 코드]

			////자신보다 Depth가 "낮은" MeshTransform 중에서의 "최대값"을 가진 MeshTransform을 찾는다.
			//apTransform_Mesh baseMT = null;
			//float maxDepth = float.MinValue;
			//for (int i = 0; i < meshGroup._childMeshTransforms.Count; i++)
			//{
			//	apTransform_Mesh mt = meshGroup._childMeshTransforms[i];
			//	if(mt == meshTransform)
			//	{
			//		continue;
			//	}
			//	if(mt._isClipping_Child)
			//	{
			//		//이미 Child라면 Pass
			//		continue;
			//	}
			//	if(mt._depth < meshTransform._depth)
			//	{
			//		if(baseMT == null || maxDepth < mt._depth)
			//		{
			//			baseMT = mt;
			//			maxDepth = mt._depth;
			//		}
			//	}
			//}

			//if(baseMT == null)
			//{
			//	//마땅한 MeshTransform을 찾지 못했다.
			//	EditorUtility.DisplayDialog("No Mask Mesh", "No Proper Mask Mesh", "Close");
			//	return;
			//}

			//if(baseMT._isClipping_Parent)
			//{
			//	//이미 Parent일때
			//	int nChildMeshes = baseMT.GetChildClippedMeshes();
			//	if(nChildMeshes >= 3)
			//	{
			//		EditorUtility.DisplayDialog("Too Many Meshes", "There are Too Many[3] Child Mesh to Target Mask Mesh", "Close");
			//		return;
			//	}
			//}
			//else
			//{
			//	baseMT._isClipping_Parent = true;
			//}

			//baseMT.AddClippedChildMesh(meshTransform);

			//meshGroup.SetDirtyToSort(); 
			#endregion
			meshGroup.RefreshForce();
			Editor.Hierarchy_MeshGroup.RefreshUnits();
			return true;
		}



		public void ReleaseClippingMeshTransform(apMeshGroup meshGroup, apTransform_Mesh meshTransform)
		{
			if (meshGroup == null || meshTransform == null)
			{
				return;
			}

			if (meshGroup.GetMeshTransform(meshTransform._transformUniqueID) == null)
			{
				//해당 메시 그룹에 존재하지 않는 트랜스폼이다.
				return;
			}

			meshTransform._isClipping_Child = false;
			meshTransform._clipIndexFromParent = -1;
			meshTransform._clipParentMeshTransform = null;

			meshGroup.SetDirtyToSort();
			meshGroup.SortRenderUnits(true);
			meshGroup.RefreshForce();
			Editor.Hierarchy_MeshGroup.RefreshUnits();
		}


		/// <summary>
		/// 작업시 TmpWorkVisible이 각 Transform에 저장되어있다. 이값을 초기화(true)한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		public void SetMeshGroupTmpWorkVisibleReset(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			for (int i = 0; i < meshGroup._renderUnits_All.Count; i++)
			{
				meshGroup._renderUnits_All[i].ResetTmpWorkVisible();
			}

			//if(meshGroup._rootMeshGroupTransform == null)
			//{
			//	return;
			//}


			//SetMeshGoupTransformTmpWorkVisibleReset(meshGroup._rootMeshGroupTransform);
			meshGroup.RefreshForce();
		}

		//private void SetMeshGoupTransformTmpWorkVisibleReset(apTransform_MeshGroup meshGroupTransform)
		//{
		//	if(meshGroupTransform == null)
		//	{
		//		return;
		//	}

		//	meshGroupTransform._isVisible_TmpWork = true;

		//	for (int i = 0; i < meshGroupTransform._meshGroup._childMeshTransforms.Count; i++)
		//	{
		//		meshGroupTransform._meshGroup._childMeshTransforms[i]._isVisible_TmpWork = true;
		//	}

		//	//ChildMeshGroup에게도 호출
		//	for (int i = 0; i < meshGroupTransform._meshGroup._childMeshGroupTransforms.Count; i++)
		//	{
		//		SetMeshGoupTransformTmpWorkVisibleReset(meshGroupTransform._meshGroup._childMeshGroupTransforms[i]);
		//	}
		//}

		//--------------------------------------------------
		// 메시 그룹의 Modifier 작업
		//--------------------------------------------------
		public void AddModifier(apModifierBase.MODIFIER_TYPE _type, int validationKey)
		{
			if (Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_AddModifier, Editor.Select.MeshGroup, null, false, Editor);

			apModifierStack modStack = Editor.Select.MeshGroup._modifierStack;
			int newID = modStack.GetNewModifierID((int)_type, validationKey);
			if (newID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Modifier Adding Failed. Please Retry", "Close");

				EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.ModifierAddFailed_Title),
												Editor.GetText(apLocalization.TEXT.ModifierAddFailed_Body),
												Editor.GetText(apLocalization.TEXT.Close));
				return;
			}



			int newLayer = modStack.GetLastLayer() + 1;
			apModifierBase newModifier = null;
			switch (_type)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					newModifier = new apModifierBase();//<<이건 처리하지 않습니다... 사실은;
					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					newModifier = new apModifier_Volume();
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					newModifier = new apModifier_Morph();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					newModifier = new apModifier_AnimatedMorph();
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					newModifier = new apModifier_Rigging();
					break;
				case apModifierBase.MODIFIER_TYPE.Physic:
					newModifier = new apModifier_Physic();
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					newModifier = new apModifier_TF();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					newModifier = new apModifier_AnimatedTF();
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					newModifier = new apModifier_FFD();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					newModifier = new apModifier_AnimatedFFD();
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입 [" + _type + "]");
					break;
			}
			newModifier.LinkPortrait(Editor._portrait);
			newModifier.SetInitSetting(newID, newLayer, Editor.Select.MeshGroup._uniqueID, Editor.Select.MeshGroup);
			modStack.AddModifier(newModifier, _type);

			modStack.RefreshAndSort(true);//<Sort!

			Editor.Select.SetModifier(newModifier);
			Editor.RefreshControllerAndHierarchy();
			Editor.SetRepaint();
		}


		/// <summary>
		/// 현재의 Modifier에 선택한 Transform을 ModMesh로 등록한다.
		/// </summary>
		public void AddModMesh_WithSubMeshOrSubMeshGroup()
		{
			if (Editor.Select.SubEditedParamSetGroup == null || Editor.Select.Modifier == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//apModifiedMesh addedModMesh = null;
			List<apModifierParamSet> modParamSetList = Editor.Select.SubEditedParamSetGroup._paramSetList;

			if (Editor.Select.SubMeshInGroup != null)
			{
				for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
				{
					//addedModMesh = Editor.Select.Modifier.AddMeshTransform(Editor.Select.MeshGroup, Editor.Select.SubMeshInGroup, modParamSetList[iParam]);
					Editor.Select.Modifier.AddMeshTransform(Editor.Select.MeshGroup, Editor.Select.SubMeshInGroup, modParamSetList[iParam], false, Editor.Select.Modifier.IsTarget_ChildMeshTransform);
				}
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
				{
					//addedModMesh = Editor.Select.Modifier.AddMeshGroupTransform(Editor.Select.MeshGroup, Editor.Select.SubMeshGroupInGroup, modParamSetList[iParam]);
					Editor.Select.Modifier.AddMeshGroupTransform(Editor.Select.MeshGroup, Editor.Select.SubMeshGroupInGroup, modParamSetList[iParam], false, true);
				}
			}

			bool isChanged = Editor.Select.SubEditedParamSetGroup.RefreshSync();
			if (isChanged)
			{
				Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink();
			}

			Editor.Select.AutoSelectModMeshOrModBone();
		}

		/// <summary>
		/// 현재의 Modifier와 선택한 Bone을 연동하여 ModBone을 생성하여 추가한다.
		/// </summary>
		public void AddModBone_WithSelectedBone()
		{
			if (Editor.Select.SubEditedParamSetGroup == null || Editor.Select.Modifier == null || Editor.Select.MeshGroup == null)
			{
				return;
			}
			if (Editor.Select.Bone == null)
			{
				return;
			}
			apBone bone = Editor.Select.Bone;

			List<apModifierParamSet> modParamSetList = Editor.Select.SubEditedParamSetGroup._paramSetList;

			for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
			{
				Editor.Select.Modifier.AddBone(bone, modParamSetList[iParam], false, true);
			}

			bool isChanged = Editor.Select.SubEditedParamSetGroup.RefreshSync();
			if (isChanged)
			{
				Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink();
			}

			//TODO : 원래는 AutoSelectModMesh가 들어와야 하지만 -> 여기서는 ModBone이 선택되어야 한다.
			//ModBone이 apSelection에 존재하도록 설정할 것
			//Editor.Select.AutoSelectModMesh();
			Editor.Select.AutoSelectModMeshOrModBone();
		}

		public void AddControlParamToModifier(apControlParam controlParam)
		{
			//이 ControlParam에 해당하는 ParamSetGroup이 있는지 체크한다.
			if (Editor.Select.Modifier == null)
			{
				Debug.LogError("AddControlParamToModifier -> No Modifier");
				return;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_LinkControlParam, Editor.Select.Modifier._meshGroup, controlParam, false, Editor);

			apModifierParamSetGroup paramSetGroup = Editor.Select.Modifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
			{
				return a._syncTarget == apModifierParamSetGroup.SYNC_TARGET.Controller &&
						a._keyControlParam == controlParam;
			});
			if (paramSetGroup == null)
			{
				//없다면 만들어주자
				paramSetGroup = new apModifierParamSetGroup(Editor._portrait, Editor.Select.Modifier, Editor.Select.Modifier.GetNextParamSetLayerIndex());
				paramSetGroup.SetController(controlParam);

				Editor.Select.Modifier._paramSetGroup_controller.Add(paramSetGroup);
				Editor.Select.SetParamSetGroupOfModifier(paramSetGroup);
			}



			//Key를 추가하자

			apModifierParamSet newParamSet = new apModifierParamSet();

			newParamSet.LinkParamSetGroup(paramSetGroup);//Link도 해준다.
														 //newParamSet.SetController(controlParam);
			switch (controlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	newParamSet._conSyncValue_Bool = controlParam._bool_Cur;
				//	break;

				case apControlParam.TYPE.Int:
					newParamSet._conSyncValue_Int = controlParam._int_Cur;
					break;

				case apControlParam.TYPE.Float:
					newParamSet._conSyncValue_Float = controlParam._float_Cur;
					break;

				case apControlParam.TYPE.Vector2:
					newParamSet._conSyncValue_Vector2 = controlParam._vec2_Cur;
					break;

					//case apControlParam.TYPE.Vector3:
					//	newParamSet._conSyncValue_Vector3 = controlParam._vec3_Cur;
					//	break;

					//case apControlParam.TYPE.Color:
					//	newParamSet._conSyncValue_Color = controlParam._color_Cur;
					//	break;
			}


			paramSetGroup._paramSetList.Add(newParamSet);
			paramSetGroup.RefreshSync();


			//추가
			//만약, 현재 선택중인 Mesh나 MeshGroup이 ModMesh로서 ParamSetList에 없다면 추가해주는 것도 좋을 것 같다.
			bool isAddedModMesh = false;
			bool isAnyTransformSelected = false;

			bool isAddedModBone = false;
			bool isAnyBoneSelected = false;
			if (Editor.Select.SubMeshInGroup != null)
			{
				isAnyTransformSelected = true;
				isAddedModMesh = paramSetGroup.IsSubMeshInGroup(Editor.Select.SubMeshInGroup);
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				isAnyTransformSelected = true;
				isAddedModMesh = paramSetGroup.IsSubMeshGroupInGroup(Editor.Select.SubMeshGroupInGroup);
			}
			else if (Editor.Select.Bone != null && Editor.Select.Modifier.IsTarget_Bone)
			{
				isAnyBoneSelected = true;
				isAddedModBone = paramSetGroup.IsBoneContain(Editor.Select.Bone);
			}

			if (!isAddedModMesh && isAnyTransformSelected)
			{
				//Debug.Log("Auto Make ModMesh");
				//자동으로 Start Edit 버튼 누른 것 같은 효과를 주자
				AddModMesh_WithSubMeshOrSubMeshGroup();//<<이 함수는 Start Edit에도 있다.
				paramSetGroup.RefreshSync();
			}
			else if (!isAddedModBone && isAnyBoneSelected)
			{
				AddModBone_WithSelectedBone();
				paramSetGroup.RefreshSync();
			}

			//Editor.Select.Modifier._paramSetList.Add(newParamSet);

			Editor.Select.SetParamSetGroupOfModifier(paramSetGroup);
			Editor.Select.SetParamSetOfModifier(newParamSet);

			Editor.Select.Modifier.RefreshParamSet();

			Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
			Editor.Select.MeshGroup.RefreshModifierLink();
			Editor.SetRepaint();
		}


		/// <summary>
		/// Static 타입 (아무런 입력 연동값이 없음)의 ParamSetGroup을 Modifier에 등록한다.
		/// Static 타입은 ParamSetGroup(연동 오브젝트) 1개와 ParamSet(키값) 1개만 가진다. (그 아래 ModifiedMesh를 여러개 가진다)
		/// </summary>
		public void AddStaticParamSetGroupToModifier()
		{
			//이 ControlParam에 해당하는 ParamSetGroup이 있는지 체크한다.
			if (Editor.Select.Modifier == null)
			{
				Debug.LogError("AddStaticParamSetGroupToModifier -> No Modifier");
				return;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_AddStaticParamSetGroup, Editor.Select.Modifier._meshGroup, null, false, Editor);

			if (Editor.Select.Modifier._paramSetGroup_controller.Count > 0)
			{
				//Static 타입은 한개의 ParamSetGroup만 적용한다.
				return;
			}

			apModifierParamSetGroup paramSetGroup = new apModifierParamSetGroup(Editor._portrait, Editor.Select.Modifier, Editor.Select.Modifier.GetNextParamSetLayerIndex());
			paramSetGroup.SetStatic();//<Static 타입

			Editor.Select.Modifier._paramSetGroup_controller.Add(paramSetGroup);
			Editor.Select.SetParamSetGroupOfModifier(paramSetGroup);



			//Static 타입은 한개의 ParamSet을 가진다.

			apModifierParamSet newParamSet = new apModifierParamSet();

			newParamSet.LinkParamSetGroup(paramSetGroup);//Link도 해준다.
			paramSetGroup._paramSetList.Add(newParamSet);
			paramSetGroup.RefreshSync();


			Editor.Select.SetParamSetGroupOfModifier(paramSetGroup);
			Editor.Select.SetParamSetOfModifier(newParamSet);

			Editor.Select.Modifier.RefreshParamSet();

			Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
			Editor.Select.MeshGroup.RefreshModifierLink();
			Editor.SetRepaint();
		}


		public void LayerChange(apModifierBase modifier, bool isLayerUp)
		{
			if (Editor._portrait == null || Editor.Select.MeshGroup == null)
			{ return; }

			apModifierStack modStack = Editor.Select.MeshGroup._modifierStack;

			if (!modStack._modifiers.Contains(modifier))
			{ return; }

			//Undo를 기록하자
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_LayerChanged, Editor.Select.MeshGroup, null, false, Editor);

			modStack.RefreshAndSort(false);

			int prevIndex = modStack._modifiers.IndexOf(modifier);
			int nextIndex = prevIndex;
			if (isLayerUp)
			{
				nextIndex++;

				if (nextIndex >= modStack._modifiers.Count)
				{ return; }
			}
			else
			{
				nextIndex--;

				if (nextIndex < 0)
				{ return; }
			}



			//순서를 바꿀 모디파이어다.
			apModifierBase swapMod = modStack._modifiers[nextIndex];

			//인덱스를 서로 바꾸자
			swapMod._layer = prevIndex;
			modifier._layer = nextIndex;

			modStack.RefreshAndSort(false);

			Editor.RefreshControllerAndHierarchy();
			Editor.SetRepaint();
		}



		public void RemoveModifier(apModifierBase modifier)
		{
			if (Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			apModifierStack modStack = Editor.Select.MeshGroup._modifierStack;

			if (!modStack._modifiers.Contains(modifier))
			{
				return;
			}
			//Undo
			apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshGroup_RemoveModifier, Editor.Select.MeshGroup, modifier, false, Editor);

			int modifierID = modifier._uniqueID;
			//Editor._portrait.PushUniqueID_Modifier(modifierID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Modifier, modifierID);

			modStack.RemoveModifier(modifier);
			modStack.RefreshAndSort(true);

			//다시 연결
			Editor.Select.MeshGroup.RefreshModifierLink();

			Editor.Select.SetModifier(null);
		}
		//----------------------------------------------------------------------------------
		// Bake
		//----------------------------------------------------------------------------------
		/// <summary>
		/// 현재 Portrait를 실행가능한 버전으로 Bake하자
		/// </summary>
		public void PortraitBake()
		{
			if (Editor._portrait == null)
			{
				return;
			}


			if (Editor._portrait._optRootUnitList != null)
			{
				for (int i = 0; i < Editor._portrait._optRootUnitList.Count; i++)
				{
					apOptRootUnit optRootUnit = Editor._portrait._optRootUnitList[i];
					if (optRootUnit != null && optRootUnit.gameObject != null)
					{
						GameObject.DestroyImmediate(optRootUnit.gameObject);
					}
				}
			}
			else
			{
				Editor._portrait._optRootUnitList = new List<apOptRootUnit>();
			}

			Editor._portrait._optRootUnitList.Clear();
			Editor._portrait._curPlayingOptRootUnit = null;
			//if(Editor._portrait._optRootUnit != null)
			//{
			//	GameObject.DestroyImmediate(Editor._portrait._optRootUnit.gameObject);
			//}

			if (Editor._portrait._optTransforms == null)
			{ Editor._portrait._optTransforms = new List<apOptTransform>(); }
			if (Editor._portrait._optMeshes == null)
			{ Editor._portrait._optMeshes = new List<apOptMesh>(); }
			if (Editor._portrait._optMaskedMeshes == null)
			{ Editor._portrait._optMaskedMeshes = new List<apOptMesh>(); }
			if (Editor._portrait._optClippedMeshes == null)
			{ Editor._portrait._optClippedMeshes = new List<apOptMesh>(); }


			Editor._portrait._optTransforms.Clear();
			Editor._portrait._optMeshes.Clear();
			Editor._portrait._optMaskedMeshes.Clear();
			Editor._portrait._optClippedMeshes.Clear();
			Editor._portrait._isAnyMaskedMeshes = false;

			for (int i = 0; i < Editor._portrait._rootUnits.Count; i++)
			{
				apRootUnit rootUnit = Editor._portrait._rootUnits[i];

				//업데이트를 한번 해주자
				rootUnit.Update(0.0f);

				apOptRootUnit optRootUnit = null;

				//1. Root Unit
				optRootUnit = AddGameObject<apOptRootUnit>("Root Portrait " + i, Editor._portrait.transform);

				optRootUnit._portrait = Editor._portrait;
				optRootUnit._rootOptTransform = null;
				optRootUnit._transform = optRootUnit.transform;

				Editor._portrait._optRootUnitList.Add(optRootUnit);



				apMeshGroup childMainMeshGroup = rootUnit._childMeshGroup;

				//0. 추가
				//일부 Modified Mesh를 갱신해야한다.
				if (childMainMeshGroup != null && rootUnit._childMeshGroupTransform != null)
				{
					//Refresh를 한번 해주자
					childMainMeshGroup.RefreshForce();

					List<apModifierBase> modifiers = childMainMeshGroup._modifierStack._modifiers;
					for (int iMod = 0; iMod < modifiers.Count; iMod++)
					{
						apModifierBase mod = modifiers[iMod];
						if (mod._paramSetGroup_controller != null)
						{
							for (int iPSG = 0; iPSG < mod._paramSetGroup_controller.Count; iPSG++)
							{
								apModifierParamSetGroup psg = mod._paramSetGroup_controller[iPSG];
								for (int iPS = 0; iPS < psg._paramSetList.Count; iPS++)
								{
									apModifierParamSet ps = psg._paramSetList[iPS];
									ps.UpdateBeforeBake(Editor._portrait, childMainMeshGroup, rootUnit._childMeshGroupTransform);
								}
							}
						}
					}
				}

				//1. 1차 Bake : GameObject 만들기
				//List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;
				if (childMainMeshGroup != null && rootUnit._childMeshGroupTransform != null)
				{
					//정렬 한번 해주고
					childMainMeshGroup.SortRenderUnits(true);

					apRenderUnit rootRenderUnit = childMainMeshGroup._rootRenderUnit;
					//apRenderUnit rootRenderUnit = Editor._portrait._rootUnit._renderUnit;
					if (rootRenderUnit != null)
					{
						//apTransform_MeshGroup meshGroupTransform = Editor._portrait._rootUnit._childMeshGroupTransform;
						apTransform_MeshGroup meshGroupTransform = rootRenderUnit._meshGroupTransform;

						if (meshGroupTransform == null)
						{
							Debug.LogError("Bake Error : MeshGroupTransform Not Found [" + childMainMeshGroup._name + "]");
						}
						else
						{
							MakeMeshGroupToOptTransform(rootRenderUnit, meshGroupTransform, optRootUnit.transform, null, optRootUnit);
							//MakeMeshGroupToOptTransform(null, meshGroupTransform, Editor._portrait._optRootUnit.transform, null);
						}
					}
					else
					{
						Debug.LogError("Bake Error : RootMeshGroup Not Found [" + childMainMeshGroup._name + "]");
					}

				}

				//optRootUnit.transform.localScale = Vector3.one * 0.01f;
				optRootUnit.transform.localScale = Vector3.one * Editor._portrait._bakeScale;
			}



			//1-2. Masked Mesh 연결해주기
			if (Editor._portrait._optMaskedMeshes.Count > 0 || Editor._portrait._optClippedMeshes.Count > 0)
			{
				Editor._portrait._isAnyMaskedMeshes = true;
			}

			for (int i = 0; i < Editor._portrait._optMeshes.Count; i++)
			{
				apOptMesh optMesh = Editor._portrait._optMeshes[i];
				if (optMesh._isMaskParent)
				{
					//Parent라면..
					//apOptMesh[] childMeshes = new apOptMesh[3];
					//for (int iChild = 0; iChild < 3; iChild++)
					//{
					//	childMeshes[iChild] = null;
					//	if(optMesh._clipChildIDs[iChild] >= 0)
					//	{
					//		apOptTransform optTransform = Editor._portrait.GetOptTransform(optMesh._clipChildIDs[iChild]);
					//		if(optTransform != null && optTransform._childMesh != null)
					//		{
					//			childMeshes[iChild] = optTransform._childMesh;
					//		}

					//	}
					//}
					//optMesh.LinkAsMaskParent(childMeshes);//<<이거 사용 안합니더
				}
				else if (optMesh._isMaskChild)
				{
					apOptTransform optTransform = Editor._portrait.GetOptTransform(optMesh._clipParentID);
					apOptMesh parentMesh = null;
					if (optTransform != null && optTransform._childMesh != null)
					{
						parentMesh = optTransform._childMesh;
					}
					optMesh.LinkAsMaskChild(parentMesh);
				}
			}

			//2. 2차 Bake : Modifier 만들기
			List<apOptTransform> optTransforms = Editor._portrait._optTransforms;
			for (int i = 0; i < optTransforms.Count; i++)
			{
				apOptTransform optTransform = optTransforms[i];

				apMeshGroup srcMeshGroup = Editor._portrait.GetMeshGroup(optTransform._meshGroupUniqueID);
				optTransform.BakeModifier(Editor._portrait, srcMeshGroup);
			}


			//3. 3차 Bake : ControlParam/KeyFrame ~~> Modifier <- [Calculated Param] -> OptTrasform + Mesh
			Editor._portrait.LinkOpt();

			//4. 첫번째 OptRoot만 보여주도록 하자
			if (Editor._portrait._optRootUnitList.Count > 0)
			{
				Editor._portrait.ShowRootUnit(Editor._portrait._optRootUnitList[0]);
			}

			//5. AnimClip의 데이터를 받아서 AnimPlay 데이터로 만들자
			if (Editor._portrait._animPlayManager == null)
			{
				Editor._portrait._animPlayManager = new apAnimPlayManager();
			}

			Editor._portrait._animPlayManager.InitAndLink();
			Editor._portrait._animPlayManager._animPlayDataList.Clear();

			for (int i = 0; i < Editor._portrait._animClips.Count; i++)
			{
				apAnimClip animClip = Editor._portrait._animClips[i];
				int animClipID = animClip._uniqueID;
				string animClipName = animClip._name;
				int targetMeshGroupID = animClip._targetMeshGroupID;

				apAnimPlayData animPlayData = new apAnimPlayData(animClipID, targetMeshGroupID, animClipName);
				Editor._portrait._animPlayManager._animPlayDataList.Add(animPlayData);

			}


			//6. Mask 메시 한번 더 갱신
			//if(Editor._portrait._optMaskedMeshes.Count > 0)
			//{
			//	for (int i = 0; i < Editor._portrait._optMaskedMeshes.Count; i++)
			//	{
			//		Editor._portrait._optMaskedMeshes[i].RefreshMaskedMesh();
			//	}
			//}
			//> 변경 : Child 위주로 변경
			if (Editor._portrait._optClippedMeshes.Count > 0)
			{
				for (int i = 0; i < Editor._portrait._optClippedMeshes.Count; i++)
				{
					Editor._portrait._optClippedMeshes[i].RefreshClippedMesh();
				}
			}

			apEditorUtil.SetEditorDirty();

		}


		private T AddGameObject<T>(string name, Transform parent) where T : MonoBehaviour
		{
			GameObject newGameObject = new GameObject(name);
			newGameObject.transform.parent = parent;
			newGameObject.transform.localPosition = Vector3.zero;
			newGameObject.transform.localRotation = Quaternion.identity;
			newGameObject.transform.localScale = Vector3.one;

			return newGameObject.AddComponent<T>();
		}

		private Transform AddTransform(string name, Transform parent)
		{
			GameObject newGameObject = new GameObject(name);
			newGameObject.transform.parent = parent;
			newGameObject.transform.localPosition = Vector3.zero;
			newGameObject.transform.localRotation = Quaternion.identity;
			newGameObject.transform.localScale = Vector3.one;

			return newGameObject.transform;
		}

		private void MakeMeshGroupToOptTransform(apRenderUnit renderUnit, apTransform_MeshGroup meshGroupTransform, Transform parent, apOptTransform parentTransform, apOptRootUnit targetOptRootUnit)
		{
			string objectName = meshGroupTransform._nickName;
			int meshGroupUniqueID = -1;
			if (meshGroupTransform._meshGroup != null)
			{
				objectName = meshGroupTransform._meshGroup._name;
				meshGroupUniqueID = meshGroupTransform._meshGroup._uniqueID;
			}

			apMeshGroup meshGroup = meshGroupTransform._meshGroup;

			//if(meshGroupTransform._nickName.Length == 0)
			//{
			//	Debug.LogWarning("Empy Name : " + meshGroupTransform._meshGroup._name);
			//}
			apOptTransform optTransform = AddGameObject<apOptTransform>(objectName, parent);

			//OptTransform을 설정하자
			#region [미사용 코드] SetBasicSetting 함수로 대체
			//optTransform._transformID = meshGroupTransform._transformUniqueID;
			//optTransform._transform = optTransform.transform;

			//optTransform._depth = meshGroupTransform._depth;
			//optTransform._defaultMatrix = new apMatrix(meshGroupTransform._matrix);

			////optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth * 0.1f);
			//optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth);
			//optTransform._transform.localRotation = Quaternion.Euler(0.0f, 0.0f, optTransform._defaultMatrix._angleDeg);
			//optTransform._transform.localScale = optTransform._defaultMatrix._scale; 
			#endregion

			int renderUnitLevel = -1;
			if (renderUnit != null)
			{
				renderUnitLevel = renderUnit._level;
			}
			optTransform.Bake(Editor._portrait,//meshGroup, 
								parentTransform,
								targetOptRootUnit,
								meshGroupTransform._transformUniqueID,
								meshGroupUniqueID,
								meshGroupTransform._matrix,
								false,
								renderUnitLevel, meshGroupTransform._depth,
								meshGroupTransform._isVisible_Default,
								meshGroupTransform._meshColor2X_Default);

			//첫 초기화 Matrix(No-Mod)를 만들어주자 - Mesh Bake에서 사용된다.
			if (optTransform._matrix_TF_ToParent == null)
			{ optTransform._matrix_TF_ToParent = new apMatrix(); }
			if (optTransform._matrix_TF_ParentWorld_NonModified == null)
			{ optTransform._matrix_TF_ParentWorld_NonModified = new apMatrix(); }
			if (optTransform._matrix_TFResult_WorldWithoutMod == null)
			{ optTransform._matrix_TFResult_WorldWithoutMod = new apMatrix(); }

			optTransform._matrix_TF_ToParent.SetMatrix(optTransform._defaultMatrix);
			optTransform._matrix_TF_ParentWorld_NonModified.SetIdentity();
			if (parentTransform != null)
			{
				optTransform._matrix_TF_ParentWorld_NonModified.SetMatrix(parentTransform._matrix_TFResult_WorldWithoutMod);
			}
			optTransform._matrix_TFResult_WorldWithoutMod.SetIdentity();
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ToParent);
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ParentWorld_NonModified);


			//추가
			//apBone을 추가해주자
			if (meshGroup._boneList_All.Count > 0)
			{
				MakeOptBone(meshGroup, optTransform);
			}
			else
			{
				optTransform._boneList_All = null;
				optTransform._boneList_Root = null;
				optTransform._isBoneUpdatable = false;
			}


			if (parentTransform != null)
			{
				parentTransform.AddChildTransforms(optTransform);
			}

			//만약 Root라면 ->
			if (parentTransform == null)
			{
				targetOptRootUnit._rootOptTransform = optTransform;
			}
			Editor._portrait._optTransforms.Add(optTransform);


			if (renderUnit != null)
			{
				for (int i = 0; i < renderUnit._childRenderUnits.Count; i++)
				{
					apRenderUnit childRenderUnit = renderUnit._childRenderUnits[i];

					apTransform_MeshGroup childTransform_MeshGroup = childRenderUnit._meshGroupTransform;
					apTransform_Mesh childTransform_Mesh = childRenderUnit._meshTransform;

					if (childTransform_MeshGroup != null)
					{
						MakeMeshGroupToOptTransform(childRenderUnit, childTransform_MeshGroup, optTransform.transform, optTransform, targetOptRootUnit);
					}
					else if (childTransform_Mesh != null)
					{
						MakeMeshToOptTransform(childRenderUnit, childTransform_Mesh, meshGroup, optTransform.transform, optTransform, targetOptRootUnit);
					}
					else
					{
						Debug.LogError("Empty Render Unit");
					}
				}
			}
			else
			{
				Debug.LogError("No RenderUnit");
			}

			#region [미사용 코드] Child 등록 코드 (RenderUnit 없음)
			//apMeshGroup meshGroup = meshGroupTransform._meshGroup;
			////Child를 연결하자
			//if (meshGroup != null)
			//{

			//	// Child Mesh를 등록한다.
			//	if (meshGroup._childMeshTransforms.Count > 0)
			//	{
			//		for (int i = 0; i < meshGroup._childMeshTransforms.Count; i++)
			//		{
			//			apTransform_Mesh childMeshTransform = meshGroup._childMeshTransforms[i];
			//			MakeMeshToOptTransform(childMeshTransform, meshGroup, optTransform.transform);
			//		}
			//	}

			//	//Child MeshGroup을 등록한다.
			//	if(meshGroup._childMeshGroupTransforms.Count > 0)
			//	{
			//		for (int i = 0; i < meshGroup._childMeshGroupTransforms.Count; i++)
			//		{
			//			apTransform_MeshGroup childMeshGroupTransform = meshGroup._childMeshGroupTransforms[i];
			//			MakeMeshGroupToOptTransform(childMeshGroupTransform, optTransform.transform);
			//		}
			//	}
			//} 
			#endregion
		}

		private void MakeMeshToOptTransform(apRenderUnit renderUnit, apTransform_Mesh meshTransform, apMeshGroup parentMeshGroup, Transform parent, apOptTransform parentTransform, apOptRootUnit targetOptRootUnit)
		{
			apOptTransform optTransform = AddGameObject<apOptTransform>(meshTransform._nickName, parent);

			//OptTransform을 설정하자
			#region [미사용 코드] SetBasicSetting 함수로 대체
			//optTransform._transformID = meshTransform._transformUniqueID;
			//optTransform._transform = optTransform.transform;

			//optTransform._depth = meshTransform._depth;
			//optTransform._defaultMatrix = new apMatrix(meshTransform._matrix);

			////optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth * 0.1f);
			//optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth);
			//optTransform._transform.localRotation = Quaternion.Euler(0.0f, 0.0f, optTransform._defaultMatrix._angleDeg);
			//optTransform._transform.localScale = optTransform._defaultMatrix._scale; 
			#endregion

			optTransform.Bake(Editor._portrait, //null, 
								parentTransform,
								targetOptRootUnit,
								meshTransform._transformUniqueID,
								-1,
								meshTransform._matrix,
								true,
								renderUnit._level, meshTransform._depth,
								meshTransform._isVisible_Default,
								meshTransform._meshColor2X_Default);

			//Debug.Log("Mesh OptTransform Bake [" + optTransform.name + "] Pivot : " + meshTransform._matrix._pos);
			//첫 초기화 Matrix(No-Mod)를 만들어주자 - Mesh Bake에서 사용된다.
			if (optTransform._matrix_TF_ToParent == null)
			{ optTransform._matrix_TF_ToParent = new apMatrix(); }
			if (optTransform._matrix_TF_ParentWorld_NonModified == null)
			{ optTransform._matrix_TF_ParentWorld_NonModified = new apMatrix(); }
			if (optTransform._matrix_TFResult_WorldWithoutMod == null)
			{ optTransform._matrix_TFResult_WorldWithoutMod = new apMatrix(); }

			optTransform._matrix_TF_ToParent.SetMatrix(optTransform._defaultMatrix);
			optTransform._matrix_TF_ParentWorld_NonModified.SetIdentity();
			if (parentTransform != null)
			{
				optTransform._matrix_TF_ParentWorld_NonModified.SetMatrix(parentTransform._matrix_TFResult_WorldWithoutMod);
			}
			optTransform._matrix_TFResult_WorldWithoutMod.SetIdentity();
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ToParent);
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ParentWorld_NonModified);


			if (parentTransform != null)
			{
				parentTransform.AddChildTransforms(optTransform);
			}

			Editor._portrait._optTransforms.Add(optTransform);


			//하위에 OptMesh를 만들자
			apMesh mesh = meshTransform._mesh;
			if (mesh != null)
			{
				apOptMesh optMesh = AddGameObject<apOptMesh>(meshTransform._nickName + "_Mesh", optTransform.transform);
				optMesh.gameObject.AddComponent<MeshFilter>();
				optMesh.gameObject.AddComponent<MeshRenderer>();

				List<apVertex> verts = mesh._vertexData;

				List<Vector3> posList = new List<Vector3>();
				List<Vector2> UVList = new List<Vector2>();
				List<int> IDList = new List<int>();
				List<int> triList = new List<int>();

				apVertex vert = null;
				for (int i = 0; i < verts.Count; i++)
				{
					vert = verts[i];
					posList.Add(vert._pos);
					UVList.Add(vert._uv);
					IDList.Add(vert._uniqueID);
				}

				for (int i = 0; i < mesh._indexBuffer.Count; i++)
				{
					triList.Add(mesh._indexBuffer[i]);
				}

				Texture2D texture = null;
				if (mesh._textureData != null)
				{
					texture = mesh._textureData._image;
				}
				//Mesh Bake를 하자
				optMesh._portrait = Editor._portrait;
				optMesh._uniqueID = meshTransform._transformUniqueID;

				//Shader 설정
				Shader shaderNormal = GetOptMeshShader(meshTransform._shaderType, false);
				Shader shaderMask = GetOptMeshShader(meshTransform._shaderType, true);

				if (meshTransform._isCustomShader && meshTransform._customShader != null)
				{
					shaderNormal = meshTransform._customShader;
					shaderMask = meshTransform._customShader;
				}

				optMesh.BakeMesh(posList.ToArray(),
									UVList.ToArray(),
									IDList.ToArray(),
									triList.ToArray(),
									mesh._offsetPos,
									optTransform,
									texture,
									meshTransform._shaderType,
									shaderNormal,
									shaderMask
									);

				//Clipping의 기본 정보를 넣고, 나중에 연결하자
				if (meshTransform._isClipping_Parent)
				{
					List<int> clipIDs = new List<int>();
					for (int iClip = 0; iClip < meshTransform._clipChildMeshes.Count; iClip++)
					{
						clipIDs.Add(meshTransform._clipChildMeshes[iClip]._transformID);
					}

					optMesh.SetMaskBasicSetting_Parent(clipIDs);
					//optMesh.SetMaskBasicSetting_Parent(meshTransform._clipChildMeshTransformIDs);

					//따로 관리할 마스크 메시에 넣는다.
					Editor._portrait._optMaskedMeshes.Add(optMesh);
				}
				else if (meshTransform._isClipping_Child)
				{
					optMesh.SetMaskBasicSetting_Child(meshTransform._clipParentMeshTransform._transformUniqueID);
					Editor._portrait._optClippedMeshes.Add(optMesh);
				}


				//Parent Transform에 등록하자
				optTransform.SetChildMesh(optMesh);

				Editor._portrait._optMeshes.Add(optMesh);
			}
		}

		//private string GetOptMeshShader(apPortrait.SHADER_TYPE shaderType, bool isClipping)
		//{
		//	switch (shaderType)
		//	{
		//		case apPortrait.SHADER_TYPE.AlphaBlend:
		//			if(!isClipping)		{ return "AnyPortrait/Transparent/Colored Texture(2X) AlphaBlend"; }
		//			else				{ return "AnyPortrait/Transparent/Masked Colored Texture (2X) AlphaBlend"; }

		//		case apPortrait.SHADER_TYPE.Additive:
		//			if(!isClipping)		{ return "AnyPortrait/Transparent/Colored Texture (2X) Additive"; }
		//			else				{ return "AnyPortrait/Transparent/Masked Colored Texture (2X) Additive"; }

		//		case apPortrait.SHADER_TYPE.SoftAdditive:
		//			if(!isClipping)		{ return "AnyPortrait/Transparent/Colored Texture (2X) SoftAdditive"; }
		//			else				{ return "AnyPortrait/Transparent/Masked Colored Texture (2X) SoftAdditive"; }

		//		case apPortrait.SHADER_TYPE.Multiplicative:
		//			if(!isClipping)		{ return "AnyPortrait/Transparent/Colored Texture (2X) Multiplicative"; }
		//			else				{ return "AnyPortrait/Transparent/Masked Colored Texture (2X) Multiplicative"; }
		//	}
		//	return "";
		//}

		private Shader GetOptMeshShader(apPortrait.SHADER_TYPE shaderType, bool isClipping)
		{
			Material targetMat = null;
			string materialAssetName = "";
			switch (shaderType)
			{
				case apPortrait.SHADER_TYPE.AlphaBlend:
					if (!isClipping)
					{ materialAssetName = "apMat_Opt_Normal"; }
					else
					{ materialAssetName = "apMat_Opt_Clipped"; }
					break;

				case apPortrait.SHADER_TYPE.Additive:
					if (!isClipping)
					{ materialAssetName = "apMat_Opt_Normal Additive"; }
					else
					{ materialAssetName = "apMat_Opt_Clipped Additive"; }
					break;

				case apPortrait.SHADER_TYPE.SoftAdditive:
					if (!isClipping)
					{ materialAssetName = "apMat_Opt_Normal SoftAdditive"; }
					else
					{ materialAssetName = "apMat_Opt_Clipped SoftAdditive"; }
					break;

				case apPortrait.SHADER_TYPE.Multiplicative:
					if (!isClipping)
					{ materialAssetName = "apMat_Opt_Normal Multiplicative"; }
					else
					{ materialAssetName = "apMat_Opt_Clipped Multiplicative"; }
					break;
			}
			if (string.IsNullOrEmpty(materialAssetName))
			{
				return null;
			}
			targetMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/AnyPortraitTool/" + materialAssetName + ".mat");
			if (targetMat == null)
			{
				Debug.LogError("Error : Invalid Shader [" + materialAssetName + "]");
				return null;
			}

			return targetMat.shader;
		}


		private void MakeOptBone(apMeshGroup srcMeshGroup, apOptTransform targetOptTransform)
		{
			//1. Bone Group을 만들고
			//2. Bone을 계층적으로 추가하자 (재귀 함수 필요)

			targetOptTransform._boneGroup = AddTransform("__Bone Group", targetOptTransform.transform);
			targetOptTransform._isBoneUpdatable = true;

			List<apBone> rootBones = srcMeshGroup._boneList_Root;
			List<apOptBone> totalOptBones = new List<apOptBone>();
			for (int i = 0; i < rootBones.Count; i++)
			{
				apOptBone newRootBone = MakeOptBoneRecursive(srcMeshGroup, rootBones[i], null, targetOptTransform, totalOptBones);
				targetOptTransform._boneList_Root = apEditorUtil.AddItemToArray<apOptBone>(newRootBone, targetOptTransform._boneList_Root);
			}

			targetOptTransform._boneList_All = totalOptBones.ToArray();

			//이제 전체 Bone을 돌면서 링크를 해주자
			//TODO
			for (int i = 0; i < totalOptBones.Count; i++)
			{
				totalOptBones[i].Link(targetOptTransform);

			}

			//Root에서부터 LinkChaining을 실행하자
			for (int i = 0; i < targetOptTransform._boneList_Root.Length; i++)
			{
				targetOptTransform._boneList_Root[i].LinkBoneChaining();
			}
		}

		private apOptBone MakeOptBoneRecursive(apMeshGroup srcMeshGroup, apBone srcBone, apOptBone parentOptBone, apOptTransform targetOptTransform, List<apOptBone> resultOptBones)
		{
			Transform parentTransform = targetOptTransform._boneGroup;
			if (parentOptBone != null)
			{
				parentTransform = parentOptBone.transform;
			}
			apOptBone newBone = AddGameObject<apOptBone>(srcBone._name, parentTransform);

			//TODO : Bake 해야한다.
			//Link를 제외한 Bake를 먼저 하자.
			//Link는 ID를 이용하여 일괄적으로 처리
			newBone.Bake(srcBone);

			if (srcBone._isSocketEnabled)
			{
				//소켓을 붙여주자
				newBone._socketTransform = AddTransform(srcBone._name + " Socket", newBone.transform);
			}

			if (parentOptBone != null)
			{
				newBone._parentBone = parentOptBone;
				parentOptBone._childBones = apEditorUtil.AddItemToArray<apOptBone>(newBone, parentOptBone._childBones);
			}


			resultOptBones.Add(newBone);
			//하위 Child Bone에 대해서도 반복

			for (int i = 0; i < srcBone._childBones.Count; i++)
			{
				MakeOptBoneRecursive(srcMeshGroup, srcBone._childBones[i], newBone, targetOptTransform, resultOptBones);
			}

			return newBone;
		}

		private Material GetOptMaterial(apMesh mesh)
		{
			//일단..
			return null;
		}


		//----------------------------------------------------------------------------------
		// GUI - Input
		//----------------------------------------------------------------------------------
		//------------------------------------------------------------------------------
		public int GUI_Controller_Upper(int width)
		{
			if (Editor == null)
			{
				return 0;
			}
			if (Editor._portrait == null)
			{
				return 0;
			}
			EditorGUILayout.LabelField("Category");
			Editor._curParamCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumMaskPopup(new GUIContent(""), Editor._curParamCategory, GUILayout.Width(width));

			GUILayout.Space(5);
			//56 - 15 = 41

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(4);
			//리셋 기능은 뺀다.
			//if(GUILayout.Button("Reset Param"))
			//{
			//	bool isResult = EditorUtility.DisplayDialog("Reset", "Really Reset Params?", "Reset", "Cancel");
			//	if (isResult)
			//	{
			//		ResetControlParams();
			//	}
			//	//Editor.Hierarchy.RefreshUnits();
			//	Editor.RefreshControllerAndHierarchy();
			//}

			if (GUILayout.Button("Set Default All"))
			{
				//bool isResult = EditorUtility.DisplayDialog("Reset", "Really Set Default Value?", "Set Default All", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.ControlParamDefaultAll_Title),
																Editor.GetText(apLocalization.TEXT.ControlParamDefaultAll_Body),
																Editor.GetText(apLocalization.TEXT.ControlParamDefaultAll_Okay),
																Editor.GetText(apLocalization.TEXT.Cancel));

				if (isResult)
				{
					List<apControlParam> cParams = Editor.ParamControl._controlParams;

					for (int i = 0; i < cParams.Count; i++)
					{
						cParams[i].SetDefault();
					}
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy();
			}
			GUILayout.Space(4);
			EditorGUILayout.EndHorizontal();
			//56 + 60 => 41 + 25 = 
			//return 56 + 60;
			return 41 + 25;
		}

		public void SetDefaultAllControlParams()
		{
			if (Editor == null || Editor._portrait == null)
			{
				return;
			}

			List<apControlParam> cParams = Editor.ParamControl._controlParams;

			for (int i = 0; i < cParams.Count; i++)
			{
				cParams[i].SetDefault();
			}
		}



		public void GUI_Controller(int width)
		{
			if (Editor == null)
			{
				return;
			}
			if (Editor._portrait == null)
			{
				return;
			}

			bool isRecording = false;
			List<apModifierParamSet> modParamSetList = null;
			apModifierParamSetGroup modParamSetGroup = null;

			//ControlParam 타입의 Timeline을 작업중인가.
			bool isAnimEditing = false;
			apAnimTimeline animTimeline = null;
			apAnimTimelineLayer animTimelineLayer = null;
			apAnimKeyframe animKeyframe = null;

			//TODO : 현재 레코딩 중인지 체크
			if (Editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				if (Editor.Select.Modifier != null)
				{
					//[중요] 모디파이어중에서 ControlParam의 영향을 받을 경우 여기서 추가해서 키를 추가할 수 있게 세팅해야 한다.
					switch (Editor.Select.Modifier.SyncTarget)
					{
						case apModifierParamSetGroup.SYNC_TARGET.Controller:
							isRecording = true;
							modParamSetGroup = Editor.Select.SubEditedParamSetGroup;
							//modParamSetList = Editor.Select.Modifier._paramSetList;
							if (modParamSetGroup != null)
							{
								modParamSetList = modParamSetGroup._paramSetList;
							}

							break;
					}
				}
			}
			else if (Editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				//추가 : Animation 상황에서도 레코딩이 가능하다. 단, isRecording(키 생성 방식)은 아니고
				//그 자체가 Keyframe의 값으로 치환되어야 한다.
				if (Editor.Select.AnimClip != null && Editor.Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
				{
					if (Editor.Select.AnimTimeline != null && Editor.Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						isAnimEditing = true;
						animTimeline = Editor.Select.AnimTimeline;
						animTimelineLayer = Editor.Select.AnimTimelineLayer;
						if (animTimelineLayer != null)
						{
							if (Editor.Select.AnimKeyframes.Count == 1 && Editor.Select.AnimKeyframe != null)
							{
								animKeyframe = Editor.Select.AnimKeyframe;
							}
						}
					}
				}
			}



			List<apControlParam> cParams = Editor.ParamControl._controlParams;
			for (int i = 0; i < cParams.Count; i++)
			{
				if ((byte)(cParams[i]._category & Editor._curParamCategory) != 0)
				{
					GUI_ControlParam(cParams[i], width, isRecording, modParamSetList, modParamSetGroup, isAnimEditing, animTimelineLayer, animKeyframe);
					GUILayout.Space(10);
				}
			}
		}


		private void GUI_ControlParam(apControlParam controlParam, int width,
			bool isRecording, List<apModifierParamSet> modParamSetList, apModifierParamSetGroup curParamSetGroup,
			bool isAnimEditing, apAnimTimelineLayer animTimelineLayer, apAnimKeyframe animKeyframe)
		{
			width -= 10;


			int labelWidth = width - (50 + 4);
			if (isRecording)
			{
				labelWidth = width - (75 + 7);
			}

			int recordBtnSize = 25;
			int presetIconSize = 32;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(presetIconSize));
			GUILayout.Box(Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(controlParam._iconPreset)), GUI.skin.label, GUILayout.Width(presetIconSize), GUILayout.Height(presetIconSize));

			EditorGUILayout.LabelField(controlParam._keyName, GUILayout.Width(labelWidth - (presetIconSize + 4)), GUILayout.Height(presetIconSize));

			apModifierParamSet recordedKey = null;
			apModifierParamSet prevRecordedKey = Editor.Select.ParamSetOfMod;

			bool isCurSelected = false;
			if (curParamSetGroup != null)
			{
				if (curParamSetGroup._keyControlParam == controlParam)
				{
					isCurSelected = true;
				}
			}
			else if (isAnimEditing && animTimelineLayer != null)
			{
				if (animTimelineLayer._linkedControlParam == controlParam)
				{
					isCurSelected = true;
				}
			}



			List<apModifierParamSet> recordKeyParamSet = null;
			if (isRecording &&
				//controlParam._isRange && 
				(controlParam._valueType == apControlParam.TYPE.Int ||
				controlParam._valueType == apControlParam.TYPE.Float ||
				controlParam._valueType == apControlParam.TYPE.Vector2
				//controlParam._valueType == apControlParam.TYPE.Vector3
				))
			{
				recordKeyParamSet = new List<apModifierParamSet>();
				bool isRecordKey = false;

				if (curParamSetGroup != null && modParamSetList != null)
				{
					//TODO : 현재 레코딩 키 위에 컨트롤러가 있는지 체크
					for (int i = 0; i < modParamSetList.Count; i++)
					{
						apModifierParamSet modParamSet = modParamSetList[i];
						if (curParamSetGroup._syncTarget != apModifierParamSetGroup.SYNC_TARGET.Controller)
						{
							continue;
						}
						if (curParamSetGroup._keyControlParam != controlParam)
						{
							continue;
						}



						recordKeyParamSet.Add(modParamSet);

						//현재 레코드 키 위에 있는지 체크하자
						float biasX = 0.0f;
						float biasY = 0.0f;

						if (recordedKey == null)
						{
							switch (controlParam._valueType)
							{
								case apControlParam.TYPE.Int:
									{
										if (controlParam._int_Cur == modParamSet._conSyncValue_Int)
										{
											isRecordKey = true;
										}
									}
									break;

								case apControlParam.TYPE.Float:
									{
										biasX = (Mathf.Abs(controlParam._float_Max - controlParam._float_Min) / (float)controlParam._snapSize) * 0.2f;

										if (controlParam._float_Cur > modParamSet._conSyncValue_Float - biasX &&
											controlParam._float_Cur < modParamSet._conSyncValue_Float + biasX
											)
										{
											isRecordKey = true;
										}
									}
									break;

								case apControlParam.TYPE.Vector2:
									{
										//biasX = Mathf.Abs(controlParam._vec2_Max.x - controlParam._vec2_Min.x) * 0.05f;
										//biasY = Mathf.Abs(controlParam._vec2_Max.y - controlParam._vec2_Min.y) * 0.05f;

										//if (biasX > 0.05f)
										//{ biasX = 0.05f; }
										//if (biasY > 0.05f)
										//{ biasY = 0.05f; }

										//bias는 기본 Snap 크기의 절반이다.
										biasX = (Mathf.Abs(controlParam._vec2_Max.x - controlParam._vec2_Min.x) / (float)controlParam._snapSize) * 0.2f;
										biasY = (Mathf.Abs(controlParam._vec2_Max.y - controlParam._vec2_Min.y) / (float)controlParam._snapSize) * 0.2f;

										if (controlParam._vec2_Cur.x > modParamSet._conSyncValue_Vector2.x - biasX &&
											controlParam._vec2_Cur.x < modParamSet._conSyncValue_Vector2.x + biasX &&
											controlParam._vec2_Cur.y > modParamSet._conSyncValue_Vector2.y - biasY &&
											controlParam._vec2_Cur.y < modParamSet._conSyncValue_Vector2.y + biasY
											)
										{
											isRecordKey = true;
										}
									}
									break;



							}
						}

						if (isRecordKey && recordedKey == null)
						{
							recordedKey = modParamSet;
						}
					}
				}


				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					if (prevRecordedKey != recordedKey)
					{
						if (recordedKey != null)
						{
							//자동으로 선택해주자
							if (Editor.Select.SubEditedParamSetGroup._keyControlParam == controlParam &&
									Editor.Select.SubEditedParamSetGroup._paramSetList.Contains(recordedKey))
							{
								//만약
								//현재 Modifier에서 Record 키 작업중이라면
								//현재 ParamSet을 Select에 지정하는 것도 좋겠다.
								Editor.Select.SetParamSetOfModifier(recordedKey);
							}
						}
						else
						{
							if (Editor.Select.SubEditedParamSetGroup._keyControlParam == controlParam)
							{
								Editor.Select.SetParamSetOfModifier(null);
								Editor.Hierarchy_MeshGroup.RefreshUnits();
							}
						}
					}
				}


				if (isRecordKey)
				{
					//선택된 RecordKey가 있다.
					if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey), GUILayout.Width(recordBtnSize), GUILayout.Height(recordBtnSize)))
					{
						//bool isResult = EditorUtility.DisplayDialog("Remove Record Key", "Remove Record Key?", "Remove", "Cancel");
						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(apLocalization.TEXT.RemoveRecordKey_Title),
																		Editor.GetText(apLocalization.TEXT.RemoveRecordKey_Body),
																		Editor.GetText(apLocalization.TEXT.Remove),
																		Editor.GetText(apLocalization.TEXT.Cancel));
						if (isResult && recordedKey != null)
						{
							RemoveRecordKey(recordedKey, recordKeyParamSet);
						}
					}
				}
				else
				{
					//선택된 RecordKey가 없다.
					//단, 이걸 출력하려면 -> 
					if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_MakeRecordKey), GUILayout.Width(recordBtnSize), GUILayout.Height(recordBtnSize)))
					{
						AddControlParamToModifier(controlParam);
					}
				}
			}

			if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_Default), GUILayout.Width(recordBtnSize), GUILayout.Height(recordBtnSize)))
			{
				controlParam.SetDefault();
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_Edit), GUILayout.Width(recordBtnSize), GUILayout.Height(recordBtnSize)))
			{
				Editor.Select.SetParam(controlParam);
				//Editor.Repaint();
				Editor._tabLeft = apEditor.TAB_LEFT.Hierarchy;
				Editor.RefreshControllerAndHierarchy();
				Editor.SetRepaint();

			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUIStyle guiStyle_RightLabel = new GUIStyle(GUI.skin.label);
			guiStyle_RightLabel.alignment = TextAnchor.UpperRight;

			int unitHeight = 24;
			int guiHeight = 0;
			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
				case apControlParam.TYPE.Float:
					{
						guiHeight += unitHeight * 3;
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						guiHeight += unitHeight * 6;
					}
					break;
			}


			Rect lastRect = GUILayoutUtility.GetLastRect();
			Color prevColor = GUI.backgroundColor;

			if (isCurSelected)
			{
				GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);
			}
			else
			{
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
			}


			GUI.Box(new Rect(lastRect.x, lastRect.y, width + 10, guiHeight), "");
			GUI.backgroundColor = prevColor;
			bool isRepaint = false;


			Vector2 guiPos = new Vector2(lastRect.x, lastRect.y + 25);

			switch (controlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	{
				//		bool boolNext = EditorGUILayout.Toggle(controlParam._bool_Cur);
				//		if(controlParam._bool_Cur != boolNext)
				//		{
				//			controlParam._bool_Cur = boolNext;
				//			isRepaint = true;
				//		}
				//	}
				//	break;

				case apControlParam.TYPE.Int:
					{
						int intNext = controlParam._int_Cur;
						//if(controlParam._isRange)
						//{	

						//1차로 체크
						intNext = EditorGUILayout.DelayedIntField(controlParam._int_Cur);
						if (intNext != controlParam._int_Cur)
						{
							controlParam._int_Cur = intNext;
							isRepaint = true;
						}

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Min, GUILayout.Width(width / 2 - 5));
						EditorGUILayout.LabelField(controlParam._label_Max, guiStyle_RightLabel, GUILayout.Width(width / 2 - 5));
						EditorGUILayout.EndHorizontal();

						//intNext = EditorGUILayout.IntSlider(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);
						intNext = apControllerGL.DrawIntSlider(guiPos + new Vector2(0, unitHeight - 3), width, controlParam, isRecording, recordKeyParamSet, recordedKey);
						intNext = Mathf.Clamp(intNext, controlParam._int_Min, controlParam._int_Max);

						GUILayout.Space(unitHeight);

						//}
						//else
						//{
						//	intNext = EditorGUILayout.IntField(controlParam._int_Cur);
						//}

						if (intNext != controlParam._int_Cur)
						{
							controlParam._int_Cur = intNext;
							isRepaint = true;
						}
					}
					break;

				case apControlParam.TYPE.Float:
					{
						float floatNext = controlParam._float_Cur;
						//if(controlParam._isRange)
						//{
						//1차로 체크
						floatNext = EditorGUILayout.DelayedFloatField(controlParam._float_Cur);
						floatNext = Mathf.Clamp(floatNext, controlParam._float_Min, controlParam._float_Max);
						if (floatNext != controlParam._float_Cur)
						{
							controlParam._float_Cur = floatNext;
							isRepaint = true;
						}


						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Min, GUILayout.Width(width / 2 - 5));
						EditorGUILayout.LabelField(controlParam._label_Max, guiStyle_RightLabel, GUILayout.Width(width / 2 - 5));
						EditorGUILayout.EndHorizontal();

						//floatNext = EditorGUILayout.Slider(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);

						floatNext = apControllerGL.DrawFloatSlider(guiPos + new Vector2(0, unitHeight - 3), width, controlParam, isRecording, recordKeyParamSet, recordedKey);

						GUILayout.Space(unitHeight);

						floatNext = Mathf.Clamp(floatNext, controlParam._float_Min, controlParam._float_Max);
						//}
						//else
						//{
						//	floatNext = EditorGUILayout.FloatField(controlParam._float_Cur);

						//}

						if (floatNext != controlParam._float_Cur)
						{
							controlParam._float_Cur = floatNext;
							isRepaint = true;
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						Vector2 vec2Next = controlParam._vec2_Cur;
						//if(controlParam._isRange)
						//{

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						vec2Next.x = EditorGUILayout.DelayedFloatField(vec2Next.x, GUILayout.Width(width / 2 - 5));
						vec2Next.y = EditorGUILayout.DelayedFloatField(vec2Next.y, GUILayout.Width(width / 2 - 5));

						vec2Next.x = Mathf.Clamp(vec2Next.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						vec2Next.y = Mathf.Clamp(vec2Next.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);



						//여기서 1차로 한번 검사
						if (vec2Next.x != controlParam._vec2_Cur.x || vec2Next.y != controlParam._vec2_Cur.y)
						{
							controlParam._vec2_Cur = vec2Next;
							isRepaint = true;
						}
						EditorGUILayout.EndHorizontal();


						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Max, GUILayout.Width(width));
						EditorGUILayout.EndHorizontal();

						//2차로 쉽게 제어
						vec2Next = apControllerGL.DrawVector2Slider(guiPos + new Vector2(0, unitHeight - 3), width, unitHeight * 3, controlParam, isRecording, recordKeyParamSet, recordedKey);

						vec2Next.x = Mathf.Clamp(vec2Next.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						vec2Next.y = Mathf.Clamp(vec2Next.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);

						GUILayout.Space(unitHeight * 3);
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Min, guiStyle_RightLabel, GUILayout.Width(width));
						EditorGUILayout.EndHorizontal();


						if (vec2Next.x != controlParam._vec2_Cur.x || vec2Next.y != controlParam._vec2_Cur.y)
						{
							controlParam._vec2_Cur = vec2Next;
							isRepaint = true;
						}
					}
					break;


			}

			GUILayout.Space(20);

			//애니메이션 작업 중이라면 => ControlParam의 값을 바로 keyframe
			if (isRepaint && isAnimEditing && animTimelineLayer != null && isCurSelected && animKeyframe != null)
			{
				apEditorUtil.SetRecord(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor._portrait, animKeyframe, true, Editor);

				switch (controlParam._valueType)
				{
					case apControlParam.TYPE.Int:
						animKeyframe._conSyncValue_Int = controlParam._int_Cur;
						break;

					case apControlParam.TYPE.Float:
						animKeyframe._conSyncValue_Float = controlParam._float_Cur;
						break;

					case apControlParam.TYPE.Vector2:
						animKeyframe._conSyncValue_Vector2 = controlParam._vec2_Cur;
						break;
				}

			}

			if (isRepaint)
			{
				//Editor.Repaint();
				Editor.SetRepaint();
			}
		}


		public void ResetControlParams()
		{
			Editor.ParamControl._controlParams.Clear();
			Editor.ParamControl.MakeReservedParams();
		}


		public void RemoveRecordKey(apModifierParamSet recordedKey, List<apModifierParamSet> recordKeyParamSet)
		{
			bool isResetSelect_ParamSet = false;
			bool isResetSelect_ParamSetGroup = false;
			if (recordedKey == null)
			{
				return;
			}

			if (recordedKey == Editor.Select.ParamSetOfMod)
			{
				isResetSelect_ParamSet = true;
			}

			if (Editor.Select.Modifier != null && Editor.Select.SubEditedParamSetGroup != null)
			{
				apModifierParamSetGroup paramSetGroup = Editor.Select.SubEditedParamSetGroup;
				paramSetGroup._paramSetList.Remove(recordedKey);

				//Editor.Select.Modifier._paramSetList.Remove(recordedKey);
				Editor.Select.Modifier.RefreshParamSet();

				Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink();

				if (!Editor.Select.Modifier._paramSetGroup_controller.Contains(paramSetGroup))
				{
					//그사이에 ParamSetGroup이 사라졌다면
					isResetSelect_ParamSetGroup = true;
				}
			}

			if (recordKeyParamSet != null)
			{
				recordKeyParamSet.Remove(recordedKey);
			}

			//Select에서 선택중인게 삭제 되었다면..
			if (isResetSelect_ParamSet)
			{
				Editor.Select.SetParamSetOfModifier(null);
			}
			if (isResetSelect_ParamSetGroup)
			{
				Editor.Select.SetParamSetGroupOfModifier(null);
			}
		}

		public void OnHotKeyEvent_GizmoSelect()
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Select);
		}
		public void OnHotKeyEvent_GizmoMove()
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Move);
		}
		public void OnHotKeyEvent_GizmoRotate()
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Rotate);
		}
		public void OnHotKeyEvent_GizmoScale()
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Scale);
		}
		////-------------------------------------------------------------------------------------------
		#region [미사용 코드] ID 할당은 Portrait에서 합니다.
		////-------------------------------------------------------------------------------------------
		////-------------------------------------------------------------------------------------------------------
		//public int GetNewTextureID()
		//{
		//	if(Editor.Select.Portrait == null)
		//	{
		//		return -1;
		//	}

		//	int cnt = 0;
		//	while(true)
		//	{
		//		int nextID = UnityEngine.Random.Range(10000, 9999999);

		//		bool isExist = Editor.Select.Portrait._textureData.Exists(delegate (apTextureData a)
		//		{
		//			return a._uniqueID == nextID;
		//		});

		//		if(!isExist)
		//		{
		//			return nextID;
		//		}
		//		cnt++;
		//		if(cnt > 100)
		//		{
		//			break;
		//		}
		//	}

		//	for (int i = 1; i < 9999; i++)
		//	{
		//		int nextID = i;
		//		bool isExist = Editor.Select.Portrait._textureData.Exists(delegate (apTextureData a)
		//		{
		//			return a._uniqueID == nextID;
		//		});
		//		if(!isExist)
		//		{
		//			return nextID;
		//		}
		//	}
		//	return -1;
		//}

		//public int GetNewMeshID()
		//{
		//	if(Editor.Select.Portrait == null)
		//	{
		//		return -1;
		//	}

		//	int cnt = 0;
		//	while(true)
		//	{
		//		int nextID = UnityEngine.Random.Range(10000, 9999999);

		//		bool isExist = Editor.Select.Portrait._meshes.Exists(delegate (apMesh a)
		//		{
		//			return a._uniqueID == nextID;
		//		});

		//		if(!isExist)
		//		{
		//			return nextID;
		//		}
		//		cnt++;
		//		if(cnt > 100)
		//		{
		//			break;
		//		}
		//	}

		//	for (int i = 1; i < 9999; i++)
		//	{
		//		int nextID = i;
		//		bool isExist = Editor.Select.Portrait._meshes.Exists(delegate (apMesh a)
		//		{
		//			return a._uniqueID == nextID;
		//		});
		//		if(!isExist)
		//		{
		//			return nextID;
		//		}
		//	}
		//	return -1;
		//}

		//public int GetNewMeshGroupID()
		//{
		//	if(Editor.Select.Portrait == null)
		//	{
		//		return -1;
		//	}

		//	int cnt = 0;
		//	while(true)
		//	{
		//		int nextID = UnityEngine.Random.Range(10000, 9999999);

		//		bool isExist = Editor.Select.Portrait._meshGroups.Exists(delegate (apMeshGroup a)
		//		{
		//			return a._uniqueID == nextID;
		//		});

		//		if(!isExist)
		//		{
		//			return nextID;
		//		}
		//		cnt++;
		//		if(cnt > 100)
		//		{
		//			break;
		//		}
		//	}

		//	for (int i = 1; i < 9999; i++)
		//	{
		//		int nextID = i;
		//		bool isExist = Editor.Select.Portrait._meshGroups.Exists(delegate (apMeshGroup a)
		//		{
		//			return a._uniqueID == nextID;
		//		});
		//		if(!isExist)
		//		{
		//			return nextID;
		//		}
		//	}
		//	return -1;
		//} 
		#endregion
	}
}
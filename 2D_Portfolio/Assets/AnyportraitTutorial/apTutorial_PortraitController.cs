/*
*	Copyright (c) 2017. RainyRizzle. All rights reserved
*	https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;

public class apTutorial_PortraitController : MonoBehaviour
{
	//터치시 : 
	// 1) 바깥 : Normal, Smile > Normal 바라보기 / Angry, Emb > Angry 바라보기 > 손떼면 : Normal > Normal, Angry > 50% 확률로 Normal, Angry
	// 2) 머리 : >> Smile
	// 3) 몸 : >> Emb > Normal
	// Members
	//------------------------------------------------
	public apPortrait portrait;
	public Camera targetCamera;

	public Collider bodyArea;
	public Collider headArea;
	public Transform headCenter;

	public string controlParamName_EyeDirection = "Eyeiris Direction";
	public string controlParamName_HeadDirection = "Head Direction";

	public Vector2 eyeAreaMaxSize = new Vector2(20, 20);
	public Vector2 headAreaMaxSize = new Vector2(10, 10);

	public ParticleSystem touchParticle;
	public Transform handGroup;
	public MeshRenderer handMesh_Released;
	public MeshRenderer handMesh_Pressed;

	private enum Status
	{
		Normal,
		Smile,
		Angry,
		Embarrassed,
		SetAngry,//Embarrassed와 비슷
	}
	private Status status = Status.Normal;
	private bool isFirstUpdate = true;
	private Vector3 touchPos = Vector3.zero;
	private bool isTouched = false;
	private bool isTouchDragLock = false;
	private bool isTouchedPrevFrame = false;

	private float smileTime = 0.0f;
	private float embarrassedTime = 0.0f;
	private float setAngryTime = 0.0f;

	private bool isTouchHead = false;
	private bool isTouchBody = false;

	private bool isTouchDown = false;
	private int numTouchBody = 0;

	private bool isEyeReturn = false;
	private float eyeReturnTime = 0.0f;
	private float eyeReturnTimeLength = 0.3f;
	private Vector2 lastEyeParam = Vector2.zero;
	private Vector2 lastHeadParam = Vector2.zero;
	

	// Use this for initialization
	void Start ()
	{
		isFirstUpdate = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(isFirstUpdate)
		{
			status = Status.Normal;
			isTouched = false;
			isTouchDragLock = false;
			isTouchedPrevFrame = false;
			touchPos = Vector3.zero;

			isTouchHead = false;
			isTouchBody = false;

			isTouchDown = false;
			isEyeReturn = false;

			portrait.Play("Idle Normal");

			isFirstUpdate = false;

			Cursor.visible = false;
		}

		isTouched = false;
		Vector2 touchPosScreen = Vector2.zero;

		
		if(Input.GetMouseButton(0))
		{
			isTouched = true;
			touchPosScreen = Input.mousePosition;
		}
		else if(Input.touchCount == 1)
		{	
			TouchPhase touchPhase = Input.GetTouch(0).phase;
			if(touchPhase == TouchPhase.Began || 
				touchPhase == TouchPhase.Moved ||
				touchPhase == TouchPhase.Stationary)
			{
				isTouched = true;
				touchPosScreen = Input.GetTouch(0).position;
			}
		}
		//if(isTouched)
		//{
		//	if (isTouchDragLock)
		//	{
		//		isTouched = false;
		//	}
		//}
		//else
		//{
		//	isTouchDragLock = false;
		//}

		
		if(isTouched)
		{
			touchPos = targetCamera.ScreenToWorldPoint(new Vector3(touchPosScreen.x, touchPosScreen.y, portrait.transform.position.z));
			
		}
		else
		{
			isTouchDown = true;
		}


		//만약 터치가 바뀌었다면
		//이벤트 발생
		if(isTouched != isTouchedPrevFrame)
		{
			isTouchHead = false;
			isTouchBody = false;

			if(isTouched)
			{	
				Ray touchRay = targetCamera.ScreenPointToRay(new Vector3(touchPosScreen.x, touchPosScreen.y, 1000.0f));
				RaycastHit[] hits = Physics.RaycastAll(touchRay);
				if(hits != null && hits.Length > 0)
				{
					for (int i = 0; i < hits.Length; i++)
					{
						if(hits[i].collider == bodyArea)
						{
							isTouchBody = true;
							break;
						}
						if(hits[i].collider == headArea)
						{
							isTouchHead = true;
							break;
						}
					}
				}

				if (isTouchHead)
				{
					if (isTouchDown)
					{
						//좋아합니다.
						ChangeStatus(Status.Smile);
						isTouchDragLock = true;
					}
				}
				else if (isTouchBody)
				{
					if (isTouchDown)
					{
						//싫어합니다.
						numTouchBody++;
						if (status == Status.Embarrassed)
						{
							ChangeStatus(Status.SetAngry);
							numTouchBody = 0;
						}
						else
						{
							if (numTouchBody < 2)
							{
								ChangeStatus(Status.Embarrassed);
								isTouchDragLock = true;
								numTouchBody++;
							}
							else
							{
								ChangeStatus(Status.SetAngry);
								numTouchBody = 0;
								isTouchDragLock = true;
							}
						}
					}
				}
				else
				{
					//바라보기 스테이터스
					switch (status)
					{
						case Status.Normal:
							//if(isTouchDown)
							//{
							//	if(numTouchBody >= 1)
							//	{
							//		numTouchBody = 0;
							//		ChangeStatus(Status.Angry);
							//	}

							//}
							break;

						case Status.Smile:
							if (isTouchDown)
							{
								ChangeStatus(Status.Normal);
							}
							break;

						case Status.Angry:
							break;

						case Status.Embarrassed:
							//if (isTouchDown)
							//{
							//	//ChangeStatus(Status.Angry);
							//	ChangeStatus(Status.SetAngry);
							//}
							break;

						case Status.SetAngry:
							break;
					}
				}
			}
			else
			{
				
				switch (status)
				{
					case Status.Normal:
					//case Status.Smile:
						ChangeStatus(Status.Normal);
						break;

					case Status.Angry:
					//case Status.Embarrassed:
						if(UnityEngine.Random.Range(0, 9) < 3)
						{
							ChangeStatus(Status.Normal);
						}
						else
						{
							ChangeStatus(Status.Angry);
						}
						break;
				}

				isEyeReturn = true;
				eyeReturnTime = 0.0f;
			}
		}


		if(isTouched 
			//&& !isTouchHead && !isTouchBody
			)
		{
			Vector2 head2Touch = new Vector2(touchPos.x - headCenter.position.x, touchPos.y - headCenter.position.y);
			Vector2 eyeParam = new Vector2(	Mathf.Clamp(head2Touch.x / eyeAreaMaxSize.x, -1, 1),
											Mathf.Clamp(head2Touch.y / eyeAreaMaxSize.y, -1, 1));

			Vector2 headParam = new Vector2(	Mathf.Clamp(head2Touch.x / headAreaMaxSize.x, -1, 1),
												Mathf.Clamp(head2Touch.y / headAreaMaxSize.y, -1, 1));

			portrait.SetControlParamVector2(controlParamName_EyeDirection, eyeParam);
			portrait.SetControlParamVector2(controlParamName_HeadDirection, headParam);

			lastEyeParam = eyeParam;
			lastHeadParam = headParam;

			if(!touchParticle.isPlaying)
			{
				touchParticle.Play();
			}

			//touchParticle.transform.position = new Vector3(touchPos.x, touchPos.y, touchParticle.transform.position.z);
			handGroup.position = new Vector3(touchPos.x, touchPos.y, handGroup.position.z);
			handMesh_Released.enabled = false;
			handMesh_Pressed.enabled = true;

		}
		else
		{
			if(touchParticle.isPlaying)
			{
				touchParticle.Stop();
			}

			if(Input.mousePresent)
			{

				Vector2 mousePosW = targetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, portrait.transform.position.z));

				handGroup.position = new Vector3(mousePosW.x, mousePosW.y, handGroup.position.z);
				handMesh_Released.enabled = true;
				handMesh_Pressed.enabled = false;
			}
			else
			{
				handMesh_Released.enabled = false;
				handMesh_Pressed.enabled = false;
			}

			if(isEyeReturn)
			{
				
				eyeReturnTime += Time.deltaTime;
				if(eyeReturnTime < eyeReturnTimeLength)
				{
					apControlParam controlParam_Eye = portrait.GetControlParam(controlParamName_EyeDirection);
					apControlParam controlParam_Head = portrait.GetControlParam(controlParamName_HeadDirection);

					float itp = 1.0f - (eyeReturnTime / eyeReturnTimeLength);
					portrait.SetControlParamVector2(controlParamName_EyeDirection, lastEyeParam * itp +  controlParam_Eye._vec2_Cur * (1-itp));
					portrait.SetControlParamVector2(controlParamName_HeadDirection, lastHeadParam * itp +  controlParam_Head._vec2_Cur * (1-itp));
				}
				else
				{
					isEyeReturn = false;
					eyeReturnTime = 0.0f;
				}
			}
		}

		if(status == Status.Smile)
		{
			smileTime += Time.deltaTime;
			if(smileTime > 5.0f && !isTouched)
			{
				ChangeStatus(Status.Normal);
			}
		}
		else if(status == Status.Embarrassed)
		{
			embarrassedTime += Time.deltaTime;
			if(embarrassedTime > 1.0f)
			{
				portrait.PlayQueued("Idle Normal");
				
				status = Status.Normal;
				embarrassedTime = 0.0f;
			}
		}
		else if(status == Status.SetAngry)
		{
			setAngryTime += Time.deltaTime;
			if(setAngryTime > 1.0f)
			{
				portrait.PlayQueued("Idle Angry");
				
				status = Status.Angry;
				setAngryTime = 0.0f;
			}
		}

		isTouchedPrevFrame = isTouched;
		if(isTouchDown && isTouched)
		{
			isTouchDown = false;
		}
	}

	private void ChangeStatus(Status nextStatus)
	{
		switch (nextStatus)
		{
			case Status.Normal:
				if(nextStatus != status)
				{
					portrait.CrossFade("Idle Normal", 0.3f);
				}
				break;

			case Status.Smile:
				smileTime = 0.0f;

				if(nextStatus != status)
				{
					portrait.CrossFade("Idle Smile", 0.3f);
				}
				break;

			case Status.Angry:
				if(nextStatus != status)
				{
					portrait.CrossFade("Idle Angry", 0.3f);
				}
				break;

			case Status.Embarrassed:
				portrait.Play("Embarrass", 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopAllLayers, true);
				embarrassedTime = 0.0f;
				break;

				case Status.SetAngry:
				portrait.Play("Angry", 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopAllLayers, true);
				setAngryTime = 0.0f;
				break;
		}
		status = nextStatus;
	}
}

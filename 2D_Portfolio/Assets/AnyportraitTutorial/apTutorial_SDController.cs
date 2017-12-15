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
using AnyPortrait;

public class apTutorial_SDController : MonoBehaviour
{
	// Target AnyPortrait
	public apPortrait portrait;

	private bool isPlaying = false;
	void Start ()
	{
		
	}
	
	void Update ()
	{
		if(Input.GetMouseButtonDown(0))
		{
			if (!isPlaying)
			{
				portrait.CrossFade("Idle", 0.3f);
				isPlaying = true;
			}
			else
			{
				portrait.StopAll(0.3f);
				isPlaying = false;
			}
		}
	}
}

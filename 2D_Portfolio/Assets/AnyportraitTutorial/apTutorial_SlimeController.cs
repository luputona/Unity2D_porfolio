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

public class apTutorial_SlimeController : MonoBehaviour
{
	// Target AnyPortrait
	public apPortrait portrait;

	// Parameter Values
	private int eyeShape = 0;
	private int mouthShape = 0;
	private float verticalPosition = 0.0f;


	void Start () { }
	
	
	void Update ()
	{
		//"Eye Shape" (0, 1, 2, 3 int)
		if(Input.GetKeyDown(KeyCode.E))
		{
			eyeShape++;
			if(eyeShape > 3) { eyeShape = 0; }

			portrait.SetControlParamInt("Eye Shape", eyeShape);
		}

		//"Mouth Shape" (0, 1, 2, int)
		if(Input.GetKeyDown(KeyCode.M))
		{
			mouthShape++;
			if(mouthShape > 2) { mouthShape = 0; }

			portrait.SetControlParamInt("Mouth Shape", mouthShape);
		}

		//"Vertical Position" (0 ~ 1 float)
		if(Input.GetKey(KeyCode.UpArrow))
		{
			// Move Upward
			verticalPosition += 2 * Time.deltaTime;
			if(verticalPosition > 1) { verticalPosition = 1; }

			portrait.SetControlParamFloat("Vertical Position", verticalPosition);
		}
		else if(Input.GetKey(KeyCode.DownArrow))
		{
			// Move Downward
			verticalPosition -= 2 * Time.deltaTime;
			if(verticalPosition < 0) { verticalPosition = 0; }

			portrait.SetControlParamFloat("Vertical Position", verticalPosition);
		}
	}
}

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

	[Serializable]
	public class apTextureData
	{
		// Members
		//-------------------------------------------
		public int _uniqueID = -1;
		public string _name = "";
		public Texture2D _image = null;

		public int _width = 0;
		public int _height = 0;

		public string _assetFullPath = "";
		public bool _isPSDFile = false;


		// Init
		//-------------------------------------------
		public apTextureData(int index)
		{
			_uniqueID = index;
		}

		public void ReadyToEdit(apPortrait portrait)
		{
			portrait.RegistUniqueID(apIDManager.TARGET.Texture, _uniqueID);
		}

		// Get / Set
		//-------------------------------------------
		public void SetImage(Texture2D image, int width, int height)
		{
			_image = image;
			_name = image.name;

			_width = width;
			_height = height;
		}
	}

}
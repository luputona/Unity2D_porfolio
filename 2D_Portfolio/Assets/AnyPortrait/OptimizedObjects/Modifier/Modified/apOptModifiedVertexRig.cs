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
	/// apModifiedVertexRig의 Opt 버전
	/// </summary>
	[Serializable]
	public class apOptModifiedVertexRig
	{
		// Members
		//-----------------------------------------------
		public int _vertexUniqueID = -1;
		public int _vertIndex = -1;

		public apOptMesh _mesh = null;

		[Serializable]
		public class OptWeightPair
		{
			public int _boneID = -1;

			[SerializeField]
			public apOptBone _bone = null;

			public int _meshGroupID = -1;

			[SerializeField]
			public apOptTransform _meshGroup = null;

			public float _weight = 0.0f;

			public OptWeightPair(apModifiedVertexRig.WeightPair srcWeightPair)
			{
				_boneID = srcWeightPair._boneID;
				_meshGroupID = srcWeightPair._meshGroupID;
				_weight = srcWeightPair._weight;
			}

			public bool Link(apPortrait portrait)
			{
				_meshGroup = portrait.GetOptTransformAsMeshGroup(_meshGroupID);
				if (_meshGroup == null)
				{
					Debug.LogError("VertRig Bake 실패 : MeshGroup을 찾을 수 없다. [" + _meshGroupID + "]");
					return false;
				}

				_bone = _meshGroup.GetBone(_boneID);
				if (_bone == null)
				{
					Debug.LogError("VertRig Bake 실패 : Bone을 찾을 수 없다. [" + _boneID + "]");
					return false;
				}

				return true;
			}
		}

		[SerializeField]
		public List<OptWeightPair> _weightPairs = new List<OptWeightPair>();



		// Init
		//-----------------------------------------------------------
		public apOptModifiedVertexRig()
		{

		}

		public bool Bake(apModifiedVertexRig srcModVert, apOptMesh mesh, apPortrait portrait)
		{
			_vertexUniqueID = srcModVert._vertexUniqueID;
			_vertIndex = srcModVert._vertIndex;
			_mesh = mesh;

			_weightPairs.Clear();

			for (int i = 0; i < srcModVert._weightPairs.Count; i++)
			{
				apModifiedVertexRig.WeightPair srcWeightPair = srcModVert._weightPairs[i];
				OptWeightPair optWeightPair = new OptWeightPair(srcWeightPair);
				optWeightPair.Link(portrait);

				_weightPairs.Add(optWeightPair);
			}



			return true;
		}
	}
}
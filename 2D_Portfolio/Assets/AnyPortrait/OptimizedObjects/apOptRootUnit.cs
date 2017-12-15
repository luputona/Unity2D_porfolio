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
using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{

	public class apOptRootUnit : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		public apPortrait _portrait = null;

		public apOptTransform _rootOptTransform = null;

		[HideInInspector]
		public Transform _transform = null;

		// Init
		//------------------------------------------------
		void Awake()
		{

		}

		void Start()
		{

		}

		// Update
		//------------------------------------------------
		void Update()
		{

		}

		void LateUpdate()
		{

		}

		// Functions
		//------------------------------------------------
		public void UpdateTransforms(float tDelta)
		{
			if (_rootOptTransform == null)
			{
				return;
			}


			//추가
			//본 업데이트 1단계
			_rootOptTransform.ReadyToUpdateBones();


#if UNITY_EDITOR
			Profiler.BeginSample("Root Unit - Update Modifier");
#endif
			//1. Modifer부터 업데이트 (Pre)
			_rootOptTransform.UpdateModifier_Pre(tDelta);

#if UNITY_EDITOR
			Profiler.EndSample();
#endif

#if UNITY_EDITOR
			Profiler.BeginSample("Root Unit - Calculate");
#endif
			//2. 실제로 업데이트
			_rootOptTransform.ReadyToUpdate();
			_rootOptTransform.UpdateCalculate_Pre();//Post 작성할 것

#if UNITY_EDITOR
			Profiler.EndSample();
#endif

			//Bone World Matrix Update
			_rootOptTransform.UpdateBonesWorldMatrix();

			//Modifier 업데이트 (Post)
			_rootOptTransform.UpdateModifier_Post(tDelta);

			_rootOptTransform.UpdateCalculate_Post();//Post Calculate

		}


		public void Show()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.Show(true);
		}



		public void Hide()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.Hide(true);
		}



		// Get / Set
		//------------------------------------------------
	}

}
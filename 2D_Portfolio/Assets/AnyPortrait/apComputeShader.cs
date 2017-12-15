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
using UnityEngine.Profiling;
using System.IO;

using AnyPortrait;

namespace AnyPortrait
{

	public class apComputeShader
	{
		// Single Tone
		//------------------------------------------------
		private static apComputeShader _instance = new apComputeShader();
		public static apComputeShader I { get { return _instance; } }


		// Members
		//------------------------------------------------

		private ComputeShader _cShader_Editor = null;
		private ComputeShader _cShader_Opt = null;


		//Vertex 처리용 - Editor
		private ComputeBuffer _inputBuffer = null;
		private ComputeBuffer _outputBuffer = null;

		private int _kernel_Editor = -1;
		private int _kernel_Opt = -1;
		private float[] _matrix_Static_Vert2Mesh = new float[9];
		private float[] _matrix_Static_Vert2MeshInverse = new float[9];//<<이건 Opt 전용
		private float[] _matrix_MeshTransform = new float[9];
		private float[] _matrix_MeshTransformInverse = new float[9];//<<이건 Editor 전용



		#region [미사용 코드] : 처음 작성된 Compute Shader의 파라미터 (느려서 안씀)
		////Size
		////int = 4
		////float = 4
		////Vector2 = 4 * 2 = 8
		////Vector3 = 4 * 3 = 12 
		////apMatrix3x3 = 4 * 16 = 64
		//private struct InputVertexStruct_Editor
		//{
		//	public Vector3 _posL;					//12
		//	public float _riggingWeight;			//4
		//	public Vector3 _posRigging;				//12
		//	public apMatrix3x3 _mtx_Cal_VertLocal;	//64
		//	public apMatrix3x3 _mtx_Cal_VertWorld;    //64
		//	public apMatrix3x3 _mtx_World;			//64

		//	//Size : 156 + 64 = 220 (임시)

		//}

		//private struct OutputVertexStruct_Editor
		//{
		//	public Vector3 _posWorld3;			//12
		//	public Vector2 _posWorld2;			//8
		//	public Vector3 _posLocalOnMesh;     //12
		//	public apMatrix3x3 _mtxWorld;			//64

		//	//Size : 96
		//} 

		//private InputVertexStruct_Editor[] _inputVerts_Editor = null;
		//private OutputVertexStruct_Editor[] _outputVerts_Editor = null;
		#endregion



		private bool _isSupport = false;
		private bool _isEnabled = true;


		public struct InputVertexStruct_Opt
		{
			public Vector3 _posL;                   //12
			public float _riggingWeight;            //4
			public Vector3 _posRigging;             //12
			public apMatrix3x3 _mtx_Cal_VertLocal;  //64
			public apMatrix3x3 _mtx_Cal_VertWorld;    //64

			//Size : 156
		}

		public struct OutputVertexStruct_Opt
		{
			public Vector3 _posWorld3;          //12
			public Vector2 _posWorld2;          //8
			public Vector3 _posLocalOnMesh;     //12

			//Size : 32
		}

		private InputVertexStruct_Opt[] _inputVerts_Opt = null;
		private OutputVertexStruct_Opt[] _outputVerts_Opt = null;

		//Size
		//int = 4
		//float = 4
		//Vector2 = 4 * 2 = 8
		//Vector3 = 4 * 3 = 12 
		//apMatrix3x3 = 4 * 9 = 36
		//Matrix4x4 = 4 * 16 = 64
		private struct InputVertexStructBatch_Editor
		{
			public Vector2 _posL;                           //8
			public float _riggingWeight;                    //4
			public Vector2 _posRigging;                     //8
			public Vector2 _cal_VertLocal;                  //8
			public Vector2 _cal_VertWorld;                  //8
			public apMatrix3x3 _matrix_Static_Vert2Mesh;        //36
			public apMatrix3x3 _matrix_MeshTransform;           //36
			public apMatrix3x3 _matrix_MeshTransformInverse;    //36
																//public float[] _matrix_Static_Vert2Mesh;
																//public float[] _matrix_MeshTransform;
																//public float[] _matrix_MeshTransformInverse;
																//Size : 8 + 4 + 8 + 8 + 8 + 36 + 36 + 36 = 144

		}

		public class RenderRequest
		{
			public apRenderUnit _renderUnit;
			public apRenderVertex _renderVert;
			public apMatrix3x3 _matrix_Static_Vert2Mesh;
			public apMatrix3x3 _matrix_MeshTransform;
			public apMatrix3x3 _matrix_MeshTransformInverse;
			public int _iVert = 0;
			public Vector2 _posL;                   //12
			public float _riggingWeight;            //4
			public Vector2 _posRigging;             //12
			public Vector2 _cal_VertLocal;  //64
			public Vector2 _cal_VertWorld;    //64

			public RenderRequest(apRenderUnit renderUnit,
								apRenderVertex renderVert,
								apMatrix3x3 matrix_Static_Vert2Mesh,
								apMatrix3x3 matrix_MeshTransform,
								apMatrix3x3 matrix_MeshTransformInverse,
								int iVert,
								Vector2 posL,
								float riggingWeight,
								Vector2 posRigging,
								Vector2 cal_VertLocal,
								Vector2 cal_VertWorld
								)
			{
				_renderUnit = renderUnit;
				_renderVert = renderVert;
				_matrix_Static_Vert2Mesh = matrix_Static_Vert2Mesh;
				_matrix_MeshTransform = matrix_MeshTransform;
				_matrix_MeshTransformInverse = matrix_MeshTransformInverse;
				_iVert = iVert;
				_posL = posL;
				_riggingWeight = riggingWeight;
				_posRigging = posRigging;
				_cal_VertLocal = cal_VertLocal;
				_cal_VertWorld = cal_VertWorld;
			}
		}
		public List<RenderRequest> _renderRequest = new List<RenderRequest>();



		// Init
		//------------------------------------------------
		private apComputeShader()
		{
			_isSupport = SystemInfo.supportsComputeShaders;
			_isEnabled = false;
			_renderRequest.Clear();
		}


		public void ClearRenderRequest()
		{
			_renderRequest.Clear();
		}

		public bool IsComputeShaderNeedToLoad_Editor
		{
			get
			{
				if (!IsSupport)
				{
					return false;
				}
				return (_cShader_Editor == null);
			}
		}

		public bool IsComputeShaderNeedToLoad_Opt
		{
			get
			{
				if (!IsSupport)
				{
					return false;
				}
				return (_cShader_Opt == null);
			}
		}

		public bool SetComputeShaderAsset_Editor(ComputeShader computeShader)
		{
			if (!IsSupport)
			{
				return false;
			}
			_cShader_Editor = computeShader;
			MakeComputeShader_Editor();
			return true;
		}

		public bool SetComputeShaderAsset_Opt(ComputeShader computeShader)
		{
			if (!IsSupport)
			{
				return false;
			}
			_cShader_Opt = computeShader;
			if (computeShader == null)
			{
				Debug.LogError("로드 실패");
			}

			MakeComputeShader_Opt();
			return true;
		}

		private void MakeComputeShader_Editor()
		{
			if (!IsSupport)
			{ return; }
			//_kernel_Editor = _cShader_Editor.FindKernel("CSMain_apcShaderVertWT");
			_kernel_Editor = _cShader_Editor.FindKernel("CSMain_apcShaderVertWTNew");

		}

		private void MakeComputeShader_Opt()
		{
			if (!IsSupport)
			{ return; }
			_kernel_Opt = _cShader_Opt.FindKernel("CSMain_apcShaderOptVertWT");
		}

		public void ReleaseBuffer()
		{
			if (_inputBuffer != null)
			{
				_inputBuffer.Release();
			}
			if (_outputBuffer != null)
			{
				_outputBuffer.Release();
			}
		}

		// Functions
		//------------------------------------------------

		#region [미사용 코드 : Batch없이 각각 렌더링. 느렸다..]
		//public bool Compute_Editor(List<apRenderVertex> renderVerts, apMatrix3x3 matrix_Static_Vert2Mesh, apMatrix3x3 matrix_MeshTransform)
		//{
		//	if(!IsSupport)
		//	{
		//		return false;
		//	}

		//	Matrix2Floats(matrix_Static_Vert2Mesh, ref _matrix_Static_Vert2Mesh);
		//	Matrix2Floats(matrix_MeshTransform, ref _matrix_MeshTransform);
		//	Matrix2Floats(matrix_MeshTransform.inverse, ref _matrix_MeshTransformInverse);

		//	//Matrix2Floats(apMatrix3x3.identity, _matrix_Static_Vert2Mesh);
		//	//Matrix2Floats(apMatrix3x3.identity, _matrix_MeshTransform);
		//	//Matrix2Floats(apMatrix3x3.identity, _matrix_MeshTransformInverse);


		//	_cShader_Editor.SetFloats("_mtx_Static_Vert2Mesh", _matrix_Static_Vert2Mesh);
		//	_cShader_Editor.SetFloats("_mtx_MeshTransform", _matrix_MeshTransform);
		//	_cShader_Editor.SetFloats("_mtx_MeshTransformInverse", _matrix_MeshTransformInverse);

		//	_inputVerts_Editor = new InputVertexStruct_Editor[renderVerts.Count];
		//	_outputVerts_Editor = new OutputVertexStruct_Editor[renderVerts.Count];

		//	int nRenderVerts = renderVerts.Count;
		//	apRenderVertex renderVert = null;
		//	for (int i = 0; i < nRenderVerts; i++)
		//	{
		//		renderVert = renderVerts[i];
		//		_inputVerts_Editor[i]._posL = renderVert._vertex._pos;
		//		_inputVerts_Editor[i]._posRigging = renderVert._pos_Rigging;

		//		_inputVerts_Editor[i]._riggingWeight = renderVert._weight_Rigging;

		//		_inputVerts_Editor[i]._mtx_Cal_VertLocal = renderVert._matrix_Cal_VertLocal;
		//		_inputVerts_Editor[i]._mtx_Cal_VertWorld = renderVert._matrix_Cal_VertWorld;
		//		_inputVerts_Editor[i]._mtx_World =		renderVert._matrix_Cal_VertWorld *
		//										matrix_MeshTransform *
		//										renderVert._matrix_Cal_VertLocal *
		//										matrix_Static_Vert2Mesh;
		//	}
		//	_inputBuffer = new ComputeBuffer(nRenderVerts, 220);//원래는 156
		//	_outputBuffer = new ComputeBuffer(nRenderVerts, 96);

		//	_inputBuffer.SetData(_inputVerts_Editor);
		//	//_outputBuffer_Editor.SetData(_outputVerts);

		//	_cShader_Editor.SetBuffer(_kernel_Editor, "_inputBuffer", _inputBuffer);
		//	_cShader_Editor.SetBuffer(_kernel_Editor, "_outputBuffer", _outputBuffer);

		//	_cShader_Editor.Dispatch(_kernel_Editor, 32, 32, 1);

		//	_outputBuffer.GetData(_outputVerts_Editor);

		//	for (int i = 0; i < nRenderVerts; i++)
		//	{
		//		renderVert = renderVerts[i];
		//		renderVert.CalculateByComputeShader(
		//			//_outputVerts_Editor[i]._posWorld3,
		//			_outputVerts_Editor[i]._posWorld2,
		//			_outputVerts_Editor[i]._posLocalOnMesh,//<<Vec2로 바꿀 것
		//			_outputVerts_Editor[i]._mtxWorld);

		//		//if (i == 5)
		//		//{
		//		//	//Debug.Log("Compute\n" + matrix_MeshTransform + "\n>>\n" + _outputVerts[i]._mtxWorld);
		//		//	Debug.Log("Compute\n" + _inputVerts[i]._mtx_World + "\n>>\n" + _outputVerts[i]._mtxWorld);
		//		//	//Debug.Log("Compute\n" + _inputVerts[i]._mtx_Cal_VertWorld + "\n>>\n" + _outputVerts[i]._mtxWorld);
		//		//}

		//	}

		//	_inputBuffer.Release();
		//	_outputBuffer.Release();

		//	_inputBuffer = null;
		//	_outputBuffer = null;

		//	return true;
		//} 
		#endregion


		public bool AddRenderRequest(apRenderUnit renderUnit,
										apRenderUnit.ComputedVert_Input[] inputVerts,
										List<apRenderVertex> renderVerts,
										apMatrix3x3 matrix_Static_Vert2Mesh,
										apMatrix3x3 matrix_MeshTransform,
										float riggingWeight)
		{
			if (!IsSupport)
			{
				return false;
			}

			apMatrix3x3 invMeshTransform = matrix_MeshTransform.inverse;
			int nVert = renderVerts.Count;
			for (int i = 0; i < nVert; i++)
			{
				_renderRequest.Add(new RenderRequest(renderUnit,
					renderVerts[i],
					matrix_Static_Vert2Mesh,
					matrix_MeshTransform,
					invMeshTransform,
					i,
					inputVerts[i]._posL,
					riggingWeight,
					inputVerts[i]._posRigging,
					inputVerts[i]._calVertLocal,
					inputVerts[i]._calVertWorld
					));
			}

			return true;
		}



		/// <summary>
		/// 저장된 Render 요청을 한번에 수행하자
		/// </summary>
		public void ComputeBatch_Editor()
		{
			if (!IsSupport)
			{
				return;
			}
			if (_renderRequest.Count == 0)
			{
				return;
			}

			int nVertTotal = _renderRequest.Count;
			int threadGroupSize = 32;
			//(NumThread * threadGroupSize) ^ 2 >= nVertTotal
			//(NumThread * threadGroupSize) >= sqrt(nVertTotal)
			//NumThread = ((sqrt(nVertTotal) + 1) / threadGroupSize) + 1;

			int nThreadGroup = (int)((float)(Mathf.Sqrt(nVertTotal) + 1) / (float)threadGroupSize) + 1;
			nThreadGroup = 32;

			InputVertexStructBatch_Editor[] inputBatch = new InputVertexStructBatch_Editor[nVertTotal];
			apRenderUnit.ComputedVert_Output[] outputBatch = new apRenderUnit.ComputedVert_Output[nVertTotal];
			for (int i = 0; i < nVertTotal; i++)
			{
				inputBatch[i]._posL = _renderRequest[i]._posL;
				inputBatch[i]._riggingWeight = _renderRequest[i]._riggingWeight;
				inputBatch[i]._posRigging = _renderRequest[i]._posRigging;
				inputBatch[i]._cal_VertLocal = _renderRequest[i]._cal_VertLocal;
				inputBatch[i]._cal_VertWorld = _renderRequest[i]._cal_VertWorld;
				//inputBatch[i]._matrix_Static_Vert2Mesh = new float[9];
				//inputBatch[i]._matrix_MeshTransform = new float[9];
				//inputBatch[i]._matrix_MeshTransformInverse = new float[9];

				//Matrix2Floats(_renderRequest[i]._matrix_Static_Vert2Mesh, ref inputBatch[i]._matrix_Static_Vert2Mesh);
				//Matrix2Floats(_renderRequest[i]._matrix_MeshTransform, ref inputBatch[i]._matrix_MeshTransform);
				//Matrix2Floats(_renderRequest[i]._matrix_MeshTransformInverse, ref inputBatch[i]._matrix_MeshTransformInverse);

				inputBatch[i]._matrix_Static_Vert2Mesh = _renderRequest[i]._matrix_Static_Vert2Mesh;
				inputBatch[i]._matrix_MeshTransform = _renderRequest[i]._matrix_MeshTransform;
				inputBatch[i]._matrix_MeshTransformInverse = _renderRequest[i]._matrix_MeshTransformInverse;
			}

			_inputBuffer = new ComputeBuffer(nVertTotal, 144);
			_outputBuffer = new ComputeBuffer(nVertTotal, 56);

			_inputBuffer.SetData(inputBatch);

			_cShader_Editor.SetBuffer(_kernel_Editor, "_inputBuffer", _inputBuffer);
			_cShader_Editor.SetBuffer(_kernel_Editor, "_outputBuffer", _outputBuffer);


			_cShader_Editor.Dispatch(_kernel_Editor, nThreadGroup, nThreadGroup, 1);

			_outputBuffer.GetData(outputBatch);

			RenderRequest request = null;
			for (int i = 0; i < nVertTotal; i++)
			{
				request = _renderRequest[i];

				request._renderVert.CalculateByComputeShader_New(
					outputBatch[i]._posWorld2,
					outputBatch[i]._posLocalOnMesh,
					outputBatch[i]._matrix_World,
					request._matrix_MeshTransform,
					request._cal_VertLocal,
					request._cal_VertWorld,
					request._riggingWeight,
					request._posRigging
					);
			}

			_inputBuffer.Release();
			_outputBuffer.Release();

			_inputBuffer = null;
			_outputBuffer = null;

			_renderRequest.Clear();

		}


		public bool Compute_Opt(apOptRenderVertex[] renderVerts,
								apMatrix3x3 matrix_Static_Vert2Mesh,
								apMatrix3x3 matrix_Static_Vert2Mesh_Inverse,
								apMatrix3x3 matrix_MeshTransform,
								ref Vector3[] targetVertList)
		{
			if (!IsSupport)
			{
				return false;
			}

			Matrix2Floats(matrix_Static_Vert2Mesh, ref _matrix_Static_Vert2Mesh);
			Matrix2Floats(matrix_MeshTransform, ref _matrix_MeshTransform);
			Matrix2Floats(matrix_Static_Vert2Mesh_Inverse, ref _matrix_Static_Vert2MeshInverse);


			_cShader_Opt.SetFloats("_mtx_Static_Vert2Mesh", _matrix_Static_Vert2Mesh);
			_cShader_Opt.SetFloats("_mtx_MeshTransform", _matrix_MeshTransform);
			_cShader_Opt.SetFloats("_mtx_Static_Vert2MeshInverse", _matrix_Static_Vert2MeshInverse);

			_inputVerts_Opt = new InputVertexStruct_Opt[renderVerts.Length];
			_outputVerts_Opt = new OutputVertexStruct_Opt[renderVerts.Length];

			int nRenderVerts = renderVerts.Length;
			apOptRenderVertex renderVert = null;
			for (int i = 0; i < nRenderVerts; i++)
			{
				renderVert = renderVerts[i];
				_inputVerts_Opt[i]._posL = renderVert._pos_Local;
				_inputVerts_Opt[i]._posRigging = renderVert._pos_Rigging;

				_inputVerts_Opt[i]._riggingWeight = renderVert._weight_Rigging;

				_inputVerts_Opt[i]._mtx_Cal_VertLocal = renderVert._matrix_Cal_VertLocal;
				_inputVerts_Opt[i]._mtx_Cal_VertWorld = renderVert._matrix_Cal_VertWorld;
			}
			_inputBuffer = new ComputeBuffer(nRenderVerts, 156);
			_outputBuffer = new ComputeBuffer(nRenderVerts, 32);

			_inputBuffer.SetData(_inputVerts_Opt);

			_cShader_Opt.SetBuffer(_kernel_Opt, "_inputBuffer", _inputBuffer);
			_cShader_Opt.SetBuffer(_kernel_Opt, "_outputBuffer", _outputBuffer);

			_cShader_Opt.Dispatch(_kernel_Opt, 32, 32, 1);

			_outputBuffer.GetData(_outputVerts_Opt);

			for (int i = 0; i < nRenderVerts; i++)
			{
				renderVert = renderVerts[i];
				renderVert.CalculateByComputeShader(ref _outputVerts_Opt[i]);
				targetVertList[i] = renderVert._vertPos3_LocalUpdated;
			}

			_inputBuffer.Release();
			_outputBuffer.Release();

			_inputBuffer = null;
			_outputBuffer = null;

			return true;
		}



		private void Matrix2Floats(apMatrix3x3 srcMatrix, ref float[] targetFloats)
		{
			//이거 Row/Column 순서가 바뀔 수도 있을 것 같다.;;
			targetFloats[0] = srcMatrix._m00;
			targetFloats[1] = srcMatrix._m01;
			targetFloats[2] = srcMatrix._m02;

			targetFloats[3] = srcMatrix._m10;
			targetFloats[4] = srcMatrix._m11;
			targetFloats[5] = srcMatrix._m12;

			targetFloats[6] = srcMatrix._m20;
			targetFloats[7] = srcMatrix._m21;
			targetFloats[8] = srcMatrix._m22;

			//targetFloats[0] = srcMatrix[0, 0];
			//targetFloats[1] = srcMatrix[1, 0];
			//targetFloats[2] = srcMatrix[2, 0];
			//targetFloats[3] = srcMatrix[3, 0];

			//targetFloats[4] = srcMatrix[0, 1];
			//targetFloats[5] = srcMatrix[1, 1];
			//targetFloats[6] = srcMatrix[2, 1];
			//targetFloats[7] = srcMatrix[3, 1];

			//targetFloats[8] = srcMatrix[0, 2];
			//targetFloats[9] = srcMatrix[1, 2];
			//targetFloats[10] = srcMatrix[2, 2];
			//targetFloats[11] = srcMatrix[3, 2];

			//targetFloats[12] = srcMatrix[0, 3];
			//targetFloats[13] = srcMatrix[1, 3];
			//targetFloats[14] = srcMatrix[2, 3];
			//targetFloats[15] = srcMatrix[3, 3];
		}


		// Get / Set
		//------------------------------------------------
		/// <summary>
		/// Compute Shader를 지원하는가
		/// </summary>
		public bool IsSupport
		{
			//get { return _isSupport; }
			get { return false; }//일단 무조건 막자
		}

		/// <summary>
		/// Compute Shader를 이용하여 렌더링을 하는가 (IsSupport && IsEnabled)
		/// </summary>
		public bool IsComputable
		{
			//get { return IsSupport && _isEnabled; }
			get { return false; }
		}

		public bool IsComputable_Opt
		{
			//get { return IsSupport && _cShader_Opt != null; }
			get { return false; }
		}

		public bool IsEnabled
		{
			get { return _isEnabled; }
		}

		public void SetEnable(bool isEnabled)
		{
			_isEnabled = isEnabled;
		}
	}

}
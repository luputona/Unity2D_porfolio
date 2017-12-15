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

	public class apOptMesh : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		public apPortrait _portrait = null;

		public int _uniqueID = -1;//meshID가 아니라 meshTransform의 ID를 사용한다.

		public apOptTransform _parentTransform;

		// Components
		//------------------------------------------------
		[HideInInspector]
		public MeshFilter _meshFilter = null;

		[HideInInspector]
		public MeshRenderer _meshRenderer = null;

		[HideInInspector]
		public Material _material = null;

		[HideInInspector]
		public Texture2D _texture = null;



		[SerializeField]
		public Mesh _mesh = null;

		// Vertex 값들
		//apRenderVertex에 해당하는 apOptRenderVertex의 배열 (리스트 아닙니더)로 저장한다.

		//<기본값>
		[SerializeField]
		private apOptRenderVertex[] _renderVerts = null;



		//RenderVert의 
		[SerializeField]
		private Vector3[] _vertPositions = null;

		[SerializeField]
		private Vector2[] _vertUVs = null;

		[SerializeField]
		private int[] _vertUniqueIDs = null;

		[SerializeField]
		private int[] _vertTris = null;

		[SerializeField]
		private int _nVert = 0;
		//TODO : Vertex에 직접 값을 입력하는건 ModVert에서 하자


		public apOptRenderVertex[] RenderVertices { get { return _renderVerts; } }
		public Vector3[] LocalVertPositions { get { return _vertPositions; } }


		//<업데이트>
		[SerializeField]
		private Vector3[] _vertPositions_Updated = null;

		[SerializeField]
		private Vector3[] _vertPositions_Local = null;

		//[SerializeField]
		//private Texture2D _texture_Updated = null;

		[SerializeField, HideInInspector]
		public Transform _transform = null;

		private bool _isInit = false;

		[SerializeField]
		private Vector2 _pivotPos = Vector2.zero;

		[NonSerialized]
		private bool _isVisible = false;


		//Mask인 경우
		//Child는 업데이트는 하지만 렌더링은 하지 않는다.
		//렌더링을 하지 않으므로 Mesh 갱신을 하지 않음
		//Parent는 업데이트 후 렌더링은 잠시 보류한다.
		//"통합" Vertex으로 정의된 SubMeshData에서 통합 작업을 거친 후에 Vertex 업데이트를 한다.
		//MaskMesh 업데이트는 Portrait에서 Calculate 후 일괄적으로 한다. (List로 관리한다.)
		public bool _isMaskParent = false;
		public bool _isMaskChild = false;

		//Child인 경우
		public int _clipParentID = -1;
		public apOptMesh _parentOptMesh = null;

		//Parent인 경우
		public int[] _clipChildIDs = null;
		//public apOptMesh[] _childOptMesh = null;

		[NonSerialized]
		private Color _multiplyColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);


		#region [미사용 코드]
		//// Mask를 만들 경우 => 통합 데이터를 만든다.
		////자신을 포함한 서브메시 데이터
		////인덱스를 합쳐야하므로 처리가 다르다.
		//[Serializable]
		//public class SubMeshData
		//{
		//	public int _meshIndex = -1;//Parent는 0 (RGB 각각 1, 2, 3)
		//	public apOptMesh _optMesh = null;
		//	public Material _material = null;
		//	//public Vector3[] _verts_World = null;
		//	//public Vector3[] _verts_Local = null;
		//	//public int[] _triangles = null;
		//	//public Vector2[] _uvs = null;
		//	public int _nVert = 0;
		//	public int _nTri = 0;

		//	public int _vertIndexOffset = 0;
		//	//public Color _color = Color.clear;
		//	public Texture _texture = null;
		//	public bool _isVisible = false;

		//	public SubMeshData(int meshIndex, apOptMesh targetOptMesh, int vertexIndexOffset)
		//	{
		//		_meshIndex = meshIndex;
		//		_optMesh = targetOptMesh;
		//		_material = targetOptMesh._material;

		//		_nVert = targetOptMesh._renderVerts.Length;
		//		_nTri = targetOptMesh._vertTris.Length;

		//		//_verts_World = new Vector3[targetOptMesh._renderVerts.Length];
		//		//_verts_Local = new Vector3[targetOptMesh._renderVerts.Length];
		//		//_triangles = new int[targetOptMesh._vertTris.Length];
		//		//_uvs = new Vector2[targetOptMesh._vertUVs.Length];

		//		//for (int i = 0; i < _verts_World.Length; i++)
		//		//{
		//		//	_verts_World[i] = targetOptMesh._renderVerts[i]._vertPos3_World;
		//		//	_verts_Local[i] = Vector3.zero;//<<이건 계산 후에 적용
		//		//}

		//		//for (int i = 0; i < _triangles.Length; i++)
		//		//{
		//		//	_triangles[i] = targetOptMesh._vertTris[i];
		//		//}

		//		//for (int i = 0; i < _uvs.Length; i++)
		//		//{
		//		//	_uvs[i] = targetOptMesh._vertUVs[i];
		//		//}

		//		//_color = _material.color;
		//		_texture = targetOptMesh._texture;

		//		_vertIndexOffset = vertexIndexOffset;
		//	}


		//	public void SetVisible(bool isVisible)
		//	{
		//		_isVisible = isVisible;
		//	}

		//	public Color MeshColor
		//	{
		//		get
		//		{
		//			return _optMesh.MeshColor;
		//		}
		//	}
		//} 
		#endregion


		#region [미사용 코드]
		//[SerializeField]
		//public SubMeshData[] _subMeshes = null;//<<Parnet일때만 만든다.
		//private const int SUBMESH_BASE = 0;
		//private const int SUBMESH_CLIP1 = 1;
		//private const int SUBMESH_CLIP2 = 2;
		//private const int SUBMESH_CLIP3 = 3;

		//private static Color VertexColor_Base = new Color(0.0f, 0.0f, 0.0f, 1.0f);// Black
		//private static Color VertexColor_Clip1 = new Color(1.0f, 0.0f, 0.0f, 1.0f); //Red
		//private static Color VertexColor_Clip2 = new Color(0.0f, 1.0f, 0.0f, 1.0f); //Green
		//private static Color VertexColor_Clip3 = new Color(0.0f, 0.0f, 1.0f, 1.0f); //Blue

		//[SerializeField]
		//private Vector3[] _vertPosList_ForMask = null;//전체 Vertex 위치 (Local)

		//[SerializeField]
		//private Color[] _vertColorList_ForMask = null;//전체 Vertex의 VertColor (Black - RGB) 
		#endregion

		[SerializeField]
		private Vector3[] _vertPosList_ClippedMerge = null;

		[SerializeField]
		private Color[] _vertColorList_ClippedMerge = null;

		[SerializeField]
		private int _nVertParent = 0;


		public apMatrix3x3 _matrix_Vert2Mesh = apMatrix3x3.identity;
		public apMatrix3x3 _matrix_Vert2Mesh_Inverse = apMatrix3x3.identity;

		[SerializeField]
		public apPortrait.SHADER_TYPE _shaderType = apPortrait.SHADER_TYPE.AlphaBlend;

		[SerializeField]
		public Shader _shaderNormal = null;

		[SerializeField]
		public Shader _shaderClipping = null;

		#region [미사용 코드]
		//private static Color[] ShaderTypeColor = new Color[] {  new Color(1.0f, 0.0f, 0.0f, 0.0f),
		//															new Color(0.0f, 1.0f, 0.0f, 0.0f),
		//															new Color(0.0f, 0.0f, 1.0f, 0.0f),
		//															new Color(0.0f, 0.0f, 0.0f, 1.0f)}; 
		#endregion

		// Init
		//------------------------------------------------
		void Awake()
		{
			_transform = transform;
		}

		void Start()
		{
			InitMesh(false);
		}


		// Bake
		//------------------------------------------------
		public void BakeMesh(Vector3[] vertPositions,
								Vector2[] vertUVs,
								int[] vertUniqueIDs,
								int[] vertTris,
								Vector2 pivotPos,
								apOptTransform parentTransform,
								Texture2D texture,
								apPortrait.SHADER_TYPE shaderType,
								Shader shaderNormal, Shader shaderClipping
			)
		{
			_parentTransform = parentTransform;

			_vertPositions = vertPositions;
			_vertUVs = vertUVs;
			_vertUniqueIDs = vertUniqueIDs;
			_vertTris = vertTris;
			_texture = texture;

			_pivotPos = pivotPos;
			_nVert = _vertPositions.Length;


			transform.localPosition += new Vector3(-_pivotPos.x, -_pivotPos.y, 0.0f);

			_matrix_Vert2Mesh = apMatrix3x3.TRS(new Vector2(-_pivotPos.x, -_pivotPos.y), 0, Vector2.one);
			_matrix_Vert2Mesh_Inverse = _matrix_Vert2Mesh.inverse;

			_shaderType = shaderType;
			_shaderNormal = shaderNormal;
			_shaderClipping = shaderClipping;

			if (_shaderNormal == null)
			{
				Debug.LogError("Shader Normal is Null");
			}
			if (_shaderClipping == null)
			{
				Debug.LogError("Shader Clipping is Null");
			}

			//RenderVert를 만들어주자
			_renderVerts = new apOptRenderVertex[_nVert];
			for (int i = 0; i < _nVert; i++)
			{
				_renderVerts[i] = new apOptRenderVertex(
											_parentTransform, this,
											_vertUniqueIDs[i], i,
											new Vector2(vertPositions[i].x, vertPositions[i].y),
											_vertUVs[i]);

				_renderVerts[i].SetMatrix_1_Static_Vert2Mesh(_matrix_Vert2Mesh);
				_renderVerts[i].SetMatrix_3_Transform_Mesh(parentTransform._matrix_TFResult_WorldWithoutMod.MtrxToSpace);
				_renderVerts[i].Calculate();
			}

			if (_meshFilter == null || _mesh == null)
			{
				_meshFilter = GetComponent<MeshFilter>();
				_mesh = new Mesh();
				_mesh.name = this.name + "_Mesh";

				_meshFilter.sharedMesh = _mesh;
			}

			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();

				//_material = new Material(Shader.Find("AnyPortrait/Transparent/Colored Texture (2X)"));
				//Debug.Log("Opt Shader : " + _shaderNormal.name + "(" + name + ")");
				//_material = new Material(Shader.Find(_shaderName));
				_material = new Material(_shaderNormal);
				_material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1.0f));
				_material.SetTexture("_MainTex", _texture);

				_meshRenderer.sharedMaterial = _material;
			}

			_isMaskParent = false;
			_isMaskChild = false;
			_clipParentID = -1;
			_clipChildIDs = null;

			_vertPositions_Updated = new Vector3[_vertPositions.Length];
			_vertPositions_Local = new Vector3[_vertPositions.Length];
			for (int i = 0; i < _vertPositions.Length; i++)
			{
				//Calculate 전에는 직접 Pivot Pos를 적용해주자 (Calculate에서는 자동 적용)
				//_vertPositions_Updated[i] = _vertPositions[i] + new Vector3(_pivotPos.x, _pivotPos.y, 0);
				//_vertPositions_Updated[i] = _vertPositions[i];
				//_vertPositions_Updated[i] = _vertPositions[i] - new Vector3(_pivotPos.x, _pivotPos.y, 0);
				_vertPositions_Updated[i] = _renderVerts[i]._vertPos3_LocalUpdated;
			}
			//_texture_Updated = _texture;


			_transform = transform;

			InitMesh(true);

			RefreshMesh();
		}



		//-----------------------------------------------------------------------
		//Bake되지 않는 Mesh의 초기화를 호출한다.
		//(Material은 Bake되지 않는다.)
		public void InitMesh(bool isForce)
		{
			if (!isForce && _isInit)
			{
				return;
			}

			_transform = transform;
			if (_mesh == null && _meshFilter != null)
			{
				_mesh = _meshFilter.sharedMesh;
			}
			else if (_meshFilter == null && _mesh == null)
			{
				_meshFilter = GetComponent<MeshFilter>();
				_mesh = new Mesh();
				_mesh.name = this.name + "_Mesh";
			}

			_meshFilter.mesh = _mesh;


			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();
			}

			_meshRenderer.material = _material;

			if (_vertPositions_Updated == null || _vertPositions_Local == null)
			{
				_vertPositions_Updated = new Vector3[_vertPositions.Length];
				_vertPositions_Local = new Vector3[_vertPositions.Length];
				for (int i = 0; i < _vertPositions.Length; i++)
				{
					_vertPositions_Updated[i] = _vertPositions[i];
				}
			}

			//_texture_Updated = _texture;

			_isInit = true;
		}

		//---------------------------------------------------------------------------
		// Mask 관련 초기화
		//---------------------------------------------------------------------------
		public void SetMaskBasicSetting_Parent(List<int> clipChildIDs)
		{
			if (clipChildIDs == null || clipChildIDs.Count == 0)
			{
				return;
			}
			_isMaskParent = true;
			_clipParentID = -1;
			_isMaskChild = false;


			if (_clipChildIDs == null || _clipChildIDs.Length != clipChildIDs.Count)
			{
				_clipChildIDs = new int[clipChildIDs.Count];
			}

			for (int i = 0; i < clipChildIDs.Count; i++)
			{
				_clipChildIDs[i] = clipChildIDs[i];
			}
			//_clipChildIDs[0] = clipChildIDs[0];
			//_clipChildIDs[1] = clipChildIDs[1];
			//_clipChildIDs[2] = clipChildIDs[2];
		}

		public void SetMaskBasicSetting_Child(int parentID)
		{
			_isMaskParent = false;
			_clipParentID = parentID;
			_isMaskChild = true;

			_clipChildIDs = null;
		}


		#region [미사용 코드] Parent 중심의 Clipping
		//public void LinkAsMaskParent(apOptMesh[] childMeshes)
		//{
		//	if(_childOptMesh == null || _childOptMesh.Length != 3)
		//	{
		//		_childOptMesh = new apOptMesh[3];
		//	}

		//	_childOptMesh[0] = childMeshes[0];
		//	_childOptMesh[1] = childMeshes[1];
		//	_childOptMesh[2] = childMeshes[2];


		//	_meshRenderer.enabled = true;

		//	//이제 SubMesh 데이터를 만들어주자
		//	_subMeshes = new SubMeshData[4];
		//	//1. 자기 자신을 넣는다.
		//	int vertexIndexOffset = 0;
		//	_subMeshes[0] = new SubMeshData(SUBMESH_BASE, this, 0);
		//	_subMeshes[0].SetVisible(true);

		//	vertexIndexOffset += _subMeshes[0]._nVert;

		//	//2. 자식 Mesh를 넣는다.
		//	int iChildMesh = 1;
		//	for (int i = 0; i < 3; i++)
		//	{
		//		if(_childOptMesh[i] == null)
		//		{
		//			_subMeshes[iChildMesh] = null;
		//			iChildMesh++;
		//			continue;
		//		}

		//		_subMeshes[iChildMesh] = new SubMeshData(iChildMesh, _childOptMesh[i], vertexIndexOffset);
		//		vertexIndexOffset += _subMeshes[iChildMesh]._nVert;

		//		iChildMesh++;
		//	}

		//	int nTotalVerts = vertexIndexOffset;//<<전체 Vertex의 개수
		//										//이제 전체 Mesh를 만들자

		//	_vertPosList_ForMask = new Vector3[nTotalVerts];
		//	_vertColorList_ForMask = new Color[nTotalVerts];
		//	List<int> vertIndexList_ForMask = new List<int>();
		//	List<Vector2> vertUVs_ForMask = new List<Vector2>();

		//	for (int iSM = 0; iSM < 4; iSM++)
		//	{
		//		SubMeshData subMesh = _subMeshes[iSM];
		//		if(subMesh == null)
		//		{
		//			continue;
		//		}
		//		//Vertex 먼저
		//		Color vertColor = Color.clear;
		//		switch (iSM)
		//		{
		//			case SUBMESH_BASE: vertColor = VertexColor_Base; break;
		//			case SUBMESH_CLIP1: vertColor = VertexColor_Clip1; break;
		//			case SUBMESH_CLIP2: vertColor = VertexColor_Clip2; break;
		//			case SUBMESH_CLIP3: vertColor = VertexColor_Clip3; break;

		//		}
		//		for (int iVert = 0; iVert < subMesh._nVert; iVert++)
		//		{
		//			_vertPosList_ForMask[iVert + subMesh._vertIndexOffset] = subMesh._optMesh._renderVerts[iVert]._pos3_Local;
		//			_vertColorList_ForMask[iVert + subMesh._vertIndexOffset] = vertColor;
		//			vertUVs_ForMask.Add(subMesh._optMesh._vertUVs[iVert]);
		//		}

		//		for (int iTri = 0; iTri < subMesh._nTri; iTri++)
		//		{
		//			vertIndexList_ForMask.Add(subMesh._optMesh._vertTris[iTri] + subMesh._vertIndexOffset);
		//		}
		//	}

		//	_mesh.Clear();
		//	_mesh.vertices = _vertPosList_ForMask;
		//	_mesh.triangles = vertIndexList_ForMask.ToArray();
		//	_mesh.uv = vertUVs_ForMask.ToArray();
		//	_mesh.colors = _vertColorList_ForMask;



		//	//재질도 다시 세팅하자
		//	Color color_Base = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		//	Color color_Clip1 = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		//	Color color_Clip2 = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		//	Color color_Clip3 = new Color(0.0f, 0.0f, 0.0f, 0.0f);

		//	Texture texture_Base = null;
		//	Texture texture_Clip1 = null;
		//	Texture texture_Clip2 = null;
		//	Texture texture_Clip3 = null;



		//	apPortrait.SHADER_TYPE shaderType_Clip1 = apPortrait.SHADER_TYPE.AlphaBlend;
		//	apPortrait.SHADER_TYPE shaderType_Clip2 = apPortrait.SHADER_TYPE.AlphaBlend;
		//	apPortrait.SHADER_TYPE shaderType_Clip3 = apPortrait.SHADER_TYPE.AlphaBlend;

		//	if(_subMeshes[SUBMESH_BASE] != null)
		//	{
		//		if (_subMeshes[SUBMESH_BASE]._isVisible)
		//		{
		//			color_Base = _subMeshes[SUBMESH_BASE].MeshColor;
		//		}
		//		else
		//		{
		//			color_Base = Color.clear;
		//		}
		//		texture_Base = _subMeshes[SUBMESH_BASE]._texture;
		//	}

		//	if(_subMeshes[SUBMESH_CLIP1] != null)
		//	{
		//		if(_subMeshes[SUBMESH_CLIP1]._isVisible)
		//		{	
		//			color_Clip1 = _subMeshes[SUBMESH_CLIP1].MeshColor;
		//		}
		//		else
		//		{
		//			color_Clip1 = Color.clear;
		//		}

		//		texture_Clip1 = _subMeshes[SUBMESH_CLIP1]._texture;
		//		shaderType_Clip1 = _subMeshes[SUBMESH_CLIP1]._optMesh._shaderType;
		//	}

		//	if(_subMeshes[SUBMESH_CLIP2] != null)
		//	{
		//		if(_subMeshes[SUBMESH_CLIP2]._isVisible)
		//		{
		//			color_Clip2 = _subMeshes[SUBMESH_CLIP2].MeshColor;
		//		}
		//		else
		//		{
		//			color_Clip2 = Color.clear;
		//		}

		//		texture_Clip2 = _subMeshes[SUBMESH_CLIP2]._texture;
		//		shaderType_Clip2 = _subMeshes[SUBMESH_CLIP2]._optMesh._shaderType;
		//	}

		//	if(_subMeshes[SUBMESH_CLIP3] != null)
		//	{
		//		if (_subMeshes[SUBMESH_CLIP3]._isVisible)
		//		{
		//			color_Clip3 = _subMeshes[SUBMESH_CLIP3].MeshColor;
		//		}
		//		else
		//		{
		//			color_Clip3 = Color.clear;
		//		}
		//		texture_Clip3 = _subMeshes[SUBMESH_CLIP3]._texture;
		//		shaderType_Clip3 = _subMeshes[SUBMESH_CLIP3]._optMesh._shaderType;
		//	}

		//	//_material = new Material(Shader.Find("AnyPortrait/Transparent/Masked Colored Texture (2X)"));
		//	//Debug.Log("Link Mask Clip - " + _shaderName_Clipping);
		//	//_material = new Material(Shader.Find(_shaderName_Clipping));
		//	_material = new Material(_shaderClipping);

		//	_material.SetColor("_Color", color_Base);
		//	_material.SetColor("_Color1", color_Clip1);
		//	_material.SetColor("_Color2", color_Clip2);
		//	_material.SetColor("_Color3", color_Clip3);

		//	_material.SetTexture("_MainTex", texture_Base);
		//	_material.SetTexture("_ClipTexture1", texture_Clip1);
		//	_material.SetTexture("_ClipTexture2", texture_Clip2);
		//	_material.SetTexture("_ClipTexture3", texture_Clip3);


		//	Debug.Log("Link Clip : " + shaderType_Clip1 + " / " + shaderType_Clip2 + " / " + shaderType_Clip3);

		//	_material.SetColor("_BlendOpt1", ShaderTypeColor[(int)shaderType_Clip1]);
		//	_material.SetColor("_BlendOpt2", ShaderTypeColor[(int)shaderType_Clip2]);
		//	_material.SetColor("_BlendOpt3", ShaderTypeColor[(int)shaderType_Clip3]);

		//	_meshRenderer.sharedMaterial = _material;

		//	RefreshMaskedMesh();

		//	_mesh.RecalculateBounds();
		//	_mesh.RecalculateNormals();


		//} 
		#endregion



		public void LinkAsMaskChild(apOptMesh parentMesh)
		{
			_parentOptMesh = parentMesh;

			//Child라면 Rendering이 되지 않는다.
			//_meshRenderer.enabled = false;
			//> 수정
			//Child도 렌더링을 다 한다.
			_meshRenderer.enabled = true;

			//일반 재질이 아니라, Parent로 부터 Mask를 받는 스텐실 재질로 전환한다.
			_nVertParent = _parentOptMesh._renderVerts.Length;

			int nTotalVert = _nVert + _nVertParent;
			_vertPosList_ClippedMerge = new Vector3[nTotalVert];
			_vertColorList_ClippedMerge = new Color[nTotalVert];
			//Parent -> 자기 자신 순으로 Vertex를 넣는다.
			Color vertColor_Parent = new Color(1.0f, 0.0f, 0.0f, 0.0f);
			Color vertColor_Self = new Color(0.0f, 0.0f, 0.0f, 0.0f);

			List<int> vertIndexList_ClippedMerge = new List<int>();
			List<Vector2> vertUVs_ClippedMerge = new List<Vector2>();

			apOptMesh[] subMeshes = new apOptMesh[] { _parentOptMesh, this };
			int iVertOffset = 0;
			for (int iMesh = 0; iMesh < 2; iMesh++)
			{
				apOptMesh subMesh = subMeshes[iMesh];
				Color vertColor = vertColor_Parent;
				iVertOffset = 0;
				if (iMesh == 1)
				{
					vertColor = vertColor_Self;
					iVertOffset = _nVertParent;
				}

				for (int iVert = 0; iVert < subMesh._renderVerts.Length; iVert++)
				{
					_vertPosList_ClippedMerge[iVert + iVertOffset] = subMesh._renderVerts[iVert]._pos_Local;
					_vertColorList_ClippedMerge[iVert + iVertOffset] = vertColor;
					vertUVs_ClippedMerge.Add(subMesh._vertUVs[iVert]);
				}

				int nTri = subMesh._vertTris.Length;
				for (int iTri = 0; iTri < nTri; iTri++)
				{
					vertIndexList_ClippedMerge.Add(subMesh._vertTris[iTri] + iVertOffset);
				}
			}

			_mesh.Clear();
			_mesh.vertices = _vertPosList_ClippedMerge;
			_mesh.triangles = vertIndexList_ClippedMerge.ToArray();
			_mesh.uv = vertUVs_ClippedMerge.ToArray();
			_mesh.colors = _vertColorList_ClippedMerge;

			_material = new Material(_shaderClipping);
			_material.SetColor("_Color", MeshColor);
			_material.SetTexture("_MainTex", _texture);

			_material.SetColor("_MaskColor", _parentOptMesh.MeshColor);
			_material.SetTexture("_MaskTex", _parentOptMesh._texture);

			_meshRenderer.sharedMaterial = _material;

			_mesh.RecalculateBounds();
			_mesh.RecalculateNormals();

			RefreshClippedMesh();
		}
		//---------------------------------------------------------------------------


		// Update
		//------------------------------------------------
		void Update()
		{

		}

		void LateUpdate()
		{

		}

		// 외부 업데이트
		//------------------------------------------------
		public void ReadyToUpdate()
		{
			//?
		}


		public void UpdateCalculate(bool isRigging, bool isVertexLocal, bool isVertexWorld, bool isVisible)
		{

			if (_isVisible != isVisible)
			{
				if (isVisible)
				{
					Show();
				}
				else
				{
					Hide();//<안보인닷!
				}
			}

			//안보이는건 업데이트하지 말자
			if (isVisible)
			{

				apOptRenderVertex rVert = null;

				//Compute Shader를 써보자
				if (apComputeShader.I.IsComputable_Opt && _portrait._isGPUAccel)
				{
					for (int i = 0; i < _nVert; i++)
					{
						rVert = _renderVerts[i];
						rVert.ReadyToCalculate();

						if (isRigging)
						{
							rVert.SetRigging_0_LocalPosWeight(_parentTransform.CalculatedStack.GetVertexRigging(i),
								_parentTransform.CalculatedStack.GetRiggingWeight());
						}

						if (isVertexLocal)
						{
							rVert.SetMatrix_2_Calculate_VertLocal(_parentTransform.CalculatedStack.GetVertexLocalPos(i));
						}

						//rVert.SetMatrix_3_Transform_Mesh(_parentTransform._matrix_TF_Cal_ToWorld);
						rVert.SetMatrix_3_Transform_Mesh(_parentTransform._matrix_TFResult_World.MtrxToSpace);

						if (isVertexWorld)
						{
							rVert.SetMatrix_4_Calculate_VertWorld(_parentTransform.CalculatedStack.GetVertexWorldPos(i));
						}
					}

					apComputeShader.I.Compute_Opt(_renderVerts, _matrix_Vert2Mesh, _matrix_Vert2Mesh_Inverse, _parentTransform._matrix_TFResult_World.MtrxToSpace, ref _vertPositions_Updated);
				}
				else
				{
					for (int i = 0; i < _nVert; i++)
					{
#if UNITY_EDITOR
						Profiler.BeginSample("Opt Mesh - Update calculate Render Vertices");
#endif
						rVert = _renderVerts[i];

#if UNITY_EDITOR
						Profiler.BeginSample("Opt Mesh - Set Matrix");
#endif
						//rVert.SetMatrix_1_Static_Vert2Mesh(...)<<이건 Bake때 넣어줍니다.

						//리깅 추가
						if (isRigging)
						{
							rVert.SetRigging_0_LocalPosWeight(_parentTransform.CalculatedStack.GetVertexRigging(i),
								_parentTransform.CalculatedStack.GetRiggingWeight());
						}

						if (isVertexLocal)
						{
							rVert.SetMatrix_2_Calculate_VertLocal(_parentTransform.CalculatedStack.GetVertexLocalPos(i));
						}

						//rVert.SetMatrix_3_Transform_Mesh(_parentTransform._matrix_TF_Cal_ToWorld);
						rVert.SetMatrix_3_Transform_Mesh(_parentTransform._matrix_TFResult_World.MtrxToSpace);

						if (isVertexWorld)
						{
							rVert.SetMatrix_4_Calculate_VertWorld(_parentTransform.CalculatedStack.GetVertexWorldPos(i));
						}

#if UNITY_EDITOR
						Profiler.EndSample();
#endif

#if UNITY_EDITOR
						Profiler.BeginSample("Opt Mesh - Matrix Calculate");
#endif

						rVert.Calculate();

						//업데이트 데이터를 넣어준다.
						_vertPositions_Updated[i] = rVert._vertPos3_LocalUpdated;

#if UNITY_EDITOR
						Profiler.EndSample();
#endif

#if UNITY_EDITOR
						Profiler.EndSample();
#endif
					}
				}
			}


			_material.SetColor("_Color", _multiplyColor * _parentTransform._meshColor2X);

#if UNITY_EDITOR
			Profiler.BeginSample("Opt Mesh - Refresh Mesh");
#endif

			//if (!_isMaskParent && !_isMaskChild)
			if (!_isMaskChild)
			{
				//Mask와 관련이 없는 경우만 갱신해준다.
				//메시 갱신
				RefreshMesh();
			}

#if UNITY_EDITOR
			Profiler.EndSample();
#endif
		}


		// Vertex Refresh
		//------------------------------------------------
		public void RefreshMesh()
		{
			//if(_isMaskChild || _isMaskParent)
			//{
			//	return;
			//}

			//변경 -> MaskParent는 그대로 업데이트 가능하고, Child만 따로 업데이트하자
			if (_isMaskChild)
			{
				return;
			}

			//TODO : 이전에 먼저 transform을 수정해야한다.
			//Debug.Log("Refresh Mesh");

			//Transform 제어 -> Vert 제어
			for (int i = 0; i < _nVert; i++)
			{
				//_vertPositions_Local[i] = _transform.InverseTransformPoint(_vertPositions_Updated[i]);
				_vertPositions_Local[i] = _vertPositions_Updated[i];
			}

			_mesh.vertices = _vertPositions_Local;
			_mesh.uv = _vertUVs;
			_mesh.triangles = _vertTris;


		}

		//public void RefreshMaskedMesh()
		//{
		//	if(!_isMaskParent)
		//	{
		//		return;
		//	}

		//	for (int i = 0; i < 4; i++)
		//	{
		//		if (_subMeshes[i] == null || _subMeshes[i]._optMesh == null)
		//		{
		//			continue;
		//		}

		//		for (int iVert = 0; iVert < _subMeshes[i]._nVert; iVert++)
		//		{
		//			_vertPosList_ForMask[iVert + _subMeshes[i]._vertIndexOffset] =
		//				_transform.InverseTransformPoint(
		//						_subMeshes[i]._optMesh._transform.TransformPoint(	
		//							_subMeshes[i]._optMesh._vertPositions_Updated[iVert]
		//						)
		//				);
		//		}
		//		if(i != 0)
		//		{
		//			_material.SetColor("_Color" + (i), _subMeshes[i].MeshColor);
		//			if (_subMeshes[i]._isVisible)
		//			{
		//				_material.SetColor("_BlendOpt" + (i), ShaderTypeColor[(int)_subMeshes[i]._optMesh._shaderType]);
		//			}
		//			else
		//			{
		//				_material.SetColor("_BlendOpt" + (i), ShaderTypeColor[0]);
		//			}
		//		}

		//	}

		//	_mesh.vertices = _vertPosList_ForMask;
		//}

		public void RefreshClippedMesh()
		{
			if (!_isMaskChild)
			{
				return;
			}
			apOptMesh targetMesh = null;
			int iVertOffset = 0;
			for (int i = 0; i < 2; i++)
			{

				if (i == 0)
				{
					targetMesh = _parentOptMesh;
					iVertOffset = 0;
				}
				else
				{
					targetMesh = this;
					iVertOffset = _nVertParent;
				}
				int nVert = targetMesh._nVert;
				for (int iVert = 0; iVert < nVert; iVert++)
				{
					_vertPosList_ClippedMerge[iVert + iVertOffset] =
						_transform.InverseTransformPoint(
							targetMesh._transform.TransformPoint(
								targetMesh._vertPositions_Updated[iVert]));
				}
			}

			_material.SetColor("_Color", MeshColor);
			_material.SetColor("_MaskColor", _parentOptMesh.MeshColor);
			_mesh.vertices = _vertPosList_ClippedMerge;
		}

		// Functions
		//------------------------------------------------
		public void Show()
		{
			//if(_isMaskChild)
			//{
			//	//Child는 Show가 되지 않아염
			//	_meshRenderer.enabled = false;

			//	//Parent에 Show 해달라고 요청
			//	if(_parentOptMesh != null)
			//	{
			//		for (int iSub = 0; iSub < _parentOptMesh._subMeshes.Length; iSub++)
			//		{
			//			if(_parentOptMesh._subMeshes[iSub]._optMesh == this)
			//			{
			//				_parentOptMesh._subMeshes[iSub].SetVisible(true);
			//				_material.SetColor("_Color" + (iSub + 1), _parentOptMesh._subMeshes[iSub].MeshColor);
			//				break;
			//			}
			//		}
			//	}

			//	_isVisible = true;
			//	return;
			//}
			//수정 : 그런거 읍다. 다 렌더링됨

			_meshRenderer.enabled = true;
			_isVisible = true;
		}


		public void Hide()
		{
			//if(_isMaskChild)
			//{
			//	//Child는 Show가 되지 않아염
			//	_meshRenderer.enabled = false;

			//	//Parent에 Show 해달라고 요청
			//	if(_parentOptMesh != null)
			//	{
			//		for (int iSub = 0; iSub < _parentOptMesh._subMeshes.Length; iSub++)
			//		{
			//			if(_parentOptMesh._subMeshes[iSub]._optMesh == this)
			//			{
			//				Color clipColor = _parentOptMesh._subMeshes[iSub].MeshColor;
			//				_parentOptMesh._subMeshes[iSub].SetVisible(false);

			//				clipColor.a = 0.0f;
			//				_material.SetColor("_Color" + (iSub + 1), clipColor);
			//				break;
			//			}
			//		}
			//	}

			//	_isVisible = false;
			//	return;
			//}
			//수정 : Child도 다 렌더링 옵션을 적용합니다.

			_meshRenderer.enabled = false;
			_isVisible = false;
		}

		public void SetColor(Color color)
		{
			_multiplyColor = color;
		}




		// Get / Set
		//------------------------------------------------
		public Color MeshColor
		{
			get
			{
				return _multiplyColor * _parentTransform._meshColor2X;
			}
		}
	}
}
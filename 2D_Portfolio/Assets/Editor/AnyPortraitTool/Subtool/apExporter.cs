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
using System.IO;

using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// Editor에 포함되어서 Export를 담당한다.
	/// Texture Render / GIF Export / 백업용 Txt
	/// GIF의 경우 NGif 라이브러리의 코드를 가져와서 사용했다.
	/// https://www.codeproject.com/Articles/11505/NGif-Animated-GIF-Encoder-for-NET
	/// </summary>
	public class apExporter
	{
		// Members
		//----------------------------------------------
		private apEditor _editor = null;
		private RenderTexture _renderTexture = null;


		//GIF용 변수
		private int _gifWidth = 0;
		private int _gifHeight = 0;
		private byte[] _gifPixels = null;
		private int _gifRepeatCount = 0;//-1 : No Repeat, 0 : Forever
		private int _gifDelay = 0;//Frame Delay (ms / 10)
		private byte[] _gifIndexedPixels = null;//Converted Frame Indexed To Palatte
		private int _gifColorDepth = 0;//Number of bit Planes
		private byte[] _gifColorTab = null;//RGB Palette
		private bool[] _gifUsedEntry = new bool[256];//Active Palette Entries
		private int _gifPalSize = 7; // color table size (bits-1)

		//Step 전용 함수
		private string _gif_FilePath = "";
		private apAnimClip _gif_AnimClip = null;
		private apMeshGroup _gif_MeshGroup = null;
		private int _gif_LoopCount = 0;
		private int _gif_WinPosX = -1;
		private int _gif_WinPosY = -1;
		private int _gif_SrcSizeWidth = -1;
		private int _gif_SrcSizeHeight = -1;
		private int _gif_DstSizeWidth = -1;
		private int _gif_DstSizeHeight = -1;
		private Color _gif_ClearColor = Color.black;
		private int _gif_Quality = -1;

		private FileStream _gif_FileStream = null;
		private int _gif_totalProcessCount = -1;

		public string GIF_FilePath { get { return _gif_FilePath; } }

		// Init
		//----------------------------------------------
		public apExporter(apEditor editor)
		{
			_editor = editor;
		}

		// Functions
		//----------------------------------------------
		public Texture2D RenderToTexture(apMeshGroup meshGroup,
										int winPosX, int winPosY,
										int srcSizeWidth, int srcSizeHeight,
										int dstSizeWidth, int dstSizeHeight,
										Color clearColor)
		{
			if (_editor == null)
			{
				return null;
			}

			//apGL의 Window Size를 바꾸어준다.
			int prev_windowWidth = apGL._windowWidth;
			int prev_windowHeight = apGL._windowHeight;
			int prev_totalEditorWidth = apGL._totalEditorWidth;
			int prev_totalEditorHeight = apGL._totalEditorHeight;
			Vector2 prev_scroll = apGL._scrol_NotCalculated;
			int prev_PosX = apGL._posX_NotCalculated;
			int prev_PosY = apGL._posY_NotCalculated;
			float prev_Zoom = apGL._zoom;

			//apGL.SetWindowSize(prev_windowWidth, prev_windowHeight, Vector2.zero, prev_Zoom, 0, 0, prev_totalEditorWidth, prev_totalEditorHeight);

			//int rtSizeWidth = prev_totalEditorWidth;
			//int rtSizeHeight = prev_totalEditorHeight + 10;
			//int rtSizeWidth = (int)_editor.position.width;
			//int rtSizeHeight = (int)_editor.position.height;
			int rtSizeWidth = ((int)_editor.position.width);
			int rtSizeHeight = ((int)_editor.position.height + 25);
			int imageSizeWidth = srcSizeWidth;
			int imageSizeHeight = srcSizeHeight;

			meshGroup.RefreshForce();

			//1. Clip Parent의 MaskTexture를 미리 구워서 Dictionary에 넣는다.
			Dictionary<apRenderUnit, Texture2D> bakedClipMaskTextures = new Dictionary<apRenderUnit, Texture2D>();

			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								Texture2D clipMaskTex = apGL.GetMaskTexture_ClippingParent(renderUnit);
								if (clipMaskTex != null)
								{
									bakedClipMaskTextures.Add(renderUnit, clipMaskTex);
								}
								else
								{
									Debug.LogError("Clip Testure Bake 실패");
								}

							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(50);


			//Debug.Log("RenderTexture EditorSize : (" + rtSizeWidth + ", " + rtSizeHeight + ")");
			//_renderTexture = RenderTexture.GetTemporary(winWidthTotal, winHeightTotal, 8);
			_renderTexture = new RenderTexture(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			_renderTexture.wrapMode = TextureWrapMode.Clamp;

			RenderTexture.active = _renderTexture;

			//System.Threading.Thread.Sleep(50);
			GL.Clear(true, true, clearColor, -100.0f);

			//System.Threading.Thread.Sleep(50);

			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								if (bakedClipMaskTextures.ContainsKey(renderUnit))
								{
									apGL.DrawRenderUnit_ClippingParent_Renew_WithoutRTT(renderUnit,
												renderUnit._meshTransform._clipChildMeshes,
												bakedClipMaskTextures[renderUnit]);
								}


								////RenderTexture.active = _renderTexture;//<<클리핑 뒤에는 다시 연결해줘야한다.
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//Pass
						}
						else
						{
							if (renderUnit._isVisible)
							{
								apGL.DrawRenderUnit_Basic(renderUnit);
							}
						}
					}
				}
			}

			//프레임 Rect
			//Vector2 framePos_Center = new Vector2(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x, _editor._captureFrame_PosY + apGL.WindowSizeHalf.y);
			//Vector2 frameHalfSize = new Vector2(_editor._captureFrame_SrcWidth / 2, _editor._captureFrame_SrcHeight / 2);
			//Vector2 framePos_LT = framePos_Center + new Vector2(-frameHalfSize.x, -frameHalfSize.y);
			//Vector2 framePos_RT = framePos_Center + new Vector2(frameHalfSize.x, -frameHalfSize.y);
			//Vector2 framePos_LB = framePos_Center + new Vector2(-frameHalfSize.x, frameHalfSize.y);
			//Vector2 framePos_RB = framePos_Center + new Vector2(frameHalfSize.x, frameHalfSize.y);
			//Color frameColor = new Color(0.3f, 1.0f, 1.0f, 1.0f);
			//apGL.DrawLineGL(framePos_LT, framePos_RT, frameColor, true);
			//apGL.DrawLineGL(framePos_RT, framePos_RB, frameColor, true);
			//apGL.DrawLineGL(framePos_RB, framePos_LB, frameColor, true);
			//apGL.DrawLineGL(framePos_LB, framePos_LT, frameColor, true);

			//winPosY -= 10;

			int clipPosX = winPosX + (-imageSizeWidth / 2);
			int clipPosY = winPosY + (-imageSizeHeight / 2);
			int clipPosX_Right = clipPosX + imageSizeWidth;
			int clipPosY_Bottom = clipPosY + imageSizeHeight;

			if (clipPosX < 0)
			{ clipPosX = 0; }
			if (clipPosY < 0)
			{ clipPosY = 0; }
			if (clipPosX_Right > rtSizeWidth)
			{ clipPosX_Right = rtSizeWidth; }
			if (clipPosY_Bottom > rtSizeHeight)
			{ clipPosY_Bottom = rtSizeHeight; }

			int clipWidth = (clipPosX_Right - clipPosX);
			int clipHeight = (clipPosY_Bottom - clipPosY);
			if (clipWidth <= 0 || clipHeight <= 0)
			{
				Debug.LogError("RenderToTexture 실패 : Clip 영역이 화면 밖으로 나갔다.");
				return null;
			}

			System.Threading.Thread.Sleep(50);

			Texture2D resultTex_SrcSize = new Texture2D(imageSizeWidth, imageSizeHeight, TextureFormat.RGB24, false);
			resultTex_SrcSize.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);
			resultTex_SrcSize.Apply();




			//System.Threading.Thread.Sleep(50);

			RenderTexture.active = null;

			//RenderTexture.ReleaseTemporary(_renderTexture);
			UnityEngine.Object.DestroyImmediate(_renderTexture);

			_renderTexture = null;

			Texture2D resultTex_DstSize = new Texture2D(dstSizeWidth, dstSizeHeight, TextureFormat.RGB24, false);
			for (int iY = 0; iY < dstSizeHeight; iY++)
			{
				for (int iX = 0; iX < dstSizeWidth; iX++)
				{
					float u = (float)iX / (float)dstSizeWidth;
					float v = (float)iY / (float)dstSizeHeight;

					resultTex_DstSize.SetPixel(iX, iY, resultTex_SrcSize.GetPixelBilinear(u, v));
				}
			}

			System.Threading.Thread.Sleep(50);
			UnityEngine.Object.DestroyImmediate(resultTex_SrcSize);//<<기존 크기의 이미지는 삭제


			return resultTex_DstSize;

		}


		public bool SaveTexture2DToPNG(Texture2D srcTexture2D, string filePathWithExtension, bool isAutoDestroy)
		{
			try
			{
				if (srcTexture2D == null)
				{
					return false;
				}

				File.WriteAllBytes(filePathWithExtension + ".png", srcTexture2D.EncodeToPNG());

				if (isAutoDestroy)
				{
					UnityEngine.Object.DestroyImmediate(srcTexture2D);
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("SaveTexture2DToPNG Exception : " + ex);

				if (isAutoDestroy)
				{
					UnityEngine.Object.Destroy(srcTexture2D);
				}
				return false;
			}
		}


		/// <summary>
		/// GIF Animation을 만든다.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="meshGroup"></param>
		/// <param name="animClip"></param>
		/// <param name="loopCount"></param>
		/// <param name="winPosX"></param>
		/// <param name="winPosY"></param>
		/// <param name="srcSizeWidth"></param>
		/// <param name="srcSizeHeight"></param>
		/// <param name="dstSizeWidth"></param>
		/// <param name="dstSizeHeight"></param>
		/// <param name="clearColor"></param>
		/// <param name="quality">1 ~ 256</param>
		/// <returns></returns>
		public bool MakeGIFAnimation(string filePath,
										apMeshGroup meshGroup,
										apAnimClip animClip,
										int loopCount,
										int winPosX, int winPosY,
										int srcSizeWidth, int srcSizeHeight,
										int dstSizeWidth, int dstSizeHeight,
										Color clearColor,
										int quality)
		{
			if (_editor == null || _editor._portrait == null || meshGroup == null || animClip == null)
			{
				return false;
			}

			int startFrame = animClip.StartFrame;
			int endFrame = animClip.EndFrame;
			if (endFrame < startFrame)
			{
				endFrame = startFrame;
			}
			if (loopCount < 1)
			{
				loopCount = 1;
			}

			//모든 AnimClip 정지
			for (int i = 0; i < _editor._portrait._animClips.Count; i++)
			{
				_editor._portrait._animClips[i].Stop_Editor();
			}
			_editor._portrait._animPlayManager.Stop_Editor();
			_editor._portrait._animPlayManager.SetAnimClip_Editor(animClip);
			meshGroup.RefreshForce();

			int curFrame = startFrame;
			bool isLoop = animClip.IsLoop;
			//Loop라면 마지막 프레임을 생략한다.
			int lastFrame = endFrame;
			if (isLoop)
			{
				lastFrame = endFrame - 1;
			}
			if (lastFrame < startFrame)
			{
				lastFrame = startFrame;
			}

			float secPerFrame = 1.0f / (float)animClip.FPS;


			FileStream fs = null;
			try
			{
				fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

				WriteString("GIF89a", fs); // header
				_gifDelay = (int)((secPerFrame * 100.0f) + 0.5f);//Delay
				_gifRepeatCount = 0;//반복
				_gifWidth = dstSizeWidth;
				_gifHeight = dstSizeHeight;
				_gifPixels = null;
				_gifIndexedPixels = null;
				_gifColorDepth = 0;
				_gifColorTab = null;
				_gifPalSize = 7;

				for (int i = 0; i < _gifUsedEntry.Length; i++)
				{
					_gifUsedEntry[i] = false;
				}

				bool isFirstFrame = true;



				//애니메이션을 돌면서 Bake를 한다.
				for (int iLoop = 0; iLoop < loopCount; iLoop++)
				{
					curFrame = startFrame;


					while (true)
					{
						animClip.SetFrame_Editor(curFrame);//메시가 자동으로 업데이트를 한다.
						meshGroup.UpdateRenderUnits(secPerFrame, true);

						Texture2D bakeImage = RenderToTexture(meshGroup, winPosX, winPosY, srcSizeWidth, srcSizeHeight, dstSizeWidth, dstSizeHeight, clearColor);

						AddFrame(bakeImage, fs, isFirstFrame, quality);
						isFirstFrame = false;

						UnityEngine.Object.DestroyImmediate(bakeImage);

						curFrame++;
						if (curFrame > lastFrame)
						{
							break;
						}
					}
				}

				Finish(fs);

				fs.Close();
				fs = null;
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("GIF Exception : " + ex);
			}
			if (fs != null)
			{
				fs.Close();
				fs = null;
			}


			return false;


		}



		// MakeGIF Animation의 분할 함수
		//1 - 준비
		//2 - 각 프레임별로 처리
		//3 - 마지막 파일 생성
		//리턴값은 퍼센트
		public int MakeGIFAnimation_Ready(string filePath,
										apMeshGroup meshGroup,
										apAnimClip animClip,
										int loopCount,
										int winPosX, int winPosY,
										int srcSizeWidth, int srcSizeHeight,
										int dstSizeWidth, int dstSizeHeight,
										Color clearColor,
										int quality)
		{
			if (_editor == null || _editor._portrait == null || meshGroup == null || animClip == null)
			{
				return -1;
			}

			int startFrame = animClip.StartFrame;
			int endFrame = animClip.EndFrame;
			if (endFrame < startFrame)
			{
				endFrame = startFrame;
			}
			if (loopCount < 1)
			{
				loopCount = 1;
			}

			_gif_FilePath = filePath;
			_gif_AnimClip = animClip;
			_gif_MeshGroup = meshGroup;
			_gif_LoopCount = loopCount;
			_gif_WinPosX = winPosX;
			_gif_WinPosY = winPosY;
			_gif_SrcSizeWidth = srcSizeWidth;
			_gif_SrcSizeHeight = srcSizeHeight;
			_gif_DstSizeWidth = dstSizeWidth;
			_gif_DstSizeHeight = dstSizeHeight;
			_gif_ClearColor = clearColor;
			_gif_Quality = quality;

			//몇번 실행할지 결정한다.
			bool isLoop = animClip.IsLoop;
			//Loop라면 마지막 프레임을 생략한다.
			int lastFrame = endFrame;
			if (isLoop)
			{
				lastFrame = endFrame - 1;
			}
			if (lastFrame < startFrame)
			{
				lastFrame = startFrame;
			}

			//Loop Count는
			//loopCount * (lastFrame - startFrame + 1) + Pre 하나 / Post 하나
			_gif_totalProcessCount = (((lastFrame - startFrame) + 1) * loopCount) + 2;

			if (_gif_FileStream != null)
			{
				_gif_FileStream.Close();
				_gif_FileStream = null;
			}
			_gif_FileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

			return _gif_totalProcessCount;
		}

		public float MakeGIFAnimation_Step(int iStep)
		{
			if (_editor == null || _editor._portrait == null || _gif_MeshGroup == null || _gif_AnimClip == null || _gif_FileStream == null)
			{
				return -1.0f;//실패
			}

			int startFrame = _gif_AnimClip.StartFrame;
			int endFrame = _gif_AnimClip.EndFrame;
			if (endFrame < startFrame)
			{
				endFrame = startFrame;
			}

			int curFrame = startFrame;
			bool isLoop = _gif_AnimClip.IsLoop;
			//Loop라면 마지막 프레임을 생략한다.
			int lastFrame = endFrame;
			if (isLoop)
			{
				lastFrame = endFrame - 1;
			}
			if (lastFrame < startFrame)
			{
				lastFrame = startFrame;
			}

			float secPerFrame = 1.0f / (float)_gif_AnimClip.FPS;

			try
			{
				if (iStep == 0)
				{
					//Step 초기
					//렌더 준비

					//모든 AnimClip 정지
					for (int i = 0; i < _editor._portrait._animClips.Count; i++)
					{
						_editor._portrait._animClips[i].Stop_Editor();
					}
					_editor._portrait._animPlayManager.Stop_Editor();
					_editor._portrait._animPlayManager.SetAnimClip_Editor(_gif_AnimClip);
					_gif_MeshGroup.RefreshForce();


					WriteString("GIF89a", _gif_FileStream); // header
					_gifDelay = (int)((secPerFrame * 100.0f) + 0.5f);//Delay
					_gifRepeatCount = 0;//반복
					_gifWidth = _gif_DstSizeWidth;
					_gifHeight = _gif_DstSizeHeight;
					_gifPixels = null;
					_gifIndexedPixels = null;
					_gifColorDepth = 0;
					_gifColorTab = null;
					_gifPalSize = 7;

					for (int i = 0; i < _gifUsedEntry.Length; i++)
					{
						_gifUsedEntry[i] = false;
					}

					return (1.0f / _gif_totalProcessCount) * 100.0f;
				}
				else if (iStep < _gif_totalProcessCount - 1)
				{
					//각 프레임을 렌더링한다.
					bool isFirstFrame = (iStep == 1);
					curFrame = (iStep - 1) + startFrame;
					int nFrame = (lastFrame - startFrame) + 1;
					while (curFrame > lastFrame)
					{
						curFrame -= nFrame;
					}

					_gif_AnimClip.SetFrame_Editor(curFrame);//메시가 자동으로 업데이트를 한다.
					_gif_MeshGroup.UpdateRenderUnits(secPerFrame, true);

					Texture2D bakeImage = RenderToTexture(_gif_MeshGroup,
															_gif_WinPosX, _gif_WinPosY,
															_gif_SrcSizeWidth, _gif_SrcSizeHeight,
															_gif_DstSizeWidth, _gif_DstSizeHeight, _gif_ClearColor);

					AddFrame(bakeImage, _gif_FileStream, isFirstFrame, _gif_Quality);

					UnityEngine.Object.DestroyImmediate(bakeImage);
					return ((iStep + 1) / _gif_totalProcessCount) * 100.0f;
				}
				else
				{
					//마지막으로 저장을 하자
					Finish(_gif_FileStream);

					_gif_FileStream.Close();
					_gif_FileStream = null;
					return 100.0f;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Make GIF Animation Exception : " + ex);
				if (_gif_FileStream != null)
				{
					_gif_FileStream.Close();
					_gif_FileStream = null;
				}
			}

			return -1.0f;


		}


		//GIF 함수
		//----------------------------------------------
		private void WriteString(string s, FileStream fs)
		{
			char[] chars = s.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				fs.WriteByte((byte)chars[i]);
			}
		}

		private bool AddFrame(Texture2D tex2D, FileStream fs, bool isFirstFrame, int quality)
		{
			if (tex2D == null)
			{
				return false;
			}

			_gifIndexedPixels = null;
			byte[] imgBytes = GetImagePixels(tex2D);
			AnalyzePixels(imgBytes, quality);
			if (isFirstFrame)
			{
				WriteLSD(fs); // logical screen descriptior
				WritePalette(fs); // global color table
				if (_gifRepeatCount >= 0)
				{
					// use NS app extension to indicate reps
					WriteNetscapeExt(fs);
				}
			}
			WriteGraphicCtrlExt(fs); // write graphic control extension
			WriteImageDesc(fs, isFirstFrame); // image descriptor
			if (!isFirstFrame)
			{
				WritePalette(fs); // local color table
			}
			WritePixels(fs); // encode and write pixel data
			return true;
		}

		private void Finish(FileStream fs)
		{
			fs.WriteByte(0x3b); // gif trailer
			fs.Flush();

			_gifWidth = 0;
			_gifHeight = 0;
			_gifRepeatCount = 0;//-1 : No Repeat, 0 : Forever
			_gifDelay = 0;//Frame Delay (ms / 10)
			_gifIndexedPixels = null;//Converted Frame Indexed To Palatte
			_gifColorDepth = 0;//Number of bit Planes
			_gifColorTab = null;//RGB Palette
								//_gifUsedEntry = new bool[256];//Active Palette Entries
			_gifPalSize = 7; // 
		}

		private byte[] GetImagePixels(Texture2D tex2D)
		{
			if (_gifPixels == null)
			{
				_gifPixels = new byte[3 * _gifWidth * _gifHeight];
			}
			int iPixel = 0;
			Color color = Color.black;
			for (int iY = 0; iY < _gifHeight; iY++)
			{
				for (int iX = 0; iX < _gifWidth; iX++)
				{
					color = tex2D.GetPixel(iX, (_gifHeight - 1) - iY);
					_gifPixels[iPixel] = (byte)(color.r * 255.0f);
					iPixel++;
					_gifPixels[iPixel] = (byte)(color.g * 255.0f);
					iPixel++;
					_gifPixels[iPixel] = (byte)(color.b * 255.0f);
					iPixel++;
				}
			}
			return _gifPixels;
		}

		private void AnalyzePixels(byte[] pixels, int quality)
		{
			int arrLength = pixels.Length;
			int nPixel = pixels.Length / 3;
			if (_gifIndexedPixels == null)
			{
				_gifIndexedPixels = new byte[nPixel];
			}
			Array.Clear(_gifIndexedPixels, 0, nPixel);


			NeuQuant nq = new NeuQuant(pixels, arrLength, quality);//퀄리티는 1~256 사이

			_gifColorTab = nq.Process();

			int k = 0;
			for (int i = 0; i < nPixel; i++)
			{
				int index =
					nq.Map(pixels[k++] & 0xff,
					pixels[k++] & 0xff,
					pixels[k++] & 0xff);
				_gifUsedEntry[index] = true;
				_gifIndexedPixels[i] = (byte)index;
			}
			//pixels = null;
			_gifColorDepth = 8;
			_gifPalSize = 7;
		}


		protected void WriteGraphicCtrlExt(FileStream fs)
		{
			fs.WriteByte(0x21); // extension introducer
			fs.WriteByte(0xf9); // GCE label
			fs.WriteByte(4); // data block size
			int transp, disp;
			transp = 0;
			disp = 0; // dispose = no action

			disp <<= 2;

			// packed fields
			fs.WriteByte(Convert.ToByte(0 | // 1:3 reserved
				disp | // 4:6 disposal
				0 | // 7   user input - 0 = none
				transp)); // 8   transparency flag

			WriteShort(_gifDelay, fs); // delay x 1/100 sec
			fs.WriteByte(Convert.ToByte(0)); // transparent color index
			fs.WriteByte(0); // block terminator
		}

		/**
		 * Writes Image Descriptor
		 */
		protected void WriteImageDesc(FileStream fs, bool isFirstFrame)
		{
			fs.WriteByte(0x2c); // image separator
			WriteShort(0, fs); // image position x,y = 0,0
			WriteShort(0, fs);
			WriteShort(_gifWidth, fs); // image size
			WriteShort(_gifHeight, fs);
			// packed fields
			if (isFirstFrame)
			{
				// no LCT  - GCT is used for first (or only) frame
				fs.WriteByte(0);
			}
			else
			{
				// specify normal LCT
				fs.WriteByte(Convert.ToByte(0x80 | // 1 local color table  1=yes
					0 | // 2 interlace - 0=no
					0 | // 3 sorted - 0=no
					0 | // 4-5 reserved
					_gifPalSize)); // 6-8 size of color table
			}
		}



		protected void WriteLSD(FileStream fs)
		{
			// logical screen size
			WriteShort(_gifWidth, fs);
			WriteShort(_gifHeight, fs);
			// packed fields
			fs.WriteByte(Convert.ToByte(0x80 | // 1   : global color table flag = 1 (gct used)
				0x70 | // 2-4 : color resolution = 7
				0x00 | // 5   : gct sort flag = 0
				_gifPalSize)); // 6-8 : gct size

			fs.WriteByte(0); // background color index
			fs.WriteByte(0); // pixel aspect ratio - assume 1:1
		}

		protected void WriteNetscapeExt(FileStream fs)
		{
			fs.WriteByte(0x21); // extension introducer
			fs.WriteByte(0xff); // app extension label
			fs.WriteByte(11); // block size
			WriteString("NETSCAPE" + "2.0", fs); // app id + auth code
			fs.WriteByte(3); // sub-block size
			fs.WriteByte(1); // loop sub-block id
			WriteShort(_gifRepeatCount, fs); // loop count (extra iterations, 0=repeat forever)
			fs.WriteByte(0); // block terminator
		}

		protected void WritePalette(FileStream fs)
		{
			fs.Write(_gifColorTab, 0, _gifColorTab.Length);
			int n = (3 * 256) - _gifColorTab.Length;
			for (int i = 0; i < n; i++)
			{
				fs.WriteByte(0);
			}
		}

		protected void WritePixels(FileStream fs)
		{
			LZWEncoder encoder =
				new LZWEncoder(_gifWidth, _gifHeight, _gifIndexedPixels, _gifColorDepth);
			encoder.Encode(fs);
		}

		protected void WriteShort(int value, FileStream fs)
		{
			fs.WriteByte(Convert.ToByte(value & 0xff));
			fs.WriteByte(Convert.ToByte((value >> 8) & 0xff));
		}

		public class NeuQuant
		{
			protected static readonly int netsize = 256; /* number of colours used */
														 /* four primes near 500 - assume no image has a length so large */
														 /* that it is divisible by all four primes */
			protected static readonly int prime1 = 499;
			protected static readonly int prime2 = 491;
			protected static readonly int prime3 = 487;
			protected static readonly int prime4 = 503;
			protected static readonly int minpicturebytes = (3 * prime4);
			/* minimum size for input image */
			/* Program Skeleton
			   ----------------
			   [select samplefac in range 1..30]
			   [read image from input file]
			   pic = (unsigned char*) malloc(3*width*height);
			   initnet(pic,3*width*height,samplefac);
			   learn();
			   unbiasnet();
			   [write output image header, using writecolourmap(f)]
			   inxbuild();
			   write output image using inxsearch(b,g,r)      */

			/* Network Definitions
			   ------------------- */
			protected static readonly int maxnetpos = (netsize - 1);
			protected static readonly int netbiasshift = 4; /* bias for colour values */
			protected static readonly int ncycles = 100; /* no. of learning cycles */

			/* defs for freq and bias */
			protected static readonly int intbiasshift = 16; /* bias for fractions */
			protected static readonly int intbias = (((int)1) << intbiasshift);
			protected static readonly int gammashift = 10; /* gamma = 1024 */
			protected static readonly int gamma = (((int)1) << gammashift);
			protected static readonly int betashift = 10;
			protected static readonly int beta = (intbias >> betashift); /* beta = 1/1024 */
			protected static readonly int betagamma =
				(intbias << (gammashift - betashift));

			/* defs for decreasing radius factor */
			protected static readonly int initrad = (netsize >> 3); /* for 256 cols, radius starts */
			protected static readonly int radiusbiasshift = 6; /* at 32.0 biased by 6 bits */
			protected static readonly int radiusbias = (((int)1) << radiusbiasshift);
			protected static readonly int initradius = (initrad * radiusbias); /* and decreases by a */
			protected static readonly int radiusdec = 30; /* factor of 1/30 each cycle */

			/* defs for decreasing alpha factor */
			protected static readonly int alphabiasshift = 10; /* alpha starts at 1.0 */
			protected static readonly int initalpha = (((int)1) << alphabiasshift);

			protected int alphadec; /* biased by 10 bits */

			/* radbias and alpharadbias used for radpower calculation */
			protected static readonly int radbiasshift = 8;
			protected static readonly int radbias = (((int)1) << radbiasshift);
			protected static readonly int alpharadbshift = (alphabiasshift + radbiasshift);
			protected static readonly int alpharadbias = (((int)1) << alpharadbshift);

			/* Types and Global Variables
			-------------------------- */

			protected byte[] thepicture; /* the input image itself */
			protected int lengthcount; /* lengthcount = H*W*3 */

			protected int samplefac; /* sampling factor 1..30 */

			//   typedef int pixel[4];                /* BGRc */
			protected int[][] network; /* the network itself - [netsize][4] */

			protected int[] netindex = new int[256];
			/* for network lookup - really 256 */

			protected int[] bias = new int[netsize];
			/* bias and freq arrays for learning */
			protected int[] freq = new int[netsize];
			protected int[] radpower = new int[initrad];
			/* radpower for precomputation */

			/* Initialise network in range (0,0,0) to (255,255,255) and set parameters
			   ----------------------------------------------------------------------- */
			public NeuQuant(byte[] thepic, int len, int sample)
			{

				int i;
				int[] p;

				thepicture = thepic;
				lengthcount = len;
				samplefac = sample;

				network = new int[netsize][];
				for (i = 0; i < netsize; i++)
				{
					network[i] = new int[4];
					p = network[i];
					p[0] = p[1] = p[2] = (i << (netbiasshift + 8)) / netsize;
					freq[i] = intbias / netsize; /* 1/netsize */
					bias[i] = 0;
				}
			}

			public byte[] ColorMap()
			{
				byte[] map = new byte[3 * netsize];
				int[] index = new int[netsize];
				for (int i = 0; i < netsize; i++)
					index[network[i][3]] = i;
				int k = 0;
				for (int i = 0; i < netsize; i++)
				{
					int j = index[i];
					map[k++] = (byte)(network[j][0]);
					map[k++] = (byte)(network[j][1]);
					map[k++] = (byte)(network[j][2]);
				}
				return map;
			}

			/* Insertion sort of network and building of netindex[0..255] (to do after unbias)
			   ------------------------------------------------------------------------------- */
			public void Inxbuild()
			{

				int i, j, smallpos, smallval;
				int[] p;
				int[] q;
				int previouscol, startpos;

				previouscol = 0;
				startpos = 0;
				for (i = 0; i < netsize; i++)
				{
					p = network[i];
					smallpos = i;
					smallval = p[1]; /* index on g */
									 /* find smallest in i..netsize-1 */
					for (j = i + 1; j < netsize; j++)
					{
						q = network[j];
						if (q[1] < smallval)
						{ /* index on g */
							smallpos = j;
							smallval = q[1]; /* index on g */
						}
					}
					q = network[smallpos];
					/* swap p (i) and q (smallpos) entries */
					if (i != smallpos)
					{
						j = q[0];
						q[0] = p[0];
						p[0] = j;
						j = q[1];
						q[1] = p[1];
						p[1] = j;
						j = q[2];
						q[2] = p[2];
						p[2] = j;
						j = q[3];
						q[3] = p[3];
						p[3] = j;
					}
					/* smallval entry is now in position i */
					if (smallval != previouscol)
					{
						netindex[previouscol] = (startpos + i) >> 1;
						for (j = previouscol + 1; j < smallval; j++)
							netindex[j] = i;
						previouscol = smallval;
						startpos = i;
					}
				}
				netindex[previouscol] = (startpos + maxnetpos) >> 1;
				for (j = previouscol + 1; j < 256; j++)
					netindex[j] = maxnetpos; /* really 256 */
			}

			/* Main Learning Loop
			   ------------------ */
			public void Learn()
			{

				int i, j, b, g, r;
				int radius, rad, alpha, step, delta, samplepixels;
				byte[] p;
				int pix, lim;

				if (lengthcount < minpicturebytes)
					samplefac = 1;
				alphadec = 30 + ((samplefac - 1) / 3);
				p = thepicture;
				pix = 0;
				lim = lengthcount;
				samplepixels = lengthcount / (3 * samplefac);
				delta = samplepixels / ncycles;
				alpha = initalpha;
				radius = initradius;

				rad = radius >> radiusbiasshift;
				if (rad <= 1)
					rad = 0;
				for (i = 0; i < rad; i++)
					radpower[i] =
						alpha * (((rad * rad - i * i) * radbias) / (rad * rad));

				//fprintf(stderr,"beginning 1D learning: initial radius=%d\n", rad);

				if (lengthcount < minpicturebytes)
					step = 3;
				else if ((lengthcount % prime1) != 0)
					step = 3 * prime1;
				else
				{
					if ((lengthcount % prime2) != 0)
						step = 3 * prime2;
					else
					{
						if ((lengthcount % prime3) != 0)
							step = 3 * prime3;
						else
							step = 3 * prime4;
					}
				}

				i = 0;
				while (i < samplepixels)
				{
					b = (p[pix + 0] & 0xff) << netbiasshift;
					g = (p[pix + 1] & 0xff) << netbiasshift;
					r = (p[pix + 2] & 0xff) << netbiasshift;
					j = Contest(b, g, r);

					Altersingle(alpha, j, b, g, r);
					if (rad != 0)
						Alterneigh(rad, j, b, g, r); /* alter neighbours */

					pix += step;
					if (pix >= lim)
						pix -= lengthcount;

					i++;
					if (delta == 0)
						delta = 1;
					if (i % delta == 0)
					{
						alpha -= alpha / alphadec;
						radius -= radius / radiusdec;
						rad = radius >> radiusbiasshift;
						if (rad <= 1)
							rad = 0;
						for (j = 0; j < rad; j++)
							radpower[j] =
								alpha * (((rad * rad - j * j) * radbias) / (rad * rad));
					}
				}
				//fprintf(stderr,"finished 1D learning: readonly alpha=%f !\n",((float)alpha)/initalpha);
			}

			/* Search for BGR values 0..255 (after net is unbiased) and return colour index
			   ---------------------------------------------------------------------------- */
			public int Map(int b, int g, int r)
			{

				int i, j, dist, a, bestd;
				int[] p;
				int best;

				bestd = 1000; /* biggest possible dist is 256*3 */
				best = -1;
				i = netindex[g]; /* index on g */
				j = i - 1; /* start at netindex[g] and work outwards */

				while ((i < netsize) || (j >= 0))
				{
					if (i < netsize)
					{
						p = network[i];
						dist = p[1] - g; /* inx key */
						if (dist >= bestd)
							i = netsize; /* stop iter */
						else
						{
							i++;
							if (dist < 0)
								dist = -dist;
							a = p[0] - b;
							if (a < 0)
								a = -a;
							dist += a;
							if (dist < bestd)
							{
								a = p[2] - r;
								if (a < 0)
									a = -a;
								dist += a;
								if (dist < bestd)
								{
									bestd = dist;
									best = p[3];
								}
							}
						}
					}
					if (j >= 0)
					{
						p = network[j];
						dist = g - p[1]; /* inx key - reverse dif */
						if (dist >= bestd)
							j = -1; /* stop iter */
						else
						{
							j--;
							if (dist < 0)
								dist = -dist;
							a = p[0] - b;
							if (a < 0)
								a = -a;
							dist += a;
							if (dist < bestd)
							{
								a = p[2] - r;
								if (a < 0)
									a = -a;
								dist += a;
								if (dist < bestd)
								{
									bestd = dist;
									best = p[3];
								}
							}
						}
					}
				}
				return (best);
			}
			public byte[] Process()
			{
				Learn();
				Unbiasnet();
				Inxbuild();
				return ColorMap();
			}

			/* Unbias network to give byte values 0..255 and record position i to prepare for sort
			   ----------------------------------------------------------------------------------- */
			public void Unbiasnet()
			{

				int i, j;

				for (i = 0; i < netsize; i++)
				{
					network[i][0] >>= netbiasshift;
					network[i][1] >>= netbiasshift;
					network[i][2] >>= netbiasshift;
					network[i][3] = i; /* record colour no */
				}
			}

			/* Move adjacent neurons by precomputed alpha*(1-((i-j)^2/[r]^2)) in radpower[|i-j|]
			   --------------------------------------------------------------------------------- */
			protected void Alterneigh(int rad, int i, int b, int g, int r)
			{

				int j, k, lo, hi, a, m;
				int[] p;

				lo = i - rad;
				if (lo < -1)
					lo = -1;
				hi = i + rad;
				if (hi > netsize)
					hi = netsize;

				j = i + 1;
				k = i - 1;
				m = 1;
				while ((j < hi) || (k > lo))
				{
					a = radpower[m++];
					if (j < hi)
					{
						p = network[j++];
						try
						{
							p[0] -= (a * (p[0] - b)) / alpharadbias;
							p[1] -= (a * (p[1] - g)) / alpharadbias;
							p[2] -= (a * (p[2] - r)) / alpharadbias;
						}
						catch (Exception e)
						{
						} // prevents 1.3 miscompilation
					}
					if (k > lo)
					{
						p = network[k--];
						try
						{
							p[0] -= (a * (p[0] - b)) / alpharadbias;
							p[1] -= (a * (p[1] - g)) / alpharadbias;
							p[2] -= (a * (p[2] - r)) / alpharadbias;
						}
						catch (Exception e)
						{
						}
					}
				}
			}

			/* Move neuron i towards biased (b,g,r) by factor alpha
			   ---------------------------------------------------- */
			protected void Altersingle(int alpha, int i, int b, int g, int r)
			{

				/* alter hit neuron */
				int[] n = network[i];
				n[0] -= (alpha * (n[0] - b)) / initalpha;
				n[1] -= (alpha * (n[1] - g)) / initalpha;
				n[2] -= (alpha * (n[2] - r)) / initalpha;
			}

			/* Search for biased BGR values
			   ---------------------------- */
			protected int Contest(int b, int g, int r)
			{

				/* finds closest neuron (min dist) and updates freq */
				/* finds best neuron (min dist-bias) and returns position */
				/* for frequently chosen neurons, freq[i] is high and bias[i] is negative */
				/* bias[i] = gamma*((1/netsize)-freq[i]) */

				int i, dist, a, biasdist, betafreq;
				int bestpos, bestbiaspos, bestd, bestbiasd;
				int[] n;

				bestd = ~(((int)1) << 31);
				bestbiasd = bestd;
				bestpos = -1;
				bestbiaspos = bestpos;

				for (i = 0; i < netsize; i++)
				{
					n = network[i];
					dist = n[0] - b;
					if (dist < 0)
						dist = -dist;
					a = n[1] - g;
					if (a < 0)
						a = -a;
					dist += a;
					a = n[2] - r;
					if (a < 0)
						a = -a;
					dist += a;
					if (dist < bestd)
					{
						bestd = dist;
						bestpos = i;
					}
					biasdist = dist - ((bias[i]) >> (intbiasshift - netbiasshift));
					if (biasdist < bestbiasd)
					{
						bestbiasd = biasdist;
						bestbiaspos = i;
					}
					betafreq = (freq[i] >> betashift);
					freq[i] -= betafreq;
					bias[i] += (betafreq << gammashift);
				}
				freq[bestpos] += beta;
				bias[bestpos] -= betagamma;
				return (bestbiaspos);
			}
		}

		public class LZWEncoder
		{

			private static readonly int EOF = -1;

			private int imgW, imgH;
			private byte[] pixAry;
			private int initCodeSize;
			private int remaining;
			private int curPixel;

			// GIFCOMPR.C       - GIF Image compression routines
			//
			// Lempel-Ziv compression based on 'compress'.  GIF modifications by
			// David Rowley (mgardi@watdcsu.waterloo.edu)

			// General DEFINEs

			static readonly int BITS = 12;

			static readonly int HSIZE = 5003; // 80% occupancy

			// GIF Image compression - modified 'compress'
			//
			// Based on: compress.c - File compression ala IEEE Computer, June 1984.
			//
			// By Authors:  Spencer W. Thomas      (decvax!harpo!utah-cs!utah-gr!thomas)
			//              Jim McKie              (decvax!mcvax!jim)
			//              Steve Davies           (decvax!vax135!petsd!peora!srd)
			//              Ken Turkowski          (decvax!decwrl!turtlevax!ken)
			//              James A. Woods         (decvax!ihnp4!ames!jaw)
			//              Joe Orost              (decvax!vax135!petsd!joe)

			int n_bits; // number of bits/code
			int maxbits = BITS; // user settable max # bits/code
			int maxcode; // maximum code, given n_bits
			int maxmaxcode = 1 << BITS; // should NEVER generate this code

			int[] htab = new int[HSIZE];
			int[] codetab = new int[HSIZE];

			int hsize = HSIZE; // for dynamic table sizing

			int free_ent = 0; // first unused entry

			// block compression parameters -- after all codes are used up,
			// and compression rate changes, start over.
			bool clear_flg = false;

			// Algorithm:  use open addressing double hashing (no chaining) on the
			// prefix code / next character combination.  We do a variant of Knuth's
			// algorithm D (vol. 3, sec. 6.4) along with G. Knott's relatively-prime
			// secondary probe.  Here, the modular division first probe is gives way
			// to a faster exclusive-or manipulation.  Also do block compression with
			// an adaptive reset, whereby the code table is cleared when the compression
			// ratio decreases, but after the table fills.  The variable-length output
			// codes are re-sized at this point, and a special CLEAR code is generated
			// for the decompressor.  Late addition:  construct the table according to
			// file size for noticeable speed improvement on small files.  Please direct
			// questions about this implementation to ames!jaw.

			int g_init_bits;

			int ClearCode;
			int EOFCode;

			// output
			//
			// Output the given code.
			// Inputs:
			//      code:   A n_bits-bit integer.  If == -1, then EOF.  This assumes
			//              that n_bits =< wordsize - 1.
			// Outputs:
			//      Outputs code to the file.
			// Assumptions:
			//      Chars are 8 bits long.
			// Algorithm:
			//      Maintain a BITS character long buffer (so that 8 codes will
			// fit in it exactly).  Use the VAX insv instruction to insert each
			// code in turn.  When the buffer fills up empty it and start over.

			int cur_accum = 0;
			int cur_bits = 0;

			int[] masks =
			{
			0x0000,
			0x0001,
			0x0003,
			0x0007,
			0x000F,
			0x001F,
			0x003F,
			0x007F,
			0x00FF,
			0x01FF,
			0x03FF,
			0x07FF,
			0x0FFF,
			0x1FFF,
			0x3FFF,
			0x7FFF,
			0xFFFF };

			// Number of characters so far in this 'packet'
			int a_count;

			// Define the storage for the packet accumulator
			byte[] accum = new byte[256];

			//----------------------------------------------------------------------------
			public LZWEncoder(int width, int height, byte[] pixels, int color_depth)
			{
				imgW = width;
				imgH = height;
				pixAry = pixels;
				initCodeSize = Math.Max(2, color_depth);
			}

			// Add a character to the end of the current packet, and if it is 254
			// characters, flush the packet to disk.
			void Add(byte c, Stream outs)
			{
				accum[a_count++] = c;
				if (a_count >= 254)
					Flush(outs);
			}

			// Clear out the hash table

			// table clear for block compress
			void ClearTable(Stream outs)
			{
				ResetCodeTable(hsize);
				free_ent = ClearCode + 2;
				clear_flg = true;

				Output(ClearCode, outs);
			}

			// reset code table
			void ResetCodeTable(int hsize)
			{
				for (int i = 0; i < hsize; ++i)
					htab[i] = -1;
			}

			void Compress(int init_bits, Stream outs)
			{
				int fcode;
				int i /* = 0 */;
				int c;
				int ent;
				int disp;
				int hsize_reg;
				int hshift;

				// Set up the globals:  g_init_bits - initial number of bits
				g_init_bits = init_bits;

				// Set up the necessary values
				clear_flg = false;
				n_bits = g_init_bits;
				maxcode = MaxCode(n_bits);

				ClearCode = 1 << (init_bits - 1);
				EOFCode = ClearCode + 1;
				free_ent = ClearCode + 2;

				a_count = 0; // clear packet

				ent = NextPixel();

				hshift = 0;
				for (fcode = hsize; fcode < 65536; fcode *= 2)
					++hshift;
				hshift = 8 - hshift; // set hash code range bound

				hsize_reg = hsize;
				ResetCodeTable(hsize_reg); // clear hash table

				Output(ClearCode, outs);

			outer_loop:
				while ((c = NextPixel()) != EOF)
				{
					fcode = (c << maxbits) + ent;
					i = (c << hshift) ^ ent; // xor hashing

					if (htab[i] == fcode)
					{
						ent = codetab[i];
						continue;
					}
					else if (htab[i] >= 0) // non-empty slot
					{
						disp = hsize_reg - i; // secondary hash (after G. Knott)
						if (i == 0)
							disp = 1;
						do
						{
							if ((i -= disp) < 0)
								i += hsize_reg;

							if (htab[i] == fcode)
							{
								ent = codetab[i];
								goto outer_loop;
							}
						} while (htab[i] >= 0);
					}
					Output(ent, outs);
					ent = c;
					if (free_ent < maxmaxcode)
					{
						codetab[i] = free_ent++; // code -> hashtable
						htab[i] = fcode;
					}
					else
						ClearTable(outs);
				}
				// Put out the final code.
				Output(ent, outs);
				Output(EOFCode, outs);
			}

			//----------------------------------------------------------------------------
			public void Encode(Stream os)
			{
				os.WriteByte(Convert.ToByte(initCodeSize)); // write "initial code size" byte

				remaining = imgW * imgH; // reset navigation variables
				curPixel = 0;

				Compress(initCodeSize + 1, os); // compress and write the pixel data

				os.WriteByte(0); // write block terminator
			}

			// Flush the packet to disk, and reset the accumulator
			void Flush(Stream outs)
			{
				if (a_count > 0)
				{
					outs.WriteByte(Convert.ToByte(a_count));
					outs.Write(accum, 0, a_count);
					a_count = 0;
				}
			}

			int MaxCode(int n_bits)
			{
				return (1 << n_bits) - 1;
			}

			//----------------------------------------------------------------------------
			// Return the next pixel from the image
			//----------------------------------------------------------------------------
			private int NextPixel()
			{
				if (remaining == 0)
					return EOF;

				--remaining;

				int temp = curPixel + 1;
				if (temp < pixAry.GetUpperBound(0))
				{
					byte pix = pixAry[curPixel++];

					return pix & 0xff;
				}
				return 0xff;
			}

			void Output(int code, Stream outs)
			{
				cur_accum &= masks[cur_bits];

				if (cur_bits > 0)
					cur_accum |= (code << cur_bits);
				else
					cur_accum = code;

				cur_bits += n_bits;

				while (cur_bits >= 8)
				{
					Add((byte)(cur_accum & 0xff), outs);
					cur_accum >>= 8;
					cur_bits -= 8;
				}

				// If the next entry is going to be too big for the code size,
				// then increase it, if possible.
				if (free_ent > maxcode || clear_flg)
				{
					if (clear_flg)
					{
						maxcode = MaxCode(n_bits = g_init_bits);
						clear_flg = false;
					}
					else
					{
						++n_bits;
						if (n_bits == maxbits)
							maxcode = maxmaxcode;
						else
							maxcode = MaxCode(n_bits);
					}
				}

				if (code == EOFCode)
				{
					// At EOF, write the rest of the buffer.
					while (cur_bits > 0)
					{
						Add((byte)(cur_accum & 0xff), outs);
						cur_accum >>= 8;
						cur_bits -= 8;
					}

					Flush(outs);
				}
			}
		}

		// Get / Set
		//----------------------------------------------
	}

}
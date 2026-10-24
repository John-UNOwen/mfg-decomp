using System;
using System.Runtime.InteropServices;
using Scope;
using UnityEngine;

public class PluginAccess
{
	private int mInitializeFlag;

	private RenderTexture mRenderTexture;

	private Texture2D mTexture2D;

	private int mInputBufferNum = 1024;

	private IntPtr mCallbackFunc = IntPtr.Zero;

	[DllImport("Dx11UnityGraphicPlugin")]
	private static extern void ShowValue(int x);

	[DllImport("Dx11UnityGraphicPlugin")]
	private static extern IntPtr GetRenderEventFunc();

	[DllImport("Dx11UnityGraphicPlugin")]
	private static extern void SetRenderCallback(IntPtr pFunc, IntPtr pUser, int EnableFlag);

	[DllImport("Dx11UnityGraphicPlugin")]
	private static extern IntPtr GetDxDevice();

	[DllImport("Dx11HighLevelScopeControllerPlugin")]
	private static extern int PluginCreate();

	[DllImport("Dx11HighLevelScopeControllerPlugin")]
	private static extern void PluginDestroy();

	[DllImport("Dx11HighLevelScopeControllerPlugin")]
	private static extern int ScopeSetup(IntPtr pDxDevice, IntPtr pDxTexture, int VideoWidth, int VideoHeight, int InputBufferNum);

	[DllImport("Dx11HighLevelScopeControllerPlugin")]
	private static extern void ScopeCleanup();

	[DllImport("Dx11HighLevelScopeControllerPlugin")]
	private static extern IntPtr GetCaptureCallbackFunction();

	[DllImport("Dx11HighLevelScopeControllerPlugin")]
	private unsafe static extern int GetInputDataPlus(byte* pBuffer, int BufferByteSize, int* pResult);

	[DllImport("Dx11HighLevelScopeControllerPlugin")]
	private static extern void SetCopyTexture(IntPtr pDxDevice, IntPtr pTextureSrc, IntPtr pTextureDst, int Flag);

	[DllImport("Dx11HighLevelScopeControllerPlugin")]
	private static extern IntPtr CreateRenderTargetTexture(IntPtr pDxDevice, int VideoWidth, int VideoHeight);

	public PluginAccess()
	{
		mInitializeFlag = 0;
	}

	public int Initiate(Camera pCamera, int width, int height)
	{
		AllocateTexture(pCamera, width, height, out var tex, out var rt);
		return Initiate(tex, rt);
	}

	public void Terminate(Camera pCamera)
	{
		Terminate();
		ReleaseTexture(pCamera);
	}

	private void AllocateTexture(Camera pCamera, int width, int height, out Texture2D tex, out RenderTexture rt)
	{
		Debug.Log($"CameraWidth={pCamera.pixelWidth}, CameraHeight={pCamera.pixelHeight}, w={width}, h={height}, ScreenWidth={Screen.width}, ScreenHeight={Screen.height}");
		rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
		rt.Create();
		pCamera.targetTexture = rt;
		tex = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false)
		{
			filterMode = FilterMode.Point
		};
		tex.Apply();
	}

	private void ReleaseTexture(Camera pCamera)
	{
		pCamera.targetTexture = null;
		if (mRenderTexture != null)
		{
			mRenderTexture.Release();
			mRenderTexture = null;
		}
	}

	private int Initiate(Texture2D pTexture2D, RenderTexture pRenderTexture)
	{
		if (mInitializeFlag == 1)
		{
			return 1;
		}
		mTexture2D = pTexture2D;
		mRenderTexture = pRenderTexture;
		int width = mTexture2D.width;
		int height = mTexture2D.height;
		if (PluginCreate() != 0)
		{
			return 2;
		}
		IntPtr dxDevice = GetDxDevice();
		IntPtr nativeTexturePtr = mTexture2D.GetNativeTexturePtr();
		if (ScopeSetup(dxDevice, nativeTexturePtr, width, height, mInputBufferNum) != 0)
		{
			return 3;
		}
		SetRenderCallback(GetCaptureCallbackFunction(), IntPtr.Zero, 1);
		mCallbackFunc = GetRenderEventFunc();
		SetCopyTexture(dxDevice, mRenderTexture.GetNativeTexturePtr(), nativeTexturePtr, 1);
		mInitializeFlag = 1;
		return 0;
	}

	private void Terminate()
	{
		if (mInitializeFlag != 0)
		{
			mInitializeFlag = 0;
			SetRenderCallback(IntPtr.Zero, IntPtr.Zero, 0);
			ScopeCleanup();
			PluginDestroy();
			mTexture2D = null;
		}
	}

	public void SetCameraData()
	{
	}

	public void VideoCaptureAndEncode()
	{
		if (mInitializeFlag != 0)
		{
			GL.IssuePluginEvent(mCallbackFunc, 1);
		}
	}

	public unsafe int GetInputData(InputData[] InputData, int[] Result)
	{
		if (Result.Length < 4)
		{
			return -1;
		}
		int inputDataPlus;
		fixed (InputData* pBuffer = InputData)
		{
			int bufferByteSize = InputData.Length * Marshal.SizeOf(typeof(InputData));
			fixed (int* pResult = Result)
			{
				inputDataPlus = GetInputDataPlus((byte*)pBuffer, bufferByteSize, pResult);
			}
		}
		return inputDataPlus;
	}

	public void AudioCapture(float[] data, int channels)
	{
	}
}

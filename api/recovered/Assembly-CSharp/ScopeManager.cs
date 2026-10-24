using System;
using System.Diagnostics;
using System.Text;
using AOT;
using Scope;
using TestModeSkeleton;
using UnityEngine;

public class ScopeManager : MonoBehaviour
{
	[SerializeField]
	private GameObject confirmDialog;

	[SerializeField]
	private GameObject confirmDialogMessageNormal;

	[SerializeField]
	private GameObject confirmDialogMessageLeague;

	private static IntPtr _hWndMain;

	private IntPtr _hWndSend;

	private IntPtr _hWndGame;

	public static ScopeManager Instance;

	private static Win32Utility.WINDOWINFO windowInfo;

	private void OnDestroy()
	{
		Instance = null;
	}

	private void Start()
	{
		Instance = this;
		UpdateSendWindowHandle();
		Win32Utility.MinimizeWindow(_hWndSend);
		UnityEngine.Debug.Log($"ScopeManager: Spec={CabinetUtility.GetSpecCode()}");
		UnityEngine.Debug.Log($"ScopeManager: IsPC={IsPC()}");
		UnityEngine.Debug.Log($"ScopeManager: IsAndroid={IsAndroid()}");
		UnityEngine.Debug.Log($"ScopeManager: IsIos={IsIos()}");
		UnityEngine.Debug.Log($"ScopeManager: IsPayByCP={IsPayByCP()}");
	}

	private void UpdateSendWindowHandle()
	{
		Win32Utility.EnumWindows(EnumWindowCallBack, IntPtr.Zero);
		if (_hWndMain != IntPtr.Zero)
		{
			_hWndSend = _hWndMain;
		}
		UnityEngine.Debug.Log($"scope hWndMain {(int)_hWndMain:X8}");
		UnityEngine.Debug.Log($"scope hWndSend {(int)_hWndSend:X8}");
	}

	[MonoPInvokeCallback(typeof(Win32Utility.EnumWindowsDelegate))]
	private static bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam)
	{
		Process currentProcess = Process.GetCurrentProcess();
		Win32Utility.GetWindowThreadProcessId(hWnd, out var lpdwProcessId);
		if (lpdwProcessId == currentProcess.Id)
		{
			int windowTextLength = Win32Utility.GetWindowTextLength(hWnd);
			if (windowTextLength > 0)
			{
				StringBuilder stringBuilder = new StringBuilder(windowTextLength + 1);
				Win32Utility.GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
				UnityEngine.Debug.Log($"EnumWindowCallBack: hWnd{hWnd}, name:{stringBuilder}");
				string value = "UKS";
				if (stringBuilder.ToString().Contains(value))
				{
					_hWndMain = hWnd;
					Win32Utility.GetWindowInfo(hWnd, out windowInfo);
				}
			}
		}
		return true;
	}

	public static bool IsPC()
	{
		return CabinetUtility.GetSpecCode() == 'K';
	}

	public static bool IsAndroid()
	{
		return CabinetUtility.GetSpecCode() == 'N';
	}

	public static bool IsIos()
	{
		return CabinetUtility.GetSpecCode() == 'O';
	}

	public static bool IsPayByCP()
	{
		if (!IsAndroid())
		{
			return IsIos();
		}
		return true;
	}

	private void OnApplicationFocus(bool focus)
	{
	}

	private void CloseCallback(InputClose d)
	{
		UnityEngine.Debug.Log($"ScopeInput({d.type}): <Close>");
		Application.Quit();
	}

	private void ClientSignalCallback(InputAppDefinedData d)
	{
	}

	public void OnConfirmYes()
	{
	}

	public void OnConfirmNo()
	{
		confirmDialog.SetActive(value: false);
		UnityEngine.Debug.Log("Confirm Off");
	}

	private void Update()
	{
	}
}

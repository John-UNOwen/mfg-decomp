using System.Collections.Generic;
using Dx11HighLevelScopeControllerPlugin;
using Scope;
using UnityEngine;

public class CloudStreamingInputDevice
{
	public struct MBKey
	{
		public bool down;

		public bool hold;

		public bool up;
	}

	public TouchPanelTelemetry TouchPanelTelemetry = new TouchPanelTelemetry();

	private static PluginAccess mPluginAccess = null;

	private static InputData[] mInputData = null;

	private static int InputDataNum = 32;

	private static int[] mResult = null;

	private static uint _frameCounter = 0u;

	private static uint _lastMouseButtonFrame = 0u;

	private static uint _triggerLossStep = 0u;

	private static bool scope_mouse_button0_on = false;

	private static bool scope_mouse_button0_on_prev = false;

	private static int scope_mouse_pos_x = 0;

	private static int scope_mouse_pos_y = 0;

	public static bool isTouch = false;

	private static MBKey[] mbKey = new MBKey[256];

	public static bool Open()
	{
		bool flag = false;
		try
		{
			mPluginAccess = PluginSingleton.Instance.gPluginAccess;
			mInputData = new InputData[InputDataNum];
			mResult = new int[4];
			Debug.Log("cloud streaming touch device: opened");
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static void Close()
	{
	}

	public static void Update()
	{
		DataReceivedHandler();
	}

	private static void DataReceivedHandler()
	{
		_frameCounter++;
		scope_mouse_button0_on_prev = scope_mouse_button0_on;
		for (uint num = 0u; num < mbKey.Length; num++)
		{
			mbKey[num].up = false;
			mbKey[num].down = false;
		}
		if (mPluginAccess.GetInputData(mInputData, mResult) == 0)
		{
			int num2 = mResult[0];
			for (int i = 0; i < num2; i++)
			{
				switch (mInputData[i].type)
				{
				case 5:
					MouseMoveCallback(mInputData[i].mouseMoveDataVer2);
					break;
				case 6:
					MouseButtonCallback(mInputData[i].mouseButtonDataVer2);
					break;
				case 2:
					KeyboardCallback(mInputData[i].keyboardDataVer2);
					break;
				case 1:
					TouchCallback(mInputData[i].touchDataVer2);
					break;
				case 8:
					ResidentFlow.AppTerminate();
					break;
				case 10:
					if (mInputData[i].appDefinedData.IsAppTerminateConfirmRequest && !ResidentFlow.GetInstance().IsAppTerminateConfirmWindowOpen)
					{
						ResidentFlow.GetInstance().IsAppTerminateConfirmRequest = true;
					}
					break;
				default:
					Debug.Log($"ScopeInput({mInputData[i].type}): 未実装の入力データです({(InputType)mInputData[i].type})");
					break;
				case 3:
					break;
				}
			}
		}
		switch (_triggerLossStep)
		{
		case 1u:
			scope_mouse_button0_on = true;
			_triggerLossStep = 2u;
			break;
		case 2u:
			scope_mouse_button0_on = false;
			_triggerLossStep = 0u;
			break;
		}
		if (scope_mouse_button0_on && !scope_mouse_button0_on_prev)
		{
			isTouch = true;
		}
		else if (!scope_mouse_button0_on && scope_mouse_button0_on_prev)
		{
			isTouch = false;
		}
		else if (scope_mouse_button0_on && scope_mouse_button0_on_prev)
		{
			isTouch = false;
		}
	}

	public static bool GetScopeMouseButtonDown(int button)
	{
		if (scope_mouse_button0_on)
		{
			return !scope_mouse_button0_on_prev;
		}
		return false;
	}

	public static bool GetScopeMouseButtonUp(int button)
	{
		if (!scope_mouse_button0_on)
		{
			return scope_mouse_button0_on_prev;
		}
		return false;
	}

	public static bool GetScopeMouseButton(int button)
	{
		return scope_mouse_button0_on;
	}

	public static Vector3 GetScopeMousePosition()
	{
		return new Vector3(scope_mouse_pos_x, scope_mouse_pos_y, 0f);
	}

	public static bool GetKey(KeyCode keyCode)
	{
		uint scopeKeyCode = GetScopeKeyCode(keyCode);
		if (scopeKeyCode < mbKey.Length)
		{
			return mbKey[scopeKeyCode].hold;
		}
		return Input.GetKey(keyCode);
	}

	public static bool GetKeyDown(KeyCode keyCode)
	{
		uint scopeKeyCode = GetScopeKeyCode(keyCode);
		if (scopeKeyCode < mbKey.Length)
		{
			return mbKey[scopeKeyCode].down;
		}
		return Input.GetKeyDown(keyCode);
	}

	public static bool GetKeyUp(KeyCode keyCode)
	{
		uint scopeKeyCode = GetScopeKeyCode(keyCode);
		if (scopeKeyCode < mbKey.Length)
		{
			return mbKey[scopeKeyCode].up;
		}
		return Input.GetKeyUp(keyCode);
	}

	public static bool GetScopeKey(uint scopeKeyCode)
	{
		if (scopeKeyCode < mbKey.Length)
		{
			return mbKey[scopeKeyCode].hold;
		}
		return false;
	}

	public static bool GetScopeKeyDown(uint scopeKeyCode)
	{
		if (scopeKeyCode < mbKey.Length)
		{
			return mbKey[scopeKeyCode].down;
		}
		return false;
	}

	public static bool GetScopeKeyUp(uint scopeKeyCode)
	{
		if (scopeKeyCode < mbKey.Length)
		{
			return mbKey[scopeKeyCode].up;
		}
		return false;
	}

	private static uint GetScopeKeyCode(KeyCode keyCode)
	{
		Dictionary<KeyCode, uint> dictionary = new Dictionary<KeyCode, uint>
		{
			{
				KeyCode.None,
				0u
			},
			{
				KeyCode.Escape,
				1u
			},
			{
				KeyCode.Alpha1,
				2u
			},
			{
				KeyCode.Alpha2,
				3u
			},
			{
				KeyCode.Alpha3,
				4u
			},
			{
				KeyCode.Alpha4,
				5u
			},
			{
				KeyCode.Alpha5,
				6u
			},
			{
				KeyCode.Alpha6,
				7u
			},
			{
				KeyCode.Alpha7,
				8u
			},
			{
				KeyCode.Alpha8,
				9u
			},
			{
				KeyCode.Alpha9,
				10u
			},
			{
				KeyCode.Alpha0,
				11u
			},
			{
				KeyCode.Minus,
				12u
			},
			{
				KeyCode.Backspace,
				14u
			},
			{
				KeyCode.Tab,
				15u
			},
			{
				KeyCode.Q,
				16u
			},
			{
				KeyCode.W,
				17u
			},
			{
				KeyCode.E,
				18u
			},
			{
				KeyCode.R,
				19u
			},
			{
				KeyCode.T,
				20u
			},
			{
				KeyCode.Y,
				21u
			},
			{
				KeyCode.U,
				22u
			},
			{
				KeyCode.I,
				23u
			},
			{
				KeyCode.O,
				24u
			},
			{
				KeyCode.P,
				25u
			},
			{
				KeyCode.Return,
				28u
			},
			{
				KeyCode.LeftControl,
				29u
			},
			{
				KeyCode.A,
				30u
			},
			{
				KeyCode.S,
				31u
			},
			{
				KeyCode.D,
				32u
			},
			{
				KeyCode.F,
				33u
			},
			{
				KeyCode.G,
				34u
			},
			{
				KeyCode.H,
				35u
			},
			{
				KeyCode.J,
				36u
			},
			{
				KeyCode.K,
				37u
			},
			{
				KeyCode.L,
				38u
			},
			{
				KeyCode.Semicolon,
				39u
			},
			{
				KeyCode.LeftShift,
				42u
			},
			{
				KeyCode.Backslash,
				43u
			},
			{
				KeyCode.Z,
				44u
			},
			{
				KeyCode.X,
				45u
			},
			{
				KeyCode.C,
				46u
			},
			{
				KeyCode.V,
				47u
			},
			{
				KeyCode.B,
				48u
			},
			{
				KeyCode.N,
				49u
			},
			{
				KeyCode.M,
				50u
			},
			{
				KeyCode.Comma,
				51u
			},
			{
				KeyCode.Period,
				52u
			},
			{
				KeyCode.Slash,
				53u
			},
			{
				KeyCode.RightShift,
				54u
			},
			{
				KeyCode.KeypadMultiply,
				55u
			},
			{
				KeyCode.LeftAlt,
				56u
			},
			{
				KeyCode.Space,
				57u
			},
			{
				KeyCode.CapsLock,
				58u
			},
			{
				KeyCode.F1,
				59u
			},
			{
				KeyCode.F2,
				60u
			},
			{
				KeyCode.F3,
				61u
			},
			{
				KeyCode.F4,
				62u
			},
			{
				KeyCode.F5,
				63u
			},
			{
				KeyCode.F6,
				64u
			},
			{
				KeyCode.F7,
				65u
			},
			{
				KeyCode.F8,
				66u
			},
			{
				KeyCode.F9,
				67u
			},
			{
				KeyCode.F10,
				68u
			},
			{
				KeyCode.Numlock,
				69u
			},
			{
				KeyCode.ScrollLock,
				70u
			},
			{
				KeyCode.Keypad7,
				71u
			},
			{
				KeyCode.Keypad8,
				72u
			},
			{
				KeyCode.Keypad9,
				73u
			},
			{
				KeyCode.KeypadMinus,
				74u
			},
			{
				KeyCode.Keypad4,
				75u
			},
			{
				KeyCode.Keypad5,
				76u
			},
			{
				KeyCode.Keypad6,
				77u
			},
			{
				KeyCode.KeypadPlus,
				78u
			},
			{
				KeyCode.Keypad1,
				79u
			},
			{
				KeyCode.Keypad2,
				80u
			},
			{
				KeyCode.Keypad3,
				81u
			},
			{
				KeyCode.Keypad0,
				82u
			},
			{
				KeyCode.KeypadPeriod,
				83u
			},
			{
				KeyCode.F11,
				85u
			},
			{
				KeyCode.F12,
				86u
			},
			{
				KeyCode.F13,
				87u
			},
			{
				KeyCode.F14,
				88u
			},
			{
				KeyCode.F15,
				89u
			},
			{
				KeyCode.Equals,
				97u
			},
			{
				KeyCode.KeypadEnter,
				106u
			},
			{
				KeyCode.RightControl,
				107u
			},
			{
				KeyCode.KeypadDivide,
				116u
			},
			{
				KeyCode.RightAlt,
				118u
			},
			{
				KeyCode.Pause,
				119u
			},
			{
				KeyCode.Home,
				120u
			},
			{
				KeyCode.UpArrow,
				121u
			},
			{
				KeyCode.PageUp,
				122u
			},
			{
				KeyCode.LeftArrow,
				123u
			},
			{
				KeyCode.RightArrow,
				124u
			},
			{
				KeyCode.End,
				125u
			},
			{
				KeyCode.DownArrow,
				126u
			},
			{
				KeyCode.PageDown,
				127u
			},
			{
				KeyCode.Insert,
				128u
			},
			{
				KeyCode.Delete,
				129u
			},
			{
				KeyCode.LeftWindows,
				130u
			},
			{
				KeyCode.RightWindows,
				131u
			}
		};
		if (dictionary.ContainsKey(keyCode))
		{
			return dictionary[keyCode];
		}
		return 0u;
	}

	private static uint GetWindowsVirtualKeyCode(uint key)
	{
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>
		{
			{ 0u, 0u },
			{ 1u, 27u },
			{ 2u, 49u },
			{ 3u, 50u },
			{ 4u, 51u },
			{ 5u, 52u },
			{ 6u, 53u },
			{ 7u, 54u },
			{ 8u, 55u },
			{ 9u, 56u },
			{ 10u, 57u },
			{ 11u, 48u },
			{ 12u, 189u },
			{ 14u, 8u },
			{ 15u, 9u },
			{ 16u, 81u },
			{ 17u, 87u },
			{ 18u, 69u },
			{ 19u, 82u },
			{ 20u, 84u },
			{ 21u, 89u },
			{ 22u, 85u },
			{ 23u, 73u },
			{ 24u, 79u },
			{ 25u, 80u },
			{ 28u, 13u },
			{ 29u, 162u },
			{ 30u, 65u },
			{ 31u, 83u },
			{ 32u, 68u },
			{ 33u, 70u },
			{ 34u, 71u },
			{ 35u, 72u },
			{ 36u, 74u },
			{ 37u, 75u },
			{ 38u, 76u },
			{ 39u, 186u },
			{ 41u, 192u },
			{ 42u, 160u },
			{ 43u, 220u },
			{ 44u, 90u },
			{ 45u, 88u },
			{ 46u, 67u },
			{ 47u, 86u },
			{ 48u, 66u },
			{ 49u, 78u },
			{ 50u, 77u },
			{ 51u, 188u },
			{ 52u, 190u },
			{ 53u, 191u },
			{ 54u, 161u },
			{ 55u, 106u },
			{ 56u, 164u },
			{ 57u, 32u },
			{ 58u, 20u },
			{ 59u, 112u },
			{ 60u, 113u },
			{ 61u, 114u },
			{ 62u, 115u },
			{ 63u, 116u },
			{ 64u, 117u },
			{ 65u, 118u },
			{ 66u, 119u },
			{ 67u, 120u },
			{ 68u, 121u },
			{ 69u, 144u },
			{ 70u, 145u },
			{ 71u, 103u },
			{ 72u, 104u },
			{ 73u, 105u },
			{ 74u, 109u },
			{ 75u, 100u },
			{ 76u, 101u },
			{ 77u, 102u },
			{ 78u, 107u },
			{ 79u, 97u },
			{ 80u, 98u },
			{ 81u, 99u },
			{ 82u, 96u },
			{ 83u, 110u },
			{ 84u, 226u },
			{ 85u, 122u },
			{ 86u, 123u },
			{ 87u, 124u },
			{ 88u, 125u },
			{ 89u, 126u },
			{ 90u, 21u },
			{ 91u, 0u },
			{ 92u, 28u },
			{ 93u, 29u },
			{ 94u, 220u },
			{ 97u, 222u },
			{ 106u, 108u },
			{ 107u, 163u },
			{ 115u, 110u },
			{ 116u, 111u },
			{ 118u, 165u },
			{ 119u, 19u },
			{ 120u, 36u },
			{ 121u, 38u },
			{ 122u, 33u },
			{ 123u, 37u },
			{ 124u, 39u },
			{ 125u, 35u },
			{ 126u, 40u },
			{ 127u, 34u },
			{ 128u, 45u },
			{ 129u, 46u },
			{ 130u, 91u },
			{ 131u, 92u },
			{ 132u, 93u }
		};
		if (dictionary.ContainsKey(key))
		{
			return dictionary[key];
		}
		return 0u;
	}

	private static void MouseMoveCallback(InputMouseMoveDataVer2 d)
	{
		scope_mouse_pos_x = d.PointX;
		scope_mouse_pos_y = ScreenHeight() - d.PointY;
	}

	private static int ScreenHeight()
	{
		return ResidentFlow.GetInstance().MainCamera().pixelHeight;
	}

	private static void MouseButtonCallback(InputMouseButtonDataVer2 d)
	{
		if (d.Button == 0)
		{
			if (_lastMouseButtonFrame == _frameCounter && scope_mouse_button0_on && d.Action != 0)
			{
				_triggerLossStep = 1u;
			}
			scope_mouse_button0_on = d.Action == 0;
			_lastMouseButtonFrame = _frameCounter;
		}
	}

	private static void TouchCallback(InputTouchDataVer2 d)
	{
		Debug.Log($"ScopeInput({d.type}): <Touch> idx={d.MultiTouchIndex} act={d.Action} pos={d.PointX},{d.PointY} opt={d.OptionType} st={d.State} t={d.LocalTime} num={d.MultiTouchNum} max={d.MultiTouchMax}");
		scope_mouse_pos_x = d.PointX;
		scope_mouse_pos_y = ScreenHeight() - d.PointY;
		if (d.Action == 0)
		{
			scope_mouse_button0_on = true;
		}
		if (d.Action == 1)
		{
			scope_mouse_button0_on = false;
		}
	}

	private static void KeyboardCallback(InputKeyboardDataVer2 d)
	{
		if (d.Key < mbKey.Length)
		{
			if (d.Action == 1)
			{
				mbKey[d.Key].down = !mbKey[d.Key].hold;
				mbKey[d.Key].hold = true;
			}
			else
			{
				mbKey[d.Key].up = true;
				mbKey[d.Key].hold = false;
			}
		}
	}
}

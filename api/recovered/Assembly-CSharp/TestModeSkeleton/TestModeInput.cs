using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IO;
using UnityEngine;

namespace TestModeSkeleton;

public class TestModeInput : SingletonBehaviour<TestModeInput>
{
	public enum Key
	{
		Up,
		Down,
		Left,
		Right,
		Enter,
		Test,
		Service,
		Coin,
		Tenkey0,
		Tenkey1,
		Tenkey2,
		Tenkey3,
		Tenkey4,
		Tenkey5,
		Tenkey6,
		Tenkey7,
		Tenkey8,
		Tenkey9,
		Tenkey00,
		TenkeyClear,
		Trigger1,
		Trigger2,
		JoystickButton,
		Button1,
		Button2,
		Button3,
		Button4,
		HeadPhone
	}

	public class SwitchingOperation
	{
		private enum State
		{
			Add,
			Sub
		}

		private State state;

		private Key targetKey;

		public SwitchingOperation(Key key)
		{
			targetKey = key;
			Reset();
		}

		public void Reset()
		{
			state = State.Add;
		}

		public void Update()
		{
			if (SingletonBehaviour<TestModeInput>.Instance.IsRelease(targetKey))
			{
				state = ((state == State.Add) ? State.Sub : State.Add);
			}
		}

		public bool IsAdd()
		{
			if (state == State.Add)
			{
				return SingletonBehaviour<TestModeInput>.Instance.IsRepeat(targetKey);
			}
			return false;
		}

		public bool IsSub()
		{
			if (state == State.Sub)
			{
				return SingletonBehaviour<TestModeInput>.Instance.IsRepeat(targetKey);
			}
			return false;
		}
	}

	[SerializeField]
	[Tooltip("リピート開始の秒数")]
	private float repeatDelaySec = 0.3f;

	[SerializeField]
	[Tooltip("リピート間隔の秒数")]
	private float repeatIntervalSec = 0.1f;

	[SerializeField]
	[Tooltip("テストモードテンキー入力")]
	private TestModeTenkey tenkey;

	[SerializeField]
	private TouchArrowKey[] m_key;

	[SerializeField]
	private TouchArrowKey m_ok;

	private float[] pressSec = new float[CabinetUtility.EnumLength<Key>()];

	private float[] repeatSec = new float[CabinetUtility.EnumLength<Key>()];

	private readonly Dictionary<Key, string> TenkeyToString = new Dictionary<Key, string>
	{
		[Key.Tenkey0] = "0",
		[Key.Tenkey1] = "1",
		[Key.Tenkey2] = "2",
		[Key.Tenkey3] = "3",
		[Key.Tenkey4] = "4",
		[Key.Tenkey5] = "5",
		[Key.Tenkey6] = "6",
		[Key.Tenkey7] = "7",
		[Key.Tenkey8] = "8",
		[Key.Tenkey9] = "9",
		[Key.Tenkey00] = "00"
	};

	public ReadOnlyDictionary<Key, string> CreateTenkeyToStringDictionary()
	{
		return new ReadOnlyDictionary<Key, string>(TenkeyToString);
	}

	private IEnumerator Start()
	{
		while (true)
		{
			for (int i = 0; i < CabinetUtility.EnumLength<Key>(); i++)
			{
				if (IsPress((Key)i))
				{
					pressSec[i] += Time.deltaTime;
					repeatSec[i] += Time.deltaTime;
					if (repeatSec[i] - repeatDelaySec >= repeatIntervalSec)
					{
						repeatSec[i] = repeatDelaySec;
					}
				}
				else
				{
					pressSec[i] = 0f;
					repeatSec[i] = 0f;
				}
			}
			tenkey.Clear();
			yield return null;
		}
	}

	public Vector2 GetTouchPosition()
	{
		return Bi2xInput.Instance.GetTouchPosition();
	}

	public Vector2 GetNoAdjustTouchPosition()
	{
		return Bi2xInput.Instance.GetNoAdjustTouchPosition();
	}

	public bool IsTouchTrigger()
	{
		return Bi2xInput.Instance.IsTouchTrigger();
	}

	public bool IsTouchPress()
	{
		return Bi2xInput.Instance.IsTouchPress();
	}

	public void Clear()
	{
		Array.Clear(pressSec, 0, pressSec.Length);
		Array.Clear(repeatSec, 0, repeatSec.Length);
	}

	public bool IsTrigger(Key key)
	{
		switch (key)
		{
		case Key.Up:
			if (!m_key[0].IsTrigger && !Bi2xInput.GetKeyDown(KeyCode.UpArrow))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N8);
			}
			return true;
		case Key.Down:
			if (!m_key[1].IsTrigger && !Bi2xInput.GetKeyDown(KeyCode.DownArrow))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N2);
			}
			return true;
		case Key.Left:
			if (!m_key[2].IsTrigger && !Bi2xInput.GetKeyDown(KeyCode.LeftArrow))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N4);
			}
			return true;
		case Key.Right:
			if (!m_key[3].IsTrigger && !Bi2xInput.GetKeyDown(KeyCode.RightArrow))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N6);
			}
			return true;
		case Key.Enter:
			if (!m_ok.IsTrigger)
			{
				return Bi2xInput.GetKeyDown(KeyCode.Return);
			}
			return true;
		case Key.Test:
			if (!Bi2xInput.IsTrigger(BUTTON.TEST))
			{
				return Bi2xInput.GetKeyDown(KeyCode.T);
			}
			return true;
		case Key.Service:
			if (!Bi2xInput.IsTrigger(BUTTON.SERVICE))
			{
				return Bi2xInput.GetKeyDown(KeyCode.S);
			}
			return true;
		case Key.Coin:
			if (!Bi2xInput.IsTrigger(BUTTON.COIN))
			{
				return Input.GetKeyDown(KeyCode.C);
			}
			return true;
		case Key.Tenkey0:
			if (!Input.GetKeyDown(KeyCode.Keypad0))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N0);
			}
			return true;
		case Key.Tenkey1:
			if (!Input.GetKeyDown(KeyCode.Keypad1))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N1);
			}
			return true;
		case Key.Tenkey2:
			if (!Input.GetKeyDown(KeyCode.Keypad2))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N2);
			}
			return true;
		case Key.Tenkey3:
			if (!Input.GetKeyDown(KeyCode.Keypad3))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N3);
			}
			return true;
		case Key.Tenkey4:
			if (!Input.GetKeyDown(KeyCode.Keypad4))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N4);
			}
			return true;
		case Key.Tenkey5:
			if (!Input.GetKeyDown(KeyCode.Keypad5))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N5);
			}
			return true;
		case Key.Tenkey6:
			if (!Input.GetKeyDown(KeyCode.Keypad6))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N6);
			}
			return true;
		case Key.Tenkey7:
			if (!Input.GetKeyDown(KeyCode.Keypad7))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N7);
			}
			return true;
		case Key.Tenkey8:
			if (!Input.GetKeyDown(KeyCode.Keypad8))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N8);
			}
			return true;
		case Key.Tenkey9:
			if (!Input.GetKeyDown(KeyCode.Keypad9))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N9);
			}
			return true;
		case Key.Tenkey00:
			if (!Input.GetKeyDown(KeyCode.KeypadPeriod))
			{
				return tenkey.IsClick(TestModeTenkey.Click.N00);
			}
			return true;
		case Key.TenkeyClear:
			if (!Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				return tenkey.IsClick(TestModeTenkey.Click.Clear);
			}
			return true;
		case Key.Button2:
			return Bi2xInput.IsTrigger(BUTTON.Q_BUTTON_1);
		case Key.Button3:
			return Bi2xInput.IsTrigger(BUTTON.Q_BUTTON_2);
		case Key.Button4:
			return Bi2xInput.IsTrigger(BUTTON.Q_BUTTON_3);
		case Key.Button1:
			return Bi2xInput.IsTrigger(BUTTON.Q_BUTTON);
		case Key.HeadPhone:
			return Bi2xInput.Instance.IsHeadphoneJacking();
		default:
			return false;
		}
	}

	public bool IsRelease(Key key)
	{
		switch (key)
		{
		case Key.Up:
			if (!m_key[0].IsRelease)
			{
				return Bi2xInput.GetKeyUp(KeyCode.UpArrow);
			}
			return true;
		case Key.Down:
			if (!m_key[1].IsRelease)
			{
				return Bi2xInput.GetKeyUp(KeyCode.DownArrow);
			}
			return true;
		case Key.Left:
			if (!m_key[2].IsRelease)
			{
				return Bi2xInput.GetKeyUp(KeyCode.LeftArrow);
			}
			return true;
		case Key.Right:
			if (!m_key[3].IsRelease)
			{
				return Bi2xInput.GetKeyUp(KeyCode.RightArrow);
			}
			return true;
		case Key.Enter:
			if (!m_ok.IsRelease)
			{
				return Bi2xInput.GetKeyUp(KeyCode.Return);
			}
			return true;
		case Key.Test:
			if (!Bi2xInput.IsRelease(BUTTON.TEST))
			{
				return Bi2xInput.GetKeyUp(KeyCode.T);
			}
			return true;
		case Key.Service:
			if (!Bi2xInput.IsRelease(BUTTON.SERVICE))
			{
				return Bi2xInput.GetKeyUp(KeyCode.S);
			}
			return true;
		case Key.Coin:
			if (!Bi2xInput.IsRelease(BUTTON.COIN))
			{
				return Input.GetKeyUp(KeyCode.C);
			}
			return true;
		case Key.Tenkey0:
			return Input.GetKeyUp(KeyCode.Keypad0);
		case Key.Tenkey1:
			return Input.GetKeyUp(KeyCode.Keypad1);
		case Key.Tenkey2:
			return Input.GetKeyUp(KeyCode.Keypad2);
		case Key.Tenkey3:
			return Input.GetKeyUp(KeyCode.Keypad3);
		case Key.Tenkey4:
			return Input.GetKeyUp(KeyCode.Keypad4);
		case Key.Tenkey5:
			return Input.GetKeyUp(KeyCode.Keypad5);
		case Key.Tenkey6:
			return Input.GetKeyUp(KeyCode.Keypad6);
		case Key.Tenkey7:
			return Input.GetKeyUp(KeyCode.Keypad7);
		case Key.Tenkey8:
			return Input.GetKeyUp(KeyCode.Keypad8);
		case Key.Tenkey9:
			return Input.GetKeyUp(KeyCode.Keypad9);
		case Key.Tenkey00:
			return Input.GetKeyUp(KeyCode.KeypadPeriod);
		case Key.TenkeyClear:
			return Input.GetKeyUp(KeyCode.KeypadEnter);
		case Key.Button2:
			return Bi2xInput.IsRelease(BUTTON.Q_BUTTON_1);
		case Key.Button3:
			return Bi2xInput.IsRelease(BUTTON.Q_BUTTON_2);
		case Key.Button4:
			return Bi2xInput.IsRelease(BUTTON.Q_BUTTON_3);
		case Key.Button1:
			return Bi2xInput.IsRelease(BUTTON.Q_BUTTON);
		case Key.HeadPhone:
			return !Bi2xInput.Instance.IsHeadphoneJacking();
		default:
			return false;
		}
	}

	public void ResetArrowKey()
	{
		m_key[0].ResetState();
		m_key[1].ResetState();
		m_key[2].ResetState();
		m_key[3].ResetState();
		m_ok.ResetState();
	}

	public bool IsPress(Key key)
	{
		switch (key)
		{
		case Key.Up:
			if (!m_key[0].IsPress)
			{
				return Bi2xInput.GetKey(KeyCode.UpArrow);
			}
			return true;
		case Key.Down:
			if (!m_key[1].IsPress)
			{
				return Bi2xInput.GetKey(KeyCode.DownArrow);
			}
			return true;
		case Key.Left:
			if (!m_key[2].IsPress)
			{
				return Bi2xInput.GetKey(KeyCode.LeftArrow);
			}
			return true;
		case Key.Right:
			if (!m_key[3].IsPress)
			{
				return Bi2xInput.GetKey(KeyCode.RightArrow);
			}
			return true;
		case Key.Enter:
			if (!m_ok.IsPress)
			{
				return Bi2xInput.GetKey(KeyCode.Return);
			}
			return true;
		case Key.Test:
			if (!Bi2xInput.IsPress(BUTTON.TEST))
			{
				return Bi2xInput.GetKey(KeyCode.T);
			}
			return true;
		case Key.Service:
			if (!Bi2xInput.IsPress(BUTTON.SERVICE))
			{
				return Bi2xInput.GetKey(KeyCode.S);
			}
			return true;
		case Key.Coin:
			if (!Bi2xInput.IsPress(BUTTON.COIN))
			{
				return Input.GetKey(KeyCode.C);
			}
			return true;
		case Key.Tenkey0:
			return Input.GetKey(KeyCode.Keypad0);
		case Key.Tenkey1:
			return Input.GetKey(KeyCode.Keypad1);
		case Key.Tenkey2:
			return Input.GetKey(KeyCode.Keypad2);
		case Key.Tenkey3:
			return Input.GetKey(KeyCode.Keypad3);
		case Key.Tenkey4:
			return Input.GetKey(KeyCode.Keypad4);
		case Key.Tenkey5:
			return Input.GetKey(KeyCode.Keypad5);
		case Key.Tenkey6:
			return Input.GetKey(KeyCode.Keypad6);
		case Key.Tenkey7:
			return Input.GetKey(KeyCode.Keypad7);
		case Key.Tenkey8:
			return Input.GetKey(KeyCode.Keypad8);
		case Key.Tenkey9:
			return Input.GetKey(KeyCode.Keypad9);
		case Key.Tenkey00:
			return Input.GetKey(KeyCode.KeypadPeriod);
		case Key.TenkeyClear:
			return Input.GetKey(KeyCode.KeypadEnter);
		case Key.Button2:
			return Bi2xInput.IsPress(BUTTON.Q_BUTTON_1);
		case Key.Button3:
			return Bi2xInput.IsPress(BUTTON.Q_BUTTON_2);
		case Key.Button4:
			return Bi2xInput.IsPress(BUTTON.Q_BUTTON_3);
		case Key.Button1:
			return Bi2xInput.IsPress(BUTTON.Q_BUTTON);
		case Key.HeadPhone:
			return Bi2xInput.Instance.IsHeadphoneJacking();
		default:
			return false;
		}
	}

	public bool IsRepeat(Key key)
	{
		if (IsTrigger(key))
		{
			return true;
		}
		return Mathf.Approximately(repeatSec[(int)key], repeatDelaySec);
	}

	public float GetSecAfterRepeat(Key key)
	{
		return Mathf.Max(pressSec[(int)key] - repeatDelaySec, 0f);
	}

	public float GetPressSec(Key key)
	{
		return pressSec[(int)key];
	}

	public bool IsMenuUp()
	{
		return IsRepeat(Key.Up);
	}

	public bool IsMenuDown()
	{
		if (!IsRepeat(Key.Down))
		{
			return IsTrigger(Key.Test);
		}
		return true;
	}

	public bool IsMenuExecute()
	{
		if (!IsTrigger(Key.Enter))
		{
			return IsTrigger(Key.Service);
		}
		return true;
	}

	public bool IsAddValue()
	{
		if (!IsRepeat(Key.Right))
		{
			return IsTrigger(Key.Service);
		}
		return true;
	}

	public bool IsSubValue()
	{
		return IsRepeat(Key.Left);
	}

	public bool IsYesOrNo()
	{
		if (!IsRepeat(Key.Left) && !IsRepeat(Key.Right))
		{
			return IsTrigger(Key.Test);
		}
		return true;
	}

	public bool IsBookKeepingPageGo()
	{
		if (!IsRepeat(Key.Right))
		{
			return IsTrigger(Key.Test);
		}
		return true;
	}

	public bool IsBookKeepingPageBack()
	{
		return IsRepeat(Key.Left);
	}

	public bool IsClockAddTimeCursor()
	{
		if (!IsRepeat(Key.Right))
		{
			return IsTrigger(Key.Test);
		}
		return true;
	}

	public bool IsClockSubTimeCursor()
	{
		return IsRepeat(Key.Left);
	}

	public bool IsClockAddOffset()
	{
		if (!IsRepeat(Key.Up))
		{
			return IsTrigger(Key.Service);
		}
		return true;
	}

	public bool IsClockSubOffset()
	{
		return IsRepeat(Key.Down);
	}

	public bool IsSpecialOperation()
	{
		if (IsPress(Key.Test) && IsTrigger(Key.Service))
		{
			return true;
		}
		return false;
	}

	public bool IsSpecialOperation2()
	{
		if (IsPress(Key.Test) && IsTrigger(Key.Service))
		{
			return true;
		}
		if (IsTrigger(Key.Enter))
		{
			return true;
		}
		return false;
	}

	public bool IsServiceStartOperation()
	{
		if (IsPress(Key.Service))
		{
			return IsTrigger(Key.Test);
		}
		return false;
	}
}

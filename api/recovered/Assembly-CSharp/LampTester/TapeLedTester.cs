using System.Collections;
using UnityEngine;

namespace LampTester;

public class TapeLedTester : MonoBehaviour
{
	private const byte Power = 127;

	protected static readonly string ON = "<color=red>ON  </color>";

	protected static readonly string OFF = "<color=white>OFF</color>";

	private static readonly string[] TAPE_COLOR_TEXT = new string[7] { "<color=white>OFF</color>", "<color=red>RED</color>", "<color=red>GREEN</color>", "<color=red>BLUE</color>", "<color=red>WHITE</color>", "<color=red>CHANGE</color>", "<color=red>SINGLE</color>" };

	private readonly LedColorTestTable[] COLOR_LED_TABLE = new LedColorTestTable[5]
	{
		new LedColorTestTable(0, 0, 0),
		new LedColorTestTable(127, 0, 0),
		new LedColorTestTable(0, 127, 0),
		new LedColorTestTable(0, 0, 127),
		new LedColorTestTable(127, 127, 127)
	};

	private int ledNum;

	private SetTapeLedFunc setTapeLedFunc;

	private SetTapeLedOneFunc setTapeLedOneFunc;

	private LedStateTextCallback ledStateTextCallback;

	public void SetFunctions(int _ledNum, SetTapeLedFunc _setTapeLedFunc, SetTapeLedOneFunc _setTapeLedOneFunc)
	{
		ledNum = _ledNum;
		setTapeLedFunc = _setTapeLedFunc;
		setTapeLedOneFunc = _setTapeLedOneFunc;
	}

	public void SetTextCallback(LedStateTextCallback _ledStateTextCallback)
	{
		ledStateTextCallback = _ledStateTextCallback;
	}

	public void Clear()
	{
		StopAllCoroutines();
		LedColorTestTable ledColorTestTable = COLOR_LED_TABLE[0];
		string text = TAPE_COLOR_TEXT[0];
		setTapeLedFunc?.Invoke(ledColorTestTable.r, ledColorTestTable.g, ledColorTestTable.b);
		ledStateTextCallback?.Invoke(text);
	}

	public int GetLedNum()
	{
		return ledNum;
	}

	public void SetState(TapeLedState_TestMode state)
	{
		StopAllCoroutines();
		if (state == TapeLedState_TestMode.SERIESE)
		{
			StartSeries(0.2f);
			return;
		}
		LedColorTestTable ledColorTestTable = COLOR_LED_TABLE[(int)state];
		string text = TAPE_COLOR_TEXT[(int)state];
		setTapeLedFunc?.Invoke(ledColorTestTable.r, ledColorTestTable.g, ledColorTestTable.b);
		ledStateTextCallback?.Invoke(text);
	}

	public void SetLedState(int ledNo, TapeLedState_TestMode state)
	{
		LedColorTestTable ledColorTestTable = COLOR_LED_TABLE[(int)state];
		_ = TAPE_COLOR_TEXT[(int)state];
		setTapeLedOneFunc?.Invoke(ledNo, ledColorTestTable.r, ledColorTestTable.g, ledColorTestTable.b);
	}

	public void StartFlashing(float sec)
	{
		StartCoroutine(CoFlashing(sec));
	}

	public void StartSeries(float sec)
	{
		StartCoroutine(CoSeries(sec));
	}

	public void StartRainbow()
	{
		StartCoroutine(CoRainbow());
	}

	public IEnumerator CoFlashing(float sec)
	{
		bool on = true;
		while (true)
		{
			LedColorTestTable ledColorTestTable = (on ? COLOR_LED_TABLE[4] : COLOR_LED_TABLE[0]);
			string text = (on ? (TAPE_COLOR_TEXT[4] + " __") : (TAPE_COLOR_TEXT[0] + " __"));
			setTapeLedFunc?.Invoke(ledColorTestTable.r, ledColorTestTable.g, ledColorTestTable.b);
			ledStateTextCallback?.Invoke(text);
			on = !on;
			yield return new WaitForSeconds(sec);
		}
	}

	public IEnumerator CoRainbow()
	{
		ledStateTextCallback?.Invoke("FULLCOLOR");
		while (true)
		{
			for (byte k = 0; k <= 127; k++)
			{
				setTapeLedFunc?.Invoke(k, (byte)(127 - k), 127);
				yield return null;
			}
			for (byte k = 0; k <= 127; k++)
			{
				setTapeLedFunc?.Invoke(127, k, (byte)(127 - k));
				yield return null;
			}
			for (byte k = 0; k <= 127; k++)
			{
				setTapeLedFunc?.Invoke((byte)(127 - k), 127, k);
				yield return null;
			}
		}
	}

	public IEnumerator CoSeries(float sec)
	{
		string valueTextBase = TAPE_COLOR_TEXT[5];
		while (true)
		{
			int i = 0;
			while (i < ledNum)
			{
				for (int j = 0; j < ledNum; j++)
				{
					setTapeLedOneFunc?.Invoke(j, 0, 0, 0);
				}
				string text = valueTextBase + "  <color=red>" + i.ToString("00") + "</color>";
				setTapeLedOneFunc?.Invoke(i, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				ledStateTextCallback?.Invoke(text);
				yield return new WaitForSeconds(sec);
				int num = i + 1;
				i = num;
			}
		}
	}
}

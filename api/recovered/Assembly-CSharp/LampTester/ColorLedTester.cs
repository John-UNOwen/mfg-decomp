using System.Collections;
using UnityEngine;

namespace LampTester;

public class ColorLedTester : MonoBehaviour
{
	private const byte Power = 127;

	protected static readonly string ON = "<color=red>ON</color>";

	protected static readonly string OFF = "<color=white>OFF</color>";

	private static readonly string[] TAPE_COLOR_TEXT = new string[5] { "<color=white>OFF</color>", "<color=red>RED</color>", "<color=red>GREEN</color>", "<color=red>BLUE</color>", "<color=red>WHITE</color>" };

	private readonly LedColorTestTable[] COLOR_LED_TABLE = new LedColorTestTable[6]
	{
		new LedColorTestTable(0, 0, 0),
		new LedColorTestTable(127, 0, 0),
		new LedColorTestTable(0, 127, 0),
		new LedColorTestTable(0, 0, 127),
		new LedColorTestTable(127, 127, 127),
		new LedColorTestTable(127, 127, 127)
	};

	private SetColorLedFunc setColorLedFunc;

	private LedStateTextCallback ledStateTextCallback;

	public void SetFunction(SetColorLedFunc func)
	{
		setColorLedFunc = func;
	}

	public void Clear()
	{
		StopAllCoroutines();
		LedColorTestTable ledColorTestTable = COLOR_LED_TABLE[0];
		string text = TAPE_COLOR_TEXT[0];
		setColorLedFunc?.Invoke(ledColorTestTable.r, ledColorTestTable.g, ledColorTestTable.b);
		ledStateTextCallback?.Invoke(text);
	}

	public void SetTextCallback(LedStateTextCallback _ledStateTextCallback)
	{
		ledStateTextCallback = _ledStateTextCallback;
	}

	public void SetState(ColorLedState_TestMode state)
	{
		StopAllCoroutines();
		LedColorTestTable ledColorTestTable = COLOR_LED_TABLE[(int)state];
		setColorLedFunc?.Invoke(ledColorTestTable.r, ledColorTestTable.g, ledColorTestTable.b);
		ledStateTextCallback?.Invoke(TAPE_COLOR_TEXT[(int)state]);
	}

	public void StartRainbow()
	{
		StartCoroutine(CoRainbow());
	}

	public IEnumerator CoRainbow()
	{
		ledStateTextCallback?.Invoke("FULLCOLOR");
		while (true)
		{
			for (byte k = 0; k <= 127; k++)
			{
				setColorLedFunc?.Invoke(k, (byte)(127 - k), 127);
				yield return null;
			}
			for (byte k = 0; k <= 127; k++)
			{
				setColorLedFunc?.Invoke(127, k, (byte)(127 - k));
				yield return null;
			}
			for (byte k = 0; k <= 127; k++)
			{
				setColorLedFunc?.Invoke((byte)(127 - k), 127, k);
				yield return null;
			}
		}
	}

	public void StartFlashing(float sec)
	{
		StartCoroutine(CoFlashing(sec));
	}

	public IEnumerator CoFlashing(float sec)
	{
		bool on = true;
		while (true)
		{
			LedColorTestTable ledColorTestTable = (on ? COLOR_LED_TABLE[4] : COLOR_LED_TABLE[0]);
			string text = (on ? TAPE_COLOR_TEXT[4] : TAPE_COLOR_TEXT[0]);
			setColorLedFunc?.Invoke(ledColorTestTable.r, ledColorTestTable.g, ledColorTestTable.b);
			ledStateTextCallback?.Invoke(text);
			on = !on;
			yield return new WaitForSeconds(sec);
		}
	}
}

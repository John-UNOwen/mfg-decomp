using System.Collections;
using UnityEngine;

namespace LampTester;

public class BinaryLedTester : MonoBehaviour
{
	protected static readonly string ON = "<color=red>ON</color>";

	protected static readonly string OFF = "<color=white>OFF</color>";

	private SetLedFunc setLedFunc;

	private LedStateTextCallback ledStateTextCallback;

	public void SetFunction(SetLedFunc func)
	{
		setLedFunc = func;
	}

	public void SetTextCallback(LedStateTextCallback _ledStateTextCallback)
	{
		ledStateTextCallback = _ledStateTextCallback;
	}

	public void Clear()
	{
		StopAllCoroutines();
		setLedFunc?.Invoke(on: false);
		ledStateTextCallback?.Invoke(OFF);
	}

	public void SetEnable(bool on)
	{
		StopAllCoroutines();
		setLedFunc?.Invoke(on);
		ledStateTextCallback?.Invoke(on ? ON : OFF);
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
			setLedFunc?.Invoke(on);
			ledStateTextCallback?.Invoke(on ? ON : OFF);
			on = !on;
			yield return new WaitForSeconds(sec);
		}
	}
}

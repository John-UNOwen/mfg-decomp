using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Types;
using UnityEngine;

public class LEDManager
{
	private CancellationTokenSource _token;

	private static LEDManager _instance;

	private bool _isAllowAnim = true;

	public static LEDManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new LEDManager();
			}
			return _instance;
		}
		private set
		{
			_instance = value;
		}
	}

	public void Apply(PlayerConfigGameData playerConfig)
	{
		bool isUKSLEDAnimAllow = playerConfig.IsUKSLEDAnimAllow;
		if (isUKSLEDAnimAllow)
		{
			SetLedColor(Color.white);
		}
		else
		{
			Stop();
		}
		AllowAnim(isUKSLEDAnimAllow);
	}

	public void AllowAnim(bool isAllow)
	{
		_isAllowAnim = isAllow;
	}

	public void Stop()
	{
		StopAnimation();
		SetLedColor(Color.black);
	}

	public async void AnimColor(Color[] colors, bool isSpread, int speed = 1)
	{
		if (!_isAllowAnim)
		{
			return;
		}
		StopAnimation();
		_token = new CancellationTokenSource();
		try
		{
			await CoAnimationAsync(colors, isSpread, speed, _token.Token);
		}
		catch (OperationCanceledException)
		{
		}
	}

	public void SetLedColor(Color color)
	{
		SetLedColor(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, color);
		SetLedColor(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, color);
		SetLedColor(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, color);
		SetLedColor(Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, color);
		SetLedColor(Bi2xInput.TAPELED.TAPELED_TBS_SIDE_PANEL, color);
		SetButtonLedColor(color);
	}

	private void StopAnimation()
	{
		if (_token != null)
		{
			_token.Cancel();
		}
	}

	public int GetLedNum(Bi2xInput.TAPELED line)
	{
		if (SystemConfigGameData.CabinetType != CabinetType.UKS && SystemConfigGameData.CabinetType != CabinetType.TBS && SystemConfigGameData.CabinetType != CabinetType.UJK)
		{
			return 2;
		}
		return line switch
		{
			Bi2xInput.TAPELED.TAPELED_LEFT_PANEL => 23, 
			Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL => 23, 
			Bi2xInput.TAPELED.TAPELED_CTRL_PANEL => 60, 
			_ => 1, 
		};
	}

	private void SetICLedColor(Color color)
	{
		Bi2xInput.Instance.SetCardLed(color);
	}

	private void SetButtonLedColor(Color color)
	{
		Bi2xInput.Instance.SetButtonLED(color);
	}

	private void SetLedColor(Bi2xInput.TAPELED line, Color color)
	{
		Bi2xInput.Instance.SetTapeLedColor(line, color);
	}

	private void SetLedColor(Bi2xInput.TAPELED line, int led_no, Color color)
	{
		Bi2xInput.Instance.SetTapeLedColor(line, led_no, color);
	}

	public Color GetLedColor(Bi2xInput.TAPELED line, int index)
	{
		return Bi2xInput.Instance.GetLedColor(line, index);
	}

	private async UniTask CoAnimationAsync(Color[] colors, bool isSpread, int speed, CancellationToken token = default(CancellationToken))
	{
		Gradient gradient = new Gradient();
		int num2 = colors.Length * 2 - 1;
		GradientColorKey[] array = new GradientColorKey[num2];
		if (num2 == 1)
		{
			array[0] = new GradientColorKey(colors[0], 0f);
		}
		else
		{
			float num3 = 1f / (float)(num2 - 1);
			for (int i = 0; i < num2; i++)
			{
				array[i] = new GradientColorKey(colors[(i < colors.Length) ? i : (num2 - 1 - i)], Mathf.Min((float)i * num3, 1f));
			}
		}
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		};
		gradient.SetKeys(array, alphaKeys);
		int diff = 0;
		int num = GetLedNum(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL);
		int quarterCtrlNum = num / 4;
		while (true)
		{
			token.ThrowIfCancellationRequested();
			SetButtonLedColor(gradient.Evaluate((float)diff / (float)num));
			for (int j = 0; j < num; j++)
			{
				int num4 = (j - speed * diff + speed * num) % num;
				if (0 <= j && j < quarterCtrlNum)
				{
					SetLedColor(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isSpread ? (quarterCtrlNum - 1 - j) : j, gradient.Evaluate((float)num4 / (float)num));
					SetLedColor(Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, isSpread ? (quarterCtrlNum - 1 - j) : j, gradient.Evaluate((float)num4 / (float)num));
				}
				else if (2 * quarterCtrlNum <= j && j < 3 * quarterCtrlNum)
				{
					SetLedColor(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isSpread ? (5 * quarterCtrlNum - 1 - j) : j, gradient.Evaluate((float)((num4 - 2 * quarterCtrlNum + num) % num) / (float)num));
					SetLedColor(Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, isSpread ? (5 * quarterCtrlNum - 1 - j) : j, gradient.Evaluate((float)((num4 - 2 * quarterCtrlNum + num) % num) / (float)num));
				}
				else if (quarterCtrlNum <= j && j < 2 * quarterCtrlNum)
				{
					SetLedColor(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isSpread ? j : (3 * quarterCtrlNum - 1 - j), gradient.Evaluate((float)Mathf.Abs(num4 - quarterCtrlNum + 1) / (float)num));
					SetLedColor(Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, isSpread ? j : (3 * quarterCtrlNum - 1 - j), gradient.Evaluate((float)Mathf.Abs(num4 - quarterCtrlNum + 1) / (float)num));
				}
				else
				{
					SetLedColor(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isSpread ? j : (7 * quarterCtrlNum - 1 - j), gradient.Evaluate((float)Mathf.Abs(num4 - 3 * quarterCtrlNum + 1) / (float)num));
					SetLedColor(Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, isSpread ? j : (7 * quarterCtrlNum - 1 - j), gradient.Evaluate((float)Mathf.Abs(num4 - 3 * quarterCtrlNum + 1) / (float)num));
				}
				SetLedColor(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, isSpread ? (num - j - 1) : j, gradient.Evaluate((float)num4 / (float)num));
				SetLedColor(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, isSpread ? (num - j - 1) : j, gradient.Evaluate((float)num4 / (float)num));
				SetLedColor(Bi2xInput.TAPELED.TAPELED_TBS_SIDE_PANEL, isSpread ? (num - j - 1) : j, gradient.Evaluate((float)num4 / (float)num));
			}
			int num5 = diff + 1;
			diff = num5 % num;
			await UniTask.Yield();
		}
	}
}

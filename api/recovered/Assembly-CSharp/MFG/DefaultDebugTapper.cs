using System;
using UnityEngine;

namespace MFG;

public class DefaultDebugTapper : DebugTapper
{
	private float normalDistributionCosine = float.MaxValue;

	public new void ManualStart(int tapCount, Sprite markSprite, int logSize = 8192)
	{
		base.ManualStart(tapCount, markSprite, logSize);
	}

	protected override void UpdateTap(int tapIndex)
	{
		Vector2 vector = new Vector2(UnityEngine.Random.Range(0f, Screen.width), UnityEngine.Random.Range(0f, Screen.height));
		float num = NormalDistributionRandom();
		num *= 2f;
		float num2 = Mathf.Pow(10f, num) * 0.01f * (float)Mathf.Max(Screen.width, Screen.height);
		float f = MathF.PI * 2f * UnityEngine.Random.value;
		Vector2 vector2 = new Vector2(Mathf.Cos(f) * num2, Mathf.Sin(f) * num2);
		Vector2 toScreenPosition = vector + vector2;
		float num3 = NormalDistributionRandom();
		num3 *= 0.5f;
		float duration = Mathf.Pow(10f, num3) * 0.1f;
		Fire(tapIndex, vector, toScreenPosition, duration);
	}

	protected override bool ToBeIgnored(GameObject gameObject)
	{
		Transform parent = gameObject.transform;
		while (parent != null)
		{
			if (parent.gameObject.name.Contains("BtDebug"))
			{
				return true;
			}
			parent = parent.parent;
		}
		return false;
	}

	private float NormalDistributionRandom()
	{
		float result;
		if (normalDistributionCosine != float.MaxValue)
		{
			result = normalDistributionCosine;
			normalDistributionCosine = float.MaxValue;
		}
		else
		{
			float value = UnityEngine.Random.value;
			float value2 = UnityEngine.Random.value;
			float num = Mathf.Sqrt(-2f * Mathf.Log(value));
			float f = MathF.PI * 2f * value2;
			normalDistributionCosine = num * Mathf.Cos(f);
			result = num * Mathf.Sin(f);
		}
		return result;
	}
}

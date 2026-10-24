using UnityEngine;

namespace TestModeSkeleton;

public class TestModeSecretCommand : MonoBehaviour
{
	private int phase;

	private float timer;

	public bool Success { get; private set; }

	protected virtual bool IsTrigger()
	{
		return SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Test);
	}

	protected virtual bool IsPress()
	{
		return SingletonBehaviour<TestModeInput>.Instance.IsPress(TestModeInput.Key.Test);
	}

	protected virtual float GetPressSec()
	{
		return SingletonBehaviour<TestModeInput>.Instance.GetPressSec(TestModeInput.Key.Test);
	}

	private void Start()
	{
		Restart(2f);
	}

	public void Restart(float initTimer = 0f)
	{
		Success = false;
		phase = 0;
		timer = initTimer;
	}

	private void Update()
	{
		if (Success)
		{
			return;
		}
		if (phase == 0)
		{
			timer += Time.deltaTime;
			if (timer >= 3f)
			{
				if (IsTrigger())
				{
					phase++;
					timer = 0f;
				}
			}
			else if (IsTrigger())
			{
				timer = 0f;
			}
		}
		else if (phase < 3)
		{
			timer += Time.deltaTime;
			if (timer < 1f)
			{
				if (IsTrigger())
				{
					phase++;
				}
			}
			else
			{
				phase = 0;
				timer = 0f;
			}
		}
		else
		{
			if (phase != 3)
			{
				return;
			}
			if (IsPress())
			{
				if (GetPressSec() > 2f)
				{
					Success = true;
					phase = 0;
					timer = 0f;
				}
			}
			else
			{
				phase = 0;
				timer = 0f;
			}
		}
	}
}

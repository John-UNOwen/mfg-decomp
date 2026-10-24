using UnityEngine;

namespace TestModeSkeleton;

public class StateFuncManager
{
	public delegate void State(StateFuncManager manager, object param);

	public const string StartStep = "";

	private static readonly State Idle = delegate
	{
	};

	private State current;

	private State request;

	private string requestStep;

	public string Step { get; private set; }

	public float Sec { get; private set; }

	public float StepSec { get; private set; }

	public bool IsStart => Mathf.Approximately(Sec, 0f);

	public bool IsStepStart => Mathf.Approximately(StepSec, 0f);

	public StateFuncManager(State state = null)
	{
		current = state;
	}

	public void Update(object param = null)
	{
		if (request != null)
		{
			current = request;
			request = null;
			Sec = 0f;
		}
		if (requestStep != null)
		{
			Step = requestStep;
			requestStep = null;
			StepSec = 0f;
		}
		if (current != null)
		{
			current(this, param);
			Sec += Time.deltaTime;
			StepSec += Time.deltaTime;
		}
	}

	public void Change(State state, string step = "")
	{
		if (state != null)
		{
			request = state;
			SetStep(step);
		}
	}

	public void ChangeIdle()
	{
		Change(Idle);
	}

	public bool IsEqual(State state)
	{
		return current == state;
	}

	public bool IsIdle()
	{
		return IsEqual(Idle);
	}

	public void SetStep(string step)
	{
		requestStep = step;
	}
}

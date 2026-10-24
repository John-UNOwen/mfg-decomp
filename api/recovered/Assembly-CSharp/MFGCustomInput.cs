using UnityEngine;
using UnityEngine.EventSystems;

public class MFGCustomInput : BaseInput
{
	public override string compositionString => "MFGCustomInput";

	public override IMECompositionMode imeCompositionMode
	{
		get
		{
			return Input.imeCompositionMode;
		}
		set
		{
			Input.imeCompositionMode = value;
		}
	}

	public override Vector2 compositionCursorPos
	{
		get
		{
			return Input.compositionCursorPos;
		}
		set
		{
			Input.compositionCursorPos = value;
		}
	}

	public override bool mousePresent => true;

	public override Vector2 mousePosition => Bi2xInput.Instance.GetTouchPosition();

	public override Vector2 mouseScrollDelta => new Vector2(0f, 0f);

	public override bool touchSupported => false;

	public override int touchCount => 0;

	public override bool GetMouseButtonDown(int button)
	{
		if (button == 0)
		{
			return Bi2xInput.Instance.IsTouchTrigger();
		}
		return false;
	}

	public override bool GetMouseButtonUp(int button)
	{
		if (button == 0)
		{
			return Bi2xInput.Instance.IsTouchRelease();
		}
		return false;
	}

	public override bool GetMouseButton(int button)
	{
		if (button == 0)
		{
			return Bi2xInput.Instance.IsTouchPress();
		}
		return false;
	}

	public override Touch GetTouch(int index)
	{
		return Input.GetTouch(index);
	}

	public override float GetAxisRaw(string axisName)
	{
		return Input.GetAxisRaw(axisName);
	}

	public override bool GetButtonDown(string buttonName)
	{
		return Input.GetButtonDown(buttonName);
	}
}

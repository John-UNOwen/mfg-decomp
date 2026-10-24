using TestModeSkeleton;
using UnityEngine;

public class TouchArrowKey : MonoBehaviour
{
	public enum ArrowType
	{
		Default,
		None,
		SelectExecute,
		SelectExit,
		SpecialOperationExit,
		Exit,
		Abort,
		Select,
		SelectChange,
		SelectChangeExecute,
		ChangeExit,
		BookKeeping,
		ClockChange,
		ClockSetTime,
		TimeChange,
		YesOrNo,
		ChangePassWord,
		IncomeLog
	}

	[SerializeField]
	private TestModeInput.Key _keyType;

	public bool IsTrigger { get; private set; }

	public bool IsPress { get; private set; }

	public bool IsRelease { get; private set; }

	private void Update()
	{
		if (IsPress && !ResidentFlow.CustomInput.GetMouseButton(0))
		{
			IsPress = false;
		}
	}

	public void Set(ArrowType type)
	{
		if (type != ArrowType.None)
		{
			_ = 2;
		}
	}

	public void OnPointerUp()
	{
		IsRelease = true;
		IsPress = false;
	}

	public void OnPointerDown()
	{
		IsTrigger = true;
		IsPress = true;
	}

	private void LateUpdate()
	{
		IsTrigger = false;
		IsRelease = false;
	}

	public void ResetState()
	{
		IsTrigger = false;
		IsRelease = false;
		IsPress = false;
	}
}

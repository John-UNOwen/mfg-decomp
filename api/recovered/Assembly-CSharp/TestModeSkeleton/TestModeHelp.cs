using System.Collections.Generic;
using KAMUNITY;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class TestModeHelp : MonoBehaviour
{
	public enum HelpType
	{
		Default,
		None,
		SelectExecute,
		SelectExit,
		SpecialOperationExit,
		Exit,
		ServiceExit,
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
		IncomeLog,
		SelectSetting,
		UDLRSelectExecute,
		SpecialOperationExit2,
		CalibrationExit,
		CalibrationStep1,
		CalibrationStep2,
		CalibrationStep3,
		ToTestMode,
		ChangeSubMonitorImageExit,
		SelectChangeLedExit
	}

	private class Strings
	{
		private List<string> textList = new List<string>();

		public string this[int index]
		{
			get
			{
				if (0 > index || index >= textList.Count)
				{
					return "";
				}
				return textList[index];
			}
		}

		public Strings(params string[] texts)
		{
			foreach (string item in texts)
			{
				textList.Add(item);
			}
		}
	}

	[SerializeField]
	[Tooltip("操作説明テキスト")]
	private List<Text> helpTexts;

	private HelpType current;

	private HelpType? saving;

	private Stack<HelpType> stack = new Stack<HelpType>();

	private static readonly string HoldTest_Service_ChangePassWord = "<l>HOLD DOWN TEST + SERVICE BUTTON = CHANGEPASSWORD</l>";

	private static readonly string HoldTest_Service_Exit = "<l>HOLD DOWN TEST + SERVICE BUTTON = EXIT</l>";

	private static readonly string HoldTest_Service_Submenu = "<l>HOLD DOWN TEST + SERVICE BUTTON = SUBMENU</l>";

	private static readonly string HoldTest_Service_Calibration = "<l>HOLD DOWN TEST + SERVICE BUTTON = CALIBRATION</l>";

	private static readonly string HoldTest_Service_Abort = "<l>HOLD DOWN TEST + SERVICE BUTTON = ABORT</l>";

	private static readonly string Service_Abort = "<l>SERVICE BUTTON = ABORT</l>";

	private static readonly string Service_ChangeValue = "<l>SERVICE BUTTON = CHANGE VALUE</l>";

	private static readonly string Service_Execute = "<l>SERVICE BUTTON = EXECUTE</l>";

	private static readonly string Service_Exit = "<l>SERVICE BUTTON = EXIT</l>";

	private static readonly string Service_Setting = "<l>SERVICE BUTTON = SETTING</l>";

	private static readonly string Service_DecideNeutral = "<l>SERVICE BUTTON = DECIDE NEUTRAL</l>";

	private static readonly string Service_DecideValueRange = "<l>SERVICE BUTTON = DECIDE VALUE RANGE</l>";

	private static readonly string Service_DecideIgnoreRange = "<l>SERVICE BUTTON = DECIDE IGNORE RANGE</l>";

	private static readonly string Ok_Abort = "<l>OK BUTTON = ABORT</l>";

	private static readonly string Ok_Execute = "<l>OK BUTTON = EXECUTE</l>";

	private static readonly string Ok_Exit = "<l>OK BUTTON = EXIT</l>";

	private static readonly string Ok_Setting = "<l>OK BUTTON = SETTING</l>";

	private static readonly string Ok_DecideValueRange = "<l>OK BUTTON = DECIDE VALUE RANGE</l>";

	private static readonly string Ok_DecideIgnoreRange = "<l>OK BUTTON = DECIDE IGNORE RANGE</l>";

	private static readonly string Test_ChangeClock = "<l>TEST BUTTON = CHANGE CLOCK</l>";

	private static readonly string Test_ChangeTime = "<l>TEST BUTTON = CHANGE TIME</l>";

	private static readonly string Test_ChangeValue = "<l>TEST BUTTON = CHANGE VALUE</l>";

	private static readonly string Test_NextPage = "<l>TEST BUTTON = NEXT PAGE</l>";

	private static readonly string Test_SelectItem = "<l>TEST BUTTON = SELECT ITEM</l>";

	private static readonly string Test_TestMode = "<l>TEST BUTTON = TEST MODE</l>";

	private static readonly string Test_ChangeSubMonitorImage = "<l>TEST BUTTON = CHANGE SUB MONITOR IMAGE</l>";

	private static readonly string L_Plus_R_ChangePassWord = "<l>L+R BUTTON = CHANGEPASSWORD</l>";

	private static readonly string L_Plus_R_Exit = "<l>L+R BUTTON = EXIT</l>";

	private static readonly string L_Plus_R_Submenu = "<l>L+R BUTTON = SUBMENU</l>";

	private static readonly string L_Plus_R_Calibration = "<l>L+R BUTTON = CALIBRATION</l>";

	private static readonly string L_Plus_R_Abort = "<l>L+R BUTTON = ABORT</l>";

	private static readonly string LR_ChangeClock = "<l>L/R BUTTON = CHANGE CLOCK</l>";

	private static readonly string LR_ChangeTime = "<l>L/R BUTTON = CHANGE TIME</l>";

	private static readonly string LR_ChangeValue = "<l>L/R BUTTON = CHANGE VALUE</l>";

	private static readonly string LR_ChangeLed = "<l>L/R BUTTON = CHANGE LED</l>";

	private static readonly string LR_PrevNextPage = "<l>L/R BUTTON = PREV/NEXT PAGE</l>";

	private static readonly string LR_SelectItem = "<l>L/R BUTTON = SELECT ITEM</l>";

	private static readonly string LR_ChangeSubMonitorImage = "<l>L/R BUTTON = CHANGE SUB MONITOR IMAGE</l>";

	private static readonly string UD_ChangeValue = "<l>U/D BUTTON = CHANGE VALUE</l>";

	private static readonly string UD_SelectItem = "<l>U/D BUTTON = SELECT ITEM</l>";

	private Dictionary<HelpType, Strings> dictionary = new Dictionary<HelpType, Strings>
	{
		[HelpType.Default] = new Strings(),
		[HelpType.None] = new Strings(),
		[HelpType.SelectExecute] = new Strings(UD_SelectItem, Test_SelectItem, Ok_Execute, Service_Execute),
		[HelpType.SelectExit] = new Strings(UD_SelectItem, Test_SelectItem, Ok_Exit, Service_Exit),
		[HelpType.SpecialOperationExit] = new Strings(L_Plus_R_Exit, HoldTest_Service_Exit),
		[HelpType.Exit] = new Strings(Ok_Exit, Service_Exit),
		[HelpType.ServiceExit] = new Strings(Service_Exit),
		[HelpType.Abort] = new Strings(Ok_Abort, Service_Abort),
		[HelpType.Select] = new Strings(UD_SelectItem, Test_SelectItem),
		[HelpType.SelectChange] = new Strings(UD_SelectItem, Test_SelectItem, LR_ChangeValue, Service_ChangeValue),
		[HelpType.SelectChangeExecute] = new Strings(LR_ChangeValue, UD_SelectItem, Test_SelectItem, Ok_Execute, Service_Execute),
		[HelpType.ChangeExit] = new Strings(LR_ChangeValue, Test_ChangeValue, Ok_Exit, Service_Exit),
		[HelpType.BookKeeping] = new Strings(LR_PrevNextPage, Test_NextPage, HoldTest_Service_Submenu, Ok_Exit, Service_Exit),
		[HelpType.ClockChange] = new Strings(UD_SelectItem, LR_ChangeClock, Test_ChangeClock),
		[HelpType.ClockSetTime] = new Strings(UD_ChangeValue, Service_ChangeValue, LR_SelectItem, Test_SelectItem),
		[HelpType.TimeChange] = new Strings(UD_SelectItem, LR_ChangeTime, Test_ChangeTime),
		[HelpType.YesOrNo] = new Strings(LR_SelectItem, Test_SelectItem, Ok_Execute, Service_Execute),
		[HelpType.ChangePassWord] = new Strings(HoldTest_Service_ChangePassWord, Ok_Exit, Service_Exit),
		[HelpType.IncomeLog] = new Strings(LR_PrevNextPage, Test_NextPage, Ok_Exit, Service_Exit),
		[HelpType.SelectSetting] = new Strings(UD_SelectItem, Test_SelectItem, Ok_Setting, Service_Setting),
		[HelpType.UDLRSelectExecute] = new Strings(UD_SelectItem, LR_SelectItem, Test_SelectItem, Ok_Execute, Service_Execute),
		[HelpType.SpecialOperationExit2] = new Strings(Ok_Exit, HoldTest_Service_Exit),
		[HelpType.CalibrationExit] = new Strings(HoldTest_Service_Calibration, Service_Exit),
		[HelpType.CalibrationStep1] = new Strings(Service_Abort),
		[HelpType.CalibrationStep2] = new Strings(Service_Abort),
		[HelpType.CalibrationStep3] = new Strings(LR_ChangeValue, Test_ChangeValue, Ok_DecideIgnoreRange, Service_DecideIgnoreRange, L_Plus_R_Abort, HoldTest_Service_Abort),
		[HelpType.ToTestMode] = new Strings(Test_TestMode),
		[HelpType.ChangeSubMonitorImageExit] = new Strings(LR_ChangeSubMonitorImage, Test_ChangeSubMonitorImage, Ok_Exit, Service_Exit),
		[HelpType.SelectChangeLedExit] = new Strings(LR_ChangeLed, UD_SelectItem, Test_SelectItem, Ok_Exit, Service_Exit)
	};

	public void Set(HelpType type, bool isForcedApply = false)
	{
		if (current != type || isForcedApply)
		{
			current = type;
			for (int i = 0; i < helpTexts.Count; i++)
			{
				helpTexts[i].SetLocalizedText(dictionary[type][i]);
			}
		}
	}

	public void SaveAndSet(HelpType type)
	{
		if (saving.HasValue)
		{
			KAMUNITY.Logger.Log($"セーブ中の値を上書きします。{saving} -> {current}");
		}
		saving = current;
		Set(type);
	}

	public void SetFromSave()
	{
		if (saving.HasValue)
		{
			Set(saving.Value);
			saving = null;
		}
	}

	public void Call()
	{
		stack.Push(current);
	}

	public void Return()
	{
		if (stack.Count > 0)
		{
			Set(stack.Pop(), isForcedApply: true);
		}
	}
}

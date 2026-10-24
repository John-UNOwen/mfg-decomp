using System.Collections;
using System.Collections.Generic;
using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class LampCheck : TestModeBase
{
	private enum LampID
	{
		All,
		Auto,
		Left,
		Right,
		Front,
		Foot,
		UpRed,
		UpGreen,
		UpBlue,
		UpAll,
		ICCardRed,
		ICCardGreen,
		ICCardBlue,
		ICCardAll,
		LeftRed,
		LeftGreen,
		LeftBlue,
		LeftAll,
		RightRed,
		RightGreen,
		RightBlue,
		RightAll,
		PanelRed,
		PanelGreen,
		PanelBlue,
		PanelAll,
		ButtonRed,
		ButtonGreen,
		ButtonBlue,
		ButtonAll,
		Max
	}

	[SerializeField]
	private List<Text> m_lampTexts;

	private CabinetType m_cabinetType;

	private Coroutine coAuto;

	protected override string TitleName => "<l>LAMP CHECK</l>";

	private IEnumerator Start()
	{
		help.Set(TestModeHelp.HelpType.SelectExit);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		m_cabinetType = SystemConfigGameData.CabinetType;
		MenuNameChange();
		menuElements.FindAll((TestModeMenuElement e) => !IsExistLamp(m_cabinetType, e.Event)).ForEach(delegate(TestModeMenuElement e)
		{
			e.State = TestModeMenuElement.TextState.NotActive;
		});
		StartOrResumeMainCoroutine();
		yield return new WaitUntil(() => SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute());
		SetAllLamps(isEnable: false);
		Exit();
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		switch (element.Event)
		{
		case "All":
			SetText(LampID.All, isOn: true);
			SetAllLamps(isEnable: true);
			break;
		case "Auto":
			SetText(LampID.Auto, isOn: true);
			coAuto = StartCoroutine(CoAutoCheck(0.8f));
			break;
		case "Left":
			SetLamp(LampID.Left, isEnable: true);
			break;
		case "Right":
			SetLamp(LampID.Right, isEnable: true);
			break;
		case "Front":
			SetLamp(LampID.Front, isEnable: true);
			break;
		case "Foot":
			SetLamp(LampID.Foot, isEnable: true);
			break;
		case "UpRed":
			SetLamp(LampID.UpRed, isEnable: true);
			break;
		case "UpGreen":
			SetLamp(LampID.UpGreen, isEnable: true);
			break;
		case "UpBlue":
			SetLamp(LampID.UpBlue, isEnable: true);
			break;
		case "UpAll":
			SetLamp(LampID.UpAll, isEnable: true);
			break;
		case "ICCardRed":
			SetLamp(LampID.ICCardRed, isEnable: true);
			break;
		case "ICCardGreen":
			SetLamp(LampID.ICCardGreen, isEnable: true);
			break;
		case "ICCardBlue":
			SetLamp(LampID.ICCardBlue, isEnable: true);
			break;
		case "ICCardAll":
			SetLamp(LampID.ICCardAll, isEnable: true);
			break;
		case "LeftRed":
			SetLamp(LampID.LeftRed, isEnable: true);
			break;
		case "LeftGreen":
			SetLamp(LampID.LeftGreen, isEnable: true);
			break;
		case "LeftBlue":
			SetLamp(LampID.LeftBlue, isEnable: true);
			break;
		case "LeftAll":
			SetLamp(LampID.LeftAll, isEnable: true);
			break;
		case "RightRed":
			SetLamp(LampID.RightRed, isEnable: true);
			break;
		case "RightGreen":
			SetLamp(LampID.RightGreen, isEnable: true);
			break;
		case "RightBlue":
			SetLamp(LampID.RightBlue, isEnable: true);
			break;
		case "RightAll":
			SetLamp(LampID.RightAll, isEnable: true);
			break;
		case "PanelRed":
			SetLamp(LampID.PanelRed, isEnable: true);
			break;
		case "PanelGreen":
			SetLamp(LampID.PanelGreen, isEnable: true);
			break;
		case "PanelBlue":
			SetLamp(LampID.PanelBlue, isEnable: true);
			break;
		case "PanelAll":
			SetLamp(LampID.PanelAll, isEnable: true);
			break;
		case "ButtonRed":
			SetLamp(LampID.ButtonRed, isEnable: true);
			break;
		case "ButtonGreen":
			SetLamp(LampID.ButtonGreen, isEnable: true);
			break;
		case "ButtonBlue":
			SetLamp(LampID.ButtonBlue, isEnable: true);
			break;
		case "ButtonAll":
			SetLamp(LampID.ButtonAll, isEnable: true);
			break;
		}
	}

	protected override void OnChangeMenuOff(TestModeMenuElement element)
	{
		string @event = element.Event;
		if (!(@event == "All"))
		{
			if (@event == "Auto")
			{
				StopCoroutine(ref coAuto);
				SetText(LampID.Auto, isOn: false);
			}
		}
		else
		{
			SetText(LampID.All, isOn: false);
		}
	}

	private void SetAllLamps(bool isEnable)
	{
		for (int i = 2; i < 30; i++)
		{
			SetText((LampID)i, isEnable);
		}
		Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp1, isEnable);
		Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp2, isEnable);
		Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp3, isEnable);
		Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp4, isEnable);
		Bi2xInput.Instance.SetPillarLED(isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetCardLed(isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_TBS_SIDE_PANEL, isEnable ? Color.white : Color.black);
		Bi2xInput.Instance.SetButtonLED(isEnable ? Color.white : Color.black, isForceDisp: true);
		Bi2xInput.Instance.SetQButtonLED((byte)(isEnable ? 255u : 0u));
	}

	private void SetLamp(LampID lampID, bool isEnable)
	{
		SetAllLamps(isEnable: false);
		SetText(lampID, isEnable);
		switch (lampID)
		{
		case LampID.Left:
			Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp1, isEnable);
			break;
		case LampID.Right:
			Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp2, isEnable);
			break;
		case LampID.Front:
			Bi2xInput.Instance.SetLamp(Bi2xInput.LampID.Lamp3, isEnable);
			break;
		case LampID.UpRed:
			Bi2xInput.Instance.SetPillarLED(isEnable ? Color.red : Color.black);
			break;
		case LampID.UpGreen:
			Bi2xInput.Instance.SetPillarLED(isEnable ? Color.green : Color.black);
			break;
		case LampID.UpBlue:
			Bi2xInput.Instance.SetPillarLED(isEnable ? Color.blue : Color.black);
			break;
		case LampID.UpAll:
			Bi2xInput.Instance.SetPillarLED(isEnable ? Color.white : Color.black);
			break;
		case LampID.ICCardRed:
			Bi2xInput.Instance.SetCardLed(isEnable ? Color.red : Color.black);
			break;
		case LampID.ICCardGreen:
			Bi2xInput.Instance.SetCardLed(isEnable ? Color.green : Color.black);
			break;
		case LampID.ICCardBlue:
			Bi2xInput.Instance.SetCardLed(isEnable ? Color.blue : Color.black);
			break;
		case LampID.ICCardAll:
			Bi2xInput.Instance.SetCardLed(isEnable ? Color.white : Color.black);
			break;
		case LampID.LeftRed:
			Bi2xInput.Instance.SetTapeLED((m_cabinetType == CabinetType.UKS) ? Bi2xInput.TAPELED.TAPELED_LEFT_PANEL : Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, isEnable ? Color.red : Color.black);
			break;
		case LampID.LeftGreen:
			Bi2xInput.Instance.SetTapeLED((m_cabinetType == CabinetType.UKS) ? Bi2xInput.TAPELED.TAPELED_LEFT_PANEL : Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, isEnable ? Color.green : Color.black);
			break;
		case LampID.LeftBlue:
			Bi2xInput.Instance.SetTapeLED((m_cabinetType == CabinetType.UKS) ? Bi2xInput.TAPELED.TAPELED_LEFT_PANEL : Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, isEnable ? Color.blue : Color.black);
			break;
		case LampID.LeftAll:
			Bi2xInput.Instance.SetTapeLED((m_cabinetType == CabinetType.UKS) ? Bi2xInput.TAPELED.TAPELED_LEFT_PANEL : Bi2xInput.TAPELED.TAPELED_TBS_TITLE_PANEL, isEnable ? Color.white : Color.black);
			break;
		case LampID.RightRed:
			Bi2xInput.Instance.SetTapeLED((m_cabinetType == CabinetType.UKS) ? Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL : Bi2xInput.TAPELED.TAPELED_TBS_SIDE_PANEL, isEnable ? Color.red : Color.black);
			break;
		case LampID.RightGreen:
			Bi2xInput.Instance.SetTapeLED((m_cabinetType == CabinetType.UKS) ? Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL : Bi2xInput.TAPELED.TAPELED_TBS_SIDE_PANEL, isEnable ? Color.green : Color.black);
			break;
		case LampID.RightBlue:
			Bi2xInput.Instance.SetTapeLED((m_cabinetType == CabinetType.UKS) ? Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL : Bi2xInput.TAPELED.TAPELED_TBS_SIDE_PANEL, isEnable ? Color.blue : Color.black);
			break;
		case LampID.RightAll:
			Bi2xInput.Instance.SetTapeLED((m_cabinetType == CabinetType.UKS) ? Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL : Bi2xInput.TAPELED.TAPELED_TBS_SIDE_PANEL, isEnable ? Color.white : Color.black);
			break;
		case LampID.PanelRed:
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.red : Color.black);
			break;
		case LampID.PanelGreen:
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.green : Color.black);
			break;
		case LampID.PanelBlue:
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.blue : Color.black);
			break;
		case LampID.PanelAll:
			Bi2xInput.Instance.SetTapeLED(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, isEnable ? Color.white : Color.black);
			break;
		case LampID.ButtonRed:
			Bi2xInput.Instance.SetButtonLED(isEnable ? Color.red : Color.black, isForceDisp: true);
			break;
		case LampID.ButtonGreen:
			Bi2xInput.Instance.SetButtonLED(isEnable ? Color.green : Color.black, isForceDisp: true);
			break;
		case LampID.ButtonBlue:
			Bi2xInput.Instance.SetButtonLED(isEnable ? Color.blue : Color.black, isForceDisp: true);
			break;
		case LampID.ButtonAll:
			Bi2xInput.Instance.SetButtonLED(isEnable ? Color.white : Color.black, isForceDisp: true);
			break;
		case LampID.All:
		case LampID.Auto:
		case LampID.Foot:
			break;
		}
	}

	private IEnumerator CoAutoCheck(float seconds)
	{
		while (true)
		{
			foreach (LampID item in Lamps(m_cabinetType))
			{
				SetLamp(item, isEnable: true);
				yield return new WaitForSeconds(seconds);
			}
		}
	}

	private IEnumerable<LampID> Lamps(CabinetType cabinetType)
	{
		switch (cabinetType)
		{
		case CabinetType.A:
			return new LampID[2]
			{
				LampID.Left,
				LampID.Right
			};
		case CabinetType.B:
			return new LampID[4]
			{
				LampID.Left,
				LampID.Right,
				LampID.Front,
				LampID.Foot
			};
		case CabinetType.C:
			return new LampID[2]
			{
				LampID.Left,
				LampID.Right
			};
		case CabinetType.HG:
			return new LampID[8]
			{
				LampID.UpRed,
				LampID.UpGreen,
				LampID.UpBlue,
				LampID.UpAll,
				LampID.ICCardRed,
				LampID.ICCardGreen,
				LampID.ICCardBlue,
				LampID.ICCardAll
			};
		case CabinetType.UKS:
			return new LampID[20]
			{
				LampID.ICCardRed,
				LampID.ICCardGreen,
				LampID.ICCardBlue,
				LampID.ICCardAll,
				LampID.LeftRed,
				LampID.LeftGreen,
				LampID.LeftBlue,
				LampID.LeftAll,
				LampID.RightRed,
				LampID.RightGreen,
				LampID.RightBlue,
				LampID.RightAll,
				LampID.PanelRed,
				LampID.PanelGreen,
				LampID.PanelBlue,
				LampID.PanelAll,
				LampID.ButtonRed,
				LampID.ButtonGreen,
				LampID.ButtonBlue,
				LampID.ButtonAll
			};
		case CabinetType.TBS:
		case CabinetType.UJK:
			return new LampID[12]
			{
				LampID.ICCardRed,
				LampID.ICCardGreen,
				LampID.ICCardBlue,
				LampID.ICCardAll,
				LampID.LeftRed,
				LampID.LeftGreen,
				LampID.LeftBlue,
				LampID.LeftAll,
				LampID.RightRed,
				LampID.RightGreen,
				LampID.RightBlue,
				LampID.RightAll
			};
		default:
			return new LampID[12]
			{
				LampID.Left,
				LampID.Right,
				LampID.Front,
				LampID.Foot,
				LampID.UpRed,
				LampID.UpGreen,
				LampID.UpBlue,
				LampID.UpAll,
				LampID.ICCardRed,
				LampID.ICCardGreen,
				LampID.ICCardBlue,
				LampID.ICCardAll
			};
		}
	}

	private void SetText(LampID textID, bool isOn)
	{
		m_lampTexts[(int)textID].text = (isOn ? TestModeBase.On : TestModeBase.Off);
	}

	private bool IsExistLamp(CabinetType cabinetType, string eventString)
	{
		if (eventString == "All" || eventString == "Auto")
		{
			return true;
		}
		switch (SystemConfigGameData.CabinetType)
		{
		case CabinetType.A:
			if (eventString == "Left" || eventString == "Right")
			{
				return true;
			}
			break;
		case CabinetType.B:
			switch (eventString)
			{
			case "Left":
			case "Right":
			case "Front":
			case "Foot":
				return true;
			}
			break;
		case CabinetType.C:
			if (eventString == "Left" || eventString == "Right")
			{
				return true;
			}
			break;
		case CabinetType.HG:
			switch (eventString)
			{
			case "UpRed":
			case "UpGreen":
			case "UpBlue":
			case "UpAll":
			case "ICCardRed":
			case "ICCardGreen":
			case "ICCardBlue":
			case "ICCardAll":
				return true;
			}
			break;
		case CabinetType.UKS:
			switch (eventString)
			{
			case "LeftRed":
			case "LeftGreen":
			case "LeftBlue":
			case "LeftAll":
			case "RightRed":
			case "RightGreen":
			case "RightBlue":
			case "RightAll":
			case "PanelRed":
			case "PanelGreen":
			case "PanelBlue":
			case "PanelAll":
			case "ButtonRed":
			case "ButtonGreen":
			case "ButtonBlue":
			case "ButtonAll":
			case "ICCardRed":
			case "ICCardGreen":
			case "ICCardBlue":
			case "ICCardAll":
				return true;
			}
			break;
		case CabinetType.TBS:
		case CabinetType.UJK:
			switch (eventString)
			{
			case "LeftRed":
			case "LeftGreen":
			case "LeftBlue":
			case "LeftAll":
			case "RightRed":
			case "RightGreen":
			case "RightBlue":
			case "RightAll":
			case "ICCardRed":
			case "ICCardGreen":
			case "ICCardBlue":
			case "ICCardAll":
				return true;
			}
			break;
		default:
			return true;
		}
		return false;
	}

	private void MenuNameChange()
	{
		if (m_cabinetType == CabinetType.TBS || m_cabinetType == CabinetType.UJK)
		{
			menuElements[14].Text.text = "UP         RED";
			menuElements[18].Text.text = "SIDE      RED";
		}
	}
}

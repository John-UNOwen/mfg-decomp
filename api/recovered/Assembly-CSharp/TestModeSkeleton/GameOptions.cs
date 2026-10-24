using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using TestModeSkeleton.CabinetDef;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class GameOptions : TestModeBase
{
	private enum RequestSateInStateMenu
	{
		Waiting,
		ClockError
	}

	[SerializeField]
	[Tooltip("閉店設定")]
	private Text shopCloseText;

	[SerializeField]
	[Tooltip("閉店時間")]
	private Text shopCloseTimeText;

	[SerializeField]
	[Tooltip("コンティニュー設定")]
	private Text continueText;

	[SerializeField]
	[Tooltip("筐体ID")]
	private Text cabinetIdText;

	[SerializeField]
	[Tooltip("神姫購入数")]
	private Text shinkiBuyNumText;

	[SerializeField]
	[Tooltip("休憩時間")]
	private Text m_breakTimeText;

	[SerializeField]
	[Tooltip("休憩時間の長さ")]
	private Text m_breakTimeLengthText;

	[SerializeField]
	[Tooltip("LED(サイドパネル)")]
	private Text m_ledWingText;

	[SerializeField]
	[Tooltip("LED(コンパネ)")]
	private Text m_ledCtrlText;

	[SerializeField]
	[Tooltip("LED(Qボタン)")]
	private Text m_ledBttnText;

	[SerializeField]
	[Tooltip("LED(タイトル)")]
	private Text m_ledHeadText;

	[SerializeField]
	[Tooltip("LED(サイド)")]
	private Text m_ledSideText;

	[SerializeField]
	[Tooltip("無料プレーキャンペーン")]
	private Text m_freePlayCampaignBttnText;

	private Dictionary<string, TestModeValue> options = new Dictionary<string, TestModeValue>
	{
		["ShopClose"] = new TestModeValue(GameOptionsDef.ShopCloseSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["ShopCloseTime"] = new TestModeValue(GameOptionsDef.ShopCloseTimeSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => TimeSpan.FromMinutes(n).ToString("hh\\:mm") + " (<l>24-HOUR CLOCK</l>)"),
		["Continue"] = new TestModeValue(GameOptionsDef.ContinueSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["ShinkiBuyNum"] = new TestModeValue(GameOptionsDef.GachaBuyNumSetting),
		["CabinetId"] = new TestModeValue(GameOptionsDef.CabinetIdSetting),
		["BreakTime"] = new TestModeValue(GameOptionsDef.BreakTimeSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["BreakTimeLength"] = new TestModeValue(GameOptionsDef.BreakTimeLengthSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => $"{n} <l>M</l>"),
		["LEDWing"] = new TestModeValue(GameOptionsDef.LEDWingSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["LEDCtrl"] = new TestModeValue(GameOptionsDef.LEDCtrlSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["LEDBttn"] = new TestModeValue(GameOptionsDef.LEDBttnSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["LEDHead"] = new TestModeValue(GameOptionsDef.LEDHeadSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["LEDSide"] = new TestModeValue(GameOptionsDef.LEDSideSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["FreePlayCampaign"] = new TestModeValue(GameOptionsDef.FreePlayCampaignSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString)
	};

	private RequestSateInStateMenu request;

	protected override string TitleName => "<l>GAME OPTIONS</l>";

	private IEnumerator Start()
	{
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "CabinetId").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = TestModeMenuElement.TextState.NotActive;
		});
		if (SystemConfigGameData.CabinetType != CabinetType.UKS)
		{
			menuElements.FindAll((TestModeMenuElement e) => e.Event == "LEDWing" || e.Event == "LEDCtrl" || e.Event == "LEDBttn").ForEach(delegate(TestModeMenuElement e)
			{
				e.State = TestModeMenuElement.TextState.NotActive;
			});
		}
		if (SystemConfigGameData.CabinetType != CabinetType.TBS && SystemConfigGameData.CabinetType != CabinetType.UJK)
		{
			menuElements.FindAll((TestModeMenuElement e) => e.Event == "LEDHead" || e.Event == "LEDSide").ForEach(delegate(TestModeMenuElement e)
			{
				e.State = TestModeMenuElement.TextState.NotActive;
			});
		}
		options["ShopClose"].Set((int)CabinetManager.Game.Data.ShopClose, shopCloseText);
		options["ShopCloseTime"].Set(CabinetManager.Game.Data.ShopCloseTime, shopCloseTimeText);
		options["Continue"].Set((int)CabinetManager.Game.Data.ContinueMenu, continueText);
		options["ShinkiBuyNum"].Set(CabinetManager.Game.Data.GachaBuyNum, shinkiBuyNumText);
		options["CabinetId"].Set(CabinetManager.Game.Data.CabinetId, cabinetIdText);
		options["BreakTime"].Set((int)CabinetManager.Game.Data.BreakTime, m_breakTimeText);
		options["BreakTimeLength"].Set(CabinetManager.Game.Data.BreakTimeLength, m_breakTimeLengthText);
		options["LEDWing"].Set((int)CabinetManager.Game.Data.LEDWingOnOff, m_ledWingText);
		options["LEDCtrl"].Set((int)CabinetManager.Game.Data.LEDCtrlOnOff, m_ledCtrlText);
		options["LEDBttn"].Set((int)CabinetManager.Game.Data.LEDBttnOnOff, m_ledBttnText);
		options["LEDHead"].Set((int)CabinetManager.Game.Data.LEDHeadOnOff, m_ledHeadText);
		options["LEDSide"].Set((int)CabinetManager.Game.Data.LEDSideOnOff, m_ledSideText);
		options["FreePlayCampaign"].Set((int)CabinetManager.Game.Data.FreePlayCampaign, m_freePlayCampaignBttnText);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ShopCloseTimeOnOff();
		BreakTimeElementsOnOff();
		ContinueElementsOnOff();
		StateFuncManager manager = new StateFuncManager(StateMenu);
		while (true)
		{
			manager.Update();
			yield return null;
		}
	}

	private void StateMenu(StateFuncManager manager, object param)
	{
		if (manager.IsStart)
		{
			StartOrResumeMainCoroutine();
		}
		RequestSateInStateMenu requestSateInStateMenu = request;
		if (requestSateInStateMenu != 0 && requestSateInStateMenu == RequestSateInStateMenu.ClockError)
		{
			request = RequestSateInStateMenu.Waiting;
			PauseMainCoroutine();
			manager.Change(StateClockError);
		}
	}

	private void StateClockError(StateFuncManager manager, object param)
	{
		if (manager.IsStart)
		{
			ArrowkeyUp.SetActive(value: false);
			ArrowkeyDown.SetActive(value: false);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: true);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			notice.StartFirstSetClock(delegate
			{
				help.SaveAndSet(TestModeHelp.HelpType.Exit);
			});
		}
		if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			notice.Stop(delegate
			{
				help.SetFromSave();
			});
			manager.Change(StateMenu);
		}
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		if ((!SingletonBehaviour<TestModeInput>.Instance.IsAddValue() && !SingletonBehaviour<TestModeInput>.Instance.IsSubValue()) || !options.ContainsKey(element.Event))
		{
			return;
		}
		if (element.Event == "ShopClose")
		{
			if (!CabinetManager.Clock.Data.Initialized && options["ShopClose"].CurrentString == "OFF")
			{
				request = RequestSateInStateMenu.ClockError;
				return;
			}
			options[element.Event].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
			ShopCloseTimeOnOff();
			return;
		}
		options[element.Event].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
		if (element.Event == "BreakTime")
		{
			BreakTimeElementsOnOff();
		}
		else if (element.Event == "Continue")
		{
			ContinueElementsOnOff();
		}
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		switch (element.Event)
		{
		case "ShopClose":
		case "ShopCloseTime":
		case "Continue":
		case "ShinkiBuyNum":
		case "CabinetId":
		case "BreakTime":
		case "BreakTimeLength":
		case "LEDWing":
		case "LEDCtrl":
		case "LEDBttn":
		case "LEDHead":
		case "LEDSide":
		case "FreePlayCampaign":
			help.Set(TestModeHelp.HelpType.SelectChange);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: false);
			break;
		default:
			help.Set(TestModeHelp.HelpType.SelectExecute);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: true);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			break;
		}
	}

	protected override void OnFactorySettings(TestModeMenuElement element)
	{
		options.Values.ToList().ForEach(delegate(TestModeValue v)
		{
			v.ToDefault();
		});
		ShopCloseTimeOnOff();
		BreakTimeElementsOnOff();
		ContinueElementsOnOff();
	}

	protected override void OnSave()
	{
		CabinetManager.Game.Data.ShopClose = (ClosingTimeOptionsDef.ShopClose)options["ShopClose"].Current;
		CabinetManager.Game.Data.ShopCloseTime = options["ShopCloseTime"].Current;
		CabinetManager.Game.Data.ContinueMenu = (GameOptionsDef.ContinueOnOff)options["Continue"].Current;
		CabinetManager.Game.Data.GachaBuyNum = options["ShinkiBuyNum"].Current;
		CabinetManager.Game.Data.CabinetId = options["CabinetId"].Current;
		CabinetManager.Game.Data.BreakTime = (GameOptionsDef.BreakTime)options["BreakTime"].Current;
		CabinetManager.Game.Data.BreakTimeLength = options["BreakTimeLength"].Current;
		CabinetManager.Game.Data.LEDWingOnOff = (GameOptionsDef.LEDWingOnOff)options["LEDWing"].Current;
		CabinetManager.Game.Data.LEDCtrlOnOff = (GameOptionsDef.LEDCtrlOnOff)options["LEDCtrl"].Current;
		CabinetManager.Game.Data.LEDBttnOnOff = (GameOptionsDef.LEDBttnOnOff)options["LEDBttn"].Current;
		CabinetManager.Game.Data.LEDHeadOnOff = (GameOptionsDef.LEDHeadOnOff)options["LEDHead"].Current;
		CabinetManager.Game.Data.LEDSideOnOff = (GameOptionsDef.LEDSideOnOff)options["LEDSide"].Current;
		CabinetManager.Game.Data.FreePlayCampaign = (GameOptionsDef.FreePlayCampaign)options["FreePlayCampaign"].Current;
		CabinetManager.Game.Save(CabinetManager.Clock.GetTime(), CabinetManager.Coin.Data);
	}

	protected override bool IsModification()
	{
		return options.Values.Any((TestModeValue v) => v.IsModification);
	}

	private void ShopCloseTimeOnOff()
	{
		bool isActive = options["ShopClose"].CurrentString == "ON";
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "ShopCloseTime").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = ((!isActive) ? TestModeMenuElement.TextState.NotActive : TestModeMenuElement.TextState.Default);
		});
	}

	private void ContinueElementsOnOff()
	{
		bool isSubItemDisp = options["Continue"].CurrentString == "ON";
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "BreakTime").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = ((!isSubItemDisp) ? TestModeMenuElement.TextState.NotActive : TestModeMenuElement.TextState.Default);
		});
		BreakTimeElementsOnOff();
	}

	private void BreakTimeElementsOnOff()
	{
		bool flag = menuElements.Find((TestModeMenuElement e) => e.Event == "BreakTime").State != TestModeMenuElement.TextState.NotActive;
		bool isSubItemDisp = flag && options["BreakTime"].CurrentString == "ON";
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "BreakTimeLength").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = ((!isSubItemDisp) ? TestModeMenuElement.TextState.NotActive : TestModeMenuElement.TextState.Default);
		});
	}
}

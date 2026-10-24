using System.Collections;
using System.Collections.Generic;
using LampTester;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class LampCheckTest : TestModeBase
{
	[SerializeField]
	private Transform MenuRoot;

	private Dictionary<string, Text> valueTextDict = new Dictionary<string, Text>();

	private Coroutine coAuto;

	private BinaryLedTester irLedTester;

	private BinaryLedTester buttonLedTester;

	private ColorLedTester stickLedTester;

	private ColorLedTester cardReaderLedTester;

	private Text debugText;

	private int curLedNo;

	protected override string TitleName => "<l>LAMP CHECK TEST</l>";

	private IEnumerator Start()
	{
		irLedTester = base.gameObject.AddComponent<BinaryLedTester>();
		buttonLedTester = base.gameObject.AddComponent<BinaryLedTester>();
		stickLedTester = base.gameObject.AddComponent<ColorLedTester>();
		cardReaderLedTester = base.gameObject.AddComponent<ColorLedTester>();
		for (int i = 0; i < MenuRoot.childCount; i++)
		{
			Transform child = MenuRoot.GetChild(i);
			Text component = child.GetComponent<Text>();
			if (!(component == null))
			{
				Text text = child.Find("Value")?.GetComponent<Text>();
				if (!(text == null))
				{
					valueTextDict[component.name] = text;
				}
			}
		}
		help.Set(TestModeHelp.HelpType.SelectExit);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		StartOrResumeMainCoroutine();
		yield return new WaitUntil(() => SingletonBehaviour<TestModeInput>.Instance.IsRepeat(TestModeInput.Key.Service) || SingletonBehaviour<TestModeInput>.Instance.IsRepeat(TestModeInput.Key.Enter));
		Exit();
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		DebugUpdate();
		_ = element.Event;
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		help.Set(TestModeHelp.HelpType.SelectExit);
		foreach (Text value in valueTextDict.Values)
		{
			value.text = TestModeBase.Off;
		}
		valueTextDict[element.Event].text = TestModeBase.On;
		curLedNo = 0;
		GameObject.Find("LampCheckTest/Canvas/Menu/TitleR/Led").GetComponent<Text>().text = "全LED";
		GameObject.Find("LampCheckTest/Canvas/Menu/TitleG/Led").GetComponent<Text>().text = "全LED";
		GameObject.Find("LampCheckTest/Canvas/Menu/TitleB/Led").GetComponent<Text>().text = "全LED";
		GameObject.Find("LampCheckTest/Canvas/Menu/TitleW/Led").GetComponent<Text>().text = "全LED";
		GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideR/Led").GetComponent<Text>().text = "全LED";
		GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideG/Led").GetComponent<Text>().text = "全LED";
		GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideB/Led").GetComponent<Text>().text = "全LED";
		GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideW/Led").GetComponent<Text>().text = "全LED";
		GameObject.Find("LampCheckTest/Canvas/Menu/TitleR/Led").GetComponent<Text>().color = Color.white;
		GameObject.Find("LampCheckTest/Canvas/Menu/TitleG/Led").GetComponent<Text>().color = Color.white;
		GameObject.Find("LampCheckTest/Canvas/Menu/TitleB/Led").GetComponent<Text>().color = Color.white;
		GameObject.Find("LampCheckTest/Canvas/Menu/TitleW/Led").GetComponent<Text>().color = Color.white;
		GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideR/Led").GetComponent<Text>().color = Color.white;
		GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideG/Led").GetComponent<Text>().color = Color.white;
		GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideB/Led").GetComponent<Text>().color = Color.white;
		GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideW/Led").GetComponent<Text>().color = Color.white;
		switch (element.Event)
		{
		case "All":
			foreach (Text value2 in valueTextDict.Values)
			{
				value2.text = TestModeBase.On;
			}
			valueTextDict["Auto"].text = TestModeBase.Off;
			break;
		case "Auto":
			coAuto = StartCoroutine(CoAutoCheck(0.8f));
			break;
		case "TitleR":
			GameObject.Find("LampCheckTest/Canvas/Menu/TitleR/Led").GetComponent<Text>().color = Color.red;
			set_help_by_menu();
			break;
		case "TitleG":
			GameObject.Find("LampCheckTest/Canvas/Menu/TitleG/Led").GetComponent<Text>().color = Color.red;
			set_help_by_menu();
			break;
		case "TitleB":
			GameObject.Find("LampCheckTest/Canvas/Menu/TitleB/Led").GetComponent<Text>().color = Color.red;
			set_help_by_menu();
			break;
		case "TitleW":
			GameObject.Find("LampCheckTest/Canvas/Menu/TitleW/Led").GetComponent<Text>().color = Color.red;
			valueTextDict["TitleR"].text = TestModeBase.On;
			valueTextDict["TitleG"].text = TestModeBase.On;
			valueTextDict["TitleB"].text = TestModeBase.On;
			set_help_by_menu();
			break;
		case "LeftSideR":
			GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideR/Led").GetComponent<Text>().color = Color.red;
			set_help_by_menu();
			break;
		case "LeftSideG":
			GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideG/Led").GetComponent<Text>().color = Color.red;
			set_help_by_menu();
			break;
		case "LeftSideB":
			GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideB/Led").GetComponent<Text>().color = Color.red;
			set_help_by_menu();
			break;
		case "LeftSideW":
			GameObject.Find("LampCheckTest/Canvas/Menu/LeftSideW/Led").GetComponent<Text>().color = Color.red;
			valueTextDict["LeftSideR"].text = TestModeBase.On;
			valueTextDict["LeftSideG"].text = TestModeBase.On;
			valueTextDict["LeftSideB"].text = TestModeBase.On;
			set_help_by_menu();
			break;
		case "Button":
			buttonLedTester.SetEnable(on: true);
			break;
		case "IR":
			irLedTester.SetEnable(on: true);
			break;
		case "StickR":
			stickLedTester.SetState(ColorLedState_TestMode.RED);
			break;
		case "StickG":
			stickLedTester.SetState(ColorLedState_TestMode.GREEN);
			break;
		case "StickB":
			stickLedTester.SetState(ColorLedState_TestMode.BLUE);
			break;
		case "StickW":
			valueTextDict["StickR"].text = TestModeBase.On;
			valueTextDict["StickG"].text = TestModeBase.On;
			valueTextDict["StickB"].text = TestModeBase.On;
			stickLedTester.SetState(ColorLedState_TestMode.WHITE);
			break;
		case "CardReaderR":
			cardReaderLedTester.SetState(ColorLedState_TestMode.RED);
			break;
		case "CardReaderG":
			cardReaderLedTester.SetState(ColorLedState_TestMode.GREEN);
			break;
		case "CardReaderB":
			cardReaderLedTester.SetState(ColorLedState_TestMode.BLUE);
			break;
		case "CardReaderW":
			valueTextDict["CardReaderR"].text = TestModeBase.On;
			valueTextDict["CardReaderG"].text = TestModeBase.On;
			valueTextDict["CardReaderB"].text = TestModeBase.On;
			cardReaderLedTester.SetState(ColorLedState_TestMode.WHITE);
			break;
		}
	}

	private void set_help_by_menu()
	{
		ArrowkeyLeft.SetActive(value: true);
		ArrowkeyRight.SetActive(value: true);
		help.Set(TestModeHelp.HelpType.SelectChangeLedExit);
	}

	protected override void OnChangeMenuOff(TestModeMenuElement element)
	{
		if (coAuto != null)
		{
			StopCoroutine(ref coAuto);
		}
	}

	private IEnumerator CoAutoCheck(float seconds)
	{
		_ = new TapeLedState_TestMode[4]
		{
			TapeLedState_TestMode.RED,
			TapeLedState_TestMode.GREEN,
			TapeLedState_TestMode.BLUE,
			TapeLedState_TestMode.WHITE
		};
		ColorLedState_TestMode[] color_led_arry = new ColorLedState_TestMode[4]
		{
			ColorLedState_TestMode.RED,
			ColorLedState_TestMode.GREEN,
			ColorLedState_TestMode.BLUE,
			ColorLedState_TestMode.WHITE
		};
		while (true)
		{
			ColorLedState_TestMode[] array = color_led_arry;
			foreach (ColorLedState_TestMode colorLedState_TestMode in array)
			{
				foreach (Text value in valueTextDict.Values)
				{
					value.text = TestModeBase.Off;
				}
				valueTextDict["Auto"].text = TestModeBase.On;
				if (colorLedState_TestMode == ColorLedState_TestMode.WHITE || colorLedState_TestMode == ColorLedState_TestMode.RED)
				{
					valueTextDict["CardReaderR"].text = TestModeBase.On;
				}
				if (colorLedState_TestMode == ColorLedState_TestMode.WHITE || colorLedState_TestMode == ColorLedState_TestMode.GREEN)
				{
					valueTextDict["CardReaderG"].text = TestModeBase.On;
				}
				if (colorLedState_TestMode == ColorLedState_TestMode.WHITE || colorLedState_TestMode == ColorLedState_TestMode.BLUE)
				{
					valueTextDict["CardReaderB"].text = TestModeBase.On;
				}
				if (colorLedState_TestMode == ColorLedState_TestMode.WHITE)
				{
					valueTextDict["CardReaderW"].text = TestModeBase.On;
				}
				cardReaderLedTester.SetState(colorLedState_TestMode);
				yield return new WaitForSeconds(seconds);
			}
			array = color_led_arry;
			foreach (ColorLedState_TestMode colorLedState_TestMode2 in array)
			{
				foreach (Text value2 in valueTextDict.Values)
				{
					value2.text = TestModeBase.Off;
				}
				valueTextDict["Auto"].text = TestModeBase.On;
				if (colorLedState_TestMode2 == ColorLedState_TestMode.WHITE || colorLedState_TestMode2 == ColorLedState_TestMode.RED)
				{
					valueTextDict["StickR"].text = TestModeBase.On;
				}
				if (colorLedState_TestMode2 == ColorLedState_TestMode.WHITE || colorLedState_TestMode2 == ColorLedState_TestMode.GREEN)
				{
					valueTextDict["StickG"].text = TestModeBase.On;
				}
				if (colorLedState_TestMode2 == ColorLedState_TestMode.WHITE || colorLedState_TestMode2 == ColorLedState_TestMode.BLUE)
				{
					valueTextDict["StickB"].text = TestModeBase.On;
				}
				if (colorLedState_TestMode2 == ColorLedState_TestMode.WHITE)
				{
					valueTextDict["StickW"].text = TestModeBase.On;
				}
				stickLedTester.SetState(colorLedState_TestMode2);
				yield return new WaitForSeconds(seconds);
			}
			foreach (Text value3 in valueTextDict.Values)
			{
				value3.text = TestModeBase.Off;
			}
			valueTextDict["Auto"].text = TestModeBase.On;
			valueTextDict["Button"].text = TestModeBase.On;
			buttonLedTester.SetEnable(on: true);
			yield return new WaitForSeconds(seconds);
			foreach (Text value4 in valueTextDict.Values)
			{
				value4.text = TestModeBase.Off;
			}
			valueTextDict["Auto"].text = TestModeBase.On;
			valueTextDict["IR"].text = TestModeBase.On;
			irLedTester.SetEnable(on: true);
			yield return new WaitForSeconds(seconds);
			foreach (Text value5 in valueTextDict.Values)
			{
				value5.text = TestModeBase.On;
			}
			yield return new WaitForSeconds(seconds);
		}
	}

	private void DebugUpdate()
	{
	}
}

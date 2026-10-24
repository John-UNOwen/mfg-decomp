using System;
using System.Collections.Generic;
using System.Linq;
using TestModeSkeleton.CabinetDef;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class LanguageOptions : TestModeBase
{
	[SerializeField]
	[Tooltip("ゲームモード設定")]
	private Text gameModeText;

	[SerializeField]
	[Tooltip("テストモード設定")]
	private Text testModeText;

	private Dictionary<string, TestModeValue> options = new Dictionary<string, TestModeValue>
	{
		["GameMode"] = new TestModeValue(LanguageOptionsDef.GameModeSetting, isLoop: true, TestModeValue.TextColorType.Default, ToGameModeString),
		["TestMode"] = new TestModeValue(LanguageOptionsDef.TestModeSetting, isLoop: true, TestModeValue.TextColorType.Default, ToTestModeString)
	};

	private static readonly Converter<int, string> ToGameModeString = (int n) => new string[5] { "日本語", "ENGLISH", "한국어", "中文(繁體)", "中文(简体)" }.ElementAtOrDefault(n) ?? n.ToString();

	private static readonly Converter<int, string> ToTestModeString = (int n) => new string[2] { "日本語", "ENGLISH" }.ElementAtOrDefault(n) ?? n.ToString();

	protected override string TitleName => "<l>LANGUAGE OPTIONS</l>";

	private void Start()
	{
		options["GameMode"].Set((int)CabinetManager.Lang.Data.GameModeLanguage, gameModeText);
		options["TestMode"].Set((int)CabinetManager.Lang.Data.TestModeLanguage, testModeText);
		StartOrResumeMainCoroutine();
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		ArrowkeyEnter.SetActive(value: true);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		if ((SingletonBehaviour<TestModeInput>.Instance.IsAddValue() || SingletonBehaviour<TestModeInput>.Instance.IsSubValue()) && options.ContainsKey(element.Event))
		{
			options[element.Event].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
		}
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		string @event = element.Event;
		if (@event == "GameMode" || @event == "TestMode")
		{
			help.Set(TestModeHelp.HelpType.SelectChange);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: false);
		}
		else
		{
			help.Set(TestModeHelp.HelpType.SelectExecute);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: true);
		}
	}

	protected override void OnFactorySettings(TestModeMenuElement element)
	{
		options.Values.ToList().ForEach(delegate(TestModeValue v)
		{
			v.ToDefault();
		});
	}

	protected override void OnSave()
	{
		CabinetManager.Lang.Data.GameModeLanguage = (LanguageOptionsDef.GameMode)options["GameMode"].Current;
		CabinetManager.Lang.Data.TestModeLanguage = (LanguageOptionsDef.TestMode)options["TestMode"].Current;
		CabinetManager.Lang.Save();
	}

	protected override bool IsModification()
	{
		return options.Values.Any((TestModeValue v) => v.IsModification);
	}
}

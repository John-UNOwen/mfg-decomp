using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.Types;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class SoundOptions : TestModeBase
{
	[Serializable]
	private class SliderSet
	{
		public Slider slider;

		public Image fillArea;

		public Text text;
	}

	[SerializeField]
	[Tooltip("スケールチェック")]
	private Text scaleCheckText;

	[SerializeField]
	[Tooltip("フェーズチェック")]
	private Text phaseCheckText;

	[SerializeField]
	[Tooltip("サウンドボリューム")]
	private SliderSet volumeSlider;

	[SerializeField]
	[Tooltip("ヘッドホンフェーズチェック")]
	private Text headphaseCheckText;

	[SerializeField]
	[Tooltip("ヘッドホンボリューム")]
	private SliderSet headvolumeSlider;

	[SerializeField]
	[Tooltip("アトラクトモード")]
	private Text attactModeText;

	[SerializeField]
	[Tooltip("UKS筐体専用文言")]
	private GameObject uksOnlyObject;

	private Coroutine scaleCheckCoroutine;

	private Coroutine phaseCheckCoroutine;

	private Coroutine headphaseCheckCoroutine;

	private TestModeInput.SwitchingOperation serviceButton = new TestModeInput.SwitchingOperation(TestModeInput.Key.Service);

	private Dictionary<string, TestModeValue> options = new Dictionary<string, TestModeValue>
	{
		["Volume"] = new TestModeValue(SoundOptionsDef.SoundVolumeSetting, isLoop: false),
		["Attract"] = new TestModeValue(SoundOptionsDef.SoundVolumeInAttractModeSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => $"{n}%"),
		["HeadVolume"] = new TestModeValue(SoundOptionsDef.SoundHeadVolumeSetting, isLoop: false, TestModeValue.TextColorType.OnlyWhite)
	};

	protected override string TitleName => "<l>SOUND OPTIONS</l>";

	private void Start()
	{
		if (!GameUtility.IsInhouseLocation())
		{
			menuElements.FindAll((TestModeMenuElement e) => e.Event == "SetVolumeTo1").ForEach(delegate(TestModeMenuElement e)
			{
				e.State = TestModeMenuElement.TextState.NotActive;
			});
		}
		if (SystemConfigGameData.CabinetType != CabinetType.UKS)
		{
			uksOnlyObject.SetActive(value: false);
		}
		options["Volume"].Set(CabinetManager.Sound.Data.SoundVolume, volumeSlider.text, volumeSlider.slider, volumeSlider.fillArea);
		options["Attract"].Set(CabinetManager.Sound.Data.SoundVolumeInAttractMode, attactModeText);
		options["HeadVolume"].Set(CabinetManager.Sound.Data.SoundHeadVolume, headvolumeSlider.text, headvolumeSlider.slider, headvolumeSlider.fillArea);
		scaleCheckText.gameObject.SetActive(value: false);
		phaseCheckText.gameObject.SetActive(value: false);
		if (headphaseCheckText != null)
		{
			headphaseCheckText.gameObject.SetActive(value: false);
		}
		StartOrResumeMainCoroutine();
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		if (element.Event == "Volume" || element.Event == "HeadVolume" || element.Event == "Attract")
		{
			bool flag = SingletonBehaviour<TestModeInput>.Instance.IsRepeat(TestModeInput.Key.Right) || serviceButton.IsAdd();
			bool flag2 = SingletonBehaviour<TestModeInput>.Instance.IsRepeat(TestModeInput.Key.Left) || serviceButton.IsSub();
			if (flag || flag2)
			{
				options[element.Event].Change(flag);
				float num = (float)options[element.Event].Current / (float)options[element.Event].Max;
				if (element.Event == "Volume")
				{
					SoundManager.SetSpeakerVolume(num);
				}
				else if (element.Event == "HeadVolume")
				{
					SoundManager.SetHeadPhoneVolume(num);
				}
				else if (element.Event == "Attract")
				{
					SoundManager.SetAttractVolume(num);
				}
			}
		}
		else if (element.Event == "SetVolumeTo1")
		{
			if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
			{
				options["Volume"].ForcedChange(1);
			}
		}
		else if ((SingletonBehaviour<TestModeInput>.Instance.IsAddValue() || SingletonBehaviour<TestModeInput>.Instance.IsSubValue()) && options.ContainsKey(element.Event))
		{
			options[element.Event].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
		}
		serviceButton.Update();
		SetHeadVolumeFillColor();
	}

	private void SetHeadVolumeFillColor()
	{
		GameObject.Find("SoundOptions/Canvas/Menu/HeadVolumeGauge/Slider/Fill Area/Fill").GetComponent<Image>().color = Color.gray;
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		SoundManager.StopSeAll().Forget();
		switch (element.Event)
		{
		case "SetVolumeTo1":
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: true);
			break;
		case "ScaleCheck":
			SoundManager.SetSpeakerVolume((float)options["Volume"].Current / (float)options["Volume"].Max);
			SoundManager.SetAttractVolume(1f);
			SoundManager.PlaySe("SFX_CHECK").Forget();
			scaleCheckText.gameObject.SetActive(value: true);
			scaleCheckCoroutine = StartCoroutine(CoScaleCheck(0.75f));
			help.Set(TestModeHelp.HelpType.Select);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			break;
		case "PhaseCheck":
			SoundManager.SetSpeakerVolume((float)options["Volume"].Current / (float)options["Volume"].Max);
			SoundManager.SetAttractVolume(1f);
			SoundManager.PlaySe("SFX_LOUDSOFT").Forget();
			phaseCheckText.gameObject.SetActive(value: true);
			phaseCheckCoroutine = StartCoroutine(CoPhaseCheck(2f));
			help.Set(TestModeHelp.HelpType.Select);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			break;
		case "HeadPhaseCheck":
			if (SystemConfigGameData.CabinetType != CabinetType.TBS && SystemConfigGameData.CabinetType != CabinetType.UJK)
			{
				SoundManager.SetSpeakerVolume(0f);
			}
			SoundManager.SetAttractVolume(1f);
			SoundManager.SetHeadPhoneVolume((float)options["HeadVolume"].Current / (float)options["HeadVolume"].Max);
			SoundManager.PlaySe("SFX_CHECK").Forget();
			headphaseCheckText.gameObject.SetActive(value: true);
			headphaseCheckCoroutine = StartCoroutine(CoHeadScaleCheck(0.75f));
			help.Set(TestModeHelp.HelpType.Select);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			break;
		case "Volume":
		case "HeadVolume":
		case "Attract":
			SoundManager.PlayBgm("BGM_HOME").Forget();
			if (element.Event == "Volume")
			{
				SoundManager.SetSpeakerVolume((float)options[element.Event].Current / (float)options[element.Event].Max);
				SoundManager.SetAttractVolume(1f);
			}
			else if (element.Event == "HeadVolume")
			{
				if (SystemConfigGameData.CabinetType != CabinetType.TBS && SystemConfigGameData.CabinetType != CabinetType.UJK)
				{
					SoundManager.SetSpeakerVolume(0f);
				}
				SoundManager.SetAttractVolume(1f);
				SoundManager.SetHeadPhoneVolume((float)options[element.Event].Current / (float)options[element.Event].Max);
			}
			else if (element.Event == "Attract")
			{
				SoundManager.SetSpeakerVolume((float)options["Volume"].Current / (float)options["Volume"].Max);
				SoundManager.SetAttractVolume((float)options[element.Event].Current / (float)options[element.Event].Max);
			}
			help.Set(TestModeHelp.HelpType.SelectChange);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			break;
		default:
			help.Set(TestModeHelp.HelpType.SelectExecute);
			ArrowkeyEnter.SetActive(value: true);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			break;
		}
		serviceButton.Reset();
	}

	protected override void OnChangeMenuOff(TestModeMenuElement element)
	{
		SoundManager.StopAll().Forget();
		switch (element.Event)
		{
		case "ScaleCheck":
			StopCoroutine(ref scaleCheckCoroutine);
			scaleCheckText.gameObject.SetActive(value: false);
			break;
		case "PhaseCheck":
			StopCoroutine(ref phaseCheckCoroutine);
			phaseCheckText.gameObject.SetActive(value: false);
			break;
		case "HeadPhaseCheck":
			StopCoroutine(ref headphaseCheckCoroutine);
			headphaseCheckText.gameObject.SetActive(value: false);
			SoundManager.SetSpeakerVolume((float)options["Volume"].Current / (float)options["Volume"].Max);
			SoundManager.SetAttractVolume(1f);
			break;
		case "HeadVolume":
			SoundManager.SetSpeakerVolume((float)options["Volume"].Current / (float)options["Volume"].Max);
			SoundManager.SetAttractVolume(1f);
			break;
		case "Attract":
			SoundManager.SetSpeakerVolume((float)options["Volume"].Current / (float)options["Volume"].Max);
			SoundManager.SetAttractVolume(1f);
			break;
		}
	}

	protected override void OnFactorySettings(TestModeMenuElement element)
	{
		options.Values.ToList().ForEach(delegate(TestModeValue v)
		{
			v.ToDefault();
		});
		SoundManager.SetSpeakerVolume((float)options["Volume"].Current / (float)options["Volume"].Max);
		SoundManager.SetHeadPhoneVolume((float)options["HeadVolume"].Current / (float)options["HeadVolume"].Max);
		SoundManager.SetAttractVolume(1f);
		SetHeadVolumeFillColor();
	}

	protected override void OnSave()
	{
		CabinetManager.Sound.Data.SoundVolume = options["Volume"].Current;
		CabinetManager.Sound.Data.SoundVolumeInAttractMode = options["Attract"].Current;
		CabinetManager.Sound.Data.SoundHeadVolume = SoundOptionsDef.SoundHeadVolumeSetting.Default;
		CabinetManager.Sound.Save();
	}

	protected override bool IsModification()
	{
		return options.Any((KeyValuePair<string, TestModeValue> v) => v.Value.IsModification && v.Key != "HeadVolume");
	}

	private IEnumerator CoScaleCheck(float seconds)
	{
		while (true)
		{
			scaleCheckText.SetLocalizedText("<l>LEFT</l>");
			yield return new WaitForSeconds(seconds);
			scaleCheckText.SetLocalizedText("<l>RIGHT</l>");
			yield return new WaitForSeconds(seconds);
		}
	}

	private IEnumerator CoPhaseCheck(float seconds)
	{
		while (true)
		{
			phaseCheckText.SetLocalizedText("<l>LOUD</l>");
			yield return new WaitForSeconds(seconds);
			phaseCheckText.SetLocalizedText("<l>SOFT</l>");
			yield return new WaitForSeconds(seconds);
		}
	}

	private IEnumerator CoHeadScaleCheck(float seconds)
	{
		while (true)
		{
			headphaseCheckText.SetLocalizedText("<l>LEFT</l>");
			yield return new WaitForSeconds(seconds);
			headphaseCheckText.SetLocalizedText("<l>RIGHT</l>");
			yield return new WaitForSeconds(seconds);
		}
	}
}

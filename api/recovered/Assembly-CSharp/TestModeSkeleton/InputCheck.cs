using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class InputCheck : TestModeBase
{
	[SerializeField]
	[Tooltip("テストボタン入力結果")]
	private Text testText;

	[SerializeField]
	[Tooltip("サービスボタン入力結果")]
	private Text serviceText;

	[SerializeField]
	[Tooltip("コイン入力結果")]
	private Text coinText;

	[SerializeField]
	private Text trigger1Text;

	[SerializeField]
	private Text trigger2Text;

	[SerializeField]
	private Text joystickButton1Text;

	[SerializeField]
	private Text button1Text;

	[SerializeField]
	private Text button2Text;

	[SerializeField]
	private Text button3Text;

	[SerializeField]
	private Text button4Text;

	[SerializeField]
	private Text headPhoneText;

	protected override string TitleName => "<l>INPUT CHECK</l>";

	private IEnumerator Start()
	{
		help.Set(TestModeHelp.HelpType.SpecialOperationExit2);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyEnter.SetActive(value: true);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		if (SystemConfigGameData.CabinetType == CabinetType.UKS)
		{
			GameObject.Find("Canvas/Menu/Button 1").SetActive(value: true);
			GameObject.Find("Canvas/Menu/Button 2").SetActive(value: true);
			GameObject.Find("Canvas/Menu/Button 3").SetActive(value: true);
			GameObject.Find("Canvas/Menu/Button 4").SetActive(value: true);
		}
		else
		{
			GameObject.Find("Canvas/Menu/Button 1").SetActive(value: false);
			GameObject.Find("Canvas/Menu/Button 2").SetActive(value: false);
			GameObject.Find("Canvas/Menu/Button 3").SetActive(value: false);
			GameObject.Find("Canvas/Menu/Button 4").SetActive(value: false);
		}
		StartCoroutine(CoCheckNormal());
		StartCoroutine(CoCheckCoin());
		Bi2xInput.Instance.ControlCoinBlocker(is_open: true);
		yield return new WaitUntil(() => SingletonBehaviour<TestModeInput>.Instance.IsSpecialOperation2());
		Bi2xInput.Instance.ControlCoinBlocker(is_open: false);
		Exit();
	}

	private IEnumerator CoCheckNormal()
	{
		Dictionary<TestModeInput.Key, Text> inputs = new Dictionary<TestModeInput.Key, Text>
		{
			[TestModeInput.Key.Test] = testText,
			[TestModeInput.Key.Service] = serviceText,
			[TestModeInput.Key.Button2] = button4Text,
			[TestModeInput.Key.Button3] = button3Text,
			[TestModeInput.Key.Button4] = button2Text,
			[TestModeInput.Key.Button1] = button1Text,
			[TestModeInput.Key.HeadPhone] = headPhoneText
		};
		while (true)
		{
			inputs.Keys.ToList().ForEach(delegate(TestModeInput.Key k)
			{
				inputs[k].text = (SingletonBehaviour<TestModeInput>.Instance.IsPress(k) ? TestModeBase.On : TestModeBase.Off);
			});
			yield return null;
		}
	}

	private IEnumerator CoCheckCoin()
	{
		while (true)
		{
			coinText.text = TestModeBase.Off;
			if (SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Coin))
			{
				coinText.text = TestModeBase.On;
				yield return new WaitForSeconds(3f);
			}
			yield return null;
		}
	}
}

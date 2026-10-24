using System.Collections;
using MFG.Types;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class MechanismCheck : TestModeBase
{
	[SerializeField]
	[Tooltip("コインブロッカー状態")]
	private Text coinBlockerText;

	[SerializeField]
	[Tooltip("USB充電状態")]
	private Text usbChargerText;

	private bool m_bCoinBlockerOpen;

	private bool m_bUsbChargerOn;

	protected override string TitleName => "<l>MECHANISM CHECK</l>";

	private IEnumerator Start()
	{
		if (SystemConfigGameData.CabinetType != CabinetType.HG)
		{
			menuElements.FindAll((TestModeMenuElement e) => e.Event == "USBCharger").ForEach(delegate(TestModeMenuElement e)
			{
				e.State = TestModeMenuElement.TextState.NotActive;
			});
		}
		help.Set(TestModeHelp.HelpType.ChangeExit);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: true);
		ArrowkeyRight.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		StartOrResumeMainCoroutine();
		yield return new WaitUntil(() => SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Enter) || SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Service));
		Bi2xInput.Instance.ControlCoinBlocker(is_open: false);
		Bi2xInput.Instance.EnableUsbCharger(is_open: false);
		Exit();
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		bool flag = SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Left) || SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Right) || SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Test);
		string @event = element.Event;
		if (!(@event == "CoinBlocker"))
		{
			if (@event == "USBCharger" && flag)
			{
				m_bUsbChargerOn = !m_bUsbChargerOn;
				if (m_bUsbChargerOn)
				{
					Bi2xInput.Instance.EnableUsbCharger(is_open: true);
					usbChargerText.SetLocalizedText("<l>ENABLE</l>");
				}
				else
				{
					Bi2xInput.Instance.EnableUsbCharger(is_open: false);
					usbChargerText.SetLocalizedText("<l>DISABLE</l>");
				}
			}
		}
		else if (flag)
		{
			m_bCoinBlockerOpen = !m_bCoinBlockerOpen;
			if (m_bCoinBlockerOpen)
			{
				Bi2xInput.Instance.ControlCoinBlocker(is_open: true);
				coinBlockerText.SetLocalizedText("<l>OPEN</l>");
			}
			else
			{
				Bi2xInput.Instance.ControlCoinBlocker(is_open: false);
				coinBlockerText.SetLocalizedText("<l>CLOSE</l>");
			}
		}
	}
}

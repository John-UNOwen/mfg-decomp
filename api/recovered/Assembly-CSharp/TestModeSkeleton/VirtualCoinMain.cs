using System;
using KAMUNITY;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class VirtualCoinMain : VirtualCoinBase
{
	[SerializeField]
	[Tooltip("シーン固有オブジェクト")]
	private GameObject[] uniqueObjects;

	[SerializeField]
	[Tooltip("ステータス表示")]
	private Text statusText;

	private void Start()
	{
		help.Set(TestModeHelp.HelpType.SelectExecute);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		if (AvsEa3Coin.Ea3EacoinGetStatus() == AvsTypes.EA3_EACOIN_STATUS.AVAILABLE)
		{
			statusText.SetLocalizedText("<color=lime><l>VC/AVAILABLE</l></color>");
		}
		else
		{
			statusText.SetLocalizedText("<color=red><l>VC/NOT AVAILABLE</l></color>");
			if (CabinetManager.NetWork.Data.EAmusement == NetworkOptionsDef.EAmusement.Off)
			{
				statusText.text += "\n";
				statusText.text += "e-amusementがOFFです";
			}
			if (CabinetManager.Coin.Data.FreePlay == CoinOptionsDef.FreePlay.On)
			{
				statusText.text += "\n";
				statusText.text += "フリープレーがONです";
			}
			menuElements.FindAll((TestModeMenuElement e) => e.Mode == TestModeMenuElement.EventMode.NextScene && e.Event != "OperationSettings").ForEach(delegate(TestModeMenuElement e)
			{
				e.State = TestModeMenuElement.TextState.UnSelctable;
			});
		}
		if (TestModeUtility.IsPayableAtVirtualCoinSettingsWithActiveSchedule())
		{
			notice.Stop();
		}
		else
		{
			notice.StartPaymentWarning();
		}
		StartOrResumeMainCoroutine();
	}

	protected override void OnCall(TestModeMenuElement element)
	{
		Array.ForEach(uniqueObjects, delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
	}

	protected override void OnReturn(TestModeMenuElement element)
	{
		Array.ForEach(uniqueObjects, delegate(GameObject o)
		{
			o.SetActive(value: true);
		});
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		if (TestModeUtility.IsPayableAtVirtualCoinSettingsWithActiveSchedule())
		{
			notice.Stop();
		}
		else
		{
			notice.StartPaymentWarning();
		}
	}
}

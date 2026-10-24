using System;
using TestModeSkeleton.CabinetDef;
using UnityEngine;

namespace TestModeSkeleton;

public class OperationSettings : VirtualCoinBase
{
	[SerializeField]
	[Tooltip("シーン固有オブジェクト")]
	private GameObject[] uniqueObjects;

	protected override string SubTitleName => "<l>OPERATION SETTINGS</l>";

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

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		switch (element.Extra)
		{
		case "1":
			Pattern.CurrentPattern = VirtualCoinDef.Patterns.P1;
			break;
		case "2":
			Pattern.CurrentPattern = VirtualCoinDef.Patterns.P2;
			break;
		case "3":
			Pattern.CurrentPattern = VirtualCoinDef.Patterns.P3;
			break;
		}
	}
}

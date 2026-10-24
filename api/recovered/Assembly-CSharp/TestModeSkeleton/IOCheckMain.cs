using System;
using MFG.Types;
using UnityEngine;

namespace TestModeSkeleton;

public class IOCheckMain : TestModeBase
{
	[SerializeField]
	[Tooltip("シーン固有オブジェクト")]
	private GameObject[] uniqueObjects;

	protected override string TitleName => "<l>I/O CHECK</l>";

	private void Start()
	{
		help.Set(TestModeHelp.HelpType.SelectExecute);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		if (SystemConfigGameData.CabinetType == CabinetType.B || SystemConfigGameData.CabinetType == CabinetType.C)
		{
			menuElements.FindAll((TestModeMenuElement e) => e.Event == "LampCheck").ForEach(delegate(TestModeMenuElement e)
			{
				e.State = TestModeMenuElement.TextState.NotActive;
			});
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
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
	}
}

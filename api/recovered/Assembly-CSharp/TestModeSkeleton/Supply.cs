using System.Collections;
using KAMUNITY;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class Supply : VirtualCoinBase
{
	[SerializeField]
	[Tooltip("EXTRA PASELI")]
	private Text extraPaseliText;

	private readonly string slotPath = "/opt/eacoin/slot1/player";

	private TestModeValue extraPaseli;

	protected override string SubTitleName => "<l>SUPPLY</l>";

	private void Start()
	{
		extraPaseli = new TestModeValue(0, AvsEa3Coin.Ea3EacoinQuerySupplyLimit(), 0, 1, isLoop: false, TestModeValue.TextColorType.OnlyYellow);
		extraPaseli.Set(AvsEa3Coin.Ea3EacoinQueryExtra(slotPath), extraPaseliText);
		StartOrResumeMainCoroutine();
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
	}

	protected override IEnumerator CoMainSaveExit()
	{
		notice.StartSetExtraPaseli(extraPaseli.Current, delegate
		{
			OnSave();
		});
		yield return new WaitForSeconds(TestModeBase.MessageWaitingSec);
		notice.Stop();
	}

	protected override IEnumerator CoMainExit()
	{
		if (IsModification())
		{
			ArrowkeyUp.SetActive(value: false);
			ArrowkeyDown.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			notice.StartSetOrNot(delegate
			{
				help.Set(TestModeHelp.HelpType.YesOrNo);
			});
			yield return new WaitUntil(() => notice.DecidedAnswer != TestModeNotice.Answer.NoAnswer);
			if (notice.DecidedAnswer == TestModeNotice.Answer.Yes)
			{
				yield return CoMainSaveExit();
				yield break;
			}
			notice.StartNoModification();
			yield return new WaitForSeconds(TestModeBase.MessageWaitingSec);
			notice.Stop();
		}
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		if (!(element.Event != "ExtraPaseli"))
		{
			extraPaseli.Delta = TestModeUtility.GetDeltaInButtonHolding(new TestModeInput.Key[2]
			{
				TestModeInput.Key.Right,
				TestModeInput.Key.Left
			});
			if (SingletonBehaviour<TestModeInput>.Instance.IsAddValue() || SingletonBehaviour<TestModeInput>.Instance.IsSubValue())
			{
				extraPaseli.Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
			}
		}
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		help.Set((element.Event == "ExtraPaseli") ? TestModeHelp.HelpType.SelectChange : TestModeHelp.HelpType.SelectExecute);
		if (element.Event == "ExtraPaseli")
		{
			ArrowkeyEnter.SetActive(value: false);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
		}
		else
		{
			ArrowkeyEnter.SetActive(value: true);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		}
	}

	protected override void OnFactorySettings(TestModeMenuElement element)
	{
		extraPaseli.ToDefault();
	}

	protected override void OnSave()
	{
		AvsEa3Coin.Ea3EacoinSupplyExtra(slotPath, extraPaseli.Current);
	}

	protected override bool IsModification()
	{
		return extraPaseli.IsModification;
	}
}

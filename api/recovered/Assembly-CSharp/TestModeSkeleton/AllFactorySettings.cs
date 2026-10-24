using System.Collections;
using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

namespace TestModeSkeleton;

public class AllFactorySettings : TestModeBase
{
	private enum NotClearedTextType
	{
		Other,
		HG
	}

	[SerializeField]
	private List<GameObject> m_notClearedText;

	[SerializeField]
	[Tooltip("隠しコマンド")]
	private TestModeSecretCommand m_command;

	protected override string TitleName => "<l>ALL FACTORY SETTINGS</l>";

	private IEnumerator Start()
	{
		bool isSuperFactory = false;
		GameObject notClearedText;
		if (SystemConfigGameData.CabinetType == CabinetType.HG || SystemConfigGameData.CabinetType == CabinetType.UKS || SystemConfigGameData.CabinetType == CabinetType.TBS || SystemConfigGameData.CabinetType == CabinetType.UJK)
		{
			m_notClearedText[0].SetActive(value: false);
			m_notClearedText[1].SetActive(value: true);
			notClearedText = m_notClearedText[1];
		}
		else
		{
			m_notClearedText[0].SetActive(value: true);
			m_notClearedText[1].SetActive(value: false);
			notClearedText = m_notClearedText[0];
		}
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: true);
		ArrowkeyRight.SetActive(value: true);
		notice.StartAllFactorySettingsOrNot(delegate
		{
			help.Set(TestModeHelp.HelpType.YesOrNo);
		});
		while (true)
		{
			if (m_command.Success)
			{
				isSuperFactory = !isSuperFactory;
				SetSubTitle(isSuperFactory ? "<color=red>ALL CLEAR FOR FACTORY</color>" : "");
				notClearedText.SetActive(!isSuperFactory);
				m_command.Restart();
			}
			if (notice.DecidedAnswer != 0)
			{
				break;
			}
			yield return null;
		}
		if (notice.DecidedAnswer == TestModeNotice.Answer.No)
		{
			notice.StartNoModification();
			yield return new WaitForSeconds(TestModeBase.MessageWaitingSec);
			Exit();
			yield break;
		}
		notice.StartSure();
		yield return new WaitUntil(() => notice.DecidedAnswer != TestModeNotice.Answer.NoAnswer);
		if (notice.DecidedAnswer == TestModeNotice.Answer.No)
		{
			notice.StartNoModification();
			yield return new WaitForSeconds(TestModeBase.MessageWaitingSec);
			Exit();
			yield break;
		}
		float start = Time.time;
		notice.StartNowSaving();
		yield return null;
		if (isSuperFactory)
		{
			CabinetManager.SuperAllFactoryAndSave();
		}
		else
		{
			CabinetManager.AllFactoryAndSave();
		}
		yield return new WaitUntil(() => Time.time - start >= TestModeBase.MessageWaitingSec);
		if (isSuperFactory)
		{
			notice.StartInitializingAndReboot();
			yield break;
		}
		notice.StartCheckAllItems(delegate
		{
			help.Set(TestModeHelp.HelpType.Exit);
		});
		yield return new WaitUntil(() => SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute());
		Exit();
	}

	protected override IEnumerator vOnBeforeUnload()
	{
		notice.Stop();
		yield break;
	}
}

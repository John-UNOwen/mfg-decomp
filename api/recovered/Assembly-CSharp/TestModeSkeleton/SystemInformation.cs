using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KAM.Usefully;
using KAMUNITY;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class SystemInformation : TestModeBase
{
	[SerializeField]
	[Tooltip("SOFT ID CODE")]
	private Text softIDCodeText;

	[SerializeField]
	[Tooltip("BOOTSTRAP VERSION")]
	private Text bootstrapVersionText;

	[SerializeField]
	[Tooltip("AVS VERSION")]
	private Text avsVersionText;

	[SerializeField]
	[Tooltip("SYSTEM ID")]
	private Text systemIDText;

	[SerializeField]
	[Tooltip("HARDWARE ID")]
	private Text hardwareIDText;

	[SerializeField]
	[Tooltip("ACCOUNT KEY")]
	private Text accountKeyText;

	[SerializeField]
	[Tooltip("LICENSE KEY")]
	private Text licenseKeyText;

	[SerializeField]
	[Tooltip("GAME VERSION")]
	private Text gameVersion;

	[SerializeField]
	[Tooltip("IO FIRM VERSION")]
	private Text ioFirmVersion;

	[SerializeField]
	[Tooltip("OS ACTIVATION")]
	private Text osActivation;

	private bool isShowActivationDate;

	private static float MaxWaitTIme = 3f;

	protected override string TitleName => "<l>SYSTEM INFORMATION</l>";

	private IEnumerator Start()
	{
		help.Set(TestModeHelp.HelpType.Exit);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		SetText();
		StartCoroutine(SecretComand());
		yield return new WaitUntil(() => SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute());
		Exit();
	}

	private void SetText()
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			softIDCodeText.text = Avs.GetSoftIdCode();
			bootstrapVersionText.text = Avs.StdGetEnv("/env/boot/version");
			avsVersionText.text = Avs.GetAvsVersion();
			systemIDText.text = HyphenInsertedString(Avs.GetSystemId());
			hardwareIDText.text = HyphenInsertedString(Avs.GetHardwareId());
			accountKeyText.text = HyphenInsertedString(Avs.GetAccountId());
			licenseKeyText.text = HyphenInsertedString(Avs.GetLicenseId());
			osActivation.text = SystemUtility.GenerateOSActivationDateString();
		}
		osActivation.transform.parent.gameObject.SetActive(value: false);
		gameVersion.text = GetGitCommitInfoString();
		ioFirmVersion.text = Bi2xInput.Instance.GetFirmVersion();
	}

	private string HyphenInsertedString(string str, int count = 4)
	{
		if (str == null)
		{
			return "";
		}
		IEnumerable<string> values = from Match m in Regex.Matches(str, $".{{1,{count}}}")
			select m.Value;
		return string.Join("-", values);
	}

	private string GetGitCommitInfoString()
	{
		GitVersionInfo gitVersionInfo = GitVersionInfo.Load();
		if (gitVersionInfo == null)
		{
			return "";
		}
		return gitVersionInfo.CommitTime + "." + gitVersionInfo.Hash;
	}

	private IEnumerator SecretComand()
	{
		float pushTime = 0f;
		while (!isShowActivationDate && pushTime < MaxWaitTIme)
		{
			pushTime = ((!SingletonBehaviour<TestModeInput>.Instance.IsPress(TestModeInput.Key.Test)) ? 0f : (pushTime + Time.deltaTime));
			yield return null;
		}
		osActivation.transform.parent.gameObject.SetActive(value: true);
	}
}

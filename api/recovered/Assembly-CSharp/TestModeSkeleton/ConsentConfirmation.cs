using System.Collections;
using KAMUNITY;
using TMPro;
using TestModeSkeleton.CabinetDef;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class ConsentConfirmation : TestModeBase
{
	private enum Status
	{
		Checking,
		ToTestMode
	}

	[SerializeField]
	[Tooltip("エラー画面表示サイズ")]
	private RectTransform messageFieldSize;

	[SerializeField]
	[Tooltip("エラー画面フレームパーツ")]
	private Image messageFrameBar;

	[SerializeField]
	[Tooltip("エラータイトル")]
	private TextMeshProUGUI errorTitleText;

	[SerializeField]
	[Tooltip("エラーメッセージ")]
	private TextMeshProUGUI errorMessage;

	[SerializeField]
	[Tooltip("エラーメッセージ1")]
	private TextMeshProUGUI errorMessage1;

	[SerializeField]
	[Tooltip("エラーメッセージ2")]
	private TextMeshProUGUI errorMessage2;

	private IEnumerator Start()
	{
		ArrowkeyGameObject.SetActive(value: false);
		GameObject obj = GameObject.Find("TestModeMain/Canvas/Menu");
		GameObject gameObject = GameObject.Find("TestModeMain/Canvas/Title");
		obj?.SetActive(value: false);
		gameObject?.SetActive(value: false);
		Vector2 sizeDelta = messageFieldSize.sizeDelta;
		sizeDelta.x = ((CabinetManager.Lang.Data.TestModeLanguage == LanguageOptionsDef.TestMode.Japanese) ? 830 : 870);
		messageFieldSize.sizeDelta = sizeDelta;
		help.Set(TestModeHelp.HelpType.None);
		Status status = Status.Checking;
		yield return CoCheck(delegate(Status s)
		{
			status = s;
		});
		yield return null;
		if (status == Status.Checking)
		{
			errorMessage2.gameObject.SetActive(value: false);
			yield return CoCheck(delegate(Status s)
			{
				status = s;
			});
			if (status == Status.Checking)
			{
				EagCore.Ea3GetStatus(out var code, out var _);
				if (code != 2012)
				{
					AvsEa3.Ea3PcbtrackerReportLicenseAgree();
				}
				yield return new WaitForSeconds(1f);
			}
		}
		if (status == Status.ToTestMode)
		{
			GameObject obj2 = GameObject.Find("TestModeMain/Canvas/Menu");
			gameObject = GameObject.Find("TestModeMain/Canvas/Title");
			obj2?.SetActive(value: true);
			gameObject?.SetActive(value: true);
			ArrowkeyGameObject.SetActive(value: true);
		}
		Exit();
	}

	private IEnumerator CoCheck(UnityAction<Status> action)
	{
		while (!SingletonBehaviour<TestModeInput>.Instance.IsServiceStartOperation())
		{
			if (SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Test))
			{
				action(Status.ToTestMode);
				break;
			}
			yield return null;
		}
	}
}

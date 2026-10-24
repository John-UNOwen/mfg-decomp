using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using TMPro;
using TestModeSkeleton.CabinetDef;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class ErrorMode : TestModeBase
{
	private enum Status
	{
		Checking,
		Ok,
		Bad
	}

	[SerializeField]
	[Tooltip("エラー画面表示サイズ")]
	private RectTransform messageFieldSize;

	[SerializeField]
	[Tooltip("エラー画面フレーム")]
	private Image messageFrame;

	[SerializeField]
	[Tooltip("エラー画面フレームパーツ")]
	private Image messageFrameBar;

	[SerializeField]
	[Tooltip("エラータイトル")]
	private TextMeshProUGUI errorTitleText;

	[SerializeField]
	[Tooltip("エラーコード")]
	private TextMeshProUGUI errorCode;

	[SerializeField]
	[Tooltip("エラーメッセージ")]
	private TextMeshProUGUI errorMessage;

	private IEnumerator Start()
	{
		ArrowkeyGameObject.SetActive(value: false);
		GameObject obj = GameObject.Find("TestModeMain/Canvas/Menu");
		GameObject gameObject = GameObject.Find("TestModeMain/Canvas/Title");
		obj?.SetActive(value: false);
		gameObject?.SetActive(value: false);
		if (!string.IsNullOrEmpty(GameDataManager.Account.AppSessionID))
		{
			yield return UniTask.ToCoroutine(async delegate
			{
				try
				{
					await RequestApiBase<Logout, Logout.ResultData>.Send(enableErrorScreenTransition: false);
				}
				catch (Exception arg)
				{
					Debug.Log($"ErrorMode Logout exception:{arg}");
				}
			});
			GameDataManager.Reset();
		}
		GameObject.Find("ErrorMode/Canvas/Credits").GetComponent<Text>();
		Vector2 sizeDelta = messageFieldSize.sizeDelta;
		sizeDelta.x = ((CabinetManager.Lang.Data.TestModeLanguage == LanguageOptionsDef.TestMode.Japanese) ? 830 : 870);
		messageFieldSize.sizeDelta = sizeDelta;
		Status status = Status.Checking;
		yield return CoCheck(delegate(Status s)
		{
			status = s;
		});
		if (status == Status.Checking)
		{
			yield return CoAbort();
		}
		GameObject obj2 = GameObject.Find("TestModeMain/Canvas/Menu");
		gameObject = GameObject.Find("TestModeMain/Canvas/Title");
		obj2?.SetActive(value: true);
		gameObject?.SetActive(value: true);
		ArrowkeyGameObject.SetActive(value: true);
		Exit();
	}

	private IEnumerator CoCheck(UnityAction<Status> action)
	{
		help.Set(TestModeHelp.HelpType.ToTestMode);
		while (!SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Test))
		{
			yield return null;
		}
	}

	private IEnumerator CoAbort()
	{
		yield return null;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using TestModeSkeleton.Price;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class TestModeNotice : MonoBehaviour
{
	public enum Answer
	{
		NoAnswer,
		No,
		Yes
	}

	private enum Type
	{
		None,
		Selectable,
		OnlyMessage,
		Waiting
	}

	[SerializeField]
	[Tooltip("本体")]
	private GameObject bodyObject;

	[SerializeField]
	[Tooltip("メッセージ部分")]
	private Text messageText;

	[SerializeField]
	[Tooltip("'Yes / No'部分")]
	private Text answerText;

	private Answer cursor = Answer.No;

	private Type type;

	private string setMessage = "";

	public Answer DecidedAnswer { get; private set; }

	private void Update()
	{
		if (type == Type.Selectable)
		{
			if (SingletonBehaviour<TestModeInput>.Instance.IsYesOrNo())
			{
				switch (cursor)
				{
				case Answer.No:
					cursor = Answer.Yes;
					break;
				case Answer.Yes:
					cursor = Answer.No;
					break;
				}
				SetColor(cursor);
			}
			else if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
			{
				DecidedAnswer = cursor;
				type = Type.None;
			}
		}
		else if (type == Type.Waiting)
		{
			IEnumerable<string> source = from n in Enumerable.Range(0, 5)
				select new string('.', n) + new string(' ', 5 - n - 1);
			int index = (int)(Time.time * 3f) % source.Count();
			messageText.text = setMessage + source.ElementAtOrDefault(index);
		}
	}

	public void StartSaveOrNot(UnityAction action = null)
	{
		Set(Type.Selectable, "<l>YOU HAVE NOT SAVED YET. SAVE CHANGES NOW?</l>", Color.white, action);
	}

	public void StartSetOrNot(UnityAction action = null)
	{
		Set(Type.Selectable, "<l>YOU HAVE NOT SET YET. SET CHANGES NOW?</l>", Color.white, action);
	}

	public void StartAllFactorySettingsOrNot(UnityAction action = null)
	{
		Set(Type.Selectable, "<l>DO YOU WANT ALL FACTORY SETTINGS?</l>", Color.white, action);
	}

	public void StartDataClearOrNot(UnityAction action = null)
	{
		Set(Type.Selectable, "<l>DO YOU WANT TO CLEAR ALL DATA?</l>", Color.white, action);
	}

	public void StartSure()
	{
		Set(Type.Selectable, "<l>SURE?</l>", Color.white);
	}

	public void StartNoModification()
	{
		Set(Type.OnlyMessage, "<l>NO MODIFICATION</l>", Color.white);
	}

	public void StartNotCleared()
	{
		Set(Type.OnlyMessage, "<l>NOT CLEARED</l>", Color.white);
	}

	public void StartNotSet()
	{
		Set(Type.OnlyMessage, "<l>NOT SET</l>", Color.white);
	}

	public void StartTouchPanel()
	{
		Set(Type.OnlyMessage, "<l>TOUCH THE PANEL TO CHECK</l>", Color.white);
	}

	public void StartCalibration()
	{
		Set(Type.OnlyMessage, "<l>CARIBLATION TO CHECK</l>", Color.white);
	}

	public void StartEAmusementPassOn()
	{
		Set(Type.OnlyMessage, "e-amusement passを読み取り部分にあててください", Color.white);
	}

	public void StartEAmusementPassAndQROn()
	{
		Set(Type.OnlyMessage, "e-amusement passを読み取り部分にあてるか、画面の2次元コードを読み取ってください", Color.white);
	}

	public void StartConfirmed()
	{
		Set(Type.OnlyMessage, "確認しました", Color.white);
	}

	public void StartInputPassWordNumber()
	{
		Set(Type.OnlyMessage, "暗証番号を入力してください", Color.white);
	}

	public void StartErrorAuthAccount()
	{
		Set(Type.OnlyMessage, "暗証番号が違います", Color.white);
	}

	public void StartInputPassWord()
	{
		Set(Type.OnlyMessage, "パスワードを入力してください", Color.white);
	}

	public void StartChangingPassWord()
	{
		Set(Type.OnlyMessage, "パスワードを変更します", Color.white);
	}

	public void StartWrongConfirmationPassWord()
	{
		Set(Type.OnlyMessage, "パスワードの確認入力が正しくありません", Color.white);
	}

	public void StartNotChangePassWord()
	{
		Set(Type.OnlyMessage, "パスワードを変更しません", Color.white);
	}

	public void StartChangedPassWord()
	{
		Set(Type.OnlyMessage, "パスワードを変更しました", Color.white);
	}

	public void StartWrongPassWord()
	{
		Set(Type.OnlyMessage, "パスワードが違います", Color.white);
	}

	public void StartUnExpectedPassWordError()
	{
		Set(Type.OnlyMessage, "パスワードの変更に失敗した可能性があります", Color.white);
	}

	public void StartPaseliNotAvailable()
	{
		Set(Type.OnlyMessage, "PASELIを利用できません", Color.white);
	}

	public void StartPaseliMaintenance()
	{
		Set(Type.OnlyMessage, "PASELIを利用できません\nただいまサーバーメンテナンス中です", Color.white);
	}

	public void StartPaseliUnExpectedError()
	{
		Set(Type.OnlyMessage, "PASELIの情報を取得できませんでした", Color.white);
	}

	public void StartPaseliErrorTryAgain()
	{
		Set(Type.OnlyMessage, "只今集計中のため情報を取得できません\nしばらくたってから再度試してください", Color.white);
	}

	public void StartEAmusementPassNoData()
	{
		Set(Type.OnlyMessage, "このe-amusement passにはデータが登録されていません。", Color.white);
	}

	public void StartEAmusementPassNetworkError()
	{
		Set(Type.OnlyMessage, "ただいま回線が混雑しています。再度お試しください。", Color.white);
	}

	public void StartEAmusementPassNoID()
	{
		Set(Type.OnlyMessage, "e-amusement pass を確認できません。", Color.white);
	}

	public void StartCalibrationComplete()
	{
		Set(Type.OnlyMessage, "<l>CARIBLATION COMPLETE</l>", Color.red);
	}

	public void StartAbort()
	{
		Set(Type.OnlyMessage, "<l>ABORTED</l>", Color.red);
	}

	public void StartNowErasing()
	{
		Set(Type.OnlyMessage, "<l>NOW ERASING</l>", Color.red);
	}

	public void StartNowSaving()
	{
		Set(Type.OnlyMessage, "<l>NOW SAVING</l>", Color.red);
	}

	public void StartCheckAllItems(UnityAction action = null)
	{
		Set(Type.OnlyMessage, "<l>CHECK ALL ITEMS BEFORE OPERATION.</l>", Color.red, action);
	}

	public void StartFirstSetClock(UnityAction action = null)
	{
		Set(Type.OnlyMessage, "<l>FIRST, SET THE CLOCK. \"CLOCK\" ITEM IS IN MAIN MENU.</l>", Color.red, action);
	}

	public void StartSetExtraPaseli(int extraPaseli, UnityAction action = null)
	{
		Set(Type.OnlyMessage, $"<l>SET EXTRA PASELI 1</l>{extraPaseli}<l>SET EXTRA PASELI 2</l>", Color.red, action);
	}

	public void StartPaymentWarning(UnityAction action = null)
	{
		Set(Type.OnlyMessage, $"{PriceUtility.PayablePrice}円を超える価格設定があります", Color.red, action);
	}

	public void StartInitializingAndReboot()
	{
		Set(Type.OnlyMessage, "<l>SUPER FACTORY REBOOT</l>", Color.red);
	}

	public void StartConfirming()
	{
		Set(Type.Waiting, "確認しています", Color.white);
	}

	public void StartWaiting()
	{
		Set(Type.Waiting, "<l>PLEASE WAIT A MOMENT</l>", Color.white);
	}

	public void Stop(UnityAction action = null)
	{
		Set(Type.None, "", Color.white, action);
	}

	private void Set(Type type, string message, Color color, UnityAction action = null)
	{
		this.type = type;
		bodyObject.SetActive(type != Type.None);
		messageText.SetLocalizedText(message);
		messageText.color = color;
		setMessage = messageText.text;
		answerText.gameObject.SetActive(type == Type.Selectable);
		DecidedAnswer = Answer.NoAnswer;
		cursor = Answer.No;
		SetColor(cursor);
		action?.Invoke();
	}

	private void SetColor(Answer answer)
	{
		Func<Answer, string> func = (Answer c) => (c != answer) ? "white" : "red";
		answerText.SetLocalizedText("<color=" + func(Answer.Yes) + "><l>YES</l></color>   /   <color=" + func(Answer.No) + "><l>NO</l></color>");
	}
}

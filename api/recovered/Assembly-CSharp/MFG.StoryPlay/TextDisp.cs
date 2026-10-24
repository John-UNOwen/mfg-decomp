using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class TextDisp : Command
{
	public string name;

	public string text;

	public int size;

	public int textLength;

	public Color color;

	public string costumeName;

	public GameObject Obj2D;

	public string TextStartAnimationTrigger;

	public string TextEndAnimationTrigger;

	private ItemID _costumeItemID;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.TextDisp;
		sec = Utility.ParseSec(values[6], 0f);
		name = values[2];
		ItemID costumeItemID = Utility.GetCostumeItemID(name, "");
		if (costumeItemID != 0)
		{
			CharaType charaType = CostumeItemMaster.GetItem(costumeItemID).CharaType;
			name = MasterGameDataManager.Character.Properties(charaType).Profile.Name;
		}
		if (values[4] == "")
		{
			size = 40;
		}
		else
		{
			size = int.Parse(values[4]);
		}
		switch (values[3])
		{
		case "":
			color = Color.white;
			break;
		case "red":
			color = Color.red;
			break;
		case "blue":
			color = Color.blue;
			break;
		case "yellow":
			color = Color.yellow;
			break;
		case "green":
			color = Color.green;
			break;
		case "black":
			color = Color.black;
			break;
		case "white":
			color = Color.white;
			break;
		default:
			try
			{
				string[] array = values[3].Split(" ");
				color.r = float.Parse(array[0]);
				color.g = float.Parse(array[1]);
				color.b = float.Parse(array[2]);
				color.a = float.Parse(array[3]);
			}
			catch
			{
				Debug.Log("TextDisp：「D.対象名」の値が不正です。whiteで設定します。「D.対象名」の値=" + values[3]);
				color = Color.white;
			}
			break;
		}
		textLength = values[5].Length;
		text = SaniTize.SanitizedString(values[5]);
		charaName = values[2];
		if (GameDataManager.Player.PlayerName != null)
		{
			Regex regex = new Regex("[\\*]{4}");
			text = regex.Replace(text, GameDataManager.Player.PlayerName);
			name = regex.Replace(name, GameDataManager.Player.PlayerName);
		}
		costumeName = values[28];
		if (!string.IsNullOrEmpty(costumeName) || !string.IsNullOrEmpty(charaName))
		{
			_costumeItemID = Utility.GetCostumeItemID(values[2], values[28]);
			if (_costumeItemID != 0)
			{
				Obj2D = StoryPlayFlow.GetInstance().GetChara2DObj(_costumeItemID);
			}
		}
		TextStartAnimationTrigger = values[15];
		TextEndAnimationTrigger = values[16];
	}

	public override async UniTask Exec()
	{
		TextMeshProUGUI textObj = StoryPlayFlow.GetInstance().textObj;
		TextMeshProUGUI nameTextObj = StoryPlayFlow.GetInstance().nameTextObj;
		GameObject cursor = StoryPlayFlow.GetInstance().cursor;
		textObj.text = text;
		textObj.maxVisibleCharacters = 0;
		textObj.fontSize = size;
		textObj.color = color;
		nameTextObj.text = name;
		StoryPlayFlow.GetInstance().textWindow.SetActive(value: true);
		StoryPlayFlow.GetInstance().textSetBg.GetComponent<Image>().DOFade(StoryPlayFlow.GetInstance().textSetBgAlpha, 0.5f).ToUniTask()
			.Forget();
		StoryPlayFlow.GetInstance().charaNameBg.GetComponent<Image>().DOFade(StoryPlayFlow.GetInstance().charaNameBgAlpha, 0.5f).ToUniTask()
			.Forget();
		StoryPlayFlow.GetInstance().charaNameBg.SetActive(value: true);
		StoryPlayFlow.GetInstance().CharaTalkStart(_costumeItemID);
		float waitTime = Mathf.Clamp(0.25f * (float)textLength, 3f, 5f);
		if (sec != 0f)
		{
			waitTime = sec;
		}
		try
		{
			if (Obj2D != null && !string.IsNullOrEmpty(TextStartAnimationTrigger))
			{
				Obj2D.GetComponent<StandingCharaController>().PlayMotion(TextStartAnimationTrigger);
			}
			await UniTask.WhenAny(UniTask.WaitUntil(() => textObj.maxVisibleCharacters >= textLength, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => IsSkip() && StoryPlayFlow.GetInstance().m_textSkipEnable, PlayerLoopTiming.Update, token));
			if (cursor != null)
			{
				cursor.SetActive(value: true);
			}
			textObj.maxVisibleCharacters = textObj.text.Length;
			bool isTextEndAnimationCalled = false;
			CallTextEndAnimationTrigger();
			if (StoryPlayFlow.GetInstance().IsAutoSkipDisable)
			{
				await UniTask.WaitUntil(() => IsSkip(), PlayerLoopTiming.Update, token);
			}
			else
			{
				await UniTask.WhenAny(UniTask.WhenAll(UniTask.WaitWhile(delegate
				{
					bool num = IsVoicePlaying();
					if (!num)
					{
						CallTextEndAnimationTrigger();
					}
					return num;
				}), UniTask.Delay(TimeSpan.FromSeconds(waitTime), ignoreTimeScale: false, PlayerLoopTiming.Update, token)), UniTask.WaitUntil(() => IsSkip(), PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => IsHighSpeed(), PlayerLoopTiming.Update, token));
				CallTextEndAnimationTrigger();
				await UniTask.Delay(TimeSpan.FromSeconds(0.30000001192092896), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			}
			void CallTextEndAnimationTrigger()
			{
				if (!isTextEndAnimationCalled && Obj2D != null && !string.IsNullOrEmpty(TextEndAnimationTrigger) && !IsVoicePlaying())
				{
					Obj2D.GetComponent<StandingCharaController>().PlayMotion(TextEndAnimationTrigger);
					isTextEndAnimationCalled = true;
				}
			}
		}
		catch (OperationCanceledException)
		{
			return;
		}
		if (cursor != null)
		{
			cursor.SetActive(value: false);
		}
		textObj.text = null;
		nameTextObj.text = null;
		StoryPlayFlow.GetInstance().textSetBg.GetComponent<Image>().DOFade(0f, 0.5f).ToUniTask()
			.Forget();
		StoryPlayFlow.GetInstance().charaNameBg.GetComponent<Image>().DOFade(0f, 0.5f).ToUniTask()
			.Forget();
		static bool IsVoicePlaying()
		{
			return SoundManager.IsPlay(StoryPlayFlow.GetInstance().m_playingVoiceLabel);
		}
	}

	private bool IsSkip()
	{
		if (StoryPlayFlow.GetInstance().IsSkipWindowOpen)
		{
			return false;
		}
		Vector2 mousePosition = ResidentFlow.CustomInput.mousePosition;
		RectTransform component = StoryPlayFlow.GetInstance().allSkipButton.transform.Find("Bt").gameObject.GetComponent<RectTransform>();
		Vector3 position = component.position;
		float num = position.x - component.sizeDelta.x / 2f;
		float num2 = position.x + component.sizeDelta.x / 2f;
		float num3 = position.y + component.sizeDelta.y / 2f;
		float num4 = position.y - component.sizeDelta.y / 2f;
		if (mousePosition.x < num || mousePosition.x > num2 || mousePosition.y > num3 || mousePosition.y < num4)
		{
			return ResidentFlow.CustomInput.GetMouseButtonDown(0);
		}
		return false;
	}

	private bool IsHighSpeed()
	{
		if (StoryPlayFlow.GetInstance()._isHiSpeed)
		{
			return true;
		}
		return false;
	}
}

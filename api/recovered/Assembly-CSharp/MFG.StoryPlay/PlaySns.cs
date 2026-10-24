using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class PlaySns : Command
{
	private string sendText;

	private string nickName;

	private string image;

	private string snsCommand;

	private bool isMe;

	private bool isTweet;

	private string viewIndex;

	private float waitSec;

	private string[] reaction;

	private string tweetImage;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.PlaySns;
		nickName = values[2];
		sendText = values[5];
		string[] array = values[3].Split('_');
		snsCommand = array[0];
		viewIndex = array[1];
		image = values[17];
		isMe = values[18] == "○";
		isTweet = values[18] == "Tweet";
		waitSec = Utility.ParseSec(values[19]);
		reaction = ((!(values[20] == "")) ? values[20].Split('_') : new string[3] { "0", "0", "0" });
		tweetImage = values[21];
	}

	public override async UniTask Exec()
	{
		string text = (isTweet ? "Twitter" : "SNS");
		string targetObjName = text + "View" + viewIndex + "(Clone)";
		Transform view = StoryPlayFlow.GetInstance().animeTop.transform.Find(targetObjName);
		if (view == null)
		{
			GameObject gameObject = await StoryPlayFlow.GetInstance().InstantiateAnime("Story/" + text + "View.prefab");
			if (gameObject != null)
			{
				gameObject.name = targetObjName;
				view = gameObject.transform;
				foreach (Transform item in view.Find("ScrollView/Viewport/Content"))
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
				StoryPlayFlow.GetInstance().SnsWaitSec.Add(targetObjName, new List<float>());
			}
		}
		if ((bool)view)
		{
			if (snsCommand == "In")
			{
				view.gameObject.SetActive(value: true);
				Transform transform = view.Find("ScrollView/Viewport/Content");
				int i = 0;
				foreach (Transform item2 in transform)
				{
					BaseComments component = item2.GetComponent<BaseComments>();
					if (component != null)
					{
						component.isPlaySe = true;
					}
					item2.gameObject.SetActive(value: true);
					Reflesh(view.Find("ScrollView/Viewport/Content"));
					float num = ((StoryPlayFlow.GetInstance().SnsWaitSec[targetObjName] != null && i < StoryPlayFlow.GetInstance().SnsWaitSec[targetObjName].Count) ? StoryPlayFlow.GetInstance().SnsWaitSec[targetObjName][i] : 2f);
					await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(num), ignoreTimeScale: false, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => IsSkip(), PlayerLoopTiming.Update, token), UniTask.WhenAll(UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => IsHighSpeed(), PlayerLoopTiming.Update, token)));
					i++;
				}
				return;
			}
			if (snsCommand == "Out")
			{
				view.gameObject.SetActive(value: false);
				{
					foreach (Transform item3 in view.Find("ScrollView/Viewport/Content"))
					{
						UnityEngine.Object.Destroy(item3.gameObject);
					}
					return;
				}
			}
			if (snsCommand == "SetupChat")
			{
				await StoryPlayFlow.GetInstance().InstantiateComment(view.Find("ScrollView/Viewport/Content"), isMe, "Story/SNSIcons/" + image + ".png", nickName, sendText);
				StoryPlayFlow.GetInstance().SnsWaitSec[targetObjName].Add(waitSec);
			}
			else if (snsCommand == "SetupImage")
			{
				await StoryPlayFlow.GetInstance().InstantiateImage(view.Find("ScrollView/Viewport/Content"), isMe, "Story/SNSIcons/" + image + ".png", nickName, "Story/Images/" + sendText + ".png");
				StoryPlayFlow.GetInstance().SnsWaitSec[targetObjName].Add(waitSec);
			}
			else if (snsCommand == "SetupTweet")
			{
				await StoryPlayFlow.GetInstance().InstantiateTweet(view.Find("ScrollView/Viewport/Content"), "Story/SNSIcons/" + image + ".png", nickName, sendText, reaction, "Story/Images/" + tweetImage + ".png");
				StoryPlayFlow.GetInstance().SnsWaitSec[targetObjName].Add(waitSec);
			}
			else if (snsCommand == "Setup")
			{
				await StoryPlayFlow.GetInstance().SetupSNSBg(view.Find("Phone"), "Story/SNSBgs/" + image + ".png");
			}
			else if (snsCommand == "SetupFinish")
			{
				view.gameObject.SetActive(value: true);
				Reflesh(view.Find("ScrollView/Viewport/Content"));
				view.gameObject.SetActive(value: false);
			}
		}
		else
		{
			Debug.LogError("SNSViewが見つかりません");
		}
	}

	private void Reflesh(Transform trs)
	{
		trs.GetComponent<ContentSizeFitter>().SetLayoutVertical();
		trs.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
		trs.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
		trs.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
		LayoutRebuilder.ForceRebuildLayoutImmediate(trs.GetComponent<RectTransform>());
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

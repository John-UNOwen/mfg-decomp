using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomMainWindow : MonoBehaviour
{
	public enum Page
	{
		Customize1,
		Customize2,
		Record,
		Story,
		Friend
	}

	[Serializable]
	public class TabPage
	{
		public Page index;

		public GameObject tab;

		public GameObject page;

		public GameObject line;

		public GameObject updateIcon;

		public void SetActive(Page p)
		{
			bool flag = p == index;
			page.SetActive(flag);
			line.SetActive(!flag);
			tab.GetComponent<Image>().color = (flag ? new Color(1f, 1f, 1f) : new Color(0.7844f, 0.7844f, 0.7844f));
			tab.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(flag);
		}

		public void UpdateIconDisp(bool isDisp)
		{
			if (updateIcon != null)
			{
				updateIcon.SetActive(isDisp);
			}
		}
	}

	private MyRoomFlow myRoomFlow;

	[SerializeField]
	private List<TabPage> tabPageList;

	[SerializeField]
	private MyRoomDetailWindow pageAll;

	[SerializeField]
	private MyRoomFavoDisplay favoWindow;

	[SerializeField]
	private MyRoomFanDisplay fanWindow;

	[SerializeField]
	private MyRoomSpiritDisplay spiritWindow;

	[SerializeField]
	private MyRoomCutinDisplay cutinWindow;

	[SerializeField]
	private MyRoomMatchingBGDisplay matchingBGWindow;

	[SerializeField]
	private MyRoomCatchphraseDisplay carchphraseWindow;

	[SerializeField]
	private MyRoomCustomizeWindow customizeWindow;

	[SerializeField]
	private MyRoomRecordWindow recordWindow;

	[SerializeField]
	private MyRoomStoryWindow storyWindow;

	[SerializeField]
	private MyRoomFriendWindow friendWindow;

	[SerializeField]
	private GameObject popupParent;

	private Page OpenIndex;

	public void Init(MyRoomFlow mrf, CancellationToken cancellationToken)
	{
		myRoomFlow = mrf;
		foreach (TabPage tabPage in tabPageList)
		{
			if (tabPage.index == Page.Friend && !GameDataManager.IsOdekakeEnable())
			{
				tabPage.tab.SetActive(value: false);
				for (Page page = Page.Customize1; page <= Page.Story; page++)
				{
					tabPageList[(int)page].line.GetComponent<RectTransform>().sizeDelta = new Vector2(282.6f, 20f);
				}
				continue;
			}
			tabPage.tab.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
			{
				if (tabPage.index != OpenIndex)
				{
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
					ChangeTab(tabPage.index, cancellationToken);
				}
			})
				.AddTo(this);
		}
		pageAll.Init(myRoomFlow, cancellationToken);
		favoWindow.Init(mrf, pageAll, cancellationToken);
		fanWindow.Init(mrf, pageAll, cancellationToken);
		spiritWindow.Init(mrf, pageAll, cancellationToken);
		cutinWindow.Init(mrf, pageAll, cancellationToken);
		matchingBGWindow.Init(mrf, pageAll, cancellationToken, isProEnable: false);
		carchphraseWindow.Init(mrf, pageAll, cancellationToken);
		customizeWindow.Init(pageAll, cancellationToken);
		recordWindow.Init(popupParent);
		storyWindow.Init(mrf);
		friendWindow.Init(pageAll, cancellationToken).Forget();
		TabIconRefresh();
		ChangeTab(Page.Customize1, cancellationToken);
	}

	public void TabIconRefresh()
	{
		try
		{
			TabPage tabPage = tabPageList.Find((TabPage x) => x.index == Page.Friend);
			int isDisp;
			if (!GameDataManager.Player.IsPro())
			{
				List<FriendInfo> friendInfos = GameDataManager.Friend.FriendInfos;
				isDisp = ((friendInfos != null && friendInfos.Where((FriendInfo x) => x.IsUsable)?.Count() > 0) ? 1 : 0);
			}
			else
			{
				isDisp = 0;
			}
			tabPage.UpdateIconDisp((byte)isDisp != 0);
		}
		catch
		{
		}
	}

	public void Refresh(CancellationToken cancellationToken)
	{
		favoWindow.SetStatus(cancellationToken);
		fanWindow.SetStatus(cancellationToken);
		spiritWindow.SetStatus(cancellationToken);
		cutinWindow.SetStatus(cancellationToken);
		matchingBGWindow.SetStatus(cancellationToken);
		carchphraseWindow.SetStatus(cancellationToken);
		storyWindow.Refresh();
		customizeWindow.Init(pageAll, cancellationToken);
		TabIconRefresh();
	}

	public async UniTask CharaSelectWindowOpen(CancellationToken cancellationToken)
	{
		await pageAll.WindowOpen(MyRoomFlow.WindowType.CharaSelectWindow, cancellationToken);
	}

	public void StopPreview()
	{
		pageAll.StopPreview();
	}

	private void ChangeTab(Page newIndex, CancellationToken cancellationToken)
	{
		if (OpenIndex == Page.Customize2 && newIndex != Page.Customize2)
		{
			customizeWindow.ClosingTask();
		}
		for (int i = 0; i < tabPageList.Count(); i++)
		{
			tabPageList[i].SetActive(newIndex);
		}
		OpenIndex = newIndex;
		Refresh(cancellationToken);
	}
}

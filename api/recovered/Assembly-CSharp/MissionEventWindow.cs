using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Mission;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MissionEventWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private TextMeshProUGUI dateText;

	[SerializeField]
	private TextMeshProUGUI debugText;

	[SerializeField]
	private GameObject progress;

	[SerializeField]
	private GameObject jandriSupport;

	[SerializeField]
	private TextMeshProUGUI newbeeDate;

	[SerializeField]
	private TextMeshProUGUI comebackDate;

	[SerializeField]
	private TextMeshProUGUI itemCounter;

	[SerializeField]
	private Button detail1Button;

	[SerializeField]
	private Button detail2Button;

	[SerializeField]
	private Button detail1CloseButton;

	[SerializeField]
	private Button detail2CloseButton;

	[SerializeField]
	private GameObject detail1Window;

	[SerializeField]
	private GameObject detail2Window;

	public bool isOpen;

	public bool isOpen1;

	public bool isOpen2;

	public GameEventType eventType;

	private MFG.Types.Mission.Item Master;

	private CategoryItem categoryMaster => MissionCategoryMaster.GetItem(eventType);

	private async void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
		if (detail1Button != null && detail1Window != null)
		{
			detail1Button.OnClickAsObservable().Subscribe(delegate
			{
				Open1Window();
			}).AddTo(this);
		}
		if (detail2Button != null && detail2Window != null)
		{
			detail2Button.OnClickAsObservable().Subscribe(delegate
			{
				Open2Window();
			}).AddTo(this);
		}
		if (detail1CloseButton != null)
		{
			detail1CloseButton.OnClickAsObservable().Subscribe(delegate
			{
				Close1Window();
			}).AddTo(this);
		}
		if (detail2CloseButton != null)
		{
			detail2CloseButton.OnClickAsObservable().Subscribe(delegate
			{
				Close2Window();
			}).AddTo(this);
		}
	}

	public void OpenWindow()
	{
		if (isOpen || categoryMaster == null)
		{
			return;
		}
		if (categoryMaster.IsCharaMainMission)
		{
			Master = MissionMaster.GetItems(categoryMaster.Category).Find((MFG.Types.Mission.Item x) => x.Chara == GameDataManager.Player.SelectChara);
		}
		else
		{
			Master = MissionMaster.GetItem(categoryMaster.MainMissionID);
		}
		dateText.text = categoryMaster.EndDateTime.ToString("yyyy年MM月dd日（ddd）HH：mmまで");
		if (jandriSupport != null && categoryMaster.IsJandriEnable)
		{
			jandriSupport.SetActive(GameDataManager.IsLoadJandriAppear());
		}
		if (newbeeDate != null && comebackDate != null && categoryMaster.IsWelcomeComebackEvent)
		{
			try
			{
				ComebackCampaignInfo comebackCampaignInfo = GameDataManager.ReceiveOnly.PlayerInfo.ComebackCampaignInfos.Find((ComebackCampaignInfo x) => x.EventType == eventType);
				if (comebackCampaignInfo != null)
				{
					newbeeDate.text = DateTimeOffsetManager.GetJSTFromString(comebackCampaignInfo.NewbeeReferenceDate).ToString("yyyy年MM月dd日");
					comebackDate.text = DateTimeOffsetManager.GetJSTFromString(comebackCampaignInfo.ComebackReferenceDateStart).ToString("yyyy年MM月dd日") + " ～ " + DateTimeOffsetManager.GetJSTFromString(comebackCampaignInfo.ComebackReferenceDateEnd).ToString("yyyy年MM月dd日");
				}
			}
			catch
			{
			}
		}
		SetProgress(isForget: true).Forget();
		isOpen = true;
		base.gameObject.SetActive(value: true);
		PlayOpenSe();
		windowObj.GetComponent<Animator>().SetTrigger("In");
	}

	private void Open1Window()
	{
		if (detail1Window != null)
		{
			isOpen1 = true;
			DetailWindowInit();
			detail1Window.SetActive(value: true);
			PlayOpenSe();
			detail1Window.GetComponent<Animator>().SetTrigger("In");
		}
	}

	private void Open2Window()
	{
		if (detail2Window != null)
		{
			isOpen2 = true;
			detail2Window.SetActive(value: true);
			PlayOpenSe();
			detail2Window.GetComponent<Animator>().SetTrigger("In");
		}
	}

	public async UniTask CloseWindow()
	{
		if (isOpen)
		{
			Close1Window(isSePlay: false);
			Close2Window(isSePlay: false);
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			base.gameObject.SetActive(value: false);
			isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	private void Close1Window(bool isSePlay = true)
	{
		if (detail1Window != null && isOpen1)
		{
			if (isSePlay)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			detail1Window.GetComponent<Animator>().SetTrigger("Out");
			detail1Window.SetActive(value: false);
			isOpen1 = false;
		}
	}

	private void Close2Window(bool isSePlay = true)
	{
		if (detail2Window != null && isOpen2)
		{
			if (isSePlay)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			detail2Window.GetComponent<Animator>().SetTrigger("Out");
			detail2Window.SetActive(value: false);
			isOpen2 = false;
		}
	}

	public void PlayOpenSe()
	{
		if (eventType == GameEventType.GrimAroeGachaTicket)
		{
			SoundManager.PlaySe("SFX_SYS_INFO3").Forget();
		}
		else
		{
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
		}
	}

	public async UniTask SetProgress(bool isForget = false)
	{
		switch (eventType)
		{
		case GameEventType.NewYear2024:
			SetNewYear2024Progress();
			break;
		case GameEventType.Bemani001:
		case GameEventType.Toho001:
		case GameEventType.Bemani002:
		case GameEventType.Bemani003:
			if (isForget)
			{
				await MusicEventProgress();
			}
			break;
		case GameEventType.GrimAroeGachaTicket:
			if (itemCounter != null)
			{
				itemCounter.text = $"{GameDataManager.PlayerItem.Count(ItemID.ExchangePointGrimAroe)}";
			}
			break;
		case GameEventType.Valentine2025:
		{
			MissionProgressInfo missionProgressInfo2 = GameDataManager.Mission.m_missionProgressInfos?.Find((MissionProgressInfo x) => x.MissionID == MissionID.Valentine2025_Main);
			if (itemCounter != null && missionProgressInfo2 != null)
			{
				itemCounter.text = $"{Math.Min(missionProgressInfo2.Progress, missionProgressInfo2.Master.AchievementList().Last())}";
			}
			break;
		}
		case GameEventType.DiaTicket:
			if (itemCounter != null)
			{
				itemCounter.text = $"{GameDataManager.PlayerItem.Count(ItemID.ExchangePointDia)}";
			}
			break;
		case GameEventType.DoubrielTicket:
			if (itemCounter != null)
			{
				itemCounter.text = $"{GameDataManager.PlayerItem.Count(ItemID.ExchangePointDoubriel)}";
			}
			break;
		case GameEventType.IppatsuTicket:
			if (itemCounter != null)
			{
				itemCounter.text = $"{GameDataManager.PlayerItem.Count(ItemID.ExchangePointIppatsu)}";
			}
			break;
		case GameEventType.ShiroeTicket:
			if (itemCounter != null)
			{
				itemCounter.text = $"{GameDataManager.PlayerItem.Count(ItemID.ExchangePointShiroe)}";
			}
			break;
		case GameEventType.JunTicket:
			if (itemCounter != null)
			{
				itemCounter.text = $"{GameDataManager.PlayerItem.Count(ItemID.ExchangePointJun)}";
			}
			break;
		case GameEventType.ToukaTicket:
			if (itemCounter != null)
			{
				itemCounter.text = $"{GameDataManager.PlayerItem.Count(ItemID.ExchangePointTouka)}";
			}
			break;
		case GameEventType.Valentine2026:
		{
			MissionProgressInfo missionProgressInfo = GameDataManager.Mission.m_missionProgressInfos?.Find((MissionProgressInfo x) => x.MissionID == MissionID.Valentine2026_Main);
			if (itemCounter != null && missionProgressInfo != null)
			{
				itemCounter.text = $"{Math.Min(missionProgressInfo.Progress, missionProgressInfo.Master.AchievementList().Last())}";
			}
			break;
		}
		}
	}

	public void DetailWindowInit()
	{
		try
		{
			GameEventType gameEventType = eventType;
			if (gameEventType != GameEventType.Valentine2025 && gameEventType != GameEventType.Valentine2026)
			{
				return;
			}
			foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
			{
				int level = GameDataManager.Player.CharaData[(int)value].Level;
				string text = ((level >= 10000) ? level.ToString() : level.ToString("D4"));
				Transform transform = detail1Window.transform.Find($"Window/Contents1/Koukando/CharaNinki ({(int)value})/Waku/Koukando");
				if (transform != null)
				{
					transform.Find("Num").GetComponent<TextMeshProUGUI>().text = text;
					List<string> extentionItems = ItemUtility.ExtraItem("ValentineReachStick").ExtentionItems;
					transform.Find("ClearStamp").gameObject.SetActive(ItemUtility.IsPossession(extentionItems?[(int)value] ?? ""));
				}
			}
		}
		catch
		{
		}
	}

	private void SetNewYear2024Progress()
	{
	}

	private async UniTask MusicEventProgress()
	{
		List<MFG.Types.Mission.Item> items = MissionMaster.GetItems(categoryMaster.Category);
		if (items == null || items.Count <= 0)
		{
			return;
		}
		List<UniTask> list = new List<UniTask>();
		foreach (MFG.Types.Mission.Item item in items)
		{
			list.Add(progress.transform.Find("Chara" + ((int)(item.Chara + 1)).ToString("D2")).transform.GetComponent<MusicEventProgress>().Set(item));
		}
		await list;
	}

	private void Update()
	{
	}
}

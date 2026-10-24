using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using MFG.GameRequest;
using MFG.Types;
using MFG.Types.YellCampaign;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class YellGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "yell_campaign";

	[NonSerialized]
	public const CampaignID CurrentCampaign = CampaignID.KAC2023;

	[NonSerialized]
	public CONSUME_MODE BeforeConsumeMode;

	public List<YellCampaignInfo> YellCampaignInfos = new List<YellCampaignInfo>();

	public YellGameData()
	{
		if (GetCurrentCampaignInfo() == null)
		{
			YellCampaignInfos.Add(new YellCampaignInfo(CampaignID.KAC2023));
		}
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("YellGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("yell_campaign", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static YellGameData LoadFromJson(string jsonString)
	{
		Debug.Log("YellGameData Load : " + jsonString);
		YellGameData yellGameData = JsonUtility.FromJson<YellGameData>(jsonString);
		if (yellGameData == null)
		{
			yellGameData = new YellGameData();
		}
		yellGameData.Validate();
		return yellGameData;
	}

	private void Validate()
	{
	}

	public YellCampaignInfo GetCurrentCampaignInfo()
	{
		return YellCampaignInfos.Find((YellCampaignInfo x) => x.ID == CampaignID.KAC2023);
	}

	public async UniTask YellStack(CONSUME_MODE mode)
	{
		if (!IsEnable())
		{
			return;
		}
		YellCampaignInfo currentCampaignInfo = GetCurrentCampaignInfo();
		if (currentCampaignInfo != null)
		{
			currentCampaignInfo.YellStack(mode);
			if ((uint)(mode - 1) <= 3u)
			{
				BeforeConsumeMode = mode;
			}
			await Save();
		}
	}

	public bool IsCharacterEnable(CharaType chara)
	{
		return chara != CharaType.Chara10;
	}

	public bool IsEnable()
	{
		return false;
	}

	private bool IsYellCampaignEnable()
	{
		return GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.YellCampaign);
	}

	public void AddYellToChara()
	{
		if (IsEnable())
		{
			GetCurrentCampaignInfo()?.AddYell();
		}
	}

	public void RefreshYellRank()
	{
		if (IsEnable())
		{
			YellCampaignInfo currentCampaignInfo = GetCurrentCampaignInfo();
			if (currentCampaignInfo != null && currentCampaignInfo.GetCurrentCharaYellInfo() != null)
			{
				GameDataManager.Player.YellRank = currentCampaignInfo.GetCurrentCharaYellInfo().GetYellRank();
			}
		}
	}
}

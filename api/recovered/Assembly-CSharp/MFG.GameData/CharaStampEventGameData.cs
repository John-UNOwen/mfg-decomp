using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.CharaStamp;
using MFG.Types.FightGirlNews;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CharaStampEventGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "chara_stamp_event";

	public List<CharaStampInfo> CharaStampInfos = new List<CharaStampInfo>();

	public CharaStampEventGameData()
	{
		InfoUpdate();
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("CharaStampEventGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("chara_stamp_event", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static CharaStampEventGameData LoadFromJson(string jsonString)
	{
		Debug.Log("CharaStampEventGameData Load : " + jsonString);
		return JsonUtility.FromJson<CharaStampEventGameData>(jsonString);
	}

	public void InfoUpdate()
	{
		foreach (CharaStampID value in Enum.GetValues(typeof(CharaStampID)))
		{
			if (value != 0)
			{
				CharaStampInfo info = GetInfo(value);
				if (info == null)
				{
					CharaStampInfos.Add(new CharaStampInfo
					{
						ID = value,
						IsUsable = CharaStampMaster.IsFinishEvent(value)
					});
				}
				else if (CharaStampMaster.IsFinishEvent(value))
				{
					info.IsUsable = true;
					info.IsHomeDispFinish = true;
				}
			}
		}
		if ((CharaStampInfos?.Find((CharaStampInfo x) => x.ID == CharaStampID.Chaos)?.IsHomeDispFinish).GetValueOrDefault())
		{
			GameDataManager.PlayerConfig.FightGirlNewsFinishIDs.Add(NewsID.UnlockChaos);
		}
	}

	public CharaStampInfo GetCurrentEventInfo()
	{
		return GetInfo(CharaStampMaster.GetCurrentStampID());
	}

	public CharaStampInfo GetInfo(CharaStampID id)
	{
		return CharaStampInfos.Find((CharaStampInfo x) => x.ID == id);
	}

	public bool IsCharacterEnable(CharaType chara)
	{
		if (chara == CharaType.Chara11 && GameDataManager.IsChaosForceActive())
		{
			return true;
		}
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		CharaStamp charaStampFromCharaType = CharaStampMaster.GetCharaStampFromCharaType(chara);
		if (charaStampFromCharaType != null)
		{
			return GetInfo(charaStampFromCharaType.CharaStampID).IsUsable;
		}
		return false;
	}

	public bool IsHomeDisplay()
	{
		if (!IsEnable() || GetCurrentEventInfo() == null)
		{
			return false;
		}
		if (!IsCharacterEnable(GetCurrentEventInfo().Master.CharaType))
		{
			return true;
		}
		if (GetCurrentEventInfo().IsDuringForceDisplay())
		{
			return true;
		}
		return false;
	}

	public bool IsEnable()
	{
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		if (!CharaStampMaster.IsCurrentEventEnable())
		{
			return false;
		}
		return true;
	}
}

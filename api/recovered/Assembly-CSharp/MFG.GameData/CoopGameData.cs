using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Coop;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CoopGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "ea_shop";

	public List<CoopInfo> CoopInfos = new List<CoopInfo>();

	public List<RightStat> RightStats = new List<RightStat>();

	public bool IsJanDriEnable(bool isRealTime = false)
	{
		return RightStats.Find((RightStat x) => CoopMaster.GetCoopID(x.ObjectID) == CoopID.JanDri000)?.IsAvailable(isRealTime) ?? false;
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("CoopGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("ea_shop", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static CoopGameData LoadFromJson(string jsonString)
	{
		Debug.Log("CoopGameData Load : " + jsonString);
		CoopGameData coopGameData = JsonUtility.FromJson<CoopGameData>(jsonString);
		if (coopGameData == null)
		{
			coopGameData = new CoopGameData();
		}
		coopGameData.Validate();
		Debug.Log("coop game data count ; " + coopGameData.CoopInfos.Count);
		return coopGameData;
	}

	private void Validate()
	{
	}

	public bool IsCharacterSelectable(CharaType chara)
	{
		if (!CoopMaster.IsCoopCharacter(chara))
		{
			return true;
		}
		return GameDataManager.Player.PurchaseCharaList.Contains(chara);
	}

	public async UniTask GrantItem()
	{
		if (CoopInfos == null || CoopInfos.Count <= 0)
		{
			return;
		}
		List<int> grantIds = new List<int>();
		foreach (CoopInfo info in CoopInfos)
		{
			if (info == null)
			{
				continue;
			}
			Coop coop = CoopMaster.GetCoop(info.CoopID);
			if (coop != null && (coop.UnlockEventType == GameEventType.None || GameDataManager.System.GameControlInfo.EventStatus.IsActive(coop.UnlockEventType)))
			{
				switch (coop.Category)
				{
				case MFG.Types.Coop.Category.CutinItem:
					await GrantCutinItem(coop);
					grantIds.Add(info.PurchaseId);
					break;
				case MFG.Types.Coop.Category.NewCharacter:
					await GrantNewCharacter(coop);
					grantIds.Add(info.PurchaseId);
					break;
				}
			}
		}
		if (grantIds.Count > 0)
		{
			await RequestApiBase<CoopDone, CoopDone.ResultData, List<int>>.Send(grantIds);
		}
	}

	private async UniTask GrantNewCharacter(Coop coopMas)
	{
		if (!GameDataManager.Player.PurchaseCharaList.Contains(coopMas.CharaType))
		{
			GameDataManager.Player.PurchaseCharaList.Add(coopMas.CharaType);
			await GameDataManager.Player.Save();
			CharaEnabledSend.ResultData resultData = await RequestApiBase<CharaEnabledSend, CharaEnabledSend.ResultData, CharaType>.Send(coopMas.CharaType);
			if (resultData.IsGachaEnable)
			{
				GameDataManager.Notice.PickupGachaNoticeCharaType = coopMas.CharaType;
				GameDataManager.Notice.PickupGachaNoticeDate = resultData.EndDateString;
			}
		}
	}

	private async UniTask GrantCutinItem(Coop coopMas)
	{
		List<CutinItemInfo> cutinItems = GameDataManager.Customize.CutinItems;
		foreach (string boughtOID in coopMas.BoughtOIDs)
		{
			ItemID cutInItemId = CutinItemMaster.GetItemID(boughtOID);
			if (cutInItemId != 0 && !cutinItems.Exists((CutinItemInfo x) => x.ItemID == cutInItemId))
			{
				Debug.Log("Add CutIn : " + cutInItemId);
				cutinItems.Add(new CutinItemInfo
				{
					ItemID = cutInItemId,
					IsOn = true,
					IsNew = true
				});
			}
		}
		await GameDataManager.Customize.Save();
	}
}

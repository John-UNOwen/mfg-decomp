using System;
using System.Collections.Generic;
using GameContents;
using MFG.Types;
using MFG.Types.YellCampaign;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class YellCampaignInfo
{
	public CampaignID ID;

	public List<YellInfo> YellInfos = new List<YellInfo>();

	public bool IsFirstDisp = true;

	public int ThisPlayGetYell;

	public YellCampaignInfo(CampaignID _ID)
	{
		List<YellInfo> yellInfos = YellInfos;
		if (yellInfos != null && yellInfos.Count > 0)
		{
			return;
		}
		ID = _ID;
		foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
		{
			YellInfos.Add(new YellInfo
			{
				Chara = value
			});
		}
	}

	public YellInfo GetCurrentCharaYellInfo()
	{
		return YellInfos.Find((YellInfo x) => x.Chara == GameDataManager.Player.SelectChara);
	}

	public void AddYell()
	{
		YellInfo currentCharaYellInfo = GetCurrentCharaYellInfo();
		if (currentCharaYellInfo != null)
		{
			currentCharaYellInfo.AddYell(ThisPlayGetYell);
			ThisPlayGetYell = 0;
		}
	}

	public void YellStack(CONSUME_MODE mode)
	{
		Debug.Log("YellStack before : " + ThisPlayGetYell);
		switch (mode)
		{
		default:
			return;
		case CONSUME_MODE.START_CREDIT:
		case CONSUME_MODE.CONTINUE_CREDIT:
		case CONSUME_MODE.GACHA_CREDIT:
			ThisPlayGetYell += 10;
			break;
		case CONSUME_MODE.START_PASELI:
		case CONSUME_MODE.CONTINUE_PASELI:
		case CONSUME_MODE.GACHA_PASELI:
			ThisPlayGetYell += 12;
			break;
		case CONSUME_MODE.GACHA5_CREDIT:
			ThisPlayGetYell += 50;
			break;
		case CONSUME_MODE.GACHA5_PASELI:
			ThisPlayGetYell += 60;
			break;
		}
		Debug.Log("YellStack after : " + ThisPlayGetYell);
	}
}

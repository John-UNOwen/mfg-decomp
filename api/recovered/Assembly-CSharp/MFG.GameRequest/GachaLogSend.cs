using System;
using System.Collections.Generic;
using System.Text;
using MFG.Types;
using UnityEngine;

namespace MFG.GameRequest;

public class GachaLogSend : RequestApiBase<GachaLogSend, GachaLogSend.ResultData, (int seriesID, string gachaType, List<int> itemIDs, List<string> itemOIDs, List<CharaType> chara_selectables, UnableCharaGachaSelectWindow.State unable_chara_gacha_state, int gift_count, int exchange_point)>
{
	public class ResultData : ResultBase
	{
	}

	public class GachaLog
	{
		public int SeriesID;

		public string GachaType;

		public List<int> ItemIDs;

		public List<string> ItemOIDs;

		public List<int> CharaSelectables;

		public string UnableCharaGachaState;

		public int AcquiredGiftCount;

		public int AcquiredExchangePoint;

		public string GetJson()
		{
			return JsonUtility.ToJson(this);
		}
	}

	protected override void OnCreateSendParameter()
	{
		GachaLog gachaLog = new GachaLog
		{
			SeriesID = base.SendParameter.seriesID,
			GachaType = base.SendParameter.gachaType,
			ItemIDs = base.SendParameter.itemIDs,
			ItemOIDs = base.SendParameter.itemOIDs,
			CharaSelectables = base.SendParameter.chara_selectables.ConvertAll((CharaType x) => (int)x),
			UnableCharaGachaState = base.SendParameter.unable_chara_gacha_state.ToString(),
			AcquiredGiftCount = base.SendParameter.gift_count,
			AcquiredExchangePoint = base.SendParameter.Rest.Item1
		};
		Debug.Log(gachaLog.GetJson());
		string text = Uri.EscapeDataString(Convert.ToBase64String(Encoding.UTF8.GetBytes(gachaLog.GetJson())));
		m_sendUrl = base.DefaultFrontServerUrl + "/gacha_log";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&log={text}";
	}
}

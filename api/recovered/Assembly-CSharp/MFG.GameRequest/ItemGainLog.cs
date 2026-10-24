using System;
using System.Text;
using MFG.Types;
using UnityEngine;

namespace MFG.GameRequest;

public class ItemGainLog : RequestApiBase<ItemGainLog, ItemGainLog.ResultData, (string oid, ItemGainType gainType, int consumeNum)>
{
	public class ResultData : ResultBase
	{
	}

	public class ConsumeLog
	{
		public string ItemOID;

		public ItemGainType GainType;

		public int ConsumeNum;

		public string GetJson()
		{
			return JsonUtility.ToJson(this);
		}
	}

	protected override void OnCreateSendParameter()
	{
		ConsumeLog consumeLog = new ConsumeLog
		{
			ItemOID = base.SendParameter.oid,
			GainType = base.SendParameter.gainType,
			ConsumeNum = base.SendParameter.consumeNum
		};
		Debug.Log(consumeLog.GetJson());
		string text = Uri.EscapeDataString(Convert.ToBase64String(Encoding.UTF8.GetBytes(consumeLog.GetJson())));
		m_sendUrl = base.DefaultFrontServerUrl + "/item_gain_log";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&log={text}";
	}
}

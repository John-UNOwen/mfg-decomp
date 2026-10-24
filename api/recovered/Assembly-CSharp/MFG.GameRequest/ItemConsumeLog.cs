using System;
using System.Text;
using MFG.Types;
using UnityEngine;

namespace MFG.GameRequest;

public class ItemConsumeLog : RequestApiBase<ItemConsumeLog, ItemConsumeLog.ResultData, (string oid, int beforeCount, int afterCount, ItemConsumeType consumeType)>
{
	public class ResultData : ResultBase
	{
	}

	public class ConsumeLog
	{
		public string ItemOID;

		public int ConsumeCount;

		public int BeforeCount;

		public int AfterCount;

		public ItemConsumeType ConsumeType;

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
			ConsumeCount = base.SendParameter.beforeCount - base.SendParameter.afterCount,
			BeforeCount = base.SendParameter.beforeCount,
			AfterCount = base.SendParameter.afterCount,
			ConsumeType = base.SendParameter.consumeType
		};
		Debug.Log(consumeLog.GetJson());
		string text = Uri.EscapeDataString(Convert.ToBase64String(Encoding.UTF8.GetBytes(consumeLog.GetJson())));
		m_sendUrl = base.DefaultFrontServerUrl + "/item_consume_log";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&log={text}";
	}
}

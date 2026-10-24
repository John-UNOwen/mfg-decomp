using System;
using System.Text;
using UnityEngine;

namespace MFG.GameRequest;

public class JanponLog : RequestApiBase<JanponLog, JanponLog.ResultData, (string oid, JanponObject.JanponKind kind, bool isSuperJanpon)>
{
	public class ResultData : ResultBase
	{
	}

	public class Log
	{
		public string ObjectID;

		public JanponObject.JanponKind JanponKind;

		public bool IsSuperJanpon;

		public string GetJson()
		{
			return JsonUtility.ToJson(this);
		}
	}

	protected override void OnCreateSendParameter()
	{
		Log log = new Log
		{
			ObjectID = base.SendParameter.oid,
			JanponKind = base.SendParameter.kind,
			IsSuperJanpon = base.SendParameter.isSuperJanpon
		};
		Debug.Log(log.GetJson());
		string text = Uri.EscapeDataString(Convert.ToBase64String(Encoding.UTF8.GetBytes(log.GetJson())));
		m_sendUrl = base.DefaultFrontServerUrl + "/janpon_log";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&log={text}";
	}
}

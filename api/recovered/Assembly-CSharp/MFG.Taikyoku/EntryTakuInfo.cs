using System;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using UnityEngine;

namespace MFG.Taikyoku;

[Serializable]
public class EntryTakuInfo
{
	public string SessionID;

	public string GameServiceUrl;

	public int TakuID;

	public int GameServiceID;

	public int PlayerIndex;

	public int NextSeqNo;

	public int LastChouKouNum;

	public int ChouKouNum;

	public int SuteOya1LimitTime;

	public int SuteLimitTime;

	public int SuteReech1LimitTime;

	public int NakiLimitTime;

	public int AgariLimitTime;

	public int NakiChoiceLimitTime;

	public int ReechiChoiceLimitTime;

	public int ChouKouLimitTime;

	public int MachingLimitTime;

	public int PayMode;

	public void Parse(XElement entryElement)
	{
		GameServiceID = entryElement.MustNeedElement("gserv_id").ValueInt();
		TakuID = entryElement.MustNeedElement("tid").ValueInt();
		PlayerIndex = entryElement.MustNeedElement("pindex").ValueInt();
		NextSeqNo = entryElement.MustNeedElement("next_sno").ValueInt();
		LastChouKouNum = entryElement.MustNeedElement("last_cyoukou_num").ValueInt();
		ChouKouNum = entryElement.MustNeedElement("cyoukou_num").ValueInt();
		SuteOya1LimitTime = entryElement.MustNeedElement("ste_oya1_limit_time").ValueInt();
		SuteLimitTime = entryElement.MustNeedElement("ste_limit_time").ValueInt();
		SuteReech1LimitTime = entryElement.MustNeedElement("ste_reechi1_limit_time").ValueInt();
		NakiLimitTime = entryElement.MustNeedElement("naki_limit_time").ValueInt();
		AgariLimitTime = entryElement.MustNeedElement("agari_limit_time").ValueInt();
		NakiChoiceLimitTime = entryElement.MustNeedElement("naki_choice_limit_time").ValueInt();
		ReechiChoiceLimitTime = entryElement.MustNeedElement("reechi_choice_limit_time").ValueInt();
		ChouKouLimitTime = entryElement.MustNeedElement("last_cyoukou_limit_time").ValueInt();
		MachingLimitTime = entryElement.MustNeedElement("last_time").ValueInt();
		GameServiceUrl = entryElement.MustNeedElement("gserv_url").Value;
		PayMode = entryElement.MustNeedElement("pay_mode").ValueInt();
		Debug.Log($"卓ID:{TakuID} PlayerIndex:{PlayerIndex} 1局あたりの長考数:{ChouKouNum} 残り長考数:{LastChouKouNum} 長考可能秒数:{ChouKouLimitTime} マッチング制限時間:{MachingLimitTime} NextSeqNo:{NextSeqNo}");
	}
}

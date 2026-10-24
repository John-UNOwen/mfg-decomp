using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class GetHaifuList : RequestApiBase<GetHaifuList, GetHaifuList.ResultData>
{
	[Serializable]
	public class HaifuIndexData
	{
		[Serializable]
		public class PlayerInfo
		{
			public string Name = "";

			public int Rank;

			public int Score;
		}

		public int GameMode;

		public int TakuClass;

		public int TakuID;

		public int CopyTakuID;

		public int PlayerIndex;

		public string EndDate;

		public List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();

		public static readonly int MAX_ENTRY_PLAYER = 4;

		public void Parse(XElement haifuDataElem)
		{
			GameMode = haifuDataElem.NeedNotElement("gmode").ValueInt();
			TakuClass = haifuDataElem.NeedNotElement("taku_class").ValueInt();
			TakuID = haifuDataElem.NeedNotElement("tid").ValueInt();
			XElement xElement = haifuDataElem.Element("copy_tid");
			if (xElement != null)
			{
				CopyTakuID = xElement.ValueInt();
			}
			PlayerIndex = haifuDataElem.NeedNotElement("pindex").ValueInt();
			int num = haifuDataElem.NeedNotElement("end_date").ValueInt();
			EndDate = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(num), TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")).ToString();
			for (int i = 0; i < MAX_ENTRY_PLAYER; i++)
			{
				PlayerInfo playerInfo = new PlayerInfo();
				playerInfo.Name = haifuDataElem.NeedNotElement("name" + i).Value;
				playerInfo.Rank = haifuDataElem.NeedNotElement("rank" + i).ValueInt();
				playerInfo.Score = haifuDataElem.NeedNotElement("score" + i).ValueInt();
				PlayerInfos.Add(playerInfo);
			}
		}
	}

	public class ResultData : ResultBase
	{
		public List<HaifuIndexData> HaifuIndexDatas = new List<HaifuIndexData>();
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/get_haifu_list";
		m_postStr = $"version={base.AppVersion}&pcuid={base.AppSessionID}&web_id={base.WebID}&area_id={base.AreaID}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			foreach (XElement item in baseElem.NeedNotElement("haifu_list").Elements())
			{
				HaifuIndexData haifuIndexData = new HaifuIndexData();
				haifuIndexData.Parse(item);
				base.Result.HaifuIndexDatas.Add(haifuIndexData);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

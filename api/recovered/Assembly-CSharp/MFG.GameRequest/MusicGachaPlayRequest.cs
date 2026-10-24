using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class MusicGachaPlayRequest : RequestApiBase<MusicGachaPlayRequest, MusicGachaPlayRequest.ResultData, ulong>
{
	public class GachaResult
	{
		public bool IsSuccess;

		public List<string> GainItems = new List<string>();

		public int AddGiftCount;

		public List<string> FightSpirits = new List<string>();

		public void Parse(XElement dataElem)
		{
			IsSuccess = dataElem.Element("is_success").ValueBool();
			if (!IsSuccess)
			{
				return;
			}
			foreach (XElement item in dataElem.Element("gain_items").Elements("item"))
			{
				GainItems.Add(item.Value);
			}
			AddGiftCount = dataElem.Element("gift").ValueInt();
			if (dataElem.Element("fight_spirits") == null)
			{
				return;
			}
			foreach (XElement item2 in dataElem.Element("fight_spirits").Elements("item"))
			{
				FightSpirits.Add(item2.Value);
			}
		}
	}

	public class ResultData : ResultBase
	{
		public GachaResult GachaResult = new GachaResult();
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/music_gacha_play";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&request_id={base.SendParameter}&api_ver=2";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("gacha_result");
			if (xElement != null)
			{
				base.Result.GachaResult = new GachaResult();
				base.Result.GachaResult.Parse(xElement);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

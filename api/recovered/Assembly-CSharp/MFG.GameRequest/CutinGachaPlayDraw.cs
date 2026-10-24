using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;

namespace MFG.GameRequest;

public class CutinGachaPlayDraw : RequestApiBase<CutinGachaPlayDraw, CutinGachaPlayDraw.ResultData, (int gachaID, int playCount, bool isTicket)>
{
	public class GachaResult
	{
		public bool IsSuccess;

		public ulong RequestID;

		public List<(string, GachaGiftType)> GainItems = new List<(string, GachaGiftType)>();

		public int AddGiftCount;

		public List<CharaType> FightSpirits = new List<CharaType>();

		public void Parse(XElement dataElem)
		{
			IsSuccess = dataElem.Element("is_success").ValueBool();
			RequestID = dataElem.Element("request_id").ValueUInt64();
			if (!IsSuccess)
			{
				return;
			}
			foreach (XElement item in dataElem.Element("gain_items").Elements("item"))
			{
				GainItems.Add((item.Element("oid").Value, (GachaGiftType)item.Element("gift_type").ValueInt()));
			}
			AddGiftCount = dataElem.Element("gift").ValueInt();
			if (dataElem.Element("fight_spirits") == null)
			{
				return;
			}
			foreach (XElement item2 in dataElem.Element("fight_spirits").Elements("item"))
			{
				FightSpirits.Add((CharaType)item2.ValueInt());
			}
		}
	}

	public class ResultData : ResultBase
	{
		public GachaResult GachaResult = new GachaResult();
	}

	protected override void OnCreateSendParameter()
	{
		List<int> values = GameDataManager.GetCharaSelectableList().Cast<int>().ToList();
		string text = "None";
		m_sendUrl = base.DefaultFrontServerUrl + "/cutin_gacha_play_draw";
		m_postStr = string.Format("web_id={0}&area_id={1}&pcuid={2}&gacha_id={3}&play_count={4}&is_ticket={5}&gift_config={6}&enable_charas={7}&debug_param={8}", base.WebID, base.AreaID, base.AppSessionID, base.SendParameter.gachaID, base.SendParameter.playCount, base.SendParameter.isTicket, GameDataManager.PlayerConfig.UnableCharaGachaState, string.Join(",", values), text);
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("gacha_draw");
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

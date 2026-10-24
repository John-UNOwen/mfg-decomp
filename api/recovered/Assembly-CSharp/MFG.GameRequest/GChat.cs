using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Taikyoku;
using MFG.Types.Customize.CustomizeItem;

namespace MFG.GameRequest;

public class GChat : RequestApiBase<GChat, GChat.ResultData, (ItemID TableStickerItemID, int TakuID, int PlayerIndex, int PlayerID)>
{
	public class ResultData : ResultBase
	{
		public StampData Stamp = new StampData();
	}

	protected override void OnCreateSendParameter()
	{
		int item = base.SendParameter.TakuID;
		int item2 = base.SendParameter.PlayerIndex;
		int item3 = base.SendParameter.PlayerID;
		string playerName = GameDataManager.Player.PlayerName;
		string text = "MATCHING";
		string text2 = ((base.SendParameter.TableStickerItemID == ItemID.None) ? "" : base.SendParameter.TableStickerItemID.ToString());
		string text3 = "";
		m_sendUrl = base.DefaultFrontServerUrl + "/gchat";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&gserv_id={base.GameServiceID}&tid={item}&scene={text}&mid={item3}&pindex={item2}&name={playerName}&contents={text2}&param={text3}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		XElement xElement = baseElem.Element("chat");
		if (xElement != null)
		{
			base.Result.Stamp.Parse(xElement);
		}
	}
}

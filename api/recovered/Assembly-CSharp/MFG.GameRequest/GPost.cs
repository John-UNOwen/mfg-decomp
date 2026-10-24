using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Taikyoku;
using MFG.Taikyoku.Command.Receive;
using MFG.Taikyoku.Command.Send;
using UnityEngine;

namespace MFG.GameRequest;

public class GPost : RequestApiBase<GPost, GPost.ResultData, (SendCommand cmd, int TakuID, int PlayerIndex, int PlayerID)>
{
	public class ResultData : ResultBase
	{
		public bool IsReconnect;

		public MatchingData Matching = new MatchingData();

		public TaikyokuData Taikyoku = new TaikyokuData();

		public TaikyokuStartInfo Reconnect = new TaikyokuStartInfo();

		public StampData Stamp = new StampData();
	}

	protected override void OnCreateSendParameter()
	{
		int item = base.SendParameter.TakuID;
		int item2 = base.SendParameter.PlayerIndex;
		int item3 = base.SendParameter.PlayerID;
		int seqNo = base.SendParameter.cmd.SeqNo;
		int kind = (int)base.SendParameter.cmd.Kind;
		int tepaiPos = base.SendParameter.cmd.TepaiPos;
		int tepaiPos2 = base.SendParameter.cmd.TepaiPos2;
		int paiID = base.SendParameter.cmd.PaiID;
		int tepaiID = base.SendParameter.cmd.TepaiID;
		int tepaiID2 = base.SendParameter.cmd.TepaiID2;
		int reachFlag = base.SendParameter.cmd.ReachFlag;
		int tsumoGiriFlag = base.SendParameter.cmd.TsumoGiriFlag;
		int chiNaki = base.SendParameter.cmd.ChiNaki;
		int num = 0;
		int num2 = 0;
		m_sendUrl = base.DefaultFrontServerUrl + "/gpost";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&gserv_id={base.GameServiceID}&tid={item}&mid={item3}&pindex={item2}&must={base.AppVersion}/{base.AppSessionID}/{item}/{item2}/{item3}/{seqNo}/{kind}/{tepaiPos}/{tepaiPos2}/{paiID}/{tepaiID}/{tepaiID2}/{reachFlag}/{tsumoGiriFlag}/{chiNaki}&app_param={num}/{num2}";
		m_serverLog = $"{item}/{item2}/{item3}/{seqNo}/{kind}/{tepaiPos}/{tepaiPos2}/{paiID}/{tepaiID}/{tepaiID2}/{reachFlag}/{tsumoGiriFlag}/{chiNaki}";
		Debug.Log($"GPost param: {base.SendParameter.cmd.Kind}({kind}). pindex:{item2} next_sno:{seqNo} tepai_pos:{tepaiPos} tepai_pos2:{tepaiPos2} pai_id:{paiID}({paiID}) tepai_id:{tepaiID}({tepaiID}) tepai_id2:{tepaiID2}({tepaiID2}) reach_flg:{reachFlag} tsumogiri_flg:{tsumoGiriFlag} chi_naki:{chiNaki}");
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		XElement xElement = baseElem.Element("game");
		if (xElement != null)
		{
			XElement xElement2 = xElement.Element("mwait");
			if (xElement2 != null)
			{
				base.Result.Matching.Parse(xElement2);
			}
			XElement xElement3 = xElement.Element("taikyoku");
			if (xElement3 != null)
			{
				base.Result.Taikyoku.Parse(xElement3);
			}
		}
		XElement xElement4 = baseElem.Element("recon");
		if (xElement4 != null)
		{
			base.Result.IsReconnect = true;
			base.Result.Reconnect.Parse(xElement4);
		}
		XElement xElement5 = baseElem.Element("chat");
		if (xElement5 != null)
		{
			base.Result.Stamp.Parse(xElement5);
		}
	}
}

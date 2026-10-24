using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class PresentDone : RequestApiBase<PresentDone, PresentDone.ResultData, List<int>>
{
	public class PresentInfo
	{
		public ulong PresentID;

		public bool IsSuccess;

		public string Content;

		public int Count;

		public void Parse(XElement dataElem)
		{
			PresentID = dataElem.Element("id").ValueUInt64();
			IsSuccess = dataElem.Element("success").ValueBool();
			if (IsSuccess)
			{
				Content = dataElem.Element("content").Value;
				Count = dataElem.Element("amount").ValueInt();
			}
		}
	}

	public class ResultData : ResultBase
	{
		public List<PresentInfo> PresentInfos = new List<PresentInfo>();
	}

	protected override void OnCreateSendParameter()
	{
		string text = string.Join(",", base.SendParameter);
		m_sendUrl = base.DefaultFrontServerUrl + "/present_done";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&done_ids={text}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("present");
			if (xElement == null)
			{
				return;
			}
			foreach (XElement item in xElement.Elements("data"))
			{
				PresentInfo presentInfo = new PresentInfo();
				presentInfo.Parse(item);
				base.Result.PresentInfos.Add(presentInfo);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

using System;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class ChkTabooword : RequestApiBase<ChkTabooword, ChkTabooword.ResultData, ChkTabooword.ChkParam>
{
	public class ChkParam
	{
		public string CheckString;
	}

	public class ResultData : ResultBase
	{
		public bool IsAvailable;
	}

	protected override void OnCreateSendParameter()
	{
		string text = Uri.EscapeDataString(base.SendParameter.CheckString);
		m_sendUrl = base.DefaultFrontServerUrl + "/chk_tabooword";
		m_postStr = "str=" + text;
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("taboo_chk");
			if (xElement != null)
			{
				base.Result.IsAvailable = !xElement.Element("result").ValueBool();
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

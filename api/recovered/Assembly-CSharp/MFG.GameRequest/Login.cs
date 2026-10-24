using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class Login : RequestApiBase<Login, Login.ResultData>
{
	public class ResultData : ResultBase
	{
		public string SessionID;
	}

	protected override void OnCreateSendParameter()
	{
		string dataID = GameDataManager.Account.DataID;
		string text = (GameDataManager.Account.IsGuestUser ? "1" : "0");
		m_sendUrl = base.DefaultFrontServerUrl + "/login";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&user_id={dataID}&bypass_auth=true&guest={text}";
		m_serverLog = text ?? "";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement baseElement = baseElem.NeedNotElement("auth");
			base.Result.SessionID = baseElement.NeedNotElement("session_id").Value;
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

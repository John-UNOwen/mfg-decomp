using System;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace MFG.GameRequest;

public class ClientStateWrite : RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string kind, string contents)>
{
	public class ResultData : ResultBase
	{
	}

	protected override void OnCreateSendParameter()
	{
		int playerID = GameDataManager.Account.PlayerID;
		string item = base.SendParameter.kind;
		string text = UnityWebRequest.EscapeURL(Convert.ToBase64String(Encoding.UTF8.GetBytes(base.SendParameter.contents)));
		m_sendUrl = base.DefaultFrontServerUrl + "/client_state_write";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&mid={playerID}&kind={item}&data={text}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
	}
}

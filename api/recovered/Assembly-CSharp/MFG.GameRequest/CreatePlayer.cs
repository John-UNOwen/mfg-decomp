using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;

namespace MFG.GameRequest;

public class CreatePlayer : RequestApiBase<CreatePlayer, CreatePlayer.ResultData, (string playerName, string userID)>
{
	public class ResultData : ResultBase
	{
	}

	protected override void OnCreateSendParameter()
	{
		string item = base.SendParameter.playerName;
		string item2 = base.SendParameter.userID;
		string text = (GameDataManager.Account.IsGuestUser ? "1" : "0");
		m_sendUrl = base.DefaultFrontServerUrl + "/create_player";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&user_id={item2}&name={item}&guest={text}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
	}
}

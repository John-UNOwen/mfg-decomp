using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Types;

namespace MFG.GameRequest;

public class SetFavoriteCharacter : RequestApiBase<SetFavoriteCharacter, SetFavoriteCharacter.ResultData, CharaType>
{
	public class ResultData : ResultBase
	{
	}

	protected override void OnCreateSendParameter()
	{
		string text = base.SendParameter.ToObjectID();
		m_sendUrl = base.DefaultFrontServerUrl + "/set_favorite_character";
		m_postStr = $"pcuid={base.AppSessionID}&area_id={base.AreaID}&web_id={base.WebID}&object_id={text}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		await base.OnParse(baseElem);
	}
}

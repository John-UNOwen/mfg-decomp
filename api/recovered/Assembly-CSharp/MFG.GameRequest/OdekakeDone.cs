using System;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class OdekakeDone : RequestApiBase<OdekakeDone, OdekakeDone.ResultData, long>
{
	public class OdekakeInfo
	{
		public string FriendID;

		public bool IsSuccess;

		public CharaGameData CharaData;

		public CustomizeGameData Customize;

		public void Parse(XElement dataElem)
		{
			FriendID = dataElem.Element("id").Value;
			IsSuccess = dataElem.Element("success").ValueBool();
			if (!IsSuccess)
			{
				return;
			}
			foreach (XElement item in dataElem.Elements("state"))
			{
				string value = item.Attribute("kind").Value;
				byte[] bytes = Convert.FromBase64String(item.Element("data").Value);
				string @string = Encoding.UTF8.GetString(bytes);
				if (!(value == "customize_item"))
				{
					if (value == "player_game")
					{
						PlayerGameData playerGameData = PlayerGameData.LoadFromJson(@string);
						CharaData = playerGameData.GetCurrentChara(IsGetMyCharaData: true);
					}
				}
				else
				{
					Customize = CustomizeGameData.LoadFromJson(@string);
				}
			}
		}
	}

	public class ResultData : ResultBase
	{
		public OdekakeInfo OdekakeInfo = new OdekakeInfo();
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/odekake_done";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&odekake_id={base.SendParameter}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("odekake");
			if (xElement != null)
			{
				OdekakeInfo odekakeInfo = new OdekakeInfo();
				odekakeInfo.Parse(xElement);
				base.Result.OdekakeInfo = odekakeInfo;
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

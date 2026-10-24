using System;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using UnityEngine;

namespace MFG.GameRequest;

public class AppliInfo : RequestApiBase<AppliInfo, AppliInfo.ResultData>
{
	public class ResultData : ResultBase
	{
		public uint ExpirationTimeSeconds;

		public ProStats ProStats;

		public GameEventStatus GameControlParamInfo;
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/appli_info";
		m_postStr = $"matching_ver={GameDataManager.Player.MatchingVersion}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			base.Result.ExpirationTimeSeconds = baseElem.MustNeedElement("expire_seconds").ValueUInt();
			Debug.Log($"appli_info Log : {baseElem}");
			foreach (XElement item in baseElem.Elements("info_data"))
			{
				string value = item.Attribute("kind").Value;
				byte[] bytes = Convert.FromBase64String(item.Value);
				string @string = Encoding.UTF8.GetString(bytes);
				if (!(value == "pro_stats"))
				{
					if (value == "events")
					{
						base.Result.GameControlParamInfo = GameEventStatus.LoadFromJson(@string);
					}
					else
					{
						Debug.LogWarning("ApliInfo-->" + value + "のロード処理がありません");
					}
				}
				else
				{
					base.Result.ProStats = ProStats.LoadFromJson(@string);
				}
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

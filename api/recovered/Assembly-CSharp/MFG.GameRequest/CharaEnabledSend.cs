using System;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;

namespace MFG.GameRequest;

public class CharaEnabledSend : RequestApiBase<CharaEnabledSend, CharaEnabledSend.ResultData, CharaType>
{
	public class ResultData : ResultBase
	{
		public bool IsGachaEnable;

		public DateTimeOffset EndDate;

		public string EndDateString => EndDate.ToString("yyyy年MM月dd日（ddd）HH：mmまで", new CultureInfo("ja-JP"));
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/chara_enabled";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&enabled_chara={base.SendParameter}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			bool isGachaEnable = baseElem.Element("is_gacha_enabled").ValueBool();
			string value = baseElem.Element("end_date").Value;
			base.Result.IsGachaEnable = isGachaEnable;
			base.Result.EndDate = DateTimeOffsetManager.GetJSTFromString(value);
		}
		catch
		{
		}
	}
}

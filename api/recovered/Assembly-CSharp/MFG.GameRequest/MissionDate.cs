using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using UnityEngine;

namespace MFG.GameRequest;

public class MissionDate : RequestApiBase<MissionDate, MissionDate.ResultData>
{
	public class ResultData : ResultBase
	{
		public MissionStatus MissionScheduleParamInfo;
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/mission_date";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&format=1";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			Debug.Log($"MissionDate Log : {baseElem}");
			IEnumerable<XElement> enumerable = baseElem?.Elements("info_data");
			if (enumerable == null)
			{
				return;
			}
			foreach (XElement item in enumerable)
			{
				string value = item.Attribute("kind").Value;
				byte[] bytes = Convert.FromBase64String(item.Value);
				string @string = Encoding.UTF8.GetString(bytes);
				if (value == "missions")
				{
					base.Result.MissionScheduleParamInfo = MissionStatus.LoadFromJson(@string);
				}
				else
				{
					Debug.Log("MissionDate-->" + value + "のロード処理がありません");
				}
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

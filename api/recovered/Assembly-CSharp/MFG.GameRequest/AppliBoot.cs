using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameRequest;

public class AppliBoot : RequestApiBase<AppliBoot, AppliBoot.ResultData>
{
	public class ResultData : ResultBase
	{
		public int boot_mask_aclink;

		public string reviewed_version;

		public bool update_recommended;

		public int enterableStat;

		public string mobileSvrUrl;

		public string boot_mes;

		public int update_required;
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/appli_boot";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&user_id=NOUID&version={base.AppVersion}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement serverSettingElem = baseElem.NeedNotElement("server_setting");
			ParseServerSetting(serverSettingElem);
			XElement bootMesElem = baseElem.NeedNotElement("boot_mes");
			ParseBootMes(bootMesElem);
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	protected void ParseServerSetting(XElement serverSettingElem)
	{
		try
		{
			base.Result.boot_mask_aclink = serverSettingElem.NeedNotElement("mask_ac_link_scene").ValueInt();
			base.Result.reviewed_version = serverSettingElem.NeedNotElement("reviewed_version").Value;
			base.Result.update_recommended = serverSettingElem.NeedNotElement("reviewed_version").Value == "true";
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	protected void ParseBootMes(XElement bootMesElem)
	{
		try
		{
			base.Result.enterableStat = bootMesElem.NeedNotElement("status").ValueInt();
			base.Result.mobileSvrUrl = bootMesElem.NeedNotElement("moserv_url").Value;
			base.Result.boot_mes = bootMesElem.NeedNotElement("message").Value;
			base.Result.update_required = bootMesElem.NeedNotElement("message").ValueInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

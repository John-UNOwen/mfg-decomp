using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;

namespace MFG.GameRequest;

public class DojoSetSlot : RequestApiBase<DojoSetSlot, DojoSetSlot.ResultData, (int slotID, CharaType charaType, bool isOverWrite, List<DojoSetSlot.TimeSavingItem> items)>
{
	public enum TimeSavingItem
	{
		OID_DOJO_DEVELOP_0MINUTES = 100,
		OID_DOJO_DEVELOP_1MINUTES,
		OID_DOJO_DEVELOP_5MINUTES
	}

	public class ResultData : ResultBase
	{
		public int SlotID;

		public bool IsUpdated;

		public DojoGetStatus.SlotInfo SlotInfo = new DojoGetStatus.SlotInfo();
	}

	protected override void OnCreateSendParameter()
	{
		if (base.SendParameter.items == null)
		{
			new List<TimeSavingItem>();
		}
		int item = base.SendParameter.slotID;
		bool item2 = base.SendParameter.isOverWrite;
		string text = "";
		if (base.SendParameter.items != null)
		{
			text = string.Join(",", base.SendParameter.items.ConvertAll((TimeSavingItem x) => x.ToString()));
		}
		string text2 = base.SendParameter.charaType.ToObjectID();
		m_sendUrl = base.DefaultFrontServerUrl + "/dojo_set_slot";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&slot_id={item}&set_character={text2}&overwrite={(item2 ? 1 : 0)}&set_item={text}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("dojo");
			if (xElement != null)
			{
				base.Result.SlotID = xElement.Element("slot_id").ValueInt();
				base.Result.IsUpdated = xElement.Element("updated").ValueBool();
				XElement slotElem = xElement.Element("slot");
				base.Result.SlotInfo.Parse(slotElem);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

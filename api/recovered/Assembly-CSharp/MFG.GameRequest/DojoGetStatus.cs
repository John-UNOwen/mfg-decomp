using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;

namespace MFG.GameRequest;

public class DojoGetStatus : RequestApiBase<DojoGetStatus, DojoGetStatus.ResultData>
{
	public class SlotInfo
	{
		public int SlotID;

		public bool IsAvailable;

		public CharaType CharaType;

		public DateTime StartTime;

		public DateTime NextTime;

		public bool HasNext;

		public int SpiritStockCount;

		public int SpiritStockMaxCount;

		public void Parse(XElement slotElem)
		{
			SlotID = slotElem.AttributeValueInt("idx");
			IsAvailable = slotElem.Element("available").ValueBool();
			if (IsAvailable)
			{
				CharaType = CharaTypeExtentions.ToCharaType(slotElem.Element("character_obj").Value);
				StartTime = slotElem.Element("start_time").ValueUInt64ToDateTime();
				NextTime = slotElem.Element("next_time").ValueUInt64ToDateTime();
				HasNext = slotElem.Element("has_next").ValueBool();
				SpiritStockCount = slotElem.Element("reserve_souls").ValueInt();
				SpiritStockMaxCount = slotElem.Element("all_souls").ValueInt();
			}
		}

		public bool IsSpiritFull()
		{
			if (!IsAvailable)
			{
				return false;
			}
			return !HasNext;
		}
	}

	public class ResultData : ResultBase
	{
		public int SlotCount;

		public List<SlotInfo> SlotInfos = new List<SlotInfo>();

		public bool IsSpiritFullSlotExist()
		{
			foreach (SlotInfo slotInfo in SlotInfos)
			{
				if (slotInfo.IsSpiritFull())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsNonSetSlotExist()
		{
			foreach (SlotInfo slotInfo in SlotInfos)
			{
				if (GameDataManager.Player.m_dojoSlotUnlocked[slotInfo.SlotID] && !slotInfo.IsAvailable)
				{
					return true;
				}
			}
			return false;
		}
	}

	protected override void OnCreateSendParameter()
	{
		m_sendUrl = base.DefaultFrontServerUrl + "/dojo_get_status";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			XElement xElement = baseElem.Element("dojo");
			if (xElement == null)
			{
				return;
			}
			base.Result.SlotCount = xElement.Element("slot_nr").ValueInt();
			foreach (XElement item in xElement.Elements("slot"))
			{
				SlotInfo slotInfo = new SlotInfo();
				slotInfo.Parse(item);
				base.Result.SlotInfos.Add(slotInfo);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

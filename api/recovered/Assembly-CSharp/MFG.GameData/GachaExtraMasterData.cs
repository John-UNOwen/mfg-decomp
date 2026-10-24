using System.Collections.Generic;
using MFG.Types;
using MFG.Types.AD;
using MFG.Types.PlayerPaidItem;

namespace MFG.GameData;

public class GachaExtraMasterData
{
	public GachaSeriesName GachaSeriesName;

	public PickupKind PickupKind;

	public List<ItemID> EnableTickets = new List<ItemID> { ItemID.ExtraGachaTicket };

	public bool IsHomeAppear;

	public bool IsReturens;

	public GachaSeriesName RelationGachaSeriesName = (GachaSeriesName)(-1);

	public List<AdTipsID> AdTipsIDs = new List<AdTipsID>();

	public bool IsExPickupOrderByDescending;

	public List<CharaType> LimitedCharaList = new List<CharaType>();

	public GachaSeriesName GachaSeriesMovieSelector
	{
		get
		{
			if (RelationGachaSeriesName == (GachaSeriesName)(-1))
			{
				return GachaSeriesName;
			}
			return RelationGachaSeriesName;
		}
	}
}

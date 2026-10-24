using System.Collections.Generic;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

public class GachaItemInfo
{
	public ItemID ItemID;

	public Dictionary<ThumbnailChangePhase, Rarity> ThumbnailChangeRaritys = new Dictionary<ThumbnailChangePhase, Rarity>();

	public GachaGiftType GiftType;

	public bool IsSameRarityIn1stAnd2nd => ThumbnailChangeRaritys[ThumbnailChangePhase.Change1st] == ThumbnailChangeRaritys[ThumbnailChangePhase.Change2nd];

	public void DebugLog()
	{
		Debug.Log($"{ItemID}");
		foreach (ThumbnailChangePhase key in ThumbnailChangeRaritys.Keys)
		{
			Debug.Log($" {key}->{ThumbnailChangeRaritys[key]}");
		}
	}
}

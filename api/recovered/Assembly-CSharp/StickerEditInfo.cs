using System;
using MFG.MasterGameData;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;

[Serializable]
public class StickerEditInfo
{
	public ItemID ItemID;

	public int Priority;

	public int PositionX;

	public int PositionY;

	public int EulerRotate;

	public bool IsEdited;

	public Item Master => CustomizeItemMaster.GetItem(ItemID);

	public StickerEditInfo(ItemID itemID, int priority, bool isDefault = false)
	{
		ItemID = itemID;
		Priority = priority;
		PositionX = ((!Master.IsBigSize) ? (isDefault ? 100 : (UnityEngine.Random.Range(-18, 19) * 10)) : 0);
		PositionY = (Master.IsBigSize ? (-30) : (isDefault ? (-50) : (UnityEngine.Random.Range(-9, 4) * 10)));
		EulerRotate = 0;
	}
}

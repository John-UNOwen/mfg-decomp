using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types.Customize.Costume;
using UnityEngine;

public class MyRoomSpiritDisplay : MyRoomDisplayBase
{
	[SerializeField]
	private CostumeIcon costumeIcon;

	[SerializeField]
	private Animator debutNotice;

	public override void SetStatus(CancellationToken cancellationToken)
	{
		myRoomFlow.SetFightSpirit(cancellationToken);
		ItemID costumeItemEquip = GameDataManager.Player.GetCurrentChara().CostumeItemEquip;
		if (costumeItemEquip == ItemID.None)
		{
			GameDataManager.Player.GetCurrentChara().CostumeItemEquip = CostumeItemMaster.GetDefaultEquipItemID(GameDataManager.Player.SelectChara);
			costumeItemEquip = GameDataManager.Player.GetCurrentChara().CostumeItemEquip;
		}
		costumeIcon.Reflesh(costumeItemEquip, cancellationToken).Forget();
		int num = CostumeItemMaster.NeedUnlockFightSpiritNum(CostumeItemMaster.GetDebutItemID(GameDataManager.Player.SelectChara));
		if (GameDataManager.Player.GetCurrentChara().IsDebut)
		{
			debutNotice.ResetTrigger("In");
			debutNotice.SetTrigger("Out");
		}
		else if (GameDataManager.PlayerItem.GetUsableSpiritCount(GameDataManager.Player.SelectChara) >= num)
		{
			debutNotice.ResetTrigger("Out");
			debutNotice.SetTrigger("In");
		}
		else
		{
			debutNotice.ResetTrigger("In");
			debutNotice.SetTrigger("Out");
		}
	}
}

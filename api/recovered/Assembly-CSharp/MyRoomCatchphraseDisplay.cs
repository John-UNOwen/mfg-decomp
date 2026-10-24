using System.Threading;
using MFG.MasterGameData;
using MFG.Types.Customize.Catchphrase;
using TMPro;
using UnityEngine;

public class MyRoomCatchphraseDisplay : MyRoomDisplayBase
{
	[SerializeField]
	private TextMeshProUGUI catchphraseName;

	public override void SetStatus(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ItemID catchphraseItemEquip = GameDataManager.Player.GetCurrentChara().CatchphraseItemEquip;
		catchphraseName.text = CatchphraseItemMaster.GetItem(catchphraseItemEquip).DispName;
		myRoomFlow.SetCatchphrase(catchphraseItemEquip, cancellationToken);
	}
}

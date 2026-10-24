using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using TMPro;
using UnityEngine;

public class MyRoomCharaStatus : MonoBehaviour
{
	[SerializeField]
	private SchoolEmblemIcon schoolEmblem;

	[SerializeField]
	private TextMeshProUGUI charaNameText;

	[SerializeField]
	private TextMeshProUGUI cvNameText;

	[SerializeField]
	private GameObject copyright;

	[SerializeField]
	private GameObject debutIconApprentice;

	[SerializeField]
	private GameObject debutIcon;

	[SerializeField]
	private CatchphrasePlate catchphrasePlate;

	[SerializeField]
	private FightSpiritNumDisp mainWindowFightSpiritNumDisp;

	public void SetStatus()
	{
		CharaType selectChara = GameDataManager.Player.SelectChara;
		schoolEmblem.Reflesh(MasterGameDataManager.Character.Properties(selectChara).Profile.SchoolID).Forget();
		string text = MasterGameDataManager.Character.Properties(selectChara).Profile.Name;
		charaNameText.text = text;
		string cV = MasterGameDataManager.Character.Properties(selectChara).Profile.CV;
		cvNameText.text = cV;
		bool isDebut = GameDataManager.Player.GetCurrentChara().IsDebut;
		copyright.SetActive(selectChara == CharaType.Chara10);
		debutIconApprentice.SetActive(!isDebut);
		debutIcon.SetActive(isDebut);
	}

	public void SetFightSpirit(CancellationToken cancellationToken)
	{
		mainWindowFightSpiritNumDisp.Reflesh(cancellationToken).Forget();
	}

	public void SetCatchphrase(ItemID itemID, CancellationToken cancellationToken)
	{
		catchphrasePlate.Reflesh(itemID, cancellationToken).Forget();
	}
}

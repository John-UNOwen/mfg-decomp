using System.Threading;
using MFG.GameData;
using TMPro;
using UnityEngine;

public class MyRoomFavoDisplay : MyRoomDisplayBase
{
	[SerializeField]
	private TextMeshProUGUI favoNumText;

	[SerializeField]
	private Transform gauge;

	[SerializeField]
	private GameObject faboNum;

	[SerializeField]
	private GameObject fabogauge;

	[SerializeField]
	private GameObject lockMassage;

	[SerializeField]
	private GameObject limitMassage;

	public override void SetStatus(CancellationToken cancellationToken)
	{
		CharaGameData currentChara = GameDataManager.Player.GetCurrentChara();
		favoNumText.text = currentChara.Level.ToString("N0");
		int num = currentChara.Exp - currentChara.Master.GetTotalExpRequired(currentChara.Level);
		int expRequiredForNextLevel = currentChara.Master.GetExpRequiredForNextLevel(currentChara.Level);
		MyRoomUtility.SetGauge(gauge, num, num, expRequiredForNextLevel);
		faboNum.SetActive(value: true);
		fabogauge.SetActive(value: true);
		btDetail.gameObject.SetActive(value: true);
		lockMassage.SetActive(value: false);
		limitMassage.SetActive(!GameDataManager.Player.GetCurrentChara().IsDebut);
	}
}

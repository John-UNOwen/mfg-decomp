using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class MyRoomFanDisplay : MyRoomDisplayBase
{
	[SerializeField]
	private TextMeshProUGUI fanNumText;

	[SerializeField]
	[Header("青赤白緑の順")]
	private List<TextMeshProUGUI> fanNumList;

	[SerializeField]
	private GameObject fanNum;

	[SerializeField]
	private GameObject fanElement;

	[SerializeField]
	private GameObject lockMassage;

	[SerializeField]
	private GameObject limitMassage;

	public override void SetStatus(CancellationToken cancellationToken)
	{
		int totalFan = GameDataManager.Player.GetCurrentChara().TotalFan;
		List<int> fans = GameDataManager.Player.GetCurrentChara().m_fans;
		fanNumText.text = totalFan.ToString("N0");
		fanNumList[0].text = fans[0].ToString("N0");
		fanNumList[1].text = fans[1].ToString("N0");
		fanNumList[2].text = fans[2].ToString("N0");
		fanNumList[3].text = fans[3].ToString("N0");
		fanNum.SetActive(value: true);
		fanElement.SetActive(value: true);
		btDetail.gameObject.SetActive(value: true);
		lockMassage.SetActive(value: false);
		limitMassage.SetActive(!GameDataManager.Player.GetCurrentChara().IsDebut);
	}
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BingoEventDisp : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI proceedingBingoSheetNoText;

	[SerializeField]
	private Button proceedingBingoSheetNoUpButton;

	[SerializeField]
	private Button proceedingBingoSheetNoDownButton;

	[SerializeField]
	private Button resetButton;

	private void Start()
	{
		proceedingBingoSheetNoUpButton.onClick.AddListener(delegate
		{
			ChangeProceedingBingoSheetNo(1);
		});
		proceedingBingoSheetNoDownButton.onClick.AddListener(delegate
		{
			ChangeProceedingBingoSheetNo(-1);
		});
		resetButton.onClick.AddListener(delegate
		{
			Reset();
		});
		Disp();
	}

	private void ChangeProceedingBingoSheetNo(int change)
	{
		GameDataManager.BingoEvent.ProceedingBingoSheetNo += change;
		GameDataManager.BingoEvent.ProceedingBingoSheetNo = Math.Clamp(GameDataManager.BingoEvent.ProceedingBingoSheetNo, 0, 9999);
		Disp();
	}

	private void Reset()
	{
		for (int i = 0; i < 9; i++)
		{
			GameDataManager.BingoEvent.Reset();
		}
	}

	private void Disp()
	{
		proceedingBingoSheetNoText.text = "進行中のカード番号：" + GameDataManager.BingoEvent.ProceedingBingoSheetNo;
	}
}

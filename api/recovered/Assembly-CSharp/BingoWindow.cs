using System;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BingoWindow : MonoBehaviour, IExclusiveWindow
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button outofRangeButton;

	[SerializeField]
	private BingoSheet bingoSheet;

	[SerializeField]
	private Transform bingoSheetParent1;

	[SerializeField]
	private TextMeshProUGUI scheduleText;

	[SerializeField]
	private TextMeshProUGUI frypanNum;

	[SerializeField]
	private GameObject debugBingoNoText;

	[SerializeField]
	private Button bingoCardButton;

	[SerializeField]
	private Button bingoCardCloseButton;

	[SerializeField]
	private GameObject bingoCardWindow;

	public bool isOpen;

	private int debugFrypanNum;

	private async void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
		if (bingoCardCloseButton != null)
		{
			bingoCardCloseButton.OnClickAsObservable().Subscribe(delegate
			{
				BingoCardCloseWindow();
			}).AddTo(this);
		}
		if (bingoCardButton != null)
		{
			bingoCardButton.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
				bingoCardWindow.SetActive(value: true);
				bingoCardWindow.GetComponent<Animator>().SetTrigger("In");
				PlayHoleClearEffeect();
			}).AddTo(this);
		}
		if (outofRangeButton != null)
		{
			outofRangeButton.OnClickAsObservable().Subscribe(delegate
			{
				CloseWindow().Forget();
			}).AddTo(this);
		}
		PlayHoleClearEffeect();
		if (scheduleText != null)
		{
			scheduleText.text = BingoEventRemakeGameData.GetBingoEventDate();
		}
	}

	public void OpenWindow(bool isSeEnable = true)
	{
		if (!isOpen)
		{
			if (frypanNum != null)
			{
				frypanNum.text = GameDataManager.BingoEventRemake.GetFryPanNum.ToString();
				debugFrypanNum = GameDataManager.BingoEventRemake.GetFryPanNum;
			}
			if (bingoSheet != null)
			{
				bingoSheet.CardSetUp(GameDataManager.BingoEventRemake.BingoCardYakuList);
			}
			PlayHoleClearEffeect();
			if (bingoSheet != null)
			{
				bingoSheet.CloseAllYakuNaviWindow();
			}
			isOpen = true;
			base.gameObject.SetActive(value: true);
			if (isSeEnable)
			{
				SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			}
			windowObj.GetComponent<Animator>().SetTrigger("In");
		}
	}

	public async UniTask CloseWindow(bool isSeEnable = true)
	{
		if (isOpen)
		{
			if (bingoCardWindow != null)
			{
				BingoCardCloseWindow();
			}
			if (bingoSheet != null)
			{
				bingoSheet.CloseAllYakuNaviWindow();
			}
			if (isSeEnable)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	private void BingoCardCloseWindow()
	{
		SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
		bingoCardWindow.GetComponent<Animator>().SetTrigger("Out");
		bingoCardWindow.SetActive(value: false);
	}

	private void PlayHoleClearEffeect()
	{
		if (!(bingoSheet != null))
		{
			return;
		}
		for (int i = 0; i < 9; i++)
		{
			if (GameDataManager.BingoEventRemake.BingoScoreHoles[i] >= 0)
			{
				bingoSheet.PlayClearLoopHoleEffect(i, GameDataManager.BingoEventRemake.BingoScoreHoles[i]);
			}
		}
	}

	private void Update()
	{
	}

	public void ExclusiveOpen()
	{
		OpenWindow();
	}

	public void OnExclusiveClose()
	{
		CloseWindow(isSeEnable: false).Forget();
	}
}

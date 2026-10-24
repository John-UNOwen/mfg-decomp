using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BingoHole : MonoBehaviour
{
	[SerializeField]
	private BingoSheet bingoSheet;

	[SerializeField]
	private GameObject yakuNaviObj;

	[SerializeField]
	private Image yakuNaviPanelImage;

	[SerializeField]
	private GameObject yakuNameParent;

	[SerializeField]
	private List<GameObject> yakuNamePrefList;

	[SerializeField]
	private List<Sprite> yakuNaviPanelList;

	[SerializeField]
	private TextMeshProUGUI scoreText;

	private bool isClearMarked;

	private bool isFree;

	private async void Start()
	{
		GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
		{
			if (!isFree)
			{
				bingoSheet.CloseAllYakuNaviWindow();
				yakuNaviObj.SetActive(value: true);
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
		}).AddTo(this);
		yakuNaviObj.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
		{
			if (!isFree)
			{
				CloseYakuNaviWindow();
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
		})
			.AddTo(this);
	}

	public void YakuSet(Yaku yaku)
	{
		foreach (Transform item in yakuNameParent.transform)
		{
			Object.Destroy(item.gameObject);
		}
		isFree = yaku == Yaku.Max;
		int index = BingoEventRemakeGameData.yakuPrefabIndex[yaku];
		Object.Instantiate(yakuNamePrefList[index], yakuNameParent.transform);
		if (!isFree)
		{
			yakuNaviPanelImage.sprite = yakuNaviPanelList[index];
		}
	}

	public bool PlayClearHoleEffect(int score)
	{
		if (isClearMarked)
		{
			return false;
		}
		scoreText.text = ((score <= 0) ? "" : score.ToString());
		GetComponent<Animator>().SetTrigger("In");
		isClearMarked = true;
		return true;
	}

	public bool PlayClearLoopHoleEffect(int score)
	{
		scoreText.text = ((score <= 0) ? "" : score.ToString());
		GetComponent<Animator>().SetTrigger("ClearLoop");
		isClearMarked = true;
		return true;
	}

	public void CloseYakuNaviWindow()
	{
		yakuNaviObj.SetActive(value: false);
	}
}

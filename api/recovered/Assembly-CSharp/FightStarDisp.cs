using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FightStarDisp : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI fightStarText;

	[SerializeField]
	private Button fightStarUpButton;

	[SerializeField]
	private Button fightStarDownButton;

	[SerializeField]
	private Button fightStar10UpButton;

	[SerializeField]
	private Button fightStar10DownButton;

	[SerializeField]
	private Button danLevelUpButton;

	[SerializeField]
	private Button danLevelDownButton;

	[SerializeField]
	private Button danLevel10UpButton;

	[SerializeField]
	private Button danLevel10DownButton;

	[SerializeField]
	private Button danLevel100UpButton;

	[SerializeField]
	private Button danLevel100DownButton;

	private void Start()
	{
		fightStarUpButton.onClick.AddListener(delegate
		{
			ChangeFightStar(1);
		});
		fightStarDownButton.onClick.AddListener(delegate
		{
			ChangeFightStar(-1);
		});
		fightStar10UpButton.onClick.AddListener(delegate
		{
			ChangeFightStar(10);
		});
		fightStar10DownButton.onClick.AddListener(delegate
		{
			ChangeFightStar(-10);
		});
		danLevelUpButton.onClick.AddListener(delegate
		{
			ChangeDanLevel(1);
		});
		danLevelDownButton.onClick.AddListener(delegate
		{
			ChangeDanLevel(-1);
		});
		danLevel10UpButton.onClick.AddListener(delegate
		{
			ChangeDanLevel(10);
		});
		danLevel10DownButton.onClick.AddListener(delegate
		{
			ChangeDanLevel(-10);
		});
		danLevel100UpButton.onClick.AddListener(delegate
		{
			ChangeDanLevel(100);
		});
		danLevel100DownButton.onClick.AddListener(delegate
		{
			ChangeDanLevel(-100);
		});
		DispFightStar();
	}

	private void ChangeFightStar(int change)
	{
		int totalFightStarRequired = DanLevelMaster.GetTotalFightStarRequired(DanLevelType.Max);
		GameDataManager.Player.FightStar += change;
		if (GameDataManager.Player.FightStar < 0)
		{
			GameDataManager.Player.FightStar = 0;
		}
		else if (GameDataManager.Player.FightStar > totalFightStarRequired)
		{
			GameDataManager.Player.FightStar = totalFightStarRequired;
		}
		DispFightStar();
	}

	private void ChangeDanLevel(int change)
	{
		int num = (int)(GameDataManager.Player.DanLevel + change);
		if (num <= (int)GameDataManager.Player.MaxDanLevel && num >= 0)
		{
			GameDataManager.Player.FightStar = DanLevelMaster.GetTotalFightStarRequired((DanLevelType)num);
			DispFightStar();
		}
	}

	private void DispFightStar()
	{
		fightStarText.text = $"{GameDataManager.Player.FightStar}(段位：{DanLevelMaster.DanLevelType_ToString(GameDataManager.Player.DanLevel)})";
	}
}

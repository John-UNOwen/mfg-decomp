using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;

public class SubMonitorScore : MonoBehaviour
{
	[SerializeField]
	private Transform charaParent;

	[SerializeField]
	private List<SubMonitorScoreChara> scores = new List<SubMonitorScoreChara>();

	public void Init(List<Player> players)
	{
		if (SystemConfigGameData.IsSXGAMonitor)
		{
			charaParent.GetComponent<RectTransform>().sizeDelta = new Vector2(1200f, 1024f);
		}
		Cha cha;
		for (cha = Cha.Jicha; cha <= Cha.Kamicha; cha++)
		{
			Player player = players.Find((Player x) => x.m_cha == cha);
			if (player != null)
			{
				scores[(int)cha].gameObject.SetActive(value: true);
				scores[(int)cha].Init(player);
			}
			else
			{
				scores[(int)cha].gameObject.SetActive(value: false);
			}
		}
	}

	public void ScoreRefresh(Cha cha, string score, Color color)
	{
		try
		{
			scores.Find((SubMonitorScoreChara x) => x.cha == cha).SetScore(score, color);
		}
		catch
		{
		}
	}

	public void KazeRefresh(Cha cha, Kaze kaze)
	{
		try
		{
			scores.Find((SubMonitorScoreChara x) => x.cha == cha).SetKaze(kaze);
		}
		catch
		{
		}
	}

	public void ReachAuraSetActive(Cha cha, bool isActive, Color color)
	{
		try
		{
			scores.Find((SubMonitorScoreChara x) => x.cha == cha).ReachAuraSetActive(isActive, color);
		}
		catch
		{
		}
	}

	public void AccessoryChange(ItemID itemID)
	{
		foreach (SubMonitorScoreChara score in scores)
		{
			score.AccessoryInit(itemID).Forget();
		}
	}

	public void AllCameraSetEnable(bool isEnable)
	{
		try
		{
			foreach (SubMonitorScoreChara score in scores)
			{
				if (score.gameObject.activeSelf)
				{
					score.SetCameraEnable(isEnable);
				}
			}
		}
		catch
		{
		}
	}
}

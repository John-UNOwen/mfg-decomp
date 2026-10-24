using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Types;
using UniRx;
using UnityEngine;

public class HaiConjectureIconList : MonoBehaviour
{
	[SerializeField]
	private MahjongPlayFlow m_mahjongPlayFlow;

	[SerializeField]
	private Hand myHand;

	[SerializeField]
	private Help help;

	private ConjectureInfos conjectureInfos = new ConjectureInfos();

	private bool m_dispflag;

	private bool m_isDisp;

	private async UniTask Start()
	{
		myHand.OnSwitchSelectedHai.Subscribe(delegate(bool flag)
		{
			if (flag)
			{
				setIconsDisp();
			}
		}).AddTo(this);
		myHand.OnDiscardTsumoHai.Subscribe(delegate
		{
			if (m_dispflag)
			{
				setIconsDisp();
			}
		}).AddTo(this);
	}

	public void SetDisp(bool isDisp)
	{
		m_isDisp = isDisp;
		setIconsDisp();
	}

	public void SetIconsDispOff()
	{
		m_dispflag = false;
		iconsDispOff();
	}

	private void iconsDispOff()
	{
		myHand.DispOffHaiJudgeMark();
	}

	public void Disp(HaiConjectureList _list)
	{
		conjectureInfos = createInfo(_list);
		setIconsDisp();
	}

	private void setIconsDisp()
	{
		iconsDispOff();
		if (!m_isDisp)
		{
			return;
		}
		m_dispflag = true;
		int safeChaNum = conjectureInfos.safeChaNum;
		int dangerChaNum = conjectureInfos.dangerChaNum;
		if (safeChaNum > 0 || dangerChaNum > 0)
		{
			help.IconInNavi();
		}
		foreach (Tile tile in myHand.GetTiles())
		{
			ConjectureInfo info = null;
			foreach (ConjectureInfo item in conjectureInfos.Info)
			{
				if (item.pai == tile.pai.ToNormal())
				{
					info = item;
				}
			}
			InitIcon(info, safeChaNum, dangerChaNum, tile);
		}
	}

	public void InitIcon(ConjectureInfo info, int safeChaNum, int dangerChaNum, Tile tile)
	{
		tile.SetAllGreenMark(set: true);
		if (m_mahjongPlayFlow.m_openMachiList.Contains(MahjongUtility.AkaToNormal(tile.pai)))
		{
			tile.SetHaiJudgeMark(Tile.HaiJudgeMark.OpenDanger);
			return;
		}
		bool num = (info?.safeList?.Count ?? (-1)) == safeChaNum && safeChaNum > 0;
		bool flag = (info?.dangerList?.Count ?? (-1)) > 0 && dangerChaNum > 0;
		if (num)
		{
			tile.SetHaiJudgeMark(Tile.HaiJudgeMark.Safety);
			return;
		}
		if (flag)
		{
			tile.SetHaiJudgeMark(Tile.HaiJudgeMark.Danger);
			return;
		}
		bool flag2 = false;
		foreach (Player player in m_mahjongPlayFlow.Players)
		{
			if (player.m_isReach && !player.IsJicha())
			{
				flag2 = true;
			}
		}
		if (flag2)
		{
			if (MahjongUtility.IsKokushiAnpai(tile.pai))
			{
				tile.SetHaiJudgeMark(Tile.HaiJudgeMark.Safety);
			}
			else
			{
				tile.SetHaiJudgeMark(Tile.HaiJudgeMark.Warning);
			}
		}
		else
		{
			tile.SetHaiJudgeMark(Tile.HaiJudgeMark.None);
		}
	}

	private ConjectureInfos createInfo(HaiConjectureList _src)
	{
		ConjectureInfos conjectureInfos = new ConjectureInfos();
		conjectureInfos.safeChaNum = _src.Safe.Count;
		foreach (KeyValuePair<Cha, HashSet<Pai>> item3 in _src.Safe)
		{
			foreach (Pai item4 in item3.Value)
			{
				bool flag = false;
				if (conjectureInfos.Info.Count > 0)
				{
					foreach (ConjectureInfo item5 in conjectureInfos.Info)
					{
						if (item5.pai == item4)
						{
							item5.safeList.Add(item3.Key);
							flag = true;
						}
					}
				}
				if (!flag)
				{
					ConjectureInfo item = new ConjectureInfo(item4, item3.Key, Cha.Jicha);
					conjectureInfos.Info.Add(item);
				}
			}
		}
		conjectureInfos.dangerChaNum = _src.Danger.Count;
		foreach (KeyValuePair<Cha, HashSet<Pai>> item6 in _src.Danger)
		{
			foreach (Pai item7 in item6.Value)
			{
				bool flag2 = false;
				if (conjectureInfos.Info.Count > 0)
				{
					foreach (ConjectureInfo item8 in conjectureInfos.Info)
					{
						if (item8.pai == item7)
						{
							item8.dangerList.Add(item6.Key);
							flag2 = true;
						}
					}
				}
				if (!flag2)
				{
					ConjectureInfo item2 = new ConjectureInfo(item7, Cha.Jicha, item6.Key);
					conjectureInfos.Info.Add(item2);
				}
			}
		}
		return conjectureInfos;
	}
}

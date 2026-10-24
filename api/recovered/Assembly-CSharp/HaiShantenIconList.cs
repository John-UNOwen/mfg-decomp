using System.Collections.Generic;
using MFG.Types;
using UniRx;
using UnityEngine;

public class HaiShantenIconList : MonoBehaviour
{
	[SerializeField]
	private Hand myHand;

	private List<ShantenInfo> infos = new List<ShantenInfo>();

	private bool isReach;

	private bool isDispOK;

	private const int ICON_NUM = 14;

	private bool m_isDisp;

	private void Start()
	{
		myHand.OnStartReach.Subscribe(delegate
		{
			isReach = true;
		}).AddTo(this);
		myHand.OnSwitchSelectedHai.Subscribe(delegate(bool flag)
		{
			if (flag)
			{
				SetHaiShantenIcon();
			}
		}).AddTo(this);
		myHand.OnDiscardTsumoHai.Subscribe(delegate
		{
			SetHaiShantenIconDispOff();
		}).AddTo(this);
	}

	public void SetDisp(bool isDisp)
	{
		m_isDisp = isDisp;
		SetHaiShantenIcon();
	}

	public void KyokuInit()
	{
		isReach = false;
		isDispOK = false;
		infos.Clear();
	}

	public void ReloadHaiShantenInfo(List<ShantenInfo> _infos)
	{
		isDispOK = true;
		infos = _infos;
		SetHaiShantenIcon();
	}

	public void SetHaiShantenIconDispOff()
	{
		for (int i = 0; i < 14; i++)
		{
			myHand.GetTile(i)?.SetShantenMark(Tile.ShantenMark.None);
		}
		isDispOK = false;
	}

	public void SetHaiShantenIcon()
	{
		if (!m_isDisp)
		{
			for (int i = 0; i < 14; i++)
			{
				myHand.GetTile(i)?.SetShantenMark(Tile.ShantenMark.None);
			}
		}
		else
		{
			if (!isDispOK || isReach)
			{
				return;
			}
			for (int j = 0; j < infos.Count; j++)
			{
				Tile tile = myHand.GetTile(j);
				switch (infos[j].ShantenDifference)
				{
				case ShantenDifference.Up:
					tile?.SetShantenMark(Tile.ShantenMark.Up);
					break;
				case ShantenDifference.Down:
					tile?.SetShantenMark(Tile.ShantenMark.Down);
					break;
				case ShantenDifference.None:
					tile?.SetShantenMark(Tile.ShantenMark.None);
					break;
				}
			}
		}
	}
}

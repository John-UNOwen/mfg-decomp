using System.Collections.Generic;
using MFG.Types;
using TMPro;
using UnityEngine;

public class BombEventDisplay : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private List<GameObject> _bombs;

	[SerializeField]
	private TextMeshProUGUI _remainCounter;

	[SerializeField]
	private List<Color> _bombColor;

	private List<Cha> _indexes = new List<Cha>
	{
		Cha.Jicha,
		Cha.Jicha,
		Cha.Jicha,
		Cha.Jicha
	};

	private Kaze _curKaze;

	public Kaze GetKaze()
	{
		return _curKaze;
	}

	private void Start()
	{
	}

	public void KyokuInit(List<Player> players)
	{
		if (!MahjongUtility.Bomb.IsEffectiveTaiyoku())
		{
			return;
		}
		foreach (GameObject bomb in _bombs)
		{
			bomb.SetActive(value: true);
			bomb.transform.GetComponent<Animator>().SetTrigger("Reset");
			_remainCounter.text = "10";
		}
		for (int i = 0; i < MahjongUtility.GetPlayerNum(GameDataManager.Mahjong.TakuType); i++)
		{
			int cha = (int)players[i].m_cha;
			_bombs[cha].transform.Find("Bomb").GetComponent<SpriteRenderer>().color = _bombColor[cha];
			_bombs[cha].transform.Find("Bomb/add1").GetComponent<SpriteRenderer>().color = _bombColor[cha];
			_bombs[cha].transform.Find("Bomb/add2").GetComponent<SpriteRenderer>().color = _bombColor[cha];
			_indexes[(int)players[i].m_jikaze] = players[i].m_cha;
			if (players[i].m_jikaze == Kaze.Ton)
			{
				_bombs[cha].transform.GetComponent<Animator>().SetTrigger("In");
			}
		}
		_curKaze = Kaze.Ton;
	}

	public Kaze GetCurrentBombPos(int junme, int actionCount, int reachCount)
	{
		if (!MahjongUtility.Bomb.IsEffectiveTaiyoku())
		{
			return (Kaze)(-1);
		}
		return (Kaze)((reachCount + actionCount + (junme - actionCount) / 10) % MahjongUtility.GetPlayerNum(GameDataManager.Mahjong.TakuType));
	}

	public int GetMoveCountDown(int junme, int actionCount)
	{
		return 10 - (junme - actionCount) % 10;
	}

	public Cha GetCurrentBombCha(int junme, int actionCount, int reachCount)
	{
		if (!MahjongUtility.Bomb.IsEffectiveTaiyoku())
		{
			return (Cha)(-1);
		}
		return _indexes[(int)GetCurrentBombPos(junme, actionCount, reachCount)];
	}

	public void Reflesh(int junme, int actionCount, int reachCount)
	{
		bool flag = MahjongUtility.Bomb.IsEffectiveTaiyoku();
		SetActive(flag);
		Kaze currentBombPos = GetCurrentBombPos(junme, actionCount, reachCount);
		if (flag && _curKaze != currentBombPos)
		{
			_bombs[(int)_indexes[(int)_curKaze]].transform.GetComponent<Animator>().SetTrigger("Out");
			_bombs[(int)_indexes[(int)currentBombPos]].transform.GetComponent<Animator>().SetTrigger("In");
			_curKaze = currentBombPos;
		}
		_remainCounter.text = $"{GetMoveCountDown(junme, actionCount)}";
	}

	public void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive && MahjongUtility.Bomb.IsEffectiveTaiyoku());
	}

	public void ForceDisp()
	{
	}
}

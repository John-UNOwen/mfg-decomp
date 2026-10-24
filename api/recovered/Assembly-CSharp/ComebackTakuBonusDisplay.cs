using System.Collections.Generic;
using MFG.Types;
using TMPro;
using UnityEngine;

public class ComebackTakuBonusDisplay : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private TextMeshPro _countText;

	[SerializeField]
	private GameObject _hanBonus;

	[SerializeField]
	private GameObject _frame;

	[SerializeField]
	private GameObject _frameKing;

	private string[] hanColor = new string[4] { "white", "yellow", "orange", "red" };

	public int bonusHan;

	private void Start()
	{
		bonusHan = 0;
	}

	public void Play(List<Player> players, Cha myCha)
	{
		if (MahjongUtility.ComebackTaku.IsEffectiveTaiyoku())
		{
			if (MahjongUtility.ComebackTaku.IsKing(players, myCha))
			{
				_frame.SetActive(value: false);
				_frameKing.SetActive(value: true);
				bonusHan = 0;
				base.gameObject.SetActive(value: true);
				_animator.SetTrigger("CountEffect");
			}
			else
			{
				_frame.SetActive(value: true);
				_frameKing.SetActive(value: false);
				bonusHan = MahjongUtility.ComebackTaku.BonusHan(players, myCha);
				_countText.text = $"<color={hanColor[bonusHan]}>{bonusHan}</color>";
				_hanBonus.SetActive(value: true);
				base.gameObject.SetActive(value: true);
				_animator.SetTrigger("CountEffect");
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void ForceDisp()
	{
		_countText.text = $"<color={hanColor[0]}>{0}</color>";
		_hanBonus.SetActive(value: true);
		base.gameObject.SetActive(value: true);
		_animator.SetTrigger("CountEffect");
	}
}

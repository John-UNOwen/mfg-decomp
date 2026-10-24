using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class AccelDoraRemainCounter : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private TextMeshProUGUI _countText;

	private bool _isDisp = true;

	private int _curRemainCount;

	private int _prevRemainCount;

	private int[] remainCountBorders = new int[4] { 40, 30, 20, 10 };

	private void Start()
	{
		KyokuInit();
	}

	public void KyokuInit()
	{
		_curRemainCount = -1;
		_prevRemainCount = -1;
	}

	private int GetCurrentRemainCount(int remainCount)
	{
		int remainBorder = GetRemainBorder(remainCount);
		if (remainBorder == 0)
		{
			return 0;
		}
		return remainCount - remainBorder;
	}

	private int GetRemainBorder(int remainCount)
	{
		for (int i = 0; i < remainCountBorders.Length; i++)
		{
			int num = remainCountBorders[i];
			if (remainCount > num)
			{
				return num;
			}
		}
		return 0;
	}

	public void ForceDisp()
	{
	}

	public async UniTask Reflesh(int totalRemainCount, bool isDisp = true)
	{
		int currentRemainCount = GetCurrentRemainCount(totalRemainCount);
		bool flag = _isDisp && MahjongUtility.AccelDora.IsEffectiveTaiyoku() && currentRemainCount > 0;
		base.gameObject.SetActive(flag);
		if (flag && currentRemainCount != _curRemainCount)
		{
			_prevRemainCount = _curRemainCount;
			_curRemainCount = currentRemainCount;
			_countText.text = _curRemainCount.ToString();
			if (_isDisp && _curRemainCount > 0)
			{
				_animator.SetTrigger("CountEffect");
			}
		}
	}
}

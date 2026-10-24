using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Types;
using TMPro;
using UnityEngine;

public class HaiShantenCounter : MonoBehaviour
{
	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private TMP_Text m_Text;

	private int m_curShanteCount;

	private int m_prevShantenCount;

	private bool m_isDisp = true;

	private void Start()
	{
		m_animator.gameObject.SetActive(value: false);
		KyokuInit();
	}

	public void KyokuInit()
	{
		m_curShanteCount = -1;
		m_prevShantenCount = -1;
	}

	public async UniTask SetShantenCount(int shanten)
	{
		if (shanten == m_curShanteCount)
		{
			return;
		}
		m_prevShantenCount = m_curShanteCount;
		m_curShanteCount = shanten;
		m_Text.text = m_curShanteCount.ToString();
		if (!m_isDisp || m_curShanteCount <= 0)
		{
			m_animator.gameObject.SetActive(value: false);
			return;
		}
		m_animator.gameObject.SetActive(value: true);
		m_animator.SetTrigger("Update");
		CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
		await UniTask.WhenAny(MahjongUtility.DelayIgnoreHighSpeedMode(1500, cancellationTokenOnDestroy), UniTask.WaitUntil(delegate
		{
			if (ResidentFlow.CustomInput.GetMouseButtonDown(0))
			{
				m_animator.Play(m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
				return true;
			}
			return false;
		}, PlayerLoopTiming.Update, cancellationTokenOnDestroy));
	}

	public void SetDisp(bool isDisp)
	{
		m_isDisp = isDisp;
		m_animator.gameObject.SetActive(m_isDisp && m_curShanteCount > 0);
	}

	public void SetShantenCountDispOff()
	{
		m_animator.gameObject.SetActive(value: false);
	}

	public void PlaySe()
	{
		PlaySe(m_prevShantenCount, m_curShanteCount);
	}

	private void PlaySe(int prevShantenCount, int curShantenCount)
	{
		switch (EvaluateShantenDifference(prevShantenCount, curShantenCount))
		{
		case ShantenDifference.Up:
			switch (curShantenCount)
			{
			case 1:
			case 2:
			case 3:
				SoundManager.PlaySe($"SFX_MJG_REMAIN{curShantenCount}").Forget();
				break;
			default:
				SoundManager.PlaySe("SFX_MJG_REMAIN3").Forget();
				break;
			case 0:
				break;
			}
			break;
		case ShantenDifference.Down:
			SoundManager.PlaySe("SFX_MJG_REMAIN4").Forget();
			break;
		}
	}

	private ShantenDifference EvaluateShantenDifference(int prevShanten, int curShanten)
	{
		ShantenDifference result = ShantenDifference.None;
		if (prevShanten < 0 && curShanten >= 0)
		{
			result = ShantenDifference.Up;
		}
		else if (curShanten > prevShanten)
		{
			result = ShantenDifference.Down;
		}
		else if (curShanten < prevShanten)
		{
			result = ShantenDifference.Up;
		}
		return result;
	}
}

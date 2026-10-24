using GameContents;
using UnityEngine;

public class InsertCoinTextController : MonoBehaviour
{
	private enum InsertCoinTextStatus
	{
		None,
		InsertCoin,
		PleaseTouch
	}

	private InsertCoinTextStatus m_insertCointStatus;

	private void Start()
	{
	}

	private void OnEnable()
	{
		m_insertCointStatus = InsertCoinTextStatus.None;
	}

	private void Update()
	{
		Reflesh();
	}

	private InsertCoinTextStatus GetStatus()
	{
		InsertCoinTextStatus result = InsertCoinTextStatus.None;
		if (TitleDemoUtility.IsGameEntryAcceptable())
		{
			result = ((!GameUtility.IsEnoughCredit(GAME_MODE.START) && !GameUtility.IsKonastaOrNetcafeBehavior) ? InsertCoinTextStatus.InsertCoin : InsertCoinTextStatus.PleaseTouch);
		}
		return result;
	}

	private void Reflesh()
	{
		InsertCoinTextStatus status = GetStatus();
		if (status != m_insertCointStatus)
		{
			Animator component = GetComponent<Animator>();
			switch (status)
			{
			case InsertCoinTextStatus.InsertCoin:
				component.SetTrigger("InsertCoin");
				break;
			case InsertCoinTextStatus.PleaseTouch:
				component.SetTrigger("Touch");
				break;
			default:
				component.SetTrigger("Wait");
				break;
			}
			m_insertCointStatus = status;
		}
	}
}

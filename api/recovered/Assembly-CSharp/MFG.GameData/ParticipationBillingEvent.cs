using System.Collections.Generic;
using GameContents;
using UnityEngine;

namespace MFG.GameData;

public class ParticipationBillingEvent
{
	private Stack<(CONSUME_MODE, int)> m_billingStack = new Stack<(CONSUME_MODE, int)>();

	public void Reserve((CONSUME_MODE mode, int count) billingData)
	{
		Debug.Log($"ParticipationBillingEvent.Reserve mode:{billingData.mode} count:{billingData.count}");
		m_billingStack.Push(billingData);
	}

	public (CONSUME_MODE, int) Consume()
	{
		if (m_billingStack.Count <= 0)
		{
			Debug.LogWarning("ParticipationBillingEvent.Consume このプレーでの課金はありません");
			return (CONSUME_MODE.NONE, 0);
		}
		(CONSUME_MODE, int) result = m_billingStack.Pop();
		switch (result.Item1)
		{
		case CONSUME_MODE.START_CREDIT:
		case CONSUME_MODE.START_PASELI:
		case CONSUME_MODE.CONTINUE_CREDIT:
		case CONSUME_MODE.CONTINUE_PASELI:
		case CONSUME_MODE.PREMIUM_START_PASELI:
		case CONSUME_MODE.PREMIUM_CONTINUE_PASELI:
		case CONSUME_MODE.PREMIUM_START_CREDIT:
		case CONSUME_MODE.PREMIUM_CONTINUE_CREDIT:
			Debug.Log($"ParticipationBillingEvent.Consume mode:{result.Item1} count:{result.Item2}");
			GameUtility.SendPpEvent(result.Item1, result.Item2);
			break;
		case CONSUME_MODE.CONTINUE_FREE:
			Debug.Log("無料コンティニューなのでPPは飛ばさない");
			return (CONSUME_MODE.NONE, 0);
		default:
			Debug.LogWarning($"ParticipationBillingEvent.Consume 未対応のppイベントです mode:{result.Item1} count:{result.Item2}");
			break;
		}
		return result;
	}
}

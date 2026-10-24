using System;
using System.Linq;
using GameContents;
using TestModeSkeleton;
using UnityEngine;

namespace MFG.GameData;

public class GachaTicketCounter
{
	private int[] m_count = new int[2];

	public void Add(TicketType ticketType, int count)
	{
		m_count[(int)ticketType] += count;
		if (ticketType == TicketType.Paid && count > 0)
		{
			CabinetManager.GachaTicketLog.RecordPurchase(count);
		}
	}

	public int Count()
	{
		return m_count.Sum();
	}

	public int Count(TicketType ticketType)
	{
		return m_count[(int)ticketType];
	}

	public bool Consume(int consumeRequestCount)
	{
		if (consumeRequestCount > Count())
		{
			Debug.LogError("消費枚数が所有枚数より多い");
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			int num = 0;
			num = ((m_count[i] - consumeRequestCount < 0) ? m_count[i] : consumeRequestCount);
			m_count[i] -= num;
			consumeRequestCount -= num;
			if (i == 1 && num > 0)
			{
				CabinetManager.GachaTicketLog.RecordConsume(num);
			}
			if (consumeRequestCount <= 0)
			{
				return true;
			}
		}
		return true;
	}

	public void Clear()
	{
		Array.Clear(m_count, 0, m_count.Length);
	}

	public void AddBonusTicket(GAME_MODE gameMode)
	{
		Add(TicketType.Bonus, GetBonusTicketCount(gameMode));
	}

	private int GetBonusTicketCount(GAME_MODE gameMode)
	{
		switch (gameMode)
		{
		case GAME_MODE.START:
			return 1;
		case GAME_MODE.CONTINUE:
			return 1;
		case GAME_MODE.GACHA:
			return 0;
		default:
			Debug.LogWarning("該当するモードがない");
			return 0;
		}
	}
}

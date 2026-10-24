using System;
using System.Collections.Generic;
using System.Linq;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Vote;

namespace MFG.GameData;

[Serializable]
public class VoteEventInfo
{
	public GameEventType FlagType;

	public List<VoteInfo> VoteInfos = new List<VoteInfo>();

	public bool IsFirstDisp = true;

	public int Point;

	public int TotalPoint;

	public int DiscardPoint;

	public int VotedPoint => VoteInfos.Select((VoteInfo x) => x.VoteCount).Sum();

	public bool IsEnable
	{
		get
		{
			if (!GameDataManager.Account.IsGuestUser)
			{
				return GameDataManager.System.GameControlInfo.EventStatus.IsActive(FlagType);
			}
			return false;
		}
	}

	public Item Master => VoteMaster.GetItem(FlagType);

	public bool IsComplete()
	{
		if (Master.VoteMaxToOneChara < 0)
		{
			return false;
		}
		foreach (CharaType chara in Master.Characters)
		{
			if ((!Master.IsOnlyEnableChara || GameDataManager.IsCharaSelectable(chara)) && (VoteInfos?.Find((VoteInfo x) => x.Chara == chara).VoteCount ?? (-1)) < Master.VoteMaxToOneChara)
			{
				return false;
			}
		}
		return true;
	}

	public VoteEventInfo(GameEventType eventType)
	{
		List<VoteInfo> voteInfos = VoteInfos;
		if (voteInfos != null && voteInfos.Count > 0)
		{
			return;
		}
		FlagType = eventType;
		foreach (CharaType character in Master.Characters)
		{
			VoteInfos.Add(new VoteInfo
			{
				Chara = character
			});
		}
	}

	public void AddPoint(int addCount)
	{
		Point += addCount;
		TotalPoint += addCount;
	}

	public void PointDiscard()
	{
		if (Master.IsPointMaxEnable)
		{
			int num = Master.AvailablePointMax();
			if (Point > num)
			{
				DiscardPoint += Point - num;
				Point = num;
			}
		}
	}

	public bool Vote(CharaType chara, int consume)
	{
		VoteInfo voteInfo = VoteInfos.Find((VoteInfo x) => x.Chara == chara);
		if (voteInfo != null && consume > 0 && consume <= Point && (Master.VoteMaxToOneChara < 0 || voteInfo.VoteCount < Master.VoteMaxToOneChara))
		{
			voteInfo.VoteCount += consume;
			Point -= consume;
			voteInfo.LastVoteDate = DateTimeOffsetManager.GetJSTTime().ToString();
			if (Master.VoteMaxToOneChara >= 0 && voteInfo.VoteCount >= Master.VoteMaxToOneChara && Master?.RewardItemList?[Master.Characters.IndexOf(chara)] != null)
			{
				ItemUtility.AddItem(Master.RewardItemList[Master.Characters.IndexOf(chara)]);
			}
			return true;
		}
		return false;
	}
}

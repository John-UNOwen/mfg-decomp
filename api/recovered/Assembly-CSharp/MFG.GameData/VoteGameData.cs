using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Types;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class VoteGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "vote_event";

	[NonSerialized]
	public readonly GameEventType CurrentEvent = (GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.RainyDay002) ? GameEventType.RainyDay002 : GameEventType.RainyDay001);

	public List<VoteEventInfo> VoteEventInfos = new List<VoteEventInfo>();

	[NonSerialized]
	public VoteResultInfo ResultInfo;

	public VoteEventInfo CurrentInfo => VoteEventInfos.Find((VoteEventInfo x) => x.FlagType == CurrentEvent);

	public bool IsEnable
	{
		get
		{
			if (CurrentInfo != null)
			{
				return CurrentInfo.IsEnable;
			}
			return false;
		}
	}

	public bool IsResultEnable
	{
		get
		{
			if (IsEnable)
			{
				return CurrentInfo.Master.IsTaikyokuBonusEnable;
			}
			return false;
		}
	}

	public VoteGameData()
	{
		VoteUpdate();
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("VoteGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("vote_event", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static VoteGameData LoadFromJson(string jsonString)
	{
		Debug.Log("VoteGameData Load : " + jsonString);
		VoteGameData voteGameData = JsonUtility.FromJson<VoteGameData>(jsonString);
		if (voteGameData == null)
		{
			voteGameData = new VoteGameData();
		}
		voteGameData.Validate();
		return voteGameData;
	}

	private void Validate()
	{
	}

	public void VoteUpdate()
	{
		if (CurrentInfo == null)
		{
			VoteEventInfos.Add(new VoteEventInfo(CurrentEvent));
		}
	}

	public void OnEvaluateResult(Rank rank, bool isJandri, bool isProMatching, bool isWin, bool isKiken)
	{
		if (IsEnable)
		{
			ResultInfo = new VoteResultInfo
			{
				IsJandriEffective = isJandri,
				IsKiken = isKiken,
				MyRank = rank,
				IsProMatching = isProMatching,
				IsProMatchingWinner = isWin,
				BeforeTotalAcquiredPoint = CurrentInfo.Point
			};
		}
	}

	public void ApplyResult()
	{
		if (ResultInfo != null)
		{
			CurrentInfo.AddPoint(ResultInfo.AcquiredPoint);
		}
	}
}

using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using UnityEngine;

public class EventStartFlow : BaseFlow
{
	[SerializeField]
	private CompetitionEntryWindow _competitionEntryWindow;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken = default(CancellationToken))
	{
		await _competitionEntryWindow.Init();
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			ResidentFlow.GetInstance().GetTimer().SetTimer(60f, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			SoundManager.PlayMyCharaMyRoomBGM().Forget();
			await _competitionEntryWindow.WindowOpen();
			CompetitionInfo inProgressCompetitionInfo = GameDataManager.Competition.GetInProgressCompetitionInfo();
			if (inProgressCompetitionInfo != null)
			{
				GameDataManager.Player.EntryConfirmedCompetitionID = inProgressCompetitionInfo.CompetitionID;
			}
			GotoNextScene("Home");
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override void OnUpdate()
	{
		if (ResidentFlow.GetInstance().GetTimer().IsEnd())
		{
			_competitionEntryWindow.WindowClose();
		}
	}
}

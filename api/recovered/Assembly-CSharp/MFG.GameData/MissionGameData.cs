using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData.EventTaku;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Mission;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class MissionGameData
{
	[Serializable]
	public class DependencyDisplayInfo
	{
		public MissionID MissionID;

		public bool IsDisplay;
	}

	private const int CurrentDataVersion = 2;

	public int DataVersion = 2;

	public const string ClientStateName = "mission";

	public List<MissionProgressInfo> m_missionProgressInfos = new List<MissionProgressInfo>();

	public List<DependencyDisplayInfo> PlayerDependencyDisplayInfos = new List<DependencyDisplayInfo>();

	public string DailyMissionTimeStamp = "";

	public string WeeklyMissionTimeStamp = "";

	public List<GameEventType> MissionEventDispFinishes = new List<GameEventType>();

	public List<MissionInfo> m_missions = new List<MissionInfo>();

	public string NextBussinessDay = "";

	[NonSerialized]
	public bool isLevelUp;

	[NonSerialized]
	public List<Yaku> HoraYakuList = new List<Yaku>();

	[NonSerialized]
	public List<Han> HoraHanList = new List<Han>();

	[NonSerialized]
	public int LessonCount;

	[NonSerialized]
	public bool IsChangeChara;

	[NonSerialized]
	public bool IsChangeEquips;

	[NonSerialized]
	public bool IsChangeCutIn;

	[NonSerialized]
	public Cha LatestAgariCha = Cha.Kamicha;

	[NonSerialized]
	public int FinishBa;

	[NonSerialized]
	public int FinishKyoku;

	[NonSerialized]
	public long StartScore = 20000L;

	[NonSerialized]
	public MahjongActorID WinningPro;

	[NonSerialized]
	public CompetitionInfo CompetitionInfo = new CompetitionInfo();

	[NonSerialized]
	public bool IsHomeJudgeFinished;

	[NonSerialized]
	public Dictionary<GameEventType, EventProgressMap.ProgressInfo> ProgressMapDict = new Dictionary<GameEventType, EventProgressMap.ProgressInfo>();

	[NonSerialized]
	public Dictionary<Category, int> MissionEventItemDict = new Dictionary<Category, int>();

	public bool IsMissionProgressBoost
	{
		get
		{
			if (!GameDataManager.CoopData.IsJanDriEnable())
			{
				return GameDataManager.Account.IsPremiumMode();
			}
			return true;
		}
	}

	public MissionGameData()
	{
		if (m_missionProgressInfos == null || m_missionProgressInfos.Count() <= 0)
		{
			MissionUpdate();
		}
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("MissionGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("mission", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static MissionGameData LoadFromJson(string jsonString)
	{
		Debug.Log("MissionGameData Load : " + jsonString);
		MissionGameData missionGameData = JsonUtility.FromJson<MissionGameData>(jsonString);
		if (missionGameData == null)
		{
			missionGameData = new MissionGameData();
		}
		missionGameData.Validate();
		return missionGameData;
	}

	private void Validate()
	{
		if (DataVersion == 1)
		{
			m_missionProgressInfos = MissionConversion.Convert(m_missions);
			string text = m_missions?.Find((MissionInfo x) => x.ObjectID == "OID_Mission_Weekly_001")?.LimitDate ?? "";
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = DateTimeOffsetManager.GetJSTFromString(text).AddDays(-7.0).ToString("yyyy/MM/dd HH:mm:ss");
				Debug.Log("ウィークリーミッションConvert : " + text2);
				WeeklyMissionTimeStamp = text2;
			}
			if (!string.IsNullOrEmpty(NextBussinessDay))
			{
				string text3 = DateTimeOffsetManager.GetJSTFromString(NextBussinessDay).AddDays(-1.0).ToString("yyyy/MM/dd HH:mm:ss");
				Debug.Log("デイリーミッションConvert : " + text3);
				DailyMissionTimeStamp = text3;
			}
			DataVersion = 2;
		}
	}

	public bool IsAutoClearItemPossessionDisplay(bool isAutoClearItemPossession, MissionID id, string itemOID)
	{
		if (isAutoClearItemPossession)
		{
			DependencyDisplayInfo dependencyDisplayInfo = PlayerDependencyDisplayInfos.Find((DependencyDisplayInfo x) => x.MissionID == id);
			if (dependencyDisplayInfo == null)
			{
				bool flag = !ItemUtility.IsPossession(itemOID);
				PlayerDependencyDisplayInfos.Add(new DependencyDisplayInfo
				{
					MissionID = id,
					IsDisplay = flag
				});
				return flag;
			}
			return dependencyDisplayInfo.IsDisplay;
		}
		return true;
	}

	public void ResultProgressAnimInit()
	{
		ProgressMapDict = new Dictionary<GameEventType, EventProgressMap.ProgressInfo>();
		if (GameDataManager.Account.IsGuestUser)
		{
			return;
		}
		try
		{
			foreach (BaseEventGameData item in GameDataManager.EventTaku.AvailableModeGameDatas())
			{
				MissionID missionID3 = item.Master.MainMissionID;
				MissionProgressInfo missionProgressInfo = m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == missionID3);
				if (missionProgressInfo != null)
				{
					EventProgressMap.ProgressInfo value = new EventProgressMap.ProgressInfo
					{
						missionID = missionID3,
						currentIndex = missionProgressInfo.Progress,
						progressingTerm = item.Master.EventItemDisplayName + "を集めよう",
						achieveIndex = missionProgressInfo.Progress,
						isJandriUsed = IsMissionProgressBoost,
						resultType = PlayCountResultWindow.ResultType.EventTaku
					};
					GameDataManager.Mission.ProgressMapDict.Add(item.Master.CurrentFlagType, value);
				}
			}
		}
		catch
		{
		}
		foreach (CategoryItem missionEvent in MissionCategoryMaster.MissionEvents)
		{
			if (missionEvent.IsEnable() && missionEvent.IsDispPlayCountWindow)
			{
				MissionID missionID2 = (missionEvent.IsCharaMainMission ? MissionMaster.GetItems(missionEvent.Category, GameDataManager.Player.GetCurrentChara().charaType).First().MissionID : missionEvent.MainMissionID);
				MissionProgressInfo missionProgressInfo2 = m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == missionID2);
				if (missionProgressInfo2 != null)
				{
					EventProgressMap.ProgressInfo value2 = new EventProgressMap.ProgressInfo
					{
						missionID = missionID2,
						currentIndex = missionProgressInfo2.Progress,
						progressingTerm = ((missionProgressInfo2.Master.Kind == Kind.MissionEventItem) ? (ItemUtility.ItemName(missionProgressInfo2.Master.CategoryMaster.EventItemOID) + "を集めよう") : "対局しよう"),
						achieveIndex = missionProgressInfo2.Progress,
						isJandriUsed = IsMissionProgressBoost,
						resultType = PlayCountResultWindow.ResultType.MissionEvent
					};
					GameDataManager.Mission.ProgressMapDict.Add(missionEvent.MissionEventType, value2);
				}
			}
		}
		foreach (CategoryItem bingoEvent in MissionCategoryMaster.BingoEvents)
		{
			if (bingoEvent.IsEnable() && bingoEvent.IsDispPlayCountWindow)
			{
				MissionID missionID = (bingoEvent.IsCharaMainMission ? MissionMaster.GetItems(bingoEvent.Category, GameDataManager.Player.GetCurrentChara().charaType).First().MissionID : bingoEvent.MainMissionID);
				MissionProgressInfo missionProgressInfo3 = m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == missionID);
				if (missionProgressInfo3 != null)
				{
					EventProgressMap.ProgressInfo value3 = new EventProgressMap.ProgressInfo
					{
						missionID = missionID,
						currentIndex = missionProgressInfo3.Progress,
						progressingTerm = "フライパンを集めよう",
						achieveIndex = missionProgressInfo3.Progress,
						isJandriUsed = false,
						resultType = PlayCountResultWindow.ResultType.BingoEvent
					};
					GameDataManager.Mission.ProgressMapDict.Add(bingoEvent.MissionEventType, value3);
				}
			}
		}
	}

	public void ContinueProcess()
	{
		isLevelUp = false;
		HoraYakuList = new List<Yaku>();
		HoraHanList = new List<Han>();
		LessonCount = 0;
		IsChangeChara = false;
		IsChangeEquips = false;
		IsChangeCutIn = false;
		LatestAgariCha = Cha.Kamicha;
		FinishBa = 0;
		FinishKyoku = 0;
		StartScore = 20000L;
		WinningPro = MahjongActorID.None;
		CompetitionInfo = new CompetitionInfo();
		IsHomeJudgeFinished = false;
		ProgressMapDict = new Dictionary<GameEventType, EventProgressMap.ProgressInfo>();
		MissionEventItemDict = new Dictionary<Category, int>();
	}

	public void MissionUpdate()
	{
		try
		{
			List<MissionProgressInfo> list = (from x in Enum.GetValues(typeof(MissionID)).Cast<MissionID>().ToList()
				where !m_missionProgressInfos.Exists((MissionProgressInfo y) => y.MissionID == x)
				select new MissionProgressInfo(x)).ToList();
			if (list != null && list.Count > 0)
			{
				m_missionProgressInfos.AddRange(list);
			}
			MissionProgressInfo missionProgressInfo = m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == MissionID.None);
			if (missionProgressInfo != null)
			{
				m_missionProgressInfos.Remove(missionProgressInfo);
			}
			string timeStamp = MissionCategoryMaster.GetTimeStamp(UpdateInterval.Weekly);
			Debug.Log("Weekly Timestamp [" + timeStamp + "]/[" + WeeklyMissionTimeStamp + "]");
			if (string.IsNullOrEmpty(WeeklyMissionTimeStamp) || !timeStamp.Equals(WeeklyMissionTimeStamp))
			{
				List<MissionProgressInfo> list2 = m_missionProgressInfos?.Where((MissionProgressInfo x) => x.MissionID != 0 && x.Master.UpdateInterval == UpdateInterval.Weekly)?.ToList();
				if (list2 != null && list2.Count > 0)
				{
					foreach (MissionProgressInfo item in list2)
					{
						item.Reset();
					}
				}
				WeeklyMissionTimeStamp = timeStamp;
			}
			string timeStamp2 = MissionCategoryMaster.GetTimeStamp(UpdateInterval.Daily);
			if (!string.IsNullOrEmpty(DailyMissionTimeStamp) && timeStamp2.Equals(DailyMissionTimeStamp))
			{
				return;
			}
			List<MissionProgressInfo> list3 = m_missionProgressInfos?.Where((MissionProgressInfo x) => x.MissionID != 0 && x.Master.UpdateInterval == UpdateInterval.Daily)?.ToList();
			if (list3 != null && list3.Count > 0)
			{
				foreach (MissionProgressInfo item2 in list3)
				{
					item2.Reset();
				}
			}
			DailyMissionTimeStamp = timeStamp2;
		}
		catch
		{
			Debug.Log("MissionUpdate Error catched.");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData.EventTaku;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using MFG.Types.EventTakuMaster;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class EventTakuGameData : ISerializationCallbackReceiver
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "event_taku";

	[NonSerialized]
	public List<BaseEventGameData> GameDatas = new List<BaseEventGameData>();

	[SerializeField]
	private List<BlowawayEventGameData> _blowaways = new List<BlowawayEventGameData>();

	[SerializeField]
	private List<FireReachEventGameData> _fireReaches = new List<FireReachEventGameData>();

	[SerializeField]
	private List<AccelDoraEventGameData> _accelDoras = new List<AccelDoraEventGameData>();

	[SerializeField]
	private List<MentanpinEventGameData> _mentanpins = new List<MentanpinEventGameData>();

	[SerializeField]
	private List<AotenjoEventGameData> _aotenjos = new List<AotenjoEventGameData>();

	[SerializeField]
	private List<ComebackTakuEventGameData> _comebacks = new List<ComebackTakuEventGameData>();

	[SerializeField]
	private List<KirisameTakuEventGameData> _kirisames = new List<KirisameTakuEventGameData>();

	[SerializeField]
	private List<MeldBonusTakuEventGameData> _meldbonuses = new List<MeldBonusTakuEventGameData>();

	[SerializeField]
	private List<FireGalaxyEventGameData> _fireGalaxys = new List<FireGalaxyEventGameData>();

	[SerializeField]
	private List<ReversalEventGameData> _reversals = new List<ReversalEventGameData>();

	[SerializeField]
	private List<BombEventGameData> _bombs = new List<BombEventGameData>();

	[SerializeField]
	private List<AllGreenEventGameData> _allgreens = new List<AllGreenEventGameData>();

	[SerializeField]
	private List<AtomicEventGameData> _atomics = new List<AtomicEventGameData>();

	[SerializeField]
	private List<PckGalaxyEventGameData> _pckGalaxys = new List<PckGalaxyEventGameData>();

	private bool IsConstancyTaku => GameDataManager.Mahjong.IsConstancyTaku;

	private bool IsCompetitionTaku => GameDataManager.Mahjong.IsCompetitionTaku;

	private MFG.Types.EventType CurrentEventType => GameDataManager.Mahjong.EventType;

	private TakuType CurrentTakuType => GameDataManager.Mahjong.TakuType;

	public EventTakuGameData()
	{
		RefleshCurrentEvent();
	}

	public void RefleshCurrentEvent()
	{
		foreach (MFG.Types.EventType value in Enum.GetValues(typeof(MFG.Types.EventType)))
		{
			try
			{
				Current(value);
			}
			catch (InvalidOperationException)
			{
			}
		}
	}

	public BaseEventGameData Current(MFG.Types.EventType eventType)
	{
		Item master = EventTakuMaster.GetItem(eventType);
		if (master == null)
		{
			throw new InvalidOperationException($"対象のイベントではありません eventType:{eventType}");
		}
		BaseEventGameData baseEventGameData = null;
		try
		{
			baseEventGameData = (from x in GameDatas.OfType<BaseEventGameData>()
				where x.EventType == eventType && master.CurrentFlagType == x.FlagType
				select x).First();
		}
		catch
		{
		}
		if (baseEventGameData == null)
		{
			BaseEventGameData baseEventGameData2 = (BaseEventGameData)Activator.CreateInstance(master.GameDataType);
			baseEventGameData2.FlagType = EventTakuMaster.GetItem(baseEventGameData2.EventType).CurrentFlagType;
			GameDatas.Add(baseEventGameData2);
		}
		return baseEventGameData;
	}

	public T Current<T>(MFG.Types.EventType eventType) where T : BaseEventGameData
	{
		return (T)Current(eventType);
	}

	public IEnumerable<BaseEventGameData> AvailableModeGameDatas()
	{
		RefleshCurrentEvent();
		return GameDatas.Where((BaseEventGameData x) => x.IsAvailable());
	}

	public IEnumerable<BaseEventGameData> AvailableModeGameDatasWithGuest()
	{
		RefleshCurrentEvent();
		return GameDatas.Where((BaseEventGameData x) => GameDataManager.System.GameControlInfo.EventStatus.IsActive(x.FlagType));
	}

	public IEnumerable<BaseEventGameData> ActiveAtTaikyokuGameDatas()
	{
		RefleshCurrentEvent();
		return GameDatas.Where((BaseEventGameData x) => x.IsActiveAtTaikyoku(CurrentEventType, CurrentTakuType, IsConstancyTaku, IsCompetitionTaku));
	}

	public IEnumerable<BaseEventGameData> ConstancyTakuAvailables()
	{
		RefleshCurrentEvent();
		Debug.Log(string.Join(',', GameDatas.Select((BaseEventGameData x) => x.FlagType).ToList()));
		return from x in GameDatas
			where x.FlagType == EventTakuMaster.GetItem(x.EventType).CurrentFlagType
			where x.IsConstancyTakuAvailable()
			select x;
	}

	public IEnumerable<BaseEventGameData> CompetitionTakuAvailables()
	{
		RefleshCurrentEvent();
		Debug.Log(string.Join(',', GameDatas.Select((BaseEventGameData x) => x.FlagType).ToList()));
		return from x in GameDatas
			where x.FlagType == EventTakuMaster.GetItem(x.EventType).CurrentFlagType
			where x.IsCompetitionTakuAvailable()
			select x;
	}

	public void ResetForContinueStart()
	{
		foreach (BaseEventGameData item in AvailableModeGameDatas())
		{
			item.ResetForContinueStart();
		}
	}

	public void ResetForBeforeTaikyokuStart(List<Player> players)
	{
		foreach (BaseEventGameData item in ActiveAtTaikyokuGameDatas())
		{
			item.ResetForBeforeTaikyokuStart(players);
		}
	}

	public void OnRyukyoku(RyuKyoku cmd, List<Player> players)
	{
		foreach (BaseEventGameData item in ActiveAtTaikyokuGameDatas())
		{
			item.OnRyukyoku(cmd, players);
		}
	}

	public void OnTsumoAgari(TsumoAgari cmd, List<Player> players)
	{
		foreach (BaseEventGameData item in ActiveAtTaikyokuGameDatas())
		{
			item.OnTsumoAgari(cmd, players);
		}
	}

	public void OnRonAgari(Ron cmd, List<Player> players)
	{
		foreach (BaseEventGameData item in ActiveAtTaikyokuGameDatas())
		{
			item.OnRonAgari(cmd, players);
		}
	}

	public void OnKyokuStart(KyokuStart cmd, List<Player> players)
	{
		foreach (BaseEventGameData item in ActiveAtTaikyokuGameDatas())
		{
			item.OnKyokuStart(cmd, players);
		}
	}

	public void OnKyokuInit(List<Player> players)
	{
		foreach (BaseEventGameData item in ActiveAtTaikyokuGameDatas())
		{
			item.OnKyokuInit(players);
		}
	}

	public void OnEvaluateResult(MahjongGameData mahjongGameData, bool isJandri)
	{
		foreach (BaseEventGameData item in ActiveAtTaikyokuGameDatas())
		{
			int totalPoint = item.TotalPoint;
			item.RefleshResultInfo(totalPoint, mahjongGameData, isJandri);
		}
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("EventTakuGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("event_taku", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static EventTakuGameData LoadFromJson(string jsonString)
	{
		Debug.Log("EventTakuGameData Load : " + jsonString);
		return JsonUtility.FromJson<EventTakuGameData>(jsonString);
	}

	public void OnBeforeSerialize()
	{
		foreach (MFG.Types.EventType value3 in Enum.GetValues(typeof(MFG.Types.EventType)))
		{
			Item item = EventTakuMaster.GetItem(value3);
			if (item != null)
			{
				object value = GetType().GetField(item.GameDataFieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
				typeof(List<>).MakeGenericType(item.GameDataType).GetMethod("Clear").Invoke(value, null);
			}
		}
		foreach (BaseEventGameData gameData in GameDatas)
		{
			Item item2 = EventTakuMaster.GetItem(gameData.EventType);
			if (item2 != null)
			{
				object value2 = GetType().GetField(item2.GameDataFieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
				typeof(List<>).MakeGenericType(item2.GameDataType).GetMethod("Add", new Type[1] { item2.GameDataType }).Invoke(value2, new object[1] { gameData });
			}
		}
	}

	public void OnAfterDeserialize()
	{
		GameDatas.Clear();
		foreach (MFG.Types.EventType value in Enum.GetValues(typeof(MFG.Types.EventType)))
		{
			Item item = EventTakuMaster.GetItem(value);
			if (item == null)
			{
				continue;
			}
			foreach (BaseEventGameData item2 in (IEnumerable<object>)GetType().GetField(item.GameDataFieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this))
			{
				GameDatas.Add(item2);
			}
		}
		RefleshCurrentEvent();
	}
}

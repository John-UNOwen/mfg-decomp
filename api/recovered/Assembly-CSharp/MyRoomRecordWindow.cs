using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Competition;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomRecordWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject _recordTaikyoku;

	[SerializeField]
	private GameObject _recordTournament;

	[SerializeField]
	private GameObject _recordScramble;

	private GameObject _tournamentPopUp;

	[SerializeField]
	private List<Button> _tabButtons = new List<Button>();

	[SerializeField]
	private List<GameObject> _graphObj = new List<GameObject>();

	[SerializeField]
	private DanBanner _danBanner;

	[SerializeField]
	private GameObject _dani;

	[SerializeField]
	private TextMeshProUGUI _fightStarText;

	[SerializeField]
	private List<GameObject> _recRankNum;

	[SerializeField]
	private TextMeshProUGUI _averageRank;

	[SerializeField]
	private TextMeshProUGUI _yakumanNum;

	[SerializeField]
	private Transform _historyParent;

	[SerializeField]
	private GameObject _historyPrefabON;

	[SerializeField]
	private GameObject _historyPrefabOFF;

	[SerializeField]
	private GameObject _recordTournamentPrefab;

	[SerializeField]
	private Transform _recordTournamentParent;

	[SerializeField]
	private GameObject _recordScramblePrefab;

	[SerializeField]
	private Transform _recordScrambleParent;

	private CompetitionScoreWindow _competitionScoreWindow;

	private bool _isCompetitionScoreWindowOpen;

	[SerializeField]
	private Image _tournamentPopupBanner;

	[SerializeField]
	private GameObject _graphLinePrefab;

	[SerializeField]
	private GameObject _graphLineParent4;

	[SerializeField]
	private GameObject _graphLineParent3;

	[SerializeField]
	private List<Sprite> _markSprites;

	[NonSerialized]
	private RecordKind _nowCategory;

	private List<string> _rankColorCode = new List<string> { "#00F1FF", "#00FF09", "#FFE400", "#FF0000" };

	private AccountGameData AccountGameData => GameDataManager.Account;

	public bool IsOpen { get; private set; }

	public bool IsRefleshed { get; private set; } = true;


	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		try
		{
			this.GetCancellationTokenOnDestroy();
			foreach (RecordKind category in Enum.GetValues(typeof(RecordKind)))
			{
				if (category == RecordKind.Nima && GameDataManager.Player.IsPro())
				{
					_tabButtons[(int)category].gameObject.SetActive(value: false);
					continue;
				}
				_tabButtons[(int)category].OnClickAsObservable().Subscribe(delegate
				{
					if (IsRefleshed)
					{
						IsRefleshed = false;
						SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
						ChangeTab(category);
					}
				}).AddTo(this);
				_tabButtons[(int)category].transform.Find("BtOn").gameObject.SetActive(category == _nowCategory);
				_tabButtons[(int)category].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(category == _nowCategory);
			}
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	public void Init(GameObject popupParent)
	{
		_tournamentPopUp = popupParent;
		foreach (RecordKind value in Enum.GetValues(typeof(RecordKind)))
		{
			CreateRecord(value);
		}
		ChangeTab(RecordKind.Yonma);
	}

	public void ChangeTab(RecordKind category)
	{
		CreateRecord(category);
		_tabButtons[(int)_nowCategory].transform.Find("BtOn").gameObject.SetActive(value: false);
		_tabButtons[(int)_nowCategory].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: false);
		_tabButtons[(int)category].transform.Find("BtOn").gameObject.SetActive(value: true);
		_tabButtons[(int)category].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: true);
		_nowCategory = category;
		IsRefleshed = true;
	}

	public void CreateRecord(RecordKind category)
	{
		switch (category)
		{
		case RecordKind.Tournament:
			CreateCompetitionRecord();
			_recordTournament.SetActive(value: true);
			_recordScramble.SetActive(value: false);
			_recordTaikyoku.SetActive(value: false);
			break;
		case RecordKind.ScrambleMatch:
			_recordTournament.SetActive(value: false);
			CreateScrambleRecord();
			_recordScramble.SetActive(value: true);
			_recordTaikyoku.SetActive(value: false);
			break;
		default:
			_recordTournament.SetActive(value: false);
			_recordTaikyoku.SetActive(value: false);
			_recordScramble.SetActive(value: false);
			CreateTaikyokuRecord(category);
			_recordTaikyoku.SetActive(value: true);
			break;
		}
	}

	private void CreateScrambleRecord()
	{
		CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
		foreach (Transform item in _recordScrambleParent)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (SchoolID value in Enum.GetValues(typeof(SchoolID)))
		{
			if (value != 0 && (value != SchoolID.School005 || GameDataManager.IsCharaEnable(CharaType.Chara12)) && (value != SchoolID.School009 || GameDataManager.IsCharaEnable(CharaType.Chara14)) && (value != SchoolID.School004 || GameDataManager.IsCharaEnable(CharaType.Chara15)) && (value != SchoolID.School010 || GameDataManager.IsCharaEnable(CharaType.Chara19)))
			{
				ScramblePanel_Init(UnityEngine.Object.Instantiate(_recordScramblePrefab, _recordScrambleParent), value, cancellationTokenOnDestroy).Forget();
			}
		}
	}

	private async UniTask ScramblePanel_Init(GameObject obj, SchoolID school, CancellationToken cancellationToken = default(CancellationToken))
	{
		string path = AssetPathResolver.SchoolEmblemIcon.ResolveBaseImage(school);
		if (await AssetLoader.Exists(path))
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(path);
			disposableAsset.AddTo(this);
			obj.transform.Find("Banner").GetComponent<Image>().sprite = disposableAsset.Asset;
		}
		obj.transform.Find("SchoolName").GetComponent<TextMeshProUGUI>().text = SchoolMaster.GetItem(school).Name;
		Dictionary<SchoolID, int> scrambleMatchTotalWinCount = GameDataManager.PlayerRecord.GetScrambleMatchTotalWinCount();
		int num = (scrambleMatchTotalWinCount.ContainsKey(school) ? scrambleMatchTotalWinCount[school] : 0);
		obj.transform.Find("Num").GetComponent<TextMeshProUGUI>().text = num.ToString();
		string pathTrophy = AssetPathResolver.ScrambleMatch.GetScrambleTrophyPath(school, GameDataManager.PlayerRecord.GetTrophyRank(school));
		if (await AssetLoader.Exists(pathTrophy))
		{
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(pathTrophy);
			disposableAsset2.AddTo(this);
			obj.transform.Find("Trophy").Find("Icon").GetComponent<Image>()
				.sprite = disposableAsset2.Asset;
			obj.transform.Find("Trophy").Find("Icon").gameObject.SetActive(value: true);
		}
	}

	private void CreateCompetitionRecord()
	{
		CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
		foreach (Transform item in _recordTournamentParent)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (CompetitionInfo item2 in GameDataManager.Competition.CompetitionInfos.OrderByDescending((CompetitionInfo a) => a.End).ToList())
		{
			RecordPanel_Init(UnityEngine.Object.Instantiate(_recordTournamentPrefab, _recordTournamentParent), item2, cancellationTokenOnDestroy).Forget();
		}
	}

	private async UniTask RecordPanel_Init(GameObject recordPanelInstance, CompetitionInfo competitionInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		string path = AssetPathResolver.ResolveCompetitionAssets(competitionInfo.CompetitionID, CompetitionAssetType.Title);
		if (await AssetLoader.Exists(path))
		{
			recordPanelInstance.transform.Find("Time").Find("Text").GetComponent<TextMeshProUGUI>()
				.text = competitionInfo.Start.ToString("yyyy/MM/dd HH:mm") + " ～ " + competitionInfo.End.ToString("yyyy/MM/dd HH:mm");
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(path);
			disposableAsset.AddTo(this);
			Image component = recordPanelInstance.transform.Find("Banner").GetComponent<Image>();
			component.sprite = disposableAsset.Asset;
			component.enabled = true;
		}
		Button component2 = recordPanelInstance.transform.Find("Touch").GetComponent<Button>();
		if (competitionInfo.IsEntried)
		{
			component2.gameObject.SetActive(value: true);
			component2.OnClickAsObservable().Subscribe(async delegate
			{
				_ = 2;
				try
				{
					if (!_isCompetitionScoreWindowOpen)
					{
						_isCompetitionScoreWindowOpen = true;
						IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>("Competition/Common/Prefabs/CompeScoreWindow.prefab");
						obj.AddTo(this);
						GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, _tournamentPopUp.transform, worldPositionStays: false);
						_competitionScoreWindow = gameObject.GetComponent<CompetitionScoreWindow>();
						_tournamentPopUp.SetActive(value: true);
						await _competitionScoreWindow.OpenAsScoreView(competitionInfo, isDetailButtonActive: false, isFinalResult: false, cancellationToken);
						await UniTask.WaitWhile(() => _competitionScoreWindow.IsOpen, PlayerLoopTiming.Update, cancellationToken);
						_isCompetitionScoreWindowOpen = false;
					}
				}
				catch (OperationCanceledException)
				{
				}
			}).AddTo(this);
		}
		else
		{
			component2.gameObject.SetActive(value: false);
		}
		if (competitionInfo.CompetitionStatus != CompetitionStatus.Result && competitionInfo.CompetitionStatus != CompetitionStatus.AfterEnd)
		{
			return;
		}
		TrophyType trophyType = competitionInfo.TrophyType;
		if (trophyType != 0)
		{
			string pathTrophy = AssetPathResolver.ResolveCompetitionTrophyIconAssets(competitionInfo.CompetitionID, trophyType);
			if (await AssetLoader.Exists(pathTrophy))
			{
				IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(pathTrophy);
				disposableAsset2.AddTo(this);
				recordPanelInstance.transform.Find("Trophy").Find("Icon").GetComponent<Image>()
					.sprite = disposableAsset2.Asset;
				recordPanelInstance.transform.Find("Trophy").Find("Icon").gameObject.SetActive(value: true);
			}
		}
	}

	public void CreateTaikyokuRecord(RecordKind category)
	{
		List<PlayerRecordInfo> playerRecordInfos = GameDataManager.PlayerRecord.PlayerRecordInfos;
		int fightStar = GameDataManager.Player.FightStar;
		int item = DanLevelMaster.GetCurrentDanLevelAndFightStarBorder(fightStar).minFightStarAtCurrentDanLevel;
		int item2 = DanLevelMaster.GetCurrentDanLevelAndFightStarBorder(fightStar).maxFightStarAtCurrentDanLevel;
		DanLevelType item3 = DanLevelMaster.GetCurrentDanLevelAndFightStarBorder(fightStar).currentDanLevel;
		_danBanner.SetDanBanner(_dani.transform, item3);
		_fightStarText.text = ((item3 != DanLevelType.Max) ? (fightStar - item + " / " + (item2 - item)) : "- / -");
		int num = 4;
		int num2 = 0;
		int num3 = 0;
		switch (category)
		{
		case RecordKind.Sanma:
			num = 3;
			break;
		case RecordKind.Nima:
			num = 2;
			break;
		}
		_recRankNum[2].SetActive(num > 2);
		_recRankNum[3].SetActive(num > 3);
		PlayerRecordInfo playerRecordInfo = playerRecordInfos.Find((PlayerRecordInfo x) => x.RecordKind == category);
		if (playerRecordInfo != null)
		{
			for (int i = 0; i < num; i++)
			{
				_recRankNum[i].transform.Find("Score").GetComponent<TextMeshProUGUI>().text = playerRecordInfo.RankBreakDownList[i] + "<size=20>回</size>";
				num2 += playerRecordInfo.RankBreakDownList[i] * (i + 1);
				num3 += playerRecordInfo.RankBreakDownList[i];
			}
		}
		_averageRank.text = ((num3 <= 0) ? "―.――<size=20>位</size>" : (((float)num2 / (float)num3).ToString("0.00") + "<size=20>位</size>"));
		if (playerRecordInfo != null)
		{
			_yakumanNum.text = playerRecordInfo.YakumanCount + "<size=20>回</size>";
		}
		for (int j = 0; j < _graphObj.Count(); j++)
		{
			_graphObj[j].SetActive(value: false);
		}
		if (category != RecordKind.Tournament && category != RecordKind.ScrambleMatch)
		{
			_graphObj[(int)category].SetActive(value: true);
		}
		foreach (Transform item4 in _historyParent)
		{
			UnityEngine.Object.Destroy(item4.gameObject);
		}
		if (playerRecordInfo == null)
		{
			return;
		}
		foreach (Transform item5 in ((playerRecordInfo.RecordKind == RecordKind.Yonma) ? _graphLineParent4 : _graphLineParent3).transform)
		{
			if (item5.name.Contains("GraphBlock"))
			{
				UnityEngine.Object.Destroy(item5.gameObject);
			}
		}
		int k = 1;
		if (playerRecordInfo.Histories.Count > 0)
		{
			foreach (PlayerRecordHistory history in playerRecordInfo.Histories)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_historyPrefabON, _historyParent);
				gameObject.transform.Find("IndexText").GetComponent<TextMeshProUGUI>().text = k.ToString();
				gameObject.transform.Find("Rank").Find("RankIndexText").GetComponent<TextMeshProUGUI>()
					.text = (int)(history.HistoryRank + 1) + "<size=20>位</size>";
				gameObject.transform.Find("Rank").Find("RankIndexText").GetComponent<TextMeshProUGUI>()
					.color = GetRankColor(history.HistoryRank);
				gameObject.transform.Find("Final").Find("FinalIndexText").GetComponent<TextMeshProUGUI>()
					.text = history.Score + "<size=20>点</size>";
				gameObject.transform.Find("Final").Find("FinalIndexText").GetComponent<TextMeshProUGUI>()
					.color = GetRankColor(history.HistoryRank);
				Sprite markSprite = GetMarkSprite(history.GetMark(playerRecordInfo.RecordKind));
				if (markSprite != null)
				{
					gameObject.transform.Find("Icon").Find("IndexImage").GetComponent<Image>()
						.sprite = markSprite;
				}
				else
				{
					gameObject.transform.Find("Icon").Find("IndexImage").gameObject.SetActive(value: false);
				}
				gameObject.transform.Find("Play").Find("PlayIndexText").GetComponent<TextMeshProUGUI>()
					.text = history.PlayDate;
				gameObject.transform.Find("Play").Find("PlayIndexText").GetComponent<TextMeshProUGUI>()
					.color = GetRankColor(history.HistoryRank);
				k++;
			}
		}
		if (k <= 10)
		{
			for (; k <= 10; k++)
			{
				UnityEngine.Object.Instantiate(_historyPrefabOFF, _historyParent);
			}
		}
		if (category == RecordKind.Yonma || category == RecordKind.Sanma)
		{
			for (int num4 = playerRecordInfo.Histories.Count() - 1; num4 >= 0; num4--)
			{
				Sprite markSprite2 = GetMarkSprite(playerRecordInfo.Histories[num4].GetMark(category));
				GraphBlockChangeActive(playerRecordInfo.RecordKind, num4, playerRecordInfo.Histories[num4], (num4 >= 1) ? playerRecordInfo.Histories[num4 - 1].HistoryRank : Rank.RankMax, markSprite2);
			}
		}
	}

	private void GraphBlockChangeActive(RecordKind kind, int count, PlayerRecordHistory history, Rank nextRank, Sprite spr = null)
	{
		if (kind <= RecordKind.Sanma)
		{
			GameObject gameObject = ((kind == RecordKind.Yonma) ? _graphLineParent4 : _graphLineParent3);
			Vector2 vector = default(Vector2);
			vector = ((kind != 0) ? new Vector2(335 - 65 * count, -65 * (int)history.HistoryRank + 71) : new Vector2(585 - 65 * count, -65 * (int)history.HistoryRank));
			GameObject obj = UnityEngine.Object.Instantiate(_graphLinePrefab, gameObject.transform);
			obj.GetComponent<RectTransform>().anchoredPosition = vector;
			obj.GetComponent<GraphBlockManager>().SetGraph(spr, nextRank, history.HistoryRank);
		}
	}

	private Sprite GetMarkSprite(RecordMark mark)
	{
		if (mark == RecordMark.None)
		{
			return null;
		}
		return _markSprites[(int)mark];
	}

	private Color GetRankColor(Rank rank)
	{
		if (ColorUtility.TryParseHtmlString(_rankColorCode[(int)rank], out var color))
		{
			return color;
		}
		return new Color(1f, 1f, 1f, 1f);
	}
}

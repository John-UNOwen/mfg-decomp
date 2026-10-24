using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.GameData.EventTaku;
using MFG.Scenes.Home;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HomeEventSelectWindow : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _headerText;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private PlayRuleWindow _ruleWindow;

	[SerializeField]
	private HomeFightStarRateSelectWindow _fightStarRateSelectWindow;

	[SerializeField]
	private List<GameObject> _modePanelInstanceParents = new List<GameObject>();

	private List<HomeModePanel> _modePanels = new List<HomeModePanel>();

	private ModeSelectWindowType _modeSelectWindowType;

	private bool _isInitialized;

	private bool _isFightStarRateDecided;

	private bool _isModeDecided;

	private TakuType _decidedTakuType;

	private MFG.Types.EventType _decidedEventType;

	private bool _isOpen;

	private Subject<(TakuType, MFG.Types.EventType, GainLossType)> m_onDecided = new Subject<(TakuType, MFG.Types.EventType, GainLossType)>();

	private Subject<Unit> m_onClose = new Subject<Unit>();

	public bool IsOpen => _isOpen;

	public IObservable<(TakuType, MFG.Types.EventType, GainLossType)> OnDecided => m_onDecided.AsObservable();

	public IObservable<Unit> OnClose => m_onClose.AsObservable();

	private async void Start()
	{
		try
		{
			this.GetCancellationTokenOnDestroy();
			await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
			_closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
			_fightStarRateSelectWindow.OnDecided.Subscribe(delegate(GainLossType gainLossType)
			{
				if (!_isFightStarRateDecided)
				{
					_isFightStarRateDecided = true;
					m_onDecided.OnNext((_decidedTakuType, _decidedEventType, gainLossType));
				}
			}).AddTo(this);
			ResidentFlow.GetInstance().GetTimer().OnEnd.Where((Unit _) => ResidentFlow.IsInitialized && _isInitialized && _isOpen).Subscribe(delegate
			{
				if (_isModeDecided && IsNeedFightStarRateSelectTransition(_decidedTakuType, _decidedEventType) && !_isFightStarRateDecided)
				{
					_fightStarRateSelectWindow.TimeupForceDecide();
				}
				else if (!_isModeDecided)
				{
					TimeupForceDecide();
				}
			}).AddTo(this);
			_isInitialized = true;
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	public async UniTask Open(ModeSelectWindowType modeSelectWindowType, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_isOpen)
		{
			_isOpen = true;
			_modeSelectWindowType = modeSelectWindowType;
			_isFightStarRateDecided = false;
			_isModeDecided = false;
			_decidedTakuType = TakuType.Tonpu;
			_decidedEventType = MFG.Types.EventType.None;
			HeaderText_Reflesh();
			await ModePanel_Create(modeSelectWindowType, cancellationToken);
			base.gameObject.SetActive(value: true);
			ModePanel_Reflesh();
			GetComponent<Animator>().SetTrigger("In");
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		}
	}

	public async UniTask OpenContinue(ModeSelectWindowType modeSelectWindowType, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_isOpen)
		{
			_isOpen = true;
			_modeSelectWindowType = modeSelectWindowType;
			_isFightStarRateDecided = false;
			_isModeDecided = true;
			_decidedTakuType = TakuType.Tonpu;
			_decidedEventType = MFG.Types.EventType.None;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			GameDataManager.Mahjong.IsPrivateMatch = GameDataManager.PlayerConfig.IsPrivateMatch;
			GameDataManager.Mahjong.TaikyokuItems.Clear();
			ResidentFlow.GetInstance().GetTimer().SetTimer(10f, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			await _fightStarRateSelectWindow.Open(GameDataManager.Mahjong.TakuType, cancellationToken);
		}
	}

	public void Close()
	{
		if (_isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			GetComponent<Animator>().SetTrigger("Out");
			m_onClose.OnNext(Unit.Default);
			_isOpen = false;
		}
	}

	public void CloseButton_SetActive(bool isActive)
	{
		_closeButton.gameObject.SetActive(isActive);
	}

	private async UniTask ModePanel_Create(ModeSelectWindowType modeSelectWindowType, CancellationToken cancellationToken)
	{
		List<(HomeModePanelType, bool, bool, TakuType)> eventHomeModePanelList = GetEventHomeModePanelList(modeSelectWindowType);
		foreach (GameObject modePanelInstanceParent in _modePanelInstanceParents)
		{
			foreach (Transform item2 in modePanelInstanceParent.transform)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
		_modePanels.Clear();
		foreach (var panel in eventHomeModePanelList.Select(((HomeModePanelType, bool, bool, TakuType) panelInfo, int index) => new { panelInfo, index }))
		{
			HomeModePanelType item = panel.panelInfo.Item1;
			bool isConstancyTaku = panel.panelInfo.Item2;
			bool isCompetitionTaku = panel.panelInfo.Item3;
			if (!GameDataManager.Player.IsPro() || item != HomeModePanelType.Nima)
			{
				IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.ResolveHomeModeSelectPanel(item, panel.panelInfo.Item4));
				obj.AddTo(this);
				GameObject obj2 = UnityEngine.Object.Instantiate(obj.Asset, _modePanelInstanceParents[panel.index].transform, worldPositionStays: false);
				obj2.name = "ModeBanner";
				HomeModePanel component = obj2.GetComponent<HomeModePanel>();
				component.IsConstancyTaku = isConstancyTaku;
				component.IsCompetitionTaku = isCompetitionTaku;
				component.OnDecided.Subscribe(async delegate((TakuType, MFG.Types.EventType, bool, bool) value)
				{
					await ModePanel_Decide(value.Item1, value.Item2, value.Item3, value.Item4, cancellationToken);
				}).AddTo(component);
				component.OnRuleButton.Subscribe(delegate(PlayRuleType playRuleType)
				{
					_ruleWindow.Open(playRuleType);
				}).AddTo(component);
				_modePanels.Add(component);
			}
		}
	}

	private void ModePanel_Reflesh()
	{
		foreach (HomeModePanel modePanel in _modePanels)
		{
			modePanel.Reflesh();
		}
	}

	private async UniTask ModePanel_Decide(TakuType takuType, MFG.Types.EventType eventType, bool isConstancyTaku, bool isComepetitionTaku, CancellationToken cancellationToken)
	{
		if (!_isModeDecided)
		{
			_isModeDecided = true;
			GameDataManager.Mahjong.TakuType = takuType;
			GameDataManager.Mahjong.EventType = eventType;
			GameDataManager.Mahjong.IsConstancyTaku = isConstancyTaku;
			GameDataManager.Mahjong.IsPrivateMatch = GameDataManager.PlayerConfig.IsPrivateMatch;
			GameDataManager.Mahjong.IsCompetitionTaku = isComepetitionTaku;
			Debug.Log($"<color=green>ModePanel_Decide : {takuType} / {eventType} / isConstancy ? {isConstancyTaku} / isCompetition ? {isComepetitionTaku} / isPrivate ? {GameDataManager.PlayerConfig.IsPrivateMatch}</color>");
			GameDataManager.Mahjong.TaikyokuItems.Clear();
			GameDataManager.PlayerConfig.IsEventTakuSelected = eventType != MFG.Types.EventType.None;
			GameDataManager.PlayerConfig.LatestSelectedEventType = eventType;
			ResidentFlow.GetInstance().GetTimer().SetTimer(10f, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			if (IsNeedFightStarRateSelectTransition(takuType, eventType))
			{
				await _fightStarRateSelectWindow.Open(takuType, cancellationToken);
			}
			else
			{
				m_onDecided.OnNext((_decidedTakuType, _decidedEventType, GainLossType.Risky));
			}
		}
	}

	private void HeaderText_Reflesh()
	{
		_headerText.text = ((_modeSelectWindowType == ModeSelectWindowType.Normal) ? "対局モードを選んでください" : "対局モードを選んでください\n※イベント対局は戦績対象外となります");
	}

	private bool IsNeedFightStarRateSelectTransition(TakuType takuType, MFG.Types.EventType eventType)
	{
		if (GameDataManager.PlayerConfig.IsPrivateMatch)
		{
			return false;
		}
		if (takuType == TakuType.Nima)
		{
			return false;
		}
		return true;
	}

	private void TimeupForceDecide()
	{
		if (_isInitialized)
		{
			_modePanels[0].TimeupForceDecide();
		}
	}

	private static List<(HomeModePanelType, bool, bool, TakuType)> GetEventHomeModePanelList(ModeSelectWindowType mode)
	{
		if (mode == ModeSelectWindowType.Normal)
		{
			return new List<(HomeModePanelType, bool, bool, TakuType)>
			{
				(HomeModePanelType.Yonma, false, false, (TakuType)(-1)),
				(HomeModePanelType.Sanma, false, false, (TakuType)(-1)),
				(HomeModePanelType.Nima, false, false, (TakuType)(-1))
			};
		}
		List<(HomeModePanelType, bool, bool, TakuType)> list = new List<(HomeModePanelType, bool, bool, TakuType)>();
		if (mode == ModeSelectWindowType.Event)
		{
			foreach (BaseEventGameData item in GameDataManager.EventTaku.AvailableModeGameDatas())
			{
				if (item.Master.EnableTakuType[0])
				{
					list.Add((item.Master.HomeModePanelType, false, false, TakuType.Tonpu));
				}
				if (item.Master.EnableTakuType[1])
				{
					list.Add((item.Master.HomeModePanelTypeSanma, false, false, TakuType.Sanma));
				}
			}
			if (!GameDataManager.EventTaku.AvailableModeGameDatas().Any())
			{
				foreach (BaseEventGameData item2 in GameDataManager.EventTaku.CompetitionTakuAvailables())
				{
					if (item2.Master.EnableTakuType[0])
					{
						list.Add((item2.Master.HomeModePanelType, false, true, TakuType.Tonpu));
					}
					if (item2.Master.EnableTakuType[1])
					{
						list.Add((item2.Master.HomeModePanelTypeSanma, false, true, TakuType.Sanma));
					}
				}
				if (list == null || list.Count <= 0)
				{
					foreach (BaseEventGameData item3 in GameDataManager.EventTaku.ConstancyTakuAvailables())
					{
						list.Add((item3.Master.HomeModePanelTypeConstancy, true, false, (TakuType)(-1)));
					}
				}
			}
		}
		return list;
	}
}

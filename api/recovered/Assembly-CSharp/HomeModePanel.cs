using System;
using MFG.GameData;
using MFG.Scenes.Home;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HomeModePanel : MonoBehaviour
{
	[SerializeField]
	private TakuType _takuType;

	[SerializeField]
	private MFG.Types.EventType _eventType;

	[SerializeField]
	private PlayRuleType _playRuleType;

	[SerializeField]
	private Button _startButton;

	[SerializeField]
	private Button _ruleButton;

	[SerializeField]
	private GameObject _proStayBanner;

	[SerializeField]
	private GameObject _competitionIcon;

	private Subject<(TakuType, MFG.Types.EventType, bool, bool)> m_onDecided = new Subject<(TakuType, MFG.Types.EventType, bool, bool)>();

	private Subject<PlayRuleType> m_onRuleButton = new Subject<PlayRuleType>();

	[NonSerialized]
	public bool IsConstancyTaku;

	[NonSerialized]
	public bool IsCompetitionTaku;

	public IObservable<(TakuType, MFG.Types.EventType, bool, bool)> OnDecided => m_onDecided.AsObservable();

	public IObservable<PlayRuleType> OnRuleButton => m_onRuleButton.AsObservable();

	private void Start()
	{
		_startButton.OnClickAsObservable().Subscribe(delegate
		{
			m_onDecided.OnNext((_takuType, _eventType, IsConstancyTaku, IsCompetitionTaku));
		}).AddTo(this);
		_ruleButton.OnClickAsObservable().Subscribe(delegate
		{
			m_onRuleButton.OnNext(_playRuleType);
		}).AddTo(this);
	}

	private void Update()
	{
		if (_takuType == TakuType.Nima)
		{
			_startButton.interactable = !GameDataManager.PlayerConfig.IsPrivateMatch;
		}
	}

	public void Reflesh()
	{
		CompetitionIcon_Reflesh();
		ProBanner_Reflesh();
	}

	public void TimeupForceDecide()
	{
		_startButton.onClick.Invoke();
		_startButton.transition = Selectable.Transition.None;
		_startButton.GetComponent<Animator>().SetTrigger("Selected");
	}

	private void ProBanner_Reflesh()
	{
		_proStayBanner.SetActive(_takuType != TakuType.Nima && GameDataManager.IsProStay(_takuType, _eventType));
	}

	private void CompetitionIcon_Reflesh()
	{
		CompetitionInfo inProgressEntriedCompetitionInfo = GameDataManager.Competition.GetInProgressEntriedCompetitionInfo();
		bool flag = inProgressEntriedCompetitionInfo?.IsEntried ?? false;
		bool isInTime = inProgressEntriedCompetitionInfo?.IsInTime ?? false;
		bool flag2 = Competition_IsAvailable(inProgressEntriedCompetitionInfo, _takuType, _eventType);
		CompetitionIcon_SetActive(flag && flag2, isInTime);
	}

	private bool Competition_IsAvailable(CompetitionInfo competitionInfo, TakuType takuType, MFG.Types.EventType eventType)
	{
		return competitionInfo?.IsValid(takuType, eventType, isConsiderIsInTime: false) ?? false;
	}

	private void CompetitionIcon_SetActive(bool isActive, bool isInTime)
	{
		_competitionIcon.SetActive(isActive);
		if (isActive)
		{
			Animator component = _competitionIcon.GetComponent<Animator>();
			_competitionIcon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = (isInTime ? "<size=24>大会対象モード" : "<size=24>大会対象モード\n【対象時間外】");
			component.Update(0f);
			component.SetTrigger(isInTime ? "In" : "Darak");
		}
	}
}

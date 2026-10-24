using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types.ScrambleFestival;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrambleTeamMatchHomeWindow : MonoBehaviour
{
	[SerializeField]
	protected Animator _windowAnimator;

	[SerializeField]
	protected Button _closeButton;

	[SerializeField]
	protected TextMeshProUGUI _dateText;

	[SerializeField]
	protected Transform _teamScoreInfoParent;

	[SerializeField]
	protected TeamChoiceWindow _teamChoiceWindow;

	[SerializeField]
	protected Button _choiceWindowOpenButton;

	[SerializeField]
	protected Animator _evolutionWindow;

	[SerializeField]
	protected Animator _evolutionAnimator;

	[SerializeField]
	protected CatchphrasePlate _beforeCatchphrase;

	[SerializeField]
	protected CatchphrasePlate _afterCatchphrase;

	[SerializeField]
	protected GameObject _infoObj;

	protected bool _isOpen;

	private bool _isResult;

	private bool _isNotice;

	public virtual bool IsOpen => _isOpen;

	protected virtual void Start()
	{
		if (_closeButton != null)
		{
			_closeButton.OnClickAsObservable().Subscribe(async delegate
			{
				await Close();
			}).AddTo(this);
		}
		if (_choiceWindowOpenButton != null)
		{
			_choiceWindowOpenButton.OnClickAsObservable().Subscribe(delegate
			{
				_teamChoiceWindow.Open();
			}).AddTo(this);
		}
		if (_dateText != null)
		{
			_dateText.text = GameDataManager.ScrambleFestival.CurrentInfo.Master.DispDate();
		}
	}

	public virtual async UniTask Open(CatchphrasePlate homeCatchphrasePlate, bool isNotice, bool isResult = false)
	{
		if (_isOpen)
		{
			return;
		}
		_closeButton.gameObject.SetActive(value: false);
		_isOpen = true;
		_isResult = isResult;
		_dateText.gameObject.SetActive(!_isResult);
		_infoObj.SetActive(!_isResult);
		_isNotice = isNotice;
		if (_isNotice)
		{
			await Reflesh(homeCatchphrasePlate);
		}
		else
		{
			Reflesh(homeCatchphrasePlate).Forget();
		}
		base.gameObject.SetActive(value: true);
		SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
		if (_windowAnimator != null)
		{
			_windowAnimator.SetTrigger("In");
		}
		if (_isNotice)
		{
			List<UniTask> list = new List<UniTask>();
			foreach (Transform item in _teamScoreInfoParent)
			{
				ScrambleTeamMatchScoreInfo component = item.GetComponent<ScrambleTeamMatchScoreInfo>();
				if (component != null)
				{
					list.Add(component.ScoreNoticeFlow(_isResult));
				}
			}
			await list;
			if (_isResult)
			{
				List<TeamType> obj = GameDataManager.ReceiveOnly.ScrambleFestivalScoreInfos?.Find((ScrambleFestivalInfo x) => x.EventType == GameEventType.Janpon4)?.WinnerTeam;
				ScrambleFestivalEventInfo currentInfo = GameDataManager.ScrambleFestival.CurrentInfo;
				TeamType team = currentInfo.Team;
				if (obj?.Contains(currentInfo.Team) ?? false)
				{
					await new List<UniTask>
					{
						_beforeCatchphrase.Reflesh(currentInfo.Master.CatchPhrases[team].Normal),
						_afterCatchphrase.Reflesh(currentInfo.Master.CatchPhrases[team].Extra)
					};
					_evolutionWindow.gameObject.SetActive(value: true);
					_evolutionWindow.SetTrigger("In");
					await UniTask.Delay(TimeSpan.FromSeconds(0.5));
					_evolutionAnimator.SetTrigger("In");
					await UniTask.Delay(TimeSpan.FromSeconds(7.0));
					_evolutionWindow.SetTrigger("Out");
					_evolutionWindow.gameObject.SetActive(value: false);
				}
			}
		}
		_closeButton.gameObject.SetActive(value: true);
	}

	protected virtual void Update()
	{
		try
		{
			if (_isNotice)
			{
				_choiceWindowOpenButton.gameObject.SetActive(value: false);
			}
			else
			{
				_choiceWindowOpenButton.gameObject.SetActive(GameDataManager.ScrambleFestival.CurrentInfo != null && GameDataManager.ScrambleFestival.CurrentInfo.Team == TeamType.None);
			}
		}
		catch
		{
		}
	}

	public virtual async UniTask Close()
	{
		if (_isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("Out");
			}
			_isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	public virtual async UniTask Reflesh(CatchphrasePlate homeCatchphrasePlate)
	{
		_teamChoiceWindow._themeChoiceText.text = GameDataManager.ScrambleFestival.CurrentInfo.Master.Theme;
		_teamChoiceWindow.Init(homeCatchphrasePlate);
		List<UniTask> list = new List<UniTask>();
		foreach (Transform item in _teamScoreInfoParent)
		{
			ScrambleTeamMatchScoreInfo component = item.GetComponent<ScrambleTeamMatchScoreInfo>();
			if (component != null)
			{
				list.Add(component.Reflesh(_isNotice, _isResult));
			}
		}
		await list;
	}
}

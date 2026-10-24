using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Keyboard;
using Konami.System.Net;
using MFG.GameRequest;
using MFG.Types.Story;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class NewRegistrationFlow : BaseFlow
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private KeyboardInputField keyboardInputField;

	[SerializeField]
	private KeyboardInputModel keyboardInputModel;

	[SerializeField]
	private GameObject confirmWindow;

	[SerializeField]
	private TextMeshProUGUI decideName;

	[SerializeField]
	private Button yesButton;

	[SerializeField]
	private Button noButton;

	[SerializeField]
	private GameObject _NGWordWindow;

	[SerializeField]
	private GameObject _confirmInfoText;

	[SerializeField]
	private GameObject _confirmNGText;

	private bool m_isNameInputTimeUp;

	private readonly string _defaultName = "ファイトガール";

	[SerializeField]
	private GameObject _questionWindow;

	[SerializeField]
	private GameObject _questionThanksWindow;

	[SerializeField]
	private List<Button> _questionButtons = new List<Button>();

	[SerializeField]
	private Button _questionDecideButton;

	[SerializeField]
	private Button _questionSkipButton;

	private bool m_isNameEntryFinish;

	private bool m_isQuestionEntryFinish;

	private readonly int QuestionNum = 7;

	private List<bool> m_answers = new List<bool>();

	private bool _nameChecking;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken = default(CancellationToken))
	{
		SoundManager.PlayBgm("BGM_MENU").Forget();
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		await NameEntry_Exec();
		await QuestionEntry_Exec();
		await FinishScene();
	}

	private async UniTask NameEntry_Exec()
	{
		ResidentFlow.GetInstance().GetTimer().SetTimer(99f, disp: true);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		ResidentFlow.GetInstance().GetResidentBG().SetActive(value: true);
		yesButton.OnClickAsObservable().Subscribe(delegate
		{
			NameEntryFinish();
		}).AddTo(this);
		noButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			confirmWindow.SetActive(value: false);
			m_isNameInputTimeUp = false;
		}).AddTo(this);
		await UniTask.WaitUntil(() => m_isNameEntryFinish);
	}

	private async UniTask QuestionEntry_Exec()
	{
		ResidentFlow.GetInstance().GetTimer().SetTimer(30f, disp: true);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		for (int i = 0; i < QuestionNum; i++)
		{
			int _i = i;
			m_answers.Add(item: false);
			_questionButtons[i].OnClickAsObservable().Subscribe(delegate
			{
				if (m_answers[_i])
				{
					SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
					m_answers[_i] = false;
					_questionButtons[_i].GetComponent<Animator>().SetTrigger("Normal");
				}
				else
				{
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
					m_answers[_i] = true;
					_questionButtons[_i].GetComponent<Animator>().SetTrigger("Selected");
				}
			}).AddTo(this);
		}
		_questionDecideButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			for (int j = 0; j < QuestionNum; j++)
			{
				GameDataManager.PlayerConfig.QuestionDatas.Add(m_answers[j]);
			}
			_questionThanksWindow.SetActive(value: true);
			_questionThanksWindow.GetComponent<Animator>().SetTrigger("In");
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			await UniTask.Delay(TimeSpan.FromSeconds(2.0));
			m_isQuestionEntryFinish = true;
		}).AddTo(this);
		_questionSkipButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_isQuestionEntryFinish = true;
		}).AddTo(this);
		_questionWindow.SetActive(value: true);
		_questionWindow.GetComponent<Animator>().SetTrigger("In");
		await UniTask.WaitUntil(() => m_isQuestionEntryFinish);
	}

	protected override void OnUpdate()
	{
		if (!m_isNameEntryFinish)
		{
			if (m_isNameInputTimeUp)
			{
				if (ResidentFlow.GetInstance().GetTimer().IsEnd())
				{
					NameEntryFinish();
				}
			}
			else if (ResidentFlow.GetInstance().GetTimer().IsEnd() && !m_isNameInputTimeUp)
			{
				m_isNameInputTimeUp = true;
				ResidentFlow.GetInstance().GetTimer().StartTimer(6);
				CheckName(isTimeUp: true).Forget();
			}
		}
		else if (!m_isQuestionEntryFinish && ResidentFlow.GetInstance().GetTimer().IsEnd())
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_isQuestionEntryFinish = true;
		}
	}

	private async UniTask FinishScene()
	{
		CancellationToken token = this.GetCancellationTokenOnDestroy();
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		_canvas.GetComponent<GraphicRaycaster>().enabled = false;
		try
		{
			await GameDataManager.Version.Save();
			await GameDataManager.Player.Save(token);
			await GameDataManager.PlayerConfig.Save(token);
			await GameDataManager.Mission.Save(token);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
		catch (NetworkResponseException ex2)
		{
			GameDataManager.PlayerConfig.IsDataSaveFailed = true;
			Debug.Log("NetworkResponseException Catched. " + ex2.Message);
		}
		SoundManager.StopBgmWithFadeout().Forget();
		if (GameDataManager.Account.IsFirstPlay)
		{
			GameDataManager.Story.m_forcePlaying = true;
			GameDataManager.Story.m_playStoryScriptID = StoryID.StoryEvent0000;
			GotoNextScene("StoryPlay");
		}
		else
		{
			GotoNextScene("FirstCharaSelect");
		}
	}

	private void NameEntryFinish()
	{
		if (!m_isNameEntryFinish)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			GameDataManager.Player.PlayerName = decideName.text;
			GameDataManager.PlayerConfig.IsNameDecided = true;
			ResidentFlow.GetInstance().GetHeader().SetPlayerName(GameDataManager.Player.PlayerName);
			m_isNameEntryFinish = true;
		}
	}

	public void OnFinish()
	{
		CheckName().Forget();
	}

	private async UniTask CheckName(bool isTimeUp = false)
	{
		if (_nameChecking)
		{
			return;
		}
		_nameChecking = true;
		if (keyboardInputField.Text == "")
		{
			decideName.text = _defaultName;
		}
		else
		{
			decideName.text = keyboardInputField.Text;
		}
		ChkTabooword.ResultData resultData = await RequestApiBase<ChkTabooword, ChkTabooword.ResultData, ChkTabooword.ChkParam>.Send(new ChkTabooword.ChkParam
		{
			CheckString = decideName.text
		});
		if (isTimeUp)
		{
			if (!resultData.IsAvailable)
			{
				decideName.text = _defaultName;
				_confirmNGText.SetActive(value: true);
			}
			_confirmInfoText.SetActive(value: false);
			confirmWindow.SetActive(value: true);
			yesButton.gameObject.SetActive(value: false);
			noButton.gameObject.SetActive(value: false);
		}
		else if (resultData.IsAvailable)
		{
			confirmWindow.SetActive(value: true);
			m_isNameInputTimeUp = true;
		}
		else
		{
			_NGWordWindow.SetActive(value: true);
			await UniTask.Delay(1000);
			_NGWordWindow.SetActive(value: false);
		}
		_nameChecking = false;
	}
}

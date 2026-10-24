using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleDemoFlow : BaseTitleFlow
{
	[SerializeField]
	private Camera m_mainCamera;

	[SerializeField]
	private GameObject m_proStayBanner;

	private string _nextSceneName = "";

	private bool _isMahjongPlayStart;

	private MahjongPlayFlow _mahjongPlayFlow;

	private bool _isUnloadMahjongPlay;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken = default(CancellationToken))
	{
		ResidentFlow.GetInstance().m_isTitleDemoMahjongPlayAlive = true;
		ResidentFlow.GetInstance().IsTestModeEffective = false;
		TitleDemoUtility.GameEntryContext_ActivateIfGameEntryAcceptable();
		ResidentFlow.GetInstance().InsertCoinMessage.SetActive(value: false);
		try
		{
			SoundManager.SetDemoVolume();
			if (TitleDemoUtility.IsNeedAdvanceToPPErrorScene())
			{
				_isNextSceneDecided = true;
				ResidentFlow.GetInstance().LoadScene("ParticipationError");
				return;
			}
			GameDataManager.Mahjong.IsHaifuPlay = true;
			m_mainCamera.gameObject.SetActive(value: false);
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("MahjongPlay", LoadSceneMode.Additive);
			if (asyncOperation == null)
			{
				return;
			}
			await asyncOperation;
			_mahjongPlayFlow = GameObject.Find("SceneFlow").GetComponent<MahjongPlayFlow>();
			base.OnUpdateAsObservable.Where((Unit _) => _mahjongPlayFlow.m_isTaikyokuActive).Take(1).Subscribe(delegate
			{
				if (!_isNextSceneDecided && !_isUnloadMahjongPlay)
				{
					ResidentFlow.GetInstance().InsertCoinMessage.SetActive(value: true);
					ResidentFlow.GetInstance().IsTestModeEffective = true;
				}
			})
				.AddTo(this);
		}
		catch (OperationCanceledException)
		{
			Debug.Log("TitleDemo: OperationCanceledException Catched.");
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		_isMahjongPlayStart = true;
		await UniTask.WaitUntil(() => _mahjongPlayFlow.m_taikyokuEnd);
		if (!_isUnloadMahjongPlay)
		{
			ResidentFlow.GetInstance().IsTestModeEffective = false;
			_isUnloadMahjongPlay = true;
			await ResidentFlow.GetInstance().m_fadePanel.GetComponent<CanvasGroup>().DOFade(1f, 1f).ToAwaiter();
			await SceneManager.UnloadSceneAsync("MahjongPlay");
			GameDataManager.Mahjong.IsHaifuPlay = false;
			m_mainCamera.gameObject.SetActive(value: true);
			if (_nextSceneName == "Login")
			{
				TitleDemoUtility.OnPreEnterLoginFlow();
			}
			if (string.IsNullOrEmpty(_nextSceneName))
			{
				_nextSceneName = "Logo";
			}
			ResidentFlow.GetInstance().LoadScene(_nextSceneName);
		}
	}

	protected override void OnUpdate()
	{
		if (GameDataManager.Mahjong.IsHaifuPlayToExit)
		{
			GameDataManager.Mahjong.IsHaifuPlayToExit = false;
			GotoNextScene("Logo");
		}
		if (!_isNextSceneDecided && _isMahjongPlayStart && !_isUnloadMahjongPlay && UpdateCommonProcess() && GotoLoginIfPossible())
		{
			ResidentFlow.GetInstance().InsertCoinMessage.SetActive(value: false);
		}
	}

	private async void OnDestroy()
	{
		if (!_isUnloadMahjongPlay)
		{
			_isUnloadMahjongPlay = true;
			ResidentFlow.GetInstance().IsTestModeEffective = false;
			_mahjongPlayFlow.KikenRequenst();
			await UniTask.WaitUntil(() => _mahjongPlayFlow.m_taikyokuEnd);
			await SceneManager.UnloadSceneAsync("MahjongPlay");
			GameDataManager.Mahjong.IsHaifuPlay = false;
		}
		ResidentFlow.GetInstance().IsTestModeEffective = true;
		ResidentFlow.GetInstance().m_isTitleDemoMahjongPlayAlive = false;
		ResidentFlow.GetInstance().InsertCoinMessage.SetActive(value: false);
	}

	protected override bool GotoLoginIfPossible()
	{
		if (_isNextSceneDecided)
		{
			return true;
		}
		if (TitleDemoUtility.IsGameEntryAcceptable() && (TitleDemoUtility.IsGameEntryActionDetected(_isCoinInserted) || (GameUtility.IsKonastaOrNetcafeBehavior && ResidentFlow.CustomInput.GetMouseButtonDown(0))))
		{
			GotoNextScene("Login");
			return true;
		}
		return false;
	}

	protected override void GotoNextScene(string nextSceneName)
	{
		if (!_isNextSceneDecided)
		{
			_isNextSceneDecided = true;
			ResidentFlow.GetInstance().IsTestModeEffective = false;
			_nextSceneName = nextSceneName;
			_mahjongPlayFlow.KikenRequenst();
		}
	}
}

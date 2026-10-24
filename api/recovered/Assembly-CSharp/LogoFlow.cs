using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LogoFlow : BaseTitleFlow
{
	[SerializeField]
	private CanvasGroup m_konamiLogo;

	[SerializeField]
	private CanvasGroup m_eamusementLogo;

	[SerializeField]
	private CanvasGroup m_rsaLogo;

	[SerializeField]
	private CanvasGroup m_warning;

	[SerializeField]
	private float m_fadeDuration = 1f;

	[SerializeField]
	private RectTransform m_CanvasTransform;

	private bool _isCompensationNoticeEnd;

	private static bool m_isLogoSceneOnceDisplayed;

	public static bool IsLogoSceneOnceDisplayed
	{
		get
		{
			if (GameUtility.IsKamunityBootEnable())
			{
				return m_isLogoSceneOnceDisplayed;
			}
			return true;
		}
		private set
		{
			m_isLogoSceneOnceDisplayed = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_konamiLogo.alpha = 0f;
		m_eamusementLogo.alpha = 0f;
		m_rsaLogo.alpha = 0f;
		m_warning.alpha = 0f;
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		TitleDemoUtility.EACard_ResetDetectedCard();
		TitleDemoUtility.GameEntryContext_ActivateIfGameEntryAcceptable();
		LEDManager.Instance.AllowAnim(isAllow: true);
		LEDManager.Instance.SetLedColor(Color.white);
		if (GameUtility.IsIOEnable)
		{
			Bi2xInput.Instance.ICReaderOff();
			await UniTask.Delay(200);
			Bi2xInput.Instance.ICReaderOn();
			await UniTask.Delay(1000);
		}
		await GameDataManager.System.GameControlInfo.Modify(isForceModify: true, suppressException: true, isSendLog: false);
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		TitleDemoUtility.SendServerLogs().Forget();
		try
		{
			SoundManager.SetDemoVolume();
			GameDataManager.Reset();
			if (AppTerminateIfNeed() || GotoParticipationErrorIfNeed())
			{
				return;
			}
			GameUtility.OnBeginFirstGameFlow();
			ResidentFlow.GetInstance().SetInGameDisp(isDisp: true);
			ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetDisp(isLowerDisp: false, isUpperDisp: false);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				if (Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Play))
				{
					await DispCompensationNotice(Cloudstrap.UserEventType.Play, cancellationToken);
				}
				if (Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha1) || Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha5))
				{
					await DispCompensationNotice(Cloudstrap.UserEventType.Gacha1, cancellationToken);
				}
				_isCompensationNoticeEnd = true;
			}
			if (!GameUtility.IsKonastaOrNetcafeBehavior)
			{
				await m_warning.DOFade(1f, m_fadeDuration).ToAwaiter(cancellationToken);
				await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				await m_warning.DOFade(0f, m_fadeDuration).ToAwaiter(cancellationToken);
			}
			SoundManager.PlaySe("SFX_KONAMI").Forget();
			await m_konamiLogo.DOFade(1f, m_fadeDuration).ToAwaiter(cancellationToken);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			await m_konamiLogo.DOFade(0f, m_fadeDuration).ToAwaiter(cancellationToken);
			await m_eamusementLogo.DOFade(1f, m_fadeDuration).ToAwaiter(cancellationToken);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			await m_eamusementLogo.DOFade(0f, m_fadeDuration).ToAwaiter(cancellationToken);
			if (!GameUtility.IsKonastaOrNetcafeBehavior)
			{
				await m_rsaLogo.DOFade(1f, m_fadeDuration).ToAwaiter(cancellationToken);
				await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				await m_rsaLogo.DOFade(0f, m_fadeDuration).ToAwaiter(cancellationToken);
			}
			IsLogoSceneOnceDisplayed = true;
			GotoNextScene("Title");
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	private async UniTask DispCompensationNotice(Cloudstrap.UserEventType userEventType, CancellationToken cancellationToken)
	{
		if (userEventType != 0)
		{
			DialogWindow dialog = DialogManager.Instance.CreateNormalDialog(m_CanvasTransform);
			dialog.ChangeFontSize(28);
			dialog.SetMessage(GetNoticeText(userEventType));
			dialog.Open(60f);
			await UniTask.WaitUntil(() => dialog == null, PlayerLoopTiming.Update, cancellationToken);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
		static string GetNoticeText(Cloudstrap.UserEventType userEventType)
		{
			switch (userEventType)
			{
			case Cloudstrap.UserEventType.Play:
				return "※以前のゲームが正常に動作しなかった可能性がある場合に\n\"FREE\"スタートの対象となります。\n※プレースタート以外は通常のプレー料金が発生します。";
			case Cloudstrap.UserEventType.Gacha1:
			case Cloudstrap.UserEventType.Gacha5:
				return "※ガチャが正常に保存されなかった可能性がある為、\n補填を実施しました。\nガチャ選択画面にてご確認ください。\n";
			default:
				throw new NotImplementedException();
			}
		}
	}

	protected override void OnUpdate()
	{
		if (!UpdateCommonProcess())
		{
			return;
		}
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			if (_isCompensationNoticeEnd)
			{
				GotoTitleIfPossible();
			}
		}
		else
		{
			GotoLoginIfPossible();
		}
	}

	protected virtual bool GotoTitleIfPossible()
	{
		if (_isNextSceneDecided)
		{
			return true;
		}
		if (TitleDemoUtility.IsGameEntryAcceptable() && TitleDemoUtility.IsTouchSkipActionDetected())
		{
			GotoNextScene("Title");
			return true;
		}
		return false;
	}

	protected override bool GotoLoginIfPossible()
	{
		if (_isNextSceneDecided)
		{
			return true;
		}
		if (TitleDemoUtility.IsGameEntryAcceptable() && TitleDemoUtility.IsGameEntryActionDetected(_isCoinInserted))
		{
			TitleDemoUtility.OnPreEnterLoginFlow();
			GotoNextScene("Login");
			return true;
		}
		return false;
	}

	private bool AppTerminateIfNeed()
	{
		if (_isNextSceneDecided)
		{
			return true;
		}
		if (GameUtility.IsKonastaOrNetcafeBehavior && !Cloudstrap.HaveSession())
		{
			Debug.LogWarning("【コナステ・ネカフェ版】セッションが無いため終了します。");
			GotoNextScene("ScopeClose");
			return true;
		}
		return false;
	}
}

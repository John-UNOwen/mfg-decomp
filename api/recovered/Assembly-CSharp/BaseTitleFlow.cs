public abstract class BaseTitleFlow : BaseFlow
{
	protected int _credit;

	protected int _coin;

	protected bool _isCoinInserted;

	public BaseTitleFlow()
	{
		_enableDebugLogin = false;
	}

	protected override void Awake()
	{
		_credit = GameUtility.GetCurrentCredit();
		_coin = GameUtility.GetCurrentFractionCoin();
	}

	protected virtual bool UpdateCommonProcess()
	{
		TitleDemoUtility.GameEntryContext_ActivateIfGameEntryAcceptable();
		if (GotoParticipationErrorIfNeed())
		{
			return false;
		}
		int currentCredit = GameUtility.GetCurrentCredit();
		int currentFractionCoin = GameUtility.GetCurrentFractionCoin();
		_isCoinInserted = currentCredit != _credit || currentFractionCoin != _coin;
		_credit = currentCredit;
		_coin = currentFractionCoin;
		return true;
	}

	protected virtual bool GotoParticipationErrorIfNeed()
	{
		if (_isNextSceneDecided)
		{
			return true;
		}
		if (TitleDemoUtility.IsNeedAdvanceToPPErrorScene())
		{
			GotoNextScene("ParticipationError");
			return true;
		}
		return false;
	}

	protected virtual bool GotoLoginIfPossible()
	{
		if (_isNextSceneDecided)
		{
			return true;
		}
		if (TitleDemoUtility.IsGameEntryAcceptable() && TitleDemoUtility.IsGameEntryActionDetectedExceptTouch(_isCoinInserted))
		{
			TitleDemoUtility.OnPreEnterLoginFlow();
			GotoNextScene("Login");
			return true;
		}
		return false;
	}
}

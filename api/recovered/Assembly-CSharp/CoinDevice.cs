using System;
using UniRx;

public class CoinDevice : IODeviceBase
{
	private int m_prevCoinTotalCount;

	private int m_serviceCoinStock;

	private int m_prevServiceCoinStock;

	private bool m_enableCoinSupplyByService;

	private CoinSoundPlayer m_coinSound = new CoinSoundPlayer();

	private bool m_enableCounter = true;

	private Subject<(int insertedCount, int currentCredit, int currentFractionCoin)> m_onTriggerIncome = new Subject<(int, int, int)>();

	public int Credit { get; private set; }

	public int FractionCoin { get; private set; }

	public IObservable<(int insertedCount, int currentCredit, int currentFractionCoin)> OnTriggerIncome => m_onTriggerIncome.AsObservable();

	public override void Update()
	{
		int totalCoinCount = Bi2xInput.Instance.GetTotalCoinCount();
		int num = totalCoinCount - m_prevCoinTotalCount;
		int num2 = 0;
		if (num > 0 && m_enableCounter)
		{
			Bi2xInput.Instance.AddCoinCounter((uint)num);
			num2 = num;
		}
		int num3 = m_serviceCoinStock - m_prevServiceCoinStock;
		int num4 = num2 + num3;
		if (0 < num2)
		{
			GameUtility.ThrowCoin(num2);
		}
		if (0 < num3)
		{
			GameUtility.ThrowCoinService(num3);
			while (num3 > 0)
			{
				num3--;
			}
		}
		m_coinSound.AddCall(num4);
		int coin = FractionCoin + num4;
		int credit = 0;
		int fraction_coin = 0;
		GameUtility.CalcCreditAndFractionCoin(coin, ref credit, ref fraction_coin);
		Credit += credit;
		FractionCoin = fraction_coin;
		if (num4 > 0)
		{
			m_onTriggerIncome.OnNext((num4, Credit, FractionCoin));
		}
		m_prevCoinTotalCount = totalCoinCount;
		m_prevServiceCoinStock = m_serviceCoinStock;
		m_coinSound.Update();
	}

	public override void PreEnterTestMode()
	{
		ClearCoinStock();
		Activate(bActive: false);
	}

	public override void PostExitTestMode()
	{
		Initialize();
		Activate(TitleDemoUtility.IsGameEntryAcceptable());
	}

	public void SetCoinSupplyCall(bool bEnable)
	{
		m_coinSound.IsEnable = bEnable;
	}

	public bool ConsumeCredit(int credit)
	{
		if (credit < 0)
		{
			return false;
		}
		if (Credit < credit)
		{
			return false;
		}
		Credit -= credit;
		return true;
	}

	public bool AddServiceCoin(int coin)
	{
		if (!m_enableCoinSupplyByService)
		{
			return false;
		}
		m_serviceCoinStock += coin;
		return true;
	}

	private void ClearCoinStock()
	{
		Credit = 0;
		FractionCoin = 0;
		m_coinSound.Clear();
	}

	public override void Activate(bool bActive)
	{
		if (bActive)
		{
			m_coinSound.IsEnable = true;
			m_enableCoinSupplyByService = true;
			Bi2xInput.Instance.ControlCoinBlocker(is_open: true);
			m_enableCounter = true;
		}
		else
		{
			m_coinSound.IsEnable = false;
			m_enableCoinSupplyByService = false;
			Bi2xInput.Instance.ControlCoinBlocker(is_open: false);
			m_enableCounter = false;
		}
		base.Activate(bActive);
	}
}

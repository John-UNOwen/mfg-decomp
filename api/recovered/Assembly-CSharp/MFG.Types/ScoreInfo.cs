using System.Collections.Generic;
using MFG.MahjonPlay.Hand;

namespace MFG.Types;

public class ScoreInfo
{
	public List<Yaku> Yakus = new List<Yaku>();

	public HaiID AgariHaiID = HaiID.None;

	public MachiType MachiType;

	public bool IsTsumo;

	public bool IsOya;

	public Han Han;

	public int HanCount;

	public int YakumanCount;

	public int DoraCount;

	public int BonusCount;

	public int Fu;

	public long TotalPayment;

	public long KoPaymentByTsumo;

	public long OyaPaymentByTsumo;
}

using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

public interface IPlayerInteractor
{
	MFG.Types.EventType EventType();

	Cha Cha();

	Kaze Kaze();

	CharaType CharaType();

	int CharaVariantID();

	int FavoLevel();

	ReachType ReachType();

	ReachLevel ReachLevel();

	Pai Dahai();

	List<CutinItemInfo> CutinItems();

	Dictionary<Category, List<ItemID>> CustomizeItemEquips();

	GameObject CutinParent();

	GameObject NakiCutinParent();

	GameObject KnockdownTileParent();

	void CreateKnockdownTiles(GameObject tileParent);

	GameObject ThrownReachStickParent();

	GameObject LandedReachStickParent();

	ItemID ReachStickID();

	bool IsActive();

	bool IsScorePaymentPlayer();

	Han PaymentAmount();

	int DoraCount();

	int MeldBonusCount();

	int AllGreenCount();

	int ComebackBonusCount();

	int AkadoraCount();

	List<Yaku> PaymentYakus();

	bool IsBombBonusPlay();

	bool IsCatharsisPlay();

	bool IsFireReachEffectActive()
	{
		return ReachLevel() >= MFG.Types.ReachLevel.FireReach;
	}

	bool IsAtomicReachEffectActive()
	{
		return ReachLevel() == MFG.Types.ReachLevel.AtomicReach;
	}

	bool IsOpenReachEffectActive()
	{
		return ReachLevel() == MFG.Types.ReachLevel.OpenReach;
	}

	bool IsAccelDoraEffectActive()
	{
		if (EventType() == MFG.Types.EventType.AccelDora || EventType() == MFG.Types.EventType.Aotenjo)
		{
			return DoraCount() >= 5;
		}
		return false;
	}

	bool IsCatharsisEffectActive()
	{
		if (EventType() == MFG.Types.EventType.Comeback || EventType() == MFG.Types.EventType.Aotenjo)
		{
			return IsCatharsisPlay();
		}
		return false;
	}

	bool IsKirisameEffectActive()
	{
		if (EventType() == MFG.Types.EventType.Kirisame || EventType() == MFG.Types.EventType.Aotenjo)
		{
			return AkadoraCount() >= 5;
		}
		return false;
	}

	bool IsMeldBonusEffectActive()
	{
		if (EventType() == MFG.Types.EventType.MeldBonus || EventType() == MFG.Types.EventType.PCKGalaxy || EventType() == MFG.Types.EventType.Aotenjo)
		{
			return MeldBonusCount() >= 4;
		}
		return false;
	}

	bool IsAllGreenEffectActive()
	{
		if (EventType() == MFG.Types.EventType.AllGreen || EventType() == MFG.Types.EventType.Aotenjo)
		{
			if (AllGreenCount() < 3)
			{
				return PaymentYakus().Contains(Yaku.Ryuiso);
			}
			return true;
		}
		return false;
	}

	bool IsChocolateReachEffectActive()
	{
		return CustomizeItemMaster.IsChocolateEffectReachStick(CharaType(), ReachStickID());
	}

	bool IsBombEffectActive()
	{
		return IsBombBonusPlay();
	}

	bool IsMentanpinLowEffectActive()
	{
		int num = PaymentYakus().Count((Yaku x) => x == Yaku.Richi || x == Yaku.DoubleRichi || x == Yaku.Tanyao || x == Yaku.Pinfu);
		if (EventType() == MFG.Types.EventType.Mentanpin || EventType() == MFG.Types.EventType.Aotenjo)
		{
			return num >= 2;
		}
		return false;
	}

	bool IsMentanpinHighEffectActive()
	{
		int num = PaymentYakus().Count((Yaku x) => x == Yaku.Richi || x == Yaku.DoubleRichi || x == Yaku.Tanyao || x == Yaku.Pinfu);
		if (EventType() == MFG.Types.EventType.Mentanpin || EventType() == MFG.Types.EventType.Aotenjo)
		{
			return num >= 3;
		}
		return false;
	}

	bool IsFakeClear()
	{
		return CharaType().IsFakeClear(CharaVariantID());
	}

	List<VoiceInfo> GachaVoices();
}

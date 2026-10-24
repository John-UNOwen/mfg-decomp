// net472-profile stand-in for api\recovered\Assembly-CSharp\MFG.MahjongPlay.Cutin\IPlayerInteractor.cs
//
// The recovered file declares 30 abstract and 13 default interface methods. asmmeta on
// the shipped Assembly-CSharp.dll confirms that split is real IL:
//
//   type MFG.MahjongPlay.Cutin.IPlayerInteractor attrs=Public, Abstract
//   -> 13/43 declared methods carry IL bodies (IsFakeClear, IsBombEffectActive, ...)
//
// i.e. C# 8 default interface implementations, which Unity's Mono runtime executes.
// The .NET Framework 4.7.2 runtime cannot host interface method bodies, so Roslyn
// refuses them under the net472 target (CS8701, not suppressable by NoWarn) even
// though the recovered source matches the shipped assembly exactly. The unity profile
// (netstandard2.1 + the game's own shipped Mono BCL) compiles the recovered file
// unmodified. This file exists only for the net472 profile and lowers the same 13
// bodies to extension methods: no implementer overrides them (PlayerInteractor and
// PreviewPlayerInteractor declare none of the 13 names, checked against the shipped
// metadata), and every call site has a static receiver typed IPlayerInteractor, so
// dispatch, arguments and results are unchanged.

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

	List<VoiceInfo> GachaVoices();
}

public static class PlayerInteractorDefaultImplementations
{
	public static bool IsFireReachEffectActive(this IPlayerInteractor @this)
	{
		return @this.ReachLevel() >= MFG.Types.ReachLevel.FireReach;
	}

	public static bool IsAtomicReachEffectActive(this IPlayerInteractor @this)
	{
		return @this.ReachLevel() == MFG.Types.ReachLevel.AtomicReach;
	}

	public static bool IsOpenReachEffectActive(this IPlayerInteractor @this)
	{
		return @this.ReachLevel() == MFG.Types.ReachLevel.OpenReach;
	}

	public static bool IsAccelDoraEffectActive(this IPlayerInteractor @this)
	{
		if (@this.EventType() == MFG.Types.EventType.AccelDora || @this.EventType() == MFG.Types.EventType.Aotenjo)
		{
			return @this.DoraCount() >= 5;
		}
		return false;
	}

	public static bool IsCatharsisEffectActive(this IPlayerInteractor @this)
	{
		if (@this.EventType() == MFG.Types.EventType.Comeback || @this.EventType() == MFG.Types.EventType.Aotenjo)
		{
			return @this.IsCatharsisPlay();
		}
		return false;
	}

	public static bool IsKirisameEffectActive(this IPlayerInteractor @this)
	{
		if (@this.EventType() == MFG.Types.EventType.Kirisame || @this.EventType() == MFG.Types.EventType.Aotenjo)
		{
			return @this.AkadoraCount() >= 5;
		}
		return false;
	}

	public static bool IsMeldBonusEffectActive(this IPlayerInteractor @this)
	{
		if (@this.EventType() == MFG.Types.EventType.MeldBonus || @this.EventType() == MFG.Types.EventType.PCKGalaxy || @this.EventType() == MFG.Types.EventType.Aotenjo)
		{
			return @this.MeldBonusCount() >= 4;
		}
		return false;
	}

	public static bool IsAllGreenEffectActive(this IPlayerInteractor @this)
	{
		if (@this.EventType() == MFG.Types.EventType.AllGreen || @this.EventType() == MFG.Types.EventType.Aotenjo)
		{
			if (@this.AllGreenCount() < 3)
			{
				return @this.PaymentYakus().Contains(Yaku.Ryuiso);
			}
			return true;
		}
		return false;
	}

	public static bool IsChocolateReachEffectActive(this IPlayerInteractor @this)
	{
		return CustomizeItemMaster.IsChocolateEffectReachStick(@this.CharaType(), @this.ReachStickID());
	}

	public static bool IsBombEffectActive(this IPlayerInteractor @this)
	{
		return @this.IsBombBonusPlay();
	}

	public static bool IsMentanpinLowEffectActive(this IPlayerInteractor @this)
	{
		int num = @this.PaymentYakus().Count((Yaku x) => x == Yaku.Richi || x == Yaku.DoubleRichi || x == Yaku.Tanyao || x == Yaku.Pinfu);
		if (@this.EventType() == MFG.Types.EventType.Mentanpin || @this.EventType() == MFG.Types.EventType.Aotenjo)
		{
			return num >= 2;
		}
		return false;
	}

	public static bool IsMentanpinHighEffectActive(this IPlayerInteractor @this)
	{
		int num = @this.PaymentYakus().Count((Yaku x) => x == Yaku.Richi || x == Yaku.DoubleRichi || x == Yaku.Tanyao || x == Yaku.Pinfu);
		if (@this.EventType() == MFG.Types.EventType.Mentanpin || @this.EventType() == MFG.Types.EventType.Aotenjo)
		{
			return num >= 3;
		}
		return false;
	}

	public static bool IsFakeClear(this IPlayerInteractor @this)
	{
		return @this.CharaType().IsFakeClear(@this.CharaVariantID());
	}
}

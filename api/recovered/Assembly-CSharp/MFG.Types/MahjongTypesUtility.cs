using System.Collections.Generic;
using MFG.Scenes.Home;
using MFG.Taikyoku.Command;

namespace MFG.Types;

public static class MahjongTypesUtility
{
	public class GameMode
	{
		public GAME_MODE Mode;

		public TakuType TakuType;

		public EventType EventType;

		public PlayRuleType PlayRuleType;
	}

	private static List<GameMode> _gameModes = new List<GameMode>
	{
		new GameMode
		{
			Mode = (GAME_MODE)0,
			TakuType = (TakuType)(-1),
			EventType = EventType.None,
			PlayRuleType = (PlayRuleType)(-1)
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU,
			TakuType = TakuType.Tonpu,
			EventType = EventType.None,
			PlayRuleType = PlayRuleType.Yonma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_HANCHAN,
			TakuType = TakuType.Hanchan,
			EventType = EventType.None,
			PlayRuleType = PlayRuleType.Yonma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA,
			TakuType = TakuType.Sanma,
			EventType = EventType.None,
			PlayRuleType = PlayRuleType.Sanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_NIMA,
			TakuType = TakuType.Nima,
			EventType = EventType.None,
			PlayRuleType = PlayRuleType.Nima
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_TOITOI_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.ToitoiButtobiGalaxy,
			PlayRuleType = PlayRuleType.ButtobiGalaxy
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_FIREREACH_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.FireReach,
			PlayRuleType = PlayRuleType.FireReach
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_TOITOI_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.ToitoiButtobiGalaxy,
			PlayRuleType = PlayRuleType.ButtobiGalaxySanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_ACCELDORA_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.AccelDora,
			PlayRuleType = PlayRuleType.AccelDora
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_MENTANPIN_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.Mentanpin,
			PlayRuleType = PlayRuleType.Mentanpin
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_AOTENJO_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.Aotenjo,
			PlayRuleType = PlayRuleType.Aotenjo
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_COMEBACK_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.Comeback,
			PlayRuleType = PlayRuleType.Comeback
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_ACCELDORA_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.AccelDora,
			PlayRuleType = PlayRuleType.AccelDoraSanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_KIRISAME_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.Kirisame,
			PlayRuleType = PlayRuleType.Kirisame
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_MENTANPIN_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.Mentanpin,
			PlayRuleType = PlayRuleType.MentanpinSanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_MELDBONUS_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.MeldBonus,
			PlayRuleType = PlayRuleType.MeldBonus
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_FIREGALAXY_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.FireGalaxy,
			PlayRuleType = PlayRuleType.FireGalaxy
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_FIREGALAXY_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.FireGalaxy,
			PlayRuleType = PlayRuleType.FireGalaxySanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_REVERSAL_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.Reversal,
			PlayRuleType = PlayRuleType.ReversalSanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_MELDBONUS_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.MeldBonus,
			PlayRuleType = PlayRuleType.MeldBonusSanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_BOMB_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.Bomb,
			PlayRuleType = PlayRuleType.Bomb
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_BOMB_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.Bomb,
			PlayRuleType = PlayRuleType.BombSanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_ALLGREEN_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.AllGreen,
			PlayRuleType = PlayRuleType.AllGreen
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_ALLGREEN_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.AllGreen,
			PlayRuleType = PlayRuleType.AllGreenSanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_COMEBACK_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.Comeback,
			PlayRuleType = PlayRuleType.ComebackSanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_ATOMIC_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.Atomic,
			PlayRuleType = PlayRuleType.Atomic
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_ATOMIC_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.Atomic,
			PlayRuleType = PlayRuleType.AtomicSanma
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_TONPU_PCKGALAXY_EVENT,
			TakuType = TakuType.Tonpu,
			EventType = EventType.PCKGalaxy,
			PlayRuleType = PlayRuleType.PCKGalaxy
		},
		new GameMode
		{
			Mode = GAME_MODE.GLOBAL_SANMA_PCKGALAXY_EVENT,
			TakuType = TakuType.Sanma,
			EventType = EventType.PCKGalaxy,
			PlayRuleType = PlayRuleType.PCKGalaxySanma
		}
	};

	public static (TakuType takuType, EventType eventType) GetTakuTypeAndEventType(this GAME_MODE gameMode)
	{
		GameMode gameMode2 = _gameModes.Find((GameMode x) => x.Mode == gameMode);
		if (gameMode2 != null)
		{
			return (takuType: gameMode2.TakuType, eventType: gameMode2.EventType);
		}
		return (takuType: (TakuType)(-1), eventType: EventType.None);
	}

	public static GAME_MODE GetGameMode(TakuType takuType, EventType eventType)
	{
		return _gameModes.Find((GameMode x) => x.TakuType == takuType && x.EventType == eventType)?.Mode ?? ((GAME_MODE)0);
	}

	public static PlayRuleType GetPlayRuleType(TakuType takuType, EventType eventType)
	{
		return _gameModes.Find((GameMode x) => x.TakuType == takuType && x.EventType == eventType)?.PlayRuleType ?? PlayRuleType.Yonma;
	}
}

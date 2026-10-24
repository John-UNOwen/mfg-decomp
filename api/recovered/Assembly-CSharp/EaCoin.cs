using System.Collections.Generic;
using GameContents;

public class EaCoin
{
	public struct PathData
	{
		public string credit;

		public string paseli;

		public string ppexArticleID;

		public bool IsPPEXItem;

		public PathData(string credit, string paseli, string ppexArticleID = "", bool isPPEXItem = false)
		{
			this.credit = credit;
			this.paseli = paseli;
			this.ppexArticleID = ppexArticleID;
			IsPPEXItem = isPPEXItem;
		}
	}

	public static string SLOT_PATH = "/opt/eacoin/slot1/player";

	public static string ECTYPE = "1";

	public static Dictionary<GAME_MODE, PathData> ConsumePaseliEventArticles = new Dictionary<GAME_MODE, PathData>
	{
		{
			GAME_MODE.START,
			new PathData("/coin/game_s", "/eacoin/game_s_ec")
		},
		{
			GAME_MODE.CONTINUE,
			new PathData("/coin/game_c", "/eacoin/game_c_ec")
		},
		{
			GAME_MODE.GACHA,
			new PathData("/coin/vfg_gacha", "/eacoin/vfg_gacha_ec")
		},
		{
			GAME_MODE.GACHA5,
			new PathData("/coin/vfg_gacha2", "/eacoin/vfg_gacha2_ec")
		},
		{
			GAME_MODE.JANDRI,
			new PathData("", "/eacoin/vfg_jandri_ppex", "VFGJA/ppex_jandori", isPPEXItem: true)
		},
		{
			GAME_MODE.PREMIUMSTART,
			new PathData("/coin/vfg_game_premium", "/eacoin/vfg_ps_ec")
		},
		{
			GAME_MODE.PREMIUMCONTINUE,
			new PathData("/coin/vfg_game_premium_c", "/eacoin/vfg_pc_ec")
		},
		{
			GAME_MODE.MUSICGACHA,
			new PathData("", "/eacoin/vfg_music_gacha_ppex", GameUtility.IsKonastaOrNetcafeBehavior ? "" : "VFGJA/ppex_music_gacha", !GameUtility.IsKonastaOrNetcafeBehavior)
		}
	};

	public static Dictionary<CONSUME_MODE, string> ConsumeCreditEventArticles = new Dictionary<CONSUME_MODE, string>
	{
		{
			CONSUME_MODE.START_CREDIT,
			"/coin/game_s"
		},
		{
			CONSUME_MODE.CONTINUE_CREDIT,
			"/coin/game_c"
		},
		{
			CONSUME_MODE.GACHA_CREDIT,
			"/coin/vfg_gacha"
		},
		{
			CONSUME_MODE.GACHA5_CREDIT,
			"/coin/vfg_gacha2"
		},
		{
			CONSUME_MODE.PREMIUM_START_CREDIT,
			"/coin/vfg_game_premium"
		},
		{
			CONSUME_MODE.PREMIUM_CONTINUE_CREDIT,
			"/coin/vfg_game_premium_c"
		}
	};

	public static Dictionary<CONSUME_MODE, string> ConsumeParticipationEventArticles = new Dictionary<CONSUME_MODE, string>
	{
		{
			CONSUME_MODE.START_CREDIT,
			"/coin/pp_vfg_start"
		},
		{
			CONSUME_MODE.START_PASELI,
			"/coin/pp_vfg_start"
		},
		{
			CONSUME_MODE.CONTINUE_CREDIT,
			"/coin/pp_vfg_continue"
		},
		{
			CONSUME_MODE.CONTINUE_PASELI,
			"/coin/pp_vfg_continue"
		},
		{
			CONSUME_MODE.GACHA_CREDIT,
			"/coin/pp_vfg_gacha"
		},
		{
			CONSUME_MODE.GACHA_PASELI,
			"/coin/pp_vfg_gacha"
		},
		{
			CONSUME_MODE.GACHA5_CREDIT,
			"/coin/pp_vfg_gacha"
		},
		{
			CONSUME_MODE.GACHA5_PASELI,
			"/coin/pp_vfg_gacha"
		},
		{
			CONSUME_MODE.PREMIUM_START_PASELI,
			"/coin/pp_vfg_premium_start"
		},
		{
			CONSUME_MODE.PREMIUM_CONTINUE_PASELI,
			"/coin/pp_vfg_premium_continue"
		},
		{
			CONSUME_MODE.PREMIUM_START_CREDIT,
			"/coin/pp_vfg_premium_start"
		},
		{
			CONSUME_MODE.PREMIUM_CONTINUE_CREDIT,
			"/coin/pp_vfg_premium_continue"
		}
	};

	public static PathData GetPathData(GAME_MODE gameMode)
	{
		return ConsumePaseliEventArticles[gameMode];
	}
}

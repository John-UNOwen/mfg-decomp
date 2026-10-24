using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Agari : MonoBehaviour
{
	private class YakuInfo
	{
		public Yaku m_yaku;

		public int m_han;

		public bool m_isKuisagari;

		public bool m_isBonus;

		public YakuInfo(Yaku yaku, int han, bool isKuisagari, bool isBonus = false)
		{
			m_yaku = yaku;
			m_han = han;
			m_isKuisagari = isKuisagari;
			m_isBonus = isBonus;
		}
	}

	[SerializeField]
	private GameObject agariPlayer;

	[SerializeField]
	private GameObject agariWanpai;

	[SerializeField]
	private Text yakuText;

	[SerializeField]
	private GameObject m_handType;

	[SerializeField]
	private GameObject m_handTypeAdd;

	[SerializeField]
	private List<GameObject> m_yakuList = new List<GameObject>();

	private static readonly int _maxYakuListNum = 15;

	[SerializeField]
	private TextMeshProUGUI m_AgariPlayerText;

	[SerializeField]
	private TextMeshProUGUI m_RonPlayerText;

	[SerializeField]
	private GameObject m_AgariCall;

	[SerializeField]
	private Text m_FuText;

	[SerializeField]
	private Text m_HanText;

	[SerializeField]
	private TextMeshProUGUI m_ScoreText;

	[SerializeField]
	private Sprite m_Frame1Sprite;

	[SerializeField]
	private Sprite m_Frame2Sprite;

	[SerializeField]
	private Sprite m_Frame3Sprite;

	[SerializeField]
	private Sprite m_Frame4Sprite;

	[SerializeField]
	private Sprite m_Frame5Sprite;

	[SerializeField]
	private Sprite m_Frame6Sprite;

	[SerializeField]
	private Sprite m_Frame7Sprite;

	[SerializeField]
	private Sprite m_Frame13Sprite;

	[SerializeField]
	private HaiConjecture m_haiConjecture;

	public static Yaku[] YakuPriority = new Yaku[60]
	{
		Yaku.Tenho,
		Yaku.Chiho,
		Yaku.Renho,
		Yaku.Tyuren9,
		Yaku.TyurenTanki,
		Yaku.Chinroto,
		Yaku.Tsuiso,
		Yaku.Ryuiso,
		Yaku.Kokushi13,
		Yaku.KokushiTanki,
		Yaku.Sukantsu,
		Yaku.Suanko,
		Yaku.SuankoTanki,
		Yaku.Daisushi,
		Yaku.Syosushi,
		Yaku.Daisangen,
		Yaku.Daisyarin,
		Yaku.Surenko,
		Yaku.Parenchan,
		Yaku.Kazoeyakuman,
		Yaku.Nagashimangan,
		Yaku.DoubleRichi,
		Yaku.Richi,
		Yaku.Ippatsu,
		Yaku.Haitei,
		Yaku.Hotei,
		Yaku.Chankan,
		Yaku.Rinsyan,
		Yaku.Menzen,
		Yaku.Chiniso,
		Yaku.ChinisoNaki,
		Yaku.Chitoitsu,
		Yaku.Pinfu,
		Yaku.Honroto,
		Yaku.Syosangen,
		Yaku.Shisanputo,
		Yaku.Junchan,
		Yaku.JunchanNaki,
		Yaku.Ryanpeko,
		Yaku.Honiso,
		Yaku.HonisoNaki,
		Yaku.Sankantsu,
		Yaku.Sananko,
		Yaku.Toitoiho,
		Yaku.Sansyokudoko,
		Yaku.Sansyokudojun,
		Yaku.SansyokudojunNaki,
		Yaku.Chanta,
		Yaku.ChantaNaki,
		Yaku.Ikkitsukan,
		Yaku.IkkitsukanNaki,
		Yaku.Sanrenko,
		Yaku.Ipeiko,
		Yaku.Tanyao,
		Yaku.Bakaze,
		Yaku.Jikaze,
		Yaku.Haku,
		Yaku.Hatsu,
		Yaku.Tyun,
		Yaku.Dora
	};

	private Dictionary<Yaku, string> m_yakuVoiceList = new Dictionary<Yaku, string>
	{
		{
			Yaku.Tenho,
			"VOX_GXX_MJG_YAKU42"
		},
		{
			Yaku.Chiho,
			"VOX_GXX_MJG_YAKU43"
		},
		{
			Yaku.Renho,
			"VOX_GXX_MJG_YAKU44"
		},
		{
			Yaku.Tyuren9,
			"VOX_GXX_MJG_YAKU49"
		},
		{
			Yaku.TyurenTanki,
			"VOX_GXX_MJG_YAKU41"
		},
		{
			Yaku.Chinroto,
			"VOX_GXX_MJG_YAKU34"
		},
		{
			Yaku.Tsuiso,
			"VOX_GXX_MJG_YAKU39"
		},
		{
			Yaku.Ryuiso,
			"VOX_GXX_MJG_YAKU40"
		},
		{
			Yaku.Kokushi13,
			"VOX_GXX_MJG_YAKU48"
		},
		{
			Yaku.KokushiTanki,
			"VOX_GXX_MJG_YAKU35"
		},
		{
			Yaku.Sukantsu,
			"VOX_GXX_MJG_YAKU37"
		},
		{
			Yaku.Suanko,
			"VOX_GXX_MJG_YAKU32"
		},
		{
			Yaku.SuankoTanki,
			"VOX_GXX_MJG_YAKU46"
		},
		{
			Yaku.Daisushi,
			"VOX_GXX_MJG_YAKU47"
		},
		{
			Yaku.Syosushi,
			"VOX_GXX_MJG_YAKU33"
		},
		{
			Yaku.Daisangen,
			"VOX_GXX_MJG_YAKU38"
		},
		{
			Yaku.Daisyarin,
			"VOX_GXX_MJG_YAKU36"
		},
		{
			Yaku.Surenko,
			""
		},
		{
			Yaku.Parenchan,
			"VOX_GXX_MJG_YAKU45"
		},
		{
			Yaku.Kazoeyakuman,
			""
		},
		{
			Yaku.Nagashimangan,
			""
		},
		{
			Yaku.DoubleRichi,
			"VOX_GXX_MJG_YAKU22"
		},
		{
			Yaku.Richi,
			"VOX_GXX_MJG_YAKU10"
		},
		{
			Yaku.Ippatsu,
			"VOX_GXX_MJG_YAKU11"
		},
		{
			Yaku.Haitei,
			"VOX_GXX_MJG_YAKU15"
		},
		{
			Yaku.Hotei,
			"VOX_GXX_MJG_YAKU16"
		},
		{
			Yaku.Chankan,
			"VOX_GXX_MJG_YAKU14"
		},
		{
			Yaku.Rinsyan,
			"VOX_GXX_MJG_YAKU13"
		},
		{
			Yaku.Menzen,
			"VOX_GXX_MJG_YAKU7"
		},
		{
			Yaku.Chiniso,
			"VOX_GXX_MJG_YAKU31"
		},
		{
			Yaku.ChinisoNaki,
			"VOX_GXX_MJG_YAKU31"
		},
		{
			Yaku.Chitoitsu,
			"VOX_GXX_MJG_YAKU26"
		},
		{
			Yaku.Pinfu,
			"VOX_GXX_MJG_YAKU8"
		},
		{
			Yaku.Honroto,
			"VOX_GXX_MJG_YAKU25"
		},
		{
			Yaku.Syosangen,
			"VOX_GXX_MJG_YAKU27"
		},
		{
			Yaku.Shisanputo,
			""
		},
		{
			Yaku.Junchan,
			"VOX_GXX_MJG_YAKU30"
		},
		{
			Yaku.JunchanNaki,
			"VOX_GXX_MJG_YAKU30"
		},
		{
			Yaku.Ryanpeko,
			"VOX_GXX_MJG_YAKU29"
		},
		{
			Yaku.Honiso,
			"VOX_GXX_MJG_YAKU28"
		},
		{
			Yaku.HonisoNaki,
			"VOX_GXX_MJG_YAKU28"
		},
		{
			Yaku.Sankantsu,
			"VOX_GXX_MJG_YAKU24"
		},
		{
			Yaku.Sananko,
			"VOX_GXX_MJG_YAKU23"
		},
		{
			Yaku.Toitoiho,
			"VOX_GXX_MJG_YAKU17"
		},
		{
			Yaku.Sansyokudoko,
			"VOX_GXX_MJG_YAKU19"
		},
		{
			Yaku.Sansyokudojun,
			"VOX_GXX_MJG_YAKU18"
		},
		{
			Yaku.SansyokudojunNaki,
			"VOX_GXX_MJG_YAKU18"
		},
		{
			Yaku.Chanta,
			"VOX_GXX_MJG_YAKU21"
		},
		{
			Yaku.ChantaNaki,
			"VOX_GXX_MJG_YAKU21"
		},
		{
			Yaku.Ikkitsukan,
			"VOX_GXX_MJG_YAKU20"
		},
		{
			Yaku.IkkitsukanNaki,
			"VOX_GXX_MJG_YAKU20"
		},
		{
			Yaku.Sanrenko,
			""
		},
		{
			Yaku.Ipeiko,
			"VOX_GXX_MJG_YAKU12"
		},
		{
			Yaku.Tanyao,
			"VOX_GXX_MJG_YAKU9"
		},
		{
			Yaku.Bakaze,
			"VOX_GXX_MJG_YAKU1"
		},
		{
			Yaku.Jikaze,
			"VOX_GXX_MJG_YAKU2"
		},
		{
			Yaku.Haku,
			"VOX_GXX_MJG_YAKU3"
		},
		{
			Yaku.Hatsu,
			"VOX_GXX_MJG_YAKU4"
		},
		{
			Yaku.Tyun,
			"VOX_GXX_MJG_YAKU5"
		},
		{
			Yaku.Dora,
			""
		}
	};

	private List<string> m_doraVoiceList = new List<string>
	{
		"VOX_GXX_MJG_DORA0", "VOX_GXX_MJG_DORA1", "VOX_GXX_MJG_DORA2", "VOX_GXX_MJG_DORA3", "VOX_GXX_MJG_DORA4", "VOX_GXX_MJG_DORA5", "VOX_GXX_MJG_DORA6", "VOX_GXX_MJG_DORA7", "VOX_GXX_MJG_DORA8", "VOX_GXX_MJG_DORA9",
		"VOX_GXX_MJG_DORA10", "VOX_GXX_MJG_DORA11", "VOX_GXX_MJG_DORA12", "VOX_GXX_MJG_DORA13", "VOX_GXX_MJG_DORA0"
	};

	private Dictionary<Han, string> m_hanVoiceList = new Dictionary<Han, string>
	{
		{
			Han.Nothing,
			""
		},
		{
			Han.Han1,
			""
		},
		{
			Han.Han2,
			""
		},
		{
			Han.Han3,
			""
		},
		{
			Han.Han4,
			""
		},
		{
			Han.Mangan,
			"VOX_GXX_MJG_RESULT5"
		},
		{
			Han.Haneman,
			"VOX_GXX_MJG_RESULT6"
		},
		{
			Han.Baiman,
			"VOX_GXX_MJG_RESULT7"
		},
		{
			Han.Sanbaiman,
			"VOX_GXX_MJG_RESULT8"
		},
		{
			Han.Yakuman,
			"VOX_GXX_MJG_RESULT9"
		},
		{
			Han.Yakuman2,
			"VOX_GXX_MJG_RESULT11"
		},
		{
			Han.Yakuman3,
			"VOX_GXX_MJG_RESULT12"
		},
		{
			Han.Yakuman4,
			"VOX_GXX_MJG_RESULT9"
		},
		{
			Han.Yakuman5,
			"VOX_GXX_MJG_RESULT9"
		},
		{
			Han.Yakuman6,
			"VOX_GXX_MJG_RESULT9"
		},
		{
			Han.Yakuman7,
			"VOX_GXX_MJG_RESULT9"
		}
	};

	private Dictionary<int, string> m_hanSeList = new Dictionary<int, string>
	{
		{ 1, "SFX_MJG_YAKU1" },
		{ 2, "SFX_MJG_YAKU2" },
		{ 3, "SFX_MJG_YAKU3" },
		{ 4, "SFX_MJG_YAKU3" },
		{ 5, "SFX_MJG_YAKU5" },
		{ 6, "SFX_MJG_YAKU6" },
		{ 7, "SFX_MJG_YAKU6" },
		{ 13, "SFX_MJG_YAKUMAN" },
		{ 26, "SFX_MJG_YAKUMAN" }
	};

	private int m_yakuDispNum;

	private List<YakuInfo> m_yakuInfoList = new List<YakuInfo>();

	private int m_doraNum;

	private bool m_isKazoeyakuman;

	private Han m_han;

	private CharaType m_charaType;

	private bool m_isEndAnimeLong;

	private List<bool> yakus = new List<bool>();

	private Hand m_objhand;

	private Meld m_objmeld;

	private void Start()
	{
	}

	private async UniTask Process(CancellationToken cancellationToken = default(CancellationToken))
	{
		await UniTask.Delay(TimeSpan.FromSeconds(1.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		Debug.Log($"役読み上げ: m_yakuDispNum {m_yakuDispNum} / m_yakuInfoList {m_yakuInfoList.Count}");
		for (int i = 0; i < m_yakuDispNum; i++)
		{
			m_yakuList[i].SetActive(value: true);
			string voiceID = null;
			if (m_yakuInfoList[i].m_yaku == Yaku.Dora)
			{
				if (0 <= m_doraNum && m_doraNum <= 13)
				{
					voiceID = SoundManager.GetCharaVoiceLabel(m_doraVoiceList[m_doraNum], m_charaType);
				}
				else
				{
					voiceID = SoundManager.GetCharaVoiceLabel("VOX_GXX_MJG_DORA0", m_charaType);
				}
			}
			else if (Yaku.Tenho <= m_yakuInfoList[i].m_yaku && m_yakuInfoList[i].m_yaku < Yaku.Max)
			{
				voiceID = SoundManager.GetCharaVoiceLabel(m_yakuVoiceList[m_yakuInfoList[i].m_yaku], m_charaType);
			}
			if (voiceID != null)
			{
				SoundManager.PlayVoice(voiceID).Forget();
				await UniTask.Delay(TimeSpan.FromSeconds(0.3333333432674408), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				if (m_yakuInfoList[i].m_yaku == Yaku.Dora)
				{
					SoundManager.PlaySe("SFX_MJG_DORA").Forget();
				}
				else if (m_yakuInfoList[i].m_han == 6)
				{
					if (m_yakuInfoList[i].m_isKuisagari)
					{
						SoundManager.PlaySe("SFX_MJG_YAKU5").Forget();
					}
					else
					{
						SoundManager.PlaySe("SFX_MJG_YAKU6").Forget();
					}
				}
				else
				{
					string text = m_hanSeList[m_yakuInfoList[i].m_han];
					if (text != null)
					{
						SoundManager.PlaySe(text).Forget();
					}
				}
				if (m_yakuDispNum == 1 && Yaku.Haitei <= m_yakuInfoList[i].m_yaku && m_yakuInfoList[i].m_yaku <= Yaku.Jikaze)
				{
					await UniTask.WaitWhile(() => SoundManager.IsPlay(voiceID), PlayerLoopTiming.Update, cancellationToken);
					await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
					voiceID = SoundManager.GetCharaVoiceLabel("VOX_GXX_MJG_YAKU50", m_charaType);
					SoundManager.PlayVoice(voiceID).Forget();
				}
			}
			else if (m_yakuInfoList[i].m_isBonus)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(0.3333333432674408), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				SoundManager.PlaySe("SFX_MJG_BONUS1").Forget();
			}
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.699999988079071), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => IsSkip()));
			if (IsSkip())
			{
				break;
			}
			if (voiceID != null)
			{
				await UniTask.WhenAny(UniTask.WaitWhile(() => SoundManager.IsPlay(voiceID), PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => IsSkip()));
			}
		}
		for (int j = 0; j < m_yakuDispNum; j++)
		{
			Debug.Log($"青天井DebugLog: m_yakuList[{j}] IsActive ? {m_yakuList[j].activeSelf}");
			if (!m_yakuList[j].activeSelf)
			{
				m_yakuList[j].SetActive(value: true);
			}
		}
		GetComponent<Animator>().SetTrigger("YakuEnd");
		await UniTask.Delay(TimeSpan.FromSeconds(0.6666666865348816), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		SoundManager.PlaySe("SFX_MJG_SCORE1").Forget();
		if (!GameDataManager.Mahjong.IsAotenjoMode)
		{
			if (m_isKazoeyakuman)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(0.8333333134651184), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				SoundManager.PlayVoice("VOX_GXX_MJG_RESULT10", m_charaType).Forget();
			}
			else if (m_han >= Han.Mangan)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(0.8333333134651184), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				SoundManager.PlayVoice(m_hanVoiceList[m_han], m_charaType).Forget();
				if (m_han >= Han.Yakuman)
				{
					SoundManager.PlaySe("SFX_MJG_SCORE4").Forget();
				}
				else if (m_han <= Han.Baiman)
				{
					SoundManager.PlaySe("SFX_MJG_SCORE3").Forget();
				}
				else
				{
					SoundManager.PlaySe("SFX_MJG_SCORE2").Forget();
				}
			}
		}
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => IsSkip()));
		if (GameUtility.IsEventMode())
		{
			await UniTask.WaitUntil(() => IsSkip());
		}
		GetComponent<Animator>().SetTrigger("EndSmall");
		GetComponent<Animator>().SetTrigger("EndBig");
		if (m_isEndAnimeLong)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(3.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
		else
		{
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
	}

	public async UniTask SetAgariTsumo(TsumoAgari cmd, List<Player> players, Player player, Wanpai wanpai, Kaze bombKaze, CancellationToken cancellationToken = default(CancellationToken))
	{
		SetAgariCommon(cmd.Yaku, cmd.Scores[cmd.PlayerIndex].YakuScore, wanpai, cmd.OpenDoraNum, players, player, isTsumo: true, bombKaze).Forget();
		m_AgariPlayerText.text = player.m_name;
		m_RonPlayerText.text = "";
		m_AgariCall.transform.Find("Ron").gameObject.SetActive(value: false);
		m_AgariCall.transform.Find("Tsumo").gameObject.SetActive(value: true);
		m_AgariCall.transform.Find("Tsumo/IconTsumox2").gameObject.SetActive(GameDataManager.Mahjong.IsReversalEnableMode && !GameDataManager.Mahjong.IsAotenjoMode);
		await Process(cancellationToken);
		Destruct();
	}

	public async UniTask SetAgariRon(Ron cmd, List<Player> players, Player agariPlayer, Player furikomiPlayer, Wanpai wanpai, int pindex, Kaze bombKaze, CancellationToken cancellationToken = default(CancellationToken))
	{
		YakuData yakuData = cmd.Yakus[pindex];
		agariPlayer.hand.AddRonPai((Pai)yakuData.PaiID, MahjongUtility.IsReachAgari(yakuData)).Forget();
		agariPlayer.hand.SetNowTsumoChoice(flag: true);
		agariPlayer.hand.ResetPos(animation: false).Forget();
		agariPlayer.hand.DispOffHaiJudgeMark();
		long score = ((cmd.Scores[pindex].WhereFore == 1) ? MahjongUtility.GetScore(GameDataManager.Mahjong.TakuType, (Han)yakuData.HanID, yakuData.FuNum, agariPlayer.m_jikaze == Kaze.Ton, isTsumo: false).totalPayment : cmd.Scores[pindex].YakuScore);
		SetAgariCommon(yakuData, score, wanpai, cmd.OpenDoraNum, players, agariPlayer, isTsumo: false, bombKaze, agariPlayer.m_isOpenReach && !furikomiPlayer.m_isReach, furikomiPlayer).Forget();
		m_AgariPlayerText.text = agariPlayer.m_name;
		m_RonPlayerText.text = furikomiPlayer.m_name + "から";
		m_AgariCall.transform.Find("Ron").gameObject.SetActive(value: true);
		m_AgariCall.transform.Find("Tsumo").gameObject.SetActive(value: false);
		m_AgariCall.transform.Find("Tsumo/IconTsumox2").gameObject.SetActive(value: false);
		await Process(cancellationToken);
		agariPlayer.hand.DispOffHaiJudgeMark();
		Destruct();
	}

	private async UniTask SetAgariCommon(YakuData yaku, long score, Wanpai wanpai, int openDoraNum, List<Player> players, Player player, bool isTsumo, Kaze bombKaze, bool isOpenFurikomi = false, Player houjuPlayer = null)
	{
		base.gameObject.SetActive(value: true);
		Destruct();
		SoundManager.PlayBgm("BGM_RESULT1").Forget();
		bool isAotenjoMode = GameDataManager.Mahjong.IsAotenjoMode;
		m_isKazoeyakuman = (yaku.YakuBit & 0x80000) != 0;
		if (yaku.HanID >= 9 && !m_isKazoeyakuman && !isAotenjoMode)
		{
			m_FuText.GetComponent<Text>().text = "";
			m_HanText.GetComponent<Text>().text = "";
		}
		else
		{
			m_FuText.GetComponent<Text>().text = yaku.FuNum + "符";
			m_HanText.GetComponent<Text>().text = yaku.HanNum + "翻";
		}
		if (isAotenjoMode)
		{
			m_ScoreText.text = MahjongUtility.Aotenjo.GetYakuResultScore(yaku.HanNum, yaku.FuNum, isTsumo, bombKaze, player, houjuPlayer, players);
		}
		else
		{
			m_ScoreText.text = score.ToString("N0");
		}
		if (GameDataManager.TenshiGowanEvent.IsEnable())
		{
			GameDataManager.TenshiGowanEvent.AddThisGameScore(score);
			Debug.Log("豪腕麻雀スコア：" + score + " / Sum : " + GameDataManager.TenshiGowanEvent.ThisGameSumScore);
		}
		foreach (Transform item in m_handType.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		foreach (Transform item2 in m_handTypeAdd.transform)
		{
			item2.gameObject.SetActive(value: false);
		}
		m_han = (Han)yaku.HanID;
		m_isEndAnimeLong = false;
		if (!GameDataManager.Mahjong.IsAotenjoMode)
		{
			if (m_isKazoeyakuman)
			{
				m_handType.transform.Find("Kazoe").gameObject.SetActive(value: true);
				m_handType.transform.Find("Yakuman").gameObject.SetActive(value: true);
				m_handTypeAdd.transform.Find("Kazoe").gameObject.SetActive(value: true);
				m_handTypeAdd.transform.Find("Yakuman").gameObject.SetActive(value: true);
				GetComponent<Animator>().SetTrigger("Grade");
				m_isEndAnimeLong = true;
			}
			else if (yaku.HanID >= 9)
			{
				switch ((Han)yaku.HanID)
				{
				case Han.Yakuman2:
					m_handType.transform.Find("2bai").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("2bai").gameObject.SetActive(value: true);
					break;
				case Han.Yakuman3:
					m_handType.transform.Find("3bai").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("3bai").gameObject.SetActive(value: true);
					break;
				case Han.Yakuman4:
					m_handType.transform.Find("4bai").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("4bai").gameObject.SetActive(value: true);
					break;
				case Han.Yakuman5:
					m_handType.transform.Find("5bai").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("5bai").gameObject.SetActive(value: true);
					break;
				case Han.Yakuman6:
					m_handType.transform.Find("6bai").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("6bai").gameObject.SetActive(value: true);
					break;
				case Han.Yakuman7:
					m_handType.transform.Find("7bai").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("7bai").gameObject.SetActive(value: true);
					break;
				}
				m_handType.transform.Find("Yakuman").gameObject.SetActive(value: true);
				m_handTypeAdd.transform.Find("Yakuman").gameObject.SetActive(value: true);
				GetComponent<Animator>().SetTrigger("Grade");
				m_isEndAnimeLong = true;
			}
			else if (yaku.HanID >= 5)
			{
				switch ((Han)yaku.HanID)
				{
				case Han.Mangan:
					m_handType.transform.Find("Mangan").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("Mangan").gameObject.SetActive(value: true);
					break;
				case Han.Haneman:
					m_handType.transform.Find("Haneman").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("Haneman").gameObject.SetActive(value: true);
					break;
				case Han.Baiman:
					m_handType.transform.Find("Baiman").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("Baiman").gameObject.SetActive(value: true);
					break;
				case Han.Sanbaiman:
					m_handType.transform.Find("Sanbaiman").gameObject.SetActive(value: true);
					m_handTypeAdd.transform.Find("Sanbaiman").gameObject.SetActive(value: true);
					break;
				}
				GetComponent<Animator>().SetTrigger("Grade");
				m_isEndAnimeLong = true;
			}
		}
		if (player.m_cha == Cha.Jicha && yaku.HanID >= 5)
		{
			GameDataManager.Mission.HoraHanList.Add((Han)yaku.HanID);
		}
		bool flag = false;
		foreach (GameObject yaku3 in m_yakuList)
		{
			yaku3.SetActive(value: false);
		}
		m_yakuDispNum = 0;
		m_yakuInfoList.Clear();
		m_doraNum = 0;
		Yaku[] array = (GameDataManager.Mahjong.IsMentanpinMode ? (from x in YakuPriority.ToList()
			orderby x == Yaku.DoubleRichi || x == Yaku.Richi descending, x == Yaku.Tanyao descending, x == Yaku.Pinfu descending
			select x).ToArray() : YakuPriority);
		foreach (Yaku yaku2 in array)
		{
			bool flag2 = ((player.m_isDoubleReach && yaku2 == Yaku.DoubleRichi) || (player.m_isReach && !player.m_isDoubleReach && yaku2 == Yaku.Richi)) && player.m_isOpenReach && isOpenFurikomi;
			if (!((yaku.YakuBit & (ulong)(1L << (int)yaku2)) != 0 || flag2) || yaku2 == Yaku.Kazoeyakuman)
			{
				continue;
			}
			int hanNum = MahjongUtility.GetHanNum(yaku2, player.m_isFireReach, player.m_isOpenReach, player.m_isAtomicReach, flag2);
			if (yaku2 == Yaku.Dora)
			{
				SetYakuFrame(isYakumanFrame: false, MahjongUtility.GetYakuKanaName(yaku2) + yaku.DoraNum, 0, GetHanFrame(hanNum));
				m_doraNum = yaku.DoraNum;
			}
			else if (flag2)
			{
				bool flag3 = m_isKazoeyakuman || isAotenjoMode;
				int han = (flag3 ? MahjongUtility.GetHanNum(yaku2, player.m_isFireReach, player.m_isOpenReach, player.m_isAtomicReach, isOpenFurikomi) : 0);
				SetYakuFrame(!flag3, "オープン" + MahjongUtility.GetYakuKanaName(yaku2), han, GetHanFrame(hanNum));
			}
			else if ((yaku2 == Yaku.Nagashimangan || MahjongUtility.GetHanNum(yaku2, player.m_isFireReach, player.m_isOpenReach, player.m_isAtomicReach, isOpenFurikomi) >= 13) && !isAotenjoMode)
			{
				SetYakuFrame(isYakumanFrame: true, MahjongUtility.GetYakuKanaName(yaku2), 0, GetHanFrame(hanNum));
			}
			else
			{
				bool isFireEffect = player.m_isFireReach && (yaku2 == Yaku.Richi || yaku2 == Yaku.DoubleRichi);
				bool isAtomicReachEffect = player.m_isAtomicReach && (yaku2 == Yaku.Richi || yaku2 == Yaku.DoubleRichi);
				bool isMentanpinEffect = GameDataManager.Mahjong.IsMentanpinMode && (yaku2 == Yaku.Richi || yaku2 == Yaku.DoubleRichi || yaku2 == Yaku.Tanyao || yaku2 == Yaku.Pinfu);
				string yakuNameText = (((yaku2 == Yaku.Richi || yaku2 == Yaku.DoubleRichi) && player.m_isOpenReach) ? "オープン" : "") + MahjongUtility.GetYakuKanaName(yaku2);
				SetYakuFrame(isYakumanFrame: false, yakuNameText, hanNum, GetHanFrame(hanNum), YakuExtention.IsNakiYaku(yaku2), isFireEffect, isMentanpinEffect, isComebackEffect: false, isPCKEffect: false, isAllGreenEffect: false, isAtomicReachEffect);
			}
			if (yaku2 == Yaku.Richi || yaku2 == Yaku.DoubleRichi)
			{
				flag = true;
			}
			m_yakuInfoList.Add(new YakuInfo(yaku2, hanNum, YakuExtention.IsNakiYaku(yaku2)));
			if (player.m_cha == Cha.Jicha)
			{
				GameDataManager.Mission.HoraYakuList.Add(yaku2);
				if (GameDataManager.BingoEventRemake.IsEnable())
				{
					GameDataManager.BingoEventRemake.SetClearYaku(yaku2);
				}
			}
			m_yakuDispNum++;
			if (m_yakuDispNum >= _maxYakuListNum)
			{
				break;
			}
		}
		if (yaku.BonusHan > 0 && m_yakuDispNum < _maxYakuListNum && (yaku.HanID < 9 || m_isKazoeyakuman || GameDataManager.Mahjong.IsAotenjoMode))
		{
			if (GameDataManager.Mahjong.IsAotenjoMode)
			{
				int bonusHan = player.m_comebackTakuBonusDisp.bonusHan;
				int num = MahjongUtility.MeldBonus.MeldCount(players);
				int num2 = yaku.BonusHan - bonusHan - num;
				if (bonusHan > 0)
				{
					SetYakuFrame(isYakumanFrame: false, "革命手当", bonusHan, GetHanFrame(bonusHan), isHanDownIcon: false, isFireEffect: false, isMentanpinEffect: false, isComebackEffect: true);
					m_yakuInfoList.Add(new YakuInfo((Yaku)61, bonusHan, isKuisagari: false, isBonus: true));
					m_yakuDispNum++;
				}
				if (num > 0 && m_yakuDispNum < _maxYakuListNum)
				{
					SetYakuFrame(isYakumanFrame: false, "PCK手当", num, GetHanFrame(num, isPCK: true), isHanDownIcon: false, isFireEffect: false, isMentanpinEffect: false, isComebackEffect: false, isPCKEffect: true);
					m_yakuInfoList.Add(new YakuInfo((Yaku)61, num, isKuisagari: false, isBonus: true));
					m_yakuDispNum++;
				}
				if (num2 > 0 && m_yakuDispNum < _maxYakuListNum)
				{
					SetYakuFrame(isYakumanFrame: false, "全緑手当", num2, GetHanFrame(num2), isHanDownIcon: false, isFireEffect: false, isMentanpinEffect: false, isComebackEffect: false, isPCKEffect: false, isAllGreenEffect: true);
					m_yakuInfoList.Add(new YakuInfo((Yaku)61, num2, isKuisagari: false, isBonus: true));
					m_yakuDispNum++;
				}
			}
			else if (GameDataManager.Mahjong.IsBonusEnableMode)
			{
				bool isMeldBonusMode = GameDataManager.Mahjong.IsMeldBonusMode;
				bool isAllGreenBonusMode = GameDataManager.Mahjong.IsAllGreenBonusMode;
				SetYakuFrame(isYakumanFrame: false, isMeldBonusMode ? "PCK手当" : (isAllGreenBonusMode ? "全緑手当" : "革命手当"), yaku.BonusHan, GetHanFrame(yaku.BonusHan, isMeldBonusMode), isHanDownIcon: false, isFireEffect: false, isMentanpinEffect: false, !isMeldBonusMode && !isAllGreenBonusMode, isMeldBonusMode && !isAllGreenBonusMode, !isMeldBonusMode && isAllGreenBonusMode);
				m_yakuInfoList.Add(new YakuInfo((Yaku)61, yaku.BonusHan, isKuisagari: false, isBonus: true));
				m_yakuDispNum++;
			}
		}
		MotifYakuJudge(player.m_charaType, player.m_startKaze);
		Hand hand = UnityEngine.Object.Instantiate(player.hand, agariPlayer.transform);
		Meld meld = UnityEngine.Object.Instantiate(player.meld, agariPlayer.transform);
		UnityEngine.Object.Instantiate(wanpai.dora, agariWanpai.transform).AlignOneLine();
		if (flag)
		{
			UnityEngine.Object.Instantiate(wanpai.uradora, agariWanpai.transform).AlignOneLine();
		}
		hand.transform.localScale = Vector3.one;
		hand.transform.localPosition = new Vector3(-0.42f, 0f, -0.45f);
		hand.StopTenpaiEffect();
		hand.StartDoraEffect(flag);
		meld.transform.localScale = Vector3.one;
		meld.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		meld.transform.localPosition = new Vector3(0.36f, 0f, -0.45f);
		meld.StopTenpaiEffect();
		meld.StartDoraEffect(flag);
		m_charaType = player.m_charaType;
		GetComponent<Animator>().ResetTrigger("EndBig");
		GetComponent<Animator>().ResetTrigger("EndSmall");
		GetComponent<Animator>().ResetTrigger("Yaku");
		GetComponent<Animator>().ResetTrigger("YakuMany");
		if (m_yakuDispNum > 10)
		{
			GetComponent<Animator>().SetTrigger("YakuMany");
		}
		else
		{
			GetComponent<Animator>().SetTrigger("Yaku");
		}
		GetComponent<Animator>().SetTrigger("Start");
	}

	private void SetYakuFrame(bool isYakumanFrame, string yakuNameText, int han, Sprite hanFrame, bool isHanDownIcon = false, bool isFireEffect = false, bool isMentanpinEffect = false, bool isComebackEffect = false, bool isPCKEffect = false, bool isAllGreenEffect = false, bool isAtomicReachEffect = false)
	{
		m_yakuList[m_yakuDispNum].transform.Find("Frame/YakuName").GetComponent<Text>().text = yakuNameText ?? "";
		m_yakuList[m_yakuDispNum].transform.Find("FrameYakuman/YakuName").GetComponent<Text>().text = yakuNameText ?? "";
		m_yakuList[m_yakuDispNum].transform.Find("Han").GetComponent<Text>().text = ((han > 0) ? (han + "翻") : "");
		m_yakuList[m_yakuDispNum].transform.Find("Frame").gameObject.SetActive(!isYakumanFrame);
		m_yakuList[m_yakuDispNum].transform.Find("FrameYakuman").gameObject.SetActive(isYakumanFrame);
		m_yakuList[m_yakuDispNum].transform.Find("Frame").Find("FireEf").gameObject.SetActive(isFireEffect);
		m_yakuList[m_yakuDispNum].transform.Find("Frame").Find("MentanpinEf").gameObject.SetActive(isMentanpinEffect && !isFireEffect);
		m_yakuList[m_yakuDispNum].transform.Find("Frame").Find("AtomicEf").gameObject.SetActive(isAtomicReachEffect && !isMentanpinEffect && !isFireEffect);
		m_yakuList[m_yakuDispNum].transform.Find("Frame").Find("ComebackEf").gameObject.SetActive(!isAllGreenEffect && !isPCKEffect && isComebackEffect && !isMentanpinEffect && !isFireEffect && !isAtomicReachEffect);
		m_yakuList[m_yakuDispNum].transform.Find("Frame").Find("PCKEf").gameObject.SetActive(!isAllGreenEffect && isPCKEffect && !isComebackEffect && !isMentanpinEffect && !isFireEffect && !isAtomicReachEffect);
		m_yakuList[m_yakuDispNum].transform.Find("Frame").Find("AllGreenEf").gameObject.SetActive(isAllGreenEffect && !isPCKEffect && !isComebackEffect && !isMentanpinEffect && !isFireEffect && !isAtomicReachEffect);
		m_yakuList[m_yakuDispNum].transform.Find("HanDownIcon").gameObject.SetActive(isHanDownIcon);
		m_yakuList[m_yakuDispNum].transform.Find("Frame").GetComponent<Image>().sprite = hanFrame;
	}

	private Sprite GetHanFrame(int han, bool isPCK = false)
	{
		if (isPCK && han >= 7)
		{
			return m_Frame7Sprite;
		}
		return han switch
		{
			1 => m_Frame1Sprite, 
			2 => m_Frame2Sprite, 
			3 => m_Frame3Sprite, 
			4 => m_Frame4Sprite, 
			5 => m_Frame5Sprite, 
			6 => m_Frame6Sprite, 
			7 => m_Frame7Sprite, 
			13 => m_Frame13Sprite, 
			26 => m_Frame13Sprite, 
			_ => m_Frame1Sprite, 
		};
	}

	private void Destruct()
	{
		foreach (Transform item in agariPlayer.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in agariWanpai.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
	}

	private void MotifYakuJudge(CharaType charaType, Kaze kaze)
	{
		List<Yaku> list = m_yakuInfoList.Select((YakuInfo x) => x.m_yaku).ToList();
		Yaku yaku = MasterGameDataManager.Character.Properties(charaType).Profile.Yaku1;
		Yaku yaku2 = MasterGameDataManager.Character.Properties(charaType).Profile.Yaku2;
		if (list.Contains(yaku) || list.Contains(yaku2))
		{
			List<Yaku> list2 = new List<Yaku>
			{
				yaku,
				yaku2,
				Yaku.Dora,
				Yaku.Richi,
				Yaku.DoubleRichi,
				Yaku.Menzen
			};
			if (yaku == Yaku.Syosangen || yaku2 == Yaku.Syosangen)
			{
				list2.Add(Yaku.Haku);
				list2.Add(Yaku.Hatsu);
				list2.Add(Yaku.Tyun);
			}
			if (list.Except(list2).Count() == 0)
			{
				VoiceManager.m_charaUniqueYakuAgari = true;
				VoiceManager.m_charaUniqueYakuAgariIndex.Add(kaze);
			}
		}
	}

	private bool IsSkip()
	{
		if (MahjongUtility.IsNoInputAutoTsumogiri() || ResidentFlow.CustomInput.GetMouseButtonDown(0))
		{
			return true;
		}
		if (GameDataManager.PlayerConfig.IsKeyboardPlay() && Bi2xInput.GetKeyDown(KeyCode.Return))
		{
			return true;
		}
		return false;
	}
}

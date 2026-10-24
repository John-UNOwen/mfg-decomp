using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData.EventTaku;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class KyokuCall : MonoBehaviour
{
	[SerializeField]
	public PlayableDirector m_kyokuCall;

	[SerializeField]
	public GameObject m_honba;

	[SerializeField]
	public GameObject m_orasu;

	[SerializeField]
	public GameObject m_bakaze;

	[SerializeField]
	public GameObject m_kyokuNum;

	[SerializeField]
	public GameObject m_kyoku;

	[SerializeField]
	private GameObject m_oyaTurnEffect;

	[SerializeField]
	private GameObject m_wanpaiTexture;

	public bool m_isOrasu;

	private int m_kyokuCount;

	private bool m_isAnimationStopped;

	public async UniTask Play(bool isOrasu, int ba, int kyoku, int honba, Kaze kaze, Rank rank, MahjongMusicTextDisplay musicText, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_isAnimationStopped = false;
		List<string> list = new List<string>
		{
			"〇", "一", "二", "三", "四", "五", "六", "七", "八", "九",
			"十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九",
			"二十"
		};
		base.gameObject.SetActive(value: true);
		if (MahjongUtility.GetViewType() == ViewType.Type1 || MahjongUtility.GetViewType() == ViewType.Type3)
		{
			m_wanpaiTexture.SetActive(value: false);
		}
		else
		{
			m_wanpaiTexture.SetActive(value: true);
		}
		m_kyokuCall.gameObject.SetActive(value: true);
		m_kyokuCall.stopped += TimeLineStopped;
		m_isOrasu = isOrasu;
		m_kyokuCount = kyoku;
		if (honba == 0)
		{
			m_honba.SetActive(value: false);
		}
		else if (honba <= 20)
		{
			m_honba.SetActive(value: true);
			m_honba.GetComponentInChildren<TextMeshProUGUI>().text = list[honba] + "本場";
		}
		else
		{
			m_honba.SetActive(value: false);
		}
		if (isOrasu)
		{
			m_orasu.SetActive(value: true);
			m_bakaze.SetActive(value: false);
			m_kyokuNum.SetActive(value: false);
			m_kyoku.SetActive(value: false);
			await SoundManager.PlayMyCharaVoice("VOX_GXX_MJG_AURUS1");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(3.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitWhile(() => SoundManager.IsPlayLastCalledVoice()));
		}
		else
		{
			m_orasu.SetActive(value: false);
			m_bakaze.SetActive(value: true);
			m_kyokuNum.SetActive(value: true);
			m_kyoku.SetActive(value: true);
			m_bakaze.GetComponentInChildren<TextMeshProUGUI>().text = ((ba == 0) ? "東" : "南");
			m_kyokuNum.GetComponentInChildren<TextMeshProUGUI>().text = list[kyoku + 1];
			await SoundManager.PlayMyCharaVoice((new string[2][]
			{
				new string[4] { "VOX_GXX_MJG_TONG1", "VOX_GXX_MJG_TONG2", "VOX_GXX_MJG_TONG3", "VOX_GXX_MJG_TONG4" },
				new string[4] { "VOX_GXX_MJG_NANG1", "VOX_GXX_MJG_NANG2", "VOX_GXX_MJG_NANG3", "VOX_GXX_MJG_NANG4" }
			})[ba][kyoku]);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitWhile(() => SoundManager.IsPlayLastCalledVoice()));
		}
		if (0 < honba && honba <= 10)
		{
			await SoundManager.PlayMyCharaVoice((new string[11]
			{
				"VOX_GXX_MJG_HONBA1", "VOX_GXX_MJG_HONBA1", "VOX_GXX_MJG_HONBA2", "VOX_GXX_MJG_HONBA3", "VOX_GXX_MJG_HONBA4", "VOX_GXX_MJG_HONBA5", "VOX_GXX_MJG_HONBA6", "VOX_GXX_MJG_HONBA7", "VOX_GXX_MJG_HONBA8", "VOX_GXX_MJG_HONBA9",
				"VOX_GXX_MJG_HONBA10"
			})[honba]);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitWhile(() => SoundManager.IsPlayLastCalledVoice()));
		}
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => m_isAnimationStopped, PlayerLoopTiming.Update, cancellationToken));
		base.gameObject.SetActive(value: false);
		(string, string) bgm = GetBgm(GameDataManager.Mahjong.TakuType, ba, kyoku);
		SoundManager.PlayBgm(bgm.Item1).Forget();
		musicText.MusicIn(bgm.Item2);
	}

	private (string label, string name) GetBgm(TakuType takuType, int ba, int kyoku)
	{
		if (ba < 0 || 1 < ba)
		{
			return (label: "BGM_4MJG1_1", name: "東一局");
		}
		if (kyoku < 0 || 3 < kyoku)
		{
			return (label: "BGM_4MJG1_1", name: "東一局");
		}
		bool isValidCompetition = GameDataManager.Taikyoku.IsValidCompetition;
		bool flag = GameDataManager.EventTaku.ActiveAtTaikyokuGameDatas().Any();
		bool isScrambleMatch = GameDataManager.Taikyoku.IsScrambleMatch;
		bool isMotherWars = GameDataManager.Taikyoku.IsMotherWars;
		(string, string)[][] array = new(string, string)[2][]
		{
			new(string, string)[4]
			{
				("BGM_4MJG1_1", "東一局"),
				("BGM_4MJG1_2", "東二局"),
				("BGM_4MJG1_3", "東三局"),
				("BGM_4MJG1_4", "東四局")
			},
			new(string, string)[4]
			{
				("BGM_4MJG1_1", "南一局"),
				("BGM_4MJG1_2", "南二局"),
				("BGM_4MJG1_3", "南三局"),
				("BGM_4MJG1_4", "南四局")
			}
		};
		(string, string)[][] array2 = new(string, string)[2][]
		{
			new(string, string)[4]
			{
				("BGM_4MJG2_1", "大会 東一局"),
				("BGM_4MJG2_2", "大会 東二局"),
				("BGM_4MJG2_3", "大会 東三局"),
				("BGM_4MJG2_4", "大会 東四局")
			},
			new(string, string)[4]
			{
				("BGM_4MJG2_1", "大会 南一局"),
				("BGM_4MJG2_2", "大会 南二局"),
				("BGM_4MJG2_3", "大会 南三局"),
				("BGM_4MJG2_4", "大会 南四局")
			}
		};
		(string, string)[][] array3 = new(string, string)[2][]
		{
			new(string, string)[4]
			{
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)")
			},
			new(string, string)[4]
			{
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)")
			}
		};
		(string, string)[][] array4 = new(string, string)[2][]
		{
			new(string, string)[4]
			{
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)")
			},
			new(string, string)[4]
			{
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)")
			}
		};
		(string, string)[][] array5 = new(string, string)[2][]
		{
			new(string, string)[4]
			{
				("BGM_3MJG1_1", "東一局"),
				("BGM_3MJG1_2", "東二局"),
				("BGM_3MJG1_3", "東三局"),
				("", "")
			},
			new(string, string)[4]
			{
				("BGM_3MJG1_4", "南一局"),
				("BGM_3MJG1_5", "南二局"),
				("BGM_3MJG1_6", "南三局"),
				("", "")
			}
		};
		(string, string)[][] array6 = new(string, string)[2][]
		{
			new(string, string)[4]
			{
				("BGM_3MJG2_1", "大会 東一局"),
				("BGM_3MJG2_2", "大会 東二局"),
				("BGM_3MJG2_3", "大会 東三局"),
				("", "")
			},
			new(string, string)[4]
			{
				("BGM_3MJG2_4", "大会 南一局"),
				("BGM_3MJG2_5", "大会 南二局"),
				("BGM_3MJG2_6", "大会 南三局"),
				("", "")
			}
		};
		(string, string)[][] array7 = new(string, string)[2][]
		{
			new(string, string)[4]
			{
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("", "")
			},
			new(string, string)[4]
			{
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("", "")
			}
		};
		(string, string)[][] array8 = new(string, string)[2][]
		{
			new(string, string)[4]
			{
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("BGM_MJG_SCRAMBLE1", "スクランブルマッチ(前半戦)"),
				("", "")
			},
			new(string, string)[4]
			{
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("BGM_MJG_SCRAMBLE2", "スクランブルマッチ(後半戦)"),
				("", "")
			}
		};
		(string, string)[][] array9 = new(string, string)[2][]
		{
			new(string, string)[4]
			{
				("BGM_2MJG1_1", "東一局"),
				("BGM_2MJG1_2", "東二局"),
				("", ""),
				("", "")
			},
			new(string, string)[4]
			{
				("BGM_2MJG1_3", "南一局"),
				("BGM_2MJG1_4", "南二局"),
				("", ""),
				("", "")
			}
		};
		(string, string) result = ("BGM_4MJG1_1", "東一局");
		switch (takuType)
		{
		case TakuType.Tonpu:
			if (isMotherWars)
			{
				return array4[ba][kyoku];
			}
			if (isScrambleMatch)
			{
				return array3[ba][kyoku];
			}
			if (isValidCompetition)
			{
				if (flag)
				{
					BaseEventGameData baseEventGameData3 = GameDataManager.EventTaku.ActiveAtTaikyokuGameDatas().First();
					bool flag3 = baseEventGameData3.IsExtraBGMAvailable();
					return flag3 ? baseEventGameData3.Master.BgmLabelName((Ba)ba, (Kyoku)kyoku, takuType, flag3) : array2[ba][kyoku];
				}
				return array2[ba][kyoku];
			}
			if (flag)
			{
				BaseEventGameData baseEventGameData4 = GameDataManager.EventTaku.ActiveAtTaikyokuGameDatas().First();
				bool isExtra2 = baseEventGameData4.IsExtraBGMAvailable();
				return baseEventGameData4.Master.BgmLabelName((Ba)ba, (Kyoku)kyoku, takuType, isExtra2);
			}
			return array[ba][kyoku];
		case TakuType.Sanma:
			if (isMotherWars)
			{
				return array8[ba][kyoku];
			}
			if (isScrambleMatch)
			{
				return array7[ba][kyoku];
			}
			if (isValidCompetition)
			{
				if (flag)
				{
					BaseEventGameData baseEventGameData = GameDataManager.EventTaku.ActiveAtTaikyokuGameDatas().First();
					bool flag2 = baseEventGameData.IsExtraBGMAvailable();
					return flag2 ? baseEventGameData.Master.BgmLabelName((Ba)ba, (Kyoku)kyoku, takuType, flag2) : array6[ba][kyoku];
				}
				return array6[ba][kyoku];
			}
			if (flag)
			{
				BaseEventGameData baseEventGameData2 = GameDataManager.EventTaku.ActiveAtTaikyokuGameDatas().First();
				bool isExtra = baseEventGameData2.IsExtraBGMAvailable();
				return baseEventGameData2.Master.BgmLabelName((Ba)ba, (Kyoku)kyoku, takuType, isExtra);
			}
			return array5[ba][kyoku];
		case TakuType.Nima:
			return array9[ba][kyoku];
		default:
			return result;
		}
	}

	private async void TimeLineStopped(PlayableDirector pd)
	{
		m_isAnimationStopped = true;
	}

	public async UniTask OyaTurnEffect()
	{
		m_oyaTurnEffect.SetActive(value: true);
		m_oyaTurnEffect.GetComponent<Animator>().SetTrigger("Next");
	}
}

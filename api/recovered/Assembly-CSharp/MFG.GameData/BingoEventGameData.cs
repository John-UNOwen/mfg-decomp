using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Types;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class BingoEventGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "bingo_event";

	[NonSerialized]
	public const int BingoSheetNum = 9;

	[NonSerialized]
	public const int BingoHoleNum = 9;

	public int ProceedingBingoSheetNo;

	public List<bool> BingoHoles = new List<bool>();

	[NonSerialized]
	public List<bool> ThisPlayCleardHole = new List<bool>();

	private static List<Yaku[]> m_BingoMasterData = new List<Yaku[]>
	{
		new Yaku[9]
		{
			Yaku.Richi,
			Yaku.Ikkitsukan,
			Yaku.Menzen,
			Yaku.Chanta,
			Yaku.Pinfu,
			Yaku.Sansyokudojun,
			Yaku.Ipeiko,
			Yaku.Chiniso,
			Yaku.Tanyao
		},
		new Yaku[9]
		{
			Yaku.Haku,
			Yaku.Hatsu,
			Yaku.Tyun,
			Yaku.Chitoitsu,
			Yaku.Toitoiho,
			Yaku.Sananko,
			Yaku.Jikaze,
			Yaku.Dora,
			Yaku.Bakaze
		},
		new Yaku[9]
		{
			Yaku.Pinfu,
			Yaku.Menzen,
			Yaku.Sananko,
			Yaku.Ipeiko,
			Yaku.Richi,
			Yaku.Chitoitsu,
			Yaku.Dora,
			Yaku.Ippatsu,
			Yaku.Tanyao
		},
		new Yaku[9]
		{
			Yaku.Chitoitsu,
			Yaku.Chitoitsu,
			Yaku.Chitoitsu,
			Yaku.Sananko,
			Yaku.Sananko,
			Yaku.Sananko,
			Yaku.Toitoiho,
			Yaku.Toitoiho,
			Yaku.Toitoiho
		},
		new Yaku[9]
		{
			Yaku.Richi,
			Yaku.Sansyokudojun,
			Yaku.Toitoiho,
			Yaku.Chitoitsu,
			Yaku.Tanyao,
			Yaku.Menzen,
			Yaku.Ipeiko,
			Yaku.Sansyokudoko,
			Yaku.Pinfu
		},
		new Yaku[9]
		{
			Yaku.Tanyao,
			Yaku.Tanyao,
			Yaku.Sananko,
			Yaku.Ryanpeko,
			Yaku.Chitoitsu,
			Yaku.Sananko,
			Yaku.Ryanpeko,
			Yaku.Toitoiho,
			Yaku.Toitoiho
		},
		new Yaku[9]
		{
			Yaku.Chanta,
			Yaku.Syosangen,
			Yaku.Honiso,
			Yaku.Honroto,
			Yaku.KokushiTanki,
			Yaku.Toitoiho,
			Yaku.Richi,
			Yaku.Junchan,
			Yaku.Chitoitsu
		},
		new Yaku[9]
		{
			Yaku.Ippatsu,
			Yaku.Tenho,
			Yaku.Rinsyan,
			Yaku.Tenho,
			Yaku.Menzen,
			Yaku.Tenho,
			Yaku.Haitei,
			Yaku.Tenho,
			Yaku.Dora
		},
		new Yaku[9]
		{
			Yaku.Kokushi13,
			Yaku.Junchan,
			Yaku.Tyuren9,
			Yaku.Chiniso,
			Yaku.Richi,
			Yaku.Honiso,
			Yaku.SuankoTanki,
			Yaku.Sansyokudojun,
			Yaku.Daisushi
		}
	};

	private static List<string> m_BingoMasterDataRewardItemNames = new List<string> { "ナナツノグループ ロゴマーク", "ファイトスピリット（だれでも）×１", "ファイトスピリット（だれでも）×２", "ナナツノグループ オリジナルリーチ棒", "ファイトスピリット（だれでも）×３", "ファイトスピリット（だれでも）×４", "ファイトスピリット（だれでも）×５", "ナナツノグループ オリジナルマスコット", "ファイトスピリット（パイン）×２０" };

	public int LoopBingoSheetNo => ProceedingBingoSheetNo % 9;

	public BingoEventGameData()
	{
		Init();
		Reset();
	}

	public bool IsEnable(bool takuTypeJudge = true)
	{
		return false;
	}

	public void Init()
	{
		ThisPlayCleardHole.Clear();
		for (int i = 0; i < 9; i++)
		{
			ThisPlayCleardHole.Add(item: false);
		}
	}

	public void Reset()
	{
		BingoHoles.Clear();
		for (int i = 0; i < 9; i++)
		{
			BingoHoles.Add(item: false);
		}
	}

	public bool IsBingoComplete()
	{
		if (BingoHoles[0] && BingoHoles[1] && BingoHoles[2])
		{
			return true;
		}
		if (BingoHoles[3] && BingoHoles[4] && BingoHoles[5])
		{
			return true;
		}
		if (BingoHoles[6] && BingoHoles[7] && BingoHoles[8])
		{
			return true;
		}
		if (BingoHoles[0] && BingoHoles[3] && BingoHoles[6])
		{
			return true;
		}
		if (BingoHoles[1] && BingoHoles[4] && BingoHoles[7])
		{
			return true;
		}
		if (BingoHoles[2] && BingoHoles[5] && BingoHoles[8])
		{
			return true;
		}
		if (BingoHoles[0] && BingoHoles[4] && BingoHoles[8])
		{
			return true;
		}
		if (BingoHoles[2] && BingoHoles[4] && BingoHoles[6])
		{
			return true;
		}
		return false;
	}

	public void SetClearYaku(Yaku yaku)
	{
		switch (yaku)
		{
		case Yaku.ChinisoNaki:
			yaku = Yaku.Chiniso;
			break;
		case Yaku.JunchanNaki:
			yaku = Yaku.Junchan;
			break;
		case Yaku.HonisoNaki:
			yaku = Yaku.Honiso;
			break;
		case Yaku.SansyokudojunNaki:
			yaku = Yaku.Sansyokudojun;
			break;
		case Yaku.ChantaNaki:
			yaku = Yaku.Chanta;
			break;
		case Yaku.IkkitsukanNaki:
			yaku = Yaku.Ikkitsukan;
			break;
		}
		int loopBingoSheetNo = LoopBingoSheetNo;
		for (int i = 0; i < 9; i++)
		{
			Yaku yaku2 = m_BingoMasterData[loopBingoSheetNo][i];
			if (yaku2 == Yaku.TyurenTanki && yaku == Yaku.Tyuren9)
			{
				ClearBingoHole(i);
			}
			else if (yaku2 == Yaku.KokushiTanki && yaku == Yaku.Kokushi13)
			{
				ClearBingoHole(i);
			}
			else if (yaku2 == Yaku.Suanko && yaku == Yaku.SuankoTanki)
			{
				ClearBingoHole(i);
			}
			else if (m_BingoMasterData[loopBingoSheetNo][i] == yaku)
			{
				ClearBingoHole(i);
			}
		}
	}

	private void ClearBingoHole(int index)
	{
		if (!BingoHoles[index])
		{
			GameDataManager.BingoEvent.ThisPlayCleardHole[index] = true;
		}
		BingoHoles[index] = true;
	}

	public bool IsBingoTargetYaku(Yaku yaku)
	{
		int loopBingoSheetNo = LoopBingoSheetNo;
		for (int i = 0; i < 9; i++)
		{
			if (!BingoHoles[i] && m_BingoMasterData[loopBingoSheetNo][i] == yaku)
			{
				return true;
			}
		}
		return false;
	}

	public string GetRewardItemName(int bingoSheetNo)
	{
		if (0 <= bingoSheetNo && bingoSheetNo < 9)
		{
			return m_BingoMasterDataRewardItemNames[bingoSheetNo];
		}
		return "ファイトスピリット（だれでも）×１";
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("BingoEventGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("bingo_event", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static BingoEventGameData LoadFromJson(string jsonString)
	{
		Debug.Log("BingoEventGameData Load : " + jsonString);
		return JsonUtility.FromJson<BingoEventGameData>(jsonString);
	}
}

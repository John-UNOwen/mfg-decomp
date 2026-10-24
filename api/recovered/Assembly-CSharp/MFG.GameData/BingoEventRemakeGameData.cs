using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Types;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class BingoEventRemakeGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const GameEventType CurrentBingoEvent = GameEventType.YakuBingo3;

	public const string ClientStateName = "bingo_event_remake";

	[NonSerialized]
	public const int BingoHoleNum = 9;

	[NonSerialized]
	public const int BingoLineNum = 8;

	public List<FryPanInfo> BingoFryPanNumList = new List<FryPanInfo>();

	public bool IsSpecialCard;

	public int SpecialCardRejectionCount;

	public Yaku[] BingoCardYakuList = new Yaku[0];

	public List<bool> BingoHoles = new List<bool>();

	public List<int> BingoScoreHoles = new List<int>();

	[NonSerialized]
	public List<bool> ThisPlayCleardHole = new List<bool>();

	[NonSerialized]
	public bool[] BeforeBingoLine = new bool[8];

	[NonSerialized]
	public bool[] AfterBingoLine = new bool[8];

	public int BingoCardNo = 1;

	public readonly int[][] BingoLineNumbers = new int[8][]
	{
		new int[3] { 0, 1, 2 },
		new int[3] { 3, 4, 5 },
		new int[3] { 6, 7, 8 },
		new int[3] { 0, 3, 6 },
		new int[3] { 1, 4, 7 },
		new int[3] { 2, 5, 8 },
		new int[3] { 0, 4, 8 },
		new int[3] { 2, 4, 6 }
	};

	private static List<int[]> bingoTableMaster = new List<int[]>
	{
		new int[9] { 4, 2, 5, 3, 0, 2, 5, 3, 4 },
		new int[9] { 5, 2, 5, 3, 0, 3, 4, 2, 4 },
		new int[9] { 1, 2, 3, 2, 0, 4, 3, 4, 5 },
		new int[9] { 2, 2, 5, 3, 0, 5, 3, 4, 4 },
		new int[9] { 5, 2, 5, 1, 0, 3, 5, 4, 5 },
		new int[9] { 2, 5, 2, 5, 0, 5, 2, 5, 2 },
		new int[9] { 3, 4, 3, 4, 0, 4, 3, 4, 3 },
		new int[9] { 4, 3, 4, 3, 0, 3, 4, 3, 4 }
	};

	private static Dictionary<int, List<Yaku>> yakuDifficultyTableMaster = new Dictionary<int, List<Yaku>>
	{
		{
			0,
			new List<Yaku> { Yaku.Max }
		},
		{
			1,
			new List<Yaku> { Yaku.Richi }
		},
		{
			2,
			new List<Yaku>
			{
				Yaku.Richi,
				Yaku.Pinfu,
				Yaku.Tanyao,
				Yaku.Menzen
			}
		},
		{
			3,
			new List<Yaku>
			{
				Yaku.Richi,
				Yaku.Pinfu,
				Yaku.Tanyao,
				Yaku.Menzen,
				Yaku.Haku,
				Yaku.Hatsu,
				Yaku.Tyun,
				Yaku.Jikaze,
				Yaku.Bakaze
			}
		},
		{
			4,
			new List<Yaku>
			{
				Yaku.Richi,
				Yaku.Pinfu,
				Yaku.Tanyao,
				Yaku.Menzen,
				Yaku.Haku,
				Yaku.Hatsu,
				Yaku.Tyun,
				Yaku.Jikaze,
				Yaku.Bakaze,
				Yaku.Toitoiho,
				Yaku.Ipeiko,
				Yaku.Sansyokudojun,
				Yaku.Honiso,
				Yaku.Ippatsu
			}
		},
		{
			5,
			new List<Yaku>
			{
				Yaku.Richi,
				Yaku.Pinfu,
				Yaku.Tanyao,
				Yaku.Menzen,
				Yaku.Haku,
				Yaku.Hatsu,
				Yaku.Tyun,
				Yaku.Jikaze,
				Yaku.Bakaze,
				Yaku.Toitoiho,
				Yaku.Ipeiko,
				Yaku.Sansyokudojun,
				Yaku.Honiso,
				Yaku.Ippatsu,
				Yaku.Ikkitsukan,
				Yaku.Chitoitsu,
				Yaku.Chanta
			}
		}
	};

	public static Dictionary<Yaku, int> yakuPrefabIndex = new Dictionary<Yaku, int>
	{
		{
			Yaku.Richi,
			0
		},
		{
			Yaku.Pinfu,
			1
		},
		{
			Yaku.Tanyao,
			2
		},
		{
			Yaku.Menzen,
			3
		},
		{
			Yaku.Haku,
			4
		},
		{
			Yaku.Hatsu,
			5
		},
		{
			Yaku.Tyun,
			6
		},
		{
			Yaku.Jikaze,
			7
		},
		{
			Yaku.Bakaze,
			8
		},
		{
			Yaku.Toitoiho,
			9
		},
		{
			Yaku.Ipeiko,
			10
		},
		{
			Yaku.Sansyokudojun,
			11
		},
		{
			Yaku.Honiso,
			12
		},
		{
			Yaku.Ippatsu,
			13
		},
		{
			Yaku.Ikkitsukan,
			14
		},
		{
			Yaku.Chitoitsu,
			15
		},
		{
			Yaku.Chanta,
			16
		},
		{
			Yaku.Tenho,
			17
		},
		{
			Yaku.Max,
			18
		}
	};

	public bool IsFirstBingoDisplay => !BingoFryPanNumList.Exists((FryPanInfo x) => x.EventType == GameEventType.YakuBingo3);

	public int GetFryPanNum
	{
		get
		{
			if (!IsFirstBingoDisplay)
			{
				return BingoFryPanNumList.Find((FryPanInfo x) => x.EventType == GameEventType.YakuBingo3).FryPanNum;
			}
			return 0;
		}
	}

	public BingoEventRemakeGameData()
	{
		Reset();
		Init();
	}

	private bool IsBingoEventEnable()
	{
		return GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.YakuBingo3);
	}

	public static string GetBingoEventDate()
	{
		return (GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.YakuBingo3)?.End ?? DateTimeOffset.MinValue).ToString("yyyy年MM月dd日（ddd）HH：mmまで");
	}

	public bool IsEnable()
	{
		if (!IsBingoEventEnable())
		{
			return false;
		}
		if (GameDataManager.Account.IsGuestUser)
		{
			return false;
		}
		if (GameDataManager.Mahjong.IsHaifuPlay)
		{
			return false;
		}
		return true;
	}

	public void AddFryPan()
	{
		try
		{
			BingoFryPanNumList.Find((FryPanInfo x) => x.EventType == GameEventType.YakuBingo3).FryPanNum += GetAddFryPanNum();
		}
		catch
		{
		}
	}

	public int GetAddFryPanNum()
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			if (!BeforeBingoLine[i] && AfterBingoLine[i])
			{
				int[] array = BingoLineNumbers[i];
				foreach (int index in array)
				{
					num += BingoScoreHoles[index];
				}
			}
		}
		return num;
	}

	public void Init()
	{
		ThisPlayCleardHole.Clear();
		for (int i = 0; i < 9; i++)
		{
			ThisPlayCleardHole.Add(item: false);
		}
		BeforeBingoLine = GetBingoLines();
	}

	public void Reset()
	{
		BingoScoreHoles.Clear();
		for (int i = 0; i < 9; i++)
		{
			BingoScoreHoles.Add(-1);
		}
	}

	public bool[] GetBingoLines()
	{
		bool[] array = new bool[8];
		for (int i = 0; i < 8; i++)
		{
			bool flag = true;
			int[] array2 = BingoLineNumbers[i];
			foreach (int index in array2)
			{
				if (BingoScoreHoles[index] < 0)
				{
					flag = false;
					break;
				}
			}
			array[i] = flag;
		}
		return array;
	}

	public void SetClearYaku(Yaku yaku)
	{
		switch (yaku)
		{
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
		for (int i = 0; i < 9; i++)
		{
			Yaku yaku2 = BingoCardYakuList[i];
			if (yaku2 == yaku)
			{
				ClearBingoHole(i);
			}
			else if (yaku2 == Yaku.Max)
			{
				ClearBingoHole(i);
			}
		}
	}

	public void ClearBingoHole(int index)
	{
		if (BingoScoreHoles[index] < 0)
		{
			if (BingoCardYakuList[index] != Yaku.Max)
			{
				ThisPlayCleardHole[index] = true;
			}
			BingoScoreHoles[index] = ((BingoCardYakuList[index] != Yaku.Max) ? (((GameDataManager.Mahjong.TakuType != 0) ? 1 : 2) * ((!GameDataManager.Mission.IsMissionProgressBoost) ? 1 : 2)) : 0);
		}
	}

	public bool IsBingoTargetYaku(Yaku yaku)
	{
		try
		{
			for (int i = 0; i < 9; i++)
			{
				if (BingoScoreHoles[i] < 0 && BingoCardYakuList[i] == yaku)
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("BingoEventRemakeGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("bingo_event_remake", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static BingoEventRemakeGameData LoadFromJson(string jsonString)
	{
		Debug.Log("BingoEventRemakeGameData Load : " + jsonString);
		return JsonUtility.FromJson<BingoEventRemakeGameData>(jsonString);
	}

	public void MakeBingoCard()
	{
		Yaku[] array = new Yaku[9]
		{
			Yaku.Richi,
			Yaku.Richi,
			Yaku.Richi,
			Yaku.Richi,
			Yaku.Tenho,
			Yaku.Richi,
			Yaku.Richi,
			Yaku.Richi,
			Yaku.Richi
		};
		if (IsFirstBingoDisplay)
		{
			BingoFryPanNumList.Add(new FryPanInfo
			{
				EventType = GameEventType.YakuBingo3
			});
		}
		if (IsGetSpecialCard())
		{
			SpecialCardRejectionCount = 0;
			IsSpecialCard = true;
			Debug.Log("SPカード / 中身:" + string.Join(',', array));
			BingoCardNo = 9;
		}
		else
		{
			SpecialCardRejectionCount++;
			IsSpecialCard = false;
			int num = UnityEngine.Random.Range(0, bingoTableMaster.Count);
			int[] array2 = bingoTableMaster[num];
			List<KeyValuePair<int, int>> list = (from x in array2.Select((int x, int i) => new KeyValuePair<int, int>(x, i))
				orderby x.Key
				select x).ToList();
			List<Yaku> list2 = new List<Yaku>();
			for (int j = 0; j < 9; j++)
			{
				int value = list[j].Value;
				int difficulty = array2[value];
				Yaku yaku = YakuChoice(difficulty, list2);
				if (yaku != Yaku.Max)
				{
					list2.Add(yaku);
				}
				array[value] = yaku;
			}
			Debug.Log($"ビンゴカードテーブルNo.{num + 1} / 中身:{string.Join(',', array)}");
			BingoCardNo = num + 1;
		}
		BingoCardYakuList = array;
		for (int k = 0; k < 9; k++)
		{
			if (BingoCardYakuList[k] == Yaku.Max)
			{
				ClearBingoHole(k);
			}
		}
	}

	private Yaku YakuChoice(int difficulty, List<Yaku> usedList)
	{
		if (difficulty == 0)
		{
			return Yaku.Max;
		}
		if (!yakuDifficultyTableMaster.ContainsKey(difficulty))
		{
			difficulty = 5;
		}
		List<Yaku> list = yakuDifficultyTableMaster[difficulty].Where((Yaku x) => !usedList.Contains(x)).ToList();
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return Yaku.Max;
	}

	private bool IsGetSpecialCard()
	{
		return false;
	}
}

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MahjongTestFlow : MonoBehaviour
{
	private enum MeldType
	{
		None,
		Pon,
		Chi,
		Minkan,
		Ankan
	}

	[SerializeField]
	public Canvas _canvas;

	[SerializeField]
	private Button _startButton;

	[SerializeField]
	private List<GameObject> _haiList;

	[SerializeField]
	private List<TMP_Dropdown> _furoSelects;

	[SerializeField]
	private HaiNaviPanel _haiNaviPanel;

	private Dictionary<HaiID, int> _haiCounter;

	private HaiNavi _haiNavi = new HaiNavi();

	private List<Player> players = new List<Player>();

	[SerializeField]
	private Player player;

	[SerializeField]
	private List<Sprite> _haiSpriteList;

	[SerializeField]
	private HaiShantenCounter _haiShantenCounter;

	[SerializeField]
	private Button _reachButton;

	private ColorBlock _selectColor;

	private ColorBlock _nomalColor;

	private bool isReach;

	private int selectIndex;

	private int meldCounter;

	private bool isAnalyze;

	private static Dictionary<KeyCode, HaiID> KeyToHai = new Dictionary<KeyCode, HaiID>
	{
		{
			KeyCode.Alpha1,
			HaiID.M1
		},
		{
			KeyCode.Alpha2,
			HaiID.M2
		},
		{
			KeyCode.Alpha3,
			HaiID.M3
		},
		{
			KeyCode.Alpha4,
			HaiID.M4
		},
		{
			KeyCode.Alpha5,
			HaiID.M5
		},
		{
			KeyCode.Alpha6,
			HaiID.M6
		},
		{
			KeyCode.Alpha7,
			HaiID.M7
		},
		{
			KeyCode.Alpha8,
			HaiID.M8
		},
		{
			KeyCode.Alpha9,
			HaiID.M9
		},
		{
			KeyCode.Q,
			HaiID.S1
		},
		{
			KeyCode.W,
			HaiID.S2
		},
		{
			KeyCode.E,
			HaiID.S3
		},
		{
			KeyCode.R,
			HaiID.S4
		},
		{
			KeyCode.T,
			HaiID.S5
		},
		{
			KeyCode.Y,
			HaiID.S6
		},
		{
			KeyCode.U,
			HaiID.S7
		},
		{
			KeyCode.I,
			HaiID.S8
		},
		{
			KeyCode.O,
			HaiID.S9
		},
		{
			KeyCode.A,
			HaiID.P1
		},
		{
			KeyCode.S,
			HaiID.P2
		},
		{
			KeyCode.D,
			HaiID.P3
		},
		{
			KeyCode.F,
			HaiID.P4
		},
		{
			KeyCode.G,
			HaiID.P5
		},
		{
			KeyCode.H,
			HaiID.P6
		},
		{
			KeyCode.J,
			HaiID.P7
		},
		{
			KeyCode.K,
			HaiID.P8
		},
		{
			KeyCode.L,
			HaiID.P9
		},
		{
			KeyCode.Z,
			HaiID.J1
		},
		{
			KeyCode.X,
			HaiID.J2
		},
		{
			KeyCode.C,
			HaiID.J3
		},
		{
			KeyCode.V,
			HaiID.J4
		},
		{
			KeyCode.B,
			HaiID.J5
		},
		{
			KeyCode.N,
			HaiID.J6
		},
		{
			KeyCode.M,
			HaiID.J7
		}
	};

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		selectIndex = 0;
		_nomalColor = _haiList[selectIndex].GetComponent<TMP_Dropdown>().colors;
		_selectColor = _nomalColor;
		_selectColor.normalColor = Color.yellow;
		_haiList[selectIndex].GetComponent<TMP_Dropdown>().colors = _selectColor;
		_reachButton.GetComponent<Image>().color = Color.gray;
		_reachButton.OnClickAsObservable().Subscribe(delegate
		{
			ExecReach();
		}).AddTo(this);
		_startButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (!isAnalyze)
			{
				await ExecAnalyze();
			}
		}).AddTo(this);
	}

	private async void Update()
	{
		KeyToHaiID();
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			_haiList[selectIndex].GetComponent<TMP_Dropdown>().colors = _nomalColor;
			selectIndex = (selectIndex + 1) % 14;
			_haiList[selectIndex].GetComponent<TMP_Dropdown>().colors = _selectColor;
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			_haiList[selectIndex].GetComponent<TMP_Dropdown>().colors = _nomalColor;
			selectIndex = (selectIndex + 13) % 14;
			_haiList[selectIndex].GetComponent<TMP_Dropdown>().colors = _selectColor;
		}
		int num = 0;
		foreach (GameObject hai in _haiList)
		{
			int value = _haiList[num].GetComponent<TMP_Dropdown>().value;
			hai.GetComponent<Image>().sprite = _haiSpriteList[value];
			num++;
		}
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			ExecReach();
		}
		if (Input.GetKeyDown(KeyCode.RightShift) && !isAnalyze)
		{
			await ExecAnalyze();
		}
	}

	private void ExecReach()
	{
		isReach = !isReach;
		if (isReach)
		{
			_haiList[13].GetComponent<TMP_Dropdown>().value = 34;
			_haiList[13].GetComponent<TMP_Dropdown>().interactable = false;
		}
		else
		{
			_haiList[13].GetComponent<TMP_Dropdown>().interactable = true;
		}
		_reachButton.GetComponent<Image>().color = (isReach ? Color.white : Color.gray);
	}

	private async UniTask ExecAnalyze()
	{
		isAnalyze = true;
		if (NaviSetup())
		{
			await _haiNavi.Analyze();
			if (_haiNavi.CurShanten > 0 && isReach)
			{
				Debug.LogWarning("聴牌していないのでリーチ出来ません");
				return;
			}
			bool isAgari = false;
			if (GetHaiKind(13) != HaiID.None)
			{
				if (_haiNavi.MachiHais().Contains(GetHaiKind(13)))
				{
					isAgari = true;
				}
				await players[0].hand.Add(GetHaiKind(13).HaiIDToPai());
			}
			await _haiNavi.Analyze(isAgari);
			await _haiNaviPanel.Reload(_haiNavi.m_yakuInfo, _haiNavi.m_oneByOneDiscardedYakuPossibilityInfos, isFireReach: false, isOpenReach: false, isAtomicReach: false, meldCounter > 0);
			_haiNaviPanel.ReloadButtonBySelect(players[0].hand.GetItemPos(null));
			_haiShantenCounter.SetShantenCount(_haiNavi.CurShanten).Forget();
			if (_haiNavi.MachiHais() != null)
			{
				Debug.Log(string.Join(',', _haiNavi.MachiHais()));
			}
			if (isAgari)
			{
				_haiNaviPanel.ReloadActive = true;
				await _haiNavi.Analyze(isTsumoAgariSelectable: true);
				_haiNaviPanel.Reload(_haiNavi.m_yakuInfo, _haiNavi.m_oneByOneDiscardedYakuPossibilityInfos, isFireReach: false, isOpenReach: false, isAtomicReach: false, meldCounter > 0).Forget();
			}
		}
		isAnalyze = false;
	}

	private bool NaviSetup()
	{
		players.Clear();
		player.Destruct();
		_haiNaviPanel.Reset();
		_haiNavi.Reset();
		InitHaiCounter();
		meldCounter = 0;
		MahjongUtility.ResetOpenPaiNum();
		if (isReach && GetHaiKind(13) != HaiID.None)
		{
			Debug.LogWarning("リーチの際は、14牌目にセットしないでください");
			return false;
		}
		for (int i = 0; i < 13; i++)
		{
			HaiID haiKind = GetHaiKind(i);
			if (haiKind != HaiID.None)
			{
				_haiCounter[haiKind]++;
				if (_haiCounter[haiKind] > 4)
				{
					Debug.LogWarning("同じ牌が5つ以上あります");
					return false;
				}
				player.hand.HaipaiAdd(haiKind.HaiIDToPai(), 0);
			}
		}
		for (int j = 0; j < 4; j++)
		{
			int idx = 3 * j + 1;
			MeldType value = (MeldType)_furoSelects[j].GetComponent<TMP_Dropdown>().value;
			if (value != 0 && !AddMeld(value, player.hand, idx))
			{
				Debug.LogWarning($"正しく鳴けませんでした。Dropdown{j} : Type {value}");
				return false;
			}
		}
		if (meldCounter > 0 && isReach)
		{
			Debug.LogWarning("鳴いているのでリーチ出来ません");
			return false;
		}
		players.Add(player);
		_haiNavi.SetAnalyzeData(0, isMainPlayer: true, players, null, 0, TakuType.Tonpu);
		players[0].m_isReach = isReach;
		return true;
	}

	private HaiID GetHaiKind(int i)
	{
		HaiID haiID = (HaiID)_haiList[i].GetComponent<TMP_Dropdown>().value;
		if (haiID != HaiID.Max)
		{
			return haiID;
		}
		return HaiID.None;
	}

	private void InitHaiCounter()
	{
		_haiCounter = new Dictionary<HaiID, int>();
		for (HaiID haiID = HaiID.M1; haiID < HaiID.Max; haiID++)
		{
			_haiCounter.Add(haiID, 0);
		}
	}

	private bool AddMeld(MeldType type, Hand hand, int idx)
	{
		Tile tile = hand.GetTile(GetHaiKind(idx).HaiIDToPai());
		Tile tile2 = hand.GetTile(GetHaiKind(idx + 1).HaiIDToPai());
		Tile tile3 = hand.GetTile(GetHaiKind(idx + 2).HaiIDToPai());
		switch (type)
		{
		case MeldType.Pon:
			if (GetHaiKind(idx) == GetHaiKind(idx + 1) && GetHaiKind(idx) == GetHaiKind(idx + 2))
			{
				player.meld.AddPon(Cha.Kamicha, tile, tile2, tile3);
				meldCounter++;
				return true;
			}
			break;
		case MeldType.Chi:
			if (GetHaiKind(idx) + 1 == GetHaiKind(idx + 1) && GetHaiKind(idx) + 2 == GetHaiKind(idx + 2) && !GetHaiKind(idx).IsJihai() && GetHaiKind(idx).GetSu() != SuID.SU8 && GetHaiKind(idx).GetSu() != SuID.SU9)
			{
				player.meld.AddChi(tile, tile2, tile3);
				meldCounter++;
				return true;
			}
			break;
		case MeldType.Minkan:
			if (GetHaiKind(idx) == GetHaiKind(idx + 1) && GetHaiKind(idx) == GetHaiKind(idx + 2))
			{
				_haiCounter[GetHaiKind(idx)]++;
				if (_haiCounter[GetHaiKind(idx)] > 4)
				{
					Debug.LogWarning("同じ牌が5つ以上あります");
					return false;
				}
				player.meld.AddMinkan(Cha.Kamicha, tile, tile2, tile3, tile3);
				meldCounter++;
				return true;
			}
			break;
		case MeldType.Ankan:
			if (GetHaiKind(idx) == GetHaiKind(idx + 1) && GetHaiKind(idx) == GetHaiKind(idx + 2))
			{
				_haiCounter[GetHaiKind(idx)]++;
				if (_haiCounter[GetHaiKind(idx)] > 4)
				{
					Debug.LogWarning("同じ牌が5つ以上あります");
					return false;
				}
				player.meld.AddAnkan(tile, tile2, tile3, tile3);
				return true;
			}
			break;
		}
		return false;
	}

	private void KeyToHaiID()
	{
		foreach (KeyValuePair<KeyCode, HaiID> item in KeyToHai)
		{
			if (Input.GetKeyDown(item.Key))
			{
				_haiList[selectIndex].GetComponent<TMP_Dropdown>().value = (int)item.Value;
				break;
			}
		}
	}

	private void SetMachiPaiPanel()
	{
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DebugTsumikomiWindow : MonoBehaviour
{
	public enum PlayMode
	{
		Tonpu,
		Sanma,
		Nima,
		Kirisame,
		MentanpinSanma,
		Aotenjo
	}

	public class PlayModeMaster
	{
		public string ModeName;

		public MFG.Types.EventType EventType;

		public TakuType TakuType;

		public int AkaCount;

		public List<Pai> UnablePais = new List<Pai>();
	}

	public class TsumikomiSaveData
	{
		public List<Pai> YonmaPais = new List<Pai>();

		public List<Pai> SanmaPais = new List<Pai>();

		public List<Pai> NimaPais = new List<Pai>();

		public List<Pai> KirisamePais = new List<Pai>();

		public List<Pai> MentanpinSanmaPais = new List<Pai>();

		public List<Pai> AotenjoPais = new List<Pai>();

		public List<Pai> Pais(PlayMode playMode)
		{
			return playMode switch
			{
				PlayMode.Tonpu => YonmaPais, 
				PlayMode.Sanma => SanmaPais, 
				PlayMode.Nima => NimaPais, 
				PlayMode.Kirisame => KirisamePais, 
				PlayMode.MentanpinSanma => MentanpinSanmaPais, 
				PlayMode.Aotenjo => AotenjoPais, 
				_ => YonmaPais, 
			};
		}
	}

	[SerializeField]
	private Button _returnButton;

	[SerializeField]
	private Button _resetButton;

	[SerializeField]
	private Button _onOffButton;

	[SerializeField]
	private Button _playerButton;

	[SerializeField]
	private Button _chichaButton;

	[SerializeField]
	private Button _cpuTsumogiriButton;

	[SerializeField]
	private Button _savePresetButton;

	[SerializeField]
	private Button _loadPresetButton;

	[SerializeField]
	private GameObject _haiPrefab;

	[SerializeField]
	private List<GameObject> _player;

	[SerializeField]
	private List<GameObject> _yama;

	[SerializeField]
	private List<GameObject> _wanpai;

	private List<GameObject>[] _playerObjs = new List<GameObject>[4]
	{
		new List<GameObject>(),
		new List<GameObject>(),
		new List<GameObject>(),
		new List<GameObject>()
	};

	private List<GameObject>[] _yamaObjs = new List<GameObject>[3]
	{
		new List<GameObject>(),
		new List<GameObject>(),
		new List<GameObject>()
	};

	private List<GameObject>[] _wanpaiObjs = new List<GameObject>[3]
	{
		new List<GameObject>(),
		new List<GameObject>(),
		new List<GameObject>()
	};

	[SerializeField]
	private List<Sprite> _haiSprites;

	public static TsumikomiSaveData TsumikomiData = new TsumikomiSaveData();

	public PlayMode currentPlayMode;

	private string[] KazeStrings = new string[4] { "東家", "南家", "西家", "北家" };

	private int TehaiCount = 13;

	private int[] YamaBlockCount = new int[3] { 52, 3, 15 };

	private int[] WanpaiBlockCount = new int[3] { 4, 5, 5 };

	public static readonly Dictionary<PlayMode, PlayModeMaster> MasterData = new Dictionary<PlayMode, PlayModeMaster>
	{
		{
			PlayMode.Tonpu,
			new PlayModeMaster
			{
				ModeName = "4人打ち",
				EventType = MFG.Types.EventType.None,
				TakuType = TakuType.Tonpu,
				AkaCount = 1
			}
		},
		{
			PlayMode.Sanma,
			new PlayModeMaster
			{
				ModeName = "3人打ち",
				EventType = MFG.Types.EventType.None,
				TakuType = TakuType.Sanma,
				AkaCount = 2,
				UnablePais = new List<Pai>
				{
					Pai.M2,
					Pai.M3,
					Pai.M4,
					Pai.M5,
					Pai.M6,
					Pai.M7,
					Pai.M8
				}
			}
		},
		{
			PlayMode.Nima,
			new PlayModeMaster
			{
				ModeName = "2人打ち",
				EventType = MFG.Types.EventType.None,
				TakuType = TakuType.Nima,
				AkaCount = 2,
				UnablePais = new List<Pai>
				{
					Pai.M1,
					Pai.M2,
					Pai.M3,
					Pai.M4,
					Pai.M5,
					Pai.M6,
					Pai.M7,
					Pai.M8,
					Pai.M9,
					Pai.J3,
					Pai.J4
				}
			}
		},
		{
			PlayMode.Kirisame,
			new PlayModeMaster
			{
				ModeName = "霧雨",
				EventType = MFG.Types.EventType.Kirisame,
				TakuType = TakuType.Tonpu,
				AkaCount = 1
			}
		},
		{
			PlayMode.MentanpinSanma,
			new PlayModeMaster
			{
				ModeName = "メタピ三",
				EventType = MFG.Types.EventType.Mentanpin,
				TakuType = TakuType.Sanma,
				AkaCount = 1,
				UnablePais = new List<Pai>
				{
					Pai.J1,
					Pai.J2,
					Pai.J3,
					Pai.J4,
					Pai.J5,
					Pai.J6,
					Pai.J7
				}
			}
		},
		{
			PlayMode.Aotenjo,
			new PlayModeMaster
			{
				ModeName = "青天井",
				EventType = MFG.Types.EventType.Aotenjo,
				TakuType = TakuType.Sanma,
				AkaCount = 1,
				UnablePais = new List<Pai>
				{
					Pai.M2,
					Pai.M3,
					Pai.M4,
					Pai.M5,
					Pai.M6,
					Pai.M7,
					Pai.M8
				}
			}
		}
	};

	private static string SavePath => Application.persistentDataPath + "/";

	private List<List<GameObject>> PaiObjects(PlayMode playMode)
	{
		return MasterData[playMode].TakuType switch
		{
			TakuType.Tonpu => new List<List<GameObject>>
			{
				_playerObjs[0],
				_playerObjs[1],
				_playerObjs[2],
				_playerObjs[3],
				_yamaObjs[0],
				_yamaObjs[1],
				_yamaObjs[2],
				_wanpaiObjs[0],
				_wanpaiObjs[1],
				_wanpaiObjs[2]
			}, 
			TakuType.Sanma => new List<List<GameObject>>
			{
				_playerObjs[0],
				_playerObjs[1],
				_playerObjs[2],
				_yamaObjs[0],
				_yamaObjs[1],
				_wanpaiObjs[0],
				_wanpaiObjs[1],
				_wanpaiObjs[2],
				_playerObjs[3],
				_yamaObjs[2]
			}, 
			_ => new List<List<GameObject>>
			{
				_playerObjs[0],
				_playerObjs[1],
				_yamaObjs[0],
				_wanpaiObjs[0],
				_wanpaiObjs[1],
				_wanpaiObjs[2],
				_playerObjs[2],
				_playerObjs[3],
				_yamaObjs[1],
				_yamaObjs[2]
			}, 
		};
	}

	private void SetTsumikomiData(PlayMode playMode)
	{
		TsumikomiData.Pais(playMode).Clear();
		foreach (List<GameObject> item in PaiObjects(playMode))
		{
			foreach (GameObject item2 in item)
			{
				TsumikomiData.Pais(playMode).Add(item2.GetComponent<DebugTsumikomiTile>().pai);
			}
		}
	}

	public void Save()
	{
		SetTsumikomiData(currentPlayMode);
		GameDataManager.Mahjong.CustomTsumikomi = TsumikomiData.Pais(currentPlayMode);
		Debug.Log(string.Join(",", TsumikomiData.Pais(currentPlayMode)));
		Debug.Log("積み込み情報を筐体に保存 " + SavePath + "/TsumikomiTmp.json");
		Save("/TsumikomiTmp.json", JsonUtility.ToJson(TsumikomiData));
	}

	public void SavePreset()
	{
		SetTsumikomiData(currentPlayMode);
		GameDataManager.Mahjong.CustomTsumikomi = TsumikomiData.Pais(currentPlayMode);
		Debug.Log(string.Join(",", TsumikomiData.Pais(currentPlayMode)));
		Debug.Log("プリセット情報を筐体に保存");
		Save("/TsumikomiPresetData.json", JsonUtility.ToJson(TsumikomiData));
	}

	private static void Save(string fileName, string jsonData)
	{
		using StreamWriter streamWriter = new StreamWriter(SavePath + fileName, append: false);
		try
		{
			streamWriter.Write(jsonData);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	public void Load(PlayMode playMode)
	{
		string text = Load("/TsumikomiTmp.json");
		if (text != null)
		{
			TsumikomiData = JsonUtility.FromJson<TsumikomiSaveData>(text);
			TextureReload(playMode);
		}
	}

	public void LoadPreset(PlayMode playMode)
	{
		string text = Load("/TsumikomiPresetData.json");
		if (text != null)
		{
			TsumikomiData = JsonUtility.FromJson<TsumikomiSaveData>(text);
			TextureReload(playMode);
		}
	}

	private static string Load(string fileName)
	{
		try
		{
			using FileStream stream = new FileStream(SavePath + fileName, FileMode.Open);
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
		}
		catch (Exception message)
		{
			Debug.Log(message);
			return null;
		}
	}

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		try
		{
			this.GetCancellationTokenOnDestroy();
			InstantiatePaiObjects();
			SetTexture(PlayMode.Tonpu);
			_returnButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
				GetComponent<Animator>().SetTrigger("Out");
				Save();
			}).AddTo(this);
			_resetButton.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				SetTexture(currentPlayMode);
			}).AddTo(this);
			_onOffButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				BtOnOff(GameDataManager.Mahjong.TsumikomiID == 0);
			}).AddTo(this);
			_chichaButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				GameDataManager.Mahjong.TsumikomiChicha = (Kaze)((int)(GameDataManager.Mahjong.TsumikomiChicha + 1) % Enum.GetValues(typeof(Kaze)).Length);
				_chichaButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = KazeStrings[(int)GameDataManager.Mahjong.TsumikomiChicha];
			}).AddTo(this);
			_playerButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				Save();
				currentPlayMode = (PlayMode)((int)(currentPlayMode + 1) % Enum.GetValues(typeof(PlayMode)).Length);
				_playerButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = MasterData[currentPlayMode].ModeName;
				ChangeActivePai(currentPlayMode);
			}).AddTo(this);
			_cpuTsumogiriButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				GameDataManager.Mahjong.TsumikomiCpuTsumogiri = !GameDataManager.Mahjong.TsumikomiCpuTsumogiri;
				_cpuTsumogiriButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = (GameDataManager.Mahjong.TsumikomiCpuTsumogiri ? "CPUツモギリ" : "CPU思考");
			}).AddTo(this);
			_savePresetButton.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				SavePreset();
			}).AddTo(this);
			_loadPresetButton.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				LoadPreset(currentPlayMode);
			}).AddTo(this);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	public void Play()
	{
		Load(currentPlayMode);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().SetTrigger("In");
		BtOnOff(GameDataManager.Mahjong.TsumikomiID == 1);
	}

	public void BtOnOff(bool isOn)
	{
		if (isOn)
		{
			GameDataManager.Mahjong.TsumikomiID = 1;
			_onOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "オン";
			_onOffButton.transform.Find("BtDisp").GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
		}
		else
		{
			GameDataManager.Mahjong.TsumikomiID = 0;
			_onOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "オフ";
			_onOffButton.transform.Find("BtDisp").GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
		}
	}

	public void ChangeActivePai(PlayMode playMode)
	{
		if (GameDataManager.Mahjong.DebugSelectingPai != null)
		{
			GameDataManager.Mahjong.DebugSelectingPai.GetComponent<Image>().color = Color.white;
			GameDataManager.Mahjong.DebugSelectingPai = null;
		}
		_yama[1].SetActive(MasterData[playMode].TakuType <= TakuType.Sanma);
		_yama[2].SetActive(MasterData[playMode].TakuType <= TakuType.Tonpu);
		_player[2].SetActive(MasterData[playMode].TakuType <= TakuType.Sanma);
		_player[3].SetActive(MasterData[playMode].TakuType <= TakuType.Tonpu);
		TextureReload(playMode);
	}

	public void InstantiatePaiObjects()
	{
		List<List<GameObject>> list = PaiObjects(PlayMode.Tonpu);
		foreach (List<GameObject> item in list)
		{
			int num = list.IndexOf(item);
			int num2;
			Transform parent;
			if (num < 4)
			{
				num2 = TehaiCount;
				parent = _player[num].transform;
			}
			else if (num < 7)
			{
				num2 = YamaBlockCount[num - 4];
				parent = _yama[num - 4].transform;
			}
			else
			{
				num2 = WanpaiBlockCount[num - 7];
				parent = _wanpai[num - 7].transform;
			}
			for (int i = 0; i < num2; i++)
			{
				item.Add(UnityEngine.Object.Instantiate(_haiPrefab, parent));
			}
		}
	}

	public void TextureReload(PlayMode playMode)
	{
		SetTexture(playMode, TsumikomiData.Pais(playMode).Count == 136);
	}

	public void SetTexture(PlayMode playMode, bool isJson = false)
	{
		List<Pai> list;
		if (isJson)
		{
			list = TsumikomiData.Pais(playMode);
		}
		else
		{
			List<Pai> list2 = new List<Pai>();
			List<Pai> list3 = new List<Pai>();
			for (Pai pai = Pai.M1; pai <= Pai.J7; pai++)
			{
				if ((int)pai % 10 != 0)
				{
					int num = (MahjongUtility.IsIncludeAkahai(pai, MasterData[playMode].EventType, MasterData[playMode].TakuType) ? MasterData[playMode].AkaCount : 0);
					if (MasterData[playMode].UnablePais.Contains(pai))
					{
						list3.AddRange(Enumerable.Repeat(pai, 4 - num));
						list3.AddRange(Enumerable.Repeat(MahjongUtility.NormalToAka(pai), num));
					}
					else
					{
						list2.AddRange(Enumerable.Repeat(pai, 4 - num));
						list2.AddRange(Enumerable.Repeat(MahjongUtility.NormalToAka(pai), num));
					}
				}
			}
			list = list2.Concat(list3).ToList();
		}
		int num2 = 0;
		foreach (List<GameObject> item in PaiObjects(playMode))
		{
			foreach (GameObject item2 in item)
			{
				item2.GetComponent<Image>().sprite = _haiSprites[(int)list[num2]];
				item2.GetComponent<DebugTsumikomiTile>().pai = list[num2];
				num2++;
			}
		}
	}
}

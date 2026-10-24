using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.GameData.EventTaku;
using MFG.MahjongPlay.Character;
using MFG.MahjongPlay.Cutin;
using MFG.MasterGameData;
using MFG.Taikyoku;
using MFG.Types;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MahjongPlayDebug : MonoBehaviour
{
	public class PaifuDemoMaster
	{
		public string FileName;

		public int KyokuIndex;
	}

	[SerializeField]
	private Button m_debug4PButton;

	[SerializeField]
	private Button m_debug3PButton;

	[SerializeField]
	private Button m_debug2PButton;

	[SerializeField]
	private Dropdown m_charaCheck;

	[SerializeField]
	private Dropdown m_motionSelect;

	[SerializeField]
	private Button m_motionPlay;

	[SerializeField]
	private Dropdown m_cutinCharaDropdown;

	[SerializeField]
	private Dropdown m_cutinRarityDropdown;

	[SerializeField]
	private Dropdown m_cutinAssetTypeDropdown;

	[SerializeField]
	private Button m_cutinPlay;

	[SerializeField]
	private Button m_cutinCancel;

	[SerializeField]
	private int m_cutinPlayerIndex;

	[SerializeField]
	private List<PreviewPlayerInteractor> m_previewPlayerInteractors;

	private CancellationTokenSource m_cts = new CancellationTokenSource();

	[SerializeField]
	private Dropdown m_backScreenChaDropdown;

	[SerializeField]
	private Dropdown m_backScreenTypeDropdown;

	[SerializeField]
	private Button m_backScreenPlay;

	[SerializeField]
	private List<GameObject> m_charaParent;

	[SerializeField]
	private List<GameObject> m_charaParentTop;

	[SerializeField]
	private List<GameObject> m_charaArmParent;

	[SerializeField]
	private List<GameObject> m_chara3DParent;

	[SerializeField]
	private GameObject m_3dTexture;

	[SerializeField]
	private GameObject m_3dTextureTehai;

	private Player m_reachedPlayer;

	[SerializeField]
	private Taku m_taku;

	private MahjongPlayFlow m_mahjongPlayFlow;

	[SerializeField]
	private GameObject m_playerAccessoryDebugParent;

	[SerializeField]
	private GameObject m_reachStickDebugParent;

	[SerializeField]
	private GameObject m_tableStickerDebugParent;

	[SerializeField]
	private Dropdown m_playerAccessoryDropdown;

	[SerializeField]
	private Dropdown m_reachStickDropdown;

	[SerializeField]
	private Dropdown m_tableStickerDropdown;

	[SerializeField]
	private Button m_viewTypeChangeButton;

	[SerializeField]
	private Button m_blowAwayCutin1;

	[SerializeField]
	private Button m_blowAwayCutin2;

	[SerializeField]
	private Button m_blowAwayCutin3;

	[SerializeField]
	private ResultInfo _blowAwayEventResultInfo = new ResultInfo();

	[SerializeField]
	private Button m_btAllInfoDisplay;

	[SerializeField]
	private Button m_btDebugPaiDisplay;

	[SerializeField]
	private List<Player> players;

	[Space(20f)]
	[Header("---- PlayerAccessory のID設定(3桁の数字を入力) ---- ")]
	[SerializeField]
	[Tooltip("3桁のIDを入力")]
	private string m_playerAccessoryItemID = "001";

	[SerializeField]
	[Tooltip("3桁のIDを入力")]
	private string m_reachStickItemID = "001";

	[SerializeField]
	[Tooltip("3桁のIDを入力")]
	private string m_tableStickerID = "001";

	private int _selectingCostume;

	private bool _initialized;

	private bool _isCharaChanging = true;

	public static List<PaifuDemoMaster> PaifuDemoList = new List<PaifuDemoMaster>
	{
		new PaifuDemoMaster
		{
			FileName = "PaifuDemo0",
			KyokuIndex = 0
		},
		new PaifuDemoMaster
		{
			FileName = "PaifuDemo1",
			KyokuIndex = 1
		},
		new PaifuDemoMaster
		{
			FileName = "PaifuDemo2",
			KyokuIndex = 4
		},
		new PaifuDemoMaster
		{
			FileName = "PaifuDemo3",
			KyokuIndex = 3
		}
	};

	private List<Player> Players => m_mahjongPlayFlow.Players;

	private AccountGameData m_accountGameData => GameDataManager.Account;

	private void Awake()
	{
		m_mahjongPlayFlow = GameObject.Find("SceneFlow").GetComponent<MahjongPlayFlow>();
	}

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		this.GetCancellationTokenOnDestroy();
		m_debug4PButton.OnClickAsObservable().Subscribe(delegate
		{
			GameDataManager.Mahjong.TakuType = TakuType.Tonpu;
			GameDataManager.Mahjong.EventType = MFG.Types.EventType.None;
			ResidentFlow.GetInstance().LoadScene("Matching");
		}).AddTo(this);
		m_debug3PButton.OnClickAsObservable().Subscribe(delegate
		{
			GameDataManager.Mahjong.TakuType = TakuType.Sanma;
			GameDataManager.Mahjong.EventType = MFG.Types.EventType.None;
			ResidentFlow.GetInstance().LoadScene("Matching");
		}).AddTo(this);
		m_debug2PButton.OnClickAsObservable().Subscribe(delegate
		{
			GameDataManager.Mahjong.TakuType = TakuType.Nima;
			GameDataManager.Mahjong.EventType = MFG.Types.EventType.None;
			ResidentFlow.GetInstance().LoadScene("Matching");
		}).AddTo(this);
		m_btAllInfoDisplay.OnClickAsObservable().Subscribe(async delegate
		{
			m_playerAccessoryItemID = "81";
			PlayerAccessory_Reflesh();
			foreach (Cha value2 in Enum.GetValues(typeof(Cha)))
			{
				await m_taku.Init(value2, new List<MFG.Types.Customize.CustomizeItem.ItemID>
				{
					MFG.Types.Customize.CustomizeItem.ItemID.TableSticker235,
					MFG.Types.Customize.CustomizeItem.ItemID.TableSticker236,
					MFG.Types.Customize.CustomizeItem.ItemID.TableSticker237,
					MFG.Types.Customize.CustomizeItem.ItemID.TableSticker238
				}, new List<StickerEditInfo>
				{
					new StickerEditInfo(MFG.Types.Customize.CustomizeItem.ItemID.TableSticker235, 0),
					new StickerEditInfo(MFG.Types.Customize.CustomizeItem.ItemID.TableSticker236, 1),
					new StickerEditInfo(MFG.Types.Customize.CustomizeItem.ItemID.TableSticker237, 2),
					new StickerEditInfo(MFG.Types.Customize.CustomizeItem.ItemID.TableSticker238, 3)
				});
			}
			m_reachStickItemID = "204";
			await ReachStick_Reflesh();
			await m_mahjongPlayFlow.SetPlayerNameAllUIDisp();
		}).AddTo(this);
		m_btDebugPaiDisplay.OnClickAsObservable().Subscribe(async delegate
		{
			MahjongUtility.ResetOpenPaiNum();
			foreach (PreviewPlayerInteractor previewPlayerInteractor2 in m_previewPlayerInteractors)
			{
				Cha cha4 = previewPlayerInteractor2.Cha();
				Player playerWork = m_mahjongPlayFlow.GetPlayerWork(cha4);
				playerWork.plate.SetActive(value: true);
				playerWork.dump.SetDebugSutePai();
			}
		}).AddTo(this);
		List<string> list = new List<string>();
		list.Add(MotionID.Wait0.ToString());
		list.Add(MotionID.Wait1.ToString());
		for (MotionID motionID = MotionID.Away1; motionID <= MotionID.Away5; motionID++)
		{
			list.Add(motionID.ToString());
		}
		for (MotionID motionID2 = MotionID.Start; motionID2 <= MotionID.ResultCritical1; motionID2++)
		{
			list.Add(motionID2.ToString());
		}
		list.Add(MotionID.Special.ToString());
		m_motionSelect.options.Clear();
		foreach (string item3 in list)
		{
			m_motionSelect.options.Add(new Dropdown.OptionData
			{
				text = item3
			});
		}
		m_motionSelect.RefreshShownValue();
		m_motionPlay.OnClickAsObservable().Subscribe(async delegate
		{
			await MotionPlay();
		}).AddTo(this);
		List<string> obj = new List<string> { "Pon", "Chi", "Kan" };
		m_cutinRarityDropdown.options.Clear();
		foreach (string item4 in obj)
		{
			m_cutinRarityDropdown.options.Add(new Dropdown.OptionData
			{
				text = item4
			});
		}
		m_cutinRarityDropdown.RefreshShownValue();
		m_cutinAssetTypeDropdown.options.Clear();
		foreach (MFG.Types.Customize.CutinItem.ItemID value3 in Enum.GetValues(typeof(MFG.Types.Customize.CutinItem.ItemID)))
		{
			m_cutinAssetTypeDropdown.options.Add(new Dropdown.OptionData
			{
				text = value3.ToString()
			});
		}
		m_cutinAssetTypeDropdown.RefreshShownValue();
		m_cutinPlay.OnClickAsObservable().Subscribe(async delegate
		{
			m_cutinPlay.interactable = false;
			m_cts = new CancellationTokenSource();
			MFG.Types.Customize.CutinItem.ItemID itemID3 = (MFG.Types.Customize.CutinItem.ItemID)Enum.Parse(typeof(MFG.Types.Customize.CutinItem.ItemID), m_cutinAssetTypeDropdown.captionText.text);
			MFG.Types.Customize.CutinItem.Item item2 = CutinItemMaster.GetItem(itemID3);
			int value = m_cutinRarityDropdown.value;
			bool isRon = true;
			CutinManager m_cutinManager = GameObject.Find("CutinManager").GetComponent<CutinManager>();
			Cha cutinPlayerIndex2 = (Cha)m_cutinPlayerIndex;
			_ = m_previewPlayerInteractors[(int)cutinPlayerIndex2];
			CutinType cutinType = CreateDebugCutinType(item2.Category, value, isRon);
			try
			{
				m_cutinManager.Play(cutinType, new CutinItemInfo
				{
					ItemID = itemID3,
					IsOn = true
				}, cutinPlayerIndex2, m_previewPlayerInteractors, null, m_cts.Token).Forget();
				if (item2.Category == MFG.Types.Customize.CutinItem.Category.AgariSmall || item2.Category == MFG.Types.Customize.CutinItem.Category.AgariBig)
				{
					await m_cutinManager.AgariCutin_WaitIntroductionEnd(m_cts.Token);
					m_cutinManager.CurrentCutin_End(m_cts.Token).Forget();
				}
				else if (item2.Category == MFG.Types.Customize.CutinItem.Category.Reach)
				{
					Player player = m_mahjongPlayFlow.GetPlayerWork(cutinPlayerIndex2);
					player.ReachBouDisp(isDisp: false);
					await ReachStick_Reflesh(cutinPlayerIndex2);
					m_reachedPlayer = player;
				}
				await UniTask.WaitWhile(() => m_cutinManager.IsPlaying, PlayerLoopTiming.Update, m_cts.Token);
			}
			catch (OperationCanceledException)
			{
			}
			m_cutinPlay.interactable = true;
		}).AddTo(this);
		m_cutinCancel.OnClickAsObservable().Subscribe(async delegate
		{
			m_cts.Cancel();
		}).AddTo(this);
		GameObject.Find("CutinManager").GetComponent<CutinManager>().OnReachStickLanded.Subscribe(delegate
		{
			if (m_reachedPlayer != null)
			{
				Cha cutinPlayerIndex = (Cha)m_cutinPlayerIndex;
				PreviewPlayerInteractor previewPlayerInteractor = m_previewPlayerInteractors[(int)cutinPlayerIndex];
				m_reachedPlayer.m_isFireReach = previewPlayerInteractor.ReachLevel() >= ReachLevel.FireReach;
				m_reachedPlayer.plate.SetActive(value: true);
				m_reachedPlayer.ReachBouDisp();
				m_reachedPlayer = null;
			}
		}).AddTo(this);
		m_backScreenChaDropdown.options.Clear();
		foreach (Cha value4 in Enum.GetValues(typeof(Cha)))
		{
			m_backScreenChaDropdown.options.Add(new Dropdown.OptionData
			{
				text = value4.ToString()
			});
		}
		m_backScreenChaDropdown.RefreshShownValue();
		Vector2 sizeDelta = m_cutinCharaDropdown.GetComponent<RectTransform>().sizeDelta;
		sizeDelta.x = 130f;
		m_cutinCharaDropdown.GetComponent<RectTransform>().sizeDelta = sizeDelta;
		m_cutinCharaDropdown.options.Clear();
		foreach (CharaType value5 in Enum.GetValues(typeof(CharaType)))
		{
			m_cutinCharaDropdown.options.Add(new Dropdown.OptionData
			{
				text = value5.ToString()
			});
		}
		m_cutinCharaDropdown.RefreshShownValue();
		m_backScreenTypeDropdown.options.Clear();
		foreach (CharaReactionData.ReactionID value6 in Enum.GetValues(typeof(CharaReactionData.ReactionID)))
		{
			m_backScreenTypeDropdown.options.Add(new Dropdown.OptionData
			{
				text = value6.ToString()
			});
		}
		m_backScreenTypeDropdown.options.Add(new Dropdown.OptionData
		{
			text = "Ron"
		});
		m_backScreenTypeDropdown.RefreshShownValue();
		m_backScreenPlay.OnClickAsObservable().Subscribe(async delegate
		{
			CharaType charaType2 = (CharaType)Enum.Parse(typeof(CharaType), m_cutinCharaDropdown.captionText.text);
			Cha cha3 = (Cha)Enum.Parse(typeof(Cha), m_backScreenChaDropdown.captionText.text);
			if (m_backScreenTypeDropdown.captionText.text == "Ron")
			{
				m_mahjongPlayFlow.BackScreen_Play_Ron(charaType2, cha3, charaType2, Cha.Jicha).Forget();
			}
			else
			{
				CharaReactionData.ReactionID reactionID2 = Enum.Parse<CharaReactionData.ReactionID>(m_backScreenTypeDropdown.captionText.text);
				CharaReactionData.ReactionData reactionData = MasterGameDataManager.Character.ReactionDatas(charaType2).m_reactionDatas.Find((CharaReactionData.ReactionData x) => x.ID == reactionID2);
				m_mahjongPlayFlow.BackScreen_Play(charaType2, cha3, reactionData).Forget();
				await reactionData.ExecDelay();
				players[(int)cha3].character.m_characterModelController.PlayMotion(reactionData.motionID).Forget();
			}
		}).AddTo(this);
		m_charaCheck.options.Clear();
		foreach (MFG.Types.Customize.Costume.ItemID value7 in Enum.GetValues(typeof(MFG.Types.Customize.Costume.ItemID)))
		{
			if (value7 != 0)
			{
				MFG.Types.Customize.Costume.Item item = CostumeItemMaster.GetItem(value7);
				string text = item.Name;
				string shortName = MasterGameDataManager.Character.Properties(item.CharaType).Profile.ShortName;
				m_charaCheck.options.Add(new Dropdown.OptionData
				{
					text = shortName + "/" + text
				});
			}
		}
		m_charaCheck.RefreshShownValue();
		m_charaCheck.onValueChanged.AsObservable().Subscribe(async delegate(int id)
		{
			_selectingCostume = id;
			await CharaChange(id);
		}).AddTo(this);
		bool active = string.IsNullOrEmpty(m_accountGameData.AppSessionID) && !GameDataManager.Mahjong.IsHaifuPlay;
		m_debug4PButton.gameObject.SetActive(active);
		m_debug3PButton.gameObject.SetActive(active);
		m_debug2PButton.gameObject.SetActive(active);
		m_backScreenChaDropdown.options.Clear();
		foreach (Cha value8 in Enum.GetValues(typeof(Cha)))
		{
			m_backScreenChaDropdown.options.Add(new Dropdown.OptionData
			{
				text = value8.ToString()
			});
		}
		m_backScreenChaDropdown.RefreshShownValue();
		TestCharaDisp_SetActive(DebugFlag.IsActive(DebugFlag.Flag.TestCharaDisp));
		TestCutinDisp_SetActive(DebugFlag.IsActive(DebugFlag.Flag.TestCutinDisp));
		TestBackScreen_SetActive(DebugFlag.IsActive(DebugFlag.Flag.TestBackScreenDisp));
		PlayerAccessory_Init();
		ReachStick_Init();
		TableSticker_Init();
		m_viewTypeChangeButton.GetComponentInChildren<TextMeshProUGUI>().text = $"View{MahjongUtility.GetViewType()}";
		m_mahjongPlayFlow.ViewType_Change(MahjongUtility.GetViewType());
		m_viewTypeChangeButton.OnClickAsObservable().Subscribe(delegate
		{
			ViewType viewType = ((MahjongUtility.GetViewType() == ViewType.Type1) ? ViewType.Type3 : ViewType.Type1);
			m_viewTypeChangeButton.GetComponentInChildren<TextMeshProUGUI>().text = $"View{MahjongUtility.GetViewType()}";
			m_mahjongPlayFlow.ViewType_Change(viewType);
		}).AddTo(this);
		m_blowAwayCutin1.OnClickAsObservable().Subscribe(async delegate
		{
			await BlowAwayEventCutin_Play(1);
		}).AddTo(this);
		m_blowAwayCutin2.OnClickAsObservable().Subscribe(async delegate
		{
			await BlowAwayEventCutin_Play(2);
		}).AddTo(this);
		m_blowAwayCutin3.OnClickAsObservable().Subscribe(async delegate
		{
			await BlowAwayEventCutin_Play(3);
		}).AddTo(this);
		_initialized = true;
		_isCharaChanging = false;
		static CutinType CreateDebugCutinType(MFG.Types.Customize.CutinItem.Category _category, int _nakiType, bool _isRon)
		{
			CutinType result = CutinType.Pon;
			switch (_category)
			{
			case MFG.Types.Customize.CutinItem.Category.Naki:
				switch (_nakiType)
				{
				case 0:
					result = CutinType.Pon;
					break;
				case 1:
					result = CutinType.Chi;
					break;
				case 2:
					result = CutinType.Kan;
					break;
				}
				break;
			case MFG.Types.Customize.CutinItem.Category.AgariSmall:
			case MFG.Types.Customize.CutinItem.Category.AgariBig:
				result = ((!_isRon) ? CutinType.Tsumo : CutinType.Ron);
				break;
			case MFG.Types.Customize.CutinItem.Category.Reach:
				result = CutinType.Reach;
				break;
			}
			return result;
		}
	}

	private async UniTask CharaChange(int id)
	{
		if (_isCharaChanging)
		{
			return;
		}
		_isCharaChanging = true;
		Debug.Log("Chara Select:" + id);
		MFG.Types.Customize.Costume.ItemID itemID = (MFG.Types.Customize.Costume.ItemID)(id + 1);
		for (int cha = 0; cha < 4; cha++)
		{
			Player player = players[cha];
			if (!m_mahjongPlayFlow.m_isTaikyokuActive || player.IsValid)
			{
				player.character.m_characterModelController.Dispose();
				player.character.m_characterArmController.Dispose();
				player.m_cha = (Cha)cha;
				CharaType charaType = CostumeItemMaster.GetItem(itemID).CharaType;
				player.SetCharaType(charaType);
				player.CostumeItemEquip = itemID;
				player.IsValid = true;
				await players[cha].character.Initialize(m_charaArmParent[cha], m_chara3DParent[cha], m_3dTexture.transform.localPosition, m_3dTextureTehai.transform.localPosition);
				m_mahjongPlayFlow.ModifyCharaOffsetParam(charaType, player.m_cha, player);
				if (cha == 0)
				{
					GameDataManager.Player.SelectChara = charaType;
				}
			}
		}
		_isCharaChanging = false;
	}

	private async UniTask MotionPlay()
	{
		if (m_motionPlay.interactable)
		{
			m_motionPlay.interactable = false;
			MotionID motionID = Enum.Parse<MotionID>(m_motionSelect.options[m_motionSelect.value].text);
			for (int i = 0; i < 4; i++)
			{
				players[i].character.m_characterModelController.PlayMotion(motionID).Forget();
			}
			await UniTask.WaitWhile(() => players[0].character.m_characterModelController.IsPlaying(motionID));
			m_motionPlay.interactable = true;
		}
	}

	private void TestCharaDisp_SetActive(bool isActive)
	{
		isActive = false;
		m_motionPlay.gameObject.SetActive(isActive);
		m_motionSelect.gameObject.SetActive(isActive);
		m_charaCheck.gameObject.SetActive(isActive);
		m_viewTypeChangeButton.gameObject.SetActive(isActive);
	}

	private void TestCutinDisp_SetActive(bool isActive)
	{
		isActive = false;
		m_cutinRarityDropdown.gameObject.SetActive(isActive);
		m_cutinAssetTypeDropdown.gameObject.SetActive(isActive);
		m_cutinPlay.gameObject.SetActive(isActive);
		m_cutinCancel.gameObject.SetActive(isActive);
		m_blowAwayCutin1.gameObject.SetActive(isActive);
		m_blowAwayCutin2.gameObject.SetActive(isActive);
		m_blowAwayCutin3.gameObject.SetActive(isActive);
		m_btAllInfoDisplay.gameObject.SetActive(isActive);
		m_btDebugPaiDisplay.gameObject.SetActive(isActive);
	}

	private void TestBackScreen_SetActive(bool isActive)
	{
		isActive = false;
		m_cutinCharaDropdown.gameObject.SetActive(isActive);
		m_backScreenChaDropdown.gameObject.SetActive(isActive);
		m_backScreenTypeDropdown.gameObject.SetActive(isActive);
		m_backScreenPlay.gameObject.SetActive(isActive);
	}

	public static void PrepareDebugHaifu()
	{
		PaifuDemoMaster paifuDemoMaster = PaifuDemoList[UnityEngine.Random.Range(0, PaifuDemoList.Count)];
		if (GameDataManager.IsTenshiTenhoDemoEnable() && UnityEngine.Random.Range(1, 100) <= 20)
		{
			paifuDemoMaster = new PaifuDemoMaster
			{
				FileName = "PaifuDemoApril",
				KyokuIndex = 0
			};
		}
		string text = ((TextAsset)Resources.Load(paifuDemoMaster.FileName)).text.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");
		text = "<root>" + text + "</root>";
		XDocument xDocument = XDocument.Parse(text);
		HaifuData haifuData = new HaifuData();
		haifuData.Parse(xDocument.Root, paifuDemoMaster.FileName);
		GameDataManager.Mahjong.Apply(haifuData);
		GameDataManager.Taikyoku.Apply(haifuData);
		GameDataManager.CommandClient.Apply(haifuData, paifuDemoMaster);
	}

	private void Update()
	{
		if (!_initialized || !DebugFlag.IsActive(DebugFlag.Flag.TestCharaDisp))
		{
			return;
		}
		if (Bi2xInput.GetKeyDown(KeyCode.DownArrow))
		{
			m_motionSelect.value = (m_motionSelect.value + 1) % m_motionSelect.options.Count();
			m_motionSelect.RefreshShownValue();
			MotionPlay().Forget();
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.UpArrow))
		{
			m_motionSelect.value = (m_motionSelect.value + m_motionSelect.options.Count() - 1) % m_motionSelect.options.Count();
			m_motionSelect.RefreshShownValue();
			MotionPlay().Forget();
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.LeftArrow))
		{
			if (!_isCharaChanging)
			{
				int num = Enum.GetValues(typeof(MFG.Types.Customize.Costume.ItemID)).Cast<MFG.Types.Customize.Costume.ItemID>().Count() - 1;
				_selectingCostume = (_selectingCostume + num - 1) % num;
				MFG.Types.Customize.Costume.Item item = CostumeItemMaster.GetItem((MFG.Types.Customize.Costume.ItemID)(_selectingCostume + 1));
				string text = item.Name;
				string shortName = MasterGameDataManager.Character.Properties(item.CharaType).Profile.ShortName;
				m_charaCheck.transform.Find("Label").GetComponent<Text>().text = shortName + "/" + text;
				CharaChange(_selectingCostume).Forget();
			}
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.RightArrow) && !_isCharaChanging)
		{
			_selectingCostume = (_selectingCostume + 1) % (Enum.GetValues(typeof(MFG.Types.Customize.Costume.ItemID)).Cast<MFG.Types.Customize.Costume.ItemID>().Count() - 1);
			MFG.Types.Customize.Costume.Item item2 = CostumeItemMaster.GetItem((MFG.Types.Customize.Costume.ItemID)(_selectingCostume + 1));
			string text2 = item2.Name;
			string shortName2 = MasterGameDataManager.Character.Properties(item2.CharaType).Profile.ShortName;
			m_charaCheck.transform.Find("Label").GetComponent<Text>().text = shortName2 + "/" + text2;
			CharaChange(_selectingCostume).Forget();
		}
	}

	private void TableSticker_Init()
	{
		TableSticker_SetActive(DebugFlag.IsActive(DebugFlag.Flag.TestCutinDisp));
	}

	private void TableSticker_SetActive(bool isActive)
	{
		isActive = false;
		m_tableStickerDebugParent.SetActive(isActive);
	}

	private async UniTask TableSticker_Reflesh()
	{
		try
		{
			MFG.Types.Customize.CustomizeItem.ItemID id = Enum.Parse<MFG.Types.Customize.CustomizeItem.ItemID>($"TableSticker{int.Parse(m_tableStickerID):D3}");
			foreach (Cha value in Enum.GetValues(typeof(Cha)))
			{
				await m_taku.Init(value, new List<MFG.Types.Customize.CustomizeItem.ItemID> { id }, new List<StickerEditInfo>
				{
					new StickerEditInfo(id, 0)
				});
			}
		}
		catch (ArgumentException)
		{
			Debug.LogWarning($"指定の TableSticker ItemID が見つかりませんでした。ItemID:{int.Parse(m_tableStickerID):D3}");
		}
	}

	private void ReachStick_Init()
	{
		ReachStick_SetActive(DebugFlag.IsActive(DebugFlag.Flag.TestCutinDisp));
	}

	private void ReachStick_SetActive(bool isActive)
	{
		isActive = false;
		m_reachStickDebugParent.SetActive(isActive);
	}

	private async UniTask ReachStick_Reflesh(Cha cha)
	{
		Player playerWork = m_mahjongPlayFlow.GetPlayerWork(cha);
		foreach (Transform item in playerWork.plate.transform.Find("ReachStickParent").transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		CutinManager component = GameObject.Find("CutinManager").GetComponent<CutinManager>();
		PreviewPlayerInteractor previewPlayerInteractor = m_previewPlayerInteractors[(int)cha];
		MFG.Types.Customize.CustomizeItem.ItemID itemID = component.m_decider.DecideThrownReachStick(previewPlayerInteractor.CustomizeItemEquips()).ItemID;
		await playerWork.ReachStick_Create(itemID);
	}

	private async UniTask ReachStick_Reflesh()
	{
		foreach (PreviewPlayerInteractor previewPlayerInteractor in m_previewPlayerInteractors)
		{
			Cha cha = previewPlayerInteractor.Cha();
			Player player = m_mahjongPlayFlow.GetPlayerWork(cha);
			try
			{
				previewPlayerInteractor.m_reachStickItemID = Enum.Parse<MFG.Types.Customize.CustomizeItem.ItemID>($"ReachStick{int.Parse(m_reachStickItemID):D3}");
			}
			catch (ArgumentException)
			{
				Debug.LogWarning($"指定の ReachStick ItemID が見つかりませんでした。ItemID:{int.Parse(m_reachStickItemID):D3}");
			}
			await ReachStick_Reflesh(cha);
			player.plate.SetActive(value: true);
			player.ReachBouDisp();
		}
	}

	private void PlayerAccessory_Init()
	{
		PlayerAccessory_SetActive(DebugFlag.IsActive(DebugFlag.Flag.TestCutinDisp));
	}

	private void PlayerAccessory_SetActive(bool isActive)
	{
		isActive = false;
		m_playerAccessoryDebugParent.SetActive(isActive);
	}

	private async void PlayerAccessory_Reflesh()
	{
		foreach (PreviewPlayerInteractor previewPlayerInteractor in m_previewPlayerInteractors)
		{
			Cha cha = previewPlayerInteractor.Cha();
			try
			{
				previewPlayerInteractor.m_playerAccessoryItemID = Enum.Parse<MFG.Types.Customize.CustomizeItem.ItemID>($"PlayerAccessory{int.Parse(m_playerAccessoryItemID):D3}");
			}
			catch (ArgumentException)
			{
				Debug.LogWarning($"指定の PlayerAccessory ItemID が見つかりませんでした。ItemID:{int.Parse(m_playerAccessoryItemID):D3}");
			}
			Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> equips = previewPlayerInteractor.CustomizeItemEquips();
			foreach (MFG.Types.Customize.CustomizeItem.Category key in equips.Keys)
			{
				List<MFG.Types.Customize.CustomizeItem.ItemID> list = equips[key];
				if (key == MFG.Types.Customize.CustomizeItem.Category.PlayerAccessory)
				{
					await m_mahjongPlayFlow.CustomizeItem_Instantiate<GameObject>(list[0], m_mahjongPlayFlow.m_viewManager.players[(int)cha].accessory.obj2D.transform);
				}
			}
		}
		if (SystemConfigGameData.IsDualMonitor)
		{
			try
			{
				SubMonitorFlow.GetInstance().ChageAccessory(Enum.Parse<MFG.Types.Customize.CustomizeItem.ItemID>($"PlayerAccessory{int.Parse(m_playerAccessoryItemID):D3}"));
			}
			catch (ArgumentException)
			{
				Debug.LogWarning($"指定の PlayerAccessory ItemID が見つかりませんでした。ItemID:{int.Parse(m_playerAccessoryItemID):D3}");
			}
		}
		m_mahjongPlayFlow.ViewType_Change(MahjongUtility.GetViewType());
	}

	private async UniTask BlowAwayEventCutin_Play(int charaCount)
	{
		m_blowAwayCutin1.interactable = false;
		m_blowAwayCutin2.interactable = false;
		m_blowAwayCutin3.interactable = false;
		m_cts = new CancellationTokenSource();
		_blowAwayEventResultInfo.BlowAwayInfo.Init(Players);
		foreach (var item in _blowAwayEventResultInfo.BlowAwayInfo.ChaInfos.Values.Select((ChaInfo chaInfo, int index) => new { chaInfo, index }))
		{
			item.chaInfo.BlowawayState = ((item.index < charaCount) ? BlowawayState.Blowawayed : BlowawayState.None);
		}
		try
		{
			await m_mahjongPlayFlow.BlowAwayEventCutin_Play(_blowAwayEventResultInfo, m_cts.Token);
			await m_mahjongPlayFlow.BlowAwayEventCutin_End(m_cts.Token);
		}
		catch (OperationCanceledException)
		{
		}
		m_blowAwayCutin1.interactable = true;
		m_blowAwayCutin2.interactable = true;
	}
}

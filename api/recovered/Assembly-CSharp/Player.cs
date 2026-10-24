using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Competition;
using MFG.Types.Customize.Catchphrase;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.YellCampaign;
using TMPro;
using UniRx;
using UnityEngine;

public class Player : MonoBehaviour
{
	private enum TenpaiEffectType
	{
		None,
		Tenpai,
		Mangan
	}

	[SerializeField]
	public Hand hand;

	[SerializeField]
	public Meld meld;

	[SerializeField]
	public Dump dump;

	[SerializeField]
	public Character character;

	[SerializeField]
	public GameObject m_taku;

	[SerializeField]
	public GameObject m_proStand;

	[SerializeField]
	public MachihaiPanel m_machihaiPanel;

	[SerializeField]
	public GameObject plate;

	[SerializeField]
	public GameObject line;

	[SerializeField]
	public ExCall m_exCall;

	[SerializeField]
	public GameObject spotLight;

	[SerializeField]
	public GameObject spotLightBehind;

	[SerializeField]
	public GameObject myOrder;

	[SerializeField]
	public ComebackTakuBonusDisplay m_comebackTakuBonusDisp;

	public Sprite m_maAgariSprite;

	public int m_level;

	public DanLevelType m_danLevel;

	public Kaze m_jikaze;

	public Kaze m_startKaze;

	public Cha m_cha;

	public bool m_isCPU;

	public bool m_isPro;

	public bool m_isOdekake;

	public YellRank m_yellRank;

	public CompetitionID m_compeID;

	public TrophyType m_trophyType;

	public bool m_isReach;

	public bool m_isDoubleReach;

	public bool m_isOpenReach;

	public bool m_isFireReach;

	public bool m_isAtomicReach;

	public bool m_isNaki;

	public bool m_isFirstTsumo = true;

	public bool m_isReachSengen;

	public bool m_isBomber;

	public bool m_isIppatsuVoice;

	public bool m_isFireReachUsed;

	public bool m_isOpenReachUsed;

	public List<Pai> m_furitenPais = new List<Pai>();

	public List<Pai> m_openMachiPais = new List<Pai>();

	public bool m_isOpenFuriten;

	public bool m_isCatharsisKing;

	private TenpaiEffectType m_tenpaiEffectType;

	public int m_playerIndexAtCommand;

	public int m_index;

	public long m_score;

	public long m_diffScore;

	public int m_rank;

	public Color m_scoreColor;

	public string m_name;

	public bool m_isDispDiffScore;

	public CharaType m_charaType;

	public int m_charaVariantID;

	public Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> CustomizeItemEquips = new Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>>();

	public List<StickerEditInfo> StickerEditInfos = new List<StickerEditInfo>();

	private GameObject _reachStickEffectInstance;

	private GameObject _reachStickAtomicEffectInstance;

	private CompositeDisposable _reachStickDisposables = new CompositeDisposable();

	public List<CutinItemInfo> CutinItems = new List<CutinItemInfo>();

	public MFG.Types.Customize.Costume.ItemID CostumeItemEquip;

	public MFG.Types.Customize.Catchphrase.ItemID CatchphraseItemEquip;

	public List<VoiceInfo> GachaVoiceItemEquip;

	public bool IsValid;

	public bool IsFakeClear => m_charaType.IsFakeClear(m_charaVariantID);

	public bool IsClearProEventCpu
	{
		get
		{
			if (GameDataManager.IsClearProEventEnable() && m_isCPU)
			{
				return m_charaType == CharaType.Chara12;
			}
			return false;
		}
	}

	public void SetIsOpenFuritenInOpenStart()
	{
		foreach (Pai openMachiPai in m_openMachiPais)
		{
			Pai pai = MahjongUtility.AkaToNormal(openMachiPai);
			if (m_furitenPais.Exists((Pai x) => MahjongUtility.AkaToNormal(x) == pai) || dump.TileList.Exists((Tile x) => MahjongUtility.AkaToNormal(x.pai) == pai))
			{
				m_isOpenFuriten = true;
			}
		}
	}

	public void SetIsOpenFuritenInMinogashi(Pai sutePai)
	{
		if (m_isOpenReach && !m_isOpenFuriten && m_openMachiPais.Exists((Pai x) => MahjongUtility.AkaToNormal(x) == MahjongUtility.AkaToNormal(sutePai)))
		{
			m_isOpenFuriten = true;
		}
	}

	public bool IsProAnimEnable()
	{
		if (GameDataManager.Player.IsPro() && m_isPro)
		{
			return GameDataManager.PlayerConfig.IsProAnimation;
		}
		return m_isPro;
	}

	public void ApplyCustomizeItemEquip(Dictionary<MFG.Types.Customize.CustomizeItem.Category, List<MFG.Types.Customize.CustomizeItem.ItemID>> equipItems, List<CustomizeItemInfo> possessionItems, bool isJicha)
	{
		CustomizeItemEquips.Clear();
		foreach (MFG.Types.Customize.CustomizeItem.Category value in Enum.GetValues(typeof(MFG.Types.Customize.CustomizeItem.Category)))
		{
			List<MFG.Types.Customize.CustomizeItem.ItemID> list = equipItems[value];
			if (list.Count == 0 && CustomizeItemMaster.IsMultiEquipEnableCategory(value))
			{
				CustomizeItemEquips[value] = (from x in possessionItems.GetPossessionItems(value, m_charaType)
					select x.ItemID).ToList();
			}
			else
			{
				CustomizeItemEquips[value] = list;
			}
			if (isJicha && GameUtility.IsKonastaOrNetcafeBehavior)
			{
				CustomizeItemEquips[value] = CustomizeItemEquips[value].Where((MFG.Types.Customize.CustomizeItem.ItemID x) => !CustomizeItemMaster.IsPonnomichi(x)).ToList();
				if (value == MFG.Types.Customize.CustomizeItem.Category.TableSticker)
				{
					StickerEditInfos = StickerEditInfos.Where((StickerEditInfo x) => !CustomizeItemMaster.IsPonnomichi(x.ItemID)).ToList();
				}
			}
			if (CustomizeItemEquips[value].Count == 0 && value != MFG.Types.Customize.CustomizeItem.Category.TableSticker)
			{
				CustomizeItemEquips[value] = new List<MFG.Types.Customize.CustomizeItem.ItemID> { CustomizeItemMaster.GetDefaultEquipItemID(value) };
			}
		}
	}

	public MFG.Types.Customize.CustomizeItem.ItemID GetCustomizeItemEquip(MFG.Types.Customize.CustomizeItem.Category category)
	{
		CustomizeItemEquips.TryGetValue(category, out var value);
		if (value == null || value.Count == 0)
		{
			return CustomizeItemMaster.GetDefaultEquipItemID(category);
		}
		return value[0];
	}

	public MFG.Types.Customize.CustomizeItem.ItemID GetCustomizeItemEquipAtRandom(MFG.Types.Customize.CustomizeItem.Category category, RandomValueGenerator randomValueGenerator)
	{
		CustomizeItemEquips.TryGetValue(category, out var value);
		if (value == null || value.Count == 0)
		{
			return CustomizeItemMaster.GetDefaultEquipItemID(category);
		}
		return value[randomValueGenerator.Range(0, value.Count)];
	}

	private void Awake()
	{
		if (plate != null)
		{
			ReachStick_SetActive(isActive: false);
			plate.transform.Find("Text_Jikaze").GetComponent<TextMeshPro>().text = "";
			plate.transform.Find("Text_Score").GetComponent<TextMeshPro>().text = "";
			OpenReflesh();
			m_machihaiPanel.Destruct();
			plate.SetActive(value: false);
			m_furitenPais.Clear();
		}
	}

	private void Start()
	{
	}

	public void Destruct()
	{
		hand.Destruct();
		meld.Destruct();
		dump.Destruct();
		ReachStick_SetActive(isActive: false);
		m_machihaiPanel.Destruct();
		OpenReflesh();
		SetActiveLine(active: false);
		m_furitenPais.Clear();
	}

	public async UniTask Tsumo(Han han, int doraCount, int meldBonusHan, int comebackBonusHan, int allgreenBonusHan, List<Yaku> yakus, int akadoraCount, Cha bombCha, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		bool isMangan = han >= Han.Mangan;
		if (character.m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => character.m_cutinManager.IsPlaying, PlayerLoopTiming.Update, cancellationToken);
		}
		if (isMangan)
		{
			await character.Action(Character.EventID.TsumoAgari2, han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, akadoraCount, null, null, bombCha, Pai.NONE, cancellationToken);
		}
		else
		{
			await character.Action(Character.EventID.TsumoAgari1, han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, akadoraCount, null, null, bombCha, Pai.NONE, cancellationToken);
		}
	}

	public async UniTask Ron(Han han, int doraCount, int meldBonusHan, int comebackBonusHan, int allgreenBonusHan, List<Yaku> yakus, int akadoraCount, Player furikomiPlayer, Cha bombCha, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (character.m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => character.m_cutinManager.IsPlaying, PlayerLoopTiming.Update, cancellationToken);
		}
		if (han >= Han.Mangan)
		{
			await character.Action(Character.EventID.RonAgari2, han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, akadoraCount, furikomiPlayer, null, bombCha, Pai.NONE, cancellationToken);
		}
		else
		{
			await character.Action(Character.EventID.RonAgari1, han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, akadoraCount, furikomiPlayer, null, bombCha, Pai.NONE, cancellationToken);
		}
	}

	public async UniTask Reach(bool isDoubleReach, bool isFireReach, bool isOpenReach, bool isAtomicReach, MahjongMusicTextDisplay musicText, Pai pai, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_isReach = true;
		m_isDoubleReach = isDoubleReach;
		m_isFireReach = isFireReach;
		m_isOpenReach = isOpenReach;
		m_isAtomicReach = isAtomicReach;
		m_isReachSengen = true;
		m_isIppatsuVoice = true;
		MahjongPlayEvent.StartReachBouDisp(this);
		if (character.m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => character.m_cutinManager.IsPlaying, PlayerLoopTiming.Update, cancellationToken);
		}
		await character.Action(isDoubleReach ? Character.EventID.DReach : Character.EventID.Reach, Han.Nothing, 0, 0, 0, 0, null, 0, null, musicText, (Cha)(-1), pai, cancellationToken);
	}

	public void ReachBouDisp(bool isDisp = true)
	{
		ReachStick_SetActive(isDisp);
	}

	public async UniTask RyuKyoku(int[] machiPaiID, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		int num = 0;
		for (int i = 0; i < machiPaiID.Length; i++)
		{
			if (machiPaiID[i] != 0)
			{
				num++;
			}
		}
		if (num > 0)
		{
			if (IsJicha())
			{
				VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Tenpai);
			}
			await character.Action(Character.EventID.Tenpai, Han.Nothing, 0, 0, 0, 0, null, 0, null, null, (Cha)(-1), Pai.NONE, cancellationToken);
			SoundManager.PlayVoiceCha("VOX_GXX_MJG_FINISH2", m_charaType, m_cha).Forget();
			m_machihaiPanel.SetMachiPaiRyuKyoku(machiPaiID);
			SetTenpai();
			m_exCall.Play(ExCall.Status.Tenpai, m_index);
			await UniTask.Delay(TimeSpan.FromSeconds(0.3333333432674408), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			SoundManager.PlaySeCha("SFX_MJG_DISPLAY8", m_cha).Forget();
		}
		else
		{
			if (IsJicha())
			{
				VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Noten);
			}
			await character.Action(Character.EventID.Noten, Han.Nothing, 0, 0, 0, 0, null, 0, null, null, (Cha)(-1), Pai.NONE, cancellationToken);
			SoundManager.PlayVoiceCha("VOX_GXX_MJG_FINISH1", m_charaType, m_cha).Forget();
			m_machihaiPanel.gameObject.SetActive(value: false);
			SetNoten();
			m_exCall.Play(ExCall.Status.Noten, m_index);
			await UniTask.Delay(TimeSpan.FromSeconds(0.3333333432674408), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			SoundManager.PlaySeCha("SFX_MJG_DISPLAY9", m_cha).Forget();
		}
	}

	public void RyuKyokuExit()
	{
		m_exCall.Exit(m_index);
	}

	public async UniTask Pon(Player sutePlayer, Pai pai0, Pai pai1, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (character.m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => character.m_cutinManager.IsPlaying, PlayerLoopTiming.Update, cancellationToken);
		}
		character.Action(Character.EventID.Pon, Han.Nothing, 0, 0, 0, 0, null, 0, null, null, (Cha)(-1), Pai.NONE, cancellationToken).Forget();
		Tile item = sutePlayer.dump.PlayNakiHaiTargetEffect(sutePlayer.m_cha);
		await UniTask.Delay(TimeSpan.FromSeconds(0.25), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		Tile tile = hand.GetTile(pai0);
		Tile tile2 = hand.GetTile(pai1);
		hand.SelectReset().Forget();
		if (item == null || tile == null || tile2 == null)
		{
			Debug.LogError("CommandPon Item == null");
			return;
		}
		meld.AddPon(MahjongUtility.GetCha((Kaze)m_index, (Kaze)sutePlayer.m_index), item, tile, tile2);
		m_isNaki = true;
	}

	public async UniTask Chi(Player sutePlayer, Pai pai0, Pai pai1, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (character.m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => character.m_cutinManager.IsPlaying, PlayerLoopTiming.Update, cancellationToken);
		}
		character.Action(Character.EventID.Chi, Han.Nothing, 0, 0, 0, 0, null, 0, null, null, (Cha)(-1), Pai.NONE, cancellationToken).Forget();
		Tile item = sutePlayer.dump.PlayNakiHaiTargetEffect(sutePlayer.m_cha);
		await UniTask.Delay(TimeSpan.FromSeconds(0.25), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		Tile tile = hand.GetTile(pai0);
		Tile tile2 = hand.GetTile(pai1);
		hand.SelectReset().Forget();
		if (item == null || tile == null || tile2 == null)
		{
			Debug.LogError("CommandChi Item == null");
			return;
		}
		meld.AddChi(item, tile, tile2);
		m_isNaki = true;
	}

	public async UniTask Minkan(Player sutePlayer, Pai pai, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (character.m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => character.m_cutinManager.IsPlaying, PlayerLoopTiming.Update, cancellationToken);
		}
		character.Action(Character.EventID.Kan, Han.Nothing, 0, 0, 0, 0, null, 0, null, null, (Cha)(-1), Pai.NONE, cancellationToken).Forget();
		Tile item = sutePlayer.dump.PlayNakiHaiTargetEffect(sutePlayer.m_cha);
		await UniTask.Delay(TimeSpan.FromSeconds(0.25), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		Tile tile;
		Tile tile2;
		Tile tile3;
		if (MahjongUtility.IsAkahai(pai))
		{
			tile = hand.GetTile(pai);
			if (tile == null)
			{
				tile = hand.GetTile(MahjongUtility.AkaToNormal(pai));
			}
			tile2 = hand.GetTile(MahjongUtility.AkaToNormal(pai));
			if (tile2 == null)
			{
				tile2 = hand.GetTile(pai);
			}
			tile3 = hand.GetTile(MahjongUtility.AkaToNormal(pai));
			if (tile3 == null)
			{
				tile3 = hand.GetTile(pai);
			}
		}
		else
		{
			tile = hand.GetTile(MahjongUtility.NormalToAka(pai));
			if (tile == null)
			{
				tile = hand.GetTile(pai);
			}
			tile2 = hand.GetTile(MahjongUtility.NormalToAka(pai));
			if (tile2 == null)
			{
				tile2 = hand.GetTile(pai);
			}
			tile3 = hand.GetTile(pai);
			if (tile3 == null)
			{
				tile3 = hand.GetTile(MahjongUtility.NormalToAka(pai));
			}
		}
		hand.SelectReset().Forget();
		if (item == null || tile == null || tile2 == null || tile3 == null)
		{
			Debug.LogError("CommandMinkan Item == null");
			return;
		}
		meld.AddMinkan(MahjongUtility.GetCha((Kaze)m_index, (Kaze)sutePlayer.m_index), item, tile, tile2, tile3);
		m_isNaki = true;
	}

	public async UniTask Ankan(Pai pai, CancellationToken cancellationToken = default(CancellationToken))
	{
		Debug.Log("Player Ankan/" + pai);
		cancellationToken.ThrowIfCancellationRequested();
		if (character.m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => character.m_cutinManager.IsPlaying, PlayerLoopTiming.Update, cancellationToken);
		}
		character.Action(Character.EventID.Kan, Han.Nothing, 0, 0, 0, 0, null, 0, null, null, (Cha)(-1), Pai.NONE, cancellationToken).Forget();
		pai = MahjongUtility.AkaToNormal(pai);
		Tile tile = hand.GetTile(MahjongUtility.NormalToAka(pai));
		if (tile == null)
		{
			tile = hand.GetTile(pai);
		}
		Tile tile2 = hand.GetTile(MahjongUtility.NormalToAka(pai));
		if (tile2 == null)
		{
			tile2 = hand.GetTile(pai);
		}
		Tile tile3 = hand.GetTile(pai);
		if (tile3 == null)
		{
			tile3 = hand.GetTile(MahjongUtility.NormalToAka(pai));
		}
		Tile tile4 = hand.GetTile(pai);
		if (tile4 == null)
		{
			tile4 = hand.GetTile(MahjongUtility.NormalToAka(pai));
		}
		hand.SelectReset().Forget();
		if (tile == null || tile2 == null || tile3 == null || tile4 == null)
		{
			Debug.LogError("CommandAnkan Item == null");
		}
		else
		{
			meld.AddAnkan(tile, tile2, tile3, tile4);
		}
	}

	public async UniTask Kakan(Pai pai, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (character.m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => character.m_cutinManager.IsPlaying, PlayerLoopTiming.Update, cancellationToken);
		}
		character.Action(Character.EventID.Kan, Han.Nothing, 0, 0, 0, 0, null, 0, null, null, (Cha)(-1), Pai.NONE, cancellationToken).Forget();
		pai = MahjongUtility.AkaToNormal(pai);
		Tile tile = hand.GetTile(pai);
		if (tile == null)
		{
			tile = hand.GetTile(MahjongUtility.NormalToAka(pai));
		}
		hand.SelectReset().Forget();
		if (tile == null)
		{
			Debug.LogError("CommandKakan Item == null");
			return;
		}
		meld.AddKakan(tile, pai);
		m_isNaki = true;
	}

	public void SetJikaze(Kaze jikaze)
	{
		string[] array = new string[4] { "東", "南", "西", "北" };
		m_jikaze = jikaze;
		plate.transform.Find("Text_Jikaze").GetComponent<TextMeshPro>().text = array[(int)m_jikaze];
		plate.transform.Find("Text_Jikaze").GetComponent<TextMeshPro>().color = ((m_jikaze == Kaze.Ton) ? Color.yellow : Color.white);
		ReachStick_SetActive(isActive: false);
		m_isReach = false;
		m_isDoubleReach = false;
		m_isFireReach = false;
		m_isOpenReach = false;
		m_isAtomicReach = false;
		m_isNaki = false;
		m_isFirstTsumo = true;
		m_tenpaiEffectType = TenpaiEffectType.None;
		m_isOpenFuriten = false;
		m_isReachSengen = false;
		m_isBomber = false;
		m_isIppatsuVoice = false;
	}

	public void SetStartKaze(Kaze kaze)
	{
		m_startKaze = kaze;
		plate.SetActive(value: true);
	}

	public void SetScore(long score)
	{
		m_score = score;
	}

	public void SetName(string name)
	{
		m_name = name;
	}

	public void SetCharaType(CharaType chara, int charaVariantID = 0)
	{
		m_charaType = chara;
		m_charaVariantID = charaVariantID;
	}

	public void SetTenpai()
	{
		hand.transform.localPosition = new Vector3(hand.transform.localPosition.x, 0f, hand.transform.localPosition.z);
		hand.transform.DOLocalRotateQuaternion(Quaternion.Euler(0f, 0f, 0f), 0.2f).SetEase(Ease.InQuad).OnComplete(delegate
		{
			SoundManager.PlaySeCha("SFX_MJG_TILE5", m_cha).Forget();
		});
		hand.StartDoraEffect();
		meld.StartDoraEffect();
		hand.IsKeyTextEnable = false;
		hand.AllKeyboardTextControll(isActive: false);
	}

	public void SetNoten()
	{
		hand.transform.localPosition = new Vector3(hand.transform.localPosition.x, 0f, hand.transform.localPosition.z);
		hand.transform.DOLocalRotateQuaternion(Quaternion.Euler(-180f, 0f, 0f), 0.2f).SetEase(Ease.InQuad).OnComplete(delegate
		{
			SoundManager.PlaySeCha("SFX_MJG_TILE6", m_cha).Forget();
		});
		hand.IsKeyTextEnable = false;
		hand.AllKeyboardTextControll(isActive: false);
	}

	public void OpenReflesh()
	{
		bool flag = MahjongUtility.IsOpen();
		hand.transform.localPosition = new Vector3(hand.transform.localPosition.x, 0.0017f * (float)((!flag) ? 1 : 0), hand.transform.localPosition.z);
		if (base.name == "Player1")
		{
			hand.transform.DOLocalRotateQuaternion(Quaternion.Euler(flag ? 0f : (-70f), 0f, 0f), 0f);
		}
		else
		{
			hand.transform.DOLocalRotateQuaternion(Quaternion.Euler(flag ? 0f : (-90f), 0f, 0f), 0f);
		}
		if (m_isOpenReach)
		{
			SetTenpai();
		}
	}

	public void SetRank(int rank)
	{
		m_rank = rank;
		m_scoreColor = Color.white;
		if (m_rank == MahjongUtility.GetWorstRank(GameDataManager.Mahjong.TakuType))
		{
			m_scoreColor = Color.red;
		}
		else if (m_rank == 0)
		{
			m_scoreColor = Color.cyan;
		}
	}

	public void SetFinalRank(int rank)
	{
		m_rank = rank;
	}

	public bool IsJicha()
	{
		return m_index == 0;
	}

	public void SetActiveLine(bool active)
	{
		line.SetActive(active);
		if (active)
		{
			string[] array = new string[4] { "Zi", "Shimo", "Toi", "Kami" };
			foreach (Transform item in m_taku.transform)
			{
				if (item.gameObject.activeSelf)
				{
					item.GetComponent<Animator>().SetTrigger(array[(int)m_cha]);
				}
			}
		}
		if (IsJicha())
		{
			if (spotLight.activeSelf)
			{
				spotLight.GetComponent<Animator>().ResetTrigger("Wait");
				spotLight.GetComponent<Animator>().SetTrigger("Start");
			}
			if (myOrder != null && myOrder.activeSelf)
			{
				myOrder.GetComponent<Animator>().ResetTrigger("Exit");
				myOrder.GetComponent<Animator>().SetTrigger("In");
			}
		}
		else
		{
			spotLight.SetActive(active);
			spotLightBehind.SetActive(active);
		}
	}

	public void StartDoraEffect()
	{
		if (IsJicha())
		{
			hand.StartDoraEffect();
		}
		meld.StartDoraEffect();
	}

	public void StartAgariDoraEffect(bool isReachAgari = true)
	{
		hand.StartDoraEffect(isReachAgari);
		meld.StartDoraEffect(isReachAgari);
	}

	public int GetDoraCount(int ignoreIndex = -1)
	{
		return hand.GetDoraCount(ignoreIndex) + meld.GetDoraCount();
	}

	public void StartTenpaiEffect(bool isMangan, bool isRestart = false)
	{
		if (isMangan && m_tenpaiEffectType != TenpaiEffectType.Mangan)
		{
			SoundManager.PlaySe("SFX_MJG_TILE2").Forget();
			m_tenpaiEffectType = TenpaiEffectType.Mangan;
		}
		if (!isMangan && m_tenpaiEffectType != TenpaiEffectType.Tenpai)
		{
			SoundManager.PlaySe("SFX_MJG_TILE4").Forget();
			m_tenpaiEffectType = TenpaiEffectType.Tenpai;
		}
		hand.StartTenpaiEffect(isMangan, isRestart);
		meld.StartTenpaiEffect(isMangan, isRestart);
	}

	public void StopTenpaiEffect()
	{
		m_tenpaiEffectType = TenpaiEffectType.None;
		hand.StopTenpaiEffect();
		meld.StopTenpaiEffect();
	}

	public async UniTask ReachStick_Create(MFG.Types.Customize.CustomizeItem.ItemID itemID, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		_reachStickDisposables.Clear();
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.ResolveCustomizeItem(itemID, MFG.Types.Customize.CustomizeItem.Category.ReachStick), null, cancellationToken);
		disposableAsset.AddTo(_reachStickDisposables);
		Transform transform = plate.transform.Find("ReachStickParent").transform;
		foreach (Transform item in transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		GameObject reachStick = UnityEngine.Object.Instantiate(disposableAsset.Asset, transform);
		IDisposableAsset<GameObject> disposableAsset2 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/LandedFireReachStickEffect.prefab", null, cancellationToken);
		disposableAsset2.AddTo(_reachStickDisposables);
		Transform transform2 = reachStick.transform.Find("Base");
		_reachStickEffectInstance = UnityEngine.Object.Instantiate(disposableAsset2.Asset, transform2.transform);
		IDisposableAsset<GameObject> disposableAsset3 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/LandedAtomicReachStickEffect.prefab", null, cancellationToken);
		disposableAsset3.AddTo(_reachStickDisposables);
		Transform transform3 = reachStick.transform.Find("Base");
		_reachStickAtomicEffectInstance = UnityEngine.Object.Instantiate(disposableAsset3.Asset, transform3.transform);
	}

	private void ReachStick_SetActive(bool isActive)
	{
		plate.transform.Find("ReachStickParent").gameObject.SetActive(isActive);
		if (_reachStickEffectInstance != null)
		{
			if (isActive && m_isFireReach)
			{
				_reachStickEffectInstance.GetComponent<ParticleSystem>().Play();
			}
			else
			{
				_reachStickEffectInstance.GetComponent<ParticleSystem>().Clear();
				_reachStickEffectInstance.GetComponent<ParticleSystem>().Stop();
			}
		}
		if (_reachStickAtomicEffectInstance != null)
		{
			if (isActive && m_isAtomicReach)
			{
				_reachStickAtomicEffectInstance.GetComponent<ParticleSystem>().Play();
				return;
			}
			_reachStickAtomicEffectInstance.GetComponent<ParticleSystem>().Clear();
			_reachStickAtomicEffectInstance.GetComponent<ParticleSystem>().Stop();
		}
	}

	public void SetDiffScore(long score)
	{
		m_diffScore = score;
	}

	public void RefleshScoreDisp()
	{
		TextMeshPro component = plate.transform.Find("Text_Score").GetComponent<TextMeshPro>();
		if (component == null)
		{
			return;
		}
		if (m_isDispDiffScore)
		{
			if (IsJicha())
			{
				component.text = "あなたとの点差";
				component.color = Color.white;
			}
			else if (m_diffScore > 0)
			{
				component.text = GetScoreText(m_diffScore);
				component.color = Color.cyan;
			}
			else if (m_diffScore < 0)
			{
				component.text = GetScoreText(m_diffScore);
				component.color = Color.red;
			}
			else
			{
				component.text = GetScoreText(m_diffScore);
				component.color = Color.white;
			}
		}
		else
		{
			component.text = GetScoreText(m_score, dispPlusSign: false);
			component.color = m_scoreColor;
		}
		if (SystemConfigGameData.IsDualMonitor)
		{
			SubMonitorFlow.GetInstance().ScoreRefresh(m_cha, component.text, component.color);
			SubMonitorFlow.GetInstance().KazeRefresh(m_cha, m_jikaze);
		}
	}

	private string GetScoreText(long score, bool dispPlusSign = true)
	{
		if (GameDataManager.Mahjong.IsAotenjoMode && Math.Abs(score) >= MahjongUtility.Aotenjo.MaxScore)
		{
			return "<align=\"center\"><color=red>ERROR</color>";
		}
		string text = "";
		if (score > 0 && dispPlusSign)
		{
			text = "+";
		}
		else if (score < 0)
		{
			text = "－";
		}
		return text + Math.Abs(score).ToString("N0");
	}

	public void CutinItem_Log(Func<CutinItemInfo, bool> predicate)
	{
		int num = CutinItems.Count(predicate);
		Debug.Log($"<color=cyan>CutinItem Count : {num}</color>");
		if (num <= 0)
		{
			return;
		}
		foreach (CutinItemInfo cutinItem in CutinItems)
		{
			if (predicate(cutinItem))
			{
				Debug.Log($" <color=cyan>{cutinItem.ItemID}</color>");
			}
		}
	}
}

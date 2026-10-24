using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.MahjonPlay.Hand;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;

public class Hand : MonoBehaviour
{
	public enum EnableDiscardType
	{
		False,
		AtomicEnable,
		OpenEnable,
		True
	}

	[SerializeField]
	private Player player;

	[SerializeField]
	private GameObject releaseEffect;

	[SerializeField]
	private GameObject releaseFastEffect;

	[SerializeField]
	private GameObject tsumoEffectObj;

	private TileManager tileManager;

	private List<Tile> Tiles = new List<Tile>();

	private const float SpaceX = 0.01f;

	private Tile m_selectedTile;

	private Tile m_discardTile;

	private bool m_nowTsumoChoice;

	private bool m_nowExecTsumoChoices;

	[SerializeField]
	private Subject<(Pai, Cha)> onAddTsumoHai = new Subject<(Pai, Cha)>();

	[SerializeField]
	public Subject<(Pai, Cha)> onDiscardTsumoHai = new Subject<(Pai, Cha)>();

	[SerializeField]
	public Subject<Cha> onStartReach = new Subject<Cha>();

	[SerializeField]
	private Subject<bool> onSwitchSelectedHai = new Subject<bool>();

	[SerializeField]
	private Subject<Cha> onTurnComing = new Subject<Cha>();

	public bool IsKeyTextEnable;

	private Tile m_targetItem;

	private Tile m_effectItem;

	public bool m_isReach;

	public bool m_isOpenReach;

	public bool m_isTsumogiri;

	public bool m_isAnpai;

	public bool m_isKikenhai;

	public bool m_isFastDiscard;

	public bool m_isTenpaiTargetIconDispTiming;

	public bool m_isOpenTargetIconDispTiming;

	public IObservable<(Pai pai, Cha cha)> OnAddTsumoHai => onAddTsumoHai.AsObservable();

	public IObservable<(Pai pai, Cha cha)> OnDiscardTsumoHai => onDiscardTsumoHai.AsObservable();

	public IObservable<Cha> OnStartReach => onStartReach.AsObservable();

	public IObservable<bool> OnSwitchSelectedHai => onSwitchSelectedHai.AsObservable();

	public IObservable<Cha> OnTurnComing => onTurnComing.AsObservable();

	private void Start()
	{
		tileManager = GameObject.Find("TileManager")?.GetComponent<TileManager>();
		if (tsumoEffectObj != null)
		{
			tsumoEffectObj.GetComponent<PlayableDirector>().stopped += TimeLineStopped;
		}
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.tag == "Debug")
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	private void Update()
	{
		if (IsKeyTextEnable)
		{
			AllKeyboardTextControll(GameDataManager.PlayerConfig.IsKeyboardGuideDisplay());
		}
	}

	public Tile GetLastItem()
	{
		if (Tiles.Count == 0)
		{
			return null;
		}
		return Tiles.Last();
	}

	public async UniTask StartHaipaiAnime()
	{
		float x = -90f;
		if (player.IsJicha())
		{
			x = -100f;
			IsKeyTextEnable = true;
		}
		for (int j = 0; j < Tiles.Count; j++)
		{
			Tile tile = Tiles[j];
			tile.gameObject.SetActive(value: true);
			tile.transform.localRotation = Quaternion.Euler(x, 0f, 0f);
		}
		for (int i = 0; i < Tiles.Count; i++)
		{
			Tile tile2 = Tiles[i];
			if (player.IsJicha())
			{
				SoundManager.PlaySe("SFX_MJG_TILE1").Forget();
			}
			tile2.transform.DOLocalRotateQuaternion(Quaternion.Euler(0f, 0f, 0f), 0.5f).SetEase(Ease.OutQuart).ToUniTask()
				.Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
		}
	}

	public void HaipaiAdd(Pai pai, int delay)
	{
		Tile tile = tileManager.GetTile(base.transform, pai);
		Tiles.Add(tile);
		ResetPos(animation: false).Forget();
		onAddTsumoHai.OnNext((pai, player.m_cha));
		if (player.IsJicha())
		{
			tile.SetOpen();
			tile.GetComponent<BoxCollider>().enabled = true;
			tile.SetLayer(isJichaTehai: true);
		}
		tile.gameObject.SetActive(value: false);
	}

	public async UniTask AddRonPai(Pai pai, bool isReachAgari)
	{
		TileSort();
		Tile tile = tileManager.GetTile(base.transform, pai);
		Tiles.Add(tile);
		ResetPos(animation: false).Forget();
		player.StartAgariDoraEffect(isReachAgari);
		if (player.IsJicha())
		{
			tile.SetLayer(isJichaTehai: true);
			tile.SetOpen();
			tile.GetComponent<BoxCollider>().enabled = true;
		}
	}

	public async UniTask Add(Pai pai, bool tsumoEffect = false, bool isRinshanTsumo = false)
	{
		TileSort();
		Tile item = tileManager.GetTile(base.transform, pai);
		Tiles.Add(item);
		ResetPos(animation: false).Forget();
		if (tsumoEffect)
		{
			onAddTsumoHai.OnNext((pai, player.m_cha));
		}
		if (!player.IsJicha())
		{
			return;
		}
		item.SetLayer(isJichaTehai: true);
		if (tsumoEffect)
		{
			bool finish = false;
			if (isRinshanTsumo)
			{
				item.StartRinshanEffect();
			}
			item.transform.Translate(new Vector3(0f, 0f, 0.15f));
			item.m_tweener = item.transform.DOLocalMoveZ(0f, 0.2f).SetEase(Ease.OutCubic).OnComplete(delegate
			{
				finish = true;
				if (MahjongUtility.IsDora(item.pai))
				{
					SoundManager.PlaySe("SFX_MJG_PICK2").Forget();
				}
				else
				{
					SoundManager.PlaySe("SFX_MJG_PICK1").Forget();
				}
				TsumoEffect(item);
				item.m_tweener = null;
			});
			await UniTask.WaitUntil(() => finish);
		}
		player.StartDoraEffect();
		item.SetOpen();
		item.GetComponent<BoxCollider>().enabled = true;
	}

	public async UniTask TurnComing()
	{
		onTurnComing.OnNext(player.m_cha);
	}

	public List<int> GetPaiPos(List<Pai> pais)
	{
		List<Pai> list = new List<Pai>();
		List<int> list2 = new List<int>();
		foreach (Tile tile in Tiles)
		{
			list.Add(tile.pai);
		}
		Debug.Log("牌サーチリスト " + string.Join(",", list));
		for (int i = 0; i < pais.Count; i++)
		{
			Debug.Log($"牌サーチ検索するやつ  {pais[i]}");
			for (int j = 0; j < Tiles.Count; j++)
			{
				if (list[j] == pais[i])
				{
					list2.Add(j);
					list[j] = Pai.NONE;
					break;
				}
			}
		}
		return list2;
	}

	public List<int> GetPaiPosAll(Pai pai)
	{
		List<int> list = new List<int>();
		bool flag = false;
		for (int i = 0; i < Tiles.Count; i++)
		{
			if (Tiles[i].pai == pai || Tiles[i].pai == MahjongUtility.NormalToAka(pai))
			{
				flag = true;
				list.Add(i);
			}
		}
		if (!flag)
		{
			list.Add(-1);
		}
		return list;
	}

	public int GetItemCount()
	{
		return Tiles.Count;
	}

	public int GetSelectingItemPos()
	{
		return GetItemPos(GetSelectedTile());
	}

	public int GetItemPos(Tile item)
	{
		for (int i = 0; i < Tiles.Count; i++)
		{
			if (Tiles[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetTimeUpPaiPos()
	{
		for (int i = 0; i < Tiles.Count; i++)
		{
			if (Tiles[i].m_isSelected)
			{
				return i;
			}
		}
		if (Tiles[Tiles.Count - 1].m_enableDiscard)
		{
			return Tiles.Count - 1;
		}
		for (int j = 0; j < Tiles.Count; j++)
		{
			if (Tiles[j].m_enableDiscard)
			{
				return j;
			}
		}
		return Tiles.Count - 1;
	}

	public Tile GetTile(int index)
	{
		if (index < 0 || Tiles.Count <= index)
		{
			return null;
		}
		return Tiles[index];
	}

	public void DiscardPreAction(Pai pai, bool isReach, bool isOpenReach, bool isTsumogiri, bool isAnpai, bool isKikenhai, bool isFast = false)
	{
		Tile item = null;
		if (isTsumogiri)
		{
			item = GetLastItem();
		}
		else
		{
			foreach (Tile tile in Tiles)
			{
				if (tile.pai == pai)
				{
					item = tile;
					break;
				}
			}
		}
		DiscardPreAction(item, isReach, isOpenReach, isTsumogiri, isAnpai, isKikenhai, isFast);
	}

	public void DiscardPreAction(Tile item, bool isReach, bool isOpenReach, bool isTsumogiri, bool isAnpai, bool isKikenhai, bool isFast = false)
	{
		Debug.Log($"{player.m_cha}捨て牌動作情報 tile : {item?.pai},m_isReach : {m_isReach}, isReach : {isReach}, isTsumogiri : {isTsumogiri}\n");
		if (!item)
		{
			return;
		}
		if (m_isReach != isReach && isReach)
		{
			onStartReach.OnNext(player.m_cha);
		}
		m_isReach = isReach;
		m_isOpenReach = isOpenReach;
		m_isTsumogiri = isTsumogiri;
		m_isAnpai = isAnpai;
		m_isKikenhai = isKikenhai;
		m_isFastDiscard = isFast;
		m_targetItem = item;
		item.IsDiscardStart = true;
		if (player.IsJicha() || isTsumogiri)
		{
			m_effectItem = item;
		}
		else
		{
			int num = UnityEngine.Random.Range(0, Tiles.Count - 1);
			if (num < 0)
			{
				num = 0;
			}
			m_effectItem = Tiles[num];
		}
		ReleaseEffect(m_effectItem, isTsumogiri, isFast);
		m_effectItem.gameObject.SetActive(value: false);
	}

	public async UniTask Discard(bool isReachThisTurn)
	{
		if (m_isTsumogiri && !m_isReach)
		{
			await player.character.Action(Character.EventID.Tsumogiri);
		}
		if ((bool)m_targetItem)
		{
			VoiceManager.ChunBeamEnableJudge(player.m_index, m_targetItem.pai);
			m_effectItem.gameObject.SetActive(value: true);
			Vector3 position = m_effectItem.transform.position;
			m_effectItem.transform.position = m_targetItem.transform.position;
			m_targetItem.transform.position = position;
			Tiles.Remove(m_targetItem);
			TileSort();
			AllEnableDiscard(enable: true);
			await player.dump.Add(m_targetItem, m_isReach, m_isTsumogiri, m_isAnpai, m_isKikenhai);
			await ResetPos(player.IsJicha());
			onDiscardTsumoHai.OnNext((m_targetItem.pai, player.m_cha));
		}
		if (!isReachThisTurn && player.m_isIppatsuVoice)
		{
			player.m_isIppatsuVoice = false;
		}
		m_targetItem = null;
		m_effectItem = null;
	}

	private void ReleaseEffect(Tile item, bool isTsumogiri, bool isFast)
	{
		if (isFast)
		{
			releaseFastEffect.transform.position = item.transform.position;
			releaseFastEffect.transform.Find("Line").GetComponent<ParticleSystem>().Play();
			releaseFastEffect.transform.Find("Kira").GetComponent<ParticleSystem>().Play();
			releaseFastEffect.transform.Find("Lightning").GetComponent<ParticleSystem>().Play();
		}
		else
		{
			releaseEffect.transform.position = item.transform.position;
			releaseEffect.transform.Find("Line").GetComponent<ParticleSystem>().Play();
			releaseEffect.transform.Find("Kira").GetComponent<ParticleSystem>().Play();
		}
	}

	private void TsumoEffect(Tile item)
	{
		if (!(tsumoEffectObj == null))
		{
			tsumoEffectObj.SetActive(value: true);
			tsumoEffectObj.transform.position = item.transform.position;
		}
	}

	private async void TimeLineStopped(PlayableDirector pd)
	{
		tsumoEffectObj.SetActive(value: false);
	}

	public Tile GetTile(Pai pai)
	{
		foreach (Tile tile in Tiles)
		{
			if (tile.pai == pai)
			{
				Tiles.Remove(tile);
				return tile;
			}
		}
		return null;
	}

	public void TileSort()
	{
		Tiles.Sort(Compare);
	}

	public static int Compare(Tile a, Tile b)
	{
		Pai pai = a.pai;
		Pai pai2 = b.pai;
		return ComparePai(pai, pai2);
	}

	public static int ComparePai(Pai paiA, Pai paiB)
	{
		Pai pai = paiA;
		Pai pai2 = paiB;
		if (MahjongUtility.IsAkahai(paiA))
		{
			pai = MahjongUtility.AkaToNormal(paiA);
		}
		if (MahjongUtility.IsAkahai(paiB))
		{
			pai2 = MahjongUtility.AkaToNormal(paiB);
		}
		if (!MahjongUtility.IsAkahai(paiB) && paiA == MahjongUtility.NormalToAka(paiB))
		{
			return -1;
		}
		if (!MahjongUtility.IsAkahai(paiA) && paiB == MahjongUtility.NormalToAka(paiA))
		{
			return 1;
		}
		return pai - pai2;
	}

	public async UniTask ResetPos(bool animation = true)
	{
		int num = 0;
		bool finish = false;
		foreach (Tile item in Tiles)
		{
			if (item.m_tweener != null)
			{
				item.m_tweener.Kill(complete: true);
			}
			item.name = $"TileRoot{num + 1}";
			if (player.IsJicha())
			{
				item.SetKeyTex(num);
			}
			float num2 = tileManager.GetTilePrefab().transform.localScale.x * 0.2f * (float)num++;
			float y = 0f;
			float z = (item.m_isSelected ? 0.02f : 0f);
			if (m_nowTsumoChoice && item == Tiles.Last())
			{
				num2 += 0.01f;
			}
			if (animation)
			{
				item.m_tweener = item.transform.DOLocalMove(new Vector3(num2, y, z), 0.2f).SetEase(Ease.OutQuart).OnComplete(delegate
				{
					finish = true;
					item.m_tweener = null;
				});
			}
			else
			{
				finish = true;
				item.transform.localPosition = new Vector3(num2, y, z);
			}
		}
		await UniTask.WaitUntil(() => finish);
		onSwitchSelectedHai.OnNext(value: true);
	}

	public async UniTask Select(Tile item, SdChara sdChara, bool animation = true)
	{
		if (item == null)
		{
			return;
		}
		MahjongUtility.SetTouchedTile(item);
		if (!item.m_enableDiscard)
		{
			return;
		}
		Tile tile = Tiles.Find((Tile a) => a == item);
		if (player.m_isReach && (!m_nowTsumoChoice || tile != GetLastItem()))
		{
			return;
		}
		foreach (Tile tile2 in Tiles)
		{
			tile2.Selected(isSelected: false);
		}
		m_selectedTile = tile;
		tile.Selected(isSelected: true);
		onSwitchSelectedHai.OnNext(value: true);
		if (m_selectedTile.GetHaiJudgeMark() == Tile.HaiJudgeMark.Safety)
		{
			SoundManager.PlaySe("SFX_MJG_TOUCH3").Forget();
		}
		else
		{
			SoundManager.PlaySe("SFX_MJG_TOUCH1").Forget();
		}
		await ResetPos(animation);
		sdChara?.PickUpAnime(tile);
	}

	public void Select(Pai pai, SdChara sdChara)
	{
		Tile tile = Tiles.Find((Tile a) => a.pai == pai);
		if (tile == null)
		{
			tile = Tiles.Find((Tile a) => a.pai == MahjongUtility.NormalToAka(pai));
		}
		Select(tile, sdChara).Forget();
	}

	public async UniTask SelectReset(SdChara sdChara = null)
	{
		foreach (Tile tile in Tiles)
		{
			tile.Selected(isSelected: false);
		}
		m_selectedTile = null;
		MahjongUtility.SetTouchedTile(null);
		onSwitchSelectedHai.OnNext(value: false);
		await ResetPos(player.IsJicha());
		sdChara?.DispOffAnime();
	}

	public void SetDiscardTile(Tile item)
	{
		m_discardTile = item;
	}

	public Tile GetDiscardTile(bool isDaiuchi = false, List<ShantenInfo> infos = null, bool isReach = false, SdChara sdChara = null)
	{
		if (isDaiuchi)
		{
			Tile discardDaiuchiTile = GetDiscardDaiuchiTile(infos, isReach);
			Select(discardDaiuchiTile, sdChara).Forget();
			SetDiscardTile(discardDaiuchiTile);
		}
		return m_discardTile;
	}

	public Tile GetDiscardDaiuchiTile(List<ShantenInfo> infos, bool isReach)
	{
		if (isReach)
		{
			List<Tile> list = Tiles.FindAll((Tile x) => x.m_enableDiscard);
			if (list.Count > 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
		}
		try
		{
			bool flag = infos.Exists((ShantenInfo x) => x.KokushiHandShanten <= 3);
			bool flag2 = infos.Exists((ShantenInfo x) => x.ChitoiHandShanten <= 1);
			Dictionary<int, ShantenInfo> dictionary;
			if (flag)
			{
				var kokushiHand = from x in infos.Select((ShantenInfo info, int idx) => new
					{
						Index = idx,
						Content = info
					})
					orderby x.Content.KokushiHandShanten
					select x;
				dictionary = kokushiHand.Where(x => x.Content.KokushiHandShanten == kokushiHand.First().Content.KokushiHandShanten).ToDictionary(x => x.Index, y => y.Content);
			}
			else if (flag2)
			{
				var normalHandSort2 = from x in infos.Select((ShantenInfo info, int idx) => new
					{
						Index = idx,
						Content = info
					})
					where x.Content.ShantenDifference == ShantenDifference.Up
					orderby x.Content.NormalHandShanten
					select x;
				dictionary = normalHandSort2.Where(x => x.Content.NormalHandShanten == normalHandSort2.First().Content.NormalHandShanten).ToDictionary(x => x.Index, y => y.Content);
			}
			else
			{
				var normalHandSort = from x in infos.Select((ShantenInfo info, int idx) => new
					{
						Index = idx,
						Content = info
					})
					orderby x.Content.NormalHandShanten
					select x;
				dictionary = normalHandSort.Where(x => x.Content.NormalHandShanten == normalHandSort.First().Content.NormalHandShanten).ToDictionary(x => x.Index, y => y.Content);
			}
			bool flag3 = dictionary.Count((KeyValuePair<int, ShantenInfo> x) => x.Value.ShantenDifference == ShantenDifference.Up) > 0;
			if (!flag3 && flag2)
			{
				dictionary = (from x in infos.Select((ShantenInfo info, int idx) => new
					{
						Index = idx,
						Content = info
					})
					where x.Content.ShantenDifference == ShantenDifference.None
					where !MahjongUtility.IsDora(Tiles[x.Index].pai)
					select x).ToDictionary(x => x.Index, y => y.Content);
			}
			if (dictionary != null && dictionary.Count() > 0)
			{
				bool flag4 = flag3;
				if (!flag)
				{
					IEnumerable<KeyValuePair<int, ShantenInfo>> source = dictionary.Where((KeyValuePair<int, ShantenInfo> x) => Tiles[x.Key].pai.ToHaiID().IsYaochu() && GetHaiCounter()[Tiles[x.Key].pai.ToHaiID()] == 1);
					if (source.Count() > 0)
					{
						flag4 = true;
						dictionary = source.ToDictionary((KeyValuePair<int, ShantenInfo> x) => x.Key, (KeyValuePair<int, ShantenInfo> y) => y.Value);
					}
				}
				if (flag4 && dictionary.Count() > 0)
				{
					int index = UnityEngine.Random.Range(0, dictionary.Count());
					return Tiles[dictionary.ElementAt(index).Key];
				}
			}
		}
		catch
		{
			Debug.LogWarning("代打ちの選択に失敗しました");
		}
		return GetLastItem();
	}

	public Pai GetSelectedPai()
	{
		foreach (Tile tile in Tiles)
		{
			if (tile.m_isSelected)
			{
				return tile.pai;
			}
		}
		return Pai.NONE;
	}

	public Tile GetSelectedTile()
	{
		return Tiles.Find((Tile a) => a.m_isSelected);
	}

	public bool IsSelected(Tile item)
	{
		Tile tile = Tiles.Find((Tile a) => a == item);
		if ((bool)tile)
		{
			return tile.m_isSelected;
		}
		return false;
	}

	public void SetNowTsumoChoice(bool flag)
	{
		m_nowTsumoChoice = flag;
	}

	public bool IsNowTsumoChoice()
	{
		return m_nowTsumoChoice;
	}

	public void SetNowExecTsumoChoices(bool flag)
	{
		m_nowExecTsumoChoices = flag;
	}

	public bool IsNowExecTsumoChoices()
	{
		return m_nowExecTsumoChoices;
	}

	public void SetEnableDiscard(Pai pai, bool enable)
	{
		foreach (Tile tile in Tiles)
		{
			if (tile.pai == pai)
			{
				tile.m_enableDiscard = enable;
				if (enable)
				{
					tile.SetDark(set: false);
				}
				else
				{
					tile.SetDark(set: true);
				}
			}
		}
	}

	public void EnableDiscardInReach(List<ChoicesData.MachiPatternData> machiPatterns, List<Pai> openMachiPais, HaiNavi haiNavi, bool isAtomicSelected)
	{
		AllEnableDiscard(enable: false);
		foreach (ChoicesData.MachiPatternData machiPattern in machiPatterns)
		{
			Debug.Log(machiPattern.SutePaiID);
			if (isAtomicSelected)
			{
				Pai pai = MahjongUtility.NormalToAka((Pai)machiPattern.SutePaiID);
				int index = Tiles.FindIndex((Tile x) => MahjongUtility.NormalToAka(x.pai) == pai);
				if (machiPattern.MachiPaiNum < 3 && haiNavi.ShantenInfos[index].MachiType != MachiType.Ryanmen && haiNavi.ShantenInfos[index].MachiType != MachiType.Other)
				{
					continue;
				}
			}
			if (machiPattern.IsMachiPaiRemain() && !openMachiPais.Contains((Pai)machiPattern.SutePaiID))
			{
				SetEnableDiscard((Pai)machiPattern.SutePaiID, enable: true);
				if (MahjongUtility.IsIncludeAkahai((Pai)machiPattern.SutePaiID))
				{
					SetEnableDiscard(MahjongUtility.NormalToAka((Pai)machiPattern.SutePaiID), enable: true);
				}
			}
		}
	}

	public bool EnableDiscardInReachRelease(List<Pai> openMachiPais)
	{
		bool result = false;
		if (player.m_isReach)
		{
			return false;
		}
		if (openMachiPais != null && openMachiPais.Count > 0)
		{
			AllEnableDiscard(enable: true, isSelectContinuous: true);
			foreach (Pai openMachiPai in openMachiPais)
			{
				if (openMachiPai != 0)
				{
					SetEnableDiscard(openMachiPai, enable: false);
					if (MahjongUtility.IsIncludeAkahai(openMachiPai))
					{
						SetEnableDiscard(MahjongUtility.NormalToAka(openMachiPai), enable: false);
					}
				}
			}
			result = true;
		}
		if (GetTiles().All((Tile x) => !x.m_enableDiscard))
		{
			AllEnableDiscard(enable: true);
			result = true;
		}
		return result;
	}

	public EnableDiscardType EnableDiscardInReachButton(List<ChoicesData.MachiPatternData> machiPatterns, List<Pai> openMachiPais, HaiNavi hainavi)
	{
		bool flag = openMachiPais != null && openMachiPais.Count > 0;
		bool flag2 = !GameDataManager.Mahjong.IsOpenReachEnableMode;
		bool flag3 = false;
		int num = -1;
		foreach (Tile tile in Tiles)
		{
			num++;
			Debug.Log($"リーチボタンチェック {tile.pai}\u3000待ち {hainavi.ShantenInfos[num].MachiType}");
			if (!tile.m_enableDiscard)
			{
				continue;
			}
			Pai pai = MahjongUtility.AkaToNormal(tile.pai);
			ChoicesData.MachiPatternData machiPatternData = machiPatterns.Find((ChoicesData.MachiPatternData x) => MahjongUtility.AkaToNormal((Pai)x.SutePaiID) == pai);
			if (machiPatternData == null)
			{
				continue;
			}
			bool flag4 = machiPatternData.MachiPaiNum >= 3 || hainavi.ShantenInfos[num].MachiType == MachiType.Ryanmen || hainavi.ShantenInfos[num].MachiType == MachiType.Other;
			if (flag4)
			{
				flag3 = true;
			}
			if (!flag || !openMachiPais.Contains(pai))
			{
				if (flag4)
				{
					return EnableDiscardType.True;
				}
				flag2 = true;
			}
		}
		return (EnableDiscardType)((flag3 ? 1 : 0) + (flag2 ? 2 : 0));
	}

	public bool EnableDiscardKaratenInReachButton(List<ChoicesData.MachiPatternData> machiPatterns, HaiNavi hainavi)
	{
		int num = -1;
		foreach (Tile tile in Tiles)
		{
			num++;
			Debug.Log($"リーチボタンチェックKaratenCheck {tile.pai}\u3000待ち {hainavi.ShantenInfos[num].MachiType}");
			if (!tile.m_enableDiscard)
			{
				continue;
			}
			Pai pai = MahjongUtility.AkaToNormal(tile.pai);
			ChoicesData.MachiPatternData machiPatternData = machiPatterns.Find((ChoicesData.MachiPatternData x) => MahjongUtility.AkaToNormal((Pai)x.SutePaiID) == pai);
			if (machiPatternData == null || (machiPatternData.MachiPaiNum < 3 && hainavi.ShantenInfos[num].MachiType != MachiType.Ryanmen && hainavi.ShantenInfos[num].MachiType != MachiType.Other))
			{
				continue;
			}
			int[] machiPaiID = machiPatternData.MachiPaiID;
			for (int i = 0; i < machiPaiID.Length; i++)
			{
				if (MahjongUtility.GetRestPaiNum((Pai)machiPaiID[i]) > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AllEnableDiscard(bool enable, bool isSelectContinuous = false)
	{
		if (!isSelectContinuous)
		{
			foreach (Tile tile in Tiles)
			{
				tile.Selected(isSelected: false);
			}
			onSwitchSelectedHai.OnNext(value: false);
		}
		if (!enable)
		{
			ResetPos(animation: false).Forget();
		}
		foreach (Tile tile2 in Tiles)
		{
			tile2.m_enableDiscard = enable;
			if (enable)
			{
				tile2.SetDark(set: false);
			}
			else
			{
				tile2.SetDark(set: true);
			}
		}
	}

	public bool IsTsumogiri(int pos)
	{
		if (m_nowTsumoChoice)
		{
			return pos == Tiles.Count - 1;
		}
		return false;
	}

	public Counter GetHaiCounter(int ignoreIndex = -1)
	{
		Counter counter = new Counter();
		int num = 0;
		foreach (Tile tile in Tiles)
		{
			if (num != ignoreIndex)
			{
				HaiID haiID = tile.pai.ToHaiID();
				counter.Add(haiID);
			}
			num++;
		}
		return counter;
	}

	public HaiID[] GetHais()
	{
		HaiID[] array = new HaiID[Tiles.Count];
		int num = 0;
		foreach (Tile tile in Tiles)
		{
			array[num] = tile.pai.ToHaiID();
			num++;
		}
		return array;
	}

	public List<Pai> GetPais()
	{
		List<Pai> list = new List<Pai>();
		foreach (Tile tile in Tiles)
		{
			list.Add(tile.pai);
		}
		return list;
	}

	public int MeldCount()
	{
		return player.meld.MeldCount;
	}

	public void Destruct()
	{
		foreach (Tile tile in Tiles)
		{
			tileManager.ReturnTile(tile);
		}
		Tiles.Clear();
		m_nowTsumoChoice = false;
		m_selectedTile = null;
	}

	public void StartDoraEffect(bool isReachAgari = true)
	{
		MahjongUtility.DoraRange doraRange = MahjongUtility.DoraRange.All;
		if (!isReachAgari)
		{
			doraRange = MahjongUtility.DoraRange.Omote;
		}
		foreach (Transform item in base.transform)
		{
			if (item.GetComponent<Tile>() != null && MahjongUtility.IsDora(item.GetComponent<Tile>().pai, doraRange))
			{
				item.GetComponent<Tile>().StartDoraEffect();
			}
		}
	}

	public int GetDoraCount(int ignoreIndex = -1)
	{
		int num = 0;
		int num2 = 0;
		foreach (Tile tile in Tiles)
		{
			if (num2 != ignoreIndex)
			{
				num += MahjongUtility.GetPureDoraCount(tile.pai);
				if (MahjongUtility.IsAkahai(tile.pai))
				{
					num++;
				}
			}
			num2++;
		}
		return num;
	}

	public int GetOmoteDoraCount()
	{
		int num = 0;
		foreach (Tile tile in Tiles)
		{
			num += MahjongUtility.GetPureDoraCount(tile.pai, MahjongUtility.DoraRange.Omote);
		}
		return num;
	}

	public int GetUraDoraCount()
	{
		int num = 0;
		foreach (Tile tile in Tiles)
		{
			num += MahjongUtility.GetPureDoraCount(tile.pai, MahjongUtility.DoraRange.Ura);
		}
		return num;
	}

	public int GetAkaDoraCount()
	{
		int num = 0;
		foreach (Tile tile in Tiles)
		{
			if (MahjongUtility.IsAkahai(tile.pai))
			{
				num++;
			}
		}
		return num;
	}

	public void StartTenpaiEffect(bool isMangan, bool isRestart)
	{
		foreach (Transform item in base.transform)
		{
			Tile component = item.GetComponent<Tile>();
			if (component != null)
			{
				component.StartTenpaiEffect(isMangan, isRestart);
			}
		}
	}

	public void StopTenpaiEffect()
	{
		foreach (Transform item in base.transform)
		{
			Tile component = item.GetComponent<Tile>();
			if (component != null)
			{
				component.StopTenpaiEffect();
			}
		}
	}

	public void SetDark(bool set)
	{
		foreach (Tile tile in Tiles)
		{
			tile.SetDark(set);
		}
	}

	public void TenpaiTargetIconSet(HaiNavi hainavi)
	{
		for (int i = 0; i < Tiles.Count; i++)
		{
			if (hainavi.ShantenInfos[i].Shanten == 0 && Tiles[i].m_enableDiscard)
			{
				Tiles[i].IsTenpaiTarget = true;
			}
			else
			{
				Tiles[i].IsTenpaiTarget = false;
			}
		}
	}

	public void TenpaiTargetIconSetDisp(bool isDisp)
	{
		foreach (Tile tile in Tiles)
		{
			if (tile.IsTenpaiTarget && isDisp && m_isTenpaiTargetIconDispTiming)
			{
				tile.SetTenpaiTargetIcon(set: true);
			}
			else
			{
				tile.SetTenpaiTargetIcon(set: false);
			}
		}
	}

	public void OpenHoujuTargetIconSet(List<Pai> openMachiList)
	{
		if (openMachiList != null && openMachiList.Count > 0)
		{
			for (int i = 0; i < Tiles.Count; i++)
			{
				if (openMachiList.Contains(MahjongUtility.AkaToNormal(Tiles[i].pai)))
				{
					Tiles[i].IsOpenTarget = true;
				}
				else
				{
					Tiles[i].IsOpenTarget = false;
				}
			}
		}
		else
		{
			for (int j = 0; j < Tiles.Count; j++)
			{
				Tiles[j].IsOpenTarget = false;
			}
		}
	}

	public void OpenHoujuTargetIconSetDisp(bool isDisp)
	{
		foreach (Tile tile in Tiles)
		{
			if (tile.IsOpenTarget && isDisp && m_isOpenTargetIconDispTiming)
			{
				tile.SetOpenTargetIcon(set: true);
				tile.SetTenpaiTargetIcon(set: false);
			}
			else
			{
				tile.SetOpenTargetIcon(set: false);
			}
		}
	}

	public List<Tile> GetTiles()
	{
		return Tiles;
	}

	public int GetTilesCount()
	{
		return Tiles.Count();
	}

	public void DispOffHaiJudgeMark()
	{
		foreach (Tile tile in Tiles)
		{
			tile.SetHaiJudgeMark(Tile.HaiJudgeMark.None);
			tile.SetAllGreenMark(set: false);
		}
	}

	public void AllKeyboardTextControll(bool isActive)
	{
		if (!DebugFlag.IsActive(DebugFlag.Flag.KeyboardEnable) && !GameUtility.IsCloudStreaming)
		{
			return;
		}
		foreach (Tile tile in Tiles)
		{
			tile.KeyTexActiveControll(isActive && player.IsJicha());
		}
	}
}

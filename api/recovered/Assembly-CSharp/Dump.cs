using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.MahjongPlay.Character.Arm;
using MFG.Types;
using UnityEngine;

public class Dump : MonoBehaviour
{
	[SerializeField]
	private Player player;

	[SerializeField]
	private GameObject putEffect;

	[SerializeField]
	private GameObject putEffectDora;

	[SerializeField]
	private GameObject nakihaiTargetEffect;

	private TileManager tileManager;

	private List<Tile> Tiles = new List<Tile>();

	public Tile _lastDumpedTile;

	private int _tsumogiriContinueCount;

	private float posx;

	private float posz;

	public List<Tile> TileList => Tiles;

	private void Start()
	{
		tileManager = GameObject.Find("TileManager").GetComponent<TileManager>();
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.tag == "Debug")
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	public async UniTask Add(Tile item, bool isReach, bool isTsumogiri, bool isAnpai, bool isKikenhai)
	{
		_lastDumpedTile = item;
		if (isTsumogiri)
		{
			_tsumogiriContinueCount++;
		}
		else
		{
			_tsumogiriContinueCount = 0;
		}
		item.Dump();
		int count = Tiles.Count;
		if (item.m_tweener != null)
		{
			item.m_tweener.Kill(complete: true);
			item.m_tweener = null;
		}
		item.transform.SetParent(player.dump.transform, worldPositionStays: false);
		item.m_isSide = isReach;
		if (player.m_isReach && Tiles.Find((Tile a) => a.m_isSide) == null)
		{
			item.m_isSide = true;
		}
		item.gameObject.SetActive(value: false);
		await UniTask.Delay(TimeSpan.FromSeconds(TileManager.GetInstance().dahaiAnimeDelay));
		SetPosition(count, item);
		if (MahjongUtility.GetViewType() == ViewType.Type3)
		{
			player.character.Action_PlayArmMotion(CharacterArmController.MotionID.Away, item.transform.position);
		}
		item.gameObject.SetActive(value: true);
		if (item.m_isSide)
		{
			item.transform.localRotation = Quaternion.identity;
			float z = item.transform.localPosition.z;
			item.transform.Translate(new Vector3(0f, 0f, 0f - TileManager.GetInstance().dahaiAnimeDistance));
			item.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
			item.IsMoving = true;
			item.m_tweener = item.transform.DOLocalMoveZ(z, TileManager.GetInstance().dahaiAnimeTime).SetEase(Ease.OutCubic).OnComplete(delegate
			{
				PutEffectStart(item);
				DiscardSoundCall(item.pai, isTsumogiri, isAnpai, isKikenhai, isReach);
				item.IsMoving = false;
				item.m_tweener = null;
			});
		}
		else
		{
			float z2 = item.transform.localPosition.z;
			item.transform.Translate(new Vector3(0f, 0f, 0f - TileManager.GetInstance().dahaiAnimeDistance));
			item.IsMoving = true;
			item.m_tweener = item.transform.DOLocalMoveZ(z2, TileManager.GetInstance().dahaiAnimeTime).SetEase(Ease.OutCubic).OnComplete(delegate
			{
				PutEffectStart(item);
				DiscardSoundCall(item.pai, isTsumogiri, isAnpai, isKikenhai, isReach);
				item.IsMoving = false;
				item.m_tweener = null;
			});
		}
		if (isTsumogiri)
		{
			item.SetTsumogiriMark(set: true, item.m_isSide);
		}
		if (GameDataManager.PlayerConfig.IsTaikyokuSupport)
		{
			if (isAnpai)
			{
				item.SetAnpaiMark(set: true);
			}
			if (isKikenhai)
			{
				item.SetKikenhaiMark(set: true);
			}
		}
		Tiles.Add(item);
		item.SetOpen();
		MahjongUtility.SetLastTargetTile(item);
		item.StartBlink();
		if (TileManager.GetInstance().dahaiAnimeIsWaitFinish)
		{
			await UniTask.WaitWhile(() => item.IsMoving);
		}
		await UniTask.Delay(TimeSpan.FromSeconds(TileManager.GetInstance().dahaiAnimeWait));
	}

	private void DiscardSoundCall(Pai pai, bool isTsumogiri, bool isAnpai, bool isKikenhai, bool isReachThisTurn)
	{
		bool flag = MahjongUtility.IsDora(pai);
		bool flag2 = false;
		if (pai == Pai.J7 && VoiceManager.IsChunBeamEnable())
		{
			flag2 = true;
		}
		if (flag2)
		{
			SoundManager.PlaySeCha("SFX_MJG_DISCARD3", player.m_cha).Forget();
		}
		else if (flag)
		{
			SoundManager.PlaySeCha("SFX_MJG_DISCARD2", player.m_cha).Forget();
		}
		else
		{
			SoundManager.PlaySeCha("SFX_MJG_DISCARD1", player.m_cha).Forget();
		}
		if (SoundManager.GetLastCalledMyVoiceLabel().Contains("_TALK_") && SoundManager.IsPlayLastCalledVoice())
		{
			return;
		}
		if (flag2)
		{
			VoiceManager.PlayVoiceChun(player);
		}
		else if (player.m_charaType == CharaType.Chara18 && player.m_isIppatsuVoice && !isReachThisTurn)
		{
			Debug.Log("<color=yellow>イッパツならずボイス</color>");
			VoiceManager.PlayVoiceDahai9(player);
		}
		else if (player.character.m_charaReaction.m_isReceivedOkkakeReach)
		{
			VoiceManager.PlayVoiceComment7(player);
		}
		else if (flag && !isReachThisTurn)
		{
			VoiceManager.PlayVoiceDahai7(player);
		}
		else if (isKikenhai && !isReachThisTurn)
		{
			if (player.m_isReach)
			{
				VoiceManager.PlayVoiceDahai8(player);
			}
			else
			{
				VoiceManager.PlayVoiceDahai3(player);
			}
		}
		else if (isAnpai && !player.m_isReach)
		{
			VoiceManager.PlayVoiceDahai2(player);
		}
		else if (isTsumogiri && !player.m_isReach)
		{
			if (_tsumogiriContinueCount <= 2)
			{
				VoiceManager.PlayVoiceDahai4(player);
			}
			else if (_tsumogiriContinueCount <= 4)
			{
				VoiceManager.PlayVoiceDahai5(player);
			}
			else
			{
				VoiceManager.PlayVoiceDahai6(player);
			}
		}
		else if (!player.m_isReach)
		{
			VoiceManager.PlayVoiceDahai1(player);
		}
	}

	private void PutEffectStart(Tile item)
	{
		if (MahjongUtility.IsDora(item.pai))
		{
			putEffectDora.transform.position = item.transform.position;
			putEffectDora.transform.Find("Hit").GetComponent<ParticleSystem>().Play();
			putEffectDora.transform.Find("Hit").Find("Particle").GetComponent<ParticleSystem>()
				.Play();
			putEffectDora.transform.Find("Hit").Find("Flash").GetComponent<ParticleSystem>()
				.Play();
			item.StartDoraEffectOnce();
		}
		else
		{
			putEffect.transform.position = item.transform.position;
			putEffect.transform.Find("Particle System").GetComponent<ParticleSystem>().Play();
			putEffect.transform.Find("Contour").GetComponent<ParticleSystem>().Play();
		}
	}

	public void Destruct()
	{
		foreach (Tile tile in Tiles)
		{
			tileManager.ReturnTile(tile);
		}
		Tiles.Clear();
		_lastDumpedTile = null;
		_tsumogiriContinueCount = 0;
		posx = 0f;
		posz = 0f;
	}

	public Tile PlayNakiHaiTargetEffect(Cha cha)
	{
		Tile lastTile = GetLastTile();
		if (lastTile != null)
		{
			nakihaiTargetEffect.transform.position = lastTile.transform.position;
			nakihaiTargetEffect.transform.Find("Line1").GetComponent<ParticleSystem>().Play();
			nakihaiTargetEffect.transform.Find("Line2").GetComponent<ParticleSystem>().Play();
			nakihaiTargetEffect.transform.Find("Particles").GetComponent<ParticleSystem>().Play();
			nakihaiTargetEffect.transform.Find("Tile").GetComponent<ParticleSystem>().Play();
			nakihaiTargetEffect.transform.Find("Circle").GetComponent<ParticleSystem>().Play();
			SoundManager.PlaySeCha("SFX_MJG_TILE3", cha).Forget();
			Tiles.Remove(lastTile);
			lastTile.EndBlink();
			Reflesh();
			lastTile.gameObject.SetActive(value: false);
		}
		return lastTile;
	}

	public Tile GetLastTile()
	{
		if (Tiles.Count > 0)
		{
			return Tiles.Last();
		}
		return null;
	}

	public Tile GetLastBeforeTile()
	{
		if (Tiles.Count > 1)
		{
			return Tiles[Tiles.Count - 2];
		}
		return null;
	}

	public void SetDark(bool set)
	{
		foreach (Tile tile in Tiles)
		{
			tile.SetDark(set);
		}
	}

	private void SetPosition(int count, Tile item)
	{
		int num = 6;
		int num2 = 3;
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima)
		{
			if (MahjongUtility.GetViewType() == ViewType.Type1 || MahjongUtility.GetViewType() == ViewType.Type3)
			{
				num = 8;
				num2 = 3;
			}
			else
			{
				num = 11;
				num2 = 2;
			}
		}
		else if (MahjongUtility.GetViewType() == ViewType.Type1 || MahjongUtility.GetViewType() == ViewType.Type3)
		{
			num = 6;
			num2 = 3;
		}
		else
		{
			num = 9;
			num2 = 2;
		}
		if (count % num == 0 && count < num * num2)
		{
			posx = 0f;
			posz -= ((count == 0) ? 0f : (item.transform.localScale.z * 0.26f));
		}
		if (item.m_isSide)
		{
			float x = posx + item.transform.localScale.z * 0.059999987f / 2f;
			float z = posz - item.transform.localScale.z * 0.059999987f / 2f;
			item.transform.localPosition = new Vector3(x, 0f, z);
			posx += item.transform.localScale.x * 0.26f;
			item.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
		}
		else
		{
			item.transform.localPosition = new Vector3(posx, 0f, posz);
			posx += item.transform.localScale.x * 0.2f;
			item.transform.localRotation = Quaternion.identity;
		}
	}

	public void Reflesh()
	{
		posx = 0f;
		posz = 0f;
		for (int i = 0; i < Tiles.Count; i++)
		{
			if (Tiles[i].m_tweener != null)
			{
				Tiles[i].m_tweener.Kill(complete: true);
			}
			SetPosition(i, Tiles[i]);
		}
	}

	public void DouhaiSearchReflesh()
	{
		foreach (Tile tile in Tiles)
		{
			tile.DouhaiSearchReflesh();
		}
	}

	public void SetDebugSutePai()
	{
	}
}

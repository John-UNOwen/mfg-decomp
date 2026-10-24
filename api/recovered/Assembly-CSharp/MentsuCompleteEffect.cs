using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MahjonPlay.Hand;
using MFG.Types;
using UnityEngine;

public class MentsuCompleteEffect : MonoBehaviour
{
	private class MentsuTileInfo
	{
		public Tile TileTop;

		public int Count;

		public static List<MentsuTileInfo> Create(Player player, BlockType blockType, Tile topTile)
		{
			List<MentsuTileInfo> list = new List<MentsuTileInfo>();
			Tile tile = null;
			List<int> list2 = null;
			switch (blockType)
			{
			case BlockType.AnKoutsu:
				list2 = player.hand.GetPaiPosAll(topTile.pai.ToNormal());
				if (list2[0] >= 0)
				{
					for (int j = 0; j < 3; j++)
					{
						tile = player.hand.GetTile(list2[j]);
						list.Add(new MentsuTileInfo
						{
							TileTop = tile,
							Count = 1
						});
					}
				}
				break;
			case BlockType.AnShuntsu:
			{
				for (int i = 0; i < 3; i++)
				{
					Pai pai = topTile.pai + i;
					list2 = player.hand.GetPaiPosAll(pai.ToNormal());
					if (list2[0] >= 0)
					{
						tile = player.hand.GetTile(list2[0]);
						list.Add(new MentsuTileInfo
						{
							TileTop = tile,
							Count = list2.Count
						});
					}
				}
				break;
			}
			}
			return list;
		}
	}

	[SerializeField]
	private Canvas m_canvas;

	[SerializeField]
	private GameObject m_shuntsuRoot;

	[SerializeField]
	private GameObject m_kotsuRoot;

	[SerializeField]
	private Camera m_camera;

	[SerializeField]
	private List<GameObject> m_originalKotsuObject;

	[SerializeField]
	private List<GameObject> m_originalShuntsuObject;

	[SerializeField]
	private GameObject m_kotsuText;

	[SerializeField]
	private GameObject m_shuntsuText;

	private List<GameObject> m_kotsuEffects = new List<GameObject>();

	private List<List<GameObject>> m_shuntsuEffectLists = new List<List<GameObject>>();

	private bool m_isDisp;

	[SerializeField]
	private int m_debugShuntsuCount;

	[SerializeField]
	private int m_animationWaitTime;

	public bool IsPlaying { get; private set; }

	public void Awake()
	{
		m_kotsuEffects.Clear();
		m_shuntsuEffectLists.Clear();
		for (int i = 0; i < 4; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_originalKotsuObject[0], m_kotsuRoot.transform);
			gameObject.SetActive(value: false);
			m_kotsuEffects.Add(gameObject);
			List<GameObject> list = new List<GameObject>();
			gameObject = UnityEngine.Object.Instantiate(m_originalShuntsuObject[0], m_shuntsuRoot.transform);
			gameObject.SetActive(value: false);
			list.Add(gameObject);
			gameObject = UnityEngine.Object.Instantiate(m_originalShuntsuObject[1], m_shuntsuRoot.transform);
			gameObject.SetActive(value: false);
			list.Add(gameObject);
			gameObject = UnityEngine.Object.Instantiate(m_originalShuntsuObject[2], m_shuntsuRoot.transform);
			gameObject.SetActive(value: false);
			list.Add(gameObject);
			gameObject = UnityEngine.Object.Instantiate(m_originalShuntsuObject[3], m_shuntsuRoot.transform);
			gameObject.SetActive(value: false);
			list.Add(gameObject);
			m_shuntsuEffectLists.Add(list);
		}
	}

	public void SetDisp(bool isDisp)
	{
		m_isDisp = isDisp;
	}

	public async UniTask PlayEffect(Player player, List<Block> blocks)
	{
		if (!m_isDisp || blocks.Count <= 0)
		{
			return;
		}
		IsPlaying = true;
		foreach (Block block in blocks)
		{
			Tile tile = null;
			BlockType blockType = block.BlockType;
			if ((uint)(blockType - 5) <= 1u)
			{
				int num = player.hand.GetPaiPosAll(block.HaiID.HaiIDToPai())[0];
				if (num >= 0)
				{
					tile = player.hand.GetTile(num);
				}
			}
			if (tile != null)
			{
				await PlayEffect(block.BlockType, MentsuTileInfo.Create(player, block.BlockType, tile));
			}
		}
		IsPlaying = false;
	}

	public async UniTask PlayEffect(Player player, BlockType blockType, Tile tile)
	{
		if (GameDataManager.PlayerConfig.IsTaikyokuSupport)
		{
			await PlayEffect(blockType, MentsuTileInfo.Create(player, blockType, tile));
		}
	}

	[Conditional("DEBUG")]
	public void DebugPlayEffect(Player player, BlockType blockType)
	{
		List<MentsuTileInfo> list = new List<MentsuTileInfo>();
		for (int i = 0; i < 3; i++)
		{
			Tile tile = null;
			switch (blockType)
			{
			case BlockType.AnKoutsu:
				tile = player.hand.GetTile(i);
				player.hand.GetPaiPosAll(tile.pai.ToNormal());
				list.Add(new MentsuTileInfo
				{
					TileTop = tile,
					Count = 1
				});
				break;
			case BlockType.AnShuntsu:
			{
				int debugShuntsuCount = m_debugShuntsuCount;
				tile = player.hand.GetTile(i * debugShuntsuCount);
				player.hand.GetPaiPosAll(tile.pai.ToNormal());
				list.Add(new MentsuTileInfo
				{
					TileTop = tile,
					Count = debugShuntsuCount
				});
				break;
			}
			}
		}
		PlayEffect(blockType, list).Forget();
	}

	private async UniTask PlayEffect(BlockType blockType, List<MentsuTileInfo> mentsuTileInfos)
	{
		try
		{
			if (mentsuTileInfos.Count < 3)
			{
				return;
			}
			GameObject gameObject = null;
			Tile tile = null;
			int num = 0;
			List<GameObject> effects = new List<GameObject>();
			for (int i = 0; i < 3; i++)
			{
				MentsuTileInfo mentsuTileInfo = mentsuTileInfos[i];
				switch (blockType)
				{
				case BlockType.AnKoutsu:
					gameObject = m_kotsuEffects[i];
					break;
				case BlockType.AnShuntsu:
					gameObject = m_shuntsuEffectLists[i][mentsuTileInfo.Count - 1];
					break;
				}
				if (tile == null)
				{
					tile = mentsuTileInfo.TileTop;
				}
				num += mentsuTileInfo.Count;
				SetPosition(mentsuTileInfo.TileTop, gameObject, mentsuTileInfo.Count);
				if (gameObject != null)
				{
					gameObject.SetActive(value: true);
					gameObject.GetComponent<Animator>().SetTrigger("In");
					effects.Add(gameObject);
				}
			}
			GameObject text = ((blockType == BlockType.AnKoutsu) ? m_kotsuText : m_shuntsuText);
			SetPosition(tile, text, 1);
			Vector2 anchoredPosition = text.GetComponent<RectTransform>().anchoredPosition;
			anchoredPosition.x += (float)(100 * num) / 2f - 50f;
			text.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			if (text.activeSelf)
			{
				text.GetComponent<Animator>().SetTrigger("In");
			}
			text.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO2").Forget();
			CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
			await UniTask.WhenAny(MahjongUtility.DelayIgnoreHighSpeedMode(m_animationWaitTime, cancellationTokenOnDestroy), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationTokenOnDestroy));
			effects.ForEach(delegate(GameObject x)
			{
				x?.SetActive(value: false);
			});
			text.SetActive(value: false);
		}
		catch (OperationCanceledException ex)
		{
			UnityEngine.Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	private void SetPosition(Tile item, GameObject effect, int itemCount)
	{
		if (!(item == null) && !(effect == null))
		{
			RectTransform component = m_canvas.GetComponent<RectTransform>();
			Vector3 vector = m_camera.WorldToViewportPoint(item.transform.position);
			Vector2 anchoredPosition = new Vector2(vector.x * component.sizeDelta.x - component.sizeDelta.x * 0.5f, vector.y * component.sizeDelta.y - component.sizeDelta.y * 0.5f);
			anchoredPosition.x += 50 * (itemCount - 1);
			anchoredPosition.y -= 13f;
			effect.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
		}
	}
}

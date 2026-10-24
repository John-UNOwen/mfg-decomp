using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MahjonPlay.Hand;
using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

public class YakuChanceEffect : MonoBehaviour
{
	[SerializeField]
	private Player m_player;

	[SerializeField]
	private Animator m_yakuChanceAnimator;

	[SerializeField]
	private GameObject m_yakuName;

	[SerializeField]
	private List<Sprite> m_yakuNameSprites = new List<Sprite>();

	[SerializeField]
	private List<GameObject> m_rarityIcons = new List<GameObject>();

	public async UniTask PlayEffect(YakuPossibilityInfo info)
	{
		m_yakuName.GetComponent<Image>().sprite = GetYakuNameSprite(info.Yaku);
		int hanNum = MahjongUtility.GetHanNum(info.Yaku, m_player.m_isFireReach, m_player.m_isOpenReach, m_player.m_isAtomicReach);
		for (int i = 0; i < m_rarityIcons.Count; i++)
		{
			GameObject obj = m_rarityIcons[i];
			bool active = i < hanNum;
			obj.SetActive(active);
		}
		PlayChanceHai(info);
		m_yakuChanceAnimator.gameObject.SetActive(value: true);
		m_yakuChanceAnimator.SetTrigger("Next");
		SoundManager.PlaySe("SFX_MJG_DISPLAY4").Forget();
		CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
		await UniTask.WhenAny(MahjongUtility.DelayIgnoreHighSpeedMode(2000, cancellationTokenOnDestroy), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationTokenOnDestroy));
		m_yakuChanceAnimator.gameObject.SetActive(value: false);
	}

	public void PlayChanceHai(YakuPossibilityInfo info)
	{
		BlockSet blocks = info.Blocks;
		ushort requiredBlockBitFlag = info.RequiredBlockBitFlag;
		int itemCount = m_player.hand.GetItemCount();
		int num = 0;
		int usedIndexFlag = 0;
		for (int i = 0; i < blocks.Count(); i++)
		{
			Block block = blocks[i];
			if ((requiredBlockBitFlag & (ushort)(1 << i)) <= 0)
			{
				continue;
			}
			if (block.IsMelded())
			{
				m_player.meld.StartYakuChanceEffect(block, ref usedIndexFlag);
				continue;
			}
			foreach (HaiID item in block.ToHaiID())
			{
				for (int j = 0; j < itemCount; j++)
				{
					if ((num & (ushort)(1 << j)) <= 0)
					{
						Tile tile = m_player.hand.GetTile(j);
						HaiID haiID = tile.pai.ToHaiID();
						if (item == haiID)
						{
							tile.StartYakuChanceEffect();
							num |= (ushort)(1 << j);
							break;
						}
					}
				}
			}
		}
	}

	private Sprite GetYakuNameSprite(Yaku yaku)
	{
		if (m_player.m_isFireReach)
		{
			if (GameDataManager.Mahjong.IsAotenjoMode)
			{
				switch (yaku)
				{
				case Yaku.Richi:
					return m_yakuNameSprites[71];
				case Yaku.DoubleRichi:
					return m_yakuNameSprites[72];
				case Yaku.Tanyao:
					return m_yakuNameSprites[66];
				case Yaku.Pinfu:
					return m_yakuNameSprites[67];
				}
			}
			switch (yaku)
			{
			case Yaku.Richi:
				return m_yakuNameSprites[61];
			case Yaku.DoubleRichi:
				return m_yakuNameSprites[62];
			}
		}
		else if (m_player.m_isAtomicReach)
		{
			if (GameDataManager.Mahjong.IsAotenjoMode)
			{
				switch (yaku)
				{
				case Yaku.Richi:
					return m_yakuNameSprites[73];
				case Yaku.DoubleRichi:
					return m_yakuNameSprites[74];
				case Yaku.Tanyao:
					return m_yakuNameSprites[66];
				case Yaku.Pinfu:
					return m_yakuNameSprites[67];
				}
			}
			switch (yaku)
			{
			case Yaku.Richi:
				return m_yakuNameSprites[69];
			case Yaku.DoubleRichi:
				return m_yakuNameSprites[70];
			}
		}
		else if (m_player.m_isOpenReach)
		{
			if (GameDataManager.Mahjong.IsAotenjoMode)
			{
				switch (yaku)
				{
				case Yaku.Richi:
					return m_yakuNameSprites[69];
				case Yaku.DoubleRichi:
					return m_yakuNameSprites[70];
				case Yaku.Tanyao:
					return m_yakuNameSprites[66];
				case Yaku.Pinfu:
					return m_yakuNameSprites[67];
				}
			}
			switch (yaku)
			{
			case Yaku.Richi:
				return m_yakuNameSprites[64];
			case Yaku.DoubleRichi:
				return m_yakuNameSprites[65];
			}
		}
		else if (GameDataManager.Mahjong.IsMentanpinMode)
		{
			switch (yaku)
			{
			case Yaku.Richi:
				return m_yakuNameSprites[64];
			case Yaku.DoubleRichi:
				return m_yakuNameSprites[65];
			case Yaku.Tanyao:
				return m_yakuNameSprites[66];
			case Yaku.Pinfu:
				return m_yakuNameSprites[67];
			}
		}
		return m_yakuNameSprites[(int)yaku];
	}
}

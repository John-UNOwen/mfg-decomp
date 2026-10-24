using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.MatchingBackground;
using UnityEngine;
using UnityEngine.UI;

public class MatchingBGPanel : MonoBehaviour, IDisposable
{
	[SerializeField]
	private Image m_actImage;

	[SerializeField]
	private Image m_typeImage;

	[SerializeField]
	private Image m_rareImage;

	[SerializeField]
	private Image m_stateImage;

	[SerializeField]
	private List<Sprite> m_typeSprite = new List<Sprite>();

	[SerializeField]
	private List<Sprite> m_rareSprite = new List<Sprite>();

	[SerializeField]
	private Sprite m_stateOnSprite;

	[SerializeField]
	private Sprite m_stateOffSprite;

	[SerializeField]
	private Sprite m_overSprite;

	[SerializeField]
	private Sprite m_noneSprite;

	public ItemID m_itemID;

	public bool m_isOn;

	public MatchingBGInfo m_info;

	private bool m_isRandomOrNone;

	private IDisposableAsset<Sprite> m_iconSprite;

	public void Dispose()
	{
		if (m_iconSprite != null)
		{
			m_iconSprite.Dispose();
			m_iconSprite = null;
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public async UniTask Reflesh(MatchingBGInfo info, bool isDispState = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_info = info;
		await RefleshCardGraphic(info.ItemID, cancellationToken);
		m_typeImage.gameObject.SetActive(value: false);
		if (m_rareSprite[(int)info.Master.Rarity] == null)
		{
			m_rareImage.gameObject.SetActive(value: false);
		}
		else
		{
			m_rareImage.gameObject.SetActive(value: true);
			m_rareImage.sprite = m_rareSprite[(int)info.Master.Rarity];
		}
		m_isOn = info.IsOn;
		if (isDispState)
		{
			m_stateImage.gameObject.SetActive(value: true);
			if (info.IsOn)
			{
				m_stateImage.sprite = m_stateOnSprite;
			}
			else
			{
				m_stateImage.sprite = m_stateOffSprite;
			}
		}
		else
		{
			m_stateImage.gameObject.SetActive(value: false);
		}
	}

	public void RefleshOnOff()
	{
		m_isOn = m_info.IsOn;
		m_stateImage.gameObject.SetActive(value: true);
		if (m_info.IsOn)
		{
			m_stateImage.sprite = m_stateOnSprite;
		}
		else
		{
			m_stateImage.sprite = m_stateOffSprite;
		}
	}

	private async UniTask RefleshCardGraphic(ItemID itemID, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (m_itemID != itemID || m_isRandomOrNone)
		{
			m_isRandomOrNone = false;
			GetComponent<CanvasGroup>().DOFade(0f, 0f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken).Forget();
			Sprite sprite = await MatchingBackgroundMaster.GetItem(itemID).IconGraphic(cancellationToken);
			m_actImage.sprite = sprite;
			m_itemID = itemID;
			GetComponent<CanvasGroup>().DOFade(1f, 0.2f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken).Forget();
		}
	}

	public void SetOverSprite()
	{
		m_isRandomOrNone = true;
		m_actImage.sprite = m_overSprite;
		m_typeImage.gameObject.SetActive(value: false);
		m_rareImage.gameObject.SetActive(value: false);
		m_stateImage.gameObject.SetActive(value: false);
	}

	public void SetNoneSprite()
	{
		m_isRandomOrNone = true;
		m_actImage.sprite = m_noneSprite;
		m_typeImage.gameObject.SetActive(value: false);
		m_rareImage.gameObject.SetActive(value: false);
		m_stateImage.gameObject.SetActive(value: false);
	}
}

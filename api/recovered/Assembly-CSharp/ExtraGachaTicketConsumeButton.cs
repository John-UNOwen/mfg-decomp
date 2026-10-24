using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.PlayerPaidItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExtraGachaTicketConsumeButton : MonoBehaviour
{
	[Serializable]
	public class TicketIconResolver
	{
		public ItemID ItemID;

		public Sprite Icon;
	}

	[SerializeField]
	private Button m_button;

	[SerializeField]
	private Button m_ticketChangebutton;

	[SerializeField]
	private TextMeshProUGUI m_consumeText;

	[SerializeField]
	private TextMeshProUGUI m_itemCaution;

	[SerializeField]
	private TextMeshProUGUI m_itemCount;

	[SerializeField]
	private Image m_ticketImage;

	[SerializeField]
	private List<TicketIconResolver> m_ticketIcons;

	private int m_value;

	private GAME_MODE m_mode;

	private int m_num;

	private UnityAction m_onClick;

	private UnityAction<bool, ConsumeWindow.EndType> m_onEnd;

	private bool m_isDecide;

	private bool m_isRenewAccessDate;

	private GachaSeriesName m_gachaSeries = (GachaSeriesName)(-1);

	private int m_ticketIndex;

	public readonly int NeedExtraGachaTicket = 10;

	private List<ItemID> m_usableGachaTickets
	{
		get
		{
			if (!DebugFlag.IsActive(DebugFlag.Flag.AllGachaTicketEnable))
			{
				return GachaSeriesServerInfo.GachaExtraMasters.Find((GachaExtraMasterData x) => x.GachaSeriesName == m_gachaSeries)?.EnableTickets;
			}
			return PlayerPaidItemMaster.GetItems(Category.GachaTicket);
		}
	}

	private List<PlayerPaidItemInfo> m_enableTickets => GameDataManager.PlayerPaidItem?.Items?.Where(delegate(PlayerPaidItemInfo x)
	{
		List<ItemID> usableGachaTickets = m_usableGachaTickets;
		return usableGachaTickets != null && usableGachaTickets.Contains(x.ItemID) && x.Count() >= NeedExtraGachaTicket;
	}).ToList();

	private ItemID m_selectingTicket => m_enableTickets?[m_ticketIndex].ItemID ?? ItemID.None;

	public void SetInteractable(bool interactable)
	{
		m_button.interactable = interactable;
	}

	public void Set(GAME_MODE mode, GachaSeriesName gachaSeries, UnityAction onClick, UnityAction<bool, ConsumeWindow.EndType> onEnd, bool isRenewAccessDate)
	{
		m_num = ((mode == GAME_MODE.GACHA) ? 1 : 5);
		m_mode = mode;
		m_value = NeedExtraGachaTicket * m_num;
		m_gachaSeries = gachaSeries;
		SetTicket();
		m_onClick = onClick;
		m_onEnd = onEnd;
		m_button.onClick.AddListener(OnClick);
		m_isRenewAccessDate = isRenewAccessDate;
		m_ticketChangebutton.OnClickAsObservable().Subscribe(delegate
		{
			OnTicketClick();
		}).AddTo(this);
	}

	private void Update()
	{
	}

	private void SetTicket()
	{
		if (m_mode != GAME_MODE.GACHA && m_mode != GAME_MODE.GACHA5)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		if (m_gachaSeries < GachaSeriesName.Normal || m_enableTickets == null || m_enableTickets.Count() <= 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		if (m_enableTickets.Count() == 1)
		{
			m_ticketChangebutton.gameObject.SetActive(value: false);
			base.gameObject.SetActive(value: true);
		}
		else
		{
			m_ticketChangebutton.gameObject.SetActive(value: true);
			base.gameObject.SetActive(value: true);
		}
		RefreshButton();
	}

	private void RefreshButton()
	{
		if (m_selectingTicket == ItemID.None)
		{
			return;
		}
		Item item = PlayerPaidItemMaster.GetItem(m_selectingTicket);
		int num = GameDataManager.PlayerPaidItem.Count(m_selectingTicket);
		DateTimeOffset dateTimeOffset = GameDataManager.PlayerPaidItem.NearestExpire(m_selectingTicket);
		m_button.interactable = num >= m_value;
		m_consumeText.SetText("{0}枚", m_value);
		if (m_ticketIcons?.Find((TicketIconResolver x) => x.ItemID == m_selectingTicket)?.Icon != null)
		{
			m_ticketImage.sprite = m_ticketIcons?.Find((TicketIconResolver x) => x.ItemID == m_selectingTicket)?.Icon;
		}
		m_itemCount.SetText($"{item.Name} <size=120%>{num}</size> 枚所持");
		m_itemCaution.SetText(dateTimeOffset.ToString("yyyy年MM月dd日 HH：mm") + "が使用期限のチケットがあります。");
		if (dateTimeOffset == default(DateTimeOffset))
		{
			m_itemCaution.gameObject.SetActive(value: false);
		}
	}

	private void OnTicketClick()
	{
		if (!m_isDecide)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_ticketIndex = ((m_ticketIndex + 1) % m_enableTickets?.Count()).GetValueOrDefault();
			SetTicket();
		}
	}

	private void OnClick()
	{
		if (!m_isDecide)
		{
			m_isDecide = true;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_onClick?.Invoke();
			StartCoroutine(CoConsume());
		}
	}

	private IEnumerator CoConsume()
	{
		if (GameUtility.IsKamunityBootEnable() && m_isRenewAccessDate)
		{
			XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse> responseCardMngInquire = new XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse>();
			XrpcModuleCardmng.XrpcMethodInquire.Request(null, GameDataManager.Account.CardType, GameDataManager.Account.CardID, update: true, responseCardMngInquire.Callback, null);
			yield return responseCardMngInquire.CoWaitForXrpcFinish();
			if (responseCardMngInquire.isError)
			{
				m_onEnd?.Invoke(arg0: false, ConsumeWindow.EndType.NetworkError);
				yield break;
			}
		}
		GameDataManager.PlayerPaidItem.Consume(m_selectingTicket, m_value, ItemConsumeType.GachaTicket).Forget();
		m_onEnd?.Invoke(arg0: true, ConsumeWindow.EndType.Success);
	}
}

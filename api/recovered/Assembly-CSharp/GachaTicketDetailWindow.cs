using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.PlayerItem;
using MFG.Types.PlayerPaidItem;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GachaTicketDetailWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject m_window;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Transform m_panelInstanceParent;

	[SerializeField]
	private GameObject m_panelPrefab;

	[SerializeField]
	private ScrollRect m_scrollRect;

	public bool _isOpen;

	public virtual bool IsOpen => _isOpen;

	public virtual void Start()
	{
		if (m_closeButton != null)
		{
			m_closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
	}

	public virtual void Update()
	{
	}

	public async UniTask Open(bool isPlaySe = true)
	{
		if (!_isOpen)
		{
			_isOpen = true;
			await SupplyReflesh();
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			if (m_window != null)
			{
				m_window.SetActive(value: true);
				m_window.GetComponent<Animator>().SetTrigger("In");
			}
			await UniTask.WaitWhile(() => _isOpen);
		}
	}

	public void Close(bool isPlaySe = true)
	{
		if (_isOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			if (m_window != null)
			{
				m_window.GetComponent<Animator>().SetTrigger("Out");
				m_window.SetActive(value: false);
			}
			_isOpen = false;
		}
	}

	public async UniTask SupplyReflesh()
	{
		m_scrollRect.verticalNormalizedPosition = 1f;
		foreach (Transform item in m_panelInstanceParent)
		{
			Object.Destroy(item.gameObject);
		}
		List<UniTask> list = new List<UniTask>();
		GameDataManager.PlayerPaidItem.RefreshAll();
		MFG.Types.PlayerItem.ItemID itemID = MFG.Types.PlayerItem.ItemID.GachaTicket;
		GachaTicketDetail component = Object.Instantiate(m_panelPrefab, m_panelInstanceParent).GetComponent<GachaTicketDetail>();
		list.Add(component.Reflesh(PlayerItemMaster.ToOID(itemID), GameDataManager.PlayerItem.Count(itemID), ""));
		IOrderedEnumerable<PlayerPaidItemInfo> orderedEnumerable = (from x in GameDataManager.PlayerPaidItem.Items?.Where((PlayerPaidItemInfo x) => x.Master.Category == MFG.Types.PlayerPaidItem.Category.GachaTicket)
			orderby x.ItemID
			select x);
		if (orderedEnumerable != null)
		{
			foreach (PlayerPaidItemInfo item2 in orderedEnumerable)
			{
				foreach (PaidItem item3 in item2.Details.OrderBy((PaidItem x) => x.Expire))
				{
					component = Object.Instantiate(m_panelPrefab, m_panelInstanceParent).GetComponent<GachaTicketDetail>();
					list.Add(component.Reflesh(item2.Master.ObjectID, item3.ItemCount, item3.FormatExpire));
				}
			}
		}
		await list;
	}
}

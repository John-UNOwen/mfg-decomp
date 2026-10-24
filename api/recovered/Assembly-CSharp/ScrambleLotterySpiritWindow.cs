using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrambleLotterySpiritWindow : MonoBehaviour
{
	[SerializeField]
	public GameObject m_window;

	[SerializeField]
	public Button m_closeButton;

	[SerializeField]
	public Button m_decideButton;

	[SerializeField]
	public ScrollRect m_scrollRect;

	[SerializeField]
	public GameObject m_panelInstanceParent;

	[SerializeField]
	public GameObject m_panelPrefab;

	[SerializeField]
	public TextMeshProUGUI m_medalText;

	[SerializeField]
	public TextMeshProUGUI m_spiritChoosingText;

	private ScrambleLotteryPlayWindow _scrambleLotteryPlayWindow;

	public bool _isOpen;

	private JanponObject.JanponKind _janponKind;

	public virtual bool IsOpen => _isOpen;

	public virtual async void Start()
	{
		if (m_closeButton != null)
		{
			m_closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
	}

	public virtual async UniTask Init(LotteryDetail detail)
	{
		if (m_decideButton != null)
		{
			m_decideButton.OnClickAsObservable().Subscribe(async delegate
			{
				await LotteryStart(detail, _janponKind);
			}).AddTo(this);
		}
		m_scrollRect.verticalNormalizedPosition = 1f;
		m_medalText.text = GameDataManager.ScrambleFestival.CurrentInfo.Medal.ToString();
		m_spiritChoosingText.text = "0";
		foreach (Transform item in m_panelInstanceParent.transform)
		{
			Object.Destroy(item.gameObject);
		}
		List<UniTask> list = new List<UniTask>();
		foreach (ItemID fightSpirit in PlayerItemMaster.GetFightSpiritList())
		{
			LotterySpiritChoice component = Object.Instantiate(m_panelPrefab, m_panelInstanceParent.transform).GetComponent<LotterySpiritChoice>();
			list.Add(component.Reflesh(fightSpirit, this));
		}
		await list;
	}

	public virtual void Refresh()
	{
		m_medalText.text = GameDataManager.ScrambleFestival.CurrentInfo.Medal.ToString();
		m_spiritChoosingText.text = "0";
		foreach (Transform item in m_panelInstanceParent.transform)
		{
			item.GetComponent<LotterySpiritChoice>().CounterInit();
		}
	}

	public int ChoosingSpiritSum()
	{
		try
		{
			int num = 0;
			foreach (Transform item in m_panelInstanceParent.transform)
			{
				num += item.GetComponent<LotterySpiritChoice>().SelectingCount;
			}
			return num;
		}
		catch
		{
		}
		return 0;
	}

	public virtual void Update()
	{
		int num = ChoosingSpiritSum();
		m_spiritChoosingText.text = $"{num}";
		m_decideButton.interactable = num == GameDataManager.ScrambleFestival.CurrentInfo.Master.SpiritLotteryConsume.Spirit;
	}

	public async UniTask Open(ScrambleLotteryPlayWindow scrambleLotteryPlayWindow, bool isPlaySe = true, JanponObject.JanponKind kind = (JanponObject.JanponKind)(-1))
	{
		if (!_isOpen)
		{
			_janponKind = kind;
			Refresh();
			_scrambleLotteryPlayWindow = scrambleLotteryPlayWindow;
			_isOpen = true;
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

	public async UniTask LotteryStart(LotteryDetail detail, JanponObject.JanponKind kind = (JanponObject.JanponKind)(-1))
	{
		if (_scrambleLotteryPlayWindow != null)
		{
			Dictionary<ItemID, int> dictionary = new Dictionary<ItemID, int>();
			foreach (Transform item in m_panelInstanceParent.transform)
			{
				LotterySpiritChoice component = item.GetComponent<LotterySpiritChoice>();
				dictionary.Add(component.m_itemID, component.SelectingCount);
			}
			await _scrambleLotteryPlayWindow.Open(isWithoutDuplicates: true, detail, dictionary, isPlaySe: true, kind);
		}
		Close(isPlaySe: false);
	}
}

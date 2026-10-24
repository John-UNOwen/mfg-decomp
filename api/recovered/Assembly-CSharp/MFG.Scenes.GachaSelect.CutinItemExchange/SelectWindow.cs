using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect.CutinItemExchange;

public class SelectWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject m_panelInstanceParent;

	[SerializeField]
	private GameObject m_panelPrefab;

	[SerializeField]
	private ConfirmWindow m_confirmWindow;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private TextMeshProUGUI m_currentExchangePointText;

	[SerializeField]
	private TextMeshProUGUI m_exchangeTextAll;

	[SerializeField]
	private Button m_upButton;

	[SerializeField]
	private Button m_downButton;

	[SerializeField]
	private ScrollRect m_scrollRect;

	[SerializeField]
	private GameObject m_raycastMaskPanel;

	private bool m_isInitialized;

	private bool m_enableForceTimeOutExit;

	private GachaSeriesServerInfo m_gachaSeriesInfo;

	private bool m_isOpen;

	public bool IsOpen => m_isOpen;

	private void Start()
	{
	}

	private void OnEnable()
	{
	}

	private void Update()
	{
		if (m_enableForceTimeOutExit && ResidentFlow.GetInstance().GetTimer().IsEnd())
		{
			m_enableForceTimeOutExit = false;
			ForceTimeOutExit();
		}
	}

	private void ForceTimeOutExit()
	{
		SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
		base.gameObject.GetComponent<Animator>().SetTrigger("Out");
		RayCastMaskPanel_Close();
		m_isOpen = false;
	}

	public async UniTask Init(GachaSeriesServerInfo gachaSeriesInfo)
	{
		CancellationToken token = this.GetCancellationTokenOnDestroy();
		try
		{
			m_scrollRect.verticalNormalizedPosition = 1f;
			m_gachaSeriesInfo = gachaSeriesInfo;
			m_enableForceTimeOutExit = true;
			if (!m_isInitialized)
			{
				m_closeButton.OnClickAsObservable().Subscribe(delegate
				{
					ForceTimeOutExit();
				}).AddTo(this);
				m_upButton?.OnClickAsObservable().Subscribe(delegate
				{
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
					m_scrollRect.velocity = new Vector2(0f, -450f);
				}).AddTo(this);
				m_downButton?.OnClickAsObservable().Subscribe(delegate
				{
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
					m_scrollRect.velocity = new Vector2(0f, 450f);
				}).AddTo(this);
				m_isInitialized = true;
			}
			foreach (Transform item in m_panelInstanceParent.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			foreach (ItemID exchangeItem in gachaSeriesInfo.ExchangeItems)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_panelPrefab, m_panelInstanceParent.transform);
				ItemPanel itemPanel = gameObject.GetComponent<ItemPanel>();
				itemPanel.Init(m_gachaSeriesInfo, exchangeItem).Forget();
				itemPanel.OnClickExchangeButton.Subscribe(async delegate
				{
					_ = 1;
					try
					{
						m_enableForceTimeOutExit = false;
						await m_confirmWindow.Open(m_gachaSeriesInfo, itemPanel.m_itemID, token);
						await UniTask.WaitWhile(() => m_confirmWindow.IsOpen, PlayerLoopTiming.Update, token);
						Reflesh();
						m_enableForceTimeOutExit = true;
					}
					catch (OperationCanceledException)
					{
					}
				}).AddTo(this);
			}
			m_exchangeTextAll.text = m_gachaSeriesInfo.GetExchangePointLabel(1);
			m_currentExchangePointText.text = $"{m_gachaSeriesInfo.GetExchangePoint()}";
		}
		catch (OperationCanceledException)
		{
		}
	}

	public async UniTask Open(GachaSeriesServerInfo gachaSeriesInfo)
	{
		await Init(gachaSeriesInfo);
		m_isOpen = true;
		RayCastMaskPanel_Open();
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		base.gameObject.GetComponent<Animator>().SetTrigger("In");
	}

	private void Reflesh()
	{
		m_exchangeTextAll.text = m_gachaSeriesInfo.GetExchangePointLabel(1);
		m_currentExchangePointText.text = $"{m_gachaSeriesInfo.GetExchangePoint()}";
		foreach (Transform item in m_panelInstanceParent.transform)
		{
			item.GetComponent<ItemPanel>().Reflesh();
		}
	}

	private void RayCastMaskPanel_Open()
	{
		Animator animator = m_raycastMaskPanel?.GetComponent<Animator>();
		if (animator != null)
		{
			animator.SetTrigger("In");
		}
		else
		{
			m_raycastMaskPanel?.SetActive(value: true);
		}
	}

	private void RayCastMaskPanel_Close()
	{
		Animator animator = m_raycastMaskPanel?.GetComponent<Animator>();
		if (animator != null)
		{
			animator.SetTrigger("Out");
		}
		else
		{
			m_raycastMaskPanel?.SetActive(value: false);
		}
	}
}

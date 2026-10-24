using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MahjongPlay.Cutin;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomCutinWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollView m_scrollView;

	[SerializeField]
	private GameObject m_actPrefab;

	[SerializeField]
	private List<GameObject> m_filterButtons = new List<GameObject>();

	[SerializeField]
	private GameObject m_detailWindow;

	[SerializeField]
	private List<Sprite> m_rareSprites = new List<Sprite>();

	[SerializeField]
	private Button m_allRemoveButton;

	[SerializeField]
	private GameObject m_copyRightPonnomichi;

	[SerializeField]
	private GameObject m_netcafeUnableText;

	private List<CutinItemInfo> m_dispItemList = new List<CutinItemInfo>();

	private CharaType m_charaType;

	private Category m_selectedCategory;

	private bool m_isTabSelecting;

	[SerializeField]
	private Button m_closeButton;

	private bool m_isClosed;

	private MyRoomFlow myRoomFlow;

	[SerializeField]
	private List<PreviewPlayerInteractor> m_previewPlayerInteractors = new List<PreviewPlayerInteractor>();

	private CancellationTokenSource _listCts;

	private CancellationTokenSource m_cutinPreviewCts;

	protected void Start()
	{
		m_detailWindow.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_detailWindow.SetActive(value: false);
			m_cutinPreviewCts?.Cancel();
		})
			.AddTo(this);
		for (int i = 0; i <= 4; i++)
		{
			int _i = i;
			m_filterButtons[i].GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
			{
				if (!m_isTabSelecting)
				{
					m_isTabSelecting = true;
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
					m_filterButtons[(int)m_selectedCategory].GetComponent<Animator>().SetTrigger("Normal");
					m_filterButtons[(int)m_selectedCategory].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: false);
					m_selectedCategory = (Category)_i;
					m_filterButtons[(int)m_selectedCategory].GetComponent<Animator>().SetTrigger("Selected");
					m_filterButtons[(int)m_selectedCategory].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: true);
					ItemListCategoryMask();
					await UniTask.Delay(TimeSpan.FromSeconds(0.5));
					m_isTabSelecting = false;
				}
			})
				.AddTo(this);
		}
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_isClosed = true;
		}).AddTo(this);
		m_allRemoveButton.OnClickAsObservable().Subscribe(async delegate
		{
			GameDataManager.Mission.IsChangeCutIn = true;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			foreach (Transform item in m_scrollView.GetListParent().transform)
			{
				if (item.gameObject.activeSelf)
				{
					item.GetComponentInChildren<CustomizeItemPanel>().m_info.IsOn = false;
					await item.GetComponentInChildren<CustomizeItemPanel>().RefleshOnOff();
				}
			}
			m_allRemoveButton.interactable = false;
		}).AddTo(this);
	}

	private void DebugAddItem(ItemID id)
	{
		CutinItemInfo item = new CutinItemInfo
		{
			ItemID = id
		};
		GameDataManager.Customize.CutinItems.Add(item);
	}

	public async UniTask Play(MyRoomFlow mrf, CancellationToken cancellationToken)
	{
		myRoomFlow = mrf;
		cancellationToken.ThrowIfCancellationRequested();
		_listCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		try
		{
			Debug.Log("MyRoomCutinWindow.Play begin");
			m_isClosed = false;
			m_charaType = GameDataManager.Player.SelectChara;
			m_detailWindow.SetActive(value: false);
			m_selectedCategory = Category.None;
			m_filterButtons[0].GetComponent<Animator>().SetTrigger("Selected");
			m_filterButtons[0].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: true);
			for (int i = 1; i <= 4; i++)
			{
				m_filterButtons[i].GetComponent<Animator>().SetTrigger("Normal");
				m_filterButtons[i].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: false);
			}
			CreateItemList(_listCts.Token).Forget();
			await UniTask.WaitUntil(() => m_isClosed, PlayerLoopTiming.Update, cancellationToken);
			_listCts.Cancel();
			_listCts.Dispose();
		}
		finally
		{
			Debug.Log($"MyRoomCutinWindow.Play end listCts:{_listCts.IsCancellationRequested}, cancellationToken:{cancellationToken.IsCancellationRequested} ");
		}
	}

	private void ItemListCategoryMask()
	{
		Category selectedCategory = m_selectedCategory;
		foreach (Transform item in m_scrollView.GetListParent().transform)
		{
			ItemID itemID = item.GetComponent<CustomizeItemPanel>().m_itemID;
			if (itemID != 0)
			{
				if (selectedCategory == Category.None)
				{
					item.gameObject.SetActive(value: true);
				}
				else
				{
					item.gameObject.SetActive(CutinItemMaster.GetItem(itemID).Category == selectedCategory);
				}
			}
		}
		m_scrollView.ResetScroll();
		m_allRemoveButton.interactable = false;
		foreach (Transform item2 in m_scrollView.GetListParent().transform)
		{
			if (item2.gameObject.activeSelf && item2.GetComponentInChildren<CustomizeItemPanel>().m_info.IsOn)
			{
				m_allRemoveButton.interactable = true;
			}
		}
	}

	private async UniTask CreateItemList(CancellationToken cancellationToken)
	{
		try
		{
			Debug.Log("CreateItemList begin");
			cancellationToken.ThrowIfCancellationRequested();
			m_scrollView.ResetList();
			m_dispItemList = (from a in GameDataManager.Customize.CutinItems
				where a.Master.CharaType == m_charaType && a.Master.IsDetachable
				orderby a.Master.Category, a.Master.Rarity, a.Master.ItemID
				select a).ToList();
			m_allRemoveButton.interactable = false;
			if (m_dispItemList.Find((CutinItemInfo a) => a.IsOn) != null)
			{
				m_allRemoveButton.interactable = true;
			}
			m_scrollView.ResetScroll();
			foreach (CutinItemInfo dispItem in m_dispItemList)
			{
				await InstantiateCustomizeItemPanel(dispItem, cancellationToken);
			}
			ItemListCategoryMask();
		}
		finally
		{
			Debug.Log($"CreateItemList end: cancel{cancellationToken.IsCancellationRequested}");
		}
	}

	private async UniTask InstantiateCustomizeItemPanel(CutinItemInfo info, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		GameObject itemView = m_scrollView.Add(m_actPrefab);
		itemView.SetActive(value: false);
		await itemView.GetComponentInChildren<CustomizeItemPanel>().Reflesh(info, isDispState: true, cancellationToken);
		itemView.SetActive(value: true);
		itemView.GetComponentInChildren<Button>().OnClickAsObservableSafety().Subscribe(async delegate
		{
			m_netcafeUnableText.SetActive(CutinItemMaster.IsPonnomichi(info.ItemID) && GameUtility.IsKonastaOrNetcafeBehavior);
			if (info.Master.IsDetachable)
			{
				GameDataManager.Mission.IsChangeCutIn = true;
				if (info.IsOn)
				{
					SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
					info.IsOn = false;
				}
				else
				{
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
					info.IsOn = true;
				}
				m_allRemoveButton.interactable = false;
				foreach (Transform item in m_scrollView.GetListParent().transform)
				{
					if (item.gameObject.activeSelf && item.GetComponentInChildren<CustomizeItemPanel>().m_info.IsOn)
					{
						m_allRemoveButton.interactable = true;
					}
				}
				try
				{
					await itemView.GetComponentInChildren<CustomizeItemPanel>().Reflesh(info, isDispState: true, cancellationToken);
					return;
				}
				catch (OperationCanceledException)
				{
					return;
				}
			}
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
		})
			.AddTo(itemView);
		itemView.GetComponentInChildren<Button>().OnLongTapAsObservable().Subscribe(delegate
		{
			DispItemDetail(info);
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		})
			.AddTo(itemView);
	}

	private void DispItemDetail(CutinItemInfo info)
	{
		m_detailWindow.SetActive(value: true);
		Item item = CutinItemMaster.GetItem(info.ItemID);
		m_copyRightPonnomichi.SetActive(CutinItemMaster.IsPonnomichi(info.ItemID));
		m_detailWindow.transform.Find("ActName").Find("ActNameText").GetComponent<TextMeshProUGUI>()
			.text = item.Name;
		if (m_rareSprites[(int)item.Rarity] != null)
		{
			m_detailWindow.transform.Find("Rare").gameObject.SetActive(value: true);
			m_detailWindow.transform.Find("Rare").GetComponent<Image>().sprite = m_rareSprites[(int)item.Rarity];
		}
		else
		{
			m_detailWindow.transform.Find("Rare").gameObject.SetActive(value: false);
		}
		m_cutinPreviewCts = new CancellationTokenSource();
		PlayPreview(info, m_cutinPreviewCts).Forget();
	}

	private async UniTask PlayPreview(CutinItemInfo info, CancellationTokenSource cts)
	{
		ItemID itemID = info.ItemID;
		Item itemInfo = CutinItemMaster.GetItem(itemID);
		int nakiType = 0;
		bool isRon = true;
		Cha sourceCha = Cha.Jicha;
		PreviewPlayerInteractor previewPlayerInteractor = m_previewPlayerInteractors[(int)sourceCha];
		CutinType cutinType = CreateDebugCutinType(itemInfo.Category, nakiType, isRon);
		previewPlayerInteractor.SetPreviewParents(myRoomFlow.CutinManager.PlayerInteractors[0]);
		try
		{
			while (true)
			{
				myRoomFlow.CutinManager.Play(cutinType, new CutinItemInfo
				{
					ItemID = itemID,
					IsOn = true
				}, sourceCha, m_previewPlayerInteractors, null, cts.Token).Forget();
				if (itemInfo.Category == Category.AgariSmall || itemInfo.Category == Category.AgariBig)
				{
					await myRoomFlow.CutinManager.AgariCutin_WaitIntroductionEnd(cts.Token);
					myRoomFlow.CutinManager.CurrentCutin_End(cts.Token).Forget();
				}
				await UniTask.WaitWhile(() => myRoomFlow.CutinManager.IsPlaying, PlayerLoopTiming.Update, cts.Token);
				await UniTask.Delay(500, ignoreTimeScale: false, PlayerLoopTiming.Update, cts.Token);
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log("CutinPreview Canceled");
		}
		static CutinType CreateDebugCutinType(Category _category, int _nakiType, bool _isRon)
		{
			CutinType result = CutinType.Pon;
			switch (_category)
			{
			case Category.Naki:
				switch (_nakiType)
				{
				case 0:
					result = CutinType.Pon;
					break;
				case 1:
					result = CutinType.Chi;
					break;
				case 2:
					result = CutinType.Kan;
					break;
				}
				break;
			case Category.AgariSmall:
			case Category.AgariBig:
				result = ((!_isRon) ? CutinType.Tsumo : CutinType.Ron);
				break;
			case Category.Reach:
				result = CutinType.Reach;
				break;
			}
			return result;
		}
	}

	private void Update()
	{
		_ = GameUtility.OnBeginSkipBootSceneFinished;
	}
}

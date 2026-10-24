using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomCustomizeWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollView m_scrollView;

	[SerializeField]
	private GameObject m_itemPrefab;

	[SerializeField]
	private Button _allOffButton;

	[SerializeField]
	private List<Button> m_categoryButtons = new List<Button>();

	[SerializeField]
	private TextMeshProUGUI _itemName;

	[SerializeField]
	private GameObject m_copyRightToho;

	[SerializeField]
	private GameObject m_copyRightPonnomichi;

	[SerializeField]
	private GameObject m_netcafeUnableText;

	[SerializeField]
	private Button m_btStickerEdit;

	[SerializeField]
	private TextMeshProUGUI m_goodsCounter;

	private Category m_selectingCategory;

	private ItemID m_selectingItemID;

	private bool m_isCategorySelecting;

	private bool m_isItemSelecting;

	private CancellationTokenSource _bgmCts;

	private CancellationToken _bgmToken;

	private CancellationTokenSource _listCts;

	protected void Start()
	{
		CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
		foreach (Category value in Enum.GetValues(typeof(Category)))
		{
			Category category = value;
			m_categoryButtons[(int)category].OnClickAsObservable().Subscribe(async delegate
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();
					await ChangeCategory(category, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					Debug.Log($"MyRoomCustomizeWindow.CategoryChange cancel. category:{category}");
				}
			}).AddTo(this);
		}
		_allOffButton.OnClickAsObservable().Subscribe(delegate
		{
			if (m_isItemSelecting || m_isCategorySelecting)
			{
				return;
			}
			try
			{
				m_isItemSelecting = true;
				m_isCategorySelecting = true;
				if (CustomizeItemMaster.IsMultiEquipEnableCategory(m_selectingCategory) || CustomizeItemMaster.IsUpperEquipEnableCategory(m_selectingCategory))
				{
					SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
					CharaGameData currentChara = GameDataManager.Player.GetCurrentChara();
					currentChara.CustomizeItemEquip_SetEquipAll(m_selectingCategory, isEquip: false);
					Refresh(m_selectingCategory, currentChara.charaType, currentChara.CustomizeItemEquips[m_selectingCategory], _listCts.Token).Forget();
					foreach (KeyValuePair<int, GameObject> @object in m_scrollView.ObjectList)
					{
						@object.Value.transform.Find("EquipIcon").gameObject.SetActive(currentChara.CustomizeItemEquip_IsEquip(m_selectingCategory, (ItemID)@object.Key));
					}
					AllOffButton_Reflesh(m_selectingCategory, currentChara);
				}
			}
			catch (OperationCanceledException)
			{
			}
			finally
			{
				m_isItemSelecting = false;
				m_isCategorySelecting = false;
			}
		}).AddTo(this);
	}

	private async UniTask ChangeCategory(Category category, CancellationToken cancellationToken)
	{
		if (m_selectingCategory == category || m_isCategorySelecting)
		{
			return;
		}
		_listCts.Cancel();
		_listCts.Dispose();
		_listCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		try
		{
			m_isCategorySelecting = true;
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			SoundManager.PlayMyCharaMyRoomBGM().Forget();
			m_categoryButtons[(int)m_selectingCategory].GetComponent<Animator>().SetTrigger("Normal");
			m_selectingCategory = category;
			m_categoryButtons[(int)m_selectingCategory].GetComponent<Animator>().SetTrigger("Selected");
			m_btStickerEdit.gameObject.SetActive(category == Category.TableSticker);
			m_goodsCounter.gameObject.SetActive(category == Category.TableSticker);
			CreateItemList(_listCts.Token).Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, _listCts.Token);
		}
		catch (OperationCanceledException)
		{
			Debug.Log($"MyRoomCustomizeWindow.CategoryChange cancel. category:{category}, listCts:{_listCts.IsCancellationRequested}, cancellationToken:{cancellationToken.IsCancellationRequested} ");
		}
		finally
		{
			m_isCategorySelecting = false;
			Debug.Log($"MyRoomCustomizeWindow.CategoryChange end. category:{category}, listCts:{_listCts.IsCancellationRequested}, cancellationToken:{cancellationToken.IsCancellationRequested} ");
		}
	}

	private async UniTask CreateItemList(CancellationToken cancellationToken)
	{
		Debug.Log("CreateItemList begin");
		Category category = m_selectingCategory;
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			CharaGameData currentChara = GameDataManager.Player.GetCurrentChara();
			List<ItemID> equips = currentChara.CustomizeItemEquips[category];
			List<(CustomizeItemInfo, bool)> list = (from a in GameDataManager.Customize.CustomizeItems.GetPossessionItems(category, currentChara.charaType)
				orderby a.Master.Sort, a.Master.charaType, a.ItemID
				select (ItemInfo: a, IsEquip: equips.Exists((ItemID b) => b == a.ItemID))).ToList();
			(CustomizeItemInfo, bool) tuple = list.Where<(CustomizeItemInfo, bool)>(((CustomizeItemInfo ItemInfo, bool IsEquip) a) => a.IsEquip).FirstOrDefault();
			var (customizeItemInfo, flag) = tuple;
			if (customizeItemInfo != null || flag)
			{
				m_selectingItemID = tuple.Item1.ItemID;
			}
			else
			{
				m_selectingItemID = CustomizeItemMaster.GetDefaultEquipItemID(category);
			}
			_itemName.text = CustomizeItemMaster.GetItem(m_selectingItemID).DispName;
			AllOffButton_Reflesh(category, GameDataManager.Player.GetCurrentChara());
			m_scrollView.ResetList();
			m_copyRightToho.SetActive(CustomizeItemMaster.IsTohoMusic(m_selectingItemID));
			m_copyRightPonnomichi.SetActive(CustomizeItemMaster.IsPonnomichi(m_selectingItemID));
			m_netcafeUnableText.SetActive(CustomizeItemMaster.IsPonnomichi(m_selectingItemID) && GameUtility.IsKonastaOrNetcafeBehavior);
			foreach (var item in list)
			{
				GameObject obj = m_scrollView.Add(m_itemPrefab, (int)item.Item1.ItemID);
				await CreateItem(item, obj, cancellationToken);
			}
		}
		finally
		{
			Debug.Log($"CreateItemList end. category:{category}, cancellationToken: {cancellationToken.IsCancellationRequested}");
		}
	}

	private void AllOffButton_Reflesh(Category category, CharaGameData currentCharaGameData)
	{
		_allOffButton.gameObject.SetActive(CustomizeItemMaster.IsMultiEquipEnableCategory(category) || CustomizeItemMaster.IsUpperEquipEnableCategory(category));
		_allOffButton.interactable = currentCharaGameData.CustomizeItemEquips[category].Count > 0;
	}

	private async UniTask CreateItem((CustomizeItemInfo ItemInfo, bool IsEquip) item, GameObject obj, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		obj.transform.Find("Icon").GetComponent<Image>().SetAlpha(0f);
		Image component = obj.transform.Find("Icon").GetComponent<Image>();
		component.sprite = await item.ItemInfo.Master.IconGraphic(cancellationToken);
		obj.transform.Find("Icon").GetComponent<Image>().DOFade(1f, 0.2f)
			.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken)
			.Forget();
		obj.transform.Find("CursorIcon").gameObject.SetActive(item.ItemInfo.ItemID == m_selectingItemID);
		obj.transform.Find("EquipIcon").gameObject.SetActive(item.IsEquip);
		obj.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
		{
			if (!m_isItemSelecting)
			{
				CharaGameData currentChara = GameDataManager.Player.GetCurrentChara();
				bool flag = CustomizeItemMaster.IsMultiEquipEnableCategory(m_selectingCategory);
				bool flag2 = CustomizeItemMaster.IsUpperEquipEnableCategory(m_selectingCategory);
				if (flag || flag2 || m_selectingItemID != item.ItemInfo.ItemID)
				{
					try
					{
						m_isItemSelecting = true;
						ItemID selectingItemID = m_selectingItemID;
						ItemID itemID = item.ItemInfo.ItemID;
						m_selectingItemID = itemID;
						GameDataManager.Mission.IsChangeEquips = true;
						bool flag3 = currentChara.CustomizeItemEquip_IsEquip(m_selectingCategory, itemID);
						if (flag)
						{
							currentChara.CustomizeItemEquip_ToggleEquip(m_selectingCategory, itemID);
						}
						else if (flag2)
						{
							if (m_selectingCategory == Category.TableSticker)
							{
								currentChara.CustomizeItemEquip_ToggleEquip_Sticker(itemID);
							}
						}
						else
						{
							currentChara.CustomizeItemEquip_SetEquip(m_selectingCategory, selectingItemID, isEquip: false);
							currentChara.CustomizeItemEquip_SetEquip(m_selectingCategory, itemID, isEquip: true);
						}
						currentChara.CustomizeItemEquip_IsEquip(m_selectingCategory, selectingItemID);
						List<ItemID> currentEquips = currentChara.CustomizeItemEquips[m_selectingCategory];
						Refresh(m_selectingCategory, currentChara.charaType, currentEquips, cancellationToken).Forget();
						cancellationToken.ThrowIfCancellationRequested();
						GameObject gameObject = ((selectingItemID == ItemID.TableStickerNone) ? null : m_scrollView.GetObject((int)selectingItemID));
						GameObject @object = m_scrollView.GetObject((int)itemID);
						bool flag4 = currentChara.CustomizeItemEquip_IsEquip(m_selectingCategory, itemID);
						AllOffButton_Reflesh(m_selectingCategory, currentChara);
						if (selectingItemID != itemID)
						{
							if (gameObject != null)
							{
								gameObject.transform.Find("CursorIcon").gameObject.SetActive(value: false);
							}
							@object.transform.Find("CursorIcon").gameObject.SetActive(value: true);
							_itemName.text = CustomizeItemMaster.GetItem(itemID).DispName;
							m_copyRightToho.SetActive(CustomizeItemMaster.IsTohoMusic(itemID));
							m_copyRightPonnomichi.SetActive(CustomizeItemMaster.IsPonnomichi(itemID));
							m_netcafeUnableText.SetActive(CustomizeItemMaster.IsPonnomichi(itemID) && GameUtility.IsKonastaOrNetcafeBehavior);
						}
						if (flag)
						{
							SoundManager.PlaySe(flag4 ? "SFX_SYS_INPUT1" : "SFX_SYS_CANCEL2").Forget();
							@object.transform.Find("EquipIcon").gameObject.SetActive(flag4);
						}
						else if (flag2)
						{
							if (!flag3 && flag4)
							{
								SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
							}
							else if (flag3 && !flag4)
							{
								SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
							}
							@object.transform.Find("EquipIcon").gameObject.SetActive(flag4);
						}
						else if (selectingItemID != itemID)
						{
							SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
							if (gameObject != null)
							{
								gameObject.transform.Find("EquipIcon").gameObject.SetActive(value: false);
							}
							@object.transform.Find("EquipIcon").gameObject.SetActive(value: true);
						}
						ItemID itemID2 = item.ItemInfo.Master.ItemID;
						if (item.ItemInfo.Master.Category == Category.ReachBgm)
						{
							BgmCancelToken_Cancel();
							SoundManager.PlayBgm(itemID2.ReachBgmToThumbBgmLabel()).Forget();
						}
						else if (item.ItemInfo.Master.Category == Category.MyRoomBgm)
						{
							BgmCancelToken_Cancel();
							SoundManager.PlayMyCharaMyRoomBGM().Forget();
						}
						await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
					}
					catch (OperationCanceledException)
					{
					}
					finally
					{
						m_isItemSelecting = false;
					}
				}
			}
		})
			.AddTo(this);
	}

	public void BgmCancelToken_Reserve()
	{
		BgmCancelToken_Cancel();
		_bgmCts = new CancellationTokenSource();
		_bgmToken = _bgmCts.Token;
	}

	private void BgmCancelToken_Cancel()
	{
		_bgmCts?.Cancel();
		_bgmCts = null;
		_bgmToken = default(CancellationToken);
	}

	private void BgmCancelToken_Clear()
	{
		_bgmCts = null;
		_bgmToken = default(CancellationToken);
	}

	public void BgmPreviewStop()
	{
		BgmCancelToken_Cancel();
	}

	public void Init(MyRoomDetailWindow pageAll, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_listCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		Debug.Log("MyRoomCustomizeWindow.Play begin");
		try
		{
			m_selectingCategory = Category.ReachStick;
			foreach (Button categoryButton in m_categoryButtons)
			{
				categoryButton.GetComponent<Animator>().SetTrigger("Normal");
			}
			m_categoryButtons[(int)m_selectingCategory].GetComponent<Animator>().ResetTrigger("Normal");
			m_categoryButtons[(int)m_selectingCategory].GetComponent<Animator>().SetTrigger("Selected");
			m_goodsCounter.gameObject.SetActive(value: false);
			m_btStickerEdit.gameObject.SetActive(value: false);
			m_btStickerEdit.OnClickAsObservable().Subscribe(async delegate
			{
				try
				{
					if (m_selectingCategory == Category.TableSticker && GameDataManager.Player.GetCurrentChara().CustomizeItemEquips[m_selectingCategory].Count > 0)
					{
						await pageAll.WindowOpen(MyRoomFlow.WindowType.StickerEditor, cancellationToken);
					}
				}
				catch (OperationCanceledException)
				{
					Debug.Log("MyRoomCustomizeWindow: OperationCanceledException Catched.");
				}
			}).AddTo(this);
			CharaGameData currentChara = GameDataManager.Player.GetCurrentChara();
			foreach (Category value in Enum.GetValues(typeof(Category)))
			{
				List<ItemID> currentEquips = currentChara.CustomizeItemEquips[value];
				Refresh(value, currentChara.charaType, currentEquips, _listCts.Token).Forget();
			}
			CreateItemList(_listCts.Token).Forget();
		}
		finally
		{
			Debug.Log("MyRoomCustomizeWindow.Play end");
		}
	}

	public void ClosingTask()
	{
		BgmPreviewStop();
		SoundManager.PlayMyCharaMyRoomBGM().Forget();
	}

	public async UniTask Refresh(Category category, CharaType charaType, List<ItemID> currentEquips, CancellationToken cancellationToken = default(CancellationToken))
	{
		Image iconImage = m_categoryButtons[(int)category].transform.Find("Bt/IconGoods").GetComponent<Image>();
		var (text, sprite) = await MyRoomUtility.GetItemNameAndIconSpriteAtCategory(category, charaType, currentEquips);
		if (sprite != null && iconImage.sprite != sprite)
		{
			iconImage.SetAlpha(0f);
			iconImage.SetSpriteAndFadeIn(sprite).Forget();
		}
		TextMeshProUGUI component = m_categoryButtons[(int)category].transform.Find("Bt/GoodsNameText").GetComponent<TextMeshProUGUI>();
		component.overflowMode = ((!SaniTize.IsSaniTized(text)) ? TextOverflowModes.Ellipsis : TextOverflowModes.Overflow);
		component.text = text;
		if (category == Category.TableSticker)
		{
			m_btStickerEdit.interactable = currentEquips.Count > 0;
			m_goodsCounter.text = $"{currentEquips.Count} / {GameDataManager.Player.EnableStickerCount}";
		}
	}
}

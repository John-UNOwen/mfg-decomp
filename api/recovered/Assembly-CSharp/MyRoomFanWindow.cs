using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Fan;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomFanWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollView m_scrollView;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Button m_helpButton;

	[SerializeField]
	private GameObject m_fanAchivementPrefab;

	[SerializeField]
	private GameObject m_fanHelp;

	[SerializeField]
	private Button m_helpCloseButton;

	[SerializeField]
	private ItemPopupManager _itemPopupManager;

	private bool m_isClosed;

	protected void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_isClosed = true;
		}).AddTo(this);
		m_helpButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_fanHelp.SetActive(value: true);
		}).AddTo(this);
		m_helpCloseButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_fanHelp.SetActive(value: false);
		}).AddTo(this);
	}

	public async UniTask Play(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_isClosed = false;
		int totalFan = GameDataManager.Player.GetCurrentChara().TotalFan;
		List<int> fans = GameDataManager.Player.GetCurrentChara().m_fans;
		base.transform.Find("FanNum").Find("FanNumFrame").Find("FanNumText")
			.GetComponent<TextMeshProUGUI>()
			.text = totalFan.ToString("N0");
		base.transform.Find("LimitMessage").gameObject.SetActive(!GameDataManager.Player.GetCurrentChara().IsDebut);
		Transform obj = base.transform.Find("FanAchievement").Find("ScrollView").Find("Index");
		obj.Find("LackFanNumText").GetComponent<TextMeshProUGUI>().text = fans[0].ToString("N0");
		obj.Find("SpeedFanNumText").GetComponent<TextMeshProUGUI>().text = fans[1].ToString("N0");
		obj.Find("PowerFanNumText").GetComponent<TextMeshProUGUI>().text = fans[2].ToString("N0");
		obj.Find("DefenceFanNumText").GetComponent<TextMeshProUGUI>().text = fans[3].ToString("N0");
		m_scrollView.ResetList();
		foreach (FanMasterData item in (from a in FanMaster.m_fanMastarTable
			where a.charaType == GameDataManager.Player.SelectChara
			orderby a.FanNum
			select a).ToList())
		{
			GameObject gameObject = m_scrollView.Add(m_fanAchivementPrefab);
			gameObject.transform.Find("FanNumText").GetComponent<TextMeshProUGUI>().text = item.FanNum.ToString("N0");
			SetFanItem(item, gameObject.transform.Find("ItemLack"), FanType.Seiryu, cancellationToken).Forget();
			SetFanItem(item, gameObject.transform.Find("ItemSpeed"), FanType.Suzaku, cancellationToken).Forget();
			SetFanItem(item, gameObject.transform.Find("ItemPower"), FanType.Byakko, cancellationToken).Forget();
			SetFanItem(item, gameObject.transform.Find("ItemDefence"), FanType.Genbu, cancellationToken).Forget();
			gameObject.transform.Find("GetLack").gameObject.SetActive(fans[0] >= item.FanNum);
			gameObject.transform.Find("GetSpeed").gameObject.SetActive(fans[1] >= item.FanNum);
			gameObject.transform.Find("GetPower").gameObject.SetActive(fans[2] >= item.FanNum);
			gameObject.transform.Find("GetDefence").gameObject.SetActive(fans[3] >= item.FanNum);
		}
		m_scrollView.ResetScroll();
		await UniTask.WaitUntil(() => m_isClosed, PlayerLoopTiming.Update, cancellationToken);
	}

	private async UniTask SetFanItem(FanMasterData masterData, Transform itemTransform, FanType fanType, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		MFG.Types.Customize.CustomizeItem.Item item = CustomizeItemMaster.GetItem(masterData.UnlockItemIDs[(int)fanType]);
		if (item != null)
		{
			itemTransform.gameObject.SetActive(value: true);
			itemTransform.Find("ItemNameText").GetComponent<TextMeshProUGUI>().text = item.DispName;
			itemTransform.GetComponent<Image>().SetAlpha(0f);
			Image component = itemTransform.GetComponent<Image>();
			component.sprite = await item.IconGraphic(cancellationToken);
			itemTransform.GetComponent<Image>().DOFade(1f, 0.2f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken)
				.Forget();
			itemTransform.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
			{
				RectTransform component3 = itemTransform.GetComponent<RectTransform>();
				await _itemPopupManager.Open(item.ObjectID, component3.position, component3.sizeDelta);
			})
				.AddTo(this);
			return;
		}
		itemTransform.gameObject.SetActive(value: true);
		MFG.Types.PlayerItem.Item playerItem = PlayerItemMaster.GetItem(PlayerItemMaster.ToFightSpiritItemID(GameDataManager.Player.SelectChara));
		itemTransform.Find("ItemNameText").GetComponent<TextMeshProUGUI>().text = playerItem.Name;
		itemTransform.GetComponent<Image>().SetAlpha(0f);
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(playerItem.IconGraphicPath);
		disposableAsset.AddTo(this);
		itemTransform.GetComponent<Image>().sprite = disposableAsset.Asset;
		itemTransform.GetComponent<Image>().DOFade(1f, 0.2f).ToUniTask()
			.Forget();
		itemTransform.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
		{
			RectTransform component2 = itemTransform.GetComponent<RectTransform>();
			await _itemPopupManager.Open(playerItem.ObjectID, component2.position, component2.sizeDelta);
		})
			.AddTo(this);
	}
}

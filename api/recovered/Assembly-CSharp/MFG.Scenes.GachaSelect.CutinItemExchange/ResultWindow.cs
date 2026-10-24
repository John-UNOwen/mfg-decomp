using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect.CutinItemExchange;

public class ResultWindow : MonoBehaviour
{
	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private GameObject m_base;

	[SerializeField]
	private GameObject m_raycastMaskPanel;

	[SerializeField]
	private GameObject m_cardPrefab;

	[SerializeField]
	private GameObject m_cardParent;

	[SerializeField]
	private GameObject m_informationParent;

	[SerializeField]
	private GameObject m_fightSpiritIconParent;

	private MFG.Types.Customize.CutinItem.ItemID m_itemID;

	private bool m_isOpen;

	private void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(async delegate
		{
			await Close();
		}).AddTo(this);
	}

	public async UniTask Open(GachaSeriesServerInfo gachaSeriesInfo, MFG.Types.Customize.CutinItem.ItemID itemID, bool isNew)
	{
		m_isOpen = true;
		m_raycastMaskPanel?.SetActive(value: true);
		CancellationToken token = this.GetCancellationTokenOnDestroy();
		await SetItem(gachaSeriesInfo, itemID, isNew);
		m_base.transform.GetComponent<ContentSizeFitter>().SetLayoutVertical();
		SoundManager.PlaySe("SFX_SYS_BONUS5").Forget();
		base.gameObject.GetComponent<Animator>().SetTrigger("In");
		try
		{
			m_itemID = itemID;
			await UniTask.WhenAny(UniTask.Delay(5000, ignoreTimeScale: false, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => !m_isOpen, PlayerLoopTiming.Update, token));
			await Close(token);
		}
		catch (OperationCanceledException)
		{
		}
	}

	public async UniTask Close(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (m_isOpen)
		{
			try
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				base.gameObject.GetComponent<Animator>().SetTrigger("Out");
				await UniTask.Delay(250, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				m_raycastMaskPanel?.SetActive(value: false);
				m_isOpen = false;
			}
			catch (OperationCanceledException)
			{
			}
		}
	}

	private GachaResultInfo GetExchangeResult(GachaSeriesServerInfo gachaSeriesInfo, MFG.Types.Customize.CutinItem.ItemID id, bool isNew)
	{
		int num = GameDataManager.PlayerItem.Count(MFG.Types.PlayerItem.ItemID.Present01);
		return new GachaResultInfo
		{
			ItemInfos = new List<GachaItemInfo>
			{
				new GachaItemInfo
				{
					ItemID = id,
					GiftType = ((!isNew) ? GachaGiftType.DuplicateGift : GachaGiftType.IsNew)
				}
			},
			FightSpiritInfos = new List<GachaFightSpiritInfo>
			{
				new GachaFightSpiritInfo
				{
					CharaType = LotteryPickUpCharaType(id)
				}
			},
			PrevGiftCount = (isNew ? num : (num - 1)),
			AfterGiftCount = num,
			AcquiredGiftCount = ((!isNew) ? 1 : 0),
			AcquiredExchangePoint = 0
		};
		CharaType LotteryPickUpCharaType(MFG.Types.Customize.CutinItem.ItemID cid)
		{
			foreach (CharaType key in GachaUtility.CharaUnlockItemIDDict.Keys)
			{
				if (cid == GachaUtility.CharaUnlockItemIDDict[key])
				{
					return key;
				}
			}
			List<CharaType> list = gachaSeriesInfo.PickUpCharas.FindAll((CharaType x) => GameDataManager.IsCharaSelectable(x));
			if (list.Count > 0 && UnityEngine.Random.Range(0f, 1f) >= 0.5f)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			List<CharaType> list2 = new List<CharaType>();
			foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
			{
				if (GameDataManager.IsCharaSelectable(value))
				{
					list2.Add(value);
				}
			}
			if (list2.Count <= 0)
			{
				return CharaType.Chara01;
			}
			return list2[UnityEngine.Random.Range(0, list2.Count)];
		}
	}

	public async UniTask SetItem(GachaSeriesServerInfo gachaSeriesInfo, MFG.Types.Customize.CutinItem.ItemID itemID, bool isNew)
	{
		GachaResultInfo gachaResultInfo = GetExchangeResult(gachaSeriesInfo, itemID, isNew);
		foreach (Transform item in m_cardParent.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in m_fightSpiritIconParent.transform)
		{
			item2.gameObject.SetActive(value: false);
		}
		int i = 0;
		while (i < gachaResultInfo.FightSpiritInfos.Count)
		{
			GachaFightSpiritInfo gachaFightSpiritInfo = gachaResultInfo.FightSpiritInfos[i];
			Image iconImage = m_fightSpiritIconParent.transform.Find($"Spirit{i + 1}")?.GetComponent<Image>();
			if (iconImage != null)
			{
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Dojo, gachaFightSpiritInfo.CharaType, DojoFlow.AssetType.FightSpirit));
				disposableAsset.AddTo(this);
				iconImage.gameObject.SetActive(value: true);
				iconImage.sprite = disposableAsset.Asset;
			}
			int num = i + 1;
			i = num;
		}
		bool isNormalGift = false;
		bool isUnableCharaGift = false;
		foreach (GachaItemInfo info in gachaResultInfo.ItemInfos)
		{
			GameObject cardView = UnityEngine.Object.Instantiate(m_cardPrefab, m_cardParent.transform);
			MFG.Types.Customize.CutinItem.Item master = CutinItemMaster.GetItem(info.ItemID);
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(master.IconGraphicPath);
			disposableAsset2.AddTo(cardView);
			cardView.GetComponent<Image>().sprite = disposableAsset2.Asset;
			cardView.transform.Find("New").gameObject.SetActive(info.GiftType == GachaGiftType.IsNew);
			cardView.transform.Find("ChangeGift").gameObject.SetActive(info.GiftType == GachaGiftType.DuplicateGift);
			if (info.GiftType == GachaGiftType.DuplicateGift)
			{
				isNormalGift = true;
			}
			cardView.transform.Find("ChangeGiftUnableChara").gameObject.SetActive(info.GiftType == GachaGiftType.ConfigGift);
			if (info.GiftType == GachaGiftType.ConfigGift)
			{
				isUnableCharaGift = true;
			}
			disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>("CharaSelect/Textures/CustomIconRare" + master.Rarity.ToString() + "Big.png");
			Transform transform = cardView.transform.Find("Rank").Find("Image");
			disposableAsset2.AddTo(transform);
			transform.GetComponent<Image>().sprite = disposableAsset2.Asset;
		}
		foreach (GachaFightSpiritInfo fightSpiritInfo in gachaResultInfo.FightSpiritInfos)
		{
			GameDataManager.PlayerItem.Add(PlayerItemMaster.ToFightSpiritItemID(fightSpiritInfo.CharaType), 1);
		}
		m_informationParent.SetActive(gachaResultInfo.AcquiredGiftCount > 0);
		m_informationParent.transform.Find("PresentsInformation/ InformationText").GetComponent<TextMeshProUGUI>().text = (isNormalGift ? "獲得済みのカットインはギフトに変換されました\n" : "") + (isUnableCharaGift ? "未所持ファイトガールのカットインはギフトに変換されました" : "");
		m_informationParent.transform.Find("PresentsInformation").Find("PresentsNumber").Find("BeforNumber")
			.GetComponent<TextMeshProUGUI>()
			.text = $"{gachaResultInfo.PrevGiftCount}";
		m_informationParent.transform.Find("PresentsInformation").Find("PresentsNumber").Find("AfterNumber")
			.GetComponent<TextMeshProUGUI>()
			.text = $"{gachaResultInfo.AfterGiftCount}";
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.GameData;
using MFG.Types;
using MFG.Types.OneShotNotice;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class OneShotNoticeWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	[Header("任意のオブジェクト群")]
	[SerializeField]
	private List<GameObject> gameObjects;

	[NonSerialized]
	public bool isOpen;

	[NonSerialized]
	public MFG.Types.OneShotNotice.Item Master;

	private async void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	public async UniTask OpenWindow()
	{
		if (!isOpen && !IsExtraUnable())
		{
			GameDataManager.PlayerConfig.OneShotNoticeFinishIDs.Add(Master.ID);
			closeButton.gameObject.SetActive(value: false);
			await (UniTask)(GetType().GetMethod(Master.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(this, null));
			closeButton.gameObject.SetActive(value: true);
			await UniTask.WhenAny(UniTask.WaitWhile(() => isOpen), UniTask.Delay(TimeSpan.FromSeconds(10.0)));
			if (isOpen)
			{
				await CloseWindow();
			}
		}
	}

	public async UniTask CloseWindow()
	{
		if (isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			base.gameObject.SetActive(value: false);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	private bool IsExtraUnable()
	{
		return Master.ID switch
		{
			NoticeID.GrimAroeExchangePoint => GameDataManager.PlayerItem.Count(ItemID.ExchangePointGrimAroe) <= 0, 
			NoticeID.DiaExchangePoint => GameDataManager.PlayerItem.Count(ItemID.ExchangePointDia) <= 0, 
			NoticeID.DoubrielExchangePoint => GameDataManager.PlayerItem.Count(ItemID.ExchangePointDoubriel) <= 0, 
			NoticeID.IppatsuExchangePoint => GameDataManager.PlayerItem.Count(ItemID.ExchangePointIppatsu) <= 0, 
			NoticeID.ShiroeExchangePoint => GameDataManager.PlayerItem.Count(ItemID.ExchangePointShiroe) <= 0, 
			NoticeID.JunExchangePoint => GameDataManager.PlayerItem.Count(ItemID.ExchangePointJun) <= 0, 
			NoticeID.ToukaExchangePoint => GameDataManager.PlayerItem.Count(ItemID.ExchangePointTouka) <= 0, 
			_ => false, 
		};
	}

	private async UniTask GrimAroeExchangePointOpen()
	{
		await TaskExchangePointOpen(ItemID.ExchangePointGrimAroe, GachaSeriesName.PickupGrimAroe);
	}

	private async UniTask DiaExchangePointOpen()
	{
		await TaskExchangePointOpen(ItemID.ExchangePointDia, GachaSeriesName.PickupDia);
	}

	private async UniTask DoubrielExchangePointOpen()
	{
		await TaskExchangePointOpen(ItemID.ExchangePointDoubriel, GachaSeriesName.PickupDoubriel);
	}

	private async UniTask IppatsuExchangePointOpen()
	{
		await TaskExchangePointOpen(ItemID.ExchangePointIppatsu, GachaSeriesName.PickupIppatsu);
	}

	private async UniTask ShiroeExchangePointOpen()
	{
		await TaskExchangePointOpen(ItemID.ExchangePointShiroe, GachaSeriesName.PickupShiroe);
	}

	private async UniTask JunExchangePointOpen()
	{
		await TaskExchangePointOpen(ItemID.ExchangePointJun, GachaSeriesName.PickupJun);
	}

	private async UniTask ToukaExchangePointOpen()
	{
		await TaskExchangePointOpen(ItemID.ExchangePointTouka, GachaSeriesName.PickupTouka);
	}

	private async UniTask TaskExchangePointOpen(ItemID exchangeItemID, GachaSeriesName gachaSeriesName)
	{
		TextMeshProUGUI magicalCandyCounter = gameObjects.Find((GameObject x) => x.name.Equals("MagicalCandyCount")).GetComponent<TextMeshProUGUI>();
		int candyCount = GameDataManager.PlayerItem.Count(exchangeItemID);
		TextMeshProUGUI exchangePointCounter = gameObjects.Find((GameObject x) => x.name.Equals("ExchangePointCount")).GetComponent<TextMeshProUGUI>();
		magicalCandyCounter.text = $"{candyCount}";
		exchangePointCounter.text = "0";
		isOpen = true;
		base.gameObject.SetActive(value: true);
		SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
		windowObj.GetComponent<Animator>().SetTrigger("In");
		GameDataManager.GachaInfo.GachaSeriesInfosFromServer?.Find((GachaSeriesServerInfo x) => x.GachaSeriesName == gachaSeriesName)?.AddExchangePoint(candyCount);
		GameDataManager.PlayerItem.Consume(exchangeItemID, candyCount, ItemConsumeType.OneShotWindow).Forget();
		await GameDataManager.Gacha.Save();
		await GameDataManager.PlayerItem.Save();
		await UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224));
		List<Tweener> tweenerList = new List<Tweener> { DOVirtual.Float(candyCount, 0f, 1f, delegate(float value)
		{
			int num = (int)value;
			magicalCandyCounter.text = $"{num}";
			exchangePointCounter.text = $"{candyCount - num}";
		}).OnComplete(delegate
		{
			magicalCandyCounter.text = "0";
			exchangePointCounter.text = $"{candyCount}";
		}) };
		await SoundManager.PlaySe("SFX_MJG_POINT1");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		if (tweenerList.Count > 0)
		{
			foreach (Tweener item in tweenerList)
			{
				item.Complete();
			}
		}
		SoundManager.StopSe("SFX_MJG_POINT1").Forget();
		SoundManager.PlaySe("SFX_MJG_POINT2").Forget();
	}
}

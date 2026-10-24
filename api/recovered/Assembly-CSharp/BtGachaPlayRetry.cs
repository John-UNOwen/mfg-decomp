using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameContents;
using Konami.System.Net;
using MFG.GameData;
using MFG.GameRequest;
using MFG.Types;
using MFG.Types.PlayerItem;
using TMPro;
using TestModeSkeleton;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BtGachaPlayRetry : MonoBehaviour
{
	[Serializable]
	private class GachaButtonFramework
	{
		public GachaPlayMode Mode;

		public Button Bt;

		public GameObject CreditFrame;

		public TextMeshProUGUI CreditText;

		public GameObject PaseliFrame;

		public TextMeshProUGUI PaseliText;

		public GameObject CompensationFrame;
	}

	[SerializeField]
	private GachaPlayFlow m_gachaPlayFlow;

	[SerializeField]
	private GameObject m_tenkeyWindow;

	[SerializeField]
	private List<GachaButtonFramework> m_gachaButtons = new List<GachaButtonFramework>();

	[SerializeField]
	private GameObject m_ticketNum;

	[SerializeField]
	private Transform m_consumeWindowParent;

	private GachaModePriceDisp m_gachaPriceDisp = new GachaModePriceDisp();

	public bool IsRetryButtonPushed;

	private GachaSeriesServerInfo info;

	public void AllRetryButton_Unable()
	{
		foreach (Button item in m_gachaButtons.Select((GachaButtonFramework x) => x.Bt))
		{
			item.interactable = false;
		}
	}

	public void AllRetryButton_Unactive()
	{
		foreach (Button item in m_gachaButtons.Select((GachaButtonFramework x) => x.Bt))
		{
			item.gameObject.SetActive(value: false);
		}
	}

	public void RetryButtonSetup(GachaPlayMode mode, GachaSeriesServerInfo gachaSeriesInfo)
	{
		info = gachaSeriesInfo;
		GachaButtonFramework gachaButtonFramework = m_gachaButtons.Find((GachaButtonFramework x) => x.Mode == mode);
		if (GachaPlayUtility.IsConsumeGacha(mode))
		{
			m_ticketNum.SetActive(value: false);
			m_gachaPriceDisp.Set((GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Music) ? GAME_MODE.MUSICGACHA : ((mode == GachaPlayMode.Five) ? GAME_MODE.GACHA5 : GAME_MODE.GACHA), gachaButtonFramework.CreditFrame, gachaButtonFramework.PaseliFrame, gachaButtonFramework.CompensationFrame, gachaButtonFramework.CreditText, gachaButtonFramework.PaseliText);
			gachaButtonFramework.Bt.interactable = !GameUtility.IsReservedGameOver && (Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha1) || !Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha5));
		}
		else
		{
			m_ticketNum.SetActive(value: true);
			gachaButtonFramework.Bt.interactable = !GameUtility.IsReservedGameOver && GameDataManager.PlayerItem.Count(ItemID.GachaTicket) >= GachaGameData.GetRequiredGachaTicket((mode == GachaPlayMode.TicketOne) ? 1 : 5);
			m_ticketNum.transform.Find("Num").GetComponent<TextMeshProUGUI>().text = $"{GameDataManager.PlayerItem.Count(ItemID.GachaTicket)}";
			gachaButtonFramework.Bt.transform.Find("NeedTicket").Find("NeedNum").GetComponent<TextMeshProUGUI>()
				.text = $"{GachaGameData.GetRequiredGachaTicket((mode == GachaPlayMode.TicketOne) ? 1 : 5)}";
		}
		gachaButtonFramework.Bt.gameObject.SetActive(value: true);
		gachaButtonFramework.Bt.OnClickAsObservable().Subscribe(async delegate
		{
			IsRetryButtonPushed = true;
			AllRetryButton_Unable();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			if (GachaPlayUtility.IsTicketGacha(mode))
			{
				if (await RequestGachaDraw(info, (mode != GachaPlayMode.TicketFive) ? 1 : 5, isTicket: true))
				{
					GameDataManager.Gacha.SelectedMode = mode;
					GameDataManager.Gacha.SelectedSeriesInfo = info;
					GotoNextScene("GachaPlay");
				}
				else
				{
					GotoNextScene("GachaSelect");
				}
			}
			else
			{
				ResidentFlow.GetInstance().GetTimer().SetTimer(20f, disp: true);
				ResidentFlow.GetInstance().GetTimer().StartTimer();
				bool isFive = mode == GachaPlayMode.Five;
				if (await RequestGachaDraw(info, (!isFive) ? 1 : 5, isTicket: false))
				{
					if (GameUtility.IsKonastaOrNetcafeBehavior && (GameDataManager.Player.IsPro() || GameUtility.IsProCardId(GameDataManager.Account.CardID) || Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha1)))
					{
						OnEnd(is_success: true, (mode == GachaPlayMode.Five) ? CONSUME_MODE.GACHA5_CREDIT : CONSUME_MODE.GACHA_CREDIT, ConsumeWindow.EndType.Success);
					}
					else
					{
						DialogManager.Instance.CreateConsumeDialog(m_consumeWindowParent).Set("", isFive ? GAME_TYPE.GACHA5 : GAME_TYPE.GACHA, PRICE_TYPE.PRICE_1, 1, info.GachaSeriesName, GachaPlayUtility.ToString(mode), OnEnd, m_tenkeyWindow, resetTimer: false).Forget();
					}
				}
				else
				{
					GotoNextScene("GachaSelect");
				}
			}
		}).AddTo(this);
		gachaButtonFramework.Bt.animator.Update(0f);
	}

	private async UniTask<bool> RequestGachaDraw(GachaSeriesServerInfo gachaSeriesInfo, int playCount, bool isTicket)
	{
		try
		{
			CutinGachaPlayDraw.ResultData resultData = await RequestApiBase<CutinGachaPlayDraw, CutinGachaPlayDraw.ResultData, (int, int, bool)>.Send((gachaSeriesInfo.SeriesID, playCount, isTicket));
			if (resultData.GachaResult.IsSuccess)
			{
				GameDataManager.Gacha.GachaResult = new GachaResultInfo(resultData);
				return true;
			}
			return false;
		}
		catch (NetworkResponseException)
		{
			ErrorData.GetInstance().ExtraMessage = $"「{playCount}回ガチャ」が実行できませんでした。";
			ErrorControl.SetError(ERROR.CONNECTION, 0u, ERROR_LEVEL.ERROR_STOP);
		}
		return false;
	}

	public void RetryUnableTask(GachaPlayMode mode)
	{
		AllRetryButton_Unable();
		try
		{
			m_gachaButtons.Find((GachaButtonFramework x) => x.Mode == mode).Bt.gameObject.SetActive(value: true);
		}
		catch
		{
		}
	}

	private void OnEnd(bool is_success, CONSUME_MODE mode, ConsumeWindow.EndType endType)
	{
		if (is_success)
		{
			GameDataManager.Gacha.SelectedSeriesInfo = info;
			GameDataManager.Gacha.ConsumeMode = mode;
			switch (mode)
			{
			case CONSUME_MODE.GACHA_CREDIT:
			case CONSUME_MODE.GACHA_PASELI:
				CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.Gacha);
				CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.Gacha1);
				if (GameUtility.IsKonastaOrNetcafeBehavior)
				{
					Cloudstrap.UserEvent.Begin(Cloudstrap.UserEventType.Gacha1);
				}
				GameDataManager.Gacha.SelectedMode = GachaPlayMode.One;
				break;
			case CONSUME_MODE.GACHA5_CREDIT:
			case CONSUME_MODE.GACHA5_PASELI:
				GameDataManager.Gacha.SelectedMode = GachaPlayMode.Five;
				CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.Gacha, 5);
				CabinetManager.Book.AddPlayCount(BookKeepingSettings.Variable.Gacha5);
				if (GameUtility.IsKonastaOrNetcafeBehavior)
				{
					Cloudstrap.UserEvent.Begin(Cloudstrap.UserEventType.Gacha5);
				}
				break;
			case CONSUME_MODE.GACHA_TICKET_EX:
				GameDataManager.Gacha.SelectedMode = GachaPlayMode.One;
				break;
			case CONSUME_MODE.GACHA5_TICKET_EX:
				GameDataManager.Gacha.SelectedMode = GachaPlayMode.Five;
				break;
			}
			GotoNextScene("GachaPlay");
		}
		else
		{
			GotoNextScene("GachaSelect");
		}
	}

	private void Update()
	{
		if (IsRetryButtonPushed && ResidentFlow.GetInstance().GetTimer().IsEnd())
		{
			IsRetryButtonPushed = false;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			GotoNextScene("GachaSelect");
		}
	}

	private void GotoNextScene(string scene)
	{
		if (m_gachaPlayFlow != null)
		{
			m_gachaPlayFlow.NextSceneLoad(scene);
		}
	}
}

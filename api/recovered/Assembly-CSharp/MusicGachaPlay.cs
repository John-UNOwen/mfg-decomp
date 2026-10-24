using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using Konami.System.Net;
using MFG.Assets;
using MFG.GameData;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MusicGachaPlay : MonoBehaviour
{
	[SerializeField]
	private GameObject m_window;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Animator m_janponAnim;

	[SerializeField]
	private TextMeshProUGUI m_itemText;

	[SerializeField]
	private TextMeshProUGUI m_itemNoText;

	[SerializeField]
	private Image m_itemImage;

	[SerializeField]
	private Button m_soundPlayButton;

	private bool isOpen;

	private bool isReachSong;

	private bool isSaveing;

	private string gainItemOID = "";

	private GachaSeriesServerInfo currentInfo;

	private CancellationTokenSource cancel;

	private CancellationToken token;

	public void Start()
	{
		if (m_closeButton != null)
		{
			m_closeButton.OnClickAsObservable().Subscribe(async delegate
			{
				await Close();
			}).AddTo(this);
		}
		if (m_soundPlayButton != null)
		{
			m_soundPlayButton.OnClickAsObservable().Subscribe(delegate
			{
				SoundPlay().Forget();
			}).AddTo(this);
		}
	}

	private async UniTask SaveAndSendLog(int seriesID, int itemNo)
	{
		isSaveing = true;
		try
		{
			await GameDataManager.Customize.Save();
			await GameDataManager.Player.Save();
			await GameDataManager.PlayerItem.Save();
			await GameDataManager.ExecLoadAtGacha();
			await RequestApiBase<GachaLogSend, GachaLogSend.ResultData, (int, string, List<int>, List<string>, List<CharaType>, UnableCharaGachaSelectWindow.State, int, int)>.Send((seriesID, "MusicOne", new List<int> { itemNo }, new List<string> { gainItemOID }, new List<CharaType>(), UnableCharaGachaSelectWindow.State.None, 0, 0), enableErrorScreenTransition: false);
			GameUtility.SendPpEvent(CONSUME_MODE.MUSIC_GACHA_PASELI);
			GameDataManager.Customize.TotalMusicGachaCount++;
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				Cloudstrap.UserEvent.End(Cloudstrap.UserEventType.MusicGacha);
			}
			if (!GameUtility.IsCloudStreaming)
			{
				await GameUtility.SendConsumeLog(CONSUME_MODE.MUSIC_GACHA_PASELI);
			}
		}
		catch (NetworkResponseException)
		{
			ErrorData.GetInstance().ExtraMessage = "「リーチソング＆ボイスガチャ」が実行できませんでした。";
			ErrorControl.SetError(ERROR.CONNECTION, 0u, ERROR_LEVEL.ERROR_STOP);
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + " OnInitialize: OperationCanceledException Catched.");
		}
		isSaveing = false;
	}

	private async UniTask<bool> LotteryPlay(int seriesID, ulong requestID)
	{
		_ = 1;
		try
		{
			gainItemOID = (await RequestApiBase<MusicGachaPlayRequest, MusicGachaPlayRequest.ResultData, ulong>.Send(requestID)).GachaResult.GainItems[0];
			isReachSong = !ItemUtility.IsVoiceItem(gainItemOID);
			ItemUtility.AddItem(gainItemOID);
			GameDataManager.PlayerItem.Add(MFG.Types.PlayerItem.ItemID.Present01, 2);
			int itemNo = ((!isReachSong) ? VoiceMaster.GetItem(gainItemOID).GachaItemNo : ItemIDExtentions.MusicGachaItemNo(gainItemOID));
			SaveAndSendLog(seriesID, itemNo).Forget();
			await SetResult(gainItemOID, itemNo);
		}
		catch (NetworkResponseException)
		{
			ErrorData.GetInstance().ExtraMessage = "「リーチソング＆ボイスガチャ」が実行できませんでした。";
			ErrorControl.SetError(ERROR.CONNECTION, 0u, ERROR_LEVEL.ERROR_STOP);
			return false;
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + " OnInitialize: OperationCanceledException Catched.");
		}
		return true;
	}

	private async UniTask SetResult(string oid, int itemNo)
	{
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(ItemUtility.ItemIconGraphicPath(oid));
		disposableAsset.AddTo(this);
		m_itemImage.sprite = disposableAsset.Asset;
		m_itemText.text = ItemUtility.ItemName(oid) ?? "";
		m_itemNoText.text = $"No.{itemNo}";
	}

	public async UniTask Open(GachaSeriesServerInfo gachaInfo, ulong reqestID)
	{
		if (isOpen)
		{
			return;
		}
		currentInfo = gachaInfo;
		isOpen = true;
		isReachSong = false;
		gainItemOID = "";
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		m_closeButton.gameObject.SetActive(value: false);
		m_soundPlayButton.interactable = false;
		m_window.SetActive(value: true);
		try
		{
			if (GameUtility.IsCloudStreaming && !Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.MusicGacha))
			{
				await GameUtility.SendConsumeLog(CONSUME_MODE.MUSIC_GACHA_PASELI);
			}
		}
		catch
		{
		}
		if (await LotteryPlay(currentInfo.SeriesID, reqestID))
		{
			if (!GameDataManager.Player.IsOdekake)
			{
				GameDataManager.PlayerConfig.IsMyRoomSceneVisited = false;
			}
			m_window.GetComponent<Animator>().SetTrigger("In");
			m_janponAnim.SetTrigger("In");
			SoundManager.PlaySe("SFX_SYS_GACYA13").Forget();
			await UniTask.WaitUntil(() => m_janponAnim.GetCurrentAnimatorStateInfo(0).IsName("JanPonPlayLoop"));
			SoundPlay().Forget();
			m_soundPlayButton.interactable = true;
			m_closeButton.gameObject.SetActive(value: true);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(10.0)), UniTask.WaitUntil(() => !isOpen));
		}
		if (isOpen)
		{
			await Close();
		}
	}

	public async UniTask Close(bool isPlaySe = true)
	{
		if (isOpen)
		{
			BgmCancelToken();
			SoundManager.PlayMusicGachaBGM(currentInfo).Forget();
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			await UniTask.WaitWhile(() => isSaveing);
			if (m_window != null)
			{
				m_window.GetComponent<Animator>().SetTrigger("Out");
				m_window.SetActive(value: false);
			}
			isOpen = false;
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
	}

	private async UniTask SoundPlay()
	{
		if (string.IsNullOrEmpty(gainItemOID))
		{
			return;
		}
		if (isReachSong)
		{
			BgmCancelToken(isNullCheck: true);
			MFG.Types.Customize.CustomizeItem.ItemID itemID = CustomizeItemMaster.GetItem(gainItemOID).ItemID;
			if (itemID != 0)
			{
				SoundManager.StopBgm().Forget();
				SoundManager.PlayBgm(itemID.ReachBgmToThumbBgmLabel()).Forget();
				try
				{
					await UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
					SoundManager.PlayMyCharaMyRoomBGM().Forget();
				}
				catch (OperationCanceledException)
				{
					Debug.Log("曲preview変更");
				}
			}
		}
		else
		{
			SoundManager.PlayVoice(SoundManager.GetCharaVoiceLabel(VoiceMaster.GetItem(gainItemOID).Label, VoiceMaster.GetCharaType(gainItemOID))).Forget();
		}
	}

	private void BgmCancelToken(bool isNullCheck = false)
	{
		if (!isNullCheck || cancel != null)
		{
			cancel?.Cancel();
			cancel = new CancellationTokenSource();
			token = cancel.Token;
		}
	}
}

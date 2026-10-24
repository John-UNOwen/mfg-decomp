using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.System.Net;
using MFG.Assets;
using MFG.GameData;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DojoFlow : BaseFlow
{
	public enum SlotStatus
	{
		Locked,
		NotSet,
		Lesson,
		Finish
	}

	public enum AssetType
	{
		Lesson01,
		Lesson02,
		Rest01,
		Rest02,
		FightSpirit
	}

	[Serializable]
	private class CharaSlot
	{
		[SerializeField]
		private GameObject slotObj;

		[SerializeField]
		private Button charaChangeButton;

		[SerializeField]
		private Button cropButton;

		[SerializeField]
		private int charaListPosX;

		[SerializeField]
		public TextMeshProUGUI debugText;

		private DojoFlow m_dojoFlow;

		private int m_slot;

		public SlotStatus m_status;

		public CharaType m_charaType;

		public int m_spiritStockNum;

		public DateTime m_time;

		public async UniTask Init(DojoFlow dojoFlow, int slot, SlotStatus status, DateTime time, CharaType charaType = CharaType.Chara01, int spiritStockNum = 0)
		{
			if (debugText != null)
			{
				debugText.gameObject.SetActive(value: false);
				debugText = null;
			}
			m_dojoFlow = dojoFlow;
			m_slot = slot;
			m_status = status;
			m_charaType = charaType;
			m_spiritStockNum = spiritStockNum;
			m_time = time;
			await Reflesh();
			charaChangeButton.OnClickAsObservable().Subscribe(async delegate
			{
				if (!m_dojoFlow.m_charaSelectWindow.IsOpen)
				{
					_ = m_dojoFlow.m_charaSlots[slot];
					SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
					Dictionary<CharaType, bool> dictionary = new Dictionary<CharaType, bool>();
					foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
					{
						dictionary.Add(value, value: true);
					}
					await m_dojoFlow.m_charaSelectWindow.Play(m_charaType, dispoffWhenDecided: true, dictionary);
					CharaType selectedCharaType = m_dojoFlow.m_charaSelectWindow.GetSelectingCharaType();
					if (m_charaType != selectedCharaType || m_status == SlotStatus.NotSet)
					{
						if (m_status != SlotStatus.NotSet)
						{
							ItemID itemID2 = PlayerItemMaster.ToFightSpiritItemID(m_charaType);
							GameDataManager.PlayerItem.Add(itemID2, m_spiritStockNum);
						}
						List<DojoSetSlot.TimeSavingItem> item = null;
						try
						{
							DojoSetSlot.ResultData resultData2 = await RequestApiBase<DojoSetSlot, DojoSetSlot.ResultData, (int, CharaType, bool, List<DojoSetSlot.TimeSavingItem>)>.Send((m_slot, selectedCharaType, true, item), enableErrorScreenTransition: false);
							_ = GameDataManager.Player.CharaData[(int)selectedCharaType];
							m_status = SlotStatus.Lesson;
							m_spiritStockNum = 0;
							m_time = resultData2.SlotInfo.NextTime;
							GameDataManager.Mission.LessonCount++;
							m_charaType = selectedCharaType;
							Reflesh().Forget();
							foreach (CharaSlot charaSlot in m_dojoFlow.m_charaSlots)
							{
								charaSlot.RefleshFightSpritNum();
							}
						}
						catch (NetworkResponseException)
						{
						}
					}
				}
			}).AddTo(dojoFlow);
			cropButton.OnClickAsObservable().Subscribe(async delegate
			{
				cropButton.interactable = false;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				try
				{
					DojoGainSoul.ResultData resultData = await RequestApiBase<DojoGainSoul, DojoGainSoul.ResultData, int>.Send(m_slot, enableErrorScreenTransition: false);
					Transform transform = slotObj.transform.Find("Unlock").Find("Display").Find("SpiritStock");
					switch (m_spiritStockNum)
					{
					case 1:
						transform.Find("Slot1").GetComponent<Animator>().SetTrigger("Crop");
						transform.Find("Slot2").GetComponent<Animator>().SetTrigger("Wait");
						transform.Find("Slot3").GetComponent<Animator>().SetTrigger("Wait");
						break;
					case 2:
						transform.Find("Slot1").GetComponent<Animator>().SetTrigger("Crop");
						transform.Find("Slot2").GetComponent<Animator>().SetTrigger("Crop");
						transform.Find("Slot3").GetComponent<Animator>().SetTrigger("Wait");
						break;
					case 3:
						transform.Find("Slot1").GetComponent<Animator>().SetTrigger("Crop");
						transform.Find("Slot2").GetComponent<Animator>().SetTrigger("Crop");
						transform.Find("Slot3").GetComponent<Animator>().SetTrigger("Crop");
						break;
					}
					ItemID itemID = PlayerItemMaster.ToFightSpiritItemID(m_charaType);
					GameDataManager.PlayerItem.Add(itemID, m_spiritStockNum);
					m_spiritStockNum = 0;
					m_time = resultData.SlotInfo.NextTime;
					m_status = SlotStatus.Lesson;
					await UniTask.Delay(TimeSpan.FromSeconds(1.0));
					await Reflesh();
					foreach (CharaSlot charaSlot2 in m_dojoFlow.m_charaSlots)
					{
						charaSlot2.RefleshFightSpritNum();
					}
				}
				catch (NetworkResponseException)
				{
				}
			}).AddTo(dojoFlow);
		}

		private void RefleshFightSpritNum()
		{
			ItemID itemID = PlayerItemMaster.ToFightSpiritItemID(m_charaType);
			slotObj.transform.Find("Unlock").Find("SpiritNumber").Find("SpiritNumText")
				.GetComponent<TextMeshProUGUI>()
				.text = GameDataManager.PlayerItem.Count(itemID).ToString();
		}

		public async UniTask Reflesh()
		{
			if (m_status == SlotStatus.Locked)
			{
				slotObj.transform.Find("Lock").gameObject.SetActive(value: true);
				slotObj.transform.Find("Unlock").gameObject.SetActive(value: false);
				return;
			}
			slotObj.transform.Find("Unlock").gameObject.SetActive(value: false);
			foreach (Transform item in slotObj.transform.Find("Unlock").Find("CharaState"))
			{
				item.gameObject.SetActive(value: false);
			}
			switch (m_status)
			{
			case SlotStatus.NotSet:
				slotObj.transform.Find("Unlock").Find("CharaState").Find("NonSet")
					.gameObject.SetActive(value: true);
				slotObj.transform.Find("Unlock").Find("Display").gameObject.SetActive(value: false);
				slotObj.transform.Find("Unlock").Find("SpiritNumber").gameObject.SetActive(value: false);
				break;
			case SlotStatus.Lesson:
			{
				slotObj.transform.Find("Unlock").Find("CharaState").Find("Lesson")
					.gameObject.SetActive(value: true);
				slotObj.transform.Find("Unlock").Find("Display").gameObject.SetActive(value: true);
				slotObj.transform.Find("Unlock").Find("SpiritNumber").gameObject.SetActive(value: true);
				IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Dojo, m_charaType, AssetType.Lesson01));
				disposableAsset2.AddTo(m_dojoFlow);
				slotObj.transform.Find("Unlock").Find("CharaState").Find("Lesson")
					.Find("Chara")
					.Find("CharaUp")
					.Find("Image")
					.GetComponent<Image>()
					.sprite = disposableAsset2.Asset;
				disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Dojo, m_charaType, AssetType.Lesson02));
				disposableAsset2.AddTo(m_dojoFlow);
				slotObj.transform.Find("Unlock").Find("CharaState").Find("Lesson")
					.Find("Chara")
					.Find("CharaDown")
					.Find("Image")
					.GetComponent<Image>()
					.sprite = disposableAsset2.Asset;
				break;
			}
			case SlotStatus.Finish:
			{
				slotObj.transform.Find("Unlock").Find("CharaState").Find("Rest")
					.gameObject.SetActive(value: true);
				slotObj.transform.Find("Unlock").Find("Display").gameObject.SetActive(value: true);
				slotObj.transform.Find("Unlock").Find("SpiritNumber").gameObject.SetActive(value: true);
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Dojo, m_charaType, AssetType.Rest01));
				disposableAsset.AddTo(m_dojoFlow);
				slotObj.transform.Find("Unlock").Find("CharaState").Find("Rest")
					.Find("Chara")
					.Find("Anime01")
					.Find("Image")
					.GetComponent<Image>()
					.sprite = disposableAsset.Asset;
				disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Dojo, m_charaType, AssetType.Rest02));
				disposableAsset.AddTo(m_dojoFlow);
				slotObj.transform.Find("Unlock").Find("CharaState").Find("Rest")
					.Find("Chara")
					.Find("Anime02")
					.Find("Image")
					.GetComponent<Image>()
					.sprite = disposableAsset.Asset;
				break;
			}
			}
			Transform SpritStock = slotObj.transform.Find("Unlock").Find("Display").Find("SpiritStock");
			if (m_status != SlotStatus.NotSet)
			{
				IDisposableAsset<Sprite> disposableAsset3 = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Dojo, m_charaType, AssetType.FightSpirit));
				disposableAsset3.AddTo(m_dojoFlow);
				SpritStock.Find("Slot1").Find("Spirit").GetComponent<Image>()
					.sprite = disposableAsset3.Asset;
				SpritStock.Find("Slot2").Find("Spirit").GetComponent<Image>()
					.sprite = disposableAsset3.Asset;
				SpritStock.Find("Slot3").Find("Spirit").GetComponent<Image>()
					.sprite = disposableAsset3.Asset;
				slotObj.transform.Find("Unlock").Find("SpiritNumber").GetComponent<Image>()
					.sprite = disposableAsset3.Asset;
			}
			slotObj.transform.Find("Lock").gameObject.SetActive(value: false);
			slotObj.transform.Find("Unlock").gameObject.SetActive(value: true);
			if (m_status == SlotStatus.Lesson && GameDataManager.SpiritGymBonusEventEnable())
			{
				int spiritGymBonusRatio = GameDataManager.GetSpiritGymBonusRatio();
				slotObj.transform.Find("Unlock").Find("CharaState").Find("Lesson")
					.GetComponent<Animator>()
					.SetFloat("Speed", spiritGymBonusRatio);
			}
			if (m_status == SlotStatus.Lesson || m_status == SlotStatus.Finish)
			{
				RefleshFightSpritNum();
				TimeSpan timeSpan = m_time - DateTime.Now + TimeSpan.FromSeconds(59.0);
				if (timeSpan.TotalHours >= 24.0)
				{
					timeSpan = new TimeSpan(23, 59, 0);
				}
				else if (timeSpan.Seconds < 0)
				{
					timeSpan = new TimeSpan(0, 0, 0);
				}
				string text = timeSpan.Hours.ToString("00") + ":" + timeSpan.Minutes.ToString("00");
				slotObj.transform.Find("Unlock").Find("Display").Find("Timer")
					.Find("Base")
					.Find("Number")
					.GetComponent<TextMeshProUGUI>()
					.text = text;
				switch (m_spiritStockNum)
				{
				case 0:
					SpritStock.Find("Slot1").GetComponent<Animator>().SetTrigger("Generation");
					SpritStock.Find("Slot2").GetComponent<Animator>().SetTrigger("Wait");
					SpritStock.Find("Slot3").GetComponent<Animator>().SetTrigger("Wait");
					break;
				case 1:
					SpritStock.Find("Slot1").GetComponent<Animator>().SetTrigger("Loop");
					SpritStock.Find("Slot2").GetComponent<Animator>().SetTrigger("Generation");
					SpritStock.Find("Slot3").GetComponent<Animator>().SetTrigger("Wait");
					break;
				case 2:
					SpritStock.Find("Slot1").GetComponent<Animator>().SetTrigger("Loop");
					SpritStock.Find("Slot2").GetComponent<Animator>().SetTrigger("Loop");
					SpritStock.Find("Slot3").GetComponent<Animator>().SetTrigger("Generation");
					break;
				case 3:
					SpritStock.Find("Slot1").GetComponent<Animator>().SetTrigger("Loop");
					SpritStock.Find("Slot2").GetComponent<Animator>().SetTrigger("Loop");
					SpritStock.Find("Slot3").GetComponent<Animator>().SetTrigger("Loop");
					break;
				}
				cropButton.interactable = m_spiritStockNum > 0;
			}
		}
	}

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private List<CharaSlot> m_charaSlots = new List<CharaSlot>();

	[SerializeField]
	private CharaSelectWindow m_charaSelectWindow;

	[SerializeField]
	private Button CloseButton;

	[SerializeField]
	private Button HelpButton;

	[SerializeField]
	private Button HelpCloseButton;

	[SerializeField]
	private GameObject HelpWindow;

	[SerializeField]
	private GameObject FlagLeft;

	[SerializeField]
	private GameObject FlagRight;

	[SerializeField]
	private Color FlagColor2X;

	[SerializeField]
	private Color FlagColor3X;

	public static readonly int DojoSlotNum = 4;

	public static readonly int DojoSpiritStockMax = 3;

	private bool m_isLessonEndCharaVoicePlay;

	private CharaType m_LessonEndChara;

	private DojoGetStatus.ResultData m_statusResult;

	[SerializeField]
	private PlayerGameData m_playerGameData => GameDataManager.Player;

	[SerializeField]
	private AccountGameData m_accountGameData => GameDataManager.Account;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			m_statusResult = await RequestApiBase<DojoGetStatus, DojoGetStatus.ResultData>.Send(enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
			if (GameDataManager.SpiritGymBonusEventEnable())
			{
				FlagLeft.SetActive(value: true);
				FlagRight.SetActive(value: true);
				switch (GameDataManager.GetSpiritGymBonusRatio())
				{
				case 2:
					FlagLeft.transform.Find("Flag").GetComponent<Image>().color = FlagColor2X;
					FlagRight.transform.Find("Flag").GetComponent<Image>().color = FlagColor2X;
					FlagLeft.transform.Find("Flag").Find("NumberText").GetComponent<TextMeshProUGUI>()
						.text = "2";
					FlagRight.transform.Find("Flag").Find("NumberText").GetComponent<TextMeshProUGUI>()
						.text = "2";
					break;
				case 3:
					FlagLeft.transform.Find("Flag").GetComponent<Image>().color = FlagColor3X;
					FlagRight.transform.Find("Flag").GetComponent<Image>().color = FlagColor3X;
					FlagLeft.transform.Find("Flag").Find("NumberText").GetComponent<TextMeshProUGUI>()
						.text = "3";
					FlagRight.transform.Find("Flag").Find("NumberText").GetComponent<TextMeshProUGUI>()
						.text = "3";
					break;
				}
			}
			else
			{
				FlagLeft.SetActive(value: false);
				FlagRight.SetActive(value: false);
			}
			GameDataManager.Player.DojoUnlock();
			HelpButton.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				HelpWindow.SetActive(value: true);
				HelpWindow.GetComponent<Animator>().SetTrigger("In");
			}).AddTo(this);
			HelpCloseButton.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				HelpWindow.GetComponent<Animator>().SetTrigger("Out");
			}).AddTo(this);
			CloseButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				await GotoNextScene(cancellationToken);
			}).AddTo(this);
			base.OnTimerEnd.First().Subscribe(async delegate
			{
				await GotoNextScene(cancellationToken);
			}).AddTo(this);
			ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
			ResidentFlow.GetInstance().GetHeader().SetStatusDisp(disp: false);
			ResidentFlow.GetInstance().GetHeader().SetSceneName("スピリットジム");
			SoundManager.PlayMyCharaMyRoomBGM().Forget();
			m_charaSelectWindow.Init(cancellationToken).Forget();
			await Slot_Init();
		}
		catch (NetworkResponseException)
		{
			await GotoNextScene(cancellationToken);
		}
		catch (OperationCanceledException ex2)
		{
			Debug.Log("OperationCanceledException Catched. " + ex2.Message);
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		if (_isNextSceneDecided)
		{
			return;
		}
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			await ResidentFlow.GetInstance().TransitionEffectClose();
			if (SystemConfigGameData.IsDualMonitor)
			{
				SubMonitorFlow.GetInstance().TransitionAnimClose();
			}
			ResidentFlow.GetInstance().GetTimer().SetTimer(60f, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			if (m_isLessonEndCharaVoicePlay)
			{
				SoundManager.PlayVoice("VOX_GXX_LESSON", m_LessonEndChara).Forget();
			}
			if (!GameDataManager.PlayerConfig.IsDispHelpFirstPlayDojo && !DebugFlag.IsActive(DebugFlag.Flag.SkipGuideDisp))
			{
				GameDataManager.PlayerConfig.IsDispHelpFirstPlayDojo = true;
				HelpWindow.SetActive(value: true);
				HelpWindow.GetComponent<Animator>().SetTrigger("In");
			}
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	private async UniTask Slot_Init()
	{
		for (int i = 0; i < DojoSlotNum; i++)
		{
			if (GameDataManager.Player.m_dojoSlotUnlocked[i])
			{
				DojoGetStatus.SlotInfo slotInfo = m_statusResult.SlotInfos[i];
				if (!slotInfo.IsAvailable)
				{
					await m_charaSlots[i].Init(this, i, SlotStatus.NotSet, DateTime.Now);
				}
				else if (slotInfo.SpiritStockCount == DojoSpiritStockMax)
				{
					await m_charaSlots[i].Init(this, i, SlotStatus.Finish, DateTime.Now, slotInfo.CharaType, slotInfo.SpiritStockCount);
					if (!m_isLessonEndCharaVoicePlay)
					{
						m_LessonEndChara = slotInfo.CharaType;
						m_isLessonEndCharaVoicePlay = true;
					}
				}
				else
				{
					DateTime nextTime = slotInfo.NextTime;
					int spiritStockCount = slotInfo.SpiritStockCount;
					await m_charaSlots[i].Init(this, i, SlotStatus.Lesson, nextTime, slotInfo.CharaType, spiritStockCount);
				}
			}
			else
			{
				await m_charaSlots[i].Init(this, i, SlotStatus.Locked, DateTime.Now);
			}
		}
	}

	private async UniTask GotoNextScene(CancellationToken cancellationToken)
	{
		if (!_isNextSceneDecided)
		{
			_isNextSceneDecided = true;
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			_canvas.GetComponent<GraphicRaycaster>().enabled = false;
			try
			{
				await GameDataManager.PlayerItem.Save(cancellationToken);
			}
			catch (OperationCanceledException ex)
			{
				Debug.Log("OperationCanceledException Catched. " + ex.Message);
			}
			catch (NetworkResponseException ex2)
			{
				GameDataManager.PlayerConfig.IsDataSaveFailed = true;
				Debug.Log("NetworkResponseException Catched. " + ex2.Message);
			}
			GotoNextScene("Home", enableDecidedCheck: false);
		}
	}
}

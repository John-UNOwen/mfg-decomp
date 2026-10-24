using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SubMonitorDojoFlow : BaseFlow
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
		private int charaListPosX;

		private SubMonitorDojoFlow m_dojoFlow;

		private int m_slot;

		public SlotStatus m_status;

		public CharaType m_charaType;

		public int m_spiritStockNum;

		public DateTime m_time;

		public async UniTask Init(SubMonitorDojoFlow dojoFlow, int slot, SlotStatus status, DateTime time, CharaType charaType = CharaType.Chara01, int spiritStockNum = 0)
		{
			m_dojoFlow = dojoFlow;
			m_slot = slot;
			m_status = status;
			m_charaType = charaType;
			m_spiritStockNum = spiritStockNum;
			m_time = time;
			if (SystemConfigGameData.IsSXGAMonitor)
			{
				slotObj.transform.GetComponent<RectTransform>().localPosition = new Vector3(-450 + m_slot * 300, 0f, 0f);
				slotObj.transform.GetComponent<RectTransform>().localScale = new Vector3(0.9f, 0.9f, 0.9f);
			}
			await Reflesh();
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
			}
		}
	}

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private List<CharaSlot> m_charaSlots = new List<CharaSlot>();

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

	private static SubMonitorDojoFlow m_instance = null;

	private DojoGetStatus.ResultData m_statusResult;

	public static SubMonitorDojoFlow GetInstance()
	{
		if (!m_instance)
		{
			m_instance = GameObject.Find("SubMonitorDojoFlow").GetComponent<SubMonitorDojoFlow>();
		}
		return m_instance;
	}

	public async UniTask Init(DojoGetStatus.ResultData statusResult)
	{
		try
		{
			m_statusResult = statusResult;
			if (SystemConfigGameData.IsSXGAMonitor)
			{
				_canvas.transform.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280f, 1024f);
			}
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
			await Slot_Init();
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	private async UniTask Slot_Init()
	{
		List<UniTask> list = new List<UniTask>();
		for (int i = 0; i < DojoSlotNum; i++)
		{
			if (GameDataManager.Player.m_dojoSlotUnlocked[i])
			{
				DojoGetStatus.SlotInfo slotInfo = m_statusResult.SlotInfos[i];
				if (!slotInfo.IsAvailable)
				{
					list.Add(m_charaSlots[i].Init(this, i, SlotStatus.NotSet, DateTime.Now));
				}
				else if (slotInfo.SpiritStockCount == DojoSpiritStockMax)
				{
					list.Add(m_charaSlots[i].Init(this, i, SlotStatus.Finish, DateTime.Now, slotInfo.CharaType, slotInfo.SpiritStockCount));
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
					list.Add(m_charaSlots[i].Init(this, i, SlotStatus.Lesson, nextTime, slotInfo.CharaType, spiritStockCount));
				}
			}
			else
			{
				list.Add(m_charaSlots[i].Init(this, i, SlotStatus.Locked, DateTime.Now));
			}
		}
		await list;
	}
}

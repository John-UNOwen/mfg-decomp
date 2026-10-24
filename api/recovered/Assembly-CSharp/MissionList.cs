using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Mission;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MissionList : MonoBehaviour
{
	[SerializeField]
	private Slider m_slider;

	[SerializeField]
	private TextMeshProUGUI m_progressCounter;

	[SerializeField]
	private Image m_charaNamePlate;

	[SerializeField]
	private TextMeshProUGUI m_charaName;

	[SerializeField]
	private TextMeshProUGUI m_missionText;

	[SerializeField]
	private Image m_itemImage;

	[SerializeField]
	private GameObject m_itemNormalParent;

	[SerializeField]
	private TextMeshProUGUI m_itemName;

	[SerializeField]
	private TextMeshProUGUI m_itemCount;

	[SerializeField]
	private TextMeshProUGUI m_remainingTime;

	[SerializeField]
	private GameObject m_itemJandriParent;

	[SerializeField]
	private TextMeshProUGUI m_itemNameJandri;

	[SerializeField]
	private TextMeshProUGUI m_itemCountJandri;

	public async UniTask Init(MissionProgressInfo info, int achievementIndex, ItemPopupManager itemPopupManager, bool isAcquisition = false, bool isFinish = false, string itemOID = "", int itemNum = -1)
	{
		Item master = info.Master;
		if (master.Chara >= CharaType.Chara01)
		{
			CharaProfile profile = MasterGameDataManager.Character.Properties(master.Chara).Profile;
			if (!isFinish)
			{
				m_charaNamePlate.color = profile.Color;
			}
			m_charaName.text = profile.ShortName;
			m_charaNamePlate.gameObject.SetActive(value: true);
		}
		else
		{
			m_charaNamePlate.gameObject.SetActive(value: false);
		}
		m_slider.gameObject.SetActive(value: false);
		if (master.IsProgressDisplay)
		{
			int num = master.AchievementCount(achievementIndex);
			if (num > 0)
			{
				m_slider.value = (info.IsClear(achievementIndex) ? 1f : ((float)info.Progress / (float)num));
				m_progressCounter.text = (info.IsClear(achievementIndex) ? $"{num}/{num}" : $"{info.Progress}/{num}");
				m_slider.gameObject.SetActive(value: true);
			}
		}
		m_missionText.text = master.MissionText(achievementIndex);
		m_remainingTime.text = (isAcquisition ? "" : master.RemainingTimeString());
		(string, int) tuple = master.AchievementItem(achievementIndex);
		string oid = tuple.Item1;
		int item = tuple.Item2;
		oid = ((!string.IsNullOrEmpty(itemOID)) ? itemOID : oid);
		item = ((itemNum != -1) ? itemNum : item);
		bool flag = master.IsJandriRewardEnable && (!isAcquisition || GameDataManager.Mission.IsMissionProgressBoost);
		m_itemJandriParent.SetActive(flag);
		m_itemNormalParent.SetActive(!flag);
		if (flag)
		{
			m_itemCountJandri.text = $"×{item}";
			m_itemNameJandri.text = ItemUtility.DisplayNameInMission(oid, master.Chara) ?? "";
		}
		else
		{
			m_itemCount.text = $"×{item}";
			m_itemName.text = ItemUtility.DisplayNameInMission(oid, master.Chara) ?? "";
		}
		try
		{
			string path = ItemUtility.ItemIconGraphicPath(oid);
			if (await AssetLoader.Exists(path))
			{
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(path);
				disposableAsset.AddTo(this);
				m_itemImage.sprite = disposableAsset.Asset;
				m_itemImage.transform.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
				{
					RectTransform component = m_itemImage.transform.GetComponent<RectTransform>();
					await itemPopupManager.Open(oid, component.position, component.sizeDelta);
				})
					.AddTo(this);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
		}
		catch (Exception ex)
		{
			Debug.Log("MissionWindow連打" + ex.ToString());
		}
	}
}

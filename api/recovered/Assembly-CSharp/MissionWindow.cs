using System;
using System.Collections.Generic;
using System.Linq;
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

public class MissionWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollView m_scrollView;

	[SerializeField]
	private GameObject m_missionListPrefab;

	[SerializeField]
	private GameObject m_missionListFinishPrefab;

	[SerializeField]
	private GameObject m_missionTabParent;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Button m_charaChangeButton;

	[SerializeField]
	private Button m_charaChangeButtonNone;

	[SerializeField]
	private GameObject m_charaInfoPrefab;

	[SerializeField]
	private GameObject m_missionScrollView;

	[SerializeField]
	private MissionCharaSelectWindow m_missionCharaSelectWindow;

	[SerializeField]
	private ItemPopupManager _itemPopup;

	[SerializeField]
	private Sprite _tabOnSpr;

	[SerializeField]
	private Sprite _tabOffSpr;

	[SerializeField]
	private GameObject _tabPrefab;

	[SerializeField]
	private ItemPopupManager _itemPopupManager;

	private Dictionary<Category, GameObject> MissionTabDict = new Dictionary<Category, GameObject>();

	private Category OpenCategory = Category.Player;

	public bool isCharaChange;

	public bool isListMaking;

	[NonSerialized]
	public CharaType m_selectingChara;

	private bool m_listRefleshed = true;

	private Color _tabOnColor = new Color(1f, 1f, 1f);

	private Color _tabOffColor = new Color(0.7844f, 0.7844f, 0.7844f);

	private Color _tabNameOnColor = new Color(0.3f, 0.3f, 0.3f);

	private Color _tabNameOffColor = new Color(0.549f, 0.51f, 0.463f);

	[SerializeField]
	private PlayerGameData m_playerGameData => GameDataManager.Player;

	[SerializeField]
	private AccountGameData m_accountGameData => GameDataManager.Account;

	public bool isOpen { get; private set; }

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		try
		{
			this.GetCancellationTokenOnDestroy();
			isOpen = false;
			foreach (Transform item in m_missionTabParent.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			foreach (CategoryItem item2 in MissionCategoryMaster.GetActiveTab())
			{
				Category category = item2.Category;
				GameObject gameObject = UnityEngine.Object.Instantiate(_tabPrefab, m_missionTabParent.transform);
				gameObject.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = item2.Name;
				gameObject.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
				{
					if (OpenCategory != category)
					{
						SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
						ChangeTab(category).Forget();
					}
				})
					.AddTo(this);
				MissionTabDict.Add(category, gameObject);
			}
			TabReflesh(Category.Player);
			await m_missionCharaSelectWindow.Init();
			m_charaChangeButton.OnClickAsObservable().Subscribe(async delegate
			{
				m_missionScrollView.SetActive(value: false);
				m_charaInfoPrefab.SetActive(value: false);
				TabInteractable(isActive: false);
				await m_missionCharaSelectWindow.Play(m_selectingChara);
				if (isCharaChange)
				{
					await CreateMissionList(Category.Character, ToFilterType(m_selectingChara));
				}
				TabInteractable(isActive: true);
				m_missionScrollView.SetActive(value: true);
				m_charaInfoPrefab.SetActive(value: true);
			}).AddTo(this);
			m_charaChangeButtonNone.OnClickAsObservable().Subscribe(async delegate
			{
				m_missionScrollView.SetActive(value: false);
				m_charaInfoPrefab.SetActive(value: false);
				TabInteractable(isActive: false);
				await m_missionCharaSelectWindow.Play(m_selectingChara);
				if (isCharaChange)
				{
					await CreateMissionList(Category.Character, ToFilterType(m_selectingChara));
				}
				TabInteractable(isActive: true);
				m_missionScrollView.SetActive(value: true);
				m_charaInfoPrefab.SetActive(value: true);
			}).AddTo(this);
			m_closeButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				Close();
			}).AddTo(this);
			TabInteractable(isActive: true);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	public void TabInteractable(bool isActive)
	{
		foreach (CategoryItem item in MissionCategoryMaster.GetActiveTab())
		{
			Category category = item.Category;
			MissionTabDict[category].transform.GetComponent<Button>().interactable = isActive;
		}
		m_closeButton.interactable = isActive;
	}

	public async UniTask Play()
	{
		base.gameObject.SetActive(value: true);
		ChangeTab(Category.Player).Forget();
		isOpen = true;
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		isListMaking = false;
		isOpen = false;
	}

	private async UniTask CreateMissionList(Category category, int filterType = 0)
	{
		if (isListMaking)
		{
			return;
		}
		m_scrollView.ResetList();
		isListMaking = true;
		IEnumerable<Item> enumerable = from x in MissionMaster.GetItems(category, (CharaType)(filterType - 1))
			where x.IsEnable
			select x;
		List<(MissionProgressInfo, int)> list = new List<(MissionProgressInfo, int)>();
		List<(MissionProgressInfo, int)> list2 = new List<(MissionProgressInfo, int)>();
		foreach (Item master in enumerable)
		{
			MissionProgressInfo info = GameDataManager.Mission.m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == master.MissionID);
			if (info == null)
			{
				continue;
			}
			if ((master.DisplayType == DisplayType.All || DebugFlag.IsActive(DebugFlag.Flag.MissionAllDisp)) && master.LoopType != LoopType.Endless)
			{
				list.AddRange((from x in Enumerable.Range(0, master.AchievementList().Count)
					where info.IsClear(x)
					select (info: info, x: x)).ToList());
				list2.AddRange((from x in Enumerable.Range(0, master.AchievementList().Count)
					where !info.IsClear(x)
					select (info: info, x: x)).ToList());
			}
			else if (info.IsAllClear)
			{
				list.Add((info, master.AchievementList().Count));
			}
			else
			{
				list2.Add((info, info.ClearCount));
			}
		}
		foreach (var item in list2)
		{
			var (Info, achievementIndex) = item;
			if (Info.Master.RequiredMissionID != 0)
			{
				MissionProgressInfo missionProgressInfo = GameDataManager.Mission.m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == Info.Master.RequiredMissionID);
				if (missionProgressInfo != null && !missionProgressInfo.IsAllClear)
				{
					continue;
				}
			}
			m_scrollView.Add(m_missionListPrefab).transform.GetComponent<MissionList>().Init(Info, achievementIndex, _itemPopup).Forget();
		}
		foreach (var (missionProgressInfo2, achievementIndex2) in list)
		{
			if (missionProgressInfo2.Master.HideMissionID == MissionID.None || !missionProgressInfo2.IsAllClear)
			{
				m_scrollView.Add(m_missionListFinishPrefab).transform.GetComponent<MissionList>().Init(missionProgressInfo2, achievementIndex2, _itemPopup, isAcquisition: false, isFinish: true).Forget();
			}
		}
		Canvas.ForceUpdateCanvases();
		foreach (Transform item2 in m_scrollView.GetListParent().transform)
		{
			item2.Find("MissionText").GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
			item2.Find("MissionText").GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
		}
		if (category == Category.Character)
		{
			if (isCharaChange)
			{
				m_charaInfoPrefab.transform.Find("SdChara").gameObject.SetActive(value: true);
				m_charaInfoPrefab.transform.Find("BtCharaChange").gameObject.SetActive(value: true);
				m_charaInfoPrefab.transform.Find("BtCharaChangeNone").gameObject.SetActive(value: false);
				await CharaSdSetImage(m_selectingChara, m_charaInfoPrefab.transform.Find("SdChara").GetComponent<Image>(), isCharaChange);
			}
			else
			{
				m_charaInfoPrefab.transform.Find("SdChara").gameObject.SetActive(value: false);
				m_charaInfoPrefab.transform.Find("BtCharaChange").gameObject.SetActive(value: false);
				m_charaInfoPrefab.transform.Find("BtCharaChangeNone").gameObject.SetActive(value: true);
			}
		}
		m_scrollView.ResetScroll();
		isListMaking = false;
	}

	public async UniTask CharaSdSetImage(CharaType charaId, Image img, bool isChange = true)
	{
		if (Enum.IsDefined(typeof(CharaType), charaId))
		{
			img.gameObject.SetActive(value: true);
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(string.Format("Result/Chara{0}/Chara{1}SdResultFan.png", ((int)(charaId + 1)).ToString("D2"), (int)(charaId + 1)));
			disposableAsset.AddTo(img);
			img.sprite = disposableAsset.Asset;
		}
		if (!isChange)
		{
			img.gameObject.SetActive(value: false);
		}
	}

	private async UniTask ChangeTab(Category category)
	{
		if (m_listRefleshed)
		{
			m_listRefleshed = false;
			m_missionCharaSelectWindow.gameObject.SetActive(value: false);
			m_missionCharaSelectWindow._isOpen = false;
			m_missionScrollView.SetActive(value: false);
			Debug.Log($"Click category : {category}");
			m_charaInfoPrefab.SetActive(category == Category.Character);
			isCharaChange = false;
			OpenCategory = category;
			TabReflesh(category);
			await CreateMissionList(category);
			m_listRefleshed = true;
			m_missionScrollView.SetActive(value: true);
		}
	}

	private void TabReflesh(Category category)
	{
		foreach (Category key in MissionTabDict.Keys)
		{
			if (key == category)
			{
				MissionTabDict[key].GetComponent<Image>().sprite = _tabOnSpr;
				MissionTabDict[key].GetComponent<Image>().color = _tabOnColor;
				MissionTabDict[key].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().color = _tabNameOnColor;
			}
			else
			{
				MissionTabDict[key].GetComponent<Image>().sprite = _tabOffSpr;
				MissionTabDict[key].GetComponent<Image>().color = _tabOffColor;
				MissionTabDict[key].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().color = _tabNameOffColor;
			}
		}
	}

	public int ToFilterType(CharaType charaType)
	{
		return (int)(charaType + 1);
	}
}

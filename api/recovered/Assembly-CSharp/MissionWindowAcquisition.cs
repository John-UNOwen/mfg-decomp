using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.Types.Mission;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MissionWindowAcquisition : MonoBehaviour, IDisposable
{
	[SerializeField]
	private GameObject m_itemParent;

	[SerializeField]
	private GameObject m_itemPrefab;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private ItemPopupManager _itemPopupManager;

	private IDisposableAsset<Sprite> m_iconSprite;

	public bool m_isAcquisitionOpen;

	private bool _isFirstDisp;

	private bool _isNextEnable;

	private JudgeLocation _location;

	private List<(MissionID missionID, int achievementIndex, string itemOID, int itemNum)> clearMissions;

	public const int GetItemDispMax = 5;

	private void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isAcquisitionOpen = false;
		}).AddTo(this);
	}

	public void ResetItem()
	{
		foreach (Transform item in m_itemParent.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	public void Judge(JudgeLocation location)
	{
		clearMissions = new List<(MissionID, int, string, int)>();
		clearMissions = MissionFlow.Judge(location);
	}

	public async UniTask Open(JudgeLocation location)
	{
		if (location != JudgeLocation.Result)
		{
			Judge(location);
		}
		List<(MissionID missionID, int achievementIndex, string itemOID, int itemNum)> list = clearMissions;
		if (list == null || list.Count <= 0)
		{
			return;
		}
		Debug.Log("ClearMission : " + string.Join(",", clearMissions.Select(((MissionID missionID, int achievementIndex, string itemOID, int itemNum) x) => x.missionID).Distinct().ToList()));
		ResetItem();
		base.gameObject.SetActive(value: true);
		m_isAcquisitionOpen = false;
		_isFirstDisp = true;
		_isNextEnable = true;
		int i;
		for (i = 0; i < clearMissions.Count(); i++)
		{
			if (i != 0 && i % 5 == 0)
			{
				await ClearAcquisitionDisplay();
			}
			await UnityEngine.Object.Instantiate(m_itemPrefab, m_itemParent.transform).GetComponent<MissionList>().Init(GameDataManager.Mission.m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == clearMissions[i].missionID), clearMissions[i].achievementIndex, _itemPopupManager, isAcquisition: true, isFinish: false, clearMissions[i].itemOID, clearMissions[i].itemNum);
		}
		await ClearAcquisitionDisplay();
		await UniTask.WaitUntil(() => _isNextEnable);
		base.gameObject.SetActive(value: false);
	}

	private async UniTask ClearAcquisitionDisplay()
	{
		await UniTask.WaitUntil(() => _isNextEnable);
		_isNextEnable = false;
		m_isAcquisitionOpen = true;
		m_closeButton.interactable = true;
		m_animator.SetTrigger("In");
		SoundManager.PlaySe("SFX_SYS_BONUS5").Forget();
		if (_location != JudgeLocation.Home && _isFirstDisp)
		{
			SoundManager.PlayMyCharaVoice("VOX_GXX_ITEM2").Forget();
			_isFirstDisp = false;
		}
		await UniTask.WhenAny(UniTask.WaitUntil(() => !m_isAcquisitionOpen), UniTask.Delay((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? TimeSpan.FromSeconds(1.0) : TimeSpan.FromSeconds(7.0)));
		await AcquisitionClose();
		ResetItem();
	}

	public async UniTask AcquisitionClose()
	{
		m_isAcquisitionOpen = false;
		SoundManager.PlaySe("SFX_MJG_INFO4").Forget();
		m_closeButton.interactable = false;
		foreach (Transform item in m_itemParent.transform)
		{
			item.GetComponent<Animator>().SetTrigger("Acquisition");
		}
		await UniTask.Delay(TimeSpan.FromSeconds(0.699999988079071));
		m_animator.SetTrigger("Out");
		_itemPopupManager.CloseForget();
		await UniTask.Delay(TimeSpan.FromSeconds(0.30000001192092896));
		_isNextEnable = true;
	}

	public void Dispose()
	{
		if (m_iconSprite != null)
		{
			m_iconSprite.Dispose();
			m_iconSprite = null;
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}
}

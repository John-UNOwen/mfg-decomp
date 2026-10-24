using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types.Customize.CustomizeItem;
using UniRx;
using UnityEngine;

public class StageBG : MonoBehaviour
{
	[SerializeField]
	private Transform m_stageBGParent;

	[SerializeField]
	private List<GameObject> m_reachStages;

	[SerializeField]
	private GameObject m_fireReachStage;

	[SerializeField]
	private GameObject m_chocoReachStage;

	[SerializeField]
	private GameObject m_openReachStage;

	[SerializeField]
	private GameObject m_atomicReachStage;

	private StageBGItem stageBGItem;

	private Animator concentratedLineEf;

	private StageState m_stageState;

	private bool _initialized;

	private GameObject reachStage;

	public StageState StageState
	{
		get
		{
			return m_stageState;
		}
		set
		{
			if (m_stageState != value)
			{
				ModifyState(value);
			}
			m_stageState = value;
		}
	}

	private void Start()
	{
	}

	public async UniTask Init(GameObject concentratedLineEffect, ItemID item)
	{
		if (!_initialized)
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>($"CustomizeItem/{item}.prefab");
			obj.AddTo(this);
			GameObject gameObject = Object.Instantiate(obj.Asset, m_stageBGParent.transform, worldPositionStays: false);
			stageBGItem = gameObject.transform.GetComponent<StageBGItem>();
			reachStage = m_reachStages.Find((GameObject x) => x.name.Equals($"Reach{item}"));
			concentratedLineEf = concentratedLineEffect.transform.GetComponent<Animator>();
			ModifyState(StageState.None);
			_initialized = true;
		}
	}

	private void ModifyState(StageState nextState)
	{
		if (nextState <= StageState.Start)
		{
			ReachStage_SetActive(isActive: false);
		}
		else if (nextState == StageState.Excite)
		{
			concentratedLineEf.SetTrigger("In");
		}
		stageBGItem.AudienceSetTrigger(nextState);
	}

	public void ReachStage_SetActive(bool isActive, bool isFireReach = false, bool isOpenReach = false, bool isAtomicReach = false, bool isChocoReach = false)
	{
		stageBGItem.SetActive(!isActive);
		reachStage.SetActive(isActive && !isFireReach && !isChocoReach && !isOpenReach && !isAtomicReach);
		m_fireReachStage.SetActive(isActive && isFireReach);
		m_openReachStage.SetActive(isActive && !isFireReach && isOpenReach);
		m_atomicReachStage.SetActive(isActive && !isFireReach && !isOpenReach && isAtomicReach);
		m_chocoReachStage.SetActive(isActive && !isFireReach && !isOpenReach && isChocoReach && !isAtomicReach);
	}

	public GameObject GetBackScreen()
	{
		return stageBGItem.GetBackScreen();
	}
}

using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using UnityEngine;

public class SubMonitorCharaFlow : BaseFlow
{
	public enum CharaState
	{
		None,
		LoadThisScene,
		Home,
		Matching,
		MahjongPlay
	}

	[SerializeField]
	private StandingCharaManager m_charaManager;

	[SerializeField]
	private Camera m_charaCamera;

	[SerializeField]
	private Light m_charaDirectionalLight;

	[SerializeField]
	private Transform m_floor;

	[NonSerialized]
	public ItemID CurrentCostumeID = ItemID.Costume01_001;

	private CharaState m_currentState;

	private StandingCharaController _standingCharaController;

	private static SubMonitorCharaFlow m_instance;

	private bool IsCharaChanging;

	public static SubMonitorCharaFlow GetInstance()
	{
		if (!m_instance)
		{
			try
			{
				m_instance = GameObject.Find("SubMonitorCharaFlow").GetComponent<SubMonitorCharaFlow>();
			}
			catch
			{
				m_instance = null;
			}
		}
		return m_instance;
	}

	public static SubMonitorCharaFlow ResetInstance()
	{
		m_instance = null;
		return null;
	}

	public async UniTask CharaChange()
	{
		if (!IsCharaChanging)
		{
			IsCharaChanging = true;
			CharaUnLoad();
			CameraRefresh();
			_standingCharaController = await m_charaManager.Load(CurrentCostumeID);
			IsCharaChanging = false;
		}
	}

	public void CharaUnLoad()
	{
		if (_standingCharaController != null)
		{
			m_charaManager.UnLoad(CurrentCostumeID);
		}
	}

	public async UniTask ViewChange(CharaState state, ItemID costume = ItemID.None)
	{
		m_currentState = state;
		switch (m_currentState)
		{
		case CharaState.LoadThisScene:
			if (costume != 0)
			{
				CurrentCostumeID = costume;
			}
			await CharaChange();
			CameraRefresh();
			break;
		case CharaState.Home:
		case CharaState.MahjongPlay:
			CharaUnLoad();
			CameraRefresh();
			break;
		case CharaState.Matching:
			break;
		}
	}

	public void CameraRefresh()
	{
		switch (m_currentState)
		{
		case CharaState.Home:
		case CharaState.Matching:
			m_charaCamera.transform.localPosition = new Vector3(0f, 2.8f, 7.56f);
			m_charaCamera.transform.localEulerAngles = new Vector3(10f, 180f, 0f);
			m_charaDirectionalLight.transform.localEulerAngles = new Vector3(137.5f, 24f, 0f);
			break;
		case CharaState.MahjongPlay:
			m_charaCamera.transform.localPosition = new Vector3(0f, 2.8f, -9.3f);
			m_charaCamera.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
			m_charaDirectionalLight.transform.localEulerAngles = new Vector3(137.5f, 204f, 0f);
			break;
		default:
			m_charaCamera.transform.localPosition = new Vector3(100f, 2.8f, 7.56f);
			m_charaCamera.transform.localEulerAngles = new Vector3(10f, 180f, 0f);
			m_charaDirectionalLight.transform.localEulerAngles = new Vector3(137.5f, 24f, 0f);
			break;
		}
		FloorAdjuster();
	}

	public void FloorAdjuster(ItemID itemID = ItemID.None)
	{
		ItemID costume = ((itemID != 0) ? itemID : ((m_currentState == CharaState.LoadThisScene) ? CurrentCostumeID : GameDataManager.Player.GetCurrentChara().CostumeItemEquip));
		CharaType charaType = CostumeItemMaster.GetItem(costume).CharaType;
		float valueOrDefault = (MasterGameDataManager.Character.Properties(charaType).TBSFloorAdjustList?.Find(delegate(TBSFloorAdjuster x)
		{
			string costumeNumber = x.CostumeNumber;
			string text = costume.ToString();
			int length = text.Length;
			int num = length - 3;
			return costumeNumber.Equals(text.Substring(num, length - num));
		})?.AdjustParam).GetValueOrDefault();
		Transform transform = m_floor.transform;
		Vector3 localPosition;
		switch (m_currentState)
		{
		case CharaState.Home:
		case CharaState.Matching:
			localPosition = new Vector3(0f, valueOrDefault, 0f);
			break;
		case CharaState.MahjongPlay:
			localPosition = new Vector3(0f, valueOrDefault, 0f);
			break;
		default:
			localPosition = new Vector3(100f, valueOrDefault, 0f);
			break;
		}
		transform.localPosition = localPosition;
	}

	public void PlayMotion(string trigger)
	{
		_standingCharaController.PlayMotion(trigger);
	}
}

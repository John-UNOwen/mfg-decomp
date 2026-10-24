using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using UnityEngine;

namespace MFG.MahjongPlay.Character;

public class CharacterModelController : IDisposable
{
	private Player _player;

	private IDisposableAsset<GameObject> _prefab3D;

	private GameObject _instance3D;

	private Vector3 _orgPosition;

	private Vector3 _orgRotation;

	private int _playRequestPosition;

	private int _currentPlayPosition;

	private CharacterPartsDispController _characterPartsDispController;

	private bool isReachAway;

	private Cha lookDirect => MahjongUtility.GetCha((Kaze)_player.m_cha, Kaze.Ton);

	public async UniTask Initialize(Player player, GameObject parent3D, MotionID initialMotionID, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		_player = player;
		ItemID costume = player.CostumeItemEquip;
		string modelAddress = $"Standing3D/{costume}_Model.prefab";
		if (!(await AssetLoader.Exists(modelAddress)))
		{
			Debug.LogWarning($"{costume} が存在しません。デフォルトをロードします。");
			costume = CostumeItemMaster.GetDefaultEquipItemID(player.m_charaType);
			modelAddress = $"Standing3D/{costume}_Model.prefab";
		}
		RuntimeAnimatorController motionController = await ModelManager.MotionLoad(costume, isMahjong: true);
		_prefab3D = await AssetLoader.LoadAssetAsync<GameObject>(modelAddress, null, cancellationToken);
		_instance3D = UnityEngine.Object.Instantiate(_prefab3D.Asset, parent3D.transform, worldPositionStays: false);
		_instance3D.GetComponent<ModelManager>().Init(costume, motionController, isMahjong: true);
		string layerName = ChaToLayerName(_player.m_cha);
		_instance3D.SetLayerRecursive(layerName);
		_orgPosition = _instance3D.transform.localPosition;
		_orgRotation = _instance3D.transform.localEulerAngles;
		_characterPartsDispController = _instance3D.GetComponentInChildren<CharacterPartsDispController>(includeInactive: true);
		Vector3 euler = MasterGameDataManager.Character.CommonProperties().MahjongPlayLightingVector + MasterGameDataManager.Character.Properties(player.m_charaType).MahjongPlayLightingOffsetParam;
		parent3D.transform.Find("Directional Light").localRotation = Quaternion.Euler(euler);
		PlayMotion(initialMotionID).Forget();
		static string ChaToLayerName(Cha argCha)
		{
			return argCha switch
			{
				Cha.Simocha => "Chara3DRight", 
				Cha.Toimen => "Chara3DCenter", 
				Cha.Kamicha => "Chara3DLeft", 
				_ => "Chara3DMyChara", 
			};
		}
	}

	public void SetOffset(Vector3 positionOffset, Vector3 rotationOffset)
	{
		_instance3D.transform.localPosition = _orgPosition + positionOffset;
		_instance3D.transform.localEulerAngles = _orgRotation + rotationOffset;
	}

	public async UniTask PlayMotion(global::Character.EventID eventID)
	{
		await PlayMotion(GetMotionID(eventID));
	}

	public async UniTask PlayMotion(MotionID motionID, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_player == null || motionID == MotionID.None)
		{
			return;
		}
		int myPlayRequest = _playRequestPosition;
		_playRequestPosition++;
		try
		{
			if (myPlayRequest != _currentPlayPosition)
			{
				Debug_DispActionLog(motionID, _player.m_cha, _player.m_name, "Wait");
				await UniTask.WaitUntil(() => myPlayRequest >= _currentPlayPosition, PlayerLoopTiming.Update, cancellationToken);
			}
			Debug_DispActionLog(motionID, _player.m_cha, _player.m_name, "Play");
			Model_Play(motionID);
		}
		finally
		{
			_currentPlayPosition++;
		}
	}

	private void Debug_DispActionLog(MotionID motionID, Cha cha, string playerName, string stateName)
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.EnableCharaActionLog))
		{
			Debug.Log($"<color=yellow>MotionPlay/【{stateName}】/{cha}/{playerName}/{motionID}</color>");
		}
	}

	private void Model_Play(MotionID motionID)
	{
		bool isReach = _player.m_isReach;
		if (!isReach)
		{
			isReachAway = false;
		}
		if (_instance3D != null)
		{
			_instance3D.GetComponent<ModelManager>().PlayMotionMahjong(motionID.GetTriggerName(), isReach, isReachAway, 1f, (int)lookDirect);
			isReachAway = isReach;
		}
	}

	public AnimatorStateInfo GetCurrentAnimatorState()
	{
		return _instance3D.GetComponent<ModelManager>().GetCurrentAnimatorState();
	}

	public bool IsCharaAdulation()
	{
		if (_instance3D != null)
		{
			return _instance3D.GetComponent<ModelManager>().IsCharaAdulation();
		}
		return false;
	}

	public Transform GetCharaHead()
	{
		return _instance3D.GetComponent<ModelManager>().GetCharaHead();
	}

	private void PlayCrossFadeInFixedTime(string stateName, float transitionTimeSec, int layerID)
	{
		_instance3D.GetComponent<ModelManager>().PlayCrossFadeInFixedTimeInMahjong(stateName, _player.m_isReach, (int)lookDirect, layerID, transitionTimeSec);
	}

	private void PlayState(string stateName, int layerID)
	{
		_instance3D.GetComponent<ModelManager>().PlayCrossFadeInFixedTimeInMahjong(stateName, _player.m_isReach, (int)lookDirect, layerID);
	}

	public void PlayThinkForce(CharaType chara)
	{
		string ryukyokuMotion = MasterGameDataManager.Character.Properties(chara).RyukyokuMotion;
		PlayCrossFadeInFixedTime(ryukyokuMotion, 0.25f, 0);
		PlayState(ryukyokuMotion, 1);
	}

	public bool IsPlaying(MotionID motionID)
	{
		return false;
	}

	public void Dispose()
	{
		if (_instance3D != null)
		{
			UnityEngine.Object.Destroy(_instance3D);
			_instance3D = null;
		}
		if (_prefab3D != null)
		{
			_prefab3D.Dispose();
			_prefab3D = null;
		}
	}

	private MotionID GetMotionID(global::Character.EventID eventID)
	{
		switch (eventID)
		{
		case global::Character.EventID.TaikyokuStart:
			return MotionID.Start;
		case global::Character.EventID.Tyoko1:
		case global::Character.EventID.Tsumo1:
		case global::Character.EventID.Tsumo2:
			return MotionID.None;
		case global::Character.EventID.Tyoko2:
		case global::Character.EventID.Tyoko3:
			return MotionID.Think;
		case global::Character.EventID.Discard:
			return MotionID.Away1;
		case global::Character.EventID.TsumoAgari1:
		case global::Character.EventID.RonAgari1:
			return MotionID.None;
		case global::Character.EventID.Result1:
			return MotionID.ResultPlus0;
		case global::Character.EventID.Result2:
			return MotionID.ResultMinus0;
		case global::Character.EventID.Result3:
			return MotionID.Wait0;
		case global::Character.EventID.Result4:
			return MotionID.ResultCritical0;
		case global::Character.EventID.EyeContact:
			return MotionID.EyeContact;
		default:
			return MotionID.None;
		}
	}

	public bool IsTsumohaiTiming()
	{
		return true;
	}

	public void ResetTsumohaiTiming()
	{
	}

	public bool IsSutehaiTiming()
	{
		return true;
	}

	public void ResetSutehaiTiming()
	{
	}

	public void Parts_SetExclusiveDisp(int prtsIndex)
	{
		if (_characterPartsDispController != null)
		{
			_characterPartsDispController.SetExclusiveDisp(prtsIndex);
		}
	}

	public void Parts_SetDispAllOff()
	{
		if (_characterPartsDispController != null)
		{
			_characterPartsDispController.SetDispAll(isDisp: false);
		}
	}
}

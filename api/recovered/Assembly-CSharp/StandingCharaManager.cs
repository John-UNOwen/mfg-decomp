using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using UTJ;
using UniRx;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class StandingCharaManager : MonoBehaviour
{
	private class CharaModel : IDisposable
	{
		private int _index;

		private CharaType _charaType;

		private ItemID _costumeItemID;

		private IDisposableAsset<GameObject> _3DPrefab;

		private IDisposableAsset<GameObject> _3DStoryPrefab;

		private GameObject _3DInstance;

		private CharacterPartsDispController _characterPartsDispController;

		private IDisposableAsset<GameObject> _2DPrefab;

		private GameObject _2DInstance;

		private StandingCharaController _controller;

		private RenderTexture _renderTexture;

		private bool _isInitialized;

		public StandingCharaController CharaController => _controller;

		public ItemID CostumeItemID => _costumeItemID;

		public async UniTask Load(int index, ItemID itemID, StandingCharaAssetType assetType, GameObject parent3D, GameObject parent2D, CancellationToken cancellationToken = default(CancellationToken), int removableIndex = 0)
		{
			bool isMatching = assetType == StandingCharaAssetType.Matching;
			if (isMatching)
			{
				assetType = StandingCharaAssetType.Home;
			}
			_index = index;
			_costumeItemID = itemID;
			_charaType = CostumeItemMaster.GetItem(itemID).CharaType;
			cancellationToken.ThrowIfCancellationRequested();
			if (_isInitialized)
			{
				return;
			}
			string address3D = "Assets/MFG/Characters/Common/Motion3D/Prefabs/Standing3D.prefab";
			string address2D = AssetPathResolver.StandingChara2DAssets(itemID, assetType);
			RuntimeAnimatorController motionController = await ModelManager.MotionLoad(itemID, isMahjong: false);
			_3DPrefab = await AssetLoader.LoadAssetAsync<GameObject>(address3D, null, cancellationToken);
			_2DPrefab = await AssetLoader.LoadAssetAsync<GameObject>(address2D, null, cancellationToken);
			_3DStoryPrefab = await AssetLoader.LoadAssetAsync<GameObject>($"Standing3D/{itemID}_Model.prefab");
			_3DInstance = UnityEngine.Object.Instantiate(_3DPrefab.Asset, parent3D.transform, worldPositionStays: false);
			if (_3DStoryPrefab != null)
			{
				_3DInstance.transform.GetComponent<Standing3DManager>().CharaLoad(_3DStoryPrefab.Asset);
			}
			_3DInstance.transform.Find("Parent/Parent3D/CharaModel").GetComponent<ModelManager>().Init(itemID, motionController, isMahjong: false, removableIndex);
			Vector3 euler = MasterGameDataManager.Character.CommonProperties().CharacterLightingVector + MasterGameDataManager.Character.Properties(_charaType).LightingOffsetParam;
			_3DInstance.transform.Find("Parent/Directional Light").localRotation = Quaternion.Euler(euler);
			_3DInstance.name = itemID.ToString();
			_3DInstance.SetLayerRecursive("StandingChara3D");
			_renderTexture = new RenderTexture(isMatching ? 880 : 2400, isMatching ? 800 : 1600, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt)
			{
				antiAliasing = (isMatching ? 2 : 4),
				filterMode = FilterMode.Bilinear
			};
			_renderTexture.Create();
			_3DInstance.GetComponentInChildren<Camera>(includeInactive: true).targetTexture = _renderTexture;
			_characterPartsDispController = _3DInstance.GetComponentInChildren<CharacterPartsDispController>(includeInactive: true);
			Vector3 position = _3DInstance.transform.position;
			position.x += 10f * (float)_index;
			_3DInstance.transform.position = position;
			SpringManager[] componentsInChildren = _3DInstance.GetComponentsInChildren<SpringManager>(includeInactive: true);
			if (componentsInChildren != null)
			{
				SpringManager[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].RefleshInitialPositionAndRotation();
				}
			}
			_2DInstance = UnityEngine.Object.Instantiate(_2DPrefab.Asset, parent2D.transform, worldPositionStays: false);
			_2DInstance.name = itemID.ToString();
			_2DInstance.transform.Find("Base").localScale = ((assetType == StandingCharaAssetType.Story) ? new Vector3(0.7f, 0.7f, 0.7f) : new Vector3(1f, 1f, 1f));
			_controller = _2DInstance.GetComponent<StandingCharaController>();
			_controller.Init(_charaType, _3DInstance, _2DInstance);
			_controller.SetTargetTexture(_renderTexture, isMatching);
			await UniTask.Yield();
			await UniTask.Yield();
			_controller.SetDisp(isDisp: true);
			_isInitialized = true;
		}

		public void Dispose()
		{
			Debug.Log($"CharaModel.Dispose: charaType={_charaType}");
			if (_isInitialized)
			{
				if (_2DInstance != null)
				{
					UnityEngine.Object.Destroy(_2DInstance);
					_2DInstance = null;
				}
				if (_3DInstance != null)
				{
					_3DInstance.GetComponentInChildren<Camera>(includeInactive: true).targetTexture = null;
					UnityEngine.Object.Destroy(_3DInstance);
					_3DInstance = null;
				}
				if (_2DPrefab != null)
				{
					_2DPrefab.Dispose();
					_2DPrefab = null;
				}
				if (_3DPrefab != null)
				{
					_3DPrefab.Dispose();
					_3DPrefab = null;
				}
				if (_renderTexture != null)
				{
					UnityEngine.Object.Destroy(_renderTexture);
					_renderTexture = null;
				}
				_isInitialized = false;
			}
		}

		public void Parts_SetExclusiveDisp(int prtsIndex)
		{
			if (_characterPartsDispController != null)
			{
				_characterPartsDispController.SetExclusiveDisp(prtsIndex);
			}
		}

		public void Parts_SetAllOff()
		{
			if (_characterPartsDispController != null)
			{
				_characterPartsDispController.SetDispAll(isDisp: false);
			}
		}
	}

	private List<CharaModel> _instances = new List<CharaModel>();

	[SerializeField]
	private StandingCharaAssetType _assetType;

	[SerializeField]
	private GameObject _parent3D;

	[SerializeField]
	private GameObject _parent2D;

	public bool IsTouchReactionEnable;

	private bool _enableTouchRotate;

	public bool AllowDuplicatesInstance;

	private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

	private ItemID _itemID;

	private StandingCharaController _chara;

	private CancellationTokenSource _charaChangeToken;

	private bool _isCharaChanging;

	private bool _isTouchReactionPlaying;

	public bool EnableTouchRotate
	{
		get
		{
			return _enableTouchRotate;
		}
		set
		{
			_enableTouchRotate = value;
			foreach (CharaModel instance in _instances)
			{
				instance.CharaController.EnableTouchRotate = _enableTouchRotate;
			}
		}
	}

	private void OnDestroy()
	{
		Debug.Log("StandingCharaManager.OnDestroy");
		foreach (CharaModel instance in _instances)
		{
			instance.Dispose();
		}
		_instances.Clear();
	}

	public void UnLoad(ItemID itemID)
	{
		CharaModel charaModel = _instances.Find((CharaModel x) => x.CostumeItemID == itemID);
		if (charaModel != null)
		{
			charaModel.Dispose();
			_instances.Remove(charaModel);
		}
	}

	public async UniTask Load(CharaType charaType, GameObject parent2D = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		ItemID defaultEquipItemID = CostumeItemMaster.GetDefaultEquipItemID(charaType);
		await Load(defaultEquipItemID, parent2D, cancellationToken);
	}

	public async UniTask<StandingCharaController> Load(ItemID itemID, GameObject parent2D = null, CancellationToken cancellationToken = default(CancellationToken), int removableIndex = 0)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await _loadSemaphore.WaitAsync();
		int index = -1;
		try
		{
			if (AllowDuplicatesInstance || !_instances.Any((CharaModel x) => x.CostumeItemID == itemID))
			{
				CharaModel charaModel = new CharaModel();
				if (parent2D == null)
				{
					parent2D = _parent2D;
				}
				index = _instances.Count;
				await charaModel.Load(index, itemID, _assetType, _parent3D, parent2D, cancellationToken, removableIndex);
				Debug.Log("Standing CharaManager: Loaded ItemID " + itemID);
				SubscribeTouchReaction(charaModel.CharaController, cancellationToken);
				charaModel.CharaController.EnableTouchRotate = EnableTouchRotate;
				_instances.Add(charaModel);
			}
		}
		finally
		{
			_loadSemaphore.Release();
		}
		if (index >= 0)
		{
			return _instances[index].CharaController;
		}
		return _instances.Find((CharaModel x) => x.CostumeItemID == itemID)?.CharaController;
	}

	public void SetExclusiveDisp(CharaType charaType)
	{
		foreach (CharaModel instance in _instances)
		{
			instance.CharaController.SetDisp(isDisp: false);
		}
		foreach (CharaModel instance2 in _instances)
		{
			if (instance2.CharaController.CharaType == charaType)
			{
				instance2.CharaController.SetDisp(isDisp: true);
			}
		}
	}

	public void SetExclusiveDisp(ItemID itemID)
	{
		foreach (CharaModel instance in _instances)
		{
			instance.CharaController.SetDisp(isDisp: false);
		}
		foreach (CharaModel instance2 in _instances)
		{
			if (instance2.CostumeItemID == itemID)
			{
				instance2.CharaController.SetDisp(isDisp: true);
			}
		}
	}

	public async UniTask SingleCharaLoad(ItemID itemID)
	{
		if (_isCharaChanging)
		{
			_charaChangeToken?.Cancel();
			_charaChangeToken?.Dispose();
		}
		_isCharaChanging = true;
		if (_itemID != 0)
		{
			_chara.CharaAnimator.speed = 1f;
			_chara.gameObject.SetActive(value: false);
			UnLoad(_itemID);
		}
		_charaChangeToken = new CancellationTokenSource();
		StandingCharaController chara = await Load(itemID, null, _charaChangeToken.Token);
		SetExclusiveDisp(itemID);
		_itemID = itemID;
		_chara = chara;
		if (SystemConfigGameData.IsSXGAMonitor)
		{
			SubMonitorCharaFlow instance = SubMonitorCharaFlow.GetInstance();
			if (instance != null)
			{
				instance.FloorAdjuster(itemID);
			}
		}
		_charaChangeToken.Dispose();
		_isCharaChanging = false;
	}

	public void Parts_SetExclusiveDisp(int instanceIndex, int partsIndex)
	{
		if (instanceIndex >= 0 && instanceIndex < _instances.Count)
		{
			_instances[instanceIndex].Parts_SetExclusiveDisp(partsIndex);
		}
	}

	public void Parts_SetDispAllOff(int instanceIndex)
	{
		if (instanceIndex >= 0 && instanceIndex < _instances.Count)
		{
			_instances[instanceIndex].Parts_SetAllOff();
		}
	}

	private void SubscribeTouchReaction(StandingCharaController charaController, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (IsTouchReactionEnable)
		{
			charaController.EnableTouchRotate = true;
		}
		charaController.OnCharaTouched.Subscribe(async delegate
		{
			if (IsTouchReactionEnable && !_isTouchReactionPlaying)
			{
				_isTouchReactionPlaying = true;
				await charaController.PlayTouchedVoice(cancellationToken);
				_isTouchReactionPlaying = false;
			}
		}).AddTo(this);
	}
}

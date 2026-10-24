using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MissionCharaSelectWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollView _scrollView;

	[SerializeField]
	private GameObject _charaButtonPrefab;

	[SerializeField]
	private TextMeshProUGUI _charaName;

	[SerializeField]
	private Image _charaSd;

	[SerializeField]
	private MissionWindow _missionWindow;

	private CharaType _selectingCharaType;

	public bool _isOpen;

	private bool _initFinished;

	private Subject<CharaType> _onCharaChange = new Subject<CharaType>();

	private bool m_isSelecting;

	public IObservable<CharaType> OnCharaChange => _onCharaChange.AsObservable();

	public async UniTask Init()
	{
		_selectingCharaType = _missionWindow.m_selectingChara;
		_charaName.text = MasterGameDataManager.Character.Properties(_selectingCharaType).Profile.Name;
		await _missionWindow.CharaSdSetImage(_selectingCharaType, _charaSd);
		_scrollView.ResetList();
		foreach (CharaType charaType in Enum.GetValues(typeof(CharaType)))
		{
			if (!GameDataManager.IsCharaEnable(charaType))
			{
				continue;
			}
			GameObject obj = _scrollView.Add(_charaButtonPrefab, (int)charaType);
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.CharaSelect, charaType, CharaSelectAssetType.CharaSelectButton));
			Image component = obj.transform.Find("Bt").GetComponent<Image>();
			disposableAsset.AddTo(component);
			component.sprite = disposableAsset.Asset;
			obj.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
			{
				if (!m_isSelecting)
				{
					m_isSelecting = true;
					_selectingCharaType = charaType;
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
					_charaName.text = MasterGameDataManager.Character.Properties(_selectingCharaType).Profile.Name;
					await _missionWindow.CharaSdSetImage(_selectingCharaType, _charaSd);
					_missionWindow.m_selectingChara = _selectingCharaType;
					_onCharaChange.OnNext(_selectingCharaType);
					await UniTask.Delay(TimeSpan.FromSeconds(0.5));
					m_isSelecting = false;
					_missionWindow.isCharaChange = true;
					_isOpen = false;
				}
			})
				.AddTo(this);
		}
		_initFinished = true;
	}

	public async UniTask Play(CharaType selectingCharaType)
	{
		if (!_isOpen)
		{
			_isOpen = true;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			await UniTask.WaitUntil(() => _initFinished);
			base.gameObject.SetActive(value: true);
			_selectingCharaType = selectingCharaType;
			await UniTask.WaitWhile(() => _isOpen);
			base.gameObject.SetActive(value: false);
		}
	}

	public CharaType GetSelectingCharaType()
	{
		return _selectingCharaType;
	}
}

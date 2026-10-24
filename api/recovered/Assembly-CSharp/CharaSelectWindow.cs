using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CharaSelectWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollView _scrollView;

	[SerializeField]
	private GameObject _charaButtonPrefab;

	[SerializeField]
	private Button _decideButton;

	[SerializeField]
	private Button _cancelButton;

	[SerializeField]
	private GameObject _blackScreen;

	private CharaType _beforeSelectCharaType;

	private CharaType _selectingCharaType;

	private bool _isOpen;

	private bool _initFinished;

	private Subject<CharaType> _onCharaChange = new Subject<CharaType>();

	public bool m_isSelecting;

	public bool m_windowSelecting;

	private static LoadableAssetSet<Sprite> _charaIconAssetSet = new LoadableAssetSet<Sprite>();

	private static LoadableAssetSet<Sprite> _fightSpiritAssetSet = new LoadableAssetSet<Sprite>();

	public bool IsOpen => _isOpen;

	public IObservable<CharaType> OnCharaChange => _onCharaChange.AsObservable();

	public static async UniTask ResidentLoad()
	{
		List<string> assetAddresses = (from x in Enum.GetValues(typeof(CharaType)).OfType<CharaType>()
			select AssetPathResolver.Resolve(PathType.CharaSelect, x, CharaSelectAssetType.CharaSelectButton)).ToList();
		await _charaIconAssetSet.Load(assetAddresses);
		List<string> assetAddresses2 = (from x in Enum.GetValues(typeof(CharaType)).OfType<CharaType>()
			select AssetPathResolver.Resolve(PathType.Dojo, x, DojoFlow.AssetType.FightSpirit)).ToList();
		await _fightSpiritAssetSet.Load(assetAddresses2);
	}

	public async UniTask Init(CancellationToken cancellationToken = default(CancellationToken))
	{
		_decideButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			_isOpen = false;
		}).AddTo(this);
		if (_cancelButton != null)
		{
			_cancelButton.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				_isOpen = false;
				_selectingCharaType = _beforeSelectCharaType;
			}).AddTo(this);
		}
		_scrollView.ResetList();
		List<UniTask> list = new List<UniTask>();
		foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (GameDataManager.IsCharaEnable(value))
			{
				GameObject button = _scrollView.Add(_charaButtonPrefab, (int)value);
				list.Add(CharaButton_Create(button, value, cancellationToken));
			}
		}
		await list;
		_initFinished = true;
	}

	private async UniTask CharaButton_Create(GameObject button, CharaType charaType, CancellationToken cancellationToken = default(CancellationToken))
	{
		string unableText = GetUnableText(charaType);
		button.transform.Find("UnableText").Find("Text").GetComponent<TextMeshProUGUI>()
			.text = unableText;
		button.transform.Find("UnableText").gameObject.SetActive(unableText != "");
		Sprite sprite = await _charaIconAssetSet.Get(AssetPathResolver.Resolve(PathType.CharaSelect, charaType, CharaSelectAssetType.CharaSelectButton), cancellationToken);
		button.transform.Find("Bt").GetComponent<Image>().sprite = sprite;
		button.transform.Find("BtAdd").GetComponent<Image>().sprite = sprite;
		Image component = button.transform.Find("SpiritNumber").GetComponent<Image>();
		component.sprite = await _fightSpiritAssetSet.Get(AssetPathResolver.Resolve(PathType.Dojo, charaType, DojoFlow.AssetType.FightSpirit), cancellationToken);
		button.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
		{
			if (_selectingCharaType != charaType && !m_isSelecting && !m_windowSelecting)
			{
				m_windowSelecting = true;
				_scrollView.GetObject((int)_selectingCharaType).GetComponent<Animator>().SetTrigger("Normal");
				_decideButton.interactable = GameDataManager.IsCharaSelectable(charaType);
				_selectingCharaType = charaType;
				_scrollView.GetObject((int)_selectingCharaType).GetComponent<Animator>().SetTrigger("Selected");
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				_onCharaChange.OnNext(_selectingCharaType);
				await UniTask.Delay(TimeSpan.FromSeconds(0.5));
				m_windowSelecting = false;
			}
		})
			.AddTo(this);
	}

	public static string GetUnableText(CharaType chara)
	{
		if (GameDataManager.IsCharaSelectable(chara))
		{
			return "";
		}
		if (CoopMaster.IsCoopCharacter(chara))
		{
			return "e-amusement\n購買部での\n利用権の購入が必要です";
		}
		if (CharaStampMaster.IsStampEventCharacter(chara) || GachaUtility.CharaUnlockItemIDDict.ContainsKey(chara))
		{
			return "条件を満たすと\n使用可能になります";
		}
		return "利用不可です";
	}

	public async UniTask Play(CharaType selectingCharaType, bool dispoffWhenDecided = true, Dictionary<CharaType, bool> actives = null)
	{
		if (_isOpen)
		{
			return;
		}
		_isOpen = true;
		if (!GameDataManager.IsCharaEnable(selectingCharaType) || !GameDataManager.IsCharaSelectable(selectingCharaType))
		{
			selectingCharaType = CharaType.Chara01;
		}
		_beforeSelectCharaType = selectingCharaType;
		await UniTask.WaitUntil(() => _initFinished);
		base.gameObject.SetActive(value: true);
		if (_blackScreen != null)
		{
			_blackScreen.SetActive(value: true);
		}
		_selectingCharaType = selectingCharaType;
		foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
		{
			if (!GameDataManager.IsCharaEnable(value))
			{
				continue;
			}
			GameObject @object = _scrollView.GetObject((int)value);
			if (@object == null)
			{
				continue;
			}
			if (value == _selectingCharaType)
			{
				@object.GetComponent<Animator>().SetTrigger("Selected");
			}
			else
			{
				@object.GetComponent<Animator>().SetTrigger("Normal");
			}
			if (actives != null)
			{
				if (actives[value])
				{
					@object.GetComponent<Animator>().SetTrigger("Normal");
					@object.GetComponent<Button>().interactable = GameDataManager.IsCharaSelectable(value);
					@object.transform.Find("NowLessonText").gameObject.SetActive(value: false);
				}
				else
				{
					@object.GetComponent<Animator>().SetTrigger("Disabled");
					@object.GetComponent<Button>().interactable = false;
					@object.transform.Find("NowLessonText").gameObject.SetActive(value: true);
				}
				ItemID itemID = PlayerItemMaster.ToFightSpiritItemID(value);
				@object.transform.Find("SpiritNumber").gameObject.SetActive(value: true);
				@object.transform.Find("SpiritNumber").Find("SpiritNumText").GetComponent<TextMeshProUGUI>()
					.text = GameDataManager.PlayerItem.Count(itemID).ToString();
			}
			bool isDebut = GameDataManager.Player.CharaData[(int)value].IsDebut;
			@object.transform.Find("DebutState").Find("IconApprentice").gameObject.SetActive(!isDebut);
			@object.transform.Find("DebutState").Find("IconDebut").gameObject.SetActive(isDebut);
		}
		_decideButton.interactable = true;
		await UniTask.WaitWhile(() => _isOpen);
		if (dispoffWhenDecided)
		{
			base.gameObject.SetActive(value: false);
		}
		if (_blackScreen != null)
		{
			_blackScreen.SetActive(value: false);
		}
	}

	public CharaType GetSelectingCharaType()
	{
		return _selectingCharaType;
	}

	private void OnDestroy()
	{
		if (!GameDataManager.IsCharaSelectable(_selectingCharaType))
		{
			Debug.Log($"before Select : {_beforeSelectCharaType}");
			_selectingCharaType = _beforeSelectCharaType;
			GameDataManager.Player.SelectChara = _selectingCharaType;
		}
	}
}

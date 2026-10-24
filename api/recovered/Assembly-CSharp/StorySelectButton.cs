using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class StorySelectButton : MonoBehaviour
{
	private Button _storySelectButton;

	private StoryInfo _storyInfo;

	[SerializeField]
	private TextMeshProUGUI _storyNameTxt;

	[SerializeField]
	private GameObject _lock;

	[SerializeField]
	private TextMeshProUGUI _lockText;

	[SerializeField]
	private GameObject _new;

	[SerializeField]
	private Image _storyImage;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public async UniTask SetStoryInfo(StoryInfo storyInfo)
	{
		_storySelectButton = base.gameObject.GetComponent<Button>();
		_storyInfo = storyInfo;
		_storyNameTxt.text = _storyInfo.Master.DispName;
		if (!_storyInfo.IsAvailable)
		{
			_storySelectButton.interactable = false;
			_lock.SetActive(value: true);
			_lockText.text = StoryMaster.UnlockText(_storyInfo.Master);
		}
		if (_storyInfo.IsRead || !_storyInfo.IsAvailable)
		{
			_new.SetActive(value: false);
		}
		else
		{
			_new.SetActive(value: true);
		}
		try
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(_storyInfo.Master.StoryImagePath);
			disposableAsset.AddTo(this);
			_storyImage.sprite = disposableAsset.Asset;
		}
		catch (Exception ex)
		{
			Debug.LogError("エラー：" + ex);
		}
		_storySelectButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			GameDataManager.Story.m_playStoryScriptID = storyInfo.StoryID;
		}).AddTo(this);
	}

	public Image GetImage()
	{
		return _storyImage;
	}
}

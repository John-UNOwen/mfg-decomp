using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationDetailWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject _window;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _returnButton;

	[SerializeField]
	private Button _closeAllButton;

	[SerializeField]
	private TextMeshProUGUI _mainText;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private GameObject _contents;

	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private GameObject _newIcon;

	[SerializeField]
	private List<GameObject> _typeLists;

	[SerializeField]
	private TextMeshProUGUI _releaseDate;

	[SerializeField]
	private TextMeshProUGUI _updateDate;

	public bool _isOpen;

	private void Start()
	{
	}

	private void Update()
	{
		RefleshWindow();
	}

	public async UniTask OpenDetail(NoticeInfo notice, bool isForced = false)
	{
		if (_isOpen)
		{
			return;
		}
		_titleText.text = notice.Title;
		_newIcon.SetActive(notice.IsNew);
		_returnButton.gameObject.SetActive(!isForced);
		_closeAllButton.gameObject.SetActive(isForced);
		_mainText.text = SaniTize.SanitizedString(notice.Text, "#000000");
		await notice.DownloadTexture2DNotice();
		_image.transform.GetComponent<Image>().sprite = notice.NoticeImage;
		_image.transform.GetComponent<Image>().SetNativeSize();
		foreach (GameObject typeList in _typeLists)
		{
			typeList.SetActive(value: false);
		}
		_typeLists[(int)notice.Category].SetActive(value: true);
		_releaseDate.text = notice.ReleaseDate;
		if (notice.UpdateDate != null)
		{
			_updateDate.text = "【更新】" + notice.UpdateDate;
		}
		else
		{
			_updateDate.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().SetTrigger("In");
		_isOpen = true;
	}

	public async UniTask Close()
	{
		_isOpen = false;
		GetComponent<Animator>().SetTrigger("Out");
		base.gameObject.SetActive(value: false);
	}

	private void RefleshWindow()
	{
		_contents.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
		_contents.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_contents.GetComponent<RectTransform>());
	}
}

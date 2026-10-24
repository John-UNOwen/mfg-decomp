using Cysharp.Threading.Tasks;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupManager : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _name;

	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private GameObject _window;

	[SerializeField]
	private GameObject _wall;

	public bool _isOpen;

	public bool IsTouchedClose;

	public void ResetItem()
	{
		_name.text = "";
		_text.text = "";
	}

	public async UniTask SetItem(string name, string text)
	{
		_name.text = name;
		_text.text = text;
	}

	public async UniTask Open(string oid, Vector3 vct, Vector3 size)
	{
		await SetItem(ItemUtility.ItemCategory(oid), ItemUtility.ItemDescription(oid));
		if (vct.x >= 1450f)
		{
			vct.x = 1450f;
		}
		_window.GetComponent<RectTransform>().position = vct;
		base.gameObject.SetActive(value: true);
		_window.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
		_window.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_window.GetComponent<RectTransform>());
		_isOpen = true;
	}

	public async UniTask Close()
	{
		base.gameObject.SetActive(value: false);
		_isOpen = false;
	}

	public void Dispose()
	{
	}

	private void OnDestroy()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void CloseForget()
	{
		Close().Forget();
	}
}

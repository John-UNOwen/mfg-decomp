using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommentsChild : BaseComments
{
	[SerializeField]
	private GameObject _comment;

	[SerializeField]
	private GameObject _textObj;

	[SerializeField]
	private GameObject _fukidashiParent;

	[SerializeField]
	private GameObject _fukidashi;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TextMeshProUGUI _name;

	[SerializeField]
	private TextMeshProUGUI _text;

	public void Reflesh()
	{
		LayoutRebuild(_fukidashi, vartical: false);
		LayoutRebuild(_fukidashiParent, vartical: true);
		LayoutRebuild(_textObj, vartical: true);
		LayoutRebuild(_comment, vartical: false);
		LayoutRebuild(base.gameObject, vartical: true);
	}

	private void LayoutRebuild(GameObject obj, bool vartical)
	{
		obj.transform.GetComponent<ContentSizeFitter>().SetLayoutVertical();
		obj.transform.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
		if (vartical)
		{
			obj.transform.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
			obj.transform.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
		}
		else
		{
			obj.transform.GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputVertical();
			obj.transform.GetComponent<HorizontalLayoutGroup>().SetLayoutVertical();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(obj.transform.GetComponent<RectTransform>());
	}

	public void SetIcon(Sprite spr)
	{
		_icon.sprite = spr;
	}

	public void SetText(string n, string t)
	{
		_name.text = n;
		_text.text = t;
	}
}

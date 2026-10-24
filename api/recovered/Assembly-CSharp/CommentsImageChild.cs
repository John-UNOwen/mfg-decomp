using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommentsImageChild : BaseComments
{
	[SerializeField]
	private GameObject _comment;

	[SerializeField]
	private GameObject _textObj;

	[SerializeField]
	private GameObject _imageParent;

	[SerializeField]
	private GameObject _image;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TextMeshProUGUI _name;

	public void Reflesh()
	{
		LayoutRebuild(_imageParent, vartical: true);
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

	public void SetName(string name)
	{
		_name.text = name;
	}

	public void SetImage(Sprite spr)
	{
		float num = spr.texture.width;
		float num2 = spr.texture.height;
		float aspectRatio = num / num2;
		if (num >= 800f)
		{
			num = 800f;
		}
		_image.transform.GetComponent<Image>().sprite = spr;
		_ = _image.transform.GetComponent<RectTransform>().sizeDelta;
		_image.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(num, num2);
		_image.transform.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
	}
}

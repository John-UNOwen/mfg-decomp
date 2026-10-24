using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TweetChild : MonoBehaviour
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
	private GameObject _image;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TextMeshProUGUI _name;

	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private TextMeshProUGUI _repNum;

	[SerializeField]
	private TextMeshProUGUI _retNum;

	[SerializeField]
	private TextMeshProUGUI _favNum;

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

	public void SetText(string n, string t, string[] r)
	{
		_name.text = n;
		_text.text = t;
		_repNum.text = r[0];
		_retNum.text = r[1];
		_favNum.text = r[2];
	}

	public void SetImage(Sprite spr)
	{
		_image.SetActive(value: true);
		float num = spr.texture.width;
		float num2 = spr.texture.height;
		float aspectRatio = num / num2;
		if (num >= 1152f)
		{
			num = 1152f;
		}
		_image.transform.GetComponent<Image>().sprite = spr;
		_ = _image.transform.GetComponent<RectTransform>().sizeDelta;
		_image.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(num, num2);
		_image.transform.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
	}
}

using UnityEngine;
using UnityEngine.UI;

public class MatchingBGController : MonoBehaviour
{
	public enum NameLayout
	{
		Left,
		Center,
		Right
	}

	[SerializeField]
	private Image m_charaImage;

	[SerializeField]
	private GameObject m_center;

	[SerializeField]
	private GameObject m_nameParent;

	[SerializeField]
	private Image m_name;

	[SerializeField]
	private NameLayout m_nameLayout;

	[SerializeField]
	private bool m_isMahjongActor;

	private void Start()
	{
		m_center.SetActive(value: false);
		foreach (Transform item in m_charaImage.transform)
		{
			item.gameObject.SetActive(value: false);
		}
	}

	public void ImageAdjuster()
	{
		float x = (m_charaImage.transform.localScale.x * m_center.transform.localPosition.x - 399f) * -1f;
		m_charaImage.transform.localPosition = new Vector3(x, m_charaImage.transform.localPosition.y);
	}

	public void CharaCenterAdjuster(int i)
	{
		Transform transform = m_charaImage.transform.Find($"Center{i + 1}");
		if (transform == null)
		{
			transform = m_center.transform;
		}
		float x = (m_charaImage.transform.localScale.x * transform.localPosition.x - 399f) * -1f;
		m_charaImage.transform.localPosition = new Vector3(x, m_charaImage.transform.localPosition.y);
	}

	public void NameParentAdjuster(int i)
	{
		float num = (1920f - 10f * (float)i) / ((float)i + 1f);
		float num2 = num / 1920f;
		float num3 = m_name.transform.localScale.x * m_name.transform.GetComponent<RectTransform>().sizeDelta.x * 0.5f * num2;
		float num4 = (m_isMahjongActor ? 0f : 399f);
		float num5 = ((m_nameLayout == NameLayout.Center) ? 0f : (num * 0.5f - num3));
		if (m_nameLayout == NameLayout.Left)
		{
			num5 = -1f * num5;
		}
		m_nameParent.transform.localScale = new Vector3(num2, num2);
		m_nameParent.transform.localPosition = new Vector3(num5 + num4, m_nameParent.transform.localPosition.y);
	}
}

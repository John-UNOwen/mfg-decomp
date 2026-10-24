using System.Collections.Generic;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DanBanner : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_banners = new List<GameObject>();

	[SerializeField]
	private List<Sprite> m_proDanBanners = new List<Sprite>();

	[SerializeField]
	private GameObject m_bannerJansei;

	public void SetDanBanner(Transform parent, DanLevelType absoluteDanLevel)
	{
		foreach (Transform item2 in parent.transform)
		{
			Object.Destroy(item2.gameObject);
		}
		if (absoluteDanLevel >= DanLevelType.JANSEI1)
		{
			GameObject obj = Object.Instantiate(m_bannerJansei, parent.transform);
			(int reincarnationCount, DanLevelType danLevelTypeWithReincarnation) danLevelTypeWithReincarnation = DanLevelMaster.GetDanLevelTypeWithReincarnation(absoluteDanLevel);
			int item = danLevelTypeWithReincarnation.reincarnationCount;
			int num = (int)(danLevelTypeWithReincarnation.danLevelTypeWithReincarnation - 21 + 1);
			obj.transform.Find("Num").GetComponent<TextMeshProUGUI>().text = $"{num}";
			Transform transform = obj.transform.Find("Star");
			int num2 = 0;
			for (int i = ((item > 5) ? (item - 5) : 0); i < item; i++)
			{
				Transform transform2 = transform.Find($"Star{i + 1}");
				if (!(transform2 == null))
				{
					if (num2 < item)
					{
						transform2.gameObject.SetActive(value: true);
						num2++;
					}
					else
					{
						transform2.gameObject.SetActive(value: false);
					}
				}
			}
		}
		else
		{
			Object.Instantiate(m_banners[(int)absoluteDanLevel], parent.transform);
		}
	}

	public void SetDanBannerPro(Transform parent, ProDanLevelType proDanLevel)
	{
		foreach (Transform item in parent.transform)
		{
			Object.Destroy(item.gameObject);
		}
		Object.Instantiate(m_banners[0], parent.transform).transform.Find("Icon").GetComponent<Image>().sprite = m_proDanBanners[(int)proDanLevel];
	}
}

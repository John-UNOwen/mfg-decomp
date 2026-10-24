using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GachaSimulationDialog : MonoBehaviour
{
	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private GameObject m_wariaiText1;

	[SerializeField]
	private GameObject m_wariaiText2;

	[SerializeField]
	private GameObject m_items1;

	[SerializeField]
	private GameObject m_items2;

	[SerializeField]
	private GameObject m_listPrefab;

	[SerializeField]
	private Button m_upButton;

	[SerializeField]
	private Button m_downButton;

	[SerializeField]
	private ScrollRect m_scrollRect;

	public bool isOpen;

	public void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			isOpen = false;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		}).AddTo(this);
		m_upButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scrollRect.velocity = new Vector2(0f, -450f);
		}).AddTo(this);
		m_downButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scrollRect.velocity = new Vector2(0f, 450f);
		}).AddTo(this);
	}

	private void SetItem(Dictionary<ItemID, int> gachaResultInfo, GachaSeriesServerInfo info)
	{
		foreach (Transform item in m_items1.transform)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in m_items2.transform)
		{
			Object.Destroy(item2.gameObject);
		}
		int[] array = new int[4];
		int[] array2 = new int[4];
		foreach (ItemID item3 in info.Items)
		{
			int num;
			GameObject obj;
			if (info.SeriesType == GachaSeriesType.Pickup || info.SeriesType == GachaSeriesType.Unlock)
			{
				if (!info.PickupItems.Contains(item3))
				{
					num = (info.LimitedItems.Contains(item3) ? 1 : 0);
					if (num == 0)
					{
						goto IL_00e6;
					}
				}
				else
				{
					num = 1;
				}
				obj = m_items1;
				goto IL_00f4;
			}
			num = 0;
			goto IL_00e6;
			IL_00e6:
			obj = m_items2;
			goto IL_00f4;
			IL_00f4:
			GameObject gameObject = obj;
			GameObject obj2 = Object.Instantiate(m_listPrefab, gameObject.transform);
			int num2 = (gachaResultInfo.ContainsKey(item3) ? gachaResultInfo[item3] : 0);
			Rarity rarity = obj2.transform.GetComponent<SimulationItemPanel>().SetGachaSimulationItem(item3, num2);
			if (num != 0)
			{
				array[(int)(rarity - 2)] += num2;
			}
			else
			{
				array2[(int)(rarity - 2)] += num2;
			}
		}
		m_wariaiText1.transform.Find("TextUR").GetComponent<TextMeshProUGUI>().text = GetProbabilityString(Rarity.UR, array[3]);
		m_wariaiText1.transform.Find("TextSR").GetComponent<TextMeshProUGUI>().text = GetProbabilityString(Rarity.SR, array[2]);
		m_wariaiText1.transform.Find("TextR").GetComponent<TextMeshProUGUI>().text = GetProbabilityString(Rarity.R, array[1]);
		m_wariaiText1.transform.Find("TextN").GetComponent<TextMeshProUGUI>().text = GetProbabilityString(Rarity.N, array[0]);
		m_wariaiText2.transform.Find("TextUR").GetComponent<TextMeshProUGUI>().text = GetProbabilityString(Rarity.UR, array2[3]);
		m_wariaiText2.transform.Find("TextSR").GetComponent<TextMeshProUGUI>().text = GetProbabilityString(Rarity.SR, array2[2]);
		m_wariaiText2.transform.Find("TextR").GetComponent<TextMeshProUGUI>().text = GetProbabilityString(Rarity.R, array2[1]);
		m_wariaiText2.transform.Find("TextN").GetComponent<TextMeshProUGUI>().text = GetProbabilityString(Rarity.N, array2[0]);
	}

	private string GetProbabilityString(Rarity rare, int count)
	{
		float num = (float)count / 10000f;
		string text = rare.ToString();
		if (num > 100f)
		{
			num = 100f;
		}
		else if (num < 0f)
		{
			num = 0f;
		}
		string text2 = num.ToString("F3") + "％";
		return text.PadRight(12 - text.Length) + text2.PadLeft(8 - text2.Length);
	}

	public void Open(Dictionary<ItemID, int> gachaResultInfo, GachaSeriesServerInfo info)
	{
		SoundManager.PlaySe("SFX_SYS_GACYA7").Forget();
		isOpen = true;
		SetItem(gachaResultInfo, info);
		base.gameObject.SetActive(value: true);
	}
}

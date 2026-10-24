using System.Text;
using KAMUNITY;
using TMPro;
using TestModeSkeleton;
using UnityEngine;
using UnityEngine.UI;

public class MaintenanceWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private TextMeshProUGUI m_title;

	[SerializeField]
	private TextMeshProUGUI m_message;

	private static bool s_isActive;

	private GameSettings.ShopClosePhase m_phase;

	private Color green = new Color(0f, 1f, 0f);

	private Color yellow = new Color(1f, 1f, 0f);

	public static void SetActive(bool isActive)
	{
		s_isActive = isActive;
	}

	private void Awake()
	{
		m_root.SetActive(value: false);
	}

	private void SetLineColor(Color color)
	{
		string[] array = new string[5] { "Line00", "Line01", "Line02", "Line03", "Line04" };
		foreach (string n in array)
		{
			m_root.transform.Find(n).GetComponent<Image>().color = color;
		}
	}

	private void UpdatePhase()
	{
		GameSettings.ShopClosePhase shopClosePhase = ShopCloseUtility.GetShopClosePhase();
		switch (shopClosePhase)
		{
		default:
			m_root.SetActive(value: false);
			return;
		case GameSettings.ShopClosePhase.Notice:
		{
			if (m_phase != shopClosePhase)
			{
				m_root.SetActive(value: true);
				SetLineColor(green);
				m_title.text = "メンテナンス時間のご案内";
			}
			int num = 0;
			if (GameUtility.IsKamunityBootEnable())
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (AvsEa3.Ea3MainteSearch(0, out var start, out var _, stringBuilder, stringBuilder.Capacity, out var _) == 1)
				{
					num = (start - 1800) / 60;
					if (num < 0)
					{
						num = 0;
					}
				}
			}
			m_message.text = $"まもなくメンテナンス時間となります。\nあと{num}分でプレー開始できなくなりますのでご注意ください。";
			break;
		}
		case GameSettings.ShopClosePhase.Enforce:
			if (m_phase != shopClosePhase)
			{
				m_root.SetActive(value: true);
				SetLineColor(yellow);
				m_title.text = "メンテナンス時間のご案内";
				m_message.text = "まもなくメンテナンス時間となるためプレー開始できません。\nメンテナンス時間終了後にご利用ください。";
			}
			break;
		case GameSettings.ShopClosePhase.Final:
			if (m_phase != shopClosePhase)
			{
				m_root.SetActive(value: true);
				SetLineColor(yellow);
				m_title.text = "サーバーメンテナンス中";
				m_message.text = "ただいまサーバーメンテナンス中です。\nメンテナンス時間終了後にご利用ください。";
			}
			break;
		}
		m_phase = shopClosePhase;
	}

	private void Update()
	{
		if (!s_isActive)
		{
			m_root.SetActive(value: false);
		}
		else if (GameUtility.IsKamunityBootEnable())
		{
			UpdatePhase();
		}
	}
}

using System;
using TMPro;
using TestModeSkeleton;
using UnityEngine;

public class ShopCloseMessage : SingletonBehaviour<ShopCloseMessage>
{
	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private TextMeshProUGUI m_message;

	private bool m_isActive;

	private GameSettings.ShopClosePhase m_phase;

	private const int SEC_PER_DAY = 86400;

	private const int MINUTE_PER_DAY = 1440;

	private const int MINUTE_PER_HOUR = 60;

	private const int SEC_PER_MINUTE = 60;

	private const int REMAIN_TIME_TO_NOTICE = 1800;

	private const int REMAIN_TIME_TO_ENFORCE = 600;

	private void Update()
	{
		if (GameUtility.IsKamunityBootEnable() && m_isActive)
		{
			GameSettings.ShopClosePhase shopClosePhase = ShopCloseUtility.GetShopClosePhase();
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				m_phase = shopClosePhase;
				OnChangePhase();
			}
			else if (shopClosePhase != m_phase)
			{
				m_phase = shopClosePhase;
				OnChangePhase();
			}
		}
	}

	private void OnChangePhase()
	{
		switch (m_phase)
		{
		default:
			Window_SetDisp(isDisp: false);
			return;
		case GameSettings.ShopClosePhase.Notice:
			if (GameUtility.IsKonastaOrNetcafeBehavior)
			{
				var (flag, num) = ShopCloseUtility.QueryServerMaintenanceSchedule();
				if (flag)
				{
					int num2 = 1800;
					int num3 = (num - num2) / 60;
					if (num3 < 0)
					{
						num3 = 0;
					}
					m_message.text = $"まもなくメンテナンス時間となります。\nあと{num3}分でプレー開始できなくなりますのでご注意ください。";
				}
			}
			else
			{
				string text = TimeSpan.FromMinutes(CabinetManager.Game.Data.ShopCloseTimeDayOfWeek[(int)ShopCloseUtility.GetDayOfWeek()]).ToString("hhmm");
				m_message.text = string.Format(TextManager.Get("SHOP_CLOSE_NOTICE"), text.Substring(0, 2), text.Substring(2, 2));
			}
			break;
		case GameSettings.ShopClosePhase.Enforce:
			m_message.text = (GameUtility.IsKonastaOrNetcafeBehavior ? "まもなくメンテナンス時間となるためプレー開始できません。メンテナンス時間終了後にご利用ください。" : TextManager.Get("SHOP_CLOSE_ENFORCE"));
			break;
		case GameSettings.ShopClosePhase.Final:
			m_message.text = (GameUtility.IsKonastaOrNetcafeBehavior ? "ただいまサーバーメンテナンス中です。メンテナンス時間終了後にご利用ください。" : TextManager.Get("SHOP_CLOSE_FINAL"));
			break;
		}
		Window_SetDisp(!string.IsNullOrEmpty(m_message.text));
	}

	public void SetActive(bool active)
	{
		m_isActive = active;
		if (active)
		{
			m_phase = ShopCloseUtility.GetShopClosePhase();
			OnChangePhase();
		}
		else
		{
			Window_SetDisp(isDisp: false);
		}
	}

	private void Window_SetDisp(bool isDisp)
	{
		if (m_root != null)
		{
			m_root.SetActive(isDisp);
		}
	}
}

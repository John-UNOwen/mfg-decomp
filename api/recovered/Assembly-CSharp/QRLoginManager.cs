using Cysharp.Threading.Tasks;
using KAMUNITY;
using UnityEngine;

public static class QRLoginManager
{
	public static async UniTask<bool> QRLibraryEnableCheck()
	{
		if (GameUtility.IsKamunityBootEnable() && !GameUtility.IsKonastaOrNetcafeBehavior && GameDataManager.IsQRLoginEnable())
		{
			float time = Time.time;
			while (Time.time - time < 20f)
			{
				await UniTask.Yield();
				switch (QRLogin.GetStatus())
				{
				case QRLogin.STATUS.EAX_SPPASS_ERROR_CLOSE:
					return true;
				case QRLogin.STATUS.EAX_SPPASS_ERROR_MAINTENANCE:
					return false;
				}
			}
		}
		return false;
	}

	public static async UniTask<Sprite> QRBeginToDisplay()
	{
		QRLogin.Begin();
		QRLogin.STATUS sTATUS = QRLogin.STATUS.EAX_SPPASS_ERROR_OPENING;
		float time = Time.time;
		while (sTATUS == QRLogin.STATUS.EAX_SPPASS_ERROR_OPENING)
		{
			await UniTask.Yield();
			sTATUS = QRLogin.GetStatus();
			if (Time.time - time >= 20f)
			{
				break;
			}
		}
		if (sTATUS > QRLogin.STATUS.EAX_SPPASS_ERROR_OPENING)
		{
			return QRLogin.GetQRCode();
		}
		QRLogin.End();
		return null;
	}
}

using System;
using System.Security.Cryptography;
using KAMUNITY;
using MFG.Types;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class AccountGameData : GameDataBase
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public string CardID = "";

	public AvsTypes.CardType CardType;

	public string CardNumber = "";

	public string RefID = "";

	public string DataID = "";

	public string AppSessionID = "";

	public bool IsCardless;

	public int PlayerID;

	public bool IsForceFreePlayUser;

	public bool IsGuestUser;

	public GachaTicketCounter GachaTicketCounter = new GachaTicketCounter();

	[NonSerialized]
	public bool IsPremiumPlayer;

	public ParticipationBillingEvent ParticipationBillingEvent = new ParticipationBillingEvent();

	public bool IsFirstPlay { get; set; }

	public GameModeType GameMode { get; set; }

	public bool IsTaikyokuEnd { get; set; }

	public bool IsPremiumMode()
	{
		return IsPremiumPlayer;
	}

	public void NotifyGuestUserStart()
	{
		CardID = "";
		RefID = "";
		CardNumber = "";
		CardType = AvsTypes.CardType.MCARD;
		IsCardless = false;
		IsGuestUser = true;
		DataID = CreateGuestUserID();
		Debug.Log("GuestUserID--> data_id:" + DataID);
	}

	private string CreateGuestUserID()
	{
		uint num = 0u;
		num = ((!GameUtility.IsKamunityBootEnable()) ? ((uint)DateTimeOffset.Now.ToUnixTimeSeconds()) : ((uint)Avs.TimeGet(AvsTypes.AVS_CLOCK.SERVER)));
		uint num2 = 0u;
		using (RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider())
		{
			byte[] array = new byte[4];
			rNGCryptoServiceProvider.GetBytes(array);
			num2 = BitConverter.ToUInt32(array, 0);
		}
		return $"{num:X8}{num2:X8}";
	}

	public void ResetForContinueStart()
	{
		IsFirstPlay = false;
	}
}

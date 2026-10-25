using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EagDef;
using IO;
using KAMUNITY;
using Konami.System.Net;
using MFG.GameData;
using SYSTEM;
using TCSNET;
using UnityEngine;

public class TitleDemoUtility
{
	private static string s_cardID;

	private static AvsTypes.CardType s_cardType;

	private static DateTimeOffset s_logLastSendTime;

	public static bool IsServiceUrlModified;

	public static bool EACard_IsDetected()
	{
		if (!IsGameEntryAcceptable())
		{
			return false;
		}
		if (Bi2xInput.Instance.IsCardOn())
		{
			s_cardID = Bi2xInput.Instance.GetCardID();
			s_cardType = Bi2xInput.Instance.GetCardType();
			return true;
		}
		return false;
	}

	public static void EACard_ResetDetectedCard()
	{
		s_cardID = null;
		s_cardType = AvsTypes.CardType.MCARD;
	}

	public static (string cardID, AvsTypes.CardType cardType) EACard_GetDetectedCard()
	{
		return (cardID: s_cardID, cardType: s_cardType);
	}

	public static bool IsNetworkActive()
	{
		int code;
		int subcode;
		if (GameUtility.IsKamunityBootEnable())
		{
			return EagCore.Ea3GetStatus(out code, out subcode) == AvsTypes.EA3_STATUS.ONLINE;
		}
		return true;
	}

	public static bool IsNetworkMaintenance()
	{
		int code;
		int subcode;
		if (GameUtility.IsKamunityBootEnable())
		{
			return EagCore.Ea3GetStatus(out code, out subcode) == AvsTypes.EA3_STATUS.MAINTE;
		}
		return false;
	}

	public static bool IsParticipationError()
	{
		if (DeviceManager.Instance.Ikey().IsFactoryBoot)
		{
			return false;
		}
		if (GameUtility.IsKamunityBootEnable())
		{
			if (EagCore.Instance.Status == STATUS.OFFLINE)
			{
				return true;
			}
			AvsTypes.EA3_BOOT_CODE code = (AvsTypes.EA3_BOOT_CODE)EagCore.Instance.Code;
			if ((uint)(code - 2014) <= 1u)
			{
				return true;
			}
			AvsTypes.EA3_PP_STATUS eA3_PP_STATUS = EagCore.Ea3GetPpStatus();
			if (eA3_PP_STATUS == AvsTypes.EA3_PP_STATUS.BOOT || eA3_PP_STATUS == AvsTypes.EA3_PP_STATUS.NOT_AVAILABLE)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsNeedAdvanceToPPErrorScene()
	{
		if (SystemUtility.Instance.PPtype == PARTICIPATIONTYPE.NONE)
		{
			return false;
		}
		if (IsParticipationError())
		{
			if (SystemUtility.Instance.RegionType != REGIONTYPE.KOREA)
			{
				return true;
			}
			if (EagCore.Instance.Code == 2014)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsTouchSkipActionDetected()
	{
		if (GameUtility.IsKonastaOrNetcafeBehavior && (ResidentFlow.GetInstance().IsAppTerminateConfirmWindowOpen || GameUtility.IsAppTerminatDecided))
		{
			return false;
		}
		if (Bi2xInput.Instance.IsTouchTrigger())
		{
			Debug.Log("IsTouchSkipActionDetected true");
			return true;
		}
		return false;
	}

	public static void OnPreEnterLoginFlow()
	{
		Debug.Log("OnPreEnterLoginFlow");
		ResidentFlow.GetInstance().IsTestModeEffective = false;
		ResidentFlow.GetInstance().m_isTitleDemoMahjongPlayAlive = false;
		ResidentFlow.GetInstance().SetInGameDisp(isDisp: false);
		SoundManager.SetGamePlayVolume();
		SoundManager.PlaySe("SFX_SYS_START1").Forget();
		GameDataManager.Reset();
		Debug.Log($"IsCardOn:{Bi2xInput.Instance.IsCardOn()}, CardID:{Bi2xInput.Instance.GetCardID()}, CardType:{Bi2xInput.Instance.GetCardType()}");
		Debug.Log("Before Account.CardID:" + GameDataManager.Account.CardID);
		Debug.Log($"Before Account.CardType:{GameDataManager.Account.CardType}");
		if (GameUtility.IsKonastaOrNetcafeBehavior && GameUtility.IsKamunityBootEnable())
		{
			GameDataManager.Account.CardID = Avs.StdGetEnv("/env/eacloud/agentarg/card_id");
			AccountGameData account = GameDataManager.Account;
			account.CardType = Avs.StdGetEnv("/env/eacloud/agentarg/card_type") switch
			{
				"0" => AvsTypes.CardType.MCARD, 
				"1" => AvsTypes.CardType.ICCARD, 
				"2" => AvsTypes.CardType.FELICA, 
				"4" => AvsTypes.CardType.VIRTUAL, 
				"5" => AvsTypes.CardType.CARDLESS, 
				_ => AvsTypes.CardType.ICCARD, 
			};
		}
		else
		{
			var (text, cardType) = EACard_GetDetectedCard();
			if (!string.IsNullOrEmpty(text))
			{
				GameDataManager.Account.CardID = text;
				GameDataManager.Account.CardType = cardType;
			}
		}
		GameDataManager.Account.IsCardless = false;
		EACard_ResetDetectedCard();
		Debug.Log("After Account.CardID:" + GameDataManager.Account.CardID);
		Debug.Log($"After Account.CardType:{GameDataManager.Account.CardType}");
		SoundManager.StopBgmWithFadeout().Forget();
		SoundManager.StopVoiceAllWithFadeout().Forget();
	}

	public static bool IsGameEntryAcceptable()
	{
		if (GameUtility.IsEAmuseActive() && IsNetworkActive() && !IsParticipationError() && ShopCloseUtility.IsEntryAcceptable() && LogoFlow.IsLogoSceneOnceDisplayed)
		{
			return EagCore.Instance.Status != STATUS.MAINTE;
		}
		return false;
	}

	public static bool IsGameEntryActionDetected(bool coinInserted)
	{
		bool flag = EACard_IsDetected();
		bool flag2 = IsTouchSkipActionDetected() || coinInserted;
		bool flag3 = Bi2xInput.IsTrigger(BUTTON.Q_BUTTON);
		if (flag || flag2 || flag3)
		{
			Debug.Log($"IsGameEntryActionDetected isCardDetected:{flag}, isTouched{IsTouchSkipActionDetected()}, coinInserted:{coinInserted}, isQButtonPush:{flag3}");
			return true;
		}
		return false;
	}

	public static bool IsGameEntryActionDetectedExceptTouch(bool coinInserted)
	{
		bool flag = EACard_IsDetected();
		bool flag2 = coinInserted;
		bool flag3 = Bi2xInput.IsTrigger(BUTTON.Q_BUTTON);
		if (flag || flag2 || flag3)
		{
			Debug.Log($"IsGameEntryActionDetectedExceptTouch isCardDetected:{flag}, coinInserted:{coinInserted}, isQButtonPush:{flag3}");
			return true;
		}
		return false;
	}

	public static bool GameEntryContext_ActivateIfGameEntryAcceptable()
	{
		bool flag = IsGameEntryAcceptable() && !GameUtility.IsFreePlay();
		DeviceManager.Instance.Coin().Activate(flag);
		return flag;
	}

	public static async UniTask SendServerLogs()
	{
		bool flag = false;
		if (s_logLastSendTime == default(DateTimeOffset))
		{
			s_logLastSendTime = DateTimeOffsetManager.GetJSTTime();
			flag = true;
		}
		if (flag)
		{
			GameUtility.ServerLog.Reserve(CreatePCBLogs());
			s_logLastSendTime = DateTimeOffsetManager.GetJSTTime();
		}
		await GameUtility.ServerLog.SendReservedLog();
	}

	private static List<(string, string)> CreatePCBLogs()
	{
		string item = SystemUtility.GenerateOSActivationDateString();
		string item2 = SystemUtility.StorageInfo.GenerateStorageInfoJsonString();
		return new List<(string, string)>
		{
			("activation_info", item),
			("storage_info", item2)
		};
	}

	public static async UniTask ModifyServiceUrl()
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			XrpcResponseHelper<TcsXrpcModuleTypes.GetServiceListResponse> xrpcHelper = new XrpcResponseHelper<TcsXrpcModuleTypes.GetServiceListResponse>();
			TcsXrpcModule.XrpcMethodGetServiceList.Request(xrpcHelper.Callback, null);
			await xrpcHelper.CoWaitForXrpcFinish();
			if (!xrpcHelper.isError)
			{
				SystemConfigGameData.FrontServerUrl = xrpcHelper.result.ServiceUrl;
				IsServiceUrlModified = true;
			}
		}
		else
		{
			SystemConfigGameData.FrontServerUrl = NetEnvironment.Instance.Connection.RequestUrl;
			IsServiceUrlModified = true;
		}
	}
}

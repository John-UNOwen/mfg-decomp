using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EagDef;
using KAMUNITY;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ParticipationErrorScene : BaseFlow
{
	[SerializeField]
	private ErrorModeDialog m_Dialog;

	[SerializeField]
	private CanvasGroup m_group;

	[SerializeField]
	private GameObject _ObjHelpTest;

	[SerializeField]
	private Button _endButton;

	private ParticipationErrorScene()
	{
		_enableDebugLogin = false;
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		DeviceManager.Instance.AllActivate(bActive: false);
		Bi2xInput.Instance.ICReaderOff();
		CreditCoinDisp.IsForceDisp = true;
		SoundManager.StopAll().Forget();
		SoundManager.SetSpeakerVolume(0f);
		SoundManager.SetHeadPhoneVolume(0f);
		Bi2xInput.Instance.SetPillarLED(Color.black);
		m_group.DOFade(0.25f, 1f).SetEase(Ease.InSine).SetLoops(-1, LoopType.Yoyo)
			.ToUniTask()
			.Forget();
		ResidentFlow.GetInstance().IsTestModeRequest = false;
		ResidentFlow.GetInstance().IsTestModeEffective = !GameUtility.IsKonastaOrNetcafeBehavior;
		_ObjHelpTest.SetActive(!GameUtility.IsKonastaOrNetcafeBehavior);
		ResidentFlow.GetInstance().SetInGameDisp(isDisp: false, edgeTouchingWarning: false);
		_endButton.gameObject.SetActive(GameUtility.IsKonastaOrNetcafeBehavior);
		ErrorModeDialog_Reflesh();
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			_endButton.OnClickAsObservable().Subscribe(delegate
			{
				Debug.Log("End button clicked.");
				_endButton.enabled = false;
				GameUtility.IsAppTerminatDecided = true;
				ResidentFlow.GetInstance().IsAppTerminateEffective = false;
				ResidentFlow.GetInstance().IsAppTerminated = true;
				GotoNextScene("ScopeClose");
			}).AddTo(this);
		}
		base.OnUpdateAsObservable.Where((Unit _) => !IsUnrecoverableError()).Subscribe(delegate
		{
			ErrorModeDialog_Reflesh();
			if (EagCore.Ea3GetPpStatus() == AvsTypes.EA3_PP_STATUS.AVAILABLE)
			{
				GotoNextScene("Logo");
			}
		}).AddTo(this);
	}

	private void ErrorModeDialog_Reflesh()
	{
		EagCore instance = EagCore.Instance;
		AvsTypes.EA3_PP_STATUS eA3_PP_STATUS = EagCore.Ea3GetPpStatus();
		int code = instance.Code;
		int subCode = instance.SubCode;
		m_Dialog.gameObject.SetActive(eA3_PP_STATUS != AvsTypes.EA3_PP_STATUS.AVAILABLE);
		if (code == 0 && eA3_PP_STATUS != AvsTypes.EA3_PP_STATUS.AVAILABLE)
		{
			string title = TextManager.Get("DEMO_CHECK_NETWORK");
			string message = TextManager.Get("DEMO_CHECK_NETWORK_MSG");
			m_Dialog.SetData(title, "", message);
			return;
		}
		ErrorData.Item item = ErrorData.GetInstance().GetItem((ERROR)code, (uint)subCode);
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			item.title = "エラーが発生しました";
			item.message = "終了後、しばらく待ってからもう一度お試しください。";
		}
		m_Dialog.SetData(item);
	}

	public void OnDestroy()
	{
		CreditCoinDisp.IsForceDisp = false;
		DeviceManager.Instance.AllActivate(bActive: true);
	}

	private bool IsUnrecoverableError()
	{
		EagCore instance = EagCore.Instance;
		if (instance.Status == STATUS.OFFLINE)
		{
			return true;
		}
		AvsTypes.EA3_BOOT_CODE code = (AvsTypes.EA3_BOOT_CODE)instance.Code;
		if ((uint)(code - 2014) <= 1u)
		{
			return true;
		}
		return false;
	}
}

using System.Threading;
using Cysharp.Threading.Tasks;
using IO;
using KAMUNITY;
using TMPro;
using UnityEngine;

public class NetworkConfirm : BaseFlow
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	private NetworkConfirm()
	{
		_enableDebugLogin = false;
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		m_text.text = TextManager.Get("ERROR_EA_SERVICE");
		m_text.text += TextManager.Get("ERROR_EA_SERVICE_MSG");
		DeviceManager.Instance.AllActivate(bActive: false);
		ResidentFlow.GetInstance().IsTestModeEffective = false;
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		await UniTask.WaitUntil(delegate
		{
			if (Bi2xInput.IsPress(BUTTON.SERVICE) && Bi2xInput.IsTrigger(BUTTON.TEST))
			{
				m_text.text = TextManager.Get("ERROR_EA_SERVICE");
				m_text.text += TextManager.Get("ERROR_EA_SERVICE_CONFIRM_MSG");
				return true;
			}
			if (Bi2xInput.IsTrigger(BUTTON.TEST))
			{
				GotoNextScene("TestModeMain");
				return true;
			}
			return false;
		}, PlayerLoopTiming.Update, cancellationToken);
		if (_isNextSceneDecided)
		{
			return;
		}
		await UniTask.WaitUntil(delegate
		{
			if (Bi2xInput.IsPress(BUTTON.SERVICE) && Bi2xInput.IsTrigger(BUTTON.TEST))
			{
				EagUtility.ConfirmAgree();
				return true;
			}
			if (Bi2xInput.IsTrigger(BUTTON.TEST))
			{
				GotoNextScene("TestModeMain");
				return true;
			}
			return false;
		}, PlayerLoopTiming.Update, cancellationToken);
		if (_isNextSceneDecided)
		{
			return;
		}
		await UniTask.WaitUntil(delegate
		{
			if (!EagUtility.IsNeedConfirm() && EagCore.Ea3GetPpStatus() == AvsTypes.EA3_PP_STATUS.AVAILABLE)
			{
				GotoNextScene("Logo");
				return true;
			}
			return false;
		}, PlayerLoopTiming.Update, cancellationToken);
	}
}

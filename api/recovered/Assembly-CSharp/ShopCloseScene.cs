using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ShopCloseScene : BaseFlow
{
	[SerializeField]
	private TextMeshProUGUI _messageText;

	private ShopCloseScene()
	{
		_enableDebugLogin = false;
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		ResidentFlow.GetInstance().SetInGameDisp(isDisp: false, edgeTouchingWarning: false);
		SoundManager.StopBgm().Forget();
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			_messageText.text = "サーバーメンテナンス中<br><br><br>ただいまサーバーメンテナンス中です。<br>メンテナンス時間終了後にご利用ください。";
			float time = Time.time;
			await UniTask.WaitUntil(() => Time.time - time >= 60f || ResidentFlow.CustomInput.GetMouseButtonDown(0));
			GotoNextScene("ScopeClose");
		}
	}
}

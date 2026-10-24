using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Scenes.MyRoom.Catchphrase;
using UnityEngine;

public class MyRoomDetailWindow : MonoBehaviour
{
	[SerializeField]
	private MyRoomFavoWindow m_favoWindow;

	[SerializeField]
	private MyRoomFanWindow m_fanWindow;

	[SerializeField]
	private MyRoomSpiritWindow m_spiritWindow;

	[SerializeField]
	private MyRoomCutinWindow m_cutinWindow;

	[SerializeField]
	private MyRoomCharaSelectWindow m_charaSelectWindow;

	[SerializeField]
	private MyRoomCatchphraseWindow m_catchphraseWindow;

	[SerializeField]
	private MyRoomMatchingBGWindow m_matchingBGWindow;

	[SerializeField]
	private MyRoomFriendConfirmWindow m_friendConfirmWindow;

	[SerializeField]
	private MyRoomStickerEditor m_stickerEditorWindow;

	[SerializeField]
	private GameObject Bg;

	private MyRoomFlow.WindowType OpeningWindowType;

	private MyRoomFlow myRoomFlow;

	public void Init(MyRoomFlow mrf, CancellationToken cancellationToken)
	{
		myRoomFlow = mrf;
		m_charaSelectWindow.Init(mrf, cancellationToken).Forget();
		m_friendConfirmWindow.Init(mrf);
		m_stickerEditorWindow.Init(mrf);
	}

	public async UniTask WindowOpen(MyRoomFlow.WindowType windowType, CancellationToken cancellationToken, params object[] args)
	{
		if (windowType != OpeningWindowType)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			myRoomFlow.SceneButtonSetActive(isActive: false);
			Bg.SetActive(value: true);
			OpeningWindowType = windowType;
			GetComponent<Animator>().SetTrigger("In");
			switch (windowType)
			{
			case MyRoomFlow.WindowType.FavoWindow:
				m_favoWindow.gameObject.SetActive(value: true);
				await m_favoWindow.Play(myRoomFlow);
				((MyRoomDisplayBase)args[0]).SetStatus(cancellationToken);
				break;
			case MyRoomFlow.WindowType.FanWindow:
				m_fanWindow.gameObject.SetActive(value: true);
				await m_fanWindow.Play(cancellationToken);
				((MyRoomDisplayBase)args[0]).SetStatus(cancellationToken);
				break;
			case MyRoomFlow.WindowType.SpiritWindow:
				m_spiritWindow.gameObject.SetActive(value: true);
				await m_spiritWindow.Play(myRoomFlow, cancellationToken);
				((MyRoomDisplayBase)args[0]).SetStatus(cancellationToken);
				break;
			case MyRoomFlow.WindowType.CutinWindow:
				m_cutinWindow.gameObject.SetActive(value: true);
				await m_cutinWindow.Play(myRoomFlow, cancellationToken);
				((MyRoomDisplayBase)args[0]).SetStatus(cancellationToken);
				break;
			case MyRoomFlow.WindowType.CharaSelectWindow:
				m_charaSelectWindow.gameObject.SetActive(value: true);
				await m_charaSelectWindow.Play();
				break;
			case MyRoomFlow.WindowType.CatchphraseWindow:
				m_catchphraseWindow.gameObject.SetActive(value: true);
				await m_catchphraseWindow.Open(myRoomFlow, GameDataManager.Player.SelectChara, cancellationToken);
				((MyRoomDisplayBase)args[0]).SetStatus(cancellationToken);
				break;
			case MyRoomFlow.WindowType.MatchingBGWindow:
				m_matchingBGWindow.gameObject.SetActive(value: true);
				await m_matchingBGWindow.Play(cancellationToken);
				((MyRoomDisplayBase)args[0]).SetStatus(cancellationToken);
				break;
			case MyRoomFlow.WindowType.FriendConfirmWindow:
				m_friendConfirmWindow.gameObject.SetActive(value: true);
				await m_friendConfirmWindow.Play((FriendInfo)args[0], cancellationToken);
				break;
			case MyRoomFlow.WindowType.StickerEditor:
				await m_stickerEditorWindow.Play(cancellationToken);
				break;
			}
			await WindowClose(cancellationToken);
		}
	}

	public async UniTask WindowClose(CancellationToken cancellationToken)
	{
		if (OpeningWindowType == MyRoomFlow.WindowType.None)
		{
			return;
		}
		GetComponent<Animator>().SetTrigger("Out");
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		await UniTask.WaitWhile(() => GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
		myRoomFlow.SceneButtonSetActive(isActive: true);
		OpeningWindowType = MyRoomFlow.WindowType.None;
		await myRoomFlow.MissionOpen();
	}

	public void StopPreview()
	{
		m_favoWindow.BgmPreviewStop();
	}
}

using System;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types.CharaStamp;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CharaStampEventWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private GameObject stampParent;

	[SerializeField]
	private CharaStampID charaStampID;

	[SerializeField]
	private TextMeshProUGUI debugText;

	[SerializeField]
	private GameObject jandriSupport;

	public bool isOpen;

	private async void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
		CharaStampInfo info = GameDataManager.CharaStampEvent.GetInfo(charaStampID);
		if (info != null)
		{
			int num = (info.IsUsable ? info.Master.ClearStampNum : info.StampNum);
			for (int i = 0; i < num; i++)
			{
				stampParent.transform.Find($"Stamp{i + 1}").GetComponent<Animator>().SetTrigger("Loop");
			}
		}
		jandriSupport.SetActive(GameDataManager.IsLoadJandriAppear());
	}

	public void OpenWindow()
	{
		if (!isOpen)
		{
			isOpen = true;
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("In");
		}
	}

	public async UniTask CloseWindow()
	{
		if (isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}
}

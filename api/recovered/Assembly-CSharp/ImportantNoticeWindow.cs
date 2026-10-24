using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types.ImportantNotice;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ImportantNoticeWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameObject noticeParent;

	[SerializeField]
	private Button closeButton;

	[NonSerialized]
	public bool isOpen;

	[NonSerialized]
	public int openNoticeID;

	[NonSerialized]
	public GameObject openObject;

	[NonSerialized]
	public List<int> readList = new List<int>();

	private async void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	public void OpenWindow(int id)
	{
		if (!isOpen)
		{
			try
			{
				openNoticeID = id;
				Transform obj = noticeParent.transform;
				NoticeID noticeID = (NoticeID)openNoticeID;
				openObject = obj.Find(noticeID.ToString()).gameObject;
				openObject.SetActive(value: true);
			}
			catch
			{
				return;
			}
			Item item = ImportantNoticeMaster.GetItem((NoticeID)openNoticeID);
			ResidentFlow.GetInstance().GetTimer().SetTimer(item.DisplayTime, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			titleText.text = item.Title;
			closeButton.gameObject.SetActive(item.IsCloseButtonDisp);
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
			readList.Add(openNoticeID);
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			base.gameObject.SetActive(value: false);
			openObject.SetActive(value: false);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}
}

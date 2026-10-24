using System;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CutinRecordWindow : MonoBehaviour
{
	[SerializeField]
	private Button _returnButton;

	[SerializeField]
	private GameObject _btList;

	[SerializeField]
	private GameObject _btPrefab;

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		try
		{
			this.GetCancellationTokenOnDestroy();
			_returnButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				Close();
			}).AddTo(this);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	public void Play()
	{
		foreach (Transform item in _btList.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().SetTrigger("In");
		foreach (Item item2 in CutinItemMaster.GetItems((Item x) => x.CharaType == GameDataManager.Player.GetCurrentChara().charaType))
		{
			GameObject obj = UnityEngine.Object.Instantiate(_btPrefab, _btList.transform);
			obj.transform.GetComponent<CutinRecordDisp>().itemID = item2.ItemID;
			obj.transform.Find("Button1/Text").GetComponent<TextMeshProUGUI>().text = CutinItemMaster.GetItem(item2.ItemID).Name;
			obj.transform.Find("Button1").GetComponent<Button>().OnClickAsObservable()
				.Subscribe(async delegate
				{
					SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
					obj.transform.GetComponent<CutinRecordDisp>().Play();
					Close();
				})
				.AddTo(this);
		}
	}

	public void Close()
	{
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		GetComponent<Animator>().SetTrigger("Out");
	}
}

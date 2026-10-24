using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types.Customize.CustomizeItem;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyRoomStickerEditor : MonoBehaviour
{
	public class EditInfo
	{
		public StickerEditInfo Info;

		public GameObject Obj;
	}

	[SerializeField]
	private Button decideButton;

	[SerializeField]
	private Transform stickerParent;

	[SerializeField]
	private GameObject stickerObj;

	[SerializeField]
	private Button flontButton;

	[SerializeField]
	private Button backButton;

	[SerializeField]
	private Button defaultButton;

	[SerializeField]
	private Button selectLeftButton;

	[SerializeField]
	private Button selectRightButton;

	[SerializeField]
	private TextMeshProUGUI itemName;

	[SerializeField]
	private TextMeshProUGUI priorityIndex;

	[SerializeField]
	private Button btUp;

	[SerializeField]
	private Button btDown;

	[SerializeField]
	private Button btLeft;

	[SerializeField]
	private Button btRight;

	[NonSerialized]
	public ItemID currentItem;

	private int goodCount;

	[NonSerialized]
	public int PosXMinValue = -180;

	[NonSerialized]
	public int PosXMaxValue = 180;

	[NonSerialized]
	public int PosYMinValue = -90;

	[NonSerialized]
	public int PosYMaxValue = 30;

	[NonSerialized]
	private int range = 10;

	[NonSerialized]
	private float waitTime = 0.5f;

	[NonSerialized]
	private float intervalTime = 0.1f;

	[NonSerialized]
	public Vector2 CurrentPos;

	public List<EditInfo> EditInfos;

	private bool isConfirmOpen;

	private bool isApplyingValue;

	private MyRoomFlow myRoomFlow;

	public EditInfo CurrentInfo => EditInfos.Find((EditInfo x) => x.Info.ItemID == currentItem);

	public void Init(MyRoomFlow mrf)
	{
		myRoomFlow = mrf;
		ObservableEventTrigger btUpTrigger = btUp.gameObject.AddComponent<ObservableEventTrigger>();
		btUpTrigger.OnPointerDownAsObservable().Subscribe(delegate
		{
			if (CurrentPos.y + (float)range <= (float)PosYMaxValue)
			{
				PosYChange(CurrentPos.y + (float)range);
			}
		}).AddTo(this);
		btUpTrigger.OnPointerDownAsObservable().SelectMany((PointerEventData _) => Observable.Timer(TimeSpan.FromSeconds(waitTime)).TakeUntil(btUpTrigger.OnPointerUpAsObservable()).SelectMany((long _) => Observable.Interval(TimeSpan.FromSeconds(intervalTime)).TakeUntil(btUpTrigger.OnPointerUpAsObservable()))).Subscribe(delegate
		{
			if (CurrentPos.y + (float)range <= (float)PosYMaxValue)
			{
				PosYChange(CurrentPos.y + (float)range);
			}
		})
			.AddTo(this);
		ObservableEventTrigger btDownTrigger = btDown.gameObject.AddComponent<ObservableEventTrigger>();
		btDownTrigger.OnPointerDownAsObservable().Subscribe(delegate
		{
			if (CurrentPos.y - (float)range >= (float)PosYMinValue)
			{
				PosYChange(CurrentPos.y - (float)range);
			}
		}).AddTo(this);
		btDownTrigger.OnPointerDownAsObservable().SelectMany((PointerEventData _) => Observable.Timer(TimeSpan.FromSeconds(waitTime)).TakeUntil(btDownTrigger.OnPointerUpAsObservable()).SelectMany((long _) => Observable.Interval(TimeSpan.FromSeconds(intervalTime)).TakeUntil(btDownTrigger.OnPointerUpAsObservable()))).Subscribe(delegate
		{
			if (CurrentPos.y - (float)range >= (float)PosYMinValue)
			{
				PosYChange(CurrentPos.y - (float)range);
			}
		})
			.AddTo(this);
		ObservableEventTrigger btLeftTrigger = btLeft.gameObject.AddComponent<ObservableEventTrigger>();
		btLeftTrigger.OnPointerDownAsObservable().Subscribe(delegate
		{
			if (CurrentPos.x - (float)range >= (float)PosXMinValue)
			{
				PosXChange(CurrentPos.x - (float)range);
			}
		}).AddTo(this);
		btLeftTrigger.OnPointerDownAsObservable().SelectMany((PointerEventData _) => Observable.Timer(TimeSpan.FromSeconds(waitTime)).TakeUntil(btLeftTrigger.OnPointerUpAsObservable()).SelectMany((long _) => Observable.Interval(TimeSpan.FromSeconds(intervalTime)).TakeUntil(btLeftTrigger.OnPointerUpAsObservable()))).Subscribe(delegate
		{
			if (CurrentPos.x - (float)range >= (float)PosXMinValue)
			{
				PosXChange(CurrentPos.x - (float)range);
			}
		})
			.AddTo(this);
		ObservableEventTrigger btRightTrigger = btRight.gameObject.AddComponent<ObservableEventTrigger>();
		btRightTrigger.OnPointerDownAsObservable().Subscribe(delegate
		{
			if (CurrentPos.x + (float)range <= (float)PosXMaxValue)
			{
				PosXChange(CurrentPos.x + (float)range);
			}
		}).AddTo(this);
		btRightTrigger.OnPointerDownAsObservable().SelectMany((PointerEventData _) => Observable.Timer(TimeSpan.FromSeconds(waitTime)).TakeUntil(btRightTrigger.OnPointerUpAsObservable()).SelectMany((long _) => Observable.Interval(TimeSpan.FromSeconds(intervalTime)).TakeUntil(btRightTrigger.OnPointerUpAsObservable()))).Subscribe(delegate
		{
			if (CurrentPos.x + (float)range <= (float)PosXMaxValue)
			{
				PosXChange(CurrentPos.x + (float)range);
			}
		})
			.AddTo(this);
		decideButton.OnClickAsObservable().Subscribe(async delegate
		{
			GameDataManager.Player.GetCurrentChara().StickerEditInfos = EditInfos.Select((EditInfo x) => x.Info).ToList();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			base.gameObject.SetActive(value: false);
			isConfirmOpen = false;
		}).AddTo(this);
		defaultButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			CurrentInfo.Info.IsEdited = true;
			CurrentInfo.Info.PositionX = ((!CurrentInfo.Info.Master.IsBigSize) ? 100 : 0);
			CurrentInfo.Info.PositionY = (CurrentInfo.Info.Master.IsBigSize ? (-30) : (-50));
			CurrentInfo.Info.EulerRotate = 0;
			CurrentInfo.Obj.transform.localPosition = (CurrentInfo.Info.Master.IsBigSize ? new Vector2(0f, -30f) : new Vector2(100f, -50f));
			CurrentInfo.Obj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			CurrentPos = (CurrentInfo.Info.Master.IsBigSize ? new Vector2(0f, -30f) : new Vector2(100f, -50f));
		}).AddTo(this);
		flontButton.OnClickAsObservable().Subscribe(async delegate
		{
			EditInfo editInfo2 = EditInfos.Find((EditInfo x) => x.Info.Priority == CurrentInfo.Info.Priority + 1);
			if (editInfo2 != null)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				CurrentInfo.Info.IsEdited = true;
				editInfo2.Info.IsEdited = true;
				editInfo2.Info.Priority--;
				CurrentInfo.Info.Priority++;
				CurrentInfo.Obj.transform.SetSiblingIndex(CurrentInfo.Info.Priority);
				priorityIndex.text = $"{CurrentInfo.Info.Priority + 1} / {goodCount}";
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		}).AddTo(this);
		backButton.OnClickAsObservable().Subscribe(async delegate
		{
			EditInfo editInfo = EditInfos.Find((EditInfo x) => x.Info.Priority == CurrentInfo.Info.Priority - 1);
			if (editInfo != null)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				CurrentInfo.Info.IsEdited = true;
				editInfo.Info.IsEdited = true;
				editInfo.Info.Priority++;
				CurrentInfo.Info.Priority--;
				CurrentInfo.Obj.transform.SetSiblingIndex(CurrentInfo.Info.Priority);
				priorityIndex.text = $"{CurrentInfo.Info.Priority + 1} / {goodCount}";
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		}).AddTo(this);
		selectLeftButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			int nextPriority2 = ((CurrentInfo.Info.Priority == 0) ? (goodCount - 1) : (CurrentInfo.Info.Priority - 1));
			SelectImageChanger(EditInfos.Find((EditInfo x) => x.Info.Priority == nextPriority2).Info.ItemID);
		}).AddTo(this);
		selectRightButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			int nextPriority = ((CurrentInfo.Info.Priority != goodCount - 1) ? (CurrentInfo.Info.Priority + 1) : 0);
			SelectImageChanger(EditInfos.Find((EditInfo x) => x.Info.Priority == nextPriority).Info.ItemID);
		}).AddTo(this);
	}

	private void SetSliderValue(EditInfo info)
	{
		int num = (int)info.Obj.transform.Find("Image").GetComponent<RectTransform>().sizeDelta.x;
		int num2 = (256 - num) / 20;
		PosXMinValue = -180 - num2 * 10;
		PosXMaxValue = 180 + num2 * 10;
		PosYMinValue = -90 - num2 * 10;
		PosYMaxValue = 30 + num2 * 10;
	}

	public void RefreshButton()
	{
		btUp.interactable = CurrentPos.y < (float)PosYMaxValue;
		btDown.interactable = CurrentPos.y > (float)PosYMinValue;
		btRight.interactable = CurrentPos.x < (float)PosXMaxValue;
		btLeft.interactable = CurrentPos.x > (float)PosXMinValue;
	}

	public async UniTask Play(CancellationToken cancellationToken)
	{
		if (GameDataManager.Player.GetCurrentChara().StickerEditInfos.Count <= 0 || isConfirmOpen)
		{
			return;
		}
		foreach (Transform item in stickerParent)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		EditInfos = new List<EditInfo>();
		goodCount = 0;
		foreach (StickerEditInfo stickerEditInfo in GameDataManager.Player.GetCurrentChara().StickerEditInfos)
		{
			EditInfos.Add(new EditInfo
			{
				Info = stickerEditInfo
			});
			goodCount++;
		}
		currentItem = EditInfos.MaxElement((EditInfo x) => x.Info.Priority).Info.ItemID;
		foreach (EditInfo info in EditInfos.OrderBy((EditInfo x) => x.Info.Priority))
		{
			info.Obj = UnityEngine.Object.Instantiate(stickerObj, stickerParent);
			info.Obj.transform.localPosition = new Vector2(info.Info.PositionX, info.Info.PositionY);
			info.Obj.transform.localEulerAngles = new Vector3(0f, 0f, -info.Info.EulerRotate);
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(CustomizeItemMaster.GetItem(info.Info.ItemID).MahjongPlayDataPath, null, cancellationToken);
			disposableAsset.AddTo(this);
			float x2 = disposableAsset.Asset.rect.size.x;
			info.Obj.transform.Find("Image").GetComponent<RectTransform>().sizeDelta = new Vector2(x2, x2);
			info.Obj.transform.Find("Choice").GetComponent<RectTransform>().sizeDelta = new Vector2(x2 + 12f, x2 + 12f);
			info.Obj.transform.Find("Image").GetComponent<Image>().sprite = disposableAsset.Asset;
			info.Obj.transform.Find("Image").GetComponent<Button>().OnClickAsObservable()
				.Subscribe(delegate
				{
					SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
					SelectImageChanger(info.Info.ItemID);
				})
				.AddTo(this);
			info.Obj.transform.GetComponent<MyRoomStickerDrag>().Init(this, info.Info.ItemID);
		}
		SelectImageChanger(currentItem);
		base.gameObject.SetActive(value: true);
		isConfirmOpen = true;
		await UniTask.WaitWhile(() => isConfirmOpen, PlayerLoopTiming.Update, cancellationToken);
	}

	private void SelectImageChanger(ItemID newItemID)
	{
		isApplyingValue = true;
		currentItem = newItemID;
		foreach (EditInfo editInfo in EditInfos)
		{
			bool flag = currentItem == editInfo.Info.ItemID;
			editInfo.Obj.transform.GetComponent<MyRoomStickerDrag>().ChangeImage(flag);
			if (flag)
			{
				SetSliderValue(editInfo);
				CurrentPos = new Vector2(editInfo.Info.PositionX, editInfo.Info.PositionY);
				RefreshButton();
				itemName.text = CustomizeItemMaster.GetItem(currentItem).DispName ?? "";
				priorityIndex.text = $"{editInfo.Info.Priority + 1} / {goodCount}";
			}
		}
		isApplyingValue = false;
	}

	private void PosXChange(float value)
	{
		if (!isApplyingValue)
		{
			CurrentInfo.Info.IsEdited = true;
			CurrentInfo.Info.PositionX = (int)value;
			CurrentInfo.Obj.transform.localPosition = new Vector2((int)value, CurrentInfo.Info.PositionY);
			CurrentPos = new Vector2((int)value, CurrentInfo.Info.PositionY);
			RefreshButton();
		}
	}

	private void PosYChange(float value)
	{
		if (!isApplyingValue)
		{
			CurrentInfo.Info.IsEdited = true;
			CurrentInfo.Info.PositionY = (int)value;
			CurrentInfo.Obj.transform.localPosition = new Vector2(CurrentInfo.Info.PositionX, (int)value);
			CurrentPos = new Vector2(CurrentInfo.Info.PositionX, (int)value);
			RefreshButton();
		}
	}

	private void RotateChange(float value)
	{
		if (!isApplyingValue)
		{
			CurrentInfo.Info.IsEdited = true;
			CurrentInfo.Info.EulerRotate = (int)value;
			CurrentInfo.Obj.transform.localEulerAngles = new Vector3(0f, 0f, -(int)value);
		}
	}
}

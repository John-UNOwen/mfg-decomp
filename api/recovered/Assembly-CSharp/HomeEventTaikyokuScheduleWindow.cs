using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HomeEventTaikyokuScheduleWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Transform eventGroup;

	[SerializeField]
	private GameObject heldCaution;

	private List<MFG.Types.EventType> _supportedConstancyTakuEvents = new List<MFG.Types.EventType>
	{
		MFG.Types.EventType.FireReach,
		MFG.Types.EventType.AccelDora,
		MFG.Types.EventType.Mentanpin
	};

	public bool isOpen;

	private void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow();
		}).AddTo(this);
	}

	private bool IsEventHeld()
	{
		if (!GameDataManager.EventTaku.AvailableModeGameDatasWithGuest().Any())
		{
			return GameDataManager.EventTaku.CompetitionTakuAvailables().Any();
		}
		return true;
	}

	public void OpenWindow()
	{
		if (isOpen)
		{
			return;
		}
		bool active = IsEventHeld();
		foreach (MFG.Types.EventType supportedConstancyTakuEvent in _supportedConstancyTakuEvents)
		{
			bool active2 = EventTakuMaster.GetItem(supportedConstancyTakuEvent).IsDispConstancySchedule();
			eventGroup.Find(supportedConstancyTakuEvent.ToString() ?? "").gameObject.SetActive(active2);
		}
		foreach (Transform item in eventGroup)
		{
			item.Find("Mask").gameObject.SetActive(active);
		}
		heldCaution.SetActive(active);
		isOpen = true;
		base.gameObject.SetActive(value: true);
		eventGroup.transform.GetComponent<ContentSizeFitter>().SetLayoutVertical();
		LayoutRebuilder.ForceRebuildLayoutImmediate(eventGroup.transform.GetComponent<RectTransform>());
		SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
		windowObj.GetComponent<Animator>().SetTrigger("In");
	}

	public void CloseWindow()
	{
		if (isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
		}
	}

	private void Update()
	{
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomMatchingBGWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollView m_scrollView;

	[SerializeField]
	private GameObject m_actPrefab;

	[SerializeField]
	private List<Sprite> m_rareSprites = new List<Sprite>();

	[SerializeField]
	private Button m_allRemoveButton;

	[SerializeField]
	private TextMeshProUGUI m_itemName;

	[SerializeField]
	private GameObject m_netcafeUnableText;

	private List<MatchingBGInfo> m_dispItemList = new List<MatchingBGInfo>();

	private CharaType m_charaType;

	[SerializeField]
	private Button m_closeButton;

	private bool m_isClosed;

	private CancellationTokenSource _listCts;

	protected void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_isClosed = true;
		}).AddTo(this);
		m_allRemoveButton.OnClickAsObservable().Subscribe(delegate
		{
			GameDataManager.Mission.IsChangeCutIn = true;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			foreach (Transform item in m_scrollView.GetListParent().transform)
			{
				if (item.gameObject.activeSelf)
				{
					item.GetComponentInChildren<MatchingBGPanel>().m_info.IsOn = false;
					item.GetComponentInChildren<MatchingBGPanel>().RefleshOnOff();
				}
			}
			m_allRemoveButton.interactable = false;
		}).AddTo(this);
		m_itemName.text = "";
	}

	public async UniTask Play(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_listCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		try
		{
			Debug.Log("MyRoomCutinWindow.Play begin");
			m_isClosed = false;
			m_charaType = GameDataManager.Player.SelectChara;
			CreateItemList(_listCts.Token).Forget();
			await UniTask.WaitUntil(() => m_isClosed, PlayerLoopTiming.Update, cancellationToken);
			_listCts.Cancel();
			_listCts.Dispose();
		}
		finally
		{
			Debug.Log($"MyRoomCutinWindow.Play end listCts:{_listCts.IsCancellationRequested}, cancellationToken:{cancellationToken.IsCancellationRequested} ");
		}
	}

	private async UniTask CreateItemList(CancellationToken cancellationToken)
	{
		try
		{
			Debug.Log("CreateItemList begin");
			cancellationToken.ThrowIfCancellationRequested();
			m_itemName.text = "";
			m_scrollView.ResetList();
			m_dispItemList = (from a in GameDataManager.Customize.MatchingBGItems
				where a.Master.CharaType == m_charaType
				orderby a.Master.Category, a.Master.Rarity, a.Master.ItemID
				select a).ToList();
			m_allRemoveButton.interactable = false;
			if (m_dispItemList.Find((MatchingBGInfo a) => a.IsOn) != null)
			{
				m_allRemoveButton.interactable = true;
			}
			m_scrollView.ResetScroll();
			foreach (MatchingBGInfo dispItem in m_dispItemList)
			{
				await InstantiateCustomizeItemPanel(dispItem, cancellationToken);
			}
		}
		finally
		{
			Debug.Log($"CreateItemList end: cancel{cancellationToken.IsCancellationRequested}");
		}
	}

	private async UniTask InstantiateCustomizeItemPanel(MatchingBGInfo info, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		GameObject itemView = m_scrollView.Add(m_actPrefab);
		itemView.SetActive(value: false);
		await itemView.GetComponentInChildren<MatchingBGPanel>().Reflesh(info, isDispState: true, cancellationToken);
		itemView.SetActive(value: true);
		itemView.GetComponentInChildren<Button>().OnClickAsObservableSafety().Subscribe(async delegate
		{
			m_netcafeUnableText.SetActive(MatchingBackgroundMaster.IsPonnomichi(info.ItemID) && GameUtility.IsKonastaOrNetcafeBehavior);
			m_itemName.text = info.Master.Name;
			if (info.IsOn)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
				info.IsOn = false;
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				info.IsOn = true;
			}
			try
			{
				await itemView.GetComponentInChildren<MatchingBGPanel>().Reflesh(info, isDispState: true, cancellationToken);
			}
			catch (OperationCanceledException)
			{
			}
			AllRemoveButtonControl();
		})
			.AddTo(itemView);
	}

	private void AllRemoveButtonControl()
	{
		m_allRemoveButton.interactable = false;
		foreach (Transform item in m_scrollView.GetListParent().transform)
		{
			if (item.gameObject.activeSelf && item.GetComponentInChildren<MatchingBGPanel>().m_info.IsOn)
			{
				m_allRemoveButton.interactable = true;
			}
		}
	}

	private void Update()
	{
		_ = GameUtility.OnBeginSkipBootSceneFinished;
	}
}

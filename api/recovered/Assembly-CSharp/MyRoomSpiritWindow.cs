using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomSpiritWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollViewHorizontal m_scrollView;

	[SerializeField]
	private GameObject m_dressSelectButtonPrefab;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private FightSpiritNumDisp _fightSpiritNumDisp;

	[SerializeField]
	private FightSpiritPayWindow m_payWindow;

	[SerializeField]
	private GameObject m_mitigationWindowBase;

	[SerializeField]
	private GameObject m_mitigationWindow;

	[SerializeField]
	private TextMeshProUGUI m_mitigationBefore;

	[SerializeField]
	private TextMeshProUGUI m_mitigationAfter;

	[SerializeField]
	private Button m_mitigationCloseButton;

	[SerializeField]
	private Transform m_cutinParent;

	[SerializeField]
	private GameObject m_cutinPrefab;

	private MyRoomFlow myRoomFlow;

	private bool m_isClosed;

	private IDisposableAsset<Sprite> _charaSpiritSprite;

	private CharaType m_charaType;

	private MFG.Types.Customize.Costume.ItemID m_selectingItemID;

	private GameObject m_selectingObject;

	private bool m_isSelecting;

	private bool isMitigationWindowOpen;

	protected async void Start()
	{
		CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_isClosed = true;
		}).AddTo(this);
		m_payWindow.PayWindowInit(this, cancellationTokenOnDestroy);
		m_mitigationCloseButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			isMitigationWindowOpen = false;
		}).AddTo(this);
	}

	public async UniTask Play(MyRoomFlow mrf, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		myRoomFlow = mrf;
		m_isClosed = false;
		m_charaType = GameDataManager.Player.SelectChara;
		m_scrollView.ResetList();
		if (_charaSpiritSprite != null)
		{
			_charaSpiritSprite.Dispose();
		}
		_charaSpiritSprite = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Dojo, m_charaType, DojoFlow.AssetType.FightSpirit), null, cancellationToken);
		_charaSpiritSprite.AddTo(this);
		RefleshSpiritNum(cancellationToken).Forget();
		await CreateCostumeList(cancellationToken);
		await UniTask.WaitUntil(() => m_isClosed, PlayerLoopTiming.Update, cancellationToken);
	}

	private async UniTask CreateCostumeList(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		List<MFG.Types.Customize.Costume.Item> list = (from a in CostumeItemMaster.GetItems((MFG.Types.Customize.Costume.Item a) => a.CharaType == m_charaType)
			where a.IsDispEnable()
			orderby !GameDataManager.Customize.CostumeItems.Exists((CostumeItemInfo b) => b.ItemID == a.ItemID), a.SortID, a.ItemID
			select a).ToList();
		m_scrollView.ResetList();
		List<UniTask> list2 = new List<UniTask>();
		foreach (MFG.Types.Customize.Costume.Item item in list)
		{
			cancellationToken.ThrowIfCancellationRequested();
			GameObject gameObject = m_scrollView.Add(m_dressSelectButtonPrefab);
			list2.Add(gameObject.transform.GetComponent<MyRoomCostumeButton>().Create(this, item, _charaSpiritSprite.Asset, cancellationToken));
		}
		await list2;
		m_scrollView.ResetScroll();
	}

	public async UniTask CharaModelChange(MyRoomCostumeButton button, MFG.Types.Customize.Costume.ItemID costume, CancellationToken cancellationToken, bool isSEPlay = false)
	{
		if (m_isSelecting)
		{
			return;
		}
		m_isSelecting = true;
		if (isSEPlay)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		}
		button.BtDressAnimatorSetTrigger("Selected");
		foreach (GameObject @object in m_scrollView.GetObjects())
		{
			MyRoomCostumeButton component = @object.transform.GetComponent<MyRoomCostumeButton>();
			if (button != component)
			{
				component.BtDressAnimatorSetTrigger("Normal", isCheckInteractable: true);
			}
		}
		GameDataManager.Player.GetCurrentChara().CostumeItemEquip = GameUtility.CostumeUtility.CostumeConversion(costume);
		try
		{
			await myRoomFlow.m_charaManager.SingleCharaLoad(GameDataManager.Player.GetCurrentChara().CostumeItemEquip);
			await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			myRoomFlow.CharaStatusRefresh(cancellationToken);
			m_isSelecting = false;
		}
	}

	private async UniTask RefleshSpiritNum(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		myRoomFlow.SetFightSpirit(cancellationToken);
		await _fightSpiritNumDisp.Reflesh(m_charaType, cancellationToken);
	}

	public async UniTask Debut(MyRoomCostumeButton button, CancellationToken cancellationToken)
	{
		_ = 1;
		try
		{
			m_closeButton.interactable = false;
			await myRoomFlow.DebutPlay(button.CharaType, button.ItemID, cancellationToken);
			await CharaModelChange(button, button.ItemID, cancellationToken);
			RefleshSpiritNum(cancellationToken).Forget();
			CreateCostumeList(cancellationToken).Forget();
			m_closeButton.interactable = true;
		}
		catch (OperationCanceledException)
		{
		}
	}

	public void PayWindowOpen(MyRoomCostumeButton button)
	{
		m_payWindow.WindowOpen(button, _charaSpiritSprite.Asset);
	}

	public async UniTask MitigationWindowOpen(MFG.Types.Customize.Costume.ItemID itemID, CancellationToken cancellationToken, bool isSEPlay = true)
	{
		if (isMitigationWindowOpen)
		{
			return;
		}
		MFG.Types.Customize.Costume.Item item = CostumeItemMaster.GetItem(itemID);
		List<MFG.Types.Customize.CutinItem.ItemID> mitigationCutinItemIDs = item.mitigationCutinItemIDs;
		if (mitigationCutinItemIDs == null || mitigationCutinItemIDs.Count <= 0)
		{
			return;
		}
		isMitigationWindowOpen = true;
		m_mitigationWindowBase.SetActive(value: true);
		m_mitigationBefore.text = item.UnlockFightSpiritNum.ToString();
		m_mitigationAfter.text = item.MitigationUnlockFightSpiritNum.ToString();
		foreach (Transform item2 in m_cutinParent)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		foreach (MFG.Types.Customize.CutinItem.ItemID mitigationCutinItemID in item.mitigationCutinItemIDs)
		{
			MFG.Types.Customize.CutinItem.Item mas = CutinItemMaster.GetItem(mitigationCutinItemID);
			Sprite sprite = await mas.IconGraphic(cancellationToken);
			GameObject obj = UnityEngine.Object.Instantiate(m_cutinPrefab, m_cutinParent);
			obj.transform.Find("Icon").GetComponent<Image>().sprite = sprite;
			obj.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = mas.ReplacedShortName;
		}
		m_mitigationWindow.SetActive(value: true);
		m_mitigationWindow.transform.GetComponent<Animator>().SetTrigger("In");
		m_mitigationWindow.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
		m_mitigationWindow.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
		LayoutRebuilder.MarkLayoutForRebuild(m_mitigationWindow.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_mitigationWindow.GetComponent<RectTransform>());
		if (isSEPlay)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		}
		await UniTask.WaitUntil(() => !isMitigationWindowOpen, PlayerLoopTiming.Update, cancellationToken);
		m_mitigationWindow.transform.GetComponent<Animator>().SetTrigger("Out");
		m_mitigationWindow.SetActive(value: false);
		m_mitigationWindowBase.SetActive(value: false);
	}
}

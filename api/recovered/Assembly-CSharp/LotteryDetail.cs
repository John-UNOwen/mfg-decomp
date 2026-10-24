using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;
using UnityEngine.UI;

public class LotteryDetail : MonoBehaviour
{
	[SerializeField]
	protected ScrollRect _scrollRect;

	[SerializeField]
	protected GameObject _panelInstanceParent;

	[SerializeField]
	protected GameObject _panelPrefab;

	public bool isOpen;

	private List<SupplyLotteryItem> items = new List<SupplyLotteryItem>();

	private async void Start()
	{
	}

	private void Update()
	{
	}

	public List<string> SupplyList(bool isWithoutDuplicates, JanponObject.JanponKind kind = (JanponObject.JanponKind)(-1))
	{
		if (kind == (JanponObject.JanponKind)(-1))
		{
			if (!isWithoutDuplicates)
			{
				return items.Select((SupplyLotteryItem x) => x.ObjectID).ToList();
			}
			return (from x in items
				where !x.IsPossession
				select x.ObjectID).ToList();
		}
		if (!isWithoutDuplicates)
		{
			return (from x in items
				where x.JanponKind == kind
				select x.ObjectID).ToList();
		}
		return (from x in items
			where x.JanponKind == kind && !x.IsPossession
			select x.ObjectID).ToList();
	}

	public void SetAlready(string oid)
	{
		items.Find((SupplyLotteryItem x) => x.ObjectID == oid).AlreadyIconRefresh();
	}

	public void Refresh()
	{
		IEnumerable<SupplyLotteryItem> enumerable = items.Where((SupplyLotteryItem x) => !x.IsPossession);
		if (enumerable == null || enumerable.Count() <= 0)
		{
			return;
		}
		foreach (SupplyLotteryItem item in enumerable)
		{
			item.AlreadyIconRefresh();
		}
	}

	public (int, int) GetItemCount(JanponObject.JanponKind kind = (JanponObject.JanponKind)(-1))
	{
		if (kind == (JanponObject.JanponKind)(-1))
		{
			return (items.Count((SupplyLotteryItem x) => x.IsPossession), items.Count());
		}
		return (items.Count((SupplyLotteryItem x) => x.JanponKind == kind && x.IsPossession), items.Count((SupplyLotteryItem x) => x.JanponKind == kind));
	}

	public async UniTask Init()
	{
		_scrollRect.verticalNormalizedPosition = 1f;
		items = new List<SupplyLotteryItem>();
		foreach (Transform item in _panelInstanceParent.transform)
		{
			Object.Destroy(item.gameObject);
		}
		List<UniTask> list = new List<UniTask>();
		Dictionary<string, int> CategorySortDict = new Dictionary<string, int>
		{
			{ "卓ステッカー", 0 },
			{ "リーチBGM", 1 },
			{ "マイルームBGM", 2 },
			{ "キャッチフレーズ", 3 }
		};
		foreach (string item2 in from x in ScrambleFestivalMaster.ScrambleFestivalSupply
			select x.oid into x
			orderby (!CategorySortDict.ContainsKey(ItemUtility.ItemCategory(x))) ? 99 : CategorySortDict[ItemUtility.ItemCategory(x)], CustomizeItemMaster.GetItem(CustomizeItemMaster.ToItemID(x))?.Category ?? ((Category)9999), ItemUtility.IsCustomizeItem(x) ? CustomizeItemMaster.GetItem(x).Sort : ((SortType)0), x
			select x)
		{
			SupplyLotteryItem component = Object.Instantiate(_panelPrefab, _panelInstanceParent.transform).GetComponent<SupplyLotteryItem>();
			list.Add(component.Init(item2, isNew: false));
			items.Add(component);
		}
		await list;
	}

	public void Open(bool isPlaySe = true)
	{
		if (!isOpen)
		{
			_scrollRect.verticalNormalizedPosition = 1f;
			isOpen = true;
			base.gameObject.SetActive(value: true);
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			GetComponent<Animator>().SetTrigger("In");
		}
	}

	public void Close(bool isPlaySe = true)
	{
		if (isOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			GetComponent<Animator>().SetTrigger("Out");
			base.gameObject.SetActive(value: false);
			isOpen = false;
		}
	}
}

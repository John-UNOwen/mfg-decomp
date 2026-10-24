using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class LotterySpiritChoice : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private TextMeshProUGUI m_havingText;

	[SerializeField]
	private TextMeshProUGUI m_choiceCountText;

	[SerializeField]
	private Image m_itemIcon;

	[SerializeField]
	private Button m_plusButton;

	[SerializeField]
	private Button m_plus10Button;

	[SerializeField]
	private Button m_minusButton;

	[SerializeField]
	private Button m_minus10Button;

	public ItemID m_itemID;

	public int SelectingCount;

	private int _max;

	private ScrambleLotterySpiritWindow _spiritWindow;

	public async UniTask Reflesh(ItemID itemID, ScrambleLotterySpiritWindow spiritWindow)
	{
		m_itemID = itemID;
		_spiritWindow = spiritWindow;
		Item playerItem = PlayerItemMaster.GetItem(itemID);
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(playerItem.IconGraphicPath);
		disposableAsset.AddTo(this);
		m_itemIcon.sprite = disposableAsset.Asset;
		m_itemNameText.text = playerItem.Name;
		CounterInit();
		int max = GameDataManager.ScrambleFestival.CurrentInfo.Master.SpiritLotteryConsume.Spirit;
		m_plusButton.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
		{
			if (_max > SelectingCount && _spiritWindow.ChoosingSpiritSum() < max)
			{
				SelectingCount++;
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		})
			.AddTo(this);
		m_plus10Button.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
		{
			if (_max > SelectingCount && _spiritWindow.ChoosingSpiritSum() < max)
			{
				SelectingCount += 10;
				if (_max <= SelectingCount)
				{
					SelectingCount = _max;
				}
				if (_spiritWindow.ChoosingSpiritSum() > max)
				{
					SelectingCount -= _spiritWindow.ChoosingSpiritSum() - max;
				}
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		})
			.AddTo(this);
		m_minusButton.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
		{
			if (SelectingCount > 0)
			{
				SelectingCount--;
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		})
			.AddTo(this);
		m_minus10Button.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
		{
			if (SelectingCount > 0)
			{
				SelectingCount -= 10;
				if (SelectingCount < 0)
				{
					SelectingCount = 0;
				}
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		})
			.AddTo(this);
	}

	public void CounterInit()
	{
		_max = GameDataManager.PlayerItem.Count(m_itemID);
		m_havingText.text = _max.ToString();
		SelectingCount = 0;
	}

	public void Update()
	{
		m_choiceCountText.text = SelectingCount.ToString();
		if (_spiritWindow != null)
		{
			int item = GameDataManager.ScrambleFestival.CurrentInfo.Master.SpiritLotteryConsume.Spirit;
			m_plusButton.interactable = _max > SelectingCount && _spiritWindow.ChoosingSpiritSum() < item;
			m_plus10Button.interactable = _max > SelectingCount && _spiritWindow.ChoosingSpiritSum() < item;
			m_minusButton.interactable = SelectingCount > 0;
			m_minus10Button.interactable = SelectingCount > 0;
		}
	}
}

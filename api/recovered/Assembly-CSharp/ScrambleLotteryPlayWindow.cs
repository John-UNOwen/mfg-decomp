using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrambleLotteryPlayWindow : MonoBehaviour
{
	[Serializable]
	public class JanponColor
	{
		public JanponObject.JanponKind Kind;

		public Sprite JanponHead;

		public Sprite JanponBody;

		public Sprite JanponCupsule;

		public Color CupsuleColor;
	}

	[SerializeField]
	private GameObject m_window;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Animator m_janponAnim;

	[SerializeField]
	private GameObject m_exchangeAttention;

	[SerializeField]
	private GameObject m_newIcon;

	[SerializeField]
	private TextMeshProUGUI m_itemText;

	[SerializeField]
	private TextMeshProUGUI m_categoryText;

	[SerializeField]
	private TextMeshProUGUI m_medalText;

	[SerializeField]
	private TextMeshProUGUI m_medalAfterText;

	[SerializeField]
	private Image m_janponHead;

	[SerializeField]
	private Image m_janponBody;

	[SerializeField]
	private Image m_janponCapsule;

	[SerializeField]
	private Image m_itemCapsule;

	[SerializeField]
	private List<JanponColor> m_janponColors;

	[SerializeField]
	private Sprite m_janponHeadSpecial;

	[SerializeField]
	private Sprite m_janponBodySpecial;

	[SerializeField]
	private Sprite m_janponCapsuleSpecial;

	[SerializeField]
	private Color m_itemCapsuleColorSpecial;

	[SerializeField]
	private Image m_itemImage;

	public bool _isOpen;

	public virtual bool IsOpen => _isOpen;

	public void Start()
	{
		if (m_closeButton != null)
		{
			m_closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
	}

	private async UniTask<bool> LotteryPlay(bool isWithoutDuplicates, LotteryDetail detail, Dictionary<ItemID, int> consumeSpiritDict = null, JanponObject.JanponKind kind = (JanponObject.JanponKind)(-1))
	{
		JanponColor janponColor = m_janponColors.Find((JanponColor x) => x.Kind == kind);
		m_janponHead.sprite = (isWithoutDuplicates ? m_janponHeadSpecial : janponColor.JanponHead);
		m_janponBody.sprite = (isWithoutDuplicates ? m_janponBodySpecial : janponColor.JanponBody);
		m_janponCapsule.sprite = (isWithoutDuplicates ? m_janponCapsuleSpecial : janponColor.JanponCupsule);
		m_itemCapsule.color = (isWithoutDuplicates ? m_itemCapsuleColorSpecial : janponColor.CupsuleColor);
		m_medalText.text = GameDataManager.ScrambleFestival.CurrentInfo.Medal.ToString();
		List<string> list = detail.SupplyList(isWithoutDuplicates, kind);
		if (list != null && GameDataManager.ScrambleFestival.CurrentInfo.IsLotteryEnable(isWithoutDuplicates) && (!isWithoutDuplicates || consumeSpiritDict.Values.Sum() == GameDataManager.ScrambleFestival.CurrentInfo.Master.SpiritLotteryConsume.Spirit))
		{
			string oid = list[UnityEngine.Random.Range(0, list.Count)];
			if (ItemUtility.IsPossession(oid))
			{
				GameDataManager.ScrambleFestival.CurrentInfo.AddMedal(5);
				m_exchangeAttention.SetActive(value: true);
				m_newIcon.SetActive(value: false);
			}
			else
			{
				ItemUtility.AddItem(oid);
				detail.SetAlready(oid);
				m_exchangeAttention.SetActive(value: false);
				m_newIcon.SetActive(value: true);
			}
			GameDataManager.ScrambleFestival.CurrentInfo.ConsumeMedal(isWithoutDuplicates);
			if (isWithoutDuplicates && consumeSpiritDict != null)
			{
				foreach (ItemID key in consumeSpiritDict.Keys)
				{
					if (consumeSpiritDict[key] > 0)
					{
						GameDataManager.PlayerItem.Consume(key, consumeSpiritDict[key], ItemConsumeType.Janpon).Forget();
					}
				}
			}
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(ItemUtility.ItemIconGraphicPath(oid));
			disposableAsset.AddTo(this);
			m_itemImage.sprite = disposableAsset.Asset;
			string text = ItemUtility.ItemName(oid);
			m_itemText.text = (text.Contains("<sprite name") ? text.Replace("tint=1>", "tint=1 color=#171F84>") : text);
			m_categoryText.text = "";
			CharaType charaType = (ItemUtility.IsCutinItem(oid) ? CutinItemMaster.GetItem(CutinItemMaster.GetItemID(oid)).CharaType : (oid.Contains("OID_ReachBgm") ? CustomizeItemMaster.GetItem(oid).charaType : ((CharaType)(-1))));
			if (charaType >= CharaType.Chara01)
			{
				CharaProfile profile = MasterGameDataManager.Character.Properties(charaType).Profile;
				TextMeshProUGUI categoryText = m_categoryText;
				categoryText.text = categoryText.text + profile.ShortName + " / ";
			}
			m_categoryText.text += ItemUtility.ItemCategory(oid);
			m_medalAfterText.text = GameDataManager.ScrambleFestival.CurrentInfo.Medal.ToString();
			await GameDataManager.Customize.Save();
			await GameDataManager.ScrambleFestival.Save();
			await GameDataManager.PlayerItem.Save();
			try
			{
				await RequestApiBase<JanponLog, JanponLog.ResultData, (string, JanponObject.JanponKind, bool)>.Send((oid, kind, isWithoutDuplicates));
			}
			catch
			{
			}
			return true;
		}
		return false;
	}

	public async UniTask Open(bool isWithoutDuplicates, LotteryDetail detail, Dictionary<ItemID, int> consumeSpiritDict = null, bool isPlaySe = true, JanponObject.JanponKind kind = (JanponObject.JanponKind)(-1))
	{
		if (_isOpen)
		{
			return;
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		m_closeButton.gameObject.SetActive(value: false);
		m_window.SetActive(value: true);
		_isOpen = true;
		if (await LotteryPlay(isWithoutDuplicates, detail, consumeSpiritDict, kind))
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			m_window.GetComponent<Animator>().SetTrigger("In");
			m_janponAnim.SetTrigger("In");
			await UniTask.WaitUntil(() => m_janponAnim.GetCurrentAnimatorStateInfo(0).IsName("JanPonPlayLoop"));
			m_closeButton.gameObject.SetActive(value: true);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(10.0)), UniTask.WaitUntil(() => !_isOpen));
		}
		if (_isOpen)
		{
			Close();
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}

	public void Close(bool isPlaySe = true)
	{
		if (_isOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			if (m_window != null)
			{
				m_window.GetComponent<Animator>().SetTrigger("Out");
				m_window.SetActive(value: false);
			}
			_isOpen = false;
		}
	}
}

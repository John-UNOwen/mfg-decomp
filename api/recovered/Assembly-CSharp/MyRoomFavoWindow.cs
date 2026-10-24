using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Favo;
using MFG.Types.PlayerItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomFavoWindow : MonoBehaviour
{
	private MyRoomFlow m_myRoomFlow;

	[SerializeField]
	private ScrollView m_scrollView;

	[SerializeField]
	private GameObject m_favoAchivementPrefab;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Button m_itemPlusButton;

	[SerializeField]
	private Button m_itemMinusButton;

	[SerializeField]
	private Button m_itemUseButton;

	[SerializeField]
	private GameObject m_clickPreventPanel;

	private CharaType m_charaType;

	private bool m_isClosed;

	private int m_itemCount;

	private int m_itemUseCount;

	private int m_getExp;

	private bool m_isLevelUp;

	private CancellationTokenSource cancel;

	private CancellationToken token;

	public bool m_plusPushing;

	public bool m_minusPushing;

	public float m_timer;

	private int totalAddCounter;

	protected void Start()
	{
		CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
		m_itemPlusButton.OnClickAsObservable().Subscribe(delegate
		{
			if (m_itemUseCount < m_itemCount)
			{
				m_itemUseCount++;
				RefleshItemCount();
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				m_itemMinusButton.GetComponent<Button>().interactable = true;
				if (m_itemUseCount == m_itemCount || !IsExpUpByNextPresent())
				{
					m_itemPlusButton.GetComponent<Button>().interactable = false;
				}
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		}).AddTo(this);
		m_itemMinusButton.OnClickAsObservable().Subscribe(delegate
		{
			if (m_itemUseCount > 0)
			{
				m_itemUseCount--;
				RefleshItemCount();
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
				m_itemPlusButton.GetComponent<Button>().interactable = true;
				if (m_itemUseCount == 0)
				{
					m_itemMinusButton.GetComponent<Button>().interactable = false;
				}
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		}).AddTo(this);
		m_itemUseButton.OnClickAsObservable().Subscribe(async delegate
		{
			_ = 2;
			try
			{
				if (m_itemUseCount > 0)
				{
					ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
					GameDataManager.PlayerItem.Consume(ItemID.Present01, m_itemUseCount, ItemConsumeType.Favo).Forget();
					GameDataManager.Player.GetCurrentChara().Exp += m_getExp;
					m_itemCount -= m_itemUseCount;
					m_itemUseCount = 0;
					m_itemMinusButton.GetComponent<Button>().interactable = false;
					SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
					if (m_isLevelUp)
					{
						m_clickPreventPanel.SetActive(value: true);
						Animator levelUpAnim = base.transform.Find("CutInLvUp").GetComponent<Animator>();
						levelUpAnim.SetTrigger("In");
						SoundManager.PlaySe("SFX_SYS_RANKUP2").Forget();
						await UniTask.Delay(TimeSpan.FromSeconds(1.5166666507720947));
						m_myRoomFlow.CharaStatusRefresh(cancellationToken);
						RefleshItemCount();
						RefleshAchievementList();
						m_clickPreventPanel.SetActive(value: false);
						await UniTask.WhenAny(UniTask.WaitUntil(() => levelUpAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "CutInResultIn"), UniTask.WaitUntil(() => m_isClosed));
						if (!m_isClosed)
						{
							await m_myRoomFlow.MissionOpen();
						}
					}
					else
					{
						RefleshItemCount();
					}
					ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (IndexOutOfRangeException)
			{
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
			}
		}).AddTo(this);
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			BgmPreviewStop();
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			SoundManager.PlayMyCharaMyRoomBGM().Forget();
			m_isClosed = true;
		}).AddTo(this);
	}

	protected void Update()
	{
		if (m_plusPushing)
		{
			m_timer += Time.deltaTime;
			if (!(m_timer >= 1f))
			{
				return;
			}
			int num = (int)((m_timer - 1f) * 100f);
			if (m_itemUseCount < m_itemCount)
			{
				int num2 = num - totalAddCounter;
				m_itemUseCount += num2;
				totalAddCounter = num;
				if (m_itemUseCount >= m_itemCount)
				{
					m_itemUseCount = m_itemCount;
				}
				GiftExpOverCount();
				RefleshItemCount();
				m_itemMinusButton.GetComponent<Button>().interactable = true;
				if (m_itemUseCount == m_itemCount || !IsExpUpByNextPresent())
				{
					m_itemPlusButton.GetComponent<Button>().interactable = false;
					m_plusPushing = false;
				}
			}
		}
		else if (m_minusPushing)
		{
			m_timer += Time.deltaTime;
			if (!(m_timer >= 1f))
			{
				return;
			}
			int num3 = (int)((m_timer - 1f) * 100f);
			if (m_itemUseCount > 0)
			{
				m_itemUseCount -= num3 - totalAddCounter;
				totalAddCounter = num3;
				if (m_itemUseCount <= 0)
				{
					m_itemUseCount = 0;
				}
				RefleshItemCount();
				m_itemPlusButton.GetComponent<Button>().interactable = true;
				if (m_itemUseCount == 0)
				{
					m_itemMinusButton.GetComponent<Button>().interactable = false;
					m_minusPushing = false;
				}
			}
		}
		else
		{
			m_timer = 0f;
			totalAddCounter = 0;
		}
	}

	public async UniTask Play(MyRoomFlow mrf)
	{
		m_myRoomFlow = mrf;
		m_isClosed = false;
		m_charaType = GameDataManager.Player.SelectChara;
		base.transform.Find("LimitMessage").gameObject.SetActive(!GameDataManager.Player.GetCurrentChara().IsDebut);
		InitItemButton();
		InitAchievementList();
		await UniTask.WaitUntil(() => m_isClosed);
	}

	private void InitItemButton()
	{
		ItemID id = ItemID.Present01;
		m_itemCount = GameDataManager.PlayerItem.Items.Find((PlayerItemInfo a) => a.ItemID == id)?.Count ?? 0;
		m_itemUseCount = 0;
		m_itemUseButton.GetComponent<Button>().interactable = false;
		RefleshItemCount();
		if (m_itemCount == 0)
		{
			m_itemPlusButton.GetComponent<Button>().interactable = false;
			m_itemMinusButton.GetComponent<Button>().interactable = false;
		}
		else
		{
			m_itemPlusButton.GetComponent<Button>().interactable = IsExpUpByNextPresent();
			m_itemMinusButton.GetComponent<Button>().interactable = false;
		}
	}

	private void InitAchievementList()
	{
		m_scrollView.ResetList();
		List<FavoMasterData> list = (from a in FavoMaster.FavoMasterDatas(GameDataManager.Player.SelectChara)
			orderby a.FavoNum
			select a).ToList();
		int num = 0;
		foreach (FavoMasterData item in list)
		{
			m_scrollView.Add(m_favoAchivementPrefab, num).transform.GetComponent<FavoAchievementList>().Init(this, item, token);
			num++;
		}
		m_scrollView.ResetScroll();
	}

	public void BgmCancelToken(bool isNullCheck = false)
	{
		if (!isNullCheck || cancel != null)
		{
			cancel?.Cancel();
			cancel = new CancellationTokenSource();
			token = cancel.Token;
		}
	}

	public void BgmPreviewStop()
	{
		BgmCancelToken();
	}

	private void RefleshAchievementList()
	{
		foreach (GameObject value in m_scrollView.ObjectList.Values)
		{
			value.transform.GetComponent<FavoAchievementList>().BtRefresh();
		}
	}

	private void RefleshItemCount()
	{
		base.transform.Find("FavoPresent").Find("SelectItem").Find("HaveNumText")
			.GetComponent<TextMeshProUGUI>()
			.text = (m_itemCount - m_itemUseCount).ToString();
		base.transform.Find("FavoPresent").Find("NumSet").Find("NumText")
			.GetComponent<TextMeshProUGUI>()
			.text = m_itemUseCount.ToString();
		m_itemUseButton.GetComponent<Button>().interactable = m_itemUseCount != 0;
		m_getExp = m_itemUseCount * GetExp();
		CharaGameData currentChara = GameDataManager.Player.GetCurrentChara();
		int level = currentChara.Level;
		int item = currentChara.ExpectedLevelAndExp(m_getExp).expectedLevel;
		int expBefore = currentChara.Exp - currentChara.Master.GetTotalExpRequired(currentChara.Level);
		int exp = currentChara.ExpectedLevelAndExp(m_getExp).expectedTotalExp - currentChara.Master.GetTotalExpRequired(item);
		int expRequiredForNextLevel = currentChara.Master.GetExpRequiredForNextLevel(item);
		if (item > currentChara.Level)
		{
			m_isLevelUp = true;
			expBefore = 0;
		}
		else
		{
			m_isLevelUp = false;
		}
		base.transform.Find("FavoNum").Find("FavoNumBefore").Find("FavoNumText")
			.GetComponent<TextMeshProUGUI>()
			.text = level.ToString("N0");
		base.transform.Find("FavoNum").Find("FavoNumAfter").Find("FavoNumText")
			.GetComponent<TextMeshProUGUI>()
			.text = item.ToString("N0");
		base.transform.Find("FavoNum").Find("FavoNumAfter").gameObject.SetActive(m_itemUseCount != 0);
		MyRoomUtility.SetGauge(base.transform.Find("FavoNum").Find("FavoGauge").Find("LvGauge"), exp, expBefore, expRequiredForNextLevel);
	}

	private bool IsExpUpByNextPresent()
	{
		int acquisitionExp = m_itemUseCount * GetExp();
		int acquisitionExp2 = (m_itemUseCount + 1) * GetExp();
		CharaGameData currentChara = GameDataManager.Player.GetCurrentChara();
		int item = currentChara.ExpectedLevelAndExp(acquisitionExp).expectedLevel;
		int item2 = currentChara.ExpectedLevelAndExp(acquisitionExp2).expectedLevel;
		int item3 = currentChara.ExpectedLevelAndExp(acquisitionExp).expectedTotalExp;
		int item4 = currentChara.ExpectedLevelAndExp(acquisitionExp2).expectedTotalExp;
		if (item == item2 && item3 == item4)
		{
			return false;
		}
		return true;
	}

	private void GiftExpOverCount()
	{
		CharaGameData currentChara = GameDataManager.Player.GetCurrentChara();
		if (m_itemUseCount <= 0 || currentChara.ExpectedLevelAndExp(m_itemUseCount * GetExp()).expectedLevel < currentChara.MaxLevel)
		{
			return;
		}
		for (int num = m_itemUseCount; num > 0; num--)
		{
			if (currentChara.ExpectedLevelAndExp((m_itemUseCount - 1) * GetExp()).expectedLevel < currentChara.MaxLevel)
			{
				return;
			}
			m_itemUseCount--;
		}
		m_itemUseCount = 0;
	}

	private int GetExp()
	{
		return 100;
	}
}

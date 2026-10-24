using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

[SerializeField]
public class HaiNaviPanel : MonoBehaviour
{
	private enum PossibilityUpDown
	{
		None,
		Up,
		Down
	}

	private const int BUTTON_ID_MAX = 4;

	[SerializeField]
	private GameObject m_haiNaviRoot;

	[SerializeField]
	private List<GameObject> m_yakuButtons;

	private YakuPossibilityInfo[] m_yakuPossibilityInfoAtButtons = new YakuPossibilityInfo[4];

	private YakuInfo m_yakuInfo;

	private YakuInfo m_prevYakuInfo;

	private Dictionary<int, List<YakuPossibilityInfo>> m_oneByOneDiscardedYakuPossibilityInfos;

	[SerializeField]
	private List<Sprite> m_yakuSprites = new List<Sprite>();

	[SerializeField]
	private List<Sprite> m_yakuNameSprites = new List<Sprite>();

	[SerializeField]
	[Tooltip("翻数がindexとなってます")]
	private List<Sprite> m_hanNameSprites = new List<Sprite>();

	[SerializeField]
	private Help m_help;

	private Animator m_reloadAnim;

	private GameObject m_yakuExplanationPanel;

	private Animator m_yakuExplanationPanelAnim;

	private bool m_isPopUpActive;

	[SerializeField]
	private YakuChanceEffect m_yakuChanceEffect;

	public bool ReloadActive = true;

	public bool IsFireReach;

	public bool IsOpenReach;

	public bool IsAtomicReach;

	public bool IsMeld;

	private bool m_isDisp = true;

	private bool m_isYakuChancePlaying;

	public bool IsPlaying { get; private set; }

	private void Start()
	{
		m_reloadAnim = m_haiNaviRoot.transform.Find("Reload").GetComponent<Animator>();
		m_yakuExplanationPanel = m_haiNaviRoot.transform.Find("Yaku").gameObject;
		ReloadButton();
		ReloadButtonBySelect(-1);
	}

	private void Update()
	{
	}

	public void OnClick(int buttonID)
	{
		StartYakuExplanationPanel(buttonID);
	}

	public void Reset()
	{
		IsFireReach = false;
		IsOpenReach = false;
		IsAtomicReach = false;
		IsMeld = false;
		m_yakuInfo = null;
		m_prevYakuInfo = null;
		m_isPopUpActive = false;
		m_isYakuChancePlaying = false;
		m_yakuPossibilityInfoAtButtons = new YakuPossibilityInfo[4];
		m_yakuExplanationPanel.SetActive(value: false);
		ReloadActive = true;
		ReloadButton();
	}

	public async UniTask Reload(YakuInfo yakuInfo, Dictionary<int, List<YakuPossibilityInfo>> oneByOneDiscardedYakuPossibilityInfos, bool isFireReach, bool isOpenReach, bool isAtomicReach, bool isMeld)
	{
		IsFireReach = isFireReach;
		IsOpenReach = isOpenReach;
		IsAtomicReach = isAtomicReach;
		IsMeld = isMeld;
		m_prevYakuInfo = m_yakuInfo;
		m_yakuInfo = yakuInfo.Clone();
		KaratenSet();
		foreach (YakuPossibilityInfo yakuPossibilityInfo in m_yakuInfo.YakuPossibilityInfos)
		{
			Debug.Log("HaiNaviPanel -> yaku : " + yakuPossibilityInfo.Yaku.ToString() + "  possibility : " + yakuPossibilityInfo.Possibility.ToString() + "  karaten : " + yakuPossibilityInfo.Karaten);
		}
		m_yakuInfo.YakuPossibilityInfos.Sort((YakuPossibilityInfo a, YakuPossibilityInfo b) => GetHanNum(b.Yaku) - GetHanNum(a.Yaku));
		m_oneByOneDiscardedYakuPossibilityInfos = oneByOneDiscardedYakuPossibilityInfos;
		if (ReloadActive && IsNeedReload())
		{
			IsPlaying = true;
			m_help.IconInNavi();
			m_yakuExplanationPanel.SetActive(value: false);
			m_reloadAnim.SetTrigger("Start");
			if (m_isDisp)
			{
				SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			}
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			ReloadButton();
			PlayYakuChanceEffect().Forget();
			IsPlaying = false;
		}
	}

	public void ReloadButtonBySelect(int handIndex)
	{
		if (handIndex < 0)
		{
			Button_SetUpDownDispAll(PossibilityUpDown.None);
			return;
		}
		if (m_oneByOneDiscardedYakuPossibilityInfos == null)
		{
			Debug.LogWarning("HaiNaviPanel.Reloadが呼ばれていない");
			return;
		}
		if (!m_oneByOneDiscardedYakuPossibilityInfos.ContainsKey(handIndex))
		{
			Button_SetUpDownDispAll(PossibilityUpDown.Down);
			return;
		}
		List<YakuPossibilityInfo> list = m_oneByOneDiscardedYakuPossibilityInfos[handIndex];
		for (int i = 0; i < m_yakuButtons.Count; i++)
		{
			if (m_yakuButtons[i].activeSelf)
			{
				if (i < m_yakuInfo.YakuPossibilityInfos.Count)
				{
					YakuPossibilityInfo yakuInfoAtButton = m_yakuPossibilityInfoAtButtons[i];
					int num = list.FindIndex((YakuPossibilityInfo x) => x.Yaku == yakuInfoAtButton.Yaku);
					PossibilityUpDown updown = PossibilityUpDown.None;
					if (num >= 0)
					{
						YakuPossibilityInfo yakuPossibilityInfo = list[num];
						if (yakuPossibilityInfo.Possibility > yakuInfoAtButton.Possibility)
						{
							updown = PossibilityUpDown.Up;
						}
						else if (yakuPossibilityInfo.Possibility < yakuInfoAtButton.Possibility)
						{
							updown = PossibilityUpDown.Down;
						}
					}
					else
					{
						updown = PossibilityUpDown.Down;
					}
					Button_SetUpDownDisp(i, updown);
				}
				else
				{
					Button_SetUpDownDisp(i, PossibilityUpDown.None);
				}
			}
			else
			{
				Button_SetUpDownDisp(i, PossibilityUpDown.None);
			}
		}
	}

	private void Button_SetUpDownDispAll(PossibilityUpDown updown)
	{
		int count = m_yakuButtons.Count;
		for (int i = 0; i < count; i++)
		{
			Button_SetUpDownDisp(i, updown);
		}
	}

	private void Button_SetUpDownDisp(int buttonIndex, PossibilityUpDown updown)
	{
		GameObject gameObject = m_yakuButtons[buttonIndex];
		switch (updown)
		{
		case PossibilityUpDown.None:
			gameObject.transform.Find("IconDown").gameObject.SetActive(value: false);
			gameObject.transform.Find("IconUp").gameObject.SetActive(value: false);
			break;
		case PossibilityUpDown.Up:
			gameObject.transform.Find("IconDown").gameObject.SetActive(value: false);
			gameObject.transform.Find("IconUp").gameObject.SetActive(value: true);
			gameObject.GetComponent<Animator>().enabled = true;
			if (gameObject.activeSelf)
			{
				gameObject.GetComponent<Animator>().SetTrigger("IconIn");
			}
			break;
		case PossibilityUpDown.Down:
			gameObject.transform.Find("IconDown").gameObject.SetActive(value: true);
			gameObject.transform.Find("IconUp").gameObject.SetActive(value: false);
			gameObject.GetComponent<Animator>().enabled = true;
			if (gameObject.activeSelf)
			{
				gameObject.GetComponent<Animator>().SetTrigger("IconIn");
			}
			break;
		}
	}

	private bool IsNeedReload()
	{
		if (m_prevYakuInfo == null)
		{
			return m_yakuInfo.YakuPossibilityInfos.Count > 0;
		}
		if (m_prevYakuInfo.YakuPossibilityInfos.Count != m_yakuInfo.YakuPossibilityInfos.Count)
		{
			return true;
		}
		bool flag = false;
		foreach (YakuPossibilityInfo prev in m_prevYakuInfo.YakuPossibilityInfos)
		{
			if (m_yakuInfo.YakuPossibilityInfos.FindIndex((YakuPossibilityInfo x) => x.Yaku == prev.Yaku && x.Possibility == prev.Possibility) < 0)
			{
				flag = true;
				break;
			}
			if (m_yakuInfo.YakuPossibilityInfos.FindIndex((YakuPossibilityInfo x) => x.Yaku == prev.Yaku && prev.Karaten == KaratenInfo.None && x.Karaten == KaratenInfo.DispKaraten) >= 0)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	private void ReloadButton()
	{
		int num = m_yakuInfo?.YakuPossibilityInfos.Count ?? 0;
		m_haiNaviRoot.transform.Find("Frame").Find("Nothing").gameObject.SetActive(num == 0);
		for (int i = 0; i < m_yakuButtons.Count; i++)
		{
			GameObject gameObject = m_yakuButtons[i];
			if (i < num)
			{
				gameObject.SetActive(value: true);
				YakuPossibilityInfo yakuPossibilityInfo = m_yakuInfo.YakuPossibilityInfos[i];
				gameObject.transform.Find("YakuName").GetComponent<Image>().sprite = GetYakuNameSprite(yakuPossibilityInfo.Yaku);
				gameObject.transform.Find("HanName").GetComponent<Image>().sprite = GetHanNameSprite(GetHanNum(yakuPossibilityInfo.Yaku));
				if (GameDataManager.BingoEventRemake.IsEnable() && GameDataManager.BingoEventRemake.IsBingoTargetYaku(yakuPossibilityInfo.Yaku))
				{
					gameObject.transform.Find("Bingo").gameObject.SetActive(value: true);
				}
				else
				{
					gameObject.transform.Find("Bingo").gameObject.SetActive(value: false);
				}
				bool flag = yakuPossibilityInfo.Possibility == YakuPossibility.Established;
				gameObject.transform.Find("IconComplete").gameObject.SetActive(flag);
				if (flag)
				{
					gameObject.GetComponent<Animator>().enabled = true;
					gameObject.GetComponent<Animator>().SetTrigger("IconIn");
				}
				bool flag2 = yakuPossibilityInfo.Karaten == KaratenInfo.DispKaraten;
				gameObject.transform.Find("IconImpossible").gameObject.SetActive(flag2);
				if (flag2)
				{
					gameObject.GetComponent<Animator>().enabled = true;
					gameObject.GetComponent<Animator>().SetTrigger("IconIn");
				}
				m_yakuPossibilityInfoAtButtons[i] = yakuPossibilityInfo.Clone();
			}
			else
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	private async void StartYakuExplanationPanel(int buttonID)
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		m_yakuButtons.ForEach(delegate(GameObject button)
		{
			button.SetActive(value: false);
		});
		YakuPossibilityInfo info = m_yakuPossibilityInfoAtButtons[buttonID];
		Button panelButton = m_yakuExplanationPanel.GetComponent<Button>();
		panelButton.enabled = false;
		m_yakuExplanationPanel.SetActive(value: true);
		m_yakuExplanationPanel.GetComponent<Image>().sprite = GetYakuExplanationContentsSprite(info.Yaku);
		m_yakuChanceEffect.PlayChanceHai(info);
		await UniTask.Delay(100);
		panelButton.enabled = true;
	}

	public async void StartYakuExplanationPanel_OnClick()
	{
		m_yakuButtons.ForEach(delegate(GameObject x)
		{
			x.GetComponent<Button>().enabled = false;
		});
		SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
		m_yakuExplanationPanel.SetActive(value: false);
		ReloadButton();
		await UniTask.Delay(100);
		m_yakuButtons.ForEach(delegate(GameObject x)
		{
			x.GetComponent<Button>().enabled = true;
		});
	}

	public void SetDisp(bool isDisp)
	{
		m_isDisp = isDisp;
		m_haiNaviRoot.SetActive(m_isDisp);
	}

	private async UniTask PlayYakuChanceEffect()
	{
		if (!m_isDisp)
		{
			return;
		}
		m_isYakuChancePlaying = true;
		if (!m_isPopUpActive)
		{
			YakuInfo yakuInfo = m_yakuInfo;
			if (yakuInfo == null || yakuInfo.YakuPossibilityInfos.Count > 0)
			{
				List<YakuPossibilityInfo> list = ((m_prevYakuInfo != null) ? (from x in m_yakuInfo?.YakuPossibilityInfos.Except(m_prevYakuInfo.YakuPossibilityInfos, new YakuPossibilityInfoComparer())
					where x.Possibility == YakuPossibility.Established
					select x).ToList() : m_yakuInfo?.YakuPossibilityInfos.Where((YakuPossibilityInfo x) => x.Possibility == YakuPossibility.Established).ToList());
				if (list != null && list.Count > 0)
				{
					foreach (YakuPossibilityInfo item in list)
					{
						if (m_isDisp)
						{
							await m_yakuChanceEffect.PlayEffect(item);
						}
					}
				}
				m_isYakuChancePlaying = false;
				m_isPopUpActive = false;
				return;
			}
		}
		m_isYakuChancePlaying = false;
	}

	public bool IsYakuchancePlaying()
	{
		return m_isYakuChancePlaying;
	}

	public void ExitYakuExplanationPopUp_OnClick()
	{
		m_isPopUpActive = false;
	}

	private Sprite GetYakuExplanationContentsSprite(Yaku yaku)
	{
		return m_yakuSprites[(int)yaku];
	}

	private Yaku ToMeldYaku(Yaku yaku)
	{
		if (IsMeld)
		{
			switch (yaku)
			{
			case Yaku.Chiniso:
				return Yaku.ChinisoNaki;
			case Yaku.Junchan:
				return Yaku.JunchanNaki;
			case Yaku.Honiso:
				return Yaku.HonisoNaki;
			case Yaku.Sansyokudojun:
				return Yaku.SansyokudojunNaki;
			case Yaku.Chanta:
				return Yaku.ChantaNaki;
			case Yaku.Ikkitsukan:
				return Yaku.IkkitsukanNaki;
			}
		}
		return yaku;
	}

	private int GetHanNum(Yaku yaku)
	{
		return MahjongUtility.GetHanNum(ToMeldYaku(yaku), IsFireReach, IsOpenReach, IsAtomicReach);
	}

	private Sprite GetYakuNameSprite(Yaku yaku)
	{
		yaku = ToMeldYaku(yaku);
		if (IsFireReach)
		{
			if (GameDataManager.Mahjong.IsAotenjoMode)
			{
				switch (yaku)
				{
				case Yaku.Richi:
					return m_yakuNameSprites[71];
				case Yaku.DoubleRichi:
					return m_yakuNameSprites[72];
				case Yaku.Tanyao:
					return m_yakuNameSprites[66];
				case Yaku.Pinfu:
					return m_yakuNameSprites[67];
				}
			}
			switch (yaku)
			{
			case Yaku.Richi:
				return m_yakuNameSprites[61];
			case Yaku.DoubleRichi:
				return m_yakuNameSprites[62];
			}
		}
		else if (IsAtomicReach)
		{
			if (GameDataManager.Mahjong.IsAotenjoMode)
			{
				switch (yaku)
				{
				case Yaku.Richi:
					return m_yakuNameSprites[73];
				case Yaku.DoubleRichi:
					return m_yakuNameSprites[74];
				case Yaku.Tanyao:
					return m_yakuNameSprites[66];
				case Yaku.Pinfu:
					return m_yakuNameSprites[67];
				}
			}
			switch (yaku)
			{
			case Yaku.Richi:
				return m_yakuNameSprites[69];
			case Yaku.DoubleRichi:
				return m_yakuNameSprites[70];
			}
		}
		else if (IsOpenReach)
		{
			if (GameDataManager.Mahjong.IsAotenjoMode)
			{
				switch (yaku)
				{
				case Yaku.Richi:
					return m_yakuNameSprites[69];
				case Yaku.DoubleRichi:
					return m_yakuNameSprites[70];
				case Yaku.Tanyao:
					return m_yakuNameSprites[66];
				case Yaku.Pinfu:
					return m_yakuNameSprites[67];
				}
			}
			switch (yaku)
			{
			case Yaku.Richi:
				return m_yakuNameSprites[64];
			case Yaku.DoubleRichi:
				return m_yakuNameSprites[65];
			}
		}
		else if (GameDataManager.Mahjong.IsMentanpinMode)
		{
			switch (yaku)
			{
			case Yaku.Richi:
				return m_yakuNameSprites[64];
			case Yaku.DoubleRichi:
				return m_yakuNameSprites[65];
			case Yaku.Tanyao:
				return m_yakuNameSprites[66];
			case Yaku.Pinfu:
				return m_yakuNameSprites[67];
			}
		}
		return m_yakuNameSprites[(int)yaku];
	}

	private Sprite GetHanNameSprite(int hanNum)
	{
		if (hanNum > 13)
		{
			hanNum = 13;
		}
		return m_hanNameSprites[hanNum];
	}

	private void KaratenSet()
	{
		int num = m_yakuInfo.YakuPossibilityInfos.Count();
		for (int i = 0; i < num; i++)
		{
			YakuPossibilityInfo info = m_yakuInfo.YakuPossibilityInfos[i];
			if (info.Karaten >= KaratenInfo.KaratenWait)
			{
				if (!m_prevYakuInfo.YakuPossibilityInfos.Exists((YakuPossibilityInfo x) => x.Yaku == info.Yaku))
				{
					info.Karaten = KaratenInfo.UnDispKaraten;
				}
				else
				{
					switch (m_prevYakuInfo.YakuPossibilityInfos.Find((YakuPossibilityInfo x) => x.Yaku == info.Yaku).Karaten)
					{
					case KaratenInfo.None:
					case KaratenInfo.DispKaraten:
						info.Karaten = KaratenInfo.DispKaraten;
						break;
					case KaratenInfo.UnDispKaraten:
						info.Karaten = KaratenInfo.UnDispKaraten;
						break;
					}
				}
			}
			m_yakuInfo.YakuPossibilityInfos[i] = info;
		}
		foreach (YakuPossibilityInfo item in m_yakuInfo.YakuPossibilityInfos.FindAll((YakuPossibilityInfo x) => x.Karaten == KaratenInfo.UnDispKaraten))
		{
			m_yakuInfo.YakuPossibilityInfos.Remove(item);
		}
	}
}

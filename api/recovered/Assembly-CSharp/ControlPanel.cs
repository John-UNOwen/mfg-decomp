using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{
	[SerializeField]
	public Button ponButton;

	[SerializeField]
	public Button chiButton;

	[SerializeField]
	public Button kanButton;

	[SerializeField]
	public Button reachButton;

	[SerializeField]
	public Button agariButton;

	[SerializeField]
	public Button nextButton;

	[SerializeField]
	public Button fireReachButton;

	[SerializeField]
	public Button openReachButton;

	[SerializeField]
	public Button atomicReachButton;

	[SerializeField]
	public GameObject kanButtonBgEffect;

	[SerializeField]
	public GameObject chiButtonBgEffect;

	[SerializeField]
	public GameObject ponButtonBgEffect;

	[SerializeField]
	public GameObject reachButtonBgEffect;

	[SerializeField]
	public GameObject fireReachButtonBgEffect;

	[SerializeField]
	public GameObject openReachButtonBgEffect;

	[SerializeField]
	public GameObject atomicReachButtonBgEffect;

	[SerializeField]
	public GameObject agariButtonBgEffect;

	[SerializeField]
	public GameObject nextButtonBgEffect;

	[SerializeField]
	public Button unableReachButton;

	[SerializeField]
	public Button unableFireReachButton;

	[SerializeField]
	public Button unableOpenReachButton;

	[SerializeField]
	public Button unableAtomicReachButton;

	[SerializeField]
	public GameObject reachUnabaleScore;

	[SerializeField]
	public GameObject reachUnabaleMachi;

	[SerializeField]
	public GameObject reachUnabaleAllDiscardEnable;

	[SerializeField]
	public GameObject fireReachUnableAlreadyUsed;

	[SerializeField]
	public GameObject fireReachUnabaleMachi;

	[SerializeField]
	public GameObject fireReachUnabaleScore;

	[SerializeField]
	public GameObject fireReachUnabaleAllDiscardEnable;

	[SerializeField]
	public GameObject openReachUnableAlreadyUsed;

	[SerializeField]
	public GameObject openReachUnabaleMachi;

	[SerializeField]
	public GameObject openReachUnabaleScore;

	[SerializeField]
	public GameObject openReachUnabaleAllDiscardEnable;

	[SerializeField]
	public GameObject atomicReachUnabaleMachi;

	[SerializeField]
	public GameObject atomicReachUnabaleScore;

	[SerializeField]
	public GameObject atomicReachUnableGukei;

	[SerializeField]
	public GameObject atomicReachUnabaleAllDiscardEnable;

	public bool m_isOnClickAgari;

	public bool m_isOnClickReach;

	public bool m_isOnClickPon;

	public bool m_isOnClickChi;

	public bool m_isOnClickKan;

	public bool m_isOnClickNext;

	public bool m_isOnClickFireReach;

	public bool m_isOnClickOpenReach;

	public bool m_isOnClickAtomicReach;

	private void Start()
	{
		agariButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isOnClickAgari = true;
			Destruct();
		}).AddTo(this);
		reachButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isOnClickReach = true;
		}).AddTo(this);
		ponButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isOnClickPon = true;
		}).AddTo(this);
		chiButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isOnClickChi = true;
		}).AddTo(this);
		kanButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isOnClickKan = true;
		}).AddTo(this);
		nextButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isOnClickNext = true;
		}).AddTo(this);
		fireReachButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isOnClickFireReach = true;
		}).AddTo(this);
		openReachButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isOnClickOpenReach = true;
		}).AddTo(this);
		atomicReachButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isOnClickAtomicReach = true;
		}).AddTo(this);
	}

	private void Update()
	{
		if (GameDataManager.PlayerConfig.IsKeyboardPlay())
		{
			if (agariButton.IsActive() && agariButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.Return))
			{
				m_isOnClickAgari = true;
				Destruct();
				MahjongUtility.ResetNoInputCount();
			}
			else if (reachButton.IsActive() && reachButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.R))
			{
				m_isOnClickReach = true;
				MahjongUtility.ResetNoInputCount();
			}
			else if (ponButton.IsActive() && ponButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.P))
			{
				m_isOnClickPon = true;
				MahjongUtility.ResetNoInputCount();
			}
			else if (chiButton.IsActive() && chiButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.C))
			{
				m_isOnClickChi = true;
				MahjongUtility.ResetNoInputCount();
			}
			else if (kanButton.IsActive() && kanButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.K))
			{
				m_isOnClickKan = true;
				MahjongUtility.ResetNoInputCount();
			}
			else if (nextButton.IsActive() && nextButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.Space))
			{
				m_isOnClickNext = true;
				MahjongUtility.ResetNoInputCount();
			}
			else if (fireReachButton.IsActive() && fireReachButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.F))
			{
				m_isOnClickFireReach = true;
				MahjongUtility.ResetNoInputCount();
			}
			else if (openReachButton.IsActive() && openReachButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.O))
			{
				m_isOnClickOpenReach = true;
				MahjongUtility.ResetNoInputCount();
			}
			else if (atomicReachButton.IsActive() && atomicReachButton.IsInteractable() && Bi2xInput.GetKeyDown(KeyCode.T))
			{
				m_isOnClickAtomicReach = true;
				MahjongUtility.ResetNoInputCount();
			}
		}
		KeyboardTextReflesh(agariButton);
		KeyboardTextReflesh(reachButton);
		KeyboardTextReflesh(ponButton);
		KeyboardTextReflesh(chiButton);
		KeyboardTextReflesh(kanButton);
		KeyboardTextReflesh(nextButton);
		KeyboardTextReflesh(fireReachButton);
		KeyboardTextReflesh(openReachButton);
		KeyboardTextReflesh(atomicReachButton);
	}

	private void KeyboardTextReflesh(Button button)
	{
		try
		{
			Transform transform = button.transform.Find("KeyboardText");
			if (transform != null)
			{
				transform.gameObject.SetActive(GameDataManager.PlayerConfig.IsKeyboardGuideDisplay() && button.gameObject.activeSelf);
			}
		}
		catch
		{
		}
	}

	public void Destruct()
	{
		ControlButtonSetActive(ponButton, active: false);
		ControlButtonSetActive(chiButton, active: false);
		ControlButtonSetActive(kanButton, active: false);
		ControlButtonSetActive(reachButton, active: false);
		ControlButtonSetActive(agariButton, active: false);
		ControlButtonSetActive(nextButton, active: false);
		ControlButtonSetActive(unableReachButton, active: false);
		ControlButtonSetActive(fireReachButton, active: false);
		ControlButtonSetActive(unableFireReachButton, active: false);
		ControlButtonSetActive(openReachButton, active: false);
		ControlButtonSetActive(unableOpenReachButton, active: false);
		ControlButtonSetActive(atomicReachButton, active: false);
		ControlButtonSetActive(unableAtomicReachButton, active: false);
		ReachButtonInit();
		ponButtonBgEffect.SetActive(value: false);
		chiButtonBgEffect.SetActive(value: false);
		kanButtonBgEffect.SetActive(value: false);
		reachButtonBgEffect.SetActive(value: false);
		fireReachButtonBgEffect.SetActive(value: false);
		openReachButtonBgEffect.SetActive(value: false);
		atomicReachButtonBgEffect.SetActive(value: false);
		agariButtonBgEffect.SetActive(value: false);
		ReachCautionActive(isActive: false);
	}

	public void ReachButtonInit()
	{
		reachButton.interactable = true;
		fireReachButton.interactable = true;
		openReachButton.interactable = true;
		atomicReachButton.interactable = true;
	}

	public void ReachCautionActive(bool isActive)
	{
		reachButton.transform.Find("Caution").gameObject.SetActive(isActive);
	}

	public void FireReachCautionActive(bool isActive)
	{
		fireReachButton.transform.Find("Caution").gameObject.SetActive(isActive);
	}

	public void OpenReachCautionActive(bool isActive)
	{
		openReachButton.transform.Find("Caution").gameObject.SetActive(isActive);
	}

	public void AtomicReachCautionActive(bool isActive)
	{
		atomicReachButton.transform.Find("Caution").gameObject.SetActive(isActive);
	}

	public void ControlReachButtonInteractable(Button button, bool isActive)
	{
		button.interactable = isActive;
		if (isActive)
		{
			if (button.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("BtNakiInNonactive"))
			{
				button.GetComponent<Animator>().SetTrigger("ToActive");
			}
		}
		else if (button.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Active"))
		{
			button.GetComponent<Animator>().SetTrigger("ToNonActive");
		}
	}

	public void ControlButtonSetActive(Button button, bool active, bool cautionDisp = false, bool scoreCaution = false, bool machiCaution = false, bool fireReachUsed = false, bool openReachUsed = false, bool atomicReachUsed = false, bool discardCaution = false, bool gukeiCaution = false)
	{
		if (active)
		{
			button.gameObject.SetActive(value: true);
			if (button != unableReachButton && button != unableFireReachButton && button != unableOpenReachButton && button != unableAtomicReachButton)
			{
				button.interactable = true;
				button.GetComponent<Animator>().ResetTrigger("Push");
				button.GetComponent<Animator>().SetTrigger("Start");
			}
			else
			{
				button.interactable = false;
				button.GetComponent<Animator>().SetTrigger("NonActive");
			}
			Transform transform = button.transform.Find("Caution");
			if (!(transform != null))
			{
				return;
			}
			transform.gameObject.SetActive(cautionDisp);
			if (button == unableReachButton)
			{
				if (scoreCaution)
				{
					reachUnabaleScore.SetActive(scoreCaution);
				}
				else if (machiCaution)
				{
					reachUnabaleMachi.SetActive(machiCaution);
				}
				else if (discardCaution)
				{
					reachUnabaleAllDiscardEnable.SetActive(discardCaution);
				}
			}
			else if (button == unableFireReachButton)
			{
				if (fireReachUsed)
				{
					fireReachUnableAlreadyUsed.SetActive(fireReachUsed);
				}
				else if (scoreCaution)
				{
					fireReachUnabaleScore.SetActive(scoreCaution);
				}
				else if (machiCaution)
				{
					fireReachUnabaleMachi.SetActive(machiCaution);
				}
				else if (discardCaution)
				{
					fireReachUnabaleAllDiscardEnable.SetActive(discardCaution);
				}
			}
			else if (button == unableOpenReachButton)
			{
				if (openReachUsed)
				{
					openReachUnableAlreadyUsed.SetActive(openReachUsed);
				}
				else if (scoreCaution)
				{
					openReachUnabaleScore.SetActive(scoreCaution);
				}
				else if (machiCaution)
				{
					openReachUnabaleMachi.SetActive(machiCaution);
				}
				else if (discardCaution)
				{
					openReachUnabaleAllDiscardEnable.SetActive(discardCaution);
				}
			}
			else if (button == unableAtomicReachButton)
			{
				if (scoreCaution)
				{
					atomicReachUnabaleScore.SetActive(scoreCaution);
				}
				else if (machiCaution)
				{
					atomicReachUnabaleMachi.SetActive(machiCaution);
				}
				else if (gukeiCaution)
				{
					atomicReachUnableGukei.SetActive(gukeiCaution);
				}
				else if (discardCaution)
				{
					atomicReachUnabaleAllDiscardEnable.SetActive(discardCaution);
				}
			}
		}
		else
		{
			button.gameObject.SetActive(value: false);
		}
	}

	public void ResetOnClickFlag()
	{
		m_isOnClickAgari = false;
		m_isOnClickReach = false;
		m_isOnClickPon = false;
		m_isOnClickChi = false;
		m_isOnClickKan = false;
		m_isOnClickNext = false;
		m_isOnClickFireReach = false;
		m_isOnClickOpenReach = false;
		m_isOnClickAtomicReach = false;
	}
}

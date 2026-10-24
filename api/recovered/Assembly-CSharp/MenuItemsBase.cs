using System;
using System.Collections.Generic;
using TestModeSkeleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class MenuItemsBase : MonoBehaviour
{
	[Serializable]
	public class MenuElement
	{
		public Text MenuText;

		public EVENT_TYPE EventType;

		public string Event;

		public TestModeHelp.HelpType InfoState;
	}

	public enum EVENT_TYPE
	{
		NEXT_SCENE,
		EVENT,
		FACTORY_SETTINGS,
		SAVE_EXIT,
		EXIT,
		ABORT
	}

	protected enum CALL_TYPE
	{
		TRIGGER,
		HOLD
	}

	public List<MenuElement> m_MenuElements;

	private int m_oldCursor;

	protected int m_currentCursor;

	private bool isExitReservation;

	protected Text titleText;

	protected TestModeHelp help;

	protected GameObject ArrowkeyUp;

	protected GameObject ArrowkeyDown;

	protected GameObject ArrowkeyLeft;

	protected GameObject ArrowkeyRight;

	protected GameObject ArrowkeyEnter;

	protected TestModeHelp.HelpType SetKeyInfoState
	{
		set
		{
			if ((bool)help)
			{
				help.Set(value);
			}
			SetTouchButton(value);
		}
	}

	private void Start()
	{
		SceneStart();
	}

	private void Update()
	{
		SceneUpdate();
	}

	protected void SceneStart()
	{
		titleText = GameObject.Find("TestModeMain/Canvas/Title/Main/Text")?.GetComponent<Text>();
		help = GameObject.Find("TestModeMain")?.GetComponent<TestModeHelp>();
		ArrowkeyUp = GameObject.Find("TestModeMain/Canvas/KeyPad/Up");
		ArrowkeyDown = GameObject.Find("TestModeMain/Canvas/KeyPad/Down");
		ArrowkeyLeft = GameObject.Find("TestModeMain/Canvas/KeyPad/Left");
		ArrowkeyRight = GameObject.Find("TestModeMain/Canvas/KeyPad/Right");
		ArrowkeyEnter = GameObject.Find("TestModeMain/Canvas/KeyPad/Enter");
		isExitReservation = false;
		m_oldCursor = -1;
		m_currentCursor = 0;
		for (int i = 0; i < m_MenuElements.Count; i++)
		{
			if (m_MenuElements[i].EventType == EVENT_TYPE.EXIT)
			{
				m_currentCursor = i;
				break;
			}
		}
		MenuSceneStart();
	}

	protected abstract void MenuSceneStart();

	protected void SceneUpdate()
	{
		if (isExitReservation)
		{
			return;
		}
		if (m_currentCursor != m_oldCursor)
		{
			bool flag = false;
			if (m_MenuElements.Count > 0)
			{
				if (0 <= m_oldCursor && m_oldCursor < m_MenuElements.Count)
				{
					OffCursorElement(m_MenuElements[m_oldCursor]);
				}
				SetKeyInfoState = m_MenuElements[m_currentCursor].InfoState;
				flag = OnCursorElement(m_MenuElements[m_currentCursor]);
			}
			if (!flag)
			{
				for (int i = 0; i < m_MenuElements.Count; i++)
				{
					if (i == m_currentCursor)
					{
						m_MenuElements[i].MenuText.color = Color.red;
					}
					else
					{
						m_MenuElements[i].MenuText.color = Color.white;
					}
				}
			}
		}
		m_oldCursor = m_currentCursor;
		TestModeInput.Key key = TestModeInput.Key.HeadPhone;
		TestModeInput.Key key2 = TestModeInput.Key.Up;
		if (TestModeSkeleton.SingletonBehaviour<TestModeInput>.Instance.IsRepeat(key2))
		{
			key = key2;
		}
		key2 = TestModeInput.Key.Down;
		if (TestModeSkeleton.SingletonBehaviour<TestModeInput>.Instance.IsRepeat(key2))
		{
			key = key2;
		}
		key2 = TestModeInput.Key.Left;
		if (TestModeSkeleton.SingletonBehaviour<TestModeInput>.Instance.IsRepeat(key2))
		{
			key = key2;
		}
		key2 = TestModeInput.Key.Right;
		if (TestModeSkeleton.SingletonBehaviour<TestModeInput>.Instance.IsRepeat(key2))
		{
			key = key2;
		}
		key2 = TestModeInput.Key.Test;
		if (TestModeSkeleton.SingletonBehaviour<TestModeInput>.Instance.IsRepeat(key2))
		{
			key = key2;
		}
		key2 = TestModeInput.Key.Enter;
		if (TestModeSkeleton.SingletonBehaviour<TestModeInput>.Instance.IsTrigger(key2))
		{
			key = key2;
		}
		key2 = TestModeInput.Key.Service;
		if (TestModeSkeleton.SingletonBehaviour<TestModeInput>.Instance.IsTrigger(key2))
		{
			key = key2;
		}
		if (key != TestModeInput.Key.HeadPhone)
		{
			TriggerAction(key);
		}
		MenuSceneUpdate(menuPanelActiveInHierarchy: true);
	}

	protected virtual void TriggerAction(TestModeInput.Key key)
	{
		switch (key)
		{
		case TestModeInput.Key.Down:
		case TestModeInput.Key.Test:
			m_currentCursor = GetNextCursor(m_MenuElements, m_currentCursor);
			return;
		case TestModeInput.Key.Up:
			m_currentCursor = GetNextCursor(m_MenuElements, m_currentCursor, isBack: true);
			return;
		}
		if (m_MenuElements.Count > 0)
		{
			CheckElement(m_MenuElements[m_currentCursor], CALL_TYPE.TRIGGER, key);
		}
	}

	private void SetTouchButton(TestModeHelp.HelpType state)
	{
		if ((bool)ArrowkeyUp)
		{
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: true);
			switch (state)
			{
			case TestModeHelp.HelpType.SelectExecute:
			case TestModeHelp.HelpType.SelectExit:
				ArrowkeyLeft.SetActive(value: false);
				ArrowkeyRight.SetActive(value: false);
				TestModeSkeleton.SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
				break;
			default:
				Debug.LogError("未対応");
				break;
			case TestModeHelp.HelpType.UDLRSelectExecute:
				break;
			}
		}
	}

	protected abstract void MenuSceneUpdate(bool menuPanelActiveInHierarchy);

	protected int GetNextCursor(List<MenuElement> menuElement, int startCursor, bool isBack = false, int add = 1)
	{
		int num = 0;
		int num2 = add;
		if (isBack)
		{
			num2 = -add;
		}
		bool flag = true;
		num = startCursor;
		if (menuElement.Count <= 0)
		{
			return num;
		}
		while (flag)
		{
			num += num2;
			if (num < 0)
			{
				num = menuElement.Count - 1;
			}
			else if (num >= menuElement.Count)
			{
				num = 0;
			}
			if (menuElement[num].MenuText != null && menuElement[num].MenuText.IsActive())
			{
				flag = false;
			}
			if (startCursor == num)
			{
				flag = false;
			}
		}
		return num;
	}

	protected void CheckElement(MenuElement element, CALL_TYPE c_type, TestModeInput.Key key)
	{
		if (element.EventType == EVENT_TYPE.EVENT)
		{
			ChosenElement(element, c_type, key);
		}
		else if (c_type == CALL_TYPE.TRIGGER)
		{
			if (element.EventType == EVENT_TYPE.EXIT)
			{
				SetSceneExit();
				SceneManager.UnloadSceneAsync(base.gameObject.scene);
			}
			else if (element.EventType != EVENT_TYPE.SAVE_EXIT && element.EventType != EVENT_TYPE.ABORT && element.EventType != EVENT_TYPE.FACTORY_SETTINGS)
			{
				_ = element.EventType;
			}
		}
	}

	protected virtual void ChosenElement(MenuElement element, CALL_TYPE c_type, TestModeInput.Key key)
	{
	}

	protected virtual bool OnCursorElement(MenuElement element)
	{
		return false;
	}

	protected virtual void OffCursorElement(MenuElement element)
	{
	}

	protected void SetSceneExit()
	{
		if (!isExitReservation)
		{
			SceneExit();
			isExitReservation = true;
		}
	}

	protected virtual void SceneExit()
	{
	}
}

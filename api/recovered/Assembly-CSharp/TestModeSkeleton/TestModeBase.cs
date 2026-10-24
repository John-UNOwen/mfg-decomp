using System.Collections;
using System.Collections.Generic;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class TestModeBase : SceneFlowBase
{
	private class Localize
	{
		public LanguageOptionsDef.TestMode Language { get; set; }

		public Dictionary<Text, string> OriginalTexts { get; set; }
	}

	[SerializeField]
	[Tooltip("メニューリスト")]
	protected List<TestModeMenuElement> menuElements;

	protected static readonly float MessageWaitingSec = 2f;

	protected static readonly string On = "<color=red>ON</color>";

	protected static readonly string Off = "<color=white>OFF</color>";

	private Text SoftIDText;

	private Text titleText;

	private Text subTitleText;

	private Text subSubTitleText;

	protected TestModeNotice notice;

	protected TestModeHelp help;

	protected GameObject ArrowkeyGameObject;

	protected GameObject ArrowkeyUp;

	protected GameObject ArrowkeyDown;

	protected GameObject ArrowkeyLeft;

	protected GameObject ArrowkeyRight;

	protected GameObject ArrowkeyEnter;

	protected GameObject tenkeyGameObject;

	private Localize localize = new Localize();

	private IEnumerator mainCoroutine;

	private int? changeCursor;

	protected virtual string TitleName => "";

	protected virtual string SubTitleName => "";

	protected virtual string SubSubTitleName => "";

	protected virtual bool IsMenuLocked => false;

	protected new void Awake()
	{
		base.Awake();
		SoftIDText = GameObject.Find("TestModeMain/Canvas/Softid")?.GetComponent<Text>();
		SoftIDText.text = SystemUtility.Instance.SoftID;
		titleText = GameObject.Find("TestModeMain/Canvas/Title/Main/Text")?.GetComponent<Text>();
		subTitleText = GameObject.Find("TestModeMain/Canvas/Title/Sub/Text")?.GetComponent<Text>();
		subSubTitleText = GameObject.Find("TestModeMain/Canvas/Title/SubSub/Text")?.GetComponent<Text>();
		notice = GameObject.Find("TestModeMain")?.GetComponent<TestModeNotice>();
		help = GameObject.Find("TestModeMain")?.GetComponent<TestModeHelp>();
		tenkeyGameObject = GameObject.Find("TestModeMain/Canvas/Tenkey");
		tenkeyGameObject.SetActive(value: false);
		ArrowkeyGameObject = GameObject.Find("TestModeMain/Canvas/KeyPad");
		ArrowkeyGameObject.SetActive(value: false);
		ArrowkeyUp = GameObject.Find("TestModeMain/Canvas/KeyPad/Up");
		ArrowkeyDown = GameObject.Find("TestModeMain/Canvas/KeyPad/Down");
		ArrowkeyLeft = GameObject.Find("TestModeMain/Canvas/KeyPad/Left");
		ArrowkeyRight = GameObject.Find("TestModeMain/Canvas/KeyPad/Right");
		ArrowkeyEnter = GameObject.Find("TestModeMain/Canvas/KeyPad/Enter");
		SingletonBehaviour<TestModeInput>.Instance.Clear();
		localize.Language = CabinetManager.Lang.Data.TestModeLanguage;
		localize.OriginalTexts = SingletonBehaviour<TestModeLocalization>.Instance.SetTagTexts(base.gameObject);
		SetTitle(TitleName, SubTitleName, SubSubTitleName);
	}

	protected void ChangeMenu(string name)
	{
		changeCursor = GetEventCursor(name);
	}

	private IEnumerator CoMain()
	{
		if (menuElements.Count == 0)
		{
			yield break;
		}
		int currentCursor = GetFirstCursor();
		menuElements.FindAll((TestModeMenuElement e) => e.State == TestModeMenuElement.TextState.Selecting).ForEach(delegate(TestModeMenuElement e)
		{
			e.State = TestModeMenuElement.TextState.Default;
		});
		menuElements[currentCursor].State = TestModeMenuElement.TextState.Selecting;
		OnChangeMenuOn(menuElements[currentCursor]);
		while (true)
		{
			TestModeMenuElement element = menuElements[currentCursor];
			OnUpdate(element);
			if (!IsMenuLocked && SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
			{
				switch (element.Mode)
				{
				case TestModeMenuElement.EventMode.NextScene:
					yield return SceneFlowBase.co_scene(element.Event, CoSceneOption.SceneEnd, delegate
					{
						help.Call();
						OnCall(element);
					}, delegate
					{
						if (this == null)
						{
							Debug.Log("<color=red>TestModeBase.funcSceneUnloaded(): this is null.</color>");
						}
						else
						{
							SetTitle(TitleName, SubTitleName, SubSubTitleName);
							help.Return();
							if (localize.Language != CabinetManager.Lang.Data.TestModeLanguage)
							{
								localize.Language = CabinetManager.Lang.Data.TestModeLanguage;
								SingletonBehaviour<TestModeLocalization>.Instance.SetTagTexts(localize.OriginalTexts);
							}
							OnReturn(element);
						}
					});
					break;
				case TestModeMenuElement.EventMode.Event:
					yield return OnCoDecide(element);
					break;
				case TestModeMenuElement.EventMode.FactorySettings:
					OnFactorySettings(element);
					break;
				case TestModeMenuElement.EventMode.SaveAndExit:
				case TestModeMenuElement.EventMode.Exit:
				{
					bool flag = element.Event == "EventMode";
					CabinetManager.Coin.EventMode = (flag ? CoinOptionsDef.EventMode.On : CoinOptionsDef.EventMode.Off);
					if (element.Mode == TestModeMenuElement.EventMode.SaveAndExit)
					{
						yield return CoMainSaveExit();
					}
					else
					{
						yield return CoMainExit();
					}
					OnBeforeExit();
					if (element.Event != "IgnoreExit")
					{
						Exit();
					}
					yield break;
				}
				}
			}
			else if (!IsMenuLocked && (SingletonBehaviour<TestModeInput>.Instance.IsMenuUp() || SingletonBehaviour<TestModeInput>.Instance.IsMenuDown()))
			{
				currentCursor = GetNextCursor(currentCursor, SingletonBehaviour<TestModeInput>.Instance.IsMenuDown(), out var isChange);
				if (isChange)
				{
					element.State = TestModeMenuElement.TextState.Default;
					menuElements[currentCursor].State = TestModeMenuElement.TextState.Selecting;
					OnChangeMenuOff(element);
					OnChangeMenuOn(menuElements[currentCursor]);
				}
			}
			else if (changeCursor.HasValue && currentCursor != changeCursor.Value)
			{
				currentCursor = changeCursor.Value;
				element.State = TestModeMenuElement.TextState.Default;
				menuElements[currentCursor].State = TestModeMenuElement.TextState.Selecting;
				OnChangeMenuOff(element);
				OnChangeMenuOn(menuElements[currentCursor]);
				changeCursor = null;
			}
			yield return null;
		}
	}

	protected virtual IEnumerator CoMainSaveExit()
	{
		float start = Time.time;
		notice.StartNowSaving();
		yield return null;
		OnSave();
		yield return new WaitUntil(() => Time.time - start >= MessageWaitingSec);
		notice.Stop();
	}

	protected virtual IEnumerator CoMainExit()
	{
		if (IsModification())
		{
			ArrowkeyUp.SetActive(value: false);
			ArrowkeyDown.SetActive(value: false);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: true);
			notice.StartSaveOrNot(delegate
			{
				help.Set(TestModeHelp.HelpType.YesOrNo);
			});
			yield return new WaitUntil(() => notice.DecidedAnswer != TestModeNotice.Answer.NoAnswer);
			if (notice.DecidedAnswer == TestModeNotice.Answer.Yes)
			{
				yield return CoMainSaveExit();
				yield break;
			}
			notice.StartNoModification();
			yield return new WaitForSeconds(MessageWaitingSec);
			notice.Stop();
		}
	}

	protected virtual void OnUpdate(TestModeMenuElement element)
	{
	}

	protected virtual void OnCall(TestModeMenuElement element)
	{
	}

	protected virtual void OnReturn(TestModeMenuElement element)
	{
	}

	protected virtual IEnumerator OnCoDecide(TestModeMenuElement element)
	{
		yield break;
	}

	protected virtual void OnChangeMenuOn(TestModeMenuElement element)
	{
	}

	protected virtual void OnChangeMenuOff(TestModeMenuElement element)
	{
	}

	protected virtual void OnFactorySettings(TestModeMenuElement element)
	{
	}

	protected virtual void OnSave()
	{
	}

	protected virtual void OnBeforeExit()
	{
	}

	protected virtual bool IsModification()
	{
		return false;
	}

	private void SetTitle(string main, string sub, string subSub)
	{
		titleText.SetLocalizedText(main);
		SetSubTitle(sub);
		SetSubSubTitle(subSub);
	}

	protected void SetSubTitle(string sub)
	{
		subTitleText.SetLocalizedText(sub);
	}

	protected void SetSubSubTitle(string subSub)
	{
		subSubTitleText.SetLocalizedText(subSub);
	}

	private int GetEventCursor(string str)
	{
		int num = menuElements.FindIndex((TestModeMenuElement e) => e.Event == str);
		if (num >= 0)
		{
			return num;
		}
		return 0;
	}

	private int GetFirstCursor()
	{
		if (GetType() == typeof(TestModeMain) || GetType() == typeof(IOCheckMain))
		{
			return 0;
		}
		int num = menuElements.FindLastIndex((TestModeMenuElement e) => e.IsSelectable && e.Mode == TestModeMenuElement.EventMode.Exit);
		if (num >= 0)
		{
			return num;
		}
		num = menuElements.FindLastIndex((TestModeMenuElement e) => e.IsSelectable && e.Mode == TestModeMenuElement.EventMode.SaveAndExit);
		if (num >= 0)
		{
			return num;
		}
		num = menuElements.FindIndex((TestModeMenuElement e) => e.IsSelectable);
		if (num >= 0)
		{
			return num;
		}
		return 0;
	}

	private int GetNextCursor(int start, bool isAdd, out bool isChange)
	{
		if (menuElements.Count == 0)
		{
			isChange = false;
			return 0;
		}
		int num = start;
		do
		{
			num = (int)Mathf.Repeat(num + (isAdd ? 1 : (-1)), menuElements.Count);
		}
		while (start != num && !menuElements[num].IsSelectable);
		isChange = num != start;
		return num;
	}

	protected void StartOrResumeMainCoroutine()
	{
		mainCoroutine = mainCoroutine ?? CoMain();
		StartCoroutine(mainCoroutine);
	}

	protected void StopMainCoroutine()
	{
		if (mainCoroutine != null)
		{
			StopCoroutine(mainCoroutine);
			mainCoroutine = null;
		}
	}

	protected void PauseMainCoroutine()
	{
		if (mainCoroutine != null)
		{
			StopCoroutine(mainCoroutine);
		}
	}

	protected void StopCoroutine(ref Coroutine coroutine)
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
	}

	protected void SetSoftIdCodeDisp(bool disp)
	{
		GameObject gameObject = GameObject.Find("TestModeMain/Canvas/Softid");
		if ((bool)gameObject)
		{
			gameObject.SetActive(disp);
		}
	}
}

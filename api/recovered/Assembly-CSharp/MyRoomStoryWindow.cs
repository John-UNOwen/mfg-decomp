using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Story;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomStoryWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject m_eventScenario;

	[SerializeField]
	private Button m_girlsTab;

	[SerializeField]
	private ScrollView m_eventScrollView;

	[SerializeField]
	private TextMeshProUGUI m_eventOpenNum;

	[SerializeField]
	public GameObject m_girlsScenario;

	[SerializeField]
	private Button m_eventTab;

	[SerializeField]
	private ScrollView m_girlsScrollView;

	[SerializeField]
	private TextMeshProUGUI m_girlsOpenNum;

	[SerializeField]
	private GameObject m_storySelectButtonPrefab;

	[SerializeField]
	private GameObject m_confirmWindow;

	[SerializeField]
	private Button m_confirmYesButton;

	[SerializeField]
	private Button m_confirmNoButton;

	[SerializeField]
	private Image m_storyImage;

	[SerializeField]
	private GameObject m_charaNameWindow;

	[SerializeField]
	private TextMeshProUGUI m_charaName;

	[SerializeField]
	private TextMeshProUGUI m_storyName;

	[SerializeField]
	private GameObject m_unableWindow;

	private MyRoomFlow myRoomFlow;

	public bool IsOpen;

	private Subject<Unit> m_onDecided = new Subject<Unit>();

	public IObservable<Unit> OnDecided => m_onDecided.AsObservable();

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
	}

	public void Refresh()
	{
		GameDataManager.Story.CheckUnlock();
		CreateGirlsStorySelectButton();
	}

	public void Init(MyRoomFlow mrf)
	{
		if (GameDataManager.PlayerConfig.IsStoryRead && !DebugFlag.IsActive(DebugFlag.Flag.EnableUnlimitedButton))
		{
			m_unableWindow.SetActive(value: true);
			return;
		}
		myRoomFlow = mrf;
		GameDataManager.Story.CheckUnlock();
		CreateEventStorySelectButton();
		CreateGirlsStorySelectButton();
		m_eventTab.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_eventScenario.SetActive(value: true);
			m_girlsScenario.SetActive(value: false);
		}).AddTo(this);
		m_girlsTab.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_girlsScenario.SetActive(value: true);
			m_eventScenario.SetActive(value: false);
		}).AddTo(this);
		m_confirmYesButton.OnClickAsObservable().Subscribe(async delegate
		{
			myRoomFlow.StoryPlay();
		}).AddTo(this);
		m_confirmNoButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_confirmWindow.GetComponent<Animator>().SetTrigger("Out");
		}).AddTo(this);
		m_eventScenario.SetActive(value: true);
	}

	private void SortStory(List<StoryInfo> posStories)
	{
		posStories.Sort((StoryInfo x, StoryInfo y) => y.Master.StoryID - x.Master.StoryID);
	}

	private void CreateStorySelectButton(ScrollView scrollView, StoryInfo storyInfo)
	{
		GameObject gameObject = scrollView.Add(m_storySelectButtonPrefab);
		StorySelectButton listButtonScript = gameObject.GetComponent<StorySelectButton>();
		listButtonScript.SetStoryInfo(storyInfo).Forget();
		gameObject.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
		{
			m_confirmWindow.SetActive(value: true);
			m_storyImage.sprite = listButtonScript.GetImage().sprite;
			if (storyInfo.Master.StoryType == StoryType.GirlsMainStory || storyInfo.Master.StoryType == StoryType.GirlsLoveStory)
			{
				m_charaNameWindow.SetActive(value: true);
				m_charaName.text = MasterGameDataManager.Character.Properties(storyInfo.Master.CharaType).Profile.Name;
			}
			else
			{
				m_charaNameWindow.SetActive(value: false);
			}
			m_storyName.text = storyInfo.Master.DispName;
			m_confirmWindow.GetComponent<Animator>().SetTrigger("In");
		})
			.AddTo(this);
	}

	private void CreateEventStorySelectButton()
	{
		List<StoryInfo> list = GameDataManager.Story.Stories.FindAll(delegate(StoryInfo x)
		{
			Story master = x.Master;
			return master != null && master.StoryType == StoryType.EventStory;
		});
		SortStory(list);
		for (int i = 0; i < list.Count; i++)
		{
			CreateStorySelectButton(m_eventScrollView, list[i]);
		}
		m_eventOpenNum.text = list.FindAll((StoryInfo x) => x.IsAvailable).Count.ToString();
	}

	public void CreateGirlsStorySelectButton()
	{
		m_girlsScrollView.ResetList();
		List<StoryInfo> list = GameDataManager.Story.Stories.FindAll(delegate(StoryInfo x)
		{
			Story master = x.Master;
			if (master == null || master.StoryType != StoryType.GirlsMainStory)
			{
				Story master2 = x.Master;
				if (master2 == null || master2.StoryType != StoryType.GirlsLoveStory)
				{
					return false;
				}
			}
			return x.Master?.CharaType == GameDataManager.Player.SelectChara;
		});
		SortStory(list);
		for (int i = 0; i < list.Count; i++)
		{
			CreateStorySelectButton(m_girlsScrollView, list[i]);
		}
		m_girlsOpenNum.text = list.FindAll((StoryInfo x) => x.IsAvailable).Count.ToString();
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.Costume;
using MFG.Types.Story;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class StoryPlayFlow : BaseFlow
{
	[SerializeField]
	public StandingCharaManager charaManager;

	[Header("デバッグ機能")]
	[SerializeField]
	public bool IsAutoSkipDisable;

	[Header("オブジェクト")]
	[SerializeField]
	public GameObject textWindow;

	[SerializeField]
	public TextMeshProUGUI textObj;

	[SerializeField]
	public GameObject textSetBg;

	[NonSerialized]
	public float textSetBgAlpha;

	[SerializeField]
	public GameObject charaNameBg;

	[NonSerialized]
	public float charaNameBgAlpha;

	[SerializeField]
	public TextMeshProUGUI nameTextObj;

	[SerializeField]
	public GameObject chara3DSet;

	[SerializeField]
	public GameObject chara2DSet;

	[SerializeField]
	public GameObject bgSet;

	[SerializeField]
	public GameObject animeTop;

	[SerializeField]
	public GameObject imageTop;

	[SerializeField]
	public GameObject imagePrefab;

	[SerializeField]
	public GameObject cursor;

	[SerializeField]
	public GameObject fadeWhite;

	[SerializeField]
	public GameObject fadeBlack;

	[SerializeField]
	public GameObject fastForwardTextObj;

	[NonSerialized]
	public float fastForwardSpeed = 3f;

	[NonSerialized]
	public float fastForwardNeedTime = 1f;

	[SerializeField]
	private GameObject debug;

	[SerializeField]
	private GameObject debugScriptList;

	[SerializeField]
	private GameObject debugScriptListButton;

	[SerializeField]
	private Button debugExitButton;

	[SerializeField]
	public GameObject selectWindow;

	[SerializeField]
	public Button selectButton1;

	[SerializeField]
	public Button selectButton2;

	[SerializeField]
	public Button selectButton3;

	[SerializeField]
	public Button selectButton4;

	[SerializeField]
	public Button selectButton5;

	[SerializeField]
	public GameObject skipWindow;

	[SerializeField]
	public Button skipYesButton;

	[SerializeField]
	public Button skipNoButton;

	[SerializeField]
	public Button allSkipButton;

	[SerializeField]
	public List<Command> m_commandList = new List<Command>();

	[SerializeField]
	private GameObject blackOut;

	public int m_selectedIndex = -1;

	public int m_nextCommand = -1;

	[NonSerialized]
	public float m_textSpeedDefault = 0.03f;

	public float m_textSpeed;

	public bool m_textSkipEnable = true;

	public string bgName = "Bg1";

	public bool IsSkipWindowOpen;

	[NonSerialized]
	public string m_playingVoiceLabel = "";

	[NonSerialized]
	public bool m_loadingCharacter;

	private static StoryPlayFlow m_instance = null;

	public List<ItemID> existCostumes = new List<ItemID>();

	private bool _isForceExit;

	private bool _isAllSkip;

	private bool m_isDebugPlay;

	private static float _debugSelectScrollPos = 1f;

	private float _time;

	public bool _isHiSpeed;

	public bool _isTouch;

	private float _screenPressDurationTime;

	private bool m_isSetNextCommand;

	[NonSerialized]
	public Dictionary<string, List<float>> SnsWaitSec = new Dictionary<string, List<float>>();

	[SerializeField]
	private AccountGameData m_accountGameData => GameDataManager.Account;

	public bool IsHiseed => _isHiSpeed;

	public static StoryPlayFlow GetInstance()
	{
		if (!m_instance)
		{
			m_instance = GameObject.Find("SceneFlow").GetComponent<StoryPlayFlow>();
		}
		return m_instance;
	}

	private StoryPlayFlow()
	{
		_enableDebugLogin = false;
		_enableLoadingTipsDisp = true;
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		textSetBgAlpha = textSetBg.GetComponent<Image>().color.a;
		charaNameBgAlpha = charaNameBg.GetComponent<Image>().color.a;
		foreach (Transform item in bgSet.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		allSkipButton.OnClickAsObservable().Subscribe(delegate
		{
			if (!IsSkipWindowOpen)
			{
				IsSkipWindowOpen = true;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				skipWindow.SetActive(value: true);
				skipWindow.GetComponent<Animator>().SetTrigger("In");
			}
		}).AddTo(this);
		skipYesButton.OnClickAsObservable().Subscribe(delegate
		{
			_isAllSkip = true;
			IsSkipWindowOpen = false;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			skipWindow.GetComponent<Animator>().SetTrigger("Out");
		}).AddTo(this);
		skipNoButton.OnClickAsObservable().Subscribe(delegate
		{
			_isAllSkip = false;
			IsSkipWindowOpen = false;
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			skipWindow.GetComponent<Animator>().SetTrigger("Out");
		}).AddTo(this);
		selectButton1.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_selectedIndex = 0;
		}).AddTo(this);
		selectButton2.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_selectedIndex = 1;
		}).AddTo(this);
		selectButton3.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_selectedIndex = 2;
		}).AddTo(this);
		selectButton4.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_selectedIndex = 3;
		}).AddTo(this);
		selectButton5.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_selectedIndex = 4;
		}).AddTo(this);
		await LoadNameChangeTable();
		await InitBgSet();
		StoryID playStoryScriptID = GameDataManager.Story.m_playStoryScriptID;
		try
		{
			m_commandList = await StoryScript_BuildCommand(await StoryScript_LoadCsvText(playStoryScriptID), new Progress<float>(delegate(float rate)
			{
				ResidentFlow.GetInstance().SetLoadingProgress(rate);
			}));
		}
		catch
		{
			Debug.LogError("スクリプトがない");
			await ResidentFlow.GetInstance().SetLoadingProgressComplete();
			ResidentFlow.GetInstance().ExitLoading();
			await ResidentFlow.GetInstance().TransitionEffectClose();
			await GotoNextScene();
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		if (!_isNextSceneDecided)
		{
			await StoryScript_Exec();
			if (!_isForceExit)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(1.0));
				await GotoNextScene();
			}
		}
	}

	private async UniTask<string> ExecDebugStorySelect()
	{
		ScrollRect scrollRect = debug.GetComponentInChildren<ScrollRect>();
		debug.SetActive(value: true);
		string path = Application.dataPath + "/MFG/Story/StoryScripts";
		string[] files = null;
		try
		{
			files = Directory.GetFiles(path, "*.csv");
		}
		catch
		{
		}
		bool onDebugStoryDecided = false;
		int decidedIndex = -1;
		for (int j = 0; j < files?.Length; j++)
		{
			GameObject obj2 = UnityEngine.Object.Instantiate(debugScriptListButton);
			obj2.transform.SetParent(debugScriptList.transform, worldPositionStays: false);
			obj2.transform.Find("Text").GetComponent<Text>().text = Path.GetFileName(files[j]);
			int i = j;
			obj2.GetComponent<Button>().onClick.AddListener(delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				onDebugStoryDecided = true;
				decidedIndex = i;
			});
		}
		scrollRect.verticalNormalizedPosition = _debugSelectScrollPos;
		SoundManager.StopBgmWithFadeout().Forget();
		SoundManager.StopSeAll(1f).Forget();
		SoundManager.StopVoiceAllWithFadeout().Forget();
		await ResidentFlow.GetInstance().TransitionEffectClose();
		await ResidentFlow.GetInstance().SetLoadingProgressComplete();
		ResidentFlow.GetInstance().ExitLoading();
		await UniTask.WaitUntil(() => onDebugStoryDecided);
		debug.SetActive(value: false);
		_debugSelectScrollPos = scrollRect.verticalNormalizedPosition;
		await ResidentFlow.GetInstance().TransitionEffectOpen("StoryPlay");
		await ResidentFlow.GetInstance().StartLoading();
		return files[decidedIndex];
	}

	private string StoryScript_LoadCsvText(string scriptFilePath)
	{
		using FileStream stream = new FileStream(scriptFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}

	private async UniTask<string> StoryScript_LoadCsvText(StoryID storyID)
	{
		if (storyID == StoryID.None)
		{
			throw new ArgumentException();
		}
		IDisposableAsset<TextAsset> obj = await AssetLoader.LoadAssetAsync<TextAsset>(StoryMaster.GetStory(storyID).ScriptPath);
		obj.AddTo(this);
		return obj.Asset.text;
	}

	private int StoryScript_GetUsedCostumeCount(string scriptCSVText)
	{
		if (string.IsNullOrEmpty(scriptCSVText))
		{
			return 0;
		}
		using StringReader stringReader = new StringReader(scriptCSVText);
		HashSet<string> hashSet = new HashSet<string>();
		stringReader.ReadLine();
		while (stringReader.Peek() > -1)
		{
			string[] array = stringReader.ReadLine().Replace("\\n", "\n").Split(',');
			if (!(array[0] != "〇"))
			{
				if (array[1].Contains("NW"))
				{
					array[1] = array[1].TrimEnd('N', 'W');
				}
				if (StoryScript_IsCharaCommand(array[1]))
				{
					hashSet.Add(array[2] + (string.IsNullOrEmpty(array[28]) ? "制服" : array[28]));
				}
			}
		}
		return hashSet.Count;
	}

	private bool StoryScript_IsCharaCommand(string commandName)
	{
		if (!(commandName == "CharaControl"))
		{
			return Assembly.GetExecutingAssembly().GetType("MFG.StoryPlay." + commandName)?.IsSubclassOf(typeof(CharaCommand)) ?? false;
		}
		return true;
	}

	private async UniTask<List<Command>> StoryScript_BuildCommand(string scriptCSVText, IProgress<float> progress = null)
	{
		int usedCostumeCount = StoryScript_GetUsedCostumeCount(scriptCSVText);
		HashSet<string> loadedCostumeList = new HashSet<string>();
		List<Command> commandList = new List<Command>();
		scriptCSVText.Count((char c) => c == '\n');
		using StringReader stringReader = new StringReader(scriptCSVText);
		stringReader.ReadLine();
		int curLineCount = 0;
		while (stringReader.Peek() > -1)
		{
			bool isCharaCommand = false;
			int num = curLineCount + 1;
			curLineCount = num;
			float value = ((usedCostumeCount == 0) ? 0f : ((float)loadedCostumeList.Count() / (float)usedCostumeCount));
			value = Math.Clamp(value, 0f, 1f);
			progress?.Report(value);
			string[] values = stringReader.ReadLine().Replace("\\n", "\n").Split(',');
			if (values[0] != "〇")
			{
				continue;
			}
			bool wait = true;
			float waitSec = 0f;
			if (values[1].Contains("NW"))
			{
				wait = false;
				values[1] = values[1].TrimEnd('N', 'W');
			}
			if (values[1] == "TextDisp")
			{
				wait = false;
			}
			if (values[1] == "CharaControl")
			{
				isCharaCommand = true;
				if (!string.IsNullOrEmpty(values[17]))
				{
					CharaPartsDisp cmdParts = new CharaPartsDisp();
					await cmdParts.Parse(values);
					commandList.Add(cmdParts);
				}
				CharaAlpha cmdAlpha = new CharaAlpha
				{
					LineNo = curLineCount + 1
				};
				await cmdAlpha.Parse(values);
				commandList.Add(cmdAlpha);
				CharaScale cmdScale = new CharaScale
				{
					LineNo = curLineCount + 1
				};
				await cmdScale.Parse(values);
				commandList.Add(cmdScale);
				CharaRotateZ cmdRotate = new CharaRotateZ
				{
					LineNo = curLineCount + 1
				};
				await cmdRotate.Parse(values);
				commandList.Add(cmdRotate);
				CharaEmotion cmdEmotion = new CharaEmotion
				{
					LineNo = curLineCount + 1
				};
				await cmdEmotion.Parse(values);
				commandList.Add(cmdEmotion);
				CharaMove cmdMove = new CharaMove
				{
					LineNo = curLineCount + 1
				};
				await cmdMove.Parse(values);
				commandList.Add(cmdMove);
				waitSec = cmdMove.sec;
			}
			else if (values[1] == "ImageControl")
			{
				isCharaCommand = false;
				ImageAlpha cmdAlpha2 = new ImageAlpha
				{
					LineNo = curLineCount + 1
				};
				await cmdAlpha2.Parse(values);
				commandList.Add(cmdAlpha2);
				ImageScale cmdScale2 = new ImageScale
				{
					LineNo = curLineCount + 1
				};
				await cmdScale2.Parse(values);
				commandList.Add(cmdScale2);
				ImageRotateZ cmdRotate2 = new ImageRotateZ
				{
					LineNo = curLineCount + 1
				};
				await cmdRotate2.Parse(values);
				commandList.Add(cmdRotate2);
				ImageMove cmdMove2 = new ImageMove
				{
					LineNo = curLineCount + 1
				};
				await cmdMove2.Parse(values);
				commandList.Add(cmdMove2);
				waitSec = cmdMove2.sec;
			}
			else
			{
				Type type = Assembly.GetExecutingAssembly().GetType("MFG.StoryPlay." + values[1]);
				if (type == null)
				{
					Debug.LogWarning($"LineNo:{curLineCount + 1} 対応していないコマンドです:{values[1]}");
				}
				else
				{
					Command cmd2 = (Command)Activator.CreateInstance(type);
					cmd2.LineNo = curLineCount + 1;
					cmd2.token = this.GetCancellationTokenOnDestroy();
					await cmd2.Parse(values);
					commandList.Add(cmd2);
					waitSec = cmd2.sec;
				}
				if (StoryScript_IsCharaCommand(values[1]))
				{
					isCharaCommand = true;
				}
			}
			if (wait)
			{
				WaitSec cmd = new WaitSec
				{
					LineNo = curLineCount + 1
				};
				await cmd.Parse(values);
				cmd.sec = waitSec;
				commandList.Add(cmd);
			}
			if (isCharaCommand)
			{
				loadedCostumeList.Add(values[2] + (string.IsNullOrEmpty(values[28]) ? "制服" : values[28]));
			}
		}
		return commandList;
	}

	private async UniTask StoryScript_Exec()
	{
		m_nextCommand = 0;
		textWindow.SetActive(value: false);
		while (!_isForceExit)
		{
			Command cmd = m_commandList[m_nextCommand];
			if (_isHiSpeed && cmd.type != CommandType.SelectWindow)
			{
				cmd.sec /= fastForwardSpeed;
			}
			Debug.Log($"LineNo:{cmd.LineNo} type:{cmd.type}, target:{cmd.targetName} begin");
			await cmd.Exec();
			Debug.Log($"LineNo:{cmd.LineNo} type:{cmd.type}, target:{cmd.targetName} end");
			if (cmd.type == CommandType.FinishSetUp)
			{
				await ResidentFlow.GetInstance().SetLoadingProgressComplete();
				ResidentFlow.GetInstance().ExitLoading();
				await ResidentFlow.GetInstance().TransitionEffectClose();
				ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetDisp(isLowerDisp: false, isUpperDisp: true);
				await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			}
			if (!m_isSetNextCommand)
			{
				m_nextCommand++;
			}
			m_isSetNextCommand = false;
			if (m_nextCommand < m_commandList.Count)
			{
				continue;
			}
			break;
		}
	}

	protected override void OnUpdate()
	{
		if (ResidentFlow.CustomInput.GetMouseButton(0) && !IsSkipWindowOpen)
		{
			_screenPressDurationTime += Time.deltaTime;
		}
		else
		{
			_screenPressDurationTime = 0f;
		}
		if (_screenPressDurationTime > fastForwardNeedTime)
		{
			_isHiSpeed = true;
		}
		else
		{
			_isHiSpeed = false;
		}
		if (_isHiSpeed)
		{
			m_textSpeed = m_textSpeedDefault / fastForwardSpeed;
			fastForwardTextObj.GetComponent<TextMeshProUGUI>().text = "早送り中・・・";
		}
		else
		{
			m_textSpeed = m_textSpeedDefault;
			fastForwardTextObj.GetComponent<TextMeshProUGUI>().text = "画面長押しで早送り";
		}
		_time += Time.deltaTime;
		if (_time > m_textSpeed)
		{
			_time = 0f;
			textObj.maxVisibleCharacters++;
		}
		if (!_isAllSkip || _isForceExit)
		{
			return;
		}
		_isForceExit = true;
		if (m_nextCommand >= 0)
		{
			while (m_nextCommand < m_commandList.Count)
			{
				Command command = m_commandList[m_nextCommand];
				if (command.type == CommandType.SelectWindow)
				{
					GetInstance().m_selectedIndex = -1;
					command.ForceDecideChoice();
				}
				if (!m_isSetNextCommand)
				{
					m_nextCommand++;
				}
				m_isSetNextCommand = false;
			}
		}
		GotoNextScene().Forget();
	}

	private async UniTask GotoNextScene()
	{
		if (m_isDebugPlay)
		{
			GotoNextScene("StoryPlay");
			return;
		}
		if (!GameDataManager.Story.m_forcePlaying)
		{
			GameDataManager.PlayerConfig.IsStoryRead = true;
		}
		try
		{
			GameDataManager.Story.Stories.Find((StoryInfo x) => x.Master?.StoryID == GameDataManager.Story.m_playStoryScriptID).IsRead = true;
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
		if (GameDataManager.Story.m_forceEventStoryPlaying)
		{
			if (GameDataManager.Story.JudgeAndSetEventStoryPlay())
			{
				GotoNextScene("StoryPlay");
			}
			else
			{
				GotoNextScene(GameDataManager.Competition.GetNextSceneWhenGotoHome());
			}
		}
		else if (GameDataManager.Account.IsFirstPlay && GameDataManager.Story.m_forcePlaying && GameDataManager.Story.m_playStoryScriptID == StoryID.StoryEvent0000)
		{
			GotoNextScene("FirstCharaSelect");
		}
		else
		{
			GotoNextScene("CharaSelect");
		}
	}

	public void CharaTalkStart(ItemID costumeItemID)
	{
		if (costumeItemID != 0)
		{
			foreach (Transform item in chara2DSet.transform)
			{
				if (item.gameObject.name == GetChara2DObj(costumeItemID).name)
				{
					Utility.SetCharaColor(GetChara2DObj(costumeItemID), new Color(1f, 1f, 1f));
				}
				else
				{
					Utility.SetCharaColor(item.gameObject, new Color(0.5f, 0.5f, 0.5f));
				}
			}
			return;
		}
		foreach (Transform item2 in chara2DSet.transform)
		{
			Utility.SetCharaColor(item2.gameObject, new Color(1f, 1f, 1f));
		}
	}

	public void CharaTalkEnd()
	{
		foreach (Transform item in chara2DSet.transform)
		{
			Utility.SetCharaColor(item.gameObject, new Color(1f, 1f, 1f));
		}
	}

	public GameObject GetChara2DObj(string charaName, string costumeID)
	{
		return GetChara2DObj(Utility.GetCostumeItemID(charaName, costumeID));
	}

	public GameObject GetChara2DObj(ItemID costumeItemID)
	{
		Transform transform = chara2DSet.transform.Find(costumeItemID.ToString());
		if ((bool)transform)
		{
			return transform.gameObject;
		}
		Debug.LogWarning("キャラオブジェクトが存在しません:" + costumeItemID);
		return null;
	}

	public GameObject GetChara3DObj(ItemID costumeItemID)
	{
		Transform transform = chara3DSet.transform.Find(costumeItemID.ToString());
		if ((bool)transform)
		{
			return transform.gameObject;
		}
		Debug.LogWarning("キャラオブジェクトが存在しません:" + costumeItemID);
		return null;
	}

	public async UniTask InitBgSet()
	{
		bgName = "Bg1";
		await SetBg("控室", next: false);
	}

	public GameObject GetBgObj(bool next = false)
	{
		return bgSet.transform.Find(next ? GetNextBgName() : bgName).gameObject;
	}

	public void ChangeBgName()
	{
		bgName = GetNextBgName();
	}

	public string GetNextBgName()
	{
		if (!bgName.Equals("Bg1"))
		{
			return "Bg1";
		}
		return "Bg2";
	}

	public async UniTask SetBg(string name, bool next)
	{
		RectTransform component = GetBgObj(next).transform.GetComponent<RectTransform>();
		string bgObjectName = Utility.GetBgObjectName(name);
		switch (bgObjectName)
		{
		case "StoryPlayBg01":
		case "StoryPlayBg02":
		case "StoryPlayBg03":
		case "StoryPlayBg07":
		case "HomeBg":
		case "ResultBg":
			GetBgObj(next).transform.GetComponent<ContentSizeFitter>().enabled = false;
			component.sizeDelta = new Vector2(2524f, 1344f);
			component.localScale = new Vector3(0.85f, 0.85f, 0.85f);
			break;
		default:
			GetBgObj(next).transform.GetComponent<ContentSizeFitter>().enabled = true;
			component.localScale = new Vector3(1f, 1f, 1f);
			break;
		}
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveStoryImage(bgObjectName));
		GetBgObj(next).transform.GetComponent<Image>().sprite = disposableAsset.Asset;
	}

	public void SetNextCommandLabel(string label)
	{
		m_nextCommand = m_commandList.FindIndex((Command a) => a.label == label);
		m_isSetNextCommand = true;
		if (m_nextCommand < 0)
		{
			Debug.LogError("行先のラベルが見つかりません:" + label);
		}
	}

	private async UniTask LoadNameChangeTable()
	{
		Utility.ResetDictionary();
		IDisposableAsset<TextAsset> obj = await AssetLoader.LoadAssetAsync<TextAsset>("Assets/MFG/Story/StoryScripts/_NameChangeTable.csv");
		obj.AddTo(this);
		StringReader stringReader = new StringReader(obj.Asset.text);
		stringReader.ReadLine();
		while (stringReader.Peek() > -1)
		{
			string[] array = stringReader.ReadLine().Split(',');
			if (array[0] != "")
			{
				Utility.AddCharaName(array[0], array[1]);
			}
			if (array[2] != "")
			{
				Utility.AddBgName(array[2], array[3]);
			}
		}
	}

	public async UniTask<GameObject> InstantiateAnime(string path)
	{
		IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(path);
		obj.AddTo(this);
		return UnityEngine.Object.Instantiate(obj.Asset, animeTop.transform, worldPositionStays: false);
	}

	public async UniTask<GameObject> InstantiateComment(Transform parent, bool isMe, string iconPath, string nickName, string sendText)
	{
		if (parent == null)
		{
			return null;
		}
		string text = (isMe ? "Me" : "Friend");
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>("Story/SNSComment" + text + ".prefab");
		disposableAsset.AddTo(this);
		GameObject obj = UnityEngine.Object.Instantiate(disposableAsset.Asset, parent, worldPositionStays: false);
		if (obj != null && !string.IsNullOrEmpty(iconPath))
		{
			CommentsChild comment = obj.transform.GetComponent<CommentsChild>();
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(iconPath);
			disposableAsset2.AddTo(this);
			comment.SetIcon(disposableAsset2.Asset);
			comment.SetText(nickName, sendText);
			comment.Reflesh();
			obj.SetActive(value: false);
		}
		return obj;
	}

	public async UniTask<GameObject> InstantiateImage(Transform parent, bool isMe, string iconPath, string nickName, string imageUrl)
	{
		if (parent == null)
		{
			return null;
		}
		string text = (isMe ? "My" : "Friend");
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>("Story/SNSComment" + text + "Image.prefab");
		disposableAsset.AddTo(this);
		GameObject obj = UnityEngine.Object.Instantiate(disposableAsset.Asset, parent, worldPositionStays: false);
		if (obj != null && !string.IsNullOrEmpty(iconPath))
		{
			CommentsImageChild comment = obj.transform.GetComponent<CommentsImageChild>();
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(iconPath);
			disposableAsset2.AddTo(this);
			comment.SetIcon(disposableAsset2.Asset);
			IDisposableAsset<Sprite> disposableAsset3 = await AssetLoader.LoadAssetAsync<Sprite>(imageUrl);
			disposableAsset3.AddTo(this);
			comment.SetImage(disposableAsset3.Asset);
			comment.SetName(nickName);
			comment.Reflesh();
			obj.SetActive(value: false);
		}
		return obj;
	}

	public async UniTask<GameObject> InstantiateTweet(Transform parent, string iconPath, string nickName, string sendText, string[] reaction, string imageUrl)
	{
		if (parent == null)
		{
			return null;
		}
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>("Story/Tweet.prefab");
		disposableAsset.AddTo(this);
		GameObject obj = UnityEngine.Object.Instantiate(disposableAsset.Asset, parent, worldPositionStays: false);
		if (obj != null && !string.IsNullOrEmpty(iconPath))
		{
			TweetChild comment = obj.transform.GetComponent<TweetChild>();
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(iconPath);
			disposableAsset2.AddTo(this);
			comment.SetIcon(disposableAsset2.Asset);
			if (await AssetLoader.Exists(imageUrl))
			{
				IDisposableAsset<Sprite> disposableAsset3 = await AssetLoader.LoadAssetAsync<Sprite>(imageUrl);
				disposableAsset3.AddTo(this);
				comment.SetImage(disposableAsset3.Asset);
			}
			comment.SetText(nickName, sendText, reaction);
			comment.Reflesh();
			obj.SetActive(value: false);
		}
		return obj;
	}

	public async UniTask SetupSNSBg(Transform trf, string path)
	{
		if (!(trf == null) && await AssetLoader.Exists(path))
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(path);
			disposableAsset.AddTo(this);
			trf.GetComponent<Image>().sprite = disposableAsset.Asset;
		}
	}
}

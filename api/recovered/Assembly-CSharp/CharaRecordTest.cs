using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MahjongPlay.Character;
using MFG.MasterGameData;
using MFG.Types.Customize.Costume;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CharaRecordTest : BaseFlow
{
	[SerializeField]
	private StandingCharaManager m_charaManager;

	[SerializeField]
	private ItemID m_costumeID;

	[SerializeField]
	private ItemID m_costumeMahjongPlayID;

	[SerializeField]
	private string m_motionTrigger = "Wait0";

	private int motionIndex;

	[SerializeField]
	private int m_recordingTime = 10;

	private StandingCharaController _standingCharaController;

	private CharacterModelController m_characterModelController = new CharacterModelController();

	private CharaReaction m_charaReaction = new CharaReaction();

	private IDisposableAsset<GameObject> _prefab3D;

	private GameObject m_mahjongPlayCharaObject;

	[SerializeField]
	private GameObject m_mahjongPlayCharaParent;

	[SerializeField]
	private Camera m_charaCamera;

	[SerializeField]
	private RenderTexture m_charaRender;

	[SerializeField]
	private RenderTexture m_mpRender;

	[SerializeField]
	private Dropdown m_charaCheck;

	[SerializeField]
	private Dropdown m_charaCheckMahjongPlay;

	[SerializeField]
	private Dropdown m_motionDrop;

	[SerializeField]
	private TMP_InputField m_recTime;

	[SerializeField]
	private GameObject m_motionSelectParent;

	[SerializeField]
	private GameObject m_recTimeParent;

	[SerializeField]
	private Button m_motionPlay;

	[SerializeField]
	private Button m_recStart;

	[SerializeField]
	private Button m_snapShot;

	[SerializeField]
	private GameObject m_reference;

	private int charaAngle;

	private List<string> MotionList = new List<string>
	{
		"Wait0", "Wait1", "Reach0", "Reach1", "Touch1", "Touch2", "Touch3", "ResultPlus0", "ResultPlus1", "ResultMinus0",
		"ResultMinus1", "MatchingIn", "MatchingOut", "Anger", "Shy", "Surprise", "EyeContact", "Start", "Think", "Away1"
	};

	private bool IsCharaChanging;

	private bool IsMahjongPlayMotion => m_motionTrigger switch
	{
		"Touch1" => false, 
		"Touch2" => false, 
		"Touch3" => false, 
		"MatchingIn" => false, 
		"MatchingOut" => false, 
		_ => true, 
	};

	private bool IsStoryMotion => m_motionTrigger switch
	{
		"Start" => false, 
		"Think" => false, 
		"Away1" => false, 
		_ => true, 
	};

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		Application.targetFrameRate = 30;
		ResidentFlow.GetInstance().IsTestModeEffective = false;
		m_charaCheck.options.Clear();
		foreach (ItemID value in Enum.GetValues(typeof(ItemID)))
		{
			if (value != 0)
			{
				Item item = CostumeItemMaster.GetItem(value);
				string text = item.Name;
				string shortName = MasterGameDataManager.Character.Properties(item.CharaType).Profile.ShortName;
				m_charaCheck.options.Add(new Dropdown.OptionData
				{
					text = shortName + "/" + text
				});
			}
		}
		m_charaCheck.RefreshShownValue();
		m_charaCheck.onValueChanged.AsObservable().Subscribe(async delegate(int id)
		{
			await CharaChange(id, isStory: true, isMahjongPlay: false);
		}).AddTo(this);
		m_charaCheckMahjongPlay.options.Clear();
		foreach (ItemID value2 in Enum.GetValues(typeof(ItemID)))
		{
			if (value2 != 0)
			{
				Item item2 = CostumeItemMaster.GetItem(value2);
				string text2 = item2.Name;
				string shortName2 = MasterGameDataManager.Character.Properties(item2.CharaType).Profile.ShortName;
				m_charaCheckMahjongPlay.options.Add(new Dropdown.OptionData
				{
					text = shortName2 + "/" + text2
				});
			}
		}
		m_charaCheckMahjongPlay.RefreshShownValue();
		m_charaCheckMahjongPlay.onValueChanged.AsObservable().Subscribe(async delegate(int id)
		{
			await CharaChange(id, isStory: false, isMahjongPlay: true);
		}).AddTo(this);
		m_motionPlay.OnClickAsObservable().Subscribe(delegate
		{
			PlayMotion();
		}).AddTo(this);
		m_recStart.OnClickAsObservable().Subscribe(async delegate
		{
			await StartRecording();
		}).AddTo(this);
		m_snapShot.OnClickAsObservable().Subscribe(async delegate
		{
			await SnapShot();
		}).AddTo(this);
		m_motionDrop.options.Clear();
		foreach (string motion in MotionList)
		{
			m_motionDrop.options.Add(new Dropdown.OptionData
			{
				text = (motion ?? "")
			});
		}
		m_motionDrop.RefreshShownValue();
		m_motionDrop.onValueChanged.AsObservable().Subscribe(delegate(int id)
		{
			motionIndex = id;
			m_motionTrigger = MotionList[id];
		}).AddTo(this);
		motionIndex = ((MotionList.IndexOf(m_motionTrigger) >= 0) ? MotionList.IndexOf(m_motionTrigger) : 0);
	}

	protected override void Update()
	{
		base.Update();
		if (Bi2xInput.GetKeyDown(KeyCode.DownArrow))
		{
			motionIndex = (motionIndex + 1) % MotionList.Count();
			m_motionTrigger = MotionList[motionIndex];
			m_motionDrop.transform.Find("Label").GetComponent<Text>().text = m_motionTrigger ?? "";
			PlayMotion();
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.UpArrow))
		{
			motionIndex = (motionIndex + MotionList.Count() - 1) % MotionList.Count();
			m_motionTrigger = MotionList[motionIndex];
			m_motionDrop.transform.Find("Label").GetComponent<Text>().text = m_motionTrigger ?? "";
			PlayMotion();
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.LeftArrow))
		{
			if (!IsCharaChanging)
			{
				int id = (int)(m_costumeID - 1 + Enum.GetValues(typeof(ItemID)).Length - 2) % (Enum.GetValues(typeof(ItemID)).Length - 1);
				CharaChange(id, isStory: true, isMahjongPlay: true).Forget();
				Item item = CostumeItemMaster.GetItem(m_costumeID);
				string text = item.Name;
				string shortName = MasterGameDataManager.Character.Properties(item.CharaType).Profile.ShortName;
				m_charaCheck.transform.Find("Label").GetComponent<Text>().text = shortName + "/" + text;
				m_charaCheckMahjongPlay.transform.Find("Label").GetComponent<Text>().text = shortName + "/" + text;
			}
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.RightArrow) && !IsCharaChanging)
		{
			int id2 = (int)m_costumeID % (Enum.GetValues(typeof(ItemID)).Length - 1);
			CharaChange(id2, isStory: true, isMahjongPlay: true).Forget();
			Item item2 = CostumeItemMaster.GetItem(m_costumeID);
			string text2 = item2.Name;
			string shortName2 = MasterGameDataManager.Character.Properties(item2.CharaType).Profile.ShortName;
			m_charaCheck.transform.Find("Label").GetComponent<Text>().text = shortName2 + "/" + text2;
			m_charaCheckMahjongPlay.transform.Find("Label").GetComponent<Text>().text = shortName2 + "/" + text2;
		}
		if (Bi2xInput.GetKeyDown(KeyCode.LeftShift))
		{
			charaAngle = (charaAngle + 270) % 360;
			CharaRotate();
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.RightShift))
		{
			charaAngle = (charaAngle + 90) % 360;
			CharaRotate();
		}
	}

	private void CharaRotate()
	{
		m_charaManager.transform.localEulerAngles = new Vector3(0f, charaAngle, 0f);
		m_mahjongPlayCharaParent.transform.localEulerAngles = new Vector3(0f, charaAngle + 180, 0f);
	}

	public async UniTask CharaChange(int id, bool isStory, bool isMahjongPlay)
	{
		IsCharaChanging = true;
		if (isStory)
		{
			if (_standingCharaController != null)
			{
				m_charaManager.UnLoad(m_costumeID);
			}
			m_costumeID = (ItemID)(id + 1);
			_standingCharaController = await m_charaManager.Load(m_costumeID);
		}
		if (isMahjongPlay)
		{
			m_costumeMahjongPlayID = (ItemID)(id + 1);
			await MahjongPlayCharaInit(m_costumeMahjongPlayID);
		}
		IsCharaChanging = false;
	}

	public void InputMotion()
	{
	}

	public void InputRecTime()
	{
		int.TryParse(m_recTime.text, out m_recordingTime);
	}

	private void OptionSetActive(bool isActive)
	{
		m_charaCheck.gameObject.SetActive(isActive);
		m_charaCheckMahjongPlay.gameObject.SetActive(isActive);
		m_motionDrop.gameObject.SetActive(isActive);
		m_motionPlay.gameObject.SetActive(isActive);
		m_recStart.gameObject.SetActive(isActive);
		m_recTimeParent.SetActive(isActive);
		m_reference.SetActive(isActive);
		m_snapShot.gameObject.SetActive(isActive);
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		if (m_costumeID != 0)
		{
			_standingCharaController = await m_charaManager.Load(m_costumeID);
			if (m_costumeMahjongPlayID != 0)
			{
				await MahjongPlayCharaInit(m_costumeMahjongPlayID);
			}
			await StartRecording();
		}
	}

	public async UniTask MahjongPlayCharaInit(ItemID costume)
	{
		if (m_mahjongPlayCharaObject != null)
		{
			UnityEngine.Object.Destroy(m_mahjongPlayCharaObject);
		}
		RuntimeAnimatorController motionController = await ModelManager.MotionLoad(costume, isMahjong: true);
		m_mahjongPlayCharaObject = UnityEngine.Object.Instantiate((await AssetLoader.LoadAssetAsync<GameObject>($"Standing3D/{costume}_Model.prefab")).Asset, m_mahjongPlayCharaParent.transform, worldPositionStays: false);
		m_mahjongPlayCharaObject.transform.GetComponent<ModelManager>().Init(costume, motionController, isMahjong: true);
		m_mahjongPlayCharaObject.SetLayerRecursive("Chara3DMyChara");
		Vector3 euler = MasterGameDataManager.Character.CommonProperties().MahjongPlayLightingVector + MasterGameDataManager.Character.Properties(CostumeItemMaster.GetItem(costume).CharaType).MahjongPlayLightingOffsetParam;
		m_mahjongPlayCharaParent.transform.Find("Directional Light").localRotation = Quaternion.Euler(euler);
	}

	public void PlayMotion()
	{
		if (_standingCharaController != null && IsStoryMotion)
		{
			if (m_motionTrigger.Contains("Touch"))
			{
				StandingCharaController standingCharaController = _standingCharaController;
				string motionTrigger = m_motionTrigger;
				standingCharaController.PlayTouchMotion(int.Parse(motionTrigger.Substring(5, motionTrigger.Length - 5)));
			}
			else
			{
				_standingCharaController.PlayMotion(m_motionTrigger);
			}
		}
		if (m_mahjongPlayCharaObject != null && IsMahjongPlayMotion)
		{
			Animator componentInChildren = m_mahjongPlayCharaObject.GetComponentInChildren<Animator>();
			if (m_motionTrigger.Equals("EyeContact"))
			{
				componentInChildren.SetInteger("LookDirection", 2);
			}
			componentInChildren.SetTrigger(m_motionTrigger);
		}
	}

	public async UniTask StartRecording()
	{
	}

	public async UniTask SnapShot()
	{
	}
}

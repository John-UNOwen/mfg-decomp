using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.CutinItem;
using MFG.Types.Customize.Voice;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GameDataTest : MonoBehaviour
{
	[SerializeField]
	private GameObject parent;

	[SerializeField]
	private Button charaExpButton;

	[SerializeField]
	private GameObject charaExpTestPrefab;

	private GameObject charaExpTest;

	[SerializeField]
	private Button charaFansButton;

	[SerializeField]
	private GameObject charaFansTestPrefab;

	private GameObject charaFansTest;

	[SerializeField]
	private Button dojoUnlockButton;

	[SerializeField]
	private GameObject dojoUnlockTestPrefab;

	private GameObject dojoUnlockTest;

	[SerializeField]
	private Button fightSpiritButton;

	[SerializeField]
	private GameObject fightSpiritTestPrefab;

	private GameObject fightSpiritTest;

	[SerializeField]
	private Button fightStarButton;

	[SerializeField]
	private GameObject fightStarTestPrefab;

	private GameObject fightStarTest;

	[SerializeField]
	private Button gachaGameDataButton;

	[SerializeField]
	private GameObject gachaGameDataTestPrefab;

	private GameObject gachaGameDataTest;

	[SerializeField]
	private Button playerConfigButton;

	[SerializeField]
	private GameObject playerConfigTestPrefab;

	private GameObject playerConfigTest;

	[SerializeField]
	private Button bingoEventButton;

	[SerializeField]
	private GameObject bingoEventTestPrefab;

	private GameObject bingoEventTest;

	[SerializeField]
	private Button storyButton;

	[SerializeField]
	private GameObject storyTestPrefab;

	private GameObject storyTest;

	[SerializeField]
	private Button catchphraseButton;

	[SerializeField]
	private GameObject catchphraseTestPrefab;

	private GameObject catchphraseTest;

	[SerializeField]
	private Button paidItemButton;

	[SerializeField]
	private GameObject paidItemTestPrefab;

	private GameObject paidItemTest;

	[SerializeField]
	private List<Button> _customizeItemButtons = new List<Button>();

	[SerializeField]
	private GameObject customizeItemTestPrefab;

	private GameObject customizeItemTest;

	[SerializeField]
	private GameObject cutinItemButtonPrefab;

	[SerializeField]
	private GameObject cutinItemTestPrefab;

	[SerializeField]
	private GameObject cutinItemButtonBase;

	[SerializeField]
	private GameObject cutinItemTest;

	[SerializeField]
	private GameObject voiceItemButtonPrefab;

	[SerializeField]
	private GameObject voiceItemTestPrefab;

	[SerializeField]
	private GameObject voiceItemButtonBase;

	[SerializeField]
	private GameObject voiceItemTest;

	[SerializeField]
	private Button resetButton;

	[SerializeField]
	private GameObject resetTestPrefab;

	private GameObject resetTest;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private GameObject text1;

	[SerializeField]
	private GameObject text2;

	[SerializeField]
	private GameObject buttons;

	[SerializeField]
	private Button saveButton;

	[SerializeField]
	private Button _unlockAllCutinButton;

	[SerializeField]
	private Button _unlockAllVoiceButton;

	[SerializeField]
	private Button _unlockAllCostumeButton;

	[SerializeField]
	private Button _unlockAllCustomizeButton;

	[SerializeField]
	private Button _unlockAllCatchphraseButton;

	private GameObject recentMenu;

	[SerializeField]
	private AccountGameData m_accountGameData => GameDataManager.Account;

	private async void Start()
	{
		charaExpButton.onClick.AddListener(delegate
		{
			EnableCharaExp();
		});
		charaFansButton.onClick.AddListener(delegate
		{
			EnableCharaFans();
		});
		dojoUnlockButton.onClick.AddListener(delegate
		{
			EnableDojoUnlock();
		});
		fightSpiritButton.onClick.AddListener(delegate
		{
			EnableFightSpirit();
		});
		fightStarButton.onClick.AddListener(delegate
		{
			EnableFightStar();
		});
		gachaGameDataButton.onClick.AddListener(delegate
		{
			EnableGachaGameData();
		});
		playerConfigButton.onClick.AddListener(delegate
		{
			EnablePlayerConfig();
		});
		bingoEventButton.onClick.AddListener(delegate
		{
			EnableBingoEvent();
		});
		storyButton.onClick.AddListener(delegate
		{
			EnableStoryTest();
		});
		catchphraseButton.onClick.AddListener(delegate
		{
			EnableCatchphraseTest();
		});
		paidItemButton.onClick.AddListener(delegate
		{
			EnablePaidItemTest();
		});
		foreach (var item in _customizeItemButtons.Select((Button value, int index) => (value: value, index: index)))
		{
			var (button, index2) = item;
			button.onClick.AddListener(delegate
			{
				EnableCustomizeItem((MFG.Types.Customize.CustomizeItem.Category)index2);
			});
		}
		foreach (CharaType charaType in Enum.GetValues(typeof(CharaType)))
		{
			GameObject obj = UnityEngine.Object.Instantiate(cutinItemButtonPrefab, cutinItemButtonBase.transform);
			obj.GetComponentInChildren<TextMeshProUGUI>().text = MasterGameDataManager.Character.Properties(charaType).Profile.ShortName;
			obj.GetComponent<Button>().onClick.AddListener(delegate
			{
				EnableCutinItem(charaType);
			});
			GameObject obj2 = UnityEngine.Object.Instantiate(voiceItemButtonPrefab, voiceItemButtonBase.transform);
			obj2.GetComponentInChildren<TextMeshProUGUI>().text = MasterGameDataManager.Character.Properties(charaType).Profile.ShortName;
			obj2.GetComponent<Button>().onClick.AddListener(delegate
			{
				EnableVoiceItem(charaType);
			});
		}
		resetButton.onClick.AddListener(delegate
		{
			EnableReset();
		});
		saveButton.onClick.AddListener(async delegate
		{
			await Save();
		});
		exitButton.onClick.AddListener(delegate
		{
			DestroyRecentMenu();
		});
		_unlockAllCutinButton.OnClickAsObservableSafety().Subscribe(delegate
		{
			UnlockAllCutinItem();
		}).AddTo(this);
		_unlockAllVoiceButton.OnClickAsObservableSafety().Subscribe(delegate
		{
			UnlockAllVoiceItem();
		}).AddTo(this);
		_unlockAllCostumeButton.OnClickAsObservableSafety().Subscribe(delegate
		{
			UnlockAllCostumeItem();
		}).AddTo(this);
		_unlockAllCustomizeButton.OnClickAsObservableSafety().Subscribe(delegate
		{
			UnlockAllCustomizeItem();
		}).AddTo(this);
		_unlockAllCatchphraseButton.OnClickAsObservableSafety().Subscribe(delegate
		{
			UnlockAllCatchphraseItem();
		}).AddTo(this);
	}

	public async UniTask Play()
	{
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		parent.SetActive(value: true);
	}

	private void EnableCharaExp()
	{
		charaExpTest = UnityEngine.Object.Instantiate(charaExpTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = charaExpTest;
	}

	private void EnableCharaFans()
	{
		charaFansTest = UnityEngine.Object.Instantiate(charaFansTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = charaFansTest;
	}

	private void EnableDojoUnlock()
	{
		dojoUnlockTest = UnityEngine.Object.Instantiate(dojoUnlockTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = dojoUnlockTest;
	}

	private void EnableFightSpirit()
	{
		fightSpiritTest = UnityEngine.Object.Instantiate(fightSpiritTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = fightSpiritTest;
	}

	private void EnableGachaGameData()
	{
		gachaGameDataTest = UnityEngine.Object.Instantiate(gachaGameDataTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = gachaGameDataTest;
	}

	private void EnableFightStar()
	{
		fightStarTest = UnityEngine.Object.Instantiate(fightStarTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = fightStarTest;
	}

	private void EnablePlayerConfig()
	{
		playerConfigTest = UnityEngine.Object.Instantiate(playerConfigTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = playerConfigTest;
	}

	private void EnableBingoEvent()
	{
		bingoEventTest = UnityEngine.Object.Instantiate(bingoEventTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = bingoEventTest;
	}

	private void EnableStoryTest()
	{
		storyTest = UnityEngine.Object.Instantiate(storyTestPrefab, base.transform);
		storyTest.GetComponent<StoryTest>().CreateDisp();
		SetActiveButtons(active: false);
		recentMenu = storyTest;
	}

	private void EnableCatchphraseTest()
	{
		catchphraseTest = UnityEngine.Object.Instantiate(catchphraseTestPrefab, base.transform);
		catchphraseTest.GetComponent<CatchphraseTest>().CreateDisp();
		SetActiveButtons(active: false);
		recentMenu = catchphraseTest;
	}

	private void EnablePaidItemTest()
	{
		paidItemTest = UnityEngine.Object.Instantiate(paidItemTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = paidItemTest;
	}

	private void EnableCustomizeItem(MFG.Types.Customize.CustomizeItem.Category category)
	{
		customizeItemTest = UnityEngine.Object.Instantiate(customizeItemTestPrefab, base.transform);
		CustomizeItemTest component = customizeItemTest.GetComponent<CustomizeItemTest>();
		component.SetCategory(category);
		component.CreateDisp();
		SetActiveButtons(active: false);
		recentMenu = customizeItemTest;
	}

	private void EnableCutinItem(CharaType charaType)
	{
		cutinItemTest = UnityEngine.Object.Instantiate(cutinItemTestPrefab, base.transform);
		CutinItemTest component = cutinItemTest.GetComponent<CutinItemTest>();
		component.SetChara(charaType);
		component.CreateDisp();
		SetActiveButtons(active: false);
		recentMenu = cutinItemTest;
	}

	private void EnableVoiceItem(CharaType charaType)
	{
		voiceItemTest = UnityEngine.Object.Instantiate(voiceItemTestPrefab, base.transform);
		GachaVoiceItemTest component = voiceItemTest.GetComponent<GachaVoiceItemTest>();
		component.SetChara(charaType);
		component.CreateDisp();
		SetActiveButtons(active: false);
		recentMenu = voiceItemTest;
	}

	private void EnableReset()
	{
		resetTest = UnityEngine.Object.Instantiate(resetTestPrefab, base.transform);
		SetActiveButtons(active: false);
		recentMenu = resetTest;
	}

	private void SetActiveButtons(bool active)
	{
		buttons.SetActive(active);
	}

	private void DestroyRecentMenu()
	{
		if (recentMenu != null)
		{
			UnityEngine.Object.Destroy(recentMenu);
			SetActiveButtons(active: true);
		}
		else
		{
			ReturnHome();
		}
	}

	private async void ReturnHome()
	{
		try
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
			parent.SetActive(value: false);
			ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	private async UniTask Save()
	{
		await GameDataManager.Version.Save();
		await GameDataManager.Player.Save();
		await GameDataManager.PlayerItem.Save();
		await GameDataManager.PlayerPaidItem.Save();
		await GameDataManager.Customize.Save();
		await GameDataManager.Story.Save();
		await GameDataManager.PlayerConfig.Save();
		await GameDataManager.Mission.Save();
		await GameDataManager.Gacha.Save();
		await GameDataManager.Vote.Save();
		await GameDataManager.ScrambleFestival.Save();
	}

	private void UnlockAllCutinItem()
	{
		GameDataManager.Customize.CutinItems = (from x in Enum.GetValues(typeof(MFG.Types.Customize.CutinItem.ItemID)).OfType<MFG.Types.Customize.CutinItem.ItemID>()
			where x != MFG.Types.Customize.CutinItem.ItemID.None
			select new CutinItemInfo
			{
				ItemID = x
			}).ToList();
		Debug.Log(GetType().Name + ": カットインを全解禁しました。");
	}

	private void UnlockAllVoiceItem()
	{
		foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
		{
			GameDataManager.Player.CharaData[(int)value].GachaVoiceItems = (from x in VoiceMaster.GetItems((MFG.Types.Customize.Voice.Item x) => x.ObtainType == ObtainType.Gacha)
				select new VoiceInfo
				{
					ItemID = x.ItemID
				}).ToList();
		}
		Debug.Log(GetType().Name + ": ガチャ解禁ボイスを全解禁しました。");
	}

	private void UnlockAllCostumeItem()
	{
		GameDataManager.Customize.CostumeItems = (from x in Enum.GetValues(typeof(MFG.Types.Customize.Costume.ItemID)).OfType<MFG.Types.Customize.Costume.ItemID>()
			where x != 0 && x != MFG.Types.Customize.Costume.ItemID.Costume12_003
			select new CostumeItemInfo
			{
				ItemID = x
			}).ToList();
		Debug.Log(GetType().Name + ": コスチュームを全解禁しました。");
	}

	private void UnlockAllCustomizeItem()
	{
		GameDataManager.Customize.CustomizeItems = (from x in Enum.GetValues(typeof(MFG.Types.Customize.CustomizeItem.ItemID)).OfType<MFG.Types.Customize.CustomizeItem.ItemID>()
			where x != MFG.Types.Customize.CustomizeItem.ItemID.None
			select new CustomizeItemInfo
			{
				ItemID = x
			}).ToList();
		Debug.Log(GetType().Name + ": カスタマイズを全解禁しました。");
	}

	private void UnlockAllCatchphraseItem()
	{
		GameDataManager.Customize.CatchphraseItems = (from x in Enum.GetValues(typeof(MFG.Types.Customize.Catchphrase.ItemID)).OfType<MFG.Types.Customize.Catchphrase.ItemID>()
			where x != MFG.Types.Customize.Catchphrase.ItemID.None
			select new CatchphraseItemInfo
			{
				ItemID = x
			}).ToList();
		Debug.Log(GetType().Name + ": キャッチフレーズを全解禁しました。");
	}
}

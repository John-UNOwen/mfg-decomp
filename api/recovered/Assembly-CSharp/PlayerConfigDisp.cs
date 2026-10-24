using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerConfigDisp : MonoBehaviour
{
	[SerializeField]
	private List<Button> configButtons;

	private void Start()
	{
	}

	private void Update()
	{
		SetConfigText(configButtons[0], "名前入力を完了したかどうか：", GameDataManager.PlayerConfig.IsNameDecided);
		SetButtonColor(configButtons[0], GameDataManager.PlayerConfig.IsNameDecided);
		SetConfigText(configButtons[1], "対局サポート機能が有効かどうか：", GameDataManager.PlayerConfig.IsTaikyokuSupport);
		SetButtonColor(configButtons[1], GameDataManager.PlayerConfig.IsTaikyokuSupport);
		SetConfigText(configButtons[2], "対局画面のViewType：", GameDataManager.PlayerConfig.ViewType);
		SetConfigText(configButtons[3], "オプション：", GameDataManager.PlayerConfig.IsDispHelpOption);
		SetButtonColor(configButtons[3], GameDataManager.PlayerConfig.IsDispHelpOption);
		SetConfigText(configButtons[4], "シャンテン：", GameDataManager.PlayerConfig.IsDispHelpSyanten);
		SetButtonColor(configButtons[4], GameDataManager.PlayerConfig.IsDispHelpSyanten);
		SetConfigText(configButtons[5], "アンパイ：", GameDataManager.PlayerConfig.IsDispHelpAnpai);
		SetButtonColor(configButtons[5], GameDataManager.PlayerConfig.IsDispHelpAnpai);
		SetConfigText(configButtons[6], "ナビ：", GameDataManager.PlayerConfig.IsDispHelpNavi);
		SetButtonColor(configButtons[6], GameDataManager.PlayerConfig.IsDispHelpNavi);
		SetConfigText(configButtons[7], "ガチャ後のホームシーン：", GameDataManager.PlayerConfig.IsDispHelpAfterGachaAtHomeScene);
		SetButtonColor(configButtons[7], GameDataManager.PlayerConfig.IsDispHelpAfterGachaAtHomeScene);
		SetConfigText(configButtons[8], "初回ホームシーン遷移時：", GameDataManager.PlayerConfig.IsDispHelpFirstPlayAtHomeScene);
		SetButtonColor(configButtons[8], GameDataManager.PlayerConfig.IsDispHelpFirstPlayAtHomeScene);
		SetConfigText(configButtons[9], "初回マイルーム遷移時：", GameDataManager.PlayerConfig.IsDispHelpFirstPlayMyRoomScene);
		SetButtonColor(configButtons[9], GameDataManager.PlayerConfig.IsDispHelpFirstPlayMyRoomScene);
		SetConfigText(configButtons[10], "初回リザルトシーン遷移時：", GameDataManager.PlayerConfig.IsDispHelpFirstPlayResultScene);
		SetButtonColor(configButtons[10], GameDataManager.PlayerConfig.IsDispHelpFirstPlayResultScene);
		SetConfigText(configButtons[11], "初回リザルトファイトスター遷移時：", GameDataManager.PlayerConfig.IsDispHelpFirstPlayResultFightStar);
		SetButtonColor(configButtons[11], GameDataManager.PlayerConfig.IsDispHelpFirstPlayResultFightStar);
		SetConfigText(configButtons[12], "初回レート選択ウィンドウ表示：", GameDataManager.PlayerConfig.IsDispHelpFirstRateWindowDisp);
		SetButtonColor(configButtons[12], GameDataManager.PlayerConfig.IsDispHelpFirstRateWindowDisp);
		SetConfigText(configButtons[13], "ファン数上限説明：", GameDataManager.PlayerConfig.IsDispHelpResultFanLimit);
		SetButtonColor(configButtons[13], GameDataManager.PlayerConfig.IsDispHelpResultFanLimit);
		SetConfigText(configButtons[14], "ヘッドフォンボリューム：", GameDataManager.PlayerConfig.headPhoneVolume);
	}

	private void SetButtonColor(Button button, bool condition)
	{
		button.GetComponent<Image>().color = (condition ? new Color(1f, 1f, 1f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f));
	}

	private void SetConfigText<T>(Button button, string text, T config)
	{
		TextMeshProUGUI component = button.transform.Find("Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>();
		T val = config;
		component.text = text + val;
	}

	private void ChangeConfig(ref bool config)
	{
		config = !config;
	}
}

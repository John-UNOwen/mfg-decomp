using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ResetTest : MonoBehaviour
{
	[SerializeField]
	private Button yesToHomeButton;

	[SerializeField]
	private Button yesToTitleButton;

	private void Start()
	{
		yesToHomeButton.onClick.AddListener(async delegate
		{
			await ResetToHome();
		});
		yesToTitleButton.onClick.AddListener(async delegate
		{
			await ResetToTitle();
		});
	}

	private void Update()
	{
	}

	private async UniTask ResetToHome()
	{
		await Reset();
		GoToHome();
	}

	private async UniTask ResetToTitle()
	{
		await Reset();
		GoToTitle();
	}

	private async UniTask Reset()
	{
		yesToHomeButton.gameObject.SetActive(value: false);
		yesToTitleButton.gameObject.SetActive(value: false);
		GameDataManager.ResetForDebugMenu();
		await GameDataManager.Version.Save();
		await GameDataManager.Player.Save();
		await GameDataManager.PlayerItem.Save();
		await GameDataManager.PlayerPaidItem.Save();
		await GameDataManager.Customize.Save();
		await GameDataManager.Story.Save();
		await GameDataManager.PlayerConfig.Save();
		await GameDataManager.Mission.Save();
		await GameDataManager.Gacha.Save();
		await GameDataManager.EventTaku.Save();
		await GameDataManager.Vote.Save();
		await GameDataManager.ScrambleFestival.Save();
		await GameDataManager.BingoEventRemake.Save();
		await GameDataManager.OthersEvent.Save();
	}

	private void GoToHome()
	{
		Object.Destroy(this);
		ResidentFlow.GetInstance().LoadScene("Home");
	}

	private void GoToTitle()
	{
		Object.Destroy(this);
		ResidentFlow.GetInstance().LoadScene("Title");
	}
}

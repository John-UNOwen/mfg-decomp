using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Mission;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MusicEventProgress : MonoBehaviour
{
	[SerializeField]
	private Image Chara;

	[SerializeField]
	private Image Jacket;

	[SerializeField]
	private TextMeshProUGUI Counter;

	[SerializeField]
	private Image MusicName;

	[SerializeField]
	private Image CharaName;

	[SerializeField]
	private GameObject ClearStamp;

	public async UniTask Set(Item master)
	{
		CharaType chara = master.Chara;
		MissionProgressInfo mission = GameDataManager.Mission.m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == master.MissionID);
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>($"SdChara/{chara}/Textures/Tsumo.png");
		disposableAsset.AddTo(this);
		Chara.sprite = disposableAsset.Asset;
		disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(string.Format("MissionEvent/{0}/Jacket/JacketMini{1}.png", master.Category, ((int)(chara + 1)).ToString("D2")));
		disposableAsset.AddTo(this);
		Jacket.sprite = disposableAsset.Asset;
		disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(string.Format("MissionEvent/{0}/BgmName{1}.png", master.Category, ((int)(chara + 1)).ToString("D2")));
		disposableAsset.AddTo(this);
		MusicName.sprite = disposableAsset.Asset;
		disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>("MissionEvent/Common/Textures/CharaName" + ((int)(chara + 1)).ToString("D2") + ".png");
		disposableAsset.AddTo(this);
		CharaName.sprite = disposableAsset.Asset;
		Counter.text = $"{mission.Progress}/{master.AchievementList().Last()}";
		ClearStamp.SetActive(mission.IsAllClear);
	}
}

using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Taikyoku;
using MFG.Types;
using MFG.Types.Competition;
using MFG.Types.Customize.MatchingBackground;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MatchingCharaMovie : MonoBehaviour
{
	[SerializeField]
	private Transform m_matchingBGParent;

	[SerializeField]
	private Image m_graphicUp;

	[SerializeField]
	private Image m_graphicDown;

	[SerializeField]
	private Image m_graphicUpMultPly;

	[SerializeField]
	private TextMeshProUGUI m_playerName;

	[SerializeField]
	private CatchphrasePlate m_catchphrasePlate;

	[SerializeField]
	private Image m_trophy;

	[SerializeField]
	private Transform m_dan;

	public async UniTask<(GameObject, Sprite)> MatchingCutinSetup(PlayerInfo info, CharaType charaType, DanBanner danBanner, ItemID debugBG = ItemID.None)
	{
		ItemID matchingBGEquip = info.PlayerGameData.GetCurrentChara().MatchingBGEquip;
		IDisposableAsset<GameObject> asset = await AssetLoader.LoadAssetAsync<GameObject>(MatchingBackgroundMaster.GetItem((matchingBGEquip != 0) ? matchingBGEquip : MatchingBackgroundMaster.GetDefaultItemIDFirst(charaType)).BGPath);
		asset.AddTo(this);
		CharacterProperties charaProperty = MasterGameDataManager.Character.Properties(charaType);
		m_graphicUp.color = charaProperty.MatchingColor1;
		m_graphicDown.color = charaProperty.MatchingColor2;
		m_graphicUpMultPly.color = charaProperty.MatchingColor2;
		m_playerName.text = info.GetPlayerName();
		m_catchphrasePlate.Reflesh(info.PlayerGameData.GetCurrentChara().CatchphraseItemEquip).Forget();
		CompetitionID recentFinishedCompetitionID = info.PlayerGameData.RecentFinishedCompetitionID;
		TrophyType recentFinishedCompetitionTrophyType = info.PlayerGameData.RecentFinishedCompetitionTrophyType;
		Sprite sprite = null;
		if (recentFinishedCompetitionID != 0 && recentFinishedCompetitionTrophyType != 0)
		{
			IDisposableAsset<Sprite> obj = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveCompetitionTrophyIconMiniAssets(recentFinishedCompetitionID, recentFinishedCompetitionTrophyType));
			obj.AddTo(this);
			sprite = obj.Asset;
			m_trophy.gameObject.SetActive(value: true);
			m_trophy.sprite = sprite;
		}
		else
		{
			m_trophy.gameObject.SetActive(value: false);
		}
		if (info.PlayerGameData.IsPro())
		{
			danBanner.SetDanBannerPro(m_dan, charaProperty.ProProfile.proDanLevelType);
		}
		else
		{
			danBanner.SetDanBanner(m_dan, info.DanLevel);
		}
		return (Object.Instantiate(asset.Asset, m_matchingBGParent), sprite);
	}
}

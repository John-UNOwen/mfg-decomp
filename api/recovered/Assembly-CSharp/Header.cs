using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using MFG.Types.Competition;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Header : MonoBehaviour
{
	[SerializeField]
	private GameObject m_header;

	[SerializeField]
	private TextMeshProUGUI m_sceneName;

	[SerializeField]
	private TextMeshProUGUI m_playerName;

	[SerializeField]
	private GameObject m_trophy;

	[SerializeField]
	private Transform m_danBannerParent;

	[SerializeField]
	private DanBanner m_danBanner;

	[SerializeField]
	private GameObject m_yellRank;

	[SerializeField]
	private Sprite m_sprDirector;

	[SerializeField]
	private GameObject m_jandriUsing;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetDisp(bool disp)
	{
		m_header.SetActive(disp);
		SetStatusDisp(disp);
		if (disp)
		{
			SetPlayerName(GameDataManager.Player.PlayerName);
			SetDanLevel(GameDataManager.Player.DanLevel);
			SetTrophy(GameDataManager.Player.RecentFinishedCompetitionID, GameDataManager.Player.RecentFinishedCompetitionTrophyType).Forget();
			SetJandri(GameDataManager.CoopData.IsJanDriEnable());
		}
	}

	public void SetStatusDisp(bool disp)
	{
		m_header.transform.Find("Common").Find("Status").gameObject.SetActive(disp);
	}

	public void SetSceneName(string sceneName)
	{
		m_sceneName.text = sceneName;
	}

	public void SetPlayerName(string name)
	{
		m_playerName.text = name;
	}

	public void SetDanLevel(DanLevelType danLevel)
	{
		m_danBanner.SetDanBanner(m_danBannerParent, danLevel);
	}

	public void SetJandri(bool isUsing)
	{
		m_jandriUsing.SetActive(isUsing);
	}

	private async UniTask SetTrophy(CompetitionID compeID, TrophyType trophyType)
	{
		if (compeID != 0 && trophyType != 0)
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveCompetitionTrophyIconMiniAssets(compeID, trophyType));
			disposableAsset.AddTo(this);
			m_trophy.GetComponent<Image>().sprite = disposableAsset.Asset;
			m_trophy.SetActive(value: true);
		}
		else
		{
			m_trophy.SetActive(value: false);
		}
	}
}

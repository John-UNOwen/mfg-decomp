using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Competition;
using MFG.Types.Customize.Catchphrase;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfilePanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _playerNameText;

	[SerializeField]
	private GameObject _danBannerParent;

	[SerializeField]
	private GameObject _cpuIconParent;

	[SerializeField]
	private GameObject _odekakeIconParent;

	[SerializeField]
	private GameObject _trophyParent;

	[SerializeField]
	private Image _trophyImage;

	[SerializeField]
	private GameObject _yellRankParent;

	[SerializeField]
	private Image _yellRankImage;

	[SerializeField]
	private Animator _yellRankAnimator;

	[SerializeField]
	private GameObject _catchphraseParent;

	[SerializeField]
	private CatchphrasePlate _catchphrasePlate;

	public async UniTask Reflesh(Player player, DanBanner danBanner, CancellationToken cancellationToken)
	{
		_playerNameText.text = player.m_name;
		if (player.m_isPro)
		{
			ProProfile proProfile = MasterGameDataManager.Character.Properties(player.m_charaType).ProProfile;
			danBanner.SetDanBannerPro(_danBannerParent.transform, proProfile.proDanLevelType);
		}
		else
		{
			danBanner.SetDanBanner(_danBannerParent.transform, player.m_danLevel);
		}
		_cpuIconParent.SetActive(!player.m_isPro && player.m_isCPU);
		_odekakeIconParent.SetActive(player.m_isOdekake);
		if (player.m_compeID != 0 && player.m_trophyType != 0)
		{
			_trophyParent.SetActive(value: true);
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveCompetitionTrophyIconMiniAssets(player.m_compeID, player.m_trophyType), null, cancellationToken);
			disposableAsset.AddTo(this);
			_trophyImage.sprite = disposableAsset.Asset;
		}
		else
		{
			_trophyParent.SetActive(value: false);
		}
		if (GameDataManager.YellCampaign.IsEnable() && !GameDataManager.Mahjong.IsHaifuPlay && GameDataManager.YellCampaign.IsCharacterEnable(player.m_charaType))
		{
			_yellRankParent.SetActive(value: true);
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveYellRankImage(player.m_yellRank, isTaikyoku: true));
			disposableAsset2.AddTo(this);
			_yellRankImage.sprite = disposableAsset2.Asset;
			_yellRankAnimator.SetTrigger("Loop");
		}
		if (player.CatchphraseItemEquip != ItemID.Catchphrase_00_000)
		{
			_catchphraseParent.SetActive(value: true);
			_catchphrasePlate.GetComponent<CatchphrasePlate>().Reflesh(player.CatchphraseItemEquip, cancellationToken).Forget();
		}
		else
		{
			_catchphraseParent.SetActive(value: false);
		}
	}

	public async UniTask AllUIDisp(DanBanner danBanner, CancellationToken cancellationToken)
	{
		_playerNameText.text = "ファイトガール";
		danBanner.SetDanBanner(_danBannerParent.transform, DanLevelType.Max);
		_cpuIconParent.SetActive(value: true);
		_odekakeIconParent.SetActive(value: true);
		_trophyParent.SetActive(value: true);
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveCompetitionTrophyIconMiniAssets(CompetitionID.Competition009, TrophyType.Platinum), null, cancellationToken);
		disposableAsset.AddTo(this);
		_trophyImage.sprite = disposableAsset.Asset;
		_catchphraseParent.SetActive(value: true);
		_catchphrasePlate.GetComponent<CatchphrasePlate>().Reflesh(ItemID.Catchphrase_00_001, cancellationToken).Forget();
	}
}

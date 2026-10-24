using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.Types.AD;
using UnityEngine;
using UnityEngine.UI;

public class AdvertiseTipsManager : MonoBehaviour
{
	[SerializeField]
	private GameObject _lowerTips;

	private GameObject _upperTips;

	private Image _upperTipsImage;

	private IDisposableAsset<Sprite> _tipsSpriteAsset;

	private bool _isUpperDisplayOpen;

	public void SetUpperTips(GameObject parent, Image image)
	{
		_upperTips = parent;
		_upperTipsImage = image;
	}

	public async UniTask Open()
	{
		await Tips_Fadeout(0f);
		TipsSprite_Clear();
		await TipsSprite_Load(await TipsSprite_DecideItem());
		if (SystemConfigGameData.IsDualMonitor)
		{
			SubMonitorFlow.GetInstance().TransitionAnimClose();
		}
		Tips_SetSprite(_tipsSpriteAsset.Asset);
		Tips_SetActive(isActive: true);
		await Tips_Fadein();
	}

	public async UniTask Close()
	{
		if (ResidentFlow.GetInstance().m_isTitleDemoMahjongPlayAlive)
		{
			ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetLowerDisp(isLowerDisp: true);
		}
		await Tips_Fadeout();
		Tips_SetActive(isActive: false);
		TipsSprite_Clear();
	}

	private async UniTask<string> TipsSprite_DecideItem()
	{
		List<AdTipsID> list = new List<AdTipsID>();
		list.AddRange(GameDataManager.GachaInfo.GetGachaAdTipsIDs());
		if (GameDataManager.IsReachSongToytoyNoticeEnable() && (PaseliUtility.PPEX.IsEnablePPEX || GameUtility.IsKonastaOrNetcafeBehavior))
		{
			list.Add(AdTipsID.Ad164);
		}
		string path = AssetPathResolver.ResolveTips(list.Select((AdTipsID x) => x.ToString()).ToList());
		if (!(await AssetLoader.Exists(path)))
		{
			Debug.Log(path + "は存在しないのでデフォルト画像を表示します");
			path = AssetPathResolver.ResolveTips(new List<string>());
		}
		Debug.Log("Tips Path -> " + path);
		return path;
	}

	private void TipsSprite_Clear()
	{
		if (_tipsSpriteAsset != null)
		{
			_tipsSpriteAsset.Dispose();
			_tipsSpriteAsset = null;
		}
	}

	private void Tips_SetActive(bool isActive)
	{
		_lowerTips.SetActive(isActive);
		if (_upperTipsImage != null)
		{
			_upperTips.SetActive(isActive);
		}
	}

	private void Tips_SetSprite(Sprite sprite)
	{
		_lowerTips.GetComponent<Image>().sprite = sprite;
		if (_upperTipsImage != null)
		{
			_upperTipsImage.sprite = sprite;
		}
	}

	private async UniTask TipsSprite_Load(string path)
	{
		_tipsSpriteAsset = await AssetLoader.LoadAssetAsync<Sprite>(path);
	}

	private async UniTask Tips_Fadein(float duration = 0.5f)
	{
		await Tips_Fade(1f, duration);
	}

	private async UniTask Tips_Fadeout(float duration = 0.5f)
	{
		await Tips_Fade(0f, duration);
	}

	private async UniTask Tips_Fade(float endValue, float duration = 0.5f)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(_lowerTips.GetComponent<Image>().DOFade(endValue, duration));
		sequence.Join(_upperTipsImage.DOFade(endValue, duration));
		await sequence.Play();
	}
}

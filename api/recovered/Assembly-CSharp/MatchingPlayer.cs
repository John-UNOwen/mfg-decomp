using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MatchingPlayer : MonoBehaviour
{
	[SerializeField]
	private Animator m_maDisplayComps;

	[SerializeField]
	private Image m_maImage;

	[SerializeField]
	private Image m_maName;

	private bool _isPro;

	public async UniTask ProCompSet(CharaType charaType, bool isPro)
	{
		_isPro = isPro;
		if (_isPro)
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Pro, charaType, ProAssetType.MatchingImage));
			disposableAsset.AddTo(this);
			m_maImage.sprite = disposableAsset.Asset;
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Pro, charaType, ProAssetType.MatchingName));
			disposableAsset2.AddTo(this);
			m_maName.sprite = disposableAsset2.Asset;
		}
	}

	public void ProCompIn()
	{
		if (_isPro)
		{
			m_maDisplayComps.SetTrigger("In");
		}
	}
}

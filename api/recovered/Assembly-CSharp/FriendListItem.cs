using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FriendListItem : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI friendName;

	[SerializeField]
	private DanBanner danBanner;

	public Button decideButton;

	[SerializeField]
	private Image charaSprite;

	public async UniTask Init(FriendInfo friend)
	{
		danBanner.SetDanBanner(danBanner.transform, friend.Dan);
		friendName.text = friend.PlayerName;
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>($"SdChara/{friend.CharaType}/Textures/BaseSmall.png");
		disposableAsset.AddTo(this);
		charaSprite.sprite = disposableAsset.Asset;
	}
}

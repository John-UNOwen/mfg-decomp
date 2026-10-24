using Cysharp.Threading.Tasks;
using MFG.Assets;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class ImageDisp : Command
{
	private bool isDisp;

	private Vector2Int posOffset;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.ImageDisp;
		targetName = values[3];
		isDisp = values[5] == "on";
		if (isDisp)
		{
			posOffset = new Vector2Int(int.Parse(values[7]), int.Parse(values[9]));
		}
	}

	public override async UniTask Exec()
	{
		Transform image = StoryPlayFlow.GetInstance().imageTop.transform.Find(targetName);
		if (isDisp)
		{
			if (!image)
			{
				image = StoryImageManager.CreateImage();
				image.gameObject.name = targetName;
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveStoryImage(targetName));
				disposableAsset.AddTo(image);
				image.GetComponent<Image>().sprite = disposableAsset.Asset;
			}
			image.transform.localPosition = new Vector3(posOffset.x, posOffset.y, 0f);
		}
		image.gameObject.SetActive(isDisp);
	}
}

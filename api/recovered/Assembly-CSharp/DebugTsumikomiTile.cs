using Cysharp.Threading.Tasks;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DebugTsumikomiTile : MonoBehaviour
{
	public Pai pai;

	public Sprite GetSprite => GetComponent<Image>().sprite;

	private void Start()
	{
		GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			if (GameDataManager.Mahjong.DebugSelectingPai == null)
			{
				GameDataManager.Mahjong.DebugSelectingPai = base.gameObject;
				GetComponent<Image>().color = Color.yellow;
			}
			else
			{
				DebugTsumikomiTile component = GameDataManager.Mahjong.DebugSelectingPai.GetComponent<DebugTsumikomiTile>();
				Sprite getSprite = component.GetSprite;
				Pai pai = component.pai;
				GameDataManager.Mahjong.DebugSelectingPai.GetComponent<Image>().sprite = GetSprite;
				GameDataManager.Mahjong.DebugSelectingPai.GetComponent<DebugTsumikomiTile>().pai = this.pai;
				GetComponent<Image>().sprite = getSprite;
				this.pai = pai;
				GameDataManager.Mahjong.DebugSelectingPai.GetComponent<Image>().color = Color.white;
				GameDataManager.Mahjong.DebugSelectingPai = null;
			}
		}).AddTo(this);
	}
}

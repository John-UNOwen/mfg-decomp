using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionRuleWindow : MonoBehaviour
{
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private GameObject detailParent;

	public async UniTask<bool> Init()
	{
		CompetitionInfo inProgressCompetitionInfo = GameDataManager.Competition.GetInProgressCompetitionInfo();
		if (inProgressCompetitionInfo != null)
		{
			string ruleDetailPath = $"Competition/{inProgressCompetitionInfo.CompetitionID}/RuleDetail.prefab";
			if (await AssetLoader.Exists(ruleDetailPath))
			{
				IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(ruleDetailPath);
				obj.AddTo(this);
				GameObject gameObject = Object.Instantiate(obj.Asset, detailParent.transform, worldPositionStays: false);
				detailParent.transform.GetComponent<RectTransform>().sizeDelta = gameObject.transform.GetComponent<RectTransform>().sizeDelta;
				closeButton.OnClickAsObservable().Subscribe(delegate
				{
					SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
					base.gameObject.SetActive(value: false);
				}).AddTo(this);
				return true;
			}
		}
		return false;
	}

	public void Open()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		base.gameObject.SetActive(value: true);
	}
}

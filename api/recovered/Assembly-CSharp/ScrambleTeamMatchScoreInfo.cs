using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.GameData;
using MFG.Types;
using MFG.Types.ScrambleFestival;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrambleTeamMatchScoreInfo : MonoBehaviour
{
	public TeamType m_teamType;

	[SerializeField]
	protected Color BgColor;

	[SerializeField]
	protected GameObject _maImagePrefab;

	[SerializeField]
	protected CatchphrasePlate Catchphrase;

	[SerializeField]
	protected TextMeshProUGUI ScoreText;

	[SerializeField]
	protected GameObject AddScoreFrame;

	[SerializeField]
	protected TextMeshProUGUI AddScoreText;

	[SerializeField]
	protected TextMeshProUGUI ChoiceRate;

	[SerializeField]
	protected GameObject WinnerMark;

	[SerializeField]
	protected Transform MaParent;

	[SerializeField]
	protected Image BgImage;

	private List<GameObject> _newMAChoiceList = new List<GameObject>();

	public virtual async UniTask Reflesh(bool isNotice, bool isResult)
	{
		BgImage.color = BgColor;
		ScrambleFestivalEventInfo info = GameDataManager.ScrambleFestival.CurrentInfo;
		ScrambleFestivalScore scrambleFestivalScore = (GameDataManager.ReceiveOnly.ScrambleFestivalScoreInfos?.Find((ScrambleFestivalInfo x) => x.EventType == info.FlagType))?.ScoreInfos.Find((ScrambleFestivalScore x) => x.Team == m_teamType);
		Catchphrase.Reflesh(info.Master.CatchPhrases[m_teamType].Normal).Forget();
		WinnerMark.SetActive(value: false);
		if (scrambleFestivalScore == null)
		{
			ScoreText.text = "集計中";
			ChoiceRate.text = "--.-";
		}
		else
		{
			ScoreText.text = (isNotice ? (scrambleFestivalScore.PrevScore.ToString("N0").Replace(",", ", ") ?? "") : (scrambleFestivalScore.Score.ToString("N0").Replace(",", ", ") ?? ""));
			AddScoreText.text = "+" + (scrambleFestivalScore.Score - scrambleFestivalScore.PrevScore).ToString("N0");
			ChoiceRate.text = (isNotice ? (((float)scrambleFestivalScore.PrevChoiceRate / 10f).ToString("F1") ?? "") : (((float)scrambleFestivalScore.ChoiceRate / 10f).ToString("F1") ?? ""));
		}
		await new List<UniTask> { SetMaImage(MaParent, scrambleFestivalScore?.MaList, (!isNotice) ? null : scrambleFestivalScore?.PrevMaList) };
	}

	public virtual async UniTask ScoreNoticeFlow(bool isResult)
	{
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		ScrambleFestivalEventInfo info = GameDataManager.ScrambleFestival.CurrentInfo;
		ScrambleFestivalInfo receiveInfo = GameDataManager.ReceiveOnly.ScrambleFestivalScoreInfos?.Find((ScrambleFestivalInfo x) => x.EventType == info.FlagType);
		ScrambleFestivalScore scoreInfo = receiveInfo?.ScoreInfos.Find((ScrambleFestivalScore x) => x.Team == m_teamType);
		if (scoreInfo == null)
		{
			return;
		}
		GetComponent<Animator>().SetTrigger("ScoreIn");
		await UniTask.Delay(TimeSpan.FromSeconds(1.5));
		List<Tweener> tweenerList2 = new List<Tweener>
		{
			DOVirtual.Float(scoreInfo.PrevScore, scoreInfo.Score, 1.5f, delegate(float value)
			{
				ScoreText.text = ((int)value).ToString("N0").Replace(",", ", ");
			}).OnComplete(delegate
			{
				ScoreText.text = scoreInfo.Score.ToString("N0").Replace(",", ", ");
			}),
			DOVirtual.Float(scoreInfo.Score - scoreInfo.PrevScore, 0f, 1.5f, delegate(float value)
			{
				AddScoreText.text = "+" + ((int)value).ToString("N0");
			}).OnComplete(delegate
			{
				AddScoreText.text = "+0";
			})
		};
		if (scoreInfo.Score - scoreInfo.PrevScore > 0)
		{
			await SoundManager.PlaySe("SFX_MJG_POINT1");
		}
		await UniTask.Delay(TimeSpan.FromSeconds(1.5));
		if (tweenerList2.Count > 0)
		{
			foreach (Tweener item in tweenerList2)
			{
				item.Complete();
			}
		}
		if (scoreInfo.Score - scoreInfo.PrevScore > 0)
		{
			SoundManager.StopSe("SFX_MJG_POINT1").Forget();
			SoundManager.PlaySe("SFX_MJG_POINT2").Forget();
		}
		await UniTask.Delay(TimeSpan.FromSeconds(0.6000000238418579));
		tweenerList2 = new List<Tweener> { DOVirtual.Float((float)scoreInfo.PrevChoiceRate / 10f, (float)scoreInfo.ChoiceRate / 10f, 1f, delegate(float value)
		{
			ChoiceRate.text = value.ToString("F1");
		}).OnComplete(delegate
		{
			ChoiceRate.text = ((float)scoreInfo.ChoiceRate / 10f).ToString("F1");
		}) };
		if (Math.Abs(scoreInfo.PrevChoiceRate - scoreInfo.ChoiceRate) > 0)
		{
			await SoundManager.PlaySe("SFX_MJG_POINT1");
		}
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		if (tweenerList2.Count > 0)
		{
			foreach (Tweener item2 in tweenerList2)
			{
				item2.Complete();
			}
		}
		if (Math.Abs(scoreInfo.PrevChoiceRate - scoreInfo.ChoiceRate) > 0)
		{
			SoundManager.StopSe("SFX_MJG_POINT1").Forget();
			SoundManager.PlaySe("SFX_MJG_POINT2").Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
		}
		if ((_newMAChoiceList?.Count() ?? 0) <= 0)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(1.5));
		}
		else
		{
			float waitTime = 1.5f / (float)_newMAChoiceList.Count;
			foreach (GameObject obj in _newMAChoiceList)
			{
				obj.SetActive(value: true);
				tweenerList2 = new List<Tweener> { DOVirtual.Float(1.3f, 1f, waitTime, delegate(float value)
				{
					obj.transform.GetComponent<RectTransform>().localScale = new Vector3(value, value, value);
				}).OnComplete(delegate
				{
					obj.transform.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
				}) };
				Color color = obj.transform.GetComponent<Image>().color;
				tweenerList2.Add(DOVirtual.Float(0f, 1f, waitTime, delegate(float value)
				{
					obj.transform.GetComponent<Image>().color = new Color(color.r, color.g, color.b, value);
				}).OnComplete(delegate
				{
					obj.transform.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 1f);
				}));
				SoundManager.PlaySe("SFX_MJG_INFO4").Forget();
				await UniTask.Delay(TimeSpan.FromSeconds(waitTime));
				foreach (Tweener item3 in tweenerList2)
				{
					item3.Complete();
				}
			}
		}
		if (isResult)
		{
			bool isWinnier = receiveInfo.WinnerTeam.Contains(m_teamType);
			WinnerMark.SetActive(isWinnier);
			GetComponent<Animator>().SetTrigger("WinnerIn");
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			if (isWinnier)
			{
				SoundManager.PlaySe("SFX_SYS_BONUS3").Forget();
			}
		}
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
	}

	private async UniTask SetMaImage(Transform trs, List<MahjongActorID> maList, List<MahjongActorID> prevMaList)
	{
		foreach (Transform tr in trs)
		{
			UnityEngine.Object.Destroy(tr.gameObject);
		}
		int num = maList?.Count ?? 0;
		if (num <= 0)
		{
			return;
		}
		float x2 = trs.GetComponent<RectTransform>().sizeDelta.x;
		GridLayoutGroup component = trs.GetComponent<GridLayoutGroup>();
		float x3 = component.cellSize.x;
		component.spacing = new Vector2((x2 >= x3 * (float)num || num <= 1) ? 0f : ((x3 - (x2 - x3) / (float)(num - 1)) * -1f), 0f);
		foreach (MahjongActorID ma in maList.OrderBy(delegate(MahjongActorID x)
		{
			List<MahjongActorID> list2 = prevMaList;
			return list2 == null || !list2.Contains(x);
		}))
		{
			CharaType charaType = (CharaType)(ma - 1);
			GameObject obj = UnityEngine.Object.Instantiate(_maImagePrefab, trs);
			IDisposableAsset<Sprite> disposableAsset = (await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(MFG.Assets.PathType.Pro, charaType, ProAssetType.MatchingImage))).AddTo(this);
			obj.transform.GetComponent<Image>().sprite = disposableAsset.Asset;
			List<MahjongActorID> list = prevMaList;
			if (list == null || !list.Contains(ma))
			{
				_newMAChoiceList.Add(obj);
				obj.SetActive(value: false);
			}
		}
	}
}

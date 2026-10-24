using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Mission;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class EventProgressMap : MonoBehaviour
{
	public class ProgressInfo
	{
		public MissionID missionID;

		public int currentIndex;

		public string progressingTerm;

		public int achieveIndex;

		public bool isJandriUsed;

		public PlayCountResultWindow.ResultType resultType;

		public Item Master => MissionMaster.GetItem(missionID);

		public bool IsDisplay => currentIndex < Master.AchievementList().Last();

		public int MoveCount => achieveIndex - currentIndex;

		public void SetAchieveIndex()
		{
			achieveIndex = Math.Min(Master.AchievementList().Last(), GameDataManager.Mission.m_missionProgressInfos.Find((MissionProgressInfo x) => x.MissionID == missionID)?.Progress ?? achieveIndex);
		}
	}

	[SerializeField]
	private RectTransform roadParent;

	[SerializeField]
	private GameObject masuObj;

	[SerializeField]
	private Image charaImage;

	private Sprite charaImageSprite;

	private Sprite charaGoalSprite;

	[SerializeField]
	private RectTransform bgParent;

	[SerializeField]
	private Image bg1;

	[SerializeField]
	private TextMeshProUGUI term;

	[SerializeField]
	private Image rewardImage;

	[SerializeField]
	private TextMeshProUGUI rewardItemNum;

	[SerializeField]
	private Image getRewardImage;

	[SerializeField]
	private TextMeshProUGUI rewardCount;

	[SerializeField]
	private TextMeshProUGUI rewardUnit;

	[SerializeField]
	private TextMeshProUGUI rewardIndex;

	[SerializeField]
	private Vector3 roadParentInitPos;

	private ProgressInfo progressInfo;

	private float masuWidth;

	private int bgWidth = 1692;

	private bool isGoal;

	private bool isSkip;

	private static float moveTime = 0.3f;

	private static float moveRatio = 0.9f;

	private static float jumpTime = moveTime * moveRatio;

	private static float waitTime = moveTime * (1f - moveRatio);

	private List<(int, int, Sprite)> rewardNextImages = new List<(int, int, Sprite)>();

	private CharaType charaType => GameDataManager.Player.GetCurrentChara().charaType;

	private Animator animator => base.transform.GetComponent<Animator>();

	public float PlayTime(ProgressInfo info)
	{
		return moveTime * (float)info.MoveCount + 10f;
	}

	public async UniTask Init(ProgressInfo info)
	{
		progressInfo = info;
		foreach (Transform item2 in roadParent.gameObject.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		rewardNextImages = new List<(int, int, Sprite)>();
		int goalCount = progressInfo.Master.AchievementList().Last();
		int achieveCount = Math.Min(goalCount, progressInfo.achieveIndex + 3);
		isGoal = progressInfo.achieveIndex >= goalCount;
		await CharaImageLoad();
		await BGImageLoad(info.Master.CategoryMaster.MapBGType);
		masuWidth = masuObj.transform.GetComponent<RectTransform>().sizeDelta.x + roadParent.GetComponent<HorizontalLayoutGroup>().spacing;
		term.text = progressInfo.progressingTerm;
		for (int j = progressInfo.currentIndex; j <= achieveCount; j++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(masuObj, roadParent);
			if (j == progressInfo.currentIndex && j != 0)
			{
				gameObject.transform.Find("BeforeRoad").gameObject.SetActive(value: true);
			}
			if (j == goalCount)
			{
				gameObject.transform.Find("Road").gameObject.SetActive(value: false);
			}
			gameObject.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = j.ToString();
			bool flag = progressInfo.Master.AchievementList().Contains(j);
			gameObject.transform.Find("Masu").gameObject.SetActive(!flag);
			gameObject.transform.Find("MasuEx").gameObject.SetActive(flag);
		}
		rewardUnit.text = progressInfo.Master.AchievementUnit();
		rewardIndex.text = progressInfo.Master.AchievementIndex();
		for (int i = progressInfo.Master.GetAchievementIndex(progressInfo.currentIndex); i <= progressInfo.Master.GetAchievementIndex(progressInfo.achieveIndex); i++)
		{
			(string, int) tuple = progressInfo.Master.AchievementItem(i);
			string item = tuple.Item1;
			int num = tuple.Item2;
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(ItemUtility.ItemIconGraphicPath(item));
			disposableAsset.AddTo(this);
			rewardNextImages.Add((progressInfo.Master.AchievementCount(i), num, disposableAsset.Asset));
		}
		SetRewardInfo(0);
	}

	private async UniTask CharaImageLoad()
	{
		if (charaImageSprite != null)
		{
			charaImage.sprite = charaImageSprite;
			animator.SetTrigger("ToWait");
			return;
		}
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>($"SdChara/{charaType}/Textures/Base.png");
		disposableAsset.AddTo(this);
		charaImageSprite = disposableAsset.Asset;
		charaImage.sprite = charaImageSprite;
		disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>($"SdChara/{charaType}/Textures/Goal.png");
		disposableAsset.AddTo(this);
		charaGoalSprite = disposableAsset.Asset;
	}

	public void CharaInit()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Goal"))
		{
			animator.SetTrigger("Init");
		}
	}

	private async UniTask BGImageLoad(BGType bgType)
	{
		if (bgType != 0)
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>($"PlayCountWindow/Textures/Bg{bgType}.png");
			disposableAsset.AddTo(this);
			bg1.sprite = disposableAsset.Asset;
		}
	}

	private void GoalImageChange()
	{
		charaImage.sprite = charaGoalSprite;
	}

	private void NormalImageChange()
	{
		charaImage.sprite = charaImageSprite;
	}

	private void CharaJumpSE()
	{
		SoundManager.PlaySe("SFX_SYS_BONUS6").Forget();
	}

	private void ItemGetSE()
	{
		SoundManager.PlaySe("SFX_SYS_BONUS7").Forget();
	}

	public async UniTask Play(CancellationToken token)
	{
		roadParent.transform.localPosition = roadParentInitPos;
		int rewardIndex = 0;
		await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		isSkip = false;
		for (int i = 0; i < progressInfo.MoveCount; i++)
		{
			animator.speed = 1f / jumpTime;
			bool isAchieveExMasu = progressInfo.Master.AchievementList().Contains(progressInfo.currentIndex + i + 1);
			if (!isSkip)
			{
				await OneStep(i, token);
			}
			else
			{
				(i, isAchieveExMasu) = SkippingStep(rewardIndex);
			}
			if (i == progressInfo.MoveCount - 1 && isGoal)
			{
				animator.speed = 1f;
				animator.Play("Goal");
			}
			else if (isAchieveExMasu)
			{
				animator.speed = 1f;
				animator.Play("CheckPoint");
				await UniTask.WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, token);
				rewardIndex++;
				SetRewardInfo(rewardIndex);
				isSkip = false;
			}
			await UniTask.WhenAny(UniTask.WaitUntil(() => isSkip), UniTask.Delay(TimeSpan.FromSeconds(waitTime), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
		}
	}

	private void SetRewardInfo(int index)
	{
		if (rewardNextImages.Count() > index)
		{
			int item = rewardNextImages[index].Item2;
			rewardItemNum.text = $"×{item}";
			rewardItemNum.gameObject.SetActive(item >= 2);
			rewardCount.text = rewardNextImages[index].Item1.ToString();
			rewardImage.sprite = rewardNextImages[index].Item3;
			getRewardImage.sprite = rewardNextImages[index].Item3;
		}
	}

	private (int, bool) SkippingStep(int rewardIndex)
	{
		int num = Math.Min(rewardNextImages[rewardIndex].Item1 - progressInfo.currentIndex - 1, progressInfo.MoveCount - 1);
		float x = roadParentInitPos.x - masuWidth * (float)num - masuWidth;
		roadParent.localPosition = new Vector3(x, roadParentInitPos.y, roadParentInitPos.z);
		return (num, progressInfo.Master.AchievementList().Contains(progressInfo.currentIndex + num + 1));
	}

	private async UniTask OneStep(int i, CancellationToken token)
	{
		List<Tweener> tweenerList = new List<Tweener>();
		float num = roadParentInitPos.x - masuWidth * (float)i;
		float goal = num - masuWidth;
		tweenerList.Add(DOVirtual.Float(num, goal, jumpTime, delegate(float value)
		{
			roadParent.localPosition = new Vector3(value, roadParentInitPos.y, roadParentInitPos.z);
		}).OnComplete(delegate
		{
			roadParent.localPosition = new Vector3(goal, roadParentInitPos.y, roadParentInitPos.z);
		}));
		if (bgParent.localPosition.x * -1f >= (float)bgWidth)
		{
			bgParent.localPosition = new Vector3(bgParent.localPosition.x + (float)bgWidth, 0f, 0f);
		}
		float x = bgParent.localPosition.x;
		float bgGoal = x - masuWidth;
		tweenerList.Add(DOVirtual.Float(x, bgGoal, jumpTime, delegate(float value)
		{
			bgParent.localPosition = new Vector3(value, 0f, 0f);
		}).OnComplete(delegate
		{
			bgParent.localPosition = new Vector3(bgGoal, 0f, 0f);
		}));
		animator.SetTrigger("Jump");
		await UniTask.WhenAny(UniTask.WaitUntil(() => isSkip), UniTask.Delay(TimeSpan.FromSeconds(jumpTime), ignoreTimeScale: false, PlayerLoopTiming.Update, token));
		foreach (Tweener item in tweenerList)
		{
			item.Complete();
		}
	}

	private void Update()
	{
		if (!isSkip && ResultFlow.Skip())
		{
			isSkip = true;
		}
	}
}

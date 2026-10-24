using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanel : MonoBehaviour
{
	[SerializeField]
	private List<PlayerScorePanel> playerScorePanels;

	[SerializeField]
	public List<GameObject> m_ranks = new List<GameObject>();

	[SerializeField]
	public GameObject m_arrow;

	[SerializeField]
	public GameObject m_tieScoreMessage;

	[SerializeField]
	public GameObject m_kyokuTransition;

	[SerializeField]
	public GameObject m_resultBGInstanceRoot;

	[NonSerialized]
	public MahjongPlayFlow m_mahjongPlayFlow;

	[SerializeField]
	private List<RenderTexture> _renderTextures;

	private bool m_isRyukyoku;

	private bool m_isOrasu;

	public async UniTask Init(int cha, Player player)
	{
		if (player.IsValid)
		{
			await playerScorePanels[cha].Init(player, _renderTextures[cha]);
		}
		else
		{
			playerScorePanels[cha].gameObject.SetActive(value: false);
		}
	}

	public void SetScore(ScoreData score, List<Player> players, Player player, Han han, bool isOrasu, bool isRyukyoku = false, bool isKazoe = false, Cha bombPos = (Cha)(-1))
	{
		m_isRyukyoku = isRyukyoku;
		m_isOrasu = isOrasu;
		playerScorePanels[player.m_index].SetScore(score, players, player, han, isRyukyoku, isKazoe, bombPos);
	}

	public void SetFinalScore(long score, int rank, int uma, Player player)
	{
		playerScorePanels[player.m_index].SetFinalScore(score, rank, uma, player);
	}

	public async UniTask Play(bool isfinal = false)
	{
		SetArrow(isfinal);
		if (!isfinal)
		{
			await PlayProgress(isfinal);
		}
		else
		{
			await PlayFinal();
		}
		foreach (PlayerScorePanel playerScorePanel in playerScorePanels)
		{
			playerScorePanel.PlayExit();
		}
	}

	private void SetArrow(bool isFinal)
	{
		foreach (Transform item in m_arrow.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		if (!isFinal)
		{
			BigInteger bigInteger = playerScorePanels?[0].scoreMove.DiffScore ?? ((BigInteger)0);
			BigInteger bigInteger2 = playerScorePanels?[1].scoreMove.DiffScore ?? ((BigInteger)0);
			BigInteger bigInteger3 = playerScorePanels?[2].scoreMove.DiffScore ?? ((BigInteger)0);
			BigInteger bigInteger4 = playerScorePanels?[3].scoreMove.DiffScore ?? ((BigInteger)0);
			if (bigInteger < 0L && bigInteger2 > 0L)
			{
				m_arrow.transform.Find("ArrowZtoS").gameObject.SetActive(value: true);
			}
			if (bigInteger < 0L && bigInteger3 > 0L)
			{
				m_arrow.transform.Find("ArrowZtoT").gameObject.SetActive(value: true);
			}
			if (bigInteger < 0L && bigInteger4 > 0L)
			{
				m_arrow.transform.Find("ArrowZtoK").gameObject.SetActive(value: true);
			}
			if (bigInteger2 < 0L && bigInteger > 0L)
			{
				m_arrow.transform.Find("ArrowStoZ").gameObject.SetActive(value: true);
			}
			if (bigInteger2 < 0L && bigInteger3 > 0L)
			{
				m_arrow.transform.Find("ArrowStoT").gameObject.SetActive(value: true);
			}
			if (bigInteger2 < 0L && bigInteger4 > 0L)
			{
				m_arrow.transform.Find("ArrowStoK").gameObject.SetActive(value: true);
			}
			if (bigInteger3 < 0L && bigInteger > 0L)
			{
				m_arrow.transform.Find("ArrowTtoZ").gameObject.SetActive(value: true);
			}
			if (bigInteger3 < 0L && bigInteger2 > 0L)
			{
				m_arrow.transform.Find("ArrowTtoS").gameObject.SetActive(value: true);
			}
			if (bigInteger3 < 0L && bigInteger4 > 0L)
			{
				m_arrow.transform.Find("ArrowTtoK").gameObject.SetActive(value: true);
			}
			if (bigInteger4 < 0L && bigInteger > 0L)
			{
				m_arrow.transform.Find("ArrowKtoZ").gameObject.SetActive(value: true);
			}
			if (bigInteger4 < 0L && bigInteger2 > 0L)
			{
				m_arrow.transform.Find("ArrowKtoS").gameObject.SetActive(value: true);
			}
			if (bigInteger4 < 0L && bigInteger3 > 0L)
			{
				m_arrow.transform.Find("ArrowKtoT").gameObject.SetActive(value: true);
			}
		}
	}

	public void BgCameraSetActive(bool isActive)
	{
		m_mahjongPlayFlow.m_3dCamera.enabled = isActive;
		m_mahjongPlayFlow.m_3dCameraMyChara.enabled = isActive;
		m_mahjongPlayFlow.m_3dCameraTehai.enabled = isActive;
		m_mahjongPlayFlow.m_bgCamera.enabled = isActive;
		if (!isActive)
		{
			return;
		}
		foreach (PlayerScorePanel playerScorePanel in playerScorePanels)
		{
			try
			{
				playerScorePanel.player.character.m_scorePanelCamera.enabled = false;
			}
			catch
			{
			}
		}
	}

	private async UniTask ScoreMovePlay(List<PlayerScorePanel> rankList)
	{
		List<UniTask> tweenerList = new List<UniTask>();
		foreach (PlayerScorePanel rank in rankList)
		{
			if (rank.scoreMove.DiffScore != 0L)
			{
				tweenerList.Add(rank.ScoreRefresh());
			}
		}
		bool isCountUpSEPlay = !rankList.All((PlayerScorePanel x) => x.scoreMove.DiffScore == 0L);
		if (isCountUpSEPlay)
		{
			await SoundManager.PlaySe("SFX_MJG_POINT1");
		}
		else
		{
			VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.NoScore);
		}
		await tweenerList;
		if (isCountUpSEPlay)
		{
			SoundManager.StopSe("SFX_MJG_POINT1").Forget();
			SoundManager.PlaySe("SFX_MJG_POINT2").Forget();
		}
	}

	private async UniTask PlayProgress(bool isFinal)
	{
		List<PlayerScorePanel> rankList = (from a in playerScorePanels
			where a.IsActive
			orderby a.scoreMove.NewScore descending, a.StartKaze
			select a).ToList();
		_ = playerScorePanels[0].scoreMove.Rank;
		_ = rankList.Count;
		base.gameObject.SetActive(value: true);
		PlayerScorePanel oya = rankList.Find((PlayerScorePanel a) => a.JiKaze == Kaze.Ton);
		oya.OyaIn(isVoicePlay: false);
		GetComponent<Animator>().SetTrigger("ProgressIn");
		await ScorePanelSoundUtility.PlayProgress(rankList, m_isOrasu, m_isRyukyoku);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(2.0)), UniTask.WaitUntil(() => IsSkip()));
		await ScoreMovePlay(rankList);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(3.0)), UniTask.WaitUntil(() => IsSkip()));
		if (GameDataManager.Mahjong.IsBlowAwayMode)
		{
			foreach (PlayerScorePanel item in rankList)
			{
				item.DangerSetActive(isActive: true);
			}
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0)), UniTask.WaitUntil(() => IsSkip()));
		}
		GetComponent<Animator>().SetTrigger("ProgressLoop");
		(bool isOyaMove, bool isRenchan) tuple = JudgeNextOya(rankList);
		var (flag, _) = tuple;
		if (tuple.isRenchan)
		{
			oya.Renchan();
			if (playerScorePanels[0].JiKaze == Kaze.Ton)
			{
				VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Renchan1);
			}
			else
			{
				VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Renchan2);
			}
		}
		else if (flag)
		{
			oya.OyaOut();
			rankList.Find((PlayerScorePanel x) => x.JiKaze == Kaze.Nan).OyaIn();
		}
		SoundManager.StopBgmWithFadeout(4.0).Forget();
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0)), UniTask.WaitUntil(() => IsSkip()));
		if (GameDataManager.Mahjong.IsBlowAwayMode)
		{
			foreach (PlayerScorePanel item2 in rankList)
			{
				item2.DangerSetActive(isActive: false);
			}
		}
		BgCameraSetActive(isActive: true);
		GetComponent<Animator>().SetTrigger("Out");
	}

	private (bool isOyaMove, bool isRenchan) JudgeNextOya(List<PlayerScorePanel> rankList)
	{
		PlayerScorePanel playerScorePanel = rankList.Find((PlayerScorePanel a) => a.JiKaze == Kaze.Ton);
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima)
		{
			Debug.Log("二麻");
			return (isOyaMove: !m_isOrasu, isRenchan: false);
		}
		if (rankList.Exists((PlayerScorePanel a) => a.scoreMove.NewScore <= 0L))
		{
			Debug.Log("飛んだ");
			return (isOyaMove: false, isRenchan: false);
		}
		if (m_isRyukyoku)
		{
			Debug.Log("二麻");
			return (isOyaMove: !m_isOrasu, isRenchan: false);
		}
		if (m_isOrasu)
		{
			Debug.Log("オーラス");
			return (isOyaMove: false, isRenchan: playerScorePanel.scoreMove.DiffScore > 0L && rankList.First().JiKaze != Kaze.Ton);
		}
		return (isOyaMove: playerScorePanel.scoreMove.DiffScore <= 0L, isRenchan: playerScorePanel.scoreMove.DiffScore > 0L);
	}

	private async UniTask SetFinalMyChara(bool isWin, bool isWorst)
	{
		ProAssetType proAssetType;
		if (playerScorePanels[0].IsProDisplay)
		{
			base.transform.Find("Result").Find("MyChara").gameObject.SetActive(value: false);
			base.transform.Find("Result").Find("Pro").gameObject.SetActive(value: true);
			if (isWin)
			{
				if (isWorst)
				{
					goto IL_00a3;
				}
				proAssetType = ProAssetType.StandResult02;
			}
			else
			{
				if (!isWorst)
				{
					goto IL_00a3;
				}
				proAssetType = ProAssetType.StandResult03;
			}
			goto IL_00a7;
		}
		base.transform.Find("Result").Find("MyChara").gameObject.SetActive(value: true);
		base.transform.Find("Result").Find("Pro").gameObject.SetActive(value: false);
		IDisposableAsset<RenderTexture> disposableAsset = await AssetLoader.LoadAssetAsync<RenderTexture>("MahjongPlayScorePanel/Textures/3DTextureResultScorePanelJi.renderTexture");
		disposableAsset.AddTo(this);
		base.transform.Find("Result/MyChara").GetComponent<RawImage>().texture = disposableAsset.Asset;
		playerScorePanels[0].player.character.m_scorePanelCamera.targetTexture = disposableAsset.Asset;
		playerScorePanels[0].player.character.m_scorePanelCamera.enabled = true;
		return;
		IL_00a7:
		ProAssetType proAssetType2 = proAssetType;
		IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Pro, playerScorePanels[0].charaType, proAssetType2));
		disposableAsset2.AddTo(this);
		base.transform.Find("Result").Find("Pro").GetComponent<Image>()
			.sprite = disposableAsset2.Asset;
		return;
		IL_00a3:
		proAssetType = ProAssetType.StandResult01;
		goto IL_00a7;
	}

	private async UniTask PlayFinal()
	{
		List<PlayerScorePanel> rankList = (from a in playerScorePanels
			where a.IsActive
			orderby a.scoreMove.NewScore descending, a.StartKaze
			select a).ToList();
		int myRank = playerScorePanels[0].scoreMove.Rank;
		bool isWin = myRank == 0;
		bool isWorst = myRank == rankList.Count - 1;
		RectTransform component = base.transform.Find("Panel").GetComponent<RectTransform>();
		component.localPosition = new UnityEngine.Vector3(-200f, 0f, 0f);
		component.localScale = new UnityEngine.Vector3(0.8f, 0.8f, 0.8f);
		IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>("MahjongPlayResultBG/Prefabs/Bg" + (isWin ? "Win" : (isWorst ? "Lose" : "Normal")) + ".prefab");
		obj.AddTo(this);
		UnityEngine.Object.Instantiate(obj.Asset, m_resultBGInstanceRoot.transform, worldPositionStays: false);
		await SetFinalMyChara(isWin, isWorst);
		SoundManager.PlayBgm($"BGM_RESULT{(isWorst ? 4 : 2)}").Forget();
		BgCameraSetActive(isActive: false);
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().SetTrigger("ResultIn");
		UnityEngine.Object.Instantiate(m_ranks[myRank], base.transform.Find("Result").Find("MyRank"));
		ScorePanelSoundUtility.PlayRankSE(myRank);
		ScorePanelSoundUtility.PlayRankVoice(myRank);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5)), UniTask.WaitUntil(() => IsSkip()));
		GetComponent<Animator>().SetTrigger("ResultLoop");
		IEnumerable<BigInteger> source = rankList.Select((PlayerScorePanel x) => x.scoreMove.BeforeScore);
		m_tieScoreMessage.SetActive(source.Count() != source.Distinct().Count());
		foreach (PlayerScorePanel item in rankList)
		{
			item.SetResultAction(rankList.Count());
		}
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(2.0)), UniTask.WaitUntil(() => IsSkip()));
		foreach (PlayerScorePanel item2 in rankList)
		{
			if (item2.cha != 0)
			{
				item2.SetRank(m_ranks[item2.scoreMove.Rank]);
			}
		}
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0)), UniTask.WaitUntil(() => IsSkip()));
		await ScoreMovePlay(rankList);
		await UniTask.Delay(TimeSpan.FromSeconds(0.5));
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(4.0)), UniTask.WaitUntil(() => IsSkip()));
	}

	private bool IsSkip()
	{
		if (MahjongUtility.IsNoInputAutoTsumogiri() || ResidentFlow.CustomInput.GetMouseButtonDown(0) || GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect)
		{
			return true;
		}
		if (GameDataManager.PlayerConfig.IsKeyboardPlay() && Bi2xInput.GetKeyDown(KeyCode.Return))
		{
			return true;
		}
		return false;
	}
}

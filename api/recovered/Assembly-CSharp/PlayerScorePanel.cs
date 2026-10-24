using System;
using System.Collections.Generic;
using System.Numerics;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MahjongPlay.Character;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScorePanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI playerName;

	[SerializeField]
	private TextMeshProUGUI playerScore;

	[SerializeField]
	private List<ScoreAddFrames> scoreAddFrames;

	[SerializeField]
	private GameObject pao;

	[SerializeField]
	private GameObject buttobiDanger;

	[SerializeField]
	private Animator oyaIcon;

	[SerializeField]
	private GameObject bombIcon;

	[SerializeField]
	private Transform rankParent;

	[SerializeField]
	private GameObject charaRenderTexture;

	[SerializeField]
	private GameObject proTexture;

	[SerializeField]
	private GameObject proIcon;

	[SerializeField]
	private GameObject catharsisKingIcon;

	[HideInInspector]
	public ScoreMove scoreMove = new ScoreMove();

	public bool IsActive;

	public Player player;

	private bool isAotenjoOverScore;

	private List<IDisposableAsset<Sprite>> proIconSprite = new List<IDisposableAsset<Sprite>>();

	public bool IsProDisplay => player.IsProAnimEnable();

	public int cha => (int)player.m_cha;

	public Kaze JiKaze => player.m_jikaze;

	public Kaze StartKaze => player.m_startKaze;

	public CharaType charaType => player.m_charaType;

	public int level => player.m_level;

	public CharacterModelController modelController => player.character.m_characterModelController;

	public async UniTask Init(Player p, RenderTexture renderTexture)
	{
		IsActive = true;
		player = p;
		playerName.text = player.m_name;
		proIcon.SetActive(IsProDisplay);
		if (IsProDisplay)
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Pro, charaType, ProAssetType.ScoreCutinJoy));
			disposableAsset.AddTo(this);
			proIconSprite.Add(disposableAsset);
			disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Pro, charaType, ProAssetType.ScoreCutinCry));
			disposableAsset.AddTo(this);
			proIconSprite.Add(disposableAsset);
			disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Pro, charaType, ProAssetType.ScoreCutinNormal));
			disposableAsset.AddTo(this);
			proIconSprite.Add(disposableAsset);
		}
		else
		{
			charaRenderTexture.transform.GetComponent<RawImage>().texture = renderTexture;
		}
		if (JiKaze == Kaze.Ton)
		{
			oyaIcon.SetTrigger("In");
			oyaIcon.SetTrigger("Loop");
		}
	}

	public void PlayExit()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetScore(ScoreData score, List<Player> players, Player p, Han han, bool isRyukyoku = false, bool isKazoe = false, Cha bombPos = (Cha)(-1))
	{
		player = p;
		base.gameObject.SetActive(value: true);
		this.scoreMove.BeforeScore = score.BeforeScore;
		if (GameDataManager.Mahjong.IsAotenjoMode && Math.Abs(score.NewScore) >= MahjongUtility.Aotenjo.MaxScore)
		{
			ScoreMove obj = this.scoreMove;
			ScoreMove scoreMove = this.scoreMove;
			(BigInteger, BigInteger) scorePanelScore = MahjongUtility.Aotenjo.GetScorePanelScore(this.scoreMove, cha, players, bombPos);
			obj.YakuScore = scorePanelScore.Item1;
			scoreMove.NewScore = scorePanelScore.Item2;
			isAotenjoOverScore = true;
			if (p.IsJicha())
			{
				GameDataManager.Mahjong.AotenjoScore = this.scoreMove.NewScore;
			}
		}
		else
		{
			this.scoreMove.NewScore = score.NewScore;
			this.scoreMove.YakuScore = score.YakuScore;
		}
		this.scoreMove.KyotakuScore = score.KyotakuScore;
		this.scoreMove.TsumifuScore = score.TsumifuScore;
		this.scoreMove.RankScore = 0;
		this.scoreMove.DiffScore = this.scoreMove.NewScore - this.scoreMove.BeforeScore;
		this.scoreMove.Han = han;
		this.scoreMove.IsKazoe = isKazoe;
		SetFrames(isFinal: false, isRyukyoku, score.WhereFore == 1);
		bool disp = this.scoreMove.DiffScore != 0L || isRyukyoku;
		CutinCharaIconType iconType = ((!(this.scoreMove.DiffScore > 0L)) ? CutinCharaIconType.Cry : CutinCharaIconType.Joy);
		SetCharaIcon(disp, iconType);
		bombIcon.SetActive(bombPos != (Cha)(-1) && cha == (int)bombPos && this.scoreMove.DiffScore != 0L);
		catharsisKingIcon.SetActive(GameDataManager.Mahjong.IsComebackMode && p.m_isCatharsisKing);
	}

	public void SetFinalScore(long score, int rank, int uma, Player p)
	{
		player = p;
		if (GameDataManager.Mahjong.IsAotenjoMode && isAotenjoOverScore)
		{
			scoreMove.BeforeScore = scoreMove.NewScore;
			scoreMove.NewScore += (BigInteger)uma;
		}
		else
		{
			scoreMove.BeforeScore = score;
			scoreMove.NewScore = score + uma;
		}
		scoreMove.YakuScore = 0;
		scoreMove.KyotakuScore = 0;
		scoreMove.TsumifuScore = 0;
		scoreMove.RankScore = uma;
		scoreMove.DiffScore = scoreMove.NewScore - scoreMove.BeforeScore;
		scoreMove.Rank = rank;
		SetFrames(isFinal: true);
		bombIcon.SetActive(value: false);
		catharsisKingIcon.SetActive(value: false);
	}

	public async UniTask ScoreRefresh()
	{
		int animFrame = 40;
		BigInteger score = scoreMove.BeforeScore;
		BigInteger addScorePerFrame = (scoreMove.NewScore - score) / animFrame;
		BigInteger addYakuScorePerFrame = scoreMove.YakuScore / animFrame;
		int addKyotakuScorePerFrame = scoreMove.KyotakuScore / animFrame;
		int addTsumifuScorePerFrame = scoreMove.TsumifuScore / animFrame;
		int addRankScorePerFrame = scoreMove.RankScore / animFrame;
		for (int i = 1; i < animFrame; i++)
		{
			SetScoreText(playerScore, score + addScorePerFrame * i, isTotalScore: true);
			SetScoreText(scoreAddFrames[2].scoreText, scoreMove.RankScore - addRankScorePerFrame * i);
			SetScoreText(scoreAddFrames[3].scoreText, scoreMove.YakuScore - addYakuScorePerFrame * i);
			SetScoreText(scoreAddFrames[0].scoreText, scoreMove.KyotakuScore - addKyotakuScorePerFrame * i);
			SetScoreText(scoreAddFrames[1].scoreText, scoreMove.TsumifuScore - addTsumifuScorePerFrame * i);
			SetScoreText(scoreAddFrames[4].scoreText, scoreMove.YakuScore - addYakuScorePerFrame * i);
			await UniTask.WhenAny(UniTask.NextFrame(), UniTask.WaitUntil(() => IsSkip()));
			if (IsSkip())
			{
				break;
			}
		}
		ScoreRefreshComplete();
	}

	public void ScoreRefreshComplete()
	{
		SetScoreText(playerScore, scoreMove.NewScore, isTotalScore: true);
		SetScoreText(scoreAddFrames[2].scoreText, 0);
		SetScoreText(scoreAddFrames[3].scoreText, 0);
		SetScoreText(scoreAddFrames[0].scoreText, 0);
		SetScoreText(scoreAddFrames[1].scoreText, 0);
		SetScoreText(scoreAddFrames[4].scoreText, 0);
	}

	public void SetFrames(bool isFinal, bool isRyukyoku = false, bool isPao = false)
	{
		SetScoreText(playerScore, scoreMove.BeforeScore, isTotalScore: true);
		scoreAddFrames[2].frame.SetActive(isFinal && scoreMove.RankScore != 0);
		SetScoreText(scoreAddFrames[2].scoreText, scoreMove.RankScore);
		scoreAddFrames[0].frame.SetActive(!isFinal && scoreMove.KyotakuScore != 0);
		SetScoreText(scoreAddFrames[0].scoreText, scoreMove.KyotakuScore);
		scoreAddFrames[1].frame.SetActive(!isFinal && scoreMove.TsumifuScore != 0);
		SetScoreText(scoreAddFrames[1].scoreText, scoreMove.TsumifuScore);
		scoreAddFrames[3].frame.SetActive(!isFinal && !isRyukyoku && scoreMove.YakuScore != 0L);
		SetScoreText(scoreAddFrames[3].scoreText, scoreMove.YakuScore);
		scoreAddFrames[4].frame.SetActive(!isFinal && isRyukyoku && scoreMove.YakuScore != 0L);
		SetScoreText(scoreAddFrames[4].scoreText, scoreMove.YakuScore);
		pao.SetActive(!isFinal && isPao);
	}

	private void SetScoreText(TextMeshProUGUI text, BigInteger score, bool isTotalScore = false)
	{
		string color = ((score < 0L) ? "#FF0000" : ((!isTotalScore && score > 0L) ? "#00F3FF" : "#FFFFFF"));
		Color color2 = ((score < 0L) ? Color.red : ((!isTotalScore && score > 0L) ? Color.cyan : Color.white));
		text.text = ((!isTotalScore && score > 0L) ? "＋" : "") + MahjongUtility.Aotenjo.GetScoreString(score, isYakuResult: false, color);
		text.color = color2;
	}

	private void SetCharaIcon(bool disp, CutinCharaIconType iconType = CutinCharaIconType.Normal, bool isFinish = false)
	{
		if (cha == 0 && isFinish)
		{
			charaRenderTexture.SetActive(value: false);
			proTexture.SetActive(value: false);
			return;
		}
		bool active = disp && !IsProDisplay;
		bool flag = disp && IsProDisplay;
		player.character.m_scorePanelCamera.enabled = active;
		charaRenderTexture.SetActive(active);
		proTexture.SetActive(flag);
		if (flag)
		{
			proTexture.transform.GetComponent<Image>().sprite = proIconSprite[(int)iconType].Asset;
		}
	}

	public void SetResultAction(int playerCount)
	{
		base.gameObject.SetActive(value: true);
		CutinCharaIconType cutinCharaIconType = ((scoreMove.Rank != 0) ? ((scoreMove.Rank == playerCount - 1) ? CutinCharaIconType.Cry : CutinCharaIconType.Normal) : CutinCharaIconType.Joy);
		SetCharaIcon(disp: true, cutinCharaIconType, isFinish: true);
		player.character.ResultAction((Character.EventID)(24 + cutinCharaIconType));
		buttobiDanger.SetActive(value: false);
	}

	public void SetRank(GameObject obj)
	{
		UnityEngine.Object.Instantiate(obj, rankParent);
	}

	public void Renchan()
	{
		oyaIcon.SetTrigger("Loop");
		oyaIcon.SetTrigger("RenchanIn");
		SoundManager.PlaySe("SFX_MJG_DISPLAY11").Forget();
		SoundManager.PlayVoice("VOX_GXX_MJG_OYA3", player.m_charaType).Forget();
	}

	public void OyaOut()
	{
		oyaIcon.SetTrigger("Out");
		SoundManager.PlaySe("SFX_MJG_DISPLAY7").Forget();
	}

	public void OyaIn(bool isVoicePlay = true)
	{
		oyaIcon.SetTrigger("In");
		oyaIcon.SetTrigger("Loop");
		if (isVoicePlay)
		{
			if (cha == 0)
			{
				SoundManager.PlayMyCharaVoice("VOX_GXX_MJG_OYA1").Forget();
			}
			else
			{
				SoundManager.PlayMyCharaVoice("VOX_GXX_MJG_OYA2").Forget();
			}
		}
	}

	public void DangerSetActive(bool isActive)
	{
		buttobiDanger.SetActive(isActive && scoreMove.NewScore <= MahjongUtility.BlowAway.DangerScore);
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

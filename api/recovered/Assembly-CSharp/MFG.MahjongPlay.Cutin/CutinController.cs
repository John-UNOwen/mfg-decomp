using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

public abstract class CutinController : MonoBehaviour
{
	private static readonly List<Yaku> s_yakumanCutinPriority = new List<Yaku>
	{
		Yaku.Tenho,
		Yaku.Chiho,
		Yaku.Tyuren9,
		Yaku.TyurenTanki,
		Yaku.Kokushi13,
		Yaku.KokushiTanki,
		Yaku.Ryuiso,
		Yaku.Chinroto,
		Yaku.Daisangen,
		Yaku.Daisushi,
		Yaku.Syosushi,
		Yaku.Tsuiso,
		Yaku.SuankoTanki,
		Yaku.Suanko,
		Yaku.Sukantsu,
		Yaku.Kazoeyakuman
	};

	protected CutinType m_cutinType;

	protected ItemID m_itemID;

	protected IPlayerInteractor m_sourcePlayer;

	protected IEnumerable<IPlayerInteractor> m_players;

	protected CutinPlayMode m_cutinPlayMode;

	protected int m_randomValueForVoice;

	public virtual bool LoopEnable { get; set; }

	public virtual bool AudioOutputEnable { get; set; } = true;


	public ItemID ItemID => m_itemID;

	public abstract UniTask Prepare(CancellationToken cancellationToken = default(CancellationToken));

	public abstract UniTask Play(CancellationToken cancellationToken = default(CancellationToken));

	public abstract UniTask End(CancellationToken cancellationToken = default(CancellationToken));

	public abstract UniTask Stop(CancellationToken cancellationToken = default(CancellationToken));

	public abstract bool IsPlaying();

	public abstract bool IsPlayingIntro();

	public abstract bool IsPrepared();

	public void SetParameters(CutinType cutinType, ItemID itemID, Cha sourceCha, IEnumerable<IPlayerInteractor> players, CutinPlayMode cutinPlayMode, bool isAudioOutputEnable, int randomValueForVoice)
	{
		m_cutinType = cutinType;
		m_itemID = itemID;
		m_sourcePlayer = players.Where((IPlayerInteractor p) => p.Cha() == sourceCha).First();
		m_players = players;
		m_cutinPlayMode = cutinPlayMode;
		AudioOutputEnable = isAudioOutputEnable;
		m_randomValueForVoice = randomValueForVoice;
	}

	protected abstract UniTask PlaySound(bool isPlay, CancellationToken cancellationToken = default(CancellationToken));

	protected bool CurrentAnimator_IsPlayingWhithState(List<string> playingStateNames)
	{
		try
		{
			if (base.gameObject == null)
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
		Animator component = GetComponent<Animator>();
		if (component == null)
		{
			return false;
		}
		AnimatorStateInfo info = component.GetCurrentAnimatorStateInfo(0);
		return playingStateNames.Find((string x) => info.IsName(x)) != null;
	}

	protected string ResolveYakumanCutin(Han han, List<Yaku> yakus)
	{
		if (han >= Han.Yakuman)
		{
			foreach (Yaku item in s_yakumanCutinPriority)
			{
				if (GameDataManager.Mahjong.IsAotenjoMode && item == Yaku.Kazoeyakuman)
				{
					return "";
				}
				if (yakus.Contains(item))
				{
					return AssetPathResolver.ResolveYakumanCutin(item);
				}
			}
		}
		Debug.LogError("想定外のケース");
		return "";
	}
}

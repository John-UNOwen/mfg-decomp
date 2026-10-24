using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Sytem.Utilities.Xml;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Types.Competition;
using MFG.Types.ScrambleFestival;
using MFG.Types.ShopCompetition;
using UnityEngine;

namespace MFG.GameRequest;

public class EntryGame : RequestApiBase<EntryGame, EntryGame.ResultData>
{
	public class ResultData : ResultBase
	{
		public EntryTakuInfo TakuInfo = new EntryTakuInfo();
	}

	protected override void OnCreateSendParameter()
	{
		GAME_MODE gameMode = GameDataManager.Mahjong.GetGameMode();
		TAKU_CLASS tAKU_CLASS = TAKU_CLASS.NORMAL;
		MATCH_MODE mATCH_MODE = MATCH_MODE.WHOLE;
		string text = " ";
		TeamType teamType = TeamType.None;
		if (GameDataManager.Mahjong.IsPrivateMatch)
		{
			Debug.Log("<color=yellow>PrivateMatch : " + GameDataManager.PlayerConfig.PrivateMatchPassCode + "</color>");
			mATCH_MODE = MATCH_MODE.PRIVATE;
			text = GameDataManager.PlayerConfig.PrivateMatchPassCode;
		}
		ScrambleFestivalEventInfo currentInfo = GameDataManager.ScrambleFestival.CurrentInfo;
		if (currentInfo != null && currentInfo.IsEnable)
		{
			teamType = GameDataManager.ScrambleFestival.CurrentInfo.Team;
		}
		string text2 = string.Join(",", GameDataManager.Mahjong.TaikyokuItems.ConvertAll<string>(PlayerItemMaster.ToOID));
		string text3 = " ";
		string text4 = "";
		int num = -1;
		string text5 = "";
		string text6 = $"&pro_stay={(GameDataManager.IsProStay() ? 1 : 0)}";
		MFG.Types.Competition.CompetitionID competitionID = MFG.Types.Competition.CompetitionID.None;
		CompetitionInfo inProgressEntriedCompetitionInfo = GameDataManager.Competition.GetInProgressEntriedCompetitionInfo();
		if (inProgressEntriedCompetitionInfo != null && inProgressEntriedCompetitionInfo.IsValid(GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.EventType))
		{
			competitionID = inProgressEntriedCompetitionInfo.CompetitionID;
		}
		MFG.Types.ShopCompetition.CompetitionID competitionID2 = MFG.Types.ShopCompetition.CompetitionID.None;
		ShopCompetitionInfo inProgressCompetitionInfo = GameDataManager.ShopCompetition.GetInProgressCompetitionInfo();
		if (inProgressCompetitionInfo != null && inProgressCompetitionInfo.IsValid(GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.EventType))
		{
			competitionID2 = inProgressCompetitionInfo.CompetitionID;
		}
		m_sendUrl = base.DefaultFrontServerUrl + "/entry_game";
		m_postStr = $"pcuid={base.AppSessionID}&area_id={base.AreaID}&web_id={base.WebID}&gmode={(int)gameMode}&taku_class={(int)tAKU_CLASS}&match_mode={(int)mATCH_MODE}&passphrase={text}&teammatch={(int)teamType}&consume_oids={text2}&entry_id={text3}{text4}{text5}{text6}&competition_id={(int)competitionID}&shop_competition_id={(int)competitionID2}&debug_score={num}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		if (baseElem.IsExistElement("preference_matching"))
		{
			OnParsePreferenceMatching(baseElem);
			return;
		}
		XElement entryElement = baseElem.MustNeedElement("entry");
		base.Result.TakuInfo.Parse(entryElement);
	}

	protected void OnParsePreferenceMatching(XElement baseElement)
	{
		XElement baseElement2 = baseElement.MustNeedElement("preference_matching").MustNeedElement("entry_matching_official_player");
		baseElement2.AttributeValueInt("entry_id");
		baseElement2.AttributeValueInt("priority");
		baseElement2.AttributeValueInt("entry_unit");
		baseElement2.MustNeedElement("unit_left").ValueInt();
	}
}

using System;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Sytem.Utilities.Xml;
using UnityEngine;

namespace MFG.GameRequest;

public class ClientStateRead : RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>
{
	public class Param
	{
		public string OneKindType;
	}

	public class ResultData : ResultBase
	{
		public VersionGameData Version;

		public ReceiveOnlyGameData ReceiveOnly;

		public PlayerConfigGameData PlayerConfig;

		public PlayerGameData Player;

		public CustomizeGameData Customize;

		public PlayerItemGameData PlayerItem;

		public StoryGameData Story;

		public NoticeGameData Notice;

		public PresentBoxGameData PresentBox;

		public MissionGameData Mission;

		public GachaGameData Gacha;

		public CardConnectGameData CardConnect;

		public BingoEventGameData BingoEvent;

		public CompetitionGameData Competition;

		public CompetitionGameData LatestCompetition;

		public CoopGameData CoopGameData;

		public LogGameData Log;

		public CharaStampEventGameData CharaStampEvent;

		public EventTakuGameData EventTaku;

		public VoteGameData Vote;

		public PlayerPaidItemGameData PlayerPaidItem;

		public ScrambleFestivalGameData ScrambleFestival;

		public FriendGameData Friend;

		public MusicGachaGameData MusicGacha;

		public BingoEventRemakeGameData BingoEventRemake;

		public OthersEventGameData OthersEvent;

		public ShopCompetitionGameData ShopCompetition;

		public ShopCompetitionGameData LatestShopCompetition;
	}

	protected override void OnCreateSendParameter()
	{
		int playerID = GameDataManager.Account.PlayerID;
		string text = (string.IsNullOrEmpty(base.SendParameter.OneKindType) ? "" : ("&one_kind=" + base.SendParameter.OneKindType));
		m_sendUrl = base.DefaultFrontServerUrl + "/client_state_read";
		m_postStr = $"web_id={base.WebID}&area_id={base.AreaID}&pcuid={base.AppSessionID}&mid={playerID}{text}";
	}

	protected override async UniTask OnParse(XElement baseElem, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await base.OnParse(baseElem, cancellationToken);
		try
		{
			foreach (XElement item in baseElem.Elements("state"))
			{
				string value = item.Attribute("kind").Value;
				byte[] bytes = Convert.FromBase64String(item.Element("data").Value);
				string @string = Encoding.UTF8.GetString(bytes);
				switch (value)
				{
				case "version_game":
					base.Result.Version = VersionGameData.LoadFromJson(@string);
					break;
				case "receive_only":
					base.Result.ReceiveOnly = ReceiveOnlyGameData.LoadFromJson(@string);
					break;
				case "player_config":
					base.Result.PlayerConfig = PlayerConfigGameData.LoadFromJson(@string);
					break;
				case "player_game":
					base.Result.Player = PlayerGameData.LoadFromJson(@string);
					break;
				case "player_item":
					base.Result.PlayerItem = PlayerItemGameData.LoadFromJson(@string);
					break;
				case "customize_item":
					base.Result.Customize = CustomizeGameData.LoadFromJson(@string);
					break;
				case "story":
					base.Result.Story = StoryGameData.LoadFromJson(@string);
					break;
				case "notice_game":
					base.Result.Notice = NoticeGameData.LoadFromJson(@string);
					break;
				case "present_box_game":
					base.Result.PresentBox = PresentBoxGameData.LoadFromJson(@string);
					break;
				case "mission":
					base.Result.Mission = MissionGameData.LoadFromJson(@string);
					break;
				case "gacha":
					base.Result.Gacha = GachaGameData.LoadFromJson(@string);
					break;
				case "cc_request":
					base.Result.CardConnect = CardConnectGameData.LoadFromJson(@string);
					break;
				case "competition_game":
				{
					ResultData result = base.Result;
					result.Competition = await CompetitionGameData.LoadFromJson(@string);
					break;
				}
				case "competition_latest_result":
				{
					ResultData result = base.Result;
					result.LatestCompetition = await CompetitionGameData.LoadFromJson(@string);
					break;
				}
				case "ea_shop":
					base.Result.CoopGameData = CoopGameData.LoadFromJson(@string);
					break;
				case "log":
					base.Result.Log = LogGameData.LoadFromJson(@string);
					break;
				case "chara_stamp_event":
					base.Result.CharaStampEvent = CharaStampEventGameData.LoadFromJson(@string);
					break;
				case "event_taku":
					base.Result.EventTaku = EventTakuGameData.LoadFromJson(@string);
					break;
				case "vote_event":
					base.Result.Vote = VoteGameData.LoadFromJson(@string);
					break;
				case "player_paid_item":
					base.Result.PlayerPaidItem = PlayerPaidItemGameData.LoadFromJson(@string);
					break;
				case "scramble_festival":
					base.Result.ScrambleFestival = ScrambleFestivalGameData.LoadFromJson(@string);
					break;
				case "friend_game":
					base.Result.Friend = FriendGameData.LoadFromJson(@string);
					break;
				case "music_gacha":
					base.Result.MusicGacha = MusicGachaGameData.LoadFromJson(@string);
					break;
				case "bingo_event_remake":
					base.Result.BingoEventRemake = BingoEventRemakeGameData.LoadFromJson(@string);
					break;
				case "others_event":
					base.Result.OthersEvent = OthersEventGameData.LoadFromJson(@string);
					break;
				case "shop_competition_game":
				{
					ResultData result = base.Result;
					result.ShopCompetition = await ShopCompetitionGameData.LoadFromJson(@string);
					break;
				}
				case "shop_competition_latest_result":
				{
					ResultData result = base.Result;
					result.LatestShopCompetition = await ShopCompetitionGameData.LoadFromJson(@string);
					break;
				}
				default:
					Debug.LogWarning("ClientStateRead-->" + value + "のロード処理がありません");
					break;
				case "direct_card":
					break;
				}
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}

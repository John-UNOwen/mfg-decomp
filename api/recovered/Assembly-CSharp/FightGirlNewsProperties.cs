using System;
using System.Collections.Generic;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.FightGirlNews;
using UnityEngine;

[CreateAssetMenu(fileName = "FightGirlNewsProperties", menuName = "FightGirlNewsProperties")]
public class FightGirlNewsProperties : ScriptableObject
{
	private enum ExtraCharaType
	{
		None = -1,
		Chara01,
		Chara02,
		Chara03,
		Chara04,
		Chara05,
		Chara06,
		Chara07,
		Chara08,
		Chara09,
		Chara10,
		Chara11,
		Chara12,
		Chara13,
		Chara14,
		Chara15,
		Chara16,
		Chara17,
		Chara18,
		Chara19,
		Chara20,
		Chara21
	}

	[Serializable]
	public class Item
	{
		[Serializable]
		public class MovieSoundManager
		{
			[Header("鳴らすサウンド")]
			public string Label;

			[Header("BGM？")]
			public bool IsBGM;

			[Header("いつ鳴らす？(動画開始から n/60 秒後に鳴らす)")]
			public int Frame;
		}

		[SerializeField]
		[Header("ニュースID")]
		public NewsID ID;

		[SerializeField]
		[Header("ニュースタイプ")]
		public NewsType NewsType;

		[SerializeField]
		[Header("関連イベント News, Present, NewCostumeはこれを参照")]
		public GameEventType GameEventType;

		[SerializeField]
		[Header("関連ガチャ ガチャが絡む告知はこれを参照")]
		public GachaSeriesName GachaSeriesName;

		[SerializeField]
		[Header("キャラクター指定")]
		private ExtraCharaType _charaType = ExtraCharaType.None;

		[SerializeField]
		[Header("ムービーのパス(Assets/StreamingAssets/以下) 空の場合はムービーをスキップ")]
		public string MoviePath = "";

		[SerializeField]
		[Header("ムービー中に鳴らすサウンド")]
		public List<MovieSoundManager> MovieSoundList = new List<MovieSoundManager>();

		[SerializeField]
		[Header("ネームプレートを表示するか")]
		public bool IsNamePlate;

		[SerializeField]
		[Header("BGMラベル")]
		public string BgmLabel = "";

		public bool IsNewGirl => NewsType == NewsType.NewCharacter;

		public CharaType CharaType => (CharaType)_charaType;

		public CharaProfile Profile
		{
			get
			{
				if (CharaType < CharaType.Chara01)
				{
					return null;
				}
				return MasterGameDataManager.Character.Properties(CharaType).Profile;
			}
		}

		public string MovieAssetsPath => Application.streamingAssetsPath + "/" + MoviePath;

		public string AppearImagePath
		{
			get
			{
				if (NewsType > NewsType.Present && (NewsType != NewsType.MusicGacha || ID > NewsID.MusicHiyori) && ID != NewsID.Ponnomichi)
				{
					return string.Format("FightGirlNews/{0}/{1}/CharaImage.png", (CharaType >= CharaType.Chara01) ? $"Chara{(int)(CharaType + 1)}" : "Common", ID);
				}
				return $"FightGirlNews/Common/{ID}/AppearImage.png";
			}
		}

		public string NameImagePath => string.Format("FightGirlNews/{0}/{1}/CostumeName.png", (CharaType >= CharaType.Chara01) ? $"Chara{(int)(CharaType + 1)}" : "Common", ID);

		public bool IsEnable()
		{
			if (GameDataManager.Account.IsGuestUser && NewsType == NewsType.Present)
			{
				return false;
			}
			if (NewsType == NewsType.MusicGacha && !PaseliUtility.PPEX.IsEnablePPEX && !GameUtility.IsKonastaOrNetcafeBehavior)
			{
				return false;
			}
			if (ID == NewsID.Ponnomichi && GameUtility.IsKonastaOrNetcafeBehavior)
			{
				return false;
			}
			if (ID == NewsID.PremiumMode || ID == NewsID.OdekakeEnable)
			{
				return false;
			}
			if (ID == NewsID.UnlockChaos)
			{
				if (GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType))
				{
					return GameDataManager.IsCharaSelectable(CharaType.Chara11);
				}
				return false;
			}
			if (GachaSeriesName != 0)
			{
				GachaSeriesServerInfo gachaSeriesServerInfo = GameDataManager.GachaInfo.GachaSeriesInfosFromServer.Find((GachaSeriesServerInfo x) => x.GachaSeriesName == GachaSeriesName);
				if (gachaSeriesServerInfo == null || !gachaSeriesServerInfo.IsDisplayInfoNewGirl())
				{
					return false;
				}
			}
			if (GameEventType != 0)
			{
				return GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType);
			}
			return true;
		}
	}

	public List<Item> Items;

	public Item GetItem(NewsID id)
	{
		return Items.Find((Item x) => x.ID == id);
	}
}

using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Mission;

public class MusicAcquireInfo
{
	public MissionID MissionID;

	public ItemID ID;

	private static Dictionary<MFG.Types.Mission.Category, Dictionary<CharaType, string>> BGMDispNameDict = new Dictionary<MFG.Types.Mission.Category, Dictionary<CharaType, string>>
	{
		{
			MFG.Types.Mission.Category.Bemani002,
			new Dictionary<CharaType, string>
			{
				{
					CharaType.Chara01,
					"Sing a Song Sign"
				},
				{
					CharaType.Chara02,
					"惑星☆ロリポップ"
				},
				{
					CharaType.Chara03,
					"murmur twins"
				},
				{
					CharaType.Chara04,
					"べィスドロップ・フリークス "
				},
				{
					CharaType.Chara05,
					"蒼が消えるとき"
				},
				{
					CharaType.Chara06,
					"Burn Burn Burning"
				},
				{
					CharaType.Chara07,
					"恋する☆宇宙戦争っ！！"
				},
				{
					CharaType.Chara08,
					"黒髪乱れし修羅となりて"
				},
				{
					CharaType.Chara09,
					"び"
				},
				{
					CharaType.Chara10,
					"MAX LOVE"
				},
				{
					CharaType.Chara11,
					"JOMANDA"
				},
				{
					CharaType.Chara12,
					"VALLIS-NERIA"
				},
				{
					CharaType.Chara13,
					"ポチコの幸せな日常"
				},
				{
					CharaType.Chara14,
					"Sleepless days"
				},
				{
					CharaType.Chara15,
					"キャトられ♥恋はモ～モク"
				},
				{
					CharaType.Chara16,
					"抱きしめてモナムール "
				}
			}
		},
		{
			MFG.Types.Mission.Category.Bemani003,
			new Dictionary<CharaType, string>
			{
				{
					CharaType.Chara01,
					"僕の気持ちを描く"
				},
				{
					CharaType.Chara02,
					"Cookie Bouquets"
				},
				{
					CharaType.Chara03,
					"アルレシャ"
				},
				{
					CharaType.Chara04,
					"CHERNOBOG"
				},
				{
					CharaType.Chara05,
					"眠りの国のステラ"
				},
				{
					CharaType.Chara06,
					"Hades Doll"
				},
				{
					CharaType.Chara07,
					"FLOWER"
				},
				{
					CharaType.Chara08,
					"朧月覆う雲をも裂きぬ"
				},
				{
					CharaType.Chara09,
					"爆なな☆てすとロイヤー"
				},
				{
					CharaType.Chara10,
					"White Lovers"
				},
				{
					CharaType.Chara11,
					"アモ"
				},
				{
					CharaType.Chara12,
					"Preserved Valkyria"
				},
				{
					CharaType.Chara13,
					"ALBA -黎明-"
				},
				{
					CharaType.Chara14,
					"スラッシュ//シスターズ"
				},
				{
					CharaType.Chara15,
					"ロクブテ"
				},
				{
					CharaType.Chara16,
					"湘南ロンリー\nMIDNIGHT RAIN"
				},
				{
					CharaType.Chara17,
					"pastel@sweets labo(*'v'*)"
				},
				{
					CharaType.Chara18,
					"黎明スケッチブック"
				},
				{
					CharaType.Chara19,
					"アマイヒミツ"
				},
				{
					CharaType.Chara20,
					"RAGE feat.H14 of LEONAIR"
				}
			}
		}
	};

	public MFG.Types.Mission.Item Master => MissionMaster.GetItem(MissionID);

	public MFG.Types.Customize.CustomizeItem.Item CustomizeMaster => CustomizeItemMaster.GetItem(ID);

	public string GetMusicLabel(bool isPreview = true)
	{
		try
		{
			if (CustomizeMaster.Category == MFG.Types.Customize.CustomizeItem.Category.MyRoomBgm)
			{
				return ID.MyRoomToMyRoomBgmLabel(isPreview);
			}
			if (CustomizeMaster.Category == MFG.Types.Customize.CustomizeItem.Category.ReachBgm)
			{
				return isPreview ? ID.ReachBgmToThumbBgmLabel() : ID.ReachBgmToBgmLabel();
			}
		}
		catch
		{
		}
		return "";
	}

	public (string category, string name) GetItemName()
	{
		try
		{
			string text = SaniTize.SanitizedString(BGMDispNameDict[Master.Category][CustomizeMaster.charaType], "", 0);
			if (CustomizeMaster.Category == MFG.Types.Customize.CustomizeItem.Category.MyRoomBgm)
			{
				return (category: "マイルームBGM", name: text ?? "");
			}
			if (CustomizeMaster.Category == MFG.Types.Customize.CustomizeItem.Category.ReachBgm)
			{
				if (CustomizeMaster.Name.Contains("（ショート）"))
				{
					return (category: "リーチBGM", name: text + "<size=10>\n\n</size><size=35>（ショート）</size>");
				}
				if (CustomizeMaster.Name.Contains("（フル）"))
				{
					return (category: "リーチBGM", name: text + "<size=10>\n\n</size><size=35>（フル）</size>");
				}
			}
		}
		catch
		{
		}
		return (category: "", name: "");
	}

	public string GetImagePath()
	{
		try
		{
			CharaType charaType = CustomizeItemMaster.GetItem(ID).charaType;
			return string.Format("MissionEvent/{0}/Jacket/Jacket{1}.png", Master.Category, ((int)(charaType + 1)).ToString("D2"));
		}
		catch
		{
		}
		return "";
	}
}

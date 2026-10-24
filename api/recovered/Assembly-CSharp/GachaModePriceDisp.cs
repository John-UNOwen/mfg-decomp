using GameContents;
using MFG.GameData;
using TMPro;
using UnityEngine;

public class GachaModePriceDisp
{
	public void Set(GAME_MODE mode, GameObject creditFrame, GameObject paseliFrame, GameObject compensationFrame, TextMeshProUGUI credit, TextMeshProUGUI paseli)
	{
		int price = GameUtility.GetPrice(mode, CONSUME_TYPE.CREDIT);
		if (price <= 0 || GameUtility.IsReservedGameOver || GameUtility.IsKonastaOrNetcafeBehavior)
		{
			creditFrame.SetActive(value: false);
			credit.text = "";
		}
		else
		{
			creditFrame.SetActive(value: true);
			string arg = ((price == 1) ? "CREDIT" : "CREDITS");
			credit.text = $"{price} {arg}";
		}
		int price2 = GameUtility.GetPrice(mode, CONSUME_TYPE.PASELI);
		bool flag = Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha1);
		bool flag2 = Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha5);
		bool flag3 = Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.MusicGacha);
		bool flag4 = ((flag || flag2) && GameDataManager.Gacha.EntryGachaSelectSceneMode != GachaSeriesType.Music) || (flag3 && GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Music);
		if (!PaseliUtility.IsEnablePaseli || price2 <= 0 || GameUtility.IsReservedGameOver || (GameUtility.IsKonastaOrNetcafeBehavior && (GameDataManager.Player.IsPro() || GameUtility.IsProCardId(GameDataManager.Account.CardID) || flag4)))
		{
			paseliFrame.SetActive(value: false);
			paseli.text = "";
		}
		else
		{
			paseliFrame.SetActive(value: true);
			paseli.text = $"{price2}円(税込)";
		}
		compensationFrame.SetActive(mode switch
		{
			GAME_MODE.GACHA => flag, 
			GAME_MODE.GACHA5 => flag2, 
			GAME_MODE.MUSICGACHA => flag3, 
			_ => false, 
		});
	}
}

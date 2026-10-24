using UnityEngine;

namespace TestModeSkeleton.Price;

public class PriceInfo
{
	public int StartPrice;

	public int ContinuePrice;

	public int ContinueFactoryPrice;

	public int ContinueLowerPrice;

	public int ContinueUpperPrice;

	public int GachaPrice;

	public int Gacha2Price;

	public int PremiumPrice;

	public int PremiumStartPrice;

	public int PremiumContinuePrice;

	public int PremiumContinueFactoryPrice;

	public int MusicGachaPrice;

	public int GetPrice(GameContentsInfo.GameContents contents)
	{
		return contents switch
		{
			GameContentsInfo.GameContents.Start => StartPrice, 
			GameContentsInfo.GameContents.Continue => ContinuePrice, 
			GameContentsInfo.GameContents.Gacha => GachaPrice, 
			GameContentsInfo.GameContents.Gacha5 => Gacha2Price, 
			GameContentsInfo.GameContents.PremiumStart => PremiumStartPrice, 
			GameContentsInfo.GameContents.PremiumContinue => PremiumContinuePrice, 
			GameContentsInfo.GameContents.MusicGacha => MusicGachaPrice, 
			_ => 0, 
		};
	}

	public void DebugLog()
	{
		Debug.Log($"StartPrice:{StartPrice}");
		Debug.Log($"ContinuePrice:{ContinuePrice}");
		Debug.Log($"ContinueFactoryPrice:{ContinueFactoryPrice}");
		Debug.Log($"ContinueLowerPrice:{ContinueLowerPrice}");
		Debug.Log($"ContinueUpperPrice:{ContinueUpperPrice}");
		Debug.Log($"GachaPrice:{GachaPrice}");
		Debug.Log($"Gacha2Price:{Gacha2Price}");
		Debug.Log($"PremiumPrice:{PremiumPrice}");
		Debug.Log($"PremiumStartPrice:{PremiumStartPrice}");
		Debug.Log($"PremiumContinuePrice:{PremiumContinuePrice}");
		Debug.Log($"PremiumContinueFactoryPrice:{PremiumContinueFactoryPrice}");
		Debug.Log($"MusicGachaPricePrice:{MusicGachaPrice}");
	}
}

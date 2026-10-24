using System;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.PlayerItem;

public static class FightSpiritUtility
{
	public static bool HasEnoughSpirit(CharaType charaType, int totalConsumeCount)
	{
		var (num, num2) = GameDataManager.PlayerItem.GetFightSpiritCount(charaType);
		return num + num2 >= totalConsumeCount;
	}

	public static (int charaSpiritCount, int wildSpiritCount) CalculateConsumeSpiritCount(CharaType charaType, int totalConsumeCount)
	{
		var (ownedCharaSpiritCount, ownedWildSpiritCount) = GameDataManager.PlayerItem.GetFightSpiritCount(charaType);
		return CalculateConsumeSpiritCount(ownedCharaSpiritCount, ownedWildSpiritCount, totalConsumeCount);
	}

	public static (int charaSpiritNum, int wildSpiritNum) CalculateConsumeSpiritCount(int ownedCharaSpiritCount, int ownedWildSpiritCount, int totalConsumeCount)
	{
		if (ownedCharaSpiritCount + ownedWildSpiritCount < totalConsumeCount)
		{
			throw new InvalidOperationException("所持ファイトスピリット数が不足しています");
		}
		if (ownedCharaSpiritCount >= totalConsumeCount)
		{
			return (charaSpiritNum: totalConsumeCount, wildSpiritNum: 0);
		}
		return (charaSpiritNum: ownedCharaSpiritCount, wildSpiritNum: totalConsumeCount - ownedCharaSpiritCount);
	}

	public static void ConsumeSpirit(CharaType charaType, int consumeCharaSpiritCount, int consumewildSpiritCount, ItemConsumeType consumeType)
	{
		if (consumeCharaSpiritCount > 0)
		{
			ItemID itemID = PlayerItemMaster.ToFightSpiritItemID(charaType);
			GameDataManager.PlayerItem.Consume(itemID, consumeCharaSpiritCount, consumeType).Forget();
		}
		if (consumewildSpiritCount > 0)
		{
			GameDataManager.PlayerItem.Consume(ItemID.FightSpiritW, consumewildSpiritCount, consumeType).Forget();
		}
	}
}

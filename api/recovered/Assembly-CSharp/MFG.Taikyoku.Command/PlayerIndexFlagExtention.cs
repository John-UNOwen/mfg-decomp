namespace MFG.Taikyoku.Command;

public static class PlayerIndexFlagExtention
{
	public static int ToPlayerIndex(this PLAYER_INDEX_FLAG playerIndexFlag)
	{
		if ((playerIndexFlag & PLAYER_INDEX_FLAG.P0) != 0)
		{
			return 0;
		}
		if ((playerIndexFlag & PLAYER_INDEX_FLAG.P1) != 0)
		{
			return 1;
		}
		if ((playerIndexFlag & PLAYER_INDEX_FLAG.P2) != 0)
		{
			return 2;
		}
		if ((playerIndexFlag & PLAYER_INDEX_FLAG.P3) != 0)
		{
			return 3;
		}
		return 0;
	}
}

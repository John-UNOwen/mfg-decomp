namespace MFG.MahjonPlay.Hand;

internal static class PatternFlag
{
	public const long Isolated_1 = 1L;

	public const long Isolated_2 = 2L;

	public const long Isolated_3 = 4L;

	public const long Isolated_4 = 8L;

	public const long Isolated_5 = 16L;

	public const long Isolated_6 = 32L;

	public const long Isolated_7 = 64L;

	public const long Isolated_8 = 128L;

	public const long Isolated_9 = 256L;

	public const long Toitsu_1 = 512L;

	public const long Toitsu_2 = 1024L;

	public const long Toitsu_3 = 2048L;

	public const long Toitsu_4 = 4096L;

	public const long Toitsu_5 = 8192L;

	public const long Toitsu_6 = 16384L;

	public const long Toitsu_7 = 32768L;

	public const long Toitsu_8 = 65536L;

	public const long Toitsu_9 = 131072L;

	public const long Kanchan_1 = 262144L;

	public const long Kanchan_2 = 524288L;

	public const long Kanchan_3 = 1048576L;

	public const long Kanchan_4 = 2097152L;

	public const long Kanchan_5 = 4194304L;

	public const long Kanchan_6 = 8388608L;

	public const long Kanchan_7 = 16777216L;

	public const long Ryanmen_1 = 33554432L;

	public const long Ryanmen_2 = 67108864L;

	public const long Ryanmen_3 = 134217728L;

	public const long Ryanmen_4 = 268435456L;

	public const long Ryanmen_5 = 536870912L;

	public const long Ryanmen_6 = 1073741824L;

	public const long Ryanmen_7 = 2147483648L;

	public const long Penchan_1 = 4294967296L;

	public const long Penchan_8 = 8589934592L;

	public const long Koutsu_1 = 17179869184L;

	public const long Koutsu_2 = 34359738368L;

	public const long Koutsu_3 = 68719476736L;

	public const long Koutsu_4 = 137438953472L;

	public const long Koutsu_5 = 274877906944L;

	public const long Koutsu_6 = 549755813888L;

	public const long Koutsu_7 = 1099511627776L;

	public const long Koutsu_8 = 2199023255552L;

	public const long Koutsu_9 = 4398046511104L;

	public const long Syuntsu_1 = 8796093022208L;

	public const long Syuntsu_2 = 17592186044416L;

	public const long Syuntsu_3 = 35184372088832L;

	public const long Syuntsu_4 = 70368744177664L;

	public const long Syuntsu_5 = 140737488355328L;

	public const long Syuntsu_6 = 281474976710656L;

	public const long Syuntsu_7 = 562949953421312L;

	public const long IsManzu = 72057594037927936L;

	public const long IsSouzu = 144115188075855872L;

	public const long IsPinzu = 288230376151711744L;

	public const long IsJihai = 576460752303423488L;

	public const long IsFuro = 1152921504606846976L;

	private static long[] s_tostringOrder = new long[50]
	{
		1L, 512L, 262144L, 33554432L, 4294967296L, 17179869184L, 8796093022208L, 2L, 1024L, 524288L,
		67108864L, 34359738368L, 17592186044416L, 4L, 2048L, 1048576L, 134217728L, 68719476736L, 35184372088832L, 8L,
		4096L, 2097152L, 268435456L, 137438953472L, 70368744177664L, 16L, 8192L, 4194304L, 536870912L, 274877906944L,
		140737488355328L, 32L, 16384L, 8388608L, 1073741824L, 549755813888L, 281474976710656L, 64L, 32768L, 16777216L,
		2147483648L, 1099511627776L, 562949953421312L, 128L, 65536L, 8589934592L, 2199023255552L, 256L, 131072L, 4398046511104L
	};

	public static string ToString(long patternFlag)
	{
		string text = "|";
		for (int i = 0; i < s_tostringOrder.Length; i++)
		{
			long num = patternFlag & s_tostringOrder[i];
			if (num <= 33554432)
			{
				if (num <= 8192)
				{
					if (num <= 128)
					{
						if (num <= 16)
						{
							long num2 = num - 1;
							if ((ulong)num2 <= 3uL)
							{
								switch (num2)
								{
								case 0L:
									text += "1|";
									continue;
								case 1L:
									text += "2|";
									continue;
								case 3L:
									text += "3|";
									continue;
								case 2L:
									continue;
								}
							}
							switch (num)
							{
							case 8L:
								text += "4|";
								break;
							case 16L:
								text += "5|";
								break;
							}
						}
						else
						{
							switch (num)
							{
							case 32L:
								text += "6|";
								break;
							case 64L:
								text += "7|";
								break;
							case 128L:
								text += "8|";
								break;
							}
						}
					}
					else
					{
						switch (num)
						{
						case 256L:
							text += "9|";
							break;
						case 512L:
							text += "11|";
							break;
						case 1024L:
							text += "22|";
							break;
						case 2048L:
							text += "33|";
							break;
						case 4096L:
							text += "44|";
							break;
						case 8192L:
							text += "55|";
							break;
						}
					}
				}
				else
				{
					switch (num)
					{
					case 16384L:
						text += "66|";
						break;
					case 32768L:
						text += "77|";
						break;
					case 65536L:
						text += "88|";
						break;
					case 131072L:
						text += "99|";
						break;
					case 262144L:
						text += "13|";
						break;
					case 524288L:
						text += "24|";
						break;
					case 1048576L:
						text += "35|";
						break;
					case 2097152L:
						text += "46|";
						break;
					case 4194304L:
						text += "57|";
						break;
					case 8388608L:
						text += "68|";
						break;
					case 16777216L:
						text += "79|";
						break;
					case 33554432L:
						text += "12|";
						break;
					}
				}
			}
			else
			{
				switch (num)
				{
				case 67108864L:
					text += "23|";
					break;
				case 134217728L:
					text += "34|";
					break;
				case 268435456L:
					text += "45|";
					break;
				case 536870912L:
					text += "56|";
					break;
				case 1073741824L:
					text += "67|";
					break;
				case 2147483648L:
					text += "78|";
					break;
				case 4294967296L:
					text += "12|";
					break;
				case 8589934592L:
					text += "89|";
					break;
				case 17179869184L:
					text += "111|";
					break;
				case 34359738368L:
					text += "222|";
					break;
				case 68719476736L:
					text += "333|";
					break;
				case 137438953472L:
					text += "444|";
					break;
				case 274877906944L:
					text += "555|";
					break;
				case 549755813888L:
					text += "666|";
					break;
				case 1099511627776L:
					text += "777|";
					break;
				case 2199023255552L:
					text += "888|";
					break;
				case 4398046511104L:
					text += "999|";
					break;
				case 8796093022208L:
					text += "123|";
					break;
				case 17592186044416L:
					text += "234|";
					break;
				case 35184372088832L:
					text += "345|";
					break;
				case 70368744177664L:
					text += "456|";
					break;
				case 140737488355328L:
					text += "567|";
					break;
				case 281474976710656L:
					text += "678|";
					break;
				case 562949953421312L:
					text += "789|";
					break;
				}
			}
		}
		return text;
	}
}

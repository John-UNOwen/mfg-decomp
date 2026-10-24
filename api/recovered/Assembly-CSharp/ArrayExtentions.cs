public static class ArrayExtentions
{
	public static void Fill<T>(this T[] array, T val)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = val;
		}
	}

	public static void FillRange<T>(this T[] array, T val, int start, int end)
	{
		for (int i = start; i <= end; i++)
		{
			array[i] = val;
		}
	}
}

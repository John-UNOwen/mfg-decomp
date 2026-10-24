using System;

public static class RandomExtensions
{
	public static float GetValue(this Random random)
	{
		return (float)random.NextDouble();
	}

	public static int Range(this Random random, int min, int max)
	{
		return random.Next(min, max);
	}

	public static float Range(this Random random, float min, float max)
	{
		return (float)(random.NextDouble() * (double)(max - min)) + min;
	}
}

using System;

public class RandomValueGenerator
{
	private Random m_random = new Random();

	public void Init(int seed)
	{
		m_random = new Random(seed);
	}

	public int Range(int minInclusive, int maxExclusive)
	{
		return m_random.Next(minInclusive, maxExclusive);
	}
}

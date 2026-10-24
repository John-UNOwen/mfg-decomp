using Cysharp.Threading.Tasks;

internal class CoinSoundPlayer
{
	private int m_restCount;

	public bool IsEnable { get; set; }

	public void Update()
	{
		if (!IsEnable)
		{
			m_restCount = 0;
		}
		else if (m_restCount > 0)
		{
			SoundManager.PlaySe("SFX_SYS_COIN").Forget();
			m_restCount--;
		}
	}

	public void AddCall(int count)
	{
		if (count >= 0)
		{
			m_restCount += count;
		}
	}

	public void Clear()
	{
		m_restCount = 0;
	}
}

public abstract class IODeviceBase
{
	protected bool m_bFlushBackup;

	public bool IsActive { get; protected set; }

	public virtual void Initialize()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void PreEnterTestMode()
	{
	}

	public virtual void PostExitTestMode()
	{
	}

	public virtual void Activate(bool bActive)
	{
		IsActive = bActive;
	}
}

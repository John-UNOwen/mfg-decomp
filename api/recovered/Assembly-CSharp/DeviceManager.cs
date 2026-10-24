public class DeviceManager
{
	public enum DEVICE_TYPE
	{
		COIN,
		IKEY,
		NUM
	}

	private static DeviceManager instance;

	private IODeviceBase[] m_devices;

	public bool Initialized { get; private set; }

	public static DeviceManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new DeviceManager();
			}
			return instance;
		}
		private set
		{
			instance = value;
		}
	}

	private DeviceManager()
	{
		m_devices = new IODeviceBase[2];
		m_devices[0] = new CoinDevice();
		m_devices[1] = new IkeyDevice();
	}

	public void Initialize()
	{
		IODeviceBase[] devices = m_devices;
		for (int i = 0; i < devices.Length; i++)
		{
			devices[i].Initialize();
		}
		Initialized = true;
	}

	public void Update()
	{
		IODeviceBase[] devices = m_devices;
		for (int i = 0; i < devices.Length; i++)
		{
			devices[i].Update();
		}
	}

	public virtual void PreEnterTestMode()
	{
		IODeviceBase[] devices = m_devices;
		for (int i = 0; i < devices.Length; i++)
		{
			devices[i].PreEnterTestMode();
		}
	}

	public virtual void PostExitTestMode()
	{
		IODeviceBase[] devices = m_devices;
		for (int i = 0; i < devices.Length; i++)
		{
			devices[i].PostExitTestMode();
		}
	}

	public virtual void AllActivate(bool bActive)
	{
		IODeviceBase[] devices = m_devices;
		foreach (IODeviceBase iODeviceBase in devices)
		{
			if (iODeviceBase == m_devices[0])
			{
				iODeviceBase.Activate(bActive && TitleDemoUtility.IsGameEntryAcceptable() && !GameUtility.IsFreePlay());
			}
			else
			{
				iODeviceBase.Activate(bActive);
			}
		}
	}

	public CoinDevice Coin()
	{
		return m_devices[0] as CoinDevice;
	}

	public IkeyDevice Ikey()
	{
		return m_devices[1] as IkeyDevice;
	}
}

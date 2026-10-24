namespace Dx11HighLevelScopeControllerPlugin;

public sealed class PluginSingleton
{
	private static volatile PluginSingleton mInstance;

	private static object syncRoot = new object();

	public PluginAccess gPluginAccess = new PluginAccess();

	public static PluginSingleton Instance
	{
		get
		{
			if (mInstance == null)
			{
				lock (syncRoot)
				{
					if (mInstance == null)
					{
						mInstance = new PluginSingleton();
					}
				}
			}
			return mInstance;
		}
	}

	private PluginSingleton()
	{
	}
}

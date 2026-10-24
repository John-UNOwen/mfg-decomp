namespace Konami.System.Net;

public class NetEnvironment
{
	private static NetEnvironment instance;

	public NetEnvironmentConnection Connection { get; private set; } = NetEnvironmentConnection.Default;


	public static NetEnvironment Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new NetEnvironment();
				instance.Initialize();
			}
			return instance;
		}
	}

	private void Initialize()
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			Connection = NetEnvironmentConnection.Default;
		}
		else
		{
			Connection = NetEnvironmentConnection.DefaultKonamiNet;
		}
	}

	public void SetEnv(NetEnvironmentConnection connection)
	{
		Connection = connection;
	}
}

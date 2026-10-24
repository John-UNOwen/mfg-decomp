using Konami.System.Net;

public class SessionCache : ResponseCacheBase<SessionCache>
{
	public string Session { get; private set; } = "";


	public override string ResponseName => "ssid";

	protected override void Initialize()
	{
		base.Initialize();
		Session = "";
	}

	public override void OnParse(object _obj)
	{
		Session = _obj?.ToString() ?? "";
	}

	public override void OnLateParse()
	{
	}

	public void ClearSession()
	{
		Session = "";
	}

	public bool IsLoggedIn()
	{
		return Session.Length > 0;
	}
}

using System.Collections;

namespace Konami.System.Net;

public abstract class ResponseCacheBase<T> : IResponseCache where T : ResponseCacheBase<T>, new()
{
	public delegate void OnParseEndHandler();

	private static T instance;

	public static T Instance => init();

	public abstract string ResponseName { get; }

	public event OnParseEndHandler OnParseEnd;

	public static void Clean()
	{
		instance = null;
	}

	private static T init()
	{
		if (instance == null)
		{
			instance = new T();
			instance.Initialize();
		}
		return instance;
	}

	protected virtual void Initialize()
	{
	}

	public virtual void Parse(Hashtable response)
	{
		if (response != null && response.ContainsKey(ResponseName))
		{
			OnParse(response[ResponseName]);
			this.OnParseEnd?.Invoke();
		}
	}

	public void LateParse()
	{
		OnLateParse();
	}

	public abstract void OnParse(object _obj);

	public abstract void OnLateParse();
}

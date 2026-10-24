using System;
using System.IO;
using UniRx;
using UnityEngine;

namespace Konami.System.Net;

public class AccountFile : SerializableFile
{
	private static AccountFile instance = null;

	[SerializeField]
	private string playerId = "";

	[SerializeField]
	private string token = "";

	[SerializeField]
	private string device = "";

	private static Subject<(string, string)> onChange = new Subject<(string, string)>();

	[SerializeField]
	private string targetServer = "";

	public static string PlayerIdentifier => Instance.playerId;

	public static string Token => Instance.token;

	public static string DeviceIdentifier => Instance.device;

	private static AccountFile Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new AccountFile();
			}
			return instance;
		}
	}

	public static IObservable<(string id, string token)> OnChange => onChange.AsObservable();

	private string Path => global::System.IO.Path.Combine(Application.persistentDataPath, "account" + targetServer);

	public static void Reset(string player = "", string token = "", string device = "")
	{
		instance = new AccountFile(player, token, device);
	}

	private AccountFile()
		: base(isCrypt: true, isCompress: true)
	{
		targetServer = NetEnvironment.Instance.Connection.Name;
		if (!base.Load(Path))
		{
			Debug.Log("アカウントファイルを作成します");
			base.Save(Path);
		}
	}

	private AccountFile(string player, string token, string device)
		: base(isCrypt: true, isCompress: true)
	{
		targetServer = NetEnvironment.Instance.Connection.Name;
		playerId = player;
		this.token = token;
		this.device = device;
		Debug.Log("アカウントファイルを作成します");
		base.Save(Path);
		onChange.OnNext((playerId, this.token));
	}
}

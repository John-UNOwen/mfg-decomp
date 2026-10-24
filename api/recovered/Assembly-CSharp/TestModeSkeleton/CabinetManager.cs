using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestModeSkeleton;

public class CabinetManager : SingletonBehaviour<CabinetManager>
{
	public static int FileOpenErrorCode = 0;

	private static readonly List<ICabinetSettings> settings = new List<ICabinetSettings> { Calibration, Clock, Sound, Game, Coin, NetWork, Lang, Book, VirtualCoin, GachaTicketLog };

	public static CalibrationSettings Calibration { get; } = new CalibrationSettings();


	public static ClockSettings Clock { get; } = new ClockSettings();


	public static SoundSettings Sound { get; } = new SoundSettings();


	public static GameSettings Game { get; } = new GameSettings();


	public static CoinSettings Coin { get; } = new CoinSettings();


	public static NetworkSettings NetWork { get; } = new NetworkSettings();


	public static LanguageSettings Lang { get; } = new LanguageSettings();


	public static BookKeepingSettings Book { get; } = new BookKeepingSettings();


	public static VirtualCoinSettings VirtualCoin { get; } = new VirtualCoinSettings();


	public static GachaTicketLogSettings GachaTicketLog { get; } = new GachaTicketLogSettings();


	public static void Startup()
	{
		UnityEngine.Object.DontDestroyOnLoad(new GameObject("CabinetManager", typeof(CabinetManager)));
	}

	private void Awake()
	{
		FileOpenErrorCode = 0;
		settings.ForEach(delegate(ICabinetSettings v)
		{
			v.InitBeforeLoad();
		});
		settings.ForEach(delegate(ICabinetSettings v)
		{
			if (!v.Load())
			{
				if (v.GetErrorCode() == -1)
				{
					v.FirstTimeStartup();
				}
				else if (FileOpenErrorCode != 2)
				{
					FileOpenErrorCode = ((!(v.GetLabel() == "book")) ? 1 : 2);
				}
			}
		});
		DateTime time = Clock.GetTime();
		settings.ForEach(delegate(ICabinetSettings v)
		{
			v.InitAfterLoad(time, Clock.Data);
		});
	}

	private void Update()
	{
		DateTime time = Clock.GetTime();
		settings.ForEach(delegate(ICabinetSettings v)
		{
			v.Update(time);
		});
	}

	public static void AllFactoryAndSave()
	{
		settings.ForEach(delegate(ICabinetSettings v)
		{
			v.Factory();
		});
		DateTime time = Clock.GetTime();
		settings.ForEach(delegate(ICabinetSettings v)
		{
			v.Save(time, Coin.Data);
		});
	}

	public static void SuperAllFactoryAndSave()
	{
		settings.ForEach(delegate(ICabinetSettings v)
		{
			v.SuperFactory();
		});
		DateTime time = Clock.GetTime();
		settings.ForEach(delegate(ICabinetSettings v)
		{
			v.Save(time, Coin.Data);
		});
	}
}

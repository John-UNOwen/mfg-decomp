using System;

namespace TestModeSkeleton;

public abstract class CabinetSettingsBase<T> : ICabinetSettings where T : CabinetSaveDataBase, new()
{
	private T data = new T();

	public T Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public abstract string Label { get; }

	public virtual string GetLabel()
	{
		return Label;
	}

	public virtual void InitBeforeLoad()
	{
	}

	public virtual void InitAfterLoad(DateTime time, ClockSettings.SaveData clock)
	{
	}

	public virtual void FirstTimeStartup()
	{
	}

	public virtual void Update(DateTime time)
	{
	}

	public virtual void Factory()
	{
		Data.Default();
	}

	public virtual void SuperFactory()
	{
		Data.Default();
	}

	public virtual bool Save(DateTime time = default(DateTime), CoinSettings.SaveData coin = null)
	{
		return CabinetSerializer.Write(Label, Data);
	}

	public virtual bool Load()
	{
		return CabinetSerializer.Read(Label, ref data);
	}

	public virtual int GetErrorCode()
	{
		return CabinetSerializer.error_code;
	}
}

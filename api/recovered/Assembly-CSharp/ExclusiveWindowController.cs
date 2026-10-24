using System;
using System.Collections.Generic;
using System.Linq;

public class ExclusiveWindowController<KeyType> where KeyType : IComparable<KeyType>
{
	private Dictionary<KeyType, IExclusiveWindow> _windows = new Dictionary<KeyType, IExclusiveWindow>();

	public void Add(KeyType key, IExclusiveWindow window)
	{
		_windows.Add(key, window);
	}

	public void Remove(KeyType key)
	{
		_windows.Remove(key);
	}

	public void Clear()
	{
		_windows.Clear();
	}

	public int Count()
	{
		return _windows.Count();
	}

	public void ExclusiveOpen(KeyType key)
	{
		if (!_windows.TryGetValue(key, out var value))
		{
			return;
		}
		foreach (IExclusiveWindow item in from x in _windows
			where key.CompareTo(x.Key) != 0
			select x.Value)
		{
			item.OnExclusiveClose();
		}
		value.ExclusiveOpen();
	}

	public void CloseAll()
	{
		foreach (IExclusiveWindow value in _windows.Values)
		{
			value.OnExclusiveClose();
		}
	}
}

using System;
using System.Collections.Generic;

namespace Shirahama;

public class CompareSelector<T, TElement> : IEqualityComparer<T>
{
	private readonly Func<T, TElement> _selector;

	public bool Equals(T x, T y)
	{
		return _selector(x).Equals(_selector(y));
	}

	public int GetHashCode(T obj)
	{
		return _selector(obj).GetHashCode();
	}

	public CompareSelector(Func<T, TElement> selector)
	{
		_selector = selector;
	}
}

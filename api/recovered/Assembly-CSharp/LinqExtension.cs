using System;
using System.Collections.Generic;
using System.Linq;

public static class LinqExtension
{
	public static TSource MaxElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
	{
		return source.OrderByDescending(selector).FirstOrDefault();
	}

	public static TSource MinElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
	{
		return source.OrderBy(selector).FirstOrDefault();
	}
}

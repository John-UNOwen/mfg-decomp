using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Shirahama;

public class BundleHashCache
{
	[Serializable]
	public class Element
	{
		public string Key;

		public string Hash;
	}

	private readonly Dictionary<string, string> _hashTable = new Dictionary<string, string>();

	public IEnumerable<Element> Contents
	{
		get
		{
			foreach (KeyValuePair<string, string> item in _hashTable)
			{
				yield return new Element
				{
					Key = item.Key,
					Hash = item.Value
				};
			}
		}
	}

	public string Get(IResourceLocation location)
	{
		string text = location?.PrimaryKey;
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (_hashTable.TryGetValue(text, out var value))
		{
			return value;
		}
		return null;
	}

	public bool Register(IResourceLocation location)
	{
		if (location == null)
		{
			return false;
		}
		if (location.Data is AssetBundleRequestOptions assetBundleRequestOptions)
		{
			_hashTable[location.PrimaryKey] = assetBundleRequestOptions.Hash;
			return true;
		}
		return false;
	}

	public BundleHashCache(IEnumerable<KeyValuePair<string, string>> contents)
	{
		if (contents == null)
		{
			throw new ArgumentNullException("BundleHashCache");
		}
		foreach (KeyValuePair<string, string> content in contents)
		{
			_hashTable[content.Key] = content.Value;
		}
	}

	public BundleHashCache()
	{
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.ResourceManagement;

namespace Shirahama;

public static class BundleHashCacheSupport
{
	[Serializable]
	private class SerialziedHashTable
	{
		public List<BundleHashCache.Element> Contents;
	}

	public static BundleHashCache LoadBundleHashCache(this ResourceManager resourceManager)
	{
		BundleHashCacheLocation @default = BundleHashCacheLocation.Default;
		string path = resourceManager.TransformInternalId(@default);
		try
		{
			Chiper chiper = new Chiper();
			byte[] data = File.ReadAllBytes(path);
			string text = chiper.Decrypt(data);
			if (text == null)
			{
				return null;
			}
			SerialziedHashTable serialziedHashTable = JsonUtility.FromJson<SerialziedHashTable>(text);
			if (serialziedHashTable == null)
			{
				return null;
			}
			return new BundleHashCache(serialziedHashTable.Contents.Select((BundleHashCache.Element e) => KeyValuePair.Create(e.Key, e.Hash)));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	public static bool Save(this ResourceManager resourceManager, BundleHashCache cache)
	{
		if (cache == null)
		{
			return false;
		}
		try
		{
			Chiper chiper = new Chiper();
			BundleHashCacheLocation @default = BundleHashCacheLocation.Default;
			string path = resourceManager.TransformInternalId(@default);
			string serialized = JsonUtility.ToJson(new SerialziedHashTable
			{
				Contents = cache.Contents.ToList()
			});
			File.WriteAllBytes(path, chiper.Encrypt(serialized));
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}
}

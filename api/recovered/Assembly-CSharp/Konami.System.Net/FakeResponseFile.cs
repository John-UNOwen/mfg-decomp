using System;
using System.Collections.Generic;
using UnityEngine;

namespace Konami.System.Net;

public class FakeResponseFile : SerializableFile, ISerializationCallbackReceiver
{
	[Serializable]
	public struct FakeInfo
	{
		public string name;

		public string fake;
	}

	private static FakeResponseFile instance;

	private Dictionary<string, string> fakeDict;

	[SerializeField]
	private List<FakeInfo> fakeList;

	public static FakeResponseFile Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new FakeResponseFile();
				instance.Initialize();
			}
			return instance;
		}
	}

	public Dictionary<string, string> FakeList => fakeDict;

	private FakeResponseFile()
		: base(isCrypt: false, isCompress: false)
	{
	}

	public void Initialize()
	{
		fakeDict = new Dictionary<string, string>();
	}

	public void OnAfterDeserialize()
	{
		fakeDict.Clear();
		foreach (FakeInfo fake in fakeList)
		{
			fakeDict.Add(fake.name, fake.fake);
		}
	}

	public void OnBeforeSerialize()
	{
		fakeList.Clear();
		foreach (KeyValuePair<string, string> item in fakeDict)
		{
			fakeList.Add(new FakeInfo
			{
				name = item.Key,
				fake = item.Value
			});
		}
	}

	public void Add(string _key, ref string _responseText)
	{
		if (fakeDict != null && !fakeDict.ContainsKey(_key))
		{
			fakeDict.Add(_key, _responseText);
			Save();
		}
	}

	public void Save()
	{
		Save("FakeResponse.json");
	}

	public override bool Save(string filename)
	{
		return base.Save(Application.persistentDataPath + "/" + filename);
	}

	public bool Load()
	{
		return Load("FakeResponse.json");
	}

	public bool Load(string filename)
	{
		return base.Load(Application.persistentDataPath + "/" + filename, bAllowNoFile: true);
	}

	public void Delete(string _key)
	{
		if (fakeDict != null)
		{
			fakeDict.Remove(_key);
			Save();
		}
	}

	public void Update(string _key, ref string _responseText)
	{
		if (fakeDict != null)
		{
			if (fakeDict.ContainsKey(_key))
			{
				fakeDict[_key] = _responseText;
				Save();
			}
			else
			{
				Add(_key, ref _responseText);
			}
		}
	}

	public void Search(string _key, ref string _responseText)
	{
		if (fakeDict != null && fakeDict.ContainsKey(_key))
		{
			_responseText = fakeDict[_key];
		}
	}
}

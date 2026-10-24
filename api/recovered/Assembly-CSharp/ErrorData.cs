using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class ErrorData
{
	public class Item
	{
		public ERROR errCode;

		public uint subCode;

		public string title;

		public string message;

		public Item()
		{
		}

		public Item(Item item)
		{
			errCode = item.errCode;
			subCode = item.subCode;
			title = item.title;
			message = item.message;
		}
	}

	private static ErrorData instance;

	private Dictionary<string, Item> m_errorItems = new Dictionary<string, Item>();

	public string ExtraMessage = "";

	public static void Initialize()
	{
		if (instance == null)
		{
			instance = new ErrorData();
			TextAsset textAsset = Resources.Load("ErrorCodeData") as TextAsset;
			instance.LoadErrorData(new TextAsset[1] { textAsset });
		}
	}

	public static ErrorData GetInstance()
	{
		if (instance == null)
		{
			Initialize();
		}
		return instance;
	}

	public void LoadErrorData(TextAsset[] errorDataXml)
	{
		if (errorDataXml == null)
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		for (int i = 0; i < errorDataXml.Length; i++)
		{
			xmlDocument.LoadXml(errorDataXml[i].text);
			foreach (XmlNode item2 in xmlDocument.SelectNodes("error_code/error"))
			{
				Item item = new Item();
				item.errCode = (ERROR)int.Parse(item2.SelectSingleNode("ecode").InnerText);
				string innerText = item2.SelectSingleNode("subcode").InnerText;
				if (string.IsNullOrEmpty(innerText))
				{
					item.subCode = 0u;
				}
				else
				{
					item.subCode = ushort.Parse(innerText);
				}
				item.title = item2.SelectSingleNode("title").InnerText;
				item.message = item2.SelectSingleNode("message").InnerText;
				string key = MakeKey(item.errCode, item.subCode);
				if (m_errorItems.ContainsKey(key))
				{
					m_errorItems[key] = item;
				}
				else
				{
					m_errorItems.Add(key, item);
				}
			}
		}
	}

	public Item GetItem(ERROR errorCode, uint subCode)
	{
		string key = MakeKey(errorCode, subCode);
		if (m_errorItems.ContainsKey(key))
		{
			return m_errorItems[key];
		}
		if (subCode != 0)
		{
			key = MakeKey(errorCode, 0u);
			if (m_errorItems.ContainsKey(key))
			{
				return new Item(m_errorItems[key])
				{
					subCode = subCode
				};
			}
		}
		return new Item
		{
			errCode = errorCode,
			subCode = subCode,
			title = $"5-{(int)errorCode}-{subCode}",
			message = "不明なエラーが発生しました"
		};
	}

	private string MakeKey(ERROR errorCode, uint subCode)
	{
		return $"{(int)errorCode}-{subCode}";
	}
}

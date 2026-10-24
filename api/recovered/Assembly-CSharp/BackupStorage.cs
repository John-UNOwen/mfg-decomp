using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

public class BackupStorage : IDisposable
{
	public enum FORMAT
	{
		XML,
		JSON,
		BINARY,
		CRYPT_XML,
		CRYPT_JSON,
		CRYPT_BINARY
	}

	private class SaveData
	{
		public FORMAT format;

		public string path;

		public object data;

		public bool saving;
	}

	private class MeasureLog : IDisposable
	{
		public MeasureLog(string name)
		{
		}

		public void Dispose()
		{
		}
	}

	private static BackupStorage instance = null;

	private Queue<SaveData> saveDataQueue;

	private Semaphore semaphore;

	private object thisLock;

	private Thread writeThread;

	private static readonly string backupExt = ".bak";

	private static bool m_ignoreSave = false;

	public static void Initialize()
	{
		if (instance == null)
		{
			instance = new BackupStorage();
		}
	}

	public static void Terminate()
	{
		if (instance != null)
		{
			instance = null;
		}
	}

	public static string GetFormatExt(FORMAT format)
	{
		return (new string[6] { ".xml", ".json", ".bin", ".xml", ".json", ".bin" })[(int)format];
	}

	private BackupStorage()
	{
		saveDataQueue = new Queue<SaveData>();
		semaphore = new Semaphore(0, 16);
		thisLock = new object();
		writeThread = new Thread(SaveThread);
		writeThread.Start();
	}

	public void Dispose()
	{
		semaphore.Release();
		writeThread.Join();
	}

	public static int GetQueueSize()
	{
		int result = 0;
		if (instance != null)
		{
			lock (instance.thisLock)
			{
				result = instance.saveDataQueue.Count;
			}
		}
		return result;
	}

	public static bool IsBusy()
	{
		return GetQueueSize() > 0;
	}

	public static void EnableSave(bool flag)
	{
		m_ignoreSave = !flag;
	}

	public static bool IsEnableSave()
	{
		return !m_ignoreSave;
	}

	private bool SaveRequest(SaveData saveData)
	{
		lock (thisLock)
		{
			foreach (SaveData item in saveDataQueue)
			{
				if (item.path == saveData.path && !item.saving)
				{
					item.data = saveData.data;
					return true;
				}
			}
			saveDataQueue.Enqueue(saveData);
			semaphore.Release();
		}
		return true;
	}

	private void SaveThread()
	{
		bool flag = false;
		do
		{
			semaphore.WaitOne();
			SaveData saveData = null;
			lock (thisLock)
			{
				if (saveDataQueue.Count > 0)
				{
					saveData = saveDataQueue.Peek();
					saveData.saving = true;
				}
				else
				{
					flag = true;
				}
			}
			if (saveData != null)
			{
				Save(saveData);
				lock (thisLock)
				{
					saveDataQueue.Dequeue();
				}
			}
		}
		while (!flag);
	}

	private static T LoadImpl<T>(string path, FORMAT format) where T : class
	{
		if (File.Exists(path))
		{
			FileCompare(path, path + backupExt);
		}
		T result = null;
		if (File.Exists(path))
		{
			switch (format)
			{
			case FORMAT.XML:
				try
				{
					using StreamReader streamReader2 = new StreamReader(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
					using (new MeasureLog("XML Deserialize: " + path))
					{
						result = (T)new XmlSerializer(typeof(T)).Deserialize(streamReader2);
					}
					streamReader2.Close();
				}
				catch (Exception message3)
				{
					Debug.Log(message3);
				}
				break;
			case FORMAT.JSON:
				try
				{
					using StreamReader streamReader = new StreamReader(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
					using (new MeasureLog("JSON Deserialize: " + path))
					{
						result = JsonUtility.FromJson<T>(streamReader.ReadToEnd());
					}
					streamReader.Close();
				}
				catch (Exception message2)
				{
					Debug.Log(message2);
				}
				break;
			case FORMAT.BINARY:
				try
				{
					using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					using (new MeasureLog("Binary Deserialize: " + path))
					{
						result = (T)binaryFormatter.Deserialize(fileStream);
					}
					fileStream.Close();
				}
				catch (Exception message)
				{
					Debug.Log(message);
				}
				break;
			}
		}
		return result;
	}

	public static T Load<T>(string path, FORMAT format) where T : class
	{
		T val = LoadImpl<T>(path, format);
		if (val == null)
		{
			val = LoadImpl<T>(path + backupExt, format);
		}
		return val;
	}

	public static bool Save<T>(string path, T dataClone, FORMAT format) where T : class
	{
		if (m_ignoreSave)
		{
			return true;
		}
		bool flag = false;
		SaveData saveData = new SaveData();
		saveData.format = format;
		saveData.path = string.Copy(path);
		saveData.data = dataClone;
		saveData.saving = false;
		if (instance != null)
		{
			return instance.SaveRequest(saveData);
		}
		Debug.Log("BackupStorage not initialized. Blocking Save.");
		return Save(saveData);
	}

	private static bool Save(SaveData saveData)
	{
		bool flag = false;
		switch (saveData.format)
		{
		case FORMAT.XML:
			try
			{
				using StreamWriter streamWriter2 = new StreamWriter(saveData.path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
				XmlSerializer xmlSerializer = new XmlSerializer(saveData.data.GetType());
				using (new MeasureLog("XML Serialize: " + saveData.path))
				{
					xmlSerializer.Serialize(streamWriter2, saveData.data);
				}
				streamWriter2.Flush();
				streamWriter2.Close();
				flag = true;
			}
			catch (Exception message3)
			{
				Debug.Log(message3);
			}
			break;
		case FORMAT.JSON:
			try
			{
				using StreamWriter streamWriter = new StreamWriter(saveData.path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
				string value;
				using (new MeasureLog("JSON Serialize: " + saveData.path))
				{
					value = JsonUtility.ToJson(saveData.data, prettyPrint: true);
				}
				streamWriter.Write(value);
				streamWriter.Flush();
				streamWriter.Close();
				flag = true;
			}
			catch (Exception message2)
			{
				Debug.Log(message2);
			}
			break;
		case FORMAT.BINARY:
			try
			{
				using FileStream fileStream = new FileStream(saveData.path, FileMode.Create, FileAccess.Write, FileShare.Read);
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				using (new MeasureLog("Binary Serialize: " + saveData.path))
				{
					binaryFormatter.Serialize(fileStream, saveData.data);
				}
				fileStream.Flush();
				fileStream.Close();
				flag = true;
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
			break;
		}
		if (flag)
		{
			SyncFile(saveData.path);
			string dst = saveData.path + backupExt;
			try
			{
				Copy(saveData.path, dst);
			}
			catch (Exception message4)
			{
				Debug.Log(message4);
			}
		}
		return flag;
	}

	private static void Copy(string src, string dst)
	{
		using Stream stream = File.OpenRead(src);
		using Stream stream2 = File.OpenWrite(dst);
		byte[] array = new byte[16384];
		for (int num = stream.Read(array, 0, array.Length); num != 0; num = stream.Read(array, 0, array.Length))
		{
			stream2.Write(array, 0, num);
		}
	}

	private static bool FileCompare(string file1, string file2)
	{
		if (file1 == file2)
		{
			return true;
		}
		if (!File.Exists(file1) || !File.Exists(file2))
		{
			return false;
		}
		bool result = false;
		using FileStream fileStream = new FileStream(file1, FileMode.Open, FileAccess.Read, FileShare.Read);
		using (FileStream fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			if (fileStream.Length == fileStream2.Length)
			{
				int num = 0;
				int num2 = 0;
				do
				{
					num = fileStream.ReadByte();
					num2 = fileStream2.ReadByte();
				}
				while (num == num2 && num != -1);
				result = num - num2 == 0;
			}
			fileStream2.Close();
		}
		fileStream.Close();
		return result;
	}

	private static int SyncFile(string file)
	{
		return 0;
	}

	private static int SyncDir(string dir)
	{
		return 0;
	}
}

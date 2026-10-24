using System;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class SerializableFile
{
	public bool IsCrypt { get; set; }

	public bool IsCompress { get; set; }

	public SerializableFile(bool isCrypt, bool isCompress)
	{
		IsCrypt = isCrypt;
		IsCompress = isCompress;
	}

	public virtual bool Load(string path, bool bAllowNoFile = false)
	{
		bool flag = false;
		try
		{
			using Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
			try
			{
				string fileName = Path.GetFileName(path);
				flag = LoadFromMemory(binaryReader.ReadBytes((int)stream.Length), bDecrypt: true, fileName);
				if (!flag && !bAllowNoFile)
				{
					Debug.LogError(path + " のLoadに失敗");
				}
			}
			catch (Exception)
			{
				Debug.LogError("serializableFile load fail: " + path);
				return false;
			}
		}
		catch
		{
			if (bAllowNoFile)
			{
				return true;
			}
			Debug.LogError(path + " のLoadに失敗");
			return false;
		}
		return flag;
	}

	public virtual bool LoadFromMemory(byte[] bytes, bool bDecrypt, string salt)
	{
		if (bytes == null || bytes.Length == 0)
		{
			return false;
		}
		try
		{
			string text = null;
			byte[] array = bytes;
			if (IsCrypt)
			{
				array = Crypto.Decrypt(array, salt, salt);
			}
			if (array == null)
			{
				return false;
			}
			text = ((bytes[0] != 239 || bytes[1] != 187 || bytes[2] != 191) ? Encoding.UTF8.GetString(array) : Encoding.UTF8.GetString(array, 3, array.Length - 3));
			JsonUtility.FromJsonOverwrite(text, this);
			return true;
		}
		catch (Exception)
		{
			Debug.LogError("setting file load failed.");
			return false;
		}
	}

	public virtual bool LoadFromJson(string json)
	{
		if (json == null || json.Length == 0)
		{
			return false;
		}
		bool flag = false;
		try
		{
			JsonUtility.FromJsonOverwrite(json, this);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public virtual bool Save(string path)
	{
		try
		{
			string s = JsonUtility.ToJson(this, prettyPrint: true);
			using (FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				using BinaryWriter binaryWriter = new BinaryWriter(output, Encoding.UTF8);
				byte[] array = Encoding.UTF8.GetBytes(s);
				if (array == null)
				{
					return false;
				}
				if (IsCrypt)
				{
					string fileName = Path.GetFileName(path);
					array = Crypto.Encrypt(array, fileName, fileName);
				}
				binaryWriter.Write(array);
				binaryWriter.Flush();
			}
			return true;
		}
		catch (Exception)
		{
			Debug.LogError("setting file save failed.");
			return false;
		}
	}
}

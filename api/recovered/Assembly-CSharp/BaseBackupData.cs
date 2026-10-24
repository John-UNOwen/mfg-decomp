using System;
using System.IO;
using System.Text;
using KAM.Usefully;
using KAMUNITY;

public abstract class BaseBackupData
{
	protected string m_dirPath = "dev/raw/backup";

	protected JsonSerializableFile m_file = new JsonSerializableFile();

	public bool IsError { get; private set; }

	public bool Load()
	{
		IsError = false;
		if (!Directory.Exists(Path.GetDirectoryName(GetSavePath())))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(GetSavePath()));
		}
		if (!m_file.LoadFromFile(GetSavePath(), bAllowNoFile: true))
		{
			IsError = m_file.LoadFromFile(GetSavePath() + ".bak");
		}
		return IsError;
	}

	public bool Save()
	{
		if (!IsDirty())
		{
			return true;
		}
		bool result = m_file.Save(GetSavePath());
		PostSave();
		return result;
	}

	public void DumpAndSave()
	{
		string path = GetSavePath() + ".dump";
		string s = m_file.Dump();
		using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(output, Encoding.UTF8);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		binaryWriter.Write(bytes);
		binaryWriter.Flush();
	}

	private string GetSavePath()
	{
		return Path.Combine(Utility.GetRawPath(), GetFileName());
	}

	public abstract string GetFileName();

	public abstract void Clear();

	protected abstract bool IsDirty();

	protected abstract void PostSave();

	public virtual void RemoveFile()
	{
		try
		{
			string savePath = GetSavePath();
			if (File.Exists(savePath))
			{
				File.Delete(savePath);
			}
		}
		catch (Exception)
		{
		}
	}
}

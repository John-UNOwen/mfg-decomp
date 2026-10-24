using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Xml;
using Compress;
using Konami.System.Net.Crypt;
using UnityEngine;

public abstract class RedemprionConfigFile
{
	public enum Mode
	{
		Plane,
		Compressed,
		Encrypted,
		DummyHeader
	}

	private static readonly int dummyHeaderSize = 96;

	private string m_Dir;

	private string m_Name;

	private string m_Path;

	private Mode m_Mode;

	public bool IsLoaded { get; private set; }

	public bool IsExistFile { get; private set; } = true;


	protected Mode mode => m_Mode;

	protected virtual string MakePass()
	{
		return m_Name;
	}

	protected virtual byte[] MakeSalt()
	{
		return Konami.System.Net.Crypt.Utils.MakeSalt(m_Name);
	}

	protected RedemprionConfigFile(Mode mode)
	{
		m_Mode = mode;
	}

	protected RedemprionConfigFile(Mode mode, string dir, string name)
	{
		m_Mode = mode;
		m_Dir = dir;
		m_Name = name;
		m_Path = Path.Combine(m_Dir, m_Name);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("UNITY_STANDALONE")]
	protected static void GetLocalSettingName(ref string name, string element)
	{
		try
		{
			XmlNode xmlNode = Read(Mode.Plane, Application.dataPath, "../localsettings.xml", "", null).SelectSingleNode("localsettings/" + element);
			if (xmlNode != null)
			{
				name = xmlNode.InnerText;
			}
		}
		catch (FileNotFoundException)
		{
		}
		catch (IsolatedStorageException)
		{
		}
		catch (XmlException)
		{
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
		}
	}

	protected virtual bool Load(string dir, string name)
	{
		m_Dir = dir;
		m_Name = name;
		m_Path = Path.Combine(m_Dir, name);
		try
		{
			XmlDocument xmlDocument = Read(m_Mode, m_Dir, m_Name, MakePass(), MakeSalt());
			if (xmlDocument != null && Parse(xmlDocument))
			{
				IsLoaded = true;
			}
		}
		catch (IsolatedStorageException)
		{
		}
		catch (FileNotFoundException)
		{
			IsExistFile = false;
		}
		catch (Exception)
		{
			LoadBackup();
		}
		return IsLoaded;
	}

	protected virtual bool Load(ref byte[] data, string dir, string name)
	{
		m_Dir = dir;
		m_Name = name;
		m_Path = Path.Combine(m_Dir, name);
		try
		{
			XmlDocument xml = Read(ref data, m_Mode, MakePass(), MakeSalt());
			if (Parse(xml))
			{
				IsLoaded = true;
			}
		}
		catch (IsolatedStorageException)
		{
		}
		catch (FileNotFoundException)
		{
		}
		catch (XmlException)
		{
		}
		catch (IOException)
		{
		}
		finally
		{
		}
		return IsLoaded;
	}

	public void Save()
	{
		try
		{
			if (IsDirty())
			{
				XmlDocument xmlDocument = new XmlDocument();
				XmlDeclaration newChild = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
				XmlNode newChild2 = MakeXmlNode(xmlDocument);
				xmlDocument.AppendChild(newChild);
				xmlDocument.AppendChild(newChild2);
				Write(m_Path, m_Mode, xmlDocument, MakePass(), MakeSalt());
				PostSave();
			}
		}
		catch (Exception)
		{
		}
	}

	protected virtual bool LoadBackup()
	{
		try
		{
			XmlDocument xmlDocument = Read(m_Mode, m_Dir, m_Name + ".bak", MakePass(), MakeSalt());
			if (xmlDocument != null && Parse(xmlDocument))
			{
				IsLoaded = true;
			}
		}
		catch (IsolatedStorageException)
		{
		}
		catch (FileNotFoundException)
		{
		}
		catch (Exception)
		{
		}
		finally
		{
		}
		return IsLoaded;
	}

	public static XmlDocument Read(Mode mode, string dir, string name, string pass, byte[] salt)
	{
		byte[] data;
		using (Stream stream = new FileStream(Path.Combine(dir, name), FileMode.Open, FileAccess.Read))
		{
			data = new byte[stream.Length];
			if (stream.Read(data, 0, (int)stream.Length) != (int)stream.Length)
			{
				throw new Exception("file length not equal");
			}
		}
		return Read(ref data, mode, pass, salt);
	}

	public static XmlDocument Read(ref byte[] data, Mode mode, string pass, byte[] salt)
	{
		byte[] bytes = data;
		switch (mode)
		{
		case Mode.DummyHeader:
		{
			bytes = Konami.System.Net.Crypt.Utils.DecryptAes(data, 0, pass, salt);
			int num = bytes.Length - dummyHeaderSize;
			byte[] array = new byte[num];
			Array.Copy(bytes, dummyHeaderSize, array, 0, num);
			bytes = Compress.Utils.DecodeGZip(array);
			break;
		}
		case Mode.Encrypted:
			bytes = Konami.System.Net.Crypt.Utils.DecryptAes(data, 0, pass, salt);
			bytes = Compress.Utils.DecodeGZip(bytes);
			break;
		case Mode.Compressed:
			bytes = Compress.Utils.DecodeGZip(data);
			break;
		}
		string @string = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetString(bytes);
		XmlDocument xmlDocument = new XmlDocument();
		StringReader txtReader = new StringReader(@string);
		xmlDocument.Load(txtReader);
		return xmlDocument;
	}

	public static void CreateBackup(string path)
	{
		File.Delete(path + ".bak");
		try
		{
			File.Move(path, path + ".bak");
		}
		catch (IOException)
		{
		}
	}

	public static void Write(string path, Mode mode, XmlDocument xml, string pass, byte[] salt, bool bDump = true)
	{
		switch (mode)
		{
		case Mode.Plane:
		{
			using Stream stream4 = new FileStream(path, FileMode.Create, FileAccess.Write);
			using StreamWriter writer = new StreamWriter(stream4, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			xml.Save(writer);
			break;
		}
		case Mode.Compressed:
		{
			using StringWriter stringWriter3 = new StringWriter();
			xml.Save(stringWriter3);
			byte[] array4 = Compress.Utils.EncodeGZip(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(stringWriter3.ToString()));
			using Stream stream3 = new FileStream(path, FileMode.Create, FileAccess.Write);
			stream3.Write(array4, 0, array4.Length);
			break;
		}
		case Mode.Encrypted:
		{
			using StringWriter stringWriter2 = new StringWriter();
			xml.Save(stringWriter2);
			byte[] array3 = Konami.System.Net.Crypt.Utils.EncryptAes(Compress.Utils.EncodeGZip(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(stringWriter2.ToString())), pass, salt);
			using Stream stream2 = new FileStream(path, FileMode.Create, FileAccess.Write);
			stream2.Write(array3, 0, array3.Length);
			break;
		}
		case Mode.DummyHeader:
		{
			using StringWriter stringWriter = new StringWriter();
			xml.Save(stringWriter);
			byte[] array = Compress.Utils.EncodeGZip(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(stringWriter.ToString()));
			byte[] data = new byte[array.Length + dummyHeaderSize];
			MakeDummyData(ref data, 0, dummyHeaderSize);
			Array.Copy(array, 0, data, dummyHeaderSize, array.Length);
			byte[] array2 = Konami.System.Net.Crypt.Utils.EncryptAes(data, pass, salt);
			using Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
			stream.Write(array2, 0, array2.Length);
			break;
		}
		}
	}

	private static void MakeDummyData(ref byte[] data, int offset, int length)
	{
		System.Random random = new System.Random();
		for (int i = 0; i < length; i++)
		{
			int num = offset + i;
			data[num] = (byte)random.Next(0, 256);
		}
	}

	protected T ReadXmlNode<T>(XmlNode node, string path)
	{
		return (T)Convert.ChangeType((node.SelectSingleNode(path) ?? throw new Exception()).InnerText, typeof(T));
	}

	protected T ReadXmlNodeDefault<T>(XmlNode node, string path, T default_value)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode == null)
		{
			return default_value;
		}
		return (T)Convert.ChangeType(xmlNode.InnerText, typeof(T));
	}

	protected bool TryReadXmlNode<T>(ref T dst, XmlNode node, string path)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode != null)
		{
			dst = (T)Convert.ChangeType(xmlNode.InnerText, typeof(T));
			return true;
		}
		return false;
	}

	protected T ReadXmlNodeEnum<T>(XmlNode node, string path)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode == null)
		{
			throw new Exception();
		}
		return (T)Enum.Parse(typeof(T), xmlNode.InnerText);
	}

	protected T ReadXmlNodeEnumDefault<T>(XmlNode node, string path, T default_value)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode == null)
		{
			return default_value;
		}
		return (T)Enum.Parse(typeof(T), xmlNode.InnerText);
	}

	protected bool TryReadXmlNodeEnum<T>(ref T dst, XmlNode node, string path)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode != null)
		{
			dst = (T)Enum.Parse(typeof(T), xmlNode.InnerText);
			return true;
		}
		return false;
	}

	protected abstract bool IsDirty();

	protected abstract void PostSave();

	protected abstract bool Parse(XmlDocument xml);

	protected abstract XmlNode MakeXmlNode(XmlDocument xml);
}

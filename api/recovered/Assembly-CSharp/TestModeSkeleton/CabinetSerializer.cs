using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using KAMUNITY;

namespace TestModeSkeleton;

public static class CabinetSerializer
{
	public class EncodingStringWriter : StringWriter
	{
		public override Encoding Encoding { get; }

		public EncodingStringWriter(StringBuilder sb, Encoding encoding)
			: base(sb)
		{
			Encoding = encoding;
		}
	}

	public static int error_code;

	public static bool Write<T>(string label, T data) where T : class
	{
		int num = Avs.FsOpenW(MakeFilePath(label));
		if (!Avs.IsFsOpenSuccess(num))
		{
			Logger.LogWarning("ファイルオープンエラー:" + MakeFilePath(label));
			return false;
		}
		byte[] array = Serialize(data);
		int num2 = Avs.FsWrite(num, array, array.Length);
		Avs.FsClose(num);
		if (num2 < 0)
		{
			Logger.LogWarning("ファイル書き込みエラー:" + MakeFilePath(label));
			return false;
		}
		return true;
	}

	public static bool Read<T>(string label, ref T data) where T : class
	{
		error_code = 0;
		int num = Avs.FsOpenR(MakeFilePath(label));
		if (!Avs.IsFsOpenSuccess(num))
		{
			error_code = -1;
			return false;
		}
		long num2 = Avs.FsFsize(num);
		if (num2 < 0)
		{
			error_code = 1;
			Avs.FsClose(num);
			return false;
		}
		byte[] array = new byte[num2];
		int num3 = Avs.FsRead(num, array, array.Length);
		Avs.FsClose(num);
		if (num3 < 0)
		{
			error_code = 2;
			return false;
		}
		data = Deserialize<T>(array, num3);
		return true;
	}

	private static byte[] Serialize<T>(T data) where T : class
	{
		EncodingStringWriter encodingStringWriter = new EncodingStringWriter(new StringBuilder(), Encoding.UTF8);
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Indent = true
		};
		using (XmlWriter xmlWriter = XmlWriter.Create(encodingStringWriter, settings))
		{
			XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
			xmlSerializerNamespaces.Add(string.Empty, string.Empty);
			new XmlSerializer(data.GetType()).Serialize(xmlWriter, data, xmlSerializerNamespaces);
		}
		return Encoding.UTF8.GetBytes(encodingStringWriter.ToString());
	}

	private static T Deserialize<T>(byte[] bytes, int count) where T : class
	{
		using XmlReader xmlReader = XmlReader.Create(new StringReader(Encoding.UTF8.GetString(bytes, 0, count)));
		return new XmlSerializer(typeof(T)).Deserialize(xmlReader) as T;
	}

	private static string MakeFilePath(string label)
	{
		return "/dev/nvram/cb_" + label + ".xml";
	}
}

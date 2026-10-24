using System.IO;

namespace KAMUNITY;

public static class UploaderUtil
{
	public static bool ValidateStream(Stream stream, XrpcModuleUploaderTypes.FindFilesResponse_Content content)
	{
		return ValidateStream(stream, content.contentSize, content.contentHash);
	}

	public static bool ValidateStream(Stream stream, XrpcModuleCcUploaderTypes.FindFilesResponse_Content content)
	{
		return ValidateStream(stream, content.contentSize, content.contentHash);
	}

	public static bool ValidateStream(Stream stream, long contentSize, string contentHash)
	{
		bool result = false;
		if (contentSize != stream.Length)
		{
			Logger.LogWarningFormat("different length {0} : {1}", contentSize, stream.Length);
		}
		else
		{
			long position = stream.Position;
			stream.Position = 0L;
			string text = Utility.MakeHashSHA256(stream);
			stream.Position = position;
			if (text.CompareTo(contentHash.ToUpper()) != 0)
			{
				Logger.LogWarningFormat("different hash {0} : {1}", contentHash, text);
			}
			else
			{
				result = true;
			}
		}
		return result;
	}
}

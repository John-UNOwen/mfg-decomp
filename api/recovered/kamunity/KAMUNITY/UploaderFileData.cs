using System.IO;

namespace KAMUNITY;

public class UploaderFileData
{
	public byte[] data;

	public XrpcModuleUploaderTypes.DeclareUploadRequest_Content content = new XrpcModuleUploaderTypes.DeclareUploadRequest_Content();

	public UploaderFileData(byte[] _data, string _relativePath, string _contentKey, string _contentType)
	{
		data = _data;
		content.relativePath = _relativePath;
		content.contentHash = Utility.MakeHashSHA256(_data);
		content.contentKey = _contentKey;
		content.contentType = _contentType;
		content.contentSize = _data.Length;
	}

	public UploaderFileData(FileStream stream, string _relativePath, string _contentKey, string _contentType)
	{
		data = new byte[stream.Length];
		stream.Read(data, 0, (int)stream.Length);
		content.relativePath = _relativePath;
		content.contentHash = Utility.MakeHashSHA256(data);
		content.contentKey = _contentKey;
		content.contentType = _contentType;
		content.contentSize = data.Length;
	}

	public UploaderFileData(string path, string _relativePath, string _contentKey, string _contentType)
	{
		using (FileStream fileStream = new FileStream(path, FileMode.Open))
		{
			data = new byte[fileStream.Length];
			fileStream.Read(data, 0, (int)fileStream.Length);
		}
		content.relativePath = _relativePath;
		content.contentHash = Utility.MakeHashSHA256(data);
		content.contentKey = _contentKey;
		content.contentType = _contentType;
		content.contentSize = data.Length;
	}
}

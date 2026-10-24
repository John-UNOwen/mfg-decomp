namespace Shirahama;

public struct DownloadProgress
{
	public long DownloadedSize { get; }

	public long TotalSize { get; }

	public float Progress => (float)DownloadedSize / (float)TotalSize;

	public DownloadProgress(long totalSize, long downloadedSize)
	{
		TotalSize = totalSize;
		DownloadedSize = downloadedSize;
	}
}

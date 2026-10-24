using System.IO;

public class FileDeleteUtility
{
	public static void DeleteDirectory(string target_dir)
	{
		if (Directory.Exists(target_dir))
		{
			string[] files = Directory.GetFiles(target_dir);
			foreach (string path in files)
			{
				File.SetAttributes(path, FileAttributes.Normal);
				File.Delete(path);
			}
			files = Directory.GetDirectories(target_dir);
			for (int i = 0; i < files.Length; i++)
			{
				DeleteDirectory(files[i]);
			}
			Directory.Delete(target_dir, recursive: false);
		}
	}
}

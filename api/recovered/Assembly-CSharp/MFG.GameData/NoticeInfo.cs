using System;
using System.Globalization;
using System.IO;
using Cysharp.Threading.Tasks;
using KAMUNITY;
using MFG.Assets;
using MFG.Scenes.CommonSystem;
using MFG.Types.Notice;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class NoticeInfo : IAssetPath
{
	public int NoticeID;

	public Category Category;

	public string Title;

	public string Text;

	public string ReleaseDate;

	public string UpdateDate;

	public bool IsNew;

	public bool IsHomeDisplay;

	public string HomeBannerName;

	public string HomeDisplayEndDate;

	[NonSerialized]
	public Sprite NoticeImage;

	public string Imageurl = "";

	[NonSerialized]
	private const int DDS_HEADER_SIZE = 128;

	public string AddressableAssetPath => AssetPathResolver.ResolveHomeBannerAssets(HomeBannerName);

	public string IEndDateString => GetEndDateString();

	public string GetEndDateString()
	{
		if (HomeDisplayEndDate != null && !string.IsNullOrEmpty(HomeDisplayEndDate))
		{
			return DateTimeOffsetManager.GetJSTFromString(HomeDisplayEndDate).ToString("yyyy年MM月dd日（ddd）HH：mmまで", new CultureInfo("ja-JP"));
		}
		return "";
	}

	public bool IsDisplayHomeBanner()
	{
		if (IsHomeDisplay)
		{
			return DateTimeOffsetManager.GetJSTFromString(HomeDisplayEndDate) >= DateTimeOffsetManager.GetJSTTime();
		}
		return false;
	}

	public bool IsForcedDisplayNotice()
	{
		if (string.IsNullOrEmpty(HomeDisplayEndDate))
		{
			return false;
		}
		return DateTimeOffsetManager.GetJSTFromString(HomeDisplayEndDate) >= DateTimeOffsetManager.GetJSTTime();
	}

	public async UniTask DownloadTexture2DNotice()
	{
		string text = "Local_";
		if (Imageurl.Contains(text))
		{
			string path = AssetPathResolver.ResolveNoticeImage(Imageurl.Replace(text, ""));
			if (await AssetLoader.Exists(path))
			{
				NoticeImage = (await AssetLoader.LoadAssetAsync<Sprite>(path)).Asset;
			}
		}
	}

	private static Texture2D Download(string DownloadUrl)
	{
		MemoryStream memoryStream = new MemoryStream(1048576);
		HttpDownloader httpDownloader = new HttpDownloader();
		Debug.Log("download : " + DownloadUrl);
		httpDownloader.SetRequest(new Uri(DownloadUrl), memoryStream);
		httpDownloader.Execute();
		return LoadDds(memoryStream.ToArray(), flip_y: true);
	}

	private static uint ReadUint32(ref byte[] data, int offset)
	{
		int num = offset * 4;
		return (uint)((data[num + 3] << 24) + (data[num + 2] << 16) + (data[num + 1] << 8) + data[num]);
	}

	private static int ReadInt32(ref byte[] data, int offset)
	{
		int num = offset * 4;
		return (data[num + 3] << 24) + (data[num + 2] << 16) + (data[num + 1] << 8) + data[num];
	}

	public static Texture2D LoadDds(byte[] data, bool flip_y)
	{
		uint num = ReadUint32(ref data, 0);
		if (num != 542327876)
		{
			throw new Exception($"magic unmatch {num}");
		}
		int num2 = ReadInt32(ref data, 1);
		if (num2 != 124)
		{
			throw new Exception($"header_size unmatch {num2}");
		}
		if ((ReadUint32(ref data, 2) & 0x20000u) != 0)
		{
			ReadInt32(ref data, 7);
			_ = 1;
		}
		char c = (char)data[84];
		string text = c.ToString();
		c = (char)data[85];
		string text2 = c.ToString();
		c = (char)data[86];
		string text3 = c.ToString();
		c = (char)data[87];
		string text4 = text + text2 + text3 + c;
		TextureFormat textureFormat;
		if (text4.CompareTo("DXT1") == 0)
		{
			textureFormat = TextureFormat.DXT1;
		}
		else
		{
			if (text4.CompareTo("DXT5") != 0)
			{
				throw new Exception("not support format " + text4);
			}
			textureFormat = TextureFormat.DXT5;
		}
		int num3 = ReadInt32(ref data, 3);
		int num4 = ReadInt32(ref data, 4);
		byte[] array = new byte[data.Length - 128];
		if (flip_y)
		{
			switch (textureFormat)
			{
			case TextureFormat.DXT1:
				array = FlipY_DXT1(ref data, num4, num3);
				break;
			case TextureFormat.DXT5:
				array = FlipY_DXT5(ref data, num4, num3);
				break;
			}
		}
		else
		{
			int num5 = textureFormat switch
			{
				TextureFormat.DXT1 => num4 * num3 / 2, 
				TextureFormat.DXT5 => num4 * num3, 
				_ => throw new Exception(), 
			};
			array = new byte[num5];
			Buffer.BlockCopy(data, 128, array, 0, num5);
		}
		Texture2D texture2D = new Texture2D(num4, num3, textureFormat, mipChain: false);
		texture2D.LoadRawTextureData(array);
		texture2D.Apply();
		return texture2D;
	}

	private static byte[] FlipY_DXT1(ref byte[] data, int width, int height)
	{
		int num = 8;
		int num2 = width / 4;
		int num3 = num2 * num;
		int num4 = height / 4;
		byte[] array = new byte[num3 * num4];
		for (int i = 0; i < num4; i++)
		{
			int num5 = (num4 - i - 1) * num3 + 128;
			int num6 = i * num3;
			for (int j = 0; j < num2; j++)
			{
				int num7 = num5 + j * num;
				int num8 = num6 + j * num;
				array[num8] = data[num7];
				array[num8 + 1] = data[num7 + 1];
				array[num8 + 2] = data[num7 + 2];
				array[num8 + 3] = data[num7 + 3];
				array[num8 + 4] = data[num7 + 7];
				array[num8 + 5] = data[num7 + 6];
				array[num8 + 6] = data[num7 + 5];
				array[num8 + 7] = data[num7 + 4];
			}
		}
		return array;
	}

	private static byte[] FlipY_DXT5(ref byte[] data, int width, int height)
	{
		int num = 16;
		int num2 = width / 4;
		int num3 = num2 * num;
		int num4 = height / 4;
		byte[] array = new byte[num3 * num4];
		for (int i = 0; i < num4; i++)
		{
			int num5 = (num4 - i - 1) * num3 + 128;
			int num6 = i * num3;
			for (int j = 0; j < num2; j++)
			{
				int num7 = num5 + j * num;
				int num8 = num6 + j * num;
				array[num8] = data[num7];
				array[num8 + 1] = data[num7 + 1];
				byte num9 = data[num7 + 2];
				byte b = data[num7 + 3];
				byte b2 = data[num7 + 4];
				byte b3 = data[num7 + 5];
				byte b4 = data[num7 + 6];
				byte b5 = data[num7 + 7];
				int num10 = num9 & 7;
				int num11 = (num9 >> 3) & 7;
				int num12 = ((num9 >> 6) & 3) + ((b << 2) & 4);
				int num13 = (b >> 1) & 7;
				int num14 = (b >> 4) & 7;
				int num15 = ((b >> 7) & 1) + ((b2 << 1) & 6);
				int num16 = (b2 >> 2) & 7;
				int num17 = (b2 >> 5) & 7;
				int num18 = b3 & 7;
				int num19 = (b3 >> 3) & 7;
				int num20 = ((b3 >> 6) & 3) + ((b4 << 2) & 4);
				int num21 = (b4 >> 1) & 7;
				int num22 = (b4 >> 4) & 7;
				int num23 = ((b4 >> 7) & 1) + ((b5 << 1) & 6);
				int num24 = (b5 >> 2) & 7;
				int num25 = (b5 >> 5) & 7;
				int num26 = num22 + (num23 << 3) + ((num24 & 3) << 6);
				int num27 = ((num24 & 4) >> 2) + (num25 << 1) + (num18 << 4) + ((num19 & 1) << 7);
				int num28 = ((num19 & 6) >> 1) + (num20 << 2) + (num21 << 5);
				int num29 = num14 + (num15 << 3) + ((num16 & 3) << 6);
				int num30 = ((num16 & 4) >> 2) + (num17 << 1) + (num10 << 4) + ((num11 & 1) << 7);
				int num31 = ((num11 & 6) >> 1) + (num12 << 2) + (num13 << 5);
				array[num8 + 2] = (byte)num26;
				array[num8 + 3] = (byte)num27;
				array[num8 + 4] = (byte)num28;
				array[num8 + 5] = (byte)num29;
				array[num8 + 6] = (byte)num30;
				array[num8 + 7] = (byte)num31;
				array[num8 + 8] = data[num7 + 8];
				array[num8 + 9] = data[num7 + 9];
				array[num8 + 10] = data[num7 + 10];
				array[num8 + 11] = data[num7 + 11];
				array[num8 + 12] = data[num7 + 15];
				array[num8 + 13] = data[num7 + 14];
				array[num8 + 14] = data[num7 + 13];
				array[num8 + 15] = data[num7 + 12];
			}
		}
		return array;
	}
}

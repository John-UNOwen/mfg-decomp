using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Shirahama;

[DisplayName("Encrypted AssetBundle Provider")]
public class EncryptedAssetProvider : ResourceProviderBase
{
	private readonly Dictionary<string, LoadedAssetBundle> _cache = new Dictionary<string, LoadedAssetBundle>();

	private readonly Dictionary<string, int> _referenceCounters = new Dictionary<string, int>();

	public ILogger Logger { get; set; }

	public override void Provide(ProvideHandle provideHandle)
	{
		try
		{
			AssetBundleRequestOptions options = provideHandle.Location.Data as AssetBundleRequestOptions;
			IResourceLocation location = provideHandle.Location;
			string text = provideHandle.ResourceManager.TransformInternalId(location);
			string bundleName = Path.GetFileNameWithoutExtension(text);
			LocationType locationType = provideHandle.ResourceManager.GetLocationType(location);
			Logger?.Log("[EncryptedAssetProvider] Convert bundle path " + text);
			CachedAssetBundleLocation location2 = new CachedAssetBundleLocation(location);
			string text2 = provideHandle.ResourceManager.TransformInternalId(location2);
			string text3 = ((locationType != LocationType.Remote) ? text : text2);
			string localPath = text3;
			Logger?.Log(string.Format("[{0}] Provide {1}({2}/{3} byte) key: {4} type: {5}/{6}", "EncryptedAssetProvider", bundleName, localPath, options.BundleSize, location.PrimaryKey, location.ResourceType, location.GetType()));
			if (_cache.TryGetValue(bundleName, out var value))
			{
				Logger?.Log("[EncryptedAssetProvider] Hit cache (" + bundleName + ")");
				_referenceCounters[bundleName] += 1;
				provideHandle.Complete(value, status: true, null);
				return;
			}
			if (!File.Exists(localPath))
			{
				Logger?.Log("[EncryptedAssetProvider] Download " + text);
				UnityWebRequest request = UnityWebRequest.Get(text);
				provideHandle.ResourceManager.WebRequestOverride?.Invoke(request);
				UnityWebRequestAsyncOperation download = request.SendWebRequest();
				provideHandle.SetDownloadProgressCallbacks(delegate
				{
					_ = download.progress;
					long result;
					long totalBytes = (long.TryParse(request.GetResponseHeader("Content-Length"), out result) ? result : options.BundleSize);
					DownloadStatus result2 = default(DownloadStatus);
					result2.DownloadedBytes = (long)request.downloadedBytes;
					result2.TotalBytes = totalBytes;
					result2.IsDone = request.isDone;
					return result2;
				});
				download.completed += delegate
				{
					if (request.result != UnityWebRequest.Result.Success)
					{
						Logger?.Log(LogType.Error, string.Format("[{0}] Failed to download {1} with {2}({3}/{4})", "EncryptedAssetProvider", request.url, request.result, (int)request.result, request.responseCode));
						provideHandle.Complete<LoadedAssetBundle>(null, status: false, null);
						request.Dispose();
						return;
					}
					byte[] data = request.downloadHandler.data;
					try
					{
						string directoryName = Path.GetDirectoryName(localPath);
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						Logger?.Log("[EncryptedAssetProvider] Write download data to " + localPath);
						File.WriteAllBytes(localPath, data);
						Logger?.Log("[EncryptedAssetProvider] Create asset bundle cache from " + localPath);
						using MemoryStream baseStream2 = new MemoryStream(data);
						using AssetBundleCryptoStream assetBundleCryptoStream2 = new AssetBundleCryptoStream(baseStream2, bundleName, bundleName);
						using BinaryReader binaryReader2 = new BinaryReader(assetBundleCryptoStream2, Encoding.UTF8);
						byte[] binary2 = binaryReader2.ReadBytes((int)assetBundleCryptoStream2.Length);
						AssetBundleCreateRequest operation2 = AssetBundle.LoadFromMemoryAsync(binary2, options.Crc);
						operation2.completed += delegate
						{
							if (operation2.assetBundle == null)
							{
								Logger?.Log(LogType.Warning, "[EncryptedAssetProvider] Failed to create asset bundle from RAM (" + bundleName + ")");
								provideHandle.Complete<LoadedAssetBundle>(null, status: false, null);
								request.Dispose();
							}
							else
							{
								Logger?.Log("[EncryptedAssetProvider] Done creating asset bundle from RAM (" + bundleName + ")");
								LoadedAssetBundle loadedAssetBundle2 = new LoadedAssetBundle(operation2.assetBundle, bundleName);
								_cache.Add(bundleName, loadedAssetBundle2);
								if (_referenceCounters.TryGetValue(bundleName, out var value3))
								{
									_referenceCounters[bundleName] = value3 + 1;
								}
								else
								{
									_referenceCounters[bundleName] = 1;
								}
								provideHandle.Complete(loadedAssetBundle2, status: true, null);
								request.Dispose();
							}
						};
					}
					catch (Exception exception2)
					{
						provideHandle.Complete<LoadedAssetBundle>(null, status: false, exception2);
						request.Dispose();
					}
				};
				return;
			}
			Logger?.Log("[EncryptedAssetProvider] Create asset bundle cache from " + localPath);
			using FileStream baseStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
			using AssetBundleCryptoStream assetBundleCryptoStream = new AssetBundleCryptoStream(baseStream, bundleName, bundleName);
			using BinaryReader binaryReader = new BinaryReader(assetBundleCryptoStream, Encoding.UTF8);
			byte[] binary = binaryReader.ReadBytes((int)assetBundleCryptoStream.Length);
			AssetBundleCreateRequest operation = AssetBundle.LoadFromMemoryAsync(binary, options.Crc);
			provideHandle.SetProgressCallback(() => operation.progress);
			operation.completed += delegate
			{
				if (operation.assetBundle == null)
				{
					Logger?.Log(LogType.Warning, "[EncryptedAssetProvider] Failed to create asset bundle from " + localPath);
					provideHandle.Complete<LoadedAssetBundle>(null, status: false, null);
				}
				else
				{
					LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle(operation.assetBundle, bundleName);
					Logger?.Log("[EncryptedAssetProvider] Done creating asset bundle from " + localPath);
					_cache.Add(bundleName, loadedAssetBundle);
					if (_referenceCounters.TryGetValue(bundleName, out var value2))
					{
						_referenceCounters[bundleName] = value2 + 1;
					}
					else
					{
						_referenceCounters[bundleName] = 1;
					}
					provideHandle.Complete(loadedAssetBundle, status: true, null);
				}
			};
		}
		catch (Exception exception)
		{
			provideHandle.Complete<LoadedAssetBundle>(null, status: false, exception);
		}
	}

	public override void Release(IResourceLocation location, object asset)
	{
		if (location == null)
		{
			throw new ArgumentNullException("location");
		}
		if (asset == null || !(asset is LoadedAssetBundle { BundleName: var bundleName } loadedAssetBundle))
		{
			return;
		}
		Logger?.Log("[EncryptedAssetProvider] Check reference counter on " + bundleName);
		if (_referenceCounters.TryGetValue(bundleName, out var value))
		{
			if (value > 1)
			{
				_referenceCounters[bundleName] = value - 1;
				return;
			}
			_referenceCounters.Remove(bundleName);
		}
		Logger?.Log("[EncryptedAssetProvider] Release " + bundleName);
		_cache.Remove(loadedAssetBundle.BundleName);
		loadedAssetBundle.Dispose();
	}
}

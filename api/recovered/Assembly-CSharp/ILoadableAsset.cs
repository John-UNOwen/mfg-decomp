using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface ILoadableAsset : IDisposable
{
	bool IsLoadCalled { get; }

	bool IsLoading { get; }

	UniTask Load(string assetAddress, CancellationToken cancellationToken = default(CancellationToken));
}

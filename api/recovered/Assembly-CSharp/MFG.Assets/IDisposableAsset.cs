using System;
using UnityEngine;

namespace MFG.Assets;

public interface IDisposableAsset<AssetType> : IDisposable where AssetType : UnityEngine.Object
{
	AssetType Asset { get; }
}

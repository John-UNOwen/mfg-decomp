using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFG.Assets;

public interface IDisposableAssetSet<TKey, AssetType> : IDictionary<TKey, AssetType>, ICollection<KeyValuePair<TKey, AssetType>>, IEnumerable<KeyValuePair<TKey, AssetType>>, IEnumerable, IDisposable where AssetType : UnityEngine.Object
{
}

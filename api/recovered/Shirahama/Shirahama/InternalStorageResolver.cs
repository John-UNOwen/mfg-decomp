using UnityEngine;

namespace Shirahama;

public static class InternalStorageResolver
{
	public static string ContentStoragePath => Application.persistentDataPath;
}

// Probe-build compat shim (not recovered source).
//
// The shipped Mono runtime (Unity 2022.3 Mono fork) carries the .NET Standard 2.1
// helper System.Collections.Generic.KeyValuePair.Create<TKey,TValue>; Microsoft's
// net472 reference assemblies (the pack profile) do not (CS0305 at the call site).
// The recovered source genuinely calls KeyValuePair.Create, so instead of editing it
// we shadow the name inside namespace Shirahama with an internal static class
// exposing the same Create. Members of the enclosing namespace win
// over types imported via `using System.Collections.Generic;`, so
// `KeyValuePair.Create(...)` in Shirahama.BundleHashCacheSupport binds here, while
// `System.Collections.Generic.KeyValuePair<TKey,TValue>` (fully qualified or
// declared in other namespaces) keeps resolving to the BCL struct unchanged.
namespace Shirahama
{
	internal static class KeyValuePair
	{
		public static global::System.Collections.Generic.KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
		{
			return new global::System.Collections.Generic.KeyValuePair<TKey, TValue>(key, value);
		}
	}
}

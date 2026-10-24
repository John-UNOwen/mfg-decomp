// Probe-build compat shim (not recovered source).
//
// System.MathF is a .NET Core 2.1+ BCL struct shipped in Unity's Mono runtime fork;
// Microsoft's net472 reference assemblies (the pack profile) do not define it, so
// `MathF.PI` / `MathF.E` in the recovered source fail with CS0117. The shipped and
// netstandard profiles reference Mono's real System.MathF and exclude this file.
// The shim mirrors the runtime shape that the recovered code relies on: static
// readonly float fields (not const), so the compiler still emits the same
// runtime float32 load/multiply/divide as the shipped assembly instead of
// folding the expression with double intermediates.
internal static class MathF
{
	public static readonly float PI = (float)global::System.Math.PI;
	public static readonly float E = (float)global::System.Math.E;
}

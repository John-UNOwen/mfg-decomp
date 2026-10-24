using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[CompilerGenerated]
[EditorBrowsable(EditorBrowsableState.Never)]
[GeneratedCode("Unity.MonoScriptGenerator.MonoScriptInfoGenerator", null)]
internal class UnitySourceGeneratedAssemblyMonoScriptTypes_v1
{
	private struct MonoScriptData
	{
		public byte[] FilePathsData;

		public byte[] TypesData;

		public int TotalTypes;

		public int TotalFiles;

		public bool IsEditorOnly;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static MonoScriptData Get()
	{
		MonoScriptData result = default(MonoScriptData);
		result.FilePathsData = new byte[58]
		{
			0, 0, 0, 2, 0, 0, 0, 50, 92, 65,
			115, 115, 101, 116, 115, 92, 80, 108, 117, 103,
			105, 110, 115, 92, 119, 105, 110, 49, 48, 97,
			99, 116, 108, 111, 103, 92, 82, 117, 110, 116,
			105, 109, 101, 92, 87, 105, 110, 49, 48, 65,
			99, 116, 76, 111, 103, 46, 99, 115
		};
		result.TypesData = new byte[56]
		{
			0, 0, 0, 0, 15, 75, 65, 77, 124, 87,
			105, 110, 49, 48, 65, 99, 116, 76, 111, 103,
			0, 0, 0, 0, 31, 75, 65, 77, 46, 87,
			105, 110, 49, 48, 65, 99, 116, 76, 111, 103,
			124, 65, 67, 84, 73, 86, 65, 84, 73, 79,
			78, 95, 73, 78, 70, 79
		};
		result.TotalFiles = 1;
		result.TotalTypes = 2;
		result.IsEditorOnly = false;
		return result;
	}
}

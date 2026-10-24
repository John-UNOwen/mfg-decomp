using System.Runtime.InteropServices;

namespace KAMUNITY;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct PACKAGE_INFO
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	private char[] _name;

	public int type;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	private char[] _sum_type;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	private char[] _sum;

	public uint size;

	public int progress;

	public string name => new string(_name).TrimEnd('\0');

	public string sum_type => new string(_sum_type).TrimEnd('\0');

	public string sum => new string(_sum).TrimEnd('\0');

	public EA3_PKGLIST_TYPE pkg_type => (EA3_PKGLIST_TYPE)type;

	public bool isProgressFinish => progress == 65535;
}

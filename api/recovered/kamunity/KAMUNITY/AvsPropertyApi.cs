using System;
using System.Runtime.InteropServices;
using System.Text;

namespace KAMUNITY;

[KamunityDllApiClass]
public static class AvsPropertyApi
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertyTotalSize([In] int nr_node, [In] int bin_size);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate IntPtr FUNC_PropertyCreate([In] uint flag, [In][Out] byte[] buf, [In] uint sz_buf);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void FUNC_PropertyDestroy([In] IntPtr prop);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate uint FUNC_PropertyGetError([In] IntPtr prop);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate uint FUNC_PropertyQuerySize([In] IntPtr prop);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertyDataQueryMemSize([In] byte[] xmldata, [In] int sz_data, out uint nr_node, out uint bin_size);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate uint FUNC_PropertySetFlag([In] IntPtr prop, [In] uint flag, [In] uint mask);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertySetBool([In] IntPtr prop, [In] IntPtr parent, [In] string path, [In][MarshalAs(UnmanagedType.U1)] bool value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertySetS32([In] IntPtr prop, [In] IntPtr parent, [In] string path, [In] int value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertySet2S32([In] IntPtr prop, [In] IntPtr parent, [In] string path, [In] int[] values);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertySetU32([In] IntPtr prop, [In] IntPtr parent, [In] string path, [In] uint value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertySetS64([In] IntPtr prop, [In] IntPtr parent, [In] string path, [In] long value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertySetU64([In] IntPtr prop, [In] IntPtr parent, [In] string path, [In] ulong value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate int FUNC_PropertySetString([In] IntPtr prop, [In] IntPtr parent, [In] string path, [In][MarshalAs(UnmanagedType.LPStr)] string value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate int FUNC_PropertySetAttribute([In] IntPtr prop, [In] IntPtr parent, [In] string path, [In][MarshalAs(UnmanagedType.LPStr)] string value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertySetBinary([In] IntPtr prop, [In] IntPtr parent, [In] string path, [In] byte[] buf, [In] int sz_buf);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertyGetBool([In] IntPtr prop, [In] IntPtr parent, [In] string path, [MarshalAs(UnmanagedType.U1)] out bool value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertyGetS32([In] IntPtr prop, [In] IntPtr parent, [In] string path, out int value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertyGet2S32([In] IntPtr prop, [In] IntPtr parent, [In] string path, [Out] int[] value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertyGetU32([In] IntPtr prop, [In] IntPtr parent, [In] string path, out uint value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertyGetS64([In] IntPtr prop, [In] IntPtr parent, [In] string path, out long value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertyGetU64([In] IntPtr prop, [In] IntPtr parent, [In] string path, out ulong value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate int FUNC_PropertyGetString([In] IntPtr prop, [In] IntPtr parent, [In] string path, [Out] StringBuilder s, [In] int sz_buf);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate int FUNC_PropertyGetAttribute([In] IntPtr prop, [In] IntPtr parent, [In] string path, [Out] StringBuilder s, [In] int sz_buf);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int FUNC_PropertyGetBinary([In] IntPtr prop, [In] IntPtr parent, [In] string path, [Out] byte[] value, [In] int sz_buf);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate IntPtr FUNC_PropertyNodeCreateVoid([In] IntPtr prop, [In] IntPtr parent, [In] string path);

	public delegate IntPtr FUNC_PropertyNodeRemove([In] IntPtr node);

	public delegate IntPtr FUNC_PropertyNodeClone([In] IntPtr prop, [In] IntPtr parent, [In] IntPtr src, [In][MarshalAs(UnmanagedType.U1)] bool deep);

	public delegate IntPtr FUNC_PropertySearch([In] IntPtr prop, [In] IntPtr parent, [In] string path);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate IntPtr FUNC_PropertyNodeTraversal([In] IntPtr node, [In] uint direct);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate IntPtr FUNC_PropertyNodeGetDesc([In] IntPtr node);

	public delegate IntPtr FUNC_PropertyMemRead([In] byte[] xmldata, [In] int sz_data, [In] int mode, [In][Out] byte[] buf, [In] int sz_buf);

	public delegate int FUNC_PropertyMemWrite([In] IntPtr prop, [In][Out] byte[] buf, [In] int sz_buf);

	public static FUNC_PropertyTotalSize PropertyTotalSize { get; private set; }

	public static FUNC_PropertyCreate PropertyCreate { get; private set; }

	public static FUNC_PropertyDestroy PropertyDestroy { get; private set; }

	public static FUNC_PropertyGetError PropertyGetError { get; private set; }

	public static FUNC_PropertyQuerySize PropertyQuerySize { get; private set; }

	public static FUNC_PropertyDataQueryMemSize PropertyDataQueryMemSize { get; private set; }

	public static FUNC_PropertySetFlag PropertySetFlag { get; private set; }

	public static FUNC_PropertySetBool PropertySetBool { get; private set; }

	public static FUNC_PropertySetS32 PropertySetS32 { get; private set; }

	public static FUNC_PropertySet2S32 PropertySet2S32 { get; private set; }

	public static FUNC_PropertySetU32 PropertySetU32 { get; private set; }

	public static FUNC_PropertySetS64 PropertySetS64 { get; private set; }

	public static FUNC_PropertySetU64 PropertySetU64 { get; private set; }

	public static FUNC_PropertySetString PropertySetString { get; private set; }

	public static FUNC_PropertySetAttribute PropertySetAttribute { get; private set; }

	public static FUNC_PropertySetBinary PropertySetBinary { get; private set; }

	public static FUNC_PropertyGetBool PropertyGetBool { get; private set; }

	public static FUNC_PropertyGetS32 PropertyGetS32 { get; private set; }

	public static FUNC_PropertyGet2S32 PropertyGet2S32 { get; private set; }

	public static FUNC_PropertyGetU32 PropertyGetU32 { get; private set; }

	public static FUNC_PropertyGetS64 PropertyGetS64 { get; private set; }

	public static FUNC_PropertyGetU64 PropertyGetU64 { get; private set; }

	public static FUNC_PropertyGetString PropertyGetString { get; private set; }

	public static FUNC_PropertyGetAttribute PropertyGetAttribute { get; private set; }

	public static FUNC_PropertyGetBinary PropertyGetBinary { get; private set; }

	public static FUNC_PropertyNodeCreateVoid PropertyNodeCreateVoid { get; private set; }

	public static FUNC_PropertyNodeRemove PropertyNodeRemove { get; private set; }

	public static FUNC_PropertyNodeClone PropertyNodeClone { get; private set; }

	public static FUNC_PropertySearch PropertySearch { get; private set; }

	public static FUNC_PropertyNodeTraversal PropertyNodeTraversal { get; private set; }

	public static FUNC_PropertyNodeGetDesc PropertyNodeGetDesc { get; private set; }

	public static FUNC_PropertyMemRead PropertyMemRead { get; private set; }

	public static FUNC_PropertyMemWrite PropertyMemWrite { get; private set; }
}

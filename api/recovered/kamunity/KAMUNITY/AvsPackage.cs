using System.Runtime.InteropServices;

namespace KAMUNITY;

[KamunityDllApiClass]
public static class AvsPackage
{
	public delegate void FUNC_AvsEa3PkglistUpdateList();

	public delegate int FUNC_AvsEa3PkglistGetCount();

	public delegate int FUNC_AvsEa3PkglistGet([Out] PACKAGE_INFO[] dst, [In] int offset, [In] int count);

	public delegate int FUNC_AvsEa3PkglistFileCopy([In] string name, [In] string dst);

	public delegate void FUNC_AvsPackageGetStatus(out int is_exists, out int is_finish, out int progress);

	public static FUNC_AvsEa3PkglistUpdateList AvsEa3PkglistUpdateList { get; private set; }

	public static FUNC_AvsEa3PkglistGetCount AvsEa3PkglistGetCount { get; private set; }

	public static FUNC_AvsEa3PkglistGet AvsEa3PkglistGet { get; private set; }

	public static FUNC_AvsEa3PkglistFileCopy AvsEa3PkglistFileCopy { get; private set; }

	public static FUNC_AvsPackageGetStatus AvsPackageGetStatus { get; private set; }
}

using System.Runtime.InteropServices;

namespace KAMUNITY.external;

[KamunityDllApiClass]
public class DistributeLoader
{
	[KamunityDllApiClass]
	internal static class InternalMethod
	{
		public delegate int FUNC_DistributeLoaderGetCheckIsValid(string filePath);

		public static FUNC_DistributeLoaderGetCheckIsValid DistributeLoaderGetCheckIsValid { get; private set; }
	}

	public delegate void FUNC_DistributeLoaderInitialize([In] string pszLocalDistributeDir, [In] string keyWord, [In] string useDrive);

	public delegate void FUNC_DistributeLoaderInitializeEx([In] string pszLocalDistributeDir, [In] string keyWord, [In] string useDrive, [In] int filePathSizeMax, [In] int driveMaxSize, [In] int driveClearRetryMax, [In] int allFileCountMax, [In] int fileBufSize);

	public delegate void FUNC_DistributeLoaderFinalize();

	public delegate void FUNC_DistributeLoaderTick();

	public static FUNC_DistributeLoaderInitialize DistributeLoaderInitialize { get; private set; }

	public static FUNC_DistributeLoaderInitializeEx DistributeLoaderInitializeEx { get; private set; }

	public static FUNC_DistributeLoaderFinalize DistributeLoaderFinalize { get; private set; }

	public static FUNC_DistributeLoaderTick DistributeLoaderTick { get; private set; }

	public static bool DistributeLoaderGetCheckIsValid(string filePath)
	{
		return InternalMethod.DistributeLoaderGetCheckIsValid(filePath) != 0;
	}
}

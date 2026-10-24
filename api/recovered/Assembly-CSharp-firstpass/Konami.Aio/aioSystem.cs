using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioSystem
{
	[DllImport("libaio")]
	public static extern void aioSystem_Initialize();

	[DllImport("libaio")]
	public static extern void aioSystem_Finalize();

	public static void SystemInitialize()
	{
		aioSystem_Initialize();
	}

	public static void SystemFinalize()
	{
		aioSystem_Finalize();
	}
}

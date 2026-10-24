using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioSciComm : aioSci
{
	[DllImport("libaio")]
	public unsafe static extern AIO_SCI_COMM* aioSciComm_OpenB8PNS1(uint i_Port, uint i_BaudRate);

	public unsafe aioSciComm(AIO_SCI_COMM* i_pSciComm)
		: base((AIO_SCI*)i_pSciComm)
	{
	}

	public unsafe static aioSciComm OpenB8PNS1(uint i_Port, uint i_BaudRate)
	{
		AIO_SCI_COMM* ptr = aioSciComm_OpenB8PNS1(i_Port, i_BaudRate);
		if (ptr == null)
		{
			return null;
		}
		return new aioSciComm(ptr);
	}
}

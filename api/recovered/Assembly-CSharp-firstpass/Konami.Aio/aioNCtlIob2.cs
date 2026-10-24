namespace Konami.Aio;

public class aioNCtlIob2 : aioNCtlIob
{
	public unsafe aioNCtlIob2(AIO_NCTL_IOB2* i_pNodeCtl)
		: base((AIO_NCTL_IOB*)i_pNodeCtl)
	{
	}
}

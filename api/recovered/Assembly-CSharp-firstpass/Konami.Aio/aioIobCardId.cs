using System.Runtime.InteropServices;

namespace Konami.Aio;

public class aioIobCardId
{
	public enum CONSTVAL
	{
		SIZEOF_CARDID = 10
	}

	public enum CARDTYPE
	{
		CT_UNKNOWN = 255,
		CT_NONE = 0,
		CT_ISO15693 = 1,
		CT_FeliCa = 2,
		CT_Mifare = 131,
		CT_eAM_PASS = 1
	}

	public struct CARDID
	{
		public byte CardType;

		public byte cbCardId;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		public byte[] CardId;
	}
}

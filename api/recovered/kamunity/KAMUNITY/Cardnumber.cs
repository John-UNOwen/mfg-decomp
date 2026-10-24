using System;
using System.Text;

namespace KAMUNITY;

public static class Cardnumber
{
	public enum CARDNUMBER_GENERATE
	{
		SUCCEED = 0,
		EBADARG = 1,
		EBADKIND = 2,
		EBADID = 3,
		TEST = 99
	}

	[KamunityDllApiClass]
	internal static class InternalMethod
	{
		public delegate CARDNUMBER_GENERATE FUNC_ConvertCardNumberToUserCode(AvsTypes.CardType cardtype, string cardid, IntPtr dst, int dstlen);

		public static FUNC_ConvertCardNumberToUserCode ConvertCardNumberToUserCode { get; private set; }
	}

	public static CARDNUMBER_GENERATE ConvertCardNumberToUserCode(AvsTypes.CardType cardtype, string cardid, StringBuilder dst, int dstlen)
	{
		CARDNUMBER_GENERATE cARDNUMBER_GENERATE;
		using (MarshalHeap marshalHeap = new MarshalHeap(2 * dstlen))
		{
			cARDNUMBER_GENERATE = InternalMethod.ConvertCardNumberToUserCode(cardtype, cardid, marshalHeap.buffer, dstlen);
			if (cARDNUMBER_GENERATE == CARDNUMBER_GENERATE.SUCCEED)
			{
				dst.Append(marshalHeap.ToStringAnsi());
			}
			else
			{
				Logger.LogWarningFormat("ConvertCardNumberToUserCode error :{0} cardtype:{1} cardid:{2}", cARDNUMBER_GENERATE, cardtype, cardid);
			}
		}
		return cARDNUMBER_GENERATE;
	}
}

using System;

namespace KAMUNITY;

public class XrpcModuleCardmngTypes
{
	public class CardmngInquireResponse
	{
		public string refid;

		public string dataid;

		public bool binded;

		public bool expired;

		public bool ecflag;

		public bool useridflag;

		public bool extidflag;

		public bool quickec;

		public DateTime lastupdate;
	}

	public class CardmngGetrefidResponse
	{
		public string refid;

		public string dataid;
	}

	public class CardmngBindmodelResponse
	{
		public string dataid;
	}

	public class CardmngGetdatalistResponse
	{
	}
}

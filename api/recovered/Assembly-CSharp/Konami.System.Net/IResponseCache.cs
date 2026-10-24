using System.Collections;

namespace Konami.System.Net;

public interface IResponseCache
{
	void Parse(Hashtable response);

	void LateParse();
}

using System;

namespace MFG.GameData;

[Serializable]
public class CardConnectInfo
{
	public int idx;

	public string kind;

	public string param;

	public string name;

	public CardConnectInfo(int _idx, string _kind, string _param, string _name)
	{
		idx = _idx;
		kind = _kind;
		param = _param;
		name = _name;
	}
}

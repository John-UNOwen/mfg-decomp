using System;
using System.Collections.Generic;

[Serializable]
public class PlayerLoginInfo
{
	public List<ComebackCampaignInfo> ComebackCampaignInfos = new List<ComebackCampaignInfo>();

	public List<SerialCodeInfo> SerialCodeInfos = new List<SerialCodeInfo>();
}

using System;

[Serializable]
public class ComebackCampaignInfo
{
	public GameEventType EventType;

	public string NewbeeReferenceDate;

	public bool IsNewbee;

	public string ComebackReferenceDateStart;

	public string ComebackReferenceDateEnd;

	public bool IsComeback;

	public bool IsEnable
	{
		get
		{
			if (!IsNewbee)
			{
				return IsComeback;
			}
			return true;
		}
	}
}

using System;

[Serializable]
public class TouchPanelTelemetry
{
	public int ReadCount;

	public int BeginCount;

	public int EndCount;

	public int ErrorCount;

	public void Reset()
	{
		ReadCount = 0;
		BeginCount = 0;
		EndCount = 0;
		ErrorCount = 0;
	}

	public TouchPanelTelemetry Clone()
	{
		return (TouchPanelTelemetry)MemberwiseClone();
	}

	public static TouchPanelTelemetry Diff(TouchPanelTelemetry after, TouchPanelTelemetry before)
	{
		return new TouchPanelTelemetry
		{
			ReadCount = after.ReadCount - before.ReadCount,
			BeginCount = after.BeginCount - before.BeginCount,
			EndCount = after.EndCount - before.EndCount,
			ErrorCount = after.ErrorCount - before.ErrorCount
		};
	}

	public void Add(TouchPanelTelemetry telemetry)
	{
		ReadCount += telemetry.ReadCount;
		BeginCount += telemetry.BeginCount;
		EndCount += telemetry.EndCount;
		ErrorCount += telemetry.ErrorCount;
	}

	public string DebugLogString()
	{
		return $"ReadCount:{ReadCount}, BeginCount:{BeginCount}, EndCount:{EndCount}, ErrorCount:{ErrorCount}";
	}
}

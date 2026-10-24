using System;

namespace KAMUNITY.AVS;

[Serializable]
public struct ErrorCode
{
	public int ecode;

	public ErrorSect sect => GetSect(ecode);

	public ErrorSubCode subCode => GetSubCode(ecode);

	public bool isFailedNz => ecode < 0;

	public bool isFailed => ecode <= 0;

	public bool isSuccess => ecode > 0;

	public ErrorCode(int _ecode)
	{
		ecode = _ecode;
	}

	public static implicit operator ErrorCode(int _ecode)
	{
		return new ErrorCode(_ecode);
	}

	public override string ToString()
	{
		return ecode.ToString("X08") + " (" + sect.ToString() + "," + subCode.ToString() + ")";
	}

	public static ErrorSect GetSect(int ecode)
	{
		return (ErrorSect)((ecode >> 16) & 0x7FFF);
	}

	public static ErrorSubCode GetSubCode(int ecode)
	{
		return (ErrorSubCode)(ecode & 0xFFFF);
	}
}

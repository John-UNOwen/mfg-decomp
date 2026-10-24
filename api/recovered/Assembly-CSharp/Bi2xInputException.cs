using System;

public class Bi2xInputException : Exception
{
	public Bi2xInput.ECODE Code { get; private set; }

	public int State { get; private set; }

	public Bi2xInputException(Bi2xInput.ECODE code)
	{
		Code = code;
	}

	public Bi2xInputException(Bi2xInput.ECODE code, int state)
	{
		Code = code;
		State = state;
	}
}

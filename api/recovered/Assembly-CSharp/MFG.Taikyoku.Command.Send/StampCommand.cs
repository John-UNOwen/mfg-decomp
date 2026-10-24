namespace MFG.Taikyoku.Command.Send;

public class StampCommand
{
	public enum CommandType
	{
		Receive,
		SendAndReceive
	}

	public CommandType m_commandType;

	public int RoomID = -1;

	public int SeqID;

	public string ObjectID = "OID_TEST";

	public string PlayerName = "";
}

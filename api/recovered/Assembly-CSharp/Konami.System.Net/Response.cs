namespace Konami.System.Net;

public class Response
{
	public StatusCode Result { get; private set; }

	public string Message { get; private set; }

	public string Contents { get; private set; }

	public Response(string content, StatusCode code, string message)
	{
		Result = code;
		Message = message;
		Contents = content;
	}

	public static explicit operator Response(RequestParameter rhs)
	{
		if (rhs.StatusCode.IsSuccess() && rhs.ResponseText == null)
		{
			return new Response("", StatusCode.NotImplemented, "");
		}
		return new Response(rhs.ResponseText, rhs.StatusCode, rhs.Error);
	}
}
public class Response<Content>
{
	public StatusCode Result { get; private set; }

	public string Message { get; private set; }

	public Content Contents { get; private set; }

	public Response(Content content, StatusCode code, string message)
	{
		Result = code;
		Contents = content;
		Message = message;
	}

	public Response(Content content, Response response)
		: this(content, response.Result, response.Message)
	{
	}

	public static implicit operator Response<Content>(Response rhs)
	{
		return new Response<Content>(default(Content), rhs);
	}

	public static implicit operator Response<Content>((Content, Response) rhs)
	{
		return new Response<Content>(rhs.Item1, rhs.Item2);
	}
}

namespace Konami.System.Net;

public static class UserImplementsClass
{
	public static IUserImplementsClass Instance { get; set; } = new UserImplementsClassDefault();

}

namespace BonfireServer.Internal.Context.User;

public class GetSelfUserContext : IBaseContext
{
    public string? Token { get; set; }
}
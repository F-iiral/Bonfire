using System.Text.Json.Serialization;

namespace BonfireServer.Internal.Context.Account;

public class GetSelfUserContext : IBaseContext
{
    [JsonIgnore] public string? Token { get; set; }
}
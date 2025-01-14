using System.Text.Json.Serialization;

namespace BonfireServer.Internal.Context;

public interface IBaseContext
{
    [JsonIgnore] public string? Token { get; set; }
}
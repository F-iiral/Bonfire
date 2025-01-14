using System.Text;
using System.Text.Json;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Content.Account;
using BonfireServer.Internal.Context.Account;

namespace BonfireServer.Internal.Paths.User;

public class GetSelfUserPath : BasePath
{
    public override string Method { get; set; } = MethodTypes.Get; 

    public override ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : default
    {
        if (!IsValid(msg, rawCtx) || rawCtx is not GetSelfUserContext ctx)
            return InvalidMessage(msg);
        if (!IsAuthorized(msg, rawCtx))
            return UnauthorizedTokenMessage(msg);
        
        var user = Database.Database.FindUserByToken(ctx.Token!);
        
        if (user == null)
            return UnprocessableMessage(msg);

        var data = new SelfUserContent(user);
        
        msg.Response.StatusCode = StatusCodes.Ok;
        msg.Response.ContentType = "application/json";
        msg.Response.ContentEncoding = Encoding.UTF8;
        msg.Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, JsonOptions));
        return msg;
    }
}
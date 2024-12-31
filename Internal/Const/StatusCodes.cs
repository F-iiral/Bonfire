namespace BonfireServer.Internal.Const;

public static class StatusCodes
{
    public const int Ok = 200;
    public const int Created = 201;
    public const int Accepted = 202;
    public const int NoContent = 204;
    
    public const int NotModified = 304;
    
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int MethodNotAllowed = 405;
    public const int RequestTimeout = 408;
    public const int Conflict = 409;
    public const int Gone = 410;
    public const int ImATeapot = 418;
    public const int UnprocessableContent = 422;
    
    public const int InternalServerError = 500;
    public const int NotImplemented = 501;
}
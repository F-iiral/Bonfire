namespace BonfireServer.Internal.Const;

public static class UserStatusCodes
{
    public const int Offline = 0;
    public const int Online = 1;
    public const int Idle = 2;
    
    public static bool IsVisible(int userId) => userId != Offline;
}
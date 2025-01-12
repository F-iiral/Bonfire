namespace BonfireServer.Internal.Const;

public class UserFlags
{
    public const int None = 0;
    public const int GlobalStaff = 1 << 0;
    public const int Contributor = 1 << 1;
    public const int Supporter = 1 << 2;
    
    public static bool IsGlobalStaff(int userFlags) => (userFlags & GlobalStaff) != 0;
    public static bool IsContributor(int userFlags) => (userFlags & Contributor) != 0;
    public static bool IsSupporter(int userFlags) => (userFlags & Supporter) != 0;

    public static bool HasAll(int userFlags, int requiredFlags) => (userFlags & requiredFlags) == requiredFlags;
    public static bool HasAny(int userFlags, int requiredFlags) => (userFlags & requiredFlags) != 0;
}
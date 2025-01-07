using System.Security.Cryptography;
using System.Text;
using DotNetEnv;

namespace BonfireServer.Internal;

[Serializable]
public readonly record struct AuthToken
{
    public readonly string Val;

    public AuthToken(LiteFlakeId id) => Val = AuthTokeGenerator.GenerateToken(id);
    public AuthToken(string token) => Val = token;
    
    public static explicit operator string(AuthToken id) => id.Val;
    
    public static bool operator ==(AuthToken a, string b) => a.Val == b;
    public static bool operator !=(AuthToken a, string b) => !(a == b);

    public static bool operator ==(string a, AuthToken b) => a == b.Val;
    public static bool operator !=(string a, AuthToken b) => !(a == b);
}

file static class AuthTokeGenerator
{
    private static HMACSHA512 Secret;

    static AuthTokeGenerator()
    {
        Env.Load();
        var secretKey = Env.GetString("BONFIRE_SECRET_KEY");

        if (secretKey == null)
            throw new Exception("BONFIRE_SECRET_KEY environment variable is not set");
        
        Secret = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
    }
    
    public static string GenerateToken(LiteFlakeId id)
    {
        var id64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(id.Val.ToString()));
        var timestamp64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(DateTime.Now.ToBinary().ToString()));
        var signature64 = Secret.ComputeHash(Encoding.UTF8.GetBytes(id64)).ToString();

        return $"{id64}.{timestamp64}.{signature64}";       // ToDo: Find out why the timestamp doesnt save??
    }
}
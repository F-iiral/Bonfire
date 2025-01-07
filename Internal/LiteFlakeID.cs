namespace BonfireServer.Internal;

[Serializable]
public readonly record struct LiteFlakeId
{
    public readonly long Val = LiteFlakeIdGenerator.GenerateId();

    public LiteFlakeId() {}
    public LiteFlakeId(long val) => Val = val;
    
    public DateTime GetTimestamp()
    {
        return new DateTime((Val >> 16) + LiteFlakeIdGenerator.Epoch);
    }
    
    public static explicit operator long(LiteFlakeId id) => id.Val;
    
    public static bool operator >=(LiteFlakeId a, LiteFlakeId b) => a.Val >= b.Val;
    public static bool operator <=(LiteFlakeId a, LiteFlakeId b) => a.Val <= b.Val;
    public static bool operator >(LiteFlakeId a, LiteFlakeId b)  => a.Val > b.Val;
    public static bool operator <(LiteFlakeId a, LiteFlakeId b)  => a.Val < b.Val;
    
    public static bool operator ==(LiteFlakeId a, long b) => a.Val == b;
    public static bool operator !=(LiteFlakeId a, long b) => !(a == b);
    public static bool operator >=(LiteFlakeId a, long b) => a.Val >= b;
    public static bool operator <=(LiteFlakeId a, long b) => a.Val <= b;
    public static bool operator >(LiteFlakeId a, long b)  => a.Val > b;
    public static bool operator <(LiteFlakeId a, long b) => a.Val < b;
    
    public static bool operator ==(long a, LiteFlakeId b) => a == b.Val;
    public static bool operator !=(long a, LiteFlakeId b) => !(a == b);
    public static bool operator >=(long a, LiteFlakeId b) => a >= b.Val;
    public static bool operator <=(long a, LiteFlakeId b) => a <= b.Val;
    public static bool operator >(long a, LiteFlakeId b)  => a > b.Val;
    public static bool operator <(long a, LiteFlakeId b)  => a < b.Val;
}

file static class LiteFlakeIdGenerator
{
    public const long Epoch = 1672531200;
    private static long LastTimestamp = 0;
    private static long LastSequence = 0;
    private static readonly ushort MaxSequence = ushort.MaxValue;

    public static long GenerateId()
    {
        var currentTimestamp = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()) - Epoch;
        var currentSequence = LastSequence;

        if (currentTimestamp == LastTimestamp)
            currentSequence = (LastSequence + 1) & MaxSequence;

        var uniqueId = (currentTimestamp << 16) | currentSequence;

        LastTimestamp = currentTimestamp;
        LastSequence = currentSequence;

        return uniqueId;
    }
}

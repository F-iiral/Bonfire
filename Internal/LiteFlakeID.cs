namespace BonfireServer.Internal;
using System;

public class LiteFlakeId
{
    public readonly long Val = LiteFlakeIdGenerator.GenerateId();

    public LiteFlakeId() {}
    public LiteFlakeId(long val) => Val = val;
    
    public DateTime GetTimestamp()
    {
        return new DateTime((Val >> 8) + LiteFlakeIdGenerator.Epoch);
    }
}

file static class LiteFlakeIdGenerator
{
    public const long Epoch = 1672531200;
    private static long LastTimestamp = 0;
    private static long LastSequence = 0;
    private static byte MaxSequence = 255;

    public static long GenerateId()
    {
        var currentTimestamp = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()) - Epoch;
        var currentSequence = LastSequence;

        if (currentTimestamp == LastTimestamp)
            currentSequence = (LastSequence + 1) & MaxSequence;

        var uniqueId = (currentTimestamp << 8) | currentSequence;

        LastTimestamp = currentTimestamp;
        LastSequence = currentSequence;

        return uniqueId;
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace BonfireServer.Internal.Converters;

public class LiteFlakeIdJsonConverter : JsonConverter<LiteFlakeId>
{
    public override LiteFlakeId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        long value = reader.GetInt64();
        return new LiteFlakeId(value);
    }

    public override void Write(Utf8JsonWriter writer, LiteFlakeId value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Val);
    }
}
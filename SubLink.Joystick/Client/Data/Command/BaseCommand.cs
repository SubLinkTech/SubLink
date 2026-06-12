using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
public class JsonInterfaceConverterAttribute(Type converterType) : JsonConverterAttribute(converterType) { }

public class IBaseCommandConverter : JsonConverter<IBaseCommand> {
    public override IBaseCommand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, IBaseCommand value, JsonSerializerOptions options) {
        switch (value) {
            case null: {
                JsonSerializer.Serialize(writer, (IBaseCommand?)null, options);
                break;
            }
            default: {
                Type type = value.GetType();
                JsonSerializer.Serialize(writer, value, type, options);
                break;
            }
        }
    }
}

[JsonInterfaceConverter(typeof(IBaseCommandConverter))]
public interface IBaseCommand { }

public abstract class BaseCommand : IBaseCommand {
    [JsonIgnore]
    internal readonly static JsonSerializerOptions _options = new() {
        IgnoreReadOnlyFields = false,
        IgnoreReadOnlyProperties = false,
        AllowTrailingCommas = false,
        PropertyNameCaseInsensitive = false,
        IncludeFields = true,
        WriteIndented = false
    };

    [JsonPropertyName("command")]
    public string Command { get => GetCommand; }

    [JsonPropertyName("identifier")]
    public string Identifier { get => SerializeIdent; }

    [JsonPropertyName("data")]
    public string Data { get => SerializeData(); }

    [JsonIgnore]
    internal virtual string GetCommand => "message";

    [JsonIgnore]
    internal virtual string SerializeIdent => "{\"channel\":\"GatewayChannel\"}";

    internal abstract string SerializeData();
}

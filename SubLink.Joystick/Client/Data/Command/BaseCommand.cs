using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

public interface IBaseCommand {
    string Command { get; }
    string Identifier { get; }
}

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
    public virtual string Command { get; } = string.Empty;
    [JsonPropertyName("identifier")]
    public string Identifier { get => SerializeIdent; }
    [JsonPropertyName("data")]
    public string Data { get => SerializeData(); }

    internal virtual string SerializeIdent =>
        "{\"channel\":\"GatewayChannel\"}";

    internal abstract string SerializeData();
}

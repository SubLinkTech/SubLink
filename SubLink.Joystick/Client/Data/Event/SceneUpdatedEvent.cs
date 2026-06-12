using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Event;

internal class SceneUpdatedEvent : BaseMessage {
    public class ConfigType {
        [JsonPropertyName("fontSize")]
        public string FontSize { get; private set; } = string.Empty;

        [JsonPropertyName("titleColor")]
        public string TitleColor { get; private set; } = string.Empty;

        [JsonPropertyName("progressColor")]
        public string ProgressColor { get; private set; } = string.Empty;

        [JsonPropertyName("completedColor")]
        public string CompletedColor { get; private set; } = string.Empty;
    }

    public class MetadataType {
        [JsonPropertyName("name")]
        public string Name { get; private set; } = string.Empty;

        [JsonPropertyName("config")]
        public ConfigType Config { get; private set; } = new();
    }

    [JsonIgnore]
    public MetadataType Metadata { get; private set; } = new();

    internal override void SetMetadata() {
        Metadata = JsonSerializer.Deserialize<MetadataType>(_metadataStr, JoystickClient._deserializationOpt) ?? new();
    }

    internal override void GetMetadata() {
        _metadataStr = JsonSerializer.Serialize(Metadata, JoystickClient._serializationOpt);
    }
}

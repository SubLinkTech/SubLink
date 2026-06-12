using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Event;

internal class ChatTimerStartedEvent : BaseMessage {
    public class MetadataType {
        [JsonPropertyName("name")]
        public string Name { get; private set; } = string.Empty;

        [JsonPropertyName("endsAt")]
        public string EndsAt { get; private set; } = string.Empty;
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

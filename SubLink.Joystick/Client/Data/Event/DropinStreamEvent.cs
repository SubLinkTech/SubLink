using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Event;

internal class DropinStreamEvent : BaseMessage {
    public class MetadataType {
        [JsonPropertyName("origin")]
        public string Origin { get; private set; } = string.Empty;

        [JsonPropertyName("destination")]
        public string Destination { get; private set; } = string.Empty;

        [JsonPropertyName("number_of_viewers")]
        public long NumberOfViewers { get; private set; } = 0;

        [JsonPropertyName("destination_username")]
        public string DestinationUsername { get; private set; } = string.Empty;
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

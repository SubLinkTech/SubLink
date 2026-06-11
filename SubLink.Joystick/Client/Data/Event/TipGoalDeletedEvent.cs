using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Event;

internal class TipGoalDeletedEvent : BaseMessage {
    public class MetadataType {
        [JsonPropertyName("title")]
        public string Title { get; private set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; private set; } = 0;
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

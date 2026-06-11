using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Event;

internal class TipGoalIncreasedEvent : BaseMessage {
    public class MetadataType {
        [JsonPropertyName("what")]
        public string What { get; private set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; private set; } = 0;

        [JsonPropertyName("by_user")]
        public string ByUser { get; private set; } = string.Empty;

        [JsonPropertyName("current")]
        public long Current { get; private set; } = 0;

        [JsonPropertyName("previous")]
        public long Previous { get; private set; } = 0;
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

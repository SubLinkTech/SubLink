using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Event;

internal class FollowerCountUpdatedEvent : BaseMessage {
    public class MetadataType {
        [JsonPropertyName("number_of_followers")]
        public long NumberOfFollowers { get; private set; } = 0;
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

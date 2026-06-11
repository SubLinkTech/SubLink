using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Event;

internal class TippedEvent : BaseMessage {
    public class MetadataType {
        [JsonPropertyName("who")]
        public string Who { get; private set; } = string.Empty;

        [JsonPropertyName("what")]
        public string What { get; private set; } = string.Empty;

        [JsonPropertyName("how_much")]
        public long Amount { get; private set; } = 0;

        [JsonPropertyName("tip_menu_item")]
        public string TipMenuItem { get; private set; } = string.Empty;
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

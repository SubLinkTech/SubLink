using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Response;

public sealed class RejectUnsubscription : BaseResponse {
    public class CSIdent {
        [JsonPropertyName("channel")]
        public string Channel { get; set; } = string.Empty;
        [JsonPropertyName("stream_id")]
        public string StreamId { get; set; } = string.Empty;
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;
    }

    [JsonPropertyName("identifier")]
    public string Identifier { set => DeserializeIdent(value); }

    [JsonIgnore]
    public CSIdent Ident { get; private set; } = new();

    private void DeserializeIdent(string json) {
        var newVal = JsonSerializer.Deserialize<CSIdent>(json);
        if (newVal != null) Ident = newVal;
    }
}

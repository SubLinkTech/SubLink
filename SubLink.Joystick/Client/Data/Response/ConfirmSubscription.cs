using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Response;

/*
{ "command":"subscribe","identifier":"{\"channel\":\"EventLogChannel\",\"stream_id\":\"vexa_io\"}"}
{ "type":"confirm_subscription","identifier":"{\"channel\":\"EventLogChannel\",\"stream_id\":\"vexa_io\"}"}

{ "command":"subscribe","identifier":"{\"channel\":\"ApplicationChannel\"}"}
{ "type":"confirm_subscription","identifier":"{\"channel\":\"ApplicationChannel\"}"}

{ "command":"subscribe","identifier":"{\"channel\":\"SystemEventChannel\",\"user_id\":\"laurarozier\"}"}
{ "type":"confirm_subscription","identifier":"{\"channel\":\"SystemEventChannel\",\"user_id\":\"laurarozier\"}"}

{ "command":"subscribe","identifier":"{\"channel\":\"WhisperChatChannel\",\"stream_id\":\"vexa_io\",\"user_id\":\"laurarozier\"}"}
{ "type":"confirm_subscription","identifier":"{\"channel\":\"WhisperChatChannel\",\"stream_id\":\"vexa_io\",\"user_id\":\"laurarozier\"}"}

{ "command":"subscribe","identifier":"{\"channel\":\"ChatChannel\",\"stream_id\":\"vexa_io\"}"}
{ "type":"confirm_subscription","identifier":"{\"channel\":\"ChatChannel\",\"stream_id\":\"vexa_io\"}"}
*/

public sealed class ConfirmSubscription : BaseResponse {
    public class CSIdent {
        [JsonPropertyName("channel"), JsonRequired]
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

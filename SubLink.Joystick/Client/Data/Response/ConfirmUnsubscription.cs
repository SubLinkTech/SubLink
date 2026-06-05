using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Response;

/*
{"command":"unsubscribe","identifier":"{\"channel\":\"WhisperChatChannel\",\"stream_id\":\"zaninibbles\",\"user_id\":\"laurarozier\"}"}
{ "type":"confirm_unsubscription","identifier":"{\"channel\":\"WhisperChatChannel\",\"stream_id\":\"zaninibbles\",\"user_id\":\"laurarozier\"}"}

{ "command":"unsubscribe","identifier":"{\"channel\":\"ChatChannel\",\"stream_id\":\"zaninibbles\"}"}
{ "type":"confirm_unsubscription","identifier":"{\"channel\":\"ChatChannel\",\"stream_id\":\"zaninibbles\"}"}

{ "command":"unsubscribe","identifier":"{\"channel\":\"ApplicationChannel\"}"}
{ "type":"confirm_unsubscription","identifier":"{\"channel\":\"ApplicationChannel\"}"}

{ "command":"unsubscribe","identifier":"{\"channel\":\"SystemEventChannel\",\"user_id\":\"laurarozier\"}"}
{ "type":"confirm_unsubscription","identifier":"{\"channel\":\"SystemEventChannel\",\"user_id\":\"laurarozier\"}"}

{ "command":"unsubscribe","identifier":"{\"channel\":\"EventLogChannel\",\"stream_id\":\"zaninibbles\"}"}
{ "type":"confirm_unsubscription","identifier":"{\"channel\":\"EventLogChannel\",\"stream_id\":\"zaninibbles\"}"}
*/

public sealed class ConfirmUnsubscription : BaseResponse {
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

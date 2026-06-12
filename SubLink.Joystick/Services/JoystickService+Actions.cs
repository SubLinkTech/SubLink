using xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

namespace xyz.yewnyx.SubLink.Joystick.Services;

internal sealed partial class JoystickService {
    public void SendChatMessage(string text) =>
        _client.SendCommand(new ChatMessage(text, _settings.ChannelId));

    public void SendDeleteMessage(string messageId) =>
        _client.SendCommand(new DeleteMessage(messageId, _settings.ChannelId));

    public void SendWhisper(string username, string text) =>
        _client.SendCommand(new Whisper(username, text, _settings.ChannelId));

    public void SendMuteUser(string messageId) =>
        _client.SendCommand(new MuteUser(messageId, _settings.ChannelId));

    public void SendUnmuteUser(string username) =>
        _client.SendCommand(new UnmuteUser(_settings.Username, username, _settings.ChannelId));

    public void SendBlockUser(string messageId) =>
        _client.SendCommand(new BlockUser(messageId, _settings.ChannelId));
}

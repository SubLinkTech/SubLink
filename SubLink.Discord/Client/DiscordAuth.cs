using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace xyz.yewnyx.SubLink.Discord.Client;

internal class DiscordAuth(string clientId, string clientSecret) {
    private readonly string _clientId = clientId;
    private readonly string _clientSecret = clientSecret;

    public async Task<string> FetchAccessTokenAsync() {
        using var httpClient = new HttpClient();
        var content = new FormUrlEncodedContent([
            new("client_id", _clientId),
            new("client_secret", _clientSecret),
            new("grant_type", "client_credentials"),
            new("scope", "rpc rpc.voice.read rpc.voice.write rpc.notifications.read messages.read identify")
        ]);

        HttpResponseMessage response = await httpClient.PostAsync("https://discord.com/api/oauth2/token", content);

        if (response.IsSuccessStatusCode) {
            string responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            return doc?.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
        }

        throw new Exception($"Failed to fetch token: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
    }
}

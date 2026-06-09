using Serilog;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Api;

namespace xyz.yewnyx.SubLink.Joystick.Client.OAuth;

internal sealed class OAuthClient {
    private const string CAuthUrlFmt = "https://joystick.tv/api/oauth/authorize?response_type=code&client_id={0}&scope=bot&state={1}";
    private const string CJoystickApi = "https://api.joystick.tv/api";
    private const string CRefreshTokenUrlFmt = CJoystickApi + "/oauth/token?grant_type=refresh_token&refresh_token={0}";
    private const string CAuthTokenUrlFmt = CJoystickApi + "/oauth/token?redirect_uri=.&code={0}&grant_type=authorization_code";
    private const int CTimeout = 60 * 1000;
    private const int CPollFreq = 100;
    private const int CMaxPolls = CTimeout / CPollFreq;

    private readonly ILogger _logger;
    private readonly ushort _oauthPort;
    private readonly string _authUrl;

    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public string State { get; private set; }
    public string AuthCode { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string ChannelId { get; private set; } = string.Empty;
    public bool IsAuthenticated { get; private set; } = false;

    public OAuthClient(ILogger logger, ushort oauthPort, string clientId, string clientSecret, string accessToken,
        string refreshToken, string state) {
        _logger = logger;
        _oauthPort = oauthPort;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        State = state;
        AuthCode = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));

        if (string.IsNullOrWhiteSpace(State))
            State = Guid.NewGuid().ToString().ToUpper();

        _authUrl = string.Format(CAuthUrlFmt, clientId, State);
    }

    public async Task AuthorizeUser() {
        if (!string.IsNullOrWhiteSpace(RefreshToken))
            UseRefreshToken();

        if (!IsAuthenticated)
            await PerformFullAuthCycle();

        _logger.Information("[{TAG}] Requesting Streamer Settings", Platform.PlatformName);
        HttpRequestMessage httpRequest = new(HttpMethod.Get, $"{CJoystickApi}/users/stream-settings");
        httpRequest.Headers.Authorization = new("Bearer", AccessToken);
        httpRequest.Headers.Add("X-JOYSTICK-STATE", State);

        HttpResponseMessage? httpResponse = MakeHttpRequest(httpRequest);
        string jsonStr = HttpResponseToContent(httpResponse);

        if (null == httpResponse || !httpResponse.IsSuccessStatusCode) {
            _logger.Error("[{TAG}] Failed with statuscode: {StatusCode}\n{jsonStr}", Platform.PlatformName,
                httpResponse?.StatusCode ?? HttpStatusCode.InternalServerError, jsonStr);
            IsAuthenticated = false;
            return;
        } else {
            StreamSettings? streamSettings = JsonSerializer.Deserialize<StreamSettings>(jsonStr);

            if (null == streamSettings) {
                _logger.Error("[{TAG}] Failed to retrieve stream settings", Platform.PlatformName);
                IsAuthenticated = false;
                return;
            }

            Username = streamSettings.Username;
            ChannelId = streamSettings.ChannelId;
            _logger.Information("[{TAG}] Successfully authenticated. Username: `{Username}` ; Channel ID: `{ChannelId}`",
                Platform.PlatformName, Username, ChannelId);
            IsAuthenticated = true;
        }
    }

    private void UseRefreshToken() {
        HttpRequestMessage httpRequest = new(HttpMethod.Post, string.Format(CRefreshTokenUrlFmt, RefreshToken));
        httpRequest.Headers.Authorization = new("Basic", AuthCode);
        httpRequest.Headers.Add("X-JOYSTICK-STATE", State);
        httpRequest.Headers.Accept.Add(new("application/json"));
        httpRequest.Content = new StringContent("", Encoding.ASCII, "application/x-www-form-urlencoded");

        HttpResponseMessage? httpResponse = MakeHttpRequest(httpRequest);
        string jsonStr = HttpResponseToContent(httpResponse);

        if (null == httpResponse || !httpResponse.IsSuccessStatusCode) {
            _logger.Error("[{TAG}] Failed with statuscode: {StatusCode}\n{jsonStr}", Platform.PlatformName,
                httpResponse?.StatusCode ?? HttpStatusCode.InternalServerError, jsonStr);
            IsAuthenticated = false;
        } else {
            AuthCodeResponse? authResponse = JsonSerializer.Deserialize<AuthCodeResponse>(jsonStr);
            if (null == authResponse) return;

            AccessToken = authResponse.AccessToken;
            RefreshToken = authResponse.RefreshToken;
            IsAuthenticated = true;
        }
    }

    private async Task PerformFullAuthCycle() {
        WebServer server = new(_oauthPort, State);
        server.Start();
        int pollCount = 0;

        Process.Start(new ProcessStartInfo { FileName = _authUrl, UseShellExecute = true });

        while (pollCount++ < CMaxPolls && !server.HasResponse) {
            await Task.Delay(CPollFreq);
        }

        server.Stop();

        if (CMaxPolls == pollCount) {
            _logger.Error("[{TAG}] Timed out waiting for OAuth", Platform.PlatformName);
            return;
        }

        HttpRequestMessage httpRequest = new(HttpMethod.Post, string.Format(CAuthTokenUrlFmt, server.TokenAuthCode));
        httpRequest.Headers.Authorization = new("Basic", AuthCode);
        httpRequest.Headers.Add("X-JOYSTICK-STATE", State);
        httpRequest.Headers.Accept.Add(new("application/json"));
        httpRequest.Content = new StringContent("", Encoding.ASCII, "application/x-www-form-urlencoded");

        HttpResponseMessage? httpResponse = MakeHttpRequest(httpRequest);

        if (null == httpResponse || !httpResponse.IsSuccessStatusCode) {
            _logger.Error("[{TAG}] Failed with statuscode: {StatusCode}", Platform.PlatformName,
                httpResponse?.StatusCode ?? HttpStatusCode.InternalServerError);
            IsAuthenticated = false;
        } else {
            string jsonStr = HttpResponseToContent(httpResponse);
            var authResponse = JsonSerializer.Deserialize<AuthCodeResponse>(jsonStr);
            if (null == authResponse) return;

            AccessToken = authResponse.AccessToken;
            RefreshToken = authResponse.RefreshToken;
            IsAuthenticated = true;
        }
    }

    private HttpResponseMessage? MakeHttpRequest(HttpRequestMessage requestMessage) {
        try {
            HttpClient Http = new();
            Task<HttpResponseMessage> authCodeTask = Http.SendAsync(requestMessage);

            if (authCodeTask.Wait(5000)) {
                return authCodeTask.Result;
            } else {
                _logger.Error("[{TAG}] Failed to get server response code in time", Platform.PlatformName);
                return null;
            }
        } catch (Exception e) {
            _logger.Error("[{TAG}] Unexpected exception during HTTP Request\n{exception}", Platform.PlatformName, e.ToString());
            return null;
        }
    }

    private string HttpResponseToContent(HttpResponseMessage? response) {
        if (null == response) return string.Empty;
        Task<string> authCodeReader = response.Content.ReadAsStringAsync();

        if (authCodeReader.Wait(5000)) {
            return authCodeReader.Result;
        } else {
            _logger.Error("[{TAG}] Failed to read server in time", Platform.PlatformName);
            return string.Empty;
        }
    }
}

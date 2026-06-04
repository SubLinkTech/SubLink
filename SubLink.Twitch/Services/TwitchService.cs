using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Helix.Models.EventSub;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;

namespace xyz.yewnyx.SubLink.Twitch.Services;

[UsedImplicitly]
internal sealed partial class TwitchService {
    private readonly static JsonSerializerOptions CJsonSerializerOptions = new() { WriteIndented = true };
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IOptionsMonitor<TwitchSettings> _settingsMonitor;
    private TwitchSettings _settings;

    private readonly TwitchAPI _api;
    private readonly TwitchClient _client;
    private readonly EventSubWebsocketClient _eventSub;

    private readonly TwitchRules _rules;

    private string? ChannelName;
    private string? ChannelId;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Shhh")]
    private IServiceScope? _twitchLoggedInScope;

    public TwitchService(
        ILogger logger,
        IHostApplicationLifetime applicationLifetime,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<TwitchSettings> settingsMonitor,
        EventSubWebsocketClient eventSub,
        TwitchRules rules) {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _serviceScopeFactory = serviceScopeFactory;
        _settingsMonitor = settingsMonitor;
        _settingsMonitor.OnChange(UpdateTwitchSettings);
        _settings = _settingsMonitor.CurrentValue;
        
        _api = new TwitchAPI();
        var clientOptions = new ClientOptions {
            //MessagesAllowedInPeriod = 750,
            //ThrottlingPeriod = TimeSpan.FromSeconds(30),
        };
        var customClient = new WebSocketClient(clientOptions);
        _client = new TwitchClient(customClient);
        //_client.OnJoinedChannel += OnJoinedChannel;
        //_client.OnMessageReceived += OnMessageReceived;

        _eventSub = eventSub ?? throw new ArgumentNullException(nameof(eventSub));
        
        _rules = rules;

        WireCallbacks();
    }

    private void UpdateTwitchSettings(TwitchSettings twitchSettings) => _settings = twitchSettings;

    public async Task StartAsync() {
        if (string.IsNullOrWhiteSpace(_settings.ClientId) ||
            string.IsNullOrWhiteSpace(_settings.ClientSecret)) {
            _logger.Warning("[{TAG}] Invalid config, skipping", Platform.PlatformName);
            return;
        }

        _api.Settings.AccessToken = _settings.AccessToken;
        _api.Settings.ClientId = _settings.ClientId;
        _api.Settings.Scopes = [
            AuthScopes.Bits_Read,
            AuthScopes.Channel_Manage_Polls,
            AuthScopes.Channel_Manage_Redemptions,
            AuthScopes.Channel_Read_Hype_Train,
            AuthScopes.Channel_Read_Predictions,
            AuthScopes.Channel_Read_Redemptions,
            AuthScopes.Channel_Read_Subscriptions,
            AuthScopes.Channel_Read_Polls,
            AuthScopes.Channel_Read_VIPs,
            AuthScopes.Moderator_Read_Followers,
            AuthScopes.User_Read_Chat,
        ];
        
        await ValidateOrUpdateAccessTokenAsync();
        // TODO: eventually, embedding a webview is preferable - that or using a server after all

        var credentials = new ConnectionCredentials(ChannelName!, _settings.AccessToken);
        _client.Initialize(credentials, ChannelName);
        await _client.ConnectAsync();

        if (await _eventSub.ConnectAsync()) {
            _logger.Information("[{TAG}] Connected to EventSub", Platform.PlatformName);
        } else {
            _logger.Warning("[{TAG}] Failed to connect to EventSub", Platform.PlatformName);
        }
    }

    public async Task StopAsync() {
        await _eventSub.DisconnectAsync();
    }

    private async Task OnWebsocketConnected(object? sender, WebsocketConnectedArgs e) {
        if (!e.IsRequestedReconnect) {
            await Task.Delay(100);
            await Task.WhenAll(
                SubscribeAsync("channel.channel_points_custom_reward_redemption.add"),
                SubscribeAsync("channel.channel_points_custom_reward_redemption.update"),
                SubscribeAsync("channel.update"),
                SubscribeAsync("channel.bits.use"),
                SubscribeAsync("channel.chat.message"),
                SubscribeAsync("channel.cheer"),
                SubscribeAsync("channel.follow"),
                SubscribeAsync("channel.hype_train.begin"),
                SubscribeAsync("channel.hype_train.end"),
                SubscribeAsync("channel.hype_train.progress"),
                SubscribeAsync("channel.poll.begin"),
                SubscribeAsync("channel.poll.end"),
                SubscribeAsync("channel.poll.progress"),
                SubscribeAsync("channel.prediction.begin"),
                SubscribeAsync("channel.prediction.end"),
                SubscribeAsync("channel.prediction.lock"),
                SubscribeAsync("channel.prediction.progress"),
                SubscribeAsync("channel.raid"),
                SubscribeAsync("channel.subscribe"),
                SubscribeAsync("channel.subscription.end"),
                SubscribeAsync("channel.subscription.gift"),
                SubscribeAsync("channel.subscription.message"),
                SubscribeAsync("stream.offline"),
                SubscribeAsync("stream.online")
            );
        }
    }

    private async Task OnWebsocketDisconnected(object? sender, EventArgs e) {
        _logger.Information("[{TAG}] sender: {Sender} event: {@E}", Platform.PlatformName, sender, e);
        await Task.CompletedTask;
    }
    private async Task OnWebsocketReconnected(object? sender, EventArgs e) {
        _logger.Information("[{TAG}] sender: {Sender} event: {@E}", Platform.PlatformName, sender, e);
        await Task.CompletedTask;
    }

    private async Task<CreateEventSubSubscriptionResponse> SubscribeAsync(string subscriptionType) {
        CreateEventSubSubscriptionResponse result;

        try
        {

            result = await _api.Helix.EventSub.CreateEventSubSubscriptionAsync(
                subscriptionType,
                SubscriptionTypeToVersion(subscriptionType),
                SubscriptionTypeToCondition(subscriptionType),
                EventSubTransportMethod.Websocket,
                _eventSub.SessionId,
                clientId: _settings.ClientId,
                accessToken: _settings.AccessToken
            );
            _logger.Debug("[{TAG}] {TYPE} Websocket subscription result = {RESULT}",
                Platform.PlatformName,
                subscriptionType,
                JsonSerializer.Serialize(result, CJsonSerializerOptions)
            );
        }
        catch (Exception ex)
        {
            result = new CreateEventSubSubscriptionResponse();
            _logger.Error("[{TAG}] {TYPE} Websocket subscription error = {RESULT}",
                Platform.PlatformName,
                subscriptionType,
                ex.Message
            );
        }

        return result;
    }

    private static string SubscriptionTypeToVersion(string subType) =>
        subType switch {
            "channel.update" => "2",
            "channel.follow" => "2",
            "channel.hype_train.begin" => "2",
            "channel.hype_train.progress" => "2",
            "channel.hype_train.end" => "2",
            _ => "1",
        };

    private Dictionary<string, string> SubscriptionTypeToCondition(string subType) =>
        subType switch {
            "channel.chat.message" => new() { { "broadcaster_user_id", ChannelId! }, { "user_id", ChannelId! } },
            "channel.follow" => new() { { "broadcaster_user_id", ChannelId! }, { "moderator_user_id", ChannelId! } },
            "channel.raid" => new() { { "to_broadcaster_user_id", ChannelId! } },
            _ => new() { { "broadcaster_user_id", ChannelId! } },
        };

    private async Task<AuthCodeResponse> LaunchOAuthFlowAsync() {
        const string redirectUri = "http://localhost:50666/authorize/";
        using var listener = new HttpListener();

        try {
            listener.Prefixes.Add(redirectUri);
        
            var state = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            var url = _api.Auth.GetAuthorizationCodeUrl(
                redirectUri,
                [
                    AuthScopes.Bits_Read,
                    AuthScopes.Channel_Manage_Polls,
                    AuthScopes.Channel_Manage_Redemptions,
                    AuthScopes.Channel_Read_Hype_Train,
                    AuthScopes.Channel_Read_Predictions,
                    AuthScopes.Channel_Read_Redemptions,
                    AuthScopes.Channel_Read_Subscriptions,
                    AuthScopes.Channel_Read_Polls,
                    AuthScopes.Channel_Read_VIPs,
                    AuthScopes.Moderator_Read_Followers,
                    AuthScopes.User_Read_Chat,
                ],
                state: state,
                clientId: _settings.ClientId);
            
            listener.Start();
            
            var processInfo = new ProcessStartInfo {FileName = url, UseShellExecute = true};
            Process.Start(processInfo);
            
            var context = await listener.GetContextAsync().ConfigureAwait(false);
            var queryString = context.Request.QueryString;
            await using (var writer = new StreamWriter(context.Response.OutputStream)) {
                await writer.WriteLineAsync("Success!");
                await writer.FlushAsync();
            }
            // Below causes issues on Linux, test if Windows is OK with this
            //context.Response.OutputStream.Close();

            var code = queryString["code"];
            
            // TODO: verify the state code
            
            var authCodeResponse = await _api.Auth.GetAccessTokenFromCodeAsync(code, _settings.ClientSecret, redirectUri, _settings.ClientId);
            return authCodeResponse;
        } finally {
            listener.Close();
        }
    }

    private async Task ValidateOrUpdateAccessTokenAsync() {
        var accessToken = _settings.AccessToken;
        var refreshToken = _settings.RefreshToken;
        ValidateAccessTokenResponse? validation;

        do {
            // No info of any kind: start full OAuth flow
            if (!accessToken.HasAnything() && !refreshToken.HasAnything()) {
                _logger.Warning("[{TAG}] Twitch access is not configured, requesting Twitch authorization", Platform.PlatformName);

                var authCodeResponse = await LaunchOAuthFlowAsync();
                accessToken = authCodeResponse.AccessToken;
                refreshToken = authCodeResponse.RefreshToken;
                validation = await _api.Auth.ValidateAccessTokenAsync(accessToken);
                if (validation != null) break;
            }

            // Has a refresh token only, inexplicably, but whatever, try and refresh it
            if (!accessToken.HasAnything() && refreshToken.HasAnything()) {
                _logger.Warning("[{TAG}] Twitch access token is not configured, but refresh token is available", Platform.PlatformName);
                _logger.Information("[{TAG}] Attempting Twitch access token refresh", Platform.PlatformName);
                
                if (await _api.Auth.RefreshAuthTokenAsync(_settings.RefreshToken, _settings.ClientSecret, _settings.ClientId) is { } refreshResponse) {
                    _logger.Verbose("Twitch access token refresh succeeded");
                    accessToken = refreshResponse.AccessToken;
                    refreshToken = refreshResponse.RefreshToken;
                    validation = await _api.Auth.ValidateAccessTokenAsync(accessToken);

                    if (validation != null) break;
                } else {
                    _logger.Information("[{TAG}] Twitch access token refresh failed", Platform.PlatformName);
                    refreshToken = null;
                }
            }
            
            // Validate auth token
            validation = await _api.Auth.ValidateAccessTokenAsync(accessToken);
            if (validation != null) break;
            _logger.Warning("[{TAG}] Your auth token may have expired. Trying to refresh it", Platform.PlatformName);

            if (await _api.Auth.RefreshAuthTokenAsync(_settings.RefreshToken, _settings.ClientSecret, _settings.ClientId) is { } refresh2) {
                _logger.Verbose("[{TAG}] Access token refresh succeeded", Platform.PlatformName);
                accessToken = refresh2.AccessToken;
                refreshToken = refresh2.RefreshToken;
                validation = await _api.Auth.ValidateAccessTokenAsync(accessToken);

                if (validation != null) break;
            }
            
            _logger.Warning("[{TAG}] Your access tokens seem to be invalid. Trying a last-ditch full re-auth", Platform.PlatformName);
            var acr2 = await LaunchOAuthFlowAsync();
            accessToken = acr2.AccessToken;
            refreshToken = acr2.RefreshToken;
            validation = await _api.Auth.ValidateAccessTokenAsync(accessToken);

            if (validation != null) {
                _logger.Information("[{TAG}] Your new OAuth token is ready. Please relaunch SubLink!", Platform.PlatformName);
                Console.ReadLine();
                _applicationLifetime.StopApplication();
                return;
            } else {
                _logger.Warning("[{TAG}] Something went wrong. Please send logs to the SubLink developers!", Platform.PlatformName);
                Console.ReadLine();
                _applicationLifetime.StopApplication();
                return;
            }
        } while (false);

        ChannelName = validation.Login;
        ChannelId = validation.UserId;

        using (await GlobalLogContext.LockAsync()) {
            GlobalLogContext.PushProperty("ChannelName", ChannelName);
        }
        
        _logger.Information("[{TAG}] Validated token for {ChannelName} ({ChannelId}) with scopes: {Scopes}",
            Platform.PlatformName, ChannelName, ChannelId, string.Join(", ", validation.Scopes));

        var json = await File.ReadAllTextAsync(Platform.PlatformConfigFile);
        var j = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        j[Platform.PlatformName]["AccessToken"] = accessToken;
        j[Platform.PlatformName]["RefreshToken"] = refreshToken;

        var scopes = new JsonArray();
        validation.Scopes.ForEach(scope => scopes.Add(scope));
        j[Platform.PlatformName]["Scopes"] = scopes;
        await File.WriteAllTextAsync(Platform.PlatformConfigFile, j.ToJsonString(new JsonSerializerOptions {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        }));
        _api.Settings.AccessToken = accessToken;
        
        _twitchLoggedInScope = _serviceScopeFactory.CreateScope();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}

internal static class StringUtil {
    internal static bool HasAnything(this string str) => !string.IsNullOrWhiteSpace(str);
}

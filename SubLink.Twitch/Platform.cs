using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TwitchLib.EventSub.Websockets.Extensions;
using xyz.yewnyx.SubLink.Platforms;
using xyz.yewnyx.SubLink.Twitch.Services;

namespace xyz.yewnyx.SubLink.Twitch;

public class Platform : IPlatform {
    internal const string PlatformName = "Twitch";
    internal static string PlatformConfigFile = GlobalUtils.GetSettingsFilePath(PlatformName);

#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE1006 // Naming Styles
    private ILogger? _logger { get; set; }
    private IServiceProvider? _serviceProvider { get; set; }
    private TwitchService? _service { get; set; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0052 // Remove unread private members

    // Useful for enumerating the loaded platforms
    public string GetPlatformName() => PlatformName;
    public string GetServiceSymbol() => "SUBLINK_TWITCH";
    public string[] GetAdditionalUsings() => [
        "TwitchLib.EventSub.Core.Models.ChannelGoals",
        "TwitchLib.EventSub.Core.Models.ChannelPoints",
        "TwitchLib.EventSub.Core.Models.Charity",
        "TwitchLib.EventSub.Core.Models.Extensions",
        "TwitchLib.EventSub.Core.Models.HypeTrain",
        "TwitchLib.EventSub.Core.Models.Polls",
        "TwitchLib.EventSub.Core.Models.Predictions",
        "TwitchLib.EventSub.Core.Models.Subscriptions",
        "TwitchLib.EventSub.Core.SubscriptionTypes.Channel",
        "TwitchLib.EventSub.Core.SubscriptionTypes.Drop",
        "TwitchLib.EventSub.Core.SubscriptionTypes.Extension",
        "TwitchLib.EventSub.Core.SubscriptionTypes.Stream",
        "TwitchLib.EventSub.Core.SubscriptionTypes.User",
        "xyz.yewnyx.SubLink.Twitch.Services",
    ];
    public string[] GetAdditionalAssemblies() => [];

    public bool EnsureConfigExists() {
        if (!File.Exists(PlatformConfigFile)) {
            File.WriteAllText(PlatformConfigFile, """
{
  "Twitch": {
    "ClientId": "",
    "ClientSecret": "",
    "AccessToken": "",
    "RefreshToken": "",
    "Scopes": [
      "bits:read",
      "channel:manage:polls",
      "channel:manage:redemptions",
      "channel:read:hype_train",
      "channel:read:polls",
      "channel:read:predictions",
      "channel:read:redemptions",
      "channel:read:subscriptions",
      "channel:read:vips",
      "user:read:chat",
      "moderator:read:followers"
    ]
  }
}
""");
            return false;
        }

        return true;
    }

    public void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder) =>
        builder.AddJsonFile(PlatformConfigFile, false, true);

    public void ConfigureServices(HostBuilderContext context, IServiceCollection services) =>
        services
            .Configure<TwitchSettings>(context.Configuration.GetSection(PlatformName))
            .AddTwitchLibEventSubWebsockets()
            .AddScoped<TwitchRules>()
            .AddScoped<TwitchService>();

    public void AppendRules(Dictionary<string, IPlatformRules> rules) {
        var rulesSvc = _serviceProvider?.GetService<TwitchRules>();

        if (rulesSvc != null)
            rules.Add(PlatformName, rulesSvc);
    }

    public void SetLogger(ILogger logger) =>
        _logger = logger;

    public void SetServiceProvider(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    // Let the interface handle this, no reflection overhead
    public async Task StartServiceAsync() {
        if (_service != null)
            await _service.StopAsync();

        _service = _serviceProvider?.GetService<TwitchService>();

        if (_service != null)
            await _service.StartAsync();
    }

    public async Task StopServiceAsync() {
        if (_service == null)
            return;

        await _service.StopAsync();
        _service = null;
    }
}

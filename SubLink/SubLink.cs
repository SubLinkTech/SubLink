using Serilog;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

var notifier = new XSNotifier();
// Also allowed people to use a different IP address:
// extraOscPort.Open(9011, "192.168.1.15");
var additionalOSCPort = extraOscPort.Open(9011);

if (additionalOSCPort == null) {
    logger.Error("Failed to open the additional OSC port");
}

oscQuery.AddEndpoint<bool>("/avatar/parameters/MuteSelf", Attributes.AccessValues.ReadWrite, new object[] { true });
oscServer.TryAddMethod("/avatar/parameters/MuteSelf", message => {
    logger.Information($"Received `/avatar/parameters/MuteSelf` value from VRChat : {message.ReadBooleanElement(0)}");
    additionalOSCPort?.SendValue("/action/give_it_a_name", true);
});

oscQuery.AddEndpoint("/tracking/vrsystem/head/pose", "ffffff", Attributes.AccessValues.WriteOnly);
oscServer.TryAddMethod("/tracking/vrsystem/head/pose", message => {
    // To make this work add the following to the top of your sublink.cs file:
    // using BuildSoft.OscCore.UnityObjects;
    var position = new Vector3(message.ReadFloatElement(0), message.ReadFloatElement(1), message.ReadFloatElement(2));
    var rotation = new Vector3(message.ReadFloatElement(3), message.ReadFloatElement(4), message.ReadFloatElement(5));
    logger.Information($"VRChat head tracking changed to : Pos{position} rot{rotation}");
    additionalOSCPort?.SendValue("/some/address/set", 0.75);
});

#if SUBLINK_TWITCH

logger.Information("[{TAG}] Twitch integration enabled", "Script");
var twitch = (TwitchRules)rules["Twitch"];

twitch.ReactToJoinedChannel(async (channel, botUsername) => {
    logger.Information("User {BotUsername} joined channel {ChannelName}", botUsername, channel);
});

/* DEPRECATED ; Please use `ReactToChatMessage` instead */
twitch.ReactToMessageReceived(async chatMessage => {
    if ("yewnyx".Equals(chatMessage.Username, StringComparison.InvariantCultureIgnoreCase)) {
        OscParameter.SendAvatarParameter("JacketToggle", false);
        OscParameter.SendAvatarParameter("Sus", true);
    }
    logger.Information(
        "Username: {UserName}, Display name: {DisplayName}, User Type: {UserType}, Message: {Message}",
        chatMessage.Username, chatMessage.DisplayName, chatMessage.UserType, chatMessage.Message);
});

twitch.ReactToChatMessage(async message => {
    if ("yewnyx".Equals(message.ChatterUserLogin, StringComparison.InvariantCultureIgnoreCase)) {
        OscParameter.SendAvatarParameter("JacketToggle", false);
        OscParameter.SendAvatarParameter("Sus", true);
    }
    logger.Information(
        "Username: {UserLogin}, Display name: {UserName}, Broadcaster: {IsBroadcaster}, Staff: {IsStaff}, Moderator: {IsModerator}, Vip: {IsVip}, Subscriber: {IsSubscriber}, Message: {Text}",
        message.ChatterUserLogin, message.ChatterUserName,
        message.IsBroadcaster, message.IsStaff, message.IsModerator, message.IsVip, message.IsSubscriber,
        message.Message.Text);
});

twitch.ReactToChannelBitsUse(async channelBitsUse => {
    switch (channelBitsUse.Type) {
        case "cheer": {
            logger.Information(
                "{UserName} cheered {Bits} bits to {BroadcasterUserName} with {Message}",
                channelBitsUse.UserName, channelBitsUse.Bits, channelBitsUse.BroadcasterUserName, channelBitsUse.Message.Text);
            return;
        }
        case "power_up": {
            string outText = string.Format(
                "{UserName} sent a {Type} type 'Power-Up' worth {Bits} bits to {BroadcasterUserName}",
                channelBitsUse.UserName, channelBitsUse.PowerUp.Type, channelBitsUse.Bits, channelBitsUse.BroadcasterUserName
            );

            switch (channelBitsUse.PowerUp.Type) {
                case "message_effect": {
                    outText += string.Format(" EffectId used: {EffectId}", channelBitsUse.PowerUp.MessageEffectId);
                    break;
                }
                case "celebration": {
                    if (channelBitsUse.PowerUp.Emote != null)
                        outText += string.Format(" Emote used: {EmoteName}", channelBitsUse.PowerUp.Emote.Name);

                    if (!string.IsNullOrWhiteSpace(channelBitsUse.PowerUp.MessageEffectId))
                        outText += string.Format(" EffectId used: {EffectId}", channelBitsUse.PowerUp.MessageEffectId);

                    break;
                }
                case "gigantify_an_emote": {
                    outText += string.Format(" Emote used: {EmoteName}", channelBitsUse.PowerUp.Emote.Name);
                    break;
                }
                default: break;
            }

            logger.Information(outText);
            return;
        }
        case "combo": {
            logger.Information(
                "{UserName} sent a combo worth {Bits} bits to {BroadcasterUserName} with {Message}",
                channelBitsUse.UserName, channelBitsUse.Bits, channelBitsUse.BroadcasterUserName, channelBitsUse.Message?.Text ?? "");
            return;
        }
        default: {
            logger.Information("Unknown channel bits use type: {Type}", channelBitsUse.Type);
            return;
        }
    }
});

twitch.ReactToCheer(async channelCheer => {
    logger.Information(
        "{UserName} cheered {Bits} bits to {BroadcasterUserName} with {Message}",
        channelCheer.UserName, channelCheer.Bits, channelCheer.BroadcasterUserName, channelCheer.Message);
    OscParameter.SendAvatarParameter("TwitchCheer", channelCheer.Bits);
});

twitch.ReactToFollow(async follow => {
    logger.Information(
        "{EventDataUserName} followed {EventDataBroadcasterUserName} at {EventDataFollowedAt}",
        follow.UserName, follow.BroadcasterUserName, follow.FollowedAt);
});

twitch.ReactToHypeTrainBegin(async hypeTrainBegin => {
    logger.Information(
        "Hype Train for {BroadcasterUserName} began at level {Level}. Progress: {Progress} - {Goal} to next level. Total Points: {Total}",
        hypeTrainBegin.BroadcasterUserName, hypeTrainBegin.Level, hypeTrainBegin.Progress, hypeTrainBegin.Goal, hypeTrainBegin.Total);
});

twitch.ReactToHypeTrainEnd(async hypeTrainEnd => {
    logger.Information(
        "Hype Train for {BroadcasterUserName} ended at level {Level}. Total Points: {Total}",
        hypeTrainEnd.BroadcasterUserName, hypeTrainEnd.Level, hypeTrainEnd.Total);
});

twitch.ReactToHypeTrainProgress(async hypeTrainProgress => {
    logger.Information(
        "Hype Train for {BroadcasterUserName} progressed to level {Level}. Progress: {Progress} - {Goal} to next level. Total Points: {Total}",
        hypeTrainProgress.BroadcasterUserName, hypeTrainProgress.Level, hypeTrainProgress.Progress, hypeTrainProgress.Goal, hypeTrainProgress.Total);
});

twitch.ReactToPointsCustomRewardRedemptionAdd(async channelPointsCustomRewardRedemption => {
    var r = channelPointsCustomRewardRedemption;
            
    logger.Information("User {DisplayName} ({Login}) redeemed {RewardId} {Title} with message {UserInput}", 
        r.UserName, r.UserLogin, r.Reward.Id, r.Reward.Title, r.UserInput);
});

twitch.ReactToRaid(async channelRaid => {
    logger.Information("{Login} raided you with {ViewerCount} viewers", channelRaid.FromBroadcasterUserName, channelRaid.Viewers);
});

twitch.ReactToSubscribe(async channelSubscribe => {
    logger.Information("New subscription: User {UserName} ({Login}) {WasGift} at tier \"{Tier}\" ",
        channelSubscribe.UserName, channelSubscribe.UserLogin, channelSubscribe.IsGift ? "was gifted a sub" : "subscribed", channelSubscribe.Tier);

    if (!channelSubscribe.IsGift) {
        OscParameter.SendAvatarParameter("TwitchSubscription", 1);
    }
});

twitch.ReactToSubscriptionGift(async channelSubscriptionGift => {
    logger.Information(
        "User {UserName} ({Login}) gifted {Total} \"{Tier}\" subs - they have gifted {CumulativeTotal} subs total",
        channelSubscriptionGift.UserName, channelSubscriptionGift.UserId, channelSubscriptionGift.Total, channelSubscriptionGift.Tier, channelSubscriptionGift.CumulativeTotal);
    OscParameter.SendAvatarParameter("TwitchCommunityGift", channelSubscriptionGift.Total);
});

twitch.ReactToSubscriptionMessage(async channelSubscriptionMessage => {
    logger.Information(
        "User {UserName} ({Login}) resubscribed at tier \"{Tier}\" for {Months} months (streak: {Streak}) with message: \"{Message}\"",
        channelSubscriptionMessage.UserName, channelSubscriptionMessage.UserLogin, channelSubscriptionMessage.Tier, channelSubscriptionMessage.DurationMonths, channelSubscriptionMessage.StreakMonths, channelSubscriptionMessage.Message);
    OscParameter.SendAvatarParameter("TwitchSubscription", channelSubscriptionMessage.DurationMonths);
});

twitch.ReactToChannelUpdate(async channelUpdate => {
    logger.Information("Streamer {UserName} updated with title: \"{Title}\" category: \"{category}\"", channelUpdate.BroadcasterUserName, channelUpdate.CategoryName, channelUpdate.Title);
});

twitch.ReactToStreamOffline(async streamOffline => {
    logger.Information("Streamer {UserName} stopped streaming", streamOffline.BroadcasterUserName);
});

twitch.ReactToStreamOnline(async streamOnline => {
    logger.Information("Streamer {UserName} started a {Type} stream at {StartedAt}", streamOnline.BroadcasterUserName, streamOnline.Type, streamOnline.StartedAt.ToString("F"));
});

#endif

#if SUBLINK_KICK

logger.Information("[{TAG}] Kick integration enabled", "Script");
var kick = (KickRules)rules["Kick"];

kick.ReactToChatMessage(async chatMessage => {
    if ("yewnyx".Equals(chatMessage.Sender.Slug, StringComparison.InvariantCultureIgnoreCase)) {
        OscParameter.SendAvatarParameter("JacketToggle", false);
        OscParameter.SendAvatarParameter("Sus", true);
    }

    // Extract the Kick emote ID and send it to OSC. Format: [emote:37224:GIGACHAD]
    if (chatMessage.Content.Contains("[emote:")) {
        int idx = chatMessage.Content.IndexOf("[emote:") + 7;
        string emoteIdStr = chatMessage.Content.Substring(idx, chatMessage.Content.IndexOf(':', idx) - idx);

        if (int.TryParse(emoteIdStr, out int emoteId)) {
            OscParameter.SendAvatarParameter("emote", emoteId);
        }
    }

    logger.Information(
        "Username: {UserName}, Slug: {Slug}, Created At: {CreatedAt}, Content: {Content}",
        chatMessage.Sender.Username, chatMessage.Sender.Slug, chatMessage.CreatedAt, chatMessage.Content);
});

kick.ReactToSubscription(async sub => {
    logger.Information("Subscription {Username} subscribed for {Months} months", sub.Username, sub.Months);
    OscParameter.SendAvatarParameter("KickSubscription", sub.Months);
});

kick.ReactToGiftedSubscriptions(async giftedSubs => {
    logger.Information("Gifter {Gifter} gifted {GiftCount} subs", giftedSubs.Gifter, giftedSubs.GetGiftCount());
    OscParameter.SendAvatarParameter("KickCommunityGift", giftedSubs.GetGiftCount());

    switch(giftedSubs.GetGiftCount()) {
        case 2: {
            OscParameter.SendAvatarParameter("BoobaGrow", true);
            break;
        }
        case 3: {
            OscParameter.SendAvatarParameter("BulgeGrow", true);
            break;
        }
        case 5: {
            OscParameter.SendAvatarParameter("Outfit", 1);
            break;
        }
        case 6: {
            OscParameter.SendAvatarParameter("Outfit", 2);
            break;
        }
        default: break;
    }
});

kick.ReactToStreamHost(async streamHost => {
    logger.Information(
        "Hosting {HostUsername} with {NumberViewers} viewers",
        streamHost.HostUsername, streamHost.NumberViewers);
});

kick.ReactToUserBanned(async banned => {
    logger.Information(
        "User {UserUsername} got banned by {ModUsername} until {ExpiresAt}",
        banned.User.Username, banned.BannedBy.Username, banned.ExpiresAt);
});

kick.ReactToUserUnbanned(async unbanned => {
    logger.Information(
        "User {UserUsername} got unbanned by {ModUsername}",
        unbanned.User.Username, unbanned.UnbannedBy.Username);
});

kick.ReactToMessageDeleted(async deletedMessage => {
    logger.Information("Message ID {Id} got deleted", deletedMessage.Message.Id);
});

kick.ReactToChatroomClear(async chatroomClear => {
    logger.Information("Chatroom was cleared");
});

kick.ReactToChatroomUpdated(async chatroomUpdate => {
    logger.Information("Chatroom settings have been updated");
});

kick.ReactToPollUpdate(async pollUpdate => {
    logger.Information("Poll has been updated");
});

kick.ReactToPollDelete(async pollDelete => {
    logger.Information("Poll was deleted");
});

kick.ReactToPinnedMessageCreated(async pinnedMessage => {
    logger.Information(
        "Message ID {Id} has beenn pinned for {Duration} seconds",
        pinnedMessage.Message.Id, pinnedMessage.Duration);
});

kick.ReactToPinnedMessageDeleted(async pinnedMessage => {
    logger.Information("Pinned message was deleted");
});

#endif

#if SUBLINK_STREAMPAD

logger.Information("[{TAG}] StreamPad integration enabled", "Script");
var streamPad = (StreamPadRules)rules["StreamPad"];

streamPad.ReactToControllerValue(async (name, value) => {
    logger.Information($"StreamPad | in rule: {name}:{value}");
    if (name == "PRESETS")
    {
        // Map specific presets to camera modes on/off (or whatever)
        // Values will be 0,1,2,3,...)
        // if (value == 0) OscParameter.XYZ("CAMERA_1", true)
    }
    // Optionally ensure supported names (YAW_LEFT_RIGHT), clamp values, etc
    // Values (currently) (-1.00,value,1.00)
    OscParameter.SendAvatarParameter(name, value);
});

#endif

#if SUBLINK_STREAMELEMENTS

logger.Information("[{TAG}] StreamElements integration enabled", "Script");
var streamElements = (StreamElementsRules)rules["StreamElements"];

streamElements.ReactToTipEvent(async tipInfo => {
    logger.Information("Stream Elements tip recieved : {Amount} {UserCurrency} from {Name} with the following message: {Message}",
        tipInfo.Amount, tipInfo.UserCurrency, tipInfo.Name, tipInfo.Message);

    if (!tipInfo.UserCurrency.Equals("USD", StringComparison.OrdinalIgnoreCase)) {
        logger.Information("Ignoring tip, wrong currency type");
        return;
    }

    OscParameter.SendAvatarParameter("StreamElementsTip", tipInfo.CentAmount);

    switch (tipInfo.CentAmount) {
        // To compare against tipInfo.Amount instead you have to use "floats", which MUST end in an `f` like: 1.23f
        // tipInfo.CentAmount is an integer, which doesn't support decimals.
        case 1000: {
            OscParameter.SendAvatarParameter("Ragdoll", true);
            break;
        }
        case 1500: {
            OscParameter.SendAvatarParameter("Yeet", true);
            break;
        }
        case 2500: {
            OscParameter.SendAvatarParameter("PopConfettiType", 1);
            break;
        }
        case 3000: {
            OscParameter.SendAvatarParameter("PopConfettiType", 2);
            break;
        }
        default: break;
    }
});

#endif

#if SUBLINK_FANSLY

logger.Information("[{TAG}] Fansly integration enabled", "Script");
var fansly = (FanslyRules)rules["Fansly"];

fansly.ReactToChatMessage(async chatMessage => {
    if ("yewnyx".Equals(chatMessage.Username, StringComparison.InvariantCultureIgnoreCase)) {
        OscParameter.SendAvatarParameter("JacketToggle", false);
        OscParameter.SendAvatarParameter("Sus", true);
    }

    DateTime timestamp = DateTimeOffset.FromUnixTimeMilliseconds(chatMessage.CreatedAt).DateTime;
    logger.Information(
        "Fansly message received > Username: {UserName}, Displayname: {Displayname}, Created At: {CreatedAt}, Content: {Content}",
        chatMessage.Username, chatMessage.Displayname, timestamp, chatMessage.Content);
});

fansly.ReactToTip(async tipInfo => {
    logger.Information("Fansly tip recieved : ${Amount} from {Displayname} with the following message: {Content}",
        tipInfo.Amount, tipInfo.Displayname, tipInfo.Content);

    OscParameter.SendAvatarParameter("FanslyTip", tipInfo.CentAmount);

    switch (tipInfo.CentAmount) {
        // To compare against tipInfo.Amount instead you have to use "floats", which MUST end in an `f` like: 1.23f
        // tipInfo.CentAmount is an integer, which doesn't support decimals.
        case 1000: {
            OscParameter.SendAvatarParameter("Ragdoll", true);
            break;
        }
        case 1500: {
            OscParameter.SendAvatarParameter("Yeet", true);
            break;
        }
        case 2500: {
            OscParameter.SendAvatarParameter("PopConfettiType", 1);
            break;
        }
        case 3000: {
            OscParameter.SendAvatarParameter("PopConfettiType", 2);
            break;
        }
        default: break;
    }
});

fansly.ReactToGoalUpdated(async goalInfo => {
    logger.Information("Fansly goal updated : `{Label}` is now at {CurrentAmount} of {GoalAmount} (in $-cents)",
        goalInfo.Label, goalInfo.CurrentAmount, goalInfo.GoalAmount);

    if (
        "Hocking Time".Equals(goalInfo.Label, StringComparison.InvariantCultureIgnoreCase) &&
        goalInfo.CurrentAmount >= goalInfo.GoalAmount
    ) {
        OscParameter.SendAvatarParameter("Honk", true);
    }
});

#endif

#if SUBLINK_OBS

logger.Information("[{TAG}] OBS integration enabled", "Script");
var obs = (OBSRules)rules["OBS"];

obs.ReactToCurrentSceneCollectionChanging(async currentSceneCollectionChanging => {
    logger.Information("OBS Current Scene Collection `{SceneCollectionName}` Changing",
        currentSceneCollectionChanging.SceneCollectionName);
});

obs.ReactToCurrentSceneCollectionChanged(async currentSceneCollectionChanged => {
    logger.Information("OBS Current Scene Collection `{SceneCollectionName}` Changed",
        currentSceneCollectionChanged.SceneCollectionName);
});

obs.ReactToSceneCollectionListChanged(async sceneCollectionListChanged => {
    logger.Information("OBS Scene Collection List Changed : `{SceneCollections}`",
        string.Join(", ", sceneCollectionListChanged.SceneCollections));
});

obs.ReactToCurrentProfileChanging(async currentProfileChanging => {
    logger.Information("OBS Current Profile `{ProfileName}` Changing",
        currentProfileChanging.ProfileName);
});

obs.ReactToCurrentProfileChanged(async currentProfileChanged => {
    logger.Information("OBS Current Profile `{ProfileName}` Changed",
        currentProfileChanged.ProfileName);
});

obs.ReactToProfileListChanged(async profileListChanged => {
    logger.Information("OBS Profile List Changed : `{Profiles}`",
        string.Join(", ", profileListChanged.Profiles));
});

obs.ReactToSourceFilterListReindexed(async sourceFilterListReindexed => {
    logger.Information("OBS Source `{SourceName}` Filter List Reindexed",
        sourceFilterListReindexed.SourceName);
});

obs.ReactToSourceFilterCreated(async sourceFilterCreated => {
    logger.Information("OBS Source `{SourceName}` Filter `{FilterName}` of kind `{FilterKind}` at index `{FilterIndex}` Created",
        sourceFilterCreated.SourceName,
        sourceFilterCreated.FilterName,
        sourceFilterCreated.FilterKind,
        sourceFilterCreated.FilterIndex);
});

obs.ReactToSourceFilterRemoved(async sourceFilterRemoved => {
    logger.Information("OBS Source `{SourceName}` Filter `{FilterName}` removed",
        sourceFilterRemoved.SourceName,
        sourceFilterRemoved.FilterName);
});

obs.ReactToSourceFilterNameChanged(async sourceFilterNameChanged => {
    logger.Information("OBS Source `{SourceName}` Filter `{OldFilterName}` renamed to `{FilterName}`",
        sourceFilterNameChanged.SourceName,
        sourceFilterNameChanged.OldFilterName,
        sourceFilterNameChanged.FilterName);
});

obs.ReactToSourceFilterSettingsChanged(async sourceFilterSettingsChanged => {
    logger.Information("OBS Source `{SourceName}` Filter `{FilterName}` settings changed",
        sourceFilterSettingsChanged.SourceName,
        sourceFilterSettingsChanged.FilterName);
});

obs.ReactToSourceFilterEnableStateChanged(async sourceFilterEnableStateChanged => {
    logger.Information("OBS Source `{SourceName}` Filter `{FilterName}` {FilterEnabled}",
        sourceFilterEnableStateChanged.SourceName,
        sourceFilterEnableStateChanged.FilterName,
        sourceFilterEnableStateChanged.FilterEnabled ? "Enabled" : "Disabled");
});

obs.ReactToExitStarted(async () => {
    logger.Information("OBS Exit Started");
});

obs.ReactToInputCreated(async inputCreated => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` of kind `{InputKind}` (Unversioned `{UnversionedInputKind})` created",
        inputCreated.InputName,
        inputCreated.InputUuid,
        inputCreated.InputKind,
        inputCreated.UnversionedInputKind);
});

obs.ReactToInputRemoved(async inputRemoved => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` removed",
        inputRemoved.InputName,
        inputRemoved.InputUuid);
});

obs.ReactToInputNameChanged(async inputNameChanged => {
    logger.Information("OBS Input `{OldInputName}` `{InputUuid}` renamed to `{InputName}`",
        inputNameChanged.OldInputName,
        inputNameChanged.InputUuid,
        inputNameChanged.InputName);
});

obs.ReactToInputSettingsChanged(async inputSettingsChanged => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` settings changed",
        inputSettingsChanged.InputName,
        inputSettingsChanged.InputUuid);
});

obs.ReactToInputActiveStateChanged(async inputActiveStateChanged => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` {VideoActive}",
        inputActiveStateChanged.InputName,
        inputActiveStateChanged.InputUuid,
        inputActiveStateChanged.VideoActive ? "Activated" : "Deactivated");
});

obs.ReactToInputShowStateChanged(async inputShowStateChanged => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` {VideoActive}",
        inputShowStateChanged.InputName,
        inputShowStateChanged.InputUuid,
        inputShowStateChanged.VideoShowing ? "Shown" : "Hidden");
});

obs.ReactToInputMuteStateChanged(async inputMuteStateChanged => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` {VideoActive}",
        inputMuteStateChanged.InputName,
        inputMuteStateChanged.InputUuid,
        inputMuteStateChanged.InputMuted ? "Muted" : "Unmuted");
});

obs.ReactToInputVolumeChanged(async inputVolumeChanged => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` volume changed, Multiplier `{InputVolumeMul}` dB `{InputVolumeDb}`",
        inputVolumeChanged.InputName,
        inputVolumeChanged.InputUuid,
        inputVolumeChanged.InputVolumeMul,
        inputVolumeChanged.InputVolumeDb);
});

obs.ReactToInputAudioBalanceChanged(async inputAudioBalanceChanged => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` audio balance changed to `{InputAudioBalance}`",
        inputAudioBalanceChanged.InputName,
        inputAudioBalanceChanged.InputUuid,
        inputAudioBalanceChanged.InputAudioBalance);
});

obs.ReactToInputAudioSyncOffsetChanged(async inputAudioSyncOffsetChanged => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` audio sync offset changed to `{InputAudioSyncOffset}`",
        inputAudioSyncOffsetChanged.InputName,
        inputAudioSyncOffsetChanged.InputUuid,
        inputAudioSyncOffsetChanged.InputAudioSyncOffset);
});

obs.ReactToInputAudioTracksChanged(async inputAudioTracksChanged => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` audio tracks changed",
        inputAudioTracksChanged.InputName,
        inputAudioTracksChanged.InputUuid);
});

obs.ReactToInputAudioMonitorTypeChanged(async inputAudioMonitorTypeChanged => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` audio monitor type changed to `{MonitorType}`",
        inputAudioMonitorTypeChanged.InputName,
        inputAudioMonitorTypeChanged.InputUuid,
        inputAudioMonitorTypeChanged.MonitorType);
});

obs.ReactToInputVolumeMeters(async inputVolumeMeters => {
    logger.Information("OBS Input Volume Meters event received");
});

obs.ReactToMediaInputPlaybackStarted(async mediaInputPlaybackStarted => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` media playback started",
        mediaInputPlaybackStarted.InputName,
        mediaInputPlaybackStarted.InputUuid);
});

obs.ReactToMediaInputPlaybackEnded(async mediaInputPlaybackEnded => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` media playback ended",
        mediaInputPlaybackEnded.InputName,
        mediaInputPlaybackEnded.InputUuid);
});

obs.ReactToMediaInputActionTriggered(async mediaInputActionTriggered => {
    logger.Information("OBS Input `{InputName}` `{InputUuid}` media action `{MediaAction}` triggered",
        mediaInputActionTriggered.InputName,
        mediaInputActionTriggered.InputUuid,
        mediaInputActionTriggered.MediaAction);
});

obs.ReactToStreamStateChanged(async streamStateChanged => {
    logger.Information("OBS Stream state changed to `{OutputActive}` ({OutputState})",
        streamStateChanged.OutputActive ? "Active" : "Inactive",
        streamStateChanged.OutputState);
});

obs.ReactToRecordStateChanged(async recordStateChanged => {
    logger.Information("OBS Recording state changed to `{OutputActive}` ({OutputState}), saved as: `{OutputPath}`",
        recordStateChanged.OutputActive ? "Active" : "Inactive",
        recordStateChanged.OutputState,
        recordStateChanged.OutputPath ?? "<SAVE FAILED>");
});

obs.ReactToRecordFileChanged(async recordFileChanged => {
    logger.Information("OBS Recording output file changed to: `{OutputActive}`",
        recordFileChanged.NewOutputPath);
});

obs.ReactToReplayBufferStateChanged(async replayBufferStateChanged => {
    logger.Information("OBS Replay Buffer state changed to `{OutputActive}` ({OutputState})",
        replayBufferStateChanged.OutputActive ? "Active" : "Inactive",
        replayBufferStateChanged.OutputState);
});

obs.ReactToVirtualcamStateChanged(async virtualcamStateChanged => {
    logger.Information("OBS Virtualcam state changed to `{OutputActive}` ({OutputState})",
        virtualcamStateChanged.OutputActive ? "Active" : "Inactive",
        virtualcamStateChanged.OutputState);
});

obs.ReactToReplayBufferSaved(async replayBufferSaved => {
    logger.Information("OBS Replay Buffer saved to: `{OutputActive}`",
        replayBufferSaved.SavedReplayPath);
});

obs.ReactToSceneItemCreated(async sceneItemCreated => {
    logger.Information("OBS Source `{SourceName}` `{SourceUuid}` added to scene `{SceneName}` `{SceneUuid}` with id {SceneItemId} at index {SceneItemIndex}",
        sceneItemCreated.SourceName,
        sceneItemCreated.SourceUuid,
        sceneItemCreated.SceneName,
        sceneItemCreated.SceneUuid,
        sceneItemCreated.SceneItemId,
        sceneItemCreated.SceneItemIndex);
});

obs.ReactToSceneItemRemoved(async sceneItemRemoved => {
    logger.Information("OBS Source `{SourceName}` `{SourceUuid}` with id {SceneItemId} removed from scene `{SceneName}` `{SceneUuid}`",
        sceneItemRemoved.SourceName,
        sceneItemRemoved.SourceUuid,
        sceneItemRemoved.SceneName,
        sceneItemRemoved.SceneUuid,
        sceneItemRemoved.SceneItemId);
});

obs.ReactToSceneItemListReindexed(async sceneItemListReindexed => {
    logger.Information("OBS Scene `{SceneName}` `{SceneUuid}` item list reindexed",
        sceneItemListReindexed.SceneName,
        sceneItemListReindexed.SceneUuid);
});

obs.ReactToSceneItemEnableStateChanged(async sceneItemEnableStateChanged => {
    logger.Information("OBS Scene item {SceneItemId} {SceneItemEnabled} in scene `{SceneName}` `{SceneUuid}`",
        sceneItemEnableStateChanged.SceneItemId,
        sceneItemEnableStateChanged.SceneItemEnabled ? "Enabled" : "Disabled",
        sceneItemEnableStateChanged.SceneName,
        sceneItemEnableStateChanged.SceneUuid);
});

obs.ReactToSceneItemLockStateChanged(async sceneItemLockStateChanged => {
    logger.Information("OBS Scene item {SceneItemId} {SceneItemLocked} in scene `{SceneName}` `{SceneUuid}`",
        sceneItemLockStateChanged.SceneItemId,
        sceneItemLockStateChanged.SceneItemLocked ? "Locked" : "Unlocked",
        sceneItemLockStateChanged.SceneName,
        sceneItemLockStateChanged.SceneUuid);
});

obs.ReactToSceneItemSelected(async sceneItemSelected => {
    logger.Information("OBS Scene `{SceneName}` `{SceneUuid}` item {SceneItemId} selected",
        sceneItemSelected.SceneName,
        sceneItemSelected.SceneUuid,
        sceneItemSelected.SceneItemId);
});

obs.ReactToSceneItemTransformChanged(async sceneItemTransformChanged => {
    logger.Information("OBS Scene `{SceneName}` `{SceneUuid}` item {SceneItemId} transform changed",
        sceneItemTransformChanged.SceneName,
        sceneItemTransformChanged.SceneUuid,
        sceneItemTransformChanged.SceneItemId);
});

obs.ReactToSceneCreated(async sceneCreated => {
    logger.Information("OBS Scene {IsGroup}`{SceneName}` `{SceneUuid}` created",
        sceneCreated.IsGroup ? "group " : "",
        sceneCreated.SceneName,
        sceneCreated.SceneUuid);
});

obs.ReactToSceneRemoved(async sceneRemoved => {
    logger.Information("OBS Scene {IsGroup}`{SceneName}` `{SceneUuid}` removed",
        sceneRemoved.IsGroup ? "group " : "",
        sceneRemoved.SceneName,
        sceneRemoved.SceneUuid);
});

obs.ReactToSceneNameChanged(async sceneNameChanged => {
    logger.Information("OBS Scene `{OldSceneName}` `{SceneUuid}` name changed to `{SceneName}`",
        sceneNameChanged.OldSceneName,
        sceneNameChanged.SceneUuid,
        sceneNameChanged.SceneName);
});

obs.ReactToCurrentProgramSceneChanged(async currentProgramSceneChanged => {
    logger.Information("OBS Current program scene changed to `{SceneName}` `{SceneUuid}`",
        currentProgramSceneChanged.SceneName,
        currentProgramSceneChanged.SceneUuid);
});

obs.ReactToCurrentPreviewSceneChanged(async currentPreviewSceneChanged => {
    logger.Information("OBS Current preview scene changed to `{SceneName}` `{SceneUuid}`",
        currentPreviewSceneChanged.SceneName,
        currentPreviewSceneChanged.SceneUuid);
});

obs.ReactToSceneListChanged(async sceneListChanged => {
    logger.Information("OBS Scene list changed");
});

obs.ReactToCurrentSceneTransitionChanged(async currentSceneTransitionChanged => {
    logger.Information("OBS Current scene transition changed to `{TransitionName}` `{TransitionUuid}`",
        currentSceneTransitionChanged.TransitionName,
        currentSceneTransitionChanged.TransitionUuid);
});

obs.ReactToCurrentSceneTransitionDurationChanged(async currentSceneTransitionDurationChanged => {
    logger.Information("OBS Current scene transition duration changed to `{TransitionDuration}`",
        currentSceneTransitionDurationChanged.TransitionDuration);
});

obs.ReactToSceneTransitionStarted(async sceneTransitionStarted => {
    logger.Information("OBS Current scene transition `{TransitionName}` `{TransitionUuid}` started",
        sceneTransitionStarted.TransitionName,
        sceneTransitionStarted.TransitionUuid);
});

obs.ReactToSceneTransitionEnded(async sceneTransitionEnded => {
    logger.Information("OBS Current scene transition `{TransitionName}` `{TransitionUuid}` ended",
        sceneTransitionEnded.TransitionName,
        sceneTransitionEnded.TransitionUuid);
});

obs.ReactToSceneTransitionVideoEnded(async sceneTransitionVideoEnded => {
    logger.Information("OBS Current scene Video transition `{TransitionName}` `{TransitionUuid}` ended",
        sceneTransitionVideoEnded.TransitionName,
        sceneTransitionVideoEnded.TransitionUuid);
});

obs.ReactToStudioModeStateChanged(async studioModeStateChanged => {
    logger.Information("OBS Studio mode state {StudioModeEnabled}",
        studioModeStateChanged.StudioModeEnabled ? "Enabled" : "Disabled");
});

obs.ReactToScreenshotSaved(async screenshotSaved => {
    logger.Information("OBS Screenshot saved to: `{SavedScreenshotPath}`",
        screenshotSaved.SavedScreenshotPath);
});

obs.ReactToVendorEvent(async vendorEvent => {
    logger.Information("OBS Vendor `{VendorName}` event of type `{EventType}` triggered",
        vendorEvent.VendorName,
        vendorEvent.EventType);
});

obs.ReactToCustomEvent(async customEvent => {
    logger.Information("OBS Custom event triggered");
});

async void LogHotkeyNames() {
    var hotkeys = await obs.GetHotkeyList();
    var resultStr = "OBS available hotkeys:";

    foreach (var item in hotkeys) {
        resultStr += $"\r\n  - {item}";
    }

    logger.Information(resultStr);
}

/**
 * Use method as follows:
 * SwapSceneForTime("My Funny Scene", TimeSpan.FromSeconds(10));
 * SwapSceneForTime("My Epic Scene", TimeSpan.FromMinutes(5));
 */
void SwapSceneForTime(string newScene, TimeSpan duration) {
    var task = Task.Run(async () => {
        // Retrieve the current scene
        var oldScene = await obs.GetActiveScene();
        // Swap to the new scene
        await obs.SetActiveScene(newScene); // Defaults to "Cut" transition
        // Delay the remaindor for the indicated duration
        await Task.Delay(duration);
        // Swap bach to the original scene
        await obs.SetActiveScene(oldScene);
    });
    task.ConfigureAwait(false);
}

#endif

#if SUBLINK_OPENSHOCK

logger.Information("[{TAG}] OpenShock integration enabled", "Script");
var openShock = (OpenShockRules)rules["OpenShock"];

async void LogOwnShockers() {
    var shockerInfo = await openShock.GetOwnShockers();
    string resultStr = "";

    foreach (var hub in shockerInfo) {
        resultStr += $"Hub `{hub.Name}` ({hub.Id}) has the following shockers:\r\n";

        foreach (var shocker in hub.Shockers) {
            resultStr += $"  - `{shocker.Name}` ({shocker.Id}) state: {(shocker.IsPaused ? "Paused" : "Live")}\r\n";
        }
    }

    logger.Information(resultStr);
}

#endif

#if SUBLINK_DISCORD

logger.Information("[{TAG}] Discord integration enabled", "Script");
var discord = (DiscordRules)rules["Discord"];

discord.ReactToReady(async () => {
    logger.Information("Discord is connected and ready");
});

discord.ReactToError(async error => {
    logger.Information("Discord Error: ({CODE}) {MESSAGE}", error.Code, error.Message);
});

discord.ReactToGuildStatus(async guild => {
    logger.Information("Discord guild status updated for {NAME} ({ID})", guild.Name, guild.Id);
});

discord.ReactToGuildCreate(async guild => {
    logger.Information("Discord guild `{NAME}` created with ID {ID}", guild.Name, guild.Id);
});

discord.ReactToSelectedVoiceChannel(async voiceChannel => {
    logger.Information("Discord voice channel selection changed to: {ID}", voiceChannel.ChannelId);
});

discord.ReactToVoiceSettingsUpdate(async voiceSettings => {
    logger.Information("""
Discord voice settings updated.
- Input Volume: {inVol}
- Output Volume: {outVol}
- Voice Activation Type: {type}
- Voice Activation Auto Threshold: {autoThreshold}
- Voice Activation Threshold: {threshold}
- Voice Activation Release Delay: {delay}
- Voice Auto Gain: {autoGain}
- Voice Echo Cancelation: {echoCancelation}
- Voice QoS: {qos}
- Voice Silence Warning: {warn}
- Voice Deafened: {deaf}
- Voice Muted: {mute}
""",
        voiceSettings.InputVolume, voiceSettings.OutputVolume,
        voiceSettings.ModeType, voiceSettings.ModeAutoThreshold, voiceSettings.ModeThreshold, voiceSettings.ModeDelay,
        voiceSettings.AutoGainControl, voiceSettings.EchoCancelation, voiceSettings.Qos,
        voiceSettings.SilenceWarning, voiceSettings.Deaf, voiceSettings.Mute
    );
});

discord.ReactToVoiceStateCreate(async voiceState => {
    logger.Information("Discord voice state created for `{NICKNAME}` ({ID})", voiceState.Nickname, voiceState.Id);
});

discord.ReactToVoiceStateUpdate(async voiceState => {
    logger.Information("Discord voice state updated for `{NICKNAME}` ({ID})", voiceState.Nickname, voiceState.Id);
});

discord.ReactToVoiceStateDelete(async voiceState => {
    logger.Information("Discord voice state deleted for `{NICKNAME}` ({ID})", voiceState.Nickname, voiceState.Id);
});

discord.ReactToVoiceStatusUpdate(async voiceStatus => {
    logger.Information("Discord voice status updated. State: {state}, State Code: {stateCode}",
        voiceStatus.State, voiceStatus.StateCode);
});

discord.ReactToChannelCreate(async channel => {
    logger.Information("Discord channel `{NAME}` created with ID {ID}", channel.Name, channel.Id);
});

discord.ReactToMessageCreate(async message => {
    logger.Information("Discord message created with ID: {ID}", message.MessageId);
});

discord.ReactToMessageUpdate(async message => {
    logger.Information("Discord message updated with ID: {ID}", message.MessageId);
});

discord.ReactToMessageDelete(async message => {
    logger.Information("Discord message deleted with ID: {ID}", message.MessageId);
});

discord.ReactToStartSpeaking(async userId => {
    logger.Information("Discord user with ID {ID} started speaking", userId);
});

discord.ReactToStopSpeaking(async userId => {
    logger.Information("Discord user with ID {ID} stopped speaking", userId);
});

discord.ReactToNotificationCreate(async notification => {
    logger.Information("Discord notification created for channel `{CHANNEL_ID}`: {TITLE} \"{BODY}\"",
        notification.ChannelId, notification.Title, notification.Body);
});

discord.ReactToActivityJoin(async () => {
    logger.Information("Discord activity joined");
});

discord.ReactToActivitySpectate(async () => {
    logger.Information("Discord spectating activity");
});

discord.ReactToActivityJoinRequest(async userId => {
    logger.Information("Discord user with ID {ID} requested to join the activity", userId);
});

#endif

#if SUBLINK_JOYSTICK

logger.Information("[{TAG}] Joystick integration enabled", "Script");
var joystick = (JoystickRules)rules["Joystick"];

joystick.ReactToChatMessage(async (eventArgs) => {
    if ("yewnyx".Equals(eventArgs.AuthorSlug, StringComparison.OrdinalIgnoreCase)) {
        OscParameter.SendAvatarParameter("JacketToggle", false);
        OscParameter.SendAvatarParameter("Sus", true);
    }

    if (!string.IsNullOrWhiteSpace(eventArgs.BotCommand)) {
        string[] args = eventArgs.BotCommandArg.Split(' ');

        switch (eventArgs.BotCommand) {
            case "applejuice": {
                joystick.ChatMessage("fixes everything");
                break;
            }
            default: break; // Do nothing C:
        }
    }

    logger.Information(
        "Username: {AuthorUsername}, Slug: {AuthorSlug}, Created At: {CreatedAt}, Text: {Text}",
        eventArgs.AuthorUsername, eventArgs.AuthorSlug, eventArgs.CreatedAt, eventArgs.Text);
});

joystick.ReactToBotMessage(async (eventArgs) => {
    logger.Information(
        "Bot name: {AuthorUsername}, Created At: {CreatedAt}, Text: {Text}",
        eventArgs.AuthorUsername, eventArgs.CreatedAt, eventArgs.Text);
});

joystick.ReactToEnterStream(async (eventArgs) => {
    logger.Information("User `{Who}` entered the chat at {CreatedAt}", eventArgs.Who, eventArgs.CreatedAt);
});

joystick.ReactToLeaveStream(async (eventArgs) => {
    logger.Information("User `{Who}` left the chat at {CreatedAt}", eventArgs.Who, eventArgs.CreatedAt);
});

joystick.ReactToStarted(async (eventArgs) => {
    logger.Information("Stream started at {CreatedAt}", eventArgs.CreatedAt);
});

joystick.ReactToStreamResuming(async (eventArgs) => {
    logger.Information("Stream resumed at {CreatedAt}", eventArgs.CreatedAt);
});

joystick.ReactToStreamEnding(async (eventArgs) => {
    logger.Information("Stream ending at {CreatedAt}", eventArgs.CreatedAt);
});

joystick.ReactToEnded(async (eventArgs) => {
    logger.Information("Stream ended at {CreatedAt}", eventArgs.CreatedAt);
});

joystick.ReactToFollowed(async (eventArgs) => {
    OscParameter.SendAvatarParameter("NewFollower", true);
    logger.Information("`{Who}` followed the stream at {CreatedAt}", eventArgs.Who, eventArgs.CreatedAt);
});

joystick.ReactToFollowerCountUpdated(async (eventArgs) => {
    logger.Information(
        "Follower count updated to {NumberOfFollowers} at {CreatedAt}",
        eventArgs.NumberOfFollowers, eventArgs.CreatedAt);
});

joystick.ReactToTipped(async (eventArgs) => {
    logger.Information(
        "`{Username}` tipped `{TipMenuItem}` worth {Amount} tokens",
        eventArgs.Who, eventArgs.TipMenuItem, eventArgs.Amount);
    OscParameter.SendAvatarParameter("JoystickTip", eventArgs.Amount);
});

joystick.ReactToTipGoalCreated(async (eventArgs) => {
    logger.Information(
        "Created new goal `{Title}` for {Amount} tokens at {CreatedAt}",
        eventArgs.Title, eventArgs.Amount, eventArgs.CreatedAt);
});

joystick.ReactToTipGoalDeleted(async (eventArgs) => {
    logger.Information(
        "Deleted goal `{Title}` for {Amount} tokens at {CreatedAt}",
        eventArgs.Title, eventArgs.Amount, eventArgs.CreatedAt);
});

joystick.ReactToTipGoalIncreased(async (eventArgs) => {
    logger.Information(
        "User `{ByUser}` increased the tip goals with {Amount} tokens at {CreatedAt}. Current: {Current}, Previous: {Previous}",
        eventArgs.ByUser, eventArgs.Amount, eventArgs.CreatedAt, eventArgs.Current, eventArgs.Previous);
});

joystick.ReactToTipGoalMet(async (eventArgs) => {
    logger.Information(
        "User `{Who}` caused goal `{Title}` to be met with {Amount} tokens at {CreatedAt}",
        eventArgs.Who, eventArgs.Title, eventArgs.Amount, eventArgs.CreatedAt);
});

joystick.ReactToTipGoalUpdated(async (eventArgs) => {
    logger.Information(
        "Updated goal `{Title}` with {Amount} tokens at {CreatedAt}",
        eventArgs.Title, eventArgs.Amount, eventArgs.CreatedAt);
});

joystick.ReactToTipMenuItemLocked(async (eventArgs) => {
    logger.Information(
        "Tip menu item `{Title}` worth {Amount} tokens locked at {CreatedAt}",
        eventArgs.Title, eventArgs.Amount, eventArgs.CreatedAt);
});

joystick.ReactToTipMenuItemUnlocked(async (eventArgs) => {
    logger.Information(
        "Tip menu item `{Title}` worth {Amount} tokens unlocked at {CreatedAt}",
        eventArgs.Title, eventArgs.Amount, eventArgs.CreatedAt);
});

joystick.ReactToChatTimerStarted(async (eventArgs) => {
    logger.Information(
        "New chat timer `{Name}` started, it ends at {EndsAt}",
        eventArgs.Name, eventArgs.EndsAt);
});

joystick.ReactToChatTimersCleared(async (eventArgs) => {
    logger.Information("Chat timers have been cleared by `{Who}`", eventArgs.Who);
});

joystick.ReactToDropinStream(async (eventArgs) => {
    logger.Information(
        "Outgoing drop-in started, targeting `{Destination}` with {NumberOfViewers} viewers at {CreatedAt}",
        eventArgs.Destination, eventArgs.NumberOfViewers, eventArgs.CreatedAt);
});

joystick.ReactToStreamDroppedIn(async (eventArgs) => {
    logger.Information(
        "Incoming drop-in from `{Who}` with {NumberOfViewers} viewers at {CreatedAt}",
        eventArgs.Who, eventArgs.NumberOfViewers, eventArgs.CreatedAt);
});

joystick.ReactToSubscribed(async (eventArgs) => {
    logger.Information("`{Who}` subscribed for {Amount} months", eventArgs.Who, eventArgs.Amount);
    OscParameter.SendAvatarParameter("JoystickSubscription", eventArgs.Amount);
});

joystick.ReactToResubscribed(async (eventArgs) => {
    logger.Information(
        "`{Who}` resubscribed for {Amount} months, total sub length {Length}",
        eventArgs.Who, eventArgs.Amount, eventArgs.Length);
    OscParameter.SendAvatarParameter("JoystickResub", eventArgs.Length);
});

joystick.ReactToGiftedSubscriptions(async (eventArgs) => {
    logger.Information("`{Who}` gifted {Amount} subscriptions", eventArgs.Who, eventArgs.Amount);
    OscParameter.SendAvatarParameter("JoystickGifts", eventArgs.Amount);
});

joystick.ReactToWheelSpinClaimed(async (eventArgs) => {
    logger.Information(
        "`{Who}` spun the wheel and got {Amount} x `{Prize}`",
        eventArgs.Who, eventArgs.Amount, eventArgs.Prize);
});

joystick.ReactToViewerCountUpdated(async (eventArgs) => {
    logger.Information("Viewer count updated to {Amount}", eventArgs.Amount);
});

joystick.ReactToSubscriberCountUpdated(async (eventArgs) => {
    logger.Information("Subscriber count updated to {Amount}", eventArgs.Amount);
});

joystick.ReactToMilestoneCompleted(async (eventArgs) => {
    logger.Information("Milestone `{Title}` completed with value {Amount}", eventArgs.Title, eventArgs.Amount);
});

joystick.ReactToPvpSessionRequested(async (eventArgs) => {
    logger.Information(
        "New PVP session request by `{Who}` with cost {Cost} on {When}",
        eventArgs.Who, eventArgs.Cost, eventArgs.When);
});

joystick.ReactToPvpSessionReady(async (eventArgs) => {
    logger.Information(
        "PVP session ready for `{Who}` with cost {Cost} on {When}",
        eventArgs.Who, eventArgs.Cost, eventArgs.When);
});

joystick.ReactToPvpSessionStarted(async (eventArgs) => {
    logger.Information("PVP session started for `{Who}` on {When}", eventArgs.Who, eventArgs.When);
});

joystick.ReactToPvpSessionEnding(async (eventArgs) => {
    logger.Information("PVP session ending for `{Who}` on {When}", eventArgs.Who, eventArgs.When);
});

joystick.ReactToPvpSessionEnded(async (eventArgs) => {
    logger.Information("PVP session ended for `{Who}` on {When}", eventArgs.Who, eventArgs.When);
});

joystick.ReactToSceneUpdated(async (eventArgs) => {
    logger.Information(
        "Scene item `{Name}` updated. [ Font Size: {FontSize}, Title Color: {TitleColor}, Progress Color: {ProgressColor}, Completed Color: {CompletedColor} ]",
        eventArgs.Name, eventArgs.FontSize, eventArgs.TitleColor, eventArgs.ProgressColor, eventArgs.CompletedColor);
});

joystick.ReactToSettingsUpdated(async (eventArgs) => {
    logger.Information("Settings were updated at {CreatedAt}", eventArgs.CreatedAt);
});

joystick.ReactToStreamModeUpdated(async (eventArgs) => {
    logger.Information("Stream mode updated by `{Who}` at {CreatedAt}", eventArgs.Who, eventArgs.CreatedAt);
});

joystick.ReactToUserMuted(async (eventArgs) => {
    logger.Information("User `{Who}` has been muted at {CreatedAt}", eventArgs.Who, eventArgs.CreatedAt);
});

joystick.ReactToUserUnmuted(async (eventArgs) => {
    logger.Information("User `{Who}` has been unmuted at {CreatedAt}", eventArgs.Who, eventArgs.CreatedAt);
});

joystick.ReactToDeviceConnected(async (eventArgs) => {
    logger.Information("Device connected at {CreatedAt}", eventArgs.CreatedAt);
});

joystick.ReactToDeviceDisconnected(async (eventArgs) => {
    logger.Information("Device disconnected at {CreatedAt}", eventArgs.CreatedAt);
});

joystick.ReactToDeviceSettingsUpdated(async (eventArgs) => {
    logger.Information("Device settings were updated at {CreatedAt}", eventArgs.CreatedAt);
});

#endif

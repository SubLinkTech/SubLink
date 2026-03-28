# SubLink Actions Discord

[Back To Readme](../../../README.md)

## Legend

- [Mute](#Mute)
- [Unmute](#Unmute)
- [Deafen](#Deafen)
- [Undeafen](#Undeafen)
- [Request Selected Voice Channel](#RequestSelectedVoiceChannel)
- [Request Voice Settings](#RequestVoiceSettings)
- [Set Input Volume](#SetInputVolume)
- [Set Output Volume](#SetOutputVolume)
- [Subscribe Event](#SubscribeEvent)
- [Unsubscribe Event](#UnsubscribeEvent)
- [Select Voice Channel](#SelectVoiceChannel)
- [Select Text Channel](#SelectTextChannel)
- [Set User Volume](#SetUserVolume)
- [Mute User](#MuteUser)
- [Unmute User](#UnmuteUser)
- [Set Activity](#SetActivity)

## Mute

Mutes you in the voicechannel.

- Parameters: Nothing
- Returns: Nothing

```csharp
discord.Mute();
```

[Back To Legend](#Legend)

## Unmute

Unmutes you in the voicechannel.

- Parameters: Nothing
- Returns: Nothing

```csharp
discord.Unmute();
```

[Back To Legend](#Legend)

## Deafen

Deafens you in the voicechannel.

- Parameters: Nothing
- Returns: Nothing

```csharp
discord.Deafen();
```

[Back To Legend](#Legend)

## Undeafen

Undeafens you in the voicechannel.

- Parameters: Nothing
- Returns: Nothing

```csharp
discord.Undeafen();
```

[Back To Legend](#Legend)

## RequestSelectedVoiceChannel

Request info about the currently selected voice channel.
The result is returned in the `ReactToSelectedVoiceChannel` event.

- Parameters: Nothing
- Returns: Nothing

```csharp
discord.RequestSelectedVoiceChannel();
```

[Back To Legend](#Legend)

## RequestVoiceSettings

Request the current voice settings (Input- and output-volumes).
The result is returned in the `ReactToVoiceSettingsUpdate` event.

- Parameters: Nothing
- Returns: Nothing

```csharp
discord.RequestVoiceSettings();
```

[Back To Legend](#Legend)

## SetInputVolume

Sets the input volume to the requested amount.

- Parameters
   - `float` vol - required - The new input volume
- Returns: Nothing

```csharp
discord.SetInputVolume(90.75f);
```

[Back To Legend](#Legend)

## SetOutputVolume

Sets the output volume to the requested amount.

- Parameters
   - `float` vol - required - The new output volume
- Returns: Nothing

```csharp
discord.SetOutputVolume(93.123f);
```

[Back To Legend](#Legend)

## SubscribeEvent

Subscribe to a specific Discord event.

- Parameters
   - `string` eventName - required - Name of the event to subscribe to
   - `string` id        - optional - Event-specific ID
- Returns: Nothing

Default subscribed events:
- READY
- ERROR
- GUILD_CREATE
- CHANNEL_CREATE
- VOICE_CHANNEL_SELECT
- VOICE_SETTINGS_UPDATE
- VOICE_CONNECTION_STATUS
- NOTIFICATION_CREATE
- ACTIVITY_JOIN
- ACTIVITY_SPECTATE
- ACTIVITY_JOIN_REQUEST

Available events:
- GUILD_STATUS  
  Sent when a subscribed server’s state changes  
  Requires `guild_id`
- VOICE_STATE_CREATE  
  Sent when a user joins a subscribed voice channel  
  Requires `channel_id`
- VOICE_STATE_UPDATE  
  Sent when a user’s voice state changes in a subscribed voice channel (mute, volume, etc.)  
  Requires `channel_id`
- VOICE_STATE_DELETE  
  Sent when a user parts a subscribed voice channel  
  Requires `channel_id`
- MESSAGE_CREATE  
  Sent when a message is created in a subscribed text channel  
  Requires `channel_id`
- MESSAGE_UPDATE  
  Sent when a message is updated in a subscribed text channel  
  Requires `channel_id`
- MESSAGE_DELETE  
  Sent when a message is deleted in a subscribed text channel  
  Requires `channel_id`
- SPEAKING_START  
  Sent when a user in a subscribed voice channel speaks  
  Requires `channel_id`
- SPEAKING_STOP  
  Sent when a user in a subscribed voice channel stops speaking  
  Requires `channel_id`

```csharp
discord.SubscribeEvent("VOICE_CHANNEL_SELECT");
discord.SubscribeEvent("SPEAKING_START", "123_channel_id_789");
```

[Back To Legend](#Legend)

## UnsubscribeEvent

Unsubscribe from a specific Discord event.  
Don't unsubscribe from default events, this may break functionality.

- Parameters
   - `string` eventName - required - Name of the event to unsubscribe from
   - `string` id        - optional - Event-specific ID
- Returns: Nothing

Available events:
- GUILD_STATUS  
  Sent when a subscribed server’s state changes  
  Requires `guild_id`
- VOICE_STATE_CREATE  
  Sent when a user joins a subscribed voice channel  
  Requires `channel_id`
- VOICE_STATE_UPDATE  
  Sent when a user’s voice state changes in a subscribed voice channel (mute, volume, etc.)  
  Requires `channel_id`
- VOICE_STATE_DELETE  
  Sent when a user parts a subscribed voice channel  
  Requires `channel_id`
- MESSAGE_CREATE  
  Sent when a message is created in a subscribed text channel  
  Requires `channel_id`
- MESSAGE_UPDATE  
  Sent when a message is updated in a subscribed text channel  
  Requires `channel_id`
- MESSAGE_DELETE  
  Sent when a message is deleted in a subscribed text channel  
  Requires `channel_id`
- SPEAKING_START  
  Sent when a user in a subscribed voice channel speaks  
  Requires `channel_id`
- SPEAKING_STOP  
  Sent when a user in a subscribed voice channel stops speaking  
  Requires `channel_id`

```csharp
discord.UnsubscribeEvent("VOICE_CHANNEL_SELECT");
discord.UnsubscribeEvent("SPEAKING_START", "123_channel_id_789");
```

[Back To Legend](#Legend)

## SelectVoiceChannel

Select a voice channel.

- Parameters
   - `string` channelId - required - ID of the channel to select
   - `bool`   force     - optional - Force channel changing when already connected to a VC (defaults to true)
- Returns: Nothing

```csharp
discord.SelectVoiceChannel("123_channel_id_789");
discord.SelectVoiceChannel("123_channel_id_789", false);
```

[Back To Legend](#Legend)

## SelectTextChannel

Select a text channel.

- Parameters
   - `string` channelId - required - ID of the channel to select
- Returns: Nothing

```csharp
discord.SelectTextChannel("123_channel_id_789");
```

[Back To Legend](#Legend)

## SetUserVolume

Sets the voice volume for another user.

- Parameters
   - `string` userId - required - ID of the user to set the volume for
   - `int`    vol    - required - Volume to set the user to
- Returns: Nothing

```csharp
discord.SetUserVolume("123_user_id_789", 50);
```

[Back To Legend](#Legend)

## MuteUser

Mute another user.

- Parameters
   - `string` userId - required - ID of the user to mute
- Returns: Nothing

```csharp
discord.MuteUser("123_user_id_789");
```

[Back To Legend](#Legend)

## UnmuteUser

Unmute another user.

- Parameters
   - `string` userId - required - ID of the user to unmute
- Returns: Nothing

```csharp
discord.UnmuteUser("123_user_id_789");
```

[Back To Legend](#Legend)

## SetActivity

Sets the current Discord activity.

- Asynchronous
- Parameters
   - `string` state   - required - Activity title
   - `string` details - required - Activity details
- Returns: Nothing

```csharp
discord.SetActivity("Important!", "SubLink is super duper sexy");
```

[Back To Legend](#Legend)

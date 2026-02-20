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
discord.SetInputVolume(0.75f);
```

[Back To Legend](#Legend)

## SetOutputVolume

Sets the output volume to the requested amount.

- Parameters
   - `float` vol - required - The new output volume
- Returns: Nothing

```csharp
discord.SetOutputVolume(0.25f);
```

[Back To Legend](#Legend)

## SubscribeEvent

Subscribe to a specific Discord event.

- Parameters
   - `string` eventName - required - Name of the event to subscribe to
   - `string` id        - optional - Event-specific ID
- Returns: Nothing

```csharp
discord.SubscribeEvent("VOICE_CHANNEL_SELECT");
discord.SubscribeEvent("SPEAKING_START", "123_channel_id_789");
```

[Back To Legend](#Legend)

## UnsubscribeEvent

Unsubscribe from a specific Discord event.

- Parameters
   - `string` eventName - required - Name of the event to unsubscribe from
   - `string` id        - optional - Event-specific ID
- Returns: Nothing

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
   - `float`  vol    - required - Volume to set the user to
- Returns: Nothing

```csharp
discord.SetUserVolume("123_user_id_789", 0.15f);
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

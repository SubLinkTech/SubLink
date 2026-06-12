# SubLink Actions Joystick

[Back To Readme](../../../README.md)

## Legend

- [ChatMessage](#chatmessage)
- [DeleteMessage](#deletemessage)
- [Whisper](#whisper)
- [MuteUser](#muteuser)
- [UnmuteUser](#unmuteuser)
- [BlockUser](#blockuser)

## ChatMessage

Sends a chat message as the bot.

- Parameters:
   - `string` text - required - The chat message's text
- Returns: Nothing

```csharp
joystick.ChatMessage("Applejuice fixes everything");
```

[Back To Legend](#Legend)

## DeleteMessage

Deletes a chat message.

- Parameters:
   - `string` messageId - required - The chat message's GUID
- Returns: Nothing

```csharp
joystick.DeleteMessage("00000000-0000-0000-0000-000000000000");
```

[Back To Legend](#Legend)

## Whisper

Send a whisper message to a user as the bot.

- Parameters:
   - `string` username - required - The target username
   - `string` text     - required - The whisper message's text
- Returns: Nothing

```csharp
joystick.Whisper("LauraRozier", "Applejuice fixes everything");
```

[Back To Legend](#Legend)

## MuteUser

Chat-mute a user.

- Parameters:
   - `string` messageId - required - The message ID of the user that is to be muted
- Returns: Nothing

```csharp
joystick.MuteUser("00000000-0000-0000-0000-000000000000");
```

[Back To Legend](#Legend)

## UnmuteUser

Remove a chat-mute from a user.

- Parameters:
   - `string` username - required - The user to un-mute
- Returns: Nothing

```csharp
joystick.UnmuteUser("LauraRozier");
```

[Back To Legend](#Legend)

## BlockUser

Blocks a user from the channel.  
**Note** There is no UnblockUser API endpoint by Joystick's own design. The streamer will have to manualy unblock them via the website.

- Parameters:
   - `string` messageId - required - The message ID of the user that is to be banned
- Returns: Nothing

```csharp
joystick.BlockUser("00000000-0000-0000-0000-000000000000");
```

[Back To Legend](#Legend)

# SubLink DataTypes Joystick Types

[Back To Readme](../../../README.md)  
[Back To Joystick DataTypes Index](Index.md)

- `int`    Code    - Error code
- `string` Message - Error message
- `bool`   Message - Error message

## JoystickEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)

## JoystickChatMessageEventArgs

- `string`   Text                   - Event in text format
- `string`   CreatedAt              - Event creation timestamp (in UTC)
- `string`   MessageId              - The message's GUID
- `string`   Visibility             - The message's visibility
- `string`   BotCommand             - The message's bot command
- `string`   BotCommandArg          - The message's bot command arguments
- `string[]` EmotesUsed             - Array of the message's emotes (undocumented, probably empty)
- `string`   AuthorSlug             - The message author's slug
- `string`   AuthorUsername         - The message author's username
- `string`   AuthorNickname         - The message author's nickname
- `bool`     AuthorIsStreamer       - Indicates whether the message was sent by the streamer
- `bool`     AuthorIsModerator      - Indicates whether the message was sent by a channel moderator
- `bool`     AuthorIsSubscriber     - Indicates whether the message was sent by a subscriber
- `bool`     AuthorIsVerified       - Indicates whether the message was sent by a verified user
- `bool`     AuthorIsContentCreator - Indicates whether the message was sent by a content creator
- `string`   StreamerSlug           - The streamer's slug
- `string`   StreamerUsername       - The streamer's username
- `string`   ChannelId              - The ID of the Channel the message was sent in
- `bool`     Mention                - Indicates whether the message mentions someone or not
- `string`   MentionedUsername      - Username of the mentioned user
- `bool`     Highlight              - Indicates whether the message in highlighted or not

## JoystickBotMessageEventArgs

- `string`   Text           - Event in text format
- `string`   CreatedAt      - Event creation timestamp (in UTC)
- `string`   MessageId      - The message's GUID
- `string`   Visibility     - The message's visibility
- `string[]` EmotesUsed     - Array of the message's emotes (undocumented, probably empty)
- `string`   AuthorUsername - The message author's username
- `bool`     Mention        - Indicates whether the message mentions someone or not

## JoystickEnterStreamEventArgs

- `string` Who       - Username of the person entering the chat
- `string` CreatedAt - Event creation timestamp (in UTC)

## JoystickLeaveStreamEventArgs

- `string` Who       - Username of the person leaving the chat
- `string` CreatedAt - Event creation timestamp (in UTC)

## JoystickWhoEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the person causing the event

## JoystickWhoWhatEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the person causing the event
- `string` What      - Event's descriptive type

## JoystickWhoWhenWhereEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the person causing the event
- `string` When      - Timestamp of when the event is set to occur (in UTC)
- `string` Where     - Originating channel name

## JoystickWhoWhatWhenWhereEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the person causing the event
- `string` What      - Event's descriptive type
- `string` When      - Timestamp of when the event is set to occur (in UTC)
- `string` Where     - Originating channel name

## JoystickFollowerCountUpdatedEventArgs

- `string` Text              - Event in text format
- `string` CreatedAt         - Event creation timestamp (in UTC)
- `long`   NumberOfFollowers - Current number of followers

## JoystickTippedEventArgs

- `string` Text        - Event in text format
- `string` CreatedAt   - Event creation timestamp (in UTC)
- `string` Who         - Username of the person causing the event
- `string` What        - Event's descriptive type
- `long`   Amount      - The tip's token count
- `string` TipMenuItem - Name of the purchased item on Tip Menu

## JoystickTitleAmountEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Title     - The event's title
- `long`   Amount    - The event's numeric value / count

## JoystickTipGoalIncreasedEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the person causing the event
- `string` What      - Event's descriptive type
- `long`   Amount    - The goal's progress amount
- `string` ByUser    - Name of the user causing the goal to progress
- `long`   Current   - The current goal progress amount
- `long`   Previous  - The previous goal progress amount

## JoystickWhoWhatTitleAmountEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the person causing the event
- `string` What      - Event's descriptive type
- `string` Title     - The event's title
- `long`   Amount    - The event's numeric value / count

## JoystickChatTimerStartedEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Name      - Timer's name
- `string` EndsAt    - Timer's end timestamp (in UTC)

## JoystickDropinStreamEventArgs

- `string` Text                - Event in text format
- `string` CreatedAt           - Event creation timestamp (in UTC)
- `string` Origin              - Name of the originating streamer
- `string` Destination         - Name of the drop-in (raid) target streamer
- `long`   NumberOfViewers     - Number of users in the drop-ip (raid)
- `string` DestinationUsername - Username of the drop-in (raid) target streamer

## JoystickStreamDroppedInEventArgs

- `string` Text            - Event in text format
- `string` CreatedAt       - Event creation timestamp (in UTC)
- `string` Who             - Username of the drop-in's (raid) originating streamer
- `string` What            - Event's descriptive type
- `long`   NumberOfViewers - Number of users in the drop-ip (raid)

## JoystickWhoWhatAmountEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the drop-in's (raid) originating streamer
- `string` What      - Event's descriptive type
- `long`   Amount    - The event's numeric value / count

## JoystickResubscribedEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the drop-in's (raid) originating streamer
- `string` What      - Event's descriptive type
- `long`   Amount    - The lotal amount of subscribed months (I guess? No docs)
- `long`   Length    - The length of the resubscription (I guess? No docs)

## JoystickWheelSpinClaimedEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the drop-in's (raid) originating streamer
- `string` What      - Event's descriptive type
- `long`   Amount    - Prize's amount
- `string` Prize     - Prize name

## JoystickAmountEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `long`   Amount    - The event's numeric value / count

## JoystickPvpSessionRequestedEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the person causing the event
- `string` What      - Event's descriptive type
- `string` When      - Timestamp of when the event is set to occur (in UTC)
- `string` Where     - Originating channel name
- `long`   Cost      - PVP session's participation cost

## JoystickPvpSessionReadyEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the person causing the event
- `string` When      - Timestamp of when the event is set to occur (in UTC)
- `string` Where     - Originating channel name
- `long`   Cost      - PVP session's participation cost

## JoystickPvpSessionStartedEventArgs

- `string` Text      - Event in text format
- `string` CreatedAt - Event creation timestamp (in UTC)
- `string` Who       - Username of the person causing the event
- `string` What      - Event's descriptive type
- `string` When      - Timestamp of when the event is set to occur (in UTC)
- `string` Where     - Originating channel name
- `string` State     - PVP session's state

## JoystickSceneUpdatedEventArgs

- `string` Text           - Event in text format
- `string` CreatedAt      - Event creation timestamp (in UTC)
- `string` Name           - Name of the modified overlay
- `string` FontSize       - Size of the overlay's font
- `string` TitleColor     - Color of the overlay's title
- `string` ProgressColor  - Color of the overlay's progress bar
- `string` CompletedColor - Color of the overlay's completed progress bar

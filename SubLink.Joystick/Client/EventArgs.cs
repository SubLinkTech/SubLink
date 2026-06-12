using System;

namespace xyz.yewnyx.SubLink.Joystick.Client;

internal sealed class JoystickErrorEventArgs : EventArgs {
    public Exception Exception { get; set; } = new();

    public JoystickErrorEventArgs() { }

    public JoystickErrorEventArgs(Exception exception) {
        Exception = exception;
    }
}

public class JoystickEventArgs : EventArgs {
    public string Text { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;

    public JoystickEventArgs() { }

    public JoystickEventArgs(string text, string createdAt) =>
        (Text, CreatedAt) = (text, createdAt);
}

public class JoystickChatMessageEventArgs : JoystickEventArgs {
    public string MessageId { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string BotCommand {  get; set; } = string.Empty;
    public string BotCommandArg {  get; set; } = string.Empty;
    public string[] EmotesUsed { get; set; } = [];
    public string AuthorSlug { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorNickname { get; set; } = string.Empty;
    public bool AuthorIsStreamer { get; set; } = false;
    public bool AuthorIsModerator { get; set; } = false;
    public bool AuthorIsSubscriber { get; set; } = false;
    public bool AuthorIsVerified { get; set; } = false;
    public bool AuthorIsContentCreator { get; set; } = false;
    public string StreamerSlug { get; set; } = string.Empty;
    public string StreamerUsername { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public bool Mention { get; set; } = false;
    public string MentionedUsername { get; set; } = string.Empty;
    public bool Highlight { get; set; } = false;

    public JoystickChatMessageEventArgs() { }
}

public class JoystickBotMessageEventArgs : JoystickEventArgs {
    public string MessageId { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string[] EmotesUsed { get; set; } = [];
    public string AuthorUsername { get; set; } = string.Empty;
    public bool Mention { get; set; } = false;

    public JoystickBotMessageEventArgs() { }
}

public class JoystickEnterStreamEventArgs : EventArgs {
    public string Who { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;

    public JoystickEnterStreamEventArgs() { }

    public JoystickEnterStreamEventArgs(string who, string createdAt) =>
        (Who, CreatedAt) = (who, createdAt);
}

public class JoystickLeaveStreamEventArgs : EventArgs {
    public string Who { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;

    public JoystickLeaveStreamEventArgs() { }

    public JoystickLeaveStreamEventArgs(string who, string createdAt) =>
        (Who, CreatedAt) = (who, createdAt);
}

public class JoystickWhoEventArgs : JoystickEventArgs {
    public string Who { get; set; } = string.Empty;

    public JoystickWhoEventArgs() { }

    public JoystickWhoEventArgs(string text, string createdAt, string who) =>
        (Text, CreatedAt, Who) = (text, createdAt, who);
}

public class JoystickWhoWhatEventArgs : JoystickWhoEventArgs {
    public string What { get; set; } = string.Empty;

    public JoystickWhoWhatEventArgs() { }

    public JoystickWhoWhatEventArgs(string text, string createdAt, string who, string what) =>
        (Text, CreatedAt, Who, What) = (text, createdAt, who, what);
}

public class JoystickWhoWhenWhereEventArgs : JoystickWhoEventArgs {
    public string When { get; set; } = string.Empty;
    public string Where { get; set; } = string.Empty;

    public JoystickWhoWhenWhereEventArgs() { }

    public JoystickWhoWhenWhereEventArgs(string text, string createdAt, string who, string when, string where) =>
        (Text, CreatedAt, Who, When, Where) = (text, createdAt, who, when, where);
}

public class JoystickWhoWhatWhenWhereEventArgs : JoystickWhoWhatEventArgs {
    public string When { get; set; } = string.Empty;
    public string Where { get; set; } = string.Empty;

    public JoystickWhoWhatWhenWhereEventArgs() { }

    public JoystickWhoWhatWhenWhereEventArgs(string text, string createdAt, string who, string what, string when, string where) =>
        (Text, CreatedAt, Who, What, When, Where) = (text, createdAt, who, what, when, where);
}

public class JoystickFollowerCountUpdatedEventArgs : JoystickEventArgs {
    public long NumberOfFollowers { get; set; } = 0;

    public JoystickFollowerCountUpdatedEventArgs() { }

    public JoystickFollowerCountUpdatedEventArgs(string text, string createdAt, long numberOfFollowers) =>
        (Text, CreatedAt, NumberOfFollowers) = (text, createdAt, numberOfFollowers);
}

public class JoystickTippedEventArgs : JoystickWhoWhatEventArgs {
    public long Amount { get; set; } = 0;
    public string TipMenuItem { get; set; } = string.Empty;

    public JoystickTippedEventArgs() { }

    public JoystickTippedEventArgs(string text, string createdAt, string who, string what, long amount, string tipMenuItem) =>
        (Text, CreatedAt, Who, What, Amount, TipMenuItem) = (text, createdAt, who, what, amount, tipMenuItem);
}

public class JoystickTitleAmountEventArgs : JoystickEventArgs {
    public string Title { get; set; } = string.Empty;
    public long Amount { get; set; } = 0;

    public JoystickTitleAmountEventArgs() { }

    public JoystickTitleAmountEventArgs(string text, string createdAt, string title, long amount) =>
        (Text, CreatedAt, Title, Amount) = (text, createdAt, title, amount);
}

public class JoystickTipGoalIncreasedEventArgs : JoystickWhoWhatEventArgs {
    public long Amount { get; set; } = 0;
    public string ByUser { get; set; } = string.Empty;
    public long Current { get; set; } = 0;
    public long Previous { get; set; } = 0;

    public JoystickTipGoalIncreasedEventArgs() { }

    public JoystickTipGoalIncreasedEventArgs(string text, string createdAt, string what, long amount, string byUser,
        long current, long previous) =>
        (Text, CreatedAt, What, Amount, ByUser, Current, Previous) =
            (text, createdAt, what, amount, byUser, current, previous);
}

public class JoystickWhoWhatTitleAmountEventArgs : JoystickWhoWhatEventArgs {
    public string Title { get; set; } = string.Empty;
    public long Amount { get; set; } = 0;

    public JoystickWhoWhatTitleAmountEventArgs() { }

    public JoystickWhoWhatTitleAmountEventArgs(string text, string createdAt, string who, string what, string title, long amount) =>
        (Text, CreatedAt, Who, What, Title, Amount) = (text, createdAt, who, what, title, amount);
}

public class JoystickChatTimerStartedEventArgs : JoystickEventArgs {
    public string Name { get; set; } = string.Empty;
    public string EndsAt { get; set; } = string.Empty;

    public JoystickChatTimerStartedEventArgs() { }

    public JoystickChatTimerStartedEventArgs(string text, string createdAt, string name, string endsAt) =>
        (Text, CreatedAt, Name, EndsAt) = (text, createdAt, name, endsAt);
}

public class JoystickDropinStreamEventArgs : JoystickEventArgs {
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public long NumberOfViewers { get; set; } = 0;
    public string DestinationUsername { get; set; } = string.Empty;

    public JoystickDropinStreamEventArgs() { }

    public JoystickDropinStreamEventArgs(string text, string createdAt, string origin, string destination,
        long numberOfViewers, string destinationUsername) =>
        (Text, CreatedAt, Origin, Destination, NumberOfViewers, DestinationUsername) =
            (text, createdAt, origin, destination, numberOfViewers, destinationUsername);
}

public class JoystickStreamDroppedInEventArgs : JoystickWhoWhatEventArgs {
    public long NumberOfViewers { get; set; } = 0;

    public JoystickStreamDroppedInEventArgs() { }

    public JoystickStreamDroppedInEventArgs(string text, string createdAt, string who, string what, long numberOfViewers) =>
        (Text, CreatedAt, Who, What, NumberOfViewers) = (text, createdAt, who, what, numberOfViewers);
}

public class JoystickWhoWhatAmountEventArgs : JoystickWhoWhatEventArgs {
    public long Amount { get; set; } = 0;

    public JoystickWhoWhatAmountEventArgs() { }

    public JoystickWhoWhatAmountEventArgs(string text, string createdAt, string who, string what, long amount) =>
        (Text, CreatedAt, Who, What, Amount) = (text, createdAt, who, what, amount);
}

public class JoystickResubscribedEventArgs : JoystickWhoWhatAmountEventArgs {
    public long Length { get; set; } = 0;

    public JoystickResubscribedEventArgs() { }

    public JoystickResubscribedEventArgs(string text, string createdAt, string who, string what, long amount, long length) =>
        (Text, CreatedAt, Who, What, Amount, Length) = (text, createdAt, who, what, amount, length);
}

public class JoystickWheelSpinClaimedEventArgs : JoystickWhoWhatEventArgs {
    public long Amount { get; set; } = 0;
    public string Prize { get; set; } = string.Empty;

    public JoystickWheelSpinClaimedEventArgs() { }

    public JoystickWheelSpinClaimedEventArgs(string text, string createdAt, string who, string what, long amount, string prize) =>
        (Text, CreatedAt, Who, What, Amount, Prize) = (text, createdAt, who, what, amount, prize);
}

public class JoystickAmountEventArgs : JoystickEventArgs {
    public long Amount { get; set; } = 0;

    public JoystickAmountEventArgs() { }

    public JoystickAmountEventArgs(string text, string createdAt, long amount) =>
        (Text, CreatedAt, Amount) = (text, createdAt, amount);
}

public class JoystickPvpSessionRequestedEventArgs : JoystickWhoWhatWhenWhereEventArgs {
    public long Cost { get; set; } = 0;

    public JoystickPvpSessionRequestedEventArgs() { }

    public JoystickPvpSessionRequestedEventArgs(string text, string createdAt, string who, string what, string when, string where, long cost) =>
        (Text, CreatedAt, Who, What, When, Where, Cost) = (text, createdAt, who, what, when, where, cost);
}

public class JoystickPvpSessionReadyEventArgs : JoystickWhoWhenWhereEventArgs {
    public long Cost { get; set; } = 0;

    public JoystickPvpSessionReadyEventArgs() { }

    public JoystickPvpSessionReadyEventArgs(string text, string createdAt, string who, string when, string where, long cost) =>
        (Text, CreatedAt, Who, When, Where, Cost) = (text, createdAt, who, when, where, cost);
}

public class JoystickPvpSessionStartedEventArgs : JoystickWhoWhatWhenWhereEventArgs {
    public string State { get; set; } = string.Empty;

    public JoystickPvpSessionStartedEventArgs() { }

    public JoystickPvpSessionStartedEventArgs(string text, string createdAt, string who, string what, string when, string where, string state) =>
        (Text, CreatedAt, Who, What, When, Where, State) = (text, createdAt, who, what, when, where, state);
}

public class JoystickSceneUpdatedEventArgs : JoystickEventArgs {
    public string Name { get; set; } = string.Empty;
    public string FontSize { get; set; } = string.Empty;
    public string TitleColor { get; set; } = string.Empty;
    public string ProgressColor { get; set; } = string.Empty;
    public string CompletedColor { get; set; } = string.Empty;

    public JoystickSceneUpdatedEventArgs() { }

    public JoystickSceneUpdatedEventArgs(string text, string createdAt, string name, string fontSize, string titleColor, string progressColor, string completedColor) =>
        (Text, CreatedAt, Name, FontSize, TitleColor, ProgressColor, CompletedColor) = (text, createdAt, name, fontSize, titleColor, progressColor, completedColor);
}

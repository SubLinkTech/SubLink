# SubLink Setup Twitch

[Back To Readme](../../README.md)

## Setup

1. On the first run, the application will create a `Settings/Twitch.json` file.
2. Add your `clientid` and `clientsecret` obtained from your Twitch Developer account (or provided to you by CatGirlEddie to the `Settings/Twitch.json` file).  
If you prefer setting this up yourself, see the [Authentication](#authentication) section
3. On the second run, the application will automatically authorize through Twitch and save an access token and refresh token to `Settings/Twitch.json`.


## Config Template

```json
{
  "Twitch": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "AccessToken": "",
    "RefreshToken": "",
    "Scopes": [
      "bits:read",
      "channel:manage:polls",
      "channel:manage:redemptions",
      "channel:read:hype_train",
      "channel:read:polls",
      "channel:read:redemptions",
      "channel:read:subscriptions",
      "channel:read:vips",
      "user:read:chat",
      "moderator:read:followers"
    ]
  }
}
```

## Authentication

If you prefer setting Twitch up yourself without our assistance, please follow the following steps:

1. Using your browser, navigate to the [Twitch Developer Console](https://dev.twitch.tv/console)
2. Register a new `Application`
3. Give it a fitting `Name` like `My SubLink App`, add `http://localhost:50666/authorize/` as `OAuth Redirect URL` and set the `Category` to `Application Integration`
![Twitch Auth 1](../twitch-auth-1.png "Twitch Auth 1")
4. Click `Manage` behind the newly generated application
![Twitch Auth 2](../twitch-auth-2.png "Twitch Auth 2")
5. Copy the `Client ID` (1) into your `Settings/Twitch.json`
6. Click `New Secret`, confirm the pop-up and copy the generated value (2) into your `Settings/Twitch.json`
![Twitch Auth 3](../twitch-auth-3.png "Twitch Auth 3")

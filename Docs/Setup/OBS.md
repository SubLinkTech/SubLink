# SubLink Setup OBS

[Back To Readme](../../README.md)

## Setup

1. On the first run, the application will create a `Settings/OBS.json` file.
2. Retrieve your IP, Port and Password.
   1. Open OBS
   2. Select `Tools` > `WebSocket Server Settings`
   3. Make sure `Enable WebSocket Server` is checked
   4. Select `Show Connect Info`
![OBS Socket Info](../obs-socket-info.png "OBS Socket Info")
1. Set the `Enabled` setting to `true` in `Settings/OBS.json`.
2. Add the `Server IP` value to the `Settings/OBS.json` file's `ServerIp` setting.
3. Add the `Server Port` value to the `Settings/OBS.json` file's `ServerPort` setting.
4. Add the `Server Password` value to the `Settings/OBS.json` file's `ServerPassword` setting.
5. On the second run, the application will automatically connect to OBS' real-time API and start receiving events.

## Config Template

```json
{
  "OBS": {
    "Enabled": false,
    "ServerIp": "127.0.0.1",
    "ServerPort": 4455,
    "ServerPassword": "your-password"
  }
}
```

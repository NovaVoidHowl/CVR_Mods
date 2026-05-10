# DataFeed

## About

This mod provides a Data Feed of additional parameters from ChilloutVR, both to the avatar as parameters in the animation
controller and for other apps over REST and Websocket APIs.

> [!NOTE]
>
> The Websocket and REST APIs only listen on 127.0.0.1 (local host), if you want to use the data over the network,
> you will need to use a proxy (such as haproxy) to expose the APIs
>
> It is recommend to use ssl (https/wss) if you do this.

## Avatar Parameters

This mod exposes the following booleans to the avatar animator, based on the restrictions set on the current world

- flyingAllowed
- propsAllowed;
- portalsAllowed;
- nameplatesEnabled;

The following booleans are exposed so that you can react to the current state of the mod

- dataFeedErrorBBCC - true if the mod is unable to read the BetterBetterCharacterController data set
- dataFeedErrorMetaPort - true if the mod is unable to read the MetaPort data set
- dataFeedErrorOSC - true if the mod is unable to read the native OSC state
- oscEnabled - true if CVR's native OSC setting is enabled
- oscRunning - true if CVR's native OSC server is currently running
- dataFeedDisabled - set to true if the whole mod or the avatar parameter output settings are disabled, other wise false
- dataFeedAPIDisabled - false if the mod's API is enabled, true if the API is disabled or the whole mod is disabled

NOTE: if the `dataFeedDisabled` is true you should disregard the values passed by the other booleans

## Controlling and Configuring the mod

### Mod Settings Menu

|             Option              | Description                                                              |
| :-----------------------------: | :----------------------------------------------------------------------- |
|             Enable              | Turns the mod on/off                                                     |
| Avatar Parameter Output Enabled | Enables/Disables sending the booleans to the avatar controller variables |
|           API Enable            | Turns on/off the websocket and REST API endpoints                        |
|          REST API Port          | The REST API endpoint port on your system                                |
|       Websocket API Port        | The websocket API endpoint port on your system                           |

### API Key

In addition to the above MellonLoader variables there is `API Key` this is used with both the REST and Websocket API endpoints.

> [!IMPORTANT]
>
> The API key will not show up in the Mod Settings Menu, this is intentional to reduce the likelihood of accidentally
> leaking keys (in screenshots etc.)

> [!TIP]
>
> Your API key for the endpoints will show up in the MellonLoader preferences file after you have run the game for the
> first time with the mod installed

For REST API you connect to [http://127.0.0.1:8080/](http://127.0.0.1:8080/), a full list of endpoints will be shown\
For Websocket API you connect to [ws://127.0.0.1:8081/](ws://127.0.0.1:8081/), note only valid endpoints will return values

> [!TIP]
>
> The above ports are the defaults you can change them in the mod's settings

To authenticate to either you use the header `X-API-Key` with the value of that header being the `API Key` value from
the MellonLoader preferences file in the `DataFeed` section

> [!TIP]
>
> You can find the MellonLoader preferences file under your ChilloutVR install folder in `/UserData/MelonPreferences.cfg`

### Data Availability

The same data is not exposed everywhere. Avatar parameters are limited to simple booleans for avatar controllers, while
the REST and WebSocket APIs expose richer structured data for external tools.

| Data                         | Avatar Parameter | REST Endpoint        | WebSocket Endpoint   |
| :--------------------------- | :--------------: | :------------------- | :------------------- |
| `flyingAllowed`              |       Yes        | `/api/v1/parameters` | `/api/v1/parameters` |
| `propsAllowed`               |       Yes        | `/api/v1/parameters` | `/api/v1/parameters` |
| `portalsAllowed`             |       Yes        | `/api/v1/parameters` | `/api/v1/parameters` |
| `nameplatesEnabled`          |       Yes        | `/api/v1/parameters` | `/api/v1/parameters` |
| `dataFeedErrorBBCC`          |       Yes        | `/api/v1/parameters` | `/api/v1/parameters` |
| `dataFeedErrorMetaPort`      |       Yes        | `/api/v1/parameters` | `/api/v1/parameters` |
| `dataFeedDisabled`           |       Yes        | `/api/v1/parameters` | `/api/v1/parameters` |
| `dataFeedAPIDisabled`        |       Yes        | No                   | `/api/v1/parameters` |
| `oscEnabled`                 |       Yes        | `/api/v1/osc`        | `/api/v1/osc`        |
| `oscRunning`                 |       Yes        | `/api/v1/osc`        | `/api/v1/osc`        |
| `dataFeedErrorOSC`           |       Yes        | `/api/v1/osc`        | `/api/v1/osc`        |
| `oscVerboseLogging`          |        No        | `/api/v1/osc`        | `/api/v1/osc`        |
| `inboundAddress`             |        No        | `/api/v1/osc`        | `/api/v1/osc`        |
| `inboundPort`                |        No        | `/api/v1/osc`        | `/api/v1/osc`        |
| `outboundAddress`            |        No        | `/api/v1/osc`        | `/api/v1/osc`        |
| `outboundPort`               |        No        | `/api/v1/osc`        | `/api/v1/osc`        |
| `oscQueryServiceName`        |        No        | `/api/v1/osc`        | `/api/v1/osc`        |
| `connectedOscClients`        |        No        | `/api/v1/osc`        | `/api/v1/osc`        |
| `oscClients`                 |        No        | `/api/v1/osc`        | `/api/v1/osc`        |
| `currentInstanceId`          |        No        | `/api/v1/instance`   | `/api/v1/instance`   |
| `currentInstanceName`        |        No        | `/api/v1/instance`   | `/api/v1/instance`   |
| `currentWorldId`             |        No        | `/api/v1/instance`, `/api/v1/world` | `/api/v1/instance`, `/api/v1/world` |
| `currentInstancePrivacy`     |        No        | `/api/v1/instance`   | `/api/v1/instance`   |
| `worldDetails`               |        No        | `/api/v1/instance`, `/api/v1/world` | `/api/v1/instance`, `/api/v1/world` |
| `currentAvatarId`            |        No        | `/api/v1/avatar`     | `/api/v1/avatar`     |
| `avatarDetails`              |        No        | `/api/v1/avatar`     | `/api/v1/avatar`     |
| `detailsAvailable`           |        No        | `/api/v1/instance`, `/api/v1/avatar`, `/api/v1/world` | `/api/v1/instance`, `/api/v1/avatar`, `/api/v1/world` |
| `currentPing`                |        No        | `/api/v1/realtime`   | `/api/v1/realtime`   |
| `isConnected`                |        No        | `/api/v1/realtime`   | `/api/v1/realtime`   |
| `connectionState`            |        No        | `/api/v1/realtime`   | `/api/v1/realtime`   |
| `dataFeedErrorNetworkManager` |       No        | `/api/v1/realtime`   | `/api/v1/realtime`   |
| `currentFPS`                 |        No        | `/api/v1/realtime`   | `/api/v1/realtime`   |
| `voiceCommsPing`             |        No        | `/api/v1/realtime`   | `/api/v1/realtime`   |
| `isVoiceConnected`           |        No        | `/api/v1/realtime`   | `/api/v1/realtime`   |
| `voiceConnectionState`       |        No        | `/api/v1/realtime`   | `/api/v1/realtime`   |
| `dataFeedErrorComms`         |        No        | `/api/v1/realtime`   | `/api/v1/realtime`   |

At present, `dataFeedAPIDisabled` is available as an avatar parameter and on the parameters WebSocket payload, but is
not included in the REST `/api/v1/parameters` payload.

### Example API output

The following are example outputs from the mod's API endpoints

#### Instance (`/api/v1/instance`)

```json
{
  "currentInstanceId": "i+74a8fa0301855f7a-001003-5d0e8d-173e8a16",
  "currentInstanceName": "ChilloutVR Hub (#111098)",
  "currentWorldId": "501e2584-ce9a-4570-8c28-ef496e033f5f",
  "currentInstancePrivacy": "OwnerMustInvite",
  "worldDetails": {
    "Tags": [],
    "CompatibilityVersion": 2,
    "Platform": 0,
    "Description": "A new dawn begins atop this chill mountain. - World by Maebbie",
    "AuthorName": "ChilloutVR",
    "UploadedAt": "2020-04-06T20:50:25",
    "UpdatedAt": "2024-03-27T02:23:12",
    "Categories": [],
    "FileSize": 44292857
  },
  "detailsAvailable": true
}
```

#### Avatar (`/api/v1/avatar`)

```json
{
  "currentAvatarId": "17c267db-18c4-4900-bb73-ad323f082640",
  "avatarDetails": {
    "AvatarName": "Space Robot Kyle",
    "SwitchPermitted": true,
    "IsPublished": true,
    "Description": "by Unity Technologies",
    "AuthorName": "ChilloutVR",
    "UploadedAt": "2020-01-24T17:24:58",
    "UpdatedAt": "2023-05-16T11:33:02",
    "Categories": [],
    "FileSize": 1267287
  },
  "detailsAvailable": true
}
```

#### World (`/api/v1/world`)

```json
{
  "currentWorldId": "501e2584-ce9a-4570-8c28-ef496e033f5f",
  "worldDetails": {
    "Tags": [],
    "CompatibilityVersion": 2,
    "Platform": 0,
    "Description": "A new dawn begins atop this chill mountain. - World by Maebbie",
    "AuthorName": "ChilloutVR",
    "Name": "ChilloutVR Hub",
    "UploadedAt": "2020-04-06T20:50:25",
    "UpdatedAt": "2024-03-27T02:23:12",
    "Categories": [],
    "FileSize": 44292857
  },
  "detailsAvailable": true
}
```

> [!NOTE]
>
> The World endpoint provides detailed information about the current world when available through the ChilloutVR API.
> If `detailsAvailable` is `false`, the `worldDetails` object will contain mostly null values, and you should fall back
> to using the instance information from the Instance endpoint.

#### Parameters (`/api/v1/parameters`)

```json
{
  "flyingAllowed": true,
  "propsAllowed": true,
  "portalsAllowed": true,
  "nameplatesEnabled": true,
  "dataFeedErrorBBCC": false,
  "dataFeedErrorMetaPort": false,
  "dataFeedDisabled": false
}
```

#### Realtime (`/api/v1/realtime`)

```json
{
  "currentPing": 22,
  "isConnected": true,
  "connectionState": "Connected",
  "dataFeedErrorNetworkManager": false,
  "currentFPS": 60,
  "voiceCommsPing": 15,
  "isVoiceConnected": true,
  "voiceConnectionState": "Connected",
  "dataFeedErrorComms": false
}
```

#### OSC (`/api/v1/osc`)

```json
{
  "oscEnabled": true,
  "oscRunning": true,
  "oscVerboseLogging": false,
  "inboundAddress": "0.0.0.0",
  "inboundPort": 9000,
  "outboundAddress": "127.0.0.1",
  "outboundPort": 9001,
  "oscQueryServiceName": "ChilloutVR-GameClient-ABC123",
  "connectedOscClients": 1,
  "oscClients": [
    {
      "serviceId": "example-client",
      "oscAddress": "127.0.0.1",
      "oscPort": 9002,
      "httpAddress": "127.0.0.1",
      "httpPort": 12345,
      "hasAvatarModule": true,
      "hasInputModule": false,
      "hasChatBoxModule": true
    }
  ],
  "dataFeedErrorOSC": false
}
```

> [!NOTE]
>
> `connectedOscClients` and `oscClients` only include clients discovered through OSCQuery.
> Plain OSC-only tools may still send to or receive from CVR without appearing in the client list.

### WebSocket Endpoints

The WebSocket API provides several endpoints for different types of data:

- `/api/v1/parameters` - World parameter states (props allowed, flying allowed, etc.)
- `/api/v1/instance` - Current instance information (updated when switching worlds/instances)
- `/api/v1/avatar` - Current avatar information (updated when switching avatars)
- `/api/v1/world` - Current world details from ChilloutVR API (updated when switching worlds)
- `/api/v1/realtime` - Real-time data updates (ping, FPS, connection status)
- `/api/v1/osc` - Native ChilloutVR OSC status, ports, and OSCQuery-discovered client summaries

> [!TIP]
>
> For real-time monitoring applications, use `/api/v1/realtime` which updates every second.
> For static information that changes less frequently, use the specific endpoints like `/api/v1/world` or `/api/v1/avatar`.

### WebSocket Real-time Data

The WebSocket endpoint (`ws://127.0.0.1:8081/`) sends real-time updates every second with the same format as the
REST realtime endpoint:

```json
{
  "currentPing": 22,
  "isConnected": true,
  "connectionState": "Connected",
  "dataFeedErrorNetworkManager": false,
  "currentFPS": 60,
  "voiceCommsPing": 15,
  "isVoiceConnected": true,
  "voiceConnectionState": "Connected",
  "dataFeedErrorComms": false
}
```

Where:

- `currentPing`: Network latency to the game server in milliseconds
- `isConnected`: Boolean indicating if connected to the game server
- `connectionState`: String representation of the connection state (e.g., "Connected", "Disconnected", "Connecting")
- `dataFeedErrorNetworkManager`: Boolean indicating if there was an error reading NetworkManager data
- `currentFPS`: Current frames per second of the game
- `voiceCommsPing`: Voice communications latency in milliseconds (currently returns 0)
- `isVoiceConnected`: Boolean indicating if connected to voice communications
- `voiceConnectionState`: String representation of the voice communications connection state
- `dataFeedErrorComms`: Boolean indicating if there was an error reading voice communications data

## Example Usage: OBS Overlay

Want to see the DataFeed mod in action? Check out the **CVR Data Feed OBS Overlay** - a real-time streaming overlay
that demonstrates how to use both the REST API and WebSocket endpoints together:

🔗 **[CVR-DataFeed-OBS-Overlay](https://github.com/NovaVoidHowl/CVR-DataFeed-OBS-Overlay)**

### What it does

- **Real-time Network Display**: Shows game ping, FPS, and connection status using the WebSocket `/api/v1/realtime`
  endpoint
- **World Information Panel**: Displays current world name, author etc. via the REST `/api/v1/instance`
  endpoint
- **Avatar Information Panel**: Shows current avatar details and tags using the REST `/api/v1/avatar` endpoint
- **Auto-reconnection**: Handles connection drops gracefully and reconnects automatically
- **OBS Integration**: Ready-to-use browser source overlay for streamers

### Important: CORS and Local File Restrictions

> [!NOTE]
>
> **As of DataFeed v0.9.2 (2025r181 update):** Opening the overlay directly as a file (`file:///C:/path/to/index.html`)
> now works thanks to improved CORS handling and multi-host binding (localhost, 127.0.0.1, ::1).
>
> **However, we still recommend serving via HTTP** due to evolving browser security policies. Chrome and other browsers
> frequently tighten CORS restrictions for `file://` origins, and what works today may be blocked in future browser updates.

**Recommended Setup Methods (for maximum compatibility):**

1. **Python Simple HTTP Server** (Easiest):

   ```powershell
   cd path/to/CVR-DataFeed-OBS-Overlay
   python -m http.server 8000
   ```

   Then in OBS, use browser source URL: `http://localhost:8000/index.html`

2. **Node.js http-server**:

   ```powershell
   npx http-server -p 8000
   ```

   Then use URL: `http://localhost:8000/index.html`

3. **VS Code Live Server Extension**:

   - Install "Live Server" extension in VS Code
   - Right-click `index.html` → "Open with Live Server"
   - Use the provided localhost URL in OBS

4. **PowerShell Server Script** (included in overlay repo):
   - Run `.\serve_overlay.ps1` in the overlay directory
   - Use URL: `http://localhost:8000/index.html`
   - Note this script needs python installed

> [!TIP]
>
> Using HTTP serving ensures consistent behavior across all browsers and protects against future browser security updates
> that may break `file://` access.

### Key Features

- **Hybrid Data Approach**: Combines WebSocket for live data (ping, FPS) with REST API calls for static information
  (world/avatar details)
- **Visual Indicators**: Connection status and data freshness indicators
- **Responsive Design**: Scales appropriately for different stream resolutions
- **Easy Setup**: Simple configuration with your DataFeed API key

This example showcases the practical applications of the DataFeed mod's API and serves as a reference implementation
for developers looking to integrate ChilloutVR data into their own applications.

## Future goals

Add OSC output for external apps to read from
